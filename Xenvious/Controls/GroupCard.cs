using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xenvious
{
    /// <summary>
    /// A card with a row of group tabs and one group shown at a time, for the rarely used
    /// settings of a page ("Advanced"). The last group is remembered in config.ini.
    /// </summary>
    public class GroupCard : SectionCard
    {
        private readonly WrapPanel _tabs = new WrapPanel();
        private readonly ContentControl _host = new ContentControl();
        private readonly List<(ToggleButton Tab, FrameworkElement Content)> _groups = new List<(ToggleButton, FrameworkElement)>();
        private readonly string _configKey;

        public GroupCard(string configKey)
        {
            _configKey = configKey;
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            CanCollapse = true;
            var body = new StackPanel();
            // Same dark box as the header navigation.
            var box = new Border { Child = _tabs, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 12) };
            box.SetResourceReference(StyleProperty, "NavGroup");
            body.Children.Add(box);
            body.Children.Add(_host);
            Content = body;
        }

        public void Add(string key, string fallback, FrameworkElement content)
        {
            var tab = new ToggleButton { Style = (Style)MainWindow.Instance.FindResource("NavTab"), Content = MainWindow.Instance.TranslateOr(key, fallback), FontSize = 13, Padding = new Thickness(9, 5, 9, 5) };
            int index = _groups.Count;
            tab.Click += (_, __) => Select(index, true);
            _groups.Add((tab, content));
            _tabs.Children.Add(tab);
            if (_groups.Count == 1 || index == Remembered())
                Select(index, false);
        }

        private int Remembered() => new ini_reader(Functions.getRoamingConfigFilePath()).ReadInteger("Settings", _configKey, 0);

        private void Select(int index, bool save)
        {
            for (int i = 0; i < _groups.Count; i++)
                _groups[i].Tab.IsChecked = i == index;
            _host.Content = _groups[index].Content;
            Summary = _groups[index].Tab.Content as string;
            if (save)
                new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", _configKey, index);
        }
    }
}
