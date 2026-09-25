using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / Inventory page.
    public partial class MainWindow
    {
        private void tbMissioninv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissioninv.Text))
                new Global(GTA.Offsets.Editor.inv + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbMissioninv.Text);
        }

        private void tbMissioninv2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissioninv2.Text))
                new Global(GTA.Offsets.Editor.inv2 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbMissioninv2.Text);
        }

        private void tbMissioninv3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissioninv3.Text))
                new Global(GTA.Offsets.Editor.inv3 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbMissioninv3.Text);
        }

        private void tbMissioninv4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissioninv4.Text))
                new Global(GTA.Offsets.Editor.inv4 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbMissioninv4.Text);
        }

        private void ddinvteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                CheckMissionInventory(true);
                SelectActiveTextBox();
                ddinvua.SelectedIndex = -1;
            }
        }


        public void CheckMissionInventory(bool ignore_focus = false)
        {
            if (ddinvteam == null)
                return;

            int missioninvteamindex = ddinvteam.SelectedIndex;

            if (!tbMissioninv.IsFocused || ignore_focus) tbMissioninv.Text = new Global(GTA.Offsets.Editor.inv + missioninvteamindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
            if (!tbMissioninv2.IsFocused || ignore_focus) tbMissioninv2.Text = new Global(GTA.Offsets.Editor.inv2 + missioninvteamindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
            if (!tbMissioninv3.IsFocused || ignore_focus) tbMissioninv3.Text = new Global(GTA.Offsets.Editor.inv3 + missioninvteamindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
            if (!tbMissioninv4.IsFocused || ignore_focus) tbMissioninv4.Text = new Global(GTA.Offsets.Editor.inv4 + missioninvteamindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();

            WeaponInventoryControl.Team teamindex = WeaponInventoryControl.Team._1;

            switch (missioninvteamindex)
            {
                case 0:
                    teamindex = WeaponInventoryControl.Team._1;
                    break;
                case 1:
                    teamindex = WeaponInventoryControl.Team._2;
                    break;
                case 2:
                    teamindex = WeaponInventoryControl.Team._3;
                    break;
                case 3:
                    teamindex = WeaponInventoryControl.Team._4;
                    break;
                default:
                    break;
            }

            InventoryPistols.ChangeTeam = teamindex;
            InventoryMeele.ChangeTeam = teamindex;
            InventoryMG.ChangeTeam = teamindex;
            InventorySG.ChangeTeam = teamindex;
            InventorySpecial.ChangeTeam = teamindex;
            InventoryRifle.ChangeTeam = teamindex;
            InventorySniper.ChangeTeam = teamindex;
            InventoryExplosive.ChangeTeam = teamindex;

            //check if spawn weapon is set to unarmed
            ddinvsw.IsEnabled = Functions.Read.checkbinary(28, GTA.Offsets.Editor.menubs2) || Functions.Read.checkbinary(21, GTA.Offsets.Editor.menubs25) ? false : true;

            if (!ddinvgsw.IsFocused || ignore_focus)
            {
                int weaponhash = new Global(GTA.Offsets.Editor.Weapon.pal).Get<int>();
                bool isinarray = ((List<GTA.Weapon>)ddinvgsw.ItemsSource).Any(x => x.Int32 == weaponhash);
                if (isinarray)
                    ddinvgsw.SelectedItem = ((List<GTA.Weapon>)ddinvgsw.ItemsSource).Where(x => x.Int32 == weaponhash).First();
                else
                {
                    ddinvgsw.SelectedItem = -1;
                    ddinvgsw.Text = "couldnt find weapon";
                }
            }
            if (!ddinvglw.IsFocused || ignore_focus)
            {
                if (Functions.Read.checkbinary(19, GTA.Offsets.Editor.menubs))
                {
                    ddinvglw.SelectedIndex = 2;
                }
                else if (Functions.Read.checkbinary(20, GTA.Offsets.Editor.menubs))
                {
                    ddinvglw.SelectedIndex = 1;
                }
                else
                {
                    ddinvglw.SelectedIndex = 0;
                }
            }

            if (!ddinvsw.IsFocused || ignore_focus)
            {
                GetSWValues();
                if (ddinvsw.Items.Count > 0)
                {
                    ddinvsw.IsEnabled = true;
                    string name = GetStartWeaponNameFromWeaponID(new Global(GTA.Offsets.Editor.invsw + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).Get<int>());
                    bool isany = ((List<string>)ddinvsw.ItemsSource).Any(x => x == name);
                    if (isany)
                    {
                        ddinvsw.SelectedIndex = ((List<string>)ddinvsw.ItemsSource).IndexOf(name);
                    }
                }
                else
                {
                    ddinvsw.IsEnabled = false;
                    ddinvsw.SelectedIndex = -1;
                }
            }
            if (!ddinvua.IsFocused || ignore_focus)
            {
                WeaponInventory selecteditem = null;
                if (ddinvua.Items.Count > 0 && ddinvua.SelectedIndex > -1)
                {
                    selecteditem = (WeaponInventory)ddinvua.SelectedItem;
                }
                GetUAValues();
                if (ddinvua.Items.Count > 0)
                {
                    ddinvua.IsEnabled = true;
                    if (selecteditem != null)
                    {
                        bool isany = ((List<WeaponInventory>)ddinvua.ItemsSource).Any(x => x.Bit == selecteditem.Bit);
                        ddinvua.SelectedItem = selecteditem;
                    }
                    else
                    {
                        ddinvua.SelectedIndex = 0;
                    }
                }
                else
                {
                    ddinvua.IsEnabled = false;
                    ddinvua.SelectedIndex = -1;
                }
            }
        }

        private void ddinvsw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddinvsw.SelectedItem != null)
            {
                new Global(GTA.Offsets.Editor.invsw + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).SetInt(GetStartWeaponIDFromWeaponmName((string)ddinvsw.SelectedItem));
            }
        }

        private void ddinvsw_DropDownOpened(object sender, EventArgs e)
        {
            GetSWValues();
            if (ddinvsw.Items.Count > 0)
            {
                ddinvsw.IsEnabled = true;
                string name = GetStartWeaponNameFromWeaponID(new Global(GTA.Offsets.Editor.invsw + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).Get<int>());
                bool isany = ((List<string>)ddinvsw.ItemsSource).Any(x => x == name);
                if (isany)
                {
                    ddinvsw.SelectedIndex = ((List<string>)ddinvsw.ItemsSource).IndexOf(name);
                }
            }
            else
            {
                ddinvsw.IsEnabled = false;
                ddinvsw.SelectedIndex = -1;
            }
        }

        public void GetSWValues()
        {
            List<string> enabledweapons = new List<string>();
            enabledweapons.Add("Unarmed");
            enabledweapons.AddRange(GetEnabledStartWeapons(InventoryPistols.WeaponList));
            enabledweapons.AddRange(GetEnabledStartWeapons(InventoryExplosive.WeaponList));
            enabledweapons.AddRange(GetEnabledStartWeapons(InventoryMeele.WeaponList));
            enabledweapons.AddRange(GetEnabledStartWeapons(InventoryMG.WeaponList));
            enabledweapons.AddRange(GetEnabledStartWeapons(InventoryRifle.WeaponList));
            enabledweapons.AddRange(GetEnabledStartWeapons(InventorySG.WeaponList));
            enabledweapons.AddRange(GetEnabledStartWeapons(InventorySniper.WeaponList));
            enabledweapons.AddRange(GetEnabledStartWeapons(InventorySpecial.WeaponList));

            ddinvsw.ItemsSource = null;

            ddinvsw.ItemsSource = enabledweapons;
        }

        public List<string> GetEnabledStartWeapons(List<WeaponInventory> list)
        {
            List<string> enabledweapons = new List<string>();
            foreach (var item in list)
            {
                long offset = 0;
                switch (item.InventoryOffset)
                {
                    case InventoryOffset._1:
                        offset = GTA.Offsets.Editor.inv + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    case InventoryOffset._2:
                        offset = GTA.Offsets.Editor.inv2 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    case InventoryOffset._3:
                        offset = GTA.Offsets.Editor.inv3 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    case InventoryOffset._4:
                        offset = GTA.Offsets.Editor.inv4 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    default:
                        break;
                }
                if (Functions.Read.checkbinary(item.Bit, offset))
                {
                    if (GetStartWeaponIDFromWeaponmName(item.Name) != -1)
                    {
                        enabledweapons.Add(item.Name);
                    }
                }
            }
            return enabledweapons;
        }

        public List<WeaponInventory> GetEnabledWeaponsObject(List<WeaponInventory> list)
        {
            List<WeaponInventory> enabledweapons = new List<WeaponInventory>();
            foreach (var item in list)
            {
                long offset = 0;
                switch (item.InventoryOffset)
                {
                    case InventoryOffset._1:
                        offset = GTA.Offsets.Editor.inv + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    case InventoryOffset._2:
                        offset = GTA.Offsets.Editor.inv2 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    case InventoryOffset._3:
                        offset = GTA.Offsets.Editor.inv3 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    case InventoryOffset._4:
                        offset = GTA.Offsets.Editor.inv4 + ddinvteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT;
                        break;
                    default:
                        break;
                }
                if (Functions.Read.checkbinary(item.Bit, offset))
                {
                    enabledweapons.Add(item);
                }
            }
            return enabledweapons;
        }

        public int GetStartWeaponIDFromWeaponmName(string name)
        {
            switch (name)
            {
                case "Unarmed":
                    return 0;

                case "Pistol":
                    return 1;

                case "Combat Pistol":
                    //case "Combat Pistol + Silencer":
                    return 2;

                case "AP-Pistol":
                    //case "AP-Pistol + Extended Clip":
                    return 3;

                case "Micro SMG":
                    //case "Micro SMG + Extended Mag":
                    //case "Micro SMG + Silencer":
                    return 4;

                case "SMG":
                    return 5;

                case "LMG":
                    return 6;

                case "Combat LMG":
                    //case "Combat LMG + Extended Mag":
                    return 7;

                case "Assault Rifle":
                    //case "Assault Rifle + Silencer":
                    //case "Assault Rifle MK II":
                    return 8;

                case "Carbine Rifle":
                    return 9;

                case "Advanced Rifle":
                    return 10;

                case "Sniper Rifle":
                    return 11;

                case "Heavy Sniper":
                    //case "Heavy Sniper MK II":
                    return 12;

                case "Pump Shotgun":
                    return 13;

                case "Sawed-Off Shotgun":
                    return 14;

                case "Assault Shotgun":
                    //case "Assault Shotgun + Silencer":
                    return 15;

                case "Grenade Launcher":
                    return 16;

                case "RPG":
                    return 17;

                case "Minigun":
                    return 18;

                case "Grenade":
                    return 19;

                case "Sticky Bomb":
                    return 20;

                case "Jerry Can":
                    return 21;

                case "Knife":
                    return 22;

                case "Baseball Bat":
                    return 23;

                case "Special Carbine":
                    //case "Special Carbine + Silencer":
                    return 24;

                case "Flare Gun":
                    return 33;

                case "Crowbar":
                    return 35;

                case "Pistol-50":
                    return 36;

                case "Nightstick":
                    return 39;

                case "Bullpop Rifle MK II":
                    return 40;

                case "Heavy Shotgun":
                    return 42;

                case "Homing Launcher":
                    return 45;

                case "Flashlight":
                    return 46;

                case "Machete":
                    return 47;

                case "Machine Pistol":
                    // "Machine Pistol + Extended Mag":
                    // "Machine Pistol + Silencer":
                    return 48;

                case "Marksman Pistol":
                    return 49;

                case "Hatchet":
                    return 50;

                case "Assault SMG":
                    //case "Assault SMG + Silencer":
                    return 51;

                case "Railgun":
                    return 52;

                case "Gusenberg Sweeper":
                    return 53;

                case "Heavy Pistol":
                    //case "Heavy Pistol + Silencer":
                    //case "Heavy Pistol + Silencer + Flash":
                    return 54;

                case "Double Barrel Shotgun":
                    return 55;

                case "Compact Rifle":
                    //case "Compact Rifle + Extended Mag":
                    return 56;

                case "Automatic Shotgun":
                    return 57;

                case "Mini SMG":
                    return 58;

                case "Compact Grenade Launcher":
                    return 59;

                case "Battle Axe":
                    return 60;

                case "Pipe Bomb":
                    return 61;

                case "Pool Cue":
                    return 62;

                case "Pipe Wrench":
                    return 63;

                case "Hammer":
                    return 64;

                case "Golf Club":
                    return 65;

                case "Bottle":
                    return 66;

                case "Antique Cavalry Dagger":
                    return 67;

                case "Knuckle Duster":
                    return 68;

                case "Switchbalde":
                    return 69;
                default:
                    return -1;
            }
        }


        public string GetStartWeaponNameFromWeaponID(int id)
        {
            switch (id)
            {
                case 0:
                    return "Unarmed";

                case 1:
                    return "Pistol";

                case 2:
                    return "Combat Pistol";

                case 3:
                    return "AP-Pistol";

                case 4:
                    return "Micro SMG";

                case 5:
                    return "SMG";

                case 6:
                    return "LMG";

                case 7:
                    return "Combat LMG";

                case 8:
                    return "Assault Rifle";

                case 9:
                    return "Carbine Rifle";

                case 10:
                    return "Advanced Rifle";

                case 11:
                    return "Sniper Rifle";

                case 12:
                    return "Heavy Sniper";

                case 13:
                    return "Pump Shotgun";

                case 14:
                    return "Sawed-Off Shotgun";

                case 15:
                    return "Assault Shotgun";

                case 16:
                    return "Grenade Launcher";

                case 17:
                    return "RPG";

                case 18:
                    return "Minigun";

                case 19:
                    return "Grenade";

                case 20:
                    return "Sticky Bomb";

                case 21:
                    return "Jerry Can";

                case 22:
                    return "Knife";

                case 23:
                    return "Baseball Bat";

                case 24:
                    return "Special Carbine";

                case 33:
                    return "Flare Gun";

                case 35:
                    return "Crowbar";

                case 36:
                    return "Pistol-50";

                case 39:
                    return "Nightstick";

                case 40:
                    return "Bullpop Rifle MK II";

                case 42:
                    return "Heavy Shotgun";

                case 45:
                    return "Homing Launcher";

                case 46:
                    return "Flashlight";

                case 47:
                    return "Machete";

                case 48:
                    return "Machine Pistol";

                case 49:
                    return "Marksman Pistol";

                case 50:
                    return "Hatchet";

                case 51:
                    return "Assault SMG";

                case 52:
                    return "Railgun";

                case 53:
                    return "Gusenberg Sweeper";

                case 54:
                    return "Heavy Pistol";

                case 55:
                    return "Double Barrel Shotgun";

                case 56:
                    return "Compact Rifle";

                case 57:
                    return "Automatic Shotgun";

                case 58:
                    return "Mini SMG";

                case 59:
                    return "Compact Grenade Launcher";

                case 60:
                    return "Battle Axe";

                case 61:
                    return "Pipe Bomb";

                case 62:
                    return "Pool Cue";

                case 63:
                    return "Pipe Wrench";

                case 64:
                    return "Hammer";

                case 65:
                    return "Golf Club";

                case 66:
                    return "Bottle";

                case 67:
                    return "Antique Cavalry Dagger";

                case 68:
                    return "Knuckle Duster";

                case 69:
                    return "Switchbalde";
                default:
                    return "Unarmed";
            }
        }

        private void ddinvua_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                if (ddinvua.SelectedIndex == -1)
                {
                    cbmissioninvua.IsEnabled = false;
                    tbMissioninvua.IsEnabled = false;
                    return;
                }

                var tempsia = ((WeaponInventory)ddinvua.SelectedItem).Sia;
                if (tempsia != 99999)
                {
                    cbmissioninvua.IsEnabled = true;
                    tbMissioninvua.IsEnabled = true;
                    tbMissioninvua.Text = new Global(GTA.Offsets.Editor.sia + tempsia + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).Get<int>().ToString();
                    cbmissioninvua.IsChecked = new Global(GTA.Offsets.Editor.sia + tempsia + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).Get<int>() == -3 ? true : false;
                }
                else
                {
                    cbmissioninvua.IsEnabled = false;
                    tbMissioninvua.IsEnabled = false;
                }

            }
        }

        private void ddinvua_DropDownOpened(object sender, EventArgs e)
        {
            WeaponInventory selecteditem = null;
            if (ddinvua.Items.Count > 0 && ddinvua.SelectedIndex > -1)
            {
                selecteditem = (WeaponInventory)ddinvua.SelectedItem;
            }
            GetUAValues();
            if (ddinvua.Items.Count > 0)
            {
                if (selecteditem != null)
                {
                    bool isany = ((List<WeaponInventory>)ddinvua.ItemsSource).Any(x => x.Bit == selecteditem.Bit);
                    ddinvua.SelectedItem = selecteditem;
                }
            }
            else
            {
                ddinvua.IsEnabled = false;
                ddinvua.SelectedIndex = -1;
            }
        }

        public void GetUAValues()
        {
            List<WeaponInventory> enabledweapons = new List<WeaponInventory>();
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventoryPistols.WeaponList));
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventoryExplosive.WeaponList));
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventoryMeele.WeaponList));
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventoryMG.WeaponList));
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventoryRifle.WeaponList));
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventorySG.WeaponList));
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventorySniper.WeaponList));
            enabledweapons.AddRange(GetEnabledWeaponsObject(InventorySpecial.WeaponList));

            ddinvua.ItemsSource = null;

            ddinvua.ItemsSource = enabledweapons;
        }

        private void cbmissioninvua_Checked(object sender, RoutedEventArgs e)
        {
            var tempsia = ((WeaponInventory)ddinvua.SelectedItem).Sia;
            new Global(GTA.Offsets.Editor.sia + tempsia + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).SetInt(cbmissioninvua.IsChecked ?? true ? -3 : 0);
            tbMissioninvua.Text = new Global(GTA.Offsets.Editor.sia + tempsia + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).Get<int>().ToString();
        }

        private void tbMissioninvua_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbMissioninvua.Text))
            {
                var tempsia = ((WeaponInventory)ddinvua.SelectedItem).Sia;
                new Global(GTA.Offsets.Editor.sia + tempsia + GTA.Offsets.Editor.team_NEXT * ddinvteam.SelectedIndex).SetInt(tbMissioninvua.Text);
            }
        }

        private void ddinvgsw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Weapon.pal).SetInt(((GTA.Weapon)ddinvgsw.SelectedItem).Int32);
        }
        private void ddinvglw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ddinvglw.SelectedIndex;
            if (index == 1)
            {
                Functions.Write.writebinary(20, GTA.Offsets.Editor.menubs, true);
                Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs, false);
            }
            else if (index == 2)
            {
                Functions.Write.writebinary(20, GTA.Offsets.Editor.menubs, false);
                Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs, true);
            }
            else
            {
                Functions.Write.writebinary(20, GTA.Offsets.Editor.menubs, false);
                Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs, false);
            }
        }
    }
}
