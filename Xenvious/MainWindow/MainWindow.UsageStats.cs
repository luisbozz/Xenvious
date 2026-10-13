using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: the usage statistics card in Settings and the page counts.
    public partial class MainWindow
    {
        private bool _loadingUsageSettings;

        private void ShowUsageSettings()
        {
            _loadingUsageSettings = true;
            cbsettingsusage.IsChecked = UsageStats.Enabled;
            _loadingUsageSettings = false;
            tbUsageInstallId.Text = UsageStats.InstallId;
        }

        private void cbsettingsusage_Checked(object sender, RoutedEventArgs e)
        {
            if (_loadingUsageSettings)
                return;
            UsageStats.SetEnabled(cbsettingsusage.IsChecked == true);
        }

        private void BtnUsageCopyId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(UsageStats.InstallId);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                // Another program holds the clipboard; the ID can still be selected in the box.
            }
        }

        private static readonly (string Name, string Key)[] MainPageKeys =
        {
            ("PageDashboard", "dashboard"), ("PageEdit", "edit"), ("PageCopyJobs", "copyjobs"),
            ("PageConverter", "converter"), ("PageTeleport", "teleport"), ("PageGEditor", "geditor"),
            ("PageMod", "mod"), ("PageSettings", "settings"),
        };

        private void CountMainPage(object page)
        {
            string name = (page as FrameworkElement)?.Name;
            foreach (var (pageName, key) in MainPageKeys)
                if (pageName == name)
                    UsageStats.Page("page." + key);
        }

        // Edit menu entries by their key in BuildEditNav, and the sub-page buttons by name.
        private static string EditNavStatKey(string entryKey)
        {
            switch (entryKey)
            {
                case "actor": return "actors";
                case "jobsubtype_mission_capture": return "capture";
                case "editnav_menubits": return "menubits";
                case "editnav_interiors": return "interiors";
                default: return entryKey;
            }
        }

        private static string EditNavSubStatKey(Button button)
        {
            switch (button?.Name)
            {
                case "BtnRaceGeneral": return "general";
                case "BtnRaceCheckpoints": return "checkpoints";
                case "BtnRaceAVEH": return "aveh";
                case "BtnRaceTune": return "tune";
                case "BtnRaceArena": return "arena";
                case "BtnMissionGeneral": return "general";
                case "BtnMissionTeamSettings": return "teams";
                case "BtnMissionPlayerSettings": return "player";
                case "BtnMissionPA": return "playarea";
                case "BtnMissionTPM": return "tpm";
                case "BtnMissionKill": return "kill";
                case "BtnMissionGC": return "gangchase";
                case "BtnMissionotzone": return "overtime";
                case "BtnMissionBlips": return "blips";
                case "BtnSectionInventory": return "inventory";
                case "BtnSectionSMS": return "sms";
                case "BtnSectionGoto": return "goto";
                case "BtnCaptureGeneral": return "general";
                case "BtnCaptureObjects": return "objects";
                case "BtnCaptureDelivery": return "delivery";
                case "BtnModdedProps": return "modded";
                case "BtnAdvancedPropPlacement": return "advanced";
                default: return null;
            }
        }

        private void CountEditNav(EditNavEntry entry, EditNavSub sub)
        {
            if (entry == null)
                return;
            string key = EditNavStatKey(entry.Key);
            // Sub-pages without a button of their own: Rules (Mission) and Fixtures (Props).
            if (sub != null && sub.Button == null && sub.Open != null)
                key = key == "props" ? "fixtures" : key + ".rules";
            else if (sub != null && EditNavSubStatKey(sub.Button) is string subKey)
                key += "." + subKey;
            UsageStats.Page("edit." + key);
        }
    }
}
