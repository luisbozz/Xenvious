using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace Xenvious
{
    // Part of MainWindow: Shared input helpers: numeric fields, paste handlers, +/- buttons.
    public partial class MainWindow
    {
        public void LocationPasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)) && sender is TextBox)
            {
                string pastedText = (e.DataObject.GetData(typeof(string)) as string);
                string result = pastedText.Replace(".", ",").Replace("?", "");
                result = Regex.Replace(result, "[^0-9\\-\\,]+", "");
                List<int> indexes = result.Select((b, i) => b.Equals(',') ? i : -1).Where(i => i != -1).ToList();
                int count = indexes.Count();
                int rem = 0;
            check:
                if (count > 1)
                {
                    result = result.Remove(rem <= 0 ? indexes.ElementAt(rem) : indexes.ElementAt(rem) - rem, 1);
                    rem++;
                    count--;
                    goto check;
                }


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

        public unsafe void CaptureTextPasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)) && sender is TextBox)
            {
                string pastedText = (e.DataObject.GetData(typeof(string)) as string);
                var arr = Encoding.UTF8.GetBytes(pastedText);
                string result = Encoding.UTF8.GetString(arr, 0, 255);

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

        private void FormatTextForLocation(object sender, KeyEventArgs e)
        {


            DataObject.AddPastingHandler((DependencyObject)sender, new DataObjectPastingEventHandler(LocationPasteHandler));


            if (e.Key == Key.OemPeriod)
            {
                //handle the event and cancel the original key
                e.Handled = true;

                //get caret position
                int tbPos = (sender as TextBox).SelectionStart;

                //insert the new text at the caret position
                (sender as TextBox).Text = (sender as TextBox).Text.Insert(tbPos, ",");


                //replace the caret back to where it should be 
                //otherwise the insertion call above will reset the position
                (sender as TextBox).Select(tbPos + 1, 0);
            }

            if (char.IsDigit(GetCharFromKey(e.Key)) && ((sender as TextBox).Text.Count(x => Char.IsDigit(x)) > 7))
            {
                e.Handled = true;
            }

            if (GetCharFromKey(e.Key) == ',' && (sender as TextBox).Text.Contains(","))
            {
                // Stop more than one dot Char
                e.Handled = true;
            }
            else if (GetCharFromKey(e.Key) == ',' && (sender as TextBox).SelectionStart == 0)
            {
                // Stop first char as a dot input
                e.Handled = true;
            }
            else if (GetCharFromKey(e.Key) == '-' && (sender as TextBox).SelectionStart != 0)
            {
                // Stop first char as a dot input
                e.Handled = true;
            }
            else if (!char.IsControl(GetCharFromKey(e.Key)) && !char.IsDigit(GetCharFromKey(e.Key)) && GetCharFromKey(e.Key) != ',' && GetCharFromKey(e.Key) != '-')
            {
                // Stop allow other than digit and control
                e.Handled = true;
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                creatorRefresh();
            }
        }

        public static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

        private bool IsValidInt(string value, bool negativ = true)
        {
            if (value.Length > 0 && value != "" && value != "-")
            {
                List<char> value_chars = value.ToList();
                if (value_chars.Count(x => char.IsLetter(x)) == 0)
                {
                    int count = value_chars.Count(x => x == '-');
                    int count_digits = value_chars.Count(x => char.IsDigit(x));
                    if (count_digits > 10)
                    {
                        return false;
                    }
                    if (negativ)
                    {
                        if (count == 0)
                        {
                            return true;
                        }
                        else if (count == 1 && value_chars[0] == '-')
                        {
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        if (count == 0)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                return false;
            }
            return false;
        }

        private bool IsValidIntNew(string value)
        {
            bool negative = value.IndexOf('-') == 0;
            string tempvalue = string.Empty;

            if (negative)
                tempvalue = value.Substring(1, value.Length - 1);
            else
                tempvalue = value;

            List<char> chars = tempvalue.ToList();

            if (chars.Count(x => !char.IsDigit(x)) == 0 && chars.Count < 11)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Convert a string from one charset to another charset
        /// </summary>
        /// <param name="strText">source string</param>
        /// <param name="strSrcEncoding">original encoding name</param>
        /// <param name="strDestEncoding">dest encoding name</param>
        /// <returns></returns>
        public static String StringEncodingConvert(String strText, String strSrcEncoding, String strDestEncoding)
        {
            System.Text.Encoding srcEnc = System.Text.Encoding.GetEncoding(strSrcEncoding);
            System.Text.Encoding destEnc = System.Text.Encoding.GetEncoding(strDestEncoding);
            byte[] bData = srcEnc.GetBytes(strText);
            byte[] bResult = System.Text.Encoding.Convert(srcEnc, destEnc, bData);
            return destEnc.GetString(bResult);
        }


        public void ChangeValuePlusMinus(object sender, KeyEventArgs e, Global alloffset, long offset, long offsetnext)
        {
            bool plus = (e.Key == Key.OemPlus || e.Key == Key.Add);
            bool minus = (e.Key == Key.OemMinus || e.Key == Key.Subtract);
            bool all = Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (all)
                {
                    if (plus)
                    {
                        for (int i = 0; i < alloffset.Get<int>(); i++)
                        {
                            Global loc = new Global(offset + (i * offsetnext));
                            loc.SetFloat(loc.Get<float>() + incrementsize);
                        }
                        (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) + incrementsize).ToString();
                    }
                    else if (minus)
                    {
                        for (int i = 0; i < alloffset.Get<int>(); i++)
                        {
                            Global loc = new Global(offset + (i * offsetnext));
                            loc.SetFloat(loc.Get<float>() - incrementsize);
                        }
                        (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) - incrementsize).ToString();
                    }
                    return;
                }
                if (plus)
                {
                    (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) + incrementsize).ToString();
                }
                else if (minus)
                {
                    (sender as TextBox).Text = (float.Parse((sender as TextBox).Text) - incrementsize).ToString();
                }
            }
        }
    }
}
