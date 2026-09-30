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

namespace Xenvious
{
    /// <summary>One goto point as the route shows it.</summary>
    public struct GotoPoint
    {
        public float X, Y, Z;
        public int WaitMs, Bits;
        public bool IsSet => !float.IsNaN(X) && !float.IsNaN(Y) && (X != 0 || Y != 0 || Z != 0);
    }

    /// <summary>
    /// An actor's goto points as a route (mockup https://claude.ai/artifact/VifG8TGRs71syLT1EsWBmP):
    /// a top view (start = the actor), the points as numbered steps with what happens there, the
    /// selected point's fields right under its step, and the route-wide options at the bottom. The creator counts
    /// the points itself (set = position not 0,0,0, from point 1 on), so points stay contiguous:
    /// deleting one moves the ones after it up.
    /// Point bits (f_9[j][0], menu labels FMMC_AOGT_*): 0 exit vehicle, 1 cover only,
    /// 2 combat goto, 5 slow at destination, 14 play idle while waiting.
    /// </summary>
    public class GotoRouteView : StackPanel
    {
        public const int Points = 12;

        public static readonly (int Bit, string Key, string Fallback)[] Flags =
        {
            (2, "gr_f_combat", "Combat goto"),
            (1, "gr_f_cover", "Cover only"),
            (0, "gr_f_exit", "Exit vehicle"),
            (5, "gr_f_slow", "Slow at destination"),
            (14, "gr_f_idle", "Idle while waiting"),
        };

        /// <summary>After each refresh: how many points are set.</summary>
        public event Action<int> Changed;

        private readonly ComboBox _pointBox;
        private readonly Func<bool> _live;
        private readonly Func<int, GotoPoint> _read;
        private readonly Func<(float X, float Y)> _start;
        private readonly Action<int> _toCursor;
        private readonly Action<int, int> _swap;
        private readonly Action<int, int, bool> _setBit;
        private readonly FrameworkElement _details;
        private readonly Canvas _map = new Canvas { Height = 190, ClipToBounds = true };
        private readonly StackPanel _steps = new StackPanel();
        private readonly TextBlock _empty = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 8) };
        private readonly WrapPanel _flags = new WrapPanel { Margin = new Thickness(0, 2, 0, 8) };
        private readonly Button _reverse, _delete;
        private bool _flagSync;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public GotoRouteView(ComboBox pointBox, Func<bool> live, Func<int, GotoPoint> read, Func<(float X, float Y)> start,
            Action<int> toCursor, Action<int, int> swap, Action<int, int, bool> setBit, FrameworkElement routeOptions, FrameworkElement details)
        {
            _pointBox = pointBox;
            _live = live;
            _read = read;
            _start = start;
            _toCursor = toCursor;
            _swap = swap;
            _setBit = setBit;

            var frame = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Child = _map, Margin = new Thickness(0, 0, 0, 12) };
            frame.Background = new RadialGradientBrush(Color.FromRgb(0x2C, 0x3A, 0x33), Color.FromRgb(0x23, 0x25, 0x29)) { Center = new Point(0.3, 0.4), GradientOrigin = new Point(0.3, 0.4), RadiusX = 0.8, RadiusY = 0.8 };
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            Children.Add(frame);
            _empty.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _empty.Text = T("gr_empty", "No goto points yet. Put the cursor where the actor should go and add a point.");
            Children.Add(_empty);
            Children.Add(_steps);

            // The selected point's fields: flags as chips, then the page's own boxes.
            var detailPanel = new StackPanel { Margin = new Thickness(38, 4, 0, 8) };
            foreach (var (bit, key, fallback) in Flags)
            {
                var chip = new CheckBox { Content = T(key, fallback), Tag = bit, Margin = new Thickness(0, 0, 6, 6) };
                chip.SetResourceReference(StyleProperty, "ChipToggle");
                int b = bit;
                chip.Click += (_, __) =>
                {
                    if (_flagSync || _pointBox.SelectedIndex < 0) return;
                    _setBit(_pointBox.SelectedIndex, b, chip.IsChecked == true);
                    Refresh();
                };
                _flags.Children.Add(chip);
            }
            detailPanel.Children.Add(_flags);
            if (details != null)
                detailPanel.Children.Add(details);
            _details = detailPanel;

            var buttons = new WrapPanel { Margin = new Thickness(0, 8, 0, 6) };
            var add = Btn(T("gr_add", "+ Point at cursor"), "FormButtonPrimary");
            add.Click += (_, __) =>
            {
                int free = Enumerable.Range(0, Points).FirstOrDefault(i => !_read(i).IsSet);
                if (!_live() || _read(free).IsSet)
                    return;
                _pointBox.SelectedIndex = free;
                _toCursor(free);
                Refresh();
            };
            _reverse = Btn(T("gr_reverse", "Reverse"), "FormButton");
            _reverse.Click += (_, __) =>
            {
                int n = Count();
                for (int i = 0; i < n / 2; i++)
                    _swap(i, n - 1 - i);
                Refresh();
            };
            _delete = Btn(T("gr_delete", "Delete point"), "FormButton");
            _delete.Click += (_, __) =>
            {
                int sel = _pointBox.SelectedIndex, n = Count();
                if (sel < 0 || sel >= n)
                    return;
                // Move the later points up, then the last one (now a copy) becomes the empty slot.
                for (int i = sel; i < n - 1; i++)
                    _swap(i, i + 1);
                DeleteLast?.Invoke(n - 1);
                _pointBox.SelectedIndex = Math.Max(0, Math.Min(sel, n - 2));
                Refresh();
            };
            buttons.Children.Add(add);
            buttons.Children.Add(_reverse);
            buttons.Children.Add(_delete);
            Children.Add(buttons);

            var hint = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Text = T("gr_hint", "Click a step to edit it right below. ⌖ moves the point to the cursor.") };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            Children.Add(hint);

            // Route-wide settings (loop, vehicle speed, hover/rappel) below the points.
            if (routeOptions != null)
            {
                var line = new Rectangle { Height = 1, Margin = new Thickness(0, 12, 0, 12) };
                line.SetResourceReference(Shape.FillProperty, "LineBrush");
                Children.Add(line);
                Children.Add(routeOptions);
            }

            _map.SizeChanged += (_, __) => Draw();
            _pointBox.SelectionChanged += (_, __) => Refresh();
        }

        /// <summary>Clears a point's position so the creator no longer counts it.</summary>
        public Action<int> DeleteLast { get; set; }

        private static Button Btn(string text, string style)
        {
            var b = new Button { Content = text, Height = 30, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 6, 6) };
            b.SetResourceReference(StyleProperty, style);
            return b;
        }

        private int Count()
        {
            if (!_live()) return 0;
            int n = 0;
            while (n < Points && _read(n).IsSet) n++;
            return n;
        }

        /// <summary>Rereads the points (after the page loaded another actor or moved a point).</summary>
        public void Refresh()
        {
            if (_details.Parent is Panel p)
                p.Children.Remove(_details);
            _steps.Children.Clear();
            int n = Count();
            _empty.Visibility = n == 0 ? Visibility.Visible : Visibility.Collapsed;
            _reverse.IsEnabled = n > 1;
            _delete.IsEnabled = n > 0 && _pointBox.SelectedIndex >= 0 && _pointBox.SelectedIndex < n;
            for (int i = 0; i < n; i++)
            {
                var point = _read(i);
                _steps.Children.Add(Step(i, point, i < n - 1));
                if (i == _pointBox.SelectedIndex)
                {
                    ShowFlags(point.Bits);
                    _steps.Children.Add(_details);
                }
            }
            Changed?.Invoke(n);
            Draw();
        }

        private void ShowFlags(int bits)
        {
            _flagSync = true;
            foreach (CheckBox chip in _flags.Children)
                chip.IsChecked = (bits & (1 << (int)chip.Tag)) != 0;
            _flagSync = false;
        }

        private string Subtitle(GotoPoint p)
        {
            var parts = new List<string>();
            parts.Add(p.WaitMs > 0 ? string.Format(CultureInfo.CurrentCulture, T("gr_waits", "waits {0} s"), Math.Round(p.WaitMs / 1000.0, 1)) : T("gr_nowait", "no wait"));
            foreach (var (bit, key, fallback) in Flags)
                if ((p.Bits & (1 << bit)) != 0)
                    parts.Add(T(key, fallback));
            return string.Join(" · ", parts);
        }

        private FrameworkElement Step(int index, GotoPoint p, bool more)
        {
            bool current = _pointBox.SelectedIndex == index;
            var grid = new Grid { Cursor = Cursors.Hand, Background = Brushes.Transparent, MinHeight = 48 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (more)
            {
                var line = new Rectangle { Width = 2, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(13, 30, 0, 0) };
                line.SetResourceReference(Shape.FillProperty, "LineBrush");
                grid.Children.Add(line);
            }
            var dot = new Border { Width = 28, Height = 28, CornerRadius = new CornerRadius(14), BorderThickness = new Thickness(2), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 2, 0, 0) };
            dot.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
            dot.Background = current ? (Brush)MainWindow.ThemeBrush("AccentBrush") : new SolidColorBrush(Color.FromArgb(0x2E, 0x5B, 0x9B, 0xE6));
            var num = new TextBlock { Text = (index + 1).ToString(CultureInfo.CurrentCulture), FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (current) num.Foreground = Brushes.White; else num.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            dot.Child = num;
            grid.Children.Add(dot);

            var text = new StackPanel { Margin = new Thickness(0, 2, 8, 8) };
            var title = new TextBlock { Text = T("gr_point", "Point") + " " + (index + 1), FontSize = 14, FontWeight = FontWeights.Bold };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            var sub = new TextBlock { Text = Subtitle(p), FontSize = 12, TextWrapping = TextWrapping.Wrap };
            sub.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            text.Children.Add(title);
            text.Children.Add(sub);
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            // The same cross-hair button as the page's "get cursor location".
            var cross = new Path { Data = Geometry.Parse("M8,1.5 L8,4.5 M8,11.5 L8,14.5 M1.5,8 L4.5,8 M11.5,8 L14.5,8 M8,5 A3,3 0 1 1 7.99,5"), StrokeThickness = 1.5, Width = 15, Height = 15, Stretch = Stretch.Uniform };
            cross.SetResourceReference(Shape.StrokeProperty, "TextColor");
            var here = new Button { Content = cross, Width = 30, Height = 30, VerticalAlignment = VerticalAlignment.Top, ToolTip = T("gr_tocursor", "Move this point to the cursor") };
            here.SetResourceReference(StyleProperty, "FieldIconButton");
            here.Click += (_, e) =>
            {
                // The page moves the selected point, so select it for the move, then give the
                // previous selection back: moving must not open or close any step.
                int before = _pointBox.SelectedIndex;
                _toCursor(index);
                if (_pointBox.SelectedIndex != before)
                    _pointBox.SelectedIndex = before;
                Refresh();
                e.Handled = true;
            };
            Grid.SetColumn(here, 2);
            grid.Children.Add(here);

            // Open / close the point's fields (clicking the step does the same).
            var chevron = new Path { Data = Geometry.Parse(current ? "M2,5 L8,11 L14,5" : "M5,2 L11,8 L5,14"), StrokeThickness = 1.8, Width = 12, Height = 12, Stretch = Stretch.Uniform,
                StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            chevron.SetResourceReference(Shape.StrokeProperty, "TextColor");
            var open = new Button { Content = chevron, Width = 30, Height = 30, Margin = new Thickness(6, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top,
                ToolTip = current ? T("gr_close", "Close") : T("gr_open", "Edit this point") };
            open.SetResourceReference(StyleProperty, "FieldIconButton");
            open.Click += (_, e) => { _pointBox.SelectedIndex = current ? -1 : index; e.Handled = true; };
            Grid.SetColumn(open, 3);
            grid.Children.Add(open);

            grid.MouseLeftButtonUp += (_, __) => _pointBox.SelectedIndex = current ? -1 : index;
            return grid;
        }

        private void Draw()
        {
            _map.Children.Clear();
            double w = _map.ActualWidth, h = _map.ActualHeight;
            if (w <= 0 || !_live())
                return;
            var start = _start();
            var pts = new List<(int Index, float X, float Y)> { (-1, start.X, start.Y) };
            int n = Count();
            pts.AddRange(Enumerable.Range(0, n).Select(i => { var p = _read(i); return (i, p.X, p.Y); }));
            double minX = pts.Min(p => p.X), maxX = pts.Max(p => p.X), minY = pts.Min(p => p.Y), maxY = pts.Max(p => p.Y);
            double scale = Math.Min((w - 40) / Math.Max(maxX - minX, 10), (h - 40) / Math.Max(maxY - minY, 10));
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            Point P(float x, float y) => new Point(w / 2 + (x - cx) * scale, h / 2 - (y - cy) * scale);

            var line = new Polyline { StrokeThickness = 2.5, StrokeDashArray = new DoubleCollection { 3, 2.5 } };
            line.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
            foreach (var p in pts) line.Points.Add(P(p.X, p.Y));
            _map.Children.Add(line);
            foreach (var p in pts)
            {
                var at = P(p.X, p.Y);
                bool current = p.Index >= 0 && p.Index == _pointBox.SelectedIndex;
                double size = p.Index < 0 ? 14 : 20;
                var dot = new Grid { Width = size, Height = size, ToolTip = p.Index < 0 ? T("gr_start", "Actor") : T("gr_point", "Point") + " " + (p.Index + 1) };
                var circle = new Ellipse();
                circle.SetResourceReference(Shape.FillProperty, p.Index < 0 ? "OkBrush" : "AccentBrush");
                if (current) { circle.StrokeThickness = 2; circle.Stroke = Brushes.White; }
                dot.Children.Add(circle);
                if (p.Index >= 0)
                {
                    dot.Children.Add(new TextBlock { Text = (p.Index + 1).ToString(CultureInfo.CurrentCulture), FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
                    int index = p.Index;
                    dot.Cursor = Cursors.Hand;
                    dot.MouseLeftButtonUp += (_, __) => _pointBox.SelectedIndex = index;
                }
                Canvas.SetLeft(dot, at.X - size / 2);
                Canvas.SetTop(dot, at.Y - size / 2);
                _map.Children.Add(dot);
            }
        }
    }
}
