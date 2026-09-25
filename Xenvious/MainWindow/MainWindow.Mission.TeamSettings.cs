using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / TeamSettings page.
    public partial class MainWindow
    {
        // freeeze threads
        public Thread freezeNRLThread;
        public Thread freezeNRLAllThread;
        public Thread freezePLYLThread;

        private void ddmissionrspmodel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                Vehicle selectedVeh = ((Vehicle)ddmissionrspmodel.SelectedItem);
                if (ddmissionrspmodel.SelectedIndex > -1)
                {
                    new Global(GTA.Offsets.Editor.teamv + ddmissionteamno.SelectedIndex).SetInt(selectedVeh.Integer);
                }
                ddmissionrspweapons.Visibility = Visibility.Collapsed;
                ddmissionrsparmor.Visibility = Visibility.Collapsed;
                ddmissionrspacm.Visibility = Visibility.Collapsed;
                ddmissionrspbombs.Visibility = Visibility.Collapsed;
                ddmissionrspweapons.SelectedIndex = -1;
                ddmissionrsparmor.SelectedIndex = -1;
                ddmissionrspacm.SelectedIndex = -1;
                ddmissionrspbombs.SelectedIndex = -1;
                //check if current rsp is deluxo to show Spawn Hovered cb
                if (rspvehs.ArmoredVehicles.Select(x => x._native).Contains(selectedVeh.Name))
                {
                    var rspveh = rspvehs.ArmoredVehicles.Where(x => x._native == selectedVeh.Name).First();
                    if (rspveh._hasweapons)
                    {
                        ddmissionrspweapons.Visibility = Visibility.Visible;
                        ddmissionrspweapons.SelectedIndex = -1;
                        ddmissionrspweapons.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = rspveh._weapons });
                        ddmissionrspweapons.DisplayMemberPath = "Name";
                        ddmissionrspweapons.SelectedIndex = new Global(GTA.Offsets.Editor.tvmt + ddmissionteamno.SelectedIndex).Get<int>() + 1;
                    }
                    if (rspveh._hasarmor)
                    {
                        ddmissionrsparmor.Visibility = Visibility.Visible;
                        ddmissionrsparmor.SelectedIndex = -1;
                        ddmissionrsparmor.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = rspveh._armor });
                        ddmissionrsparmor.DisplayMemberPath = "Name";
                        ddmissionrsparmor.SelectedIndex = new Global(GTA.Offsets.Editor.tvma + ddmissionteamno.SelectedIndex).Get<int>() + 1;
                    }
                    if (rspveh._hasacm)
                    {
                        ddmissionrspacm.Visibility = Visibility.Visible;
                        ddmissionrspacm.SelectedIndex = -1;
                        ddmissionrspacm.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = rspveh._acm });
                        ddmissionrspacm.DisplayMemberPath = "Name";
                        ddmissionrspacm.SelectedIndex = new Global(GTA.Offsets.Editor.tvmac + ddmissionteamno.SelectedIndex).Get<int>() + 1;
                    }
                    if (rspveh._hasbombs)
                    {
                        ddmissionrspbombs.Visibility = Visibility.Visible;
                        ddmissionrspbombs.SelectedIndex = -1;
                        ddmissionrspbombs.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = rspveh._bombs });
                        ddmissionrspbombs.DisplayMemberPath = "Name";
                        ddmissionrspbombs.SelectedIndex = new Global(GTA.Offsets.Editor.tvBomb + ddmissionteamno.SelectedIndex).Get<int>() + 1;
                    }
                }
                cbmissionrspdeluxohover.Visibility = selectedVeh.Name == "deluxo" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void cbmissionrspdeluxohover_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(28, GTA.Offsets.Editor.tmbt3 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionrspdeluxohover);
        }

        private void tbmissionrsphealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrsphealth.Text, false))
                new Global(GTA.Offsets.Editor.tmvhp + ddmissionteamno.SelectedIndex).SetInt(tbmissionrsphealth.Text);
        }

        private void cbmissionrspbptires_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.tmbts + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionrspbptires);
        }

        private void ddmissionrspvehcol1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.teamrvc + ddmissionteamno.SelectedIndex).SetInt(ddmissionrspvehcol1.SelectedIndex - 1);
        }

        private void ddmissionrspvehcol2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.teamrvcs + ddmissionteamno.SelectedIndex).SetInt(ddmissionrspvehcol2.SelectedIndex - 1);
        }

        private void tbmissionrspspoiler_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrspspoiler.Text))
                new Global(GTA.Offsets.Editor.tvmspoil + ddmissionteamno.SelectedIndex).SetInt(tbmissionrspspoiler.Text);
            //new Global(GTA.Offsets.Editor.tehrn + (10 * 5) + ddmissionteamno.SelectedIndex).SetInt(tbmissionrspspoiler.Text);
        }

        private void tbmissionrspexhaust_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrspexhaust.Text))
                new Global(GTA.Offsets.Editor.tvmet + ddmissionteamno.SelectedIndex).SetInt(tbmissionrspexhaust.Text);
            //new Global(GTA.Offsets.Editor.tehrn + (6 * 5) + ddmissionteamno.SelectedIndex).SetInt(tbmissionrspexhaust.Text);
        }

        private void tbmissionrspdmgm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global(GTA.Offsets.Editor.tmvds + ddmissionteamno.SelectedIndex).SetFloat(tbmissionrspdmgm.Text);
        }

        private void tbmissionrspbodyhealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrspbodyhealth.Text, false))
                new Global(GTA.Offsets.Editor.teamrvbh + ddmissionteamno.SelectedIndex).SetInt(tbmissionrspbodyhealth.Text);
        }

        public void CheckTeamSettingSection(bool ignore_focus = false)
        {
            if (m.IsProcOpen)
            {
                //Respawn Vehicle
                try
                {
                    //Personal Vehicle
                    if (!cbmissionrsppersonal.IsFocused || ignore_focus) Functions.Read.checkbinary(11, GTA.Offsets.Editor.rlopt, cbmissionrsppersonal);

                    //Model
                    if (!ddmissionrspmodel.IsFocused || ignore_focus)
                    {
                        int index = Array.IndexOf(ddmissionrspmodel.ItemsSource.Cast<Vehicle>().Select(x => x.Integer).ToArray(), new Global(GTA.Offsets.Editor.teamv + ddmissionteamno.SelectedIndex).Get<int>());
                        if (index != -1)
                            ddmissionrspmodel.SelectedIndex = index;
                        else
                            ddmissionrspmodel.SelectedIndex = 0;
                    }

                    //Primary
                    if (!ddmissionrspvehcol1.IsFocused || ignore_focus) ddmissionrspvehcol1.SelectedIndex = new Global(GTA.Offsets.Editor.teamrvc + ddmissionteamno.SelectedIndex).Get<int>() + 1;

                    //Secondary
                    if (!ddmissionrspvehcol2.IsFocused || ignore_focus) ddmissionrspvehcol2.SelectedIndex = new Global(GTA.Offsets.Editor.teamrvcs + ddmissionteamno.SelectedIndex).Get<int>() + 1;

                    //Health
                    if (!tbmissionrsphealth.IsFocused || ignore_focus) tbmissionrsphealth.Text = new Global(GTA.Offsets.Editor.tmvhp + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //Body Health
                    if (!tbmissionrspbodyhealth.IsFocused || ignore_focus) tbmissionrspbodyhealth.Text = new Global(GTA.Offsets.Editor.teamrvbh + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //Dmg Multiplier
                    if (!tbmissionrspdmgm.IsFocused || ignore_focus) tbmissionrspdmgm.Text = new Global(GTA.Offsets.Editor.tmvds + ddmissionteamno.SelectedIndex).Get<float>().ToString();

                    //Bulletproof Tires
                    if (!cbmissionrspbptires.IsFocused || ignore_focus) Functions.Read.checkbinary(15, GTA.Offsets.Editor.tmbts + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionrspbptires);

                    //Deluxo Hover
                    if (!cbmissionrspdeluxohover.IsFocused || ignore_focus) Functions.Read.checkbinary(28, GTA.Offsets.Editor.tmbt3 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionrspdeluxohover);

                    //Spoiler
                    if (!tbmissionrspspoiler.IsFocused || ignore_focus) tbmissionrspspoiler.Text = new Global(GTA.Offsets.Editor.tvmspoil + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //Exhaust
                    if (!tbmissionrspexhaust.IsFocused || ignore_focus) tbmissionrspexhaust.Text = new Global(GTA.Offsets.Editor.tvmet + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //Weapon
                    if (!tbmissionrsptvmt.IsFocused || ignore_focus) tbmissionrsptvmt.Text = new Global(GTA.Offsets.Editor.tvmt + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //Armor
                    if (!tbmissionrsptvma.IsFocused || ignore_focus) tbmissionrsptvma.Text = new Global(GTA.Offsets.Editor.tvma + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //ACM
                    if (!tbmissionrsptvmac.IsFocused || ignore_focus) tbmissionrsptvmac.Text = new Global(GTA.Offsets.Editor.tvmac + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //Bomb
                    if (!tbmissionrsptvbomb.IsFocused || ignore_focus) tbmissionrsptvbomb.Text = new Global(GTA.Offsets.Editor.tvBomb + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //tvpm
                    if (!tbmissionrsptvpm.IsFocused || ignore_focus) tbmissionrsptvpm.Text = new Global(GTA.Offsets.Editor.tvpm + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //tvnc
                    if (!tbmissionrsptvnc.IsFocused || ignore_focus) tbmissionrsptvnc.Text = new Global(GTA.Offsets.Editor.tvnc + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                }
                catch (Exception)
                {
                }

                //Other
                try
                {
                    //MarkPlayzone
                    if (!cbmissionmarkpz.IsFocused || ignore_focus) Functions.Read.checkbinary(18, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionmarkpz);

                    //CantLeaveVehicle
                    if (!cbmissionclv.IsFocused || ignore_focus) Functions.Read.checkbinary(9, GTA.Offsets.Editor.irbs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionclv);

                    //SlowMovement
                    if (!cbmissionslowm.IsFocused || ignore_focus)
                    {
                        cbmissionslowm.IsChecked =
                        (Functions.Read.checkbinary(19, GTA.Offsets.Editor.tmbt2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionslowm) &&
                        Functions.Read.checkbinary(31, GTA.Offsets.Editor.menubs13, cbmissionslowm)) ? true : false;
                    }

                    //disable veh weapon
                    if (!cbmissiondisablevehweap.IsFocused || ignore_focus) Functions.Read.checkbinary(24, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissiondisablevehweap);

                    //veh special ability bar
                    if (!cbmissionvehsabar.IsFocused || ignore_focus) Functions.Read.checkbinary(1, GTA.Offsets.Editor.irbs14 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionvehsabar);

                    //Explode
                    if (!cbmissionexplodearea.IsFocused || ignore_focus)
                    {
                        cbmissionexplodearea.IsChecked =
                        (Functions.Read.checkbinary(9, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionexplodearea) &&
                        Functions.Read.checkbinary(8, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionexplodearea)) ? true : false;
                    }
                    if (!tbmissionexplodeareatimer.IsFocused || ignore_focus) tbmissionexplodeareatimer.Text = new Global(GTA.Offsets.Editor.bdprt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();

                    //nightvision, heatvision
                    if (!cbmissionnv.IsFocused || ignore_focus) Functions.Read.checkbinary(25, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionnv);
                    if (!cbmissionnv.IsFocused || ignore_focus) Functions.Read.checkbinary(10, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhv);
                    if (!cbmissionhvalways.IsFocused || ignore_focus) Functions.Read.checkbinary(18, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhvalways);
                    if (!tbmissionhvstart.IsFocused || ignore_focus) tbmissionhvstart.Text = new Global(GTA.Offsets.Editor.itvsd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionhvend.IsFocused || ignore_focus) tbmissionhvend.Text = new Global(GTA.Offsets.Editor.itved + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!cbmissionlosehealth.IsFocused || ignore_focus) Functions.Read.checkbinary(15, GTA.Offsets.Editor.irbs6 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionlosehealth);
                    if (!cbmissionlosehealthoopa.IsFocused || ignore_focus) Functions.Read.checkbinary(32, GTA.Offsets.Editor.irbs9 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionlosehealthoopa);

                    //Gear
                    Gear tmp_Gear = new Gear(new Global(GTA.Offsets.Editor.gear + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.gear_NEXT).Get<int>(),
                        new Global(GTA.Offsets.Editor.geard + ddmissionteamno.SelectedIndex).Get<int>());
                    try
                    {
                        ddmissiongear.SelectedItem = ddmissiongear.Items.Cast<ComboBoxItem>().Where(x => (((Gear)x.Tag).gear == tmp_Gear.gear && ((Gear)x.Tag).geard == tmp_Gear.geard)).First();
                    }
                    catch (Exception)
                    {
                        ddmissiongear.SelectedIndex = -1;
                    }

                    //Outfit
                    int rloft = new Global(GTA.Offsets.Editor.rloft + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>();
                    if (ddmissionoutfit.SelectedIndex == 0)
                    {
                        if (!tbmissionoutfitcustom.IsFocused || ignore_focus) tbmissionoutfitcustom.Text = rloft.ToString();
                    }
                    else
                    {
                        try
                        {
                            ddmissionoutfit.SelectedItem = ddmissionoutfit.ItemsSource.Cast<Outfit>().ToArray().Where(y => y.Value == rloft).First();
                        }
                        catch (Exception)
                        {
                            ddmissionoutfit.SelectedIndex = 0;
                            if (!tbmissionoutfitcustom.IsFocused || ignore_focus) tbmissionoutfitcustom.Text = rloft.ToString();
                        }
                    }

                    if (plylfreeze == null)
                    {
                        plylfreeze = new List<int>();
                        for (int i = 0; i < 4; i++)
                        {
                            plylfreeze.Add(new Global(GTA.Offsets.Editor.plyl + i * GTA.Offsets.Editor.team_NEXT).Get<int>());
                        }
                    }
                    if (!Functions.Read.isLTS())
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            plylfreeze[i] = new Global(GTA.Offsets.Editor.plyl + i * GTA.Offsets.Editor.team_NEXT).Get<int>();
                        }
                    }
                    if (!ddmissionplyl.IsFocused || ignore_focus) ddmissionplyl.SelectedIndex = plylfreeze[ddmissionteamno.SelectedIndex];
                    if (!tbmissionplvrl.IsFocused || ignore_focus) tbmissionplvrl.Text = new Global(GTA.Offsets.Editor.plvrl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();

                    //teamname
                    string csttn = new Global(GTA.Offsets.Editor.csttn + ddmissionteamno.SelectedIndex * 4).GetString(80);
                    if (ddmissioncsttn.SelectedIndex == 0)
                    {
                        if (!tbmissioncsttncustom.IsFocused || ignore_focus) tbmissioncsttncustom.Text = csttn;
                    }
                    else
                    {
                        try
                        {
                            if (new Global(GTA.Offsets.Editor.tenms + ddmissionteamno.SelectedIndex).Get<int>() != 38)
                                throw new Exception();
                            ddmissioncsttn.SelectedItem = ddmissioncsttn.ItemsSource.Cast<Teamname>().ToArray().Where(y => y.Value == csttn).First();
                        }
                        catch (Exception)
                        {
                            ddmissioncsttn.SelectedIndex = 0;
                            if (!tbmissioncsttncustom.IsFocused || ignore_focus) tbmissioncsttncustom.Text = csttn;
                        }
                    }

                    if (!ddmissionwantedlevel.IsFocused || ignore_focus)
                    {
                        if (new Global(GTA.Offsets.Editor.fkwl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>() == 6)
                        {
                            ddmissionwantedlevel.SelectedIndex = 6;
                        }
                        else
                        {
                            ddmissionwantedlevel.SelectedIndex = (new Global(GTA.Offsets.Editor.wchg + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>() - 1);
                        }
                    }

                    //hide wanted level
                    if (!cbmissionhidewantedlevel.IsFocused || ignore_focus) Functions.Read.checkbinary(2, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhidewantedlevel);


                    //hide time
                    if (!tbmissionnrl.IsFocused || ignore_focus) tbmissionnrl.Text = new Global(GTA.Offsets.Editor.nrl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!cbmissionhidetime.IsFocused || ignore_focus) Functions.Read.checkbinary(22, GTA.Offsets.Editor.irbs8 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhidetime);
                    if (!cbmissionsinv.IsFocused || ignore_focus) Functions.Read.checkbinary(12, GTA.Offsets.Editor.irbs10 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionsinv);
                    if (!cbmissiondob.IsFocused || ignore_focus) Functions.Read.checkbinary(20, GTA.Offsets.Editor.irbs10 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissiondob);
                    if (!cbmissionflare.IsFocused || ignore_focus) Functions.Read.checkbinary(18, GTA.Offsets.Editor.irbs14 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionflare);
                    if (!cbmissionhidemap.IsFocused || ignore_focus) Functions.Read.checkbinary(32, GTA.Offsets.Editor.irbs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhidemap);
                    if (!cbmissionspawninaction.IsFocused || ignore_focus) Functions.Read.checkbinary(25, GTA.Offsets.Editor.irbs2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionspawninaction);
                    if (!tbmissionminspd.IsFocused || ignore_focus) tbmissionminspd.Text = new Global(GTA.Offsets.Editor.minspd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionmspdlp.IsFocused || ignore_focus) tbmissionmspdlp.Text = new Global(GTA.Offsets.Editor.mspdlp + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionmspdsv.IsFocused || ignore_focus) tbmissionmspdsv.Text = new Global(GTA.Offsets.Editor.mspdsv + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionfsdtmr.IsFocused || ignore_focus) tbmissionfsdtmr.Text = new Global(GTA.Offsets.Editor.fsdtmr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionmspdmx.IsFocused || ignore_focus) tbmissionmspdmx.Text = new Global(GTA.Offsets.Editor.mspdmx + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionrloft.IsFocused || ignore_focus) tbmissionrloft.Text = new Global(GTA.Offsets.Editor.rloft + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionrloftv.IsFocused || ignore_focus) tbmissionrloftv.Text = new Global(GTA.Offsets.Editor.rloftv + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionmcry.IsFocused || ignore_focus) tbmissionmcry.Text = new Global(GTA.Offsets.Editor.mcry + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionmcstr.IsFocused || ignore_focus) tbmissionmcstr.Text = new Global(GTA.Offsets.Editor.mcstr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionmcmp.IsFocused || ignore_focus) tbmissionmcmp.Text = new Global(GTA.Offsets.Editor.mcmp + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionmcsrm.IsFocused || ignore_focus) tbmissionmcsrm.Text = new Global(GTA.Offsets.Editor.mcsrm + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissiontmt.IsFocused || ignore_focus) tbmissiontmt.Text = new Global(GTA.Offsets.Editor.tmt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissiontms.IsFocused || ignore_focus) tbmissiontms.Text = new Global(GTA.Offsets.Editor.tms + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionshdtxt.IsFocused || ignore_focus) tbmissionshdtxt.Text = new Global(GTA.Offsets.Editor.shdtxt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionarmor.IsFocused || ignore_focus) tbmissionarmor.Text = new Global(GTA.Offsets.Editor.armr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionfail_txt.IsFocused || ignore_focus) tbmissionfail_txt.Text = Encoding.UTF8.GetString(new Global(GTA.Offsets.Editor.fail_txt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).GetBytes(80));
                    if (!tbmissionvss.IsFocused || ignore_focus) tbmissionvss.Text = Encoding.UTF8.GetString(new Global(GTA.Offsets.Editor.vss + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).GetBytes(80));
                    if (!cbmissionvehrsp.IsFocused || ignore_focus) Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.vehrsp, cbmissionvehrsp);
                    if (!cbmissionb2pa.IsFocused || ignore_focus) Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.boud, cbmissionb2pa);
                    if (!tbmissionmrtl.IsFocused || ignore_focus) tbmissionmrtl.Text = new Global(GTA.Offsets.Editor.mrtl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>().ToString();


                    // team based settings
                    if (!tbmissiontmbts.IsFocused || ignore_focus) tbmissiontmbts.Text = new Global(GTA.Offsets.Editor.tmbts + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiontmbt2.IsFocused || ignore_focus) tbmissiontmbt2.Text = new Global(GTA.Offsets.Editor.tmbt2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiontmbt3.IsFocused || ignore_focus) tbmissiontmbt3.Text = new Global(GTA.Offsets.Editor.tmbt3 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiontmbt4.IsFocused || ignore_focus) tbmissiontmbt4.Text = new Global(GTA.Offsets.Editor.tmbt4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionabits.IsFocused || ignore_focus) tbmissionabits.Text = new Global(GTA.Offsets.Editor.abits + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionsdobs.IsFocused || ignore_focus) tbmissionsdobs.Text = new Global(GTA.Offsets.Editor.sdobs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionedobs.IsFocused || ignore_focus) tbmissionedobs.Text = new Global(GTA.Offsets.Editor.edobs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiondogps.IsFocused || ignore_focus) tbmissiondogps.Text = new Global(GTA.Offsets.Editor.dogps + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionmnumpt.IsFocused || ignore_focus) tbmissionmnumpt.Text = new Global(GTA.Offsets.Editor.mnumpt + ddmissionteamno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionnumpt.IsFocused || ignore_focus) tbmissionnumpt.Text = new Global(GTA.Offsets.Editor.numpt + ddmissionteamno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionboud.IsFocused || ignore_focus) tbmissionboud.Text = new Global(GTA.Offsets.Editor.boud + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionmts.IsFocused || ignore_focus) tbmissionmts.Text = new Global(GTA.Offsets.Editor.mts + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionmcvbs.IsFocused || ignore_focus) tbmissionmcvbs.Text = new Global(GTA.Offsets.Editor.mcvbs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionclrovr.IsFocused || ignore_focus) tbmissionclrovr.Text = new Global(GTA.Offsets.Editor.clrovr + ddmissionteamno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissiontblpv1.IsFocused || ignore_focus) tbmissiontblpv1.Text = new Global(GTA.Offsets.Editor.tblpv1 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiontblpv2.IsFocused || ignore_focus) tbmissiontblpv2.Text = new Global(GTA.Offsets.Editor.tblpv2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiontblpv3.IsFocused || ignore_focus) tbmissiontblpv3.Text = new Global(GTA.Offsets.Editor.tblpv3 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiontblpv4.IsFocused || ignore_focus) tbmissiontblpv4.Text = new Global(GTA.Offsets.Editor.tblpv4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionfail.IsFocused || ignore_focus) tbmissionfail.Text = new Global(GTA.Offsets.Editor.fail + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionpcbd.IsFocused || ignore_focus) tbmissionpcbd.Text = new Global(GTA.Offsets.Editor.pcbd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionvehrsp.IsFocused || ignore_focus) tbmissionvehrsp.Text = new Global(GTA.Offsets.Editor.vehrsp + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissioninpts.IsFocused || ignore_focus) tbmissioninpts.Text = new Global(GTA.Offsets.Editor.inpts + ddmissionteamno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissiontmtsr.IsFocused || ignore_focus) tbmissiontmtsr.Text = new Global(GTA.Offsets.Editor.tmtsr + ddmissionteamno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissiontmrph.IsFocused || ignore_focus) tbmissiontmrph.Text = new Global(GTA.Offsets.Editor.tmrph + ddmissionteamno.SelectedIndex).Get<int>().ToString();
                    if (!tbmissionspar.IsFocused || ignore_focus) tbmissionspar.Text = new Global(GTA.Offsets.Editor.spar + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionmslr.IsFocused || ignore_focus) tbmissionmslr.Text = new Global(GTA.Offsets.Editor.mslr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissionppk.IsFocused || ignore_focus) tbmissionppk.Text = new Global(GTA.Offsets.Editor.ppk + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();

                }
                catch (Exception)
                {
                }

                //advanced
                try
                {
                    //irbs
                    if (!tbmissionirbs.IsFocused || ignore_focus) tbmissionirbs.Text = new Global(GTA.Offsets.Editor.irbs + (GTA.Offsets.Editor.irbs_NEXT * ddmissionirbsno.SelectedIndex) + (GTA.Offsets.Editor.team_NEXT * ddmissionteamno.SelectedIndex) + ddmissionnrlno.SelectedIndex).Get<int>().ToString();

                    //Job time
                    int time = new Global(GTA.Offsets.Editor.tmt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).Get<int>();
                    try
                    {
                        ddtime.SelectedItem = ddtime.ItemsSource.Cast<JobTime>().ToArray().Where(y => y.Value == time).First();
                    }
                    catch (Exception)
                    {
                        ddtime.SelectedIndex = -1;
                    }

                    if (!cbmissionequippara.IsFocused || ignore_focus) Functions.Read.checkbinary(22, GTA.Offsets.Editor.menubs3, cbmissionequippara);

                    if (!ddmissionparacolor.IsFocused || ignore_focus)
                    {
                        try
                        {
                            ddmissionparacolor.SelectedIndex = new Global(GTA.Offsets.Editor.ptint + ddmissionteamno.SelectedIndex).Get<int>();
                        }
                        catch (Exception)
                        {
                            ddmissionparacolor.SelectedIndex = -1;
                        }
                    }
                    if (!tbmissionparastyle.IsFocused || ignore_focus) tbmissionparastyle.Text = new Global(GTA.Offsets.Editor.pptint + ddmissionteamno.SelectedIndex).Get<int>().ToString();

                    //hide map

                    if (ddmissionteamno.SelectedIndex == 0)
                    {
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
                    }
                    else if (ddmissionteamno.SelectedIndex == 1)
                    {
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
                    }
                    else if (ddmissionteamno.SelectedIndex == 2)
                    {
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
                    }
                    else if (ddmissionteamno.SelectedIndex == 4)
                    {
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
                        Functions.Read.checkbinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
                    }



                    // beast mode
                    if (!cbmissionbeastenable.IsFocused || ignore_focus)
                    {
                        if (Functions.Read.checkbinary(12, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex)
                            && Functions.Read.checkbinary(8, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex))
                        {
                            cbmissionbeastenable.IsChecked = true;
                        }
                        else
                        {
                            cbmissionbeastenable.IsChecked = false;
                        }
                    }
                    if (!cbmissionbeastenablembs.IsFocused || ignore_focus)
                    {
                        if (Functions.Read.checkbinary(27, GTA.Offsets.Editor.menubs5) &&
                            Functions.Read.checkbinary(24, GTA.Offsets.Editor.menubs5) &&
                            Functions.Read.checkbinary(20, GTA.Offsets.Editor.menubs5) &&
                            Functions.Read.checkbinary(3, GTA.Offsets.Editor.menubs5))
                        {
                            cbmissionbeastenablembs.IsChecked = true;
                        }
                        else
                        {
                            cbmissionbeastenablembs.IsChecked = false;
                        }
                    }

                    if (!cbmissionbeastjump.IsFocused || ignore_focus) Functions.Read.checkbinary(14, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastjump);
                    if (!cbmissionbeastinv.IsFocused || ignore_focus) Functions.Read.checkbinary(15, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastinv);
                    if (!cbmissionbeastcrit.IsFocused || ignore_focus) Functions.Read.checkbinary(20, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastcrit);
                    if (!cbmissionbeastrunspd.IsFocused || ignore_focus) Functions.Read.checkbinary(13, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastrunspd);

                    if (!tbmissionbeasthealth.IsFocused || ignore_focus) tbmissionbeasthealth.Text = new Global((GTA.Offsets.Editor.bmmxh + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).Get<int>().ToString();
                    if (!tbmissionbeastjmpdmg.IsFocused || ignore_focus) tbmissionbeastjmpdmg.Text = new Global((GTA.Offsets.Editor.bmsjd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).Get<int>().ToString();
                    if (!tbmissionbeastrecharge.IsFocused || ignore_focus) tbmissionbeastrecharge.Text = new Global((GTA.Offsets.Editor.bmhrgn + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).Get<float>().ToString();
                    if (!tbmissionbeastinvdmg.IsFocused || ignore_focus) tbmissionbeastinvdmg.Text = new Global((GTA.Offsets.Editor.bmstd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).Get<int>().ToString();
                    if (!tbmissionbeastmeeledmg.IsFocused || ignore_focus) tbmissionbeastmeeledmg.Text = new Global((GTA.Offsets.Editor.bmmdm + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).Get<int>().ToString();
                    if (!tbmissionbeastrunspd.IsFocused || ignore_focus) tbmissionbeastrunspd.Text = new Global((GTA.Offsets.Editor.bmspm + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).Get<float>().ToString();
                }
                catch (Exception)
                {
                }
            }
        }

        private void cbmissionmarkpz_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(18, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionmarkpz);
        }

        private void cbmissionclv_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(9, GTA.Offsets.Editor.irbs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionclv);
        }

        private void cbmissionslowm_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.tmbt2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionslowm);
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs13, cbmissionslowm);
        }

        private void cbmissionexplodearea_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(8, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionexplodearea);
            Functions.Write.writebinary(9, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionexplodearea);
        }

        private void missionexplodeareatimer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionexplodeareatimer.Text, false))
                new Global(GTA.Offsets.Editor.bdprt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionexplodeareatimer.Text);
        }

        private void cbmissionrspspawninveh_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs, cbmissionrspspawninveh);
        }

        private void ddmissiongear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var gear = (Gear)((ComboBoxItem)ddmissiongear.SelectedItem).Tag;

                new Global(GTA.Offsets.Editor.gear + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.gear_NEXT).SetInt(gear.gear);
                new Global(GTA.Offsets.Editor.geard + ddmissionteamno.SelectedIndex).SetInt(gear.geard);

            }
        }

        private void cbmissionrsppersonal_Checked(object sender, RoutedEventArgs e)
        {
            bool enabled = cbmissionrsppersonal.IsChecked == true;
            ddmissionrspmodel.IsEnabled = !enabled;
            ddmissionrspvehcol1.IsEnabled = !enabled;
            ddmissionrspvehcol2.IsEnabled = !enabled;
            Functions.Write.writebinary(11, GTA.Offsets.Editor.rlopt, cbmissionrsppersonal);
        }

        private void ddmissionoutfit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && !(((Outfit)ddmissionoutfit.SelectedItem).Name).Equals("Custom"))
            {
                new Global(GTA.Offsets.Editor.rloft + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(((Outfit)ddmissionoutfit.SelectedItem).Value);
                try
                {

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"https://xenvious.com/resources/outfits/" + ((Outfit)ddmissionoutfit.SelectedItem).Value + ".jpg", UriKind.Absolute);
                    bitmap.EndInit();

                    imgmissionrloftdd.Source = bitmap;
                }
                catch (Exception)
                {

                }
            }
        }

        private void tbmissionoutfitcustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && (((Outfit)ddmissionoutfit.SelectedItem).Name).Equals("Custom"))
            {
                new Global(GTA.Offsets.Editor.rloft + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionoutfitcustom.Text);

                try
                {

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"https://xenvious.com/resources/outfits/" + tbmissionoutfitcustom.Text + ".jpg", UriKind.Absolute);
                    bitmap.EndInit();

                    imgmissionrloftcst.Source = bitmap;
                }
                catch (Exception)
                {

                }
            }
        }

        private void ddmissionirbsno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                tbmissionirbs.Text = new Global(GTA.Offsets.Editor.irbs + (GTA.Offsets.Editor.irbs_NEXT * ddmissionirbsno.SelectedIndex) + (GTA.Offsets.Editor.team_NEXT * ddmissionteamno.SelectedIndex + ddmissionnrlno.SelectedIndex)).Get<int>().ToString();

                IInputElement focusedControl = FocusManager.GetFocusedElement(this);

                if (focusedControl is TextBox)
                {
                    if ((focusedControl as TextBox).Name == "tbmissionirbs")
                    {
                        tbmissionirbs.SelectAll();
                    }
                }
            }
        }

        private void tbmissionirbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionirbs.Text))
            {
                new Global(GTA.Offsets.Editor.irbs + (GTA.Offsets.Editor.irbs_NEXT * ddmissionirbsno.SelectedIndex) + (GTA.Offsets.Editor.team_NEXT * ddmissionteamno.SelectedIndex + ddmissionnrlno.SelectedIndex)).SetInt(tbmissionirbs.Text);
            }
        }

        private void cbmissiondisablevehweap_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(24, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissiondisablevehweap);
        }

        private void cbmissionvehsabar_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.irbs14 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionvehsabar);
        }

        private void cbmissionequippara_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(22, GTA.Offsets.Editor.menubs3, cbmissionequippara);
        }

        private void tbmissionparastyle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionparastyle.Text))
            {
                new Global(GTA.Offsets.Editor.pptint + ddmissionteamno.SelectedIndex).SetInt(tbmissionparastyle.Text);
            }
        }

        private void ddmissionparacolor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.ptint + ddmissionteamno.SelectedIndex).SetInt(ddmissionparacolor.SelectedIndex);
            }
        }


        private void ddtime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.tmt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(((JobTime)ddtime.SelectedItem).Value);
            }
        }

        private void cbmissionbeastenable_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastenable);
            Functions.Write.writebinary(8, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastenable);

            Functions.Write.writebinary(27, GTA.Offsets.Editor.menubs5, cbmissionbeastenable);
            Functions.Write.writebinary(24, GTA.Offsets.Editor.menubs5, cbmissionbeastenable);
            Functions.Write.writebinary(20, GTA.Offsets.Editor.menubs5, cbmissionbeastenable);
            Functions.Write.writebinary(3, GTA.Offsets.Editor.menubs5, cbmissionbeastenable);
        }

        private void cbmissionbeastjump_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(14, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastjump);
        }

        private void cbmissionbeastinv_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastinv);
        }

        private void cbmissionbeastcrit_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastcrit);
        }

        private void cbmissionbeastrunspd_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionbeastrunspd);
        }

        private void tbmissionbeasthealth_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.bmmxh + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).SetInt(tbmissionbeasthealth.Text);
        }

        private void tbmissionbeastjmpdmg_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.bmsjd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).SetInt(tbmissionbeastjmpdmg.Text);
        }

        private void tbmissionbeastrecharge_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.bmhrgn + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).SetFloat(tbmissionbeastrecharge.Text);
        }

        private void tbmissionbeastinvdmg_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.bmstd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).SetInt(tbmissionbeastinvdmg.Text);
        }

        private void tbmissionbeastmeeledmg_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.bmmdm + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).SetInt(tbmissionbeastmeeledmg.Text);
        }

        private void tbmissionbeastrunspd_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.bmspm + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.beast_next)).SetFloat(tbmissionbeastrunspd.Text);
        }

        private void tbmissioncsttncustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && (((Teamname)ddmissioncsttn.SelectedItem).Name).Equals("Custom"))
            {
                new Global(GTA.Offsets.Editor.tenms + ddmissionteamno.SelectedIndex * 1).SetInt(38); //enables csttn team names
                new Global(GTA.Offsets.Editor.csttn + ddmissionteamno.SelectedIndex * 4).SetString(tbmissioncsttncustom.Text);
            }
        }

        private void ddmissioncsttn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && !(((Teamname)ddmissioncsttn.SelectedItem).Name).Equals("Custom"))
            {
                new Global(GTA.Offsets.Editor.tenms + ddmissionteamno.SelectedIndex * 1).SetInt(38); //enables csttn team names
                new Global(GTA.Offsets.Editor.csttn + ddmissionteamno.SelectedIndex * 4).SetString(((Teamname)ddmissioncsttn.SelectedItem).Value);
            }
        }

        private void ddmissionwantedlevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                switch (ddmissionwantedlevel.SelectedIndex)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        new Global(GTA.Offsets.Editor.wchg + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(ddmissionwantedlevel.SelectedIndex + 1);
                        new Global(GTA.Offsets.Editor.fkwl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(0);
                        break;
                    case 6:
                        new Global(GTA.Offsets.Editor.fkwl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(6);
                        break;
                    default:
                        break;
                }
            }
        }

        private void cbmissionhidewantedlevel_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.irbs4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhidewantedlevel);
        }

        private void tbmissionminspd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionminspd.Text))
                new Global(GTA.Offsets.Editor.minspd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionminspd.Text);
        }

        private void tbmissionmspdlp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmspdlp.Text))
                new Global(GTA.Offsets.Editor.mspdlp + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionmspdlp.Text);
        }

        private void tbmissionmspdsv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmspdsv.Text))
                new Global(GTA.Offsets.Editor.mspdsv + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionmspdsv.Text);
        }

        private void tbmissionfsdtmr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionfsdtmr.Text))
                new Global(GTA.Offsets.Editor.fsdtmr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionfsdtmr.Text);
        }

        private void tbmissionmspdmx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmspdmx.Text))
                new Global(GTA.Offsets.Editor.mspdmx + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionmspdmx.Text);
        }

        private void cbmissionnv_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(25, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionnv);
        }

        private void tbmissiontmbts_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontmbts.Text))
                new Global(GTA.Offsets.Editor.tmbts + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontmbts.Text);
        }

        private void tbmissiontmbt2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontmbt2.Text))
                new Global(GTA.Offsets.Editor.tmbt2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontmbt2.Text);
        }

        private void tbmissiontmbt3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontmbt3.Text))
                new Global(GTA.Offsets.Editor.tmbt3 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontmbt3.Text);
        }

        private void tbmissiontmbt4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontmbt4.Text))
                new Global(GTA.Offsets.Editor.tmbt4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontmbt4.Text);
        }

        private void cbmissionhvalways_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(18, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhvalways);
        }

        private void cbmissionhv_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.irbs5 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhv);
        }

        private void tbmissionhvend_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.itved + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionhvend.Text);
        }

        private void tbmissionhvstart_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.itvsd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionhvstart.Text);
        }

        private void cbmissionlosehealth_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.irbs6 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionlosehealth);
        }

        private void cbmissionlosehealthoopa_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(32, GTA.Offsets.Editor.irbs9 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionlosehealthoopa);
        }

        private void tbmissionrloft_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionrloft.Text))
            {
                new Global(GTA.Offsets.Editor.rloft + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionrloft.Text);
                try
                {

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"https://xenvious.com/resources/outfits/" + tbmissionrloft.Text + ".jpg", UriKind.Absolute);
                    bitmap.EndInit();

                    imgmissionrloft.Source = bitmap;
                }
                catch (Exception)
                {

                }
            }
        }

        private void tbmissionrloftv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionrloftv.Text))
            {
                new Global(GTA.Offsets.Editor.rloftv + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionrloftv.Text);
            }
        }

        private void cbmissionhidetime_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(22, GTA.Offsets.Editor.irbs8 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhidetime);
        }

        private void tbmissionmnumpt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmnumpt.Text))
                new Global(GTA.Offsets.Editor.mnumpt + ddmissionteamno.SelectedIndex).SetInt(tbmissionmnumpt.Text);
        }

        private void tbmissionnumpt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionnumpt.Text))
                new Global(GTA.Offsets.Editor.numpt + ddmissionteamno.SelectedIndex).SetInt(tbmissionnumpt.Text);
        }

        private void tbmissionboud_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionboud.Text))
                new Global(GTA.Offsets.Editor.boud + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionboud.Text);
        }

        private void cbmissionsinv_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.irbs10 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionsinv);
        }

        private void cbmissiondob_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.irbs10 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissiondob);
        }

        private void tbmissionnrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionnrl.Text))
                new Global(GTA.Offsets.Editor.nrl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionnrl.Text);
        }

        private void cbmissionflare_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(18, GTA.Offsets.Editor.irbs14 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionflare);
        }

        private void cbmissionhidemap_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(32, GTA.Offsets.Editor.irbs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionhidemap);
        }

        private void cbmissionspawninaction_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(25, GTA.Offsets.Editor.irbs2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex, cbmissionspawninaction);
        }

        private void cb_nrl_freeze_Checked(object sender, RoutedEventArgs e)
        {
            bool ischecked = cb_nrl_freeze.IsChecked ?? true;
            tbmissionnrl.IsEnabled = !ischecked;
            cb_nrl_freeze_all.IsEnabled = !ischecked;
            freeze = ischecked;
            if (freeze)
            {
                freezeNRLThread = new Thread(new ParameterizedThreadStart(freezeNRL));
                freezeNRLThread.Priority = ThreadPriority.Highest;
                freezeNRLThread.IsBackground = true;
                freezeNRLThread.Start(ddmissionteamno.SelectedIndex);
            }
            else
            {
                freezeNRLThread.Abort();
            }
        }

        private void cb_nrl_freeze_all_Checked(object sender, RoutedEventArgs e)
        {
            bool ischecked = cb_nrl_freeze_all.IsChecked ?? true;
            tbmissionnrl.IsEnabled = !ischecked;
            cb_nrl_freeze.IsEnabled = !ischecked;
            freeze = ischecked;
            if (freeze)
            {
                freezeNRLAllThread = new Thread(new ThreadStart(freezeNRLAll));
                freezeNRLAllThread.Priority = ThreadPriority.Highest;
                freezeNRLAllThread.IsBackground = true;
                freezeNRLAllThread.Start();
            }
            else
            {
                freezeNRLAllThread.Abort();
            }
        }

        public static void freezeNRL(object index)
        {
            int nrl = new Global(GTA.Offsets.Editor.nrl + (int)index * GTA.Offsets.Editor.team_NEXT).Get<int>();
            while (true)
            {
                new Global(GTA.Offsets.Editor.nrl + (int)index * GTA.Offsets.Editor.team_NEXT).SetInt(nrl);
            }
        }

        public static void freezeNRLAll()
        {
            int[] nrlall = new int[]
            {
                new Global(GTA.Offsets.Editor.nrl + 0 * GTA.Offsets.Editor.team_NEXT).Get<int>(),
                new Global(GTA.Offsets.Editor.nrl + 1 * GTA.Offsets.Editor.team_NEXT).Get<int>(),
                new Global(GTA.Offsets.Editor.nrl + 2 * GTA.Offsets.Editor.team_NEXT).Get<int>(),
                new Global(GTA.Offsets.Editor.nrl + 3 * GTA.Offsets.Editor.team_NEXT).Get<int>()
            };
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.nrl + i * GTA.Offsets.Editor.team_NEXT).SetInt(nrlall[i]);
                }
            }

        }

        private void ddmissionnrlno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckTeamSettingSection(true);

            SelectActiveTextBox();
        }

        private void tbmissionclrovr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionclrovr.Text))
            {
                new Global(GTA.Offsets.Editor.clrovr + ddmissionteamno.SelectedIndex).SetInt(tbmissionclrovr.Text);
            }
        }


        private void cbplylfreeze_Checked(object sender, RoutedEventArgs e)
        {
            if (plylfreeze == null)
            {
                if (m.IsProcOpen)
                {
                    plylfreeze = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        plylfreeze[i] = new Global(GTA.Offsets.Editor.plyl + i * GTA.Offsets.Editor.team_NEXT).Get<int>();
                    }
                }
                else
                    return;
            }
            bool ischecked = cbplylfreeze.IsChecked ?? true;
            if (ischecked)
            {
                freezePLYLThread = new Thread(new ThreadStart(freezePLYLAll));
                freezePLYLThread.Priority = ThreadPriority.Highest;
                freezePLYLThread.IsBackground = true;
                freezePLYLThread.Start();
            }
            else
            {
                freezePLYLThread.Abort();
            }
        }

        public static void freezePLYLAll()
        {
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global(GTA.Offsets.Editor.plyl + i * GTA.Offsets.Editor.team_NEXT).SetInt(plylfreeze[i]);
                }
            }

        }

        private void cbmissionhft1_Checked(object sender, RoutedEventArgs e)
        {
            if (ddmissionteamno.SelectedIndex == 0)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
            else if (ddmissionteamno.SelectedIndex == 1)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
            else if (ddmissionteamno.SelectedIndex == 2)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
            else if (ddmissionteamno.SelectedIndex == 3)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 0 * GTA.Offsets.Editor.team_NEXT, cbmissionhft1);
        }

        private void cbmissionhft2_Checked(object sender, RoutedEventArgs e)
        {

            if (ddmissionteamno.SelectedIndex == 0)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
            else if (ddmissionteamno.SelectedIndex == 1)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
            else if (ddmissionteamno.SelectedIndex == 2)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
            else if (ddmissionteamno.SelectedIndex == 3)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 1 * GTA.Offsets.Editor.team_NEXT, cbmissionhft2);
        }

        private void cbmissionhft3_Checked(object sender, RoutedEventArgs e)
        {

            if (ddmissionteamno.SelectedIndex == 0)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
            else if (ddmissionteamno.SelectedIndex == 1)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
            else if (ddmissionteamno.SelectedIndex == 2)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
            else if (ddmissionteamno.SelectedIndex == 3)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 2 * GTA.Offsets.Editor.team_NEXT, cbmissionhft3);
        }

        private void cbmissionhft4_Checked(object sender, RoutedEventArgs e)
        {

            if (ddmissionteamno.SelectedIndex == 0)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv1 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
            else if (ddmissionteamno.SelectedIndex == 1)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv2 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
            else if (ddmissionteamno.SelectedIndex == 2)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv3 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
            else if (ddmissionteamno.SelectedIndex == 3)
                Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.tblpv4 + 3 * GTA.Offsets.Editor.team_NEXT, cbmissionhft4);
        }

        private void tbmissiontblpv1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontblpv1.Text, false))
                new Global(GTA.Offsets.Editor.tblpv1 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontblpv1.Text);
        }

        private void tbmissiontblpv2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontblpv2.Text, false))
                new Global(GTA.Offsets.Editor.tblpv2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontblpv2.Text);
        }

        private void tbmissiontblpv3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontblpv3.Text, false))
                new Global(GTA.Offsets.Editor.tblpv3 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontblpv3.Text);
        }

        private void tbmissiontblpv4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontblpv4.Text, false))
                new Global(GTA.Offsets.Editor.tblpv4 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiontblpv4.Text);
        }

        private void tbmissionfail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionfail.Text))
                new Global(GTA.Offsets.Editor.fail + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionfail.Text);
        }

        private void tbmissionpcbd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionpcbd.Text))
                new Global(GTA.Offsets.Editor.pcbd + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionpcbd.Text);
        }

        private void tbmissionvehrsp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionvehrsp.Text))
                new Global(GTA.Offsets.Editor.vehrsp + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionvehrsp.Text);
        }

        private void tbmissionfail_txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.fail_txt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).SetString(tbmissionfail_txt.Text);
        }

        private void tbmissionabits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionabits.Text))
                new Global(GTA.Offsets.Editor.abits + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionabits.Text);
        }

        private void tbmissionsdobs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionsdobs.Text))
                new Global(GTA.Offsets.Editor.sdobs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionsdobs.Text);
        }

        private void tbmissionedobs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionedobs.Text))
                new Global(GTA.Offsets.Editor.edobs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionedobs.Text);
        }

        private void tbmissiondogps_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiondogps.Text))
                new Global(GTA.Offsets.Editor.dogps + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiondogps.Text);
        }

        private void ddmissionrspweapons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissionrspweapons.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.tvmt + ddmissionteamno.SelectedIndex).SetInt(ddmissionrspweapons.SelectedIndex - 1);
        }

        private void ddmissionrsparmor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissionrsparmor.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.tvma + ddmissionteamno.SelectedIndex).SetInt(ddmissionrsparmor.SelectedIndex - 1);
        }

        private void ddmissionrspbombs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissionrspbombs.SelectedIndex > -1)
            {
                Functions.Write.writebinary(28, GTA.Offsets.Editor.tmbt2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, true);
                new Global(GTA.Offsets.Editor.tvBomb + ddmissionteamno.SelectedIndex).SetInt(ddmissionrspbombs.SelectedIndex - 1);
            }
            else
            {
                Functions.Write.writebinary(28, GTA.Offsets.Editor.tmbt2 + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, false);
            }
        }

        private void ddmissionrspacm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissionrspacm.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.tvmac + ddmissionteamno.SelectedIndex).SetInt(ddmissionrspacm.SelectedIndex - 1);
        }

        private void tbmissionrsptvmt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrsptvmt.Text))
                new Global(GTA.Offsets.Editor.tvmt + ddmissionteamno.SelectedIndex).SetInt(tbmissionrsptvmt.Text);
        }

        private void tbmissionrsptvma_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrsptvma.Text))
                new Global(GTA.Offsets.Editor.tvma + ddmissionteamno.SelectedIndex).SetInt(tbmissionrsptvma.Text);
        }

        private void tbmissionrsptvmac_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrsptvmac.Text))
                new Global(GTA.Offsets.Editor.tvmac + ddmissionteamno.SelectedIndex).SetInt(tbmissionrsptvmac.Text);
        }

        private void tbmissionrsptvbomb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrsptvbomb.Text))
                new Global(GTA.Offsets.Editor.tvBomb + ddmissionteamno.SelectedIndex).SetInt(tbmissionrsptvbomb.Text);
        }

        private void tbmissionrsptvpm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrsptvpm.Text))
                new Global(GTA.Offsets.Editor.tvpm + ddmissionteamno.SelectedIndex).SetInt(tbmissionrsptvpm.Text);
        }

        private void tbmissionrsptvnc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbmissionrsptvnc.Text))
                new Global(GTA.Offsets.Editor.tvnc + ddmissionteamno.SelectedIndex).SetInt(tbmissionrsptvnc.Text);
        }

        private void tbmissioninpts_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissioninpts.Text))
                new Global(GTA.Offsets.Editor.inpts + ddmissionteamno.SelectedIndex).SetInt(tbmissioninpts.Text);
        }

        private void tbmissionmts_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmts.Text))
                new Global(GTA.Offsets.Editor.mts + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionmts.Text);
        }

        private void tbmissionmcvbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmcvbs.Text))
                new Global(GTA.Offsets.Editor.mcvbs + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionmcvbs.Text);
        }

        private void tbmissionmcry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmcry.Text))
                new Global(GTA.Offsets.Editor.mcry + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionmcry.Text);
        }

        private void tbmissionmcstr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmcstr.Text))
                new Global(GTA.Offsets.Editor.mcstr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionmcstr.Text);
        }

        private void tbmissionmcmp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmcmp.Text))
                new Global(GTA.Offsets.Editor.mcmp + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionmcmp.Text);
        }

        private void tbmissionmcsrm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmcsrm.Text))
                new Global(GTA.Offsets.Editor.mcsrm + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionmcsrm.Text);
        }

        private void tbmissiontmtsr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontmtsr.Text))
                new Global(GTA.Offsets.Editor.tmtsr + ddmissionteamno.SelectedIndex).SetInt(tbmissiontmtsr.Text);
        }

        private void tbmissiontmrph_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontmrph.Text))
                new Global(GTA.Offsets.Editor.tmrph + ddmissionteamno.SelectedIndex).SetInt(tbmissiontmrph.Text);
        }

        private void tbmissionspar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionspar.Text))
                new Global(GTA.Offsets.Editor.spar + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionspar.Text);
        }

        private void tbmissiontmt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontmt.Text))
                new Global(GTA.Offsets.Editor.tmt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissiontmt.Text);
        }

        private void tbmissiontms_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiontms.Text))
                new Global(GTA.Offsets.Editor.tms + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissiontms.Text);
        }

        private void tbmissionvss_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.vss + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).SetString(tbmissionvss.Text);
        }

        private void tbmissionarmor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionarmor.Text))
            {
                new Global(GTA.Offsets.Editor.armr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionarmor.Text);
            }
        }

        private void cbmissionvehrsp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.vehrsp, cbmissionvehrsp);
        }

        private void cbmissionb2pa_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(ddmissionnrlno.SelectedIndex + 1, GTA.Offsets.Editor.boud, cbmissionb2pa);
        }

        private void tbmissionshdtxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionshdtxt.Text))
                new Global(GTA.Offsets.Editor.shdtxt + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionshdtxt.Text);
        }

        private void cbmissionbeastenablembs_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Read.checkbinary(27, GTA.Offsets.Editor.menubs5, cbmissionbeastenablembs);
            Functions.Read.checkbinary(24, GTA.Offsets.Editor.menubs5, cbmissionbeastenablembs);
            Functions.Read.checkbinary(20, GTA.Offsets.Editor.menubs5, cbmissionbeastenablembs);
            Functions.Read.checkbinary(3, GTA.Offsets.Editor.menubs5, cbmissionbeastenablembs);
        }

        private void tbmissionmrtl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmrtl.Text))
                new Global(GTA.Offsets.Editor.mrtl + ddmissionnrlno.SelectedIndex + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionmrtl.Text);
        }

        private void tbmissionppk_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionppk.Text))
                new Global(GTA.Offsets.Editor.ppk + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionppk.Text);
        }

        private void tbmissionmslr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionmslr.Text))
                new Global(GTA.Offsets.Editor.mslr + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissionmslr.Text);
        }

        private void ddmissionplyl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int index = ddmissionplyl.SelectedIndex;
            plylfreeze[ddmissionteamno.SelectedIndex] = index;
            new Global((GTA.Offsets.Editor.plyl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex)).SetInt(index);
            if (index == 16 || index == 17)
            {
                tbmissionplvrl.Visibility = Visibility.Visible;
            }
            else
            {
                tbmissionplvrl.Visibility = Visibility.Collapsed;
            }
        }

        private void tbmissionplvrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissionplvrl.Text))
                new Global(GTA.Offsets.Editor.plvrl + ddmissionteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddmissionnrlno.SelectedIndex).SetInt(tbmissionplvrl.Text);
        }
    }
}
