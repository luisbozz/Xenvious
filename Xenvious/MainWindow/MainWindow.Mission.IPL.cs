using System;
using System.ComponentModel;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / IPL page.
    public partial class MainWindow
    {
        private void tbMissioniplop_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissioniplop.Text) && IsValidInt(tbMissioniplop.Text))
                new Global(GTA.Offsets.Editor.iplop).SetInt(tbMissioniplop.Text);
        }

        private void tbMissioniplop2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissioniplop2.Text) && IsValidInt(tbMissioniplop2.Text))
                new Global(GTA.Offsets.Editor.iplop2).SetInt(tbMissioniplop2.Text);
        }

        private void tbMissionintop_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionintop.Text) && IsValidInt(tbMissionintop.Text))
                new Global(GTA.Offsets.Editor.intop).SetInt(tbMissionintop.Text);
        }

        private void tbMissionintop2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionintop2.Text) && IsValidInt(tbMissionintop2.Text))
                new Global(GTA.Offsets.Editor.intop2).SetInt(tbMissionintop2.Text);
        }

        private void tbMissionintop3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionintop3.Text) && IsValidInt(tbMissionintop3.Text))
                new Global(GTA.Offsets.Editor.intop3).SetInt(tbMissionintop3.Text);
        }
    }
}
