using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Props page.
    public partial class MainWindow
    {
        private void BtnDynamicProps_Click(object sender, RoutedEventArgs e)
        {
            PageInnerProps.SelectedItem = PageInnerDynamicProps;

            BtnNormalProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnModdedProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];

            BtnDynamicProps.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
        }

        private void BtnNormalProps_Click(object sender, RoutedEventArgs e)
        {
            PageInnerProps.SelectedItem = PageInnerNormalProps;

            BtnDynamicProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnModdedProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];

            BtnNormalProps.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
        }



        public void FloatPasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)) && sender is TextBox)
            {
                string pastedText = (e.DataObject.GetData(typeof(string)) as string);
                string result = pastedText.Replace(".", ",").Replace("?", "");
                result = Regex.Replace(result, "[^0-9\\-\\,]+", "");
                List<int> indexes = result.Select((b, i) => b.Equals(',') ? i : -1).Where(i => i != -1).ToList();
                int count = indexes.Count();
                int rem = 0;
            check:
                if (count > 1)
                {
                    result = result.Remove(rem <= 0 ? indexes.ElementAt(rem) : indexes.ElementAt(rem) - rem, 1);
                    rem++;
                    count--;
                    goto check;
                }


                DataObject d = new DataObject();
                d.SetData(DataFormats.Text, result);
                e.DataObject = d;

                e.Handled = true;
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void FormatTextForFloat(object sender, KeyEventArgs e)
        {
            DataObject.AddPastingHandler((DependencyObject)sender, new DataObjectPastingEventHandler(FloatPasteHandler));

            if (e.Key == Key.OemPeriod)
            {
                //handle the event and cancel the original key
                e.Handled = true;

                //get caret position
                int tbPos = (sender as TextBox).SelectionStart;

                //insert the new text at the caret position
                (sender as TextBox).Text = (sender as TextBox).Text.Insert(tbPos, ",");


                //replace the caret back to where it should be 
                //otherwise the insertion call above will reset the position
                (sender as TextBox).Select(tbPos + 1, 0);
            }

            if (char.IsDigit(GetCharFromKey(e.Key)) && ((sender as TextBox).Text.Count(x => Char.IsDigit(x)) > 7))
            {
                e.Handled = true;
            }

            if (GetCharFromKey(e.Key) == ',' && (sender as TextBox).Text.Contains(","))
            {
                // Stop more than one dot Char
                e.Handled = true;
            }
            else if (GetCharFromKey(e.Key) == ',' && (sender as TextBox).SelectionStart == 0)
            {
                // Stop first char as a dot input
                e.Handled = true;
            }
            else if (GetCharFromKey(e.Key) == '-' && (sender as TextBox).SelectionStart != 0)
            {
                // Stop first char as a dot input
                e.Handled = true;
            }
            else if (!char.IsControl(GetCharFromKey(e.Key)) && !char.IsDigit(GetCharFromKey(e.Key)) && GetCharFromKey(e.Key) != ',' && GetCharFromKey(e.Key) != '-')
            {
                // Stop allow other than digit and control
                e.Handled = true;
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                creatorRefresh();
            }
        }

        private void FormatTextForModel(object sender, KeyEventArgs e)
        {
            if (char.IsDigit(GetCharFromKey(e.Key)))
            {
                e.Handled = false;
            }
            else if (GetCharFromKey(e.Key) == '-' && (sender as TextBox).SelectionStart != 0)
            {
                e.Handled = true;
            }
            else if (!char.IsControl(GetCharFromKey(e.Key)) && !char.IsDigit(GetCharFromKey(e.Key)) && GetCharFromKey(e.Key) != '-')
            {
                e.Handled = true;
            }
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                creatorRefresh();
            }
        }




        private void ddpropmodel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            propModelList.DataContext = null;

            ObservableCollection<GTA.Prop> props = new ObservableCollection<GTA.Prop>();
            GTA.Editor.PropList.Where(x => x.Category == GTA.Editor.PropCategories[ddpropcategory.SelectedIndex]).ToList().ForEach(x => props.Add(x));

            //propModelList.Items.Cast<GTA.Prop>().Where(x => GTA.Editor.prop_model_blacklisted.Contains(x.Integer.ToString()))
            propModelList.DataContext = props;
        }

        private void ddpropscolor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddpropscolor.SelectedIndex > -1 && m.IsProcOpen)
            {

                if (((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
                {
                    for (int i = 0; i < new Global(GTA.Offsets.Editor.Props.number).Get<int>(); i++)
                    {
                        new Global(GTA.Offsets.Editor.Props.prpclr + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt(ddpropscolor.SelectedIndex - 1);
                    }
                    return;
                }

                new Global((GTA.Offsets.Editor.Props.prpclr + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(ddpropscolor.SelectedIndex - 1);
            }
        }

        private void ddpropsboosterspeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddpropsboosterspeed.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.prpsba + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(ddpropsboosterspeed.SelectedIndex + 1);
        }

        private void Btnpropsgetloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbpropslocx.Text = loc[0];
                tbpropslocy.Text = loc[1];
                tbpropslocz.Text = loc[2];
                creatorRefresh();
            }
        }

        private void propModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (propModelList.SelectedIndex > -1 && m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.Props.model + ddpropno.SelectedIndex * GTA.Offsets.Editor.Props.NEXT).SetInt((propModelList.SelectedItem as GTA.Prop).Integer);
                Lbl_props_blinfo.Visibility = GTA.Editor.prop_model_blacklisted.Contains((propModelList.SelectedItem as GTA.Prop).Integer.ToString()) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ddpropno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetProps(false, true);

            SelectActiveTextBox();
        }

        public void GetProps(bool skipcategorymodel = false, bool ignore_focus = false)
        {
            int index = ddpropno.SelectedIndex;

            ddpropmodelswicther.IsEnabled = index < 0 ? false : true;
            ddpropcategory.IsEnabled = index < 0 ? false : true;
            propModelList.IsEnabled = index < 0 ? false : true;
            Btnpropsgetloc.IsEnabled = index < 0 ? false : true;
            Btnpropsrotchain.IsEnabled = index < 0 ? false : true;
            tbpropsmodel.IsEnabled = index < 0 ? false : true;
            lblpropsmodelnm.IsEnabled = index < 0 ? false : true;
            tbpropslocx.IsEnabled = index < 0 ? false : true;
            tbpropslocy.IsEnabled = index < 0 ? false : true;
            tbpropslocz.IsEnabled = index < 0 ? false : true;
            tbpropsrotx.IsEnabled = index < 0 ? false : true;
            tbpropsroty.IsEnabled = index < 0 ? false : true;
            tbpropsrotz.IsEnabled = index < 0 ? false : true;
            cb_props_lockpos.IsEnabled = index < 0 ? false : true;
            cb_props_lockrot.IsEnabled = index < 0 ? false : true;
            tbpropshead.IsEnabled = index < 0 ? false : true;
            tbpropsrender.IsEnabled = index < 0 ? false : true;
            tbpropshas.IsEnabled = index < 0 ? false : true;
            tbpropssettings1.IsEnabled = index < 0 ? false : true;
            tbpropssettings2.IsEnabled = index < 0 ? false : true;
            ddpropsboosterspeed.IsEnabled = index < 0 ? false : true;
            ddpropscolor.IsEnabled = index < 0 ? false : true;
            cb_props_invisible.IsEnabled = index < 0 ? false : true;
            cb_props_ignorevscheck.IsEnabled = index < 0 ? false : true;
            ddpropsteamrlprio.IsEnabled = index < 0 ? false : true;
            tbpropsasso.IsEnabled = index < 0 ? false : true;
            tbpropsasst.IsEnabled = index < 0 ? false : true;
            tbpropsasss.IsEnabled = index < 0 ? false : true;
            tbpropspasc.IsEnabled = index < 0 ? false : true;
            tbpropsprpsdp.IsEnabled = index < 0 ? false : true;
            tbpropsprpcr.IsEnabled = index < 0 ? false : true;
            tbpropsprpct.IsEnabled = index < 0 ? false : true;
            tbpropsclearrule.IsEnabled = index < 0 ? false : true;
            ddpropsteamclear.IsEnabled = index < 0 ? false : true;
            ddpropsspawnon.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                int model = new Global(GTA.Offsets.Editor.Props.model + GTA.Offsets.Editor.Props.NEXT * index).Get<int>();

                if (!skipcategorymodel)
                {
                    if (ddpropmodelswicther.SelectedIndex == 1)
                    {
                        try
                        {
                            int modelindex = GTA.Editor.PropListID.IndexOf(model);
                            if (modelindex > -1)
                            {
                                int categoryindex = GTA.Editor.PropCategories.IndexOf(GTA.Editor.PropList[modelindex].Category);
                                if (categoryindex > -1)
                                {
                                    ddpropcategory.SelectedIndex = categoryindex;
                                    GTA.Prop[] temp = new GTA.Prop[propModelList.Items.Count];
                                    propModelList.Items.CopyTo(temp, 0);
                                    propModelList.SelectedIndex = propModelList.Items.IndexOf(temp.ToList().Where(x => x.Integer == model).First());
                                    propModelList.ScrollIntoView(propModelList.SelectedItem);
                                }
                            }
                            else
                            {
                                ddpropmodelswicther.SelectedIndex = 0;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                if (ddpropmodelswicther.SelectedIndex == 0)
                {
                    ddpropcategory.IsEnabled = false;
                }

                if (!tbpropsmodel.IsFocused || ignore_focus) tbpropsmodel.Text = model.ToString();
                if (!tbpropslocx.IsFocused || ignore_focus) tbpropslocx.Text = new Global((GTA.Offsets.Editor.Props.loc + 0 + GTA.Offsets.Editor.Props.NEXT * index)).Get<float>().ToString();
                if (!tbpropslocy.IsFocused || ignore_focus) tbpropslocy.Text = new Global((GTA.Offsets.Editor.Props.loc + 1 + GTA.Offsets.Editor.Props.NEXT * index)).Get<float>().ToString();
                if (!tbpropslocz.IsFocused || ignore_focus) tbpropslocz.Text = new Global((GTA.Offsets.Editor.Props.loc + 2 + GTA.Offsets.Editor.Props.NEXT * index)).Get<float>().ToString();
                if (!tbpropsrotx.IsFocused || ignore_focus) tbpropsrotx.Text = new Global((GTA.Offsets.Editor.Props.vrot + 0 + GTA.Offsets.Editor.Props.NEXT * index)).Get<float>().ToString();
                if (!tbpropsroty.IsFocused || ignore_focus) tbpropsroty.Text = new Global((GTA.Offsets.Editor.Props.vrot + 1 + GTA.Offsets.Editor.Props.NEXT * index)).Get<float>().ToString();
                if (!tbpropsrotz.IsFocused || ignore_focus) tbpropsrotz.Text = new Global((GTA.Offsets.Editor.Props.vrot + 2 + GTA.Offsets.Editor.Props.NEXT * index)).Get<float>().ToString();
                if (!tbpropshead.IsFocused || ignore_focus) tbpropshead.Text = new Global((GTA.Offsets.Editor.Props.head + GTA.Offsets.Editor.Props.NEXT * index)).Get<float>().ToString();
                if (!tbpropsrender.IsFocused || ignore_focus) tbpropsrender.Text = new Global((GTA.Offsets.Editor.Props.ploddist + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>().ToString();
                if (!tbpropshas.IsFocused || ignore_focus) tbpropshas.Text = new Global((GTA.Offsets.Editor.Props.fcuat + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>().ToString();
                if (!tbpropssettings1.IsFocused || ignore_focus) tbpropssettings1.Text = new Global((GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>().ToString();
                if (!tbpropssettings2.IsFocused || ignore_focus) tbpropssettings2.Text = new Global((GTA.Offsets.Editor.Props.prpbs2 + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>().ToString();

                Functions.Read.checkbinary(9, GTA.Offsets.Editor.Props.prpbs2 + GTA.Offsets.Editor.Props.NEXT * index, cb_props_nocollision);
                Functions.Read.checkbinary(10, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * index, cb_props_invisible);
                Functions.Read.checkbinary(2, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * index, cb_props_ignorevscheck);
                Functions.Read.checkbinary(4, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * index, cb_props_lockpos);
                Functions.Read.checkbinary(5, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * index, cb_props_lockrot);


                if (Functions.Read.isStuntPropWithColorOption(model))
                {
                    int color = new Global((GTA.Offsets.Editor.Props.prpclr + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>();

                    //ddpropscolor.SelectedIndex = (color >= 0 && color <= ddpropscolor.Items.Count - 2) ? (color + 1) : -1;
                    ddpropscolor.SelectedIndex = color + 1;

                    ddpropscolor.IsEnabled = true;
                }
                else
                {
                    ddpropscolor.SelectedIndex = 0;
                    ddpropscolor.IsEnabled = false;
                }

                if (Functions.Read.isBoosterProp(model) || Functions.Read.isSlowDownProp(model))
                {
                    int boosterspeed = new Global((GTA.Offsets.Editor.Props.prpsba + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>();
                    //ddpropsboosterspeed.SelectedIndex = (boosterspeed >= 0 && boosterspeed <= ddpropsboosterspeed.Items.Count - 2) ? (boosterspeed + 1) : -1;
                    ddpropsboosterspeed.SelectedIndex = (boosterspeed <= 0 || boosterspeed > ddpropsboosterspeed.Items.Count) ? -1 : boosterspeed - 1;
                    ddpropsboosterspeed.IsEnabled = true;
                }
                else
                {
                    ddpropsboosterspeed.SelectedIndex = -1;
                    ddpropsboosterspeed.IsEnabled = false;
                }

                int clrrule = new Global((GTA.Offsets.Editor.Props.prpcr + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>();
                int clrteam = new Global((GTA.Offsets.Editor.Props.prpct + GTA.Offsets.Editor.Props.NEXT * index)).Get<int>();

                if (!tbpropsprpcr.IsFocused || ignore_focus) tbpropsprpcr.Text = clrrule.ToString();
                if (!tbpropsprpct.IsFocused || ignore_focus) tbpropsprpct.Text = clrteam.ToString();

                if (!tbpropsclearrule.IsFocused || ignore_focus) tbpropsclearrule.Text = clrrule.ToString();
                if (!ddpropsteamclear.IsFocused || ignore_focus) ddpropsteamclear.SelectedIndex = clrteam + 1;


                GetPropsSpecialValues(ignore_focus);
            }
        }

        private void ddpropmodelswicther_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ddpropcategory != null)
                {
                    int model = new Global(GTA.Offsets.Editor.Props.model + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex).Get<int>();
                    int modelindex = GTA.Editor.PropListID.IndexOf(model);
                    if (ddpropmodelswicther.SelectedIndex == 0)
                    {
                        ddpropcategory.IsEnabled = false;
                        propModelList.Visibility = Visibility.Collapsed;
                        tbpropsmodel.Visibility = Visibility.Visible;
                        tbpropsmodel.Text = model.ToString();
                        lblpropsmodelnm.Text = GTA.Editor.PropList[modelindex].Name;
                    }
                    else
                    {
                        ddpropcategory.IsEnabled = true;
                        propModelList.Visibility = Visibility.Visible;
                        tbpropsmodel.Visibility = Visibility.Collapsed;

                        try
                        {
                            if (m.IsProcOpen)
                            {
                                if (modelindex > -1)
                                {
                                    int categoryindex = GTA.Editor.PropCategories.IndexOf(GTA.Editor.PropList[modelindex].Category);
                                    if (categoryindex > -1)
                                    {
                                        ddpropcategory.SelectedIndex = categoryindex;
                                        GTA.Prop[] temp = new GTA.Prop[propModelList.Items.Count];
                                        propModelList.Items.CopyTo(temp, 0);
                                        propModelList.SelectedIndex = propModelList.Items.IndexOf(temp.ToList().Where(x => x.Integer == model).First());
                                        propModelList.ScrollIntoView(propModelList.SelectedItem);
                                    }
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


        private void BtnPropsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int propnum = new Global(GTA.Offsets.Editor.Props.number).Get<int>();
                if (propnum < 200 && propnum > -1)
                {
                    int new_index = propnum + 1;

                    new Global(GTA.Offsets.Editor.Props.number).SetInt(new_index);
                    if (propsrotchainactive)
                    {
                        new Global((GTA.Offsets.Editor.Props.vrot + 0 + GTA.Offsets.Editor.Props.NEXT * (ddpropno.SelectedIndex + 1))).SetFloat(tbpropsrotx.Text);
                        new Global((GTA.Offsets.Editor.Props.vrot + 1 + GTA.Offsets.Editor.Props.NEXT * (ddpropno.SelectedIndex + 1))).SetFloat(tbpropsroty.Text);
                        new Global((GTA.Offsets.Editor.Props.vrot + 2 + GTA.Offsets.Editor.Props.NEXT * (ddpropno.SelectedIndex + 1))).SetFloat(tbpropsrotz.Text);
                    }
                    else
                    {
                        tbpropsrotx.Text = "";
                        tbpropsroty.Text = "";
                        tbpropsrotz.Text = "";
                    }

                    Global locx = new Global(GTA.Offsets.Editor.Props.loc + 0 + GTA.Offsets.Editor.Props.NEXT * (ddpropno.SelectedIndex + 1));
                    Global locy = new Global(GTA.Offsets.Editor.Props.loc + 1 + GTA.Offsets.Editor.Props.NEXT * (ddpropno.SelectedIndex + 1));
                    Global locz = new Global(GTA.Offsets.Editor.Props.loc + 2 + GTA.Offsets.Editor.Props.NEXT * (ddpropno.SelectedIndex + 1));

                    var loc = Functions.Read.getlocation();

                    if (locx.Get<float>() == 0)
                        locx.SetFloat(loc[0]);
                    if (locy.Get<float>() == 0)
                        locy.SetFloat(loc[1]);
                    if (locz.Get<float>() == 0)
                        locz.SetFloat(loc[2]);

                    ddpropno.SelectedIndex = new_index - 1;

                    if (new Global((GTA.Offsets.Editor.Props.model + GTA.Offsets.Editor.Props.NEXT * (ddpropno.SelectedIndex + 1))).Get<int>() != 0)
                    {
                        creatorRefresh();
                    }
                }
            }
        }

        private void tbpropslocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.loc + 0 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetFloat(tbpropslocx.Text);
        }

        private void tbpropslocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.loc + 1 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetFloat(tbpropslocy.Text);
        }

        private void tbpropslocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.loc + 2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetFloat(tbpropslocz.Text);
        }

        private void tbpropsrotx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.vrot + 0 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetFloat(tbpropsrotx.Text);
        }

        private void tbpropsroty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.vrot + 1 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetFloat(tbpropsroty.Text);
        }

        private void tbpropsrotz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.vrot + 2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetFloat(tbpropsrotz.Text);
        }

        private void tbpropshead_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Props.head + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetFloat(tbpropshead.Text);
        }

        private void tbpropsrender_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropsrender.Text))
                new Global((GTA.Offsets.Editor.Props.ploddist + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsrender.Text);
        }

        private void tbpropshas_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropshas.Text))
                new Global((GTA.Offsets.Editor.Props.fcuat + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropshas.Text);
        }

        private void tbpropsmodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (m.IsProcOpen)
                {
                    int model = Functions.int_parse(tbpropsmodel.Text);
                    new Global(GTA.Offsets.Editor.Props.model + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex).SetInt(model);
                    int modelindex = GTA.Editor.PropListID.IndexOf(model);
                    lblpropsmodelnm.Text = modelindex > -1 ? GTA.Editor.PropList[modelindex].Name : "idk";
                    Lbl_props_blinfo.Visibility = GTA.Editor.prop_model_blacklisted.Contains(Functions.int_parse(tbpropsmodel.Text).ToString()) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
        }

        private void tbpropssettings2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropssettings2.Text))
                new Global((GTA.Offsets.Editor.Props.prpbs2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropssettings2.Text);
        }

        private void tbpropssettings1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropssettings1.Text))
                new Global((GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropssettings1.Text);
        }

        private void tbpropsasso_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropsasso.Text))
            {
                if (ddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Props.asso + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasso.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Props.asso2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasso.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Props.asso3 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasso.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Props.asso4 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasso.Text);
                }
            }
        }

        private void tbpropsasst_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropsasst.Text))
            {
                if (ddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Props.asst + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasst.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Props.asst2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasst.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Props.asst3 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasst.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Props.asst4 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasst.Text);
                }
            }

        }

        private void tbpropsasss_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropsasss.Text))
            {
                if (ddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Props.asss + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasss.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Props.asss2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasss.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Props.asss3 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasss.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Props.asss4 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsasss.Text);
                }
            }
        }

        private void tbpropspasc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropspasc.Text))
            {
                if (ddpropsteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Props.pasc + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropspasc.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Props.pasc2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropspasc.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Props.pasc3 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropspasc.Text);
                }
                else if (ddpropsteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Props.pasc4 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropspasc.Text);
                }
            }
        }

        private void tbpropsprpsdp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbpropsprpsdp.Text))
            {
                new Global((GTA.Offsets.Editor.Props.prpsdp + ddpropsteamrlprio.SelectedIndex + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsprpsdp.Text);
            }
        }

        private void ddpropsteamrlprio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetPropsSpecialValues(true);
            SelectActiveTextBox();
        }

        public void GetPropsSpecialValues(bool ignore_focus = false)
        {
            if (ddpropsteamrlprio != null)
            {
                int index = ddpropsteamrlprio.SelectedIndex;
                if (m.IsProcOpen && index > -1)
                {
                    tbpropsprpsdp.Text = new Global((GTA.Offsets.Editor.Props.prpsdp + index + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>().ToString();

                    long asso, asss, asst, pasc;

                    if (index == 1)
                    {
                        asso = GTA.Offsets.Editor.Props.asso2;
                        asss = GTA.Offsets.Editor.Props.asss2;
                        asst = GTA.Offsets.Editor.Props.asst2;
                        pasc = GTA.Offsets.Editor.Props.pasc2;
                    }
                    else if (index == 2)
                    {
                        asso = GTA.Offsets.Editor.Props.asso3;
                        asss = GTA.Offsets.Editor.Props.asss3;
                        asst = GTA.Offsets.Editor.Props.asst3;
                        pasc = GTA.Offsets.Editor.Props.pasc3;
                    }
                    else if (index == 3)
                    {
                        asso = GTA.Offsets.Editor.Props.asso4;
                        asss = GTA.Offsets.Editor.Props.asss4;
                        asst = GTA.Offsets.Editor.Props.asst4;
                        pasc = GTA.Offsets.Editor.Props.pasc4;
                    }
                    else
                    {
                        asso = GTA.Offsets.Editor.Props.asso;
                        asss = GTA.Offsets.Editor.Props.asss;
                        asst = GTA.Offsets.Editor.Props.asst;
                        pasc = GTA.Offsets.Editor.Props.pasc;
                    }


                    if (!tbpropsasso.IsFocused || ignore_focus) tbpropsasso.Text = new Global((asso + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbpropsasst.IsFocused || ignore_focus) tbpropsasst.Text = new Global((asst + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbpropsasss.IsFocused || ignore_focus) tbpropsasss.Text = new Global((asss + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbpropspasc.IsFocused || ignore_focus) tbpropspasc.Text = new Global((pasc + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!tbpropsspawnrule.IsFocused || ignore_focus) tbpropsspawnrule.Text = new Global((asso + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>().ToString();
                    if (!ddpropsspawnteam.IsFocused || ignore_focus) ddpropsspawnteam.SelectedIndex = new Global((asst + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>() + 1;
                    if (!ddpropsspawnon.IsFocused || ignore_focus) ddpropsspawnon.SelectedIndex = new Global((asss + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).Get<int>();


                    if (ddpropsspawnon.SelectedIndex == 0)
                    {
                        ddpropsspawnteam.IsEnabled = false;
                        tbpropsspawnrule.IsEnabled = false;
                    }
                    else
                    {
                        ddpropsspawnteam.IsEnabled = true;
                        tbpropsspawnrule.IsEnabled = true;
                    }

                }
            }
        }

        private void cb_props_ignorevscheck_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex, cb_props_ignorevscheck);
        }

        private void cb_props_lockpos_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex, cb_props_lockpos);
        }

        private void cb_props_lockrot_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(5, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex, cb_props_lockrot);
        }

        private void tbpropsprpcr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbpropsprpcr.Text))
            {
                new Global((GTA.Offsets.Editor.Props.prpcr + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsprpcr.Text);
            }
        }

        private void tbpropsprpct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbpropsprpct.Text))
            {
                new Global((GTA.Offsets.Editor.Props.prpct + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsprpct.Text);
            }
        }

        private void cb_props_invisible_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex, false);
        }

        private void cb_props_invisible_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Props.prpbs + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex, true);
        }

        private void cb_props_nocollision_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(9, GTA.Offsets.Editor.Props.prpbs2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex, true);
        }

        private void cb_props_nocollision_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(9, GTA.Offsets.Editor.Props.prpbs2 + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex, false);
        }

        bool propsrotchainactive = false;
        private void Btnpropsrotchain_Click(object sender, RoutedEventArgs e)
        {
            Binding b = new Binding();
            if (propsrotchainactive)
            {
                propsrotchain.Source = (BitmapImage)FindResource("link2");
                b.Path = new PropertyPath("Translation[fixedrotn]");
            }
            else
            {
                propsrotchain.Source = (BitmapImage)FindResource("link1");
                b.Path = new PropertyPath("Translation[fixedroty]");
            }
            Btnpropsrotchain.SetBinding(Button.ToolTipProperty, b);
            propsrotchainactive = !propsrotchainactive;
        }

        private void ChangePropLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Props.number), GTA.Offsets.Editor.Props.loc + 0, GTA.Offsets.Editor.Props.NEXT);
        }

        private void ChangePropLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Props.number), GTA.Offsets.Editor.Props.loc + 1, GTA.Offsets.Editor.Props.NEXT);
        }

        private void ChangePropLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Props.number), GTA.Offsets.Editor.Props.loc + 2, GTA.Offsets.Editor.Props.NEXT);
        }

        private void BtnAdvancedPropPlacement_Click(object sender, RoutedEventArgs e)
        {
            PageInnerProps.SelectedItem = PageInnerAdvancedPropPlacement;
        }

        private void Btnpropshasexpand_Click(object sender, RoutedEventArgs e)
        {
            changeHasExpandButtonPath();
        }

        public void changeHasExpandButtonPath()
        {
            if (haslistcontainer.Visibility == Visibility.Collapsed)
            {
                pathpropshasexpand.Data = Geometry.Parse("M 0 0 L 6 6 L 12 0 Z");
                haslistcontainer.Visibility = Visibility.Visible;
            }
            else
            {
                pathpropshasexpand.Data = Geometry.Parse("M 4 0 L 0 4 L 4 8 Z");
                haslistcontainer.Visibility = Visibility.Collapsed;
            }
        }

        public void initializePropHasList()
        {
            int oldindex = 0;
            try
            {
                if (PropHASList.SelectedItem != null)
                {
                    oldindex = ((PropHAS)PropHASList.SelectedItem).Id;
                }

                List<PropHAS> templist = new List<PropHAS>();

                for (int i = 0; i < new Global(GTA.Offsets.Editor.Props.number).Get<int>() + 1; i++)
                {
                    int has = new Global(GTA.Offsets.Editor.Props.fcuat + i * GTA.Offsets.Editor.Props.NEXT).Get<int>();
                    if (has > 0)
                    {
                        int model = new Global(GTA.Offsets.Editor.Props.model + i * GTA.Offsets.Editor.Props.NEXT).Get<int>();
                        templist.Add(new PropHAS(i, has, model));
                    }
                }

                PropHASList.ItemsSource = templist;
                if (templist.Count > 0)
                {
                    Lblpropshasmax.Content = templist.Max(x => x.Time);
                    Lblpropshasmin.Content = templist.Min(x => x.Time);
                    bool exists = templist.Any(x => x.Id == oldindex);

                    if (exists)
                    {
                        PropHASList.SelectedItem = templist.First(x => x.Id == oldindex);
                    }
                    else
                    {
                        PropHASList.SelectedIndex = 0;
                    }

                }
            }
            catch (Exception)
            {
            }
        }

        public void IncreaseHasValue(int id)
        {
            if (IsValidInt(tbpropshasinc.Text, false))
            {
                var memobj = new Global(GTA.Offsets.Editor.Props.fcuat + id * GTA.Offsets.Editor.Props.NEXT);
                memobj.SetInt(memobj.Get<int>() + Functions.int_parse(tbpropshasinc.Text));
            }
            else
            {
                displayScreenMessage("could parse integer value");
            }
        }

        public void LowerHasValue(int id)
        {
            if (IsValidInt(tbpropshasinc.Text, false))
            {
                var memobj = new Global(GTA.Offsets.Editor.Props.fcuat + id * GTA.Offsets.Editor.Props.NEXT);
                int newvalue = memobj.Get<int>() - Functions.int_parse(tbpropshasinc.Text);
                if (newvalue < 0)
                    memobj.SetInt(0);
                else
                    memobj.SetInt(newvalue);
            }
            else
            {
                displayScreenMessage("could parse integer value");
            }
        }

        public void EditHasValue(int id)
        {
            ScreenMessage.MouseLeftButtonUp -= IMGBackground_MouseLeftButtonUp;

            Grid tempgrid = new Grid();
            tempgrid.Width = 300;
            tempgrid.Height = 100;
            RowDefinition row1 = new RowDefinition();
            row1.Height = new GridLength(0.5, GridUnitType.Star);
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(0.5, GridUnitType.Star);
            RowDefinition row3 = new RowDefinition();
            row3.Height = new GridLength(30, GridUnitType.Pixel);
            tempgrid.RowDefinitions.Add(row1);
            tempgrid.RowDefinitions.Add(row2);
            tempgrid.RowDefinitions.Add(row3);
            ColumnDefinition col1 = new ColumnDefinition();
            col1.Width = new GridLength(0.5, GridUnitType.Star);
            ColumnDefinition col2 = new ColumnDefinition();
            col2.Width = new GridLength(0.5, GridUnitType.Star);
            tempgrid.ColumnDefinitions.Add(col1);
            tempgrid.ColumnDefinitions.Add(col2);

            TextBox temptb = new TextBox();
            temptb.Margin = new Thickness(5, 0, 5, 0);
            temptb.Height = 30;
            temptb.Style = (Style)FindResource("Watermark");
            temptb.SetBinding(TagProperty, new Binding { Path = new PropertyPath("Translation[integer]"), FallbackValue = "Integer" });

            Border b1 = new Border();
            b1.CornerRadius = new CornerRadius(5);
            b1.Margin = new Thickness(5);
            b1.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            b1.Effect = new DropShadowEffect
            {
                ShadowDepth = 2,
                BlurRadius = 5
            };
            Border b2 = new Border();
            b2.CornerRadius = new CornerRadius(5);
            b2.Margin = new Thickness(5);
            b2.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            b2.Effect = new DropShadowEffect
            {
                ShadowDepth = 2,
                BlurRadius = 5
            };

            TextBlock tb1 = new TextBlock();
            tb1.Height = 20;
            tb1.HorizontalAlignment = HorizontalAlignment.Center;
            tb1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF04646"));
            tb1.Text = "";
            var dp = DependencyPropertyDescriptor.FromProperty(
             TextBlock.TextProperty,
             typeof(TextBlock));
            dp.AddValueChanged(tb1, async (sender, args) =>
            {
                await Task.Delay(5000);
                tb1.Text = "";
            });

            Button tempbtn = new Button();
            tempbtn.Style = (Style)FindResource("CustomButton");
            tempbtn.Content = "Set";
            tempbtn.SetValue(CornerRadiusSetter.CornerRadiusProperty, new CornerRadius(5));
            tempbtn.Click += delegate
            {
                if (IsValidInt(temptb.Text, false))
                {
                    new Global(GTA.Offsets.Editor.Props.fcuat + id * GTA.Offsets.Editor.Props.NEXT).SetInt(temptb.Text);
                    ScreenMessageContainer.Children.Remove(tempgrid);
                    ScreenMessage.MouseLeftButtonUp += IMGBackground_MouseLeftButtonUp;
                    ScreenMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tb1.Text = "could parse integer value";
                }
            };

            Button tempbtncancel = new Button();
            tempbtncancel.Style = (Style)FindResource("CustomButton");
            tempbtncancel.Content = "Cancel";
            tempbtncancel.SetValue(CornerRadiusSetter.CornerRadiusProperty, new CornerRadius(5));
            tempbtncancel.Click += delegate
            {
                ScreenMessageContainer.Children.Remove(tempgrid);
                ScreenMessage.MouseLeftButtonUp += IMGBackground_MouseLeftButtonUp;
                ScreenMessage.Visibility = Visibility.Collapsed;
            };



            b1.Child = tempbtn;
            b2.Child = tempbtncancel;
            tempgrid.Children.Add(temptb);
            tempgrid.Children.Add(b1);
            tempgrid.Children.Add(b2);
            tempgrid.Children.Add(tb1);

            Grid.SetRow(temptb, 0);
            Grid.SetColumnSpan(temptb, 2);
            Grid.SetRow(b1, 1);
            Grid.SetColumn(b1, 0);
            Grid.SetRow(b2, 1);
            Grid.SetColumn(b2, 1);
            Grid.SetRow(tb1, 2);
            Grid.SetColumnSpan(tb1, 2);


            ScreenMessageContainer.Children.Add(tempgrid);
            ScreenMessage.Visibility = Visibility.Visible;
            FocusManager.SetFocusedElement(tempgrid, temptb);
        }

        private void BtnPropsHasNV_Click(object sender, RoutedEventArgs e)
        {
            var item = PropHASList.SelectedItem;
            if (item != null)
            {
                EditHasValue(((PropHAS)item).Id);
            }
        }

        private void Btnpropshasplus_Click(object sender, RoutedEventArgs e)
        {
            var item = PropHASList.SelectedItem;
            if (item != null)
            {
                IncreaseHasValue(((PropHAS)item).Id);
            }
        }

        private void Btnpropshasminus_Click(object sender, RoutedEventArgs e)
        {
            var item = PropHASList.SelectedItem;
            if (item != null)
            {
                LowerHasValue(((PropHAS)item).Id);
            }
        }

        private void ddpropsteamclear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Props.prpct + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(ddpropsteamclear.SelectedIndex - 1);
        }

        private void tbpropsclearrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbpropsclearrule.Text))
            {
                new Global((GTA.Offsets.Editor.Props.prpcr + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex)).SetInt(tbpropsclearrule.Text);
            }
        }

        private void tbpropsspawnrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbpropsspawnrule.Text))
            {
                long asso;

                if (ddpropsteamrlprio.SelectedIndex == 1)
                {
                    asso = GTA.Offsets.Editor.Props.asso2;
                }
                else if (ddpropsteamrlprio.SelectedIndex == 2)
                {
                    asso = GTA.Offsets.Editor.Props.asso3;
                }
                else if (ddpropsteamrlprio.SelectedIndex == 3)
                {
                    asso = GTA.Offsets.Editor.Props.asso4;
                }
                else
                {
                    asso = GTA.Offsets.Editor.Props.asso;
                }

                new Global(asso + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex).SetInt(tbpropsspawnrule.Text);
            }
        }

        private void ddpropsspawnteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long asst;

            if (ddpropsteamrlprio.SelectedIndex == 1)
            {
                asst = GTA.Offsets.Editor.Props.asst2;
            }
            else if (ddpropsteamrlprio.SelectedIndex == 2)
            {
                asst = GTA.Offsets.Editor.Props.asst3;
            }
            else if (ddpropsteamrlprio.SelectedIndex == 3)
            {
                asst = GTA.Offsets.Editor.Props.asst4;
            }
            else
            {
                asst = GTA.Offsets.Editor.Props.asst;
            }

            new Global(asst + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex).SetInt(ddpropsspawnteam.SelectedIndex - 1);
        }

        private void ddpropsspawnon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long asss;

            if (ddpropsteamrlprio.SelectedIndex == 1)
            {
                asss = GTA.Offsets.Editor.Props.asss2;
            }
            else if (ddpropsteamrlprio.SelectedIndex == 2)
            {
                asss = GTA.Offsets.Editor.Props.asss3;
            }
            else if (ddpropsteamrlprio.SelectedIndex == 3)
            {
                asss = GTA.Offsets.Editor.Props.asss4;
            }
            else
            {
                asss = GTA.Offsets.Editor.Props.asss;
            }

            new Global(asss + GTA.Offsets.Editor.Props.NEXT * ddpropno.SelectedIndex).SetInt(ddpropsspawnon.SelectedIndex);
        }
    


        private Dictionary<int, string> _propNames;

        /// <summary>Readable name of a prop model ("Giant Wooden Block"), "" when unknown.</summary>
        private string PropDisplayName(int model)
        {
            if (_propNames == null || _propNames.Count == 0)
            {
                if (GTA.Editor.PropList == null || GTA.Editor.PropList.Count == 0)
                    return "";
                _propNames = new Dictionary<int, string>();
                foreach (GTA.Prop p in GTA.Editor.PropList)
                {
                    if (string.IsNullOrEmpty(p.Native)) continue;
                    string label = string.IsNullOrEmpty(p.Name) || p.Name == p.Native ? p.Native : $"{p.Name} ({p.Native})";
                    _propNames[p.Integer] = label;
                }
            }
            return _propNames.TryGetValue(model, out string name) ? name : "";
        }
}
}
