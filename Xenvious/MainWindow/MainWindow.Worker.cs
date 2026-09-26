using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;
using static mry.mem;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: The main polling loop that keeps the UI in sync with the game.
    public partial class MainWindow
    {
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Action<int> workMethod = async delegate
            {
                if (Process.GetProcessesByName(GameVariant.ProcessName).Length > 0)
                {
                    if (!m.IsProcOpen)
                    {
                        m.OpenProcess(GameVariant.ProcessName);
                    }

                    if (m.IsProcOpen)
                    {
                        BtnOnlineEnabler.Visibility = OnlineEnable.Available && !OnlineEnable.isDLLInjected ? Visibility.Visible : Visibility.Collapsed;

                        // Launching and leaving write globals, so a resolved GlobalPTR
                        // is what makes the button do anything at all. Inside a creator
                        // it leaves the creator (see MainWindow.Dashboard.cs).
                        BtnLaunchCreator.Visibility =
                            globalPtrSanityCheck(GTA.Offsets.Editor.GlobalPTRversion)
                                ? Visibility.Visible
                                : Visibility.Collapsed;

                        // custom_check is reset whenever the game rebuilds its globals
                        // (a new creator session, a restart), so the ticked settings are
                        // put back here -- the same bits their checkboxes write: script
                        // features = dispatch bits 1..5 plus bit 30 (gates the dev
                        // patches), camera key = bit 31. writebinary only writes when a
                        // bit is not already as wanted.
                        if (globalPtrSanityCheck(GTA.Offsets.Editor.GlobalPTRversion))
                        {
                            if (cbsettingsexpscrfeat.IsChecked == true)
                            {
                                WriteScriptFeatureBits(true);
                            }
                            if (cbsettingsswitchcamkey.IsChecked == true)
                            {
                                Functions.Write.writebinary(31, GTA.Offsets.Editor.custom_check);
                            }
                        }
                        
                        if (IsInCreator())
                        {

                            // Check if Dashboard is currently the main Page
                            if (MainPages.SelectedItem == PageDashboard)
                            {
                                // get values for Main page
                                if (!tb_JobTitle.IsFocused) tb_JobTitle.Text = new Global(GTA.Offsets.Editor.nm).GetString();
                                if (!tb_JobDec.IsFocused) tb_JobDec.Text = getDescribtion();

                                if (!ddweather.IsFocused) ddweather.SelectedIndex = (new Global(GTA.Offsets.Editor.weth).Get<int>() > -1 && new Global(GTA.Offsets.Editor.weth).Get<int>() <= ddweather.Items.Count) ? new Global(GTA.Offsets.Editor.weth).Get<int>() : -1;
                                if (!ddtimeofday.IsFocused)
                                {
                                    int tod = new Global(GTA.Offsets.Editor.tod).Get<int>();
                                    ddtimeofday.SelectedIndex = (tod >= 0 && tod < ddtimeofday.Items.Count) ? tod : -1;
                                }
                                if (!ddmusic.IsFocused) ddmusic.SelectedIndex = (new Global(GTA.Offsets.Editor.ausc).Get<int>() > -1 && new Global(GTA.Offsets.Editor.ausc).Get<int>() <= ddmusic.Items.Count) ? new Global(GTA.Offsets.Editor.ausc).Get<int>() : -1;
                                if (!ddradio.IsFocused) ddradio.SelectedIndex = Array.IndexOf(GTA.Editor.radioArray, new Global(GTA.Offsets.Editor.musx).Get<int>()) != -1 ? Array.IndexOf(GTA.Editor.radioArray, new Global(GTA.Offsets.Editor.musx).Get<int>()) : 0;

                                if (!tb_Teams.IsFocused) tb_Teams.Text = new Global(GTA.Offsets.Editor.tnum).Get<int>().ToString();
                                if (!tb_minp.IsFocused) tb_minp.Text = new Global(GTA.Offsets.Editor.min).Get<int>().ToString();
                                if (!tb_maxp.IsFocused)
                                {
                                    int maxpadjustvalue = 0;
                                    if (Functions.Read.isRace() || Functions.Read.isDeathmatch())
                                    {
                                        maxpadjustvalue = 2;
                                    }
                                    else if (Functions.Read.isMission())
                                    {
                                        maxpadjustvalue = 1;
                                    }
                                    tb_maxp.Text = (new Global(GTA.Offsets.Editor.num).Get<int>() + maxpadjustvalue).ToString();
                                }
                                if (!tb_Rounds.IsFocused) tb_Rounds.Text = (Functions.Read.isMission()) ? (new Global(GTA.Offsets.Editor.numRounds).Get<int>()).ToString() : (new Global(GTA.Offsets.Editor.Race.Checkpoints.lap).Get<int>() + 1).ToString();

                                if (Functions.Read.checkbinary(16, GTA.Offsets.Editor.menubs2, cb_customdaytime))
                                {
                                    if (!tb_cdth.IsFocused) tb_cdth.Text = new Global(GTA.Offsets.Editor.todhr).Get<int>().ToString();
                                    if (!tb_cdtm.IsFocused) tb_cdtm.Text = new Global(GTA.Offsets.Editor.todmn).Get<int>().ToString();
                                }

                                Functions.Read.checkbinary(7, GTA.Offsets.Editor.menubs10, cb_hdvision);
                                Functions.Read.checkbinary(28, GTA.Offsets.Editor.menubs14, cb_blackout);
                                //Functions.Read.checkbinary(3, GTA.Offsets.Editor.menubs2, cbpcdmusic);
                                //Functions.Read.checkbinary(21, GTA.Offsets.Editor.menubs2, cbprogtod);

                                if (!cb_dev.IsFocused) cb_dev.IsChecked = m.memory(GTA.Offsets.Editor.dev).Get<int>() == GTA.DevPatched;


                                if (!tbjobtype.IsFocused) tbjobtype.Text = new Global(GTA.Offsets.Editor.type).Get<int>().ToString();
                                if (!tbjobsubtype.IsFocused) tbjobsubtype.Text = new Global(GTA.Offsets.Editor.subtype).Get<int>().ToString();
                                if (!tbjobracetype.IsFocused) tbjobracetype.Text = new Global(GTA.Offsets.Editor.racetype).Get<int>().ToString();

                                if (!ddjobtype.IsFocused)
                                {
                                    switch (new Global(GTA.Offsets.Editor.type).Get<int>())
                                    {
                                        case 0:
                                            ddjobtype.SelectedIndex = 0;
                                            break;
                                        case 1:
                                            ddjobtype.SelectedIndex = 1;
                                            break;
                                        case 2:
                                            ddjobtype.SelectedIndex = 2;
                                            break;
                                        case 3:
                                            ddjobtype.SelectedIndex = 3;
                                            break;
                                        default:
                                            ddjobtype.SelectedIndex = -1;
                                            break;
                                    }
                                }

                                if (!ddjobsubtype.IsFocused)
                                {
                                    switch (ddjobtype.SelectedIndex)
                                    {
                                        case (int)GTA.Editor.JobTypes.Mission:
                                            if (new Global(GTA.Offsets.Editor.subtype).Get<int>() == 6)
                                            {
                                                ddjobsubtype.SelectedIndex = 0;
                                            }
                                            else if (new Global(GTA.Offsets.Editor.subtype).Get<int>() == 5)
                                            {
                                                ddjobsubtype.SelectedIndex = 1;
                                            }
                                            else if (new Global(GTA.Offsets.Editor.subtype).Get<int>() == 4)
                                            {
                                                ddjobsubtype.SelectedIndex = 2;
                                            }
                                            else
                                            {
                                                ddjobsubtype.SelectedIndex = -1;
                                            }
                                            break;
                                        case (int)GTA.Editor.JobTypes.Deathmatch:
                                            if (Functions.Read.checkbinary(13, GTA.Offsets.Editor.intop2))
                                            {
                                                ddjobsubtype.SelectedIndex = 2;
                                            }
                                            else if (new Global(GTA.Offsets.Editor.vdm).Get<int>() == 1)
                                            {
                                                ddjobsubtype.SelectedIndex = 1;
                                            }
                                            else if (new Global(GTA.Offsets.Editor.vdm).Get<int>() == 0 && new Global(GTA.Offsets.Editor.subtype).Get<int>() == 0)
                                            {
                                                ddjobsubtype.SelectedIndex = 0;
                                            }
                                            else if (new Global(GTA.Offsets.Editor.vdm).Get<int>() == 0 && new Global(GTA.Offsets.Editor.subtype).Get<int>() == 3)
                                            {
                                                ddjobsubtype.SelectedIndex = 3;
                                            }
                                            else
                                            {
                                                ddjobsubtype.SelectedIndex = -1;
                                            }
                                            break;
                                        case (int)GTA.Editor.JobTypes.Race:
                                            if (IsArenaRace())
                                            {
                                                ddjobsubtype.SelectedIndex = 8;
                                            }
                                            else
                                            {
                                                if (IsLandRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 0;
                                                }
                                                else if (IsWaterRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 1;
                                                }
                                                else if (IsAirRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 2;
                                                }
                                                else if (IsStuntRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 3;
                                                }
                                                else if (IsParachuteRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 4;
                                                }
                                                else if (IsSpecialVehRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 5;
                                                }
                                                else if (IsTargetRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 6;
                                                }
                                                else if (IsTransformRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 7;
                                                    //cbenable30transform.Visibility = Visibility.Visible;
                                                }
                                                else if (IsOpenWheelRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 9;
                                                }
                                                else if (IsPursuitRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 10;
                                                }
                                                else if (IsStreetRace())
                                                {
                                                    ddjobsubtype.SelectedIndex = 11;
                                                }
                                                else
                                                {
                                                    ddjobsubtype.SelectedIndex = -1;
                                                }
                                            }
                                            break;
                                        case (int)GTA.Editor.JobTypes.Survival:
                                            ddjobsubtype.SelectedIndex = 0;
                                            break;
                                        default:
                                            ddjobsubtype.SelectedIndex = -1;
                                            break;
                                    }
                                }
                            }
                            else if (MainPages.SelectedItem == PageEdit)
                            {
                                checkSection();

                                if (EditPages.SelectedItem == PageProps)
                                {
                                    if (PageInnerProps.SelectedItem == PageInnerNormalProps)
                                    {
                                        int propnum = new Global(GTA.Offsets.Editor.Props.number).Get<int>();

                                        if (ddpropno.Items.Count != propnum)
                                        {
                                            if (propnum <= 0)
                                            {
                                                ddpropno.ItemsSource = null;
                                            }
                                            else
                                            {
                                                if (propnum < 201)
                                                {
                                                    ddpropno.ItemsSource = Enumerable.Range(1, propnum);
                                                }
                                            }
                                        }

                                        if (Functions.Read.isMission())
                                        {
                                            if (ddpropno.SelectedIndex > -1)
                                            {
                                                cb_props_nocollision.IsEnabled = true;
                                            }
                                        }
                                        else
                                        {
                                            cb_props_nocollision.IsEnabled = false;
                                        }

                                        GetProps(true);

                                        if (haslistcontainer.Visibility == Visibility.Visible)
                                        {
                                            initializePropHasList();
                                        }
                                    }
                                    else if (PageInnerProps.SelectedItem == PageInnerDynamicProps)
                                    {
                                        int dpropnum = new Global(GTA.Offsets.Editor.DProps.number).Get<int>();

                                        if (dddpropno.Items.Count != dpropnum)
                                        {
                                            if (dpropnum <= 0)
                                            {
                                                dddpropno.ItemsSource = null;
                                            }
                                            else
                                            {
                                                if (dpropnum < 33)
                                                {
                                                    dddpropno.ItemsSource = Enumerable.Range(1, dpropnum);
                                                }
                                            }
                                        }

                                        GetDProps(true);
                                    }
                                    //{
                                    //}
                                }
                                else if (EditPages.SelectedItem == PageRace)
                                {
                                    if (PageInnerRace.SelectedItem == PageInnerRaceCP)
                                    {
                                        int cpnum = new Global(GTA.Offsets.Editor.Race.Checkpoints.number).Get<int>();

                                        if (ddcpno.Items.Count != cpnum)
                                        {
                                            if (cpnum <= 0)
                                            {
                                                ddcpno.ItemsSource = null;
                                            }
                                            else
                                            {
                                                if (cpnum < 101)
                                                {
                                                    ddcpno.ItemsSource = Enumerable.Range(1, cpnum);
                                                }
                                            }
                                        }

                                        if (!ddRaceCPTransform.IsDropDownOpen)
                                        {
                                            ObservableCollection<string> temp = new ObservableCollection<string>();

                                            temp.Add("Lobby Vehice");

                                            for (int i = 0; i < GTA.Editor.TransformVehiclesCount; i++)
                                            {
                                                Global transformVeh = new Global(GTA.Offsets.Editor.Race.Checkpoints.trfmvm + i);
                                                if (transformVeh.Get<int>() != 0)
                                                {
                                                    temp.Add(GTA.Editor.Vehiclenames.Where(x => (Functions.int_parse(Functions.joaat(x).ToString()) == transformVeh.Get<int>())).FirstOrDefault());
                                                }
                                            }

                                            ddRaceCPTransform.ItemsSource = temp;
                                        }
                                    }
                                    else if (PageInnerRace.SelectedItem == PageInnerRaceGeneral)
                                    {
                                        int carnum = (Functions.Read.isMission()) ? (new Global(GTA.Offsets.Editor.num).Get<int>() + 1) : (new Global(GTA.Offsets.Editor.num).Get<int>() + 2);

                                        if (ddcpssg.Items.Count != carnum)
                                        {
                                            if (carnum <= 0)
                                            {
                                                ddcpssg.ItemsSource = null;
                                            }
                                            else
                                            {
                                                ddcpssg.ItemsSource = Enumerable.Range(1, carnum);
                                            }
                                        }

                                        if (!ddracetype.IsFocused)
                                            ddracetype.SelectedIndex = new Global(GTA.Offsets.Editor.Race.Checkpoints.gtar).Get<int>();

                                        if (!ddroutetype.IsFocused)
                                        {
                                            if (new Global(GTA.Offsets.Editor.Race.Checkpoints.ptp).Get<int>() == 0)
                                                ddroutetype.SelectedIndex = 0;
                                            else if (new Global(GTA.Offsets.Editor.Race.Checkpoints.ptp).Get<int>() == 1)
                                                ddroutetype.SelectedIndex = 1;
                                        }

                                        if (!tbraceblimptext.IsFocused)
                                            tbraceblimptext.Text = new Global(GTA.Offsets.Editor.blmpmsg).GetString(60);

                                        Functions.Read.checkbinary(31, GTA.Offsets.Editor.menubs8, cbraceololockcatchup);
                                        Functions.Read.checkbinary(16, GTA.Offsets.Editor.menubs2, cbraceolohidetod);
                                        Functions.Read.checkbinary(3, GTA.Offsets.Editor.menubs9, cbraceolohideag);
                                        Functions.Read.checkbinary(12, GTA.Offsets.Editor.menubs17, cbraceololocktod);
                                        Functions.Read.checkbinary(19, GTA.Offsets.Editor.menubs18, cbraceoloosnc);
                                        Functions.Read.checkbinary(10, GTA.Offsets.Editor.menubs20, cbraceolohidetraffic);
                                        Functions.Read.checkbinary(20, GTA.Offsets.Editor.menubs20, cbraceolohidedlp);
                                        Functions.Read.checkbinary(18, GTA.Offsets.Editor.menubs20, cbraceolohidenol);
                                        Functions.Read.checkbinary(19, GTA.Offsets.Editor.menubs20, cbraceolohidewl);
                                        Functions.Read.checkbinary(31, GTA.Offsets.Editor.menubs21, cbraceolohiderally);
                                        Functions.Read.checkbinary(31, GTA.Offsets.Editor.menubs28, cbraceolomaxwl);
                                        Functions.Read.checkbinary(31, GTA.Offsets.Editor.menubs22, cbraceolohideweth);
                                        cbraceolohidecstveh.IsChecked = new Global(GTA.Offsets.Editor.rcvs).Get<int>() == 2 ? true : false;
                                    }
                                    else if (PageInnerRace.SelectedItem == PageInnerRaceAVEH)
                                    {
                                        bool class1 = Functions.Read.checkbinary(1, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehCompact);
                                        bool class2 = Functions.Read.checkbinary(2, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSedan);
                                        bool class3 = Functions.Read.checkbinary(3, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSUV);
                                        bool class4 = Functions.Read.checkbinary(4, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehCOUPE);
                                        bool class5 = Functions.Read.checkbinary(5, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehMuscle);
                                        bool class6 = Functions.Read.checkbinary(6, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSports_Classics);
                                        bool class7 = Functions.Read.checkbinary(7, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSports);
                                        bool class8 = Functions.Read.checkbinary(8, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSuper);
                                        bool class9 = Functions.Read.checkbinary(9, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehMotorcycles);
                                        bool class10 = Functions.Read.checkbinary(10, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehOff_Road);
                                        bool class11 = Functions.Read.checkbinary(11, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehIndustrial);
                                        bool class12 = Functions.Read.checkbinary(12, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehUtility);
                                        bool class13 = Functions.Read.checkbinary(13, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehVans);
                                        bool class14 = Functions.Read.checkbinary(14, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehCycles);
                                        bool class15 = Functions.Read.checkbinary(16, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSpecial);
                                        bool class16 = Functions.Read.checkbinary(17, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehWeaponized);
                                        bool class17 = Functions.Read.checkbinary(18, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehArena_Contender);
                                        bool class18 = Functions.Read.checkbinary(19, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehOpenWheel);
                                        bool class19 = Functions.Read.checkbinary(20, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehGoKart);
                                        bool class20 = Functions.Read.checkbinary(21, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehTuner);

                                        int index = new Global(GTA.Offsets.Editor.Race.Checkpoints.icv).Get<int>();

                                        if (ddRaceVehClass.Items.Cast<ComboBoxItem>().Where(d => (int)(d.Tag) == index).Any())
                                        {
                                            ddRaceVehClass.SelectedItem = ddRaceVehClass.Items.Cast<ComboBoxItem>().Where(d => (int)(d.Tag) == index).First();
                                        }
                                        else
                                        {
                                            ddRaceVehClass.SelectedItem = null;
                                        }
                                    }
                                }
                                else if (EditPages.SelectedItem == PageVehicle)
                                {
                                    int carnum = new Global(GTA.Offsets.Editor.Vehicle.number).Get<int>();

                                    if (ddvehno.Items.Count != carnum)
                                    {
                                        if (carnum <= 0)
                                        {
                                            ddvehno.ItemsSource = null;
                                        }
                                        else
                                        {
                                            if (carnum < 33)
                                            {
                                                ddvehno.ItemsSource = Enumerable.Range(1, carnum);
                                            }
                                        }
                                    }

                                    GetVehicle(true);
                                }
                                else if (EditPages.SelectedItem == PageWeapon)
                                {
                                    int weapnum = new Global(GTA.Offsets.Editor.Weapon.number).Get<int>();

                                    if (ddweapno.Items.Count != weapnum)
                                    {
                                        if (weapnum <= 0)
                                        {
                                            ddweapno.ItemsSource = null;
                                        }
                                        else
                                        {
                                            if (weapnum < 61)
                                            {
                                                ddweapno.ItemsSource = Enumerable.Range(1, weapnum);
                                            }
                                        }
                                    }

                                    GetWeapons(true);
                                }
                                else if (EditPages.SelectedItem == PageActor)
                                {
                                    int actornum = new Global(GTA.Offsets.Editor.Actor.number).Get<int>();

                                    if (ddactorno.Items.Count != actornum)
                                    {
                                        if (actornum <= 0)
                                        {
                                            ddactorno.ItemsSource = null;
                                        }
                                        else
                                        {
                                            if (actornum < 81)
                                            {
                                                ddactorno.ItemsSource = Enumerable.Range(1, actornum);
                                            }
                                        }
                                    }

                                    if (!ddActorcar.IsDropDownOpen)
                                    {
                                        int counter = 0;
                                        ObservableCollection<string> temp = new ObservableCollection<string>();
                                        temp.Add(_Translation.Where(x => x.Key == "none").First().Value);
                                        for (int i = 0; i < new Global(GTA.Offsets.Editor.Vehicle.number).Get<int>(); i++)
                                        {
                                            //get the memory hadnle for the right addy
                                            Global vehicle = new Global(GTA.Offsets.Editor.Vehicle.model + i * GTA.Offsets.Editor.Vehicle.NEXT);
                                            if (vehicle.Get<int>() != 0)
                                            {
                                                //calculate the number
                                                counter++;
                                                try
                                                {
                                                    //try to get vehicle name
                                                    temp.Add(counter.ToString() + ". " + GTA.Editor.Vehiclenames.Where(x => (Functions.int_parse(Functions.joaat(x).ToString()) == vehicle.Get<int>())).FirstOrDefault());
                                                }
                                                catch (Exception)
                                                {
                                                    //if vehicle name could not be found, use integer
                                                    temp.Add(counter.ToString() + ". " + vehicle.Get<int>().ToString());
                                                }
                                            }
                                        }

                                        ddActorcar.IsEnabled = counter > 0 ? true : false;

                                        ddActorcar.ItemsSource = temp;
                                    }
                                    GetActorValues(true);
                                    GetActorACTVValues();
                                }
                                else if (EditPages.SelectedItem == Pagecentity)
                                {
                                    int cdefnum = new Global(GTA.Offsets.Editor.DHProp.number).Get<int>();

                                    if (ddcentityno.Items.Count != cdefnum)
                                    {
                                        if (cdefnum <= 0)
                                        {
                                            ddcentityno.ItemsSource = null;
                                        }
                                        else
                                        {
                                            if (cdefnum < 41)
                                            {
                                                ddcentityno.ItemsSource = Enumerable.Range(1, cdefnum);
                                            }
                                        }
                                    }
                                    GetFixtureValues();
                                }
                                else if (EditPages.SelectedItem == PageDoors)
                                {
                                    int doorsnum = new Global(GTA.Offsets.Editor.Doors.number).Get<int>();

                                    if (dddoorsno.Items.Count != doorsnum)
                                    {
                                        if (doorsnum <= 0)
                                        {
                                            dddoorsno.ItemsSource = null;
                                        }
                                        else
                                        {
                                            if (doorsnum < 33)
                                            {
                                                dddoorsno.ItemsSource = Enumerable.Range(1, doorsnum);
                                            }
                                        }
                                    }
                                    GetDoorsValues();
                                }
                                else if (EditPages.SelectedItem == PageMission)
                                {
                                    if (PageInnerMission.SelectedItem == PageInnerMissionMenubs)
                                    {
                                        if (!tbmbs1.IsFocused) tbmbs1.Text = new Global(GTA.Offsets.Editor.menubs).Get<int>().ToString();
                                        if (!tbmbs2.IsFocused) tbmbs2.Text = new Global(GTA.Offsets.Editor.menubs2).Get<int>().ToString();
                                        if (!tbmbs3.IsFocused) tbmbs3.Text = new Global(GTA.Offsets.Editor.menubs3).Get<int>().ToString();
                                        if (!tbmbs4.IsFocused) tbmbs4.Text = new Global(GTA.Offsets.Editor.menubs4).Get<int>().ToString();
                                        if (!tbmbs5.IsFocused) tbmbs5.Text = new Global(GTA.Offsets.Editor.menubs5).Get<int>().ToString();
                                        if (!tbmbs6.IsFocused) tbmbs6.Text = new Global(GTA.Offsets.Editor.menubs6).Get<int>().ToString();
                                        if (!tbmbs7.IsFocused) tbmbs7.Text = new Global(GTA.Offsets.Editor.menubs7).Get<int>().ToString();
                                        if (!tbmbs8.IsFocused) tbmbs8.Text = new Global(GTA.Offsets.Editor.menubs8).Get<int>().ToString();
                                        if (!tbmbs9.IsFocused) tbmbs9.Text = new Global(GTA.Offsets.Editor.menubs9).Get<int>().ToString();
                                        if (!tbmbs10.IsFocused) tbmbs10.Text = new Global(GTA.Offsets.Editor.menubs10).Get<int>().ToString();
                                        if (!tbmbs11.IsFocused) tbmbs11.Text = new Global(GTA.Offsets.Editor.menubs11).Get<int>().ToString();
                                        if (!tbmbs12.IsFocused) tbmbs12.Text = new Global(GTA.Offsets.Editor.menubs12).Get<int>().ToString();
                                        if (!tbmbs13.IsFocused) tbmbs13.Text = new Global(GTA.Offsets.Editor.menubs13).Get<int>().ToString();
                                        if (!tbmbs14.IsFocused) tbmbs14.Text = new Global(GTA.Offsets.Editor.menubs14).Get<int>().ToString();
                                        if (!tbmbs15.IsFocused) tbmbs15.Text = new Global(GTA.Offsets.Editor.menubs15).Get<int>().ToString();
                                        if (!tbmbs16.IsFocused) tbmbs16.Text = new Global(GTA.Offsets.Editor.menubs16).Get<int>().ToString();
                                        if (!tbmbs17.IsFocused) tbmbs17.Text = new Global(GTA.Offsets.Editor.menubs17).Get<int>().ToString();
                                        if (!tbmbs18.IsFocused) tbmbs18.Text = new Global(GTA.Offsets.Editor.menubs18).Get<int>().ToString();
                                        if (!tbmbs19.IsFocused) tbmbs19.Text = new Global(GTA.Offsets.Editor.menubs19).Get<int>().ToString();
                                        if (!tbmbs20.IsFocused) tbmbs20.Text = new Global(GTA.Offsets.Editor.menubs20).Get<int>().ToString();
                                        if (!tbmbs21.IsFocused) tbmbs21.Text = new Global(GTA.Offsets.Editor.menubs21).Get<int>().ToString();
                                        if (!tbmbs22.IsFocused) tbmbs22.Text = new Global(GTA.Offsets.Editor.menubs22).Get<int>().ToString();
                                        if (!tbmbs23.IsFocused) tbmbs23.Text = new Global(GTA.Offsets.Editor.menubs23).Get<int>().ToString();
                                        if (!tbmbs24.IsFocused) tbmbs24.Text = new Global(GTA.Offsets.Editor.menubs24).Get<int>().ToString();
                                        if (!tbmbs25.IsFocused) tbmbs25.Text = new Global(GTA.Offsets.Editor.menubs25).Get<int>().ToString();
                                        if (!tbmbs26.IsFocused) tbmbs26.Text = new Global(GTA.Offsets.Editor.menubs26).Get<int>().ToString();
                                        if (!tbmbs27.IsFocused) tbmbs27.Text = new Global(GTA.Offsets.Editor.menubs27).Get<int>().ToString();
                                        if (!tbmbs28.IsFocused) tbmbs28.Text = new Global(GTA.Offsets.Editor.menubs28).Get<int>().ToString();
                                        if (!tbmbs29.IsFocused) tbmbs29.Text = new Global(GTA.Offsets.Editor.menubs29).Get<int>().ToString();
                                        if (!tbmbs30.IsFocused) tbmbs30.Text = new Global(GTA.Offsets.Editor.menubs30).Get<int>().ToString();
                                        if (!tbmbs31.IsFocused) tbmbs31.Text = new Global(GTA.Offsets.Editor.menubs31).Get<int>().ToString();
                                        if (!tbmbs32.IsFocused) tbmbs32.Text = new Global(GTA.Offsets.Editor.menubs32).Get<int>().ToString();

                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionExtras)
                                    {
                                        GetMissionExtraValues();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionPA)
                                    {
                                        GetOUTBValues();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionRA)
                                    {
                                        GetBD2Values();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionInventory)
                                    {
                                        CheckMissionInventory();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionSMS)
                                    {
                                        GetSMSValues();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionGeneral)
                                    {
                                        if (!tbstartlocx.IsFocused) tbstartlocx.Text = new Global(GTA.Offsets.Editor.start).Get<float>().ToString();
                                        if (!tbstartlocy.IsFocused) tbstartlocy.Text = new Global(GTA.Offsets.Editor.start + 1).Get<float>().ToString();
                                        if (!tbstartlocz.IsFocused) tbstartlocz.Text = new Global(GTA.Offsets.Editor.start + 2).Get<float>().ToString();

                                        if (!tblobbycamlocx.IsFocused) tblobbycamlocx.Text = new Global(GTA.Offsets.Editor.cam + 0 * 1).Get<float>().ToString();
                                        if (!tblobbycamlocy.IsFocused) tblobbycamlocy.Text = new Global(GTA.Offsets.Editor.cam + 1 * 1).Get<float>().ToString();
                                        if (!tblobbycamlocz.IsFocused) tblobbycamlocz.Text = new Global(GTA.Offsets.Editor.cam + 2 * 1).Get<float>().ToString();

                                        if (!tbcamflocx.IsFocused) tbcamflocx.Text = new Global(GTA.Offsets.Editor.camf + 0 * 1).Get<float>().ToString();
                                        if (!tbcamflocy.IsFocused) tbcamflocy.Text = new Global(GTA.Offsets.Editor.camf + 1 * 1).Get<float>().ToString();
                                        if (!tbcamflocz.IsFocused) tbcamflocz.Text = new Global(GTA.Offsets.Editor.camf + 2 * 1).Get<float>().ToString();

                                        Functions.Read.checkbinary(11, GTA.Offsets.Editor.menubs2, cbmissionteambalancing);
                                        Functions.Read.checkbinary(28, GTA.Offsets.Editor.menubs2, cbmissionspawnunarmed);
                                        Functions.Read.checkbinary(21, GTA.Offsets.Editor.menubs25, cbmissionspawnunarmed);
                                        Functions.Read.checkbinary(10, GTA.Offsets.Editor.menubs27, cbmissionaddflash);
                                        Functions.Read.checkbinary(20, GTA.Offsets.Editor.menubs, cbmissionremweap);
                                        Functions.Read.checkbinary(19, GTA.Offsets.Editor.menubs11, cbmissionbulletproofplayers);
                                        Functions.Read.checkbinary(10, GTA.Offsets.Editor.menubs, cbmissionwlad);
                                        if (!tbMissionfsr.IsFocused) tbMissionfsr.Text = new Global(GTA.Offsets.Editor.fiispr).Get<float>().ToString();
                                        int weaponrsptime = new Global(GTA.Offsets.Editor.Weapon.time).Get<int>();
                                        if (!ddweaponrsptime.IsFocused) ddweaponrsptime.SelectedIndex = weaponrsptime == -1 ? 2 : weaponrsptime;
                                        // enable prison alarms
                                        // is true if menubs7 bit 32 is not set and menubs1 bit 28 is set 
                                        if (!cbmissionenableprisonalarms.IsFocused) cbmissionenableprisonalarms.IsChecked = (!Functions.Read.checkbinary(32, GTA.Offsets.Editor.menubs7) && Functions.Read.checkbinary(28, GTA.Offsets.Editor.menubs));
                                        Functions.Read.checkbinary(30, GTA.Offsets.Editor.menubs3, cbmissiondisspeccam);
                                        Functions.Read.checkbinary(5, GTA.Offsets.Editor.menubs26, cbmissiondisjipspec);
                                        Functions.Read.checkbinary(27, GTA.Offsets.Editor.menubs3, cbmissiondislsc);
                                        Functions.Read.checkbinary(21, GTA.Offsets.Editor.menubs2, cbmissionptod);
                                        Functions.Read.checkbinary(3, GTA.Offsets.Editor.menubs2, cbmissionplaycm);
                                        int polValue = new Global(GTA.Offsets.Editor.pol).Get<int>();
                                        ddmissionmaxwl.SelectedIndex = polValue >= 0 && polValue < ddmissionmaxwl.Items.Count ? polValue : -1;
                                        GetMissionDensity(ddmissiontraffic, GTA.Offsets.Editor.traf);
                                        GetMissionDensity(ddmissionpeds, GTA.Offsets.Editor.apeds);
                                        Functions.Read.checkbinary(6, GTA.Offsets.Editor.twrst, cbmissiondispwd);
                                        Functions.Read.checkbinary(19, GTA.Offsets.Editor.menubs12, cbmissionactorremarmor);
                                        Functions.Read.checkbinary(17, GTA.Offsets.Editor.trel, cbmissionjlwnp);
                                        Functions.Read.checkbinary(1, GTA.Offsets.Editor.twrst, cbmissiondwd);
                                        Functions.Read.checkbinary(2, GTA.Offsets.Editor.twrst, cbmissionrwbs1);
                                        Functions.Read.checkbinary(3, GTA.Offsets.Editor.twrst, cbmissionrwbs2);
                                        Functions.Read.checkbinary(4, GTA.Offsets.Editor.twrst, cbmissionrwbs3);
                                        Functions.Read.checkbinary(5, GTA.Offsets.Editor.twrst, cbmissionrwbs4);
                                        Functions.Read.checkbinary(21, GTA.Offsets.Editor.trel, cbmissiontrel12);
                                        Functions.Read.checkbinary(22, GTA.Offsets.Editor.trel, cbmissiontrel13);
                                        Functions.Read.checkbinary(23, GTA.Offsets.Editor.trel, cbmissiontrel14);
                                        Functions.Read.checkbinary(24, GTA.Offsets.Editor.trel, cbmissiontrel23);
                                        Functions.Read.checkbinary(25, GTA.Offsets.Editor.trel, cbmissiontrel24);
                                        Functions.Read.checkbinary(26, GTA.Offsets.Editor.trel, cbmissiontrel34);
                                        Functions.Read.checkbinary(6, GTA.Offsets.Editor.menubs9, cbmissionedw);
                                        Functions.Read.checkbinary(25, GTA.Offsets.Editor.menubs24, cbmissionemp);
                                        Functions.Read.checkbinary(9, GTA.Offsets.Editor.menubs25, cbmissionempnm);
                                        if (!tbmissionempdura.IsFocused) tbmissionempdura.Text = new Global(GTA.Offsets.Editor.pnEMPd).Get<int>().ToString();
                                        if (!tbmissionemppsr.IsFocused) tbmissionemppsr.Text = new Global(GTA.Offsets.Editor.pnEMPp).Get<int>().ToString();


                                        if (Functions.Read.checkbinary(1, GTA.Offsets.Editor.menubs))
                                        {
                                            ddmissionplyrlm.SelectedIndex = 1;
                                        }
                                        else if (Functions.Read.checkbinary(6, GTA.Offsets.Editor.menubs))
                                        {
                                            ddmissionplyrlm.SelectedIndex = 2;
                                        }
                                        else
                                        {
                                            ddmissionplyrlm.SelectedIndex = 0;
                                        }


                                        if (!tbMissionltm.IsFocused) tbMissionltm.Text = new Global(GTA.Offsets.Editor.ltm).Get<int>().ToString();
                                        if (!tbMissioninumbnc.IsFocused) tbMissioninumbnc.Text = new Global(GTA.Offsets.Editor.inumbnc).Get<int>().ToString();
                                        if (!tbMissionmrd.IsFocused) tbMissionmrd.Text = new Global(GTA.Offsets.Editor.mrd).Get<int>().ToString();
                                        if (!tbMissionxpr.IsFocused) tbMissionxpr.Text = new Global(GTA.Offsets.Editor.xpr).Get<int>().ToString();
                                        if (!tbMissioncshr.IsFocused) tbMissioncshr.Text = new Global(GTA.Offsets.Editor.cshr).Get<int>().ToString();
                                        if (!tbMissiontrel.IsFocused) tbMissiontrel.Text = new Global(GTA.Offsets.Editor.trel).Get<int>().ToString();
                                        if (!tbMissionrlopt.IsFocused) tbMissionrlopt.Text = new Global(GTA.Offsets.Editor.rlopt).Get<int>().ToString();
                                        if (!tbMissionctsc.IsFocused) tbMissionctsc.Text = new Global(GTA.Offsets.Editor.ctsc).Get<int>().ToString();
                                        if (!tbMissionadverm.IsFocused) tbMissionadverm.Text = new Global(GTA.Offsets.Editor.adverm).Get<int>().ToString();
                                        if (!tbMissiontwrst.IsFocused) tbMissiontwrst.Text = new Global(GTA.Offsets.Editor.twrst).Get<int>().ToString();
                                        if (!tbMissionvsbsout.IsFocused) tbMissionvsbsout.Text = new Global(GTA.Offsets.Editor.vsbsout).Get<int>().ToString();
                                        if (!tbMissionvsdfstc.IsFocused) tbMissionvsdfstc.Text = new Global(GTA.Offsets.Editor.vsdfstc).Get<int>().ToString();
                                        if (!tbMissionteambal.IsFocused) tbMissionteambal.Text = new Global(GTA.Offsets.Editor.teambal).Get<int>().ToString();
                                        if (!tbMissiondlcrel.IsFocused) tbMissiondlcrel.Text = new Global(GTA.Offsets.Editor.dlcrel).Get<int>().ToString();
                                        if (!tbMissiondtmp.IsFocused) tbMissiondtmp.Text = new Global(GTA.Offsets.Editor.dtmp).Get<int>().ToString();
                                        if (!tbMissiondtmp2.IsFocused) tbMissiondtmp2.Text = new Global(GTA.Offsets.Editor.dtmp2).Get<int>().ToString();
                                        if (!tbMissiondtmp3.IsFocused) tbMissiondtmp3.Text = new Global(GTA.Offsets.Editor.dtmp3).Get<int>().ToString();
                                        if (!tbMissionchksfx.IsFocused) tbMissionchksfx.Text = new Global(GTA.Offsets.Editor.chksfx).Get<int>().ToString();
                                        if (!tbMissionalrtLocal.IsFocused) tbMissionalrtLocal.Text = new Global(GTA.Offsets.Editor.alrtLocal).Get<int>().ToString();
                                        if (!tbMissionendtype.IsFocused) tbMissionendtype.Text = new Global(GTA.Offsets.Editor.endtype).Get<int>().ToString();


                                        Functions.Read.checkbinary(31, GTA.Offsets.Editor.menubs3, cboloarmor);
                                        Functions.Read.checkbinary(1, GTA.Offsets.Editor.menubs2, cboloweapon);
                                        Functions.Read.checkbinary(16, GTA.Offsets.Editor.menubs4, cbolohideweth);
                                        Functions.Read.checkbinary(23, GTA.Offsets.Editor.menubs8, cboloroundtime);
                                        Functions.Read.checkbinary(13, GTA.Offsets.Editor.menubs9, cbologametype);
                                        Functions.Read.checkbinary(19, GTA.Offsets.Editor.menubs3, cbolosuddendeath);
                                        Functions.Read.checkbinary(15, GTA.Offsets.Editor.menubs4, cbolohidetod);
                                        Functions.Read.checkbinary(2, GTA.Offsets.Editor.menubs5, cbololockvehicleclass);
                                        Functions.Read.checkbinary(15, GTA.Offsets.Editor.menubs5, cbolonumberofteams);
                                        Functions.Read.checkbinary(21, GTA.Offsets.Editor.menubs6, cboloci);
                                        Functions.Read.checkbinary(9, GTA.Offsets.Editor.menubs8, cbolotargetscore);
                                        Functions.Read.checkbinary(10, GTA.Offsets.Editor.menubs10, cbolorounds);
                                        Functions.Read.checkbinary(1, GTA.Offsets.Editor.menubs10, cbolokm);
                                        Functions.Read.checkbinary(2, GTA.Offsets.Editor.menubs10, cbologametype2);
                                        Functions.Read.checkbinary(1, GTA.Offsets.Editor.menubs13, cbolopu);
                                        Functions.Read.checkbinary(4, GTA.Offsets.Editor.menubs18, cbolovehlist);
                                        Functions.Read.checkbinary(26, GTA.Offsets.Editor.menubs2, cbolodisableblips);
                                        getNRCIDValues(false);
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionPlayerSettings)
                                    {
                                        int tindex = ddplyrteam.SelectedIndex;
                                        if (tindex > -1)
                                            if (!tbplyrno.IsFocused) tbplyrno.Text = new Global(GTA.Offsets.Editor.player_number + tindex).Get<int>().ToString();

                                        refreshPLYRCount();

                                        if (!ddplyrveh.IsDropDownOpen)
                                        {
                                            int counter = 0;
                                            ObservableCollection<string> temp = new ObservableCollection<string>();

                                            temp.Add(_Translation.Where(x => x.Key == "none").First().Value);

                                            for (int i = 0; i < new Global(GTA.Offsets.Editor.Vehicle.number).Get<int>(); i++)
                                            {
                                                //get the memory hadnle for the right addy
                                                Global vehicle = new Global(GTA.Offsets.Editor.Vehicle.model + i * GTA.Offsets.Editor.Vehicle.NEXT);
                                                if (vehicle.Get<int>() != 0)
                                                {
                                                    //calculate the number
                                                    counter++;
                                                    try
                                                    {
                                                        //try to get vehicle name
                                                        temp.Add(counter.ToString() + ". " + GTA.Editor.Vehiclenames.Where(x => (Functions.int_parse(Functions.joaat(x).ToString()) == vehicle.Get<int>())).FirstOrDefault());
                                                    }
                                                    catch (Exception)
                                                    {
                                                        //if vehicle name could not be found, use integer
                                                        temp.Add(counter.ToString() + ". " + vehicle.Get<int>().ToString());
                                                    }
                                                }
                                            }

                                            ddplyrveh.IsEnabled = counter > 0 ? true : false;
                                            ddplyrveh.ItemsSource = temp;


                                        }


                                        GetPlyrValues();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionTeamSettings)
                                    {
                                        CheckTeamSettingSection();
                                        Functions.Read.checkbinary(31, GTA.Offsets.Editor.menubs, cbmissionrspspawninveh);
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionIPL)
                                    {
                                        if (!tbMissioniplop.IsFocused) tbMissioniplop.Text = new Global(GTA.Offsets.Editor.iplop).Get<int>().ToString();
                                        if (!tbMissioniplop2.IsFocused) tbMissioniplop2.Text = new Global(GTA.Offsets.Editor.iplop2).Get<int>().ToString();
                                        if (!tbMissionintop.IsFocused) tbMissionintop.Text = new Global(GTA.Offsets.Editor.intop).Get<int>().ToString();
                                        if (!tbMissionintop2.IsFocused) tbMissionintop2.Text = new Global(GTA.Offsets.Editor.intop2).Get<int>().ToString();
                                        if (!tbMissionintop3.IsFocused) tbMissionintop3.Text = new Global(GTA.Offsets.Editor.intop3).Get<int>().ToString();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionTeleportMarkers)
                                    {
                                        GetTPMValues();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionKill)
                                    {
                                        GetKillValues();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionGang)
                                    {
                                        getGangValues();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionotzone)
                                    {
                                        GetOtzone();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionBlips)
                                    {
                                        int blipnum = new Global(GTA.Offsets.Editor.ddblip.number).Get<int>();

                                        if (ddmissionddblipno.Items.Count != blipnum)
                                        {
                                            if (blipnum <= 0)
                                            {
                                                ddmissionddblipno.ItemsSource = null;
                                            }
                                            else
                                            {
                                                if (blipnum < 16)
                                                {
                                                    ddmissionddblipno.ItemsSource = Enumerable.Range(1, blipnum);
                                                }
                                            }
                                        }
                                        GetBlips();
                                    }
                                    else if (PageInnerMission.SelectedItem == PageInnerMissionGoto)
                                    {
                                        int gotonum = new Global(GTA.Offsets.Editor.Locations.number).Get<int>();

                                        if (ddgotono.Items.Count != gotonum)
                                        {
                                            if (gotonum <= 0)
                                            {
                                                ddgotono.ItemsSource = null;
                                            }
                                            else
                                            {
                                                if (gotonum < 21)
                                                {
                                                    ddgotono.ItemsSource = Enumerable.Range(1, gotonum);
                                                }
                                            }
                                        }

                                        GetGoto();
                                    }

                                }
                                else if (EditPages.SelectedItem == PageCapture)
                                {
                                    if (PageInnerCapture.SelectedItem == PageInnerCaptureGeneral)
                                    {
                                        if (ddcaptureno.SelectedIndex == -1)
                                        {
                                            ddcaptureno.SelectedIndex = 0;
                                        }
                                    }
                                    else if (PageInnerCapture.SelectedItem == PageInnerCaptureObjects)
                                    {
                                        int objnum = new Global(GTA.Offsets.Editor.Objects.number).Get<int>();

                                        if (ddobjno.Items.Count != objnum)
                                        {
                                            if (objnum <= 0)
                                            {
                                                ddobjno.ItemsSource = null;
                                            }
                                            else
                                            {
                                                if (objnum < 21)
                                                {
                                                    ddobjno.ItemsSource = Enumerable.Range(1, objnum);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (EditPages.SelectedItem == PageZone)
                                {
                                    int zonenum = new Global(GTA.Offsets.Editor.Zones.number).Get<int>();

                                    if (ddzoneno.Items.Count != zonenum)
                                    {
                                        if (zonenum <= 0)
                                        {
                                            ddzoneno.ItemsSource = null;
                                        }
                                        else
                                        {
                                            if (zonenum < 33)
                                            {
                                                ddzoneno.ItemsSource = Enumerable.Range(1, zonenum);
                                            }
                                        }
                                    }

                                    GetZone();
                                }
                                else if (EditPages.SelectedItem == PageSurvival)
                                {
                                    GetSurvivalValues();
                                }
                                else if (EditPages.SelectedItem == PageDeathmatch)
                                {
                                }

                            }
                        }

                        if (MainPages.SelectedItem == PageDashboard)
                        {
                            Lbl_SCName.Text = getSCName();
                            Lblonlineversion.Text = GTA.getOnlineVersion();
                            Lblbuildversion.Text = GTA.getBuildVersion();
                            if (!cb_dev.IsFocused) cb_dev.IsChecked = m.memory(GTA.Offsets.Editor.dev).Get<int>() == GTA.DevPatched ? true : false;
                            tbjobid.Text = new Global(GTA.Offsets.Editor.jobid).GetString();
                        }
                        else if (MainPages.SelectedItem == PageEdit)
                        {
                            if (EditPages.SelectedItem == PageProps)
                            {
                                if (PageInnerProps.SelectedItem == PageInnerModdedProps)
                                {
                                    if (!cbMPropsForceMurica.IsFocused)
                                    {
                                        cbMPropsForceMurica.IsChecked = new Global(GTA.Offsets.Editor.enable_murica).Get<int>() == 1;
                                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (System.Action)(() =>
                                        {
                                            EnsureModdedPropSourcesInitialized();
                                        }));
                                    }
                                }
                            }
                        }
                        else if (MainPages.SelectedItem == PageMod)
                        {
                            if (PageInnerMod.SelectedItem == PageInnerModJL)
                            {
                                getjoblinks();
                            }
                            else if (PageInnerMod.SelectedItem == PageInnerModnrcidjc)
                            {
                                if (!tbnrcidjcstartlocx.IsFocused) tbnrcidjcstartlocx.Text = new Global(GTA.Offsets.Editor.start).Get<float>().ToString();
                                if (!tbnrcidjcstartlocy.IsFocused) tbnrcidjcstartlocy.Text = new Global(GTA.Offsets.Editor.start + 1).Get<float>().ToString();
                                if (!tbnrcidjcstartlocz.IsFocused) tbnrcidjcstartlocz.Text = new Global(GTA.Offsets.Editor.start + 2).Get<float>().ToString();
                                getNRCIDValuesJC(false);
                            }
                        }
                        else if (MainPages.SelectedItem == PageSettings)
                        {
                            //int rating = 0;
                            //int num = new Global(GTA.Offsets.Editor.Jobs.published_number).Get<int>();

                            //tbSCMapsCreated.Text = num.ToString();


                            //rating = rating / num;
                            //tbSCRating.Text = rating.ToString();
                        }
                    }
                }
                else
                {
                    // No game: nothing to launch, and a button left over
                    // from the last session would act on a dead process.
                    BtnLaunchCreator.Visibility = Visibility.Collapsed;
                }
            };

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, workMethod, null);
        }
    }
}
