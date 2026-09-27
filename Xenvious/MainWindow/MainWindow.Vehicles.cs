using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Vehicles page.
    public partial class MainWindow
    {
        private void ddvehcol2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddvehcol2.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.Vehicle.col2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetInt(((VehicleColors)ddvehcol2.SelectedItem).id);
        }

        private void ddvehcol1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddvehcol1.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.Vehicle.col + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetInt(((VehicleColors)ddvehcol1.SelectedItem).id);
        }

        private void cb_veh_freeze_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(25, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_freeze);
        }
        private void cb_veh_remove_windows_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 20; i < 29; i++)
            {
                Functions.Write.writebinary(i, GTA.Offsets.Editor.Vehicle.vbs3 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_remove_windows);
            }
        }

        private void ddvehcategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vehModelList.DataContext = null;

            ObservableCollection<GTA.Vehicle> vehicles = new ObservableCollection<GTA.Vehicle>();
            GTA.Editor.VehList.Where(x => x.Category == GTA.Editor.VehCategories[ddvehcategory.SelectedIndex]).ToList().ForEach(x => vehicles.Add(x));

            vehModelList.DataContext = vehicles;
        }

        private void ddvehmodelswicther_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ddvehcategory != null)
                {
                    if (ddvehmodelswicther.SelectedIndex == 0)
                    {
                        ddvehcategory.IsEnabled = false;
                        vehModelList.Visibility = Visibility.Collapsed;
                        tbvehmodel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ddvehcategory.IsEnabled = true;
                        vehModelList.Visibility = Visibility.Visible;
                        tbvehmodel.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void tbvehlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.loc + 2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehlocz.Text);
        }

        private void tbvehlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.loc + 1 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehlocy.Text);
        }

        private void tbvehlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.loc + 0 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehlocx.Text);
        }

        private void tbvehrotx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.vrot + 0 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehrotx.Text);
        }

        private void tbvehroty_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.vrot + 1 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehroty.Text);
        }

        private void tbvehrotz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.vrot + 2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehrotz.Text);
        }

        private void ddvehwindowtint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddvehwindowtint.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.Vehicle.vehwtci + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetInt(GTA.Editor.window_value[ddvehwindowtint.SelectedIndex].ToString());
        }

        private void tbvehhead_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.head + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehhead.Text);
        }

        private void Btnvehgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbvehlocx.Text = loc[0];
            tbvehlocy.Text = loc[1];
            tbvehlocz.Text = loc[2];
            creatorRefresh();
        }

        private void ddvehno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetVehicle(false, true);
            SelectActiveTextBox();
        }

        public void GetVehicle(bool skipcategorymodel = false, bool ignore_focus = false)
        {
            int index = ddvehno.SelectedIndex;

            ddvehmodelswicther.IsEnabled = index < 0 ? false : true;
            ddvehcategory.IsEnabled = index < 0 ? false : true;
            vehModelList.IsEnabled = index < 0 ? false : true;
            Btnvehgetloc.IsEnabled = index < 0 ? false : true;
            tbvehmodel.IsEnabled = index < 0 ? false : true;
            tbvehlocx.IsEnabled = index < 0 ? false : true;
            tbvehlocy.IsEnabled = index < 0 ? false : true;
            tbvehlocz.IsEnabled = index < 0 ? false : true;
            tbvehrotx.IsEnabled = index < 0 ? false : true;
            tbvehroty.IsEnabled = index < 0 ? false : true;
            tbvehrotz.IsEnabled = index < 0 ? false : true;
            tbvehhead.IsEnabled = index < 0 ? false : true;
            ddvehvbs.IsEnabled = index < 0 ? false : true;
            tbvehvbs.IsEnabled = index < 0 ? false : true;
            tbvehvebs.IsEnabled = index < 0 ? false : true;
            tbvehdrbs.IsEnabled = index < 0 ? false : true;
            tbvehiconsize.IsEnabled = index < 0 ? false : true;
            tbvehlivery.IsEnabled = index < 0 ? false : true;
            ddvehcol1.IsEnabled = index < 0 ? false : true;
            ddvehcol2.IsEnabled = index < 0 ? false : true;
            ddvehwindowtint.IsEnabled = index < 0 ? false : true;
            ddvehicon.IsEnabled = index < 0 ? false : true;
            cb_veh_freeze.IsEnabled = index < 0 ? false : true;
            cb_veh_remove_windows.IsEnabled = index < 0 ? false : true;
            cb_veh_bptires.IsEnabled = index < 0 ? false : true;
            ddvehrsp.IsEnabled = index < 0 ? false : true;
            tbvehvrr.IsEnabled = index < 0 ? false : true;
            cb_veh_engine.IsEnabled = index < 0 ? false : true;
            cb_veh_godmode.IsEnabled = index < 0 ? false : true;
            cb_veh_lights.IsEnabled = index < 0 ? false : true;
            cb_veh_locveh.IsEnabled = index < 0 ? false : true;
            cb_veh_locvehforp.IsEnabled = index < 0 ? false : true;
            cb_veh_mark.IsEnabled = index < 0 ? false : true;
            cb_veh_neon.IsEnabled = index < 0 ? false : true;
            cb_veh_sirens.IsEnabled = index < 0 ? false : true;
            cb_veh_sirens_audio.IsEnabled = index < 0 ? false : true;
            cb_veh_nottargetable.IsEnabled = index < 0 ? false : true;
            cb_veh_box.IsEnabled = index < 0 ? false : true;
            cb_veh_explodeinwater.IsEnabled = index < 0 ? false : true;
            cb_veh_lockteam1.IsEnabled = index < 0 ? false : true;
            cb_veh_lockteam2.IsEnabled = index < 0 ? false : true;
            cb_veh_lockteam3.IsEnabled = index < 0 ? false : true;
            cb_veh_lockteam4.IsEnabled = index < 0 ? false : true;

            cb_veh_door_close_hood.IsEnabled = index < 0 ? false : true;
            cb_veh_door_close_trunk.IsEnabled = index < 0 ? false : true;
            cb_veh_door_close_fl.IsEnabled = index < 0 ? false : true;
            cb_veh_door_close_fr.IsEnabled = index < 0 ? false : true;
            cb_veh_door_close_rl.IsEnabled = index < 0 ? false : true;
            cb_veh_door_close_rr.IsEnabled = index < 0 ? false : true;

            cb_veh_door_open_hood.IsEnabled = index < 0 ? false : true;
            cb_veh_door_open_trunk.IsEnabled = index < 0 ? false : true;
            cb_veh_door_open_fl.IsEnabled = index < 0 ? false : true;
            cb_veh_door_open_fr.IsEnabled = index < 0 ? false : true;
            cb_veh_door_open_rl.IsEnabled = index < 0 ? false : true;
            cb_veh_door_open_rr.IsEnabled = index < 0 ? false : true;

            ddvehteamrlprio.IsEnabled = index < 0 ? false : true;
            tbvehrule.IsEnabled = index < 0 ? false : true;
            tbvehpriority.IsEnabled = index < 0 ? false : true;
            tbvehjtop.IsEnabled = index < 0 ? false : true;
            tbvehjtof.IsEnabled = index < 0 ? false : true;
            tbvehobjt.IsEnabled = index < 0 ? false : true;
            tbvehteam.IsEnabled = index < 0 ? false : true;
            tbvehspwn.IsEnabled = index < 0 ? false : true;
            tbvehspsrc.IsEnabled = index < 0 ? false : true;
            tbvehspasr.IsEnabled = index < 0 ? false : true;
            tbvehvehcr.IsEnabled = index < 0 ? false : true;
            tbvehvehct.IsEnabled = index < 0 ? false : true;
            ddvehteamclear.IsEnabled = index < 0 ? false : true;
            tbvehclearrule.IsEnabled = index < 0 ? false : true;
            ddvehspawnon.IsEnabled = index < 0 ? false : true;
            cb_veh_clrlivc.IsEnabled = index < 0 ? false : true;
            cb_veh_spwnrlivc.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                int model = new Global(GTA.Offsets.Editor.Vehicle.model + GTA.Offsets.Editor.Vehicle.NEXT * index).Get<int>();

                if (!skipcategorymodel)
                {
                    if (ddvehmodelswicther.SelectedIndex == 1)
                    {
                        try
                        {
                            int modelindex = GTA.Editor.VehListID.IndexOf(model);
                            if (modelindex > -1)
                            {
                                int categoryindex = GTA.Editor.VehCategories.IndexOf(GTA.Editor.VehList[modelindex].Category);
                                if (categoryindex > -1)
                                {
                                    ddvehcategory.IsEnabled = true;
                                    ddvehcategory.SelectedIndex = categoryindex;
                                    ddvehmodelswicther.SelectedIndex = 1;
                                    GTA.Vehicle[] temp = new GTA.Vehicle[vehModelList.Items.Count];
                                    vehModelList.Items.CopyTo(temp, 0);
                                    vehModelList.SelectedIndex = vehModelList.Items.IndexOf(temp.ToList().Where(x => x.Int32 == model).First());
                                    vehModelList.ScrollIntoView(vehModelList.SelectedItem);
                                }
                                else
                                {
                                    ddvehcategory.IsEnabled = false;
                                    ddvehmodelswicther.SelectedIndex = 0;
                                }
                            }
                            else
                            {
                                ddvehcategory.IsEnabled = false;
                                ddvehmodelswicther.SelectedIndex = 0;
                            }
                        }
                        catch (Exception)
                        {
                            ddvehcategory.IsEnabled = false;
                            ddvehmodelswicther.SelectedIndex = 0;
                        }
                    }
                }

                if (ddvehmodelswicther.SelectedIndex == 0)
                {
                    ddvehmodelswicther.IsEnabled = false;
                }


                if (!tbvehmodel.IsFocused || ignore_focus) tbvehmodel.Text = model.ToString();
                if (!tbvehlocx.IsFocused || ignore_focus) tbvehlocx.Text = new Global((GTA.Offsets.Editor.Vehicle.loc + 0 + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehlocy.IsFocused || ignore_focus) tbvehlocy.Text = new Global((GTA.Offsets.Editor.Vehicle.loc + 1 + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehlocz.IsFocused || ignore_focus) tbvehlocz.Text = new Global((GTA.Offsets.Editor.Vehicle.loc + 2 + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehrotx.IsFocused || ignore_focus) tbvehrotx.Text = new Global((GTA.Offsets.Editor.Vehicle.vrot + 0 + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehroty.IsFocused || ignore_focus) tbvehroty.Text = new Global((GTA.Offsets.Editor.Vehicle.vrot + 1 + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehrotz.IsFocused || ignore_focus) tbvehrotz.Text = new Global((GTA.Offsets.Editor.Vehicle.vrot + 2 + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehhead.IsFocused || ignore_focus) tbvehhead.Text = new Global((GTA.Offsets.Editor.Vehicle.head + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehlivery.IsFocused || ignore_focus) tbvehlivery.Text = new Global((GTA.Offsets.Editor.Vehicle.liv + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();
                if (!tbvehenghp.IsFocused || ignore_focus) tbvehenghp.Text = new Global((GTA.Offsets.Editor.Vehicle.enghp + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehptrhp.IsFocused || ignore_focus) tbvehptrhp.Text = new Global((GTA.Offsets.Editor.Vehicle.ptrhp + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehbdyhp.IsFocused || ignore_focus) tbvehbdyhp.Text = new Global((GTA.Offsets.Editor.Vehicle.bdyhp + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();
                if (!tbvehhlth.IsFocused || ignore_focus) tbvehhlth.Text = new Global((GTA.Offsets.Editor.Vehicle.hlth + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();
                if (!tbvehvebs.IsFocused || ignore_focus) tbvehvebs.Text = new Global((GTA.Offsets.Editor.Vehicle.vebs + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();
                if (!tbvehdrbs.IsFocused || ignore_focus) tbvehdrbs.Text = new Global((GTA.Offsets.Editor.Vehicle.drbs + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();

                if (ddvehvbs.SelectedIndex > -1)
                {
                    tbvehvbs.IsEnabled = true;
                    if (!tbvehvbs.IsFocused || ignore_focus) tbvehvbs.Text = new Global(GTA.Offsets.Editor.Vehicle.vbs2 + (1 * ddvehvbs.SelectedIndex) + (GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                }
                else
                {
                    tbvehvbs.IsEnabled = false;
                }

                try
                {
                    ddvehcol1.SelectedItem = ddvehcol1.ItemsSource.Cast<VehicleColors>().ToArray().Where(y => y.id == new Global((GTA.Offsets.Editor.Vehicle.col + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>()).First();
                }
                catch (Exception)
                {
                    ddvehcol1.SelectedIndex = 0;
                }
                try
                {
                    ddvehcol2.SelectedItem = ddvehcol2.ItemsSource.Cast<VehicleColors>().ToArray().Where(y => y.id == new Global((GTA.Offsets.Editor.Vehicle.col2 + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>()).First();
                }
                catch (Exception)
                {
                    ddvehcol2.SelectedIndex = 0;
                }


                if (Array.IndexOf(GTA.Editor.window_value, new Global((GTA.Offsets.Editor.Vehicle.vehwtci + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>()) != -1)
                    ddvehwindowtint.SelectedIndex = Array.IndexOf(GTA.Editor.window_value, new Global((GTA.Offsets.Editor.Vehicle.vehwtci + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>());

                if (!tbvehiconsize.IsFocused || ignore_focus) tbvehiconsize.Text = new Global((GTA.Offsets.Editor.Vehicle.vehbs + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();

                if (new Global((GTA.Offsets.Editor.Vehicle.vbvrr + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>() != 0)
                    ddvehicon.SelectedIndex = 0;
                else
                    ddvehicon.SelectedIndex = (new Global((GTA.Offsets.Editor.Vehicle.vehbso + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>());

                Functions.Read.checkbinary(22, GTA.Offsets.Editor.Vehicle.vbs7 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_bptires);
                Functions.Read.checkbinary(6, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_godmode);
                if (cb_veh_godmode.IsChecked == false)
                {
                    tbvehenghp.IsEnabled = true;
                    tbvehptrhp.IsEnabled = true;
                    tbvehbdyhp.IsEnabled = true;
                    tbvehhlth.IsEnabled = true;
                }
                else
                {
                    tbvehenghp.IsEnabled = false;
                    tbvehptrhp.IsEnabled = false;
                    tbvehbdyhp.IsEnabled = false;
                    tbvehhlth.IsEnabled = false;
                }
                //Functions.Read.checkbinary(7, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cbvehdisableptdmg);
                Functions.Read.checkbinary(14, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_sirens);
                Functions.Read.checkbinary(18, GTA.Offsets.Editor.Vehicle.vbs6 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_mark);
                Functions.Read.checkbinary(10, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_box);
                Functions.Read.checkbinary(7, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_lights);
                Functions.Read.checkbinary(8, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_locveh);
                Functions.Read.checkbinary(14, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_locvehforp);
                Functions.Read.checkbinary(20, GTA.Offsets.Editor.Vehicle.vbs3 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_remove_windows);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Vehicle.vbs2 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_engine);
                Functions.Read.checkbinary(25, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_freeze);
                //Functions.Read.checkbinary(26, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cbveharrow);
                Functions.Read.checkbinary(32, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_nottargetable);
                Functions.Read.checkbinary(4, GTA.Offsets.Editor.Vehicle.vbs8 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_explodeinwater);
                Functions.Read.checkbinary(3, GTA.Offsets.Editor.Vehicle.vbs2 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_lockteam1);
                Functions.Read.checkbinary(4, GTA.Offsets.Editor.Vehicle.vbs2 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_lockteam2);
                Functions.Read.checkbinary(5, GTA.Offsets.Editor.Vehicle.vbs2 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_lockteam3);
                Functions.Read.checkbinary(6, GTA.Offsets.Editor.Vehicle.vbs2 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_lockteam4);


                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_fl);
                Functions.Read.checkbinary(2, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_fr);
                Functions.Read.checkbinary(3, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_rl);
                Functions.Read.checkbinary(4, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_rr);
                Functions.Read.checkbinary(5, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_hood);
                Functions.Read.checkbinary(6, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_trunk);
                Functions.Read.checkbinary(10, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_fl);
                Functions.Read.checkbinary(11, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_fr);
                Functions.Read.checkbinary(12, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_rl);
                Functions.Read.checkbinary(13, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_rr);
                Functions.Read.checkbinary(14, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_hood);
                Functions.Read.checkbinary(15, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_trunk);

                cb_veh_neon.IsChecked = new Global((GTA.Offsets.Editor.Vehicle.ncol + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>() > -1 ? true : false;

                if (!tbvehrule.IsFocused || ignore_focus) tbvehrule.Text = new Global((GTA.Offsets.Editor.Vehicle.rule + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();
                if (!tbvehpriority.IsFocused || ignore_focus) tbvehpriority.Text = new Global((GTA.Offsets.Editor.Vehicle.pri + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();
                if (!tbvehjtop.IsFocused || ignore_focus) tbvehjtop.Text = new Global((GTA.Offsets.Editor.Vehicle.jtop + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();
                if (!tbvehjtof.IsFocused || ignore_focus) tbvehjtof.Text = new Global((GTA.Offsets.Editor.Vehicle.jtof + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>().ToString();

                GetVehSpecialValues(ignore_focus);

                int clrteam = new Global((GTA.Offsets.Editor.Vehicle.vehct + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>();
                int clrrule = new Global((GTA.Offsets.Editor.Vehicle.vehcr + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>();

                if (!tbvehvehcr.IsFocused || ignore_focus) tbvehvehcr.Text = clrrule.ToString();
                if (!tbvehvehct.IsFocused || ignore_focus) tbvehvehct.Text = clrteam.ToString();

                if (!tbvehclearrule.IsFocused || ignore_focus) tbvehclearrule.Text = clrrule.ToString();
                ddvehteamclear.SelectedIndex = clrteam + 1;

                Functions.Read.checkbinary(20, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_clrlivc);
                Functions.Read.checkbinary(19, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_spwnrlivc);

                ddvehrsp.SelectedIndex = new Global((GTA.Offsets.Editor.Vehicle.rsp + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<int>();
                tbvehvrr.Text = new Global((GTA.Offsets.Editor.Vehicle.vrr + GTA.Offsets.Editor.Vehicle.NEXT * index)).Get<float>().ToString();


                GetVehicalASRLValues(ignore_focus);

            }
        }

        private void tbvehmodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbvehmodel.Text))
            {
                new Global((GTA.Offsets.Editor.Vehicle.model + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehmodel.Text);
            }
        }

        private void ddvehicon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddvehicon.SelectedIndex > 0)
            {
                new Global((GTA.Offsets.Editor.Vehicle.vbvrr + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(0);
                new Global((GTA.Offsets.Editor.Vehicle.vehbso + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(ddvehicon.SelectedIndex);
            }
            else if (ddvehicon.SelectedIndex == 0)
            {
                new Global((GTA.Offsets.Editor.Vehicle.vbvrr + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(-2);
            }
        }

        private void tbvehiconsize_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.vehbs + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehiconsize.Text);
        }

        private void tbvehenghp_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.enghp + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehenghp.Text);
        }

        private void tbvehptrhp_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.ptrhp + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehptrhp.Text);
        }

        private void tbvehbdyhp_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.bdyhp + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetFloat(tbvehbdyhp.Text);
        }

        private void tbvehhlth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbvehhlth.Text, false))
                new Global((GTA.Offsets.Editor.Vehicle.hlth + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehhlth.Text);
        }

        private void tbvehlivery_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbvehlivery.Text, false))
                new Global((GTA.Offsets.Editor.Vehicle.liv + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehlivery.Text);
        }

        private void cb_veh_mark_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(18, GTA.Offsets.Editor.Vehicle.vbs6 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_mark);
        }

        private void cb_veh_locveh_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(8, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_locveh);
        }

        private void cb_veh_locvehforp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(14, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_locvehforp);
        }

        private void cb_veh_lights_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_lights);
        }

        private void cb_veh_sirens_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(14, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_sirens);
        }

        private void cb_veh_sirens_audio_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_sirens_audio);
        }

        private void cb_veh_bptires_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(22, GTA.Offsets.Editor.Vehicle.vbs7 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_bptires);
        }


        private void cb_veh_godmode_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(6, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_godmode);
            tbvehenghp.IsEnabled = cb_veh_godmode.IsChecked == false;
            tbvehptrhp.IsEnabled = cb_veh_godmode.IsChecked == false;
            tbvehbdyhp.IsEnabled = cb_veh_godmode.IsChecked == false;
            tbvehhlth.IsEnabled = cb_veh_godmode.IsChecked == false;
        }

        private void cb_veh_engine_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Vehicle.vbs2 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_engine);
        }

        // vbs2 bits 3..6: the mission controller locks the vehicle's doors for
        // team 1..4.
        private void cb_veh_lockteam_Checked(object sender, RoutedEventArgs e)
        {
            if (ddvehno.SelectedIndex < 0)
                return;

            CheckBox[] boxes = { cb_veh_lockteam1, cb_veh_lockteam2, cb_veh_lockteam3, cb_veh_lockteam4 };
            int team = Array.IndexOf(boxes, sender);
            if (team > -1)
                Functions.Write.writebinary(3 + team, GTA.Offsets.Editor.Vehicle.vbs2 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, boxes[team]);
        }

        private void cb_veh_neon_Checked(object sender, RoutedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.ncol + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(0);
        }

        private void cb_veh_nottargetable_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(32, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_nottargetable);
        }

        private void cb_veh_box_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_box);
        }

        private void cb_veh_door_open_lf_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_fl);
        }

        private void cb_veh_door_open_lr_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_fr);
        }

        private void cb_veh_door_open_rf_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_rl);
        }

        private void cb_veh_door_open_rr_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_rr);
        }

        private void cb_veh_door_open_hood_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(5, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_hood);
        }

        private void cb_veh_door_open_trunk_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(6, GTA.Offsets.Editor.Vehicle.drbs + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_open_trunk);
        }

        private void cb_veh_door_close_lf_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_fl);
        }

        private void cb_veh_door_close_lr_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_fr);
        }

        private void cb_veh_door_close_rf_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_rl);
        }

        private void cb_veh_door_close_rr_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_rr);
        }

        private void cb_veh_door_close_hood_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(14, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_hood);
        }

        private void cb_veh_door_close_trunk_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.Vehicle.vbs4 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_door_close_trunk);
        }

        private void vehModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (vehModelList.SelectedIndex > -1 && m.IsProcOpen)
                new Global(GTA.Offsets.Editor.Vehicle.model + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).SetInt((vehModelList.SelectedItem as GTA.Vehicle).Int32);
        }

        private void BtnVehAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int vehnum = new Global(GTA.Offsets.Editor.Vehicle.number).Get<int>();
                if (vehnum < 200 && vehnum > -1)
                {
                    int new_index = vehnum + 1;

                    new Global(GTA.Offsets.Editor.Vehicle.number).SetInt(new_index);

                    new Global((GTA.Offsets.Editor.Vehicle.vbvrr + GTA.Offsets.Editor.Vehicle.NEXT * (ddvehno.SelectedIndex + 1))).SetInt(0);

                    ddvehno.SelectedIndex = new_index - 1;

                    if (new Global((GTA.Offsets.Editor.Vehicle.model + GTA.Offsets.Editor.Vehicle.NEXT * (ddvehno.SelectedIndex + 1))).Get<int>() != 0)
                    {
                        creatorRefresh();
                    }
                }
            }
        }

        private void ChangeVEHLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Vehicle.number), GTA.Offsets.Editor.Vehicle.loc + 0, GTA.Offsets.Editor.Vehicle.NEXT);
        }

        private void ChangeVEHLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Vehicle.number), GTA.Offsets.Editor.Vehicle.loc + 1, GTA.Offsets.Editor.Vehicle.NEXT);
        }

        private void ChangeVEHLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Vehicle.number), GTA.Offsets.Editor.Vehicle.loc + 2, GTA.Offsets.Editor.Vehicle.NEXT);
        }

        private void ddvehvbs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddvehvbs == null)
                return;
            if (m.IsProcOpen)
            {
                tbvehvbs.Text = new Global(GTA.Offsets.Editor.Vehicle.vbs2 + (1 * ddvehvbs.SelectedIndex) + (GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();

                SelectActiveTextBox();
            }
        }

        private void tbvehvbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehvbs.Text))
            {
                new Global(GTA.Offsets.Editor.Vehicle.vbs2 + (1 * ddvehvbs.SelectedIndex) + (GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehvbs.Text);
            }
        }

        private void ddvehteamrlprio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddvehno.SelectedIndex > -1 && ddvehteamrlprio.SelectedIndex > -1)
            {
                tbvehrule.Text = new Global((GTA.Offsets.Editor.Vehicle.rule + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                tbvehpriority.Text = new Global((GTA.Offsets.Editor.Vehicle.pri + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                tbvehjtop.Text = new Global((GTA.Offsets.Editor.Vehicle.jtop + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                tbvehjtof.Text = new Global((GTA.Offsets.Editor.Vehicle.jtof + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                GetVehSpecialValues(true);
                GetVehicalASRLValues(true);
                SelectActiveTextBox();
            }
        }

        private void tbvehrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.rule + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehrule.Text);
        }

        private void tbvehpriority_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.pri + ddvehteamrlprio.SelectedIndex + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehpriority.Text);
        }


        private void cb_veh_explodeinwater_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.Vehicle.vbs8 + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT, cb_veh_explodeinwater);
        }



        private void tbvehvebs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehvebs.Text))
                new Global((GTA.Offsets.Editor.Vehicle.vebs + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehvebs.Text);
        }

        private void tbkilljtof_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkilljtof.Text))
                kill.values[ddkillno.SelectedIndex].jtof[ddkillteamno.SelectedIndex] = Convert.ToInt32(tbkilljtof.Text);
        }

        private void tbkilljtop_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbkilljtop.Text))
                kill.values[ddkillno.SelectedIndex].jtop[ddkillteamno.SelectedIndex] = Convert.ToInt32(tbkilljtop.Text);
        }

        public void GetVehSpecialValues(bool ignore_focus = false)
        {
            if (ddvehteamrlprio != null)
            {
                if (m.IsProcOpen && ddvehteamrlprio.SelectedIndex > -1)
                {
                    if (ddvehteamrlprio.SelectedIndex == 1)
                    {
                        if (!tbvehteam.IsFocused || ignore_focus) tbvehteam.Text = new Global((GTA.Offsets.Editor.Vehicle.team2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspwn.IsFocused || ignore_focus) tbvehspwn.Text = new Global((GTA.Offsets.Editor.Vehicle.spwn2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!ddvehspawnon.IsFocused || ignore_focus) ddvehspawnon.SelectedIndex = new Global((GTA.Offsets.Editor.Vehicle.spwn2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>();
                        if (!tbvehobjt.IsFocused || ignore_focus) tbvehobjt.Text = new Global((GTA.Offsets.Editor.Vehicle.objt2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspasr.IsFocused || ignore_focus) tbvehspasr.Text = new Global((GTA.Offsets.Editor.Vehicle.spasr2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspsrc.IsFocused || ignore_focus) tbvehspsrc.Text = new Global((GTA.Offsets.Editor.Vehicle.spsrc2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                    }
                    else if (ddvehteamrlprio.SelectedIndex == 2)
                    {
                        if (!tbvehteam.IsFocused || ignore_focus) tbvehteam.Text = new Global((GTA.Offsets.Editor.Vehicle.team3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspwn.IsFocused || ignore_focus) tbvehspwn.Text = new Global((GTA.Offsets.Editor.Vehicle.spwn3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!ddvehspawnon.IsFocused || ignore_focus) ddvehspawnon.SelectedIndex = new Global((GTA.Offsets.Editor.Vehicle.spwn3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>();
                        if (!tbvehobjt.IsFocused || ignore_focus) tbvehobjt.Text = new Global((GTA.Offsets.Editor.Vehicle.objt3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspasr.IsFocused || ignore_focus) tbvehspasr.Text = new Global((GTA.Offsets.Editor.Vehicle.spasr3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspsrc.IsFocused || ignore_focus) tbvehspsrc.Text = new Global((GTA.Offsets.Editor.Vehicle.spsrc3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                    }
                    else if (ddvehteamrlprio.SelectedIndex == 3)
                    {
                        if (!tbvehteam.IsFocused || ignore_focus) tbvehteam.Text = new Global((GTA.Offsets.Editor.Vehicle.team4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspwn.IsFocused || ignore_focus) tbvehspwn.Text = new Global((GTA.Offsets.Editor.Vehicle.spwn4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!ddvehspawnon.IsFocused || ignore_focus) ddvehspawnon.SelectedIndex = new Global((GTA.Offsets.Editor.Vehicle.spwn4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>();
                        if (!tbvehobjt.IsFocused || ignore_focus) tbvehobjt.Text = new Global((GTA.Offsets.Editor.Vehicle.objt4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspasr.IsFocused || ignore_focus) tbvehspasr.Text = new Global((GTA.Offsets.Editor.Vehicle.spasr4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspsrc.IsFocused || ignore_focus) tbvehspsrc.Text = new Global((GTA.Offsets.Editor.Vehicle.spsrc4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                    }
                    else
                    {
                        if (!tbvehteam.IsFocused || ignore_focus) tbvehteam.Text = new Global((GTA.Offsets.Editor.Vehicle.team + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspwn.IsFocused || ignore_focus) tbvehspwn.Text = new Global((GTA.Offsets.Editor.Vehicle.spwn + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!ddvehspawnon.IsFocused || ignore_focus) ddvehspawnon.SelectedIndex = new Global((GTA.Offsets.Editor.Vehicle.spwn + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>();
                        if (!tbvehobjt.IsFocused || ignore_focus) tbvehobjt.Text = new Global((GTA.Offsets.Editor.Vehicle.objt + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspasr.IsFocused || ignore_focus) tbvehspasr.Text = new Global((GTA.Offsets.Editor.Vehicle.spasr + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                        if (!tbvehspsrc.IsFocused || ignore_focus) tbvehspsrc.Text = new Global((GTA.Offsets.Editor.Vehicle.spsrc + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).Get<int>().ToString();
                    }
                }
            }
        }

        private void tbvehobjt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehobjt.Text))
            {
                if (ddvehteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.objt + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehobjt.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.objt2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehobjt.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.objt3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehobjt.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.objt4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehobjt.Text);
                }
            }
        }

        private void tbvehspwn_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehspwn.Text))
            {
                if (ddvehteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spwn + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspwn.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spwn2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspwn.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spwn3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspwn.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spwn4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspwn.Text);
                }
            }
        }

        private void tbvehteam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehteam.Text))
            {
                if (ddvehteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.team + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehteam.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.team2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehteam.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.team3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehteam.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.team4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehteam.Text);
                }
            }
        }

        private void tbvehspsrc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehspsrc.Text))
            {
                if (ddvehteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spsrc + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspsrc.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spsrc2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspsrc.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spsrc3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspsrc.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spsrc4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspsrc.Text);
                }
            }
        }

        private void tbvehspasr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehspasr.Text))
            {
                if (ddvehteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spasr + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspasr.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spasr2 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspasr.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spasr3 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspasr.Text);
                }
                else if (ddvehteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Vehicle.spasr4 + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehspasr.Text);
                }
            }
        }

        private void tbvehdrbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbvehdrbs.Text))
                new Global((GTA.Offsets.Editor.Vehicle.drbs + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehdrbs.Text);
        }

        private void tbvehvehcr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbvehvehcr.Text))
            {
                new Global((GTA.Offsets.Editor.Vehicle.vehcr + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehvehcr.Text);
            }
        }

        private void tbvehvehct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbvehvehct.Text))
            {
                new Global((GTA.Offsets.Editor.Vehicle.vehct + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(tbvehvehct.Text);
            }
        }

        private void ddvehteamclear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Vehicle.vehct + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).SetInt(ddvehteamclear.SelectedIndex - 1);
        }

        private void tbvehclearrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbvehclearrule.Text))
                new Global(GTA.Offsets.Editor.Vehicle.vehcr + ddvehno.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).SetInt(tbvehclearrule.Text);
        }

        private void cb_veh_clrlivc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.Vehicle.drbs, cb_veh_clrlivc);
        }

        private void cb_veh_spwnrlivc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.Vehicle.drbs, cb_veh_spwnrlivc);
        }

        private void ddvehspawnon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long spwn;

            if (ddvehteamrlprio.SelectedIndex == 1)
            {
                spwn = GTA.Offsets.Editor.Vehicle.spwn2;
            }
            else if (ddvehteamrlprio.SelectedIndex == 2)
            {
                spwn = GTA.Offsets.Editor.Vehicle.spwn3;
            }
            else if (ddvehteamrlprio.SelectedIndex == 3)
            {
                spwn = GTA.Offsets.Editor.Vehicle.spwn4;
            }
            else
            {
                spwn = GTA.Offsets.Editor.Vehicle.spwn;
            }

            new Global(spwn + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetInt(ddvehspawnon.SelectedIndex);

            GetVehicalASRLValues(true);
        }

        public void GetVehicalASRLValues(bool ignore_focus = false)
        {
            long team, spwn, objt;

            if (ddvehteamrlprio.SelectedIndex == 1)
            {
                team = GTA.Offsets.Editor.Vehicle.team2;
                spwn = GTA.Offsets.Editor.Vehicle.spwn2;
                objt = GTA.Offsets.Editor.Vehicle.objt2;
            }
            else if (ddvehteamrlprio.SelectedIndex == 2)
            {
                team = GTA.Offsets.Editor.Vehicle.team3;
                spwn = GTA.Offsets.Editor.Vehicle.spwn3;
                objt = GTA.Offsets.Editor.Vehicle.objt3;
            }
            else if (ddvehteamrlprio.SelectedIndex == 3)
            {
                team = GTA.Offsets.Editor.Vehicle.team4;
                spwn = GTA.Offsets.Editor.Vehicle.spwn4;
                objt = GTA.Offsets.Editor.Vehicle.objt4;
            }
            else
            {
                team = GTA.Offsets.Editor.Vehicle.team;
                spwn = GTA.Offsets.Editor.Vehicle.spwn;
                objt = GTA.Offsets.Editor.Vehicle.objt;
            }

            if (ddvehspawnon.IsFocused || ignore_focus) ddvehspawnon.SelectedIndex = new Global(spwn + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).Get<int>();

            if (ddvehspawnon.SelectedIndex == 0)
            {
                ddvehspawnteam.IsEnabled = false;
                tbvehspawnrule.IsEnabled = false;
            }
            else
            {
                ddvehspawnteam.IsEnabled = true;
                tbvehspawnrule.IsEnabled = true;
                if (ddvehspawnteam.IsFocused || ignore_focus) ddvehspawnteam.SelectedIndex = new Global(team + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).Get<int>() + 1;
                if (tbvehspawnrule.IsFocused || ignore_focus) tbvehspawnrule.Text = new Global(objt + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).Get<int>().ToString();
            }
        }

        private void ddvehspawnteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long team;

            if (ddvehteamrlprio.SelectedIndex == 1)
            {
                team = GTA.Offsets.Editor.Vehicle.team2;
            }
            else if (ddvehteamrlprio.SelectedIndex == 2)
            {
                team = GTA.Offsets.Editor.Vehicle.team3;
            }
            else if (ddvehteamrlprio.SelectedIndex == 3)
            {
                team = GTA.Offsets.Editor.Vehicle.team4;
            }
            else
            {
                team = GTA.Offsets.Editor.Vehicle.team;
            }

            new Global(team + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetInt(ddvehspawnteam.SelectedIndex - 1);
        }

        private void tbvehspawnrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbvehspawnrule.Text))
            {
                long objt;

                if (ddvehteamrlprio.SelectedIndex == 1)
                {
                    objt = GTA.Offsets.Editor.Vehicle.objt2;
                }
                else if (ddvehteamrlprio.SelectedIndex == 2)
                {
                    objt = GTA.Offsets.Editor.Vehicle.objt3;
                }
                else if (ddvehteamrlprio.SelectedIndex == 3)
                {
                    objt = GTA.Offsets.Editor.Vehicle.objt4;
                }
                else
                {
                    objt = GTA.Offsets.Editor.Vehicle.objt;
                }

                new Global(objt + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetInt(tbvehspawnrule.Text);
            }
        }

        private void ddvehrsp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddvehrsp.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.Vehicle.rsp + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetInt(ddvehrsp.SelectedIndex);
        }

        private void tbvehvrr_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Vehicle.vrr + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex).SetFloat(tbvehvrr.Text);
        }
    }
}
