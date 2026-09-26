using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: DynamicProps page.
    public partial class MainWindow
    {
        private void Btndpropsgetloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();
                tbdpropslocx.Text = loc[0];
                tbdpropslocy.Text = loc[1];
                tbdpropslocz.Text = loc[2];
                creatorRefresh();
            }
        }

        private void tbdpropslocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.loc + 0 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropslocx.Text);
        }

        private void tbdpropslocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.loc + 1 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropslocy.Text);
        }

        private void tbdpropslocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.loc + 2 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropslocz.Text);
        }

        private void tbdpropsrotx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.vrot + 0 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropsrotx.Text);
        }

        private void tbdpropsroty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.vrot + 1 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropsroty.Text);
        }

        private void tbdpropsrotz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.vrot + 2 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropsrotz.Text);
        }

        private void tbdpropshead_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.head + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropshead.Text);
        }

        private void tbdpropssettings1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdpropssettings1.Text))
                new Global((GTA.Offsets.Editor.DProps.prpbs + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropssettings1.Text);
        }

        private void dddpropscolor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dddpropscolor.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.DProps.prpdclr + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(dddpropscolor.SelectedIndex - 1);
        }

        private void dddpropmodelswicther_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dddpropcategory != null)
                {
                    if (dddpropmodelswicther.SelectedIndex == 0)
                    {
                        dddpropcategory.IsEnabled = false;
                        dpropModelList.Visibility = Visibility.Collapsed;
                        tbdpropsmodel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dddpropcategory.IsEnabled = true;
                        dpropModelList.Visibility = Visibility.Visible;
                        tbdpropsmodel.Visibility = Visibility.Collapsed;

                        try
                        {
                            int model = new Global(GTA.Offsets.Editor.DProps.model + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex).Get<int>();
                            int modelindex = GTA.Editor.PropListID.IndexOf(model);
                            if (modelindex > -1)
                            {
                                int categoryindex = GTA.Editor.PropCategories.IndexOf(GTA.Editor.PropList[modelindex].Category);
                                if (categoryindex > -1)
                                {
                                    dddpropcategory.SelectedIndex = categoryindex;
                                    dddpropmodelswicther.SelectedIndex = 1;
                                    GTA.Prop[] temp = new GTA.Prop[dpropModelList.Items.Count];
                                    dpropModelList.Items.CopyTo(temp, 0);
                                    dpropModelList.SelectedIndex = dpropModelList.Items.IndexOf(temp.ToList().Where(x => x.Integer == model).First());
                                    dpropModelList.ScrollIntoView(dpropModelList.SelectedItem);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void dddpropcategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dpropModelList.DataContext = null;

            ObservableCollection<GTA.Prop> props = new ObservableCollection<GTA.Prop>();
            GTA.Editor.PropList.Where(x => x.Category == GTA.Editor.PropCategories[dddpropcategory.SelectedIndex]).ToList().ForEach(x => props.Add(x));

            dpropModelList.DataContext = props;
        }

        private void tbdpropsmodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbdpropsmodel.Text, true))
            {
                new Global((GTA.Offsets.Editor.DProps.model + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsmodel.Text);
                Lbl_dprops_blinfo.Visibility = GTA.Editor.prop_model_blacklisted.Contains(Functions.int_parse(tbdpropsmodel.Text).ToString()) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void dpropModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpropModelList.SelectedIndex > -1 && m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.DProps.model + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex).SetInt((dpropModelList.SelectedItem as GTA.Prop).Integer);
                Lbl_dprops_blinfo.Visibility = GTA.Editor.prop_model_blacklisted.Contains((dpropModelList.SelectedItem as GTA.Prop).Integer.ToString()) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Btndpropsadd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int propnum = new Global(GTA.Offsets.Editor.DProps.number).Get<int>();
                if (propnum < 32 && propnum > -1)
                {
                    int new_index = propnum + 1;

                    new Global(GTA.Offsets.Editor.DProps.number).SetInt(new_index);
                    new Global((GTA.Offsets.Editor.DProps.vrot + 0 + GTA.Offsets.Editor.DProps.NEXT * (dddpropno.SelectedIndex + 1))).SetFloat(tbdpropsrotx.Text);
                    new Global((GTA.Offsets.Editor.DProps.vrot + 1 + GTA.Offsets.Editor.DProps.NEXT * (dddpropno.SelectedIndex + 1))).SetFloat(tbdpropsroty.Text);
                    new Global((GTA.Offsets.Editor.DProps.vrot + 2 + GTA.Offsets.Editor.DProps.NEXT * (dddpropno.SelectedIndex + 1))).SetFloat(tbdpropsrotz.Text);
                    dddpropno.SelectedIndex = new_index - 1;

                    if (new Global((GTA.Offsets.Editor.DProps.model + GTA.Offsets.Editor.DProps.NEXT * (dddpropno.SelectedIndex + 1))).Get<int>() != 0)
                    {
                        creatorRefresh();
                    }
                }
            }
        }

        private void dddpropno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetDProps(false, true);
            SelectActiveTextBox();
        }


        public void GetDProps(bool skipcategorymodel = false, bool ignore_focus = false)
        {
            int index = dddpropno.SelectedIndex;

            dddpropmodelswicther.IsEnabled = index < 0 ? false : true;
            dddpropcategory.IsEnabled = index < 0 ? false : true;
            dpropModelList.IsEnabled = index < 0 ? false : true;
            Btndpropsgetloc.IsEnabled = index < 0 ? false : true;
            Btndpropsrotchain.IsEnabled = index < 0 ? false : true;
            tbdpropsmodel.IsEnabled = index < 0 ? false : true;
            tbdpropslocx.IsEnabled = index < 0 ? false : true;
            tbdpropslocy.IsEnabled = index < 0 ? false : true;
            tbdpropslocz.IsEnabled = index < 0 ? false : true;
            tbdpropsrotx.IsEnabled = index < 0 ? false : true;
            tbdpropsroty.IsEnabled = index < 0 ? false : true;
            tbdpropsrotz.IsEnabled = index < 0 ? false : true;
            tbdpropshead.IsEnabled = index < 0 ? false : true;
            tbdpropssettings1.IsEnabled = index < 0 ? false : true;
            tbdpropsprpcr.IsEnabled = index < 0 ? false : true;
            tbdpropsprpct.IsEnabled = index < 0 ? false : true;
            tbdpropsasso.IsEnabled = index < 0 ? false : true;
            tbdpropsasst.IsEnabled = index < 0 ? false : true;
            tbdpropsasss.IsEnabled = index < 0 ? false : true;
            tbdpropspasc.IsEnabled = index < 0 ? false : true;
            dddpropsteamrlprio.IsEnabled = index < 0 ? false : true;
            cb_dprops_ignorevscheck.IsEnabled = index < 0 ? false : true;
            tbdpropsclearrule.IsEnabled = index < 0 ? false : true;
            dddpropsteamclear.IsEnabled = index < 0 ? false : true;
            dddpropsspawnon.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                int model = new Global(GTA.Offsets.Editor.DProps.model + GTA.Offsets.Editor.DProps.NEXT * index).Get<int>();

                if (!skipcategorymodel)
                {

                    if (dddpropmodelswicther.SelectedIndex == 1)
                    {
                        try
                        {
                            int modelindex = GTA.Editor.PropListID.IndexOf(model);
                            if (modelindex > -1)
                            {
                                int categoryindex = GTA.Editor.PropCategories.IndexOf(GTA.Editor.PropList[modelindex].Category);
                                if (categoryindex > -1)
                                {
                                    dddpropcategory.SelectedIndex = categoryindex;
                                    dddpropmodelswicther.SelectedIndex = 1;
                                    GTA.Prop[] temp = new GTA.Prop[dpropModelList.Items.Count];
                                    dpropModelList.Items.CopyTo(temp, 0);
                                    dpropModelList.SelectedIndex = dpropModelList.Items.IndexOf(temp.ToList().Where(x => x.Integer == model).First());
                                    dpropModelList.ScrollIntoView(dpropModelList.SelectedItem);
                                }
                            }
                            else
                            {
                                dddpropmodelswicther.SelectedIndex = 0;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                if (dddpropmodelswicther.SelectedIndex == 0)
                {
                    dddpropcategory.IsEnabled = false;
                }

                if (!tbdpropsmodel.IsFocused || ignore_focus) tbdpropsmodel.Text = model.ToString();
                DPropModelCard.SetModel(unchecked((uint)model));
                if (!tbdpropslocx.IsFocused || ignore_focus) tbdpropslocx.Text = new Global((GTA.Offsets.Editor.DProps.loc + 0 + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>().ToString();
                if (!tbdpropslocy.IsFocused || ignore_focus) tbdpropslocy.Text = new Global((GTA.Offsets.Editor.DProps.loc + 1 + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>().ToString();
                if (!tbdpropslocz.IsFocused || ignore_focus) tbdpropslocz.Text = new Global((GTA.Offsets.Editor.DProps.loc + 2 + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>().ToString();
                if (!tbdpropsrotx.IsFocused || ignore_focus) tbdpropsrotx.Text = new Global((GTA.Offsets.Editor.DProps.vrot + 0 + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>().ToString();
                if (!tbdpropsroty.IsFocused || ignore_focus) tbdpropsroty.Text = new Global((GTA.Offsets.Editor.DProps.vrot + 1 + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>().ToString();
                if (!tbdpropsrotz.IsFocused || ignore_focus) tbdpropsrotz.Text = new Global((GTA.Offsets.Editor.DProps.vrot + 2 + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>().ToString();
                if (!tbdpropshead.IsFocused || ignore_focus) tbdpropshead.Text = new Global((GTA.Offsets.Editor.DProps.head + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>().ToString();
                if (!tbdpropssettings1.IsFocused || ignore_focus) tbdpropssettings1.Text = new Global((GTA.Offsets.Editor.DProps.prpbs + GTA.Offsets.Editor.DProps.NEXT * index)).Get<int>().ToString();

                if (Functions.Read.isStuntPropWithColorOption(model))
                {
                    int color = new Global((GTA.Offsets.Editor.DProps.prpdclr + GTA.Offsets.Editor.DProps.NEXT * index)).Get<int>();

                    //dddpropscolor.SelectedIndex = (color >= 0 && color <= dddpropscolor.Items.Count - 2) ? (color + 1) : -1;
                    dddpropscolor.SelectedIndex = color + 1;

                    dddpropscolor.IsEnabled = true;
                }
                else
                {
                    ddpropscolor.SelectedIndex = 0;
                    ddpropscolor.IsEnabled = false;
                }

                Functions.Read.checkbinary(2, GTA.Offsets.Editor.DProps.prpbs + GTA.Offsets.Editor.DProps.NEXT * index, cb_dprops_ignorevscheck);

                if (Functions.Read.isDPropActivationTimer(model))
                {
                    float timer = new Global((GTA.Offsets.Editor.DProps.dptrpx + GTA.Offsets.Editor.DProps.NEXT * index)).Get<float>();

                    if (!tbdpropsdptrpx.IsFocused || ignore_focus) tbdpropsdptrpx.Text = timer.ToString();
                    tbdpropsdptrpx.IsEnabled = true;
                }
                else
                {
                    tbdpropsdptrpx.IsEnabled = false;
                }

                int clrrule = new Global((GTA.Offsets.Editor.DProps.prpcr + GTA.Offsets.Editor.DProps.NEXT * index)).Get<int>();
                int clrteam = new Global((GTA.Offsets.Editor.DProps.prpct + GTA.Offsets.Editor.DProps.NEXT * index)).Get<int>();

                if (!tbdpropsprpcr.IsFocused || ignore_focus) tbdpropsprpcr.Text = clrrule.ToString();
                if (!tbdpropsprpct.IsFocused || ignore_focus) tbdpropsprpct.Text = clrteam.ToString();

                if (!tbdpropsclearrule.IsFocused || ignore_focus) tbdpropsclearrule.Text = clrrule.ToString();
                if (!dddpropsteamclear.IsFocused || ignore_focus) dddpropsteamclear.SelectedIndex = clrteam + 1;

                GetDPropsSpecialValues(ignore_focus);
            }
        }


        private void tbdpropsprpcr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdpropsprpcr.Text))
                new Global((GTA.Offsets.Editor.DProps.prpcr + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsprpcr.Text);
        }

        private void tbdpropsprpct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdpropsprpct.Text))
                new Global((GTA.Offsets.Editor.DProps.prpct + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsprpct.Text);
        }

        private void dddpropsteamrlprio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetDPropsSpecialValues(true);
            SelectActiveTextBox();
        }

        private void tbdpropsasso_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbdpropsasso.Text))
            {
                if (dddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.DProps.asso + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasso.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.DProps.asso2 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasso.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.DProps.asso3 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasso.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.DProps.asso4 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasso.Text);
                }
            }
        }

        private void tbdpropsasst_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbdpropsasst.Text))
            {
                if (dddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.DProps.asst + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasst.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.DProps.asst2 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasst.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.DProps.asst3 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasst.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.DProps.asst4 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasst.Text);
                }
            }
        }

        private void tbdpropsasss_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbdpropsasss.Text))
            {
                if (dddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.DProps.asss + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasss.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.DProps.asss2 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasss.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.DProps.asss3 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasss.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.DProps.asss4 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsasss.Text);
                }
            }
        }

        private void tbdpropspasc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbdpropspasc.Text))
            {
                if (dddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.DProps.pasc + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropspasc.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.DProps.pasc2 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropspasc.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.DProps.pasc3 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropspasc.Text);
                }
                else if (dddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.DProps.pasc4 + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropspasc.Text);
                }
            }
        }

        public void GetDPropsSpecialValues(bool ignore_focus = false)
        {
            if (dddpropsteamrlprio != null)
            {
                int index = dddpropsteamrlprio.SelectedIndex;
                if (m.IsProcOpen && index > -1)
                {
                    long asso, asss, asst, pasc;

                    if (index == 1)
                    {
                        asso = GTA.Offsets.Editor.DProps.asso2;
                        asss = GTA.Offsets.Editor.DProps.asss2;
                        asst = GTA.Offsets.Editor.DProps.asst2;
                        pasc = GTA.Offsets.Editor.DProps.pasc2;
                    }
                    else if (index == 2)
                    {
                        asso = GTA.Offsets.Editor.DProps.asso3;
                        asss = GTA.Offsets.Editor.DProps.asss3;
                        asst = GTA.Offsets.Editor.DProps.asst3;
                        pasc = GTA.Offsets.Editor.DProps.pasc3;
                    }
                    else if (index == 3)
                    {
                        asso = GTA.Offsets.Editor.DProps.asso4;
                        asss = GTA.Offsets.Editor.DProps.asss4;
                        asst = GTA.Offsets.Editor.DProps.asst4;
                        pasc = GTA.Offsets.Editor.DProps.pasc4;
                    }
                    else
                    {
                        asso = GTA.Offsets.Editor.DProps.asso;
                        asss = GTA.Offsets.Editor.DProps.asss;
                        asst = GTA.Offsets.Editor.DProps.asst;
                        pasc = GTA.Offsets.Editor.DProps.pasc;
                    }


                    if (!tbdpropsasso.IsFocused || ignore_focus) tbdpropsasso.Text = new Global((asso + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbdpropsasst.IsFocused || ignore_focus) tbdpropsasst.Text = new Global((asst + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbdpropsasss.IsFocused || ignore_focus) tbdpropsasss.Text = new Global((asss + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbdpropspasc.IsFocused || ignore_focus) tbdpropspasc.Text = new Global((pasc + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbdpropsspawnrule.IsFocused || ignore_focus) tbdpropsspawnrule.Text = new Global((asso + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!dddpropsspawnteam.IsFocused || ignore_focus) dddpropsspawnteam.SelectedIndex = new Global((asst + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).Get<int>() + 1;
                    if (!dddpropsspawnon.IsFocused || ignore_focus) dddpropsspawnon.SelectedIndex = new Global((asss + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).Get<int>();


                    if (dddpropsspawnon.SelectedIndex == 0)
                    {
                        dddpropsspawnteam.IsEnabled = false;
                        tbdpropsspawnrule.IsEnabled = false;
                    }
                    else
                    {
                        dddpropsspawnteam.IsEnabled = true;
                        tbdpropsspawnrule.IsEnabled = true;
                    }
                }
            }
        }

        private void cb_dprops_ignorevscheck_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.DProps.prpbs + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex, cb_dprops_ignorevscheck);
        }

        private void tbdpropsdptrpx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbdpropsdptrpx.Text, true))
                new Global((GTA.Offsets.Editor.DProps.dptrpx + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetFloat(tbdpropsdptrpx.Text);
        }
        bool dpropsrotchainactive = false;

        private void Btndpropsrotchain_Click(object sender, RoutedEventArgs e)
        {
            Binding b = new Binding();
            if (dpropsrotchainactive)
            {
                dpropsrotchain.Source = (BitmapImage)FindResource("link2");
                b.Path = new PropertyPath("Translation[fixedrotn]");
            }
            else
            {
                dpropsrotchain.Source = (BitmapImage)FindResource("link1");
                b.Path = new PropertyPath("Translation[fixedroty]");
            }
            Btndpropsrotchain.SetBinding(Button.ToolTipProperty, b);
            dpropsrotchainactive = !dpropsrotchainactive;
        }

        private void ChangeDPropLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.DProps.number), GTA.Offsets.Editor.DProps.loc + 0, GTA.Offsets.Editor.DProps.NEXT);
        }

        private void ChangeDPropLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.DProps.number), GTA.Offsets.Editor.DProps.loc + 1, GTA.Offsets.Editor.DProps.NEXT);
        }

        private void ChangeDPropLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.DProps.number), GTA.Offsets.Editor.DProps.loc + 2, GTA.Offsets.Editor.DProps.NEXT);
        }

        private void dddpropsteamclear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.DProps.prpct + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(dddpropsteamclear.SelectedIndex - 1);
        }

        private void tbdpropsclearrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdpropsclearrule.Text))
            {
                new Global((GTA.Offsets.Editor.DProps.prpcr + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex)).SetInt(tbdpropsclearrule.Text);
            }
        }

        private void tbdpropsspawnrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdpropsspawnrule.Text))
            {
                long asso;

                if (dddpropsteamrlprio.SelectedIndex == 1)
                {
                    asso = GTA.Offsets.Editor.DProps.asso2;
                }
                else if (dddpropsteamrlprio.SelectedIndex == 2)
                {
                    asso = GTA.Offsets.Editor.DProps.asso3;
                }
                else if (dddpropsteamrlprio.SelectedIndex == 3)
                {
                    asso = GTA.Offsets.Editor.DProps.asso4;
                }
                else
                {
                    asso = GTA.Offsets.Editor.DProps.asso;
                }

                new Global(asso + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex).SetInt(tbdpropsspawnrule.Text);
            }
        }

        private void dddpropsspawnteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long asst;

            if (dddpropsteamrlprio.SelectedIndex == 1)
            {
                asst = GTA.Offsets.Editor.DProps.asst2;
            }
            else if (dddpropsteamrlprio.SelectedIndex == 2)
            {
                asst = GTA.Offsets.Editor.DProps.asst3;
            }
            else if (dddpropsteamrlprio.SelectedIndex == 3)
            {
                asst = GTA.Offsets.Editor.DProps.asst4;
            }
            else
            {
                asst = GTA.Offsets.Editor.DProps.asst;
            }

            new Global(asst + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex).SetInt(dddpropsspawnteam.SelectedIndex - 1);
        }

        private void dddpropsspawnon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long asss;

            if (dddpropsteamrlprio.SelectedIndex == 1)
            {
                asss = GTA.Offsets.Editor.DProps.asss2;
            }
            else if (dddpropsteamrlprio.SelectedIndex == 2)
            {
                asss = GTA.Offsets.Editor.DProps.asss3;
            }
            else if (dddpropsteamrlprio.SelectedIndex == 3)
            {
                asss = GTA.Offsets.Editor.DProps.asss4;
            }
            else
            {
                asss = GTA.Offsets.Editor.DProps.asss;
            }

            new Global(asss + GTA.Offsets.Editor.DProps.NEXT * dddpropno.SelectedIndex).SetInt(dddpropsspawnon.SelectedIndex);
        }
    }
}
