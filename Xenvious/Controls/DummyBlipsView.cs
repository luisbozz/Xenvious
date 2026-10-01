using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// The free (dummy) blips (Global_4980736.f_218705[i /*122*/], count f_222366) as in the mockup
    /// https://claude.ai/artifact/VifG8TGRs71syLT1EsWBmP, tab "Blips": a top view with every blip
    /// in its colour, a list (name, colour, rule, team), "+ at cursor" and colour swatches for the
    /// picked blip. A click picks the blip in the page's own index box, which the editing cards
    /// below follow.
    /// </summary>
    public class DummyBlipsView : SectionCard
    {
        public const int Max = 56;

        private readonly ComboBox _indexBox;
        private readonly Action _rebuild;
        private readonly Canvas _map = new Canvas { Height = 190, ClipToBounds = true, Background = Brushes.Transparent };
        private readonly StackPanel _rows = new StackPanel();
        private readonly WrapPanel _swatches = new WrapPanel { Margin = new Thickness(0, 2, 0, 8) };
        private readonly Button _add = new Button { Height = 30, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 4, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private List<(int Index, float X, float Y, int Colour, string Name, int Rule, int Team)> _blips = new List<(int, float, float, int, string, int, int)>();
        private string _shown;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        // GTA blip colours (SET_BLIP_COLOUR ids), approximated from the radar; unknown ids show grey.
        private static readonly Dictionary<int, Color> Colours = new Dictionary<int, Color>
        {
            [0] = C(0xFE, 0xFE, 0xFE), [1] = C(0xE0, 0x32, 0x32), [2] = C(0x71, 0xCB, 0x71), [3] = C(0x5D, 0xB6, 0xE5),
            [4] = C(0xFE, 0xFE, 0xFE), [5] = C(0xEE, 0xC6, 0x4E), [6] = C(0xC2, 0x50, 0x50), [7] = C(0x9C, 0x6E, 0xAF),
            [8] = C(0xFE, 0x7A, 0xC3), [9] = C(0xF5, 0x9D, 0x79), [10] = C(0xB1, 0x8F, 0x83), [11] = C(0x8D, 0xCE, 0xA7),
            [12] = C(0x70, 0xA8, 0xAE), [13] = C(0xD3, 0xD1, 0xE7), [14] = C(0x8F, 0x7E, 0x98), [15] = C(0x6A, 0xC4, 0xBF),
            [16] = C(0xD5, 0xC3, 0x98), [17] = C(0xEA, 0x8E, 0x50), [18] = C(0x97, 0xCA, 0xE9), [19] = C(0xB2, 0x62, 0x87),
            [20] = C(0x8F, 0x8D, 0x79), [21] = C(0xA6, 0x75, 0x5E), [22] = C(0xAF, 0xA8, 0xA8), [23] = C(0xE8, 0x8E, 0x9B),
            [24] = C(0xBB, 0xD6, 0x5B), [25] = C(0x0C, 0x7B, 0x56), [26] = C(0x7B, 0xC4, 0xFF), [27] = C(0xAB, 0x3C, 0xE6),
            [28] = C(0xCE, 0xA9, 0x0D), [29] = C(0x47, 0x63, 0xAD), [30] = C(0x2A, 0xA6, 0xB9), [31] = C(0xBA, 0x9D, 0x7D),
            [38] = C(0x2C, 0x6D, 0xB8), [40] = C(0x50, 0x50, 0x50), [46] = C(0xEC, 0xF0, 0x29), [47] = C(0xFF, 0x9A, 0x18),
            [48] = C(0xF6, 0x44, 0xA5), [49] = C(0xE0, 0x3A, 0x3A), [52] = C(0x6A, 0xC4, 0xBF), [57] = C(0x00, 0x9F, 0xFF),
            [59] = C(0xE6, 0x15, 0x15), [69] = C(0x67, 0xB2, 0x4B), [83] = C(0x9C, 0x6E, 0xAF), [84] = C(0x3E, 0x99, 0xEA),
        };

        // Swatches offered for the picked blip.
        private static readonly int[] Offered = { 0, 1, 2, 3, 5, 7, 8, 17, 15, 25, 27, 29, 40, 47, 46, 69 };

        private static Color C(byte r, byte g, byte b) => Color.FromRgb(r, g, b);
        public static Color ColourOf(int id) => Colours.TryGetValue(id, out var c) ? c : C(0x99, 0x99, 0x99);

        public DummyBlipsView(ComboBox indexBox, Action rebuild)
        {
            _indexBox = indexBox;
            _rebuild = rebuild;
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            Icon = Geometry.Parse("M12,2 C8,2 5,5 5,9 C5,14 12,22 12,22 C12,22 19,14 19,9 C19,5 16,2 12,2 M12,6.5 A2.5,2.5 0 1 0 12,11.5 A2.5,2.5 0 1 0 12,6.5");
            Title = T("db_title", "Free blips");
            Margin = new Thickness(0, 0, 0, 12);
            var body = new StackPanel();
            var frame = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Child = _map, Margin = new Thickness(0, 0, 0, 10),
                ToolTip = T("db_map_hint", "Every free blip from above in its colour, north up. Click a dot to pick that blip.") };
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            body.Children.Add(frame);
            body.Children.Add(_rows);
            var colourLabel = new TextBlock { Text = T("db_colour", "Colour of the picked blip"), Margin = new Thickness(0, 8, 0, 4) };
            colourLabel.SetResourceReference(StyleProperty, "FieldLabel");
            body.Children.Add(colourLabel);
            body.Children.Add(_swatches);
            _add.Content = T("db_add", "+ Blip at cursor");
            _add.ToolTip = T("db_add_hint", "Adds a blip at the creator's cursor and rebuilds so the creator shows it.");
            _add.SetResourceReference(StyleProperty, "FormButtonPrimary");
            _add.Click += (_, __) => AddAtCursor();
            body.Children.Add(_add);
            Content = body;
            _map.SizeChanged += (_, __) => DrawMap();
            SatelliteTiles.TileLoaded += () => Dispatcher.BeginInvoke(new Action(() => { if (IsVisible) DrawMap(); }), DispatcherPriority.Background);
            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Refresh(false); };
            _indexBox.SelectionChanged += (_, __) => Refresh(true);
        }

        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.ddblip.number != 0 && GTA.Offsets.Editor.ddblip.NEXT != 0 && GTA.Offsets.Editor.ddblip.pos != 0;
        private static long At(long field, int i) => field + i * GTA.Offsets.Editor.ddblip.NEXT;

        private void Refresh(bool force)
        {
            if (!Live)
            {
                _rows.Children.Clear();
                _map.Children.Clear();
                _swatches.Children.Clear();
                _shown = null;
                _add.Visibility = Visibility.Collapsed;
                _rows.Children.Add(Faint(T("db_nocreator", "Open a job in the creator to see its blips."), 13));
                return;
            }
            int n = Math.Max(0, Math.Min(new Global(GTA.Offsets.Editor.ddblip.number).Get<int>(), Max));
            var blips = Enumerable.Range(0, n).Select(i => (
                Index: i,
                X: new Global(At(GTA.Offsets.Editor.ddblip.pos, i)).Get<float>(),
                Y: new Global(At(GTA.Offsets.Editor.ddblip.pos, i) + 1).Get<float>(),
                Colour: GTA.Offsets.Editor.ddblip.clr == 0 ? 0 : new Global(At(GTA.Offsets.Editor.ddblip.clr, i)).Get<int>(),
                Name: GTA.Offsets.Editor.ddblip.dbnm == 0 ? "" : new Global(At(GTA.Offsets.Editor.ddblip.dbnm, i)).GetString(),
                Rule: GTA.Offsets.Editor.ddblip.rule == 0 ? -1 : new Global(At(GTA.Offsets.Editor.ddblip.rule, i)).Get<int>(),
                Team: GTA.Offsets.Editor.ddblip.team == 0 ? -1 : new Global(At(GTA.Offsets.Editor.ddblip.team, i)).Get<int>())).ToList();
            string key = string.Join(";", blips) + "|" + _indexBox.SelectedIndex;
            if (!force && key == _shown)
                return;
            _shown = key;
            _blips = blips;
            Summary = string.Format(CultureInfo.CurrentCulture, "{0} / {1}", n, Max);
            _add.Visibility = Visibility.Visible;
            _add.IsEnabled = n < Max;

            _rows.Children.Clear();
            if (n == 0)
                _rows.Children.Add(Faint(T("db_none", "No free blip yet."), 13));
            foreach (var b in blips)
                _rows.Children.Add(Row(b.Index, b.Colour, b.Name, b.Rule, b.Team, b.X, b.Y, b.Index == _indexBox.SelectedIndex));
            Swatches();
            DrawMap();
        }

        private FrameworkElement Row(int index, int colour, string name, int rule, int team, float x, float y, bool selected)
        {
            var button = new ToggleButton { IsChecked = selected, HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(0), Margin = new Thickness(0, 0, 0, 6), Cursor = System.Windows.Input.Cursors.Hand };
            button.SetResourceReference(StyleProperty, "ChoiceTile");
            var grid = new Grid { Margin = new Thickness(10, 6, 10, 6) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var dot = new Ellipse { Width = 14, Height = 14, Fill = new SolidColorBrush(ColourOf(colour)), Stroke = Brushes.Black, StrokeThickness = 0.8, VerticalAlignment = VerticalAlignment.Center };
            grid.Children.Add(dot);
            var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var title = new TextBlock { FontSize = 13.5, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis,
                Text = string.IsNullOrWhiteSpace(name) ? string.Format(CultureInfo.CurrentCulture, T("db_blip_n", "Blip {0}"), index + 1) : (index + 1) + " · " + name };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            text.Children.Add(title);
            text.Children.Add(Faint(string.Format(CultureInfo.InvariantCulture, "{0:0.0}, {1:0.0}", x, y), 11.5));
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
            var chips = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            if (rule >= 0)
                chips.Children.Add(Chip(T("rl_rule_n", "Rule {0}").Replace("{0}", (rule + 1).ToString(CultureInfo.CurrentCulture))));
            if (team >= 0 && team < 4)
                chips.Children.Add(Chip(T("dash_team", "Team") + " " + (team + 1)));
            Grid.SetColumn(chips, 2);
            grid.Children.Add(chips);
            button.Content = grid;
            button.Click += (_, __) => Pick(index);
            return button;
        }

        private static FrameworkElement Chip(string text)
        {
            var b = new Border { CornerRadius = new CornerRadius(9), BorderThickness = new Thickness(1), Padding = new Thickness(7, 1, 7, 2), Margin = new Thickness(6, 0, 0, 0) };
            b.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            var t = new TextBlock { Text = text, FontSize = 11.5, FontWeight = FontWeights.SemiBold };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            b.Child = t;
            return b;
        }

        private void Pick(int index)
        {
            if (index >= 0 && index < _indexBox.Items.Count)
                _indexBox.SelectedIndex = index;
            Refresh(true);
        }

        private void Swatches()
        {
            _swatches.Children.Clear();
            int index = _indexBox.SelectedIndex;
            bool live = Live && GTA.Offsets.Editor.ddblip.clr != 0 && index >= 0 && index < _blips.Count;
            int current = live ? _blips[index].Colour : -1;
            foreach (int id in Offered)
            {
                var swatch = new Border { Width = 24, Height = 24, CornerRadius = new CornerRadius(5), Margin = new Thickness(0, 0, 6, 6), Background = new SolidColorBrush(ColourOf(id)),
                    BorderThickness = new Thickness(2), Cursor = live ? System.Windows.Input.Cursors.Hand : null, ToolTip = id.ToString(CultureInfo.InvariantCulture), Opacity = live ? 1 : 0.4 };
                if (id == current)
                    swatch.SetResourceReference(Border.BorderBrushProperty, "TextColor");
                else
                    swatch.BorderBrush = Brushes.Transparent;
                int colour = id;
                swatch.MouseLeftButtonUp += (_, __) =>
                {
                    if (!live) return;
                    new Global(At(GTA.Offsets.Editor.ddblip.clr, index)).SetInt(colour);
                    MainWindow.Instance?.GetBlips(true);
                    Refresh(true);
                };
                _swatches.Children.Add(swatch);
            }
        }

        private void AddAtCursor()
        {
            if (!Live)
                return;
            int n = new Global(GTA.Offsets.Editor.ddblip.number).Get<int>();
            if (n < 0 || n >= Max)
                return;
            var at = Functions.Read.getlocation();
            if (at == null || at.Count < 3
                || !float.TryParse(at[0], NumberStyles.Float, CultureInfo.CurrentCulture, out float x)
                || !float.TryParse(at[1], NumberStyles.Float, CultureInfo.CurrentCulture, out float y)
                || !float.TryParse(at[2], NumberStyles.Float, CultureInfo.CurrentCulture, out float z)
                || (x == 0 && y == 0 && z == 0))
                return;
            new Global(At(GTA.Offsets.Editor.ddblip.pos, n)).SetFloat(x);
            new Global(At(GTA.Offsets.Editor.ddblip.pos, n) + 1).SetFloat(y);
            new Global(At(GTA.Offsets.Editor.ddblip.pos, n) + 2).SetFloat(z);
            new Global(GTA.Offsets.Editor.ddblip.number).SetInt(n + 1);
            _rebuild?.Invoke();
            // The index box only grows with the next worker pass.
            Dispatcher.BeginInvoke(new Action(() => Pick(n)), DispatcherPriority.ApplicationIdle);
            Refresh(true);
        }

        // North up, one scale for both axes, at least 100 m across; the Pleb Masters map under it.
        private void DrawMap()
        {
            _map.Children.Clear();
            double w = _map.ActualWidth, h = _map.ActualHeight;
            if (w < 20 || h < 20 || _blips.Count == 0)
                return;
            const double pad = 20;
            double minX = _blips.Min(b => b.X), maxX = _blips.Max(b => b.X), minY = _blips.Min(b => b.Y), maxY = _blips.Max(b => b.Y);
            double scale = Math.Min((w - 2 * pad) / Math.Max(100, maxX - minX), (h - 2 * pad) / Math.Max(100, maxY - minY));
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            SatelliteTiles.Draw(_map, w, h, cx, cy, scale);
            foreach (var b in _blips.OrderBy(b => b.Index == _indexBox.SelectedIndex))
            {
                bool picked = b.Index == _indexBox.SelectedIndex;
                double size = picked ? 16 : 12;
                var dot = new Ellipse { Width = size, Height = size, Fill = new SolidColorBrush(ColourOf(b.Colour)), Stroke = picked ? Brushes.White : Brushes.Black, StrokeThickness = picked ? 2 : 1,
                    Cursor = System.Windows.Input.Cursors.Hand, ToolTip = string.IsNullOrWhiteSpace(b.Name) ? string.Format(CultureInfo.CurrentCulture, T("db_blip_n", "Blip {0}"), b.Index + 1) : b.Name };
                Canvas.SetLeft(dot, w / 2 + (b.X - cx) * scale - size / 2);
                Canvas.SetTop(dot, h / 2 - (b.Y - cy) * scale - size / 2);
                int index = b.Index;
                dot.MouseLeftButtonUp += (_, __) => Pick(index);
                _map.Children.Add(dot);
            }
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }
    }
}
