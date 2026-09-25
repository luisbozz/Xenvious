using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;

namespace Xenvious
{
    // Part of MainWindow: The legacy in-window message box.
    public partial class MainWindow
    {
        public void displayScreenMessage(string message)
        {
            ScreenMessage.MouseLeftButtonUp -= IMGBackground_MouseLeftButtonUp;

            Grid tempgrid = new Grid();
            RowDefinition row1 = new RowDefinition();
            row1.Height = GridLength.Auto;
            row1.MaxHeight = 300;
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(40);
            tempgrid.RowDefinitions.Add(row1);
            tempgrid.RowDefinitions.Add(row2);
            tempgrid.Width = 500;


            ScrollViewer sv = new ScrollViewer();
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;


            TextBlock temptb = new TextBlock();
            temptb.Height = 30;
            temptb.HorizontalAlignment = HorizontalAlignment.Center;
            temptb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF04646"));
            temptb.Text = message;
            temptb.TextWrapping = TextWrapping.Wrap;

            Border b1 = new Border();
            b1.CornerRadius = new CornerRadius(5);
            b1.Width = 150;
            b1.Height = 30;
            b1.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            b1.Effect = new DropShadowEffect
            {
                ShadowDepth = 2,
                BlurRadius = 5
            };

            Button tempbtn = new Button();
            tempbtn.Style = (Style)FindResource("CustomButton");
            tempbtn.Content = "Ok";
            tempbtn.SetValue(CornerRadiusSetter.CornerRadiusProperty, new CornerRadius(5));
            tempbtn.Click += delegate
            {
                ScreenMessageContainer.Children.Remove(tempgrid);
                ScreenMessage.MouseLeftButtonUp += IMGBackground_MouseLeftButtonUp;
                ScreenMessage.Visibility = Visibility.Collapsed;
            };

            sv.Content = temptb;
            b1.Child = tempbtn;
            tempgrid.Children.Add(sv);
            tempgrid.Children.Add(b1);

            Grid.SetRow(sv, 0);
            Grid.SetRow(b1, 1);

            ScreenMessageContainer.Children.Add(tempgrid);
            ScreenMessage.Visibility = Visibility.Visible;
            FocusManager.SetFocusedElement(tempgrid, tempbtn);
        }
    }
}
