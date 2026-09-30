using System;
using System.Globalization;
using System.Linq;
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
        private TextBox _box;
        private bool _sync;

        /// <summary>
        /// Turns the heading field into "Heading · 135°" with a slider and the box beside it. The
        /// box moves into the slider row; its old label is hidden.
        /// </summary>
        public static HeadingSlider Attach(TextBox box)
        {
            var slider = new HeadingSlider { _box = box, Margin = new Thickness(0, 0, 0, 10) };
            var home = box.Parent as Panel;
            int at = home?.Children.IndexOf(box) ?? -1;
            if (home != null && at > 0 && home.Children[at - 1] is TextBlock oldLabel)
                oldLabel.Visibility = Visibility.Collapsed;

            // Where the slider goes: past the grid cells the box sits in.
            FrameworkElement row = box;
            while (row.Parent is Grid || row.Parent is StackPanel cell && cell.Children.Count <= 2 && cell.Parent is Grid)
                row = (FrameworkElement)row.Parent;
            home?.Children.Remove(box);
            if (home is StackPanel c && c.Parent is Grid && c.Children.Cast<UIElement>().All(e => e.Visibility != Visibility.Visible))
                c.Visibility = Visibility.Collapsed;
            slider.Build();
            if (row.Parent is Panel panel)
                panel.Children.Insert(panel.Children.IndexOf(row) + 1, slider);
            else if (home != null)
                home.Children.Insert(Math.Max(0, at), slider);
            return slider;
        }

        private readonly TextBlock _label = new TextBlock();

        private void Build()
        {
            _label.SetResourceReference(StyleProperty, "FieldLabel");
            DockPanel.SetDock(_label, Dock.Top);
            Children.Add(_label);
            _box.Width = 70;
            _box.Margin = new Thickness(10, 0, 0, 0);
            DockPanel.SetDock(_box, Dock.Right);
            Children.Add(_box);
            var north = new Button { Content = "N", Padding = new Thickness(7, 2, 7, 2), Height = 30, Margin = new Thickness(0, 0, 10, 0), ToolTip = MainWindow.Instance?.TranslateOr("hs_north", "Face north (0°)") ?? "0°" };
            north.SetResourceReference(StyleProperty, "FormButton");
            north.Click += (_, __) => _slider.Value = 0;
            DockPanel.SetDock(north, Dock.Left);
            Children.Add(north);
            ComboViews.Tint(_slider);
            Children.Add(_slider);

            _slider.ValueChanged += (_, __) =>
            {
                ShowLabel(_slider.Value);
                if (_sync || !_box.IsEnabled)
                    return;
                string text = Math.Round(_slider.Value, 1).ToString(CultureInfo.CurrentCulture);
                if (_box.Text != text)
                    _box.Text = text;
            };
            _box.TextChanged += (_, __) => FromBox();
            _slider.IsEnabled = north.IsEnabled = _box.IsEnabled;
            _box.IsEnabledChanged += (_, __) => _slider.IsEnabled = north.IsEnabled = _box.IsEnabled;
            FromBox();
            ShowLabel(_slider.Value);
        }

        private void ShowLabel(double v) => _label.Text = (MainWindow.Instance?.TranslateOr("heading", "Heading") ?? "Heading") + " · " + Math.Round(v).ToString(CultureInfo.CurrentCulture) + "°";

        private void FromBox()
        {
            if (!float.TryParse(_box.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float v)
                && !float.TryParse(_box.Text?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                return;
            v %= 360;
            if (v < 0) v += 360;
            _sync = true;
            _slider.Value = v;
            _sync = false;
        }
    }
}
