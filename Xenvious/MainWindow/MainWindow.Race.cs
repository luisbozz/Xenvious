using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Race page.
    public partial class MainWindow
    {
        private void BtnRaceGeneral_Click(object sender, RoutedEventArgs e)
        {
            PageInnerRace.SelectedItem = PageInnerRaceGeneral;
        }

        private void BtnRaceCheckpoints_Click(object sender, RoutedEventArgs e)
        {
            PageInnerRace.SelectedItem = PageInnerRaceCP;
        }

        private void BtnRaceArena_Click(object sender, RoutedEventArgs e)
        {
            PageInnerRace.SelectedItem = PageInnerRaceArena;
        }

        private void BtnRaceAVEH_Click(object sender, RoutedEventArgs e)
        {
            PageInnerRace.SelectedItem = PageInnerRaceAVEH;

            avehCompactList.DataContext = GTA.Editor.Compacts;
            avehSedanList.DataContext = GTA.Editor.Sedans;
            avehSUVList.DataContext = GTA.Editor.SUVs;
            avehArena_ContenderList.DataContext = GTA.Editor.Arena_Contender;
            avehCOUPEList.DataContext = GTA.Editor.Coupes;
            avehCyclesList.DataContext = GTA.Editor.Cycles;
            avehIndustrialList.DataContext = GTA.Editor.Industrial;
            avehMotorcyclesList.DataContext = GTA.Editor.Motorcycles;
            avehMuscleList.DataContext = GTA.Editor.Muscle;
            avehOff_RoadList.DataContext = GTA.Editor.Off_Road;
            avehSpecialList.DataContext = GTA.Editor.Special;
            avehSportsList.DataContext = GTA.Editor.Sports;
            avehSports_ClassicsList.DataContext = GTA.Editor.Sports_Classics;
            avehSuperList.DataContext = GTA.Editor.Super;
            avehUtilityList.DataContext = GTA.Editor.Utility;
            avehVansList.DataContext = GTA.Editor.Vans;
            avehWeaponizedList.DataContext = GTA.Editor.Weaponized;
            avehOpenWheelList.DataContext = GTA.Editor.Open_Wheel;
            avehGoKartList.DataContext = GTA.Editor.Go_Kart;
            avehTunerList.DataContext = GTA.Editor.Tuner;
        }

        private void ddracetype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Race.Checkpoints.gtar).SetInt(ddracetype.SelectedIndex);
        }

        private void ddroutetype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.Race.Checkpoints.ptp).SetInt(ddroutetype.SelectedIndex);

                switch (ddroutetype.SelectedIndex)
                {
                    case 0:
                        if (IsLandRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(0);
                        }
                        else if (IsWaterRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(2);
                        }
                        else if (IsAirRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(4);
                        }
                        else if (IsSpecialVehRace() || IsArenaRace() || IsStuntRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(6);
                        }
                        else if (IsTransformRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(6);
                        }
                        else if (IsParachuteRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(8);
                        }
                        else if (IsTargetRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(18);
                        }
                        else if (IsOpenWheelRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(24);
                        }
                        else if (IsPursuitRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(26);
                        }
                        else if (IsStreetRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(27);
                        }
                        break;
                    case 1:
                        if (IsLandRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(1);
                        }
                        else if (IsWaterRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(3);
                        }
                        else if (IsAirRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(5);
                        }
                        else if (IsSpecialVehRace() || IsArenaRace() || IsStuntRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(7);
                        }
                        else if (IsTransformRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(7);
                        }
                        else if (IsParachuteRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(9);
                        }
                        else if (IsTargetRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(19);
                        }
                        else if (IsOpenWheelRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(25);
                        }
                        else if (IsPursuitRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(26);
                        }
                        else if (IsStreetRace())
                        {
                            new Global(GTA.Offsets.Editor.racetype).SetInt(27);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void tbraceblimptext_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.blmpmsg).SetString(tbraceblimptext.Text);
        }

        private void ddcpssg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbcpssglocx.Text = new Global(GTA.Offsets.Editor.Vehicle.loc + 0 + ddcpssg.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).Get<float>().ToString();
            tbcpssglocy.Text = new Global(GTA.Offsets.Editor.Vehicle.loc + 1 + ddcpssg.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).Get<float>().ToString();
            tbcpssglocz.Text = new Global(GTA.Offsets.Editor.Vehicle.loc + 2 + ddcpssg.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).Get<float>().ToString();

            SelectActiveTextBox();
        }

        private void Btncpssggetloc_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    for (int i = 0; i < ddcpssg.Items.Count; i++)
                    {
                        new Global(GTA.Offsets.Editor.Vehicle.loc + 0 + i * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(loc[0]);
                        new Global(GTA.Offsets.Editor.Vehicle.loc + 1 + i * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(loc[1]);
                        new Global(GTA.Offsets.Editor.Vehicle.loc + 2 + i * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(loc[2]);
                    }
                    return;
                }

                tbcpssglocx.Text = loc[0];
                tbcpssglocy.Text = loc[1];
                tbcpssglocz.Text = loc[2];
            }
        }

        private void tbcpssglocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                for (int i = 0; i < ddcpssg.Items.Count; i++)
                {
                    new Global(GTA.Offsets.Editor.Vehicle.loc + i * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssglocx.Text);
                }
            }
            new Global(GTA.Offsets.Editor.Vehicle.loc + ddcpssg.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssglocx.Text);
        }

        private void tbcpssglocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                for (int i = 0; i < ddcpssg.Items.Count; i++)
                {
                    new Global(GTA.Offsets.Editor.Vehicle.loc + 1 + i * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssglocy.Text);
                }
                return;
            }
            new Global(GTA.Offsets.Editor.Vehicle.loc + 1 + ddcpssg.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssglocy.Text);
        }

        private void tbcpssglocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                for (int i = 0; i < ddcpssg.Items.Count; i++)
                {
                    new Global(GTA.Offsets.Editor.Vehicle.loc + 2 + i * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssglocz.Text);
                }
                return;
            }
            new Global(GTA.Offsets.Editor.Vehicle.loc + 2 + ddcpssg.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssglocz.Text);
        }

        private void tbcpssghead_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                for (int i = 0; i < ddcpssg.Items.Count; i++)
                {
                    new Global(GTA.Offsets.Editor.Vehicle.head + i * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssghead.Text);
                }
                return;
            }
            new Global(GTA.Offsets.Editor.Vehicle.head + ddcpssg.SelectedIndex * GTA.Offsets.Editor.Vehicle.NEXT).SetFloat(tbcpssghead.Text);
        }

        private void ddtrfmvmno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                try
                {
                    ddtrfmvm.SelectedItem = ddtrfmvm.ItemsSource.Cast<Vehicle>().Where(x => x.Integer == new Global(GTA.Offsets.Editor.Race.Checkpoints.trfmvm + ddtrfmvmno.SelectedIndex).Get<int>()).First();
                }
                catch (Exception)
                {
                    ddtrfmvm.SelectedIndex = 0;
                }
            }
        }

        private void ddtrfmvm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                if (ddtrfmvm.SelectedIndex > -1)
                {
                    if (ddtrfmvmno.SelectedIndex > -1)
                    {
                        new Global(GTA.Offsets.Editor.Race.Checkpoints.trfmvm + ddtrfmvmno.SelectedIndex).SetInt(((Vehicle)ddtrfmvm.SelectedItem).Integer == -1 ? 0 : ((Vehicle)ddtrfmvm.SelectedItem).Integer);
                    }
                }
            }
        }

        private void PageInnerRace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnRaceGeneral.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnRaceCheckpoints.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnRaceAVEH.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];

            if (PageInnerRace.SelectedItem == PageInnerRaceGeneral)
            {
                BtnRaceGeneral.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerRace.SelectedItem == PageInnerRaceCP)
            {
                BtnRaceCheckpoints.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerRace.SelectedItem == PageInnerRaceAVEH)
            {
                BtnRaceAVEH.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
        }

        private void cbraceololockcatchup_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs8, cbraceololockcatchup);
        }

        private void cbraceolohidetod_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(16, GTA.Offsets.Editor.menubs2, cbraceolohidetod);
        }

        private void cbraceolohideag_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.menubs9, cbraceolohideag);
        }

        private void cbraceololocktod_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.menubs17, cbraceololocktod);
        }

        private void cbraceoloosnc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs18, cbraceoloosnc);
        }

        private void cbraceolohidetraffic_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.menubs20, cbraceolohidetraffic);
        }

        private void cbraceolohidedlp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.menubs20, cbraceolohidedlp);
        }

        private void cbraceolohidenol_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(18, GTA.Offsets.Editor.menubs20, cbraceolohidenol);
        }

        private void cbraceolohidewl_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs20, cbraceolohidewl);
        }

        private void cbraceolohiderally_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs21, cbraceolohiderally);
        }

        private void cbraceolomaxwl_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs28, cbraceolomaxwl);
        }

        private void cbraceolohidecstveh_Checked(object sender, RoutedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.rcvs).SetInt(cbraceolohidecstveh.IsChecked ?? true ? 2 : 0);
        }

        private void cbraceolohideweth_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs22, cbraceolohideweth);
        }
    }
}
