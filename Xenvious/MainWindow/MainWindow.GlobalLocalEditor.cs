using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of MainWindow: Global Editor page (a global or script local: read, write, its bits or text, the freeze lists).
    public partial class MainWindow
    {
        public ObservableCollection<GlobalFreezeer> GlobalFreezes { get; } = new ObservableCollection<GlobalFreezeer>();
        public ObservableCollection<LocalFreezeer> LocalFreezes { get; } = new ObservableCollection<LocalFreezeer>();

        /// <summary>Scripts last used on the Local card, newest first (config.ini "lelocalscripts").</summary>
        public List<string> RecentScripts { get; } = new List<string>();
        private const int RecentScriptCount = 5;

        private static readonly string[] ControllerScripts =
        {
            "fm_mission_controller", "fm_mission_controller_2020", "fm_mission_controller_v3", "public_mission_controller",
            "fm_deathmatch_controler", "fm_race_controler", "fm_survival_controller"
        };

        private bool _geReady, _geSync, _geBitsLocal, _geShowLocalFreezes;
        private DispatcherTimer _geTimer;
        private readonly ToggleButton[] _geBits = new ToggleButton[32];
        private (string Key, long Address, DateTime At) _geLocalAddress;

        // ----- page -----

        private void GeEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool visible = (bool)e.NewValue;
            if (visible && !_geReady)
            {
                _geReady = true;
                BuildGeBits();
                freezeList.ItemsSource = GlobalFreezes;
                localfreezeList.ItemsSource = LocalFreezes;
                GlobalFreezes.CollectionChanged += (_, __) => ShowFreezeCounts();
                LocalFreezes.CollectionChanged += (_, __) => ShowFreezeCounts();
                Freezer.CountChanged += () => Dispatcher.BeginInvoke(new Action(ShowFreezeCounts));
                ShowFreezeList(true);
                _geTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                _geTimer.Tick += (_, __) => RefreshGeBits();
                ApplyValueBoxMode(tbgeglobalvalue, GeKind(false));
                ApplyValueBoxMode(tblelocalvalue, GeKind(true));
            }
            if (_geTimer == null)
                return;
            if (visible)
            {
                _geTimer.Start();
                RefreshGeBits();
            }
            else
                _geTimer.Stop();
        }

        private EditorKind GeKind(bool local)
        {
            int i = (local ? ddlechangemode : ddgechangemode).SelectedIndex;
            return i < 0 ? EditorKind.Int : (EditorKind)i;
        }

        private TextBox GeValueBox(bool local) => local ? tblelocalvalue : tbgeglobalvalue;

        private void GeType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_geReady)
                return;
            bool local = sender == ddlechangemode;
            ApplyValueBoxMode(GeValueBox(local), GeKind(local));
            if (local == _geBitsLocal)
                RefreshGeBits();
        }

        private void GeSource_Changed(object sender, TextChangedEventArgs e)
        {
            if (sender == tbgeglobal)
                UpdateGeSuggestions();
            if (_geReady && (sender == tblelocal) == _geBitsLocal)
                RefreshGeBits();
        }

        // ----- names from offsets.ini -----

        private bool _geTaking;

        /// <summary>Opens the list of offsets.ini names that match what is typed, while it is a name and not a path or number.</summary>
        private void UpdateGeSuggestions()
        {
            string text = tbgeglobal.Text.Trim();
            bool name = !_geTaking && tbgeglobal.IsKeyboardFocused && text.Any(char.IsLetter)
                && !text.StartsWith("Global_", StringComparison.OrdinalIgnoreCase);
            var hits = !name ? new List<GlobalSuggestion>() : BuildData.CurrentGlobalNames()
                .Select(n => (n, Score: SmartSearch.Score(text, n.Key, n.Path, n.Note)))
                .Where(h => h.Score > 0)
                .OrderByDescending(h => h.Score).ThenBy(h => h.n.Key.Length)
                .Take(8)
                .Select(h => new GlobalSuggestion(h.n.Key, h.n.Path, h.n.Note))
                .ToList();
            GeSuggestList.ItemsSource = hits;
            if (hits.Count > 0)
                GeSuggestList.SelectedIndex = 0;
            GeSuggestPopup.IsOpen = hits.Count > 0;
        }

        private void TakeGeSuggestion(GlobalSuggestion hit)
        {
            if (hit == null)
                return;
            _geTaking = true;
            tbgeglobal.Text = hit.Filled;
            tbgeglobal.CaretIndex = tbgeglobal.Text.Length;
            _geTaking = false;
            GeSuggestPopup.IsOpen = false;
        }

        private void tbgeglobal_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!GeSuggestPopup.IsOpen)
                return;
            int count = GeSuggestList.Items.Count;
            switch (e.Key)
            {
                case Key.Down:
                    GeSuggestList.SelectedIndex = (GeSuggestList.SelectedIndex + 1) % count;
                    break;
                case Key.Up:
                    GeSuggestList.SelectedIndex = (GeSuggestList.SelectedIndex - 1 + count) % count;
                    break;
                case Key.Enter:
                case Key.Tab:
                    TakeGeSuggestion(GeSuggestList.SelectedItem as GlobalSuggestion);
                    break;
                case Key.Escape:
                    GeSuggestPopup.IsOpen = false;
                    break;
                default:
                    return;
            }
            GeSuggestList.ScrollIntoView(GeSuggestList.SelectedItem);
            e.Handled = true;
        }

        private void GeSuggestList_Click(object sender, MouseButtonEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is GlobalSuggestion hit)
                TakeGeSuggestion(hit);
        }

        // A click into the list takes focus from the box too; the click itself still lands.
        private void tbgeglobal_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!GeSuggestList.IsKeyboardFocusWithin && !GeSuggestList.IsMouseOver)
                GeSuggestPopup.IsOpen = false;
        }

        // A string gets a value box of several lines.
        private static void ApplyValueBoxMode(TextBox box, EditorKind kind)
        {
            bool text = kind == EditorKind.String;
            box.AcceptsReturn = text;
            box.TextWrapping = text ? TextWrapping.Wrap : TextWrapping.NoWrap;
            box.Height = text ? double.NaN : 30;
            box.MinHeight = text ? 92 : 0;
            box.VerticalContentAlignment = text ? VerticalAlignment.Top : VerticalAlignment.Center;
        }

        // ----- addresses -----

        /// <summary>Address of the global or local on its card, 0 when there is none. Says why unless quiet.</summary>
        private long GeAddress(bool local, bool quiet)
        {
            if (!m.IsProcOpen)
                return 0;
            if (!local)
            {
                if (!GlobalPath.TryEvaluate(tbgeglobal.Text, out long index) || index <= 0)
                {
                    if (!quiet)
                        displayScreenMessage(TranslateOr("ge_need_global", "Enter a global first, as Global_N.f_M or a number."));
                    return 0;
                }
                return new Global(index).GetAddress();
            }

            string script = cbLocalScript.Text.Trim();
            if (script.Length == 0 || !GlobalPath.TryEvaluate(tblelocal.Text, out long localIndex))
            {
                if (!quiet)
                    displayScreenMessage(TranslateOr("ge_need_local", "Choose a script and enter a local first."));
                return 0;
            }
            // The live read asks twice a second; finding the thread walks the whole thread list.
            string key = script + "|" + localIndex.ToString(CultureInfo.InvariantCulture);
            if (_geLocalAddress.Key == key && (DateTime.UtcNow - _geLocalAddress.At).TotalSeconds < 2)
                return _geLocalAddress.Address;
            long[] thread = GTA.getLocalScriptAddy(script);
            long address = thread == null ? 0
                : m.memory(thread[0], new long[] { thread[1], GTA.Offsets.Editor.OFFSET_script_local_start, localIndex * 8 }).GetAddress();
            _geLocalAddress = (key, address, DateTime.UtcNow);
            if (address == 0 && !quiet)
                displayScreenMessage(string.Format(CultureInfo.CurrentCulture, TranslateOr("ge_no_script", "{0} is not running."), script));
            return address;
        }

        private static string Hex(long address) => address.ToString("X", CultureInfo.InvariantCulture);

        private byte[] GeRead(long address, EditorKind kind) => m.memory(Hex(address)).GetBytes(EditorValue.Size(kind));

        // ----- read, write, copy, freeze -----

        private void GeReadInto(bool local)
        {
            long address = GeAddress(local, false);
            if (address == 0)
                return;
            var kind = GeKind(local);
            GeValueBox(local).Text = EditorValue.Decode(kind, GeRead(address, kind));
            if (local)
                RememberScript(cbLocalScript.Text);
        }

        private void GeWrite(bool local)
        {
            byte[] bytes;
            try
            {
                bytes = EditorValue.Encode(GeKind(local), GeValueBox(local).Text);
            }
            catch (Exception e) when (e is FormatException || e is OverflowException)
            {
                // A value too big for the type (131072 as Short) threw out of mry and closed the app.
                displayScreenMessage(TranslateOr("ge_bad_value", "The value does not fit the type. Check the number and the type."));
                return;
            }
            long address = GeAddress(local, false);
            if (address == 0)
                return;
            m.memory(Hex(address)).SetBytes(bytes);
            if (local)
                RememberScript(cbLocalScript.Text);
            RefreshGeBits();
        }

        private void GeCopyAddress(bool local)
        {
            long address = GeAddress(local, false);
            if (address != 0)
                Clipboard.SetDataObject(Hex(address));
        }

        private async void GeAddFreeze(bool local)
        {
            var kind = GeKind(local);
            string text = GeValueBox(local).Text;
            try
            {
                EditorValue.Encode(kind, text);
            }
            catch (Exception e) when (e is FormatException || e is OverflowException)
            {
                displayScreenMessage(TranslateOr("ge_bad_value", "The value does not fit the type. Check the number and the type."));
                return;
            }
            string path = local ? tblelocal.Text.Trim() : tbgeglobal.Text.Trim();
            string script = cbLocalScript.Text.Trim();
            if (!GlobalPath.TryEvaluate(path, out long index) || (!local && index <= 0) || (local && script.Length == 0))
            {
                displayScreenMessage(local ? TranslateOr("ge_need_local", "Choose a script and enter a local first.")
                    : TranslateOr("ge_need_global", "Enter a global first, as Global_N.f_M or a number."));
                return;
            }

            var name = new TextBox { Style = (Style)FindResource("Watermark"), Height = 30, Text = local ? script + " · " + path : path };
            name.Loaded += (_, __) => { name.Focus(); name.SelectAll(); };
            bool ok = await ShowContentAsync(TranslateOr("ge_add2f", "Add to freeze list"), TranslateOr("ge_add_name", "Name of the entry:"), name,
                TranslateOr("ge_add", "Add"), TranslateOr("dialog_cancel", "Cancel"), 420);
            if (!ok)
                return;
            string label = name.Text.Trim().Length > 0 ? name.Text.Trim() : path;
            if (local)
            {
                LocalFreezes.Add(new LocalFreezeer(label, script, path, kind, text));
                UsageStats.Action("freeze.local");
            }
            else
            {
                GlobalFreezes.Add(new GlobalFreezeer(label, path, kind, text));
                UsageStats.Action("freeze.global");
            }
            ShowFreezeList(!local);
        }

        private void Btngereadglobal_Click(object sender, RoutedEventArgs e) => GeReadInto(false);
        private void Btngewriteglobal_Click(object sender, RoutedEventArgs e) => GeWrite(false);
        private void Btngecopyaddy_Click(object sender, RoutedEventArgs e) => GeCopyAddress(false);
        private void Btngeadd2f_Click(object sender, RoutedEventArgs e) => GeAddFreeze(false);
        private void Btnlereadglobal_Click(object sender, RoutedEventArgs e) => GeReadInto(true);
        private void Btnlewriteglobal_Click(object sender, RoutedEventArgs e) => GeWrite(true);
        private void Btnlecopyaddy_Click(object sender, RoutedEventArgs e) => GeCopyAddress(true);
        private void Btnleadd2f_Click(object sender, RoutedEventArgs e) => GeAddFreeze(true);

        // ----- bits and text -----

        // Four byte tiles like the Converter's, highest byte first; inside a byte the highest bit is on the left.
        private void BuildGeBits()
        {
            for (int b = 3; b >= 0; b--)
            {
                var cells = new UniformGrid { Columns = 8 };
                for (int i = b * 8 + 7; i >= b * 8; i--)
                {
                    var text = new TextBlock { TextAlignment = TextAlignment.Center, Text = i.ToString(CultureInfo.InvariantCulture), FontSize = 13, FontWeight = FontWeights.SemiBold };
                    var bit = new ToggleButton { Style = (Style)FindResource("ConvBit"), Content = text, Tag = i, Height = 32, ToolTip = "Bit " + i };
                    bit.Click += GeBit_Click;
                    _geBits[i] = bit;
                    cells.Children.Add(bit);
                }
                var tile = new StackPanel();
                var caption = new TextBlock { Text = $"Byte {b} · {b * 8}-{b * 8 + 7}", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 0, 4) };
                caption.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
                tile.Children.Add(caption);
                tile.Children.Add(cells);
                var frame = new Border { Child = tile, Margin = new Thickness(0, 0, b == 0 ? 0 : 8, 0), Padding = new Thickness(6), CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(1), Tag = b };
                frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                GeBitsPanel.Children.Add(frame);
            }
        }

        private void SetBitsSource(bool local)
        {
            if (_geBitsLocal == local)
                return;
            _geBitsLocal = local;
            BtnGeBitsGlobal.Style = (Style)FindResource(local ? "FormButton" : "FormButtonPrimary");
            BtnGeBitsLocal.Style = (Style)FindResource(local ? "FormButtonPrimary" : "FormButton");
            RefreshGeBits();
        }

        private void BtnGeBitsGlobal_Click(object sender, RoutedEventArgs e) => SetBitsSource(false);
        private void BtnGeBitsLocal_Click(object sender, RoutedEventArgs e) => SetBitsSource(true);

        // The Bits card follows the card being worked on.
        private void GeGlobalCard_Focus(object sender, KeyboardFocusChangedEventArgs e) => SetBitsSource(false);
        private void GeLocalCard_Focus(object sender, KeyboardFocusChangedEventArgs e) => SetBitsSource(true);

        /// <summary>Reads the value of the Bits card's source and shows its bits, or its text for a string.</summary>
        private void RefreshGeBits()
        {
            if (!_geReady)
                return;
            bool local = _geBitsLocal;
            var kind = GeKind(local);
            int width = EditorValue.Bits(kind);
            bool text = kind == EditorKind.String;
            GeBitsTitle.Text = text ? TranslateOr("ge_text", "Text") : "Bits";
            GeBitsPanel.Visibility = text ? Visibility.Collapsed : Visibility.Visible;
            GeBitsInfo.Visibility = text ? Visibility.Collapsed : Visibility.Visible;
            GeTextBox.Visibility = text ? Visibility.Visible : Visibility.Collapsed;
            GeBitsPath.Text = local ? cbLocalScript.Text.Trim() + " · " + tblelocal.Text.Trim() : tbgeglobal.Text.Trim();

            long address = GeAddress(local, true);
            byte[] bytes = null;
            if (address != 0)
            {
                try { bytes = GeRead(address, kind); } catch { }
            }
            GeBitsLive.Visibility = bytes != null && (width > 0 || text) ? Visibility.Visible : Visibility.Collapsed;

            if (text)
            {
                if (bytes != null && !GeTextBox.IsKeyboardFocusWithin)
                {
                    string now = EditorValue.Decode(kind, bytes);
                    if (GeTextBox.Text != now)
                    {
                        _geSync = true;
                        GeTextBox.Text = now;
                        _geSync = false;
                    }
                }
                GeBitsHint.Text = bytes == null ? GeNotFound(local)
                    : string.Format(CultureInfo.CurrentCulture, TranslateOr("ge_chars", "{0} characters. Typing here fills the value above; Write writes it."), GeTextBox.Text.Length);
                return;
            }

            ulong raw = bytes == null ? 0 : EditorValue.Raw(bytes, kind);
            GeBitsValue.Text = bytes == null ? "–" : EditorValue.Decode(kind, bytes);
            GeBitsType.Text = width > 0 ? string.Format(CultureInfo.CurrentCulture, "{0} · {1} Bits", kind, width) : kind.ToString();
            foreach (Border frame in GeBitsPanel.Children)
                frame.Opacity = width > 0 && (int)frame.Tag * 8 < width ? 1 : 0.35;
            for (int i = 0; i < 32; i++)
            {
                bool usable = bytes != null && i < width;
                _geBits[i].IsEnabled = usable;
                _geBits[i].IsChecked = usable && ((raw >> i) & 1) != 0;
            }
            GeBitsHint.Text = width == 0 ? TranslateOr("ge_bits_none", "Bits only for Byte, Short and Integer. Switch the type to use them.")
                : bytes == null ? GeNotFound(local)
                : TranslateOr("ge_bits_hint", "Click a bit: it is written at once. 0-based like the scripts and the Converter.");
        }

        private string GeNotFound(bool local) => !m.IsProcOpen ? TranslateOr("ge_no_game", "GTA is not open.")
            : local ? TranslateOr("ge_bits_nolocal", "No value: choose a running script and a local above.")
            : TranslateOr("ge_bits_noglobal", "No value: enter a global above.");

        private void GeBit_Click(object sender, RoutedEventArgs e)
        {
            bool local = _geBitsLocal;
            var kind = GeKind(local);
            int bit = (int)((ToggleButton)sender).Tag;
            long address = GeAddress(local, true);
            if (address == 0 || bit >= EditorValue.Bits(kind))
            {
                RefreshGeBits();
                return;
            }
            // Read again right before the write: the game may have changed other bits meanwhile.
            byte[] bytes = GeRead(address, kind);
            bytes[bit / 8] ^= (byte)(1 << (bit % 8));
            m.memory(Hex(address)).SetBytes(bytes);
            GeValueBox(local).Text = EditorValue.Decode(kind, bytes);
            if (local)
                RememberScript(cbLocalScript.Text);
            RefreshGeBits();
        }

        private void GeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_geSync || !_geReady)
                return;
            GeValueBox(_geBitsLocal).Text = GeTextBox.Text;
            GeBitsHint.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("ge_chars", "{0} characters. Typing here fills the value above; Write writes it."), GeTextBox.Text.Length);
        }

        // ----- script list -----

        public void LoadRecentScripts(string csv)
        {
            RecentScripts.Clear();
            foreach (string s in (csv ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                if (!RecentScripts.Contains(s.Trim()))
                    RecentScripts.Add(s.Trim());
        }

        private void RememberScript(string script)
        {
            script = (script ?? "").Trim().ToLowerInvariant();
            if (script.Length == 0)
                return;
            RecentScripts.Remove(script);
            RecentScripts.Insert(0, script);
            while (RecentScripts.Count > RecentScriptCount)
                RecentScripts.RemoveAt(RecentScripts.Count - 1);
        }

        // Last used, then the creators, the controllers and every script running now, each under a heading.
        private void cbLocalScript_DropDownOpened(object sender, EventArgs e)
        {
            string typed = cbLocalScript.Text;
            var running = new List<string>();
            if (m.IsProcOpen)
            {
                try { running = GTA.LiveScriptNames(); } catch { }
            }
            var shown = new HashSet<string>();
            cbLocalScript.Items.Clear();
            void Group(string heading, IEnumerable<string> names, bool unique)
            {
                var list = names.Where(n => !unique || !shown.Contains(n)).ToList();
                if (list.Count == 0)
                    return;
                if (cbLocalScript.Items.Count > 0)
                    cbLocalScript.Items.Add(new Separator());
                var head = new ComboBoxItem { Content = heading, IsEnabled = false, FontSize = 11, FontWeight = FontWeights.Bold };
                head.SetResourceReference(ForegroundProperty, "FaintTextBrush");
                cbLocalScript.Items.Add(head);
                foreach (string n in list)
                {
                    shown.Add(n);
                    cbLocalScript.Items.Add(new ComboBoxItem { Content = n, Tag = n });
                }
            }
            Group(TranslateOr("ge_sc_recent", "Last used"), RecentScripts, false);
            Group(TranslateOr("ge_sc_creators", "Creators"), GTA.CreatorScripts.Concat(new[] { "creator" }), false);
            Group(TranslateOr("ge_sc_controllers", "Controllers"), ControllerScripts, false);
            Group(TranslateOr("ge_sc_running", "Running now"), running, true);
            cbLocalScript.SelectedIndex = -1;
            cbLocalScript.Text = typed;
        }

        private void cbLocalScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLocalScript.SelectedItem is ComboBoxItem item && item.Tag is string name)
            {
                cbLocalScript.Text = name;
                if (_geBitsLocal)
                    RefreshGeBits();
            }
        }

        // ----- freeze lists -----

        // Global and local freeze list live in one card; the header buttons decide which is shown.
        private void ShowFreezeList(bool global)
        {
            _geShowLocalFreezes = !global;
            freezeList.Visibility = global ? Visibility.Visible : Visibility.Collapsed;
            localfreezeList.Visibility = global ? Visibility.Collapsed : Visibility.Visible;
            BtnFreezeListGlobal.Style = (Style)FindResource(global ? "FormButtonPrimary" : "FormButton");
            BtnFreezeListLocal.Style = (Style)FindResource(global ? "FormButton" : "FormButtonPrimary");
            ShowFreezeCounts();
        }

        private void ShowFreezeCounts()
        {
            BtnFreezeListGlobal.Content = TranslateOr("ge_global", "Global") + "  " + GlobalFreezes.Count;
            BtnFreezeListLocal.Content = TranslateOr("ge_local", "Local") + "  " + LocalFreezes.Count;
            int shown = _geShowLocalFreezes ? LocalFreezes.Count : GlobalFreezes.Count;
            GeFreezeEmpty.Visibility = shown == 0 ? Visibility.Visible : Visibility.Collapsed;
            GeFrozenCount.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("ge_frozen", "{0} frozen"), Freezer.Count);
        }

        private void BtnFreezeListGlobal_Click(object sender, RoutedEventArgs e) => ShowFreezeList(true);
        private void BtnFreezeListLocal_Click(object sender, RoutedEventArgs e) => ShowFreezeList(false);

        private void GeFreezeDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is FreezeItem item))
                return;
            item.IsFrozen = false;
            if (item is GlobalFreezeer g)
                GlobalFreezes.Remove(g);
            else if (item is LocalFreezeer l)
                LocalFreezes.Remove(l);
        }

        // A double-click puts the entry back into its card above.
        private void freezeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(freezeList.SelectedItem is GlobalFreezeer item) || e.OriginalSource is TextBox)
                return;
            ddgechangemode.SelectedIndex = (int)item.Kind;
            tbgeglobal.Text = item.GlobalName;
            tbgeglobalvalue.Text = item.Text;
            SetBitsSource(false);
        }

        private void localfreezeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(localfreezeList.SelectedItem is LocalFreezeer item) || e.OriginalSource is TextBox)
                return;
            ddlechangemode.SelectedIndex = (int)item.Kind;
            cbLocalScript.Text = item.Script;
            tblelocal.Text = item.LocalName;
            tblelocalvalue.Text = item.Text;
            SetBitsSource(true);
        }
    }

    /// <summary>A global of offsets.ini offered by the Global Editor while a name is typed.</summary>
    public sealed class GlobalSuggestion
    {
        public GlobalSuggestion(string key, string path, string note)
        {
            Key = key;
            Path = path;
            Detail = string.IsNullOrEmpty(note) ? path : path + " · " + note;
            // Every array index starts at 0; the user changes it after taking the path.
            Filled = System.Text.RegularExpressions.Regex.Replace(path, @"\[i\b", "[0");
            Index = GlobalPath.TryEvaluate(Filled, out long index) ? index.ToString(CultureInfo.InvariantCulture) : "";
        }

        public string Key { get; }
        public string Path { get; }
        public string Detail { get; }
        public string Filled { get; }
        public string Index { get; }
    }
}
