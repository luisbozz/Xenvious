using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xenvious
{
    /// <summary>
    /// Zones › "Type settings": the options of the picked zone type as the Mission Creator offers
    /// them (ZoneTypeSettings), with their real names instead of raw bits. Hidden for types
    /// without options. The zones page calls Show with the zone and its type.
    /// </summary>
    public class ZoneTypeSettingsCard : Border
    {
        private readonly StackPanel _body = new StackPanel { Margin = new Thickness(14, 12, 14, 4) };
        private readonly TextBlock _title = new TextBlock();
        private ZoneTypeSettings.Setting[] _settings;
        private FrameworkElement[] _rows;
        private Action[] _readers;
        private int _zone = -1, _type = -1;
        private bool _sync;

        public ZoneTypeSettingsCard()
        {
            Visibility = Visibility.Collapsed;
            Loaded += (_, __) =>
            {
                if (Child != null)
                    return;
                Style = (Style)FindResource("DashCard");
                var header = new Border { Style = (Style)FindResource("DashCardHeader") };
                _title.Style = (Style)FindResource("DashCardTitle");
                _title.SetBinding(TextBlock.TextProperty, Text("zs_card", "Type settings"));
                header.Child = _title;
                var dock = new DockPanel();
                DockPanel.SetDock(header, Dock.Top);
                dock.Children.Add(header);
                dock.Children.Add(_body);
                Child = dock;
            };
        }

        private static Binding Text(string key, string fallback) => new Binding($"Translation[{key}]") { FallbackValue = fallback };

        /// <summary>Shows the options of <paramref name="type"/> for zone <paramref name="zone"/> and reads their values.</summary>
        public void Show(int zone, int type)
        {
            var settings = zone < 0 ? null : ZoneTypeSettings.For(type);
            Visibility = settings == null ? Visibility.Collapsed : Visibility.Visible;
            if (settings == null || Child == null)
            {
                _type = -1;
                return;
            }
            if (type != _type || zone != _zone)
            {
                _type = type;
                _zone = zone;
                Build(settings);
            }
            Read();
        }

        private void Build(ZoneTypeSettings.Setting[] settings)
        {
            _settings = settings;
            _rows = new FrameworkElement[settings.Length];
            _readers = new Action[settings.Length];
            _body.Children.Clear();
            WrapPanel chips = null;
            for (int i = 0; i < settings.Length; i++)
            {
                var s = settings[i];
                if (s.Kind == ZoneTypeSettings.Kind.Bit)
                {
                    // Switches in a row of chips, as the other pages show bit options.
                    if (chips == null)
                    {
                        chips = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
                        _body.Children.Add(chips);
                    }
                    var box = new CheckBox { Style = (Style)FindResource("ChipToggle") };
                    box.SetBinding(ContentControl.ContentProperty, Text(s.Key, s.Fallback));
                    if (s.TipKey != null)
                        box.SetBinding(ToolTipProperty, Text(s.TipKey, s.TipFallback));
                    box.Click += (_, __) => WriteBit(s, box.IsChecked == true);
                    _readers[i] = () => box.IsChecked = ReadBit(s);
                    _rows[i] = box;
                    chips.Children.Add(box);
                    continue;
                }
                chips = null;
                var row = new StackPanel();
                var label = new TextBlock { Style = (Style)FindResource("FieldLabel") };
                label.SetBinding(TextBlock.TextProperty, Text(s.Key, s.Fallback));
                row.Children.Add(label);
                switch (s.Kind)
                {
                    case ZoneTypeSettings.Kind.Choice:
                        var pick = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                        foreach (var (value, key, fallback) in s.Options)
                        {
                            var item = new ComboBoxItem { Tag = value };
                            item.SetBinding(ContentControl.ContentProperty, Text(key, fallback));
                            pick.Items.Add(item);
                        }
                        pick.SelectionChanged += (_, __) =>
                        {
                            if (!_sync && pick.SelectedItem is ComboBoxItem item)
                                WriteInt(s, (int)item.Tag);
                        };
                        _readers[i] = () =>
                        {
                            if (pick.IsDropDownOpen)
                                return;
                            int value = ReadInt(s);
                            pick.SelectedIndex = Array.FindIndex(s.Options, o => o.Value == value);
                        };
                        row.Children.Add(pick);
                        break;
                    case ZoneTypeSettings.Kind.Slider:
                        var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                        var slider = new Slider { Minimum = s.Min, Maximum = s.Max, SmallChange = s.Step, LargeChange = s.Step, TickFrequency = s.Step, IsSnapToTickEnabled = true, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
                        var shown = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right, FontWeight = FontWeights.SemiBold };
                        shown.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                        Grid.SetColumn(shown, 1);
                        grid.Children.Add(slider);
                        grid.Children.Add(shown);
                        slider.ValueChanged += (_, __) =>
                        {
                            shown.Text = slider.Value.ToString("0.#", CultureInfo.CurrentCulture) + " " + s.Unit;
                            if (!_sync)
                                WriteFloat(s, slider.Value);
                        };
                        _readers[i] = () =>
                        {
                            if (slider.IsMouseCaptureWithin || slider.IsKeyboardFocusWithin)
                                return;
                            slider.Value = Math.Max(s.Min, Math.Min(s.Max, ReadShown(s)));
                            shown.Text = slider.Value.ToString("0.#", CultureInfo.CurrentCulture) + " " + s.Unit;
                        };
                        row.Children.Add(grid);
                        break;
                    case ZoneTypeSettings.Kind.Number:
                        var number = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
                        for (int n = (int)s.Min; n <= (int)s.Max; n++)
                            number.Items.Add(new ComboBoxItem { Tag = n, Content = n.ToString(CultureInfo.InvariantCulture) });
                        number.SelectionChanged += (_, __) =>
                        {
                            if (!_sync && number.SelectedItem is ComboBoxItem item)
                                WriteInt(s, (int)item.Tag);
                        };
                        _readers[i] = () =>
                        {
                            if (!number.IsDropDownOpen)
                                number.SelectedIndex = ReadInt(s) - (int)s.Min;
                        };
                        row.Children.Add(number);
                        break;
                }
                _rows[i] = row;
                _body.Children.Add(row);
            }
        }

        private void Read()
        {
            if (_settings == null || !MainWindow.m.IsProcOpen)
                return;
            _sync = true;
            try
            {
                for (int i = 0; i < _settings.Length; i++)
                {
                    var when = _settings[i].When;
                    _rows[i].Visibility = when == null || ReadBit(new ZoneTypeSettings.Setting { Field = ZoneTypeSettings.Field.Znbs3, Bit = when.Value.Bit }) == when.Value.Set
                        ? Visibility.Visible : Visibility.Collapsed;
                    _readers[i]();
                }
            }
            finally
            {
                _sync = false;
            }
        }

        private long At(ZoneTypeSettings.Setting s) => ZoneTypeSettings.Address(s.Field, _zone);

        private bool ReadBit(ZoneTypeSettings.Setting s)
        {
            long at = At(s);
            return at != 0 && Functions.Read.checkbinary(s.Bit, at);
        }

        private void WriteBit(ZoneTypeSettings.Setting s, bool on)
        {
            long at = At(s);
            if (at != 0 && MainWindow.m.IsProcOpen)
                Functions.Write.writebinary(s.Bit, at, on);
            Read(); // a switch can show or hide other rows (air drop-off)
        }

        // Value1 (znwd) is a float; the creator keeps whole numbers in it for stars.
        private int ReadInt(ZoneTypeSettings.Setting s)
        {
            long at = At(s);
            if (at == 0)
                return -1;
            return s.Field == ZoneTypeSettings.Field.Value1 ? (int)Math.Floor(new Global(at).Get<float>()) : new Global(at).Get<int>();
        }

        private void WriteInt(ZoneTypeSettings.Setting s, int value)
        {
            long at = At(s);
            if (at == 0 || !MainWindow.m.IsProcOpen)
                return;
            if (s.Field == ZoneTypeSettings.Field.Value1)
                new Global(at).SetFloat(value);
            else
                new Global(at).SetInt(value);
        }

        private double ReadShown(ZoneTypeSettings.Setting s)
        {
            long at = At(s);
            double raw = at == 0 ? 0 : new Global(at).Get<float>();
            return s.Invert ? 100 - raw : raw * s.Scale;
        }

        private void WriteFloat(ZoneTypeSettings.Setting s, double shown)
        {
            long at = At(s);
            if (at == 0 || !MainWindow.m.IsProcOpen)
                return;
            new Global(at).SetFloat((float)(s.Invert ? 100 - shown : shown / s.Scale));
        }
    }
}
