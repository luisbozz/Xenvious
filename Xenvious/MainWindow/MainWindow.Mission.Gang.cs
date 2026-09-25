using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / Gang page.
    public partial class MainWindow
    {
        private void cbmissionganghfm_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.irbs8 + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionganghfm);
        }

        private void ddmissiongangno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getGangValues(true);
            SelectActiveTextBox();
        }

        private void tbmissiongangv1locx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv1 + 0 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv1locx.Text);
        }

        private void tbmissiongangv1locy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv1 + 1 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv1locy.Text);
        }

        private void tbmissiongangv1locz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv1 + 2 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv1locz.Text);
        }

        private void tbmissiongangv2locx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv2 + 0 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv2locx.Text);
        }

        private void tbmissiongangv2locy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv2 + 1 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv2locy.Text);
        }

        private void tbmissiongangv2locz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv2 + 2 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv2locz.Text);
        }

        private void Btmissiongangv1getloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbmissiongangv1locx.Text = loc[0].ToString();
            tbmissiongangv1locy.Text = loc[1].ToString();
            tbmissiongangv1locz.Text = loc[2].ToString();
        }

        private void Btmissiongangv2getloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbmissiongangv2locx.Text = loc[0].ToString();
            tbmissiongangv2locy.Text = loc[1].ToString();
            tbmissiongangv2locz.Text = loc[2].ToString();
        }

        public void getGangValues(bool ignore_focus = false)
        {
            if (ddmissiongangno == null || ddmissiongangteamno == null)
                return;

            int index = ddmissiongangno.SelectedIndex;
            int tindex = ddmissiongangteamno.SelectedIndex;
            bool enable = index > -1 && tindex > -1 ? true : false;
            if (m.IsProcOpen)
            {
                Btmissiongangv1getloc.IsEnabled = enable;
                Btmissiongangv2getloc.IsEnabled = enable;
                tbmissiongangv1locx.IsEnabled = enable;
                tbmissiongangv1locy.IsEnabled = enable;
                tbmissiongangv1locz.IsEnabled = enable;
                tbmissiongangv2locx.IsEnabled = enable;
                tbmissiongangv2locy.IsEnabled = enable;
                tbmissiongangv2locz.IsEnabled = enable;
                ddmissiongangteamno.IsEnabled = enable;
                tbmissiongbnum.IsEnabled = enable;
                tbmissiongbmax.IsEnabled = enable;
                tbmissiongbdel.IsEnabled = enable;
                tbmissiongbaw.IsEnabled = enable;
                tbmissiongbfnr.IsEnabled = enable;
                tbmissiongacc.IsEnabled = enable;
                ddmissiongbcol.IsEnabled = enable;
                cbmissionganghfm.IsEnabled = enable;
                cbmissiongangnewrule.IsEnabled = enable;
                ddmissiongangtype.IsEnabled = enable;
                cbmissiongangtyperw.IsEnabled = enable;
                ddmissiongbat.IsEnabled = enable;

                if (enable)
                {
                    int gangtype = new Global(GTA.Offsets.Editor.gbtp + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>();
                    try
                    {
                        ddmissiongangtype.SelectedItem = ddmissiongangtype.ItemsSource.Cast<Gang>().ToArray().Where(y => y.Value == gangtype).First();
                    }
                    catch (Exception)
                    {
                        ddmissiongangtype.SelectedIndex = -1;
                    }

                    ddmissiongbat.SelectedIndex = new Global(GTA.Offsets.Editor.gbat + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>();

                    if (!tbmissiongangv1locx.IsFocused || ignore_focus) tbmissiongangv1locx.Text = new Global(GTA.Offsets.Editor.gbv1 + 0 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv1locy.IsFocused || ignore_focus) tbmissiongangv1locy.Text = new Global(GTA.Offsets.Editor.gbv1 + 1 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv1locz.IsFocused || ignore_focus) tbmissiongangv1locz.Text = new Global(GTA.Offsets.Editor.gbv1 + 2 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv2locx.IsFocused || ignore_focus) tbmissiongangv2locx.Text = new Global(GTA.Offsets.Editor.gbv2 + 0 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv2locy.IsFocused || ignore_focus) tbmissiongangv2locy.Text = new Global(GTA.Offsets.Editor.gbv2 + 1 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv2locz.IsFocused || ignore_focus) tbmissiongangv2locz.Text = new Global(GTA.Offsets.Editor.gbv2 + 2 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongbnum.IsFocused || ignore_focus) tbmissiongbnum.Text = new Global(GTA.Offsets.Editor.gbnum + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiongbmax.IsFocused || ignore_focus) tbmissiongbmax.Text = new Global(GTA.Offsets.Editor.gbmax + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiongbdel.IsFocused || ignore_focus) tbmissiongbdel.Text = new Global(GTA.Offsets.Editor.gbdel + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiongbaw.IsFocused || ignore_focus) tbmissiongbaw.Text = new Global(GTA.Offsets.Editor.gbaw + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongbfnr.IsFocused || ignore_focus) tbmissiongbfnr.Text = new Global(GTA.Offsets.Editor.gbfnr + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongacc.IsFocused || ignore_focus) tbmissiongacc.Text = new Global(GTA.Offsets.Editor.gacc + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!ddmissiongbcol.IsDropDownOpen || ignore_focus) ddmissiongbcol.SelectedIndex = new Global(GTA.Offsets.Editor.gbcol + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>() + 1;
                    Functions.Read.checkbinary(11, GTA.Offsets.Editor.irbs8 + index + tindex * GTA.Offsets.Editor.team_NEXT, cbmissionganghfm);
                    Functions.Read.checkbinary(17, GTA.Offsets.Editor.irbs3 + index + tindex * GTA.Offsets.Editor.team_NEXT, cbmissiongangnewrule);
                    Functions.Read.checkbinary(11, GTA.Offsets.Editor.irbs3 + index + tindex * GTA.Offsets.Editor.team_NEXT, cbmissiongangtyperw);
                }
            }
        }

        private void tbmissiongbnum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongbnum.Text))
                new Global(GTA.Offsets.Editor.gbnum + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongbnum.Text);
        }

        private void tbmissiongbmax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongbmax.Text))
                new Global(GTA.Offsets.Editor.gbmax + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongbmax.Text);
        }

        private void tbmissiongbdel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongbdel.Text))
                new Global(GTA.Offsets.Editor.gbdel + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongbdel.Text);
        }

        private void tbmissiongbaw_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbaw + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongbaw.Text);
        }

        private void ddmissiongbcol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbcol + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(ddmissiongbcol.SelectedIndex - 1);
        }

        private void ddmissiongangteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getGangValues(true);
            SelectActiveTextBox();
        }

        private void tbmissiongbfnr_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbfnr + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongbfnr.Text);
        }

        private void ddmissiongangtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissiongangtype.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.gbtp + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(((Gang)ddmissiongangtype.SelectedItem).Value);
        }

        private void cbmissiongangnewrule_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(17, GTA.Offsets.Editor.irbs3 + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissiongangnewrule);
        }

        private void cbmissiongangtyperw_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.irbs3 + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissiongangtyperw);
        }

        private void ddmissiongbat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissiongbat.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.gbat + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(ddmissiongbat.SelectedIndex);
        }

        private void tbmissiongacc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongacc.Text))
                new Global(GTA.Offsets.Editor.gacc + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongacc.Text);
        }
    }
}
