using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Race / Checkpoints page.
    public partial class MainWindow
    {
        private void ddcpno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getCPValues();
        }

        public void getCPValues()
        {
            int index = ddcpno.SelectedIndex;
            bool primary = ddcpsno.SelectedIndex == 0;

            if (index < 0)
            {
                Btncpgetloc.IsEnabled = false;
                Btncprspcar1getloc.IsEnabled = false;
                Btncprspcar2getloc.IsEnabled = false;
                Btncprspcar3getloc.IsEnabled = false;
                tbcpslocx.IsEnabled = false;
                tbcpslocy.IsEnabled = false;
                tbcpslocz.IsEnabled = false;
                tbrspcar1x.IsEnabled = false;
                tbrspcar1y.IsEnabled = false;
                tbrspcar1z.IsEnabled = false;
                tbrspcar2x.IsEnabled = false;
                tbrspcar2y.IsEnabled = false;
                tbrspcar2z.IsEnabled = false;
                tbrspcar3x.IsEnabled = false;
                tbrspcar3y.IsEnabled = false;
                tbrspcar3z.IsEnabled = false;
                tbcpshead.IsEnabled = false;
                tbcpscpbs1.IsEnabled = false;
                tbcpscpbs2.IsEnabled = false;
                tbcpschvs.IsEnabled = false;
                tbcpssize.IsEnabled = false;
                cb_Race_CP_BigCP.IsEnabled = false;
                cb_Race_CP_Circular.IsEnabled = false;
                cb_Race_CP_Temporary.IsEnabled = false;
                cb_Race_CP_transform.IsEnabled = false;
                cb_Race_CP_Warp.IsEnabled = false;
                cbRaceCPPitStop.IsEnabled = false;
                cbRaceCPHighCP.IsEnabled = false;
                tbRaceCPHighCPrdist.IsEnabled = false;

            }
            else
            {
                Btncpgetloc.IsEnabled = true;
                Btncprspcar1getloc.IsEnabled = true;
                Btncprspcar2getloc.IsEnabled = true;
                Btncprspcar3getloc.IsEnabled = true;
                tbcpslocx.IsEnabled = true;
                tbcpslocy.IsEnabled = true;
                tbcpslocz.IsEnabled = true;
                tbrspcar1x.IsEnabled = true;
                tbrspcar1y.IsEnabled = true;
                tbrspcar1z.IsEnabled = true;
                tbrspcar2x.IsEnabled = true;
                tbrspcar2y.IsEnabled = true;
                tbrspcar2z.IsEnabled = true;
                tbrspcar3x.IsEnabled = true;
                tbrspcar3y.IsEnabled = true;
                tbrspcar3z.IsEnabled = true;
                tbcpshead.IsEnabled = true;
                tbcpscpbs1.IsEnabled = true;
                tbcpscpbs2.IsEnabled = true;
                tbcpschvs.IsEnabled = true;
                tbcpssize.IsEnabled = true;
                cb_Race_CP_BigCP.IsEnabled = true;
                cb_Race_CP_Circular.IsEnabled = true;
                cb_Race_CP_Temporary.IsEnabled = true;
                cb_Race_CP_transform.IsEnabled = true;
                cb_Race_CP_Warp.IsEnabled = true;
                cbRaceCPPitStop.IsEnabled = true;
                cbRaceCPHighCP.IsEnabled = true;
            }


            if (m.IsProcOpen && index > -1)
            {
                tbcpslocx.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.locx : GTA.Offsets.Editor.Race.Checkpoints.sndchk + 0) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbcpslocy.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.locy : GTA.Offsets.Editor.Race.Checkpoints.sndchk + 1) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbcpslocz.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.locz : GTA.Offsets.Editor.Race.Checkpoints.sndchk + 2) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbcpshead.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.chh : GTA.Offsets.Editor.Race.Checkpoints.sndrsp) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbcpschvs.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.chvs : GTA.Offsets.Editor.Race.Checkpoints.chvs) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbcpssize.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.chs : GTA.Offsets.Editor.Race.Checkpoints.chs2) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbcpscpbs1.Text = new Global(GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<int>().ToString();
                tbcpscpbs2.Text = new Global(GTA.Offsets.Editor.Race.Checkpoints.cpbs2 + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<int>().ToString();

                tbrspcar1x.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 0 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 0) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar1y.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 1 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 1) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar1z.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 2 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 2) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar2x.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 3 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 3) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar2y.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 4 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 4) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar2z.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 5 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 5) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar3x.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 6 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 6) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar3y.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 7 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 7) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();
                tbrspcar3z.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 8 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 8) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();

                int tfrmnum = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.cptfrm : GTA.Offsets.Editor.Race.Checkpoints.cptfrms) + (index * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).Get<int>();

                ddRaceCPTransform.SelectedIndex = tfrmnum;

                if (tfrmnum != -1)
                    cb_Race_CP_transform.IsChecked = true;
                else
                    cb_Race_CP_transform.IsChecked = false;

                Functions.Read.checkbinary(primary ? 2 : 3, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + index * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cb_Race_CP_Circular);
                Functions.Read.checkbinary(primary ? 28 : 29, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + index * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cb_Race_CP_Warp);
                Functions.Read.checkbinary(primary ? 11 : 12, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + index * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cb_Race_CP_Temporary);
                Functions.Read.checkbinary(primary ? 10 : 14, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + index * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cb_Race_CP_BigCP);

                Functions.Read.checkbinary(primary ? 17 : 18, GTA.Offsets.Editor.Race.Checkpoints.cpbs2 + index * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cbRaceCPPitStop);
                Functions.Read.checkbinary(primary ? 21 : 22, GTA.Offsets.Editor.Race.Checkpoints.cpbs2 + index * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cbRaceCPHighCP);
                tbRaceCPHighCPrdist.Text = new Global((primary ? GTA.Offsets.Editor.Race.Checkpoints.chstR : GTA.Offsets.Editor.Race.Checkpoints.chstRs) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * index)).Get<float>().ToString();

                SelectActiveTextBox();
            }
        }

        private void Btncpgetloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbcpslocx.Text = loc[0];
                tbcpslocy.Text = loc[1];
                tbcpslocz.Text = loc[2];
            }
        }

        private void tbcplocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.locx : GTA.Offsets.Editor.Race.Checkpoints.sndchk) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbcpslocx.Text);
        }

        private void tbcplocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.locy : GTA.Offsets.Editor.Race.Checkpoints.sndchk + 1) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbcpslocy.Text);
        }

        private void tbcplocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.locz : GTA.Offsets.Editor.Race.Checkpoints.sndchk + 2) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbcpslocz.Text);
        }

        private void tbcpshead_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.chh : GTA.Offsets.Editor.Race.Checkpoints.sndrsp) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbcpshead.Text);
        }

        private void tbcpschvs_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.chvs : GTA.Offsets.Editor.Race.Checkpoints.chvs) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbcpschvs.Text);
        }

        private void tbcpssize_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.chs : GTA.Offsets.Editor.Race.Checkpoints.chs2) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbcpssize.Text);
        }

        private void cb_Race_CP_Circular_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 2 : 3, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, true);
        }

        private void cb_Race_CP_Circular_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 2 : 3, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, false);
        }

        private void ddRaceCPTransform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.cptfrm : GTA.Offsets.Editor.Race.Checkpoints.cptfrms) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetInt(ddRaceCPTransform.SelectedIndex);
        }

        private void Btncprspcar1getloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbrspcar1x.Text = loc[0];
                tbrspcar1y.Text = loc[1];
                tbrspcar1z.Text = loc[2];
            }
        }

        private void tbrspcar1x_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn : GTA.Offsets.Editor.Race.Checkpoints.vspns) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar1x.Text);
        }

        private void tbrspcar1y_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 1 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 1) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar1y.Text);
        }

        private void tbrspcar1z_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 2 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 2) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar1z.Text);
        }

        private void Btncprspcar2getloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbrspcar2x.Text = loc[0];
                tbrspcar2y.Text = loc[1];
                tbrspcar2z.Text = loc[2];
            }
        }

        private void tbrspcar2x_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 3 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 3) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar2x.Text);
        }

        private void tbrspcar2y_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 4 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 4) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar2y.Text);
        }

        private void tbrspcar2z_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 5 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 5) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar2z.Text);
        }

        private void Btncprspcar3getloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbrspcar3x.Text = loc[0];
                tbrspcar3y.Text = loc[1];
                tbrspcar3z.Text = loc[2];
            }
        }

        private void tbrspcar3x_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 6 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 6) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar3x.Text);
        }

        private void tbrspcar3y_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 7 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 7) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar3y.Text);
        }

        private void tbrspcar3z_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.vspn + 8 : GTA.Offsets.Editor.Race.Checkpoints.vspns + 8) + GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex).SetFloat(tbrspcar3z.Text);
        }

        private void ddcpsno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddcpno.SelectedIndex > -1)
            {
                getCPValues();
            }
        }

        private void cb_Race_CP_Temporary_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 11 : 12, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, true);
        }

        private void cb_Race_CP_Warp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary((ddcpsno.SelectedIndex == 0 ? 28 : 29), GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, true);
        }

        private void cb_Race_CP_BigCP_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 10 : 14, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, true);
        }

        private void cb_Race_CP_transform_Checked(object sender, RoutedEventArgs e)
        {
            if (new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.cptfrm : GTA.Offsets.Editor.Race.Checkpoints.cptfrms) + (ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).Get<int>() == -1)
            {
                try
                {
                    ddRaceCPTransform.SelectedIndex = 0;
                }
                catch (Exception)
                {

                }
            }

        }

        private void cb_Race_CP_transform_Unchecked(object sender, RoutedEventArgs e)
        {
            ddRaceCPTransform.SelectedIndex = -1;
        }

        private void cb_Race_CP_BigCP_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 10 : 14, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, false);
        }

        private void cb_Race_CP_Warp_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary((ddcpsno.SelectedIndex == 0 ? 28 : 29), GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, false);
        }

        private void cb_Race_CP_Temporary_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 11 : 12, GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, false);
        }

        private void cbRaceCPPitStop_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 17 : 18, GTA.Offsets.Editor.Race.Checkpoints.cpbs2 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cbRaceCPPitStop.IsChecked == true);
        }

        private void tbcpscpbs1_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT).SetInt(tbcpscpbs1.Text);
        }

        private void tbcpscpbs2_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Race.Checkpoints.cpbs2 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT).SetInt(tbcpscpbs2.Text);
        }

        private void cbRaceCPHighCP_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddcpsno.SelectedIndex == 0 ? 21 : 22, GTA.Offsets.Editor.Race.Checkpoints.cpbs2 + ddcpno.SelectedIndex * GTA.Offsets.Editor.Race.Checkpoints.NEXT, cbRaceCPHighCP.IsChecked == true);
        }

        private void ddRaceCPHighCPrdist_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((ddcpsno.SelectedIndex == 0 ? GTA.Offsets.Editor.Race.Checkpoints.chstR : GTA.Offsets.Editor.Race.Checkpoints.chstRs) + (GTA.Offsets.Editor.Race.Checkpoints.NEXT * ddcpno.SelectedIndex)).SetFloat(tbRaceCPHighCPrdist.Text);
        }

        public void ChangeValuePlusMinusZeroCheck(object sender, KeyEventArgs e, Global alloffset, long offset, long offsetnext)
        {
            bool plus = (e.Key == Key.OemPlus || e.Key == Key.Add);
            bool minus = (e.Key == Key.OemMinus || e.Key == Key.Subtract);
            bool all = Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (all)
                {
                    if (plus)
                    {
                        for (int i = 0; i < alloffset.Get<int>(); i++)
                        {
                            Global loc = new Global(offset + (i * offsetnext));
                            if (loc.Get<float>() != 0)
                            {
                                loc.SetFloat(loc.Get<float>() + incrementsize);
                            }
                        }
                        (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) + incrementsize).ToString();
                    }
                    else if (minus)
                    {
                        for (int i = 0; i < alloffset.Get<int>(); i++)
                        {
                            Global loc = new Global(offset + (i * offsetnext)); if (loc.Get<float>() != 0)
                            {
                                loc.SetFloat(loc.Get<float>() - incrementsize);
                            }
                        }
                        (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) - incrementsize).ToString();
                    }
                    return;
                }
                if (plus)
                {
                    (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) + incrementsize).ToString();
                }
                else if (minus)
                {
                    (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) - incrementsize).ToString();
                }
            }
        }

        private void ChangeCPLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            if (ddcpsno.SelectedIndex == 0)
            {
                ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.locx, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar1x, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 0, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar2x, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 1, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar3x, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 2, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
            }
            else if (ddcpsno.SelectedIndex == 1)
            {
                ChangeValuePlusMinusZeroCheck(sender, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.sndchk, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar1x, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 0, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar2x, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 1, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar3x, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 2, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
            }
        }

        private void ChangeCPLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            if (ddcpsno.SelectedIndex == 0)
            {
                ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.locy, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar1y, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 3, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar2y, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 4, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar3y, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 5, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
            }
            else if (ddcpsno.SelectedIndex == 1)
            {
                ChangeValuePlusMinusZeroCheck(sender, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.sndchk + 1, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar1y, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 3, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar2y, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 4, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar3y, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 5, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
            }
        }

        private void ChangeCPLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            if (ddcpsno.SelectedIndex == 0)
            {
                ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.locz, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar1z, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 6, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar2z, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 7, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinus(tbrspcar3z, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspn + 8, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
            }
            else if (ddcpsno.SelectedIndex == 1)
            {
                ChangeValuePlusMinusZeroCheck(sender, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.sndchk + 2, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar1z, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 6, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar2z, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 7, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
                ChangeValuePlusMinusZeroCheck(tbrspcar3z, e, new Global(GTA.Offsets.Editor.Race.Checkpoints.number), GTA.Offsets.Editor.Race.Checkpoints.vspns + 8, GTA.Offsets.Editor.Race.Checkpoints.NEXT);
            }
        }
    }
}
