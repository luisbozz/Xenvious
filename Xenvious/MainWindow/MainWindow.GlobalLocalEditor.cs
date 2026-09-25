using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;
using static mry.mem;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: GlobalLocalEditor page.
    public partial class MainWindow
    {
        // Global and local freeze list live in one card now. Only the header buttons
        // decide which of the two grids is on screen; both keep their own binding.
        private void ShowFreezeList(bool global)
        {
            if (gridFreezeGlobal == null || gridFreezeLocal == null)
                return;

            gridFreezeGlobal.Visibility = global ? Visibility.Visible : Visibility.Collapsed;
            gridFreezeLocal.Visibility = global ? Visibility.Collapsed : Visibility.Visible;

            BtnFreezeListGlobal.Background = (SolidColorBrush)Resources[global ? "ButtonHoverBackgroundBrush" : "SectionBackgroundBrush"];
            BtnFreezeListLocal.Background = (SolidColorBrush)Resources[global ? "SectionBackgroundBrush" : "ButtonHoverBackgroundBrush"];
        }

        private void BtnFreezeListGlobal_Click(object sender, RoutedEventArgs e) => ShowFreezeList(true);
        private void BtnFreezeListLocal_Click(object sender, RoutedEventArgs e) => ShowFreezeList(false);

        private void Btngereadglobal_Click(object sender, RoutedEventArgs e)
        {
            switch (ddgechangemode.SelectedIndex)
            {
                case 0:
                    tbgeglobalvalue.Text = new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).GetBytes(1)[0].ToString();
                    break;
                case 1:
                    tbgeglobalvalue.Text = new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).Get<short>().ToString();
                    break;
                case 2:
                    tbgeglobalvalue.Text = new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).Get<int>().ToString();
                    break;
                case 3:
                    tbgeglobalvalue.Text = new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).Get<long>().ToString();
                    break;
                case 4:
                    tbgeglobalvalue.Text = new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).Get<float>().ToString();
                    break;
                case 5:
                    tbgeglobalvalue.Text = new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).Get<double>().ToString();
                    break;
                case 6:
                    tbgeglobalvalue.Text = new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).GetString();
                    break;
                default:
                    break;
            }
        }

        private void Btngewriteglobal_Click(object sender, RoutedEventArgs e)
        {
            switch (ddgechangemode.SelectedIndex)
            {
                case 0:
                    try
                    {
                        new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).SetBytes(new byte[] { byte.Parse(tbgeglobalvalue.Text) });
                    }
                    catch (Exception)
                    {

                    }
                    break;
                case 1:
                    new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).SetShort(tbgeglobalvalue.Text);
                    break;
                case 2:
                    new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).SetInt(tbgeglobalvalue.Text);
                    break;
                case 3:
                    new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).SetLong(tbgeglobalvalue.Text);
                    break;
                case 4:
                    new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).SetFloat(tbgeglobalvalue.Text);
                    break;
                case 5:
                    new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).SetDouble(tbgeglobalvalue.Text);
                    break;
                case 6:
                    new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).SetString(tbgeglobalvalue.Text);
                    break;
                default:
                    break;
            }
        }

        private void Btngecopyaddy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(new Global(OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(tbgeglobal.Text))).GetAddress().ToString("X"));
        }

        private void Btngeadd2f_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tbgeglobal.Text))
            {
                displayScreenMessage("Please input a global.");
                return;
            }
            if (String.IsNullOrEmpty(tbgeglobalvalue.Text))
            {
                displayScreenMessage("Please input a global value.");
                return;
            }

            ScreenMessage.MouseLeftButtonUp -= IMGBackground_MouseLeftButtonUp;

            Grid tempgrid = new Grid();
            tempgrid.Width = 300;
            tempgrid.Height = 100;
            RowDefinition row1 = new RowDefinition();
            row1.Height = new GridLength(0.5, GridUnitType.Star);
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(0.5, GridUnitType.Star);
            tempgrid.RowDefinitions.Add(row1);
            tempgrid.RowDefinitions.Add(row2);
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
            temptb.SetBinding(TagProperty, new Binding { Path = new PropertyPath("Translation[name]"), FallbackValue = "Name" });

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

            Button tempbtn = new Button();
            tempbtn.Style = (Style)FindResource("CustomButton");
            tempbtn.Content = "Add";
            tempbtn.SetValue(CornerRadiusSetter.CornerRadiusProperty, new CornerRadius(5));
            tempbtn.Click += delegate
            {
                string global = tbgeglobal.Text;
                GlobalFreezeer globalFreezeer = null;
                try
                {
                    switch (ddgechangemode.SelectedIndex)
                    {
                        case 0:
                            globalFreezeer = new GlobalFreezeer(temptb.Text, global, new byte[] { byte.Parse(tbgeglobalvalue.Text) });
                            break;
                        case 1:
                            globalFreezeer = new GlobalFreezeer(temptb.Text, global, short.Parse(tbgeglobalvalue.Text));
                            break;
                        case 2:
                            globalFreezeer = new GlobalFreezeer(temptb.Text, global, int.Parse(tbgeglobalvalue.Text));
                            break;
                        case 3:
                            globalFreezeer = new GlobalFreezeer(temptb.Text, global, long.Parse(tbgeglobalvalue.Text));
                            break;
                        case 4:
                            globalFreezeer = new GlobalFreezeer(temptb.Text, global, float.Parse(tbgeglobalvalue.Text));
                            break;
                        case 5:
                            globalFreezeer = new GlobalFreezeer(temptb.Text, global, double.Parse(tbgeglobalvalue.Text));
                            break;
                        case 6:
                            globalFreezeer = new GlobalFreezeer(temptb.Text, global, tbgeglobalvalue.Text);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    displayScreenMessage("couldnt parse value");
                }
                addItemToGEFreezeList(globalFreezeer);
                ScreenMessageContainer.Children.Remove(tempgrid);
                ScreenMessage.MouseLeftButtonUp += IMGBackground_MouseLeftButtonUp;
                ScreenMessage.Visibility = Visibility.Collapsed;
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

            Grid.SetRow(temptb, 0);
            Grid.SetColumnSpan(temptb, 2);
            Grid.SetRow(b1, 1);
            Grid.SetColumn(b1, 0);
            Grid.SetRow(b2, 1);
            Grid.SetColumn(b2, 1);


            ScreenMessageContainer.Children.Add(tempgrid);
            ScreenMessage.Visibility = Visibility.Visible;
            FocusManager.SetFocusedElement(tempgrid, temptb);
        }


        public void addItemToGEFreezeList(GlobalFreezeer globalFreezeer)
        {
            freezeList.Items.Add(globalFreezeer);
        }

        public void addItemToLEFreezeList(LocalFreezeer localFreezeer)
        {
            localfreezeList.Items.Add(localFreezeer);
        }

        private void Btnlefreezelistdel_Click(object sender, RoutedEventArgs e)
        {
            if (localfreezeList.SelectedItem != null)
            {
                int index = localfreezeList.SelectedIndex;
                localfreezeList.Items.Remove(localfreezeList.SelectedItem);
                if (localfreezeList.Items.Count >= index)
                {
                    localfreezeList.SelectedIndex = index;
                }
                else if (localfreezeList.Items.Count == 1)
                {
                    localfreezeList.SelectedIndex = 0;
                }
            }
        }

        private void Btngefreezelistdel_Click(object sender, RoutedEventArgs e)
        {
            if (freezeList.SelectedItem != null)
            {
                int index = freezeList.SelectedIndex;
                freezeList.Items.Remove(freezeList.SelectedItem);
                if (freezeList.Items.Count >= index)
                {
                    freezeList.SelectedIndex = index;
                }
                else if (freezeList.Items.Count == 1)
                {
                    freezeList.SelectedIndex = 0;
                }
            }
        }

        private void ddlechangemode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Btnlereadglobal_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tblelocalscript.Text))
            {
                displayScreenMessage("Please input a scriptname.");
                return;
            }
            if (String.IsNullOrEmpty(tblelocal.Text))
            {
                displayScreenMessage("Please input a local.");
                return;
            }
            long[] scriptptr = GTA.getLocalScriptAddy(tblelocalscript.Text);
            if (scriptptr == null)
            {
                displayScreenMessage("Couldnt find Script.");
                return;
            }
            long convertedlocal = OffsetLoader.GetGlobalOffset(tblelocal.Text);
            mry.mem.Memory lem = m.memory(scriptptr[0], new long[] { scriptptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, convertedlocal * 8 });

            switch (ddlechangemode.SelectedIndex)
            {
                case 0:
                    tblelocalvalue.Text = string.Join(", ", lem.GetBytes(8));
                    break;
                case 1:
                    tblelocalvalue.Text = lem.Get<short>().ToString();
                    break;
                case 2:
                    tblelocalvalue.Text = lem.Get<int>().ToString();
                    break;
                case 3:
                    tblelocalvalue.Text = lem.Get<long>().ToString();
                    break;
                case 4:
                    tblelocalvalue.Text = lem.Get<float>().ToString();
                    break;
                case 5:
                    tblelocalvalue.Text = lem.Get<double>().ToString();
                    break;
                case 6:
                    tblelocalvalue.Text = lem.GetString();
                    break;
                default:
                    break;
            }
        }

        private void Btnlewriteglobal_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tblelocalscript.Text))
            {
                displayScreenMessage("Please input a scriptname.");
                return;
            }
            if (String.IsNullOrEmpty(tblelocal.Text))
            {
                displayScreenMessage("Please input a local.");
                return;
            }
            if (String.IsNullOrEmpty(tblelocalvalue.Text))
            {
                displayScreenMessage("Please input a local value.");
                return;
            }
            long[] scriptptr = GTA.getLocalScriptAddy(tblelocalscript.Text);
            if (scriptptr == null)
            {
                displayScreenMessage("Couldnt find Script.");
                return;
            }
            long convertedlocal = OffsetLoader.GetGlobalOffset(tblelocal.Text);
            mry.mem.Memory lem = m.memory(scriptptr[0], new long[] { scriptptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, convertedlocal * 8 });

            try
            {
                switch (ddlechangemode.SelectedIndex)
                {
                    case 0:
                        lem.SetBytes(new byte[] { byte.Parse(tblelocalvalue.Text) });
                        break;
                    case 1:
                        lem.SetShort(short.Parse(tblelocalvalue.Text));
                        break;
                    case 2:
                        lem.SetInt(int.Parse(tblelocalvalue.Text));
                        break;
                    case 3:
                        lem.SetLong(long.Parse(tblelocalvalue.Text));
                        break;
                    case 4:
                        lem.SetFloat(float.Parse(tblelocalvalue.Text));
                        break;
                    case 5:
                        lem.SetDouble(double.Parse(tblelocalvalue.Text));
                        break;
                    case 6:
                        lem.SetString(tblelocalvalue.Text);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                displayScreenMessage("Couldnt parse value.. please check format");
            }
        }

        private void Btnlecopyaddy_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tblelocalscript.Text))
            {
                displayScreenMessage("Please input a scriptname.");
                return;
            }
            if (String.IsNullOrEmpty(tblelocal.Text))
            {
                displayScreenMessage("Please input a local.");
                return;
            }
            long[] scriptptr = GTA.getLocalScriptAddy(tblelocalscript.Text);
            if (scriptptr == null)
            {
                displayScreenMessage("Couldnt find Script.");
                return;
            }
            long convertedlocal = OffsetLoader.GetGlobalOffset(tblelocal.Text);
            Clipboard.SetText(m.memory(scriptptr[0], new long[] { scriptptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, convertedlocal * 8 }).GetAddress().ToString("X"));
        }

        private void Btnleadd2f_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tblelocalscript.Text))
            {
                displayScreenMessage("Please input a scriptname.");
                return;
            }
            if (String.IsNullOrEmpty(tblelocal.Text))
            {
                displayScreenMessage("Please input a local.");
                return;
            }
            if (String.IsNullOrEmpty(tblelocalvalue.Text))
            {
                displayScreenMessage("Please input a local value.");
                return;
            }

            ScreenMessage.MouseLeftButtonUp -= IMGBackground_MouseLeftButtonUp;

            Grid tempgrid = new Grid();
            tempgrid.Width = 300;
            tempgrid.Height = 100;
            RowDefinition row1 = new RowDefinition();
            row1.Height = new GridLength(0.5, GridUnitType.Star);
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(0.5, GridUnitType.Star);
            tempgrid.RowDefinitions.Add(row1);
            tempgrid.RowDefinitions.Add(row2);
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
            temptb.SetBinding(TagProperty, new Binding { Path = new PropertyPath("Translation[name]"), FallbackValue = "Name" });

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

            Button tempbtn = new Button();
            tempbtn.Style = (Style)FindResource("CustomButton");
            tempbtn.Content = "Add";
            tempbtn.SetValue(CornerRadiusSetter.CornerRadiusProperty, new CornerRadius(5));
            tempbtn.Click += delegate
            {
                string local = tblelocal.Text;
                string script = tblelocalscript.Text;
                LocalFreezeer localFreezeer = null;
                try
                {
                    switch (ddgechangemode.SelectedIndex)
                    {
                        case 0:
                            localFreezeer = new LocalFreezeer(temptb.Text, script, local, new byte[] { byte.Parse(tblelocalvalue.Text) });
                            break;
                        case 1:
                            localFreezeer = new LocalFreezeer(temptb.Text, script, local, short.Parse(tblelocalvalue.Text));
                            break;
                        case 2:
                            localFreezeer = new LocalFreezeer(temptb.Text, script, local, int.Parse(tblelocalvalue.Text));
                            break;
                        case 3:
                            localFreezeer = new LocalFreezeer(temptb.Text, script, local, long.Parse(tbgeglobalvalue.Text));
                            break;
                        case 4:
                            localFreezeer = new LocalFreezeer(temptb.Text, script, local, float.Parse(tblelocalvalue.Text));
                            break;
                        case 5:
                            localFreezeer = new LocalFreezeer(temptb.Text, script, local, double.Parse(tblelocalvalue.Text));
                            break;
                        case 6:
                            localFreezeer = new LocalFreezeer(temptb.Text, script, local, tblelocalvalue.Text);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    displayScreenMessage("couldnt parse value");
                }
                addItemToLEFreezeList(localFreezeer);
                ScreenMessageContainer.Children.Remove(tempgrid);
                ScreenMessage.MouseLeftButtonUp += IMGBackground_MouseLeftButtonUp;
                ScreenMessage.Visibility = Visibility.Collapsed;
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

            Grid.SetRow(temptb, 0);
            Grid.SetColumnSpan(temptb, 2);
            Grid.SetRow(b1, 1);
            Grid.SetColumn(b1, 0);
            Grid.SetRow(b2, 1);
            Grid.SetColumn(b2, 1);


            ScreenMessageContainer.Children.Add(tempgrid);
            ScreenMessage.Visibility = Visibility.Visible;
            FocusManager.SetFocusedElement(tempgrid, temptb);
        }

        private void freezeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (freezeList.SelectedItem != null)
            {
                var obj = ((GlobalFreezeer)freezeList.SelectedItem);
                var type = obj.val.GetType();

                if (type == typeof(int))
                {
                    ddgechangemode.SelectedIndex = 2;
                }
                else if (type == typeof(long))
                {
                    ddgechangemode.SelectedIndex = 3;
                }
                else if (type == typeof(float))
                {
                    ddgechangemode.SelectedIndex = 4;
                }
                else if (type == typeof(double))
                {
                    ddgechangemode.SelectedIndex = 5;
                }
                else if (type == typeof(short))
                {
                    ddgechangemode.SelectedIndex = 1;
                }
                else if (type == typeof(string))
                {
                    ddgechangemode.SelectedIndex = 6;
                }
                else if (true)
                {
                    ddgechangemode.SelectedIndex = 0;
                }
                tbgeglobalvalue.Text = obj.val.ToString();
                tbgeglobal.Text = obj.GlobalName.ToString();
            }
        }

        private void localfreezeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (localfreezeList.SelectedItem != null)
            {
                var obj = ((LocalFreezeer)localfreezeList.SelectedItem);
                var type = obj.val.GetType();

                if (type == typeof(int))
                {
                    ddlechangemode.SelectedIndex = 2;
                }
                else if (type == typeof(long))
                {
                    ddlechangemode.SelectedIndex = 3;
                }
                else if (type == typeof(float))
                {
                    ddlechangemode.SelectedIndex = 4;
                }
                else if (type == typeof(double))
                {
                    ddlechangemode.SelectedIndex = 5;
                }
                else if (type == typeof(short))
                {
                    ddlechangemode.SelectedIndex = 1;
                }
                else if (type == typeof(string))
                {
                    ddlechangemode.SelectedIndex = 6;
                }
                else if (true)
                {
                    ddlechangemode.SelectedIndex = 0;
                }
                tblelocalvalue.Text = obj.val.ToString();
                tblelocal.Text = obj.LocalName.ToString();
                tblelocalscript.Text = obj.Script.ToString();
            }
        }
    }
}
