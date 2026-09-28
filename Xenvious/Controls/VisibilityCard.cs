using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xenvious
{
    /// <summary>
    /// "Show in game" card: one switch per creator visibility group (VisibilityGroups). Sits on
    /// Creator Tools.
    /// </summary>
    public class VisibilityCard : Border
    {
        private readonly UniformGrid _tiles = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, -6, 4) };
        private bool _built;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public VisibilityCard()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) => { if (IsVisible) Refresh(); };
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            Style = (Style)FindResource("DashCard");

            var header = new Border { Style = (Style)FindResource("DashCardHeader"), Child = new TextBlock { Style = (Style)FindResource("DashCardTitle"), Text = T("vis_card", "Show in game") } };
            var hint = new TextBlock
            {
                Text = T("vis_hint", "The creator draws these all the time, not only while placing. LTS and Capture start with all of them off; Xenvious switches them on."),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 10)
            };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");

            var groups = new System.Collections.Generic.List<(string Key, string Name)>(VisibilityGroups.Groups);
            groups.AddRange(VisibilityGroups.Extra);
            for (int i = 0; i < groups.Count; i++)
            {
                int bit = i;
                var tile = new ToggleButton
                {
                    Style = (Style)FindResource("ChoiceTile"),
                    MinHeight = 34,
                    Height = 34,
                    Margin = new Thickness(0, 0, 6, 6),
                    FontSize = 13,
                    Content = T(groups[i].Key, groups[i].Name)
                };
                tile.Click += (_, __) =>
                {
                    VisibilityGroups.Set(bit, tile.IsChecked == true);
                };
                _tiles.Children.Add(tile);
            }

            var body = new StackPanel { Margin = new Thickness(14, 12, 14, 8) };
            body.Children.Add(hint);
            body.Children.Add(_tiles);
            var extraHint = new TextBlock
            {
                Text = T("vis_custom_hint", "\"(custom)\" is drawn by Xenvious' own script functions and needs the experimental script features (Settings)."),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12
            };
            extraHint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            body.Children.Add(extraHint);
            var dock = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            dock.Children.Add(body);
            Child = dock;
            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < _tiles.Children.Count; i++)
                ((ToggleButton)_tiles.Children[i]).IsChecked = VisibilityGroups.IsOn(i);
        }
    }
}
