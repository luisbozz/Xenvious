using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Xenvious
{
    /// <summary>
    /// The job's option bitsets (g_FMMC_STRUCT.iOptionsMenuBitSet .. Thirty, keys menubs .. menubs30)
    /// as named switches. Names come from Rockstar's constants (OfflineData/menubits.json, built from
    /// the IS_BIT_SET(g_FMMC_STRUCT.iOptionsMenuBitSetN, ci...) uses in the source). By default only
    /// the bits that are set are listed, so a job shows at a glance which options it uses.
    /// </summary>
    public class MenuBitsView : Border
    {
        private const int Sets = 30;
        private static Dictionary<int, Dictionary<int, string>> _names;

        private readonly TextBox _search = new TextBox { Height = 30 };
        private readonly CheckBox _onlySet = new CheckBox { IsChecked = true };
        private readonly TextBlock _summary = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
        private readonly StackPanel _list = new StackPanel();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        private readonly int[] _values = new int[Sets + 1];
        private string _shownKey;
        private bool _built;
        private HashSet<string> _favorites;
        private string _favoritesCreator;

        // Starting favourites per creator ("field:bit"), options a job of that type usually touches.
        private static readonly string[] CommonFavorites = { "3:29", "3:26", "2:18", "2:4", "3:13", "1:28", "3:20", "4:14", "4:15", "2:1" };
        private static readonly Dictionary<string, string[]> CreatorFavorites = new Dictionary<string, string[]>
        {
            { "fm_lts_creator", new[] { "1:0", "3:31", "5:22" } },
            { "fm_capture_creator", new[] { "1:21", "5:29", "4:27" } },
            { "fm_race_creator", new[] { "2:28", "2:29", "3:5" } },
            { "fm_deathmatch_creator", new[] { "1:18", "1:24", "5:22" } },
            { "fm_mission_creator", new[] { "1:0", "3:13" } },
        };

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public MenuBitsView()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Refresh(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => Refresh();
        }

        private static Dictionary<int, Dictionary<int, string>> Names
        {
            get
            {
                if (_names == null)
                {
                    var raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(OfflineData.MenuBits);
                    _names = raw.ToDictionary(p => int.Parse(p.Key, CultureInfo.InvariantCulture),
                        p => p.Value.ToDictionary(b => int.Parse(b.Key, CultureInfo.InvariantCulture), b => b.Value));
                }
                return _names;
            }
        }

        // menubs, menubs2 ... menubs30 in GTA.Offsets.Editor.
        private static long Offset(int set)
        {
            var field = typeof(GTA.Offsets.Editor).GetField(set == 1 ? "menubs" : "menubs" + set.ToString(CultureInfo.InvariantCulture), BindingFlags.Public | BindingFlags.Static);
            return field == null ? 0 : Convert.ToInt64(field.GetValue(null), CultureInfo.InvariantCulture);
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            Style = (Style)FindResource("DashCard");
            Margin = new Thickness(0, 0, 0, 12);

            _summary.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
            var header = new DockPanel();
            DockPanel.SetDock(_summary, Dock.Right);
            header.Children.Add(_summary);
            header.Children.Add(new TextBlock { Style = (Style)FindResource("DashCardTitle"), Text = T("mb_title", "Job options") });

            _search.Style = (Style)FindResource("Watermark");
            _search.Tag = T("mb_search", "Search options");
            _search.TextChanged += (_, __) => Render(force: true);
            _onlySet.Style = (Style)FindResource("FormToggle");
            _onlySet.Click += (_, __) => Render(force: true);

            var filter = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            filter.ColumnDefinitions.Add(new ColumnDefinition());
            filter.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            filter.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            filter.Children.Add(_search);
            var onlyLabel = new TextBlock { Text = T("mb_onlyset", "Only set"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 8, 0), FontSize = 14 };
            Grid.SetColumn(onlyLabel, 1);
            Grid.SetColumn(_onlySet, 2);
            filter.Children.Add(onlyLabel);
            filter.Children.Add(_onlySet);

            var hint = new TextBlock
            {
                Text = T("mb_hint", "Every switch is one bit of the job's option fields (menubs). Names are Rockstar's; bits without a name are listed as numbers."),
                TextWrapping = TextWrapping.Wrap, FontSize = 12, Margin = new Thickness(0, 0, 0, 8),
            };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");

            var body = new StackPanel { Margin = new Thickness(14, 12, 14, 8) };
            body.Children.Add(hint);
            body.Children.Add(filter);
            body.Children.Add(_list);
            var dock = new DockPanel();
            var head = new Border { Style = (Style)FindResource("DashCardHeader"), Child = header };
            DockPanel.SetDock(head, Dock.Top);
            dock.Children.Add(head);
            dock.Children.Add(body);
            Child = dock;
            Refresh();
        }

        public void Refresh()
        {
            if (!_built || MainWindow.m == null || !MainWindow.m.IsProcOpen)
                return;
            for (int set = 1; set <= Sets; set++)
            {
                long offset = Offset(set);
                _values[set] = offset == 0 ? 0 : new Global(offset).Get<int>();
            }
            Render(force: false);
        }

        // Kept per creator in config.ini ("menubsfav_<script>"); a creator without a saved list
        // starts with the common options plus its own.
        private HashSet<string> Favorites
        {
            get
            {
                string creator = GTA.CurrentCreatorName() ?? "";
                if (_favorites != null && creator == _favoritesCreator)
                    return _favorites;
                _favoritesCreator = creator;
                // "#none" marks a missing key (the ini API returns "" for an empty one too).
                string saved = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString("Settings", "menubsfav_" + creator, "#none");
                if (saved != "#none")
                    _favorites = new HashSet<string>(saved.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                else
                {
                    _favorites = new HashSet<string>(CommonFavorites);
                    if (CreatorFavorites.TryGetValue(creator, out var own))
                        _favorites.UnionWith(own);
                }
                return _favorites;
            }
        }

        private void ToggleFavorite(string id)
        {
            if (!Favorites.Remove(id))
                Favorites.Add(id);
            new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "menubsfav_" + _favoritesCreator, string.Join("|", Favorites));
            Render(force: true);
        }

        private void Render(bool force)
        {
            string query = _search.Text.Trim();
            bool onlySet = _onlySet.IsChecked == true;
            string key = string.Join(",", _values) + "|" + query + "|" + onlySet + "|" + string.Join(",", Favorites) + "|" + _favoritesCreator;
            if (!force && key == _shownKey)
                return;
            _shownKey = key;

            _list.Children.Clear();
            int setCount = 0, shown = 0;

            // Favourites first, set or not, so the options a job usually needs are one click away.
            var favRows = new List<FrameworkElement>();
            foreach (var id in Favorites.OrderBy(f => f, StringComparer.Ordinal))
            {
                string[] parts = id.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int fs) || !int.TryParse(parts[1], out int fb) || fs < 1 || fs > Sets || fb < 0 || fb > 31)
                    continue;
                string raw = Names.TryGetValue(fs, out var fn) && fn.TryGetValue(fb, out var n) ? n : null;
                string label = raw == null ? string.Format(T("mb_unknown", "Unknown bit {0}"), fb) : Readable(raw);
                if (query.Length > 0 && label.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0 && (raw ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                favRows.Add(Row(fs, fb, (_values[fs] & (1 << fb)) != 0, label, raw));
            }
            if (favRows.Count > 0)
            {
                _list.Children.Add(GroupLabel(T("mb_favorites", "Favourites")));
                _list.Children.Add(Columns(favRows));
                shown += favRows.Count;
            }

            for (int set = 1; set <= Sets; set++)
            {
                Names.TryGetValue(set, out var named);
                var rows = new List<FrameworkElement>();
                for (int bit = 0; bit < 32; bit++)
                {
                    bool on = (_values[set] & (1 << bit)) != 0;
                    if (on) setCount++;
                    string raw = named != null && named.TryGetValue(bit, out var n) ? n : null;
                    if (raw == null && !on)
                        continue;
                    string label = raw == null ? string.Format(T("mb_unknown", "Unknown bit {0}"), bit) : Readable(raw);
                    if (onlySet && !on)
                        continue;
                    if (Favorites.Contains(set.ToString(CultureInfo.InvariantCulture) + ":" + bit.ToString(CultureInfo.InvariantCulture)))
                        continue;
                    if (query.Length > 0 && label.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0
                        && (raw ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    rows.Add(Row(set, bit, on, label, raw));
                }
                if (rows.Count == 0)
                    continue;
                _list.Children.Add(GroupLabel(string.Format(T("mb_group", "Field {0}"), set) + "  ·  menubs" + (set == 1 ? "" : set.ToString(CultureInfo.InvariantCulture))));
                _list.Children.Add(Columns(rows));
                shown += rows.Count;
            }
            _summary.Text = string.Format(T("mb_summary", "{0} set"), setCount);
            if (shown == 0)
            {
                var none = new TextBlock { Text = T("mb_none", "No option matches."), FontSize = 13, Margin = new Thickness(0, 4, 0, 4) };
                none.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
                _list.Children.Add(none);
            }
        }

        // Three rows side by side; the page is wide enough.
        private static FrameworkElement Columns(List<FrameworkElement> rows)
        {
            var grid = new System.Windows.Controls.Primitives.UniformGrid { Columns = 3 };
            foreach (var row in rows)
            {
                row.Margin = new Thickness(0, 0, 18, 2);
                grid.Children.Add(row);
            }
            return grid;
        }

        private TextBlock GroupLabel(string text)
        {
            var group = new TextBlock { Text = text, FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, _list.Children.Count == 0 ? 0 : 10, 0, 2) };
            group.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return group;
        }

        private static ControlTemplate _starTemplate;

        private FrameworkElement Row(int set, int bit, bool on, string label, string raw)
        {
            var row = new Grid { Style = (Style)FindResource("FormRow"), ToolTip = (raw ?? "") + "  (menubs" + (set == 1 ? "" : set.ToString(CultureInfo.InvariantCulture)) + ", bit " + bit + ")" };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new TextBlock { Style = (Style)FindResource("FormLabel"), Text = label, FontSize = 14 });
            string id = set.ToString(CultureInfo.InvariantCulture) + ":" + bit.ToString(CultureInfo.InvariantCulture);
            bool favorite = Favorites.Contains(id);
            if (_starTemplate == null)
                _starTemplate = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(
                    "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'>" +
                    "<Border Background='Transparent' Padding='4,0'><ContentPresenter VerticalAlignment='Center'/></Border></ControlTemplate>");
            var glyph = new TextBlock { Text = favorite ? "\u2605" : "\u2606", FontSize = 14 };
            glyph.SetResourceReference(TextBlock.ForegroundProperty, favorite ? "AccentBrush" : "NavMutedBrush");
            var star = new Button { Template = _starTemplate, Content = glyph, Cursor = System.Windows.Input.Cursors.Hand, Opacity = favorite ? 1 : 0,
                ToolTip = T(favorite ? "editnav_unfav" : "editnav_fav", favorite ? "Remove from favourites" : "Add to favourites") };
            star.Click += (_, __) => ToggleFavorite(id);
            if (!favorite)
            {
                row.Background = System.Windows.Media.Brushes.Transparent;
                row.MouseEnter += (_, __) => star.Opacity = 1;
                row.MouseLeave += (_, __) => star.Opacity = 0;
            }
            Grid.SetColumn(star, 3);
            row.Children.Add(star);
            var number = new TextBlock { Text = bit.ToString(CultureInfo.InvariantCulture), FontSize = 11, Margin = new Thickness(8, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center };
            number.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            Grid.SetColumn(number, 1);
            row.Children.Add(number);
            var toggle = new CheckBox { Style = (Style)FindResource("FormToggle"), IsChecked = on };
            toggle.Click += (_, __) => Toggle(set, bit, toggle.IsChecked == true);
            Grid.SetColumn(toggle, 2);
            row.Children.Add(toggle);
            return row;
        }

        private void Toggle(int set, int bit, bool on)
        {
            long offset = Offset(set);
            if (offset == 0 || !MainWindow.m.IsProcOpen)
                return;
            int value = new Global(offset).Get<int>();
            value = on ? value | (1 << bit) : value & ~(1 << bit);
            new Global(offset).SetInt(value);
            _values[set] = value;
            // Keep the row where it is until the next search or refresh, so it can be switched back.
            _shownKey = string.Join(",", _values) + "|" + _search.Text.Trim() + "|" + (_onlySet.IsChecked == true) + "|" + string.Join(",", Favorites) + "|" + _favoritesCreator;
        }

        // "ciOptionsBS23_EnableBlipPlayerOnHit" -> "Enable blip player on hit".
        private static string Readable(string names)
        {
            string first = names.Split(new[] { " / " }, StringSplitOptions.None)[0];
            string s = Regex.Replace(first, @"^ci(OptionsBS\d*_|OPTIONS\d*_|OPTION\d*_)?", "");
            s = Regex.Replace(s, @"([a-z0-9])([A-Z])", "$1 $2").Replace('_', ' ');
            s = Regex.Replace(s, @"\s+", " ").Trim();
            if (s.Length == 0)
                return first;
            // All-caps names read better in sentence case.
            s = s.ToLowerInvariant();
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}
