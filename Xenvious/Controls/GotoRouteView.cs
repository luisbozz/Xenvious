using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xenvious
{
    /// <summary>
    /// An actor's goto points as a route: a top view (start = the actor, then the points in
    /// order) and a list of the set points. Picking a point selects it in the page's point box,
    /// so the page's own fields below show and write it. A point counts as set when X or Y is
    /// not 0 (the creator leaves unused points at 0,0,0).
    /// </summary>
    public class GotoRouteView : StackPanel
    {
        public const int Points = 12;

        /// <summary>After each refresh: how many points are set.</summary>
        public event Action<int> Changed;

        private readonly ComboBox _pointBox;
        private readonly Func<int> _actor;
        private readonly Func<int, (float X, float Y)> _read;
        private readonly Func<(float X, float Y)> _start;
        private readonly Canvas _map = new Canvas { Height = 170, ClipToBounds = true };
        private readonly StackPanel _list = new StackPanel();
        private readonly TextBlock _empty = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 8) };

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public GotoRouteView(ComboBox pointBox, Func<int> actor, Func<int, (float X, float Y)> read, Func<(float X, float Y)> start, Action addAtCursor)
        {
            _pointBox = pointBox;
            _actor = actor;
            _read = read;
            _start = start;

            var frame = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Child = _map, Margin = new Thickness(0, 0, 0, 10) };
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            Children.Add(frame);
            _empty.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _empty.Text = T("gr_empty", "No goto points yet. Put the cursor where the actor should go and add a point.");
            Children.Add(_empty);
            Children.Add(_list);

            var add = new Button { Content = T("gr_add", "+ Point at cursor"), Padding = new Thickness(12, 0, 12, 0), Height = 30, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 6, 0, 12) };
            add.SetResourceReference(StyleProperty, "FormButtonPrimary");
            add.Click += (_, __) =>
            {
                int free = Enumerable.Range(0, Points).FirstOrDefault(i => !IsSet(i));
                if (IsSet(free))
                    return;
                _pointBox.SelectedIndex = free;
                addAtCursor();
                Refresh();
            };
            Children.Add(add);

            _map.SizeChanged += (_, __) => Draw();
            _pointBox.SelectionChanged += (_, __) => Refresh();
        }

        private bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && _actor() >= 0;

        private bool IsSet(int i)
        {
            if (!Live) return false;
            var p = _read(i);
            return !float.IsNaN(p.X) && !float.IsNaN(p.Y) && (p.X != 0 || p.Y != 0);
        }

        /// <summary>Rereads the points (after the page loaded another actor or moved a point).</summary>
        public void Refresh()
        {
            _list.Children.Clear();
            var set = Enumerable.Range(0, Points).Where(IsSet).ToList();
            _empty.Visibility = set.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            Changed?.Invoke(set.Count);
            (float X, float Y)? prev = Live ? _start() : ((float, float)?)null;
            foreach (int i in set)
            {
                var p = _read(i);
                string dist = prev == null ? "" : Math.Round(Math.Sqrt(Math.Pow(p.X - prev.Value.X, 2) + Math.Pow(p.Y - prev.Value.Y, 2))).ToString(CultureInfo.CurrentCulture) + " m";
                prev = p;
                _list.Children.Add(Row(i, string.Format(CultureInfo.CurrentCulture, "{0:0.#}, {1:0.#}", p.X, p.Y), dist));
            }
            Draw();
        }

        private FrameworkElement Row(int index, string where, string dist)
        {
            bool current = _pointBox.SelectedIndex == index;
            var row = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Padding = new Thickness(8, 6, 10, 6), Margin = new Thickness(0, 0, 0, 6), Cursor = Cursors.Hand };
            row.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            row.SetResourceReference(Border.BorderBrushProperty, current ? "AccentBrush" : "LineBrush");
            var dock = new DockPanel();
            var num = new Border { Width = 26, Height = 26, CornerRadius = new CornerRadius(13), BorderThickness = new Thickness(2), Margin = new Thickness(0, 0, 10, 0) };
            num.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
            var n = new TextBlock { Text = (index + 1).ToString(CultureInfo.CurrentCulture), FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            n.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            num.Child = n;
            DockPanel.SetDock(num, Dock.Left);
            dock.Children.Add(num);
            var d = new TextBlock { Text = dist, FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
            d.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            DockPanel.SetDock(d, Dock.Right);
            dock.Children.Add(d);
            var text = new TextBlock { Text = T("gr_point", "Point") + " " + (index + 1) + "  ·  " + where, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
            text.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            dock.Children.Add(text);
            row.Child = dock;
            row.MouseLeftButtonUp += (_, __) => _pointBox.SelectedIndex = index;
            return row;
        }

        private void Draw()
        {
            _map.Children.Clear();
            double w = _map.ActualWidth, h = _map.ActualHeight;
            if (w <= 0 || !Live)
                return;
            var start = _start();
            var pts = new List<(int Index, float X, float Y)> { (-1, start.X, start.Y) };
            pts.AddRange(Enumerable.Range(0, Points).Where(IsSet).Select(i => { var p = _read(i); return (i, p.X, p.Y); }));
            double minX = pts.Min(p => p.X), maxX = pts.Max(p => p.X), minY = pts.Min(p => p.Y), maxY = pts.Max(p => p.Y);
            double scale = Math.Min((w - 30) / Math.Max(maxX - minX, 10), (h - 30) / Math.Max(maxY - minY, 10));
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            Point P(float x, float y) => new Point(w / 2 + (x - cx) * scale, h / 2 - (y - cy) * scale);

            var line = new Polyline { StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 3 } };
            line.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
            foreach (var p in pts) line.Points.Add(P(p.X, p.Y));
            _map.Children.Add(line);
            foreach (var p in pts)
            {
                var at = P(p.X, p.Y);
                bool current = p.Index >= 0 && p.Index == _pointBox.SelectedIndex;
                var dot = new Ellipse { Width = current ? 16 : 12, Height = current ? 16 : 12, ToolTip = p.Index < 0 ? T("gr_start", "Actor") : T("gr_point", "Point") + " " + (p.Index + 1) };
                dot.SetResourceReference(Shape.FillProperty, p.Index < 0 ? "OkBrush" : "AccentBrush");
                if (current) { dot.StrokeThickness = 2; dot.Stroke = Brushes.White; }
                Canvas.SetLeft(dot, at.X - dot.Width / 2);
                Canvas.SetTop(dot, at.Y - dot.Height / 2);
                if (p.Index >= 0)
                {
                    int index = p.Index;
                    dot.Cursor = Cursors.Hand;
                    dot.MouseLeftButtonUp += (_, __) => _pointBox.SelectedIndex = index;
                }
                _map.Children.Add(dot);
            }
        }
    }
}
