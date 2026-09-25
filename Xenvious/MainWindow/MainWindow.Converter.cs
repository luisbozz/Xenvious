using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Converter page.
    public partial class MainWindow
    {
        public void UIntegerPasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)) && sender is TextBox)
            {
                string pastedText = (e.DataObject.GetData(typeof(string)) as string);
                string result = pastedText.Substring(0, pastedText.Length > 10 ? 10 : pastedText.Length);
                result = Regex.Replace(result, "[^0-9]+", "");

                DataObject d = new DataObject();
                d.SetData(DataFormats.Text, result);
                e.DataObject = d;

                e.Handled = true;
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void FormatTextForUInteger(object sender, KeyEventArgs e)
        {
            DataObject.AddPastingHandler((DependencyObject)sender, new DataObjectPastingEventHandler(UIntegerPasteHandler));

            if (!char.IsControl(GetCharFromKey(e.Key)) && !char.IsDigit(GetCharFromKey(e.Key)))
            {
                e.Handled = true;
            }
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                creatorRefresh();
            }
        }

        private void tbconvnative_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace((sender as TextBox).Text) || (sender as TextBox).IsFocused == false)
                    return;

                try
                {
                    uint temp = Functions.joaat(tbconvnative.Text);
                    tbconvhex.Text = temp.ToString("X");
                    tbconvint.Text = unchecked((int)temp).ToString();
                    tbconvuint.Text = temp.ToString();
                    tbconvbinary.Text = Convert.ToString(temp, 2);
                }
                catch
                {
                }
            }
            catch (Exception)
            {

            }
        }

        private void tbconvhex_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace((sender as TextBox).Text) || !Functions.isHex(tbconvhex.Text) || (sender as TextBox).IsFocused == false)
                    return;

                uint temp = uint.Parse(tbconvhex.Text, NumberStyles.HexNumber);

                string native = "";

                try
                {
                    native = GTA.Editor.PropList.Where(x => x.UInt == temp).FirstOrDefault().Native;
                    tbconvnative.Text = native;
                }
                catch (Exception)
                {
                    tbconvnative.Text = "couldnt load native";
                }

                tbconvint.Text = unchecked((int)temp).ToString();
                tbconvuint.Text = temp.ToString();
                tbconvbinary.Text = Convert.ToString(temp, 2);
            }
            catch (Exception)
            {

            }
        }

        private void tbconvint_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace((sender as TextBox).Text) || (sender as TextBox).IsFocused == false || !IsValidInt(tbconvint.Text))
                    return;

                uint temp = unchecked((uint)Convert.ToInt32(tbconvint.Text));

                string native = "";

                try
                {
                    native = GTA.Editor.PropList.Where(x => x.UInt == temp).FirstOrDefault().Native;
                    tbconvnative.Text = native;
                }
                catch (Exception)
                {
                    tbconvnative.Text = "couldnt load native";
                }

                tbconvhex.Text = temp.ToString("X");
                tbconvuint.Text = temp.ToString();
                tbconvbinary.Text = Convert.ToString(temp, 2);
            }
            catch (Exception)
            {

            }
        }

        private void tbconvuint_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace((sender as TextBox).Text) || (sender as TextBox).IsFocused == false || !IsValidInt(tbconvuint.Text, false))
                    return;

                uint temp = uint.Parse(tbconvuint.Text);

                string native = "";

                try
                {
                    native = GTA.Editor.PropList.Where(x => x.UInt == temp).FirstOrDefault().Native;
                    tbconvnative.Text = native;
                }
                catch (Exception)
                {
                    tbconvnative.Text = "couldnt load native";
                }

                tbconvhex.Text = temp.ToString("X");
                tbconvint.Text = unchecked((int)temp).ToString();
                tbconvbinary.Text = Convert.ToString(temp, 2);
            }
            catch (Exception)
            {

            }
        }

        private void tbconvbinary_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                List<char> bits = tbconvbinary.Text.ToList();
                bits.Reverse();
                List<int> index = new List<int>();

                for (int i = 0; i < bits.Count; i++)
                {
                    if (bits[i] == '1')
                    {
                        index.Add(i);
                    }
                }
                string temptooltip = "";
                index.ForEach(x => temptooltip += $"{x + 1},");

                Lblconvbinarynums.SetValue(Label.ContentProperty, temptooltip.Remove(temptooltip.Length - 1));

                if (String.IsNullOrWhiteSpace((sender as TextBox).Text) || (sender as TextBox).IsFocused == false)
                    return;

                uint temp = 0;


                temp = Convert.ToUInt32(tbconvbinary.Text, 2);


                string native = "";

                try
                {
                    native = GTA.Editor.PropList.Where(x => x.UInt == temp).FirstOrDefault().Native;
                    tbconvnative.Text = native;
                }
                catch (Exception)
                {
                    tbconvnative.Text = "couldnt load native";
                }

                tbconvhex.Text = temp.ToString("X");
                tbconvint.Text = unchecked((int)temp).ToString();
                tbconvuint.Text = temp.ToString();
            }
            catch (Exception)
            {

            }
        }

        private async void Btncenvgetimg_Click(object sender, RoutedEventArgs e)
        {
            if (tbconvnative.Text.Length > 0 && tbconvnative.Text != "couldnt load native")
            {
                string url = $"https://xenvious.com/resources/props/{tbconvnative.Text}.jpg";
                string url2 = $"https://cdn.rage.mp/public/odb/imgs/{tbconvnative.Text}-{tbconvuint.Text}.jpg";
                if (await RemoteFileExists(url) || await RemoteFileExists(url2))
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                    img.VerticalAlignment = VerticalAlignment.Stretch;
                    img.HorizontalAlignment = HorizontalAlignment.Stretch;
                    img.Style = (Style)FindResource("PopupImage");

                    try
                    {
                        using (WebClient web = new WebClient())
                        {
                            img.Source = byteArrayToImage((await web.DownloadDataTaskAsync(url)).ToArray());
                        }

                        ScreenMessageContainer.Children.Add(img);
                        ScreenMessage.Visibility = Visibility.Visible;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            using (WebClient web = new WebClient())
                            {
                                img.Source = byteArrayToImage((await web.DownloadDataTaskAsync(url2)).ToArray());
                            }

                            ScreenMessageContainer.Children.Add(img);
                            ScreenMessage.Visibility = Visibility.Visible;
                        }
                        catch (Exception)
                        {
                            displayScreenMessage("couldnt get Prop Image!");
                            ScreenMessage.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    displayScreenMessage("couldnt get Prop Image!");
                }
            }
        }
    }
}
