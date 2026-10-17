using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xenvious
{
    /// <summary>
    /// Objects page: "turn into a vehicle while carrying". The classic fm_mission_controller
    /// (func_16023, 1.73) puts a player who carries an object into that object's vehicle for as long
    /// as they carry it, and back into their own vehicle 3 seconds after they drop it. It needs:
    /// - menubs20 bit 2 (job option, for all objects),
    /// - per object objtvi (f_417, index of a placed vehicle, whose model is used) or tl63vts
    ///   (f_401, a model name such as "rhino"); objtvi wins when both are set,
    /// - dlcrel below 2: fm_mission_controller_2020 and _v3 do not read these fields.
    /// No creator offers this in its menu; they only load and save it.
    /// </summary>
    public class ObjectTransformCard : Border
    {
        private const int OptionBit = 2;
        private readonly StackPanel _body = new StackPanel { Margin = new Thickness(14, 12, 14, 6) };
        private readonly TextBlock _title = new TextBlock();
        private Func<int> _index = () => -1;
        private string _shown;
        private int _kindOverride = -1;

        public ObjectTransformCard()
        {
            SetResourceReference(StyleProperty, "DashCard");
            Margin = new Thickness(0, 0, 0, 12);
            _title.SetResourceReference(StyleProperty, "DashCardTitle");
            _title.Text = T("ot_card", "Turn into a vehicle while carrying");
            var header = new Border { Child = _title };
            header.SetResourceReference(StyleProperty, "DashCardHeader");
            var dock = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            dock.Children.Add(_body);
            Child = dock;
        }

        /// <summary>Where the page keeps the selected object.</summary>
        public void Bind(Func<int> index) => _index = index;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen;

        private static long At(long field, int index) => field + (long)index * GTA.Offsets.Editor.Objects.NEXT;

        private static int VehicleIndex(int index) =>
            GTA.Offsets.Editor.Objects.tvi == 0 ? -1 : new Global(At(GTA.Offsets.Editor.Objects.tvi, index)).Get<int>();

        private static string ModelName(int index) =>
            GTA.Offsets.Editor.Objects.tvs == 0 ? "" : (new Global(At(GTA.Offsets.Editor.Objects.tvs, index)).GetString(63, false) ?? "").Trim('\0', ' ');

        private static void SetVehicleIndex(int index, int vehicle)
        {
            if (Live && GTA.Offsets.Editor.Objects.tvi != 0)
                new Global(At(GTA.Offsets.Editor.Objects.tvi, index)).SetInt(vehicle);
        }

        private static void SetModelName(int index, string name)
        {
            if (!Live || GTA.Offsets.Editor.Objects.tvs == 0)
                return;
            // The field is a 64 byte text label: pad with zeros so no rest of a longer name stays.
            name = (name ?? "").Trim();
            if (name.Length > 63)
                name = name.Substring(0, 63);
            new Global(At(GTA.Offsets.Editor.Objects.tvs, index)).SetString(name.PadRight(64, '\0'), false);
        }

        private static bool OptionOn() =>
            GTA.Offsets.Editor.menubs20 != 0 && (new Global(GTA.Offsets.Editor.menubs20).Get<int>() & (1 << OptionBit)) != 0;

        private static void SetOption(bool on)
        {
            if (!Live || GTA.Offsets.Editor.menubs20 == 0)
                return;
            var g = new Global(GTA.Offsets.Editor.menubs20);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << OptionBit) : v & ~(1 << OptionBit));
        }

        private static int PlacedVehicles() =>
            GTA.Offsets.Editor.Vehicle.number == 0 ? 0 : Math.Max(0, Math.Min(32, new Global(GTA.Offsets.Editor.Vehicle.number).Get<int>()));

        private static string VehicleName(int model) =>
            GTA.Editor.VehList?.FirstOrDefault(v => v.Int32 == model)?.Native ?? model.ToString(CultureInfo.InvariantCulture);

        private static int PlacedModel(int vehicle) =>
            new Global(GTA.Offsets.Editor.Vehicle.model + (long)vehicle * GTA.Offsets.Editor.Vehicle.NEXT).Get<int>();

        /// <summary>Redraws for the selected object when something changed.</summary>
        public void Refresh(bool force = false)
        {
            int index = _index();
            bool ready = index >= 0 && Live && GTA.Offsets.Editor.Objects.tvi != 0;
            IsEnabled = ready;
            if (!ready)
            {
                _shown = null;
                _body.Children.Clear();
                return;
            }
            int vehicle = VehicleIndex(index);
            string name = ModelName(index);
            string state = string.Join("|", index, vehicle, name, OptionOn(), WeaponPickups.ControllerRelease(), PlacedVehicles());
            if (!force && (state == _shown || IsKeyboardFocusWithin))
                return;
            _shown = state;
            _body.Children.Clear();

            // How it works first: the option has no menu in any creator, so nobody knows it.
            _body.Children.Add(Muted(T("ot_hint", "While a player carries this object they sit in the vehicle; 3 seconds after they drop it they are back in their own. No creator offers this in its menu.")));

            var option = new CheckBox { IsChecked = OptionOn(), VerticalAlignment = VerticalAlignment.Center };
            option.SetResourceReference(StyleProperty, "FormToggle");
            option.Click += (_, __) => { SetOption(option.IsChecked == true); Refresh(true); };
            var optionRow = new DockPanel { Margin = new Thickness(0, 0, 0, 12), ToolTip = T("ot_option_tip", "Job option menubs20 bit 2. Without it no object turns anyone into a vehicle.") };
            DockPanel.SetDock(option, Dock.Right);
            optionRow.Children.Add(option);
            var optionText = new TextBlock { Text = T("ot_option", "On for this job (all objects)"), FontSize = 13, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            optionText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            optionRow.Children.Add(optionText);
            _body.Children.Add(optionRow);

            // The object's vehicle: three tiles instead of a list, then what the chosen one needs.
            var target = new StackPanel { Opacity = OptionOn() ? 1 : 0.5 };
            _body.Children.Add(target);
            target.Children.Add(Label(T("ot_target", "Vehicle for this object")));
            int selected = vehicle >= 0 ? 1 : name.Length > 0 ? 2 : 0;
            var tiles = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 10) };
            string[] tileTexts = { T("ot_none", "none"), T("ot_placed_short", "Placed vehicle"), T("ot_model_short", "Vehicle model") };
            for (int t = 0; t < 3; t++)
            {
                int choice = t;
                var tile = new ToggleButton { Content = tileTexts[t], IsChecked = t == selected, Height = 34, Margin = new Thickness(0, 0, t < 2 ? 6 : 0, 0), FontSize = 13 };
                tile.SetResourceReference(StyleProperty, "ChoiceTile");
                tile.Click += (_, __) =>
                {
                    switch (choice)
                    {
                        case 0:
                            SetVehicleIndex(index, -1);
                            SetModelName(index, "");
                            break;
                        case 1:
                            SetModelName(index, "");
                            SetVehicleIndex(index, PlacedVehicles() > 0 ? Math.Max(0, VehicleIndex(index)) : -1);
                            break;
                        case 2:
                            SetVehicleIndex(index, -1);
                            if (ModelName(index).Length == 0)
                                SetModelName(index, "rhino");
                            break;
                    }
                    _kindOverride = choice;
                    Refresh(true);
                };
                tiles.Children.Add(tile);
            }
            target.Children.Add(tiles);
            if (_kindOverride == 1 && selected == 0)
                selected = 1;   // "placed vehicle" chosen while the job has none yet
            _kindOverride = -1;
            if (selected == 1)
            {
                int count = PlacedVehicles();
                if (count == 0)
                    target.Children.Add(Muted(T("ot_no_vehicles", "The job has no placed vehicles.")));
                var list = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                for (int v = 0; v < count; v++)
                    list.Items.Add(new ComboBoxItem { Content = string.Format(CultureInfo.CurrentCulture, "{0} {1} · {2}", T("ot_vehicle", "Vehicle"), v + 1, VehicleName(PlacedModel(v))), Tag = v });
                list.SelectedItem = list.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == vehicle);
                list.SelectionChanged += (_, __) =>
                {
                    if (list.SelectedItem is ComboBoxItem item)
                        SetVehicleIndex(index, (int)item.Tag);
                };
                target.Children.Add(list);
            }
            else if (selected == 2)
            {
                var box = new TextBox { Height = 30, Text = name, Margin = new Thickness(0, 0, 0, 4) };
                box.SetResourceReference(StyleProperty, "Watermark");
                var check = Muted("");
                void Check()
                {
                    bool known = GTA.Editor.VehList?.Any(v => string.Equals(v.Native, box.Text.Trim(), StringComparison.OrdinalIgnoreCase)) == true;
                    check.Text = known ? "" : T("ot_unknown", "Not a vehicle model Xenvious knows. The game then does nothing.");
                }
                box.TextChanged += (_, __) => { SetModelName(index, box.Text); Check(); };
                Check();
                target.Children.Add(box);
                var chips = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
                foreach (string model in new[] { "rhino", "khanjali", "apc", "insurgent3", "phantom2", "ruiner2", "dune4", "oppressor" })
                {
                    var chip = new Button { Content = model, Height = 26, Padding = new Thickness(8, 0, 8, 0), Margin = new Thickness(0, 0, 4, 4) };
                    chip.SetResourceReference(StyleProperty, "FormButton");
                    chip.Click += (_, __) => box.Text = model;
                    chips.Children.Add(chip);
                }
                target.Children.Add(chips);
                target.Children.Add(check);
            }

            // Only the classic controller does it: say so where it does not run, with the fix.
            if (!WeaponPickups.ClassicController())
            {
                var warn = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 2, 0, 10) };
                warn.SetResourceReference(Border.BorderBrushProperty, "WarnBrush");
                var warnBody = new StackPanel();
                var warnText = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8),
                    Text = string.Format(CultureInfo.CurrentCulture, T("ot_classic_no", "No effect in this job: only the classic fm_mission_controller does this (dlcrel below 2). This job has dlcrel {0}."), WeaponPickups.ControllerRelease()) };
                warnText.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                warnBody.Children.Add(warnText);
                var fix = new Button { Content = T("wp_eff_fix", "Use the classic controller (dlcrel 1)"), Height = 28, Padding = new Thickness(10, 0, 10, 0), HorizontalAlignment = HorizontalAlignment.Left,
                    ToolTip = T("wp_eff_fix_tip", "The classic controller lacks features added since 2020 (newer rule and zone options). Restart GTA before testing.") };
                fix.SetResourceReference(StyleProperty, "FormButton");
                fix.Click += (_, __) => { WeaponPickups.UseClassicController(); Refresh(true); };
                warnBody.Children.Add(fix);
                warn.Child = warnBody;
                _body.Children.Add(warn);
            }
            else
            {
                var ok = Muted(T("ot_classic_ok", "Runs: this job uses the classic fm_mission_controller (dlcrel below 2)."));
                ok.SetResourceReference(TextBlock.ForegroundProperty, "OkBrush");
                _body.Children.Add(ok);
            }
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
    }
}
