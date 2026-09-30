using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    /// <summary>
    /// A 0-360° slider for a page's heading box. It only sets the box's text, so the page's own
    /// TextChanged handler writes the game on every step (live while dragging); the slider follows
    /// the box when the page loads another entity. Negative headings (-180..180) show as 180..360.
    /// </summary>
    public class HeadingSlider : DockPanel
    {
        private readonly Slider _slider = new Slider { Minimum = 0, Maximum = 360, SmallChange = 1, LargeChange = 15, VerticalAlignment = VerticalAlignment.Center };
        private readonly TextBlock _value = new TextBlock { Width = 44, TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold };
        private TextBox _box;
        private bool _sync;

        /// <summary>Puts a slider under <paramref name="box"/>'s row.</summary>
        public static HeadingSlider Attach(TextBox box)
        {
            var slider = new HeadingSlider { _box = box, Margin = new Thickness(0, 0, 0, 10) };
            slider.Build();

            // Walk up past the grid cells the box sits in, then insert after that row.
            FrameworkElement row = box;
            while (row.Parent is Grid || row.Parent is StackPanel cell && cell.Children.Count <= 2 && cell.Parent is Grid)
                row = (FrameworkElement)row.Parent;
            if (row.Parent is Panel panel)
                panel.Children.Insert(panel.Children.IndexOf(row) + 1, slider);
            return slider;
        }

        private void Build()
        {
            _value.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            _slider.SetResourceReference(StyleProperty, typeof(Slider));
            DockPanel.SetDock(_value, Dock.Right);
            Children.Add(_value);
            var north = new Button { Content = "N", Padding = new Thickness(7, 2, 7, 2), Margin = new Thickness(0, 0, 8, 0), ToolTip = MainWindow.Instance?.TranslateOr("hs_north", "Face north (0°)") ?? "0°" };
            north.SetResourceReference(StyleProperty, "FormButton");
            north.Click += (_, __) => _slider.Value = 0;
            DockPanel.SetDock(north, Dock.Left);
            Children.Add(north);
            Children.Add(_slider);

            _slider.ValueChanged += (_, __) =>
            {
                _value.Text = Math.Round(_slider.Value).ToString(CultureInfo.CurrentCulture) + "°";
                if (_sync || !_box.IsEnabled)
                    return;
                string text = Math.Round(_slider.Value, 1).ToString(CultureInfo.CurrentCulture);
                if (_box.Text != text)
                    _box.Text = text;
            };
            _box.TextChanged += (_, __) => FromBox();
            _box.IsEnabledChanged += (_, __) => IsEnabled = _box.IsEnabled;
            IsEnabled = _box.IsEnabled;
            FromBox();
        }

        private void FromBox()
        {
            if (!float.TryParse(_box.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float v)
                && !float.TryParse(_box.Text?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                return;
            v %= 360;
            if (v < 0) v += 360;
            _sync = true;
            _slider.Value = v;
            _value.Text = Math.Round(v).ToString(CultureInfo.CurrentCulture) + "°";
            _sync = false;
        }
    }
}
