using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / Kill page.
    public partial class MainWindow
    {
        public Thread freezeKILLThread;

        private void tbkillrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkillrule.Text))
                kill.values[ddkillno.SelectedIndex].rule[ddkillteamno.SelectedIndex] = Convert.ToInt32(tbkillrule.Text);
        }

        private void tbkillpri_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkillpri.Text))
                kill.values[ddkillno.SelectedIndex].prio[ddkillteamno.SelectedIndex] = Convert.ToInt32(tbkillpri.Text);
        }

        private void tbkilllim_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkilllim.Text))
                kill.values[ddkillno.SelectedIndex].lim[ddkillteamno.SelectedIndex] = Convert.ToInt32(tbkilllim.Text);
        }

        private void tbkillprbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkillprbs.Text))
                kill.values[ddkillno.SelectedIndex].prbs[ddkillteamno.SelectedIndex] = Convert.ToInt32(tbkillprbs.Text);
        }

        private void tbkillmcf_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkillmcf.Text))
                kill.values[ddkillno.SelectedIndex].mcf = Convert.ToInt32(tbkillmcf.Text);
        }

        private void tbkillmcp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkillmcp.Text))
                kill.values[ddkillno.SelectedIndex].mcp = Convert.ToInt32(tbkillmcp.Text);
        }

        private void tbkillno_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkillno.Text))
                kill.number[ddkillteamno.SelectedIndex] = Convert.ToInt32(tbkillno.Text);
        }

        private void ddkillteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetKillValues();
            SelectActiveTextBox();
        }

        private void ddkillno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetKillValues();
            SelectActiveTextBox();
        }

        public void GetKillValues(bool ignore_focus = true)
        {
            if (!m.IsProcOpen)
                return;
            if (ddkillno == null || ddkillteamno == null)
                return;

            int tindex = ddkillteamno.SelectedIndex;
            int index = ddkillno.SelectedIndex;

            bool enabled = !((tindex > 3 || tindex < 0) || index == -1);

            tbkillno.IsEnabled = enabled ? true : false;
            tbkillrule.IsEnabled = enabled ? true : false;
            tbkillpri.IsEnabled = enabled ? true : false;
            tbkilllim.IsEnabled = enabled ? true : false;
            tbkilljtop.IsEnabled = enabled ? true : false;
            tbkilljtof.IsEnabled = enabled ? true : false;
            tbkillprbs.IsEnabled = enabled ? true : false;
            tbkillmcf.IsEnabled = enabled ? true : false;
            tbkillmcp.IsEnabled = enabled ? true : false;

            if (enabled)
            {
                if (!tbkillno.IsFocused || ignore_focus) tbkillno.Text = kill.number[tindex].ToString();
                if (!tbkillrule.IsFocused || ignore_focus) tbkillrule.Text = kill.values[index].rule[tindex].ToString();
                if (!tbkillpri.IsFocused || ignore_focus) tbkillpri.Text = kill.values[index].prio[tindex].ToString();
                if (!tbkilllim.IsFocused || ignore_focus) tbkilllim.Text = kill.values[index].lim[tindex].ToString();
                if (!tbkilljtop.IsFocused || ignore_focus) tbkilljtop.Text = kill.values[index].jtop[tindex].ToString();
                if (!tbkilljtof.IsFocused || ignore_focus) tbkilljtof.Text = kill.values[index].jtof[tindex].ToString();
                if (!tbkillprbs.IsFocused || ignore_focus) tbkillprbs.Text = kill.values[index].prbs[tindex].ToString();
                if (!tbkillmcf.IsFocused || ignore_focus) tbkillmcf.Text = kill.values[index].mcf.ToString();
                if (!tbkillmcp.IsFocused || ignore_focus) tbkillmcp.Text = kill.values[index].mcp.ToString();
            }
        }


        public static void freezeKILL()
        {
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.Kill.number + i).SetInt(kill.number[i]);
                }
                for (int d = 0; d < kill.values.Count(); d++)
                {
                    for (int e = 0; e < 4; e++)
                    {
                        new Global(GTA.Offsets.Editor.Kill.rule + e + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].rule[e]);
                        new Global(GTA.Offsets.Editor.Kill.pri + e + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].prio[e]);
                        new Global(GTA.Offsets.Editor.Kill.lim + e + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].lim[e]);
                        new Global(GTA.Offsets.Editor.Kill.jtop + e + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].jtop[e]);
                        new Global(GTA.Offsets.Editor.Kill.jtof + e + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].jtof[e]);
                        new Global(GTA.Offsets.Editor.Kill.prbs + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].prbs[e]);
                    }

                    new Global(GTA.Offsets.Editor.Kill.mcf + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].mcf);
                    new Global(GTA.Offsets.Editor.Kill.mcp + d * GTA.Offsets.Editor.Kill.NEXT).SetInt(kill.values[d].mcp);
                }
            }

        }

        private void cbmissionkillfreeze_Checked(object sender, RoutedEventArgs e)
        {
            bool ischecked = cbmissionkillfreeze.IsChecked ?? true;
            if (ischecked)
            {
                freezeKILLThread = new Thread(new ThreadStart(freezeKILL));
                freezeKILLThread.Priority = ThreadPriority.Highest;
                freezeKILLThread.IsBackground = true;
                freezeKILLThread.Start();
            }
            else
            {
                freezeKILLThread.Abort();
            }
        }

        private void Btnkillload_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                for (int i = 0; i < 4; i++)
                {
                    kill.number[i] = new Global(GTA.Offsets.Editor.Kill.number + i).Get<int>();
                }
                for (int d = 0; d < kill.values.Count(); d++)
                {
                    for (int g = 0; g < 4; g++)
                    {
                        kill.values[d].rule[g] = new Global(GTA.Offsets.Editor.Kill.rule + g + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                        kill.values[d].prio[g] = new Global(GTA.Offsets.Editor.Kill.pri + g + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                        kill.values[d].lim[g] = new Global(GTA.Offsets.Editor.Kill.lim + g + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                        kill.values[d].jtop[g] = new Global(GTA.Offsets.Editor.Kill.jtop + g + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                        kill.values[d].jtof[g] = new Global(GTA.Offsets.Editor.Kill.jtof + g + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                        kill.values[d].prbs[g] = new Global(GTA.Offsets.Editor.Kill.prbs + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                    }

                    kill.values[d].mcf = new Global(GTA.Offsets.Editor.Kill.mcf + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                    kill.values[d].mcp = new Global(GTA.Offsets.Editor.Kill.mcp + d * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                }
            }
        }
    }
}
