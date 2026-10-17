using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of MainWindow: the Edit side list search also finds fields on the pages (their labels,
    // switches and card titles), and the settings of pages built in code (Rules detail groups,
    // Test page, inventory sets), opens the page and flashes the field. Matching: SmartSearch.
    public partial class MainWindow
    {
        private sealed class FieldHit
        {
            public FrameworkElement Label;  // a label, a switch or a card
            public Func<string> Text;       // read at search time (language changes)
            public EditNavEntry Entry;
            public EditNavSub Sub;
        }

        private List<FieldHit> _fieldIndex;

        // Sub-page buttons whose name does not match the name of their page.
        private static readonly Dictionary<string, string> SubPageNames = new Dictionary<string, string>
        {
            { "BtnMissionTPM", "PageInnerMissionTeleportMarkers" },
            { "BtnMissionGC", "PageInnerMissionGang" },
            { "BtnRaceCheckpoints", "PageInnerRaceCP" },
            { "BtnMissionInterior", "PageInnerMissionIPL" },
        };

        // Labels of every page, collected once from the logical tree (pages that were never
        // opened are not in the visual tree yet). The text is read at search time, so a language
        // change needs no new index.
        private List<FieldHit> FieldIndex()
        {
            if (_fieldIndex != null)
                return _fieldIndex;
            _fieldIndex = new List<FieldHit>();
            foreach (var entry in EditNav.Where(e => e.Page != null))
            {
                // Sub-pages on a page of their own (fixtures, the capture pages) are searched too.
                var pages = new[] { entry.Page }.Concat(entry.Subs.Select(s => s.Page)).Where(p => p != null).Distinct();
                foreach (var element in pages.SelectMany(p => LogicalDescendants(p)).OfType<FrameworkElement>())
                {
                    Func<string> text = null;
                    if (element is TextBlock label && IsSearchableLabel(label))
                        text = () => label.Text;
                    // A switch with its own text (weapon chips are no settings).
                    else if (element is ToggleButton toggle && toggle.Content is string && toggle.Style != TryFindResource("ChipToggle"))
                        text = () => toggle.Content as string;
                    else if (element is SectionCard card)
                        text = () => card.Title as string;
                    if (text == null)
                        continue;
                    _fieldIndex.Add(new FieldHit { Label = element, Text = text, Entry = entry, Sub = SubFor(entry, element) });
                }
            }
            return _fieldIndex;
        }

        private bool IsSearchableLabel(TextBlock label)
        {
            var style = label.Style;
            return style != null && (style == TryFindResource("FieldLabel") || style == TryFindResource("FormLabel") || style == TryFindResource("DashCardTitle")
                || style == TryFindResource("PageSectionLabel"));
        }

        private static IEnumerable<DependencyObject> LogicalDescendants(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var deeper in LogicalDescendants(child))
                    yield return deeper;
            }
        }

        // The innermost page (TabItem) around the label, matched to a sub-page of the entry.
        private EditNavSub SubFor(EditNavEntry entry, DependencyObject label)
        {
            foreach (var tab in AncestorTabs(label))
            {
                if (tab == entry.Page)
                    break;
                var sub = entry.Subs.FirstOrDefault(s => s.Page == tab)
                    ?? entry.Subs.FirstOrDefault(s => s.Button != null && ButtonOpens(s.Button.Name, tab.Name));
                if (sub != null)
                    return sub;
            }
            return null;
        }

        private static bool ButtonOpens(string button, string page)
        {
            if (string.IsNullOrEmpty(button) || string.IsNullOrEmpty(page))
                return false;
            if (SubPageNames.TryGetValue(button, out string known))
                return known == page;
            string rest = button.StartsWith("Btn") ? button.Substring(3) : button;
            rest = rest.StartsWith("Section") ? rest.Substring(7) : rest;
            return page.EndsWith(rest, StringComparison.Ordinal);
        }

        private static IEnumerable<TabItem> AncestorTabs(DependencyObject element)
        {
            for (var at = LogicalTreeHelper.GetParent(element); at != null; at = LogicalTreeHelper.GetParent(at))
                if (at is TabItem tab)
                    yield return tab;
        }

        /// <summary>Field hits for the side list search, best first, at most <paramref name="max"/>.</summary>
        private List<(string Title, string Path, Action Open)> SearchFields(string query, int max)
        {
            var hits = new List<(int Score, string Title, string Path, Action Open)>();
            var seen = new HashSet<string>();
            foreach (var hit in FieldIndex())
            {
                if (!EditNavFits(hit.Entry) || (hit.Sub?.Button != null && hit.Sub.Button.Visibility != Visibility.Visible))
                    continue;
                string text = hit.Text()?.Trim();
                if (string.IsNullOrEmpty(text))
                    continue;
                string page = TranslateOr(hit.Entry.Key, hit.Entry.Fallback);
                string path = hit.Sub != null && SubLabel(hit.Sub) != page ? page + "  ›  " + SubLabel(hit.Sub) : page;
                int score = SmartSearch.Score(query, text, path, hit.Label.ToolTip as string);
                // The same label on the same page (e.g. "X" "Y" "Z" rows) shows once.
                if (score == 0 || !seen.Add(text + "|" + path))
                    continue;
                var target = hit;
                hits.Add((score, text, path, () => RevealField(target)));
            }
            foreach (var code in CodeSettings())
            {
                int score = SmartSearch.Score(query, code.Title, code.Path, code.Keywords);
                if (score > 0 && seen.Add(code.Title + "|" + code.Path))
                    hits.Add((score, code.Title, code.Path, code.Open));
            }
            return hits.OrderByDescending(h => h.Score).Take(max).Select(h => (h.Title, h.Path, h.Open)).ToList();
        }

        // Settings of pages that are built in code when shown, so the logical tree does not have
        // them: the Rules page's detail groups and options, the Test page, the inventory sets.
        private IEnumerable<(string Title, string Path, string Keywords, Action Open)> CodeSettings()
        {
            var mission = EditNav.FirstOrDefault(e => e.Key == "mission");
            if (mission == null || !EditNavFits(mission))
                yield break;
            string missionName = TranslateOr("mission", "Mission");
            string rules = missionName + "  ›  " + TranslateOr("rl_page", "Rules");
            Action Rule(string group) => () => { OpenRules(); rulesView.RevealGroup(group); };

            var groups = new List<(string Key, string Title, IEnumerable<string> Options)>
            {
                ("sms", TranslateOr("rl_g_sms", "Text messages"), new[] { "SMS" }),
                ("teams", RuleOptions.GameText("PMCT_OJT_OPL", "Other Team Progress Limit"), new string[0]),
                ("flags", RuleOptions.GameText("PMCT_OMF", "Mission Flags"), new[] { RuleOptions.GameText("PMCT_OJT_OLP", "Mission Flag Limit"), RuleOptions.GameText("PMCT_OMF_SOO", "Set On Start"), RuleOptions.GameText("PMCT_OMF_COO", "Clear On Start") }),
                ("bounds", RuleOptions.GameText("PMCT_BNDS", "Bounds"), new string[0]),
                ("wanted", TranslateOr("rl_g_wanted", "Wanted level"), RuleOptions.MoreGroups[0].Options.Select(RuleOptions.Title)),
                ("drop", TranslateOr("dof_title", "Drop-off"), new[] { TranslateOr("dof_radius", "Radius"), TranslateOr("dof_shape", "Shape"), RuleOptions.GameText("PMCT_OJT_DOZ", "Specify Drop-off Zones") }),
                ("beast", TranslateOr("rl_g_beast", "Beast mode"), new[] { "BST_HELTH", "BST_DMG", "BST_MOV", "BST_JMP", "BST_SPD", "BST_STELO", "AB_BSTS_ON" }.Select(l => RuleOptions.GameText("PMCT_" + l, l))),
                ("texts", TranslateOr("rl_more_texts", "More texts"), new[] { RuleOptions.GameText("PMCT_OBJ2", "Objective Text 2"), RuleOptions.GameText("PMCT_OBJ3", "Objective Text 3"), RuleOptions.GameText("PMC_OBLIP", "Blip Name"), RuleOptions.GameText("PMC_TKRT", "Ticker Name"), RuleOptions.GameText("PMC_PTKRT", "Plural Ticker Name") }),
                ("checkpoints", TranslateOr("rcp_title", "Restart checkpoints"), new[] { TranslateOr("rcp_roles", "Minimum players per role"), TranslateOr("rcp_b_test", "Test from this checkpoint"), TranslateOr("rcp_time", "Least time left after the restart"), TranslateOr("rcp_b_spawn", "Spawn points after the restart") }),
            };
            foreach (var g in RuleOptions.MoreGroups.Skip(1))
                groups.Add((g.Key, TranslateOr(g.Key, g.Fallback), g.Options.Select(RuleOptions.Title)));
            foreach (var g in groups)
            {
                var options = g.Options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
                yield return (g.Title, rules, string.Join(" ", options), Rule(g.Key));
                foreach (string option in options)
                    yield return (option, rules + "  ›  " + g.Title, null, Rule(g.Key));
            }
            foreach (var o in RuleOptions.EndOptions.Where(RuleOptions.Available))
                yield return (RuleOptions.Title(o), rules + "  ›  " + TranslateOr("rl_b_end", "When the rule ends"), RuleOptions.Help(o), (Action)OpenRules);

            string test = missionName + "  ›  " + TranslateOr("olt_title", "Test like online");
            foreach (var e in OnlineTestView.SearchEntries())
            {
                string key = e.Key;
                yield return (e.Name, test, e.Help, () => { BtnSectionMission_Click(null, null); ClickButton(BtnSectionTest); OnlineTestPage.Reveal(key); });
            }

            string inventory = missionName + "  ›  " + TranslateOr("inv_title", "Inventory");
            foreach (string name in new[] { RuleOptions.GameText("PMCT_INV_TS_MM", "Secondary Inventory"), RuleOptions.GameText("PMCT_INV_TS_MM2", "Tertiary Inventory"), TranslateOr("inv_given", "Given when") })
                yield return (name, inventory, TranslateOr("inv_mid_hint", ""), () => { BtnSectionMission_Click(null, null); ClickButton(BtnSectionInventory); });
        }

        private void RevealField(FieldHit hit)
        {
            OpenEditNav(hit.Entry, hit.Sub, rememberBack: true);
            // Pages inside pages (tabs the side list does not know) are selected on the way down.
            foreach (var tab in AncestorTabs(hit.Label).Reverse())
                tab.IsSelected = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                hit.Label.BringIntoView();
                FlashOutline.Show(hit.Label, hit.Label is TextBlock label ? InputFor(label) : null);
            }), DispatcherPriority.Loaded);
        }

        // The field that belongs to a label: the next input in its panel (FieldLabel over a box),
        // or the switch in the same row (FormLabel + FormToggle).
        private static FrameworkElement InputFor(TextBlock label)
        {
            if (!(label.Parent is Panel panel))
                return null;
            int at = panel.Children.IndexOf(label);
            for (int i = at + 1; i < panel.Children.Count; i++)
            {
                var found = FirstInput(panel.Children[i]);
                if (found != null)
                    return found;
                if (panel.Children[i] is TextBlock t && (t.Style == label.Style))
                    break;
            }
            return panel is Grid ? panel.Children.OfType<UIElement>().Select(FirstInput).FirstOrDefault(f => f != null) : null;
        }

        private static FrameworkElement FirstInput(object element)
        {
            if (element is FrameworkElement fe && fe.Visibility != Visibility.Visible)
                return null;
            if (element is TextBox || element is ComboBox || element is CheckBox || element is Slider)
                return (FrameworkElement)element;
            if (element is DependencyObject d)
                foreach (var child in LogicalTreeHelper.GetChildren(d))
                {
                    var found = FirstInput(child);
                    if (found != null)
                        return found;
                }
            return null;
        }
    }

    /// <summary>
    /// One rounded accent frame around a label and its field that blinks twice and fades out:
    /// where a search hit landed.
    /// </summary>
    internal sealed class FlashOutline : Adorner
    {
        private readonly FrameworkElement[] _parts;
        private readonly Pen _pen;
        private readonly Brush _fill;

        private FlashOutline(UIElement host, FrameworkElement[] parts, Color color) : base(host)
        {
            IsHitTestVisible = false;
            _parts = parts;
            _pen = new Pen(new SolidColorBrush(color), 2);
            _fill = new SolidColorBrush(Color.FromArgb(0x26, color.R, color.G, color.B));
        }

        public static void Show(FrameworkElement label, FrameworkElement input)
        {
            var parts = input == null ? new[] { label } : new[] { label, input };
            // The frame is drawn on the closest element that holds both, so it covers the pair.
            DependencyObject host = label;
            while (host != null && !(host is UIElement u && parts.All(p => u == p || u.IsAncestorOf(p))))
                host = VisualTreeHelper.GetParent(host);
            if (!(host is UIElement element))
                return;
            var layer = AdornerLayer.GetAdornerLayer(element);
            if (layer == null)
                return;
            var accent = (MainWindow.ThemeBrush("AccentBrush") as SolidColorBrush)?.Color ?? Colors.SteelBlue;
            var outline = new FlashOutline(element, parts, accent);
            layer.Add(outline);
            var blink = new DoubleAnimationUsingKeyFrames();
            double[] steps = { 0, 1, 0.25, 1, 0.25, 1 };
            for (int i = 0; i < steps.Length; i++)
                blink.KeyFrames.Add(new LinearDoubleKeyFrame(steps[i], KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(i * 220))));
            blink.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000))));
            blink.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3200))));
            blink.Completed += (_, __) => layer.Remove(outline);
            outline.BeginAnimation(OpacityProperty, blink);
        }

        protected override void OnRender(DrawingContext dc)
        {
            Rect area = Rect.Empty;
            foreach (var part in _parts)
            {
                if (part == AdornedElement)
                    area.Union(new Rect(part.RenderSize));
                else if (part.IsVisible && AdornedElement.IsAncestorOf(part))
                    area.Union(part.TransformToAncestor(AdornedElement).TransformBounds(new Rect(part.RenderSize)));
            }
            if (area.IsEmpty)
                return;
            area.Inflate(6, 4);
            dc.DrawRoundedRectangle(_fill, _pen, area, 8, 8);
        }
    }
}
