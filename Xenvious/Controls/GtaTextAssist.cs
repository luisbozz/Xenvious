using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Helpers for texts the game shows with its own codes (objective texts): a toolbar over the
    /// selection that wraps it in colour or font codes, and a picker on Ctrl+Space for icons and codes.
    /// The game counts every code character against the text's length limit.
    /// </summary>
    public static class GtaTextAssist
    {
        public enum Group { Icon, Colour, Format }

        public class Code
        {
            public string Text, Key, Name;
            public Group Group;
            public Color? Colour;
            public Code(Group group, string text, string key, string name, Color? colour = null)
            {
                Group = group; Text = text; Key = key; Name = name; Colour = colour;
            }
        }

        // The icons are characters the game's font draws as symbols; the Windows fonts show other glyphs.
        public static readonly Code[] Codes =
        {
            new Code(Group.Icon, "¦", "gt_verified", "Rockstar Verified icon"),
            new Code(Group.Icon, "‹", "gt_created", "Rockstar Created icon"),
            new Code(Group.Icon, "÷", "gt_rockstar", "Rockstar icon"),
            new Code(Group.Icon, "∑", "gt_rockstar2", "Rockstar icon 2"),
            new Code(Group.Icon, "›", "gt_blank", "Blank white icon"),
            new Code(Group.Icon, "Ω", "gt_lock", "Lock icon"),
            new Code(Group.Icon, "~ws~", "gt_star", "Wanted star"),
            new Code(Group.Colour, "~r~", "gt_red", "Red", Color.FromRgb(0xE0, 0x60, 0x5A)),
            new Code(Group.Colour, "~o~", "gt_orange", "Orange", Color.FromRgb(0xFA, 0xA6, 0x1A)),
            new Code(Group.Colour, "~y~", "gt_yellow", "Yellow", Color.FromRgb(0xF0, 0xC8, 0x50)),
            new Code(Group.Colour, "~g~", "gt_green", "Green", Color.FromRgb(0x43, 0xB5, 0x81)),
            new Code(Group.Colour, "~b~", "gt_blue", "Blue", Color.FromRgb(0x5B, 0x9B, 0xE6)),
            new Code(Group.Colour, "~f~", "gt_lightblue", "Light blue", Color.FromRgb(0x8E, 0xC5, 0xFF)),
            new Code(Group.Colour, "~d~", "gt_darkblue", "Dark blue", Color.FromRgb(0x2F, 0x5C, 0xA8)),
            new Code(Group.Colour, "~p~", "gt_purple", "Purple", Color.FromRgb(0xB0, 0x8B, 0xE6)),
            new Code(Group.Colour, "~q~", "gt_pink", "Pink", Color.FromRgb(0xE0, 0x7B, 0xC4)),
            new Code(Group.Colour, "~c~", "gt_grey", "Grey", Color.FromRgb(0x9B, 0x9B, 0x9B)),
            new Code(Group.Colour, "~m~", "gt_darkgrey", "Dark grey", Color.FromRgb(0x68, 0x68, 0x68)),
            new Code(Group.Colour, "~u~", "gt_black", "Black", Color.FromRgb(0x20, 0x20, 0x20)),
            new Code(Group.Colour, "~s~", "gt_white", "White (resets the colour)", Color.FromRgb(0xF2, 0xF2, 0xF2)),
            new Code(Group.Format, "~bold~", "gt_bold", "Bold"),
            new Code(Group.Format, "~italic~", "gt_italic", "Italic"),
            new Code(Group.Format, "~n~", "gt_newline", "New line"),
        };

        /// <summary>The colour a code switches to, for previews; null for ~s~ and codes without a colour.</summary>
        public static Color? ColourOf(string code)
        {
            if (code == "~s~" || code == "~w~") return null;
            var c = Codes.FirstOrDefault(x => x.Text == code && x.Group == Group.Colour);
            if (c != null) return c.Colour;
            switch (code)
            {
                case "~l~": case "~v~": return Color.FromRgb(0x20, 0x20, 0x20);
                case "~t~": return Color.FromRgb(0x9B, 0x9B, 0x9B);
                default: return null;
            }
        }

        /// <summary>A stand-in for an icon character, since the Windows fonts do not have the game's symbols.</summary>
        public static string IconPreview(string icon)
        {
            switch (icon)
            {
                case "¦": return "✔";
                case "‹": return "✪";
                case "÷": case "∑": return "R*";
                case "›": return "■";
                case "Ω": return "🔒";
                case "~ws~": return "★";
                default: return null;
            }
        }

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;
        private static string NameOf(Code c) => T(c.Key, c.Name);

        private static Brush Res(FrameworkElement at, string key, Brush fallback) => at.TryFindResource(key) as Brush ?? fallback;

        public static void Attach(TextBox box)
        {
            var state = new Assist(box);
            box.SelectionChanged += (_, __) => state.QueueToolbar();
            box.PreviewKeyDown += state.OnKey;
            box.LostKeyboardFocus += (_, __) => state.HideAll();
            box.Unloaded += (_, __) => state.HideAll();
            box.PreviewMouseLeftButtonDown += (_, __) => state.ClosePicker();
        }

        private class Assist
        {
            private readonly TextBox _box;
            private Popup _toolbar, _picker;
            private Border _toolbarFrame;
            private StackPanel _pickerList;
            private TextBlock _pickerFilter;
            private string _filter = "";
            private int _index;
            private List<Code> _shown = new List<Code>();

            public Assist(TextBox box) { _box = box; }

            private int Room => (_box.MaxLength > 0 ? _box.MaxLength : int.MaxValue) - _box.Text.Length;

            public void HideAll()
            {
                if (_toolbar != null) _toolbar.IsOpen = false;
                ClosePicker();
            }

            public void ClosePicker()
            {
                if (_picker != null) _picker.IsOpen = false;
            }

            // Waits for the selection to settle (a mouse drag fires many changes).
            public void QueueToolbar() => _box.Dispatcher.BeginInvoke(new Action(ShowToolbar), DispatcherPriority.Input);

            private Border Frame(UIElement child)
            {
                var b = new Border
                {
                    Child = child,
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(4),
                    Margin = new Thickness(8),
                    Background = Res(_box, "SectionBackgroundBrush", new SolidColorBrush(Color.FromRgb(0x1E, 0x1F, 0x22))),
                    BorderBrush = Res(_box, "LineBrush", Brushes.Gray),
                    Effect = new DropShadowEffect { BlurRadius = 12, ShadowDepth = 2, Opacity = 0.45 },
                };
                TextElement.SetForeground(b, Res(_box, "TextColor", Brushes.White));
                return b;
            }

            private Border Tool(UIElement content, string tip, Action click)
            {
                var hover = Res(_box, "HoverBackgroundBrush", new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)));
                var b = new Border
                {
                    Child = content,
                    Width = 30, Height = 30,
                    CornerRadius = new CornerRadius(5),
                    Background = Brushes.Transparent,
                    Cursor = Cursors.Hand,
                    ToolTip = tip,
                };
                b.MouseEnter += (_, __) => b.Background = hover;
                b.MouseLeave += (_, __) => b.Background = Brushes.Transparent;
                // Handled on mouse down so the text box keeps its selection and focus.
                b.PreviewMouseLeftButtonDown += (_, e) => { e.Handled = true; click(); };
                return b;
            }

            private static TextBlock Glyph(string text, FontWeight weight, FontStyle style = default(FontStyle), TextDecorationCollection deco = null) =>
                new TextBlock
                {
                    Text = text, FontSize = 15, FontWeight = weight, FontStyle = style == default(FontStyle) ? FontStyles.Normal : style,
                    TextDecorations = deco, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                };

            private void BuildToolbar()
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                row.Children.Add(Tool(Glyph("B", FontWeights.Bold), T("gt_bold", "Bold") + "  ~bold~", () => Wrap("~bold~", "~bold~")));
                row.Children.Add(Tool(Glyph("I", FontWeights.Normal, FontStyles.Italic), T("gt_italic", "Italic") + "  ~italic~", () => Wrap("~italic~", "~italic~")));
                row.Children.Add(ToolSeparator());
                foreach (var c in Codes.Where(x => x.Group == Group.Colour && x.Text != "~m~" && x.Text != "~u~" && x.Text != "~d~"))
                {
                    var dot = new Border
                    {
                        Width = 14, Height = 14, CornerRadius = new CornerRadius(7),
                        Background = new SolidColorBrush(c.Colour.Value),
                        BorderBrush = Res(_box, "LineBrush", Brushes.Gray), BorderThickness = new Thickness(1),
                        HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                    };
                    var code = c;
                    row.Children.Add(Tool(dot, NameOf(code) + "  " + code.Text, () => Wrap(code.Text, code.Text == "~s~" ? "" : "~s~")));
                }
                row.Children.Add(ToolSeparator());
                row.Children.Add(Tool(Glyph("⌫", FontWeights.Normal), T("gt_clear", "Remove codes from the selection"), ClearCodes));
                _toolbarFrame = Frame(row);
                _toolbar = new Popup
                {
                    Child = _toolbarFrame,
                    AllowsTransparency = true,
                    StaysOpen = true,
                    PlacementTarget = _box,
                    Placement = PlacementMode.Top,
                    PopupAnimation = PopupAnimation.Fade,
                };
            }

            private UIElement ToolSeparator() =>
                new Border { Width = 1, Margin = new Thickness(5, 6, 5, 6), Background = Res(_box, "LineBrush", Brushes.Gray) };

            private Rect SelectionRect()
            {
                var a = _box.GetRectFromCharacterIndex(_box.SelectionStart);
                var b = _box.GetRectFromCharacterIndex(_box.SelectionStart + _box.SelectionLength, true);
                if (a.IsEmpty) return new Rect(0, 0, _box.ActualWidth, _box.ActualHeight);
                double right = b.IsEmpty || b.Top > a.Top ? _box.ActualWidth : b.Right;
                return new Rect(a.Left, a.Top, Math.Max(1, right - a.Left), a.Height);
            }

            private void ShowToolbar()
            {
                if (!_box.IsKeyboardFocused || _box.SelectionLength == 0 || (_picker != null && _picker.IsOpen))
                {
                    if (_toolbar != null) _toolbar.IsOpen = false;
                    return;
                }
                if (_toolbar == null) BuildToolbar();
                // Popup.Top flips below the selection by itself when there is no room above.
                _toolbar.PlacementRectangle = SelectionRect();
                _toolbar.IsOpen = false;
                _toolbar.IsOpen = true;
            }

            private void Replace(int start, int length, string text, int caret, int selLength = 0)
            {
                _box.Select(start, length);
                _box.SelectedText = text;
                _box.Select(caret, selLength);
            }

            private void NoRoom()
            {
                var frame = _picker != null && _picker.IsOpen ? _picker.Child as Border : _toolbarFrame;
                if (frame == null) return;
                var normal = frame.BorderBrush;
                frame.BorderBrush = Res(_box, "BadBrush", Brushes.IndianRed);
                frame.ToolTip = string.Format(T("gt_no_room", "Not enough room: the text may have at most {0} characters, codes included."), _box.MaxLength);
                var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(900) };
                t.Tick += (_, __) => { t.Stop(); frame.BorderBrush = normal; };
                t.Start();
            }

            private void Wrap(string open, string close)
            {
                int start = _box.SelectionStart, len = _box.SelectionLength;
                if (len == 0) return;
                if (open.Length + close.Length > Room) { NoRoom(); return; }
                string inner = _box.SelectedText;
                Replace(start, len, open + inner + close, start + open.Length, inner.Length);
                ShowToolbar();
            }

            private void ClearCodes()
            {
                int start = _box.SelectionStart, len = _box.SelectionLength;
                string clean = Regex.Replace(_box.SelectedText, "~[A-Za-z_0-9]+~", "");
                // A code cut in half at the selection's edge stays, so nothing half-open is left behind.
                Replace(start, len, clean, start, clean.Length);
                ShowToolbar();
            }

            // ----- Ctrl+Space picker -----

            public void OnKey(object sender, KeyEventArgs e)
            {
                bool open = _picker != null && _picker.IsOpen;
                if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    if (open) ClosePicker(); else OpenPicker();
                    return;
                }
                if (!open) return;
                switch (e.Key)
                {
                    case Key.Escape: ClosePicker(); e.Handled = true; return;
                    case Key.Down: Move(1); e.Handled = true; return;
                    case Key.Up: Move(-1); e.Handled = true; return;
                    case Key.Enter:
                    case Key.Tab:
                        if (_shown.Count > 0) Insert(_shown[_index]);
                        e.Handled = true;
                        return;
                    case Key.Back:
                        if (_filter.Length == 0) ClosePicker();
                        else { _filter = _filter.Substring(0, _filter.Length - 1); Fill(); }
                        e.Handled = true;
                        return;
                    case Key.Left: case Key.Right: case Key.Home: case Key.End:
                        ClosePicker();
                        return;
                }
            }

            // Letters typed while the picker is open filter it instead of going into the text.
            private void OnTextInput(object sender, TextCompositionEventArgs e)
            {
                if (_picker == null || !_picker.IsOpen || string.IsNullOrEmpty(e.Text) || char.IsControl(e.Text[0])) return;
                e.Handled = true;
                _filter += e.Text;
                Fill();
            }

            private void OpenPicker()
            {
                if (_toolbar != null) _toolbar.IsOpen = false;
                if (_picker == null)
                {
                    _pickerList = new StackPanel();
                    _pickerFilter = new TextBlock { FontSize = 12, Margin = new Thickness(6, 2, 6, 6), Foreground = Res(_box, "MutedTextBrush", Brushes.Gray) };
                    var panel = new StackPanel { Width = 280 };
                    panel.Children.Add(_pickerFilter);
                    panel.Children.Add(new ScrollViewer { Content = _pickerList, MaxHeight = 320, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
                    _picker = new Popup
                    {
                        Child = Frame(panel),
                        AllowsTransparency = true,
                        StaysOpen = true,
                        PlacementTarget = _box,
                        Placement = PlacementMode.Bottom,
                    };
                    _box.PreviewTextInput += OnTextInput;
                }
                _filter = "";
                Fill();
                var caret = _box.GetRectFromCharacterIndex(_box.CaretIndex);
                _picker.PlacementRectangle = caret.IsEmpty ? new Rect(0, 0, 1, _box.ActualHeight) : caret;
                _picker.IsOpen = true;
            }

            private void Move(int by)
            {
                if (_shown.Count == 0) return;
                _index = (_index + by + _shown.Count) % _shown.Count;
                Highlight();
            }

            private void Fill()
            {
                string f = _filter.Trim();
                _shown = Codes.Where(c => f.Length == 0
                    || NameOf(c).IndexOf(f, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || c.Text.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                _index = 0;
                _pickerFilter.Text = f.Length == 0
                    ? T("gt_picker_hint", "Icons and codes · type to filter · Enter inserts")
                    : "🔍 " + f;
                _pickerList.Children.Clear();
                Group? last = null;
                foreach (var c in _shown)
                {
                    if (last != c.Group)
                    {
                        last = c.Group;
                        _pickerList.Children.Add(new TextBlock
                        {
                            Text = GroupName(c.Group).ToUpperInvariant(), FontSize = 10.5, FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(6, _pickerList.Children.Count == 0 ? 0 : 8, 6, 3),
                            Foreground = Res(_box, "FaintTextBrush", Brushes.Gray),
                        });
                    }
                    _pickerList.Children.Add(Row(c));
                }
                if (_shown.Count == 0)
                    _pickerList.Children.Add(new TextBlock { Text = T("gt_none", "Nothing found"), Margin = new Thickness(6), Foreground = Res(_box, "FaintTextBrush", Brushes.Gray) });
                Highlight();
            }

            private static string GroupName(Group g)
            {
                switch (g)
                {
                    case Group.Icon: return T("gt_icons", "Icons");
                    case Group.Colour: return T("gt_colours", "Colours");
                    default: return T("gt_format", "Font and lines");
                }
            }

            private Border Row(Code c)
            {
                UIElement preview;
                if (c.Colour.HasValue)
                    preview = new Border { Width = 14, Height = 14, CornerRadius = new CornerRadius(7), Background = new SolidColorBrush(c.Colour.Value), BorderBrush = Res(_box, "LineBrush", Brushes.Gray), BorderThickness = new Thickness(1) };
                else
                    preview = new TextBlock
                    {
                        Text = IconPreview(c.Text) ?? (c.Group == Group.Format ? (c.Text == "~bold~" ? "B" : c.Text == "~italic~" ? "I" : "↵") : c.Text),
                        FontWeight = c.Text == "~bold~" ? FontWeights.Bold : FontWeights.Normal,
                        FontStyle = c.Text == "~italic~" ? FontStyles.Italic : FontStyles.Normal,
                        FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center,
                    };
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                var holder = new Border { Child = preview, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                var name = new TextBlock { Text = NameOf(c), FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
                var code = new TextBlock { Text = c.Text, FontSize = 12, FontFamily = new FontFamily("Consolas"), VerticalAlignment = VerticalAlignment.Center, Foreground = Res(_box, "MutedTextBrush", Brushes.Gray), Margin = new Thickness(8, 0, 0, 0) };
                Grid.SetColumn(name, 1);
                Grid.SetColumn(code, 2);
                grid.Children.Add(holder);
                grid.Children.Add(name);
                grid.Children.Add(code);
                var row = new Border { Child = grid, Padding = new Thickness(6, 5, 8, 5), CornerRadius = new CornerRadius(5), Cursor = Cursors.Hand, Tag = c };
                row.MouseEnter += (_, __) => { _index = _shown.IndexOf(c); Highlight(); };
                row.PreviewMouseLeftButtonDown += (_, e) => { e.Handled = true; Insert(c); };
                return row;
            }

            private void Highlight()
            {
                var on = Res(_box, "SelectedBackgroundBrush", new SolidColorBrush(Color.FromArgb(0x40, 0x58, 0x65, 0xF2)));
                foreach (var row in _pickerList.Children.OfType<Border>())
                {
                    bool active = _shown.Count > 0 && ReferenceEquals(row.Tag, _shown[_index]);
                    row.Background = active ? on : Brushes.Transparent;
                    if (active) row.BringIntoView();
                }
            }

            private void Insert(Code c)
            {
                int start = _box.SelectionStart, len = _box.SelectionLength;
                string text = c.Text;
                // With a selection, a colour or font code wraps it like the toolbar does.
                string close = c.Group == Group.Colour && c.Text != "~s~" ? "~s~" : c.Text == "~bold~" || c.Text == "~italic~" ? c.Text : null;
                if (len > 0 && close != null)
                {
                    if (text.Length + close.Length > Room) { NoRoom(); return; }
                    string inner = _box.SelectedText;
                    Replace(start, len, text + inner + close, start + text.Length, inner.Length);
                }
                else
                {
                    if (text.Length - len > Room) { NoRoom(); return; }
                    Replace(start, len, text, start + text.Length);
                }
                ClosePicker();
            }
        }
    }
}
