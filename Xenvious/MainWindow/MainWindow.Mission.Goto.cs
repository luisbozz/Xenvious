using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / Goto page.
    public partial class MainWindow
    {
        public void GetGoto()
        {
            int index = ddgotono.SelectedIndex;

            Btngotogetstart.IsEnabled = index < 0 ? false : true;
            Btngotogetend.IsEnabled = index < 0 ? false : true;
            Btngotogetloc.IsEnabled = index < 0 ? false : true;
            tbgotolocx.IsEnabled = index < 0 ? false : true;
            tbgotolocy.IsEnabled = index < 0 ? false : true;
            tbgotolocz.IsEnabled = index < 0 ? false : true;
            tbgotostartx.IsEnabled = index < 0 ? false : true;
            tbgotostarty.IsEnabled = index < 0 ? false : true;
            tbgotostartz.IsEnabled = index < 0 ? false : true;
            tbgotoendx.IsEnabled = index < 0 ? false : true;
            tbgotoendy.IsEnabled = index < 0 ? false : true;
            tbgotoendz.IsEnabled = index < 0 ? false : true;
            tbgotowidth.IsEnabled = index < 0 ? false : true;
            //tbgotoheight.IsEnabled = index < 0 ? false : true;
            ddgotovariation.IsEnabled = index < 0 ? false : true;
            ddgototeam.IsEnabled = index < 0 ? false : true;
            tbgotorule.IsEnabled = index < 0 ? false : true;
            tbgotopriority.IsEnabled = index < 0 ? false : true;
            tbgotolbs.IsEnabled = index < 0 ? false : true;
            tbgotolcbs2.IsEnabled = index < 0 ? false : true;
            tbgotolcbs3.IsEnabled = index < 0 ? false : true;
            tbgotolocaaw.IsEnabled = index < 0 ? false : true;
            tbgotolocstd.IsEnabled = index < 0 ? false : true;
            tbgotoloclbr.IsEnabled = index < 0 ? false : true;
            tbgotolocdir.IsEnabled = index < 0 ? false : true;
            tbgotoloctol.IsEnabled = index < 0 ? false : true;
            tbgotoloc2rd.IsEnabled = index < 0 ? false : true;
            tbgotocmp.IsEnabled = index < 0 ? false : true;
            tbgotogps.IsEnabled = index < 0 ? false : true;
            tbgotowtm.IsEnabled = index < 0 ? false : true;
            tbgotosz.IsEnabled = index < 0 ? false : true;
            tbgotodir.IsEnabled = index < 0 ? false : true;
            cbgotosdt1.IsEnabled = index < 0 ? false : true;
            cbgotosdt2.IsEnabled = index < 0 ? false : true;
            cbgotosdt3.IsEnabled = index < 0 ? false : true;
            cbgotosdt4.IsEnabled = index < 0 ? false : true;
            cbgotoexitveh.IsEnabled = index < 0 ? false : true;
            cbgotostopveh.IsEnabled = index < 0 ? false : true;
            cbgotosncp.IsEnabled = index < 0 ? false : true;
            cbgotobf.IsEnabled = index < 0 ? false : true;
            cbgotocf.IsEnabled = index < 0 ? false : true;
            ddgotocpvariation.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                tbgotostartx.Text = new Global(GTA.Offsets.Editor.Locations.locaa1x + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotostarty.Text = new Global(GTA.Offsets.Editor.Locations.locaa1y + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotostartz.Text = new Global(GTA.Offsets.Editor.Locations.locaa1z + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotoendx.Text = new Global(GTA.Offsets.Editor.Locations.locaa2x + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotoendy.Text = new Global(GTA.Offsets.Editor.Locations.locaa2y + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotoendz.Text = new Global(GTA.Offsets.Editor.Locations.locaa2z + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotolocx.Text = new Global(GTA.Offsets.Editor.Locations.locx + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotolocy.Text = new Global(GTA.Offsets.Editor.Locations.locy + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotolocz.Text = new Global(GTA.Offsets.Editor.Locations.locz + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();

                tbgotowidth.Text = new Global(GTA.Offsets.Editor.Locations.locaaw + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                //tbgotoheight.Text = (new Global(GTA.Offsets.Editor.Locations.locaa1z + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>() - new Global(GTA.Offsets.Editor.Locations.locaa2z + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>()).ToString();
                ddgotovariation.SelectedIndex = new Global(GTA.Offsets.Editor.Locations.locart + GTA.Offsets.Editor.Locations.NEXT * index).Get<int>();

                Functions.Read.checkbinary(10, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index, cbgotosdt1);
                Functions.Read.checkbinary(11, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index, cbgotosdt2);
                Functions.Read.checkbinary(12, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index, cbgotosdt3);
                Functions.Read.checkbinary(13, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index, cbgotosdt4);
                Functions.Read.checkbinary(12, GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * index, cbgotoexitveh);
                Functions.Read.checkbinary(4, GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * index, cbgotostopveh);
                Functions.Read.checkbinary(6, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index, cbgotosncp);
                Functions.Read.checkbinary(19, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index, cbgotobf);
                Functions.Read.checkbinary(2, GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * index, cbgotocf);

                int iVar0 = 0;
                while (iVar0 < 6)
                {
                    if (Functions.Read.checkbinary(iVar0, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index))
                    {
                        ddgotocpvariation.SelectedIndex = iVar0;
                        break;
                    }
                    iVar0++;
                }
                if (iVar0 == 6)
                {
                    ddgotocpvariation.SelectedIndex = -1;
                }

                tbgotolocaaw.Text = new Global(GTA.Offsets.Editor.Locations.locaaw + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotolocstd.Text = new Global(GTA.Offsets.Editor.Locations.locstd + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotoloclbr.Text = new Global(GTA.Offsets.Editor.Locations.loclbr + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotolocdir.Text = new Global(GTA.Offsets.Editor.Locations.locdir + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotoloctol.Text = new Global(GTA.Offsets.Editor.Locations.loctol + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotoloc2rd.Text = new Global(GTA.Offsets.Editor.Locations.loc2rd + GTA.Offsets.Editor.Locations.NEXT * index).Get<float>().ToString();
                tbgotolbs.Text = new Global(GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * index).Get<int>().ToString();
                tbgotolcbs2.Text = new Global(GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * index).Get<int>().ToString();
                tbgotolcbs3.Text = new Global(GTA.Offsets.Editor.Locations.lcbs3 + GTA.Offsets.Editor.Locations.NEXT * index).Get<int>().ToString();

                tbgotorule.Text = new Global(GTA.Offsets.Editor.Locations.rule + GTA.Offsets.Editor.Locations.NEXT * index + ddgototeam.SelectedIndex).Get<int>().ToString();
                tbgotopriority.Text = new Global(GTA.Offsets.Editor.Locations.pri + GTA.Offsets.Editor.Locations.NEXT * index + ddgototeam.SelectedIndex).Get<int>().ToString();
                tbgotocmp.Text = new Global(GTA.Offsets.Editor.Locations.cmp + GTA.Offsets.Editor.Locations.NEXT * index + ddgototeam.SelectedIndex).Get<int>().ToString();
                tbgotogps.Text = new Global(GTA.Offsets.Editor.Locations.gps + GTA.Offsets.Editor.Locations.NEXT * index + ddgototeam.SelectedIndex).Get<int>().ToString();
                tbgotowtm.Text = new Global(GTA.Offsets.Editor.Locations.wtm + GTA.Offsets.Editor.Locations.NEXT * index + ddgototeam.SelectedIndex).Get<int>().ToString();
                tbgotosz.Text = new Global(GTA.Offsets.Editor.Locations.sz + GTA.Offsets.Editor.Locations.NEXT * index + ddgototeam.SelectedIndex).Get<float>().ToString();
                tbgotodir.Text = new Global(GTA.Offsets.Editor.Locations.dir + GTA.Offsets.Editor.Locations.NEXT * index + ddgototeam.SelectedIndex).Get<float>().ToString();
            }
        }

        private void tbgotostartx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaa1x + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotostartx.Text);
        }

        private void tbgotostarty_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaa1y + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotostarty.Text);
        }

        private void tbgotostartz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaa1z + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotostartz.Text);
        }

        private void Btngotogetstart_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbgotostartx.Text = loc[0].ToString();
            tbgotostarty.Text = loc[1].ToString();
            tbgotostartz.Text = loc[2].ToString();
        }

        private void tbgotoendx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaa2x + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotoendx.Text);
        }

        private void tbgotoendy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaa2y + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotoendy.Text);
        }

        private void tbgotoendz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaa2z + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotoendz.Text);
        }

        private void Btngotogetend_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbgotoendx.Text = loc[0].ToString();
            tbgotoendy.Text = loc[1].ToString();
            tbgotoendz.Text = loc[2].ToString();
        }

        private void ddgototeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                tbgotorule.Text = new Global(GTA.Offsets.Editor.Locations.rule + ddgototeam.SelectedIndex + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).Get<int>().ToString();
                tbgotopriority.Text = new Global(GTA.Offsets.Editor.Locations.pri + ddgototeam.SelectedIndex + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).Get<int>().ToString();
                tbgotocmp.Text = new Global(GTA.Offsets.Editor.Locations.cmp + ddgototeam.SelectedIndex + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).Get<int>().ToString();
                tbgotogps.Text = new Global(GTA.Offsets.Editor.Locations.gps + ddgototeam.SelectedIndex + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).Get<int>().ToString();
                tbgotowtm.Text = new Global(GTA.Offsets.Editor.Locations.wtm + ddgototeam.SelectedIndex + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).Get<int>().ToString();
                tbgotosz.Text = new Global(GTA.Offsets.Editor.Locations.sz + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).Get<float>().ToString();
                tbgotodir.Text = new Global(GTA.Offsets.Editor.Locations.dir + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).Get<float>().ToString();
                SelectActiveTextBox();
            }
        }

        private void tbgotorule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbgotorule.Text))
                new Global(GTA.Offsets.Editor.Locations.rule + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetInt(tbgotorule.Text);
        }

        private void tbgotopriority_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbgotopriority.Text))
                new Global(GTA.Offsets.Editor.Locations.pri + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetInt(tbgotopriority.Text);
        }

        private void tbgotolocaaw_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaaw + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotolocaaw.Text);
        }

        private void tbgotolocdir_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locdir + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotolocdir.Text);
        }

        private void tbgotoloctol_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.loctol + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotoloctol.Text);
        }

        private void tbgotolbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbgotolbs.Text))
                new Global(GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetInt(tbgotolbs.Text);
        }

        private void tbgotolcbs2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbgotolcbs2.Text))
                new Global(GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetInt(tbgotolcbs2.Text);
        }

        private void tbgotolcbs3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbgotolcbs3.Text))
                new Global(GTA.Offsets.Editor.Locations.lcbs3 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetInt(tbgotolcbs3.Text);
        }

        private void Btngotogetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbgotolocx.Text = loc[0].ToString();
            tbgotolocy.Text = loc[1].ToString();
            tbgotolocz.Text = loc[2].ToString();
        }

        private void tbgotolocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locx + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotolocx.Text);
        }

        private void tbgotolocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locy + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotolocy.Text);
        }

        private void tbgotolocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locz + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotolocz.Text);
        }

        private void tbgotocmp_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.cmp + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetInt(tbgotocmp.Text);
        }

        private void tbgotogps_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.gps + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetInt(tbgotogps.Text);
        }

        private void ddgotono_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetGoto();
            SelectActiveTextBox();
        }

        private void tbgotowtm_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.wtm + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetInt(tbgotowtm.Text);
        }

        private void tbgotosz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.sz + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetFloat(tbgotosz.Text);
        }

        private void tbgotodir_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.dir + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetFloat(tbgotodir.Text);
        }

        private void tbgotoloc2rd_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.loc2rd + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex + ddgototeam.SelectedIndex).SetFloat(tbgotoloc2rd.Text);
        }

        private void ddgotovariation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locart + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetInt(ddgotovariation.SelectedIndex);
        }

        private void tbgotoheight_TextChanged(object sender, TextChangedEventArgs e)
        {
            //tbgotoendz.Text = (Convert.ToSingle(tbgotoendz.Text) + Convert.ToSingle(tbgotoheight.Text)).ToString();
        }

        private void tbgotowidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locaaw + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotowidth.Text);
        }


        private void BtngotAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int gotonum = new Global(GTA.Offsets.Editor.Locations.number).Get<int>();
                if (gotonum < 20 && gotonum > -1)
                {
                    int new_index = gotonum + 1;

                    new Global(GTA.Offsets.Editor.Locations.number).SetInt(new_index);
                    ddgotono.SelectedIndex = new_index - 1;
                }
            }
        }

        private void BtngotoDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddgotono.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Locations.number).Get<int>();

                if (index > -1)
                {
                    try
                    {
                        List<List<int>> valuesafterdeletedgoto = new List<List<int>>();

                        // get props after deleted goto
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Locations.NEXT; d++)
                            {
                                int test = new Global(((GTA.Offsets.Editor.Locations.locart) + ((i + 1) * GTA.Offsets.Editor.Locations.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeletedgoto.Add(temp);
                        }

                        // clear deleted goto values

                        for (int i = 0; i < GTA.Offsets.Editor.Locations.NEXT; i++)
                        {
                            long deletedgotobase = (GTA.Offsets.Editor.Locations.locart) + (index * GTA.Offsets.Editor.Locations.NEXT);
                            new Global(deletedgotobase + i).SetInt(GTA.Defaults.Location[i]);
                        }

                        // lower goto number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Locations.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeletedgoto.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeletedgoto[i].Count(); d++)
                            {
                                long gotobase = (GTA.Offsets.Editor.Locations.locart) + ((index + i) * GTA.Offsets.Editor.Locations.NEXT);
                                new Global(gotobase + d).SetInt(valuesafterdeletedgoto[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void ChangeGotoLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locx, GTA.Offsets.Editor.Locations.NEXT);
        }

        private void ChangeGotoLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locy, GTA.Offsets.Editor.Locations.NEXT);
        }

        private void ChangeGotoLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locz, GTA.Offsets.Editor.Locations.NEXT);
        }

        private void ChangeGoto2LocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locaa1x, GTA.Offsets.Editor.Locations.NEXT);
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locaa2x, GTA.Offsets.Editor.Locations.NEXT);
        }

        private void ChangeGoto2LocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locaa1y, GTA.Offsets.Editor.Locations.NEXT);
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locaa2y, GTA.Offsets.Editor.Locations.NEXT);
        }

        private void ChangeGoto2LocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locaa1z, GTA.Offsets.Editor.Locations.NEXT);
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Locations.number), GTA.Offsets.Editor.Locations.locaa2z, GTA.Offsets.Editor.Locations.NEXT);
        }

        private void tbgotolocstd_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.locstd + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotolocstd.Text);
        }

        private void tbgotoloclbr_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Locations.loclbr + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex).SetFloat(tbgotoloclbr.Text);
        }

        private void cbgotosdt1_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotosdt1);
        }

        private void cbgotosdt2_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotosdt2);
        }

        private void cbgotosdt3_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotosdt3);
        }

        private void cbgotosdt4_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotosdt4);
        }

        private void cbgotoexitveh_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotoexitveh);
        }

        private void cbgotostopveh_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotostopveh);
        }

        private void cbgotosncp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(6, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotosncp);
        }

        private void cbgotobf_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotobf);
        }

        private void cbgotocf_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.Locations.lbs + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex, cbgotocf);
        }

        private void ddgotocpvariation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddgotocpvariation.SelectedIndex != -1)
                Functions.Write.writebinary(ddgotocpvariation.SelectedIndex, GTA.Offsets.Editor.Locations.lcbs2 + GTA.Offsets.Editor.Locations.NEXT * ddgotono.SelectedIndex);
        }
    }
}
