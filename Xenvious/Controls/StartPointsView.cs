using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Player Settings (mockup https://claude.ai/artifact/VifG8TGRs71syLT1EsWBmP, tab "Player Settings"):
    /// a team's start points (Global_4980736.f_201288[team][i /*71*/], count f_197021[team]) as a
    /// list with heading and vehicle, above it a top-down map with every team's points as arrows
    /// in the team colours; a click picks the point in the page's own index box, which the
    /// editing cards below follow.
    /// </summary>
    public class StartPointsView : SectionCard
    {
        private readonly ComboBox _teamBox;
        private readonly ComboBox _indexBox;
        private readonly ComboBox _vehicleBox;
        private readonly StackPanel _list = new StackPanel();
        private readonly StackPanel _rows = new StackPanel();
        private readonly Canvas _map = new Canvas { Height = 210, ClipToBounds = true, Background = Brushes.Transparent };
        private List<(int Team, int Index, float X, float Y, float Head)> _points = new List<(int, int, float, float, float)>();
        private (float X, float Y)? _cursor;
        private readonly WrapPanel _add = new WrapPanel { Margin = new Thickness(0, 4, 0, 0) };
        private readonly ComboBox _addKind = new ComboBox { Height = 30, MinWidth = 150, Margin = new Thickness(0, 0, 6, 6) };
        private readonly Action _rebuild;
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private string _shown;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public StartPointsView(ComboBox teamBox, ComboBox indexBox, ComboBox vehicleBox, Action rebuild)
        {
            _rebuild = rebuild;
            _teamBox = teamBox;
            _indexBox = indexBox;
            _vehicleBox = vehicleBox;
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            Icon = Geometry.Parse("M12,2 C8,2 5,5 5,9 C5,14 12,22 12,22 C12,22 19,14 19,9 C19,5 16,2 12,2 M12,6.5 A2.5,2.5 0 1 0 12,11.5 A2.5,2.5 0 1 0 12,6.5");
            Content = _list;
            var frame = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Child = _map, Margin = new Thickness(0, 0, 0, 10),
                ToolTip = T("sp_map_hint", "Every team's start points from above, north up. Click an arrow to pick that point.") };
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            _list.Children.Add(frame);
            _map.SizeChanged += (_, __) => DrawMap();
            _list.Children.Add(_rows);
            _list.Children.Add(_add);
            BuildAdd();
            Margin = new Thickness(0, 0, 0, 12);
            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => Refresh(false);
            _teamBox.SelectionChanged += (_, __) => Refresh(true);
            _indexBox.SelectionChanged += (_, __) => Refresh(true);
        }

        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.player_number != 0
            && GTA.Offsets.Editor.player_loc != 0 && GTA.Offsets.Editor.next_settings != 0 && GTA.Offsets.Editor.team_NEXT_settings != 0;

        private static long At(long field, int team, int i) => field + team * GTA.Offsets.Editor.team_NEXT_settings + i * GTA.Offsets.Editor.next_settings;

        public static int Seat(int team, int i) => GTA.Offsets.Editor.player_seat == 0 ? -3 : new Global(At(GTA.Offsets.Editor.player_seat, team, i)).Get<int>();

        /// <summary>The creator's seats (func_4682 bones): -1 driver, 0 front passenger, then the rear rows.</summary>
        public static string SeatName(int seat)
        {
            switch (seat)
            {
                case -1: return T("sp_seat_driver", "Driver");
                case 0: return T("sp_seat_front", "Front passenger");
                case 1: return T("sp_seat_rl", "Rear left");
                case 2: return T("sp_seat_rr", "Rear right");
                default: return seat < -1 ? T("sp_seat_any", "Any free seat") : string.Format(CultureInfo.CurrentCulture, T("sp_seat_n", "Seat {0}"), seat + 2);
            }
        }

        private void Refresh(bool force)
        {
            if (!Live)
            {
                _rows.Children.Clear();
                _shown = null;
                Title = T("sp_points", "Start points");
                _add.Visibility = Visibility.Collapsed;
                HeaderRight = null;
                _points.Clear();
                _map.Children.Clear();
                _rows.Children.Add(Faint(T("sp_nocreator", "Open a mission in the creator to see its start points."), 13));
                return;
            }
            int team = Math.Max(0, _teamBox.SelectedIndex);
            int n = Math.Max(0, Math.Min(new Global(GTA.Offsets.Editor.player_number + team).Get<int>(), 60));
            var rows = Enumerable.Range(0, n).Select(i => (
                Index: i,
                X: new Global(At(GTA.Offsets.Editor.player_loc, team, i)).Get<float>(),
                Y: new Global(At(GTA.Offsets.Editor.player_loc, team, i) + 1).Get<float>(),
                Z: new Global(At(GTA.Offsets.Editor.player_loc, team, i) + 2).Get<float>(),
                Head: new Global(At(GTA.Offsets.Editor.player_head, team, i)).Get<float>(),
                Veh: new Global(At(GTA.Offsets.Editor.player_veh, team, i)).Get<int>(),
                Seat: Seat(team, i),
                Bits: StartPoints.Bits(team, i))).ToList();
            var points = new List<(int Team, int Index, float X, float Y, float Head)>();
            int teams = Rules.Teams();
            for (int t = 0; t < teams; t++)
            {
                if (t == team)
                {
                    points.AddRange(rows.Select(r => (t, r.Index, r.X, r.Y, r.Head)));
                    continue;
                }
                int count = Math.Max(0, Math.Min(new Global(GTA.Offsets.Editor.player_number + t).Get<int>(), 60));
                for (int i = 0; i < count; i++)
                    points.Add((t, i, new Global(At(GTA.Offsets.Editor.player_loc, t, i)).Get<float>(), new Global(At(GTA.Offsets.Editor.player_loc, t, i) + 1).Get<float>(),
                        new Global(At(GTA.Offsets.Editor.player_head, t, i)).Get<float>()));
            }
            _cursor = Cursor();
            string key = string.Join(";", rows) + "|" + string.Join(";", points) + "|" + _cursor + "|" + team + "|" + teams + "|" + _indexBox.SelectedIndex + "|" + Rules.PublicCreator;
            if (!force && key == _shown)
                return;
            _shown = key;
            _points = points;
            HeaderRight = TeamTabs(team, teams);
            DrawMap();
            _add.Visibility = Visibility.Visible;
            _add.IsEnabled = StartPoints.Ready && n < StartPoints.Max;

            Title = string.Format(CultureInfo.CurrentCulture, T("sp_points_team", "Start points · team {0}"), team + 1);
            Summary = string.Format(CultureInfo.CurrentCulture, T("sp_points_sum", "{0} points"), n);
            _rows.Children.Clear();
            if (n == 0)
                _rows.Children.Add(Faint(T("sp_none", "This team has no start point yet: place one in the creator."), 13));
            foreach (var r in rows)
                _rows.Children.Add(Row(r.Index, r.X, r.Y, r.Z, r.Head, r.Veh, r.Seat, r.Bits, r.Index == _indexBox.SelectedIndex));
        }

        private FrameworkElement Row(int index, float x, float y, float z, float head, int veh, int seat, int bits, bool selected)
        {
            var button = new System.Windows.Controls.Primitives.ToggleButton { IsChecked = selected, HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(0), Margin = new Thickness(0, 0, 0, 6), Cursor = System.Windows.Input.Cursors.Hand };
            button.SetResourceReference(StyleProperty, "ChoiceTile");
            var grid = new Grid { Margin = new Thickness(10, 7, 10, 7) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var dot = new Border { Width = 28, Height = 28, CornerRadius = new CornerRadius(14), BorderThickness = new Thickness(2), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
            dot.SetResourceReference(Border.BorderBrushProperty, selected ? "AccentBrush" : "LineBrush");
            var num = new TextBlock { Text = (index + 1).ToString(CultureInfo.CurrentCulture), FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            num.SetResourceReference(TextBlock.ForegroundProperty, selected ? "AccentBrush" : "MutedTextBrush");
            dot.Child = num;
            grid.Children.Add(dot);

            var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var title = new TextBlock { FontSize = 13.5, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            title.Text = veh > -1
                ? VehicleName(veh) + " · " + SeatName(seat)
                : T("sp_onfoot", "On foot");
            text.Children.Add(title);
            var roles = new[]
            {
                (StartPoints.BitStart, T("sp_tag_start", "Start")),
                (StartPoints.BitRespawn, T("sp_tag_respawn", "Respawn")),
                (StartPoints.BitCheckpoint1, T("sp_tag_cp1", "CP 1")),
                (StartPoints.BitCheckpoint2, T("sp_tag_cp2", "CP 2")),
            }.Where(b => (bits & (1 << b.Item1)) != 0).Select(b => b.Item2).ToList();
            string where = string.Format(CultureInfo.InvariantCulture, "{0:0.0}, {1:0.0}, {2:0.0}", x, y, z);
            text.Children.Add(Faint((roles.Count > 0 ? string.Join(" · ", roles) + "   " : T("sp_tag_none", "unused") + "   ") + where, 11.5));
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            var heading = new TextBlock { Text = ((int)Math.Round(((head % 360) + 360) % 360)).ToString(CultureInfo.InvariantCulture) + "°", FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
            heading.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            Grid.SetColumn(heading, 2);
            grid.Children.Add(heading);

            button.Content = grid;
            button.Click += (_, __) => { _indexBox.SelectedIndex = index; Refresh(true); };
            return button;
        }

        // "+ At cursor" with what the new point is for (the creator's two switches).
        private void BuildAdd()
        {
            _addKind.Items.Add(new ComboBoxItem { Content = T("sp_kind_both", "Start and respawn point"), Tag = 3 });
            _addKind.Items.Add(new ComboBoxItem { Content = T("sp_kind_start", "Start point"), Tag = 1 });
            _addKind.Items.Add(new ComboBoxItem { Content = T("sp_kind_respawn", "Respawn point"), Tag = 2 });
            _addKind.SelectedIndex = 0;
            var button = new Button { Content = T("sp_add_cursor", "+ At cursor"), Height = 30, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 6, 6),
                ToolTip = T("sp_add_hint", "Adds a point at the creator's cursor and rebuilds so the creator shows it.") };
            button.SetResourceReference(StyleProperty, "FormButtonPrimary");
            button.Click += (_, __) => AddAtCursor();
            _add.Children.Add(button);
            _add.Children.Add(Faint(T("sp_as", "as"), 13).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(0, 0, 6, 6); }));
            _add.Children.Add(_addKind);
        }

        private void AddAtCursor()
        {
            if (MainWindow.m == null || !MainWindow.m.IsProcOpen || !StartPoints.Ready)
                return;
            int kind = _addKind.SelectedItem is ComboBoxItem k ? (int)k.Tag : 3;
            var at = Functions.Read.getlocation();
            if (at == null || at.Count < 3
                || !float.TryParse(at[0], NumberStyles.Float, CultureInfo.CurrentCulture, out float x)
                || !float.TryParse(at[1], NumberStyles.Float, CultureInfo.CurrentCulture, out float y)
                || !float.TryParse(at[2], NumberStyles.Float, CultureInfo.CurrentCulture, out float z)
                || (x == 0 && y == 0 && z == 0))
                return;
            int team = Math.Max(0, _teamBox.SelectedIndex);
            int index = StartPoints.Add(team, x, y, z, 0f, (kind & 1) != 0, (kind & 2) != 0);
            if (index < 0)
                return;
            MainWindow.Instance?.refreshPLYRCount();
            _indexBox.SelectedIndex = index;
            _rebuild?.Invoke();
            Refresh(true);
        }

        // Teams as nav tabs in their colours (team selector style); a click shows that team.
        private FrameworkElement TeamTabs(int selected, int teams)
        {
            if (teams < 2)
                return null;
            var tabs = new StackPanel { Orientation = Orientation.Horizontal };
            for (int t = 0; t < teams; t++)
            {
                int team = t;
                var tab = new System.Windows.Controls.Primitives.ToggleButton { Content = (t + 1).ToString(CultureInfo.CurrentCulture), IsChecked = t == selected, ToolTip = T("dash_team", "Team") + " " + (t + 1) };
                tab.SetResourceReference(StyleProperty, "NavTab");
                if (MainWindow.ThemeBrush("TeamBrush" + (t + 1)) is SolidColorBrush colour)
                {
                    tab.Resources["AccentBrush"] = colour;
                    tab.Resources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x70, colour.Color.R, colour.Color.G, colour.Color.B));
                }
                tab.Click += (_, __) => Pick(team, -1);
                tabs.Children.Add(tab);
            }
            var box = new Border { Child = tabs };
            box.SetResourceReference(StyleProperty, "NavGroup");
            return box;
        }

        private void Pick(int team, int index)
        {
            if (_teamBox.SelectedIndex != team && team < _teamBox.Items.Count)
                _teamBox.SelectedIndex = team;
            if (index >= 0 && index < _indexBox.Items.Count)
                _indexBox.SelectedIndex = index;
            Refresh(true);
        }

        private static (float X, float Y)? Cursor()
        {
            var at = Functions.Read.getlocation();
            if (at == null || at.Count < 2
                || !float.TryParse(at[0], NumberStyles.Float, CultureInfo.CurrentCulture, out float x)
                || !float.TryParse(at[1], NumberStyles.Float, CultureInfo.CurrentCulture, out float y)
                || (x == 0 && y == 0))
                return null;
            return (x, y);
        }

        // North up, one scale for both axes, at least 40 m across so a single point does not fill it.
        private void DrawMap()
        {
            _map.Children.Clear();
            double w = _map.ActualWidth, h = _map.ActualHeight;
            if (w < 20 || h < 20)
                return;
            var xs = _points.Select(p => p.X).ToList();
            var ys = _points.Select(p => p.Y).ToList();
            if (_cursor.HasValue)
            {
                xs.Add(_cursor.Value.X);
                ys.Add(_cursor.Value.Y);
            }
            if (xs.Count == 0)
            {
                var empty = Faint(T("sp_map_empty", "No start points yet."), 12);
                Canvas.SetLeft(empty, 12);
                Canvas.SetTop(empty, 10);
                _map.Children.Add(empty);
                return;
            }
            const double pad = 22;
            double minX = xs.Min(), maxX = xs.Max(), minY = ys.Min(), maxY = ys.Max();
            double scale = Math.Min((w - 2 * pad) / Math.Max(40, maxX - minX), (h - 2 * pad) / Math.Max(40, maxY - minY));
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            Point ToMap(double x, double y) => new Point(w / 2 + (x - cx) * scale, h / 2 - (y - cy) * scale);

            var north = new TextBlock { Text = "N ↑", FontSize = 11, FontWeight = FontWeights.SemiBold };
            north.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            Canvas.SetRight(north, 8);
            Canvas.SetTop(north, 6);
            _map.Children.Add(north);

            if (_cursor.HasValue)
            {
                var c = ToMap(_cursor.Value.X, _cursor.Value.Y);
                var cross = new Path { Data = Geometry.Parse("M-7,0 L7,0 M0,-7 L0,7"), StrokeThickness = 1.5, RenderTransform = new TranslateTransform(c.X, c.Y), ToolTip = T("sp_map_cursor", "Creator cursor") };
                cross.SetResourceReference(Shape.StrokeProperty, "MutedTextBrush");
                _map.Children.Add(cross);
            }

            int shown = Math.Max(0, _teamBox.SelectedIndex);
            // The shown team last, so its arrows lie on top.
            foreach (var p in _points.OrderBy(p => p.Team == shown).ThenBy(p => p.Team == shown && p.Index == _indexBox.SelectedIndex))
            {
                bool own = p.Team == shown, picked = own && p.Index == _indexBox.SelectedIndex;
                var at = ToMap(p.X, p.Y);
                var transform = new TransformGroup();
                transform.Children.Add(new ScaleTransform(picked ? 1.35 : 1, picked ? 1.35 : 1));
                // GTA headings turn counter-clockwise from north, WPF rotations clockwise.
                transform.Children.Add(new RotateTransform(-p.Head));
                transform.Children.Add(new TranslateTransform(at.X, at.Y));
                var arrow = new Path
                {
                    Data = Geometry.Parse("M0,-9 L6.5,7 L0,3.5 L-6.5,7 Z"),
                    Fill = MainWindow.ThemeBrush("TeamBrush" + (p.Team + 1)),
                    Opacity = own ? 1 : 0.45,
                    StrokeThickness = picked ? 1.6 : 0,
                    RenderTransform = transform,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    ToolTip = T("dash_team", "Team") + " " + (p.Team + 1) + " · " + (p.Index + 1),
                };
                if (picked)
                    arrow.SetResourceReference(Shape.StrokeProperty, "TextColor");
                int team = p.Team, index = p.Index;
                arrow.MouseLeftButtonUp += (_, __) => Pick(team, index);
                _map.Children.Add(arrow);
                if (own)
                {
                    var num = new TextBlock { Text = (p.Index + 1).ToString(CultureInfo.CurrentCulture), FontSize = 10.5, FontWeight = FontWeights.Bold, IsHitTestVisible = false };
                    num.SetResourceReference(TextBlock.ForegroundProperty, picked ? "TextColor" : "MutedTextBrush");
                    Canvas.SetLeft(num, at.X + 9);
                    Canvas.SetTop(num, at.Y - 16);
                    _map.Children.Add(num);
                }
            }
        }

        // The vehicle box lists "none" first, then the placed vehicles by index.
        private string VehicleName(int veh)
        {
            if (veh + 1 < _vehicleBox.Items.Count)
            {
                object item = _vehicleBox.Items[veh + 1];
                string name = item is ComboBoxItem c ? c.Content?.ToString() : item?.ToString();
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
            return string.Format(CultureInfo.CurrentCulture, T("sp_vehicle_n", "Vehicle {0}"), veh + 1);
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }
    }
}
