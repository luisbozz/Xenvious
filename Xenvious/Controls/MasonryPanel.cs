using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// Card columns for the edit pages. A WrapPanel starts every new row below the tallest card of
    /// the row above, which left big gaps under short cards; here each child goes below the column
    /// that is shortest so far, and the columns share the full width.
    /// </summary>
    public class MasonryPanel : Panel
    {
        public static readonly DependencyProperty MinColumnWidthProperty = DependencyProperty.Register(
            nameof(MinColumnWidth), typeof(double), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(340.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            nameof(Spacing), typeof(double), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>At most this many columns (0 = no limit), so a few cards grow with the window instead of leaving it half empty.</summary>
        public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register(
            nameof(MaxColumns), typeof(int), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public int MaxColumns
        {
            get => (int)GetValue(MaxColumnsProperty);
            set => SetValue(MaxColumnsProperty, value);
        }

        public double MinColumnWidth
        {
            get => (double)GetValue(MinColumnWidthProperty);
            set => SetValue(MinColumnWidthProperty, value);
        }

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        private readonly List<Rect> _slots = new List<Rect>();
        private double[] _columnBottoms = new double[0];
        private double _columnWidth;
        private double _fillTo;

        // Height the columns are filled to: the visible part of the surrounding scroll viewer.
        private double ViewportHeight()
        {
            for (DependencyObject p = VisualTreeHelper.GetParent(this); p != null; p = VisualTreeHelper.GetParent(p))
                if (p is ScrollViewer viewer)
                    return viewer.ViewportHeight - Margin.Top - Margin.Bottom;
            return 0;
        }

        protected override Size MeasureOverride(Size available)
        {
            double spacing = Spacing;
            double width = double.IsInfinity(available.Width) ? (MinColumnWidth + spacing) * 3 - spacing : available.Width;
            int columns = Math.Max(1, (int)((width + spacing) / (MinColumnWidth + spacing)));
            if (MaxColumns > 0)
                columns = Math.Min(columns, MaxColumns);
            double columnWidth = Math.Max(0, (width - spacing * (columns - 1)) / columns);
            var heights = new double[columns];

            _slots.Clear();
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(columnWidth, double.PositiveInfinity));
                if (child.Visibility == Visibility.Collapsed)
                {
                    _slots.Add(Rect.Empty);
                    continue;
                }
                int column = 0;
                for (int i = 1; i < columns; i++)
                    if (heights[i] < heights[column] - 0.5)
                        column = i;
                double top = heights[column] > 0 ? heights[column] + spacing : 0;
                _slots.Add(new Rect(column * (columnWidth + spacing), top, columnWidth, child.DesiredSize.Height));
                heights[column] = top + child.DesiredSize.Height;
            }

            double height = 0;
            foreach (double h in heights)
                height = Math.Max(height, h);
            // Empty space below a short column is filled with an empty card down to the bottom of
            // the page, so the columns read as columns (OnRender).
            _columnBottoms = heights;
            _columnWidth = columnWidth;
            _fillTo = Math.Max(height, ViewportHeight());
            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var slot = i < _slots.Count ? _slots[i] : Rect.Empty;
                InternalChildren[i].Arrange(slot.IsEmpty ? new Rect(0, 0, 0, 0) : slot);
            }
            InvalidateVisual();
            return finalSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (!(TryFindResource("SectionBackgroundBrush") is Brush fill))
                return;
            double spacing = Spacing;
            for (int c = 0; c < _columnBottoms.Length; c++)
            {
                double top = _columnBottoms[c];   // already includes the last card's bottom margin
                if (_fillTo - top < 40)
                    continue;
                dc.DrawRoundedRectangle(fill, null, new Rect(c * (_columnWidth + spacing), top, _columnWidth, _fillTo - top), 5, 5);
            }
        }
    }
}
