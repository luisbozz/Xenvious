using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Zones page.
    public partial class MainWindow
    {
        public void GetZone()
        {
            int index = ddzoneno.SelectedIndex;

            Btnzonegetstart.IsEnabled = index < 0 ? false : true;
            Btnzonegetend.IsEnabled = index < 0 ? false : true;
            tbzonestartx.IsEnabled = index < 0 ? false : true;
            tbzonestarty.IsEnabled = index < 0 ? false : true;
            tbzonestartz.IsEnabled = index < 0 ? false : true;
            tbzoneendx.IsEnabled = index < 0 ? false : true;
            tbzoneendy.IsEnabled = index < 0 ? false : true;
            tbzoneendz.IsEnabled = index < 0 ? false : true;
            tbzonewidth.IsEnabled = index < 0 ? false : true;
            tbzoneheight.IsEnabled = index < 0 ? false : true;
            tbzoneznwd.IsEnabled = index < 0 ? false : true;
            tbzoneznwvd.IsEnabled = index < 0 ? false : true;
            tbzonezntp.IsEnabled = index < 0 ? false : true;
            ddzonevariation.IsEnabled = index < 0 ? false : true;
            ddzoneteam.IsEnabled = index < 0 ? false : true;
            tbzonerule.IsEnabled = index < 0 ? false : true;
            tbzonepriority.IsEnabled = index < 0 ? false : true;
            tbzoneznbs.IsEnabled = index < 0 ? false : true;
            tbzoneznbs2.IsEnabled = index < 0 ? false : true;
            tbzoneznbs3.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                if (!tbzonezntp.IsFocused) tbzonezntp.Text = new Global(GTA.Offsets.Editor.Zones.zntp + GTA.Offsets.Editor.Zones.NEXT * index).Get<int>().ToString();
                if (!tbzonestartx.IsFocused) tbzonestartx.Text = new Global(GTA.Offsets.Editor.Zones.vtox + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzonestarty.IsFocused) tbzonestarty.Text = new Global(GTA.Offsets.Editor.Zones.vtoy + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzonestartz.IsFocused) tbzonestartz.Text = new Global(GTA.Offsets.Editor.Zones.vtoz + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzoneendx.IsFocused) tbzoneendx.Text = new Global(GTA.Offsets.Editor.Zones.vldx + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzoneendy.IsFocused) tbzoneendy.Text = new Global(GTA.Offsets.Editor.Zones.vldy + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzoneendz.IsFocused) tbzoneendz.Text = new Global(GTA.Offsets.Editor.Zones.vldz + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzonewidth.IsFocused) tbzonewidth.Text = new Global(GTA.Offsets.Editor.Zones.znwid + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzoneheight.IsFocused) tbzoneheight.Text = new Global(GTA.Offsets.Editor.Zones.znhei + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzoneznwd.IsFocused) tbzoneznwd.Text = new Global(GTA.Offsets.Editor.Zones.znwd + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!tbzoneznwvd.IsFocused) tbzoneznwvd.Text = new Global(GTA.Offsets.Editor.Zones.znwvd + GTA.Offsets.Editor.Zones.NEXT * index).Get<float>().ToString();
                if (!ddzonevariation.IsFocused) ddzonevariation.SelectedIndex = new Global(GTA.Offsets.Editor.Zones.znatp + GTA.Offsets.Editor.Zones.NEXT * index).Get<int>();
                if (!tbzoneznbs.IsFocused) tbzoneznbs.Text = new Global(GTA.Offsets.Editor.Zones.znbs + GTA.Offsets.Editor.Zones.NEXT * index).Get<int>().ToString();
                if (!tbzoneznbs2.IsFocused) tbzoneznbs2.Text = new Global(GTA.Offsets.Editor.Zones.znbs2 + GTA.Offsets.Editor.Zones.NEXT * index).Get<int>().ToString();
                if (!tbzoneznbs3.IsFocused) tbzoneznbs3.Text = new Global(GTA.Offsets.Editor.Zones.znbs3 + GTA.Offsets.Editor.Zones.NEXT * index).Get<int>().ToString();

                if (!tbzonerule.IsFocused) tbzonerule.Text = new Global(GTA.Offsets.Editor.Zones.znpr + 1 * ddzoneteam.SelectedIndex + GTA.Offsets.Editor.Zones.NEXT * index).Get<int>().ToString();
                if (!tbzonepriority.IsFocused) tbzonepriority.Text = new Global(GTA.Offsets.Editor.Zones.znepr + 1 * ddzoneteam.SelectedIndex + GTA.Offsets.Editor.Zones.NEXT * index).Get<int>().ToString();
            }
        }

        private void Btnzonegetstart_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbzonestartx.Text = loc[0];
                tbzonestarty.Text = loc[1];
                tbzonestartz.Text = loc[2];
            }
        }

        private void Btnzonegetend_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbzoneendx.Text = loc[0];
                tbzoneendy.Text = loc[1];
                tbzoneendz.Text = loc[2];
            }
        }

        private void tbzonestartx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.vtox + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzonestartx.Text);
        }

        private void tbzonestarty_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.vtoy + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzonestarty.Text);
        }

        private void tbzonestartz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.vtoz + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzonestartz.Text);
        }

        private void tbzoneendx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.vldx + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzoneendx.Text);
        }

        private void tbzoneendy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.vldy + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzoneendy.Text);
        }

        private void tbzoneendz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.vldz + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzoneendz.Text);
        }

        private void BtnzoneAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int zonenum = new Global(GTA.Offsets.Editor.Zones.number).Get<int>();
                if (zonenum < 32 && zonenum > -1)
                {
                    int new_index = zonenum + 1;

                    new Global(GTA.Offsets.Editor.Zones.number).SetInt(new_index);
                    ddzoneno.SelectedIndex = new_index - 1;
                }
            }
        }

        private void ddzoneno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetZone();
            SelectActiveTextBox();
        }

        private void tbzonewidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.znwid + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzonewidth.Text);
        }

        private void tbzoneheight_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.znhei + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzoneheight.Text);
        }

        private void tbzoneznwd_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.znwd + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzoneznwd.Text);
        }

        private void tbzoneznwvd_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.znwvd + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetFloat(tbzoneznwvd.Text);
        }

        private void ddzonevariation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.znatp + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetInt(ddzonevariation.SelectedIndex);
        }

        private void tbzonezntp_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Zones.zntp + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).SetInt(tbzonezntp.Text);
        }

        private void BtnZoneDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddzoneno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Zones.number).Get<int>();

                if (index > -1)
                {
                    try
                    {
                        List<List<int>> valuesafterdeletedzone = new List<List<int>>();

                        // get props after deleted zone
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Zones.NEXT; d++)
                            {
                                int test = new Global(((GTA.Offsets.Editor.Zones.vtox - 1) + ((i + 1) * GTA.Offsets.Editor.Zones.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeletedzone.Add(temp);
                        }

                        // clear deleted zone values

                        for (int i = 0; i < GTA.Offsets.Editor.Zones.NEXT; i++)
                        {
                            long deletedzonebase = (GTA.Offsets.Editor.Zones.vtox - 1) + (index * GTA.Offsets.Editor.Zones.NEXT);
                            new Global(deletedzonebase + i).SetInt(GTA.Defaults.Zone[i]);
                        }

                        // lower zone number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Zones.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeletedzone.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeletedzone[i].Count(); d++)
                            {
                                long zonebase = (GTA.Offsets.Editor.Zones.vtox - 1) + ((index + i) * GTA.Offsets.Editor.Zones.NEXT);
                                new Global(zonebase + d).SetInt(valuesafterdeletedzone[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void ddzoneteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddzoneno.SelectedIndex > -1 && ddzoneteam.SelectedIndex > -1)
            {
                tbzonerule.Text = new Global((GTA.Offsets.Editor.Zones.znpr + 1 * ddzoneteam.SelectedIndex + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).Get<int>().ToString();
                tbzonepriority.Text = new Global((GTA.Offsets.Editor.Zones.znepr + 1 * ddzoneteam.SelectedIndex + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex)).Get<int>().ToString();
                SelectActiveTextBox();
            }
        }

        private void tbzonerule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbzonerule.Text))
                new Global(GTA.Offsets.Editor.Zones.znpr + 1 * ddzoneteam.SelectedIndex + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex).SetInt(tbzonerule.Text);
        }

        private void tbzonepriority_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbzonepriority.Text))
                new Global(GTA.Offsets.Editor.Zones.znepr + 1 * ddzoneteam.SelectedIndex + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex).SetInt(tbzonepriority.Text);
        }

        private void tbzoneznbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbzoneznbs.Text))
                new Global(GTA.Offsets.Editor.Zones.znbs + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex).SetInt(tbzoneznbs.Text);
        }

        private void tbzoneznbs2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbzoneznbs2.Text))
                new Global(GTA.Offsets.Editor.Zones.znbs2 + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex).SetInt(tbzoneznbs2.Text);
        }

        private void tbzoneznbs3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbzoneznbs3.Text))
                new Global(GTA.Offsets.Editor.Zones.znbs3 + GTA.Offsets.Editor.Zones.NEXT * ddzoneno.SelectedIndex).SetInt(tbzoneznbs3.Text);
        }

        private void ChangeZoneLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Zones.number), GTA.Offsets.Editor.Zones.vtox, GTA.Offsets.Editor.Zones.NEXT);
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Zones.number), GTA.Offsets.Editor.Zones.vldx, GTA.Offsets.Editor.Zones.NEXT);
        }

        private void ChangeZoneLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Zones.number), GTA.Offsets.Editor.Zones.vtoy, GTA.Offsets.Editor.Zones.NEXT);
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Zones.number), GTA.Offsets.Editor.Zones.vldy, GTA.Offsets.Editor.Zones.NEXT);
        }

        private void ChangeZoneLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Zones.number), GTA.Offsets.Editor.Zones.vtoz, GTA.Offsets.Editor.Zones.NEXT);
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Zones.number), GTA.Offsets.Editor.Zones.vldz, GTA.Offsets.Editor.Zones.NEXT);
        }
    }
}
