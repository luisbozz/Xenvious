using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / PA page.
    public partial class MainWindow
    {
        public Thread freezeBFMThread;

        private void Btnpastartgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.GetLocationVec();

            tbpastartlocx.Text = loc.X.ToString();
            tbpastartlocy.Text = loc.Y.ToString();
            tbpastartlocz.Text = loc.Z.ToString();
        }

        private void tbpastartlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbpastartlocx.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outb1vx + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocx.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb1vx + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocx.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb1vx + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocx.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outb1vx + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocx.Text);
                }
            }
        }

        private void tbpastartlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbpastartlocy.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outb1vy + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocy.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb1vy + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocy.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb1vy + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocy.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outb1vy + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocy.Text);
                }
            }
        }

        private void tbpastartlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbpastartlocz.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outb1vz + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocz.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb1vz + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocz.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb1vz + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocz.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outb1vz + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpastartlocz.Text);
                }
            }
        }

        private void tbpaendlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbpaendlocx.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outb2vx + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocx.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb2vx + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocx.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb2vx + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocx.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outb2vx + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocx.Text);
                }
            }
        }

        private void tbpaendlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbpaendlocy.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outb2vy + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocy.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb2vy + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocy.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb2vy + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocy.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outb2vy + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocy.Text);
                }
            }
        }

        private void tbpaendlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbpaendlocz.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outb2vz + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocz.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb2vz + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocz.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outb2vz + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocz.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outb2vz + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpaendlocz.Text);
                }
            }
        }

        private void Btnpaendgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.GetLocationVec();

            tbpaendlocx.Text = loc.X.ToString();
            tbpaendlocy.Text = loc.Y.ToString();
            tbpaendlocz.Text = loc.Z.ToString();
        }

        private void tbpawidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && !String.IsNullOrWhiteSpace(tbpawidth.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outw + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpawidth.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outw + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpawidth.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outw + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpawidth.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outw + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tbpawidth.Text);
                }
            }
        }

        private void cbpaenable_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                bool enabled = cbpaenable.IsChecked == true;

                tbpastartlocx.IsEnabled = enabled;
                tbpastartlocy.IsEnabled = enabled;
                tbpastartlocz.IsEnabled = enabled;
                Btnpastartgetloc.IsEnabled = enabled;

                tbpaendlocx.IsEnabled = enabled;
                tbpaendlocy.IsEnabled = enabled;
                tbpaendlocz.IsEnabled = enabled;
                Btnpaendgetloc.IsEnabled = enabled;

                tbpawidth.IsEnabled = enabled;

                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outbt + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbt + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbt + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outbt + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                }
            }
        }

        private void tboutbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tboutbs.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outbs + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutbs.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbs + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutbs.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbs + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutbs.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outbs + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutbs.Text);
                }
            }
        }

        private void tboutety_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tboutety.Text))
            {
                new Global(GTA.Offsets.Editor.PlayArea.outety + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutety.Text);
            }
        }

        private void tbouteid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbouteid.Text))
            {
                new Global(GTA.Offsets.Editor.PlayArea.outeid + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tbouteid.Text);
            }
        }

        private void tboutblocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbx + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocx.Text);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outbx + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocx.Text);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outbx + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocx.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.outbx + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocx.Text);
            }
        }

        private void tboutblocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outby + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocy.Text);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outby + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocy.Text);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outby + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocy.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.outby + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocy.Text);
            }
        }

        private void tboutblocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbz + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocz.Text);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outbz + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocz.Text);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outbz + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocz.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.outbz + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutblocz.Text);
            }
        }

        private void Btnoutbgetloc_Click(object sender, RoutedEventArgs e)
        {
            var temp = Functions.Read.getlocation();

            tboutblocx.Text = temp[0].ToString();
            tboutblocy.Text = temp[1].ToString();
            tboutblocz.Text = temp[2].ToString();
        }

        private void tboutr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outr + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutr.Text);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outr + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutr.Text);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outr + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutr.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.outr + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutr.Text);
            }
        }

        private void tboutmm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tboutmm.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outmm + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutmm.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outmm + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutmm.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outmm + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutmm.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outmm + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutmm.Text);
                }
            }
        }

        private void tboutw_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outw + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutw.Text);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outw + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutw.Text);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outw + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutw.Text);
                }
            }
            else
            {
                new Global(GTA.Offsets.Editor.PlayArea.outw + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetFloat(tboutw.Text);
            }
        }

        public void GetOUTBValues(bool ignore_focus = false)
        {
            if (!m.IsProcOpen)
                return;
            if (ddoutbteam == null || ddoutbno == null)
                return;

            int tindex = ddoutbteam.SelectedIndex;
            int index = ddoutbno.SelectedIndex;

            bool enabled = !((tindex > 3 || tindex < 0) || index == -1);

            Btnoutbgetloc.IsEnabled = enabled ? true : false;
            tboutblocx.IsEnabled = enabled ? true : false;
            tboutblocy.IsEnabled = enabled ? true : false;
            tboutblocz.IsEnabled = enabled ? true : false;
            tboutbs.IsEnabled = enabled ? true : false;
            tboutr.IsEnabled = enabled ? true : false;
            tboutw.IsEnabled = enabled ? true : false;
            tboutmm.IsEnabled = enabled ? true : false;
            tboutilv.IsEnabled = enabled ? true : false;
            tboutonfv.IsEnabled = enabled ? true : false;
            tbouteid.IsEnabled = enabled ? true : false;
            tboutety.IsEnabled = enabled ? true : false;
            ddouthc.IsEnabled = enabled ? true : false;
            tboutpribt.IsEnabled = enabled ? true : false;
            tbbfm.IsEnabled = enabled ? true : false;
            cbpzvisible.IsEnabled = enabled ? true : false;
            cbpzvisibleinside.IsEnabled = enabled ? true : false;
            cbpaenable.IsEnabled = enabled ? true : false;

            //get plyr values
            if (!tboutblocx.IsFocused || ignore_focus) tboutblocx.Text = new Global(GTA.Offsets.Editor.PlayArea.outbx + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tboutblocy.IsFocused || ignore_focus) tboutblocy.Text = new Global(GTA.Offsets.Editor.PlayArea.outby + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tboutblocz.IsFocused || ignore_focus) tboutblocz.Text = new Global(GTA.Offsets.Editor.PlayArea.outbz + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tboutr.IsFocused || ignore_focus) tboutr.Text = new Global(GTA.Offsets.Editor.PlayArea.outr + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tboutw.IsFocused || ignore_focus) tboutw.Text = new Global(GTA.Offsets.Editor.PlayArea.outw + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            if (!tboutbs.IsFocused || ignore_focus) tboutbs.Text = new Global(GTA.Offsets.Editor.PlayArea.outbs + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tboutmm.IsFocused || ignore_focus) tboutmm.Text = new Global(GTA.Offsets.Editor.PlayArea.outmm + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tboutilv.IsFocused || ignore_focus) tboutilv.Text = new Global(GTA.Offsets.Editor.PlayArea.outilv + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tboutonfv.IsFocused || ignore_focus) tboutonfv.Text = new Global(GTA.Offsets.Editor.PlayArea.out2onfv + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tboutety.IsFocused || ignore_focus) tboutety.Text = new Global(GTA.Offsets.Editor.PlayArea.outety + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!tbouteid.IsFocused || ignore_focus) tbouteid.Text = new Global(GTA.Offsets.Editor.PlayArea.outeid + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>().ToString();
            if (!ddouthc.IsFocused || ignore_focus) ddouthc.SelectedIndex = new Global(GTA.Offsets.Editor.PlayArea.outhc + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>();
            if (!tboutpribt.IsFocused || ignore_focus) tboutpribt.Text = new Global(GTA.Offsets.Editor.PlayArea.pribt + tindex * GTA.Offsets.Editor.team_NEXT + index).Get<int>().ToString();
            if (!tbbfm.IsFocused || ignore_focus)
            {
                if (index == 0)
                {
                    tbbfm.Text = bfmfreeze[tindex][index].ToString();
                }
                else if (index > 0)
                {
                    tbbfm.Text = new Global(GTA.Offsets.Editor.bfm + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.txt_NEXT).GetString();
                }
            }

            Functions.Read.checkbinary(7, GTA.Offsets.Editor.PlayArea.outbs + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT, cbpzvisible);
            Functions.Read.checkbinary(15, GTA.Offsets.Editor.irbs8 + tindex * GTA.Offsets.Editor.team_NEXT + index, cbpzvisibleinside);



            //rectangle pz
            bool enable = new Global(GTA.Offsets.Editor.PlayArea.outbt + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<int>() == 1;
            cbpaenable.IsChecked = enable;

            if (enable)
            {
                if (!tbpastartlocx.IsFocused || ignore_focus) tbpastartlocx.Text = new Global(GTA.Offsets.Editor.PlayArea.outb1vx + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbpastartlocy.IsFocused || ignore_focus) tbpastartlocy.Text = new Global(GTA.Offsets.Editor.PlayArea.outb1vy + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbpastartlocz.IsFocused || ignore_focus) tbpastartlocz.Text = new Global(GTA.Offsets.Editor.PlayArea.outb1vz + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();

                if (!tbpaendlocx.IsFocused || ignore_focus) tbpaendlocx.Text = new Global(GTA.Offsets.Editor.PlayArea.outb2vx + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbpaendlocy.IsFocused || ignore_focus) tbpaendlocy.Text = new Global(GTA.Offsets.Editor.PlayArea.outb2vy + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
                if (!tbpaendlocz.IsFocused || ignore_focus) tbpaendlocz.Text = new Global(GTA.Offsets.Editor.PlayArea.outb2vz + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();

                if (!tbpawidth.IsFocused || ignore_focus) tbpawidth.Text = new Global(GTA.Offsets.Editor.PlayArea.outw + tindex * GTA.Offsets.Editor.team_NEXT + index * GTA.Offsets.Editor.PlayArea.NEXT).Get<float>().ToString();
            }
        }

        private void ddoutbno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetOUTBValues(true);
            SelectActiveTextBox();
        }

        private void ddoutbteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetOUTBValues(true);
            SelectActiveTextBox();
        }

        private void ddouthc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddouthc.SelectedIndex > -1)
            {
                new Global(GTA.Offsets.Editor.PlayArea.outhc + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(ddouthc.SelectedIndex);
            }
        }

        private void tboutpribt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tboutpribt.Text))
            {
                new Global(GTA.Offsets.Editor.PlayArea.pribt + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex).SetInt(tboutpribt.Text);
            }
        }

        private void tbbfm_TextChanged(object sender, TextChangedEventArgs e)
        {
            byte[] temparr = Encoding.UTF8.GetBytes(tbbfm.Text);

            if (temparr.Length <= 80)
            {
                if (ddoutbno.SelectedIndex == 0)
                {
                    if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                    {
                        for (int i = 0; i < 17; i++)
                        {
                            for (int d = 0; d < 4; d++)
                            {
                                bfmfreeze[d][i] = tbbfm.Text;
                            }
                        }
                    }
                    else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            bfmfreeze[i][ddoutbno.SelectedIndex] = tbbfm.Text;
                        }
                    }
                    else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                    {
                        for (int i = 0; i < 17; i++)
                        {
                            bfmfreeze[ddoutbteam.SelectedIndex][i] = tbbfm.Text;
                        }
                    }
                    else
                    {
                        bfmfreeze[ddoutbteam.SelectedIndex][ddoutbno.SelectedIndex] = tbbfm.Text;
                    }
                }
                else if (ddoutbno.SelectedIndex > 0)
                {
                    if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                    {
                        for (int i = 0; i < 17; i++)
                        {
                            for (int d = 0; d < 4; d++)
                            {
                                new Global(GTA.Offsets.Editor.bfm + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.txt_NEXT).SetString(tbbfm.Text);
                            }
                        }
                    }
                    else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            new Global(GTA.Offsets.Editor.bfm + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).SetString(tbbfm.Text);
                        }
                    }
                    else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                    {
                        for (int i = 0; i < 17; i++)
                        {
                            new Global(GTA.Offsets.Editor.bfm + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.txt_NEXT).SetString(tbbfm.Text);
                        }
                    }
                    else
                    {
                        new Global(GTA.Offsets.Editor.bfm + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).SetString(tbbfm.Text);
                    }
                }
            }
        }


        private void cbbfmfreeze_Checked(object sender, RoutedEventArgs e)
        {
            bool ischecked = cbbfmfreeze.IsChecked ?? true;
            if (ischecked)
            {
                freezeBFMThread = new Thread(new ThreadStart(freezebfm));
                freezeBFMThread.Priority = ThreadPriority.Highest;
                freezeBFMThread.IsBackground = true;
                freezeBFMThread.Start();
            }
            else
            {
                freezeBFMThread.Abort();
            }
        }

        public static void freezebfm()
        {
            while (true)
            {
                for (int d = 0; d < 17; d++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.bfm + i * GTA.Offsets.Editor.team_NEXT + d * GTA.Offsets.Editor.txt_NEXT).SetString(bfmfreeze[i][d]);
                    }
                }
            }

        }

        private void cbpzvisible_Checked(object sender, RoutedEventArgs e)
        {
            bool isvisible = cbpzvisible.IsChecked == true;
            cbpzvisibleinside.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.outbs + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT, isvisible);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.outbs + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT, isvisible);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.outbs + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT, isvisible);
                }
            }
            else
            {
                Functions.Write.writebinary(7, GTA.Offsets.Editor.PlayArea.outbs + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT, isvisible);
            }

        }

        private void cbpaafat_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                bool enabled = cbpaenable.IsChecked == true;
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outbt + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbt + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outbt + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outbt + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(enabled ? 1 : 0);
                }
            }
        }

        private void tboutilv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tboutilv.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outilv + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutilv.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outilv + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutilv.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outilv + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutilv.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outilv + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutilv.Text);
                }
            }
        }

        private void tboutonfv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tboutonfv.Text))
            {
                if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            new Global(GTA.Offsets.Editor.PlayArea.outonfv + d * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutonfv.Text);
                        }
                    }
                }
                else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outonfv + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutonfv.Text);
                    }
                }
                else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        new Global(GTA.Offsets.Editor.PlayArea.outonfv + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutonfv.Text);
                    }
                }
                else
                {
                    new Global(GTA.Offsets.Editor.PlayArea.outonfv + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex * GTA.Offsets.Editor.PlayArea.NEXT).SetInt(tboutonfv.Text);
                }
            }
        }

        private void cbpzvisibleinside_Checked(object sender, RoutedEventArgs e)
        {
            bool isvisible = cbpzvisibleinside.IsChecked == true;
            if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        Functions.Write.writebinary(15, GTA.Offsets.Editor.irbs8 + d * GTA.Offsets.Editor.team_NEXT + i, isvisible);
                    }
                }
            }
            else if (cbpaafat.IsChecked == true && cbpaafap.IsChecked == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    Functions.Write.writebinary(15, GTA.Offsets.Editor.irbs8 + i * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex, isvisible);
                }
            }
            else if (cbpaafat.IsChecked == false && cbpaafap.IsChecked == true)
            {
                for (int i = 0; i < 17; i++)
                {
                    Functions.Write.writebinary(15, GTA.Offsets.Editor.irbs8 + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + i, isvisible);
                }
            }
            else
            {
                Functions.Write.writebinary(15, GTA.Offsets.Editor.irbs8 + ddoutbteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddoutbno.SelectedIndex, isvisible);
            }
        }
    }
}
