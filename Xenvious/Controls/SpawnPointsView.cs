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
    /// Player Settings › Spawn points (mockup https://claude.ai/artifact/N54PKsjqaQEMZ3xDnWeJHZ):
    /// the job's spawn points (SpawnPoints) on a top-down map and as a list on the left, the picked
    /// point on the right. Position and heading show in the creator at once (EntityMover moves the
    /// point's arrow decal); adding and deleting rebuild.
    /// </summary>
    public class SpawnPointsView : Grid
    {
        private readonly SectionCard _listCard = new SectionCard();
        private readonly SectionCard _editCard = new SectionCard();
        private readonly Canvas _map = new Canvas { Height = 220, ClipToBounds = true, Background = Brushes.Transparent };
        private readonly StackPanel _rows = new StackPanel();
        private readonly StackPanel _edit = new StackPanel();
        private readonly TextBox _x = Box(), _y = Box(), _z = Box(), _heading = Box(), _spmr = Box();
        private readonly StackPanel _teams = new StackPanel { Orientation = Orientation.Horizontal };
        private readonly FrameworkElement _teamsRow;
        private readonly Button _add, _addPlayer, _delete;
        private readonly Action _rebuild;
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private List<(int Index, float X, float Y, float Z, float Head, int Teams)> _points = new List<(int, float, float, float, float, int)>();
        private int _picked;
        private bool _loading;
        private string _shown;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static TextBox Box()
        {
            var box = new TextBox { Height = 30 };
            box.SetResourceReference(StyleProperty, "Watermark");
            return box;
        }

        public SpawnPointsView(Action rebuild)
        {
            _rebuild = rebuild;
            Margin = new Thickness(0, 12, 0, 12);
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            ColumnDefinitions.Add(new ColumnDefinition());

            foreach (var card in new[] { _listCard, _editCard })
                card.Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            _listCard.Icon = Geometry.Parse("M12,2 C8,2 5,5 5,9 C5,14 12,22 12,22 C12,22 19,14 19,9 C19,5 16,2 12,2 M12,6.5 A2.5,2.5 0 1 0 12,11.5 A2.5,2.5 0 1 0 12,6.5");
            _editCard.Icon = Geometry.Parse("M12,2 L15,9 L12,7.5 L9,9 Z M12,10 A6,6 0 1 0 12.01,10 Z");
            Children.Add(_listCard);
            Grid.SetColumn(_editCard, 2);
            Children.Add(_editCard);
            _listCard.VerticalAlignment = _editCard.VerticalAlignment = VerticalAlignment.Top;

            // Left: map, list, buttons.
            var list = new StackPanel();
            var frame = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Child = _map, Margin = new Thickness(0, 0, 0, 10),
                ToolTip = T("spn_map_hint", "Every spawn point from above, north up. Click an arrow to pick it.") };
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            list.Children.Add(frame);
            _map.SizeChanged += (_, __) => DrawMap();
            SatelliteTiles.TileLoaded += () => Dispatcher.BeginInvoke(new Action(() => { if (IsVisible) DrawMap(); }), DispatcherPriority.Background);
            list.Children.Add(_rows);
            var buttons = new WrapPanel { Margin = new Thickness(0, 4, 0, 0) };
            _add = Button(T("spn_add_cursor", "+ At cursor"), true, () => Add(Cursor()));
            _addPlayer = Button(T("spn_add_player", "+ Player position"), false, () => Add(Player()));
            _delete = Button(T("spn_delete", "Delete"), false, Delete);
            buttons.Children.Add(_add);
            buttons.Children.Add(_addPlayer);
            buttons.Children.Add(_delete);
            list.Children.Add(buttons);
            list.Children.Add(Faint(T("spn_add_hint", "Adding and deleting rebuild the creator so it shows the points again."), 11.5));
            _listCard.Content = list;

            // Right: the picked point.
            var xyz = new UniformGrid { Columns = 3 };
            xyz.Children.Add(Field("X", _x, new Thickness(0, 0, 6, 0)));
            xyz.Children.Add(Field("Y", _y, new Thickness(3, 0, 3, 0)));
            xyz.Children.Add(Field("Z", _z, new Thickness(6, 0, 0, 0)));
            _edit.Children.Add(xyz);
            var headingPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            headingPanel.Children.Add(Label(T("spn_heading", "Direction")));
            headingPanel.Children.Add(_heading);
            _edit.Children.Add(headingPanel);
            var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
            actions.Children.Add(Button(T("spn_take_cursor", "Take position"), false, TakeCursor));
            actions.Children.Add(Button(T("spn_goto", "⌖ Go there"), false, GoThere));
            _edit.Children.Add(actions);
            var teams = new StackPanel { Margin = new Thickness(0, 6, 0, 0) };
            teams.Children.Add(Label(T("spn_teams", "For teams")));
            var teamRow = new WrapPanel();
            teamRow.Children.Add(_teams);
            teamRow.Children.Add(Faint(T("spn_teams_none", "none = all"), 12).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(10, 0, 0, 0); }));
            teams.Children.Add(teamRow);
            _teamsRow = teams;
            _edit.Children.Add(teams);
            var spmr = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
            spmr.Children.Add(Label(T("spn_spmr", "dmSPMR (bits 0-24, 0 = all)")));
            spmr.Children.Add(_spmr);
            spmr.Children.Add(Faint(T("spn_spmr_hint", "What these bits pick is not known yet; the creator checks bits up to its cgsCNT."), 11.5));
            _edit.Children.Add(spmr);
            _editCard.Content = _edit;
            _editCard.HeaderRight = LiveChip();
            HeadingSlider.Attach(_heading, () => Show(true));

            _x.TextChanged += (_, __) => WritePos();
            _y.TextChanged += (_, __) => WritePos();
            _z.TextChanged += (_, __) => WritePos();
            _heading.TextChanged += (_, __) =>
            {
                if (!_loading && float.TryParse(_heading.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float h))
                {
                    SpawnPoints.SetHeading(_picked, h);
                    Show(false);
                }
            };
            _spmr.TextChanged += (_, __) =>
            {
                if (!_loading && int.TryParse(_spmr.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                    SpawnPoints.Set(GTA.Offsets.Editor.SpawnPoints.spmr, _picked, v);
            };
            foreach (var box in new[] { _x, _y, _z })
                box.KeyDown += (_, e) => { if (e.Key == Key.Enter) Show(true); };

            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => Refresh(false);
        }

        private Button Button(string text, bool primary, Action click)
        {
            var b = new Button { Content = text, Height = 30, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 6, 6) };
            b.SetResourceReference(StyleProperty, primary ? "FormButtonPrimary" : "FormButton");
            b.Click += (_, __) => click();
            return b;
        }

        private static StackPanel Field(string label, TextBox box, Thickness margin)
        {
            var panel = new StackPanel { Margin = margin };
            panel.Children.Add(Label(label));
            panel.Children.Add(box);
            return panel;
        }

        private static TextBlock Label(string text)
        {
            var t = new TextBlock { Text = text };
            t.SetResourceReference(StyleProperty, "FieldLabel");
            return t;
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }

        private static FrameworkElement LiveChip()
        {
            var chip = new Border { CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(1), Padding = new Thickness(9, 2, 9, 2),
                ToolTip = T("spn_live_hint", "Position and direction show in the creator at once.") };
            chip.SetResourceReference(Border.BorderBrushProperty, "OkBrush");
            var text = new TextBlock { Text = T("spn_live", "live in the creator"), FontSize = 12, FontWeight = FontWeights.SemiBold };
            text.SetResourceReference(TextBlock.ForegroundProperty, "OkBrush");
            chip.Child = text;
            return chip;
        }

        private void Refresh(bool force)
        {
            if (!SpawnPoints.Ready)
            {
                _shown = null;
                _points.Clear();
                _rows.Children.Clear();
                _rows.Children.Add(Faint(T("spn_nocreator", "Open a job in the creator to see its spawn points."), 13));
                _listCard.Title = T("spn_points", "Spawn points");
                _listCard.Summary = "";
                _editCard.Visibility = Visibility.Collapsed;
                _add.IsEnabled = _addPlayer.IsEnabled = _delete.IsEnabled = false;
                DrawMap();
                return;
            }
            int n = SpawnPoints.Count;
            var points = Enumerable.Range(0, n).Select(i =>
            {
                var p = SpawnPoints.Pos(i);
                return (Index: i, X: p.X, Y: p.Y, Z: p.Z, Head: SpawnPoints.Heading(i), Teams: SpawnPoints.Get(GTA.Offsets.Editor.SpawnPoints.teams, i));
            }).ToList();
            if (_picked >= n)
                _picked = Math.Max(0, n - 1);
            int teams = Rules.Teams();
            string key = string.Join(";", points) + "|" + _picked + "|" + teams;
            if (!force && key == _shown)
                return;
            _shown = key;
            _points = points;
            _listCard.Title = T("spn_points", "Spawn points");
            _listCard.Summary = string.Format(CultureInfo.CurrentCulture, "{0} / {1}", n, SpawnPoints.Max);
            _add.IsEnabled = _addPlayer.IsEnabled = n < SpawnPoints.Max;
            _delete.IsEnabled = n > 0;
            _rows.Children.Clear();
            if (n == 0)
                _rows.Children.Add(Faint(T("spn_none", "This job has no spawn points yet."), 13));
            foreach (var p in points)
                _rows.Children.Add(Row(p.Index, p.X, p.Y, p.Z, p.Head, p.Teams, p.Index == _picked));
            DrawMap();
            LoadPicked(teams);
        }

        private void LoadPicked(int teams)
        {
            if (_points.Count == 0)
            {
                _editCard.Visibility = Visibility.Collapsed;
                return;
            }
            _editCard.Visibility = Visibility.Visible;
            _editCard.Title = string.Format(CultureInfo.CurrentCulture, T("spn_point_n", "Spawn point {0}"), _picked + 1);
            var p = _points[_picked];
            _loading = true;
            Put(_x, p.X.ToString(CultureInfo.CurrentCulture));
            Put(_y, p.Y.ToString(CultureInfo.CurrentCulture));
            Put(_z, p.Z.ToString(CultureInfo.CurrentCulture));
            Put(_heading, p.Head.ToString(CultureInfo.CurrentCulture));
            Put(_spmr, SpawnPoints.Get(GTA.Offsets.Editor.SpawnPoints.spmr, _picked).ToString(CultureInfo.InvariantCulture));
            _loading = false;
            _teamsRow.Visibility = teams >= 2 ? Visibility.Visible : Visibility.Collapsed;
            _teams.Children.Clear();
            for (int t = 0; t < Math.Max(teams, 0) && t < 4; t++)
                _teams.Children.Add(TeamToggle(t, (p.Teams & (1 << t)) != 0));
        }

        // A box the user is typing in keeps its text.
        private static void Put(TextBox box, string text)
        {
            if (!box.IsKeyboardFocusWithin && box.Text != text)
                box.Text = text;
        }

        private FrameworkElement TeamToggle(int team, bool on)
        {
            var tab = new ToggleButton { Content = (team + 1).ToString(CultureInfo.CurrentCulture), IsChecked = on, Margin = new Thickness(0, 0, 4, 0),
                ToolTip = T("dash_team", "Team") + " " + (team + 1) };
            tab.SetResourceReference(StyleProperty, "NavTab");
            if (MainWindow.ThemeBrush("TeamBrush" + (team + 1)) is SolidColorBrush colour)
            {
                tab.Resources["AccentBrush"] = colour;
                tab.Resources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x70, colour.Color.R, colour.Color.G, colour.Color.B));
            }
            tab.Click += (_, __) =>
            {
                int bits = SpawnPoints.Get(GTA.Offsets.Editor.SpawnPoints.teams, _picked);
                bits = tab.IsChecked == true ? bits | (1 << team) : bits & ~(1 << team);
                SpawnPoints.Set(GTA.Offsets.Editor.SpawnPoints.teams, _picked, bits);
                Refresh(true);
            };
            return tab;
        }

        private FrameworkElement Row(int index, float x, float y, float z, float head, int teams, bool selected)
        {
            var button = new ToggleButton { IsChecked = selected, HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(0), Margin = new Thickness(0, 0, 0, 6), Cursor = Cursors.Hand };
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
            var title = new TextBlock { FontSize = 13.5, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, Text = TeamsText(teams) };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            text.Children.Add(title);
            text.Children.Add(Faint(string.Format(CultureInfo.InvariantCulture, "{0:0.0}, {1:0.0}, {2:0.0}", x, y, z), 11.5));
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
            var heading = new TextBlock { Text = ((int)Math.Round(((head % 360) + 360) % 360)).ToString(CultureInfo.InvariantCulture) + "°", FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
            heading.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            Grid.SetColumn(heading, 2);
            grid.Children.Add(heading);
            button.Content = grid;
            button.Click += (_, __) => Pick(index);
            return button;
        }

        private static string TeamsText(int teams)
        {
            if (teams == 0)
                return T("spn_all_teams", "All teams");
            var list = Enumerable.Range(0, 4).Where(t => (teams & (1 << t)) != 0).Select(t => (t + 1).ToString(CultureInfo.CurrentCulture));
            return T("dash_team", "Team") + " " + string.Join(", ", list);
        }

        private void Pick(int index)
        {
            _picked = index;
            Refresh(true);
        }

        private void WritePos()
        {
            if (_loading
                || !float.TryParse(_x.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float x)
                || !float.TryParse(_y.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float y)
                || !float.TryParse(_z.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float z))
                return;
            SpawnPoints.SetPos(_picked, x, y, z);
            Show(false);
        }

        // Shows the picked point's new pose in the creator; with `orRebuild` a rebuild when that
        // is not possible (Enter, the slider coming to rest, "take position").
        private async void Show(bool orRebuild)
        {
            if (await EntityMover.MoveAsync(EntityMover.Kind.SpawnPoint, _picked) || !orRebuild)
                return;
            _rebuild?.Invoke();
        }

        private static (float X, float Y, float Z)? Cursor()
        {
            try
            {
                var at = Functions.Read.GetLocationVec();
                return at.X == 0 && at.Y == 0 && at.Z == 0 ? ((float, float, float)?)null : (at.X, at.Y, at.Z);
            }
            catch
            {
                return null;
            }
        }

        private static (float X, float Y, float Z)? Player()
        {
            try
            {
                long at = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_player_loc).GetAddress();
                float x = MainWindow.m.memory(at.ToString("X")).Get<float>(), y = MainWindow.m.memory((at + 4).ToString("X")).Get<float>(), z = MainWindow.m.memory((at + 8).ToString("X")).Get<float>();
                return x == 0 && y == 0 && z == 0 ? ((float, float, float)?)null : (x, y, z);
            }
            catch
            {
                return null;
            }
        }

        private void Add((float X, float Y, float Z)? at)
        {
            if (at == null)
                return;
            int heading = 0;
            try { heading = (int)Functions.Read.GetHeading(); } catch { }
            int index = SpawnPoints.Add(at.Value.X, at.Value.Y, at.Value.Z, heading);
            if (index < 0)
                return;
            _picked = index;
            _rebuild?.Invoke();
            Refresh(true);
        }

        private void Delete()
        {
            if (_points.Count == 0)
                return;
            SpawnPoints.Delete(_picked);
            _picked = Math.Max(0, _picked - 1);
            _rebuild?.Invoke();
            Refresh(true);
        }

        private void TakeCursor()
        {
            var at = Cursor();
            if (at == null)
                return;
            SpawnPoints.SetPos(_picked, at.Value.X, at.Value.Y, at.Value.Z);
            Refresh(true);
            Show(true);
        }

        private void GoThere()
        {
            if (_picked < _points.Count)
            {
                var p = _points[_picked];
                GTA.Teleport(new XenVector3(p.X, p.Y, p.Z + 1));
            }
        }

        // North up, one scale for both axes, at least 40 m across (same as the start point map).
        private void DrawMap()
        {
            _map.Children.Clear();
            double w = _map.ActualWidth, h = _map.ActualHeight;
            if (w < 20 || h < 20)
                return;
            if (_points.Count == 0)
            {
                var empty = Faint(T("spn_map_empty", "No spawn points yet."), 12);
                Canvas.SetLeft(empty, 12);
                Canvas.SetTop(empty, 10);
                _map.Children.Add(empty);
                return;
            }
            const double pad = 22;
            double minX = _points.Min(p => p.X), maxX = _points.Max(p => p.X), minY = _points.Min(p => p.Y), maxY = _points.Max(p => p.Y);
            double scale = Math.Min((w - 2 * pad) / Math.Max(40, maxX - minX), (h - 2 * pad) / Math.Max(40, maxY - minY));
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            SatelliteTiles.Draw(_map, w, h, cx, cy, scale);
            var north = new TextBlock { Text = "N ↑", FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, Opacity = 0.85 };
            Canvas.SetRight(north, 8);
            Canvas.SetTop(north, 6);
            _map.Children.Add(north);
            foreach (var p in _points.OrderBy(p => p.Index == _picked))
            {
                bool picked = p.Index == _picked;
                var at = new Point(w / 2 + (p.X - cx) * scale, h / 2 - (p.Y - cy) * scale);
                var transform = new TransformGroup();
                transform.Children.Add(new ScaleTransform(picked ? 1.35 : 1, picked ? 1.35 : 1));
                // GTA headings turn counter-clockwise from north, WPF rotations clockwise.
                transform.Children.Add(new RotateTransform(-p.Head));
                transform.Children.Add(new TranslateTransform(at.X, at.Y));
                var arrow = new Path
                {
                    Data = Geometry.Parse("M0,-9 L6.5,7 L0,3.5 L-6.5,7 Z"),
                    RenderTransform = transform,
                    Cursor = Cursors.Hand,
                    Stroke = picked ? Brushes.White : Brushes.Black,
                    StrokeThickness = picked ? 1.6 : 0.8,
                    ToolTip = (p.Index + 1).ToString(CultureInfo.CurrentCulture),
                };
                arrow.SetResourceReference(Shape.FillProperty, picked ? "AccentBrush" : "MutedTextBrush");
                int index = p.Index;
                arrow.MouseLeftButtonUp += (_, __) => Pick(index);
                _map.Children.Add(arrow);
            }
        }
    }
}
