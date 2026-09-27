using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of MainWindow: the Edit side list search also finds fields on the pages (their labels
    // and card titles), opens the page and flashes the field.
    public partial class MainWindow
    {
        private sealed class FieldHit
        {
            public TextBlock Label;
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
        };

        // Labels of every page, collected once from the logical tree (pages that were never
        // opened are not in the visual tree yet). The text is read at search time, so a language
        // change needs no new index.
        private List<FieldHit> FieldIndex()
        {
            if (_fieldIndex != null)
                return _fieldIndex;
            _fieldIndex = new List<FieldHit>();
            foreach (var entry in EditNav.Where(e => e.Group < 2 && e.Page != null))
            {
                foreach (var label in LogicalDescendants(entry.Page).OfType<TextBlock>())
                {
                    if (!IsSearchableLabel(label))
                        continue;
                    _fieldIndex.Add(new FieldHit { Label = label, Entry = entry, Sub = SubFor(entry, label) });
                }
            }
            return _fieldIndex;
        }

        private bool IsSearchableLabel(TextBlock label)
        {
            var style = label.Style;
            return style != null && (style == TryFindResource("FieldLabel") || style == TryFindResource("FormLabel") || style == TryFindResource("DashCardTitle"));
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

        /// <summary>Field hits for the side list search, at most <paramref name="max"/>.</summary>
        private List<(string Title, string Path, Action Open)> SearchFields(string query, int max)
        {
            var hits = new List<(string, string, Action)>();
            var seen = new HashSet<string>();
            foreach (var hit in FieldIndex())
            {
                if (!EditNavFits(hit.Entry) || (hit.Sub?.Button != null && hit.Sub.Button.Visibility != Visibility.Visible))
                    continue;
                string text = hit.Label.Text?.Trim();
                if (string.IsNullOrEmpty(text) || !Matches(text, query))
                    continue;
                string page = TranslateOr(hit.Entry.Key, hit.Entry.Fallback);
                string path = hit.Sub != null && SubLabel(hit.Sub) != page ? page + "  ›  " + SubLabel(hit.Sub) : page;
                // The same label on the same page (e.g. "X" "Y" "Z" rows) shows once.
                if (!seen.Add(text + "|" + path))
                    continue;
                var target = hit;
                hits.Add((text, path, () => RevealField(target)));
                if (hits.Count >= max)
                    break;
            }
            return hits;
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
                Flash(hit.Label);
                var input = InputFor(hit.Label);
                if (input != null)
                    FlashOutline.Show(input);
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

        // A short accent glow behind the label.
        private static void Flash(TextBlock label)
        {
            var accent = ThemeBrush("AccentBrush") as SolidColorBrush;
            var color = accent?.Color ?? Colors.SteelBlue;
            var brush = new SolidColorBrush(Color.FromArgb(0x90, color.R, color.G, color.B));
            label.Background = brush;
            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(1800)) { BeginTime = TimeSpan.FromMilliseconds(600) };
            fade.Completed += (_, __) => label.Background = null;
            brush.BeginAnimation(Brush.OpacityProperty, fade);
        }
    }

    /// <summary>A rounded accent frame around a field that fades out: where a search hit landed.</summary>
    internal sealed class FlashOutline : Adorner
    {
        private readonly Pen _pen;

        private FlashOutline(UIElement element, Color color) : base(element)
        {
            IsHitTestVisible = false;
            _pen = new Pen(new SolidColorBrush(color), 2.5);
        }

        public static void Show(FrameworkElement element)
        {
            var layer = AdornerLayer.GetAdornerLayer(element);
            if (layer == null)
                return;
            var accent = (MainWindow.ThemeBrush("AccentBrush") as SolidColorBrush)?.Color ?? Colors.SteelBlue;
            var outline = new FlashOutline(element, accent);
            layer.Add(outline);
            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(1800)) { BeginTime = TimeSpan.FromMilliseconds(900) };
            fade.Completed += (_, __) => layer.Remove(outline);
            outline.BeginAnimation(OpacityProperty, fade);
        }

        protected override void OnRender(DrawingContext dc)
        {
            var size = AdornedElement.RenderSize;
            dc.DrawRoundedRectangle(null, _pen, new Rect(-3, -3, size.Width + 6, size.Height + 6), 7, 7);
        }
    }
}
