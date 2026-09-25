using System.ComponentModel;
using System.Linq;
using System.Windows;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Misc page.
    public partial class MainWindow
    {
        private void BtnModJobLinks_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMod.SelectedItem = PageInnerModJL;
        }

        private void BtnModStuff_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMod.SelectedItem = PageInnerModStuff;
            if (GTA.Offsets.Editor.localptr != null)
            {
                tbmodstuffccptr.Text = string.Join(", ", GTA.Offsets.Editor.localptr.Select(x => x.ToString("X")));
                tbmodstuffccprace.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_race * 8 }).GetAddress().ToString("X");
                tbmodstuffccplts.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_lts * 8 }).GetAddress().ToString("X");
                tbmodstuffccpcapture.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_capture * 8 }).GetAddress().ToString("X");
                tbmodstuffccpdm.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_dm * 8 }).GetAddress().ToString("X");
                tbmodstuffccpsurvival.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_survival * 8 }).GetAddress().ToString("X");
                tbmodstuffpst.Text = m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_menu * 8).ToString("X")).GetAddress().ToString("X");
            }
        }

        private void BtnModnrcidjc_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMod.SelectedItem = PageInnerModnrcidjc;
        }

        private void BtnModSCJobs_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMod.SelectedItem = PageInnerModSCJobs;
        }

        private void BtnModLogs_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMod.SelectedItem = PageInnerModLogs;
        }

        private void BtnModScrPatches_Click(object sender, RoutedEventArgs e)
        {
            LoadScrPatchesPage();
            PageInnerMod.SelectedItem = PageInnerModScrPatches;
        }

        private void BtnCopyJobs_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageCopyJobs;
            FillCopyJobModes();
        }

        private void BtnMS_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageMod;
            PageInnerMod.SelectedItem = PageMenuSwitcher;
        }
    }
}
