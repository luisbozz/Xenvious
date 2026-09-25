using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / Blips page.
    public partial class MainWindow
    {
        public void GetBlips(bool ignore_focus = false)
        {
            int index = ddmissionddblipno.SelectedIndex;

            Btmissionddblipposgetloc.IsEnabled = index < 0 ? false : true;
            tbmissionddblipposox.IsEnabled = index < 0 ? false : true;
            tbmissionddblipposoy.IsEnabled = index < 0 ? false : true;
            tbmissionddblipposoz.IsEnabled = index < 0 ? false : true;
            tbmissionddblipsize.IsEnabled = index < 0 ? false : true;
            tbmissionddblipbits.IsEnabled = index < 0 ? false : true;
            tbmissionddbliprule.IsEnabled = index < 0 ? false : true;
            tbmissionddblipteam.IsEnabled = index < 0 ? false : true;
            tbmissionddblipname.IsEnabled = index < 0 ? false : true;
            tbmissionddblipclr.IsEnabled = index < 0 ? false : true;
            tbmissionddblipentt.IsEnabled = index < 0 ? false : true;
            tbmissionddblipenti.IsEnabled = index < 0 ? false : true;
            tbmissionddbliptype.IsEnabled = index < 0 ? false : true;
            tbmissionddblipveh.IsEnabled = index < 0 ? false : true;
            tbmissionddblipsprite.IsEnabled = index < 0 ? false : true;
            cbmissionddblipeft1.IsEnabled = index < 0 ? false : true;
            cbmissionddblipeft2.IsEnabled = index < 0 ? false : true;
            cbmissionddblipeft3.IsEnabled = index < 0 ? false : true;
            cbmissionddblipeft4.IsEnabled = index < 0 ? false : true;
            cbmissionddbliperoute.IsEnabled = index < 0 ? false : true;
            ddmissionddbliptype.IsEnabled = index < 0 ? false : true;
            tbmissionddblipbindindex.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                if (!tbmissionddblipposox.IsFocused || ignore_focus) tbmissionddblipposox.Text = new Global((GTA.Offsets.Editor.ddblip.pos + 0 + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<float>().ToString();
                if (!tbmissionddblipposoy.IsFocused || ignore_focus) tbmissionddblipposoy.Text = new Global((GTA.Offsets.Editor.ddblip.pos + 1 + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<float>().ToString();
                if (!tbmissionddblipposoz.IsFocused || ignore_focus) tbmissionddblipposoz.Text = new Global((GTA.Offsets.Editor.ddblip.pos + 2 + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<float>().ToString();
                if (!tbmissionddblipsize.IsFocused || ignore_focus) tbmissionddblipsize.Text = new Global((GTA.Offsets.Editor.ddblip.size + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipbits.IsFocused || ignore_focus) tbmissionddblipbits.Text = new Global((GTA.Offsets.Editor.ddblip.bits + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddbliprule.IsFocused || ignore_focus) tbmissionddbliprule.Text = new Global((GTA.Offsets.Editor.ddblip.rule + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipteam.IsFocused || ignore_focus) tbmissionddblipteam.Text = new Global((GTA.Offsets.Editor.ddblip.team + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipname.IsFocused || ignore_focus) tbmissionddblipname.Text = new Global((GTA.Offsets.Editor.ddblip.dbnm + GTA.Offsets.Editor.ddblip.NEXT * index)).GetString();
                if (!tbmissionddblipclr.IsFocused || ignore_focus) tbmissionddblipclr.Text = new Global((GTA.Offsets.Editor.ddblip.clr + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipentt.IsFocused || ignore_focus) tbmissionddblipentt.Text = new Global((GTA.Offsets.Editor.ddblip.entt + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipenti.IsFocused || ignore_focus) tbmissionddblipenti.Text = new Global((GTA.Offsets.Editor.ddblip.enti + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddbliptype.IsFocused || ignore_focus) tbmissionddbliptype.Text = new Global((GTA.Offsets.Editor.ddblip.type + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipveh.IsFocused || ignore_focus) tbmissionddblipveh.Text = new Global((GTA.Offsets.Editor.ddblip.veh + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipsprite.IsFocused || ignore_focus) tbmissionddblipsprite.Text = new Global((GTA.Offsets.Editor.ddblip.spri + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!tbmissionddblipbindindex.IsFocused || ignore_focus) tbmissionddblipbindindex.Text = new Global((GTA.Offsets.Editor.ddblip.veh + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>().ToString();
                if (!ddmissionddbliptype.IsFocused || ignore_focus)
                {
                    int type = new Global((GTA.Offsets.Editor.ddblip.type + GTA.Offsets.Editor.ddblip.NEXT * index)).Get<int>();
                    if (type == 0 || type == 1)
                    {
                        ddmissionddbliptype.SelectedIndex = 0;
                    }
                    else if (type == 5)
                    {
                        ddmissionddbliptype.SelectedIndex = 1;
                    }
                    else if (type == 3)
                    {
                        ddmissionddbliptype.SelectedIndex = 2;
                    }
                    else if (type == 2)
                    {
                        ddmissionddbliptype.SelectedIndex = 3;
                    }
                    else if (type == 4)
                    {
                        ddmissionddbliptype.SelectedIndex = 4;
                    }
                    else
                    {
                        ddmissionddbliptype.SelectedIndex = -1;
                    }
                }
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.ddblip.bits + GTA.Offsets.Editor.ddblip.NEXT * index, cbmissionddblipeft1);
                Functions.Read.checkbinary(2, GTA.Offsets.Editor.ddblip.bits + GTA.Offsets.Editor.ddblip.NEXT * index, cbmissionddblipeft2);
                Functions.Read.checkbinary(3, GTA.Offsets.Editor.ddblip.bits + GTA.Offsets.Editor.ddblip.NEXT * index, cbmissionddblipeft3);
                Functions.Read.checkbinary(4, GTA.Offsets.Editor.ddblip.bits + GTA.Offsets.Editor.ddblip.NEXT * index, cbmissionddblipeft4);
                Functions.Read.checkbinary(7, GTA.Offsets.Editor.ddblip.bits + GTA.Offsets.Editor.ddblip.NEXT * index, cbmissionddbliperoute);
            }

        }

        private void ddmissionddblipno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBlips(true);
        }

        private void tbmissionddblipposox_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.ddblip.pos + 0 + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetFloat(tbmissionddblipposox.Text);
        }

        private void tbmissionddblipposoy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.ddblip.pos + 1 + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetFloat(tbmissionddblipposoy.Text);
        }

        private void tbmissionddblipposoz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.ddblip.pos + 2 + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetFloat(tbmissionddblipposoz.Text);
        }

        private void Btmissionddblipposgetloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbmissionddblipposox.Text = loc[0];
                tbmissionddblipposoy.Text = loc[1];
                tbmissionddblipposoz.Text = loc[2];
                creatorRefresh();
            }
        }

        private void tbmissionddblipsize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipsize.Text))
                new Global(GTA.Offsets.Editor.ddblip.size + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipsize.Text);
        }

        private void tbmissionddblipbits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipbits.Text))
                new Global(GTA.Offsets.Editor.ddblip.bits + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipbits.Text);
        }

        private void tbmissionddblipname_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.ddblip.dbnm + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetString(tbmissionddblipname.Text);
        }

        private void tbmissionddbliprule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddbliprule.Text))
                new Global(GTA.Offsets.Editor.ddblip.rule + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddbliprule.Text);
        }

        private void tbmissionddblipteam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipteam.Text))
                new Global(GTA.Offsets.Editor.ddblip.team + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipteam.Text);
        }

        private void tbmissionddblipclr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipclr.Text))
                new Global(GTA.Offsets.Editor.ddblip.clr + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipclr.Text);
        }

        private void tbmissionddblipentt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipentt.Text))
                new Global(GTA.Offsets.Editor.ddblip.entt + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipentt.Text);
        }

        private void tbmissionddblipenti_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipenti.Text))
                new Global(GTA.Offsets.Editor.ddblip.enti + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipenti.Text);
        }

        private void BtnmissionddblipAdd_Click(object sender, RoutedEventArgs e)
        {
            int blipnum = new Global(GTA.Offsets.Editor.ddblip.number).Get<int>();
            if (blipnum < 56 && blipnum > -1)
            {
                int new_index = blipnum + 1;

                new Global(GTA.Offsets.Editor.ddblip.number).SetInt(new_index);
                ddmissionddblipno.SelectedIndex = new_index - 1;
            }
        }

        private void BtnmissionddblipDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = ddmissionddblipno.SelectedIndex;
            int num = new Global(GTA.Offsets.Editor.ddblip.number).Get<int>();

            if (index > -1)
            {
                if (num > 0)
                {
                    new Global(GTA.Offsets.Editor.ddblip.number).SetInt(num - 1);
                }
            }
        }

        private void cbmissionddblipeft1_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.ddblip.bits, cbmissionddblipeft1);
        }
        private void cbmissionddblipeft2_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.ddblip.bits, cbmissionddblipeft2);
        }
        private void cbmissionddblipeft3_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.ddblip.bits, cbmissionddblipeft3);
        }
        private void cbmissionddblipeft4_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.ddblip.bits, cbmissionddblipeft4);
        }

        private void tbmissionddbliptype_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddbliptype.Text))
                new Global(GTA.Offsets.Editor.ddblip.type + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddbliptype.Text);
        }

        private void tbmissionddblipveh_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipveh.Text))
                new Global(GTA.Offsets.Editor.ddblip.veh + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipveh.Text);
        }

        private void tbmissionddblipsprite_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipsprite.Text))
                new Global(GTA.Offsets.Editor.ddblip.spri + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipsprite.Text);
        }

        private void cbmissionddbliperoute_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.ddblip.bits, cbmissionddbliperoute);
        }

        private void tbmissionddblipbindindex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionddblipbindindex.Text))
                new Global(GTA.Offsets.Editor.ddblip.veh + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).SetInt(tbmissionddblipbindindex.Text);
        }

        private void ddmissionddbliptype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Global type = new Global(GTA.Offsets.Editor.ddblip.type + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex);
            tbmissionddblipbindindex.Text = new Global(GTA.Offsets.Editor.ddblip.veh + GTA.Offsets.Editor.ddblip.NEXT * ddmissionddblipno.SelectedIndex).Get<int>().ToString();
            if (ddmissionddbliptype.SelectedIndex == 0)
            {
                type.SetInt(0);
                tbmissionddblipbindindex.Visibility = Visibility.Collapsed;
            }
            else if (ddmissionddbliptype.SelectedIndex == 1)
            {
                type.SetInt(5);
                tbmissionddblipbindindex.Visibility = Visibility.Visible;
            }
            else if (ddmissionddbliptype.SelectedIndex == 2)
            {
                type.SetInt(3);
                tbmissionddblipbindindex.Visibility = Visibility.Visible;
            }
            else if (ddmissionddbliptype.SelectedIndex == 3)
            {
                type.SetInt(2);
                tbmissionddblipbindindex.Visibility = Visibility.Collapsed;
            }
            else if (ddmissionddbliptype.SelectedIndex == 4)
            {
                type.SetInt(4);
                tbmissionddblipbindindex.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbmissionddblipbindindex.Visibility = Visibility.Collapsed;
            }
        }
    }
}
