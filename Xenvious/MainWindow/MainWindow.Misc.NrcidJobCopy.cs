using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Misc / NrcidJobCopy page.
    public partial class MainWindow
    {
        private void Btnnrcidjcgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.GetLocationVec();

            tbnrcidjcstartlocx.Text = loc.X.ToString();
            tbnrcidjcstartlocy.Text = loc.Y.ToString();
            tbnrcidjcstartlocz.Text = loc.Z.ToString();
        }

        private void tbnrcidjcstartlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbnrcidjcstartlocx.Text))
                new Global(GTA.Offsets.Editor.start).SetFloat(tbnrcidjcstartlocx.Text);
        }

        private void tbnrcidjcstartlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbnrcidjcstartlocy.Text))
                new Global(GTA.Offsets.Editor.start + 1).SetFloat(tbnrcidjcstartlocy.Text);
        }

        private void tbnrcidjcstartlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbnrcidjcstartlocz.Text))
                new Global(GTA.Offsets.Editor.start + 2).SetFloat(tbnrcidjcstartlocz.Text);
        }

        private void ddnrcidjcno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getNRCIDValuesJC(true);
        }

        private void tbnrcidjc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.nrcid + ddnrcidjcno.SelectedIndex * 0x30).SetString(tbnrcidjc.Text);
        }

        private void tbnrmttjc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.nrmtt + ddnrcidjcno.SelectedIndex).SetInt(tbnrmttjc.Text);
        }

        public void getNRCIDValuesJC(bool ignore_focus = false)
        {
            if (m.IsProcOpen && ddnrcidjcno.SelectedIndex > -1)
            {
                if (!tbnrcidjc.IsFocused || ignore_focus) tbnrcidjc.Text = new Global(GTA.Offsets.Editor.nrcid + ddnrcidjcno.SelectedIndex * 0x30).GetString();
                if (!tbnrmttjc.IsFocused || ignore_focus) tbnrmttjc.Text = new Global(GTA.Offsets.Editor.nrmtt + ddnrcidjcno.SelectedIndex).Get<int>().ToString();
                if (ignore_focus)
                    SelectActiveTextBox();
            }
        }

        public void prepareNRCIDJobCopyJob()
        {
            if (m.IsProcOpen)
            {
                if (string.IsNullOrEmpty(new Global(GTA.Offsets.Editor.nm).GetString()))
                    new Global(GTA.Offsets.Editor.nm).SetString("secret job name");
                if (string.IsNullOrEmpty(new Global(GTA.Offsets.Editor.dec).GetString()))
                    new Global(GTA.Offsets.Editor.dec).SetString("secret job dec");


                new Global(GTA.Offsets.Editor.dlcrel).SetInt(5);
                new Global(GTA.Offsets.Editor.num).SetInt(4);
                new Global(GTA.Offsets.Editor.min).SetInt(1);
                new Global(GTA.Offsets.Editor.tnum).SetInt(1);
                new Global(GTA.Offsets.Editor.tmt).SetInt(19);
                new Global(GTA.Offsets.Editor.mnumpt).SetInt(4);
                new Global(GTA.Offsets.Editor.trel).SetInt(1082419);

                new Global(GTA.Offsets.Editor.player_number).SetInt(4);
                for (int i = 0; i < 5; i++)
                {
                    new Global(GTA.Offsets.Editor.player_loc + 0 + i * GTA.Offsets.Editor.next_settings).SetFloat(new Global(GTA.Offsets.Editor.start + 0).Get<float>());
                    new Global(GTA.Offsets.Editor.player_loc + 1 + i * GTA.Offsets.Editor.next_settings).SetFloat(new Global(GTA.Offsets.Editor.start + 1).Get<float>());
                    new Global(GTA.Offsets.Editor.player_loc + 2 + i * GTA.Offsets.Editor.next_settings).SetFloat(new Global(GTA.Offsets.Editor.start + 2).Get<float>() + 2000f);
                }


                new Global(GTA.Offsets.Editor.Locations.number).SetInt(1);
                new Global(GTA.Offsets.Editor.Locations.locx).SetFloat(new Global(GTA.Offsets.Editor.start + 0).Get<float>());
                new Global(GTA.Offsets.Editor.Locations.locy).SetFloat(new Global(GTA.Offsets.Editor.start + 1).Get<float>());
                new Global(GTA.Offsets.Editor.Locations.locz).SetFloat(new Global(GTA.Offsets.Editor.start + 2).Get<float>() + 900f);
                new Global(GTA.Offsets.Editor.Locations.sz).SetFloat(2000f);
                new Global(GTA.Offsets.Editor.Locations.dir).SetFloat(2000f);
                new Global(GTA.Offsets.Editor.Locations.rule).SetInt(4);
                new Global(GTA.Offsets.Editor.Locations.pri).SetInt(0);

                new Global(GTA.Offsets.Editor.Objects.number).SetInt(1);
                new Global(GTA.Offsets.Editor.Objects.model).SetInt(-1249748547);
                new Global(GTA.Offsets.Editor.Objects.loc + 0).SetFloat(tbnrcidjcstartlocx.Text);
                new Global(GTA.Offsets.Editor.Objects.loc + 1).SetFloat(tbnrcidjcstartlocy.Text);
                new Global(GTA.Offsets.Editor.Objects.loc + 2).SetFloat(tbnrcidjcstartlocz.Text);
                new Global(GTA.Offsets.Editor.Objects.rule).SetInt(1);
                new Global(GTA.Offsets.Editor.Objects.pri).SetInt(0);
                new Global(GTA.Offsets.Editor.tsc).SetInt(1);

                new Global(GTA.Offsets.Editor.dpos + 0).SetFloat(new Global(GTA.Offsets.Editor.start + 0).Get<float>());
                new Global(GTA.Offsets.Editor.dpos + 1).SetFloat(new Global(GTA.Offsets.Editor.start + 1).Get<float>());
                new Global(GTA.Offsets.Editor.dpos + 2).SetFloat(new Global(GTA.Offsets.Editor.start + 2).Get<float>());
                new Global(GTA.Offsets.Editor.drpr).SetFloat(3);

                new Global(GTA.Offsets.Editor.cam).SetFloat(3537.481f);
                new Global(GTA.Offsets.Editor.cam).SetFloat(3665.026f);
                new Global(GTA.Offsets.Editor.cam).SetFloat(28.5f);
                new Global(GTA.Offsets.Editor.camh).SetFloat(170.5f);
                new Global(GTA.Offsets.Editor.camp).SetFloat(2.6f);

                plylfreeze[0] = 4;
                cbplylfreeze.IsChecked = true;
            }
        }

        private void Btnnrcidprep_Click(object sender, RoutedEventArgs e)
        {
            prepareNRCIDJobCopyJob();
        }

        private void Btnnrcidcopy_Click(object sender, RoutedEventArgs e)
        {
            long saddy, eaddy;

            if (!long.TryParse(tbnrcidcopycs.Text, NumberStyles.HexNumber, null, out saddy))
            {
                displayScreenMessage("couldnt parse start address.");
                return;
            }
            if (!long.TryParse(tbnrcidcopyce.Text, NumberStyles.HexNumber, null, out eaddy))
            {
                displayScreenMessage("couldnt parse end address.");
                return;
            }

            nrcidcopy = new nrcidcopy(saddy, eaddy);

            if (nrcidcopy == null)
            {
                displayScreenMessage("an error occured.");
                return;
            }

            nrcidcopy.values = new List<int>();

            for (int i = 0; i < nrcidcopy.range / 4; i++)
            {
                nrcidcopy.values.Add(m.memory((nrcidcopy.start + (i * 4)).ToString("X")).Get<int>());
            }
        }

        private void Btnnrcidgetcvalues_Click(object sender, RoutedEventArgs e)
        {
            tbnrcidcopycs.Text = m.memory(GTA.Offsets.Editor.version, 0x0).GetAddress().ToString("X");
            tbnrcidcopyce.Text = new Global(GTA.Offsets.Editor.creator_end).GetAddress().ToString("X");
        }

        private void Btnnrcidrestore_Click(object sender, RoutedEventArgs e)
        {
            if (nrcidcopy == null)
            {
                displayScreenMessage("it seems that you havent done all steps you need to do before you can use this function.");
                return;
            }
            for (int i = 0; i < nrcidcopy.values.Count; i++)
            {
                m.memory((nrcidcopy.start + (i * 4)).ToString("X")).SetInt(nrcidcopy.values[i]);
            }
        }
    }
}
