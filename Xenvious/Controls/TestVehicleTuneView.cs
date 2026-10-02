using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Race › Test vehicle: tunes the vehicle the player drives in a race creator test through the
    /// injected fn7 (TestVehicleTune). Cards for the vehicle and presets, performance, look and the
    /// body parts the model offers. The creator spawns a new vehicle on every restart of the test;
    /// the changes made here are kept per model and put on the new vehicle again.
    /// </summary>
    public class TestVehicleTuneView : DockPanel
    {
        private static readonly (int Slot, string Key, string Fallback)[] PerformanceSlots =
        {
            (11, "tv_engine", "Engine"), (12, "tv_brakes", "Brakes"), (13, "tv_transmission", "Transmission"),
            (15, "tv_suspension", "Suspension"), (16, "tv_armor", "Armor"),
        };

        private static readonly Dictionary<int, (string Key, string Fallback)> BodySlots = new Dictionary<int, (string, string)>
        {
            [0] = ("tv_spoiler", "Spoiler"), [1] = ("tv_front_bumper", "Front bumper"), [2] = ("tv_rear_bumper", "Rear bumper"),
            [3] = ("tv_skirts", "Side skirts"), [4] = ("tv_exhaust", "Exhaust"), [5] = ("tv_frame", "Roll cage"), [6] = ("tv_grille", "Grille"),
            [7] = ("tv_hood", "Hood"), [8] = ("tv_fender", "Fender"), [9] = ("tv_right_fender", "Right fender"), [10] = ("tv_roof", "Roof"),
            [14] = ("tv_horn", "Horn"), [25] = ("tv_plate_holder", "Plate holder"), [26] = ("tv_vanity_plate", "Vanity plate"),
            [27] = ("tv_trim_design", "Trim design"), [28] = ("tv_ornaments", "Ornaments"), [29] = ("tv_dashboard", "Dashboard"),
            [30] = ("tv_dials", "Dials"), [31] = ("tv_door_speakers", "Door speakers"), [32] = ("tv_seats", "Seats"),
            [33] = ("tv_steering_wheel", "Steering wheel"), [34] = ("tv_shifter", "Shifter"), [35] = ("tv_plaques", "Plaques"),
            [36] = ("tv_speakers", "Speakers"), [37] = ("tv_trunk", "Trunk"), [38] = ("tv_hydraulics", "Hydraulics"),
            [39] = ("tv_engine_block", "Engine block"), [40] = ("tv_air_filter", "Air filter"), [41] = ("tv_struts", "Struts"),
            [42] = ("tv_arch_covers", "Arch covers"), [43] = ("tv_aerials", "Aerials"), [44] = ("tv_trim", "Trim"),
            [45] = ("tv_tank", "Tank"), [46] = ("tv_windows", "Windows"), [49] = ("tv_lightbar", "Light bar"),
        };

        private static readonly string[] WheelTypeNames =
        {
            "Sport", "Muscle", "Lowrider", "SUV", "Offroad", "Tuner", "Bike", "High End", "Benny's Original", "Benny's Bespoke", "Open Wheel", "Street", "Track",
        };

        private static readonly string[] TintNames = { "None", "Pure Black", "Dark Smoke", "Light Smoke", "Stock", "Limo", "Green" };

        private static readonly string[] XenonNames =
        {
            "White", "Blue", "Electric Blue", "Mint Green", "Lime Green", "Yellow", "Golden Shower", "Orange", "Red", "Pony Pink", "Hot Pink", "Purple", "Blacklight",
        };

        // The game's paint names (index = colour id), as the mod shop shows them.
        private static readonly string[] PaintNames =
        {
            "Black", "Graphite Black", "Black Steel", "Dark Silver", "Silver", "Blue Silver", "Steel Gray", "Shadow Silver", "Stone Silver", "Midnight Silver",
            "Gun Metal", "Anthracite Gray", "Matte Black", "Matte Gray", "Matte Light Gray", "Util Black", "Util Black Poly", "Util Dark Silver", "Util Silver", "Util Gun Metal",
            "Util Shadow Silver", "Worn Black", "Worn Graphite", "Worn Silver Gray", "Worn Silver", "Worn Blue Silver", "Worn Shadow Silver", "Red", "Torino Red", "Formula Red",
            "Blaze Red", "Graceful Red", "Garnet Red", "Desert Red", "Cabernet Red", "Candy Red", "Sunrise Orange", "Classic Gold", "Orange", "Matte Red",
            "Matte Dark Red", "Matte Orange", "Matte Yellow", "Util Red", "Util Bright Red", "Util Garnet Red", "Worn Red", "Worn Golden Red", "Worn Dark Red", "Dark Green",
            "Racing Green", "Sea Green", "Olive Green", "Green", "Gasoline Blue Green", "Matte Lime Green", "Util Dark Green", "Util Green", "Worn Dark Green", "Worn Green",
            "Worn Sea Wash", "Midnight Blue", "Dark Blue", "Saxony Blue", "Blue", "Mariner Blue", "Harbor Blue", "Diamond Blue", "Surf Blue", "Nautical Blue",
            "Bright Blue", "Purple Blue", "Spinnaker Blue", "Ultra Blue", "Bright Blue", "Util Dark Blue", "Util Midnight Blue", "Util Blue", "Util Sea Foam Blue", "Util Lightning Blue",
            "Util Maui Blue Poly", "Util Bright Blue", "Matte Dark Blue", "Matte Blue", "Matte Midnight Blue", "Worn Dark Blue", "Worn Blue", "Worn Light Blue", "Taxi Yellow", "Race Yellow",
            "Bronze", "Yellow Bird", "Lime", "Champagne", "Pueblo Beige", "Dark Ivory", "Choco Brown", "Golden Brown", "Light Brown", "Straw Beige",
            "Moss Brown", "Biston Brown", "Beechwood", "Dark Beechwood", "Choco Orange", "Beach Sand", "Sun Bleached Sand", "Cream", "Util Brown", "Util Medium Brown",
            "Util Light Brown", "White", "Frost White", "Worn Honey Beige", "Worn Brown", "Worn Dark Brown", "Worn Straw Beige", "Brushed Steel", "Brushed Black Steel", "Brushed Aluminium",
            "Chrome", "Worn Off White", "Util Off White", "Worn Orange", "Worn Light Orange", "Securicor Green", "Worn Taxi Yellow", "Police Car Blue", "Matte Green", "Matte Brown",
            "Worn Orange", "Matte White", "Worn White", "Worn Olive Army Green", "Pure White", "Hot Pink", "Salmon Pink", "Vermillion Pink", "Orange", "Green",
            "Blue", "Black Blue", "Black Purple", "Black Red", "Hunter Green", "Purple", "V Dark Blue", "Modshop Black", "Matte Purple", "Matte Dark Purple",
            "Lava Red", "Matte Forest Green", "Matte Olive Drab", "Matte Desert Brown", "Matte Desert Tan", "Matte Foliage Green", "Default Alloy", "Epsilon Blue", "Pure Gold", "Brushed Gold",
        };

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private readonly StackPanel _left = new StackPanel(), _right = new StackPanel();
        private readonly TextBlock _vehicleName = new TextBlock { FontSize = 18, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis };
        private readonly StackPanel _status = new StackPanel();
        private readonly StackPanel _presets = new StackPanel();
        private readonly CheckBox _keep = new CheckBox { IsChecked = true };
        private SectionCard _performance, _look, _body;

        // Changes made here per model, put on the next vehicle of the same model.
        private readonly Dictionary<int, Dictionary<int, int>> _wanted = new Dictionary<int, Dictionary<int, int>>();
        private Dictionary<int, TestVehicleTune.Answer> _shown;
        private int _vehicle = -1;

        // Online (garage) vehicle used for the test, -1 = none.
        private int _onlineSlot = -1, _onlineModel;
        private SectionCard _online;
        private readonly ComboBox _garage = new ComboBox { Height = 30 };
        private readonly TextBlock _onlineState = new TextBlock { FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };
        private Button _scan, _use, _apply, _stop;
        private bool _busy, _built;

        // Option counts per model from earlier tests (TuneStore), so the page works before a test.
        private readonly Dictionary<int, Dictionary<int, int>> _counts = TuneStore.LoadCounts();
        private List<TunePreset> _presetList = TuneStore.LoadPresets();
        private SectionCard _presetCard;
        private readonly StackPanel _presetTiles = new StackPanel();
        private readonly StackPanel _testButtons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 4) };
        private readonly TextBox _presetName = new TextBox { Height = 30, VerticalContentAlignment = VerticalAlignment.Center };
        private readonly TextBlock _presetState = new TextBlock { FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };
        private string _renderedKey;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public TestVehicleTuneView()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Poll(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => Poll();
        }

        // ----- building -----

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            var grid = new Grid { Margin = new Thickness(0, 12, 0, 12) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 300 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 300 });
            grid.Children.Add(_left);
            Grid.SetColumn(_right, 2);
            grid.Children.Add(_right);
            scroll.Content = grid;
            Children.Add(scroll);

            _left.Children.Add(VehicleCard());
            _performance = Card(T("tv_performance", "Performance"), "M4,16 A8,8 0 1 1 20,16 M12,16 L16,9");
            _look = Card(T("tv_look", "Paint & wheels"), "M5,19 C5,13 9,11 12,4 C15,11 19,13 19,19 Z");
            _body = Card(T("tv_body", "Body parts"), "M3,15 L5,10 L9,8 L16,8 L20,11 L21,15 Z M6,15 A2,2 0 1 0 6,19 M17,15 A2,2 0 1 0 17,19");
            _left.Children.Add(_performance);
            _left.Children.Add(_look);
            _presetCard = PresetCard();
            _right.Children.Add(_presetCard);
            _online = OnlineCard();
            _right.Children.Add(_online);
            _right.Children.Add(_body);
            Render(true);
        }

        private SectionCard Card(string title, string icon)
        {
            return new SectionCard
            {
                Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard)),
                Title = title,
                Icon = Geometry.Parse(icon),
                Margin = new Thickness(0, 0, 0, 12),
                Content = new StackPanel(),
            };
        }

        private SectionCard VehicleCard()
        {
            var card = Card(T("tv_vehicle", "Test vehicle"), "M3,15 L5,10 L9,8 L16,8 L20,11 L21,15 Z");
            card.IsHero = true;
            card.CanCollapse = false;
            var body = (StackPanel)card.Content;
            body.Children.Add(_vehicleName);
            body.Children.Add(_testButtons);
            body.Children.Add(_status);

            var buttons = new WrapPanel { Margin = new Thickness(0, 4, 0, 4) };
            buttons.Children.Add(Button(T("tv_max_perf", "Max performance"), true, () => Preset(maxLook: false)));
            buttons.Children.Add(Button(T("tv_max_all", "Max everything"), false, () => Preset(maxLook: true)));
            buttons.Children.Add(Button(T("tv_stock", "All stock"), false, Stock));
            buttons.Children.Add(Button(T("tv_reload", "Read again"), false, () => { _vehicle = -1; Poll(); }));
            _presets.Children.Add(buttons);
            body.Children.Add(_presets);
            body.Children.Add(SwitchRow(T("tv_keep", "Put my changes on the vehicle when the test starts or restarts"), _keep));
            body.Children.Add(Faint(T("tv_hint", "Works on the vehicle you drive while testing the race. Only for this test: the job itself does not store tuning."), 11.5));
            return card;
        }

        private SectionCard OnlineCard()
        {
            var card = Card(T("tv_online", "Online vehicle"), "M4,10 L12,4 L20,10 L20,20 L4,20 Z M8,20 L8,14 L16,14 L16,20");
            var body = (StackPanel)card.Content;
            body.Children.Add(Faint(T("tv_online_hint", "Tests the race with a vehicle from your GTA Online garage, with its mods and paint. Xenvious sets it as the race's start vehicle; restart the test to drive it."), 12));
            _scan = Button(T("tv_online_scan", "Read my garage"), false, ScanGarage);
            _scan.Margin = new Thickness(0, 8, 0, 8);
            _scan.HorizontalAlignment = HorizontalAlignment.Left;
            body.Children.Add(_scan);
            var label = new TextBlock { Text = T("tv_online_pick", "Vehicle") };
            label.SetResourceReference(StyleProperty, "FieldLabel");
            body.Children.Add(label);
            body.Children.Add(_garage);
            var buttons = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
            _use = Button(T("tv_online_use", "Use for the test"), true, UseOnline);
            _apply = Button(T("tv_online_apply", "Put on now"), false, () => LoadOnline(true));
            _stop = Button(T("tv_online_stop", "Stop using"), false, () => { _onlineSlot = -1; Render(true); });
            buttons.Children.Add(_use);
            buttons.Children.Add(_apply);
            buttons.Children.Add(_stop);
            body.Children.Add(buttons);
            body.Children.Add(_onlineState);
            _garage.SelectionChanged += (_, __) => RenderOnline();
            return card;
        }

        private void ScanGarage()
        {
            if (_busy)
                return;
            _busy = true;
            _onlineState.Text = T("tv_online_reading", "Reading the garage …");
            Task.Run(() =>
            {
                var list = TestVehicleTune.ScanGarage();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _busy = false;
                    _garage.Items.Clear();
                    if (list == null)
                    {
                        _onlineState.Text = T("tv_no_answer", "The creator does not answer. Restart GTA if the script features were switched on while the creator was already open.");
                        return;
                    }
                    foreach (var v in list.OrderBy(v => VehicleName(v.Model)))
                    {
                        string text = VehicleName(v.Model) + (FindRaceVehicle(v.Model) == null ? "  (" + T("tv_online_no_race", "not a race vehicle") + ")" : "");
                        _garage.Items.Add(new ComboBoxItem { Content = text, Tag = v });
                    }
                    _onlineState.Text = list.Count == 0 ? T("tv_online_empty", "No vehicle found in the garage of this character.") : "";
                    if (_garage.Items.Count > 0)
                        _garage.SelectedIndex = 0;
                    RenderOnline();
                }));
            });
        }

        private TestVehicleTune.OnlineVehicle? Picked => (_garage.SelectedItem as ComboBoxItem)?.Tag as TestVehicleTune.OnlineVehicle?;

        private static (RaceVehicles.VehicleClass Class, RaceVehicles.Vehicle Vehicle)? FindRaceVehicle(int model)
        {
            uint hash = unchecked((uint)model);
            foreach (var c in RaceVehicles.Classes)
                foreach (var v in c.All)
                    if (v.Hash == hash)
                        return (c, v);
            return null;
        }

        // The creator spawns the race's start vehicle for a test, so the online vehicle's model
        // becomes the start vehicle: its class switched on, the vehicle allowed and picked.
        private void UseOnline()
        {
            var picked = Picked;
            var race = picked.HasValue ? FindRaceVehicle(picked.Value.Model) : null;
            if (!picked.HasValue || race == null || !RaceVehicles.Ready)
                return;
            MakeStartVehicle(picked.Value.Model);
            _onlineSlot = picked.Value.Slot;
            _onlineModel = picked.Value.Model;
            _vehicle = -1;
            LoadOnline(false);
        }

        // The creator spawns the race's start vehicle for a test: its class switched on, the
        // vehicle allowed and picked. False when the model is not in the race lists.
        private static bool MakeStartVehicle(int model)
        {
            var race = FindRaceVehicle(model);
            if (race == null || !RaceVehicles.Ready)
                return false;
            var (cls, vehicle) = race.Value;
            RaceVehicles.SetClass(cls.Index, true);
            RaceVehicles.SetAllowed(cls.Index, vehicle, true);
            new Global(GTA.Offsets.Editor.Race.Checkpoints.icv).SetInt(cls.Index);
            RaceVehicles.SetStartVehicle(vehicle.StartIndex);
            return true;
        }

        private void LoadOnline(bool userAsked)
        {
            int slot = userAsked && Picked.HasValue ? Picked.Value.Slot : _onlineSlot;
            if (slot < 0 || _busy)
            {
                RenderOnline();
                return;
            }
            _busy = true;
            Task.Run(() =>
            {
                var result = TestVehicleTune.LoadOnto(slot);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _busy = false;
                    if (userAsked && result != TestVehicleTune.LoadResult.Applied && result != TestVehicleTune.LoadResult.NoAnswer)
                        result = TestVehicleTune.LoadResult.NoAnswer + 1;
                    switch (result)
                    {
                        case TestVehicleTune.LoadResult.Applied: _onlineState.Text = T("tv_online_applied", "On the test vehicle."); _vehicle = -1; break;
                        case TestVehicleTune.LoadResult.OtherModel: _onlineState.Text = T("tv_online_restart", "Set as start vehicle. Restart the test (or start one) to drive it."); break;
                        case TestVehicleTune.LoadResult.NoVehicle: _onlineState.Text = T("tv_online_restart", "Set as start vehicle. Restart the test (or start one) to drive it."); break;
                        case TestVehicleTune.LoadResult.NoAnswer + 1: _onlineState.Text = T("tv_online_sit", "Sit in a vehicle of this model to put it on."); break;
                        default: _onlineState.Text = T("tv_no_answer", "The creator does not answer. Restart GTA if the script features were switched on while the creator was already open."); break;
                    }
                    RenderOnline();
                    Poll();
                }));
            });
        }

        private void RenderOnline()
        {
            if (_online == null)
                return;
            bool ready = TestVehicleTune.OnlineReady && TestVehicleTune.ScriptFeaturesOn;
            _online.IsEnabled = ready;
            var picked = Picked;
            bool race = picked.HasValue && FindRaceVehicle(picked.Value.Model) != null;
            _use.IsEnabled = race && RaceVehicles.Ready;
            _apply.IsEnabled = picked.HasValue;
            _stop.IsEnabled = _onlineSlot >= 0;
            _online.Summary = _onlineSlot >= 0 ? VehicleName(_onlineModel) : "";
            if (picked.HasValue && !race)
                _onlineState.Text = T("tv_online_not_race", "The creator has no race list entry for this model, so it cannot be the start vehicle. \"Put on now\" still works when you sit in one.");
        }

        private SectionCard PresetCard()
        {
            var card = Card(T("tv_presets", "Presets"), "M6,3 L18,3 L18,21 L12,16 L6,21 Z");
            var body = (StackPanel)card.Content;
            var nameLabel = new TextBlock { Text = T("tv_preset_name", "Name") };
            nameLabel.SetResourceReference(StyleProperty, "FieldLabel");
            body.Children.Add(nameLabel);
            _presetName.SetResourceReference(StyleProperty, "Watermark");
            _presetName.Tag = T("tv_preset_name_hint", "e.g. Osiris race setup");
            var saveRow = new DockPanel();
            var save = Button(T("tv_preset_save", "Save"), true, SavePreset);
            save.Margin = new Thickness(8, 0, 0, 0);
            DockPanel.SetDock(save, Dock.Right);
            saveRow.Children.Add(save);
            saveRow.Children.Add(_presetName);
            body.Children.Add(saveRow);
            body.Children.Add(_presetState);
            var listLabel = new TextBlock { Text = T("tv_presets_saved", "Saved presets"), FontSize = 15, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 14, 0, 8) };
            listLabel.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            body.Children.Add(listLabel);
            body.Children.Add(_presetTiles);
            return card;
        }

        // The race creator's own test entries (primary and secondary checkpoints), and ending a running test.
        private void RenderTestButtons(bool ready, bool testing)
        {
            _testButtons.Children.Clear();
            _testButtons.Visibility = TestVehicleTune.InRaceCreator ? Visibility.Visible : Visibility.Collapsed;
            if (testing)
            {
                _testButtons.Children.Add(Button(T("dash_endtest", "End test"), true, () => { MainWindow.Instance.EndRaceTestFromPage(); _renderedKey = null; }));
                return;
            }
            _testButtons.Children.Add(Button(T("tv_test_primary", "Test (primary checkpoints)"), true, () => MainWindow.Instance.StartRaceTest(false)));
            _testButtons.Children.Add(Button(T("tv_test_secondary", "Test (secondary checkpoints)"), false, () => MainWindow.Instance.StartRaceTest(true)));
        }

        private void RenderPresets()
        {
            if (_presetCard == null)
                return;
            int planned = PlannedModel;
            _presetTiles.Children.Clear();
            // This vehicle's presets first, then the newest.
            foreach (var p in _presetList.OrderBy(p => p.Model == planned ? 0 : 1).ThenByDescending(p => p.Saved))
                _presetTiles.Children.Add(PresetTile(p, p.Model == planned));
            if (_presetList.Count == 0)
                _presetTiles.Children.Add(Faint(T("tv_preset_none", "No preset yet. Tune the vehicle, give it a name and save."), 12.5));
            _presetCard.Summary = _presetList.Count > 0 ? _presetList.Count.ToString(CultureInfo.CurrentCulture) : "";
        }

        // One saved preset in the look of the job backup list: picture, name, vehicle and date,
        // Load and a delete cross.
        private FrameworkElement PresetTile(TunePreset preset, bool sameModel)
        {
            var tile = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 8) };
            tile.SetResourceReference(Border.BackgroundProperty, "SeactionHeaderBackgroundBrush");
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var frame = new Border { Width = 96, Height = 54, CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 12, 0), ClipToBounds = true };
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            var picture = TuneStore.LoadImage(preset.Image);
            if (picture != null)
                frame.Child = new Image { Source = picture, Stretch = Stretch.UniformToFill };
            else
                frame.Child = new Path
                {
                    Data = Geometry.Parse("M3,15 L5,10 L9,8 L16,8 L20,11 L21,15 Z M6,15 A2,2 0 1 0 6,19 M17,15 A2,2 0 1 0 17,19"),
                    Stretch = Stretch.Uniform, Width = 34, StrokeThickness = 1.5,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                }.Also(path => path.SetResourceReference(Shape.StrokeProperty, "FaintTextBrush"));
            grid.Children.Add(frame);

            var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var title = new TextBlock { Text = preset.Name, FontSize = 15, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            text.Children.Add(title);
            string sub = VehicleName(preset.Model) + "  ·  " + string.Format(CultureInfo.CurrentCulture, T("tv_preset_count", "{0} settings"), preset.Values.Count)
                + (preset.Saved != default ? "  ·  " + preset.Saved.ToString("g", CultureInfo.CurrentCulture) : "");
            var subtitle = new TextBlock { Text = sub, FontSize = 12, Margin = new Thickness(0, 2, 0, 0), TextWrapping = TextWrapping.Wrap };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            text.Children.Add(subtitle);
            if (!sameModel)
            {
                var other = new TextBlock { Text = T("tv_preset_other_short", "other vehicle: body parts are not carried over"), FontSize = 11.5, Margin = new Thickness(0, 2, 0, 0), TextWrapping = TextWrapping.Wrap };
                other.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                text.Children.Add(other);
            }
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var load = Button(T("tv_preset_load", "Load"), true, () => LoadPreset(preset));
            load.Margin = new Thickness(10, 0, 6, 0);
            actions.Children.Add(load);
            actions.Children.Add(DeleteCross(T("tv_preset_delete", "Delete"), () => DeletePreset(preset)));
            Grid.SetColumn(actions, 2);
            grid.Children.Add(actions);
            tile.Child = grid;
            return tile;
        }

        // Everything the page shows for the vehicle now: the values read from it in a test, the
        // values picked for the next test otherwise.
        private Dictionary<int, int> Snapshot()
        {
            var values = new Dictionary<int, int>();
            if (Live)
            {
                foreach (var kv in _shown)
                {
                    int slot = kv.Key;
                    bool offered = slot < TestVehicleTune.ModSlots ? kv.Value.Count > 0 || TestVehicleTune.IsToggle(slot)
                        : slot != TestVehicleTune.Livery || kv.Value.Count > 0;
                    if (offered)
                        values[slot] = kv.Value.Current;
                }
            }
            else
            {
                lock (_wanted)
                    if (_wanted.TryGetValue(PlannedModel, out var w))
                        foreach (var kv in w)
                            values[kv.Key] = kv.Value;
            }
            return values;
        }

        private async void SavePreset()
        {
            string name = _presetName.Text.Trim();
            int model = PlannedModel;
            var values = Snapshot();
            if (name.Length == 0 || model == 0)
            {
                _presetState.Text = T("tv_preset_need_name", "Give the preset a name.");
                return;
            }
            if (values.Count == 0)
            {
                _presetState.Text = T("tv_preset_empty", "Nothing to save yet: tune the vehicle first.");
                return;
            }
            var (ok, image) = await MainWindow.Instance.PickImageAsync(T("tv_preset_image_title", "Picture for the preset"),
                T("tv_preset_image_text", "Optional: take a screenshot of the game, paste an image with Ctrl+V or drop one here."),
                T("tv_preset_save", "Save"));
            if (!ok)
                return;
            foreach (var old in _presetList.Where(p => p.Name == name && p.Model == model).ToList())
            {
                TuneStore.DeleteImage(old.Image);
                _presetList.Remove(old);
            }
            _presetList.Add(new TunePreset { Name = name, Model = model, Values = values, Image = TuneStore.SaveImage(image), Saved = DateTime.Now });
            TuneStore.SavePresets(_presetList);
            _presetName.Text = "";
            _presetState.Text = string.Format(CultureInfo.CurrentCulture, T("tv_preset_saved", "Saved \"{0}\"."), name);
            RenderPresets();
        }

        private void LoadPreset(TunePreset preset)
        {
            var values = new Dictionary<int, int>(preset.Values);
            // A preset belongs to its vehicle: that vehicle becomes the start vehicle, and the
            // values wait for it (put on when the test starts, or now when you already sit in it).
            if (preset.Model != PlannedModel && MakeStartVehicle(preset.Model))
            {
                if (_onlineSlot >= 0 && _onlineModel != preset.Model)
                    _onlineSlot = -1;
                bool driving = Live;
                lock (_wanted)
                {
                    if (!_wanted.TryGetValue(preset.Model, out var w))
                        _wanted[preset.Model] = w = new Dictionary<int, int>();
                    foreach (var kv in values)
                        w[kv.Key] = kv.Value;
                }
                _presetState.Text = string.Format(CultureInfo.CurrentCulture,
                    driving ? T("tv_preset_start_restart", "Loaded \"{0}\". The {1} is the start vehicle now; restart the test to drive it.")
                            : T("tv_preset_start", "Loaded \"{0}\". The {1} is the start vehicle now; the preset goes on it when the test starts."),
                    preset.Name, VehicleName(preset.Model));
                Render(true);
                return;
            }
            // Body parts and liveries are numbered per model; on another model only what every
            // vehicle shares carries over (performance as its highest level, paint, wheel type, tint, lights).
            if (preset.Model != PlannedModel)
            {
                var shared = new HashSet<int> { 11, 12, 13, 15, 16, TestVehicleTune.Turbo, TestVehicleTune.TyreSmoke, TestVehicleTune.Xenon,
                    TestVehicleTune.WheelType, TestVehicleTune.Primary, TestVehicleTune.Secondary, TestVehicleTune.Pearl,
                    TestVehicleTune.WheelColour, TestVehicleTune.WindowTint, TestVehicleTune.XenonColour };
                values = values.Where(kv => shared.Contains(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => PerformanceSlots.Any(p => p.Slot == kv.Key) && kv.Value >= 0 ? TestVehicleTune.Max : kv.Value);
                _presetState.Text = T("tv_preset_other", "Preset of another vehicle: performance, paint, wheel type, tint and lights carried over, body parts not.");
            }
            else
                _presetState.Text = string.Format(CultureInfo.CurrentCulture, T("tv_preset_loaded", "Loaded \"{0}\"."), preset.Name);
            Send(values);
        }

        private async void DeletePreset(TunePreset preset)
        {
            if (!await MainWindow.Instance.ConfirmAsync(T("tv_preset_delete", "Delete"),
                string.Format(CultureInfo.CurrentCulture, T("tv_preset_delete_text", "Delete the preset \"{0}\"?"), preset.Name),
                T("tv_preset_delete", "Delete"), T("dialog_cancel", "Cancel"), danger: true))
                return;
            _presetList.Remove(preset);
            TuneStore.DeleteImage(preset.Image);
            TuneStore.SavePresets(_presetList);
            _presetState.Text = string.Format(CultureInfo.CurrentCulture, T("tv_preset_deleted", "Deleted \"{0}\"."), preset.Name);
            RenderPresets();
        }

        // The job backup list's delete cross: plain glyph, red on hover, no button chrome.
        private static Button DeleteCross(string tip, Action click)
        {
            var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(
                "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'>" +
                "<Border x:Name='b' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' Background='Transparent' CornerRadius='4' Padding='8,4'>" +
                "<TextBlock x:Name='x' Text='✕' FontSize='13' Foreground='{DynamicResource FaintTextBrush}' VerticalAlignment='Center'/></Border>" +
                "<ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'>" +
                "<Setter TargetName='x' Property='Foreground' Value='{DynamicResource BadBrush}'/></Trigger></ControlTemplate.Triggers>" +
                "</ControlTemplate>");
            var button = new Button { Template = template, Cursor = System.Windows.Input.Cursors.Hand, ToolTip = tip, VerticalAlignment = VerticalAlignment.Center };
            button.Click += (_, __) => click();
            return button;
        }

        private static Button Button(string text, bool primary, Action click)
        {
            var b = new Button { Content = text, Margin = new Thickness(0, 0, 8, 6), MinWidth = 90 };
            b.SetResourceReference(StyleProperty, primary ? "FormButtonPrimary" : "FormButton");
            b.Click += (_, __) => click();
            return b;
        }

        // ----- state -----

        private void Poll()
        {
            if (!_built || _busy)
                return;
            if (!TestVehicleTune.InRaceCreator || !TestVehicleTune.ScriptFeaturesOn)
            {
                _shown = null;
                _vehicle = -1;
                Render(false);
                return;
            }
            _busy = true;
            int known = _vehicle;
            bool keep = _keep.IsChecked == true;
            int onlineSlot = _onlineSlot, onlineModel = _onlineModel;
            Task.Run(() =>
            {
                Dictionary<int, TestVehicleTune.Answer> all = null;
                bool answered = TestVehicleTune.Request(TestVehicleTune.FrontWheels, TestVehicleTune.ReadOnly, out var probe);
                if (answered && probe.Vehicle != 0 && probe.Vehicle != known)
                {
                    // A new vehicle (first look, or the creator respawned it): first the online
                    // vehicle, then the changes kept from this page on top.
                    if (onlineSlot >= 0 && probe.Model == onlineModel)
                        TestVehicleTune.LoadOnto(onlineSlot);
                    Dictionary<int, int> wanted;
                    lock (_wanted)
                        wanted = keep && _wanted.TryGetValue(probe.Model, out var w) ? new Dictionary<int, int>(w) : null;
                    if (wanted != null && wanted.Count > 0)
                        TestVehicleTune.Apply(wanted);
                    all = TestVehicleTune.ReadAll();
                    answered = all != null;
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _busy = false;
                    if (!answered) { _shown = null; _vehicle = -2; }
                    else if (probe.Vehicle == 0) { _shown = null; _vehicle = 0; }
                    else if (all != null) { _shown = all; _vehicle = probe.Vehicle; Remember(all); }
                    else return;
                    Render(false);
                }));
            });
        }

        private void Send(IEnumerable<KeyValuePair<int, int>> values)
        {
            var list = values.ToList();
            int model = PlannedModel;
            if (model == 0 || _busy)
                return;
            lock (_wanted)
            {
                if (!_wanted.TryGetValue(model, out var w))
                    _wanted[model] = w = new Dictionary<int, int>();
                foreach (var kv in list)
                    w[kv.Key] = kv.Value;
            }
            if (!Live)
            {
                // Not in the vehicle: kept for the model and put on when the test starts (Poll).
                Render(true);
                return;
            }
            _busy = true;
            Task.Run(() =>
            {
                bool ok = TestVehicleTune.Apply(list);
                var all = ok ? TestVehicleTune.ReadAll() : null;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _busy = false;
                    if (all != null && all.Count > 0)
                    {
                        _shown = all;
                        Remember(all);
                    }
                    else
                        _vehicle = -1;
                    Render(true);
                }));
            });
        }

        private void Send(int slot, int value) => Send(new[] { new KeyValuePair<int, int>(slot, value) });

        private void Preset(bool maxLook)
        {
            if (PlannedModel == 0)
                return;
            // Without known counts the highest option is resolved on the vehicle (TestVehicleTune.Max).
            bool known = Live || _counts.ContainsKey(PlannedModel);
            var values = new List<KeyValuePair<int, int>>();
            foreach (var p in PerformanceSlots)
                if (!known || Count(p.Slot) > 0)
                    values.Add(new KeyValuePair<int, int>(p.Slot, known ? Count(p.Slot) - 1 : TestVehicleTune.Max));
            values.Add(new KeyValuePair<int, int>(TestVehicleTune.Turbo, 0));
            if (maxLook)
            {
                foreach (var slot in BodySlots.Keys)
                    if (!known || Count(slot) > 0)
                        values.Add(new KeyValuePair<int, int>(slot, known ? Count(slot) - 1 : TestVehicleTune.Max));
                values.Add(new KeyValuePair<int, int>(TestVehicleTune.Xenon, 0));
                values.Add(new KeyValuePair<int, int>(TestVehicleTune.WindowTint, 1));
            }
            Send(values);
        }

        private void Stock()
        {
            int model = PlannedModel;
            if (model == 0)
                return;
            var values = new List<KeyValuePair<int, int>>();
            for (int slot = 0; slot < TestVehicleTune.ModSlots; slot++)
                if (Count(slot) > 0 || TestVehicleTune.IsToggle(slot))
                    values.Add(new KeyValuePair<int, int>(slot, -1));
            values.Add(new KeyValuePair<int, int>(TestVehicleTune.WindowTint, 0));
            if (Live)
                Send(values);
            // Stock is the vehicle as the creator spawns it, so nothing is kept for the next one.
            lock (_wanted)
                _wanted.Remove(model);
            Render(true);
        }

        private bool Live => _shown != null && _shown.Count > 0;

        // The model the next test drives: the one you sit in, else the online vehicle, else the
        // race's start vehicle.
        private int PlannedModel
        {
            get
            {
                if (Live)
                    return _shown.Values.First().Model;
                if (_onlineSlot >= 0)
                    return _onlineModel;
                if (!RaceVehicles.Ready)
                    return 0;
                var c = RaceVehicles.Classes.FirstOrDefault(x => x.Index == RaceVehicles.StartClass);
                var v = c?.All.ElementAtOrDefault(RaceVehicles.StartVehicle);
                return v == null ? 0 : unchecked((int)v.Hash);
            }
        }

        private int Count(int slot)
        {
            if (Live)
                return _shown.TryGetValue(slot, out var a) ? a.Count : 0;
            return _counts.TryGetValue(PlannedModel, out var c) && c.TryGetValue(slot, out var n) ? n : 0;
        }

        private int Current(int slot)
        {
            if (Live)
                return _shown.TryGetValue(slot, out var a) ? a.Current : -1;
            lock (_wanted)
                if (_wanted.TryGetValue(PlannedModel, out var w) && w.TryGetValue(slot, out var v))
                    return v;
            return slot == TestVehicleTune.XenonColour ? 255 : -1;
        }

        // Keeps the option counts of the vehicle just read for this model.
        private void Remember(Dictionary<int, TestVehicleTune.Answer> all)
        {
            if (all.Count == 0)
                return;
            var counts = all.Where(kv => kv.Value.Count > 0).ToDictionary(kv => kv.Key, kv => kv.Value.Count);
            int model = all.Values.First().Model;
            if (_counts.TryGetValue(model, out var old) && old.Count == counts.Count && !old.Except(counts).Any())
                return;
            _counts[model] = counts;
            TuneStore.SaveCounts(_counts);
        }

        // ----- rendering -----

        private void Render(bool force)
        {
            if (!_built)
                return;
            bool live = Live;
            int planned = TestVehicleTune.InRaceCreator ? PlannedModel : 0;
            bool ready = TestVehicleTune.InRaceCreator && TestVehicleTune.ScriptFeaturesOn;
            // The timer polls every second; rebuilding the rows each time would close an open box.
            bool testing = MainWindow.Instance?.RaceTestActive == true;
            string key = string.Join("|", live, _vehicle, planned, ready, _onlineSlot, testing);
            if (!force && key == _renderedKey)
                return;
            _renderedKey = key;
            _status.Children.Clear();
            RenderTestButtons(ready, testing);
            bool planning = !live && ready && planned != 0 && _vehicle != -2;
            _presets.IsEnabled = live || planning;
            _performance.IsEnabled = _look.IsEnabled = _body.IsEnabled = live || planning;
            _presetCard.IsEnabled = live || planning;

            if (!TestVehicleTune.InRaceCreator)
            {
                _vehicleName.Text = "–";
                _status.Children.Add(Notice(T("tv_need_race", "Open a race in the race creator and start a test.")));
            }
            else if (!TestVehicleTune.ScriptFeaturesOn)
            {
                _vehicleName.Text = "–";
                _status.Children.Add(Notice(T("tv_need_features", "Needs the script features (Settings). They bring the function that tunes the vehicle.")));
            }
            else if (_vehicle == -2)
            {
                _vehicleName.Text = "–";
                _status.Children.Add(Notice(T("tv_no_answer", "The creator does not answer. Restart GTA if the script features were switched on while the creator was already open.")));
            }
            else if (planning)
            {
                _vehicleName.Text = VehicleName(planned) + "  ·  " + T("tv_next_test", "next test");
                _status.Children.Add(Notice(T("tv_planning", "Not in a test. What you pick here is kept for this vehicle and put on it when the test starts.")));
            }
            else if (!live)
            {
                _vehicleName.Text = "–";
                _status.Children.Add(Notice(T("tv_no_vehicle", "Start a test of the race and get into the vehicle.")));
            }
            else
            {
                _vehicleName.Text = VehicleName(_shown.Values.First().Model);
            }

            RenderOnline();
            RenderPresets();
            bool rows = live || planning;
            Fill((StackPanel)_performance.Content, rows ? PerformanceRows() : null);
            Fill((StackPanel)_look.Content, rows ? LookRows() : null);
            Fill((StackPanel)_body.Content, rows ? BodyRows() : null);
        }

        private static void Fill(StackPanel panel, IEnumerable<FrameworkElement> rows)
        {
            panel.Children.Clear();
            if (rows == null)
            {
                panel.Children.Add(Faint(T("tv_waiting", "Shows the options once you sit in the test vehicle."), 12.5));
                return;
            }
            foreach (var row in rows)
                panel.Children.Add(row);
        }

        private IEnumerable<FrameworkElement> PerformanceRows()
        {
            if (!Live && !_counts.ContainsKey(PlannedModel))
                yield return Notice(T("tv_need_once", "Drive this vehicle in a test once, then its levels and parts show up here before a test too. \"Max performance\" works already."));
            foreach (var p in PerformanceSlots)
            {
                int count = Count(p.Slot);
                if (count <= 0)
                    continue;
                var options = new List<(int, string)> { (-1, T("tv_stock_option", "Stock")) };
                for (int i = 0; i < count; i++)
                    options.Add((i, T("tv_level", "Level") + " " + (i + 1)));
                yield return Pick(T(p.Key, p.Fallback), options, Current(p.Slot), v => Send(p.Slot, v));
            }
            yield return Toggle(T("tv_turbo", "Turbo"), TestVehicleTune.Turbo);
        }

        private IEnumerable<FrameworkElement> LookRows()
        {
            var paints = PaintNames.Select((n, i) => (i, i + " · " + n)).ToList();
            yield return Pick(T("tv_primary", "Primary colour"), paints, Current(TestVehicleTune.Primary), v => Send(TestVehicleTune.Primary, v));
            yield return Pick(T("tv_secondary", "Secondary colour"), paints, Current(TestVehicleTune.Secondary), v => Send(TestVehicleTune.Secondary, v));
            yield return Pick(T("tv_pearl", "Pearlescent"), paints, Current(TestVehicleTune.Pearl), v => Send(TestVehicleTune.Pearl, v));
            yield return Pick(T("tv_wheel_colour", "Wheel colour"), paints, Current(TestVehicleTune.WheelColour), v => Send(TestVehicleTune.WheelColour, v));
            yield return Pick(T("tv_tint", "Window tint"), TintNames.Select((n, i) => (i, n)).ToList(), Current(TestVehicleTune.WindowTint), v => Send(TestVehicleTune.WindowTint, v));

            yield return Pick(T("tv_wheel_type", "Wheel type"), WheelTypeNames.Select((n, i) => (i, n)).ToList(), Current(TestVehicleTune.WheelType), v => Send(TestVehicleTune.WheelType, v));
            foreach (var slot in new[] { TestVehicleTune.FrontWheels, TestVehicleTune.BackWheels })
            {
                int count = Count(slot);
                if (count <= 0)
                    continue;
                var options = new List<(int, string)> { (-1, T("tv_stock_option", "Stock")) };
                for (int i = 0; i < count; i++)
                    options.Add((i, (i + 1).ToString(CultureInfo.CurrentCulture)));
                string name = slot == TestVehicleTune.FrontWheels ? T("tv_wheels", "Wheels") : T("tv_back_wheels", "Back wheel");
                yield return Pick(name, options, Current(slot), v => Send(slot, v));
            }

            yield return Toggle(T("tv_tyre_smoke", "Tyre smoke"), TestVehicleTune.TyreSmoke);
            yield return Toggle(T("tv_xenon", "Xenon lights"), TestVehicleTune.Xenon);
            var xenon = new List<(int, string)> { (255, T("tv_default", "Default")) };
            xenon.AddRange(XenonNames.Select((n, i) => (i, n)));
            yield return Pick(T("tv_xenon_colour", "Xenon colour"), xenon, Current(TestVehicleTune.XenonColour), v => Send(TestVehicleTune.XenonColour, v));

            // Newer vehicles carry their livery as mod 48, older ones as SET_VEHICLE_LIVERY.
            int liveryMods = Count(TestVehicleTune.LiveryMod);
            int liveries = Count(TestVehicleTune.Livery);
            if (liveryMods > 0)
            {
                var options = new List<(int, string)> { (-1, T("tv_stock_option", "Stock")) };
                for (int i = 0; i < liveryMods; i++)
                    options.Add((i, (i + 1).ToString(CultureInfo.CurrentCulture)));
                yield return Pick(T("tv_livery", "Livery"), options, Current(TestVehicleTune.LiveryMod), v => Send(TestVehicleTune.LiveryMod, v));
            }
            else if (liveries > 0)
            {
                var options = Enumerable.Range(0, liveries).Select(i => (i, (i + 1).ToString(CultureInfo.CurrentCulture))).ToList();
                yield return Pick(T("tv_livery", "Livery"), options, Current(TestVehicleTune.Livery), v => Send(TestVehicleTune.Livery, v));
            }
        }

        private IEnumerable<FrameworkElement> BodyRows()
        {
            bool any = false;
            foreach (var kv in BodySlots)
            {
                int count = Count(kv.Key);
                if (count <= 0)
                    continue;
                any = true;
                var options = new List<(int, string)> { (-1, T("tv_stock_option", "Stock")) };
                for (int i = 0; i < count; i++)
                    options.Add((i, (i + 1).ToString(CultureInfo.CurrentCulture)));
                int slot = kv.Key;
                yield return Pick(T(kv.Value.Key, kv.Value.Fallback), options, Current(slot), v => Send(slot, v));
            }
            if (!any && (Live || _counts.ContainsKey(PlannedModel)))
                yield return Faint(T("tv_no_parts", "This vehicle has no body parts to change."), 12.5);
            else if (!any)
                yield return Notice(T("tv_need_once_parts", "The body parts of this vehicle show up after it was driven in a test once."));
        }

        private FrameworkElement Pick(string label, List<(int Value, string Text)> options, int current, Action<int> write)
        {
            var box = new ComboBox { Height = 30, Width = 200, VerticalContentAlignment = VerticalAlignment.Center };
            int selected = -1;
            for (int i = 0; i < options.Count; i++)
            {
                box.Items.Add(new ComboBoxItem { Content = options[i].Text, Tag = options[i].Value });
                if (options[i].Value == current)
                    selected = i;
            }
            box.SelectedIndex = selected;
            box.SelectionChanged += (_, __) =>
            {
                if (box.SelectedItem is ComboBoxItem item && (int)item.Tag != current)
                    write((int)item.Tag);
            };
            DockPanel.SetDock(box, Dock.Right);
            var row = new DockPanel { Margin = new Thickness(0, 2, 0, 6) };
            row.Children.Add(box);
            var t = new TextBlock { Text = label, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(t);
            return row;
        }

        private FrameworkElement Toggle(string label, int slot)
        {
            var box = new CheckBox { IsChecked = Current(slot) >= 0 };
            box.Click += (_, __) => Send(slot, box.IsChecked == true ? 0 : -1);
            return SwitchRow(label, box);
        }

        private static FrameworkElement SwitchRow(string text, CheckBox box)
        {
            box.SetResourceReference(StyleProperty, "FormToggle");
            DockPanel.SetDock(box, Dock.Right);
            var row = new DockPanel { Margin = new Thickness(0, 2, 0, 6) };
            row.Children.Add(box);
            var t = new TextBlock { Text = text, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(t);
            return row;
        }

        private static string VehicleName(int model)
        {
            uint hash = unchecked((uint)model);
            string language = RaceVehicles.NameLanguage(MainWindow.Instance?.LanguageCode ?? "en");
            var vehicle = RaceVehicles.Classes.SelectMany(c => c.All).FirstOrDefault(v => v.Hash == hash);
            return vehicle != null ? vehicle.Name(language) : "0x" + hash.ToString("X8");
        }

        private static FrameworkElement Notice(string text)
        {
            var border = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(10, 7, 10, 7), Margin = new Thickness(0, 6, 0, 8) };
            border.SetResourceReference(Border.BorderBrushProperty, "WarnBrush");
            border.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            var t = new TextBlock { Text = text, FontSize = 12.5, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            border.Child = t;
            return border;
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }
    }
}
