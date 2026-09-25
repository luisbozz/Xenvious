using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / OtZone page.
    public partial class MainWindow
    {
        private void ddmissionotzoneno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetOtzone(true);
        }

        public void GetOtzone(bool ignore_focus = false)
        {
            if (!m.IsProcOpen)
                return;
            if (ddplyrteam == null || ddplyrno == null)
                return;

            int index = ddmissionotzoneno.SelectedIndex;

            bool enabled = !(index == -1);

            if (!enabled)
            {
                tbotzoneno.IsEnabled = false;
                tbmissionotzoneotvox.IsEnabled = false;
                tbmissionotzoneotvoy.IsEnabled = false;
                tbmissionotzoneotvoz.IsEnabled = false;
                tbmissionotzoneotvtx.IsEnabled = false;
                tbmissionotzoneotvty.IsEnabled = false;
                tbmissionotzoneotvtz.IsEnabled = false;
                tbmissionotzonebits.IsEnabled = false;
                tbmissionotzoneotpg.IsEnabled = false;
                tbmissionotzoneotpl.IsEnabled = false;
                tbmissionotzoneotrw.IsEnabled = false;
                Btmissionotzoneotvogetloc.IsEnabled = false;
                Btmissionotzoneotvtgetloc.IsEnabled = false;
                return;
            }
            else
            {
                tbotzoneno.IsEnabled = true;
                tbmissionotzoneotvox.IsEnabled = true;
                tbmissionotzoneotvoy.IsEnabled = true;
                tbmissionotzoneotvoz.IsEnabled = true;
                tbmissionotzoneotvtx.IsEnabled = true;
                tbmissionotzoneotvty.IsEnabled = true;
                tbmissionotzoneotvtz.IsEnabled = true;
                tbmissionotzonebits.IsEnabled = true;
                tbmissionotzoneotpg.IsEnabled = true;
                tbmissionotzoneotpl.IsEnabled = true;
                tbmissionotzoneotrw.IsEnabled = true;
                Btmissionotzoneotvogetloc.IsEnabled = true;
                Btmissionotzoneotvtgetloc.IsEnabled = true;
            }

            //get otzone values
            if (!tbotzoneno.IsFocused || ignore_focus) tbotzoneno.Text = new Global(GTA.Offsets.Editor.otzone.number).Get<int>().ToString();
            if (!tbmissionotzoneotvox.IsFocused || ignore_focus) tbmissionotzoneotvox.Text = new Global(GTA.Offsets.Editor.otzone.otvo + 0 + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<float>().ToString();
            if (!tbmissionotzoneotvox.IsFocused || ignore_focus) tbmissionotzoneotvoy.Text = new Global(GTA.Offsets.Editor.otzone.otvo + 1 + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<float>().ToString();
            if (!tbmissionotzoneotvox.IsFocused || ignore_focus) tbmissionotzoneotvoz.Text = new Global(GTA.Offsets.Editor.otzone.otvo + 2 + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<float>().ToString();
            if (!tbmissionotzoneotvtx.IsFocused || ignore_focus) tbmissionotzoneotvtx.Text = new Global(GTA.Offsets.Editor.otzone.otvt + 0 + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<float>().ToString();
            if (!tbmissionotzoneotvtx.IsFocused || ignore_focus) tbmissionotzoneotvty.Text = new Global(GTA.Offsets.Editor.otzone.otvt + 1 + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<float>().ToString();
            if (!tbmissionotzoneotvtx.IsFocused || ignore_focus) tbmissionotzoneotvtz.Text = new Global(GTA.Offsets.Editor.otzone.otvt + 2 + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<float>().ToString();
            if (!tbmissionotzoneotrw.IsFocused || ignore_focus) tbmissionotzoneotrw.Text = new Global(GTA.Offsets.Editor.otzone.otrw + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<float>().ToString();
            if (!tbmissionotzonebits.IsFocused || ignore_focus) tbmissionotzonebits.Text = new Global(GTA.Offsets.Editor.otzone.otbs + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<int>().ToString();
            if (!tbmissionotzoneotpg.IsFocused || ignore_focus) tbmissionotzoneotpg.Text = new Global(GTA.Offsets.Editor.otzone.otpg + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<int>().ToString();
            if (!tbmissionotzoneotpl.IsFocused || ignore_focus) tbmissionotzoneotpl.Text = new Global(GTA.Offsets.Editor.otzone.otpl + (index * GTA.Offsets.Editor.otzone.NEXT)).Get<int>().ToString();

        }

        private void tbmissionotzoneotvox_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.otvo + 0 + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetFloat(tbmissionotzoneotvox.Text);
        }

        private void tbmissionotzoneotvoy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.otvo + 1 + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetFloat(tbmissionotzoneotvoy.Text);
        }

        private void tbmissionotzoneotvoz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.otvo + 2 + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetFloat(tbmissionotzoneotvoz.Text);
        }

        private void Btmissionotzoneotvogetloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbmissionotzoneotvox.Text = loc[0];
                tbmissionotzoneotvoy.Text = loc[1];
                tbmissionotzoneotvoz.Text = loc[2];
            }
        }

        private void tbmissionotzoneotvtx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.otvt + 0 + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetFloat(tbmissionotzoneotvtx.Text);
        }

        private void tbmissionotzoneotvty_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.otvt + 1 + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetFloat(tbmissionotzoneotvty.Text);
        }

        private void tbmissionotzoneotvtz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.otvt + 2 + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetFloat(tbmissionotzoneotvtz.Text);
        }

        private void Btmissionotzoneotvtgetloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbmissionotzoneotvtx.Text = loc[0];
                tbmissionotzoneotvty.Text = loc[1];
                tbmissionotzoneotvtz.Text = loc[2];
            }
        }

        private void tbmissionotzoneotrw_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.otrw + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetFloat(tbmissionotzoneotrw.Text);
        }

        private void tbotzoneno_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.otzone.number).SetInt(tbotzoneno.Text);
        }

        private void tbmissionotzonebits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionotzonebits.Text))
            {
                new Global(GTA.Offsets.Editor.otzone.otbs + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetInt(tbmissionotzonebits.Text);
            }
        }

        private void tbmissionotzoneotpg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionotzoneotpg.Text))
            {
                new Global(GTA.Offsets.Editor.otzone.otpg + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetInt(tbmissionotzoneotpg.Text);
            }
        }

        private void tbmissionotzoneotpl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionotzoneotpl.Text))
            {
                new Global(GTA.Offsets.Editor.otzone.otpl + (ddmissionotzoneno.SelectedIndex * GTA.Offsets.Editor.otzone.NEXT)).SetInt(tbmissionotzoneotpl.Text);
            }
        }
    }
}
