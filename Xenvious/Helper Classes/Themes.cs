using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// Colour themes. Every theme sets the same tokens; XAML uses them as DynamicResource, code
    /// through FindResource. A new colour in the UI gets a token here (in every theme) instead of a
    /// fixed hex value, otherwise it stays dark in the light themes (docs/handoff/CHECKLIST.md).
    /// </summary>
    public static class Themes
    {
        public sealed class Theme
        {
            public string Key;          // config.ini value
            public string NameKey;      // translation key
            public string Fallback;
            public bool Light;          // dark logo, light shadow
            public Dictionary<string, string> Colors;
        }

        private static readonly string[] Tokens =
        {
            "BackgroundBrush", "SectionBackgroundBrush", "SeactionHeaderBackgroundBrush", "DeepBrush", "LineBrush",
            "SelectedBackgroundBrush", "HoverBackgroundBrush", "ButtonHoverBackgroundBrush", "ButtonClickBackgroundBrush",
            "CheckBoxBackground", "TextBoxBackground", "TextBoxBorder", "TextBoxBorderInner",
            "ComboBoxBackground", "ComboBoxBorder", "ComboBoxBorderInner", "ComboBoxSelected", "ComboBoxHighlighted", "ComboBoxArrow",
            "TextColor", "TextBoxForegroundThemeBrush", "MutedTextBrush", "NavMutedBrush", "FaintTextBrush", "DisabledForegroundBrush",
            "ScrollThumbBrush", "PrimaryButtonBackground", "PrimaryButtonForeground", "HighlightBrush", "HighlightForeground",
            "SelectedTileBrush", "OkBrush", "WarnBrush", "BadBrush", "AccentBrush",
        };

        /// <summary>Every brush a theme sets, plus the ones derived from them.</summary>
        public static IEnumerable<string> TokenKeys => Tokens.Concat(new[] { "NavGroupBackgroundBrush", "AccentSoftBrush" });

        // Order: token list above.
        private static Theme T(string key, string nameKey, string fallback, bool light, params string[] hex)
            => new Theme { Key = key, NameKey = nameKey, Fallback = fallback, Light = light, Colors = Tokens.Zip(hex, (t, h) => (t, h)).ToDictionary(p => p.t, p => p.h) };

        public static readonly IReadOnlyList<Theme> All = new[]
        {
            T("gray", "theme_dark", "Dark", false,
                "#202225", "#2F3136", "#36393F", "#232529", "#3A3D43",
                "#232529", "#2A2C30", "#34373C", "#37393F",
                "#36393F", "#303339", "#131517", "#232529",
                "#303339", "#131517", "#232529", "#404349", "#383B40", "#FFFFFF",
                "#FFFFFF", "#FFFFFF", "#A3A6AA", "#A3A6AA", "#72767D", "#888888",
                "#5A5E66", "#E3E5E8", "#202225", "#FAC828", "#202225",
                "#4F545C", "#43B581", "#FAA61A", "#F04747", "#5B9BE6"),
            // Light greys with GTA Online's freemode blue as accent.
            T("white", "theme_light", "Light", true,
                "#E4E6EA", "#F4F5F7", "#E9EBEE", "#E9EBEE", "#D5D8DC",
                "#DDE0E4", "#EEF0F2", "#DDE0E4", "#D2D6DB",
                "#DDE0E4", "#FFFFFF", "#C9CCD1", "#E1E4E8",
                "#FFFFFF", "#C9CCD1", "#E1E4E8", "#DDE6F3", "#EEF1F4", "#5F656D",
                "#2E3338", "#2E3338", "#5F656D", "#5F656D", "#8A9098", "#9AA0A6",
                "#B9BDC3", "#2D6EB9", "#FFFFFF", "#2D6EB9", "#FFFFFF",
                "#D6E2F2", "#3C9A3C", "#C8961E", "#C82828", "#2D6EB9"),
            // catppuccin.com, Latte (light) and Mocha (dark).
            T("latte", "theme_latte", "Catppuccin Latte", true,
                "#DCE0E8", "#EFF1F5", "#E6E9EF", "#E6E9EF", "#CCD0DA",
                "#CCD0DA", "#E6E9EF", "#CCD0DA", "#BCC0CC",
                "#CCD0DA", "#FFFFFF", "#BCC0CC", "#DCE0E8",
                "#FFFFFF", "#BCC0CC", "#DCE0E8", "#CCD0DA", "#E6E9EF", "#6C6F85",
                "#4C4F69", "#4C4F69", "#6C6F85", "#6C6F85", "#9CA0B0", "#9CA0B0",
                "#ACB0BE", "#1E66F5", "#FFFFFF", "#8839EF", "#FFFFFF",
                "#CCD0DA", "#40A02B", "#DF8E1D", "#D20F39", "#1E66F5"),
            T("mocha", "theme_mocha", "Catppuccin Mocha", false,
                "#11111B", "#1E1E2E", "#181825", "#181825", "#313244",
                "#313244", "#181825", "#313244", "#45475A",
                "#313244", "#313244", "#45475A", "#181825",
                "#313244", "#45475A", "#181825", "#45475A", "#3E4057", "#CDD6F4",
                "#CDD6F4", "#CDD6F4", "#A6ADC8", "#A6ADC8", "#6C7086", "#6C7086",
                "#585B70", "#89B4FA", "#11111B", "#CBA6F7", "#11111B",
                "#45475A", "#A6E3A1", "#F9E2AF", "#F38BA8", "#89B4FA"),
        };

        public static Theme Find(string key) => All.FirstOrDefault(t => t.Key == key) ?? All[0];
    }
}
