using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Race › Checkpoints (mockup: https://claude.ai/artifact/BdhyL4BCf1JPa9oE18oyg1): a picker with
    /// primary/secondary on top, the route on a map, position, respawn points and sizes in the
    /// middle, the options as chips and the transform settings on the right. Memory: RaceCheckpoints.
    /// </summary>
    public class RaceCheckpointsView : DockPanel
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private bool _built, _loading;

        private readonly ComboBox _pointBox = new ComboBox { Width = 92, Height = 30, BorderThickness = new Thickness(0) };
        private readonly ToggleButton _primaryTab = new ToggleButton { IsChecked = true }, _secondaryTab = new ToggleButton();
        private readonly CheckBox _skipFake = new CheckBox();
        private readonly TextBlock _count = new TextBlock { FontSize = 17, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };
        private readonly TextBlock _noSecondary = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 12.5 };
        private Border _noSecondaryBox;

        private readonly Canvas _map = new Canvas { Height = 380, ClipToBounds = true, Background = Brushes.Transparent };

        private TextBox[] _pos, _heading = new TextBox[1];
        private readonly TextBox[][] _respawn = new TextBox[3][];
        private TextBox _collect, _visual, _shrink, _pitch, _raw1, _raw2, _raw3;
        private readonly CheckBox _tall = new CheckBox(), _pitchOn = new CheckBox();
        private readonly List<(RaceCheckpoints.Flag Flag, CheckBox Chip)> _chips = new List<(RaceCheckpoints.Flag, CheckBox)>();
        private readonly ComboBox _transform = new ComboBox { Height = 30 }, _planeTurn = new ComboBox { Height = 30 },
            _deluxo = new ComboBox { Height = 30 }, _stromberg = new ComboBox { Height = 30 }, _random = new ComboBox { Height = 30 };
        private string _transformNames = "";

        private readonly List<(int Index, bool Secondary, float X, float Y, bool Fake, bool Transform)> _points = new List<(int, bool, float, float, bool, bool)>();

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static string F(float v) => v.ToString("0.###", CultureInfo.CurrentCulture);

        private static bool TryFloat(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                || float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public RaceCheckpointsView()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Load(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => Load();
        }

        private int Index => _pointBox.SelectedIndex;
        private bool Secondary => _secondaryTab.IsChecked == true;
        private bool Live => _built && RaceCheckpoints.Ready && Index >= 0 && Index < RaceCheckpoints.Count;

        // ----- building -----

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            LastChildFill = true;

            var top = TopBar();
            DockPanel.SetDock(top, Dock.Top);
            Children.Add(top);

            var grid = new Grid { Margin = new Thickness(0, 12, 0, 12) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var left = new StackPanel();
            left.Children.Add(MapCard());
            grid.Children.Add(left);

            var middle = new StackPanel();
            _noSecondaryBox = Note(_noSecondary);
            _noSecondary.Text = T("cp_nosecondary", "This checkpoint has no secondary point. Set a position to give it one.");
            _noSecondaryBox.Visibility = Visibility.Collapsed;
            middle.Children.Add(_noSecondaryBox);
            middle.Children.Add(PositionCard());
            middle.Children.Add(RespawnCard());
            middle.Children.Add(SizeCard());
            Grid.SetColumn(middle, 2);
            grid.Children.Add(middle);

            var right = new StackPanel();
            right.Children.Add(ChipCard("cp_look", "Look", RaceCheckpoints.LookFlags));
            right.Children.Add(ChipCard("cp_behaviour", "Race behaviour", RaceCheckpoints.RaceFlags));
            right.Children.Add(TransformCard());
            right.Children.Add(RawCard());
            Grid.SetColumn(right, 4);
            grid.Children.Add(right);

            Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = grid });

            _pointBox.SelectionChanged += (_, __) => { ShowPoint(); DrawMap(); };
            _primaryTab.Click += (_, __) => PickMode(false);
            _secondaryTab.Click += (_, __) => PickMode(true);
            Load();
            // The bar was built after the page opened; put it into the header now.
            MainWindow.Instance?.RequestEntryBarMove();
        }

        private Border TopBar()
        {
            var bar = new DockPanel();
            var right = new StackPanel { Orientation = Orientation.Horizontal };
            DockPanel.SetDock(right, Dock.Right);

            var seg = new Border { CornerRadius = new CornerRadius(7), BorderThickness = new Thickness(1), Padding = new Thickness(2), Margin = new Thickness(0, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center };
            seg.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            seg.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            var segPanel = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (var (tab, key, fallback) in new[] { (_primaryTab, "primary", "Primary"), (_secondaryTab, "secondary", "Secondary") })
            {
                tab.Style = (Style)FindResource("TeamSegButton");
                tab.Content = T(key, fallback);
                segPanel.Children.Add(tab);
            }
            seg.Child = segPanel;
            right.Children.Add(seg);

            var picker = new Border { CornerRadius = new CornerRadius(4), BorderThickness = new Thickness(1), Height = 32, Margin = new Thickness(0, 0, 12, 0) };
            picker.SetResourceReference(Border.BackgroundProperty, "ComboBoxBackground");
            picker.SetResourceReference(Border.BorderBrushProperty, "ComboBoxBorder");
            var pickerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var prev = new Button { Style = (Style)FindResource("EntryStepButton"), Content = "‹", ToolTip = T("cp_prev", "Previous checkpoint") };
            var next = new Button { Style = (Style)FindResource("EntryStepButton"), Content = "›", ToolTip = T("cp_next", "Next checkpoint") };
            prev.Click += (_, __) => Step(-1);
            next.Click += (_, __) => Step(1);
            _pointBox.SetResourceReference(Control.FontFamilyProperty, "Volte");
            pickerPanel.Children.Add(prev);
            pickerPanel.Children.Add(_pointBox);
            pickerPanel.Children.Add(next);
            picker.Child = pickerPanel;
            right.Children.Add(picker);

            var insert = new Button { Style = (Style)FindResource("FormButton"), Content = T("cp_insert", "+ Insert after"), Height = 32, Margin = new Thickness(0, 0, 8, 0), ToolTip = T("cp_insert_tip", "Adds a checkpoint right after this one, halfway to the next (after the last one: at the creator cursor). The ones behind it move up one number.") };
            insert.Click += async (_, __) => await InsertAsync();
            right.Children.Add(insert);

            var fly = new Button { Style = (Style)FindResource("FormButton"), Content = T("cp_flyto", "Fly there"), Height = 32, Margin = new Thickness(0, 0, 14, 0), ToolTip = T("cp_flyto_tip", "Moves you and the creator camera to this checkpoint.") };
            fly.Click += (_, __) => FlyTo();
            right.Children.Add(fly);

            _skipFake.Style = (Style)FindResource("SwitchToggle");
            right.Children.Add(_skipFake);
            right.Children.Add(Muted(T("cp_skipfake", "Skip fake checkpoints"), new Thickness(8, 0, 0, 0)));

            bar.Children.Add(right);
            _count.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            var countLine = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            countLine.Children.Add(_count);
            countLine.Children.Add(Muted(" / " + RaceCheckpoints.Max + " " + T("cps_placed", "checkpoints"), new Thickness(0, 2, 0, 0)));
            bar.Children.Add(countLine);
            return new Border { Padding = new Thickness(2, 0, 2, 0), Margin = new Thickness(0, 12, 0, 0), Tag = "EntryBar", Child = bar };
        }

        private Border Card(string key, string fallback, FrameworkElement body, FrameworkElement headerRight = null)
        {
            var header = new DockPanel();
            if (headerRight != null)
            {
                DockPanel.SetDock(headerRight, Dock.Right);
                header.Children.Add(headerRight);
            }
            header.Children.Add(new TextBlock { Style = (Style)FindResource("DashCardTitle"), Text = T(key, fallback) });
            var dock = new DockPanel();
            var head = new Border { Style = (Style)FindResource("DashCardHeader"), Child = header };
            DockPanel.SetDock(head, Dock.Top);
            dock.Children.Add(head);
            body.Margin = new Thickness(14, 12, 14, 12);
            dock.Children.Add(body);
            return new Border { Style = (Style)FindResource("DashCard"), Margin = new Thickness(0, 0, 0, 12), Child = dock };
        }

        private static TextBlock Muted(string text, Thickness margin)
        {
            var t = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, Margin = margin, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            return t;
        }

        private TextBlock Label(string key, string fallback) => new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = T(key, fallback) };

        private Border Note(TextBlock text)
        {
            text.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            var box = new Border { BorderThickness = new Thickness(3, 0, 0, 0), CornerRadius = new CornerRadius(0, 6, 6, 0), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 12), Child = text };
            box.SetResourceReference(Border.BorderBrushProperty, "WarnBrush");
            box.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            return box;
        }

        private TextBox Box(Action<float> write)
        {
            var box = new TextBox { Style = (Style)FindResource("Watermark"), Height = 30 };
            box.TextChanged += (_, __) =>
            {
                if (!_loading && Live && TryFloat(box.Text, out float v))
                {
                    write(v);
                    DrawMap();
                }
            };
            return box;
        }

        private TextBox IntBox(Action<int> write)
        {
            var box = new TextBox { Style = (Style)FindResource("Watermark"), Height = 30, FontFamily = new FontFamily("Consolas") };
            box.TextChanged += (_, __) =>
            {
                if (!_loading && Live && int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    write(v);
                    ShowPoint();
                }
            };
            return box;
        }

        private Button IconButton(string tip, string geometry, Action click)
        {
            var path = new Path { Data = Geometry.Parse(geometry), StrokeThickness = 1.5, Width = 16, Height = 16, Stretch = Stretch.None };
            var button = new Button { Style = (Style)FindResource("FieldIconButton"), ToolTip = tip, Width = 30, Height = 30, Content = path };
            path.SetBinding(Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { Source = button });
            button.Click += (_, __) => click();
            return button;
        }

        private const string CursorIcon = "M8,1.5 L8,4.5 M8,11.5 L8,14.5 M1.5,8 L4.5,8 M11.5,8 L14.5,8 M8,5 A3,3 0 1 1 7.99,5";
        private const string ResetIcon = "M3,3 L13,13 M13,3 L3,13";

        // One row: a short label, X/Y/Z boxes and buttons at the end.
        private Grid VectorRow(string label, TextBox[] boxes, params Button[] buttons)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            for (int i = 0; i < 3; i++)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition());
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            }
            foreach (var _ in buttons)
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
            var name = Muted(label, new Thickness(0));
            row.Children.Add(name);
            for (int i = 0; i < 3; i++)
            {
                Grid.SetColumn(boxes[i], 1 + i * 2);
                row.Children.Add(boxes[i]);
            }
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetColumn(buttons[i], 7 + i);
                row.Children.Add(buttons[i]);
            }
            return row;
        }

        private Grid AxisHeader(int buttons)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            for (int i = 0; i < 3; i++)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition());
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            }
            for (int i = 0; i < buttons; i++)
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
            string[] axes = { "X", "Y", "Z" };
            for (int i = 0; i < 3; i++)
            {
                var t = new TextBlock { Style = (Style)FindResource("FieldAxis"), Text = axes[i] };
                Grid.SetColumn(t, 1 + i * 2);
                row.Children.Add(t);
            }
            return row;
        }

        private static Grid Pair(FrameworkElement a, FrameworkElement b)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.Children.Add(a);
            Grid.SetColumn(b, 2);
            grid.Children.Add(b);
            return grid;
        }

        private StackPanel Field(string key, string fallback, FrameworkElement input)
        {
            var panel = new StackPanel();
            panel.Children.Add(Label(key, fallback));
            panel.Children.Add(input);
            return panel;
        }

        // A switch in front of a value box; the box is only enabled while the switch is on.
        private DockPanel Switched(CheckBox toggle, TextBox box, RaceCheckpoints.Flag flag)
        {
            toggle.Style = (Style)FindResource("SwitchToggle");
            toggle.Margin = new Thickness(0, 0, 8, 0);
            toggle.Click += (_, __) =>
            {
                if (!Live) return;
                RaceCheckpoints.SetFlag(flag, Index, Secondary, toggle.IsChecked == true);
                ShowPoint();
            };
            var dock = new DockPanel();
            DockPanel.SetDock(toggle, Dock.Left);
            dock.Children.Add(toggle);
            dock.Children.Add(box);
            return dock;
        }

        private Border MapCard()
        {
            var body = new StackPanel();
            var frame = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Child = _map, Margin = new Thickness(0, 0, 0, 10) };
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            body.Children.Add(frame);
            _map.SizeChanged += (_, __) => DrawMap();
            SatelliteTiles.TileLoaded += () => Dispatcher.BeginInvoke(new Action(() => { if (IsVisible) DrawMap(); }), DispatcherPriority.Background);

            var legend = new WrapPanel();
            foreach (var (brush, key, fallback, hollow) in new[]
            {
                (Accent, "cp_map_selected", "Selected", false), (Yellow, "cp_map_cp", "Checkpoint", false), (Orange, "cp_map_fake", "Fake", false),
                (Purple, "cp_map_transform", "Transform", false), (Green, "cp_map_respawn", "Respawn", false), (Red, "cp_map_finish", "Finish", false),
            })
            {
                var item = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 14, 4) };
                item.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = hollow ? Brushes.Transparent : brush, Stroke = hollow ? brush : null, StrokeThickness = 1.5, StrokeDashArray = hollow ? new DoubleCollection { 2, 1 } : null, Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });
                item.Children.Add(Muted(T(key, fallback), new Thickness(0)));
                legend.Children.Add(item);
            }
            body.Children.Add(legend);
            return Card("cp_map", "Map", body, Muted(T("cp_map_hint", "Click a point to pick it"), new Thickness(0)));
        }

        private Border PositionCard()
        {
            var body = new StackPanel();
            _pos = new[]
            {
                Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.PosField(Secondary), Index, v)),
                Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.PosField(Secondary) + 1, Index, v)),
                Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.PosField(Secondary) + 2, Index, v)),
            };
            body.Children.Add(AxisHeader(1));
            body.Children.Add(VectorRow("CP", _pos, IconButton(T("cp_tocursor", "Set to the creator cursor"), CursorIcon, () => ToCursor(_pos))));
            _heading[0] = Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.HeadingField(Secondary), Index, v));
            body.Children.Add(Field("heading", "Heading", _heading[0]));
            // Slider 0-360° beside the box, like the other heading fields; the creator shows the
            // new heading after its refresh.
            HeadingSlider.Attach(_heading[0], () => MainWindow.Instance?.creatorRefresh());
            return Card("cp_position", "Position", body);
        }

        private Border RespawnCard()
        {
            var body = new StackPanel();
            body.Children.Add(AxisHeader(2));
            for (int n = 0; n < 3; n++)
            {
                int slot = n;
                _respawn[n] = new[]
                {
                    Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.RespawnField(Secondary) + slot * 3, Index, v)),
                    Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.RespawnField(Secondary) + slot * 3 + 1, Index, v)),
                    Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.RespawnField(Secondary) + slot * 3 + 2, Index, v)),
                };
                var boxes = _respawn[n];
                body.Children.Add(VectorRow("R" + (n + 1), boxes,
                    IconButton(T("cp_tocursor", "Set to the creator cursor"), CursorIcon, () => ToCursor(boxes)),
                    IconButton(T("cp_resp_reset", "Remove this respawn point (0, 0, 0)"), ResetIcon, () => ResetRespawn(slot))));
            }
            var hint = Muted(T("cp_resp_explain", "Where a player comes back after a crash or a respawn, once this checkpoint was the last one passed. Up to three places, so players do not spawn on top of each other. 0, 0, 0 = no own place, the game uses its usual respawn."), new Thickness(0, 4, 0, 0));
            hint.FontSize = 12;
            body.Children.Add(hint);
            return Card("cp_respawns", "Respawn points", body, Muted(T("cp_resp_green", "green on the map"), new Thickness(0)));
        }

        private Border SizeCard()
        {
            var body = new StackPanel();
            _collect = Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.CollectSizeField(Secondary), Index, v));
            _visual = Box(v => RaceCheckpoints.SetFloat(GTA.Offsets.Editor.Race.Checkpoints.chvs, Index, v));
            _visual.ToolTip = T("cp_visual_tip", "The visual size is shared by the primary and the secondary point.");
            body.Children.Add(Pair(Field("cpcollectsize", "Collect Size", _collect), Field("cpsize", "Checkpoint Size", _visual)));
            _shrink = Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.ShrinkField(Secondary), Index, v));
            _pitch = Box(v => RaceCheckpoints.SetFloat(RaceCheckpoints.PitchField(Secondary), Index, v));
            var tall = Field("cp_tall", "Tall, shrink radius", Switched(_tall, _shrink, RaceCheckpoints.Tall));
            tall.ToolTip = T("cp_tall_tip", "A tall checkpoint that shrinks once you are within this radius.");
            var pitch = Field("cp_pitch", "Pitch", Switched(_pitchOn, _pitch, RaceCheckpoints.Pitch));
            pitch.ToolTip = T("cp_pitch_tip", "Tilts the checkpoint instead of always facing the player.");
            body.Children.Add(Pair(tall, pitch));
            return Card("cp_size", "Size", body);
        }

        private Border ChipCard(string key, string fallback, RaceCheckpoints.Flag[] flags)
        {
            var wrap = new WrapPanel();
            foreach (var flag in flags)
            {
                var chip = new CheckBox { Style = (Style)FindResource("ChipToggle"), Content = T(flag.Key, flag.Fallback) };
                var f = flag;
                chip.Click += (_, __) =>
                {
                    if (!Live) return;
                    RaceCheckpoints.SetFlag(f, Index, Secondary, chip.IsChecked == true);
                    ShowPoint();
                    DrawMap();
                };
                _chips.Add((flag, chip));
                wrap.Children.Add(chip);
            }
            return Card(key, fallback, wrap);
        }

        private static void Fill(ComboBox box, params (string Key, string Fallback)[] items)
        {
            foreach (var (key, fallback) in items)
                box.Items.Add(T(key, fallback));
        }

        // The list reads "No transform", "Random", then the vehicle slots; the creator stores
        // -1 for none, -2 for random and the slot number otherwise.
        private static int TransformItem(int value) => value == -1 ? 0 : value == -2 ? 1 : value + 2;

        private static int TransformValue(int item) => item == 0 ? -1 : item == 1 ? -2 : item - 2;

        private Border TransformCard()
        {
            var body = new StackPanel();
            _transform.SelectionChanged += (_, __) =>
            {
                if (!_loading && Live && _transform.SelectedIndex >= 0)
                {
                    RaceCheckpoints.SetInt(RaceCheckpoints.TransformField(Secondary), Index, TransformValue(_transform.SelectedIndex));
                    DrawMap();
                }
            };
            Fill(_planeTurn, ("cp_pt_none", "None"), ("cp_pt_flat", "Flat"), ("cp_pt_right", "Banked right"), ("cp_pt_inverted", "Upside down"), ("cp_pt_left", "Banked left"));
            _planeTurn.SelectionChanged += (_, __) =>
            {
                if (!_loading && Live && _planeTurn.SelectedIndex >= 0)
                    RaceCheckpoints.SetPlaneTurn(Index, Secondary, _planeTurn.SelectedIndex);
            };
            body.Children.Add(Pair(Field("transformveh", "Transform Vehicle", _transform), Field("cp_planeturn", "Plane turn", _planeTurn)));

            Fill(_deluxo, ("cp_keep", "Unchanged"), ("cp_dl_drive", "Drive only"), ("cp_dl_hover", "Hover allowed"), ("cp_dl_hoverforce", "Hover forced"), ("cp_dl_flyforce", "Flight forced"), ("cp_dl_fly", "Flight allowed"));
            _deluxo.SelectionChanged += (_, __) =>
            {
                if (!_loading && Live && _deluxo.SelectedIndex >= 0)
                    RaceCheckpoints.SetInt(RaceCheckpoints.DeluxoField(Secondary), Index, _deluxo.SelectedIndex - 1);
            };
            Fill(_stromberg, ("cp_keep", "Unchanged"), ("cp_st_car", "Car"), ("cp_st_sub", "Submarine"), ("cp_st_auto", "Submarine under water"));
            _stromberg.SelectionChanged += (_, __) =>
            {
                if (!_loading && Live && _stromberg.SelectedIndex >= 0)
                    RaceCheckpoints.SetInt(RaceCheckpoints.StrombergField(Secondary), Index, _stromberg.SelectedIndex - 1);
            };
            body.Children.Add(Pair(Field("cp_deluxo", "Deluxo", _deluxo), Field("cp_stromberg", "Stromberg", _stromberg)));

            Fill(_random, ("cp_rt_off", "Off"), ("cp_rt_marked", "Checkpoints set to random"), ("cp_rt_all", "Every checkpoint"));
            _random.SelectionChanged += (_, __) =>
            {
                if (!_loading && RaceCheckpoints.Ready && _random.SelectedIndex >= 0)
                    RaceCheckpoints.RandomTransform = _random.SelectedIndex;
            };
            var random = Field("cp_randomtransform", "Random transform (whole race)", _random);
            random.ToolTip = T("cp_randomtransform_tip", "Gives random vehicles at the checkpoints whose transform is set to random, or at every checkpoint.");
            body.Children.Add(random);
            return Card("cp_transform", "Transform", body);
        }

        private Border RawCard()
        {
            var body = new StackPanel();
            _raw1 = IntBox(v => RaceCheckpoints.SetInt(GTA.Offsets.Editor.Race.Checkpoints.cpbs1, Index, v));
            _raw2 = IntBox(v => RaceCheckpoints.SetInt(GTA.Offsets.Editor.Race.Checkpoints.cpbs2, Index, v));
            _raw3 = IntBox(v => RaceCheckpoints.SetInt(GTA.Offsets.Editor.Race.Checkpoints.cpbs3, Index, v));
            var grid = new UniformGrid { Columns = 3 };
            grid.Children.Add(Field("cpbs1", "cpbs1", _raw1));
            grid.Children.Add(Field("cpbs2", "cpbs2", _raw2));
            grid.Children.Add(Field("cpbs3", "cpbs3", _raw3));
            foreach (FrameworkElement f in grid.Children)
                f.Margin = new Thickness(0, 0, 8, 0);
            body.Children.Add(grid);
            var expander = new Expander { Header = T("cp_raw", "Raw values"), Content = body, IsExpanded = false };
            expander.SetResourceReference(Control.ForegroundProperty, "TextColor");
            var card = new Border { Style = (Style)FindResource("DashCard"), Margin = new Thickness(0, 0, 0, 12), Padding = new Thickness(14, 10, 14, 10), Child = expander };
            body.Margin = new Thickness(0, 10, 0, 0);
            return card;
        }

        // ----- data -----

        private bool _listLap;

        /// <summary>
        /// The number the creator shows. In a lap race checkpoint 0 lies on the start line and is
        /// start and finish, so the others count from 1; point to point, the last one is the finish.
        /// </summary>
        private static int Number(int index, bool lap) => lap ? index : index + 1;

        private static bool IsFinish(int index, bool lap, int count) => lap ? index == 0 : index == count - 1;

        private void Load()
        {
            if (!_built || !IsVisible)
                return;
            int count = RaceCheckpoints.Count;
            _count.Text = count.ToString(CultureInfo.CurrentCulture);
            bool lap = RaceCheckpoints.IsLap;
            if (_pointBox.Items.Count != count || _listLap != lap)
            {
                _listLap = lap;
                int keep = _pointBox.SelectedIndex;
                _loading = true;
                _pointBox.Items.Clear();
                for (int i = 0; i < count; i++)
                    _pointBox.Items.Add(IsFinish(i, lap, count) ? T("cp_map_finish", "Finish") : Number(i, lap).ToString(CultureInfo.CurrentCulture));
                _loading = false;
                _pointBox.SelectedIndex = count == 0 ? -1 : Math.Max(0, Math.Min(keep, count - 1));
            }
            LoadTransformNames();
            ShowPoint();
            ReadPoints();
            DrawMap();
        }

        private void LoadTransformNames()
        {
            if (_transform.IsDropDownOpen || !RaceCheckpoints.Ready)
                return;
            var names = new List<string> { T("cp_tf_none", "No transform"), T("cp_tf_random", "Random"), T("cp_tf_lobby", "Lobby vehicle") };
            foreach (var (slot, hash) in RaceCheckpoints.TransformVehicles())
            {
                string name = RaceCheckpoints.VehicleName(hash);
                names.Add(name != null ? slot + " · " + name : string.Format(CultureInfo.CurrentCulture, T("cp_tf_empty", "{0} · empty slot"), slot));
            }
            string joined = string.Join("|", names);
            if (joined == _transformNames)
                return;
            _transformNames = joined;
            _loading = true;
            int keep = _transform.SelectedIndex;
            _transform.Items.Clear();
            foreach (var n in names)
                _transform.Items.Add(n);
            _transform.SelectedIndex = keep;
            _loading = false;
        }

        private static void Put(TextBox box, float value)
        {
            if (!box.IsKeyboardFocusWithin)
                box.Text = F(value);
        }

        private static void Put(TextBox box, int value)
        {
            if (!box.IsKeyboardFocusWithin)
                box.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void Pick(ComboBox box, int index)
        {
            if (!box.IsDropDownOpen && box.SelectedIndex != index)
                box.SelectedIndex = index;
        }

        private void ShowPoint()
        {
            if (!_built)
                return;
            bool live = Live, sec = Secondary;
            foreach (var element in new FrameworkElement[] { _pos[0], _pos[1], _pos[2], _heading[0], _collect, _shrink, _pitch, _tall, _pitchOn, _transform, _planeTurn, _deluxo, _stromberg, _raw1, _raw2, _raw3 })
                element.IsEnabled = live;
            foreach (var row in _respawn)
                foreach (var box in row)
                    box.IsEnabled = live;
            _visual.IsEnabled = live && !sec;
            _random.IsEnabled = RaceCheckpoints.Ready;
            _noSecondaryBox.Visibility = live && sec && !RaceCheckpoints.HasSecondary(Index) ? Visibility.Visible : Visibility.Collapsed;
            if (!live)
            {
                foreach (var (_, chip) in _chips)
                    chip.IsEnabled = false;
                return;
            }

            _loading = true;
            int i = Index;
            var p = RaceCheckpoints.GetPos(i, sec);
            Put(_pos[0], p.X);
            Put(_pos[1], p.Y);
            Put(_pos[2], p.Z);
            Put(_heading[0], RaceCheckpoints.GetFloat(RaceCheckpoints.HeadingField(sec), i));
            for (int n = 0; n < 3; n++)
            {
                var r = RaceCheckpoints.GetRespawn(i, sec, n);
                Put(_respawn[n][0], r.X);
                Put(_respawn[n][1], r.Y);
                Put(_respawn[n][2], r.Z);
            }
            Put(_collect, RaceCheckpoints.GetFloat(RaceCheckpoints.CollectSizeField(sec), i));
            Put(_visual, RaceCheckpoints.GetFloat(GTA.Offsets.Editor.Race.Checkpoints.chvs, i));
            Put(_shrink, RaceCheckpoints.GetFloat(RaceCheckpoints.ShrinkField(sec), i));
            Put(_pitch, RaceCheckpoints.GetFloat(RaceCheckpoints.PitchField(sec), i));
            _tall.IsChecked = RaceCheckpoints.GetFlag(RaceCheckpoints.Tall, i, sec);
            _pitchOn.IsChecked = RaceCheckpoints.GetFlag(RaceCheckpoints.Pitch, i, sec);
            _shrink.IsEnabled = _tall.IsChecked == true;
            _pitch.IsEnabled = _pitchOn.IsChecked == true;
            foreach (var (flag, chip) in _chips)
            {
                // Options shared by both points are only offered on the primary one.
                chip.IsEnabled = !sec || flag.Secondary != flag.Primary;
                chip.IsChecked = RaceCheckpoints.GetFlag(flag, i, sec);
            }
            Pick(_transform, TransformItem(RaceCheckpoints.GetInt(RaceCheckpoints.TransformField(sec), i)));
            Pick(_planeTurn, RaceCheckpoints.GetPlaneTurn(i, sec));
            Pick(_deluxo, RaceCheckpoints.GetInt(RaceCheckpoints.DeluxoField(sec), i) + 1);
            Pick(_stromberg, RaceCheckpoints.GetInt(RaceCheckpoints.StrombergField(sec), i) + 1);
            Pick(_random, RaceCheckpoints.RandomTransform);
            Put(_raw1, RaceCheckpoints.GetInt(GTA.Offsets.Editor.Race.Checkpoints.cpbs1, i));
            Put(_raw2, RaceCheckpoints.GetInt(GTA.Offsets.Editor.Race.Checkpoints.cpbs2, i));
            Put(_raw3, RaceCheckpoints.GetInt(GTA.Offsets.Editor.Race.Checkpoints.cpbs3, i));
            _loading = false;
        }

        private void PickMode(bool secondary)
        {
            _primaryTab.IsChecked = !secondary;
            _secondaryTab.IsChecked = secondary;
            ShowPoint();
            DrawMap();
        }

        private void Step(int direction)
        {
            int count = _pointBox.Items.Count;
            if (count == 0)
                return;
            int i = Index < 0 ? 0 : Index;
            for (int tries = 0; tries < count; tries++)
            {
                i = (i + direction + count) % count;
                if (_skipFake.IsChecked != true || !RaceCheckpoints.Ready || !RaceCheckpoints.GetFlag(RaceCheckpoints.Fake, i, false))
                    break;
            }
            _pointBox.SelectedIndex = i;
        }

        private void ToCursor(TextBox[] boxes)
        {
            if (!Live)
                return;
            var at = Functions.Read.getlocation();
            for (int k = 0; k < 3; k++)
                boxes[k].Text = at[k];
        }

        private void ResetRespawn(int slot)
        {
            if (!Live)
                return;
            RaceCheckpoints.SetRespawn(Index, Secondary, slot, 0, 0, 0);
            ShowPoint();
            DrawMap();
        }

        private async System.Threading.Tasks.Task InsertAsync()
        {
            if (!Live)
                return;
            int i = Index, count = RaceCheckpoints.Count;
            var a = RaceCheckpoints.GetPos(i, false);
            float x, y, z;
            if (i + 1 < count)
            {
                var b = RaceCheckpoints.GetPos(i + 1, false);
                (x, y, z) = ((a.X + b.X) / 2, (a.Y + b.Y) / 2, (a.Z + b.Z) / 2);
            }
            else
            {
                var at = Functions.Read.getlocation();
                if (!TryFloat(at[0], out x) || !TryFloat(at[1], out y) || !TryFloat(at[2], out z))
                    return;
            }
            int added = RaceCheckpoints.InsertAfter(i, x, y, z);
            if (added < 0)
                return;
            Load();
            _pointBox.SelectedIndex = added;
            // The creator only shows the moved checkpoints after rebuilding from the job data.
            await CreatorMap.RebuildAsync();
            Load();
        }

        private void FlyTo()
        {
            if (!Live)
                return;
            var p = RaceCheckpoints.GetPos(Index, Secondary);
            if (p.X == 0 && p.Y == 0 && p.Z == 0)
                return;
            GTA.Teleport(new XenVector3(p.X, p.Y, p.Z + 2));
        }

        // ----- map -----

        private static readonly Brush Accent = Frozen(0x5B, 0x9B, 0xE6), Yellow = Frozen(0xE8, 0xC5, 0x47), Purple = Frozen(0xC7, 0x7D, 0xFF),
            Green = Frozen(0x43, 0xB5, 0x81), Red = Frozen(0xF0, 0x47, 0x47), Orange = Frozen(0xF2, 0x8C, 0x28);

        private static Brush Frozen(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private void ReadPoints()
        {
            _points.Clear();
            int count = RaceCheckpoints.Count;
            for (int i = 0; i < count; i++)
            {
                for (int s = 0; s < 2; s++)
                {
                    bool sec = s == 1;
                    var p = RaceCheckpoints.GetPos(i, sec);
                    if (p.X == 0 && p.Y == 0 && p.Z == 0)
                        continue;
                    bool fake = RaceCheckpoints.GetFlag(RaceCheckpoints.Fake, i, sec);
                    bool transform = RaceCheckpoints.GetInt(RaceCheckpoints.TransformField(sec), i) != -1;
                    _points.Add((i, sec, p.X, p.Y, fake, transform));
                }
            }
        }

        // North up, one scale for both axes, at least 60 m across.
        private void DrawMap()
        {
            _map.Children.Clear();
            double w = _map.ActualWidth, h = _map.ActualHeight;
            if (w < 20 || h < 20)
                return;
            if (_points.Count == 0)
            {
                var empty = Muted(T("cp_map_empty", "No checkpoints yet."), new Thickness(12, 10, 0, 0));
                _map.Children.Add(empty);
                return;
            }

            var respawns = new List<(float X, float Y, int N)>();
            if (Live)
                for (int n = 0; n < 3; n++)
                {
                    var r = RaceCheckpoints.GetRespawn(Index, Secondary, n);
                    if (r.X != 0 || r.Y != 0)
                        respawns.Add((r.X, r.Y, n + 1));
                }

            const double pad = 26;
            var xs = _points.Select(p => (double)p.X).Concat(respawns.Select(r => (double)r.X)).ToList();
            var ys = _points.Select(p => (double)p.Y).Concat(respawns.Select(r => (double)r.Y)).ToList();
            double minX = xs.Min(), maxX = xs.Max(), minY = ys.Min(), maxY = ys.Max();
            double scale = Math.Min((w - 2 * pad) / Math.Max(60, maxX - minX), (h - 2 * pad) / Math.Max(60, maxY - minY));
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            Point ToMap(double x, double y) => new Point(w / 2 + (x - cx) * scale, h / 2 - (y - cy) * scale);
            SatelliteTiles.Draw(_map, w, h, cx, cy, scale);

            // The route through the primary points, closed for lap races.
            var primary = _points.Where(p => !p.Secondary).OrderBy(p => p.Index).ToList();
            if (primary.Count > 1)
            {
                var line = new Polyline { Stroke = Yellow, StrokeThickness = 2.5, StrokeDashArray = new DoubleCollection { 3, 2 }, Opacity = 0.85, IsHitTestVisible = false };
                foreach (var p in primary)
                    line.Points.Add(ToMap(p.X, p.Y));
                if (RaceCheckpoints.IsLap)
                    line.Points.Add(ToMap(primary[0].X, primary[0].Y));
                _map.Children.Add(line);
            }
            // Each secondary point hangs off its primary one.
            foreach (var s in _points.Where(p => p.Secondary))
            {
                var main = primary.FirstOrDefault(p => p.Index == s.Index);
                if (main.X == 0 && main.Y == 0)
                    continue;
                var a = ToMap(main.X, main.Y);
                var b = ToMap(s.X, s.Y);
                _map.Children.Add(new Line { X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y, Stroke = Yellow, StrokeThickness = 1.2, Opacity = 0.6, IsHitTestVisible = false });
            }

            var selected = _points.FirstOrDefault(p => p.Index == Index && p.Secondary == Secondary);
            bool hasSelected = Live && (selected.X != 0 || selected.Y != 0);
            if (hasSelected)
            {
                var at = ToMap(selected.X, selected.Y);
                foreach (var r in respawns)
                {
                    var rp = ToMap(r.X, r.Y);
                    _map.Children.Add(new Line { X1 = at.X, Y1 = at.Y, X2 = rp.X, Y2 = rp.Y, Stroke = Green, StrokeThickness = 1.2, StrokeDashArray = new DoubleCollection { 3, 2 }, Opacity = 0.9, IsHitTestVisible = false });
                    var dot = new Border { Width = 22, Height = 16, CornerRadius = new CornerRadius(8), Background = Green, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1),
                        Child = new TextBlock { Text = "R" + r.N, FontSize = 9.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                        ToolTip = T("cp_map_respawn", "Respawn") + " R" + r.N };
                    Canvas.SetLeft(dot, rp.X - 11);
                    Canvas.SetTop(dot, rp.Y - 8);
                    Panel.SetZIndex(dot, 10);
                    _map.Children.Add(dot);
                }
            }

            int count = _pointBox.Items.Count;
            bool lap = RaceCheckpoints.IsLap;
            // The selected point last, so it lies on top.
            foreach (var p in _points.OrderBy(p => p.Index == Index && p.Secondary == Secondary))
            {
                bool picked = p.Index == Index && p.Secondary == Secondary;
                bool finish = !p.Secondary && IsFinish(p.Index, lap, count);
                Brush fill = picked ? Accent : finish ? Red : p.Fake ? Orange : p.Transform ? Purple : Yellow;
                double size = picked ? 22 : p.Secondary ? 14 : 18;
                var at = ToMap(p.X, p.Y);
                var dot = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = fill,
                    Stroke = picked ? Brushes.White : Brushes.Black,
                    StrokeThickness = picked ? 2 : 0.8,
                    Cursor = Cursors.Hand,
                    ToolTip = (finish ? T("cp_map_finish", "Finish") : Number(p.Index, lap).ToString(CultureInfo.CurrentCulture)) + (p.Secondary ? " · " + T("secondary", "Secondary") : "") + (p.Fake ? " · " + T("cp_f_fake", "Fake") : ""),
                };
                Canvas.SetLeft(dot, at.X - size / 2);
                Canvas.SetTop(dot, at.Y - size / 2);
                int index = p.Index;
                bool sec = p.Secondary;
                dot.MouseLeftButtonUp += (_, __) => PickPoint(index, sec);
                _map.Children.Add(dot);

                string label = finish ? "Z" : Number(p.Index, lap).ToString(CultureInfo.CurrentCulture) + (p.Secondary ? "b" : "");
                var text = new TextBlock
                {
                    Text = label,
                    FontSize = p.Index >= 99 ? 8.5 : 10,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false,
                    Foreground = picked || finish ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x1D, 0x1D, 0x1D)),
                };
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(text, at.X - text.DesiredSize.Width / 2);
                Canvas.SetTop(text, at.Y - text.DesiredSize.Height / 2);
                _map.Children.Add(text);
            }

            var north = new TextBlock { Text = "N ↑", FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, Opacity = 0.85 };
            Canvas.SetRight(north, 8);
            Canvas.SetTop(north, 6);
            _map.Children.Add(north);
        }

        private void PickPoint(int index, bool secondary)
        {
            if (secondary != Secondary)
            {
                _primaryTab.IsChecked = !secondary;
                _secondaryTab.IsChecked = secondary;
            }
            if (_pointBox.SelectedIndex != index)
                _pointBox.SelectedIndex = index;
            else
            {
                ShowPoint();
                DrawMap();
            }
        }
    }
}
