using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / TeleportMarkers page.
    public partial class MainWindow
    {
        private void tbmissiontpwalocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WA + 0 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwalocx.Text);
        }

        private void tbmissiontpwalocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WA + 1 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwalocy.Text);
        }

        private void tbmissiontpwalocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WA + 2 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwalocz.Text);
        }

        private void Btmissiontpwagetloc_Click(object sender, RoutedEventArgs e)
        {
            var temp = Functions.Read.getlocation();

            tbmissiontpwalocx.Text = temp[0].ToString();
            tbmissiontpwalocy.Text = temp[1].ToString();
            tbmissiontpwalocz.Text = temp[2].ToString();
        }

        private void ddmissiontpteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddmissiontpteamno != null)
            {
                tbmissiontpwF.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WF + ddmissiontpteamno.SelectedIndex + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>().ToString();
                tbmissiontpwG.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WG + ddmissiontpteamno.SelectedIndex + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>().ToString();
            }
            SelectActiveTextBox();
        }

        private void tbmissiontpwc_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WC + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwc.Text);
        }

        private void tbmissiontpwh_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WH + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwh.Text);
        }

        private void tbmissiontpwe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontpwe.Text))
                new Global(GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetInt(tbmissiontpwe.Text);
        }

        private void ddmissiontpcolor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ddmissiontpcolor.SelectedIndex;
            if (index > -1)
            {
                new Global(GTA.Offsets.Editor.Teleport_Marker.WAj + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetInt(index);
            }
        }

        private void tbmissiontpwAk_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WAk + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwAk.Text);
        }

        private void tbmissiontpwF_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontpwF.Text))
                new Global(GTA.Offsets.Editor.Teleport_Marker.WF + ddmissiontpteamno.SelectedIndex + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetInt(tbmissiontpwF.Text);
        }

        private void tbmissiontpwG_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontpwG.Text))
                new Global(GTA.Offsets.Editor.Teleport_Marker.WG + ddmissiontpteamno.SelectedIndex + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetInt(tbmissiontpwG.Text);
        }


        public void GetTPMValues(bool ignore_focus = false)
        {
            if (!m.IsProcOpen)
                return;
            if (ddmissiontpno == null)
                return;

            int tindex = ddmissiontpteamno.SelectedIndex;
            int index = ddmissiontpno.SelectedIndex;

            bool enabled = index > -1;

            Btmissiontpwagetloc.IsEnabled = enabled ? true : false;
            Btmissiontpwkgetloc.IsEnabled = enabled ? true : false;
            Btmissiontpwmgetloc.IsEnabled = enabled ? true : false;
            ddmissiontpteamno.IsEnabled = enabled ? true : false;
            ddmissiontpenteranimationwj.IsEnabled = enabled ? true : false;
            tbmissiontpwalocx.IsEnabled = enabled ? true : false;
            tbmissiontpwalocy.IsEnabled = enabled ? true : false;
            tbmissiontpwalocz.IsEnabled = enabled ? true : false;
            tbmissiontpwklocx.IsEnabled = enabled ? true : false;
            tbmissiontpwklocy.IsEnabled = enabled ? true : false;
            tbmissiontpwklocz.IsEnabled = enabled ? true : false;
            tbmissiontpwmlocx.IsEnabled = enabled ? true : false;
            tbmissiontpwmlocy.IsEnabled = enabled ? true : false;
            tbmissiontpwmlocz.IsEnabled = enabled ? true : false;
            tbmissiontpwc.IsEnabled = enabled ? true : false;
            tbmissiontpwe.IsEnabled = enabled ? true : false;
            ddmissiontpcolor.IsEnabled = enabled ? true : false;
            tbmissiontpwAk.IsEnabled = enabled ? true : false;
            tbmissiontpwF.IsEnabled = enabled ? true : false;
            tbmissiontpwG.IsEnabled = enabled ? true : false;
            tbmissiontpwh.IsEnabled = enabled ? true : false;
            tbmissiontpwl.IsEnabled = enabled ? true : false;
            tbmissiontpwnx.IsEnabled = enabled ? true : false;
            tbmissiontpwny.IsEnabled = enabled ? true : false;
            tbmissiontpwnz.IsEnabled = enabled ? true : false;
            tbmissiontpwo.IsEnabled = enabled ? true : false;
            cbmissiontphidetpatp.IsEnabled = enabled ? true : false;
            cbmissiontpmarkonmap.IsEnabled = enabled ? true : false;
            cbmissiontpdtphin.IsEnabled = enabled ? true : false;
            cbmissiontpdtphin2.IsEnabled = enabled ? true : false;
            cbmissiontphidetp1.IsEnabled = enabled ? true : false;
            cbmissiontpe2e.IsEnabled = enabled ? true : false;
            cbmissiontpcamanimleft.IsEnabled = enabled ? true : false;
            cbmissiontpls.IsEnabled = enabled ? true : false;
            cbmissiontpwv.IsEnabled = enabled ? true : false;
            tbmissiontpbind.IsEnabled = enabled ? true : false;

            if (enabled)
            {
                if (!tbmissiontpbind.IsFocused || ignore_focus) tbmissiontpbind.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WAz + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>().ToString();
                if (!tbmissiontpwalocx.IsFocused || ignore_focus) tbmissiontpwalocx.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WA + 0 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwalocy.IsFocused || ignore_focus) tbmissiontpwalocy.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WA + 1 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwalocz.IsFocused || ignore_focus) tbmissiontpwalocz.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WA + 2 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwh.IsFocused || ignore_focus) tbmissiontpwh.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WH + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwc.IsFocused || ignore_focus) tbmissiontpwc.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WC + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwe.IsFocused || ignore_focus) tbmissiontpwe.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>().ToString();
                if (!tbmissiontpwF.IsFocused || ignore_focus) tbmissiontpwF.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WF + tindex + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>().ToString();
                if (!tbmissiontpwG.IsFocused || ignore_focus) tbmissiontpwG.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WG + tindex + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>().ToString();
                if (!tbmissiontpwAk.IsFocused || ignore_focus) tbmissiontpwAk.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WAk + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();

                int anim = new Global(GTA.Offsets.Editor.Teleport_Marker.WJ + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>() + 1;

                //error check
                if (!(anim < -1))
                {
                    if (!ddmissiontpenteranimationwj.IsFocused || ignore_focus) ddmissiontpenteranimationwj.SelectedIndex = anim;
                } // check if anim is higher than highest animation
                else if (anim > 9)
                {
                    ddmissiontpenteranimationwj.SelectedIndex = -1;
                }

                int color = new Global(GTA.Offsets.Editor.Teleport_Marker.WAj + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>();

                //error check
                if (!(color < -1))
                {
                    if (!ddmissiontpcolor.IsFocused || ignore_focus) ddmissiontpcolor.SelectedIndex = color;
                } // check if color is higher than highest color
                else if (color > 10)
                {
                    ddmissiontpcolor.SelectedIndex = -1;
                }


                if (!tbmissiontpwklocx.IsFocused || ignore_focus) tbmissiontpwklocx.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WK + 0 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwklocy.IsFocused || ignore_focus) tbmissiontpwklocy.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WK + 1 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwklocz.IsFocused || ignore_focus) tbmissiontpwklocz.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WK + 2 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwl.IsFocused || ignore_focus) tbmissiontpwl.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WL + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwmlocx.IsFocused || ignore_focus) tbmissiontpwmlocx.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WM + 0 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwmlocy.IsFocused || ignore_focus) tbmissiontpwmlocy.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WM + 1 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwmlocz.IsFocused || ignore_focus) tbmissiontpwmlocz.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WM + 2 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwnx.IsFocused || ignore_focus) tbmissiontpwnx.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WN + 0 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwny.IsFocused || ignore_focus) tbmissiontpwny.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WN + 1 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwnz.IsFocused || ignore_focus) tbmissiontpwnz.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WN + 2 + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();
                if (!tbmissiontpwo.IsFocused || ignore_focus) tbmissiontpwo.Text = new Global(GTA.Offsets.Editor.Teleport_Marker.WO + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<float>().ToString();

                //if (!cbmissiontpenable.IsFocused || ignore_focus) cbmissiontpenable.IsChecked = new Global(GTA.Offsets.Editor.Teleport_Marker.WAz + index * GTA.Offsets.Editor.Teleport_Marker.NEXT).Get<int>() == 0 ? true : false;
                if (!cbmissiontpmarkonmap.IsFocused || ignore_focus) Functions.Read.checkbinary(4, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpmarkonmap);
                if (!cbmissiontphidetpatp.IsFocused || ignore_focus) Functions.Read.checkbinary(5, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontphidetpatp);
                if (!cbmissiontpdtphin.IsFocused || ignore_focus) Functions.Read.checkbinary(10, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpdtphin);
                if (!cbmissiontpdtphin2.IsFocused || ignore_focus) Functions.Read.checkbinary(12, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpdtphin2);
                if (!cbmissiontphidetp1.IsFocused || ignore_focus) Functions.Read.checkbinary(15, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontphidetp1);
                if (!cbmissiontpe2e.IsFocused || ignore_focus) Functions.Read.checkbinary(9, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpe2e);
                if (!cbmissiontpcamanimleft.IsFocused || ignore_focus) Functions.Read.checkbinary(20, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpcamanimleft);
                if (!cbmissiontpls.IsFocused || ignore_focus) Functions.Read.checkbinary(1, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpls);


                bool purpleblur = Functions.Read.checkbinary(2, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT);
                bool fade = Functions.Read.checkbinary(26, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT);
                if (!purpleblur && fade) //if purple blue is enabled the fade animation wont work - thats why we dont need to check if its even enabled bitwise
                {
                    cbmissiontpfade.IsEnabled = true;
                    cbmissiontpfade.IsChecked = true;
                    cbmissiontppb.IsEnabled = false;
                }
                else if (!purpleblur && !fade)
                {
                    cbmissiontpfade.IsEnabled = true;
                    cbmissiontppb.IsEnabled = true;
                    cbmissiontpfade.IsChecked = false;
                    cbmissiontppb.IsChecked = false;
                }
                else
                {
                    cbmissiontppb.IsEnabled = true;
                    cbmissiontppb.IsChecked = true;
                    cbmissiontpfade.IsEnabled = false;
                    cbmissiontpfade.IsChecked = false;
                }
                if (!cbmissiontpwv.IsFocused || ignore_focus) Functions.Read.checkbinary(6, GTA.Offsets.Editor.Teleport_Marker.WE + index * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpwv);
            }
        }

        private void ddmissiontpno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTPMValues(true);
            SelectActiveTextBox();
        }

        private void cbmissiontpmarkonmap_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpmarkonmap);
        }

        private void cbmissiontphidetpatp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(5, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontphidetpatp);
        }

        private void cbmissiontphidetp1_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontphidetp1);
        }

        private void cbmissiontpe2e_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(9, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpe2e);
        }

        private void cbmissiontpdtphin_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpdtphin);
        }

        private void cbmissiontpdtphin2_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpdtphin2);
        }

        private void ddmissiontpenteranimationwj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ddmissiontpenteranimationwj.SelectedIndex;
            if (index > -1)
            {
                new Global(GTA.Offsets.Editor.Teleport_Marker.WJ + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetInt(ddmissiontpenteranimationwj.SelectedIndex - 1);

                if (index == 5 || index == 6 || index == 7)
                {
                    cbmissiontpcamanimleft.Visibility = Visibility.Visible;
                }
                else
                {
                    cbmissiontpcamanimleft.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void tbmissiontpwklocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WK + 0 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwklocx.Text);
        }

        private void tbmissiontpwklocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WK + 1 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwklocy.Text);
        }

        private void tbmissiontpwklocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WK + 2 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwklocz.Text);
        }

        private void Btmissiontpwkgetloc_Click(object sender, RoutedEventArgs e)
        {
            var temp = Functions.Read.getlocation();

            tbmissiontpwklocx.Text = temp[0].ToString();
            tbmissiontpwklocy.Text = temp[1].ToString();
            tbmissiontpwklocz.Text = temp[2].ToString();
        }

        private void tbmissiontpwl_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WL + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwl.Text);
        }

        private void tbmissiontpwmlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WM + 0 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwmlocx.Text);
        }

        private void tbmissiontpwmlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WM + 1 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwmlocy.Text);
        }

        private void tbmissiontpwmlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WM + 2 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwmlocz.Text);
        }

        private void Btmissiontpwmgetloc_Click(object sender, RoutedEventArgs e)
        {
            var temp = Functions.Read.getlocation();

            tbmissiontpwmlocx.Text = temp[0].ToString();
            tbmissiontpwmlocy.Text = temp[1].ToString();
            tbmissiontpwmlocz.Text = temp[2].ToString();
        }

        private void tbmissiontpwnx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WN + 0 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwnx.Text);
        }

        private void cbmissiontpcamanimleft_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpcamanimleft);
        }

        private void cbmissiontpls_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpls);
        }

        private void cbmissiontppb_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontppb);
            try
            {
                if (cbmissiontppb.IsChecked == true)
                {
                    cbmissiontpfade.IsEnabled = false;
                }
                else
                {
                    cbmissiontpfade.IsEnabled = true;
                }
            }
            catch (Exception)
            {

            }
        }

        private void tbmissiontpwo_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WO + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwo.Text);
        }

        private void cbmissiontpwv_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(6, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpwv);
        }

        private void cbmissiontpfade_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(26, GTA.Offsets.Editor.Teleport_Marker.WE + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT, cbmissiontpfade);
            try
            {
                if (cbmissiontpfade.IsChecked == true)
                {
                    cbmissiontppb.IsEnabled = false;
                }
                else
                {
                    cbmissiontppb.IsEnabled = true;
                }
            }
            catch (Exception)
            {

            }
        }

        private void tbmissiontpwny_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WN + 1 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwny.Text);
        }

        private void tbmissiontpwnz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WN + 2 + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetFloat(tbmissiontpwnz.Text);
        }

        private void tbmissiontpbind_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Teleport_Marker.WAz + ddmissiontpno.SelectedIndex * GTA.Offsets.Editor.Teleport_Marker.NEXT).SetInt(tbmissiontpbind.Text);
        }
    }
}
