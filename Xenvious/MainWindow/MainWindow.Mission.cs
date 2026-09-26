using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static Xenvious.GTA;

namespace Xenvious
{
    // Part of MainWindow: Mission page.
    public partial class MainWindow
    {
        bool Inventory_Initialized = false;
        private void BtnSectionInventory_Click(object sender, RoutedEventArgs e)
        {
            if (!Inventory_Initialized)
            {
                InventoryPistols.WeaponList = Weapons.Pistols;
                InventoryMeele.WeaponList = Weapons.Meele;
                InventoryMG.WeaponList = Weapons.MG;
                InventorySG.WeaponList = Weapons.SG;
                InventorySpecial.WeaponList = Weapons.Special;
                InventoryRifle.WeaponList = Weapons.Rifle;
                InventorySniper.WeaponList = Weapons.Sniper;
                InventoryExplosive.WeaponList = Weapons.Explosive;
                Inventory_Initialized = true;
            }
            PageInnerMission.SelectedItem = PageInnerMissionInventory;
        }

        private void BtnSectionSMS_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionSMS;
        }

        private void BtnSectionGoto_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionGoto;
        }




        private void BtnMissionMenubs_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionMenubs;
        }

        private void BtnMissionPlayerSettings_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionPlayerSettings;

            if (m.IsProcOpen && ddplyrno.SelectedIndex == -1)
                ddplyrno.SelectedIndex = 0;
        }

        bool MissionTeamSettingsInitialized = false;
        private void BtnMissionTeamSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!MissionTeamSettingsInitialized)
            {
                ddmissionirbsno.ItemsSource = Enumerable.Range(1, 15);
                MissionTeamSettingsInitialized = true;
            }
            CheckTeamSettingSection(true);
            PageInnerMission.SelectedItem = PageInnerMissionTeamSettings;
            if (ddmissionirbsno.SelectedIndex == -1)
            {
                ddmissionirbsno.SelectedIndex = 0;
            }
        }

        private void BtnMissionPA_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionPA;
        }

        private void BtnMissionGeneral_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionGeneral;
        }

        private void BtnMissionExtras_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionExtras;
        }

        public void InitializeGear()
        {
            if (ddmissiongear != null)
            {
                ddmissiongear.Items.Clear();

                var missionitem = new ComboBoxItem();
                missionitem.Tag = new Gear(0, -1);
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding { Path = new PropertyPath("Translation[none]"), FallbackValue = "None" });
                ddmissiongear.Items.Add(missionitem);

                missionitem = new ComboBoxItem();
                missionitem.Tag = new Gear(4, 2);
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding { Path = new PropertyPath("Translation[missiongeardb]"), FallbackValue = "Duffle Bag" });
                ddmissiongear.Items.Add(missionitem);

                missionitem = new ComboBoxItem();
                missionitem.Tag = new Gear(16, 2);
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding { Path = new PropertyPath("Translation[missiongearrb]"), FallbackValue = "Rebreather" });
                ddmissiongear.Items.Add(missionitem);

                missionitem = new ComboBoxItem();
                missionitem.Tag = new Gear(32, 2);
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding { Path = new PropertyPath("Translation[missiongeargrayep]"), FallbackValue = "Gray Earpiece" });
                ddmissiongear.Items.Add(missionitem);

                missionitem = new ComboBoxItem();
                missionitem.Tag = new Gear(64, 2);
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding { Path = new PropertyPath("Translation[missiongearredep]"), FallbackValue = "Red Earpiece" });
                ddmissiongear.Items.Add(missionitem);

                missionitem = new ComboBoxItem();
                missionitem.Tag = new Gear(128, 2);
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding { Path = new PropertyPath("Translation[missiongearlcdep]"), FallbackValue = "LCD Earpiece" });
                ddmissiongear.Items.Add(missionitem);

                missionitem = new ComboBoxItem();
                missionitem.Tag = new Gear(256, 2);
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding { Path = new PropertyPath("Translation[missiongearheadset]"), FallbackValue = "Headset" });
                ddmissiongear.Items.Add(missionitem);

                GearInitialized = true;
            }
        }

        bool GearInitialized = false;
        private void PageInnerMission_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageInnerMission.SelectedItem == PageInnerMissionTeamSettings)
            {
                if (!GearInitialized)
                {
                    InitializeGear();
                }
            }

            BtnMissionGeneral.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionExtras.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionMenubs.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionInterior.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionPlayerSettings.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionTeamSettings.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionPA.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionRA.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionTPM.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionKill.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionGC.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionotzone.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnMissionBlips.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];

            if (PageInnerMission.SelectedItem == PageInnerMissionGeneral)
            {
                BtnMissionGeneral.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionExtras)
            {
                BtnMissionExtras.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionMenubs)
            {
                BtnMissionMenubs.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionIPL)
            {
                BtnMissionInterior.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionPlayerSettings)
            {
                BtnMissionPlayerSettings.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionTeamSettings)
            {
                BtnMissionTeamSettings.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionPA)
            {
                BtnMissionPA.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionRA)
            {
                BtnMissionRA.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionTeleportMarkers)
            {
                BtnMissionTPM.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionKill)
            {
                BtnMissionKill.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionKill)
            {
                BtnMissionGC.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionotzone)
            {
                BtnMissionotzone.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerMission.SelectedItem == PageInnerMissionotzone)
            {
                BtnMissionBlips.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
        }

        bool IPL_Initialized = false;
        private void BtnMissionInterior_Click(object sender, RoutedEventArgs e)
        {
            if (!IPL_Initialized)
            {
                IPLYachtH.IPLEntry = GTA.Location.YachtH;
                IPLYachtP.IPLEntry = GTA.Location.YachtP;
                IPLCarrier.IPLEntry = GTA.Location.Carrier;
                IPLMorgue.IPLEntry = GTA.Location.Morgue;
                IPLFranklin.IPLEntry = GTA.Location.Franklin;
                IPLMichael.IPLEntry = GTA.Location.Michael;
                IPLRockClub.IPLEntry = GTA.Location.RockClub;
                IPLStripClub.IPLEntry = GTA.Location.StripClub;
                IPLONeil.IPLEntry = GTA.Location.ONeil;
                IPLLiveinvader.IPLEntry = GTA.Location.Liveinvader;
                IPLSlaughter.IPLEntry = GTA.Location.Slaughter;
                IPLvrecycle.IPLEntry = GTA.Location.vrecycle;
                IPLFoundry.IPLEntry = GTA.Location.Foundry;
                IPLServerFarm.IPLEntry = GTA.Location.ServerFarm;
                IPLIAA.IPLEntry = GTA.Location.IAA;
                IPLFIB.IPLEntry = GTA.Location.FIB;
                IPLCaypPerico.IPLEntry = GTA.Location.CayoPerico;

                IPL_Initialized = true;
            }
            PageInnerMission.SelectedItem = PageInnerMissionIPL;
        }

        private void BtnMissionTPM_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionTeleportMarkers;
        }

        private void BtnMissionKill_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionKill;
        }

        private void BtnMissionGC_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionGang;
            if (m.IsProcOpen && ddmissiongangno.SelectedIndex == -1)
            {
                ddmissiongangno.SelectedIndex = 0;
            }
        }

        private void BtnMissionotzone_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionotzone;
            if (m.IsProcOpen && ddmissionotzoneno.SelectedIndex == -1)
            {
                ddmissionotzoneno.SelectedIndex = 0;
            }
        }

        private void BtnMissionRA_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionRA;
        }

        private void BtnMissionBlips_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMission.SelectedItem = PageInnerMissionBlips;
            if (m.IsProcOpen && ddmissionddblipno.SelectedIndex == -1)
            {
                ddmissionddblipno.SelectedIndex = 0;
            }
        }
    }
}
