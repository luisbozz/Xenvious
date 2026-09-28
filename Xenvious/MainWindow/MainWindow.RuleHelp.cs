using System.Diagnostics;
using System.Windows;

namespace Xenvious
{
    // Part of MainWindow: the help for teams and rules, linked from pages that are set per team and rule.
    public partial class MainWindow
    {
        private const string RuleTutorialUrl = "https://www.youtube.com/watch?v=MB6_YLavkyU";
        private const string RuleTutorialPlaylistUrl = "https://www.youtube.com/playlist?list=PLuKCHkXZGkYBE2fnGXsbnsVsIccLX-SSt";

        private async void BtnRuleHelp_Click(object sender, RoutedEventArgs e)
        {
            var choice = await ChooseAsync(
                TranslateOr("rulehelp_title", "Teams and rules"),
                TranslateOr("rulehelp_text",
                    "A mission runs as a list of rules per team (\"number of rules\" in Team settings). Rule 1 is the first objective, then rule 2 and so on.\n\n" +
                    "Settings on this page belong to one team and one rule: pick the team, then the rule whose values you want to see or change. " +
                    "A gang chase set on rule 3 only starts once the team reaches rule 3.\n\n" +
                    "The tutorial explains rules step by step."),
                TranslateOr("rulehelp_video", "Watch the tutorial"),
                TranslateOr("rulehelp_playlist", "Playlist"),
                TranslateOr("close", "Close"));
            if (choice == DialogChoice.Confirm)
                OpenUrl(RuleTutorialUrl);
            else if (choice == DialogChoice.Alternative)
                OpenUrl(RuleTutorialPlaylistUrl);
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // No browser registered; nothing else to do.
            }
        }
    }
}
