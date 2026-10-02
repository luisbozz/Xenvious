using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: Kill page layout (player rules as goals on top, the old raw view below).
    public partial class MainWindow
    {
        private void InitKillLayout()
        {
            if (!(PageInnerMissionKill.Content is FrameworkElement old))
                return;
            PageInnerMissionKill.Content = null;
            var page = new StackPanel { Margin = new Thickness(10, 0, 10, 0) };
            // The team comes from the page's header (the old page's team box). The old player rule
            // number box there belongs to the raw view and would only confuse next to the steps.
            page.Children.Add(new PlayerRulesView(ddkillteamno, () =>
            {
                MainPages.SelectedItem = PageMod;
                BtnModScrPatches_Click(null, null);
            }));
            if (ddkillno.Parent is FrameworkElement inner && inner.Parent is Border numberBox && numberBox.Parent is Panel bar)
            {
                int at = bar.Children.IndexOf(numberBox);
                numberBox.Visibility = Visibility.Collapsed;
                if (at > 0 && bar.Children[at - 1] is TextBlock numberLabel)
                    numberLabel.Visibility = Visibility.Collapsed;
            }
            // The old page (raw fields, freeze) stays reachable, closed.
            var raw = new Expander { Header = TranslateOr("pr_raw", "Raw values (old view)"), IsExpanded = false, Margin = new Thickness(0, 0, 0, 12), Content = old };
            raw.SetResourceReference(StyleProperty, "CardExpander");
            old.Height = 640;
            page.Children.Add(raw);
            PageInnerMissionKill.Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = page };
        }
    }
}
