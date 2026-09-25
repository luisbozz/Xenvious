using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Fixtures page.
    public partial class MainWindow
    {
        private void ddcentityno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetFixtureValues(true);
            SelectActiveTextBox();
        }

        public void GetFixtureValues(bool ignore_focus = false)
        {
            int index = ddcentityno.SelectedIndex;

            tbcdefbulk.IsEnabled = index < 0 ? false : true;
            cbcdefhide.IsEnabled = index < 0 ? false : true;
            cbcdeffreeze.IsEnabled = index < 0 ? false : true;
            cbcdefmissionent.IsEnabled = index < 0 ? false : true;
            tbcdefmodel.IsEnabled = index < 0 ? false : true;
            tbcdefbits.IsEnabled = index < 0 ? false : true;
            tbcdefmnswap.IsEnabled = index < 0 ? false : true;
            tbcdefwprad.IsEnabled = index < 0 ? false : true;
            tbcdeflocx.IsEnabled = index < 0 ? false : true;
            tbcdeflocy.IsEnabled = index < 0 ? false : true;
            tbcdeflocz.IsEnabled = index < 0 ? false : true;
            cbcdefadvanced.IsEnabled = index < 0 ? false : true;
            Btncdefgetmodel.IsEnabled = index < 0 ? false : true;
            Btncdefgetloc.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                int model = new Global(index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.model).Get<int>();
                float locx = new Global(index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.locx).Get<float>();
                float locy = new Global(index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.locy).Get<float>();
                float locz = new Global(index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.locz).Get<float>();
                string native = "";

                if (!tbcdefbulk.IsFocused || ignore_focus)
                {
                    try
                    {
                        native = GTA.Editor.PropList.Where(x => x.Integer == model).FirstOrDefault().Native;
                        tbcdefbulk.Text = $"{native}: 99999: X:{locx} Y:{locy} Z:{locz}";
                    }
                    catch (Exception)
                    {
                        tbcdefbulk.Text = "couldn't load the centitydef string";
                    }
                }

                if (!tbcdefmodel.IsFocused || ignore_focus) tbcdefmodel.Text = model.ToString();
                if (!tbcdeflocx.IsFocused || ignore_focus) tbcdeflocx.Text = locx.ToString();
                if (!tbcdeflocy.IsFocused || ignore_focus) tbcdeflocy.Text = locy.ToString();
                if (!tbcdeflocz.IsFocused || ignore_focus) tbcdeflocz.Text = locz.ToString();
                if (!tbcdefbits.IsFocused || ignore_focus) tbcdefbits.Text = new Global(index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits).Get<int>().ToString();
                if (!tbcdefmnswap.IsFocused || ignore_focus) tbcdefmnswap.Text = new Global(index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.mnswap).Get<int>().ToString();
                if (!tbcdefwprad.IsFocused || ignore_focus) tbcdefwprad.Text = new Global(index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.wprad).Get<int>().ToString();
                Functions.Read.checkbinary(1, index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits, cbcdefhide);
                Functions.Read.checkbinary(2, index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits, cbcdeffreeze);
                Functions.Read.checkbinary(3, index * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits, cbcdefmissionent);
            }
        }


        private void cbcdefhide_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits, cbcdefhide);
        }


        private void cbcdeffreeze_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits, cbcdeffreeze);
        }


        private void cbcdefmissionent_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits, cbcdefmissionent);
        }

        private void cbcdefadvanced_Checked(object sender, RoutedEventArgs e)
        {
            if (cdefadvanced != null)
                cdefadvanced.Visibility = cdefadvanced.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Btncdefgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbcdeflocx.Text = loc[0];
            tbcdeflocy.Text = loc[1];
            tbcdeflocz.Text = loc[2];
            creatorRefresh();
        }

        private void Btncdefgetmodel_Click(object sender, RoutedEventArgs e)
        {
            tbcdefmodel.Text = new Global(GTA.Offsets.Editor.custom_hovered_model).Get<int>().ToString();
        }

        private void tbcdeflocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.locx).SetFloat(tbcdeflocx.Text);
        }

        private void tbcdeflocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.locy).SetFloat(tbcdeflocy.Text);
        }

        private void tbcdeflocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.locz).SetFloat(tbcdeflocz.Text);
        }

        private void tbcdefbulk_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Regex.Match(tbcdefbulk.Text, @".+: \d+: X:.+ Y:.+ Z:.+").Success)
            {
                return;
            }

            if (m.IsProcOpen)
            {
                centitydef centitydefInput;

                string[] centitydefInputArray = tbcdefbulk.Text.Replace(":", "")
                                                                    .Replace("X", "")
                                                                    .Replace("Y", "")
                                                                    .Replace("Z", "")
                                                                    .Replace(" ", "/").Split('/');

                centitydefInput = new centitydef(Functions.int_parse(Functions.joaat(centitydefInputArray[0]).ToString()).ToString(),
                                                Convert.ToSingle(centitydefInputArray[2]),
                                                Convert.ToSingle(centitydefInputArray[3]),
                                                Convert.ToSingle(centitydefInputArray[4]));

                tbcdefmodel.Text = centitydefInput.model;
                tbcdeflocx.Text = centitydefInput.locx.ToString();
                tbcdeflocy.Text = centitydefInput.locy.ToString();
                tbcdeflocz.Text = centitydefInput.locz.ToString();
            }
        }

        private void tbcdefmodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbcdefmodel.Text) && m.IsProcOpen)
            {
                new Global(ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.model).SetInt(tbcdefmodel.Text);
            }
        }

        private void tbcdefbits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbcdefbits.Text) && m.IsProcOpen)
            {
                new Global(ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.bits).SetInt(tbcdefbits.Text);
            }
        }

        private void tbcdefmnswap_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbcdefmnswap.Text) && m.IsProcOpen)
            {
                new Global(ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.mnswap).SetInt(tbcdefmnswap.Text);
            }
        }

        private void tbcdefwprad_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbcdefwprad.Text) && m.IsProcOpen)
            {
                new Global(ddcentityno.SelectedIndex * GTA.Offsets.Editor.DHProp.NEXT + GTA.Offsets.Editor.DHProp.wprad).SetInt(tbcdefwprad.Text);
            }
        }

        class centitydef
        {
            public string model;
            public float locx;
            public float locy;
            public float locz;

            public centitydef(string model, float locx, float locy, float locz)
            {
                this.model = model;
                this.locx = locx;
                this.locy = locy;
                this.locz = locz;
            }
        }

        private void BtncdefAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int cdefnum = new Global(GTA.Offsets.Editor.DHProp.number).Get<int>();
                if (cdefnum < 40 && cdefnum > -1)
                {
                    int new_index = cdefnum + 1;
                    new Global(GTA.Offsets.Editor.DHProp.number).SetInt(new_index);
                    ddcentityno.SelectedIndex = new_index - 1;
                }
            }
        }
    }
}
