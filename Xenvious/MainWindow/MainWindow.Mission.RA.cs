using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / RA page.
    public partial class MainWindow
    {
        public void GetBD2Values(bool ignore_focus = false)
        {
            if (!m.IsProcOpen)
                return;
            if (ddbd2team == null || ddbd2no == null)
                return;

            int tindex = ddbd2team.SelectedIndex;
            int index = ddbd2no.SelectedIndex;

            bool enabled = !((tindex > 3 || tindex < 0) || index == -1);

            tbbd2vlocx.IsEnabled = enabled ? true : false;
            tbbd2vlocy.IsEnabled = enabled ? true : false;
            tbbd2vlocz.IsEnabled = enabled ? true : false;
            tbbd2r.IsEnabled = enabled ? true : false;
            tbout2bs.IsEnabled = enabled ? true : false;
            tbout2mm.IsEnabled = enabled ? true : false;
            tbout2wg.IsEnabled = enabled ? true : false;
            tbout2iv.IsEnabled = enabled ? true : false;
            tbout2io.IsEnabled = enabled ? true : false;
            tbout2fp.IsEnabled = enabled ? true : false;
            tbout2sg.IsEnabled = enabled ? true : false;
            tbout2bh.IsEnabled = enabled ? true : false;
            tbout2ilv.IsEnabled = enabled ? true : false;
            tbout2onfv.IsEnabled = enabled ? true : false;
            tbout2hc.IsEnabled = enabled ? true : false;
            Btnbd2vgetloc.IsEnabled = enabled ? true : false;
            cbbd2paenable.IsEnabled = enabled ? true : false;
            cbbd2pzvisible.IsEnabled = enabled ? true : false;
            tbout2et.IsEnabled = enabled ? true : false;
            tbout2id.IsEnabled = enabled ? true : false;

            if (!tbbd2vlocx.IsFocused || ignore_focus) tbbd2vlocx.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2vx + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tbbd2vlocx.IsFocused || ignore_focus) tbbd2vlocy.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2vy + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tbbd2vlocx.IsFocused || ignore_focus) tbbd2vlocz.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2vz + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tbbd2r.IsFocused || ignore_focus) tbbd2r.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2r + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tbout2bs.IsFocused || ignore_focus) tbout2bs.Text = new Global(GTA.Offsets.Editor.PlayArea.out2bs + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2mm.IsFocused || ignore_focus) tbout2mm.Text = new Global(GTA.Offsets.Editor.PlayArea.out2mm + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2wg.IsFocused || ignore_focus) tbout2wg.Text = new Global(GTA.Offsets.Editor.PlayArea.out2wg + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2iv.IsFocused || ignore_focus) tbout2iv.Text = new Global(GTA.Offsets.Editor.PlayArea.out2iv + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2io.IsFocused || ignore_focus) tbout2io.Text = new Global(GTA.Offsets.Editor.PlayArea.out2io + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2fp.IsFocused || ignore_focus) tbout2fp.Text = new Global(GTA.Offsets.Editor.PlayArea.out2fp + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2sg.IsFocused || ignore_focus) tbout2sg.Text = new Global(GTA.Offsets.Editor.PlayArea.out2sg + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2bh.IsFocused || ignore_focus) tbout2bh.Text = new Global(GTA.Offsets.Editor.PlayArea.out2bh + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2ilv.IsFocused || ignore_focus) tbout2ilv.Text = new Global(GTA.Offsets.Editor.PlayArea.out2ilv + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2onfv.IsFocused || ignore_focus) tbout2onfv.Text = new Global(GTA.Offsets.Editor.PlayArea.out2onfv + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2hc.IsFocused || ignore_focus) tbout2hc.Text = new Global(GTA.Offsets.Editor.PlayArea.out2hc + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2et.IsFocused || ignore_focus) tbout2et.Text = new Global(GTA.Offsets.Editor.PlayArea.out2et + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbout2id.IsFocused || ignore_focus) tbout2id.Text = new Global(GTA.Offsets.Editor.PlayArea.out2id + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();


            Functions.Read.checkbinary(7, GTA.Offsets.Editor.PlayArea.out2bs + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT, cbbd2pzvisible);

            //rectangle pz bd2
            bool enable = new Global(GTA.Offsets.Editor.PlayArea.bd2t + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>() == 1;
            cbbd2paenable.IsChecked = enable;

            if (enable)
            {
                if (!tbbd2pastartlocx.IsFocused || ignore_focus) tbbd2pastartlocx.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2v1x + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbbd2pastartlocy.IsFocused || ignore_focus) tbbd2pastartlocy.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2v1y + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbbd2pastartlocz.IsFocused || ignore_focus) tbbd2pastartlocz.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2v1z + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();

                if (!tbbd2paendlocx.IsFocused || ignore_focus) tbbd2paendlocx.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2v2x + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbbd2paendlocy.IsFocused || ignore_focus) tbbd2paendlocy.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2v2y + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbbd2paendlocz.IsFocused || ignore_focus) tbbd2paendlocz.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2v2z + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();

                if (!tbbd2pawidth.IsFocused || ignore_focus) tbbd2pawidth.Text = new Global(GTA.Offsets.Editor.PlayArea.bd2w + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            }

        }

        private void Btnbd2vgetloc_Click(object sender, RoutedEventArgs e)
        {
            var temp = Functions.Read.getlocation();

            tbbd2vlocx.Text = temp[0].ToString();
            tbbd2vlocy.Text = temp[1].ToString();
            tbbd2vlocz.Text = temp[2].ToString();
        }

        private void cbbd2paenable_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                bool enabled = cbbd2paenable.IsChecked == true;

                tbbd2pastartlocx.IsEnabled = enabled;
                tbbd2pastartlocy.IsEnabled = enabled;
                tbbd2pastartlocz.IsEnabled = enabled;
                Btnbd2pastartgetloc.IsEnabled = enabled;

                tbbd2paendlocx.IsEnabled = enabled;
                tbbd2paendlocy.IsEnabled = enabled;
                tbbd2paendlocz.IsEnabled = enabled;
                Btnbd2paendgetloc.IsEnabled = enabled;

                tbbd2pawidth.IsEnabled = enabled;

                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.bd2t + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2t + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2t + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2t + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                }
            }
        }

        private void tbbd2vlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2vx + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocx.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2vx + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocx.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2vx + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocx.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2vx + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocx.Text);
            }
        }

        private void tbbd2vlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2vy + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocy.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2vy + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocy.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2vy + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocy.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2vy + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocy.Text);
            }
        }

        private void tbbd2vlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2vz + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocz.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2vz + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocz.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2vz + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocz.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2vz + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2vlocz.Text);
            }
        }

        private void tbbd2r_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2r + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2r.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2r + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2r.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2r + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2r.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2r + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2r.Text);
            }
        }

        private void tbbd2pastartlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2v1x + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocx.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v1x + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocx.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v1x + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocx.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2v1x + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocx.Text);
            }
        }

        private void tbbd2pastartlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2v1y + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocy.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v1y + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocy.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v1y + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocy.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2v1y + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocy.Text);
            }
        }

        private void tbbd2pastartlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2v1z + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocz.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v1z + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocz.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v1z + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocz.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2v1z + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pastartlocz.Text);
            }
        }

        private void Btnbd2pastartgetloc_Click(object sender, RoutedEventArgs e)
        {
            var temp = Functions.Read.getlocation();

            tbbd2pastartlocx.Text = temp[0].ToString();
            tbbd2pastartlocy.Text = temp[1].ToString();
            tbbd2pastartlocz.Text = temp[2].ToString();
        }

        private void tbbd2paendlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2v2x + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocx.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v2x + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocx.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v2x + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocx.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2v2x + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocx.Text);
            }
        }

        private void tbbd2paendlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2v2y + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocy.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v2y + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocy.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v2y + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocy.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2v2y + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocy.Text);
            }
        }

        private void tbbd2paendlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2v2z + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocz.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v2z + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocz.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2v2z + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocz.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2v2z + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2paendlocz.Text);
            }
        }

        private void Btnbd2paendgetloc_Click(object sender, RoutedEventArgs e)
        {
            var temp = Functions.Read.getlocation();

            tbbd2paendlocx.Text = temp[0].ToString();
            tbbd2paendlocy.Text = temp[1].ToString();
            tbbd2paendlocz.Text = temp[2].ToString();
        }

        private void tbbd2pawidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2w + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pawidth.Text);
                    }
                }
            }
            else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2w + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pawidth.Text);
                }
            }
            else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2w + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pawidth.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.bd2w + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbbd2pawidth.Text);
            }
        }

        private void cbbd2paafat_Checked(object sender, RoutedEventArgs e)
        {

            if (m.IsProcOpen)
            {
                bool enabled = cbbd2paenable.IsChecked == true;
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.bd2t + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2t + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.bd2t + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.bd2t + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                }
            }
        }

        private void ddbd2team_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBD2Values(true);
            SelectActiveTextBox();
        }

        private void ddbd2no_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBD2Values(true);
            SelectActiveTextBox();
        }

        private void tbout2hc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2hc.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2hc + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2hc.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2hc + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2hc.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2hc + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2hc.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2hc + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2hc.Text);
                }
            }
        }

        private void tbout2bs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2bs.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2bs + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bs.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2bs + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bs.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2bs + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bs.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2bs + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bs.Text);
                }
            }
        }

        private void tbout2mm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2mm.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2mm + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2mm.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2mm + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2mm.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2mm + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2mm.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2mm + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2mm.Text);
                }
            }
        }

        private void tbout2ilv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2ilv.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2ilv + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2ilv.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2ilv + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2ilv.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2ilv + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2ilv.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2ilv + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2ilv.Text);
                }
            }
        }

        private void tbout2onfv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2onfv.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2onfv + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2onfv.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2onfv + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2onfv.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2onfv + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2onfv.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2onfv + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2onfv.Text);
                }
            }
        }

        private void tbout2et_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2et.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2et + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2et.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2et + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2et.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2et + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2et.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2et + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2et.Text);
                }
            }
        }

        private void tbout2ei_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2id.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2id + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2id.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2id + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2id.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2id + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2id.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2id + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2id.Text);
                }
            }
        }

        private void tbout2bh_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2bh.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2bh + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bh.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2bh + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bh.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2bh + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bh.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2bh + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2bh.Text);
                }
            }
        }

        private void tbout2sg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2sg.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2sg + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2sg.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2sg + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2sg.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2sg + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2sg.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2sg + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2sg.Text);
                }
            }
        }

        private void tbout2fp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2fp.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2fp + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2fp.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2fp + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2fp.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2fp + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2fp.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2fp + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2fp.Text);
                }
            }
        }

        private void tbout2io_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2io.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2io + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2io.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2io + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2io.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2io + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2io.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2io + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2io.Text);
                }
            }
        }

        private void tbout2iv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2iv.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2iv + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2iv.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2iv + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2iv.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2iv + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2iv.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2iv + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2iv.Text);
                }
            }
        }

        private void tbout2wg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbout2wg.Text))
            {
                if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.out2wg + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2wg.Text);
                        }
                    }
                }
                else if (cbbd2paafat.IsChecked == true && cbbd2paafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2wg + i * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2wg.Text);
                    }
                }
                else if (cbbd2paafat.IsChecked == false && cbbd2paafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.out2wg + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2wg.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.out2wg + ddbd2team.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddbd2no.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbout2wg.Text);
                }
            }
        }

        private void cbbd2pzvisible_Checked(object sender, RoutedEventArgs e)
        {
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.out2bs + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT, cbbd2pzvisible);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.out2bs + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT, cbbd2pzvisible);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.out2bs + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT, cbbd2pzvisible);
                }
            }
            else
            {
                Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.out2bs + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT, cbbd2pzvisible);
            }
        }
    }
}
