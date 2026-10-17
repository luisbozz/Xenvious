using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Xenvious
{
    /// <summary>
    /// Weapons page: what the selected weapon gives when it is collected (WeaponPickups).
    /// The card itself shows, by kind, the script pickup types, the vehicle pickup types, or the
    /// ammo and attachments of a plain weapon. KindBar (weapon / script / vehicle pickup) goes
    /// under the model card, HideBar (hide for team 1..4, brest) into the Options card.
    /// </summary>
    public class WeaponPickupCard : Border
    {
        private readonly TextBlock _title = new TextBlock();
        private readonly StackPanel _body = new StackPanel { Margin = new Thickness(14, 12, 14, 6) };
        private readonly List<ToggleButton> _kindTabs = new List<ToggleButton>();
        private readonly List<ToggleButton> _hideTabs = new List<ToggleButton>();
        private int _index = -1;
        private string _shown;

        public FrameworkElement KindBar { get; }
        public FrameworkElement HideBar { get; }

        public WeaponPickupCard()
        {
            SetResourceReference(StyleProperty, "DashCard");
            Margin = new Thickness(0, 0, 0, 12);
            _title.SetResourceReference(StyleProperty, "DashCardTitle");
            var header = new Border { Child = _title };
            header.SetResourceReference(StyleProperty, "DashCardHeader");
            var dock = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            dock.Children.Add(_body);
            Child = dock;

            KindBar = BuildKindBar();
            HideBar = BuildHideBar();
        }

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static string Language() => Translation._Language.GetCode(MainWindow.Instance?.Translation);

        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen;

        // ----- kind and hide bars -----

        private FrameworkElement BuildKindBar()
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var label = new TextBlock { Text = T("wp_kind", "Pickup kind"), Margin = new Thickness(2, 0, 0, 4) };
            label.SetResourceReference(StyleProperty, "FieldLabel");
            panel.Children.Add(label);
            var tabs = new StackPanel { Orientation = Orientation.Horizontal };
            var kinds = new[]
            {
                (WeaponPickups.Kind.Weapon, T("wp_kind_weapon", "Weapon")),
                (WeaponPickups.Kind.Script, T("wp_kind_script", "Custom Script")),
                (WeaponPickups.Kind.Vehicle, T("wp_kind_vehicle", "Vehicle pickup")),
            };
            foreach (var (kind, text) in kinds)
            {
                var tab = new ToggleButton { Content = text };
                tab.SetResourceReference(StyleProperty, "NavTab");
                tab.Click += (_, __) =>
                {
                    if (_index >= 0 && Live)
                        WeaponPickups.SetKind(_index, kind);
                    Changed?.Invoke(this, EventArgs.Empty);
                    Show(_index, true);
                };
                _kindTabs.Add(tab);
                tabs.Children.Add(tab);
            }
            panel.Children.Add(Group(tabs));
            return panel;
        }

        private FrameworkElement BuildHideBar()
        {
            var panel = new StackPanel { Margin = new Thickness(0, 4, 0, 10) };
            var label = new TextBlock { Text = T("wp_hidefor", "Hide for") };
            label.SetResourceReference(StyleProperty, "FieldLabel");
            panel.Children.Add(label);
            var tabs = new StackPanel { Orientation = Orientation.Horizontal };
            for (int t = 0; t < 4; t++)
            {
                int team = t;
                var tab = new ToggleButton { Content = T("dash_team", "Team") + " " + (t + 1) };
                tab.SetResourceReference(StyleProperty, "NavTab");
                if (MainWindow.ThemeBrush("TeamBrush" + (t + 1)) is SolidColorBrush colour)
                {
                    tab.Resources["AccentBrush"] = colour;
                    tab.Resources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x70, colour.Color.R, colour.Color.G, colour.Color.B));
                }
                tab.Click += (_, __) =>
                {
                    if (_index >= 0 && Live)
                        WeaponPickups.SetHiddenFor(_index, team, tab.IsChecked == true);
                };
                _hideTabs.Add(tab);
                tabs.Children.Add(tab);
            }
            panel.Children.Add(Group(tabs));
            var hint = Muted(T("wp_hidefor_tip", "Pressed: the team does not see this weapon."));
            hint.Margin = new Thickness(2, 4, 0, 0);
            panel.Children.Add(hint);
            return panel;
        }

        private static Border Group(FrameworkElement content)
        {
            var box = new Border { Child = content, HorizontalAlignment = HorizontalAlignment.Left };
            box.SetResourceReference(StyleProperty, "NavGroup");
            return box;
        }

        /// <summary>Raised after the card wrote a change the page shows elsewhere (model, raw fields).</summary>
        public event EventHandler Changed;

        // ----- showing -----

        /// <summary>Shows weapon <paramref name="index"/>; redraws only when something changed, and
        /// not while a field of the card is being edited.</summary>
        public void Show(int index, bool force = false)
        {
            _index = index;
            bool ready = index >= 0 && Live;
            KindBar.IsEnabled = ready;
            HideBar.IsEnabled = ready;
            IsEnabled = ready;
            if (!ready)
            {
                _shown = null;
                _title.Text = T("wp_card_weapon", "Ammo & attachments");
                _body.Children.Clear();
                return;
            }
            var kind = WeaponPickups.KindOf(index);
            for (int k = 0; k < _kindTabs.Count; k++)
                _kindTabs[k].IsChecked = k == (int)kind;
            int teams = Rules.Teams();
            for (int t = 0; t < _hideTabs.Count; t++)
            {
                _hideTabs[t].IsChecked = WeaponPickups.HiddenFor(index, t);
                _hideTabs[t].Opacity = t < teams ? 1 : 0.45;
            }

            string state = string.Join(",", index, WeaponPickups.Model(index), WeaponPickups.ScriptType(index), WeaponPickups.VehicleType(index),
                WeaponPickups.Variant(index), WeaponPickups.Clip(index), WeaponPickups.AmmoForAll(index),
                WeaponPickups.Lowers(index, 17), WeaponPickups.Lowers(index, 18), WeaponPickups.ControllerRelease(), WeaponPickups.SpecialSeconds(15), WeaponPickups.SpecialSeconds(16), string.Join(".", WeaponPickups.Components(index)), Language());
            if (!force && (state == _shown || IsKeyboardFocusWithin || _body.Children.OfType<FrameworkElement>().Any(Busy)))
                return;
            _shown = state;
            _body.Children.Clear();
            switch (kind)
            {
                case WeaponPickups.Kind.Script:
                    _title.Text = T("wp_card_script", "Script pickup");
                    DrawScript(index);
                    break;
                case WeaponPickups.Kind.Vehicle:
                    _title.Text = T("wp_card_vehicle", "Vehicle pickup");
                    DrawVehicle(index);
                    break;
                default:
                    _title.Text = T("wp_card_weapon", "Ammo & attachments");
                    DrawWeapon(index);
                    break;
            }
        }

        private static bool Busy(FrameworkElement e) =>
            e is ComboBox c && c.IsDropDownOpen || e is Panel p && p.Children.OfType<FrameworkElement>().Any(Busy);

        private void Redraw()
        {
            Changed?.Invoke(this, EventArgs.Empty);
            Show(_index, true);
        }

        // ----- script pickup -----

        private void DrawScript(int index)
        {
            int current = WeaponPickups.ScriptType(index);
            var type = WeaponPickups.ScriptTypes.FirstOrDefault(t => t.Value == current);
            _body.Children.Add(Heading(T("wp_grp_item", "Mission items")));
            _body.Children.Add(Tiles(WeaponPickups.ScriptTypes.Where(t => t.Group == "item"), current, v => { WeaponPickups.SetScriptType(index, v); Redraw(); }));
            _body.Children.Add(Heading(T("wp_grp_power", "Power-ups")));
            _body.Children.Add(Tiles(WeaponPickups.ScriptTypes.Where(t => t.Group == "power"), current, v => { WeaponPickups.SetScriptType(index, v); Redraw(); }));
            _body.Children.Add(EffectNote(type));
            if (type != null && type.Variants.Length > 0)
            {
                _body.Children.Add(Label(type.Value == 12 ? T("wp_item", "Item") : T("wp_variant", "Variant")));
                var box = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                foreach (var v in type.Variants)
                    box.Items.Add(new ComboBoxItem { Content = T(v.Key, v.Fallback), Tag = v.Value });
                box.SelectedItem = box.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == WeaponPickups.Variant(index));
                box.SelectionChanged += (_, __) =>
                {
                    if (box.SelectedItem is ComboBoxItem item && Live)
                        WeaponPickups.SetVariant(index, (int)item.Tag);
                    Changed?.Invoke(this, EventArgs.Empty);
                };
                _body.Children.Add(box);
            }
            if (current == 15)
            {
                var row = Columns(2);
                var amount = Cell(row, 0, T("wp_ammo_amount", "Amount"));
                amount.Children.Add(Number(WeaponPickups.Clip(index), v => WeaponPickups.SetClip(index, v)));
                var target = Cell(row, 2, T("wp_ammo_for", "For"));
                var forBox = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                forBox.Items.Add(T("wp_ammo_current", "the weapon in hand"));
                forBox.Items.Add(T("wp_ammo_all", "all weapons"));
                forBox.SelectedIndex = WeaponPickups.AmmoForAll(index) ? 1 : 0;
                forBox.SelectionChanged += (_, __) => { if (Live) WeaponPickups.SetAmmoForAll(index, forBox.SelectedIndex == 1); };
                target.Children.Add(forBox);
                _body.Children.Add(row);
            }
            _body.Children.Add(Muted(T("wp_kind_hint", "Of the creators only the Deathmatch Creator offers these in its menu. Mission, LTS and Capture run them from the job.")));
        }

        // ----- vehicle pickup -----

        private void DrawVehicle(int index)
        {
            int current = WeaponPickups.VehicleType(index);
            foreach (var (group, key, fallback) in new[] { ("weapon", "wp_grp_weapon", "Weapons"), ("drive", "wp_grp_drive", "Driving"), ("other", "wp_grp_other", "Other") })
            {
                _body.Children.Add(Heading(T(key, fallback)));
                _body.Children.Add(Tiles(WeaponPickups.VehicleTypes.Where(t => t.Group == group), current, v => { WeaponPickups.SetVehicleType(index, v); Redraw(); },
                    (current == 17 || current == 18) && WeaponPickups.Lowers(index, current)));
            }
            _body.Children.Add(EffectNote(WeaponPickups.VehicleTypes.FirstOrDefault(t => t.Value == current)));
            if (current >= 14 && current <= 16)
            {
                var row = Columns(2);
                int column = 0;
                foreach (var (special, key, fallback) in new[] { (15, "wp_vp_15_secs", "Seconds as Ruiner 2000"), (16, "wp_vp_16_secs", "Seconds as Ramp Buggy") })
                {
                    if (current != 14 && current != special)
                        continue;
                    int vt = special;
                    var cell = Cell(row, column, T(key, fallback));
                    cell.Children.Add(Number(Math.Max(0, WeaponPickups.SpecialSeconds(vt)), v => WeaponPickups.SetSpecialSeconds(vt, v)));
                    if (WeaponPickups.SpecialSeconds(vt) == 0)
                    {
                        var warn = Muted(T("wp_secs_zero", "0: the vehicle turns back at once. A new LTS uses 15."));
                        warn.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                        cell.Children.Add(warn);
                    }
                    column += 2;
                }
                _body.Children.Add(row);
                _body.Children.Add(Muted(T("wp_secs_hint", "Job options for all special vehicle pickups (vwp15, vwp16). After this time the player is put back into the vehicle they had.")));
            }
            if (current == 17 || current == 18)
            {
                _body.Children.Add(Label(current == 17 ? T("wp_vp_17", "Blast size") : T("wp_vp_18", "Bomb count")));
                var dir = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                dir.Items.Add(T(current == 17 ? "wp_vp_17_up" : "wp_vp_18_up", "Increase"));
                dir.Items.Add(T(current == 17 ? "wp_vp_17_down" : "wp_vp_18_down", "Decrease"));
                dir.SelectedIndex = WeaponPickups.Lowers(index, current) ? 1 : 0;
                dir.SelectionChanged += (_, __) => { if (Live) WeaponPickups.SetLowers(index, current, dir.SelectedIndex == 1); Redraw(); };
                _body.Children.Add(dir);
            }
            // Label left, switch right: the plain CheckBox style here shows no text.
            var glow = new CheckBox { IsChecked = WeaponPickups.LowGlow(index), VerticalAlignment = VerticalAlignment.Center };
            glow.SetResourceReference(StyleProperty, "FormToggle");
            glow.Click += (_, __) => { if (Live) WeaponPickups.SetLowGlow(index, glow.IsChecked == true); Redraw(); };
            var glowRow = new DockPanel { Margin = new Thickness(0, 4, 0, 8) };
            DockPanel.SetDock(glow, Dock.Right);
            glowRow.Children.Add(glow);
            var glowText = new TextBlock { Text = T("wp_lowglow", "Weaker glow"), FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
            glowText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            glowRow.Children.Add(glowText);
            _body.Children.Add(glowRow);
            _body.Children.Add(Muted(T("wp_veh_hint", "Collected from a vehicle. The game places it 0.4 m to the side (special vehicles 3 m) and gives it its own blip.")));
            _body.Children.Add(Muted(T("wp_kind_hint", "Of the creators only the Deathmatch Creator offers these in its menu. Mission, LTS and Capture run them from the job.")));
        }

        // ----- plain weapon: ammo and attachments -----

        private void DrawWeapon(int index)
        {
            int pickup = WeaponPickups.Model(index);
            var weapon = WeaponPickups.WeaponOf(pickup);
            int size = WeaponPickups.ClipSize(weapon);
            int clips = WeaponPickups.Clip(index);

            _body.Children.Add(Label(T("wp_ammo", "Ammo")));
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            var result = Muted("");
            result.VerticalAlignment = VerticalAlignment.Center;
            result.Margin = new Thickness(10, 0, 0, 0);
            void ShowResult(int n) => result.Text = n <= 0 ? T("wp_ammo_default", "the weapon's default ammo")
                : size > 0 ? string.Format(CultureInfo.CurrentCulture, T("wp_ammo_calc", "= {0} rounds ({1} × {2})"), n * size, n, size)
                : T("wp_ammo_mags", "magazines");
            var box = Number(clips, v => { WeaponPickups.SetClip(index, v); ShowResult(v); });
            box.Width = 70;
            box.Margin = new Thickness(4, 0, 4, 0);
            row.Children.Add(Step("−", () => box.Text = Math.Max(0, Parse(box.Text) - 1).ToString(CultureInfo.InvariantCulture)));
            row.Children.Add(box);
            row.Children.Add(Step("+", () => box.Text = (Parse(box.Text) + 1).ToString(CultureInfo.InvariantCulture)));
            row.Children.Add(result);
            ShowResult(clips);
            _body.Children.Add(row);
            _body.Children.Add(Muted(T("wp_ammo_hint", "Magazines: the player gets this many times the weapon's magazine size. 0 = the weapon's default ammo.")));

            _body.Children.Add(Label(T("wp_comps", "Attachments")));
            var slots = weapon?.Slots.Where(s => s.Items.Any(i => i != s.Default)).ToList();
            if (slots == null || slots.Count == 0)
            {
                _body.Children.Add(Muted(T("wp_comps_none", "No attachments for this weapon.")));
                return;
            }
            var lang = Language();
            var set = WeaponPickups.Components(index);
            var grid = Columns(2);
            int cell = 0;
            foreach (var slot in slots)
            {
                var items = slot.Items.Select(WeaponPickups.ComponentByName).Where(c => c != null).ToList();
                var panel = Cell(grid, (cell % 2) * 2, SlotName(slot.Bone), cell / 2);
                var combo = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                var standard = WeaponPickups.ComponentByName(slot.Default);
                combo.Items.Add(new ComboBoxItem { Content = standard != null ? T("wp_comps_default", "Standard") + " · " + standard.Label(lang) : "–", Tag = 0 });
                foreach (var c in items.Where(c => c.Name != slot.Default))
                    combo.Items.Add(new ComboBoxItem { Content = c.Label(lang), Tag = c.Hash, ToolTip = c.Name });
                var chosen = items.FirstOrDefault(c => c.Name != slot.Default && set.Contains(c.Hash));
                combo.SelectedItem = combo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == (chosen?.Hash ?? 0));
                combo.SelectionChanged += (_, __) =>
                {
                    if (!Live || !(combo.SelectedItem is ComboBoxItem item))
                        return;
                    // One component per slot: take the slot's others out first.
                    foreach (var c in items)
                        WeaponPickups.SetComponent(index, c.Hash, false);
                    int hash = (int)item.Tag;
                    if (hash != 0 && !WeaponPickups.SetComponent(index, hash, true))
                        _ = MainWindow.Instance?.ConfirmAsync(T("wp_comps", "Attachments"), T("wp_comps_full", "All six component slots are in use."), "OK", null);
                    Changed?.Invoke(this, EventArgs.Empty);
                };
                panel.Children.Add(combo);
                cell++;
            }
            _body.Children.Add(grid);
            _body.Children.Add(Muted(T("wp_comps_hint", "Given to the player's weapon when the pickup is collected. The creator shows the first attachment with a model on the pickup.")));
        }

        private static string SlotName(string bone)
        {
            string b = bone.ToLowerInvariant();
            if (b.StartsWith("wapclip")) return T("wp_slot_clip", "Magazine");
            if (b.StartsWith("wapflsh") || b.StartsWith("torch")) return T("wp_slot_flash", "Flashlight");
            if (b.StartsWith("wapsupp")) return T("wp_slot_supp", "Muzzle");
            if (b.StartsWith("wapscop")) return T("wp_slot_scope", "Scope");
            if (b.StartsWith("wapgrip")) return T("wp_slot_grip", "Grip");
            if (b.StartsWith("wapbarrel")) return T("wp_slot_barrel", "Barrel");
            return T("wp_slot_finish", "Finish");
        }

        // ----- small parts -----

        // The types as tiles with their in-game blip icon; the chosen one is outlined.
        private FrameworkElement Tiles(IEnumerable<WeaponPickups.PickupType> types, int current, Action<int> pick, bool lowers = false)
        {
            var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
            foreach (var type in types)
            {
                bool on = type.Value == current;
                var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                string uri = WeaponPickups.IconUri(type, on && lowers);
                if (uri != null)
                    stack.Children.Add(new Image { Source = new BitmapImage(new Uri(uri, UriKind.Absolute)), Width = 30, Height = 30,
                        Margin = new Thickness(0, 0, 0, 5), SnapsToDevicePixels = true });
                var name = new TextBlock { Text = T(type.Key, type.Fallback), FontSize = 11.5, TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap, FontWeight = on ? FontWeights.Bold : FontWeights.Normal };
                name.SetResourceReference(TextBlock.ForegroundProperty, on ? "TextColor" : "MutedTextBrush");
                stack.Children.Add(name);
                var tile = new Border { Width = 92, MinHeight = 76, Margin = new Thickness(0, 0, 6, 6), Padding = new Thickness(4, 8, 4, 6),
                    CornerRadius = new CornerRadius(7), BorderThickness = new Thickness(on ? 2 : 1), Child = stack, Cursor = Cursors.Hand,
                    ToolTip = type.Model };
                tile.SetResourceReference(Border.BackgroundProperty, on ? "AccentSoftBrush" : "DeepBrush");
                tile.SetResourceReference(Border.BorderBrushProperty, on ? "AccentBrush" : "LineBrush");
                if (type.Effect != WeaponPickups.Effect.Any && !Works(type.Effect))
                    stack.Opacity = 0.55;
                tile.MouseEnter += (_, __) => { if (!on) tile.SetResourceReference(Border.BorderBrushProperty, "AccentBrush"); };
                tile.MouseLeave += (_, __) => { if (!on) tile.SetResourceReference(Border.BorderBrushProperty, "LineBrush"); };
                tile.MouseLeftButtonUp += (_, __) =>
                {
                    if (Live)
                        pick(type.Value);
                };
                wrap.Children.Add(tile);
            }
            return wrap;
        }

        private static bool Works(WeaponPickups.Effect effect) =>
            effect == WeaponPickups.Effect.Any || effect == WeaponPickups.Effect.Heist || WeaponPickups.ClassicController();

        // Whether the chosen type does something in this job, and what to change when not.
        private FrameworkElement EffectNote(WeaponPickups.PickupType type)
        {
            if (type == null)
                return new Border();
            bool ok = Works(type.Effect);
            string text;
            switch (type.Effect)
            {
                case WeaponPickups.Effect.Any:
                    text = T("wp_eff_any", "Works in every mission controller.");
                    break;
                case WeaponPickups.Effect.Heist:
                    text = T("wp_eff_heist", "Only does something in heist jobs (the heist bag option). Elsewhere it is a bag without effect.");
                    break;
                default:
                    text = ok
                        ? T("wp_eff_classic_ok", "Works: this job runs the classic fm_mission_controller (dlcrel below 2), which handles the effect.")
                        : string.Format(CultureInfo.CurrentCulture, T("wp_eff_classic_no",
                            "No effect in this job: only the classic fm_mission_controller handles it, and freemode only sends it while dlcrel is below 2. This job has dlcrel {0}."),
                            WeaponPickups.ControllerRelease());
                    if (type.Effect == WeaponPickups.Effect.ClassicTwoTeams)
                        text += " " + T("wp_eff_twoteams", "Only for teams 1 and 2.");
                    break;
            }
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            var mark = new TextBlock { Text = ok ? "✓" : "!", FontWeight = FontWeights.Bold, FontSize = 14, Width = 18, VerticalAlignment = VerticalAlignment.Top };
            mark.SetResourceReference(TextBlock.ForegroundProperty, ok ? "OkBrush" : "WarnBrush");
            DockPanel.SetDock(mark, Dock.Left);
            row.Children.Add(mark);
            var stack = new StackPanel();
            var t = new TextBlock { Text = text, FontSize = 12, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            stack.Children.Add(t);
            if (!ok)
            {
                var fix = new Button { Content = T("wp_eff_fix", "Use the classic controller (dlcrel 1)"), Height = 28, Padding = new Thickness(10, 0, 10, 0),
                    Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Left,
                    ToolTip = T("wp_eff_fix_tip", "The classic controller lacks features added since 2020 (newer rule and zone options). Restart GTA before testing.") };
                fix.SetResourceReference(StyleProperty, "FormButton");
                fix.Click += (_, __) => { WeaponPickups.UseClassicController(); Redraw(); };
                stack.Children.Add(fix);
            }
            row.Children.Add(stack);
            var box = new Border { Child = row, CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 8, 10, 0), Margin = new Thickness(0, 0, 0, 10) };
            box.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            return box;
        }

        private static TextBlock Heading(string text)
        {
            var t = new TextBlock { Text = text.ToUpper(CultureInfo.CurrentCulture), FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(2, 2, 0, 4) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            return t;
        }

        private static TextBlock Label(string text)
        {
            var t = new TextBlock { Text = text };
            t.SetResourceReference(StyleProperty, "FieldLabel");
            return t;
        }

        private static TextBlock Muted(string text)
        {
            var t = new TextBlock { Text = text, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            return t;
        }

        private static int Parse(string text) =>
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;

        private static TextBox Number(int value, Action<int> write)
        {
            var box = new TextBox { Height = 30, Text = value.ToString(CultureInfo.InvariantCulture), Margin = new Thickness(0, 0, 0, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center };
            box.SetResourceReference(StyleProperty, "Watermark");
            box.TextChanged += (_, __) =>
            {
                if (Live && int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= 0)
                    write(v);
            };
            return box;
        }

        private static Button Step(string text, Action click)
        {
            var b = new Button { Content = text, Width = 30, Height = 30, FontWeight = FontWeights.Bold };
            b.SetResourceReference(StyleProperty, "FormButton");
            b.Click += (_, __) => click();
            return b;
        }

        private static Grid Columns(int count)
        {
            var grid = new Grid();
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            return grid;
        }

        private static StackPanel Cell(Grid grid, int column, string label, int row = 0)
        {
            while (grid.RowDefinitions.Count <= row)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cell = new StackPanel();
            cell.Children.Add(Label(label));
            Grid.SetColumn(cell, column);
            Grid.SetRow(cell, row);
            grid.Children.Add(cell);
            return cell;
        }
    }
}
