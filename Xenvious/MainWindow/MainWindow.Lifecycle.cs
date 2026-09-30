using System.Windows;

namespace Xenvious
{
    // Part of MainWindow: lifecycle cards on the props, dynamic props and weapons pages (in place of "Associated rule").
    public partial class MainWindow
    {
        private void InitLifecycleCards()
        {
            // Collapsed: on these pages the model, look and physics matter more than the rules.
            new LifecycleCard().Attach(ddpropsspawnon, ddpropsspawnteam, tbpropsspawnrule, ddpropsteamclear, tbpropsclearrule, false);
            new LifecycleCard().Attach(dddpropsspawnon, dddpropsspawnteam, tbdpropsspawnrule, dddpropsteamclear, tbdpropsclearrule, false);
            new LifecycleCard().Attach(ddweapspawnon, ddweapspawnteam, tbweapspawnrule, ddweapteamclear, tbweapclearrule, false);
            // Wider columns on these pages so the lifecycle's three pickers fit in one row, as on the Vehicles page.
            foreach (var box in new FrameworkElement[] { ddpropsspawnon, dddpropsspawnon, ddweapspawnon })
            {
                var up = box;
                while (up != null && !(up is MasonryPanel))
                    up = up.Parent as FrameworkElement;
                if (up is MasonryPanel masonry)
                    masonry.MinColumnWidth = 420;
            }
        }
    }
}
