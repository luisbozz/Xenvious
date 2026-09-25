using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Survival page.
    public partial class MainWindow
    {
        public void GetSurvivalValues(bool ignore_focus = false)
        {
            Functions.Read.checkbinary(3, GTA.Offsets.Editor.Survival.sbits, cbsurvalienmode);
            Functions.Read.checkbinary(4, GTA.Offsets.Editor.Survival.sbits, cbsurvalienmodespawn);
            if (!tbsurvwaves.IsFocused || ignore_focus) tbsurvwaves.Text = new Global(GTA.Offsets.Editor.Survival.wave).Get<int>().ToString();
        }

        private void cbsurvalienmode_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.Survival.sbits, cbsurvalienmode);
        }

        private void cbsurvalienmodespawn_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.Survival.sbits, cbsurvalienmodespawn);
        }

        private void tbsurvwaves_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbsurvwaves.Text, false))
                new Global(GTA.Offsets.Editor.Survival.wave).SetInt(tbsurvwaves.Text);
        }
    }
}
