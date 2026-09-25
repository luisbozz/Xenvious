using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Race / AvailableVehicles page.
    public partial class MainWindow
    {
        //        //ListItemCollection

        //        var collectionAdded = itemsAdded.Cast<string>();
        //        var collectionRemoved = itemsRemoved.Cast<string>();

        //                    switch (temp)
        //                        default:
        //                    switch (temp)
        //                        default:

        private void avehCompactList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                //ListItemCollection
                System.Collections.IList itemsAdded = e.AddedItems;
                System.Collections.IList itemsRemoved = e.RemovedItems;

                var collectionAdded = itemsAdded.Cast<string>();
                var collectionRemoved = itemsRemoved.Cast<string>();

                bool added = collectionAdded.Count() > 0;
                var newlist = added ? collectionAdded : collectionRemoved;
                bool aveh = added ? false : true;
                bool adlc = added ? true : false;

                foreach (var item in newlist)
                {
                    int temp = avehCompactList.Items.IndexOf(item);
                    if (temp != -1)
                    {
                        switch (temp)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                                Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh, aveh);
                                break;
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                Functions.Write.writebinary(temp - 4 + 1, GTA.Offsets.Editor.Race.adlc, adlc);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void cbavehCompact_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehCompact);
        }

        //        //ListItemCollection

        //        var collectionAdded = itemsAdded.Cast<string>();
        //        var collectionRemoved = itemsRemoved.Cast<string>();

        //                    switch (temp)
        //                        default:
        //                    switch (temp)
        //                        default:

        private void avehSedanList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                //ListItemCollection
                System.Collections.IList itemsAdded = e.AddedItems;
                System.Collections.IList itemsRemoved = e.RemovedItems;

                var collectionAdded = itemsAdded.Cast<string>();
                var collectionRemoved = itemsRemoved.Cast<string>();

                bool added = collectionAdded.Count() > 0;
                var newlist = added ? collectionAdded : collectionRemoved;
                bool aveh = added ? false : true;
                bool adlc = added ? true : false;

                foreach (var item in newlist)
                {
                    int temp = avehSedanList.Items.IndexOf(item);
                    if (temp != -1)
                    {
                        switch (temp)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 1, aveh);
                                break;
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                                Functions.Write.writebinary(temp - 11 + 1, GTA.Offsets.Editor.Race.adlc + (1 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void cbavehSedan_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSedan);
        }

        private void cbavehSUV_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSUV);
        }

        private void avehSUVList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                //ListItemCollection
                System.Collections.IList itemsAdded = e.AddedItems;
                System.Collections.IList itemsRemoved = e.RemovedItems;

                var collectionAdded = itemsAdded.Cast<string>();
                var collectionRemoved = itemsRemoved.Cast<string>();

                bool added = collectionAdded.Count() > 0;
                var newlist = added ? collectionAdded : collectionRemoved;
                bool aveh = added ? false : true;
                bool adlc = added ? true : false;

                foreach (var item in newlist)
                {
                    int temp = avehSUVList.Items.IndexOf(item);
                    if (temp != -1)
                    {
                        switch (temp)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 12:
                            case 13:
                            case 14:
                                Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 2, aveh);
                                break;
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                            case 30:
                            case 31:
                            case 32:
                            case 33:
                            case 34:
                                Functions.Write.writebinary(temp - 15 + 1, GTA.Offsets.Editor.Race.adlc + (2 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void cbavehCOUPE_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(4, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehCOUPE);
        }

        private void avehCOUPEList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                //ListItemCollection
                System.Collections.IList itemsAdded = e.AddedItems;
                System.Collections.IList itemsRemoved = e.RemovedItems;

                var collectionAdded = itemsAdded.Cast<string>();
                var collectionRemoved = itemsRemoved.Cast<string>();

                bool added = collectionAdded.Count() > 0;
                var newlist = added ? collectionAdded : collectionRemoved;
                bool aveh = added ? false : true;
                bool adlc = added ? true : false;

                foreach (var item in newlist)
                {
                    int temp = avehCOUPEList.Items.IndexOf(item);
                    if (temp != -1)
                    {
                        switch (temp)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 3, aveh);
                                break;
                            case 7:
                            case 8:
                            case 9:
                                Functions.Write.writebinary(temp - 7 + 1, GTA.Offsets.Editor.Race.adlc + (3 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }


        private void cbavehMuscle_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(5, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehMuscle);
        }

        private void avehMuscleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                //ListItemCollection
                System.Collections.IList itemsAdded = e.AddedItems;
                System.Collections.IList itemsRemoved = e.RemovedItems;

                var collectionAdded = itemsAdded.Cast<string>();
                var collectionRemoved = itemsRemoved.Cast<string>();

                bool added = collectionAdded.Count() > 0;
                var newlist = added ? collectionAdded : collectionRemoved;
                bool aveh = added ? false : true;
                bool adlc = added ? true : false;

                foreach (var item in newlist)
                {
                    int temp = avehMuscleList.Items.IndexOf(item);
                    if (temp != -1)
                    {
                        switch (temp)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 4, aveh);
                                break;
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                            case 30:
                            case 31:
                            case 32:
                            case 33:
                            case 34:
                            case 35:
                            case 36:
                            case 37:
                            case 38:
                            case 39:
                            case 40:
                                Functions.Write.writebinary(temp - 9 + 1, GTA.Offsets.Editor.Race.adlc + (4 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                                break;
                            case 41:
                            case 42:
                            case 43:
                            case 44:
                            case 45:
                            case 46:
                            case 47:
                            case 48:
                            case 49:
                            case 50:
                            case 51:
                            case 52:
                            case 53:
                                Functions.Write.writebinary(temp - 41 + 1, GTA.Offsets.Editor.Race.adlc2 + (4 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }


        private void cbavehSports_Classics_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(6, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSports_Classics);
        }

        private void avehSports_ClassicsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                //ListItemCollection
                System.Collections.IList itemsAdded = e.AddedItems;
                System.Collections.IList itemsRemoved = e.RemovedItems;

                var collectionAdded = itemsAdded.Cast<string>();
                var collectionRemoved = itemsRemoved.Cast<string>();

                bool added = collectionAdded.Count() > 0;
                var newlist = added ? collectionAdded : collectionRemoved;
                bool aveh = added ? false : true;
                bool adlc = added ? true : false;

                foreach (var item in newlist)
                {
                    int temp = avehSports_ClassicsList.Items.IndexOf(item);
                    if (temp != -1)
                    {
                        switch (temp)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                                Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 5, aveh);
                                break;
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                            case 30:
                            case 31:
                            case 32:
                            case 33:
                            case 34:
                            case 35:
                                Functions.Write.writebinary(temp - 4 + 1, GTA.Offsets.Editor.Race.adlc + (5 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }


        private void cbavehSports_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSports);
        }

        private void avehSportsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehSportsList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 6, aveh);
                            break;
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                        case 32:
                        case 33:
                        case 34:
                        case 35:
                        case 36:
                        case 37:
                        case 38:
                        case 39:
                        case 40:
                        case 41:
                            Functions.Write.writebinary(temp - 11 + 1, GTA.Offsets.Editor.Race.adlc + (6 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        case 42:
                        case 43:
                        case 44:
                        case 45:
                        case 46:
                        case 47:
                        case 48:
                        case 49:
                        case 50:
                        case 51:
                        case 52:
                        case 53:
                        case 54:
                        case 55:
                        case 56:
                        case 57:
                        case 58:
                        case 59:
                        case 60:
                        case 61:
                        case 62:
                        case 63:
                        case 64:
                        case 65:
                        case 66:
                        case 67:
                        case 68:
                        case 69:
                        case 70:
                        case 71:
                        case 72:
                            Functions.Write.writebinary(temp - 42 + 1, GTA.Offsets.Editor.Race.adlc2 + (6 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        case 73:
                        case 74:
                        case 75:
                        case 76:
                        case 77:
                            Functions.Write.writebinary(temp - 73 + 1, GTA.Offsets.Editor.Race.adlc3 + (6 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehSuper_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(8, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSuper);
        }

        private void avehSuperList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehSuperList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 7, aveh);
                            break;
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                        case 32:
                        case 33:
                        case 34:
                        case 35:
                        case 36:
                        case 37:
                            Functions.Write.writebinary(temp - 7 + 1, GTA.Offsets.Editor.Race.adlc + (7 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        case 38:
                        case 39:
                        case 40:
                        case 41:
                        case 42:
                        case 43:
                        case 44:
                        case 45:
                        case 46:
                        case 47:
                        case 48:
                        case 49:
                        case 50:
                            Functions.Write.writebinary(temp - 38 + 1, GTA.Offsets.Editor.Race.adlc2 + (7 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehMotorcycles_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(9, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehMotorcycles);
        }

        private void avehMotorcyclesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehMotorcyclesList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 8, aveh);
                            break;
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                        case 32:
                        case 33:
                        case 34:
                        case 35:
                        case 36:
                        case 37:
                        case 38:
                        case 39:
                        case 40:
                        case 41:
                        case 42:
                        case 43:
                        case 44:
                            Functions.Write.writebinary(temp - 14 + 1, GTA.Offsets.Editor.Race.adlc + (8 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        case 45:
                        case 46:
                        case 47:
                        case 48:
                        case 49:
                        case 50:
                            Functions.Write.writebinary(temp - 45 + 1, GTA.Offsets.Editor.Race.adlc2 + (8 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehOff_Road_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehOff_Road);
        }

        private void avehOff_RoadList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehOff_RoadList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 9, aveh);
                            break;
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                        case 32:
                        case 33:
                        case 34:
                        case 35:
                        case 36:
                        case 37:
                        case 38:
                        case 39:
                        case 40:
                        case 41:
                        case 42:
                        case 43:
                        case 44:
                        case 45:
                        case 46:
                        case 47:
                        case 48:
                            Functions.Write.writebinary(temp - 16 + 1, GTA.Offsets.Editor.Race.adlc + (9 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehIndustrial_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehIndustrial);
        }

        private void avehIndustrialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehIndustrialList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 10, aveh);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehUtility_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehUtility);
        }

        private void avehUtilityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehUtilityList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 11, aveh);
                            break;
                        case 5:
                            Functions.Write.writebinary(temp - 5 + 1, GTA.Offsets.Editor.Race.adlc + (11 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehCycles_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(14, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehCycles);
        }

        private void avehCyclesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehCyclesList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 13, aveh);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehVans_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehVans);
        }

        private void avehVansList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehVansList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.aveh + 12, aveh);
                            break;
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                            Functions.Write.writebinary(temp - 13 + 1, GTA.Offsets.Editor.Race.adlc + (12 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehSpecial_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(16, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehSpecial);
        }

        private void avehSpecialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehSpecialList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.adlc + (15 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehWeaponized_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(17, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehWeaponized);
        }

        private void avehWeaponizedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehWeaponizedList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.adlc + (16 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private void cbavehArena_Contender_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(18, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehArena_Contender);
        }

        private void avehArena_ContenderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehArena_ContenderList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.adlc + (17 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void cbavehOpenWheel_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehOpenWheel);
        }

        private void avehOpenWheelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehOpenWheelList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.adlc + (18 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void cbavehGoKart_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehGoKart);
        }

        private void avehGoKartList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehGoKartList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.adlc + (19 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void cbavehTuner_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(21, GTA.Offsets.Editor.Race.Checkpoints.clbs, cbavehTuner);
        }

        private void avehTunerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListItemCollection
            System.Collections.IList itemsAdded = e.AddedItems;
            System.Collections.IList itemsRemoved = e.RemovedItems;

            var collectionAdded = itemsAdded.Cast<string>();
            var collectionRemoved = itemsRemoved.Cast<string>();

            bool added = collectionAdded.Count() > 0;
            var newlist = added ? collectionAdded : collectionRemoved;
            bool aveh = added ? false : true;
            bool adlc = added ? true : false;

            foreach (var item in newlist)
            {
                int temp = avehTunerList.Items.IndexOf(item);
                if (temp != -1)
                {
                    switch (temp)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                            Functions.Write.writebinary(temp + 1, GTA.Offsets.Editor.Race.adlc + (20 * GTA.Offsets.Editor.Race.adlc_NEXT), adlc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void ddRaceVehClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddRaceVehClass.SelectedIndex > -1)
            {
                new Global(GTA.Offsets.Editor.Race.Checkpoints.icv).SetInt((int)((ComboBoxItem)ddRaceVehClass.SelectedItem).Tag);
            }
        }

        private void ddRaceVehClass_DropDownOpened(object sender, EventArgs e)
        {
            ddRaceVehClass.Items.Clear();
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

            var missionitem = new ComboBoxItem();

            if (class1)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 0;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_compacts]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class2)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 1;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_sedan]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class3)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 2;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_suv]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class4)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 3;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_coupe]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class5)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 4;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_muscle]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class6)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 5;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_sports_classic]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class7)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 6;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_sport]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class8)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 7;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_super]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class9)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 8;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_motorcycle]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class10)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 9;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_off_road]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class11)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 10;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_industrial]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class12)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 11;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_utility]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class13)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 12;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_van]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class14)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 13;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_cycle]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class15)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 15;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_special]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class16)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 16;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_weaponized]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class17)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 17;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_arena_contender]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class18)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 18;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_open_wheel]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class19)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 19;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_go_kart]"));
                ddRaceVehClass.Items.Add(missionitem);
            }
            if (class20)
            {
                missionitem = new ComboBoxItem();
                missionitem.Tag = 20;
                missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[vc_tuner]"));
                ddRaceVehClass.Items.Add(missionitem);
            }

            missionitem = null;
        }
    }
}
