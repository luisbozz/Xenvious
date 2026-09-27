using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Xenvious
{
    // Part of MainWindow: applies a colour theme from Helper Classes/Themes.cs.
    public partial class MainWindow
    {
        public Themes.Theme CurrentTheme { get; private set; } = Themes.All[0];

        /// <summary>A theme brush for code that builds UI; follows theme changes (the brush is changed in place).</summary>
        public static Brush ThemeBrush(string key)
            => Instance?.TryFindResource(key) as Brush ?? Brushes.Gray;

        public void ApplyTheme(Themes.Theme theme)
        {
            CurrentTheme = theme;
            foreach (var pair in theme.Colors)
            {
                var color = (Color)ColorConverter.ConvertFromString(pair.Value);
                // A brush that is still unfrozen is changed in place, so StaticResource users and
                // brushes code already took with FindResource follow as well.
                if (TryFindResource(pair.Key) is SolidColorBrush brush && !brush.IsFrozen)
                    brush.Color = color;
                else
                    Application.Current.Resources[pair.Key] = new SolidColorBrush(color);
            }
            // The side navigation groups follow the deep background.
            if (TryFindResource("NavGroupBackgroundBrush") is SolidColorBrush group && !group.IsFrozen)
                group.Color = (Color)ColorConverter.ConvertFromString(theme.Colors["DeepBrush"]);
            else
                Application.Current.Resources["NavGroupBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.Colors["DeepBrush"]));
            Resources["ShadowColor"] = theme.Light ? Colors.White : Colors.Black;
            XenviousImage.Source = (BitmapImage)FindResource(theme.Light ? "ogimageb256" : "ogimage256");
        }
    }
}
