using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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

        protected override Size MeasureOverride(Size available)
        {
            double spacing = Spacing;
            double width = double.IsInfinity(available.Width) ? (MinColumnWidth + spacing) * 3 - spacing : available.Width;
            int columns = Math.Max(1, (int)((width + spacing) / (MinColumnWidth + spacing)));
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
            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var slot = i < _slots.Count ? _slots[i] : Rect.Empty;
                InternalChildren[i].Arrange(slot.IsEmpty ? new Rect(0, 0, 0, 0) : slot);
            }
            return finalSize;
        }
    }
}
