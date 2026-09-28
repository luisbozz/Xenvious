using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xenvious
{
    /// <summary>
    /// One cell per rule of a team: green while the entity exists, red where it is gone, grey when
    /// it cannot be told (it depends on another team). A dot marks the entity's own rule, a dashed
    /// ring its extra objectives. Colours are theme brushes.
    /// </summary>
    public class RuleStrip : UniformGrid
    {
        public enum State { Unknown, Live, Gone }

        public RuleStrip()
        {
            Rows = 1;
            Margin = new Thickness(0, 4, 0, 0);
        }

        public void Show(IReadOnlyList<State> states, int mainRule, ICollection<int> extraRules, System.Func<int, string> tip)
        {
            Children.Clear();
            Columns = states.Count < 1 ? 1 : states.Count;
            for (int i = 0; i < states.Count; i++)
                Children.Add(Cell(i, states[i], i == mainRule, extraRules.Contains(i), tip(i)));
        }

        private static UIElement Cell(int rule, State state, bool main, bool extra, string tip)
        {
            string brush = state == State.Live ? "OkBrush" : state == State.Gone ? "BadBrush" : null;
            var grid = new Grid { Height = 30, Margin = new Thickness(0, 5, 3, 0), ToolTip = tip };
            var bg = new Border { CornerRadius = new CornerRadius(5) };
            bg.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            grid.Children.Add(bg);
            var frame = new Border { CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(1) };
            frame.SetResourceReference(Border.BorderBrushProperty, brush ?? "LineBrush");
            if (brush != null)
            {
                var tint = new Border { CornerRadius = new CornerRadius(5), Opacity = 0.16 };
                tint.SetResourceReference(Border.BackgroundProperty, brush);
                grid.Children.Add(tint);
            }
            grid.Children.Add(frame);
            var text = new TextBlock { Text = (rule + 1).ToString(CultureInfo.CurrentCulture), FontSize = 12.5, FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            text.SetResourceReference(TextBlock.ForegroundProperty, brush ?? "FaintTextBrush");
            grid.Children.Add(text);
            if (main || extra)
            {
                var dot = new Ellipse { Width = 10, Height = 10, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, -5, -3, 0) };
                if (main)
                    dot.SetResourceReference(Shape.FillProperty, "AccentBrush");
                else
                {
                    dot.StrokeThickness = 2;
                    dot.StrokeDashArray = new DoubleCollection { 1.5, 1 };
                    dot.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
                    dot.SetResourceReference(Shape.FillProperty, "SectionBackgroundBrush");
                }
                grid.Children.Add(dot);
            }
            return grid;
        }
    }
}
