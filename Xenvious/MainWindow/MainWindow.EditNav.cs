using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xenvious
{
    // Part of MainWindow: the Edit side list (pages per creator, sub-pages, search, back, script patch status).
    public partial class MainWindow
    {
        // Creator letters: R race, L LTS, C capture, D deathmatch, S survival, M other missions.
        private const string AllCreators = "RLCDSM";

        /// <summary>One entry of the side list. A sub-page opens through the button that used
        /// to switch it on the page itself, so whatever that click did still happens.</summary>
        private sealed class EditNavEntry
        {
            public string Key;
            public string Fallback;
            public string Icon;
            public int Group;               // 0 place, 1 job rules, 2 more options
            public string Creators;
            public TabItem Page;
            public Action Open;
            public List<EditNavSub> Subs = new List<EditNavSub>();
        }

        private sealed class EditNavSub
        {
            public string Label;            // null: take the button's own text
            public Button Button;           // the page's own switch button, clicked for us
            public Action Open;             // instead of Button, e.g. a page of its own
            public TabItem Page;            // for Open: the page that counts as this sub-page
            public Func<int> Count;         // entries of this sub-page in the job, shown as a badge
        }

        private List<EditNavEntry> _editNav;
        private string _editNavCreator;
        private EditNavEntry _editNavEntry;
        private EditNavSub _editNavSub;
        private (EditNavEntry Entry, EditNavSub Sub)? _editNavBack;
        private bool _editNavCollapsed;
        // Entries whose sub-pages are shown: opening another page leaves them open.
        private readonly HashSet<EditNavEntry> _editNavOpen = new HashSet<EditNavEntry>();

        private List<EditNavEntry> EditNav => _editNav ?? (_editNav = BuildEditNav());

        private List<EditNavEntry> BuildEditNav()
        {
            EditNavEntry Page(string key, string fallback, string icon, int group, string creators, TabItem page, Action open, params EditNavSub[] subs)
            {
                var entry = new EditNavEntry { Key = key, Fallback = fallback, Icon = icon, Group = group, Creators = creators, Page = page, Open = open };
                entry.Subs.AddRange(subs);
                return entry;
            }
            EditNavSub Sub(Button button) => new EditNavSub { Button = button };
            EditNavSub Counted(Button button, Func<int> count) => new EditNavSub { Button = button, Count = count };

            return new List<EditNavEntry>
            {
                Page("props", "Props", "EditIconProps", 0, AllCreators, PageProps, () => BtnSectionProps_Click(null, null),
                    Sub(BtnNormalProps), Sub(BtnDynamicProps),
                    // Fixtures have a page of their own; they belong with the props.
                    new EditNavSub { Label = TranslateOr("editnav_fixtures", "Fixtures"), Open = () => BtnSectioncentity_Click(null, null), Page = Pagecentity },
                    Sub(BtnModdedProps), Sub(BtnAdvancedPropPlacement)),
                Page("vehicles", "Vehicles", "EditIconVehicle", 0, AllCreators, PageVehicle, () => BtnSectionVeh_Click(null, null)),
                Page("weapons", "Weapons", "EditIconWeapon", 0, AllCreators, PageWeapon, () => BtnSectionWeap_Click(null, null)),
                // Actors and doors exist in every creator except the race creator.
                Page("actor", "Actors", "EditIconActor", 0, "LCDSM", PageActor, () => BtnSectionActor_Click(null, null)),
                Page("doors", "Doors", "EditIconDoor", 0, "LCDSM", PageDoors, () => BtnSectionDoors_Click(null, null)),
                Page("zones", "Zones", "EditIconZone", 0, AllCreators, PageZone, () => BtnSectionZone_Click(null, null)),

                Page("mission", "Mission", "EditIconMission", 1, "LCM", PageMission, () => BtnSectionMission_Click(null, null),
                    Sub(BtnMissionGeneral), Sub(BtnMissionTeamSettings), Sub(BtnMissionPlayerSettings), Sub(BtnMissionPA), Sub(BtnMissionRA),
                    Sub(BtnMissionTPM), Counted(BtnMissionKill, () => GlobalCount(GTA.Offsets.Editor.Kill.number)), Counted(BtnMissionGC, GangChaseRuleCount),
                    Counted(BtnMissionotzone, () => GlobalCount(GTA.Offsets.Editor.otzone.number)), Counted(BtnMissionBlips, () => GlobalCount(GTA.Offsets.Editor.ddblip.number)),
                    Sub(BtnSectionInventory), Counted(BtnSectionSMS, SmsCount), Counted(BtnSectionGoto, () => GlobalCount(GTA.Offsets.Editor.Locations.number)),
                    new EditNavSub { Label = TranslateOr("eo_page", "Extra objectives"), Open = OpenExtraObjectives, Page = PageMission, Count = ExtraObjectives.UsedSlots }),
                Page("jobsubtype_mission_capture", "Capture", "EditIconCapture", 1, "C", PageCapture, () => BtnSectionObj_Click(null, null),
                    Sub(BtnCaptureGeneral), Sub(BtnCaptureObjects), Sub(BtnCaptureDelivery)),
                Page("race", "Race", "EditIconRace", 1, "R", PageRace, () => BtnSectionRace_Click(null, null),
                    Sub(BtnRaceGeneral), Sub(BtnRaceCheckpoints), Sub(BtnRaceAVEH), Sub(BtnRaceArena)),
                Page("deathmatch", "Deathmatch", "EditIconDeathmatch", 1, "D", PageDeathmatch, () => BtnSectionDeathmatch_Click(null, null)),
                Page("survival", "Survival", "EditIconSurvival", 1, "S", PageSurvival, () => BtnSectionSurvival_Click(null, null)),

                // Raw data and special pages.
                Page("editnav_menubits", "Menu bits (raw)", "EditIconBits", 2, "LCM", PageMission,
                    () => { BtnSectionMission_Click(null, null); ClickButton(BtnMissionMenubs); }),
                Page("editnav_interiors", "Interiors (IPL)", "EditIconInterior", 2, "LCM", PageMission,
                    () => { BtnSectionMission_Click(null, null); ClickButton(BtnMissionInterior); }),
            };
        }

        private static void ClickButton(Button button)
        {
            button?.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        }

        private static string CreatorLetter(string creator)
        {
            switch (creator)
            {
                case "fm_race_creator": return "R";
                case "fm_lts_creator": return "L";
                case "fm_capture_creator": return "C";
                case "fm_deathmatch_creator": return "D";
                case "fm_survival_creator": return "S";
                case "fm_mission_creator": return "M";
                default: return "";
            }
        }

        // Outside a creator every page is listed; inside, only the ones this creator has.
        private bool EditNavFits(EditNavEntry entry)
        {
            string letter = CreatorLetter(_editNavCreator ?? "");
            return letter.Length == 0 || entry.Creators.Contains(letter);
        }

        // The race creator has no mission menu, but the mission options (team, kill, gang chase ...)
        // are job data that still works there; they are listed greyed out.
        private bool EditNavGreyed(EditNavEntry entry)
            => CreatorLetter(_editNavCreator ?? "") == "R" && entry.Key == "mission";

        /// <summary>Called once a second from the dashboard status; rebuilds when the creator changes.</summary>
        private void UpdateEditNav(string creator)
        {
            if (_editNavCreator == creator && EditNavList.Children.Count > 0)
            {
                string counts = EditNavCountKey();
                if (counts != _editNavCounts)
                {
                    _editNavCounts = counts;
                    RenderEditNav();
                }
                UpdateEditScriptStatus();
                return;
            }
            _editNavCreator = creator;
            HideInnerSwitchers();
            RenderEditNav();
            UpdateEditScriptStatus();
        }

        // The pages still carry their own list of sub-page buttons on the left; the side
        // list replaces it, so that column is folded away (the buttons stay usable from code).
        private bool _innerSwitchersHidden;

        private void HideInnerSwitchers()
        {
            if (_innerSwitchersHidden)
                return;
            _innerSwitchersHidden = true;
            foreach (var inner in new[] { PageInnerProps, PageInnerRace, PageInnerMission, PageInnerCapture, PageInnerDM, PageInnerSurvival })
            {
                // The page grid is the first ancestor with the three columns (buttons, line, pages);
                // on Mission the TabControl sits one Grid deeper.
                FrameworkElement element = inner;
                Grid grid = null;
                while (element?.Parent is FrameworkElement parent)
                {
                    if (parent is Grid candidate && candidate.ColumnDefinitions.Count >= 3 && Grid.GetColumn(element) == 2)
                    {
                        grid = candidate;
                        break;
                    }
                    element = parent;
                }
                if (grid == null)
                    continue;
                grid.ColumnDefinitions[0].Width = new GridLength(0);
                grid.ColumnDefinitions[1].Width = new GridLength(0);
                element.Margin = new Thickness(0);
                foreach (UIElement child in grid.Children)
                {
                    if (Grid.GetColumn(child) < 2)
                        child.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void RenderEditNav()
        {
            EditNavList.Children.Clear();
            string[] groups =
            {
                TranslateOr("editnav_grp_place", "Place"),
                TranslateOr("editnav_grp_rules", "Job rules"),
                TranslateOr("editnav_grp_more", "More options"),
            };
            RenderFavorites();
            for (int group = 0; group < groups.Length; group++)
            {
                var entries = EditNav.Where(e => e.Group == group && (EditNavFits(e) || EditNavGreyed(e))).ToList();
                if (entries.Count == 0)
                    continue;
                if (!_editNavCollapsed)
                    EditNavList.Children.Add(new TextBlock { Text = groups[group], Style = (Style)FindResource("SideNavGroup") });
                else if (group > 0)
                    EditNavList.Children.Add(new Border { Height = 1, Background = (Brush)FindResource("NavMutedBrush"), Opacity = 0.3, Margin = new Thickness(6, 8, 6, 8) });

                foreach (var entry in entries)
                {
                    var button = NavEntryButton(entry);
                    if (!EditNavFits(entry))
                    {
                        button.Opacity = 0.5;
                        button.ToolTip = TranslateOr("editnav_other_creator", "Not part of this creator's menu; the values still work.");
                    }
                    EditNavList.Children.Add(WithStar(button, FavoriteId(entry, null)));
                    if (!_editNavCollapsed && _editNavOpen.Contains(entry) && entry.Subs.Count > 0)
                        EditNavList.Children.Add(NavSubList(entry));
                }
            }
        }

        private Button NavEntryButton(EditNavEntry entry)
        {
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(NavIcon(entry.Icon));
            string label = TranslateOr(entry.Key, entry.Fallback);
            if (!_editNavCollapsed)
                content.Children.Add(new TextBlock { Text = label, Style = (Style)FindResource("NavLabel") });

            var button = new Button
            {
                Style = (Style)FindResource("SideNavButton"),
                Content = content,
                Tag = entry == _editNavEntry && (_editNavSub == null || _editNavCollapsed) ? "active" : null,
                ToolTip = _editNavCollapsed ? label : null,
            };
            button.Click += (_, __) =>
            {
                // A second click on the open page folds its sub-pages away (and back).
                if (entry == _editNavEntry && entry.Subs.Count > 0 && !_editNavCollapsed)
                {
                    if (!_editNavOpen.Remove(entry))
                        _editNavOpen.Add(entry);
                    RenderEditNav();
                    return;
                }
                OpenEditNav(entry, null, rememberBack: false);
            };
            return button;
        }

        private FrameworkElement NavIcon(string key)
        {
            return new Viewbox
            {
                Style = (Style)FindResource("NavIconBox"),
                Child = new Canvas
                {
                    Width = 24,
                    Height = 24,
                    Children = { new Path { Style = (Style)FindResource("NavIcon"), Data = (Geometry)FindResource(key) } }
                }
            };
        }

        private FrameworkElement NavSubList(EditNavEntry entry)
        {
            var list = new StackPanel { Margin = new Thickness(28, 0, 0, 6) };
            foreach (var sub in entry.Subs)
            {
                if (sub.Button != null && sub.Button.Visibility != Visibility.Visible)
                    continue;
                var button = new Button
                {
                    Style = (Style)FindResource("SideNavButton"),
                    FontSize = 13,
                    Padding = new Thickness(10, 5, 10, 5),
                    Content = SubContent(sub),
                    Tag = sub == _editNavSub ? "active" : null,
                };
                button.Click += (_, __) => OpenEditNav(entry, sub, rememberBack: false);
                list.Children.Add(WithStar(button, FavoriteId(entry, sub)));
            }
            return list;
        }

        // Label plus a green badge with how many entries the job has on that sub-page.
        private object SubContent(EditNavSub sub)
        {
            var label = new TextBlock { Text = SubLabel(sub), Style = (Style)FindResource("NavLabel") };
            int count = SafeCount(sub);
            if (count <= 0)
                return label;
            var row = new DockPanel { LastChildFill = true };
            var badge = new Border
            {
                CornerRadius = new CornerRadius(9), Padding = new Thickness(7, 0, 7, 1), Margin = new Thickness(8, 0, 0, 0),
                Background = ThemeBrush("DeepBrush"), VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = count.ToString(CultureInfo.CurrentCulture), FontSize = 11, FontWeight = FontWeights.Bold,
                    Foreground = ThemeBrush("OkBrush") },
            };
            DockPanel.SetDock(badge, Dock.Right);
            row.Children.Add(badge);
            row.Children.Add(label);
            return row;
        }

        private int SafeCount(EditNavSub sub)
        {
            if (sub.Count == null || _editNavCreator == null || _editNavCreator.Length == 0 || m == null || !m.IsProcOpen)
                return 0;
            try { return sub.Count(); }
            catch { return 0; }
        }

        // Counts of all sub-pages; the side list is drawn again when one of them changes.
        private string EditNavCountKey()
            => string.Join(",", EditNav.SelectMany(e => e.Subs).Select(SafeCount));

        private string _editNavCounts;

        private static int GlobalCount(long offset) => offset == 0 ? 0 : new Global(offset).Get<int>();

        // Messages that have a text and a rule.
        private int SmsCount()
        {
            if (GTA.Offsets.Editor.SMS.txt == 0 || GTA.Offsets.Editor.SMS.rule == 0)
                return 0;
            int count = 0;
            for (int i = 0; i < ddsmsno.Items.Count; i++)
            {
                long next = GTA.Offsets.Editor.SMS.NEXT * i;
                if (new Global(GTA.Offsets.Editor.SMS.rule + next).Get<int>() >= 0
                    && !string.IsNullOrWhiteSpace(new Global(GTA.Offsets.Editor.SMS.txt + next).GetString()))
                    count++;
            }
            return count;
        }

        // Rules of any team that have a gang chase type set.
        private static int GangChaseRuleCount()
        {
            if (GTA.Offsets.Editor.gbtp == 0 || GTA.Offsets.Editor.nrl == 0)
                return 0;
            int count = 0;
            for (int team = 0; team < 4; team++)
            {
                int rules = Math.Min(new Global(GTA.Offsets.Editor.nrl + team * GTA.Offsets.Editor.team_NEXT).Get<int>(), 30);
                for (int rule = 0; rule < rules; rule++)
                    if (new Global(GTA.Offsets.Editor.gbtp + rule + team * GTA.Offsets.Editor.team_NEXT).Get<int>() > 0)
                        count++;
            }
            return count;
        }

        private static string SubLabel(EditNavSub sub)
        {
            return sub.Label ?? (sub.Button?.Content as string ?? sub.Button?.Content?.ToString() ?? "");
        }

        private void OpenEditNav(EditNavEntry entry, EditNavSub sub, bool rememberBack)
        {
            _editNavBack = null;
            if (rememberBack && _editNavEntry != null)
                _editNavBack = (_editNavEntry, _editNavSub);

            _editNavEntry = entry;
            _editNavOpen.Add(entry);
            _editNavSub = sub ?? entry.Subs.FirstOrDefault(s => s.Button != null && s.Button.Visibility == Visibility.Visible);
            _openingFromNav = true;
            try
            {
                if (sub?.Open != null)
                    sub.Open();
                else
                {
                    entry.Open();
                    if (_editNavSub?.Button != null)
                        ClickButton(_editNavSub.Button);
                }
            }
            finally
            {
                _openingFromNav = false;
            }
            UpdateEditHeader();
            RenderEditNav();
        }

        private bool _openingFromNav;

        // A page opened some other way (dashboard tiles, restrictions, code): follow it.
        private void OnEditPageChanged()
        {
            if (_openingFromNav)
                return;
            var page = EditPages.SelectedItem as TabItem;
            var entry = EditNav.FirstOrDefault(e => e.Page == page && e.Group < 2)
                ?? EditNav.FirstOrDefault(e => e.Subs.Any(s => s.Page == page));
            if (entry == null)
                return;
            _editNavEntry = entry;
            _editNavOpen.Add(entry);
            _editNavSub = entry.Subs.FirstOrDefault(s => s.Page == page)
                ?? entry.Subs.FirstOrDefault(s => s.Button != null && s.Button.Visibility == Visibility.Visible);
            _editNavBack = null;
            UpdateEditHeader();
            RenderEditNav();
        }

        private void UpdateEditHeader()
        {
            if (_editNavEntry == null)
                return;
            string page = TranslateOr(_editNavEntry.Key, _editNavEntry.Fallback);
            // "Props › Props" says nothing twice; the parent only shows when the sub page has its own name.
            bool hasSub = _editNavSub != null && _editNavEntry.Subs.Count > 1 && SubLabel(_editNavSub) != page;
            EditCrumbParent.Text = hasSub ? page + "  ›  " : "";
            HeaderLabel.Text = hasSub ? SubLabel(_editNavSub) : page;
            BtnEditBack.Visibility = _editNavBack.HasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnEditBack_Click(object sender, RoutedEventArgs e)
        {
            if (!_editNavBack.HasValue)
                return;
            var (entry, sub) = _editNavBack.Value;
            OpenEditNav(entry, sub, rememberBack: false);
        }

        private void BtnEditNavCollapse_Click(object sender, RoutedEventArgs e)
        {
            _editNavCollapsed = !_editNavCollapsed;
            EditNavColumn.Width = new GridLength(_editNavCollapsed ? 74 : 250);
            ((FrameworkElement)((FrameworkElement)tbEditNavSearch.Parent).Parent).Visibility = _editNavCollapsed ? Visibility.Collapsed : Visibility.Visible;
            EditScriptLabel.Visibility = EditScriptSummary.Visibility = _editNavCollapsed ? Visibility.Collapsed : Visibility.Visible;
            EditNavCollapseIcon.Data = (Geometry)FindResource(_editNavCollapsed ? "EditIconForward" : "EditIconBack");
            if (_editNavCollapsed)
                tbEditNavSearch.Text = "";
            RenderEditNav();
        }

        // ----- favourites: pages and sub-pages the user starred, kept in config.ini -----

        private List<string> _editNavFavorites;

        private List<string> EditNavFavorites
        {
            get
            {
                if (_editNavFavorites == null)
                {
                    string raw = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString("Settings", "editnavfav", "");
                    _editNavFavorites = raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return _editNavFavorites;
            }
        }

        // Entry key, or entry key + "#" + sub index (labels change with the language, indices do not).
        private static string FavoriteId(EditNavEntry entry, EditNavSub sub)
            => sub == null ? entry.Key : entry.Key + "#" + entry.Subs.IndexOf(sub).ToString(CultureInfo.InvariantCulture);

        private (EditNavEntry Entry, EditNavSub Sub) FromFavoriteId(string id)
        {
            string[] parts = id.Split('#');
            var entry = EditNav.FirstOrDefault(e => e.Key == parts[0]);
            if (entry == null || parts.Length == 1)
                return (entry, null);
            return int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) && i >= 0 && i < entry.Subs.Count
                ? (entry, entry.Subs[i]) : (null, null);
        }

        private void ToggleFavorite(string id)
        {
            if (!EditNavFavorites.Remove(id))
                EditNavFavorites.Add(id);
            new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "editnavfav", string.Join("|", EditNavFavorites));
            RenderEditNav();
        }

        private void RenderFavorites()
        {
            var favorites = EditNavFavorites.Select(FromFavoriteId)
                .Where(f => f.Entry != null && EditNavFits(f.Entry) && (f.Sub?.Button == null || f.Sub.Button.Visibility == Visibility.Visible))
                .ToList();
            if (favorites.Count == 0)
                return;
            if (!_editNavCollapsed)
                EditNavList.Children.Add(new TextBlock { Text = TranslateOr("editnav_grp_fav", "Favourites"), Style = (Style)FindResource("SideNavGroup") });
            foreach (var (entry, sub) in favorites)
            {
                // Compact like the sub-page rows: one line, the parent page after it in grey.
                string page = TranslateOr(entry.Key, entry.Fallback);
                var content = new StackPanel { Orientation = Orientation.Horizontal };
                var icon = NavIcon(entry.Icon);
                icon.Width = icon.Height = 14;
                content.Children.Add(icon);
                if (!_editNavCollapsed)
                {
                    content.Children.Add(new TextBlock { Text = sub == null ? page : SubLabel(sub), Style = (Style)FindResource("NavLabel") });
                    if (sub != null)
                        content.Children.Add(new TextBlock { Text = page, FontSize = 11, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)FindResource("NavMutedBrush") });
                }
                bool active = entry == _editNavEntry && (sub == null ? _editNavSub == null || entry.Subs.Count == 0 : sub == _editNavSub);
                var button = new Button
                {
                    Style = (Style)FindResource("SideNavButton"),
                    FontSize = 13,
                    Padding = new Thickness(10, 4, 10, 4),
                    Content = content,
                    Tag = active ? "active" : null,
                    ToolTip = _editNavCollapsed ? (sub == null ? page : SubLabel(sub)) : null,
                };
                button.Click += (_, __) => OpenEditNav(entry, sub, rememberBack: false);
                EditNavList.Children.Add(WithStar(button, FavoriteId(entry, sub)));
            }
        }

        private static ControlTemplate _starTemplate;

        // The row's button with a star on its right: filled for favourites, shown on hover otherwise.
        private FrameworkElement WithStar(Button button, string id)
        {
            if (_editNavCollapsed)
                return button;
            bool favorite = EditNavFavorites.Contains(id);
            if (_starTemplate == null)
                _starTemplate = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(
                    "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'>" +
                    "<Border Background='Transparent' Padding='6,0'><ContentPresenter VerticalAlignment='Center'/></Border></ControlTemplate>");
            var glyph = new TextBlock { Text = favorite ? "\u2605" : "\u2606", FontSize = 14 };
            glyph.SetResourceReference(TextBlock.ForegroundProperty, favorite ? "AccentBrush" : "NavMutedBrush");
            var star = new Button
            {
                Template = _starTemplate,
                Content = glyph,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, 0, 4, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = TranslateOr(favorite ? "editnav_unfav" : "editnav_fav", favorite ? "Remove from favourites" : "Add to favourites"),
                Opacity = favorite ? 1 : 0,
            };
            star.Click += (_, __) => ToggleFavorite(id);
            var row = new Grid();
            row.Children.Add(button);
            row.Children.Add(star);
            if (!favorite)
            {
                row.MouseEnter += (_, __) => star.Opacity = 1;
                row.MouseLeave += (_, __) => star.Opacity = 0;
            }
            return row;
        }

        // ----- search: pages and sub-pages of the open creator, then fields (MainWindow.EditSearch.cs) -----

        private void tbEditNavSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditNavResults.Children.Clear();
            string query = tbEditNavSearch.Text.Trim();
            EditNavList.Visibility = query.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (query.Length == 0)
                return;

            var hits = new List<(string Title, string Path, EditNavEntry Entry, EditNavSub Sub)>();
            foreach (var entry in EditNav.Where(EditNavFits))
            {
                string page = TranslateOr(entry.Key, entry.Fallback);
                if (Matches(page, query))
                    hits.Add((page, "", entry, null));
                foreach (var sub in entry.Subs.Where(s => s.Button == null || s.Button.Visibility == Visibility.Visible))
                {
                    string label = SubLabel(sub);
                    if (Matches(label, query))
                        hits.Add((label, page, entry, sub));
                }
            }

            var fields = SearchFields(query, 25);
            if (hits.Count == 0 && fields.Count == 0)
            {
                EditNavResults.Children.Add(new TextBlock
                {
                    Text = TranslateOr("editnav_nohits", "Nothing found."),
                    Margin = new Thickness(10, 6, 0, 0),
                    FontSize = 13,
                    Foreground = (Brush)FindResource("NavMutedBrush"),
                });
                return;
            }

            foreach (var hit in hits)
            {
                var text = new StackPanel();
                text.Children.Add(Highlighted(hit.Title, query));
                if (hit.Path.Length > 0)
                    text.Children.Add(new TextBlock { Text = hit.Path, FontSize = 11, Foreground = (Brush)FindResource("NavMutedBrush") });
                var button = new Button { Style = (Style)FindResource("SideNavButton"), Content = text };
                button.Click += (_, __) =>
                {
                    tbEditNavSearch.Text = "";
                    OpenEditNav(hit.Entry, hit.Sub, rememberBack: true);
                };
                EditNavResults.Children.Add(button);
            }

            if (fields.Count == 0)
                return;
            EditNavResults.Children.Add(new TextBlock { Text = TranslateOr("editnav_grp_fields", "On the pages"), Style = (Style)FindResource("SideNavGroup") });
            foreach (var field in fields)
            {
                var text = new StackPanel();
                var title = Highlighted(field.Title, query);
                title.FontSize = 13;
                title.FontWeight = FontWeights.SemiBold;
                title.TextTrimming = TextTrimming.CharacterEllipsis;
                text.Children.Add(title);
                text.Children.Add(new TextBlock { Text = field.Path, FontSize = 11, Foreground = (Brush)FindResource("NavMutedBrush"), TextTrimming = TextTrimming.CharacterEllipsis });
                var button = new Button { Style = (Style)FindResource("SideNavButton"), Content = text, Padding = new Thickness(10, 5, 10, 5) };
                var open = field.Open;
                button.Click += (_, __) =>
                {
                    tbEditNavSearch.Text = "";
                    open();
                };
                EditNavResults.Children.Add(button);
            }
        }

        private static bool Matches(string text, string query)
        {
            return text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        // The matching part in the accent colour.
        private TextBlock Highlighted(string text, string query)
        {
            var block = new TextBlock { FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("TextColor") };
            int at = text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
            if (at < 0)
            {
                block.Text = text;
                return block;
            }
            block.Inlines.Add(new System.Windows.Documents.Run(text.Substring(0, at)));
            block.Inlines.Add(new System.Windows.Documents.Run(text.Substring(at, query.Length)) { Foreground = DotWarn });
            block.Inlines.Add(new System.Windows.Documents.Run(text.Substring(at + query.Length)));
            return block;
        }

        // ----- script patches of the open creator -----

        // Green: written into the creator script. Yellow: switched on but not (yet) found in
        // the script -- after a game update the pattern may no longer match. Grey: off.
        private void UpdateEditScriptStatus()
        {
            string creator = _editNavCreator ?? "";
            if (creator.Length == 0)
            {
                EditScriptDot.Fill = DotOff;
                EditScriptSummary.Text = "–";
                return;
            }
            if (_scrPatchGroups.Count == 0)
                LoadScrPatchesPage();

            var states = EditScriptStates(creator);
            int on = states.Count(s => s.State == 2), waiting = states.Count(s => s.State == 1);
            EditScriptDot.Fill = states.Count == 0 ? DotOff : waiting > 0 ? DotWarn : on == states.Count ? DotOk : DotOff;
            EditScriptSummary.Text = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", on, states.Count);
            if (EditScriptPopup.IsOpen)
                FillEditScriptPopup(states);
        }

        // State: 0 off, 1 on but not in the script, 2 applied.
        private List<(string Name, string Description, int State)> EditScriptStates(string creator)
        {
            var result = new List<(string, string, int)>();
            foreach (var group in _scrPatchGroups)
            {
                var patches = group.Patches.Where(p => p.script_name == creator).ToList();
                if (patches.Count == 0)
                    continue;
                int state = !patches.All(p => p.enabled) ? 0 : patches.Any(ScrPatchesRunner.IsApplied) ? 2 : 1;
                result.Add((group.Name, group.Description, state));
            }
            return result;
        }

        private void BtnEditScriptStatus_Click(object sender, RoutedEventArgs e)
        {
            string creator = _editNavCreator ?? "";
            EditScriptPopupTitle.Text = creator.Length == 0
                ? TranslateOr("dash_creator_none", "No creator")
                : TranslateOr("adv_features_label", "Script features") + " · " + CreatorDisplayName(creator);
            FillEditScriptPopup(creator.Length == 0 ? new List<(string, string, int)>() : EditScriptStates(creator));
            EditScriptPopup.IsOpen = true;
        }

        private void FillEditScriptPopup(List<(string Name, string Description, int State)> states)
        {
            EditScriptList.Children.Clear();
            string[] reasons =
            {
                TranslateOr("editnav_patch_off", "Off"),
                TranslateOr("editnav_patch_waiting", "On, not found in the script yet"),
                TranslateOr("editnav_patch_on", "Active"),
            };
            Brush[] dots = { DotOff, DotWarn, DotOk };
            foreach (var (name, description, state) in states)
            {
                var row = new DockPanel { Margin = new Thickness(0, 3, 0, 3), ToolTip = string.IsNullOrEmpty(description) ? null : description };
                var dot = new Ellipse { Width = 8, Height = 8, Fill = dots[state], Margin = new Thickness(0, 5, 9, 0), VerticalAlignment = VerticalAlignment.Top };
                DockPanel.SetDock(dot, Dock.Left);
                row.Children.Add(dot);
                var text = new StackPanel();
                text.Children.Add(new TextBlock { Text = name, FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("TextColor"), TextWrapping = TextWrapping.Wrap });
                text.Children.Add(new TextBlock { Text = reasons[state], FontSize = 11, Foreground = state == 1 ? DotWarn : (Brush)FindResource("NavMutedBrush") });
                row.Children.Add(text);
                EditScriptList.Children.Add(row);
            }
            if (states.Count == 0)
                EditScriptList.Children.Add(new TextBlock { Text = TranslateOr("editnav_patch_none", "No script patches for this creator."), FontSize = 13, Foreground = (Brush)FindResource("NavMutedBrush") });
        }

        private void BtnEditScriptRefresh_Click(object sender, RoutedEventArgs e)
        {
            // The runner scans every quarter second on its own; this only reads the result again.
            UpdateEditScriptStatus();
            FillEditScriptPopup(EditScriptStates(_editNavCreator ?? ""));
        }

        private void BtnEditScriptMisc_Click(object sender, RoutedEventArgs e)
        {
            EditScriptPopup.IsOpen = false;
            MainPages.SelectedItem = PageMod;
            BtnModScrPatches_Click(sender, e);
        }
    }
}
