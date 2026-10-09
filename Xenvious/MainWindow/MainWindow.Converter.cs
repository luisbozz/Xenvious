using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xenvious
{
    // Part of MainWindow: Converter page (one input, the conversions, bits as byte tiles, recognised model).
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

        private bool _convSync;
        private CatalogItem _convHit;
        private readonly ToggleButton[] _convBits = new ToggleButton[32];

        // Four byte tiles, highest byte first like the binary text; inside a byte the highest bit
        // is on the left. Big number: Xenvious bit (1-based, as checkbinary/writebinary use it),
        // small number: the bit index the scripts use (0-based).
        private void BuildConverterBits()
        {
            if (_convBits[0] != null)
                return;
            for (int b = 3; b >= 0; b--)
            {
                var cells = new UniformGrid { Columns = 8 };
                for (int i = b * 8 + 7; i >= b * 8; i--)
                {
                    var text = new TextBlock { TextAlignment = TextAlignment.Center, LineHeight = 13 };
                    text.Inlines.Add(new System.Windows.Documents.Run((i + 1).ToString()) { FontSize = 13, FontWeight = FontWeights.SemiBold });
                    text.Inlines.Add(new System.Windows.Documents.LineBreak());
                    text.Inlines.Add(new System.Windows.Documents.Run(i.ToString()) { FontSize = 10 });
                    var bit = new ToggleButton { Style = (Style)ConverterMain.FindResource("ConvBit"), Content = text, Tag = i, ToolTip = $"Xenvious {i + 1} · Script {i}" };
                    bit.Click += ConvBit_Click;
                    _convBits[i] = bit;
                    cells.Children.Add(bit);
                }
                var tile = new StackPanel();
                var caption = new TextBlock { Text = $"Byte {b} · {b * 8 + 1}-{b * 8 + 8}", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 0, 4) };
                caption.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
                tile.Children.Add(caption);
                tile.Children.Add(cells);
                // Resource references, so a theme change recolours the tiles too.
                var frame = new Border { Child = tile, Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(6), CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(1) };
                frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                ConvBitsPanel.Children.Add(frame);
            }
        }

        private uint ConverterValue()
        {
            uint.TryParse(tbconvuint.Text, NumberStyles.None, CultureInfo.InvariantCulture, out uint v);
            return v;
        }

        private void ConvBit_Click(object sender, RoutedEventArgs e)
        {
            int bit = (int)((ToggleButton)sender).Tag;
            SetConverterValue(ConverterValue() ^ (1u << bit), null);
        }

        /// <summary>Fills every field except the one being typed in, the bits and the recognised model.</summary>
        private void SetConverterValue(uint v, TextBox source)
        {
            BuildConverterBits();
            _convHit = FindConverterModel(v);
            _convSync = true;
            try
            {
                if (source != tbconvnative)
                    tbconvnative.Text = _convHit?.Native ?? v.ToString(CultureInfo.InvariantCulture);
                if (source != tbconvint)
                    tbconvint.Text = unchecked((int)v).ToString(CultureInfo.InvariantCulture);
                if (source != tbconvuint)
                    tbconvuint.Text = v.ToString(CultureInfo.InvariantCulture);
                if (source != tbconvhex)
                    tbconvhex.Text = v.ToString("X8", CultureInfo.InvariantCulture);
                if (source != tbconvbinary)
                    tbconvbinary.Text = Regex.Replace(Convert.ToString(unchecked((int)v), 2).PadLeft(32, '0'), "(.{8})(?!$)", "$1 ");
            }
            finally
            {
                _convSync = false;
            }

            var set = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                bool on = (v & (1u << i)) != 0;
                _convBits[i].IsChecked = on;
                if (on)
                    set.Add(i + 1);
            }
            Lblconvbinarynums.Content = set.Count == 0 ? "" : TranslateOr("conv_setbits", "Set bits (Xenvious):") + " " + string.Join(", ", set);
            ShowConverterHit();
        }

        // Only our own lists know names: a hash cannot be turned back into one. Props first, the
        // creator uses them most; the other lists rarely share a hash with a prop.
        private CatalogItem FindConverterModel(uint v)
        {
            if (v == 0)
                return null;
            foreach (var list in new Func<List<CatalogItem>>[] { () => PropCatalog, () => VehicleCatalog, () => ActorCatalog, () => WeaponCatalog })
            {
                var hit = list().FirstOrDefault(i => i.Hash == v);
                if (hit != null)
                    return hit;
            }
            return null;
        }

        private void ShowConverterHit()
        {
            ConvHitImage.Source = null;
            ConvHitSource.Text = "";
            bool hit = _convHit != null;
            ConvHitPanel.Visibility = hit ? Visibility.Visible : Visibility.Collapsed;
            ConvHitNone.Visibility = hit ? Visibility.Collapsed : Visibility.Visible;
            if (!hit)
                return;
            ConvHitKind.Text = TranslateOr("conv_kind_" + _convHit.ImageKind, _convHit.ImageKind).ToUpperInvariant();
            ConvHitKind.Foreground = KindBrush(_convHit.ImageKind);
            ConvHitName.Text = _convHit.Name;
            // The picture loads by itself, a moment after typing stops (every key is a new hit).
            if (_convImageTimer == null)
            {
                _convImageTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
                _convImageTimer.Tick += (_, __) => { _convImageTimer.Stop(); Btncenvgetimg_Click(null, null); };
            }
            _convImageTimer.Stop();
            _convImageTimer.Start();
            ConvHitNative.Text = _convHit.Native ?? "";
            ConvHitCategory.Text = _convHit.Category;
        }

        // The colours props, vehicles, actors and weapons have everywhere (App.xaml).
        private static Brush KindBrush(string kind)
        {
            switch (kind)
            {
                case "vehicle": return ThemeBrush("VehicleBrush");
                case "actor": return ThemeBrush("ActorBrush");
                case "weapon": return ThemeBrush("WeaponBrush");
                default: return ThemeBrush("PropBrush");
            }
        }

        private void tbconvnative_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = tbconvnative.Text.Trim();
            if (_convSync || text.Length == 0)
                return;
            if (TryParseConverterInput(text, out uint v))
                SetConverterValue(v, tbconvnative);
        }

        // 0x.. is hex, 0b.. binary, a plain number int or uint, anything else a name that is hashed.
        private static bool TryParseConverterInput(string text, out uint v)
        {
            v = 0;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return uint.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v);
            if (text.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                return TryParseBinary(text.Substring(2), out v);
            if (Regex.IsMatch(text, @"^-?\d+$"))
            {
                if (uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out v))
                    return true;
                if (int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int i))
                {
                    v = unchecked((uint)i);
                    return true;
                }
                return false;
            }
            v = Functions.joaat(text);
            return true;
        }

        private static bool TryParseBinary(string text, out uint v)
        {
            v = 0;
            text = text.Replace(" ", "");
            if (text.Length == 0 || text.Length > 32 || text.Any(c => c != '0' && c != '1'))
                return false;
            v = Convert.ToUInt32(text, 2);
            return true;
        }

        private void tbconvhex_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = tbconvhex.Text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(2);
            if (!_convSync && uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint v))
                SetConverterValue(v, tbconvhex);
        }

        private void tbconvint_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_convSync && int.TryParse(tbconvint.Text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int v))
                SetConverterValue(unchecked((uint)v), tbconvint);
        }

        private void tbconvuint_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_convSync && uint.TryParse(tbconvuint.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out uint v))
                SetConverterValue(v, tbconvuint);
        }

        private void tbconvbinary_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_convSync && TryParseBinary(tbconvbinary.Text.Trim(), out uint v))
                SetConverterValue(v, tbconvbinary);
        }

        private void ConvCopy_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is TextBox box && box.Text.Length > 0)
            {
                try
                {
                    Clipboard.SetText(box.Text);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    // Another program holds the clipboard; the user can simply click again.
                }
            }
        }

        // Disk cache first, then the online source (only props have one so far).
        private System.Windows.Threading.DispatcherTimer _convImageTimer;

        private async void Btncenvgetimg_Click(object sender, RoutedEventArgs e)
        {
            var hit = _convHit;
            if (hit == null)
                return;
            ConvHitSource.Text = "…";
            var image = await ModelImageCache.GetAsync(hit.ImageKind, hit.Native, hit.Hash);
            if (hit != _convHit)
                return;
            ConvHitImage.Source = image;
            ConvHitSource.Text = image != null ? "" : TranslateOr("conv_nopic", "No picture: not in the cache and no source for this kind yet.");
        }

        private void BtnConvCatalog_Click(object sender, RoutedEventArgs e)
        {
            var hit = _convHit;
            if (hit == null)
                return;
            List<CatalogItem> items;
            string title;
            switch (hit.ImageKind)
            {
                case "vehicle": items = VehicleCatalog; title = TranslateOr("vehicles", "Vehicles"); break;
                case "actor": items = ActorCatalog; title = TranslateOr("actor", "Actors"); break;
                case "weapon": items = WeaponCatalog; title = TranslateOr("weapons", "Weapons"); break;
                default: items = PropCatalog; title = TranslateOr("prop", "Props"); break;
            }
            ModelCatalogOverlay.Show(title, items, hit.Hash, item => SetConverterValue(item.Hash, null));
        }
    }
}
