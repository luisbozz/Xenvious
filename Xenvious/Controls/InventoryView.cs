using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// A team's weapon inventory (mockup https://claude.ai/artifact/VifG8TGRs71syLT1EsWBmP, tab
    /// "Inventory"). The inventory is four bit fields per team (inv..inv4 = f_3838[team].f_61..64),
    /// one bit per weapon (Weapons.cs), so a whole category or a preset is only bits: each category
    /// is a row with a switch for all of it and its weapons as chips, plus presets and a search.
    /// The team comes from the page's team box; the tabs in the header pick it too.
    /// </summary>
    public class InventoryView : SectionCard
    {
        private readonly ComboBox _teamBox;
        private readonly StackPanel _body = new StackPanel();
        private readonly StackPanel _rows = new StackPanel();
        private readonly TextBox _search = new TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 8) };
        private readonly HashSet<string> _open = new HashSet<string>();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private string _shown;
        private bool _loading;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static readonly (string Key, string Fallback, Func<List<WeaponInventory>> List)[] Categories =
        {
            ("inv_pistol", "Pistol", () => Weapons.Pistols),
            ("inv_rifle", "Rifle", () => Weapons.Rifle),
            ("inv_mg", "MG", () => Weapons.MG),
            ("inv_sg", "Shotguns", () => Weapons.SG),
            ("inv_sniper", "Sniper", () => Weapons.Sniper),
            ("inv_explosive", "Explosive", () => Weapons.Explosive),
            ("inv_special", "Special", () => Weapons.Special),
            ("inv_melee", "Melee", () => Weapons.Meele),
        };

        public InventoryView(ComboBox teamBox)
        {
            _teamBox = teamBox;
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            Icon = Geometry.Parse("M3,8 L21,8 L21,20 L3,20 Z M8,8 L8,5 L16,5 L16,8 M3,13 L21,13");
            Content = _body;
            Margin = new Thickness(0, 0, 0, 12);
            _search.SetResourceReference(StyleProperty, "Watermark");
            _search.ToolTip = T("inv_search", "Search weapon …");
            _search.TextChanged += (_, __) => Refresh(true);
            _body.Children.Add(Presets());
            var searchLabel = new TextBlock { Text = T("inv_search", "Search weapon …") };
            searchLabel.SetResourceReference(StyleProperty, "FieldLabel");
            _body.Children.Add(searchLabel);
            _body.Children.Add(_search);
            _body.Children.Add(_rows);
            _body.Children.Add(Faint(T("inv_hint", "The switch turns a whole category on or off, a chip one weapon. Changes go to the game right away."), 11.5));
            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Refresh(false); };
            _teamBox.SelectionChanged += (_, __) => Refresh(true);
        }

        private int Team => Math.Max(0, Math.Min(_teamBox.SelectedIndex, 3));
        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.inv != 0 && GTA.Offsets.Editor.team_NEXT != 0;

        private static long Field(WeaponInventory w, int team)
        {
            long field;
            switch (w.InventoryOffset)
            {
                case InventoryOffset._1: field = GTA.Offsets.Editor.inv; break;
                case InventoryOffset._2: field = GTA.Offsets.Editor.inv2; break;
                case InventoryOffset._3: field = GTA.Offsets.Editor.inv3; break;
                default: field = GTA.Offsets.Editor.inv4; break;
            }
            return field == 0 ? 0 : field + team * GTA.Offsets.Editor.team_NEXT;
        }

        private static bool Has(WeaponInventory w, int team)
        {
            long at = Field(w, team);
            return at != 0 && Functions.Read.checkbinary(w.Bit, at);
        }

        private static void Set(WeaponInventory w, int team, bool on)
        {
            long at = Field(w, team);
            if (at != 0)
                Functions.Write.writebinary(w.Bit, at, on);
        }

        private static IEnumerable<WeaponInventory> All => Categories.SelectMany(c => c.List());

        private void Refresh(bool force)
        {
            if (!Live)
            {
                _rows.Children.Clear();
                _shown = null;
                Title = T("inv_title", "Inventory");
                _rows.Children.Add(Faint(T("inv_nocreator", "Open a job in the creator to see its inventory."), 13));
                return;
            }
            int team = Team;
            var state = All.Select(w => Has(w, team)).ToList();
            string filter = _search.Text?.Trim() ?? "";
            string key = team + "|" + Rules.Teams() + "|" + filter + "|" + string.Join(",", _open) + "|" + string.Concat(state.Select(b => b ? '1' : '0'));
            if (!force && key == _shown)
                return;
            _shown = key;
            Title = string.Format(CultureInfo.CurrentCulture, T("inv_title_team", "Inventory · team {0}"), team + 1);
            Summary = string.Format(CultureInfo.CurrentCulture, T("inv_sum", "{0} of {1} weapons"), state.Count(b => b), state.Count);
            HeaderRight = TeamTabs(team);

            _loading = true;
            _rows.Children.Clear();
            foreach (var cat in Categories)
            {
                var weapons = cat.List().Where(w => filter.Length == 0 || w.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                if (weapons.Count == 0)
                    continue;
                _rows.Children.Add(CategoryRow(cat.Key, T(cat.Key, cat.Fallback), cat.List(), weapons, team, filter.Length > 0));
            }
            if (_rows.Children.Count == 0)
                _rows.Children.Add(Faint(T("inv_nomatch", "No weapon matches."), 13));
            _loading = false;
        }

        private FrameworkElement CategoryRow(string key, string name, List<WeaponInventory> all, List<WeaponInventory> shown, int team, bool searching)
        {
            int on = all.Count(w => Has(w, team));
            bool open = searching || _open.Contains(key);
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 6) };

            var head = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Padding = new Thickness(10, 6, 10, 6), Cursor = System.Windows.Input.Cursors.Hand };
            head.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            head.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            var dock = new DockPanel();
            var all_ = new CheckBox { IsChecked = on == all.Count ? true : on == 0 ? (bool?)false : null, IsThreeState = false, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0),
                ToolTip = T("inv_all_tip", "All weapons of this category on or off") };
            all_.SetResourceReference(StyleProperty, "FormToggle");
            all_.Click += (_, __) =>
            {
                if (_loading || !Live) return;
                bool target = on != all.Count;
                foreach (var w in all) Set(w, team, target);
                Refresh(true);
            };
            DockPanel.SetDock(all_, Dock.Left);
            dock.Children.Add(all_);
            var chev = new TextBlock { Text = open ? "▾" : "▸", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
            chev.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            DockPanel.SetDock(chev, Dock.Right);
            dock.Children.Add(chev);
            var count = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontSize = 12,
                Text = on == all.Count ? string.Format(CultureInfo.CurrentCulture, T("inv_count_all", "all {0}"), all.Count) : on + " / " + all.Count };
            count.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            DockPanel.SetDock(count, Dock.Right);
            dock.Children.Add(count);
            var title = new TextBlock { Text = name, FontWeight = FontWeights.SemiBold, FontSize = 13.5, VerticalAlignment = VerticalAlignment.Center };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            dock.Children.Add(title);
            head.Child = dock;
            head.MouseLeftButtonUp += (_, e) =>
            {
                if (e.OriginalSource is DependencyObject d && IsInside(d, all_)) return;
                if (!_open.Remove(key)) _open.Add(key);
                Refresh(true);
            };
            panel.Children.Add(head);

            if (open)
            {
                var chips = new WrapPanel { Margin = new Thickness(6, 8, 0, 2) };
                foreach (var w in shown)
                {
                    var chip = new CheckBox { Content = w.Name, IsChecked = Has(w, team) };
                    chip.SetResourceReference(StyleProperty, "ChipToggle");
                    var weapon = w;
                    chip.Click += (_, __) =>
                    {
                        if (_loading || !Live) return;
                        Set(weapon, team, chip.IsChecked == true);
                        Refresh(true);
                    };
                    chips.Children.Add(chip);
                }
                panel.Children.Add(chips);
            }
            return panel;
        }

        private static bool IsInside(DependencyObject d, DependencyObject parent)
        {
            for (; d != null; d = d is Visual || d is System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(d) : LogicalTreeHelper.GetParent(d))
                if (d == parent) return true;
            return false;
        }

        // Presets replace the team's whole inventory; "from team N" copies another team's.
        private FrameworkElement Presets()
        {
            var panel = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
            var label = new TextBlock { Text = T("inv_presets", "Preset"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 6) };
            label.SetResourceReference(StyleProperty, "FieldLabel");
            panel.Children.Add(label);
            panel.Children.Add(PresetButton(T("inv_p_all", "All"), w => true));
            panel.Children.Add(PresetButton(T("inv_p_none", "Empty"), w => false));
            panel.Children.Add(PresetButton(T("inv_p_pistols", "Pistols only"), w => Weapons.Pistols.Contains(w)));
            panel.Children.Add(PresetButton(T("inv_p_melee", "Melee only"), w => Weapons.Meele.Contains(w)));
            var copy = new ComboBox { Height = 26, MinWidth = 130, Margin = new Thickness(0, 0, 6, 6), ToolTip = T("inv_copy_tip", "Takes over another team's inventory") };
            copy.Items.Add(new ComboBoxItem { Content = T("inv_copy", "Copy from team …"), IsEnabled = false });
            for (int t = 0; t < 4; t++)
                copy.Items.Add(new ComboBoxItem { Content = T("dash_team", "Team") + " " + (t + 1), Tag = t });
            copy.SelectedIndex = 0;
            copy.DropDownOpened += (_, __) =>
            {
                for (int i = 1; i < copy.Items.Count; i++)
                    ((ComboBoxItem)copy.Items[i]).Visibility = (int)((ComboBoxItem)copy.Items[i]).Tag < Rules.Teams() && (int)((ComboBoxItem)copy.Items[i]).Tag != Team ? Visibility.Visible : Visibility.Collapsed;
            };
            copy.SelectionChanged += (_, __) =>
            {
                if (!(copy.SelectedItem is ComboBoxItem item) || !(item.Tag is int from) || !Live) return;
                int team = Team;
                foreach (var w in All) Set(w, team, Has(w, from));
                copy.SelectedIndex = 0;
                Refresh(true);
            };
            panel.Children.Add(copy);
            return panel;
        }

        private Button PresetButton(string text, Func<WeaponInventory, bool> on)
        {
            var b = new Button { Content = text, Height = 26, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 6) };
            b.SetResourceReference(StyleProperty, "FormButton");
            b.Click += (_, __) =>
            {
                if (!Live) return;
                int team = Team;
                foreach (var w in All) Set(w, team, on(w));
                Refresh(true);
            };
            return b;
        }

        // Teams as nav tabs in their colours (team selector style); a click shows that team.
        private FrameworkElement TeamTabs(int selected)
        {
            int teams = Rules.Teams();
            if (teams < 2)
                return null;
            var tabs = new StackPanel { Orientation = Orientation.Horizontal };
            for (int t = 0; t < teams; t++)
            {
                int team = t;
                var tab = new ToggleButton { Content = (t + 1).ToString(CultureInfo.CurrentCulture), IsChecked = t == selected, ToolTip = T("dash_team", "Team") + " " + (t + 1) };
                tab.SetResourceReference(StyleProperty, "NavTab");
                if (MainWindow.ThemeBrush("TeamBrush" + (t + 1)) is SolidColorBrush colour)
                {
                    tab.Resources["AccentBrush"] = colour;
                    tab.Resources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x70, colour.Color.R, colour.Color.G, colour.Color.B));
                }
                tab.Click += (_, __) => { if (team < _teamBox.Items.Count) _teamBox.SelectedIndex = team; Refresh(true); };
                tabs.Children.Add(tab);
            }
            var box = new Border { Child = tabs };
            box.SetResourceReference(StyleProperty, "NavGroup");
            return box;
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }
    }
}
