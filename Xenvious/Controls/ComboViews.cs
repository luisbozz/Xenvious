using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// Other faces for a page's ComboBox: tiles (few choices) or a stepped slider (ordered
    /// choices like accuracy or health). The ComboBox stays hidden and keeps its handler, so
    /// picking here sets its SelectedIndex and the page writes the game as before.
    /// </summary>
    public static class ComboViews
    {
        private static string ItemText(object item) => (item as ComboBoxItem)?.Content?.ToString() ?? item?.ToString() ?? "";

        /// <summary>Replaces the box with one tile per item.</summary>
        public static FrameworkElement Tiles(ComboBox box, int columns)
        {
            var grid = new UniformGrid { Columns = columns, Margin = new Thickness(0, 0, -6, 4) };
            var tiles = new List<ToggleButton>();
            for (int i = 0; i < box.Items.Count; i++)
            {
                int index = i;
                // A wrapping text block: the tile grows instead of cutting the text off.
                var text = new TextBlock { Text = ItemText(box.Items[i]), TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, FontSize = 13, FontWeight = FontWeights.SemiBold };
                var tile = new ToggleButton { Content = text, Margin = new Thickness(0, 0, 6, 6), MinHeight = 36, Height = double.NaN, Padding = new Thickness(6, 5, 6, 5) };
                tile.SetResourceReference(FrameworkElement.StyleProperty, "ChoiceTile");
                tile.MinHeight = 36;
                tile.Click += (_, __) => { box.SelectedIndex = index; Sync(); };
                tiles.Add(tile);
                grid.Children.Add(tile);
            }
            void Sync()
            {
                for (int i = 0; i < tiles.Count; i++)
                {
                    tiles[i].IsChecked = i == box.SelectedIndex;
                    tiles[i].IsEnabled = box.IsEnabled;
                }
            }
            box.SelectionChanged += (_, __) => Sync();
            box.IsEnabledChanged += (_, __) => Sync();
            Sync();
            Replace(box, grid);
            return grid;
        }

        /// <summary>Replaces the box with a slider over its items; the label shows the picked item.</summary>
        public static FrameworkElement Steps(ComboBox box, TextBlock label)
        {
            string title = label?.Text ?? "";
            var slider = new Slider { Minimum = 0, Maximum = System.Math.Max(0, box.Items.Count - 1), IsSnapToTickEnabled = true, TickFrequency = 1, SmallChange = 1, LargeChange = 1,
                Margin = new Thickness(0, 4, 0, 12) };
            Tint(slider);
            bool sync = false;
            void Show()
            {
                sync = true;
                if (box.SelectedIndex >= 0) slider.Value = box.SelectedIndex;
                slider.IsEnabled = box.IsEnabled;
                if (label != null)
                    label.Text = box.SelectedIndex >= 0 ? title + " · " + ItemText(box.Items[box.SelectedIndex]) : title;
                sync = false;
            }
            slider.ValueChanged += (_, __) => { if (!sync) { box.SelectedIndex = (int)slider.Value; Show(); } };
            box.SelectionChanged += (_, __) => Show();
            box.IsEnabledChanged += (_, __) => Show();
            Show();
            Replace(box, slider);
            return slider;
        }

        /// <summary>
        /// The app's slider draws its right part in the card colour; on a card it looks like a
        /// lone thumb. A local override gives that part the darker field colour; the filled part
        /// stays the app's grey.
        /// </summary>
        public static void Tint(Slider slider)
        {
            if (MainWindow.Instance?.TryFindResource("DeepBrush") is Brush deep)
                slider.Resources["SectionBackgroundBrush"] = deep;
        }

        private static void Replace(ComboBox box, FrameworkElement with)
        {
            if (box.Parent is Panel panel)
            {
                panel.Children.Insert(panel.Children.IndexOf(box) + 1, with);
                box.Visibility = Visibility.Collapsed;
            }
        }
    }
}
