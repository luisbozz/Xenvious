using System.Windows;

namespace Xenvious
{
    // Part of MainWindow: lifecycle cards on the props, dynamic props and weapons pages (in place of "Associated rule").
    public partial class MainWindow
    {
        private void InitLifecycleCards()
        {
            // Props and dynamic props collapsed: there the model, look and physics matter more than the rules.
            new LifecycleCard().Attach(ddpropsspawnon, ddpropsspawnteam, tbpropsspawnrule, ddpropsteamclear, tbpropsclearrule, false);
            new LifecycleCard().Attach(dddpropsspawnon, dddpropsspawnteam, tbdpropsspawnrule, dddpropsteamclear, tbdpropsclearrule, false);
            // Weapons: open, the pickup's rules are what it is mostly about.
            new LifecycleCard().Attach(ddweapspawnon, ddweapspawnteam, tbweapspawnrule, ddweapteamclear, tbweapclearrule, true);
            // Zones: start and end rule per team, above the zone's advanced card (whose team,
            // rule and priority rows it replaces).
            var zoneAdvanced = (FrameworkElement)ddzoneteam;
            while (zoneAdvanced != null && !(zoneAdvanced is System.Windows.Controls.Border b && b.Style == (Style)FindResource("DashCard")))
                zoneAdvanced = zoneAdvanced.Parent as FrameworkElement;
            if (zoneAdvanced != null)
            {
                new ZoneLifecycleCard().AttachZone(ddzoneteam, tbzonerule, tbzonepriority, zoneAdvanced);
                foreach (var box in new FrameworkElement[] { ddzoneteam, tbzonepriority })
                {
                    FrameworkElement row = box;
                    while (row.Parent is System.Windows.Controls.Grid || row.Parent is System.Windows.Controls.StackPanel cell && cell.Children.Count <= 2 && cell.Parent is System.Windows.Controls.Grid)
                        row = (FrameworkElement)row.Parent;
                    row.Visibility = Visibility.Collapsed;
                }
            }

            // Wider columns (props, dynamic props, weapons, doors, zones) so the lifecycle's three pickers fit in one row, as on the Vehicles page.
            foreach (var box in new FrameworkElement[] { ddpropsspawnon, dddpropsspawnon, ddweapspawnon, dddoorsno, ddzoneno })
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
