using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Xenvious
{
    /// <summary>
    /// Reorders the first rows of a panel by dragging a grip: the picked-up row follows the mouse
    /// with a shadow, the rows it passes slide out of its way, and on release the owner gets
    /// (from, to) to do the actual move and redraw. Used for the rule lists.
    /// </summary>
    public sealed class RowDragger
    {
        private static readonly Duration Slide = new Duration(TimeSpan.FromMilliseconds(160));
        private static readonly IEasingFunction Ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        private readonly Panel _panel;
        private readonly Func<int> _rows;
        private readonly Action<int, int> _drop;

        private FrameworkElement _row;
        private int _from, _to;
        private double _startY, _height;

        /// <summary>Called when a drag starts and ends (e.g. to hide what is drawn over the rows).</summary>
        public event Action<bool> Dragging;

        /// <param name="panel">The panel whose children are the rows.</param>
        /// <param name="rows">How many of its first children can be moved (rows after them stay).</param>
        /// <param name="drop">Called with the old and the new position when a row was moved.</param>
        public RowDragger(Panel panel, Func<int> rows, Action<int, int> drop)
        {
            _panel = panel;
            _rows = rows;
            _drop = drop;
        }

        /// <summary>A six-dot grip for row <paramref name="index"/>; disabled, it is greyed out and says why.</summary>
        public FrameworkElement Grip(int index, bool enabled, string tip)
        {
            var dots = new Canvas { Width = 10, Height = 16 };
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 2; x++)
                {
                    var dot = new Ellipse { Width = 3.4, Height = 3.4 };
                    dot.SetResourceReference(Shape.FillProperty, "MutedTextBrush");
                    Canvas.SetLeft(dot, x * 5.5);
                    Canvas.SetTop(dot, y * 6);
                    dots.Children.Add(dot);
                }
            var grip = new Border
            {
                Width = 26, Height = 30, CornerRadius = new CornerRadius(6), Background = Brushes.Transparent,
                Child = dots, ToolTip = tip, VerticalAlignment = VerticalAlignment.Center,
                Cursor = enabled ? Cursors.SizeNS : Cursors.Arrow, Opacity = enabled ? 1 : 0.35,
            };
            ToolTipService.SetShowOnDisabled(grip, true);
            dots.HorizontalAlignment = HorizontalAlignment.Center;
            dots.VerticalAlignment = VerticalAlignment.Center;
            if (!enabled)
                return grip;
            grip.MouseEnter += (_, __) => grip.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            grip.MouseLeave += (_, __) => { if (_row == null) grip.Background = Brushes.Transparent; };
            grip.MouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true;   // the row's own click selects it
                Start(index, e.GetPosition(_panel).Y);
                grip.CaptureMouse();
            };
            grip.MouseMove += (s, e) => { if (_row != null) Move(e.GetPosition(_panel).Y); };
            grip.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true;
                grip.ReleaseMouseCapture();
                End();
            };
            grip.LostMouseCapture += (_, __) => { if (_row != null) End(); };
            return grip;
        }

        private static TranslateTransform Shift(FrameworkElement row)
        {
            if (!(row.RenderTransform is TranslateTransform t) || t.IsFrozen)
                row.RenderTransform = t = new TranslateTransform();
            return t;
        }

        private static double Height(FrameworkElement row) => row.ActualHeight + row.Margin.Top + row.Margin.Bottom;

        private void Start(int index, double y)
        {
            if (index < 0 || index >= _rows() || index >= _panel.Children.Count)
                return;
            _row = (FrameworkElement)_panel.Children[index];
            _from = _to = index;
            _startY = y;
            _height = Height(_row);
            Panel.SetZIndex(_row, 10);
            _row.Effect = new DropShadowEffect { BlurRadius = 16, ShadowDepth = 3, Opacity = 0.45 };
            var scale = new ScaleTransform(1, 1);
            var group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(Shift(_row));
            _row.RenderTransformOrigin = new Point(0.5, 0.5);
            _row.RenderTransform = group;
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.015, Slide) { EasingFunction = Ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.015, Slide) { EasingFunction = Ease });
            Dragging?.Invoke(true);
        }

        private TranslateTransform RowShift => (TranslateTransform)((TransformGroup)_row.RenderTransform).Children[1];

        private void Move(double y)
        {
            int rows = Math.Min(_rows(), _panel.Children.Count);
            double dy = y - _startY;
            // Keep the row inside the movable rows.
            double top = 0;
            for (int i = 0; i < _from; i++)
                top -= Height((FrameworkElement)_panel.Children[i]);
            double bottom = 0;
            for (int i = _from + 1; i < rows; i++)
                bottom += Height((FrameworkElement)_panel.Children[i]);
            dy = Math.Max(top, Math.Min(bottom, dy));
            RowShift.BeginAnimation(TranslateTransform.YProperty, null);
            RowShift.Y = dy;

            // The slot whose middle the row's middle has passed.
            int to = _from;
            double moved = dy;
            if (moved > 0)
                for (int i = _from + 1; i < rows && moved > Height((FrameworkElement)_panel.Children[i]) / 2; i++)
                {
                    to = i;
                    moved -= Height((FrameworkElement)_panel.Children[i]);
                }
            else
                for (int i = _from - 1; i >= 0 && -moved > Height((FrameworkElement)_panel.Children[i]) / 2; i--)
                {
                    to = i;
                    moved += Height((FrameworkElement)_panel.Children[i]);
                }
            if (to == _to)
                return;
            _to = to;
            for (int i = 0; i < rows; i++)
            {
                if (i == _from)
                    continue;
                double target = _from < _to && i > _from && i <= _to ? -_height
                    : _from > _to && i < _from && i >= _to ? _height : 0;
                var row = (FrameworkElement)_panel.Children[i];
                Shift(row).BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(target, Slide) { EasingFunction = Ease });
            }
        }

        private void End()
        {
            var row = _row;
            if (row == null)
                return;
            _row = null;
            int from = _from, to = _to;
            int rows = Math.Min(_rows(), _panel.Children.Count);
            // Settle into the slot, then hand over; the owner redraws the list from the game.
            double slot = 0;
            if (to > from)
                for (int i = from + 1; i <= to; i++) slot += Height((FrameworkElement)_panel.Children[i]);
            else
                for (int i = to; i < from; i++) slot -= Height((FrameworkElement)_panel.Children[i]);
            var settle = new DoubleAnimation(slot, Slide) { EasingFunction = Ease };
            settle.Completed += (_, __) =>
            {
                row.Effect = null;
                Panel.SetZIndex(row, 0);
                for (int i = 0; i < rows && i < _panel.Children.Count; i++)
                {
                    var r = (FrameworkElement)_panel.Children[i];
                    r.RenderTransform = Transform.Identity;
                }
                Dragging?.Invoke(false);
                if (to != from)
                    _drop(from, to);
            };
            var group = (TransformGroup)row.RenderTransform;
            var scale = (ScaleTransform)group.Children[0];
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, Slide));
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, Slide));
            ((TranslateTransform)group.Children[1]).BeginAnimation(TranslateTransform.YProperty, settle);
        }
    }
}
