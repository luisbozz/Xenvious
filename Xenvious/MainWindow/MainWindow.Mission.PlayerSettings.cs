using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / PlayerSettings page.
    public partial class MainWindow
    {
        private void ddplyrteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetPlyrValues(true);
            SelectActiveTextBox();
        }

        private void ddplyrno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetPlyrValues(true);
            SelectActiveTextBox();
        }

        public void GetPlyrValues(bool ignore_focus = false)
        {
            if (!m.IsProcOpen)
                return;
            if (ddplyrteam == null || ddplyrno == null)
                return;

            int tindex = ddplyrteam.SelectedIndex;
            int index = ddplyrno.SelectedIndex;

            bool enabled = !((tindex > 3 || tindex < 0) || index == -1);

            if (!enabled)
            {
                tbplyrlocx.IsEnabled = false;
                tbplyrlocy.IsEnabled = false;
                tbplyrlocz.IsEnabled = false;
                tbplyrhead.IsEnabled = false;
                tbplyrbits.IsEnabled = false;
                tbplyrveh.IsEnabled = false;
                tbplyrtars.IsEnabled = false;
                tbplyrvfrs.IsEnabled = false;
                tbplyrvfre.IsEnabled = false;
                tbplyrty.IsEnabled = false;
                tbplyras.IsEnabled = false;
                tbplyrqu.IsEnabled = false;
                tbplyrgg.IsEnabled = false;
                tbplyrar.IsEnabled = false;
                Btnplyrgetloc.IsEnabled = false;
                return;
            }
            else
            {
                tbplyrlocx.IsEnabled = true;
                tbplyrlocy.IsEnabled = true;
                tbplyrlocz.IsEnabled = true;
                tbplyrhead.IsEnabled = true;
                tbplyrbits.IsEnabled = true;
                tbplyrveh.IsEnabled = true;
                tbplyrtars.IsEnabled = true;
                tbplyrvfrs.IsEnabled = true;
                tbplyrvfre.IsEnabled = true;
                tbplyrty.IsEnabled = true;
                tbplyras.IsEnabled = true;
                tbplyrqu.IsEnabled = true;
                tbplyrgg.IsEnabled = true;
                tbplyrar.IsEnabled = true;
                Btnplyrgetloc.IsEnabled = true;
            }

            //get plyr values

            if (!tbplyrlocx.IsFocused || ignore_focus) tbplyrlocx.Text = new Global(GTA.Offsets.Editor.player_loc + 0 + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<float>().ToString();
            if (!tbplyrlocy.IsFocused || ignore_focus) tbplyrlocy.Text = new Global(GTA.Offsets.Editor.player_loc + 1 + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<float>().ToString();
            if (!tbplyrlocz.IsFocused || ignore_focus) tbplyrlocz.Text = new Global(GTA.Offsets.Editor.player_loc + 2 + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<float>().ToString();
            if (!tbplyrhead.IsFocused || ignore_focus) tbplyrhead.Text = new Global(GTA.Offsets.Editor.player_head + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<float>().ToString();
            if (!tbplyrbits.IsFocused || ignore_focus) tbplyrbits.Text = new Global(GTA.Offsets.Editor.player_bit + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrveh.IsFocused || ignore_focus) tbplyrveh.Text = new Global(GTA.Offsets.Editor.player_veh + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrtars.IsFocused || ignore_focus) tbplyrtars.Text = new Global(GTA.Offsets.Editor.player_tars + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrvfrs.IsFocused || ignore_focus) tbplyrvfrs.Text = new Global(GTA.Offsets.Editor.player_vfrs + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrvfre.IsFocused || ignore_focus) tbplyrvfre.Text = new Global(GTA.Offsets.Editor.player_vfre + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrty.IsFocused || ignore_focus) tbplyrty.Text = new Global(GTA.Offsets.Editor.player_ty + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyras.IsFocused || ignore_focus) tbplyras.Text = new Global(GTA.Offsets.Editor.player_as + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrqu.IsFocused || ignore_focus) tbplyrqu.Text = new Global(GTA.Offsets.Editor.player_qu + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrgg.IsFocused || ignore_focus) tbplyrgg.Text = new Global(GTA.Offsets.Editor.player_gg + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();
            if (!tbplyrar.IsFocused || ignore_focus) tbplyrar.Text = new Global(GTA.Offsets.Editor.player_ar + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>().ToString();

            bool enoughvehs = new Global(GTA.Offsets.Editor.Vehicle.number).Get<int>() > 0;
            ddplyrveh.IsEnabled = enoughvehs;
            if (enoughvehs)
            {
                if (!ddplyrveh.IsDropDownOpen || ignore_focus)
                {
                    //get vehicle index
                    int veh = new Global(GTA.Offsets.Editor.player_veh + (tindex * GTA.Offsets.Editor.team_NEXT_settings) + (index * GTA.Offsets.Editor.next_settings)).Get<int>();
                    try
                    {
                        //check if veh index is set higher than none 
                        ddplyrveh.SelectedIndex = veh > -1 ? veh + 1 : 0;
                    }
                    catch (Exception) //if calculated veh index is greater then dropdown item count
                    {
                        ddplyrveh.SelectedIndex = -1;
                    }
                }
            }
        }

        private void tbplyrhead_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.player_head + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetFloat(tbplyrhead.Text);
        }

        private void tbplyrlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.player_loc + 0 + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetFloat(tbplyrlocx.Text);
        }

        private void tbplyrlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.player_loc + 1 + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetFloat(tbplyrlocy.Text);
        }

        private void tbplyrlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.player_loc + 2 + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetFloat(tbplyrlocz.Text);
        }

        private void Btnplyrgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();
            tbplyrlocx.Text = loc[0];
            tbplyrlocy.Text = loc[1];
            tbplyrlocz.Text = loc[2];
        }

        private void ddplyrveh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddplyrveh.SelectedIndex != -1)
                new Global(GTA.Offsets.Editor.player_veh + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(ddplyrveh.SelectedIndex - 1);
        }

        private void tbplyrbits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrbits.Text))
                new Global(GTA.Offsets.Editor.player_bit + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrbits.Text);
        }

        private void tbplyrveh_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrveh.Text))
                new Global(GTA.Offsets.Editor.player_veh + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrveh.Text);
        }

        private void tbplyrtars_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrtars.Text))
                new Global(GTA.Offsets.Editor.player_tars + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrtars.Text);
        }

        private void tbplyrvfrs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrvfrs.Text))
                new Global(GTA.Offsets.Editor.player_vfrs + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrvfrs.Text);
        }

        private void tbplyrvfre_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrvfre.Text))
                new Global(GTA.Offsets.Editor.player_vfre + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrvfre.Text);
        }

        private void tbplyrty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrty.Text))
                new Global(GTA.Offsets.Editor.player_ty + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrty.Text);
        }

        private void tbplyras_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyras.Text))
                new Global(GTA.Offsets.Editor.player_as + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyras.Text);
        }

        private void tbplyrqu_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrqu.Text))
                new Global(GTA.Offsets.Editor.player_qu + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrqu.Text);
        }

        private void tbplyrgg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrgg.Text))
                new Global(GTA.Offsets.Editor.player_gg + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrgg.Text);
        }

        private void tbplyrar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrar.Text))
                new Global(GTA.Offsets.Editor.player_ar + (ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings) + (ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings)).SetInt(tbplyrar.Text);
        }

        private void tbplyrno_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbplyrno.Text, false))
            {
                new Global(GTA.Offsets.Editor.player_number + ddplyrteam.SelectedIndex).SetInt(tbplyrno.Text);
                refreshPLYRCount();
            }
        }

        public void refreshPLYRCount()
        {
            if (ddplyrteam == null)
                return;
            if (!m.IsProcOpen || ddplyrteam.SelectedIndex == -1)
                return;

            int plyrnonum = new Global(GTA.Offsets.Editor.player_number + ddplyrteam.SelectedIndex).Get<int>();

            if (ddplyrno.Items.Count != plyrnonum)
            {
                if (plyrnonum <= 0)
                {
                    ddplyrno.ItemsSource = null;
                }
                else
                {
                    if (plyrnonum < 61)
                    {
                        ddplyrno.ItemsSource = Enumerable.Range(1, plyrnonum);
                    }
                }
            }
        }
    }
}
