using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: DeleteItems page.
    public partial class MainWindow
    {
        private void BtnPropsDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddpropno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Props.number).Get<int>();

                if (index > -1)
                {
                    try
                    {
                        for (int i = 0; i < 125; i++)
                        {
                            int test = new Global((GTA.Offsets.Editor.Props.loc + (10 * GTA.Offsets.Editor.Props.NEXT) + i)).Get<int>();
                        }
                        List<List<int>> valuesafterdeleteprop = new List<List<int>>();

                        // get props after deleted prop
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Props.NEXT; d++)
                            {
                                int test = new Global((GTA.Offsets.Editor.Props.loc + ((i + 1) * GTA.Offsets.Editor.Props.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeleteprop.Add(temp);
                        }

                        // clear deleted prop values

                        for (int i = 0; i < GTA.Offsets.Editor.Props.NEXT; i++)
                        {
                            long deletedpropbase = GTA.Offsets.Editor.Props.loc + (index * GTA.Offsets.Editor.Props.NEXT);
                            new Global(deletedpropbase + i).SetInt(GTA.Defaults.Prop[i]);
                        }

                        // lower prop number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Props.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeleteprop.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeleteprop[i].Count(); d++)
                            {
                                long propbase = GTA.Offsets.Editor.Props.loc + ((index + i) * GTA.Offsets.Editor.Props.NEXT);
                                new Global(propbase + d).SetInt(valuesafterdeleteprop[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void BtndpropsDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = dddpropno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.DProps.number).Get<int>();

                if (index > -1)
                {
                    try
                    {
                        List<List<int>> valuesafterdeleteddprop = new List<List<int>>();

                        // get props after deleted dprop
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.DProps.NEXT; d++)
                            {
                                int test = new Global((GTA.Offsets.Editor.DProps.loc + ((i + 1) * GTA.Offsets.Editor.DProps.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeleteddprop.Add(temp);
                        }

                        // clear deleted dprop values

                        for (int i = 0; i < GTA.Offsets.Editor.DProps.NEXT; i++)
                        {
                            long deleteddpropbase = GTA.Offsets.Editor.DProps.loc + (index * GTA.Offsets.Editor.DProps.NEXT);
                            new Global(deleteddpropbase + i).SetInt(GTA.Defaults.DProp[i]);
                        }

                        // lower dprop number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.DProps.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeleteddprop.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeleteddprop[i].Count(); d++)
                            {
                                long dpropbase = GTA.Offsets.Editor.DProps.loc + ((index + i) * GTA.Offsets.Editor.DProps.NEXT);
                                new Global(dpropbase + d).SetInt(valuesafterdeleteddprop[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void BtnActorDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddactorno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Actor.number).Get<int>();

                if (index > -1)
                {
                    if (num < 80 && num > 0)
                    {
                        new Global(GTA.Offsets.Editor.Actor.locx + num * GTA.Offsets.Editor.Actor.NEXT).SetBytes(new Global(GTA.Offsets.Editor.Actor.locx + index * GTA.Offsets.Editor.Actor.NEXT).GetBytes((int)(GTA.Offsets.Editor.Actor.NEXT * 8)));
                        new Global(GTA.Offsets.Editor.Actor.number).SetInt(num + 1);
                    }
                }
            }
        }
        private void BtnActorDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddactorno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Actor.number).Get<int>();

                if (index > -1)
                {
                    try
                    {

                        List<List<int>> valuesafterdeletedactor = new List<List<int>>();

                        // get props after deleted actor
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Actor.NEXT; d++)
                            {
                                int test = new Global((GTA.Offsets.Editor.Actor.locx + ((i + 1) * GTA.Offsets.Editor.Actor.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeletedactor.Add(temp);
                        }

                        // clear deleted actor values

                        for (int i = 0; i < GTA.Offsets.Editor.Actor.NEXT; i++)
                        {
                            long deletedactorbase = GTA.Offsets.Editor.Actor.locx + (index * GTA.Offsets.Editor.Actor.NEXT);
                            new Global(deletedactorbase + i).SetInt(GTA.Defaults.Actor[i]);
                        }

                        // lower actor number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Actor.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeletedactor.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeletedactor[i].Count(); d++)
                            {
                                long actorbase = GTA.Offsets.Editor.Actor.locx + ((index + i) * GTA.Offsets.Editor.Actor.NEXT);
                                new Global(actorbase + d).SetInt(valuesafterdeletedactor[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }



        private void BtnVehDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddvehno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Vehicle.number).Get<int>();

                if (index > -1)
                {
                    try
                    {
                        List<List<int>> valuesafterdeletedveh = new List<List<int>>();

                        // get props after deleted Vehicle
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Vehicle.NEXT; d++)
                            {
                                int test = new Global((GTA.Offsets.Editor.Vehicle.loc + ((i + 1) * GTA.Offsets.Editor.Vehicle.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeletedveh.Add(temp);
                        }

                        // clear deleted Vehicle values

                        for (int i = 0; i < GTA.Offsets.Editor.Vehicle.NEXT; i++)
                        {
                            long deletedvehbase = GTA.Offsets.Editor.Vehicle.loc + (index * GTA.Offsets.Editor.Vehicle.NEXT);
                            new Global(deletedvehbase + i).SetInt(GTA.Defaults.Vehicle[i]);
                        }

                        // lower Vehicle number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Vehicle.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeletedveh.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeletedveh[i].Count(); d++)
                            {
                                long vehbase = GTA.Offsets.Editor.Vehicle.loc + ((index + i) * GTA.Offsets.Editor.Vehicle.NEXT);
                                new Global(vehbase + d).SetInt(valuesafterdeletedveh[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void BtnWeapDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddweapno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Weapon.number).Get<int>();

                if (index > -1)
                {
                    try
                    {
                        List<List<int>> valuesafterdeletedweap = new List<List<int>>();

                        // get props after deleted weap
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Weapon.NEXT; d++)
                            {
                                int test = new Global((GTA.Offsets.Editor.Weapon.locx + ((i + 1) * GTA.Offsets.Editor.Weapon.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeletedweap.Add(temp);
                        }

                        // clear deleted weap values

                        for (int i = 0; i < GTA.Offsets.Editor.Weapon.NEXT; i++)
                        {
                            long deletedweapbase = GTA.Offsets.Editor.Weapon.locx + (index * GTA.Offsets.Editor.Weapon.NEXT);
                            new Global(deletedweapbase + i).SetInt(GTA.Defaults.Weapon[i]);
                        }

                        // lower weap number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Weapon.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeletedweap.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeletedweap[i].Count(); d++)
                            {
                                long weapbase = GTA.Offsets.Editor.Weapon.locx + ((index + i) * GTA.Offsets.Editor.Weapon.NEXT);
                                new Global(weapbase + d).SetInt(valuesafterdeletedweap[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }
}
