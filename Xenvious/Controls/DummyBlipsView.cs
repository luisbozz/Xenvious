using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// The free (dummy) blips of a Mission or LTS job (Global_4980736.f_218705[i /*122*/], count f_222366):
    /// map and list on the left, the picked blip's editor on the right.
    ///
    /// The mission controller (FM_Mission_Controller_HUD.sch) shows a dummy blip only when the player's
    /// team bit is set (bits 0-3) and the rule fits: iRule -2 (every rule), iRule n (that rule of the team
    /// in iTeam) or the window sTeamBlip[0] start..end (end -1 = to the end). The creator adds blips with
    /// iRule -1, iTeam -1 and no team bit, so they never show; new blips here start with team 1 and every
    /// rule. Type 3 or 5 with iEntityToUse hangs the blip on that vehicle or ped.
    /// </summary>
    public class DummyBlipsView : Grid
    {
        public const int Max = 56;

        // Team bits 0-3, then the ciDUMMY_BLIP_* bits the editor offers.
        private const int BitHideInVehicle = 5, BitGps = 6, BitHideWhenLinked = 9, BitShowWhenLinked = 10, BitHideInInterior = 11, BitNoHeight = 14;

        // ciFMMC_DROP_OFF_TYPE: cylinder and area sit on vPos, vehicle and ped follow iEntityToUse.
        private const int TypeCylinder = 0, TypeArea = 1, TypeProperty = 2, TypeVehicle = 3, TypeGarage = 4, TypePed = 5;

        private enum When { Always, One, Range, Never }

        private sealed class Blip
        {
            public int Index;
            public float X, Y, Z;
            public int Rule, Team, Type, Size, Entity, Colour, Sprite, Bits, From, To, LinkType, LinkIndex;
            public float ShowRange, HideRange;
            public string Name;

            public bool Follows => Type == TypeVehicle || Type == TypePed;
            public bool ForTeam(int t) => (Bits & (1 << t)) != 0;
            public bool AnyTeam => (Bits & 0xF) != 0;

            public When When
            {
                get
                {
                    if (Rule == -2 || From == -2) return When.Always;
                    if (From >= 0) return When.Range;
                    if (Rule >= 0) return When.One;
                    return When.Never;
                }
            }

            // IS_DUMMY_BLIP_WITHIN_SPECIFIED_RULES with the counting team on the given rule.
            public bool InRule(int rule)
            {
                if (From == -2 || Rule == -2) return true;
                if (From >= 0 && From <= rule && (To == -1 || To >= rule)) return true;
                return Rule >= 0 && Rule == rule;
            }

            public string Key => string.Join(",", X, Y, Z, Rule, Team, Type, Size, Entity, Colour, Sprite, Bits, From, To, LinkType, LinkIndex, ShowRange, HideRange, Name);
        }

        // GET_DUMMY_BLIP_SPRITE_FROM_SELECTION: 0 keeps the game's default sprite.
        private static readonly (int Id, string File, string Key, string Fallback)[] Sprites =
        {
            (0, "level", "bl_spr_default", "Default"), (1, "production_weed", "bl_spr_weed", "Weed"), (2, "production_crack", "bl_spr_crack", "Crack"),
            (3, "production_meth", "bl_spr_meth", "Meth"), (4, "laptop", "bl_spr_laptop", "Laptop"),
            (5, "target_a", "", "A"), (6, "target_b", "", "B"), (7, "target_c", "", "C"), (8, "target_d", "", "D"),
            (9, "target_e", "", "E"), (10, "target_f", "", "F"), (11, "target_g", "", "G"), (12, "target_h", "", "H"),
            (13, "numbered_1", "", "1"), (14, "numbered_2", "", "2"), (15, "numbered_3", "", "3"), (16, "numbered_4", "", "4"), (17, "numbered_5", "", "5"),
            (18, "numbered_6", "", "6"), (19, "numbered_7", "", "7"), (20, "numbered_8", "", "8"), (21, "numbered_9", "", "9"), (22, "numbered_10", "", "10"),
            (23, "elevator", "bl_spr_elevator", "Elevator"), (24, "stairs", "bl_spr_stairs", "Stairs"), (25, "testosterone", "bl_spr_testosterone", "Testosterone"),
        };

        // FMMC_BLIP_COLOUR_* as GET_BLIP_COLOUR_FROM_CREATOR maps them, in the radar's colours.
        private static readonly Dictionary<int, (string Key, string Fallback)> SpriteGroups = new Dictionary<int, (string, string)>
        {
            [0] = ("bl_grp_general", "General"), [5] = ("bl_grp_targets", "Targets"), [13] = ("bl_grp_numbers", "Numbers"), [23] = ("bl_grp_places", "Places"),
        };

        private static readonly (int Id, string Key, string Fallback, Color Colour)[] Colours =
        {
            (0, "bl_clr_default", "Default", C(0xFE, 0xFE, 0xFE)), (1, "bl_clr_red", "Red", C(0xE0, 0x32, 0x32)),
            (2, "bl_clr_darkblue", "Dark blue", C(0x2C, 0x6D, 0xB8)), (3, "bl_clr_lightblue", "Light blue", C(0x5D, 0xB6, 0xE5)),
            (4, "bl_clr_green", "Green", C(0x71, 0xCB, 0x71)), (5, "bl_clr_yellow", "Yellow", C(0xEE, 0xC6, 0x4E)),
            (6, "bl_clr_white", "White", C(0xFE, 0xFE, 0xFE)), (7, "bl_clr_black", "Black", C(0x30, 0x30, 0x30)),
            (8, "bl_clr_purple", "Purple", C(0x9C, 0x6E, 0xAF)), (9, "bl_clr_orange", "Orange", C(0xEA, 0x8E, 0x50)),
            (10, "bl_clr_blue", "Blue", C(0x5D, 0xB6, 0xE5)),
        };

        private readonly ComboBox _indexBox;
        private readonly Action _rebuild;
        private readonly SectionCard _mapCard = new SectionCard(), _listCard = new SectionCard(), _editCard = new SectionCard(), _linkCard = new SectionCard(), _lookCard = new SectionCard(), _visCard = new SectionCard();
        private readonly StackPanel _link = new StackPanel(), _look = new StackPanel(), _vis = new StackPanel();
        private readonly Canvas _map = new Canvas { Height = 320, ClipToBounds = true, Background = Brushes.Transparent, Cursor = Cursors.Cross };
        private readonly ComboBox _previewTeam = new ComboBox { Height = 28, MinWidth = 90, Margin = new Thickness(6, 0, 6, 0) };
        private readonly ComboBox _previewRule = new ComboBox { Height = 28, MinWidth = 160, MaxWidth = 280 };
        private readonly TextBlock _previewCount = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0), FontSize = 12.5 };
        private readonly StackPanel _rows = new StackPanel();
        private readonly StackPanel _editor = new StackPanel();
        private readonly Button _add = new Button { Height = 28, Padding = new Thickness(10, 0, 10, 0) };
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private List<Blip> _blips = new List<Blip>();
        private string _shown;
        private bool _rangeStart = true;
        private bool _previewSync;
        private double _cx, _cy, _scale;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;
        private static Color C(byte r, byte g, byte b) => Color.FromRgb(r, g, b);

        public DummyBlipsView(ComboBox indexBox, Action rebuild)
        {
            _indexBox = indexBox;
            _rebuild = rebuild;
            Margin = new Thickness(0, 12, 0, 12);
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 320 });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(540) });

            var left = new StackPanel();
            Card(_mapCard, T("bl_map", "Map"), "M3,6 L9,3 L15,6 L21,3 V18 L15,21 L9,18 L3,21 Z M9,3 V18 M15,6 V21");
            var previewBar = new DockPanel { Margin = new Thickness(0, 0, 0, 8), LastChildFill = false };
            previewBar.Children.Add(Faint(T("bl_preview", "In the test:"), 12.5).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(0); }));
            previewBar.Children.Add(_previewTeam);
            previewBar.Children.Add(_previewRule);
            previewBar.Children.Add(_previewCount);
            _previewCount.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            var frame = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Child = _map, ClipToBounds = true,
                ToolTip = T("bl_map_hint", "Every free blip from above, north up. Faded blips are hidden for the team and rule above. Click a blip to pick it, click the map to move a blip on a fixed spot there.") };
            frame.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            _mapCard.Content = new StackPanel { Children = { previewBar, frame } };
            left.Children.Add(_mapCard);

            Card(_listCard, T("bl_all", "All blips"), "M4,6 H20 M4,12 H20 M4,18 H20");
            _add.Content = T("bl_add", "+ New blip at cursor");
            _add.ToolTip = T("bl_add_hint", "Adds a blip at the creator's cursor, visible for team 1 on every rule.");
            _add.SetResourceReference(StyleProperty, "FormButtonPrimary");
            _add.Click += (_, __) => AddAtCursor();
            _listCard.HeaderRight = _add;
            _listCard.Content = _rows;
            left.Children.Add(_listCard);
            Children.Add(left);

            Card(_editCard, T("bl_blip", "Blip"), "M12,2 C8,2 5,5 5,9 C5,14 12,22 12,22 C12,22 19,14 19,9 C19,5 16,2 12,2 M12,6.5 A2.5,2.5 0 1 0 12,11.5 A2.5,2.5 0 1 0 12,6.5");
            _editCard.Content = _editor;
            Card(_linkCard, T("bl_link", "Tie to an entity"), "M10,14 L14,10 M8.5,11.5 L6,14 A3.2,3.2 0 0 0 10,18 L12.5,15.5 M11.5,8.5 L14,6 A3.2,3.2 0 0 1 18,10 L15.5,12.5");
            _linkCard.CanCollapse = true;
            _linkCard.Content = _link;
            Card(_lookCard, T("bl_look", "Look"), "M12,3 A9,9 0 1 0 12,21 C13.5,21 14,20 13.4,18.8 C12.8,17.6 13.6,16.5 15,16.5 H17 A4,4 0 0 0 21,12.5 C21,7.3 17,3 12,3 M7.5,11 A1.2,1.2 0 1 0 7.5,11.01 M10.5,7 A1.2,1.2 0 1 0 10.5,7.01 M15,7.5 A1.2,1.2 0 1 0 15,7.51");
            _lookCard.Content = _look;
            Card(_visCard, T("bl_visibility", "Visibility"), "M2,12 C5,6 19,6 22,12 C19,18 5,18 2,12 Z M12,9 A3,3 0 1 0 12,15 A3,3 0 1 0 12,9");
            _visCard.Content = _vis;
            var right = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { _editCard, _linkCard, _lookCard, _visCard } };
            SetColumn(right, 2);
            Children.Add(right);

            _map.SizeChanged += (_, __) => DrawMap();
            _map.MouseLeftButtonUp += MapClick;
            SatelliteTiles.TileLoaded += () => Dispatcher.BeginInvoke(new Action(() => { if (IsVisible) DrawMap(); }), DispatcherPriority.Background);
            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Refresh(false); };
            _indexBox.SelectionChanged += (_, __) => { _rangeStart = true; Refresh(true); };
            _previewTeam.SelectionChanged += (_, __) => { if (!_previewSync) { FillPreviewRules(); DrawMap(); } };
            _previewRule.SelectionChanged += (_, __) => { if (!_previewSync) DrawMap(); };
        }

        private static void Card(SectionCard card, string title, string icon)
        {
            card.Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            card.Title = title;
            card.Icon = Geometry.Parse(icon);
            card.Margin = new Thickness(0, 0, 0, 12);
        }

        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.ddblip.number != 0 && GTA.Offsets.Editor.ddblip.NEXT != 0 && GTA.Offsets.Editor.ddblip.pos != 0;
        private static long At(long field, int i) => field + i * GTA.Offsets.Editor.ddblip.NEXT;
        private static int Int(long field, int i, int fallback) => field == 0 ? fallback : new Global(At(field, i)).Get<int>();
        private static float Float(long field, int i) => field == 0 ? 0 : new Global(At(field, i)).Get<float>();

        private static void SetInt(long field, int i, int value)
        {
            if (field != 0 && Live)
                new Global(At(field, i)).SetInt(value);
        }

        private static void SetFloat(long field, int i, float value)
        {
            if (field != 0 && Live)
                new Global(At(field, i)).SetFloat(value);
        }

        private static Blip Read(int i)
        {
            var o = GTA.Offsets.Editor.ddblip.pos;
            return new Blip
            {
                Index = i,
                X = new Global(At(o, i)).Get<float>(),
                Y = new Global(At(o, i) + 1).Get<float>(),
                Z = new Global(At(o, i) + 2).Get<float>(),
                Rule = Int(GTA.Offsets.Editor.ddblip.rule, i, -1),
                Team = Int(GTA.Offsets.Editor.ddblip.team, i, -1),
                Type = Int(GTA.Offsets.Editor.ddblip.type, i, 0),
                Size = Int(GTA.Offsets.Editor.ddblip.size, i, 4),
                Entity = Int(GTA.Offsets.Editor.ddblip.veh, i, -1),
                Colour = Int(GTA.Offsets.Editor.ddblip.clr, i, 0),
                Sprite = Int(GTA.Offsets.Editor.ddblip.spri, i, 0),
                Bits = Int(GTA.Offsets.Editor.ddblip.bits, i, 0),
                From = Int(GTA.Offsets.Editor.ddblip.frul, i, -1),
                To = Int(GTA.Offsets.Editor.ddblip.trul, i, -1),
                LinkType = Int(GTA.Offsets.Editor.ddblip.entt, i, -1),
                LinkIndex = Int(GTA.Offsets.Editor.ddblip.enti, i, -1),
                ShowRange = Float(GTA.Offsets.Editor.ddblip.sbr, i),
                HideRange = Float(GTA.Offsets.Editor.ddblip.hbr, i),
                Name = GTA.Offsets.Editor.ddblip.dbnm == 0 ? "" : new Global(At(GTA.Offsets.Editor.ddblip.dbnm, i)).GetString(),
            };
        }

        private int Selected => _indexBox.SelectedIndex >= 0 && _indexBox.SelectedIndex < _blips.Count ? _indexBox.SelectedIndex : -1;

        private void Refresh(bool force)
        {
            if (!Live)
            {
                _shown = null;
                _blips.Clear();
                _rows.Children.Clear();
                _editor.Children.Clear();
                _map.Children.Clear();
                _add.IsEnabled = false;
                _rows.Children.Add(Faint(T("db_nocreator", "Open a job in the creator to see its blips."), 13));
                return;
            }
            int n = Math.Max(0, Math.Min(new Global(GTA.Offsets.Editor.ddblip.number).Get<int>(), Max));
            var blips = Enumerable.Range(0, n).Select(Read).ToList();
            string key = string.Join(";", blips.Select(b => b.Key)) + "|" + _indexBox.SelectedIndex + "|" + Rules.Teams();
            if (!force && key == _shown)
                return;
            _shown = key;
            _blips = blips;
            _listCard.Summary = string.Format(CultureInfo.CurrentCulture, "{0} / {1}", n, Max);
            _add.IsEnabled = n < Max;
            FillPreviewTeams();
            BuildList();
            BuildEditor();
            DrawMap();
        }

        // ---------- texts ----------

        private static string BlipName(Blip b)
            => string.IsNullOrWhiteSpace(b.Name) ? string.Format(CultureInfo.CurrentCulture, T("db_blip_n", "Blip {0}"), b.Index + 1) : (b.Index + 1) + " · " + b.Name;

        private static string RuleText(int team, int rule)
        {
            string text = Rules.Ready ? Regex.Replace(Rules.Text(team, rule) ?? "", "~[^~]*~", "").Trim() : "";
            string number = T("rl_rule_n", "Rule {0}").Replace("{0}", (rule + 1).ToString(CultureInfo.CurrentCulture));
            return text.Length == 0 ? number : number + ": " + text;
        }

        private static string RuleNumber(int rule) => T("rl_rule_n", "Rule {0}").Replace("{0}", (rule + 1).ToString(CultureInfo.CurrentCulture));

        private static string TeamsText(Blip b)
        {
            var teams = Enumerable.Range(0, 4).Where(b.ForTeam).Select(t => (t + 1).ToString(CultureInfo.CurrentCulture)).ToList();
            return teams.Count == 0 ? T("bl_noteam", "no team") : T("dash_team", "Team") + " " + string.Join(", ", teams);
        }

        private static string WhenText(Blip b)
        {
            string team = b.Team >= 0 && b.Team < 4 ? " (" + T("dash_team", "Team") + " " + (b.Team + 1) + ")" : "";
            switch (b.When)
            {
                case When.Always: return T("bl_when_always", "Every rule");
                case When.One: return RuleNumber(b.Rule) + team;
                case When.Range: return RuleNumber(b.From) + " – " + (b.To == -1 ? T("bl_to_end", "end") : RuleNumber(b.To)) + team;
                default: return T("bl_when_never", "Off");
            }
        }

        private static string WhereText(Blip b)
        {
            if (b.Type == TypeVehicle)
                return T("bl_follows", "follows") + " " + (b.Entity >= 0 ? EntityPicker.Label(EntityPicker.Vehicle, b.Entity) : "?");
            if (b.Type == TypePed)
                return T("bl_follows", "follows") + " " + (b.Entity >= 0 ? EntityPicker.Label(EntityPicker.Actor, b.Entity) : "?");
            return T("bl_fixed", "fixed spot");
        }

        // Why the blip will not show in a test, or null when it will.
        private static string Problem(Blip b)
        {
            if (!b.AnyTeam)
                return T("bl_p_noteam", "No team is ticked. The blip never shows.");
            if (b.When == When.Never)
                return T("bl_p_never", "\"When\" is off. The blip never shows.");
            if ((b.When == When.One || b.When == When.Range) && (b.Team < 0 || b.Team > 3))
                return T("bl_p_ruleteam", "No team counts the rules. Pick the team whose rule decides.");
            return null;
        }

        // ---------- sprites ----------

        private static readonly Dictionary<string, BitmapSource> SpriteBase = new Dictionary<string, BitmapSource>();
        private static readonly Dictionary<(int, int), BitmapSource> SpriteCache = new Dictionary<(int, int), BitmapSource>();

        /// <summary>
        /// The radar sprite in the blip colour. The pictures are white with a black outline; the game tints
        /// the white part, so every pixel is multiplied with the colour.
        /// </summary>
        private static BitmapSource Sprite(int sprite, int colour)
        {
            if (SpriteCache.TryGetValue((sprite, colour), out var done))
                return done;
            var s = Sprites.FirstOrDefault(x => x.Id == sprite);
            string file = s.File ?? "level";
            if (!SpriteBase.TryGetValue(file, out var source))
            {
                try
                {
                    var bmp = new BitmapImage(new Uri("pack://application:,,,/Images/blips/radar_" + file + ".png", UriKind.Absolute));
                    source = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
                    source.Freeze();
                }
                catch (Exception)
                {
                    source = null;
                }
                SpriteBase[file] = source;
            }
            if (source == null)
                return null;
            var c = Colours.FirstOrDefault(x => x.Id == colour).Colour;
            if (c.A == 0)
                c = Colours[0].Colour;
            int w = source.PixelWidth, h = source.PixelHeight, stride = w * 4;
            var px = new byte[stride * h];
            source.CopyPixels(px, stride, 0);
            for (int k = 0; k < px.Length; k += 4)
            {
                double l = (px[k] + px[k + 1] + px[k + 2]) / 765.0;
                px[k] = (byte)(c.B * l);
                px[k + 1] = (byte)(c.G * l);
                px[k + 2] = (byte)(c.R * l);
            }
            var tinted = BitmapSource.Create(w, h, source.DpiX, source.DpiY, PixelFormats.Bgra32, null, px, stride);
            tinted.Freeze();
            SpriteCache[(sprite, colour)] = tinted;
            return tinted;
        }

        // The pictures of the numbered sprites are bare circles and the elevator's is a flat square,
        // so the number and the arrows are drawn on top.
        private static FrameworkElement SpriteImage(int sprite, int colour, double size)
        {
            bool numbered = sprite >= 13 && sprite <= 22, elevator = sprite == 23;
            var image = new Image { Source = Sprite(numbered || elevator ? 0 : sprite, colour), Width = size, Height = size, Stretch = Stretch.Uniform };
            if (!numbered && !elevator)
                return image.Also(i => i.VerticalAlignment = VerticalAlignment.Center);
            var grid = new Grid { Width = size, Height = size, VerticalAlignment = VerticalAlignment.Center };
            grid.Children.Add(image);
            if (numbered)
                grid.Children.Add(new TextBlock { Text = (sprite - 12).ToString(CultureInfo.InvariantCulture), FontWeight = FontWeights.Black, FontSize = size * (sprite == 22 ? 0.4 : 0.5),
                    Foreground = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 0, size * 0.04) });
            else
                grid.Children.Add(new Path { Data = Geometry.Parse("M5,4.2 L7.6,7.4 H2.4 Z M5,15.8 L7.6,12.6 H2.4 Z"), Fill = Brushes.Black, Width = size * 0.42, Height = size * 0.6, Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
            return grid;
        }

        // ---------- list ----------

        private void BuildList()
        {
            _rows.Children.Clear();
            if (_blips.Count == 0)
                _rows.Children.Add(Faint(T("db_none", "No free blip yet."), 13));
            foreach (var b in _blips)
            {
                var button = new ToggleButton { IsChecked = b.Index == Selected, HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(0), Margin = new Thickness(0, 0, 0, 6), Cursor = Cursors.Hand };
                button.SetResourceReference(StyleProperty, "ChoiceTile");
                var grid = new Grid { Margin = new Thickness(10, 6, 10, 6) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.Children.Add(SpriteImage(b.Sprite, b.Colour, 22).Also(i => i.HorizontalAlignment = HorizontalAlignment.Left));
                var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                var title = new TextBlock { FontSize = 13.5, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, Text = BlipName(b) };
                title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                text.Children.Add(title);
                text.Children.Add(Faint(TeamsText(b) + " · " + WhenText(b) + " · " + WhereText(b), 11.5).Also(t => { t.TextWrapping = TextWrapping.NoWrap; t.TextTrimming = TextTrimming.CharacterEllipsis; t.Margin = new Thickness(0, 1, 0, 0); }));
                Grid.SetColumn(text, 1);
                grid.Children.Add(text);
                bool ok = Problem(b) == null;
                var pill = Pill(ok ? T("bl_active", "active") : T("bl_hidden", "never shown"), ok);
                Grid.SetColumn(pill, 2);
                grid.Children.Add(pill);
                button.Content = grid;
                int index = b.Index;
                button.Click += (_, __) => Pick(index);
                _rows.Children.Add(button);
            }
        }

        private static FrameworkElement Pill(string text, bool ok)
        {
            var b = new Border { CornerRadius = new CornerRadius(9), BorderThickness = new Thickness(1), Padding = new Thickness(8, 1, 8, 2), Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            b.BorderBrush = ok ? MainWindow.ThemeBrush("OkBrush") ?? Brushes.SeaGreen : MainWindow.ThemeBrush("WarnBrush") ?? Brushes.Goldenrod;
            var t = new TextBlock { Text = text, FontSize = 11.5, FontWeight = FontWeights.SemiBold, Foreground = b.BorderBrush };
            b.Child = t;
            return b;
        }

        private void Pick(int index)
        {
            if (index >= 0 && index < _indexBox.Items.Count)
                _indexBox.SelectedIndex = index;
            _rangeStart = true;
            Refresh(true);
        }

        // ---------- editor ----------

        private void BuildEditor()
        {
            _editor.Children.Clear();
            int i = Selected;
            if (i < 0)
            {
                _editCard.Title = T("bl_blip", "Blip");
                _editCard.HeaderRight = null;
                _linkCard.Visibility = _lookCard.Visibility = _visCard.Visibility = Visibility.Collapsed;
                _editor.Children.Add(Faint(_blips.Count == 0 ? T("bl_add_first", "Add a blip with \"+ New blip at cursor\".") : T("bl_pick", "Pick a blip in the list or on the map."), 13));
                return;
            }
            var b = _blips[i];
            _editCard.Title = BlipName(b);
            _linkCard.Visibility = _lookCard.Visibility = _visCard.Visibility = Visibility.Visible;
            _look.Children.Clear();
            _vis.Children.Clear();
            _link.Children.Clear();
            _link.Children.Add(LinkPanel(b));
            _linkCard.Summary = b.LinkType >= 1 && b.LinkType <= 3 && b.LinkIndex >= 0 ? EntityPicker.Label(b.LinkType, b.LinkIndex) : T("bl_link_none", "none");
            _editCard.HeaderRight = SpriteImage(b.Sprite, b.Colour, 24);

            string problem = Problem(b);
            _editor.Children.Add(Notice(problem ?? string.Format(CultureInfo.CurrentCulture, T("bl_ok", "Shows in the test for {0}: {1}."), TeamsText(b), WhenText(b)), problem == null));

            // Name
            _editor.Children.Add(Label(T("bl_name", "Name")));
            var name = new TextBox { Text = b.Name, Height = 30, VerticalContentAlignment = VerticalAlignment.Center, MaxLength = 23 };
            name.SetResourceReference(StyleProperty, "Watermark");
            void SaveName()
            {
                if (name.Text == b.Name || GTA.Offsets.Editor.ddblip.dbnm == 0 || !Live) return;
                new Global(At(GTA.Offsets.Editor.ddblip.dbnm, i)).SetString(name.Text);
                Refresh(true);
            }
            name.LostKeyboardFocus += (_, __) => SaveName();
            name.KeyDown += (_, e) => { if (e.Key == Key.Enter) SaveName(); };
            _editor.Children.Add(name);

            // Where
            _editor.Children.Add(Label(T("bl_where", "Where")));
            // GET_DUMMY_BLIP_LOCATION: own spot, the vehicle or ped in iEntityToUse, or the heist leader's
            // apartment entrance or garage; every other type falls through to the vehicle branch.
            int[] anchorTypes = { TypeCylinder, TypeVehicle, TypePed, TypeProperty, TypeGarage };
            int anchor = b.Type == TypeArea ? 0 : Array.IndexOf(anchorTypes, b.Type);
            _editor.Children.Add(Tiles(new[] { T("bl_fixed_spot", "Fixed spot"), T("bl_follow_veh", "Follows vehicle"), T("bl_follow_ped", "Follows actor"),
                T("bl_apartment", "Leader's apartment door"), T("bl_garage", "Leader's garage") }, anchor, a =>
            {
                if (a == anchor) return;
                SetInt(GTA.Offsets.Editor.ddblip.type, i, anchorTypes[a]);
                SetInt(GTA.Offsets.Editor.ddblip.veh, i, a == 1 || a == 2 ? Math.Max(0, b.Entity) : -1);
                Refresh(true);
            }, 3));
            if (b.Type == TypeProperty || b.Type == TypeGarage)
                _editor.Children.Add(Faint(T("bl_leader_hint", "Heists only: the blip sits at the entrance of the lobby leader's apartment or garage, not at the spot below."), 12));
            if (anchor < 0)
                _editor.Children.Add(Faint(string.Format(CultureInfo.CurrentCulture, T("bl_other_type", "Set in the creator as type {0}; pick one above to change it."), b.Type), 12));
            if (b.Follows)
            {
                int type = b.Type == TypeVehicle ? EntityPicker.Vehicle : EntityPicker.Actor;
                var list = new ComboBox { Height = 30, Margin = new Thickness(0, 6, 0, 0) };
                int count = Math.Max(0, EntityPicker.Count(type));
                for (int e = 0; e < count; e++)
                    list.Items.Add(new ComboBoxItem { Content = EntityPicker.Label(type, e), Tag = e });
                if (b.Entity < 0 || b.Entity >= count)
                    list.Items.Add(new ComboBoxItem { Content = b.Entity < 0 ? "—" : "#" + (b.Entity + 1) + " (" + T("ep_missing", "not in the job") + ")", Tag = b.Entity });
                list.SelectedItem = list.Items.OfType<ComboBoxItem>().FirstOrDefault(x => (int)x.Tag == b.Entity);
                list.SelectionChanged += (_, __) =>
                {
                    if (list.SelectedItem is ComboBoxItem item && (int)item.Tag != b.Entity)
                    {
                        SetInt(GTA.Offsets.Editor.ddblip.veh, i, (int)item.Tag);
                        Refresh(true);
                    }
                };
                _editor.Children.Add(list);
                _editor.Children.Add(Faint(b.Type == TypeVehicle
                    ? T("bl_follow_veh_hint", "The blip hangs on the vehicle and moves with it. When it is wrecked, the blip stays at the spot below.")
                    : T("bl_follow_ped_hint", "The blip hangs on the actor and moves with it. When the actor is dead, the blip stays at the spot below."), 12));
            }
            _editor.Children.Add(PositionRow(b));

            // Sprite
            var spriteName = Sprites.FirstOrDefault(s => s.Id == b.Sprite);
            _look.Children.Add(Label(T("bl_sprite", "Symbol") + "  ·  " + SpriteLabel(spriteName.Id)));
            var sprites = new StackPanel();
            WrapPanel row = null;
            foreach (var s in Sprites)
            {
                if (SpriteGroups.TryGetValue(s.Id, out var group))
                {
                    sprites.Children.Add(Faint(T(group.Key, group.Fallback), 12).Also(t => t.Margin = new Thickness(0, s.Id == 0 ? 0 : 4, 0, 4)));
                    row = new WrapPanel { Margin = new Thickness(0, 0, -6, 0) };
                    sprites.Children.Add(row);
                }
                var tile = new ToggleButton { Width = 42, Height = 42, Padding = new Thickness(0), Margin = new Thickness(0, 0, 6, 6), IsChecked = s.Id == b.Sprite, ToolTip = SpriteLabel(s.Id), Cursor = Cursors.Hand,
                    Content = SpriteImage(s.Id, b.Colour, 26) };
                tile.SetResourceReference(StyleProperty, "ChoiceTile");
                int id = s.Id;
                tile.Click += (_, __) => { SetInt(GTA.Offsets.Editor.ddblip.spri, i, id); Refresh(true); };
                row.Children.Add(tile);
            }
            _look.Children.Add(sprites);

            // Colour
            var colourName = Colours.FirstOrDefault(c => c.Id == b.Colour);
            _look.Children.Add(Label(T("bl_colour", "Colour") + (colourName.Key != null ? "  ·  " + T(colourName.Key, colourName.Fallback) : "")));
            var swatches = new WrapPanel();
            foreach (var c in Colours)
            {
                var swatch = new Border { Width = 26, Height = 26, CornerRadius = new CornerRadius(5), Margin = new Thickness(0, 0, 6, 6), Background = new SolidColorBrush(c.Colour),
                    BorderThickness = new Thickness(2), Cursor = Cursors.Hand, ToolTip = T(c.Key, c.Fallback) };
                if (c.Id == b.Colour)
                    swatch.SetResourceReference(Border.BorderBrushProperty, "TextColor");
                else
                    swatch.BorderBrush = new SolidColorBrush(Color.FromArgb(0x50, 0, 0, 0));
                if (c.Id == 0)
                    swatch.Child = new TextBlock { Text = "A", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                int id = c.Id;
                swatch.MouseLeftButtonUp += (_, __) => { SetInt(GTA.Offsets.Editor.ddblip.clr, i, id); Refresh(true); };
                swatches.Children.Add(swatch);
            }
            _look.Children.Add(swatches);

            // Size
            // GET_BLIP_SIZE_FROM_CREATOR: ped, object and pickup are BLIP_SIZE_* 0.7, vehicle and location 1.0.
            _look.Children.Add(Label(T("bl_size", "Size")));
            bool small = b.Size == 0 || b.Size == 1 || b.Size == 3;
            var sizes = new UniformGrid { Columns = 2, Rows = 1, Margin = new Thickness(0, 0, -6, 0) };
            foreach (var (big, key, fallback) in new[] { (false, "bl_size_small", "Small"), (true, "bl_size_normal", "Normal") })
            {
                var content = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                content.Children.Add(new Grid { Width = 30, Height = 30, Children = { SpriteImage(b.Sprite, b.Colour, big ? 28 : 20).Also(x => x.HorizontalAlignment = HorizontalAlignment.Center) } });
                content.Children.Add(new TextBlock { Text = T(key, fallback), FontSize = 12.5, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) });
                var tile = new ToggleButton { IsChecked = big != small, Margin = new Thickness(0, 0, 6, 6), Padding = new Thickness(4, 6, 4, 6), Cursor = Cursors.Hand, Content = content };
                tile.SetResourceReference(StyleProperty, "ChoiceTile");
                tile.Click += (_, __) => { SetInt(GTA.Offsets.Editor.ddblip.size, i, big ? 4 : 0); Refresh(true); };
                sizes.Children.Add(tile);
            }
            _look.Children.Add(sizes);
            _look.Children.Add(Faint(T("bl_size_hint", "The creator's sizes Ped, Object and Pickup draw the symbol at 70 %, Vehicle and Location at 100 %. The game has no other blip sizes here."), 12));

            // Teams
            _vis.Children.Add(Label(T("bl_who", "Who sees it")));
            _vis.Children.Add(TeamToggles(b));

            // When
            _vis.Children.Add(Label(T("bl_when", "When") + "  ·  " + WhenText(b)));
            var when = b.When;
            _vis.Children.Add(Tiles(new[] { T("bl_when_always", "Every rule"), T("bl_when_one", "One rule"), T("bl_when_range", "From – to"), T("bl_when_never", "Off") }, (int)when, w => SetWhen(b, (When)w)));
            if (when == When.One || when == When.Range)
                _vis.Children.Add(RulePanel(b));

            // Behaviour
            _vis.Children.Add(Label(T("bl_behaviour", "Behaviour")));
            _vis.Children.Add(BitCheck(b, BitGps, T("bl_gps", "GPS route to the blip")));
            _vis.Children.Add(BitCheck(b, BitHideInVehicle, T("bl_hide_vehicle", "Hide while in a vehicle")));
            _vis.Children.Add(BitCheck(b, BitHideInInterior, T("bl_hide_interior", "Hide inside buildings")));
            _vis.Children.Add(BitCheck(b, BitNoHeight, T("bl_no_height", "No height arrow")));

            // More
            var more = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
            more.Children.Add(SubLabel(T("bl_show_range", "Only visible within (m, 0 = always)")));
            more.Children.Add(FloatBox(b.ShowRange, v => SetFloat(GTA.Offsets.Editor.ddblip.sbr, i, v)));
            more.Children.Add(SubLabel(T("bl_hide_range", "Hide when closer than (m, 0 = never)")));
            more.Children.Add(FloatBox(b.HideRange, v => SetFloat(GTA.Offsets.Editor.ddblip.hbr, i, v)));
            var expander = new Expander { Header = T("bl_more_range", "More: visible range"), Content = more, Margin = new Thickness(0, 12, 0, 0), IsExpanded = b.ShowRange != 0 || b.HideRange != 0 };
            expander.SetResourceReference(Control.ForegroundProperty, "TextColor");
            _vis.Children.Add(expander);

            // Raw values
            var raw = new TextBlock { FontFamily = new FontFamily("Consolas"), FontSize = 11.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 0),
                Text = string.Format(CultureInfo.InvariantCulture, "rule {0}  team {1}  type {2}  veh {3}  size {4}  clr {5}  spri {6}  bits {7}  frul {8}  trul {9}  entt {10}  enti {11}",
                    b.Rule, b.Team, b.Type, b.Entity, b.Size, b.Colour, b.Sprite, b.Bits, b.From, b.To, b.LinkType, b.LinkIndex) };
            raw.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _vis.Children.Add(raw);
            foreach (var panel in new[] { _look, _vis })
                if (panel.Children.Count > 0 && panel.Children[0] is TextBlock first)
                    first.Margin = new Thickness(0, 0, 0, 7);
        }

        private static string SpriteLabel(int id)
        {
            var s = Sprites.FirstOrDefault(x => x.Id == id);
            if (id >= 5 && id <= 12) return T("bl_spr_target", "Target") + " " + s.Fallback;
            if (id >= 13 && id <= 22) return T("bl_spr_number", "Number") + " " + s.Fallback;
            return s.Key == null ? id.ToString(CultureInfo.InvariantCulture) : T(s.Key, s.Fallback);
        }

        private FrameworkElement PositionRow(Blip b)
        {
            int i = b.Index;
            var row = new Grid { Margin = new Thickness(0, 6, 0, 0) };
            for (int k = 0; k < 3; k++)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition());
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            }
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            float[] values = { b.X, b.Y, b.Z };
            for (int k = 0; k < 3; k++)
            {
                int axis = k;
                var box = new TextBox { Text = values[k].ToString("0.0", CultureInfo.InvariantCulture), Height = 30, VerticalContentAlignment = VerticalAlignment.Center, ToolTip = "XYZ"[k].ToString(), Tag = "XYZ"[k].ToString() };
                box.SetResourceReference(StyleProperty, "Watermark");
                void Save()
                {
                    if (!float.TryParse(box.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float v) || v == values[axis] || !Live) return;
                    new Global(At(GTA.Offsets.Editor.ddblip.pos, i) + axis).SetFloat(v);
                    Refresh(true);
                }
                box.LostKeyboardFocus += (_, __) => Save();
                box.KeyDown += (_, e) => { if (e.Key == Key.Enter) Save(); };
                Grid.SetColumn(box, k * 2);
                row.Children.Add(box);
            }
            var here = new Button { Content = T("bl_cursor", "Cursor"), Height = 30, Padding = new Thickness(10, 0, 10, 0), ToolTip = T("bl_cursor_hint", "Puts the blip where the creator's cursor is.") };
            here.SetResourceReference(StyleProperty, "FormButton");
            here.Click += (_, __) =>
            {
                if (!TryCursor(out float x, out float y, out float z)) return;
                long o = At(GTA.Offsets.Editor.ddblip.pos, i);
                new Global(o).SetFloat(x);
                new Global(o + 1).SetFloat(y);
                new Global(o + 2).SetFloat(z);
                _rebuild?.Invoke();
                Refresh(true);
            };
            Grid.SetColumn(here, 6);
            row.Children.Add(here);
            return row;
        }

        private FrameworkElement TeamToggles(Blip b)
        {
            int teams = Math.Max(Rules.Teams(), Enumerable.Range(0, 4).Where(b.ForTeam).Select(t => t + 1).DefaultIfEmpty(1).Max());
            var tabs = new StackPanel { Orientation = Orientation.Horizontal };
            for (int t = 0; t < teams; t++)
            {
                int team = t;
                var tab = new ToggleButton { Content = (b.ForTeam(t) ? "✓ " : "") + T("dash_team", "Team") + " " + (t + 1), IsChecked = b.ForTeam(t) };
                tab.SetResourceReference(StyleProperty, "NavTab");
                if (MainWindow.ThemeBrush("TeamBrush" + (t + 1)) is SolidColorBrush colour)
                {
                    tab.Resources["AccentBrush"] = colour;
                    tab.Resources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x70, colour.Color.R, colour.Color.G, colour.Color.B));
                }
                tab.Click += (_, __) =>
                {
                    int bits = b.Bits ^ (1 << team);
                    SetInt(GTA.Offsets.Editor.ddblip.bits, b.Index, bits);
                    // The controller reads the rule of iTeam; -1 would index its rule array out of range.
                    if ((bits & (1 << team)) != 0 && (b.Team < 0 || b.Team > 3))
                        SetInt(GTA.Offsets.Editor.ddblip.team, b.Index, team);
                    Refresh(true);
                };
                tabs.Children.Add(tab);
            }
            var box = new Border { Child = tabs, HorizontalAlignment = HorizontalAlignment.Left };
            box.SetResourceReference(StyleProperty, "NavGroup");
            return box;
        }

        private static int FirstTeam(Blip b)
        {
            int t = Enumerable.Range(0, 4).FirstOrDefault(b.ForTeam);
            return b.Team >= 0 && b.Team < 4 ? b.Team : t;
        }

        private void SetWhen(Blip b, When when)
        {
            int i = b.Index;
            int rule = -1, from = -1, to = -1;
            switch (when)
            {
                case When.Always: rule = -2; break;
                case When.One: rule = b.When == When.One ? b.Rule : Math.Max(0, b.From); break;
                case When.Range: from = b.When == When.Range ? b.From : Math.Max(0, b.Rule); to = b.When == When.Range ? b.To : -1; break;
            }
            SetInt(GTA.Offsets.Editor.ddblip.rule, i, rule);
            SetInt(GTA.Offsets.Editor.ddblip.frul, i, from);
            SetInt(GTA.Offsets.Editor.ddblip.trul, i, to);
            SetInt(GTA.Offsets.Editor.ddblip.team, i, FirstTeam(b));
            _rangeStart = true;
            Refresh(true);
        }

        // Which team's rule counts, then that team's rules; a click picks the rule, or start and end of the window.
        private FrameworkElement RulePanel(Blip b)
        {
            int i = b.Index;
            var panel = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
            var teamRow = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            teamRow.Children.Add(Faint(T("bl_rules_of", "Rules count of"), 12.5).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(0, 0, 8, 0); }));
            var teamBox = new ComboBox { Height = 28, Width = 110, HorizontalAlignment = HorizontalAlignment.Left };
            for (int t = 0; t < Math.Max(1, Rules.Teams()); t++)
                teamBox.Items.Add(T("dash_team", "Team") + " " + (t + 1));
            int team = b.Team >= 0 && b.Team < teamBox.Items.Count ? b.Team : 0;
            teamBox.SelectedIndex = team;
            teamBox.SelectionChanged += (_, __) => { if (teamBox.SelectedIndex >= 0 && teamBox.SelectedIndex != b.Team) { SetInt(GTA.Offsets.Editor.ddblip.team, i, teamBox.SelectedIndex); Refresh(true); } };
            teamRow.Children.Add(teamBox);
            panel.Children.Add(teamRow);

            int count = Rules.Ready ? Rules.Count(team) : 0;
            if (b.When == When.Range)
            {
                var end = new CheckBox { IsChecked = b.To == -1 };
                end.Click += (_, __) => { SetInt(GTA.Offsets.Editor.ddblip.trul, i, end.IsChecked == true ? -1 : Math.Max(b.From, Math.Min(count - 1, b.From))); Refresh(true); };
                panel.Children.Add(SwitchRow(T("bl_to_end_check", "Until the end"), end));
            }
            if (count == 0)
                panel.Children.Add(Faint(T("bl_norules", "This team has no rules yet."), 12));
            for (int r = 0; r < count; r++)
            {
                int rule = r;
                bool on = b.When == When.One ? b.Rule == r : r >= b.From && (b.To == -1 || r <= b.To);
                var row = new ToggleButton { IsChecked = on, HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 0, 0, 4), Cursor = Cursors.Hand,
                    Content = new TextBlock { Text = RuleText(team, r), TextTrimming = TextTrimming.CharacterEllipsis, FontSize = 12.5 } };
                row.SetResourceReference(StyleProperty, "ChoiceTile");
                row.Click += (_, __) =>
                {
                    if (b.When == When.One)
                        SetInt(GTA.Offsets.Editor.ddblip.rule, i, rule);
                    else if (_rangeStart)
                    {
                        SetInt(GTA.Offsets.Editor.ddblip.frul, i, rule);
                        if (b.To != -1) SetInt(GTA.Offsets.Editor.ddblip.trul, i, rule);
                        _rangeStart = b.To == -1;
                    }
                    else
                    {
                        SetInt(GTA.Offsets.Editor.ddblip.frul, i, Math.Min(b.From, rule));
                        SetInt(GTA.Offsets.Editor.ddblip.trul, i, Math.Max(b.From, rule));
                        _rangeStart = true;
                    }
                    Refresh(true);
                };
                panel.Children.Add(row);
            }
            if (b.When == When.Range && count > 0)
                panel.Children.Add(Faint(b.To == -1 ? T("bl_range_hint_end", "Click the rule the blip starts on.") : T("bl_range_hint", "First click: start rule, second click: end rule."), 12));
            return panel;
        }

        private FrameworkElement BitCheck(Blip b, int bit, string text)
        {
            var box = new CheckBox { IsChecked = (b.Bits & (1 << bit)) != 0 };
            box.Click += (_, __) =>
            {
                int bits = box.IsChecked == true ? b.Bits | (1 << bit) : b.Bits & ~(1 << bit);
                SetInt(GTA.Offsets.Editor.ddblip.bits, b.Index, bits);
                Refresh(true);
            };
            return SwitchRow(text, box);
        }

        private static FrameworkElement SwitchRow(string text, CheckBox box)
        {
            box.SetResourceReference(StyleProperty, "FormToggle");
            DockPanel.SetDock(box, Dock.Right);
            var row = new DockPanel { Margin = new Thickness(0, 2, 0, 6) };
            row.Children.Add(box);
            var t = new TextBlock { Text = text, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(t);
            return row;
        }

        private FrameworkElement FloatBox(float value, Action<float> write)
        {
            var box = new TextBox { Text = value.ToString("0.##", CultureInfo.InvariantCulture), Height = 30, Width = 120, HorizontalAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Center };
            box.SetResourceReference(StyleProperty, "Watermark");
            void Save()
            {
                if (!float.TryParse(box.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float v) || v == value || v < 0) return;
                write(v);
                Refresh(true);
            }
            box.LostKeyboardFocus += (_, __) => Save();
            box.KeyDown += (_, e) => { if (e.Key == Key.Enter) Save(); };
            return box;
        }

        // Entity link (iEntityType / iEntityIndex) with bit 9 (hide while it exists) or 10 (hide while it does not).
        // The controller only checks peds (alive), vehicles (driveable) and objects (exist); other types never exist.
        private FrameworkElement LinkPanel(Blip b)
        {
            int i = b.Index;
            var panel = new StackPanel();
            panel.Children.Add(Faint(T("bl_link_hint", "Shows or hides the blip by whether an actor is alive, a vehicle drivable or an object there. The blip keeps its own place; to move it along with an entity use \"Follows\" below."), 12).Also(t => t.Margin = new Thickness(0, 0, 0, 8)));
            int[] types = { EntityPicker.None, EntityPicker.Actor, EntityPicker.Vehicle, EntityPicker.Object };
            int type = b.LinkIndex >= 0 ? Array.IndexOf(types, b.LinkType) : 0;
            if (type < 0) type = 0;
            panel.Children.Add(Tiles(new[] { T("bl_link_none_tile", "None"), T("ep_actor", "Actor"), T("ep_vehicle", "Vehicle"), T("ep_object", "Object") }, type, t =>
            {
                SetInt(GTA.Offsets.Editor.ddblip.entt, i, t == 0 ? -1 : types[t]);
                SetInt(GTA.Offsets.Editor.ddblip.enti, i, t == 0 ? -1 : (types[t] == b.LinkType ? Math.Max(0, b.LinkIndex) : 0));
                int bits = b.Bits & ~(1 << BitHideWhenLinked) & ~(1 << BitShowWhenLinked);
                if (t != 0)
                    bits |= 1 << ((b.Bits & (1 << BitHideWhenLinked)) != 0 ? BitHideWhenLinked : BitShowWhenLinked);
                SetInt(GTA.Offsets.Editor.ddblip.bits, i, bits);
                Refresh(true);
            }));
            if (type == 0)
                return panel;
            int entityType = types[type];
            var list = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 6) };
            int count = Math.Max(0, EntityPicker.Count(entityType));
            for (int e = 0; e < count; e++)
                list.Items.Add(new ComboBoxItem { Content = EntityPicker.Label(entityType, e), Tag = e });
            if (b.LinkIndex >= count)
                list.Items.Add(new ComboBoxItem { Content = "#" + (b.LinkIndex + 1) + " (" + T("ep_missing", "not in the job") + ")", Tag = b.LinkIndex });
            list.SelectedItem = list.Items.OfType<ComboBoxItem>().FirstOrDefault(x => (int)x.Tag == b.LinkIndex);
            list.SelectionChanged += (_, __) =>
            {
                if (list.SelectedItem is ComboBoxItem item && (int)item.Tag != b.LinkIndex)
                {
                    SetInt(GTA.Offsets.Editor.ddblip.enti, i, (int)item.Tag);
                    Refresh(true);
                }
            };
            panel.Children.Add(list);
            int mode = (b.Bits & (1 << BitHideWhenLinked)) != 0 ? 1 : 0;
            panel.Children.Add(Tiles(new[] { T("bl_link_show", "Only while it exists"), T("bl_link_hide", "Hide while it exists") }, mode, m =>
            {
                int bits = b.Bits & ~(1 << BitHideWhenLinked) & ~(1 << BitShowWhenLinked);
                bits |= 1 << (m == 1 ? BitHideWhenLinked : BitShowWhenLinked);
                SetInt(GTA.Offsets.Editor.ddblip.bits, i, bits);
                Refresh(true);
            }));
            return panel;
        }

        // ---------- add and delete ----------

        private static bool TryCursor(out float x, out float y, out float z)
        {
            x = y = z = 0;
            if (!Live) return false;
            var at = Functions.Read.getlocation();
            return at != null && at.Count >= 3
                && float.TryParse(at[0], NumberStyles.Float, CultureInfo.CurrentCulture, out x)
                && float.TryParse(at[1], NumberStyles.Float, CultureInfo.CurrentCulture, out y)
                && float.TryParse(at[2], NumberStyles.Float, CultureInfo.CurrentCulture, out z)
                && !(x == 0 && y == 0 && z == 0);
        }

        /// <summary>A new blip at the cursor that shows for team 1 on every rule.</summary>
        public void AddAtCursor()
        {
            if (!Live)
                return;
            int n = new Global(GTA.Offsets.Editor.ddblip.number).Get<int>();
            if (n < 0 || n >= Max || !TryCursor(out float x, out float y, out float z))
                return;
            long o = At(GTA.Offsets.Editor.ddblip.pos, n);
            new Global(o).SetFloat(x);
            new Global(o + 1).SetFloat(y);
            new Global(o + 2).SetFloat(z);
            SetInt(GTA.Offsets.Editor.ddblip.type, n, TypeCylinder);
            SetInt(GTA.Offsets.Editor.ddblip.size, n, 4);
            SetInt(GTA.Offsets.Editor.ddblip.veh, n, -1);
            SetInt(GTA.Offsets.Editor.ddblip.rule, n, -2);
            SetInt(GTA.Offsets.Editor.ddblip.team, n, 0);
            SetInt(GTA.Offsets.Editor.ddblip.frul, n, -1);
            SetInt(GTA.Offsets.Editor.ddblip.trul, n, -1);
            SetInt(GTA.Offsets.Editor.ddblip.bits, n, 1);
            SetInt(GTA.Offsets.Editor.ddblip.entt, n, -1);
            SetInt(GTA.Offsets.Editor.ddblip.enti, n, -1);
            SetInt(GTA.Offsets.Editor.ddblip.clr, n, 0);
            SetInt(GTA.Offsets.Editor.ddblip.spri, n, 0);
            SetFloat(GTA.Offsets.Editor.ddblip.sbr, n, 0);
            SetFloat(GTA.Offsets.Editor.ddblip.hbr, n, 0);
            if (GTA.Offsets.Editor.ddblip.dbnm != 0)
                new Global(At(GTA.Offsets.Editor.ddblip.dbnm, n)).SetString("");
            new Global(GTA.Offsets.Editor.ddblip.number).SetInt(n + 1);
            _rebuild?.Invoke();
            // The index box only grows with the next worker pass.
            Dispatcher.BeginInvoke(new Action(() => Pick(n)), DispatcherPriority.ApplicationIdle);
            Refresh(true);
        }

        /// <summary>Removes the picked blip and moves the ones after it up, as the creator does.</summary>
        public void DeleteSelected()
        {
            int i = Selected;
            if (!Live || i < 0)
                return;
            int n = Math.Min(new Global(GTA.Offsets.Editor.ddblip.number).Get<int>(), Max);
            int next = (int)GTA.Offsets.Editor.ddblip.NEXT;
            long start = GTA.Offsets.Editor.ddblip.pos;
            for (int j = i; j < n - 1; j++)
                for (int k = 0; k < next; k++)
                    new Global(start + j * next + k).SetInt(new Global(start + (j + 1) * next + k).Get<int>());
            new Global(GTA.Offsets.Editor.ddblip.number).SetInt(n - 1);
            _rebuild?.Invoke();
            if (_indexBox.SelectedIndex >= n - 1)
                _indexBox.SelectedIndex = n - 2;
            Refresh(true);
        }

        // ---------- map ----------

        private void FillPreviewTeams()
        {
            int teams = Math.Max(1, Rules.Teams());
            if (_previewTeam.Items.Count == teams)
            {
                FillPreviewRules();
                return;
            }
            _previewSync = true;
            int keep = Math.Max(0, Math.Min(_previewTeam.SelectedIndex, teams - 1));
            _previewTeam.Items.Clear();
            for (int t = 0; t < teams; t++)
                _previewTeam.Items.Add(T("dash_team", "Team") + " " + (t + 1));
            _previewTeam.SelectedIndex = keep;
            _previewSync = false;
            FillPreviewRules();
        }

        private void FillPreviewRules()
        {
            int team = Math.Max(0, _previewTeam.SelectedIndex);
            int count = Rules.Ready ? Math.Max(1, Rules.Count(team)) : 1;
            var items = Enumerable.Range(0, count).Select(r => RuleText(team, r)).ToList();
            if (_previewRule.Items.Count == items.Count && _previewRule.Items.Cast<string>().SequenceEqual(items))
                return;
            _previewSync = true;
            int keep = Math.Max(0, Math.Min(_previewRule.SelectedIndex, count - 1));
            _previewRule.Items.Clear();
            foreach (var s in items)
                _previewRule.Items.Add(s);
            _previewRule.SelectedIndex = keep;
            _previewSync = false;
        }

        // Where the blip shows: on its vehicle or actor when it follows one, else on its own spot.
        private static (float X, float Y) MapPos(Blip b)
        {
            if (b.Follows && b.Entity >= 0 && b.Entity < EntityPicker.Count(b.Type == TypeVehicle ? EntityPicker.Vehicle : EntityPicker.Actor))
            {
                if (b.Type == TypeVehicle && GTA.Offsets.Editor.Vehicle.loc != 0 && GTA.Offsets.Editor.Vehicle.NEXT != 0)
                {
                    long o = GTA.Offsets.Editor.Vehicle.loc + b.Entity * GTA.Offsets.Editor.Vehicle.NEXT;
                    return (new Global(o).Get<float>(), new Global(o + 1).Get<float>());
                }
                if (b.Type == TypePed && GTA.Offsets.Editor.Actor.locx != 0 && GTA.Offsets.Editor.Actor.NEXT != 0)
                    return (new Global(GTA.Offsets.Editor.Actor.locx + b.Entity * GTA.Offsets.Editor.Actor.NEXT).Get<float>(),
                            new Global(GTA.Offsets.Editor.Actor.locy + b.Entity * GTA.Offsets.Editor.Actor.NEXT).Get<float>());
            }
            return (b.X, b.Y);
        }

        // North up, one scale for both axes, at least 150 m across; the Pleb Masters map under it.
        private void DrawMap()
        {
            _map.Children.Clear();
            double w = _map.ActualWidth, h = _map.ActualHeight;
            if (w < 20 || h < 20 || _blips.Count == 0)
            {
                _previewCount.Text = "";
                return;
            }
            var spots = _blips.Select(b => (Blip: b, Pos: MapPos(b))).ToList();
            const double pad = 30;
            double minX = spots.Min(s => s.Pos.X), maxX = spots.Max(s => s.Pos.X), minY = spots.Min(s => s.Pos.Y), maxY = spots.Max(s => s.Pos.Y);
            _scale = Math.Min((w - 2 * pad) / Math.Max(150, maxX - minX), (h - 2 * pad) / Math.Max(150, maxY - minY));
            _cx = (minX + maxX) / 2;
            _cy = (minY + maxY) / 2;
            SatelliteTiles.Draw(_map, w, h, _cx, _cy, _scale);

            int team = Math.Max(0, _previewTeam.SelectedIndex), rule = Math.Max(0, _previewRule.SelectedIndex);
            int shown = 0;
            foreach (var s in spots.OrderBy(s => s.Blip.Index == Selected))
            {
                var b = s.Blip;
                // Preview: every team on the picked rule number.
                bool visible = b.ForTeam(team) && b.InRule(rule);
                if (visible) shown++;
                bool picked = b.Index == Selected;
                double size = picked ? 26 : 20;
                var pin = new Grid { Width = size + 10, Height = size + 10, Cursor = Cursors.Hand, Opacity = visible ? 1 : 0.3, Background = Brushes.Transparent, ToolTip = BlipName(b) + "\n" + WhenText(b) + " · " + WhereText(b) };
                if (picked)
                    pin.Children.Add(new Ellipse { Stroke = Brushes.White, StrokeThickness = 2, Fill = new SolidColorBrush(Color.FromArgb(0x50, 0, 0, 0)) });
                pin.Children.Add(SpriteImage(b.Sprite, b.Colour, size).Also(img => { img.HorizontalAlignment = HorizontalAlignment.Center; img.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 4, ShadowDepth = 1, Opacity = 0.8 }; }));
                Canvas.SetLeft(pin, w / 2 + (s.Pos.X - _cx) * _scale - (size + 10) / 2);
                Canvas.SetTop(pin, h / 2 - (s.Pos.Y - _cy) * _scale - (size + 10) / 2);
                int index = b.Index;
                pin.MouseLeftButtonUp += (_, e) => { e.Handled = true; Pick(index); };
                _map.Children.Add(pin);
            }
            _previewCount.Text = string.Format(CultureInfo.CurrentCulture, T("bl_preview_count", "{0} of {1} visible"), shown, _blips.Count);
        }

        // A click on the map moves the picked blip there when it sits on a fixed spot (Z stays).
        private void MapClick(object sender, MouseButtonEventArgs e)
        {
            int i = Selected;
            if (i < 0 || _blips[i].Follows || _scale <= 0 || !Live)
                return;
            var p = e.GetPosition(_map);
            float x = (float)(_cx + (p.X - _map.ActualWidth / 2) / _scale);
            float y = (float)(_cy - (p.Y - _map.ActualHeight / 2) / _scale);
            long o = At(GTA.Offsets.Editor.ddblip.pos, i);
            new Global(o).SetFloat(x);
            new Global(o + 1).SetFloat(y);
            Refresh(true);
        }

        // ---------- small parts ----------

        private static FrameworkElement Tiles(string[] labels, int selected, Action<int> pick, int columns = 0)
        {
            var grid = new UniformGrid { Columns = columns > 0 ? columns : labels.Length, Margin = new Thickness(0, 0, -6, 0) };
            for (int k = 0; k < labels.Length; k++)
            {
                int index = k;
                var tile = new ToggleButton { IsChecked = k == selected, Margin = new Thickness(0, 0, 6, 6), Padding = new Thickness(4, 6, 4, 6), Cursor = Cursors.Hand,
                    Content = new TextBlock { Text = labels[k], FontSize = 12.5, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center } };
                tile.SetResourceReference(StyleProperty, "ChoiceTile");
                tile.Click += (_, __) => pick(index);
                grid.Children.Add(tile);
            }
            return grid;
        }

        private static TextBlock Label(string text)
        {
            var t = new TextBlock { Text = text, FontSize = 15, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 18, 0, 7) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            return t;
        }

        private static TextBlock SubLabel(string text)
        {
            var t = new TextBlock { Text = text, Margin = new Thickness(0, 10, 0, 5) };
            t.SetResourceReference(StyleProperty, "FieldLabel");
            return t;
        }

        private static FrameworkElement Notice(string text, bool ok)
        {
            var border = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(10, 7, 10, 7) };
            border.BorderBrush = ok ? MainWindow.ThemeBrush("OkBrush") ?? Brushes.SeaGreen : MainWindow.ThemeBrush("WarnBrush") ?? Brushes.Goldenrod;
            border.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            var t = new TextBlock { Text = text, FontSize = 12.5, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            border.Child = t;
            return border;
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }
    }
}
