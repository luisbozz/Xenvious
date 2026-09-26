using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / General page.
    public partial class MainWindow
    {
        public static int xprfreeze = 10;
        public Thread freezeXPRThread;

        private void BtnMissionlobbycamgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.GetLocationVec();

            tblobbycamlocx.Text = loc.X.ToString();
            tblobbycamlocy.Text = loc.Y.ToString();
            tblobbycamlocz.Text = loc.Z.ToString();
        }

        private void tblobbycamlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tblobbycamlocx.Text))
                new Global(GTA.Offsets.Editor.cam + 0).SetFloat(tblobbycamlocx.Text);
        }

        private void tblobbycamlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tblobbycamlocy.Text))
                new Global(GTA.Offsets.Editor.cam + 1).SetFloat(tblobbycamlocy.Text);
        }

        private void tblobbycamlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tblobbycamlocz.Text))
                new Global(GTA.Offsets.Editor.cam + 2).SetFloat(tblobbycamlocz.Text);
        }

        private void BtnMissionStartgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.GetLocationVec();

            tbstartlocx.Text = loc.X.ToString();
            tbstartlocy.Text = loc.Y.ToString();
            tbstartlocz.Text = loc.Z.ToString();
        }

        private void tbstartlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbstartlocx.Text))
                new Global(GTA.Offsets.Editor.start).SetFloat(tbstartlocx.Text);
        }

        private void tbstartlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbstartlocy.Text))
                new Global(GTA.Offsets.Editor.start + 1).SetFloat(tbstartlocy.Text);
        }

        private void tbstartlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbstartlocz.Text))
                new Global(GTA.Offsets.Editor.start + 2).SetFloat(tbstartlocz.Text);
        }

        private void BtnMissioncamfgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.GetLocationVec();

            tbcamflocx.Text = loc.X.ToString();
            tbcamflocy.Text = loc.Y.ToString();
            tbcamflocz.Text = loc.Z.ToString();
        }

        private void tbcamflocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbcamflocx.Text))
                new Global(GTA.Offsets.Editor.camf + 0).SetFloat(tbcamflocx.Text);
        }

        private void tbcamflocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbcamflocy.Text))
                new Global(GTA.Offsets.Editor.camf + 1).SetFloat(tbcamflocy.Text);
        }

        private void tbcamflocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbcamflocz.Text))
                new Global(GTA.Offsets.Editor.camf + 2).SetFloat(tbcamflocz.Text);
        }

        private void tbMissionltm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionltm.Text) && IsValidInt(tbMissionltm.Text))
                new Global(GTA.Offsets.Editor.ltm).SetInt(tbMissionltm.Text);
        }

        private void tbMissioninumbnc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissioninumbnc.Text) && IsValidInt(tbMissioninumbnc.Text))
                new Global(GTA.Offsets.Editor.inumbnc).SetInt(tbMissioninumbnc.Text);
        }

        private void tbMissionmrd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionmrd.Text) && IsValidInt(tbMissionmrd.Text))
                new Global(GTA.Offsets.Editor.mrd).SetInt(tbMissionmrd.Text);
        }

        private void tbMissioncshr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissioncshr.Text) && IsValidInt(tbMissioncshr.Text))
                new Global(GTA.Offsets.Editor.cshr).SetInt(tbMissioncshr.Text);
        }

        private void tbMissiontrel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissiontrel.Text) && IsValidInt(tbMissiontrel.Text))
                new Global(GTA.Offsets.Editor.trel).SetInt(tbMissiontrel.Text);
        }

        private void tbMissionrlopt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionrlopt.Text) && IsValidInt(tbMissionrlopt.Text))
                new Global(GTA.Offsets.Editor.rlopt).SetInt(tbMissionrlopt.Text);
        }

        private void tbMissionctsc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionctsc.Text) && IsValidInt(tbMissionctsc.Text))
                new Global(GTA.Offsets.Editor.ctsc).SetInt(tbMissionctsc.Text);
        }

        private void tbMissionadverm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionadverm.Text) && IsValidInt(tbMissionadverm.Text))
                new Global(GTA.Offsets.Editor.adverm).SetInt(tbMissionadverm.Text);
        }

        private void tbMissiontwrst_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissiontwrst.Text) && IsValidInt(tbMissiontwrst.Text))
                new Global(GTA.Offsets.Editor.twrst).SetInt(tbMissiontwrst.Text);
        }

        private void tbMissionvsbsout_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionvsbsout.Text) && IsValidInt(tbMissionvsbsout.Text))
                new Global(GTA.Offsets.Editor.vsbsout).SetInt(tbMissionvsbsout.Text);
        }

        private void tbMissionvsdfstc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionvsdfstc.Text) && IsValidInt(tbMissionvsdfstc.Text))
                new Global(GTA.Offsets.Editor.vsdfstc).SetInt(tbMissionvsdfstc.Text);
        }

        private void tbMissionteambal_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionteambal.Text) && IsValidInt(tbMissionteambal.Text))
                new Global(GTA.Offsets.Editor.teambal).SetInt(tbMissionteambal.Text);
        }

        private void cbmissionenableprisonalarms_Checked(object sender, RoutedEventArgs e)
        {
            if (cbmissionenableprisonalarms.IsChecked == true)
                Functions.Write.writebinary(32, GTA.Offsets.Editor.menubs7, false);
            Functions.Write.writebinary(28, GTA.Offsets.Editor.menubs, cbmissionenableprisonalarms);
        }

        private void cbmissionremweap_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.menubs, cbmissionremweap);
        }

        private void cbmissionspawnunarmed_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(28, GTA.Offsets.Editor.menubs2, cbmissionspawnunarmed);
            Functions.Write.writebinary(21, GTA.Offsets.Editor.menubs25, cbmissionspawnunarmed);
        }

        private void cbmissionbulletproofplayers_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs11, cbmissionbulletproofplayers);
        }

        private void cbmissionwlad_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.menubs, cbmissionwlad);
        }

        private void tbMissionfsr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionfsr.Text))
                new Global(GTA.Offsets.Editor.fiispr).SetFloat(tbMissionfsr.Text);
        }

        private void ddweaponrsptime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.Weapon.time).SetInt(ddweaponrsptime.SelectedIndex);
        }

        private void tbMissionxpr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbMissionxpr.Text) && IsValidInt(tbMissionxpr.Text))
            {
                int.TryParse(tbMissionxpr.Text, out xprfreeze);
                new Global(GTA.Offsets.Editor.xpr).SetInt(xprfreeze);
            }
        }

        private void cboloarmor_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs3, cboloarmor);
        }

        private void cboloweapon_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.menubs2, cboloweapon);
        }

        private void cbolohideweth_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(16, GTA.Offsets.Editor.menubs4, cbolohideweth);
        }

        private void cboloroundtime_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(23, GTA.Offsets.Editor.menubs8, cboloroundtime);
        }

        private void cbologametype_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.menubs9, cbologametype);
        }

        private void cbolosuddendeath_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs3, cbolosuddendeath);
        }

        private void cbolohidetod_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.menubs4, cbolohidetod);
        }

        private void cbololockvehicleclass_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.menubs5, cbololockvehicleclass);
        }

        private void cbolonumberofteams_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.menubs5, cbolonumberofteams);
        }

        private void cboloci_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(21, GTA.Offsets.Editor.menubs6, cboloci);
        }

        private void cbolotargetscore_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(9, GTA.Offsets.Editor.menubs8, cbolotargetscore);
        }

        private void cbolorounds_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.menubs10, cbolorounds);
        }

        private void cbolokm_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.menubs10, cbolokm);
        }

        private void cbologametype2_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.menubs10, cbologametype2);
        }

        private void cbolopu_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.menubs13, cbolopu);
        }

        private void cbolovehlist_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.menubs18, cbolovehlist);
        }

        private void cbolodisableblips_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(26, GTA.Offsets.Editor.menubs2, cbolodisableblips);
        }

        private void tbMissiondlcrel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                if (IsValidInt(tbMissiondlcrel.Text))
                    new Global(GTA.Offsets.Editor.dlcrel).SetInt(tbMissiondlcrel.Text);
            }
        }

        private void cbmissionteambalancing_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.menubs2, cbmissionteambalancing);
        }

        private void tbMissiondtmp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissiondtmp.Text))
                new Global(GTA.Offsets.Editor.dtmp).SetInt(tbMissiondtmp.Text);
        }

        private void tbMissiondtmp2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissiondtmp2.Text))
                new Global(GTA.Offsets.Editor.dtmp2).SetInt(tbMissiondtmp2.Text);
        }

        private void tbMissiondtmp3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissiondtmp3.Text))
                new Global(GTA.Offsets.Editor.dtmp3).SetInt(tbMissiondtmp3.Text);
        }

        private void cbmissionaddflash_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.menubs27, cbmissionaddflash);
        }

        private void tbMissionchksfx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissionchksfx.Text))
                new Global(GTA.Offsets.Editor.chksfx).SetInt(tbMissionchksfx.Text);
        }

        private void tbMissionalrtLocal_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissionalrtLocal.Text))
                new Global(GTA.Offsets.Editor.chksfx).SetInt(tbMissionalrtLocal.Text);
        }

        private void tbmissionnrcid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.nrcid + ddmissionnrcidno.SelectedIndex * 0x30).SetString(tbmissionnrcid.Text);
        }

        private void tbmissionnrmtt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.nrmtt + ddmissionnrcidno.SelectedIndex).SetInt(tbmissionnrmtt.Text);
        }

        private void ddmissionnrcidno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getNRCIDValues(true);
        }

        public void getNRCIDValues(bool ignore_focus = false)
        {
            if (m.IsProcOpen && ddmissionnrcidno.SelectedIndex > -1)
            {
                if (!tbmissionnrcid.IsFocused || ignore_focus) tbmissionnrcid.Text = new Global(GTA.Offsets.Editor.nrcid + ddmissionnrcidno.SelectedIndex * 0x30).GetString();
                if (!tbmissionnrmtt.IsFocused || ignore_focus) tbmissionnrmtt.Text = new Global(GTA.Offsets.Editor.nrmtt + ddmissionnrcidno.SelectedIndex).Get<int>().ToString();
                if (ignore_focus)
                    SelectActiveTextBox();
            }
        }

        public static void freezeXPRAll()
        {
            while (true)
            {
                new Global(GTA.Offsets.Editor.xpr).SetInt(xprfreeze);
            }

        }
        private void cbMissionxprfreeze_Checked(object sender, RoutedEventArgs e)
        {
            bool ischecked = cbMissionxprfreeze.IsChecked ?? true;
            if (ischecked)
            {
                freezeXPRThread = new Thread(new ThreadStart(freezeXPRAll));
                freezeXPRThread.Priority = ThreadPriority.Highest;
                freezeXPRThread.IsBackground = true;
                freezeXPRThread.Start();
            }
            else
            {
                freezeXPRThread.Abort();
            }
        }

        private void cbmissiondisspeccam_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(30, GTA.Offsets.Editor.menubs3, cbmissiondisspeccam);
        }

        private void cbmissiondisjipspec_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(5, GTA.Offsets.Editor.menubs26, cbmissiondisjipspec);
        }

        private void cbmissiondislsc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(27, GTA.Offsets.Editor.menubs3, cbmissiondislsc);
        }

        private void cbmissionptod_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(21, GTA.Offsets.Editor.menubs2, cbmissionptod);
        }

        private void cbmissionplaycm_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.menubs2, cbmissionplaycm);
        }

        // pol: 0 = normal police, 1 = no police, 2..6 = at most 1..5 stars. The
        // dropdown lists them in that order.
        private void ddmissionmaxwl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissionmaxwl.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.pol).SetInt(ddmissionmaxwl.SelectedIndex);
        }

        // traf and apeds store an index that the mission controller turns into a
        // density: 0 = 0, 1 = 0.2, 2 = 0.5, 3 = 1.0, 4 = 0.85. The creator offers
        // these five; the dropdowns list them from low to high.
        private static readonly int[] MissionDensityIndex = { 0, 1, 2, 4, 3 };

        private void ddmissiontraffic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetMissionDensity(ddmissiontraffic, GTA.Offsets.Editor.traf);
        }

        private void ddmissionpeds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetMissionDensity(ddmissionpeds, GTA.Offsets.Editor.apeds);
        }

        private void SetMissionDensity(ComboBox box, long offset)
        {
            if (box.SelectedIndex > -1 && offset != 0)
                new Global(offset).SetInt(MissionDensityIndex[box.SelectedIndex]);
        }

        private void GetMissionDensity(ComboBox box, long offset)
        {
            if (offset == 0 || box.IsDropDownOpen)
                return;
            box.SelectedIndex = Array.IndexOf(MissionDensityIndex, new Global(offset).Get<int>());
        }

        private void tbMissionendtype_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissionendtype.Text))
                new Global(GTA.Offsets.Editor.endtype).SetInt(tbMissionendtype.Text);
        }

        private void cbmissiondispwd_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(6, GTA.Offsets.Editor.twrst, cbmissiondispwd);
        }

        private void cbmissionactorremarmor_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs12, cbmissionactorremarmor);
        }

        private void cbmissionjlwnp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(17, GTA.Offsets.Editor.trel, cbmissionjlwnp);
        }

        private void cbmissiontrel12_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(21, GTA.Offsets.Editor.trel, cbmissiontrel12);
        }
        private void cbmissiontrel13_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(22, GTA.Offsets.Editor.trel, cbmissiontrel13);
        }
        private void cbmissiontrel14_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(23, GTA.Offsets.Editor.trel, cbmissiontrel14);
        }
        private void cbmissiontrel23_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(24, GTA.Offsets.Editor.trel, cbmissiontrel23);
        }
        private void cbmissiontrel24_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(25, GTA.Offsets.Editor.trel, cbmissiontrel24);
        }
        private void cbmissiontrel34_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(26, GTA.Offsets.Editor.trel, cbmissiontrel34);
        }

        private void ddmissionplyrlm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissionplyrlm != null)
            {
                if (ddmissionplyrlm.SelectedIndex == 0)
                {
                    Functions.Write.writebinary(1, GTA.Offsets.Editor.menubs, false);
                    Functions.Write.writebinary(6, GTA.Offsets.Editor.menubs, false);
                }
                else if (ddmissionplyrlm.SelectedIndex == 1)
                {
                    Functions.Write.writebinary(1, GTA.Offsets.Editor.menubs, true);
                    Functions.Write.writebinary(6, GTA.Offsets.Editor.menubs, false);
                }
                else if (ddmissionplyrlm.SelectedIndex == 2)
                {
                    Functions.Write.writebinary(1, GTA.Offsets.Editor.menubs, false);
                    Functions.Write.writebinary(6, GTA.Offsets.Editor.menubs, true);
                }
            }
        }

        private void cbmissiondwd_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.twrst, cbmissiondwd);
        }

        private void cbmissionrwbs1_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.twrst, cbmissionrwbs1);
        }

        private void cbmissionrwbs2_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.twrst, cbmissionrwbs2);
        }

        private void cbmissionrwbs3_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.twrst, cbmissionrwbs3);
        }

        private void cbmissionrwbs4_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(5, GTA.Offsets.Editor.twrst, cbmissionrwbs4);
        }

        private void cbmissionedw_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(6, GTA.Offsets.Editor.menubs9, cbmissionedw);
        }

        private void cbmissionemp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(25, GTA.Offsets.Editor.menubs24, cbmissionemp);
        }

        private void cbmissionempnm_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(9, GTA.Offsets.Editor.menubs25, cbmissionempnm);
        }

        private void tbmissionempdura_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionempdura.Text, false))
                new Global(GTA.Offsets.Editor.pnEMPd).SetInt(tbmissionempdura.Text);
        }

        private void tbmissionemppsr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionemppsr.Text, false))
                new Global(GTA.Offsets.Editor.pnEMPp).SetInt(tbmissionemppsr.Text);
        }
    }
}
