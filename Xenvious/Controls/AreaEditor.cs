using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xenvious
{
    /// <summary>Shape of a two-point area, same numbers as ciFMMC_ZONE_SHAPE__ (zones).</summary>
    public enum AreaShape
    {
        AxisBox = 0,
        Sphere = 1,
        AngledBox = 2,
        Cylinder = 3
    }

    /// <summary>
    /// Height, width and preview for a two-point area (zones, play area, gang chase area).
    /// The page keeps its start / end / width text boxes and attaches them; this panel only
    /// writes into those boxes, so the page's own TextChanged handlers still write the game.
    ///
    /// Why: both points come from the cursor, which sits on the ground, so start and end get
    /// about the same Z and the area is flat. Here the start goes to the lower ground (minus
    /// 1 m if wanted, so players on slopes stay inside) and the end to ground + height.
    /// What the game reads (IS_POINT_IN_ZONE, DRAW_FMMC_ZONE):
    ///   axis box:   vPos[0] is the min corner, vPos[1] the max corner.
    ///   angled box: height = Z difference of the points, width = fRadius.
    ///   cylinder:   centre vPos[0], vPos[1] = vPos[0] + height, fHeight, radius = fRadius.
    ///   sphere:     centre vPos[0], radius = fRadius; no height.
    /// </summary>
    public class AreaEditor : StackPanel
    {
        private TextBox _sx, _sy, _sz, _ex, _ey, _ez, _width, _height;
        private AreaShape _shape = AreaShape.AngledBox;

        // Z of the floor of the area. Taken from the points when an area is loaded and from the
        // ground when a point is picked.
        private float _base;
        private bool _sync;
        private bool _built;

        // The sliders have no fixed end: their range follows the value (see Rescale), and the box
        // next to each takes any number.
        private readonly Slider _widthSlider = new Slider { Minimum = 0, Maximum = 50, VerticalAlignment = VerticalAlignment.Center };
        private readonly Slider _heightSlider = new Slider { Minimum = 0, Maximum = 50, VerticalAlignment = VerticalAlignment.Center };
        private readonly TextBox _widthValue = new TextBox { Width = 76, Height = 30, Margin = new Thickness(10, 0, 0, 0) };
        private readonly TextBox _heightValue = new TextBox { Width = 76, Height = 30, Margin = new Thickness(10, 0, 0, 0) };
        private readonly CheckBox _below = new CheckBox { IsChecked = true };
        private FrameworkElement _belowRow;
        private float _widthM, _heightM;
        private readonly TextBlock _hint = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 12, Margin = new Thickness(0, 0, 0, 10) };
        private readonly TextBlock _status = new TextBlock { FontSize = 12, Margin = new Thickness(0, 6, 0, 0), TextWrapping = TextWrapping.Wrap };
        private readonly Canvas _top = new Canvas { Height = 130, ClipToBounds = true };
        private readonly Canvas _side = new Canvas { Height = 130, ClipToBounds = true };
        private FrameworkElement _widthRow, _heightRow;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public AreaEditor()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) => { if (IsVisible) Refresh(); };
        }

        /// <summary>The page's boxes. <paramref name="height"/> is the separate height field (zones: fHeight), or null.</summary>
        public void Attach(TextBox sx, TextBox sy, TextBox sz, TextBox ex, TextBox ey, TextBox ez, TextBox width, TextBox height = null)
        {
            _sx = sx; _sy = sy; _sz = sz; _ex = ex; _ey = ey; _ez = ez; _width = width; _height = height;
            foreach (var box in new[] { sx, sy, sz, ex, ey, ez, width })
                box.TextChanged += (_, __) => { if (!_sync) Refresh(); };
            HidePageField(width);
        }

        // The width row here replaces the page's own width field (same value, one place to edit).
        private static void HidePageField(TextBox box)
        {
            if (box.Parent is Panel panel)
            {
                int at = panel.Children.IndexOf(box);
                if (panel.Children.Count == 2 && at == 1 && panel.Children[0] is TextBlock)
                {
                    panel.Visibility = Visibility.Collapsed;
                    return;
                }
                if (at > 0 && panel.Children[at - 1] is TextBlock label)
                    label.Visibility = Visibility.Collapsed;
            }
            box.Visibility = Visibility.Collapsed;
        }

        public AreaShape Kind
        {
            get => _shape;
            set { _shape = value; Refresh(); }
        }

        // Ground Z under each point, remembered with the X/Y it was picked at. A point whose X/Y
        // changed since (other area selected, typed in) has no known ground any more.
        private Vec? _startGround, _endGround;

        /// <summary>Call after the start point was taken from the cursor.</summary>
        public void StartPicked()
        {
            if (Read(out var s, out _, out _))
                _startGround = s;
            PointPicked();
        }

        /// <summary>Call after the end point was taken from the cursor.</summary>
        public void EndPicked()
        {
            if (Read(out _, out var e, out _))
                _endGround = e;
            PointPicked();
        }

        // The cursor sits on the ground, so a picked point's Z is the ground there; the lower
        // ground of the two is the floor.
        private void PointPicked()
        {
            if (!Read(out var s, out var e, out _))
                return;
            float? a = Ground(_startGround, s), b = Ground(_endGround, e);
            if (a == null && b == null)
                return;
            _base = Math.Min(a ?? b.Value, b ?? a.Value) - (_below.IsChecked == true ? 1f : 0f);
            Apply();
        }

        private static float? Ground(Vec? picked, Vec now)
        {
            if (picked == null || Math.Abs(picked.Value.X - now.X) > 0.01f || Math.Abs(picked.Value.Y - now.Y) > 0.01f)
                return null;
            return picked.Value.Z;
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            _hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _status.SetResourceReference(TextBlock.ForegroundProperty, "OkBrush");
            _hint.Text = T("area_hint", "Take both points with the cursor on the ground. Xenvious puts the start on the lower ground and the end at ground + height.");
            _belowRow = ToggleRow(T("area_below", "Reach 1 m below the ground (players on slopes stay inside)"), _below);

            _widthRow = SliderRow(T("width", "Width"), _widthSlider, _widthValue, v => SetWidth(v));
            _heightRow = SliderRow(T("area_height", "Height above the ground"), _heightSlider, _heightValue, v => { _heightM = v; Apply(); });
            _below.Click += (_, __) =>
            {
                _base += _below.IsChecked == true ? -1f : 1f;
                Apply();
            };

            Children.Add(_hint);
            Children.Add(_widthRow);
            Children.Add(_heightRow);
            Children.Add(_belowRow);

            var views = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            views.ColumnDefinitions.Add(new ColumnDefinition());
            views.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            views.ColumnDefinitions.Add(new ColumnDefinition());
            var top = View(T("area_top", "Top view"), _top);
            var side = View(T("area_side", "Side view"), _side);
            Grid.SetColumn(side, 2);
            views.Children.Add(top);
            views.Children.Add(side);
            Children.Add(views);
            Children.Add(_status);

            _top.SizeChanged += (_, __) => Draw();
            _side.SizeChanged += (_, __) => Draw();
            Refresh();
        }

        private FrameworkElement SliderRow(string label, Slider slider, TextBox box, Action<float> changed)
        {
            box.Style = (Style)FindResource("Watermark");
            box.Tag = "m";
            slider.ValueChanged += (_, __) =>
            {
                if (_sync)
                    return;
                box.Text = Metres(slider.Value);
                changed((float)slider.Value);
            };
            // Letting go near the end makes room; letting go far below it zooms in.
            slider.AddHandler(System.Windows.Controls.Primitives.Thumb.DragCompletedEvent,
                new System.Windows.Controls.Primitives.DragCompletedEventHandler((_, __) => Rescale(slider, slider.Value)));
            void Commit()
            {
                if (!TryParse(box.Text, out float v) || v < 0)
                    return;
                Show(slider, box, v);
                changed(v);
            }
            box.LostFocus += (_, __) => Commit();
            box.KeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Enter) Commit(); };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = label });
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            DockPanel.SetDock(box, Dock.Right);
            row.Children.Add(box);
            row.Children.Add(slider);
            panel.Children.Add(row);
            return panel;
        }

        private FrameworkElement ToggleRow(string label, CheckBox box)
        {
            // No fixed row height: the label wraps instead of being cut off.
            var row = new Grid { Margin = new Thickness(0, 2, 0, 10) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new TextBlock { Style = (Style)FindResource("FormLabel"), Text = label, TextWrapping = TextWrapping.Wrap, TextTrimming = TextTrimming.None, Margin = new Thickness(0, 0, 10, 0) });
            box.Style = (Style)FindResource("FormToggle");
            Grid.SetColumn(box, 1);
            row.Children.Add(box);
            return row;
        }

        // Slider and box show v without writing anything.
        private void Show(Slider slider, TextBox box, float v)
        {
            _sync = true;
            Rescale(slider, v);
            slider.Value = v;
            if (!box.IsKeyboardFocused)
                box.Text = Metres(v);
            _sync = false;
        }

        // Range steps 10, 25, 50, 100, 250 ... so the value sits in the lower two thirds: a 3 m
        // area gets a fine 0-10 slider, a 700 m one 0-2500. Only grows when the value is near the
        // end and only shrinks when it is far below, so the thumb does not jump while dragging.
        private void Rescale(Slider slider, double v)
        {
            double max = slider.Maximum;
            if (v <= max * 0.9 && v >= max * 0.15)
                return;
            bool sync = _sync;
            _sync = true;
            slider.Maximum = NiceMax(v);
            slider.Value = v;
            _sync = sync;
        }

        private static double NiceMax(double v)
        {
            double need = Math.Max(v * 1.5, 10);
            for (double step = 10; ; step *= 10)
            {
                if (need <= step) return step;
                if (need <= step * 2.5) return step * 2.5;
                if (need <= step * 5) return step * 5;
            }
        }

        private void SetWidth(float v)
        {
            _widthM = v;
            if (_width == null)
                return;
            _sync = true;
            _width.Text = Format(v);
            _sync = false;
            Draw();
        }

        private FrameworkElement View(string label, Canvas canvas)
        {
            var panel = new StackPanel();
            var caption = new TextBlock { Text = label, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) };
            caption.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            var frame = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Child = canvas };
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            panel.Children.Add(caption);
            panel.Children.Add(frame);
            return panel;
        }

        /// <summary>Takes floor, height and width from the boxes (a new area was selected or loaded).</summary>
        public void Refresh()
        {
            if (!_built)
                return;
            bool usesHeight = _shape != AreaShape.Sphere;
            _heightRow.Visibility = usesHeight ? Visibility.Visible : Visibility.Collapsed;
            _belowRow.Visibility = usesHeight ? Visibility.Visible : Visibility.Collapsed;
            _widthRow.Visibility = _shape == AreaShape.AxisBox ? Visibility.Collapsed : Visibility.Visible;
            if (_widthRow is Panel widthPanel && widthPanel.Children[0] is TextBlock widthLabel)
                widthLabel.Text = _shape == AreaShape.Sphere || _shape == AreaShape.Cylinder ? T("pa_radius", "Radius") : T("width", "Width");
            if (!Read(out var s, out var e, out float width))
            {
                Draw();
                return;
            }
            _base = Math.Min(s.Z, e.Z);
            float height = Math.Abs(e.Z - s.Z);
            if (_shape == AreaShape.Cylinder && _height != null && TryParse(_height.Text, out float h) && h > 0)
                height = h;
            _heightM = height;
            _widthM = width;
            Show(_heightSlider, _heightValue, height);
            Show(_widthSlider, _widthValue, width);
            Draw();
        }

        // Writes both points from floor + height into the page's boxes.
        private void Apply()
        {
            if (!Read(out var s, out var e, out _))
                return;
            float height = _heightM;
            switch (_shape)
            {
                case AreaShape.AxisBox:
                    // The game tests min < p < max per axis, so order the corners.
                    Set(_sx, Math.Min(s.X, e.X)); Set(_sy, Math.Min(s.Y, e.Y));
                    Set(_ex, Math.Max(s.X, e.X)); Set(_ey, Math.Max(s.Y, e.Y));
                    Set(_sz, _base); Set(_ez, _base + height);
                    break;
                case AreaShape.Cylinder:
                    Set(_ex, s.X); Set(_ey, s.Y);
                    Set(_sz, _base); Set(_ez, _base + height);
                    if (_height != null)
                        Set(_height, height);
                    break;
                case AreaShape.AngledBox:
                    Set(_sz, _base); Set(_ez, _base + height);
                    break;
            }
            Draw();
        }

        private void Set(TextBox box, float value)
        {
            _sync = true;
            box.Text = Format(value);
            _sync = false;
        }

        private bool Read(out Vec s, out Vec e, out float width)
        {
            s = default; e = default; width = 0;
            if (_sx == null)
                return false;
            bool ok = TryParse(_sx.Text, out s.X) & TryParse(_sy.Text, out s.Y) & TryParse(_sz.Text, out s.Z)
                    & TryParse(_ex.Text, out e.X) & TryParse(_ey.Text, out e.Y) & TryParse(_ez.Text, out e.Z);
            TryParse(_width.Text, out width);
            return ok;
        }

        // ----- Preview -----

        private void Draw()
        {
            _top.Children.Clear();
            _side.Children.Clear();
            if (!_built || !Read(out var s, out var e, out float width))
            {
                _status.Text = "";
                return;
            }
            float height = Math.Abs(e.Z - s.Z);
            DrawTop(s, e, width);
            DrawSide(s, e, width, height);

            if (_shape == AreaShape.Sphere)
                _status.Text = string.Format(T("area_ok_sphere", "Sphere, radius {0}"), Metres(width) + " m");
            else if (height < 0.5f)
            {
                _status.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                _status.Text = T("area_flat", "The area is flat: start and end have the same Z. Set a height.");
                return;
            }
            else
                _status.Text = string.Format(T("area_ok", "The area is {0} high"), Metres(height) + " m");
            _status.SetResourceReference(TextBlock.ForegroundProperty, "OkBrush");
        }

        private void DrawTop(Vec s, Vec e, float width)
        {
            double w = _top.ActualWidth, h = _top.ActualHeight;
            if (w <= 0)
                return;
            double cx = (s.X + e.X) / 2, cy = (s.Y + e.Y) / 2;
            double dx = e.X - s.X, dy = e.Y - s.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double r = width;
            double extent;
            switch (_shape)
            {
                case AreaShape.Sphere:
                case AreaShape.Cylinder:
                    extent = r * 2;
                    cx = s.X; cy = s.Y;
                    break;
                case AreaShape.AxisBox:
                    extent = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    break;
                default:
                    extent = Math.Max(len, r);
                    break;
            }
            double scale = Math.Min(w, h) * 0.75 / Math.Max(extent, 1);
            Point P(double x, double y) => new Point(w / 2 + (x - cx) * scale, h / 2 - (y - cy) * scale);

            if (_shape == AreaShape.Sphere || _shape == AreaShape.Cylinder)
            {
                var c = P(s.X, s.Y);
                var ring = new Ellipse { Width = r * 2 * scale, Height = r * 2 * scale };
                Canvas.SetLeft(ring, c.X - r * scale);
                Canvas.SetTop(ring, c.Y - r * scale);
                Fill(ring);
                _top.Children.Add(ring);
                Dot(_top, c, "OkBrush");
                return;
            }

            Point[] corners;
            if (_shape == AreaShape.AxisBox)
                corners = new[] { P(s.X, s.Y), P(e.X, s.Y), P(e.X, e.Y), P(s.X, e.Y) };
            else
            {
                // Angled area: the points are the middle of two opposite sides, width across.
                double nx = len > 0 ? -dy / len : 0, ny = len > 0 ? dx / len : 1;
                double hw = r / 2;
                corners = new[]
                {
                    P(s.X + nx * hw, s.Y + ny * hw), P(e.X + nx * hw, e.Y + ny * hw),
                    P(e.X - nx * hw, e.Y - ny * hw), P(s.X - nx * hw, s.Y - ny * hw)
                };
                var mid = new Line { X1 = P(s.X, s.Y).X, Y1 = P(s.X, s.Y).Y, X2 = P(e.X, e.Y).X, Y2 = P(e.X, e.Y).Y, StrokeDashArray = new DoubleCollection { 4, 3 }, StrokeThickness = 1 };
                mid.SetResourceReference(Shape.StrokeProperty, "MutedTextBrush");
                _top.Children.Add(Outline(corners));
                _top.Children.Add(mid);
                Dot(_top, P(s.X, s.Y), "OkBrush");
                Dot(_top, P(e.X, e.Y), "WarnBrush");
                return;
            }
            _top.Children.Add(Outline(corners));
            Dot(_top, P(s.X, s.Y), "OkBrush");
            Dot(_top, P(e.X, e.Y), "WarnBrush");
        }

        private void DrawSide(Vec s, Vec e, float width, float height)
        {
            double w = _side.ActualWidth, h = _side.ActualHeight;
            if (w <= 0)
                return;
            double ground = h - 18;
            var line = new Line { X1 = 0, X2 = w, Y1 = ground, Y2 = ground, StrokeThickness = 2 };
            line.SetResourceReference(Shape.StrokeProperty, "LineBrush");
            _side.Children.Add(line);

            if (_shape == AreaShape.Sphere)
            {
                double d = Math.Min(w, ground) * 0.7;
                var ball = new Ellipse { Width = d, Height = d };
                Canvas.SetLeft(ball, (w - d) / 2);
                Canvas.SetTop(ball, ground - d / 2 - d / 4);
                Fill(ball);
                _side.Children.Add(ball);
                return;
            }

            double dx = e.X - s.X, dy = e.Y - s.Y;
            double len = _shape == AreaShape.Cylinder ? width * 2 : Math.Sqrt(dx * dx + dy * dy);
            double scale = Math.Min((w - 60) / Math.Max(len, 1), (ground - 12) / Math.Max(height, 1));
            double boxW = Math.Max(len * scale, 4), boxH = height * scale;
            double left = (w - 40 - boxW) / 2;
            // Draw the box from the floor; the -1 m reach shows as the part below the ground line.
            double below = _below.IsChecked == true && _shape != AreaShape.Sphere ? Math.Min(1 * scale, 12) : 0;
            var box = new Rectangle { Width = boxW, Height = Math.Max(boxH, 1) };
            Canvas.SetLeft(box, left);
            Canvas.SetTop(box, ground + below - boxH);
            Fill(box);
            _side.Children.Add(box);
            Dot(_side, new Point(left, ground + below), "OkBrush");
            Dot(_side, new Point(left + boxW, ground + below - boxH), "WarnBrush");

            var label = new TextBlock { Text = Metres(height) + " m", FontSize = 11 };
            label.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            Canvas.SetLeft(label, left + boxW + 6);
            Canvas.SetTop(label, ground + below - boxH / 2 - 8);
            _side.Children.Add(label);
        }

        private static Polygon Outline(Point[] points)
        {
            var poly = new Polygon { Points = new PointCollection(points) };
            Fill(poly);
            return poly;
        }

        private static void Fill(Shape shape)
        {
            shape.StrokeThickness = 2;
            shape.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
            // A see-through accent: the brush is shared, so the tint goes on a copy.
            var accent = MainWindow.ThemeBrush("AccentBrush") as SolidColorBrush;
            if (accent != null)
                shape.Fill = new SolidColorBrush(Color.FromArgb(64, accent.Color.R, accent.Color.G, accent.Color.B));
        }

        private static void Dot(Canvas canvas, Point p, string brush)
        {
            var dot = new Ellipse { Width = 9, Height = 9 };
            dot.SetResourceReference(Shape.FillProperty, brush);
            Canvas.SetLeft(dot, p.X - 4.5);
            Canvas.SetTop(dot, p.Y - 4.5);
            canvas.Children.Add(dot);
        }

        // ----- Numbers -----

        private struct Vec
        {
            public float X, Y, Z;
        }

        private static string Metres(double value) => value.ToString("0.#", CultureInfo.CurrentCulture);

        // The boxes hold UI-culture text (they are filled with float.ToString()), so write the same.
        private static string Format(float value) => Math.Round(value, 3).ToString(CultureInfo.CurrentCulture);

        private static bool TryParse(string text, out float value)
        {
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return true;
            return float.TryParse(text?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
