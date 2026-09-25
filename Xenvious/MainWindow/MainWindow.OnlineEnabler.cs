using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;

namespace Xenvious
{
    // Part of MainWindow: Online enabler panel.
    public partial class MainWindow
    {
        private void BtnOnlineEnabler_Click(object sender, RoutedEventArgs e)
        {
            showOnlineEnablePanel();
        }

        public void showOnlineEnablePanel()
        {
            ScreenMessage.MouseLeftButtonUp -= IMGBackground_MouseLeftButtonUp;

            Grid tempgrid = new Grid();
            tempgrid.Width = 300;
            tempgrid.Height = 300;
            RowDefinition row1 = new RowDefinition();
            row1.Height = new GridLength(1, GridUnitType.Star);
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(50, GridUnitType.Pixel);
            tempgrid.RowDefinitions.Add(row1);
            tempgrid.RowDefinitions.Add(row2);
            ColumnDefinition col1 = new ColumnDefinition();
            col1.Width = new GridLength(0.5, GridUnitType.Star);
            ColumnDefinition col2 = new ColumnDefinition();
            col2.Width = new GridLength(0.5, GridUnitType.Star);
            tempgrid.ColumnDefinitions.Add(col1);
            tempgrid.ColumnDefinitions.Add(col2);

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

            StackPanel sp = new StackPanel();

            Label disclaimer = new Label();
            disclaimer.Content = "Disclaimer";
            disclaimer.FontWeight = FontWeights.Bold;
            disclaimer.FontSize = 20;
            disclaimer.HorizontalAlignment = HorizontalAlignment.Center;
            disclaimer.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF04646"));
            disclaimer.Height = 35;

            TextBlock temptb = new TextBlock();
            temptb.HorizontalAlignment = HorizontalAlignment.Center;
            temptb.FontSize = 16;
            temptb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD3D3D3"));
            temptb.Text = "By proceeding with this DLL injection, you acknowledge that you are doing so at your own risk. No responsibility or liability will be taken for any potential damage, bans, crashes, or other consequences that may occur. If you continue, you accept full responsibility for your actions.\r\n\r\nDo you wish to proceed?";
            temptb.TextWrapping = TextWrapping.Wrap;

            Button tempbtn = new Button();
            tempbtn.Style = (Style)FindResource("CustomButton");
            tempbtn.Content = "Ok";
            tempbtn.SetValue(CornerRadiusSetter.CornerRadiusProperty, new CornerRadius(5));
            tempbtn.Click += async delegate
            {
                if (!m.IsProcOpen)
                    return;

                ScreenMessageContainer.Children.Remove(tempgrid);
                ScreenMessage.MouseLeftButtonUp += IMGBackground_MouseLeftButtonUp;
                ScreenMessage.Visibility = Visibility.Collapsed;
                switch (await OnlineEnable.init())
                {
                    case OnlineEnable.Responses.SUCCESFULL:
                        displayScreenMessage(_Translation.Where(x => x.Key == "oe_inject_success").First().Value);
                        break;
                    case OnlineEnable.Responses.ALREADY_INJECTED:
                        displayScreenMessage(_Translation.Where(x => x.Key == "oe_inject_already").First().Value);
                        break;
                    case OnlineEnable.Responses.FAILURE_INJECT:
                        displayScreenMessage(_Translation.Where(x => x.Key == "oe_inject_fail").First().Value);
                        break;
                    case OnlineEnable.Responses.FAILURE_DOWNLOAD:
                        displayScreenMessage(_Translation.Where(x => x.Key == "error_oe_download").First().Value);
                        break;
                    case OnlineEnable.Responses.FAILURE_OTHER:
                        displayScreenMessage(_Translation.Where(x => x.Key == "error_happened").First().Value);
                        break;
                    default:
                        displayScreenMessage(_Translation.Where(x => x.Key == "error_happened").First().Value);
                        break;
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

            sp.Children.Add(disclaimer);
            sp.Children.Add(temptb);
            b1.Child = tempbtn;
            b2.Child = tempbtncancel;
            tempgrid.Children.Add(sp);
            tempgrid.Children.Add(b1);
            tempgrid.Children.Add(b2);

            Grid.SetRow(sp, 0);
            Grid.SetColumnSpan(sp, 2);
            Grid.SetRow(b1, 1);
            Grid.SetColumn(b1, 0);
            Grid.SetRow(b2, 1);
            Grid.SetColumn(b2, 1);

            ScreenMessageContainer.Children.Add(tempgrid);
            ScreenMessage.Visibility = Visibility.Visible;
            FocusManager.SetFocusedElement(tempgrid, b1);
        }
    }
}
