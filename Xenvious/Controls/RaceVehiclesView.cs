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
    /// Race › Available Vehicles (mockup: https://claude.ai/artifact/BZ3JwEXfjwR6Vf9gemiaGB):
    /// race type and grid hint, start vehicle and a summary on top, then the classes on the
    /// left and the selected class as vehicle tiles with pictures on the right.
    /// Memory: RaceVehicles.
    /// </summary>
    public class RaceVehiclesView : DockPanel
    {
        private static readonly Dictionary<int, (string Key, string Fallback)> ClassNames = new Dictionary<int, (string, string)>
        {
            [0] = ("vc_compacts", "Compacts"), [1] = ("vc_sedan", "Sedans"), [2] = ("vc_suv", "SUVs"), [3] = ("vc_coupe", "Coupes"),
            [4] = ("vc_muscle", "Muscle"), [5] = ("vc_sports_classic", "Sports Classics"), [6] = ("vc_sport", "Sports"), [7] = ("vc_super", "Super"),
            [8] = ("vc_motorcycle", "Motorcycles"), [9] = ("vc_off_road", "Off-Road"), [10] = ("vc_industrial", "Industrial"), [11] = ("vc_utility", "Utility"),
            [12] = ("vc_van", "Vans"), [13] = ("vc_cycle", "Cycles"), [15] = ("vc_special", "Special"), [16] = ("vc_weaponized", "Weaponized"),
            [17] = ("vc_arena_contender", "Arena Contender"), [18] = ("vc_open_wheel", "Open Wheel"), [19] = ("vc_go_kart", "Go-Kart"), [20] = ("vc_tuner", "Tuner"),
            [25] = ("vc_drift", "Drift"),
        };

        // Race types whose vehicles come from the land race lists shown here (the script's
        // table switches these to the same case); air, sea and the others use other lists.
        private static readonly HashSet<int> LandListTypes = new HashSet<int> { 0, 1, 6, 7, 12, 13, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 30, 31 };

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        private bool _built;
        private RaceVehicles.VehicleClass _selected;
        private string _query = "";

        private readonly StackPanel _rail = new StackPanel();
        private readonly Dictionary<int, (CheckBox Box, TextBlock Count, Border Row)> _railRows = new Dictionary<int, (CheckBox, TextBlock, Border)>();
        private readonly WrapPanel _tiles = new WrapPanel();
        private readonly List<(RaceVehicles.VehicleClass Class, RaceVehicles.Vehicle Vehicle, ToggleButton Tile)> _shown = new List<(RaceVehicles.VehicleClass, RaceVehicles.Vehicle, ToggleButton)>();
        private readonly TextBlock _classTitle = new TextBlock(), _classCount = new TextBlock();
        private readonly CheckBox _classOn = new CheckBox();
        private FrameworkElement _classTools;
        private readonly TextBlock _typeText = new TextBlock { FontSize = 16, FontWeight = FontWeights.Bold };
        private readonly TextBlock _typeNote = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 12.5 };
        private Border _typeNoteBox;
        private readonly ComboBox _startClass = new ComboBox { Height = 30 };
        private readonly ComboBox _startVehicle = new ComboBox { Height = 30 };
        private readonly Image _startPicture = new Image { Width = 120, Height = 68, Stretch = Stretch.Uniform };
        private readonly TextBlock _startName = new TextBlock { FontSize = 16, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis };
        private readonly TextBlock _statClasses = Stat(), _statVehicles = Stat();
        private (int Class, int Index) _startShown = (-1, -1);
        private bool _loadingStart;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static string Language => RaceVehicles.NameLanguage(MainWindow.Instance?.LanguageCode ?? "en");

        private static string ClassName(int cls) => ClassNames.TryGetValue(cls, out var n) ? T(n.Key, n.Fallback) : T("rv_class", "Class") + " " + cls;

        private static TextBlock Stat() => new TextBlock { FontSize = 20, FontWeight = FontWeights.Bold };

        public RaceVehiclesView()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Load(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => Load();
        }

        private bool Live => _built && RaceVehicles.Ready;

        // ----- building -----

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            LastChildFill = true;

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            var page = new StackPanel { Margin = new Thickness(0, 12, 0, 12) };

            var top = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, -12, 0) };
            top.Children.Add(Spaced(TypeCard()));
            top.Children.Add(Spaced(StartCard()));
            top.Children.Add(Spaced(SummaryCard()));
            page.Children.Add(top);

            var main = new Grid();
            main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
            main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            main.ColumnDefinitions.Add(new ColumnDefinition());
            var rail = RailCard();
            main.Children.Add(rail);
            var cls = ClassCard();
            Grid.SetColumn(cls, 2);
            main.Children.Add(cls);
            page.Children.Add(main);

            scroll.Content = page;
            Children.Add(scroll);

            BuildRail();
            Select(RaceVehicles.Classes.FirstOrDefault(c => c.Index == 7) ?? RaceVehicles.Classes.FirstOrDefault());
            Load();
        }

        private static Border Spaced(Border card)
        {
            card.Margin = new Thickness(0, 0, 12, 12);
            return card;
        }

        private Border Card(FrameworkElement title, FrameworkElement body, FrameworkElement headerRight = null)
        {
            var header = new DockPanel();
            if (headerRight != null)
            {
                DockPanel.SetDock(headerRight, Dock.Right);
                header.Children.Add(headerRight);
            }
            header.Children.Add(title);
            var dock = new DockPanel();
            var head = new Border { Style = (Style)FindResource("DashCardHeader"), Child = header };
            DockPanel.SetDock(head, Dock.Top);
            dock.Children.Add(head);
            body.Margin = new Thickness(14, 12, 14, 12);
            dock.Children.Add(body);
            return new Border { Style = (Style)FindResource("DashCard"), Margin = new Thickness(0, 0, 0, 12), Child = dock };
        }

        private TextBlock Title(string key, string fallback) => new TextBlock { Style = (Style)FindResource("DashCardTitle"), Text = T(key, fallback) };

        private static TextBlock Muted(string text, double size = 12)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            return t;
        }

        private Border TypeCard()
        {
            var body = new StackPanel();
            body.Children.Add(_typeText);
            _typeNoteBox = new Border { BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 10, 0, 0), Child = _typeNote, Visibility = Visibility.Collapsed };
            _typeNoteBox.SetResourceReference(Border.BorderBrushProperty, "WarnBrush");
            _typeNoteBox.SetResourceReference(Border.BackgroundProperty, "SeactionHeaderBackgroundBrush");
            body.Children.Add(_typeNoteBox);
            return Card(Title("rv_racetype", "Race type"), body);
        }

        private Border StartCard()
        {
            var body = new StackPanel();
            body.Children.Add(new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = T("vc_header", "Vehicle class") });
            _startClass.Margin = new Thickness(0, 0, 0, 10);
            _startClass.SelectionChanged += (_, __) =>
            {
                if (!_loadingStart && Live && _startClass.SelectedItem is ComboBoxItem item)
                    RaceVehicles.SetStartClass((int)item.Tag);
            };
            _startClass.DropDownOpened += (_, __) => FillStartClasses();
            body.Children.Add(_startClass);

            // Only the vehicles the class allows; the creator skips blocked ones as well.
            body.Children.Add(new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = T("rv_start", "Start vehicle") });
            _startVehicle.Margin = new Thickness(0, 0, 0, 10);
            _startVehicle.SelectionChanged += (_, __) =>
            {
                if (!_loadingStart && Live && _startVehicle.SelectedItem is ComboBoxItem item)
                    RaceVehicles.SetStartVehicle((int)item.Tag);
            };
            _startVehicle.DropDownOpened += (_, __) => FillStartVehicles();
            body.Children.Add(_startVehicle);

            var row = new DockPanel();
            var frame = new Border { CornerRadius = new CornerRadius(5), Margin = new Thickness(0, 0, 12, 0), Child = _startPicture };
            frame.SetResourceReference(Border.BackgroundProperty, "SeactionHeaderBackgroundBrush");
            DockPanel.SetDock(frame, Dock.Left);
            row.Children.Add(frame);
            var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            text.Children.Add(_startName);
            text.Children.Add(Muted(T("rv_start_hint2", "Saved with the job at once; in game the creator may show it only once its vehicle menu is open.")));
            row.Children.Add(text);
            body.Children.Add(row);
            return Card(Title("rv_start", "Start vehicle"), body);
        }

        private Border SummaryCard()
        {
            var body = new StackPanel { Orientation = Orientation.Horizontal };
            StackPanel Figure(TextBlock value, string key, string fallback)
            {
                var p = new StackPanel { Margin = new Thickness(0, 0, 24, 0) };
                p.Children.Add(value);
                p.Children.Add(Muted(T(key, fallback)));
                return p;
            }
            body.Children.Add(Figure(_statClasses, "rv_classes_on", "classes in the race"));
            body.Children.Add(Figure(_statVehicles, "rv_vehicles_on", "vehicles allowed"));
            return Card(Title("rv_summary", "Summary"), body);
        }

        private Border RailCard()
        {
            var body = new StackPanel();
            var search = new TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 8), ToolTip = T("rv_search_tip", "Searches the vehicles of every class") };
            search.SetResourceReference(StyleProperty, "Watermark");
            search.Tag = T("rv_search", "Search");
            search.TextChanged += (_, __) => { _query = search.Text.Trim(); ShowTiles(); };
            body.Children.Add(search);
            body.Children.Add(_rail);
            return Card(Title("rv_classes", "Classes"), body);
        }

        private Border ClassCard()
        {
            var body = new StackPanel();
            var tools = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (var (key, fallback, allowed) in new[] { ("rv_all", "All", true), ("rv_none", "None", false) })
            {
                var b = new Button { Style = (Style)FindResource("FormButton"), Content = T(key, fallback), Margin = new Thickness(8, 0, 0, 0), MinWidth = 70 };
                b.Click += (_, __) =>
                {
                    if (!Live || _selected == null) return;
                    RaceVehicles.SetAllAllowed(_selected, allowed);
                    Load();
                };
                buttons.Children.Add(b);
            }
            DockPanel.SetDock(buttons, Dock.Right);
            tools.Children.Add(buttons);
            var on = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            _classOn.Style = (Style)FindResource("FormToggle");
            _classOn.Click += (_, __) =>
            {
                if (!Live || _selected == null) return;
                RaceVehicles.SetClass(_selected.Index, _classOn.IsChecked == true);
                Load();
            };
            on.Children.Add(_classOn);
            on.Children.Add(new TextBlock { Text = T("rv_class_on", "Class in the race"), FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) });
            tools.Children.Add(on);
            _classTools = tools;
            body.Children.Add(tools);

            _tiles.Margin = new Thickness(0, 0, -8, 0);
            body.Children.Add(_tiles);
            body.Children.Add(Muted(T("rv_legend", "Click a vehicle to allow or block it. Blue: DLC vehicle. Pictures: docs.fivem.net."), 12));

            _classTitle.Style = (Style)FindResource("DashCardTitle");
            _classCount.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _classCount.FontSize = 12;
            _classCount.VerticalAlignment = VerticalAlignment.Center;
            return Card(_classTitle, body, _classCount);
        }

        private void BuildRail()
        {
            foreach (var c in RaceVehicles.Classes)
            {
                var cls = c;
                var box = new CheckBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
                box.Click += (_, __) =>
                {
                    if (Live) RaceVehicles.SetClass(cls.Index, box.IsChecked == true);
                    Load();
                };
                var count = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
                count.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                var row = new DockPanel();
                DockPanel.SetDock(box, Dock.Left);
                DockPanel.SetDock(count, Dock.Right);
                row.Children.Add(box);
                row.Children.Add(count);
                row.Children.Add(new TextBlock { Text = ClassName(cls.Index), VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis });
                var border = new Border { Child = row, Padding = new Thickness(8, 7, 8, 7), CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(0, 0, 0, 2), Cursor = System.Windows.Input.Cursors.Hand, Background = Brushes.Transparent };
                border.MouseLeftButtonUp += (_, __) =>
                {
                    if (!box.IsMouseOver)
                        Select(cls);
                };
                _railRows[cls.Index] = (box, count, border);
                _rail.Children.Add(border);
            }
        }

        private void Select(RaceVehicles.VehicleClass c)
        {
            _selected = c;
            foreach (var pair in _railRows)
            {
                bool sel = c != null && pair.Key == c.Index;
                if (sel)
                {
                    pair.Value.Row.SetResourceReference(Border.BackgroundProperty, "SeactionHeaderBackgroundBrush");
                    pair.Value.Row.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
                }
                else
                {
                    pair.Value.Row.Background = Brushes.Transparent;
                    pair.Value.Row.BorderBrush = Brushes.Transparent;
                }
            }
            ShowTiles();
        }

        // The selected class, or while searching the matches in every class.
        private void ShowTiles()
        {
            if (!_built)
                return;
            _tiles.Children.Clear();
            _shown.Clear();
            bool searching = _query.Length > 0;
            string lang = Language;
            IEnumerable<(RaceVehicles.VehicleClass, RaceVehicles.Vehicle)> list;
            if (searching)
                list = RaceVehicles.Classes.SelectMany(c => c.All.Select(v => (c, v)))
                    .Where(p => p.v.Name(lang).IndexOf(_query, StringComparison.OrdinalIgnoreCase) >= 0
                             || (p.v.Model ?? "").IndexOf(_query, StringComparison.OrdinalIgnoreCase) >= 0);
            else
                list = _selected == null ? Enumerable.Empty<(RaceVehicles.VehicleClass, RaceVehicles.Vehicle)>() : _selected.All.Select(v => (_selected, v));

            foreach (var (c, v) in list)
            {
                var tile = Tile(c, v, lang, searching);
                _tiles.Children.Add(tile);
                _shown.Add((c, v, tile));
            }
            _classTitle.Text = searching ? T("rv_results", "Search results") : _selected != null ? ClassName(_selected.Index) : "";
            _classTools.Visibility = searching ? Visibility.Collapsed : Visibility.Visible;
            Load();
        }

        private ToggleButton Tile(RaceVehicles.VehicleClass c, RaceVehicles.Vehicle v, string lang, bool showClass)
        {
            var panel = new StackPanel { Width = 150 };
            var picture = new Grid { Height = 72 };
            var placeholder = new TextBlock { Text = v.Model ?? "0x" + v.Hash.ToString("X8", CultureInfo.InvariantCulture), FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            placeholder.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var image = new Image { Stretch = Stretch.Uniform };
            picture.Children.Add(placeholder);
            picture.Children.Add(image);
            panel.Children.Add(picture);
            panel.Children.Add(new TextBlock { Text = v.Name(lang), FontSize = 13.5, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 6, 0, 0) });
            var sub = new TextBlock { FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis,
                Text = (showClass ? ClassName(c.Index) + " · " : "") + (v.Dlc ? "DLC" : T("rv_base", "Base")) + " · " + (v.Model ?? "?") };
            sub.SetResourceReference(TextBlock.ForegroundProperty, v.Dlc ? "AccentBrush" : "FaintTextBrush");
            panel.Children.Add(sub);

            var tile = new ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = panel, Margin = new Thickness(0, 0, 8, 8), ToolTip = v.Model };
            tile.Click += (_, __) =>
            {
                if (!Live) { tile.IsChecked = !tile.IsChecked; return; }
                RaceVehicles.SetAllowed(c.Index, v, tile.IsChecked == true);
                Load();
            };
            if (v.Model != null)
                LoadPicture(image, placeholder, v);
            return tile;
        }

        private static async void LoadPicture(Image image, TextBlock placeholder, RaceVehicles.Vehicle v)
        {
            var source = await ModelImageCache.GetAsync("vehicle", v.Model, v.Hash);
            if (source == null)
                return;
            image.Source = source;
            placeholder.Visibility = Visibility.Collapsed;
        }

        // ----- loading -----

        private void Load()
        {
            if (!Live)
                return;
            try
            {
                int classesOn = 0, allowed = 0;
                int clbs = RaceVehicles.ClassBits;
                var states = new Dictionary<int, RaceVehicles.ClassState>();
                foreach (var c in RaceVehicles.Classes)
                {
                    bool on = (clbs & (1 << c.Index)) != 0;
                    var state = RaceVehicles.Read(c.Index);
                    states[c.Index] = state;
                    int n = c.All.Count(state.Allowed);
                    if (on)
                    {
                        classesOn++;
                        allowed += n;
                    }
                    if (_railRows.TryGetValue(c.Index, out var row))
                    {
                        row.Box.IsChecked = on;
                        row.Count.Text = n.ToString(CultureInfo.InvariantCulture) + " / " + (c.Base.Count + c.Dlc.Count).ToString(CultureInfo.InvariantCulture);
                        row.Row.Opacity = on ? 1 : 0.6;
                    }
                }
                _statClasses.Text = classesOn.ToString(CultureInfo.InvariantCulture);
                _statVehicles.Text = allowed.ToString(CultureInfo.InvariantCulture);

                foreach (var (c, v, tile) in _shown)
                    tile.IsChecked = states[c.Index].Allowed(v);
                if (_selected != null && _query.Length == 0)
                {
                    _classOn.IsChecked = (clbs & (1 << _selected.Index)) != 0;
                    int n = _selected.All.Count(states[_selected.Index].Allowed);
                    _classCount.Text = string.Format(CultureInfo.InvariantCulture, T("rv_allowed_of", "{0} of {1} allowed"), n, _selected.Base.Count + _selected.Dlc.Count);
                }
                else
                    _classCount.Text = _shown.Count.ToString(CultureInfo.InvariantCulture);

                LoadType();
                LoadStart();
            }
            catch (Exception ex)
            {
                Logging.Log.Debug("race vehicles: " + ex.Message, source: "race");
            }
        }

        private void LoadType()
        {
            int type = RaceVehicles.RaceType;
            string name;
            switch (type)
            {
                case 0: case 1: name = T("rv_type_land", "Land race"); break;
                case 2: case 3: name = T("rv_type_air", "Air race"); break;
                case 4: case 5: name = T("rv_type_sea", "Sea race"); break;
                case 6: case 7: name = T("rv_type_stunt", "Stunt race"); break;
                case 24: case 25: name = T("rv_type_arena", "Arena race"); break;
                default: name = T("rv_racetype", "Race type") + " " + type.ToString(CultureInfo.InvariantCulture); break;
            }
            _typeText.Text = name;

            string note = null;
            if (!LandListTypes.Contains(type))
                note = T("rv_note_other", "This race type uses other vehicle lists than the land race lists shown here; changes here may not apply.");
            // Stunt races with a small starting grid: the creator only offers Motorcycles,
            // Cycles and Go-Kart and drops every other class when the job loads.
            else if ((type == 6 || type == 7) && RaceVehicles.GridSize == 0)
                note = string.Format(CultureInfo.InvariantCulture,
                    T("rv_note_smallgrid", "Small starting grid: the creator only offers Motorcycles, Cycles and Go-Kart and drops the other classes when the job loads. The script patch \"{0}\" lifts this."),
                    T("scrpatchname_race_small_grid_all_classes", "Small grid, all classes"));
            _typeNote.Text = note ?? "";
            _typeNoteBox.Visibility = note == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void FillStartClasses()
        {
            _loadingStart = true;
            _startClass.Items.Clear();
            foreach (var c in RaceVehicles.Classes)
            {
                if (!Live || !RaceVehicles.ClassOn(c.Index))
                    continue;
                _startClass.Items.Add(new ComboBoxItem { Tag = c.Index, Content = ClassName(c.Index) });
            }
            SelectStartClass(RaceVehicles.StartClass);
            _loadingStart = false;
        }

        private void FillStartVehicles()
        {
            _loadingStart = true;
            _startVehicle.Items.Clear();
            int cls = RaceVehicles.StartClass;
            var c = RaceVehicles.Classes.FirstOrDefault(x => x.Index == cls);
            if (Live && c != null)
            {
                var state = RaceVehicles.Read(cls);
                foreach (var v in c.All.Where(state.Allowed).OrderBy(v => v.Name(Language), StringComparer.CurrentCultureIgnoreCase))
                    _startVehicle.Items.Add(new ComboBoxItem { Tag = v.StartIndex, Content = v.Name(Language) });
            }
            SelectStartVehicle(RaceVehicles.StartVehicle);
            _loadingStart = false;
        }

        private void SelectStartVehicle(int index)
        {
            var item = _startVehicle.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == index);
            if (item == null && _startShown.Class >= 0)
            {
                var v = RaceVehicles.Classes.FirstOrDefault(x => x.Index == _startShown.Class)?.All.FirstOrDefault(x => x.StartIndex == index);
                if (v != null)
                {
                    item = new ComboBoxItem { Tag = index, Content = v.Name(Language) };
                    _startVehicle.Items.Add(item);
                }
            }
            _startVehicle.SelectedItem = item;
        }

        private void SelectStartClass(int cls)
        {
            var item = _startClass.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == cls);
            if (item == null && ClassNames.ContainsKey(cls))
            {
                item = new ComboBoxItem { Tag = cls, Content = ClassName(cls) };
                _startClass.Items.Add(item);
            }
            _startClass.SelectedItem = item;
        }

        private void LoadStart()
        {
            int cls = RaceVehicles.StartClass, index = RaceVehicles.StartVehicle;
            if (!_startClass.IsDropDownOpen)
            {
                _loadingStart = true;
                SelectStartClass(cls);
                _loadingStart = false;
            }
            if (_startShown == (cls, index))
                return;
            // Another class: the list holds the old class's vehicles.
            if (cls != _startShown.Class && !_startVehicle.IsDropDownOpen)
                _startVehicle.Items.Clear();
            _startShown = (cls, index);
            if (!_startVehicle.IsDropDownOpen)
            {
                _loadingStart = true;
                SelectStartVehicle(index);
                _loadingStart = false;
            }
            var vehicle = RaceVehicles.Classes.FirstOrDefault(c => c.Index == cls)?.All.FirstOrDefault(v => v.StartIndex == index);
            _startName.Text = vehicle != null ? vehicle.Name(Language) : "–";
            _startPicture.Source = null;
            if (vehicle?.Model != null)
                LoadStartPicture(vehicle);
        }

        private async void LoadStartPicture(RaceVehicles.Vehicle v)
        {
            var source = await ModelImageCache.GetAsync("vehicle", v.Model, v.Hash);
            if (_startShown.Index == v.StartIndex)
                _startPicture.Source = source;
        }
    }
}
