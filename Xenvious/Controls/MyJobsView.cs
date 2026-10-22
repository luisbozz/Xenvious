using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Misc › My Jobs: every published and saved job of the player (UgcList), with its photo, a
    /// map of what is placed and the job's settings from its job file. The list comes from the
    /// open creator's payload; photo and job file come straight from Rockstar's cloud, where
    /// the payload's UGC_GET_CONTENT_PATH points (saved jobs too).
    /// </summary>
    public class MyJobsView : Grid
    {
        private const string Cloud = "https://prod.cloud.rockstargames.com";
        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        // Job type (gen.type/subtype) to label and colour key, as on the Edit pages.
        private static readonly (string Key, string Name, string Brush)[] Types =
        {
            ("L", "LTS", "WarnBrush"), ("C", "Capture", "DynamicPropBrush"), ("M", "Mission", "AccentBrush"),
            ("R", "Race", "OkBrush"), ("D", "Deathmatch", "BadBrush"), ("S", "Survival", "CheckpointBrush"),
        };

        // Map layers: section of the job file, its position key, label key, brush.
        private static readonly (string Section, string Pos, string Key, string Fallback, string Brush)[] Layers =
        {
            ("prop", "loc", "map_part_prop", "Props", "PropBrush"),
            ("dprop", "loc", "dash_dynprops", "Dynamic Props", "DynamicPropBrush"),
            ("dhprop", "pos", "editnav_fixtures", "Fixtures", "FixtureBrush"),
            ("veh", "loc", "rl_m_vehs", "Vehicles", "VehicleBrush"),
            ("weap", "loc", "missionrspweapons", "Weapons", "WeaponBrush"),
            ("obj", "loc", "mj_objects", "Objects", "ZoneBrush"),
            ("ene", "loc", "dash_actors", "Actors", "ActorBrush"),
        };

        private sealed class Entry
        {
            public UgcList.Job Job;
            // The mission dict of the job file, only while the job is the one shown: big jobs
            // parse to tens of MB, and a list of public jobs holds hundreds of them.
            public JObject Mission;
            public string TypeKey;      // L C M R D S, once the job file was read
            public bool FileFailed;
            public ImageSource Thumb;   // decoded small, for the list
            public bool PhotoMissing;
        }

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;
        private static Brush B(string key) => MainWindow.ThemeBrush(key);

        /// <summary>A list on the left: one cloud query. The own jobs load when the page opens, the rest on their first click.</summary>
        private sealed class Source
        {
            public string Id, Key, Fallback;
            public UgcList.Kind Kind;
            public int Category;
            public bool Mine;
        }

        /// <summary>How far a list is loaded: the public lists go on for thousands, so they load in batches as the list scrolls.</summary>
        private sealed class Paging
        {
            public int Total = -1;
            public bool Done, Failed;
        }

        private const int Batch = 100;

        private static readonly Source[] Sources =
        {
            new Source { Id = "pub", Key = "jl_published", Fallback = "Published", Kind = UgcList.Kind.Published, Mine = true },
            new Source { Id = "saved", Key = "jl_saved", Fallback = "Saved", Kind = UgcList.Kind.Saved, Mine = true },
            new Source { Id = "bm", Key = "mj_src_bookmarked", Fallback = "Bookmarked", Kind = UgcList.Kind.Bookmarked },
            new Source { Id = "played", Key = "mj_src_played", Fallback = "Recently played", Kind = UgcList.Kind.RecentlyPlayed },
            new Source { Id = "friends", Key = "mj_src_friends", Fallback = "Friends", Kind = UgcList.Kind.Friends },
            new Source { Id = "rsc", Key = "mj_src_rockstar", Fallback = "Rockstar created", Kind = UgcList.Kind.Category, Category = UgcList.RockstarCreated },
            new Source { Id = "rsv", Key = "mj_src_verified", Fallback = "Rockstar verified", Kind = UgcList.Kind.Category, Category = UgcList.RockstarVerified },
            new Source { Id = "top", Key = "mj_src_top", Fallback = "Top rated", Kind = UgcList.Kind.TopRated },
            new Source { Id = "new", Key = "mj_src_newest", Fallback = "Newest", Kind = UgcList.Kind.Newest },
        };
        private const string SearchId = "id";

        private readonly Dictionary<string, List<Entry>> _lists = new Dictionary<string, List<Entry>>();
        private readonly Dictionary<string, Paging> _paging = new Dictionary<string, Paging>();
        // ul_main runs one query at a time.
        private readonly SemaphoreSlim _queryLock = new SemaphoreSlim(1);
        private static readonly SemaphoreSlim FileGate = new SemaphoreSlim(3), PhotoGate = new SemaphoreSlim(4);
        private ScrollViewer _rowScroll;
        private string _source = "pub";
        private readonly TextBox _idBox = new TextBox { Height = 30 };
        private readonly HashSet<string> _typeFilter = new HashSet<string>();
        private string _query = "";
        private Entry _current;
        private readonly HashSet<string> _layersOff = new HashSet<string>();

        private readonly StackPanel _sources = new StackPanel();
        private readonly TextBox _search = new TextBox { Height = 30 };
        private readonly WrapPanel _chips = new WrapPanel { Margin = new Thickness(0, 6, 0, 6) };
        private readonly StackPanel _rows = new StackPanel();
        private readonly TextBlock _status = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(2, 6, 2, 0) };
        private readonly ScrollViewer _detailScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        private readonly JobMap _map = new JobMap { Interactive = true, MinSpan = 6 };
        private readonly Grid _mapHost = new Grid();
        private readonly Grid _overlay = new Grid { Visibility = Visibility.Collapsed };

        public MyJobsView()
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(310) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Loaded += (_, __) => Build();
            IsVisibleChanged += async (_, __) =>
            {
                if (IsVisible && _lists.Count == 0)
                    await ReloadAsync();
            };
        }

        private bool _built;

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            var left = Card(new StackPanel { Children = { _sources } });
            left.Margin = new Thickness(0, 0, 10, 0);
            Children.Add(left);

            _search.Style = (Style)FindResource("Watermark");
            _search.Tag = T("mj_search", "Name or content ID");
            _search.TextChanged += (_, __) => { _query = _search.Text.Trim(); RenderRows(); };
            var listPanel = new DockPanel();
            var top = new StackPanel { Children = { _search, _chips } };
            DockPanel.SetDock(top, Dock.Top);
            listPanel.Children.Add(top);
            DockPanel.SetDock(_status, Dock.Bottom);
            _status.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            listPanel.Children.Add(_status);
            _rowScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = _rows };
            _rowScroll.ScrollChanged += (_, __) => OnRowsScrolled();
            listPanel.Children.Add(_rowScroll);
            var middle = Card(listPanel);
            middle.Margin = new Thickness(0, 0, 10, 0);
            SetColumn(middle, 1);
            Children.Add(middle);

            var right = Card(_detailScroll);
            SetColumn(right, 2);
            Children.Add(right);

            // The map can fill the whole page; the same control moves between its two hosts.
            _overlay.Background = new SolidColorBrush(Color.FromArgb(0xB0, 0, 0, 0));
            SetColumnSpan(_overlay, 3);
            Children.Add(_overlay);

            RenderSources();
            RenderChips();
            ShowDetail();
        }

        private Border Card(UIElement child)
        {
            var card = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(10), Child = child };
            card.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            return card;
        }

        // ----- loading -----

        private async Task ReloadAsync()
        {
            if (!UgcList.Available)
            {
                SetStatus(T("mj_need_creator", "Open a creator in GTA (with script features on); the job list is fetched from there."));
                return;
            }
            foreach (var source in Sources.Where(x => x.Mine || x.Id == _source))
                await LoadMoreAsync(source);
        }

        /// <summary>
        /// The next batch of a list, page by page; each page shows up in the list as it comes.
        /// A batch stops early when the player switches to another list, so that one starts soon.
        /// </summary>
        private async Task LoadMoreAsync(Source source)
        {
            if (!_paging.TryGetValue(source.Id, out var paging))
                _paging[source.Id] = paging = new Paging();
            if (paging.Done)
                return;
            if (!UgcList.Available)
            {
                SetStatus(T("mj_need_creator", "Open a creator in GTA (with script features on); the job list is fetched from there."));
                return;
            }
            if (!await _queryLock.WaitAsync(0))
                return;
            try
            {
                if (!_lists.TryGetValue(source.Id, out var list))
                    _lists[source.Id] = list = new List<Entry>();
                string name = T(source.Key, source.Fallback);
                int target = list.Count + Batch;
                while (!paging.Done && list.Count < target)
                {
                    SetStatus(string.Format(CultureInfo.CurrentCulture, paging.Total < 0 ? T("mj_loading", "Loading {0} …") : T("mj_loading_n", "Loading {0}: {1} of {2}"),
                        name, list.Count, paging.Total));
                    var page = await UgcList.PageAsync(source.Kind, list.Count, UgcList.PageSize, category: source.Category);
                    // The creator hub runs queries of its own, and one of them can drop ours
                    // (no answer at all); asking again usually works.
                    if (page == null && UgcList.Available)
                        page = await UgcList.PageAsync(source.Kind, list.Count, UgcList.PageSize, category: source.Category);
                    if (page == null)
                    {
                        paging.Failed = list.Count == 0;
                        paging.Done = true;
                        break;
                    }
                    paging.Total = page.Value.Total;
                    var added = page.Value.Jobs.Select(j => new Entry { Job = j }).ToList();
                    list.AddRange(added);
                    if (added.Count == 0 || list.Count >= paging.Total)
                        paging.Done = true;
                    RenderSources();
                    if (source.Id == _source)
                        AppendRows(added);
                    _ = LoadTypesAsync(added);
                    if (source.Id != _source)
                        break;
                }
                SetStatus(paging.Failed ? T("mj_failed", "The cloud did not answer. Try Reload.") : "");
                if (source.Id == _source && _lists[source.Id].Count == 0)
                    RenderRows();
            }
            finally
            {
                _queryLock.Release();
            }
            // The player may have scrolled to the end, or switched lists, while this ran.
            OnRowsScrolled();
        }

        private async Task OpenSourceAsync(Source source)
        {
            _source = source.Id;
            RenderSources();
            RenderRows();
            if (!_lists.ContainsKey(source.Id))
                await LoadMoreAsync(source);
            else
                await Dispatcher.InvokeAsync(OnRowsScrolled, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // At the end of the list the next batch loads; rows coming into view get their photo.
        private void OnRowsScrolled()
        {
            if (_rowScroll == null)
                return;
            LoadVisibleThumbs();
            var source = Sources.FirstOrDefault(x => x.Id == _source);
            if (source != null && (!_lists.ContainsKey(source.Id) || _rowScroll.VerticalOffset >= _rowScroll.ScrollableHeight - 300))
                _ = LoadMoreAsync(source);
        }

        // A job by its content id or Social Club link: the latest version, from the cloud.
        private async Task FindByIdAsync()
        {
            var match = System.Text.RegularExpressions.Regex.Match(_idBox.Text ?? "", @"([\w-]{22})");
            if (!match.Success)
            {
                SetStatus(T("mj_id_invalid", "Enter a content id (22 characters) or a Social Club job link."));
                return;
            }
            if (!UgcList.Available)
            {
                SetStatus(T("mj_need_creator", "Open a creator in GTA (with script features on); the job list is fetched from there."));
                return;
            }
            await _queryLock.WaitAsync();
            try
            {
                SetStatus(T("mj_id_searching", "Searching …"));
                var job = await UgcList.ByIdAsync(match.Groups[1].Value);
                var list = _lists.TryGetValue(SearchId, out var l) ? l : new List<Entry>();
                if (job != null && !list.Any(x => x.Job.ContentId == job.ContentId))
                    list.Insert(0, new Entry { Job = job });
                _lists[SearchId] = list;
                _source = SearchId;
                if (job != null)
                    Select(list.First(x => x.Job.ContentId == job.ContentId));
                SetStatus(job == null ? T("mj_id_none", "The cloud has no job with this id.") : "");
                RenderSources();
                RenderRows();
                _ = LoadTypesAsync(list);
            }
            finally
            {
                _queryLock.Release();
            }
        }

        private void Select(Entry e)
        {
            if (_current != null && _current != e)
                _current.Mission = null;
            _current = e;
            ShowDetail();
        }

        // The type of each job for the filter and the badge, a few files at a time. Only the
        // type stays; the parsed file is dropped again.
        private async Task LoadTypesAsync(IEnumerable<Entry> entries)
        {
            var todo = entries.Where(e => e.TypeKey == null && !e.FileFailed).ToList();
            await Task.WhenAll(todo.Select(async e =>
            {
                await FileGate.WaitAsync();
                try { await LoadFileAsync(e); }
                finally { FileGate.Release(); }
            }));
            if (todo.Count > 0)
            {
                RenderChips();
                RefreshBadges();
                // With a type filter on, these were hidden until their type was known.
                if (_typeFilter.Count > 0 && _lists.TryGetValue(_source, out var shown))
                    AppendRows(todo.Where(shown.Contains));
            }
        }

        private static string CacheDir => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "jobcache");

        // The path names the file version, so a cached file is the current one.
        private static string CachePath(Entry e, string suffix) =>
            System.IO.Path.Combine(CacheDir, e.Job.ContentId + "_" + System.IO.Path.GetFileNameWithoutExtension(e.Job.Path0 ?? "") + suffix);

        private async Task LoadFileAsync(Entry e)
        {
            if (e.Mission != null || string.IsNullOrEmpty(e.Job.Path0))
                return;
            try
            {
                string cache = CachePath(e, ".json");
                string text;
                if (File.Exists(cache))
                    text = await Task.Run(() => File.ReadAllText(cache));
                else
                {
                    text = await http.GetStringAsync(Cloud + e.Job.Path0);
                    Directory.CreateDirectory(CacheDir);
                    File.WriteAllText(cache, text);
                }
                var mission = await Task.Run(() => JObject.Parse(text)["mission"] as JObject);
                e.TypeKey = TypeKey(mission);
                if (e == _current)
                {
                    e.Mission = mission;
                    ShowDetail();
                }
            }
            catch (Exception ex)
            {
                e.FileFailed = true;
                Log.Debug($"my jobs: job file of {e.Job.ContentId}: {ex.Message}", source: "myjobs");
            }
        }

        private static string TypeKey(JObject mission)
        {
            int type = (int?)mission?["gen"]?["type"] ?? -1, subtype = (int?)mission?["gen"]?["subtype"] ?? -1;
            switch (type)
            {
                case 1: return "D";
                case 2: return "R";
                case 3: return "S";
                case 0: return subtype == 5 ? "L" : subtype == 6 ? "C" : "M";
                default: return null;
            }
        }

        // The photo sits next to the job file as 2_0.jpg; jobs with only a lo-res photo have none there.
        // Decoded at the size it is shown: a full photo is 8 MB in memory.
        private async Task<ImageSource> PhotoAsync(Entry e, int width)
        {
            if (e.PhotoMissing)
                return null;
            if ((e.Job.Flags & UgcList.JobFlags.HiResPhoto) == 0 || string.IsNullOrEmpty(e.Job.Path0))
            {
                e.PhotoMissing = true;
                return null;
            }
            await PhotoGate.WaitAsync();
            try
            {
                string cache = CachePath(e, ".jpg");
                byte[] bytes;
                if (File.Exists(cache))
                    bytes = await Task.Run(() => File.ReadAllBytes(cache));
                else
                {
                    string folder = e.Job.Path0.Substring(0, e.Job.Path0.LastIndexOf('/') + 1);
                    bytes = await http.GetByteArrayAsync(Cloud + folder + "2_0.jpg");
                    Directory.CreateDirectory(CacheDir);
                    File.WriteAllBytes(cache, bytes);
                }
                return await Task.Run(() =>
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = width;
                    image.StreamSource = new MemoryStream(bytes);
                    image.EndInit();
                    image.Freeze();
                    return (ImageSource)image;
                });
            }
            catch
            {
                e.PhotoMissing = true;
                return null;
            }
            finally
            {
                PhotoGate.Release();
            }
        }

        private void SetStatus(string text) => _status.Text = text ?? "";

        // ----- left: sources -----

        private void RenderSources()
        {
            _sources.Children.Clear();
            Button Item(string text, string id, Action click)
            {
                // What the cloud has in all, not only what is loaded yet.
                int n = _paging.TryGetValue(id, out var p) && p.Total > 0 ? p.Total : _lists.TryGetValue(id, out var l) ? l.Count : 0;
                var row = new DockPanel();
                var count = new TextBlock { Text = n > 0 ? n.ToString("N0", CultureInfo.CurrentCulture) : "", Margin = new Thickness(8, 0, 0, 0), FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
                count.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                DockPanel.SetDock(count, Dock.Right);
                row.Children.Add(count);
                row.Children.Add(new TextBlock { Text = text, Style = (Style)FindResource("NavLabel"), Margin = new Thickness(0) });
                var button = new Button { Style = (Style)FindResource("SideNavButton"), Content = row, Tag = id == _source ? "active" : null, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += (_, __) => click();
                _sources.Children.Add(button);
                return button;
            }
            _sources.Children.Add(Header(T("mj_mine", "My jobs")));
            foreach (var source in Sources.Where(x => x.Mine))
                Item(T(source.Key, source.Fallback), source.Id, async () => await OpenSourceAsync(source));
            _sources.Children.Add(Header(T("mj_discover", "Discover")));
            foreach (var source in Sources.Where(x => !x.Mine))
                Item(T(source.Key, source.Fallback), source.Id, async () => await OpenSourceAsync(source));

            _sources.Children.Add(Header(T("mj_by_id", "Find by id")));
            if (_idBox.Style == null)
            {
                _idBox.Style = (Style)FindResource("Watermark");
                _idBox.Tag = "ID";
                _idBox.ToolTip = T("mj_id_hint", "Content id or link");
                _idBox.KeyDown += async (_, e) => { if (e.Key == System.Windows.Input.Key.Enter) await FindByIdAsync(); };
            }
            if (_idBox.Parent is Panel old)
                old.Children.Remove(_idBox);
            _idBox.Margin = new Thickness(4, 0, 4, 4);
            _sources.Children.Add(_idBox);
            var find = new Button { Content = T("mj_id_find", "Find"), Height = 28, Margin = new Thickness(4, 0, 4, 4) };
            find.SetResourceReference(StyleProperty, "FormButton");
            find.Click += async (_, __) => await FindByIdAsync();
            _sources.Children.Add(find);
            if (_lists.ContainsKey(SearchId))
                Item(T("mj_src_found", "Found"), SearchId, () => { _source = SearchId; RenderSources(); RenderRows(); });

            _sources.Children.Add(Header(T("mj_actions", "Actions")));
            var reload = new Button { Style = (Style)FindResource("SideNavButton"), Content = new TextBlock { Text = "↻  " + T("mj_reload", "Reload"), Style = (Style)FindResource("NavLabel"), Margin = new Thickness(0) } };
            reload.Click += async (_, __) =>
            {
                foreach (var id in Sources.Where(x => x.Mine || x.Id == _source).Select(x => x.Id))
                {
                    _lists.Remove(id);
                    _paging.Remove(id);
                }
                _current = null;
                ShowDetail();
                RenderSources();
                RenderRows();
                await ReloadAsync();
            };
            _sources.Children.Add(reload);

            // Searches the game cannot do (any player's jobs, more filters) live on the web.
            var more = new TextBlock { Text = T("mj_more", "For bigger searches (any player's jobs, more filters) use Social Club Plus by Bulzo."),
                FontSize = 11.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(8, 14, 4, 0) };
            more.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _sources.Children.Add(more);
        }

        private TextBlock Header(string text)
        {
            var t = new TextBlock { Text = text.ToUpper(CultureInfo.CurrentCulture), FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(8, 10, 0, 4) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            return t;
        }

        // ----- middle: list -----

        // One flat chip per job type in the list: colour dot, name, how many. A chip that is on
        // gets a tint of its colour; types the list does not have stay hidden.
        private void RenderChips()
        {
            _chips.Children.Clear();
            var list = _lists.TryGetValue(_source, out var l) ? l : new List<Entry>();
            foreach (var (key, name, brush) in Types)
            {
                int count = list.Count(e => e.TypeKey == key);
                bool on = _typeFilter.Contains(key);
                if (count == 0 && !on)
                    continue;
                var tint = B(brush);
                var label = new TextBlock { Text = name, FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
                label.SetResourceReference(TextBlock.ForegroundProperty, on ? "TextColor" : "MutedTextBrush");
                var number = new TextBlock { Text = count.ToString("N0", CultureInfo.CurrentCulture), FontSize = 11, Margin = new Thickness(6, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
                number.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                var chip = new Border
                {
                    CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 4, 9, 4), Margin = new Thickness(0, 0, 5, 5),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Child = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children = { new Ellipse { Width = 7, Height = 7, Fill = tint, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center }, label, number },
                    },
                };
                Brush idle;
                if (on)
                {
                    var fill = tint.Clone();
                    fill.Opacity = 0.22;
                    idle = fill;
                }
                else
                    idle = B("DeepBrush");
                chip.Background = idle;
                chip.MouseEnter += (_, __) => { if (!on) chip.Background = B("ButtonHoverBackgroundBrush"); };
                chip.MouseLeave += (_, __) => chip.Background = idle;
                chip.MouseLeftButtonUp += (_, __) =>
                {
                    if (!_typeFilter.Remove(key))
                        _typeFilter.Add(key);
                    RenderChips();
                    RenderRows();
                };
                _chips.Children.Add(chip);
            }
        }

        private void RenderRows()
        {
            if (!_built)
                return;
            _rows.Children.Clear();
            RenderChips();
            AppendRows(_lists.TryGetValue(_source, out var list) ? list : new List<Entry>());
            if (_rows.Children.Count == 0 && _paging.TryGetValue(_source, out var paging) && paging.Done)
                _rows.Children.Add(new TextBlock { Text = T("mj_none", "No jobs."), Margin = new Thickness(8), Foreground = B("MutedTextBrush") });
            _rowScroll?.ScrollToTop();
        }

        // Adds the rows of new jobs that pass the filter, without building the list again.
        private void AppendRows(IEnumerable<Entry> entries)
        {
            if (!_built)
                return;
            foreach (var e in Filter(entries))
                _rows.Children.Add(Row(e));
            Dispatcher.BeginInvoke(new Action(LoadVisibleThumbs), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private IEnumerable<Entry> Filter(IEnumerable<Entry> list) =>
            list.Where(e => (_typeFilter.Count == 0 || (e.TypeKey != null && _typeFilter.Contains(e.TypeKey)))
                && (_query.Length == 0 || Name(e).IndexOf(_query, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || e.Job.ContentId.IndexOf(_query, StringComparison.OrdinalIgnoreCase) >= 0));

        private static string Name(Entry e) => WebUtility.HtmlDecode(e.Job.Name ?? "");

        private sealed class RowParts
        {
            public Entry Entry;
            public Border Thumb;
            public DockPanel Line;
            public FrameworkElement Badge;
            public bool ThumbAsked;
        }

        private FrameworkElement Row(Entry e)
        {
            var thumb = new Border { Width = 48, Height = 27, CornerRadius = new CornerRadius(3), Margin = new Thickness(0, 0, 9, 0), ClipToBounds = true };
            thumb.SetResourceReference(Border.BackgroundProperty, "DeepBrush");

            var name = new TextBlock { Text = Name(e), FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
            name.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            bool mine = Sources.Any(x => x.Mine && x.Id == _source);
            var meta = new TextBlock { Text = mine ? Updated(e.Job) : (e.Job.Creator ?? "") + "  ·  " + Updated(e.Job), FontSize = 11.5, TextTrimming = TextTrimming.CharacterEllipsis };
            meta.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            var text = new StackPanel { Children = { name, meta }, VerticalAlignment = VerticalAlignment.Center };

            var row = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(thumb, Dock.Left);
            row.Children.Add(thumb);
            var parts = new RowParts { Entry = e, Thumb = thumb, Line = row };
            var badge = TypeBadge(e.TypeKey);
            if (badge != null)
            {
                DockPanel.SetDock(badge, Dock.Right);
                row.Children.Add(badge);
                parts.Badge = badge;
            }
            row.Children.Add(text);
            if (e.Thumb != null)
                thumb.Child = new Image { Source = e.Thumb, Stretch = Stretch.UniformToFill };

            var button = new Button
            {
                Style = (Style)FindResource("SideNavButton"), Content = row, Padding = new Thickness(6, 5, 6, 5),
                HorizontalContentAlignment = HorizontalAlignment.Stretch, Tag = e == _current ? "active" : null, DataContext = parts,
            };
            button.Click += (_, __) =>
            {
                foreach (var other in _rows.Children.OfType<Button>())
                    other.Tag = other == button ? "active" : null;
                Select(e);
            };
            return button;
        }

        // Badges of jobs whose type came in after their row was made.
        private void RefreshBadges()
        {
            foreach (var parts in _rows.Children.OfType<Button>().Select(b => b.DataContext).OfType<RowParts>())
            {
                if (parts.Badge != null || parts.Entry.TypeKey == null)
                    continue;
                var badge = TypeBadge(parts.Entry.TypeKey);
                if (badge == null)
                    continue;
                DockPanel.SetDock(badge, Dock.Right);
                parts.Line.Children.Insert(1, badge);
                parts.Badge = badge;
            }
        }

        // Only rows on screen (and a little below) fetch their photo.
        private void LoadVisibleThumbs()
        {
            if (_rowScroll == null || !_rowScroll.IsVisible)
                return;
            double bottom = _rowScroll.ViewportHeight + 200;
            foreach (var button in _rows.Children.OfType<Button>())
            {
                if (!(button.DataContext is RowParts parts) || parts.ThumbAsked || parts.Entry.Thumb != null)
                    continue;
                double y;
                try { y = button.TransformToAncestor(_rowScroll).Transform(new Point(0, 0)).Y; }
                catch (InvalidOperationException) { continue; }
                if (y < -60 || y > bottom)
                    continue;
                parts.ThumbAsked = true;
                _ = FillThumbAsync(parts);
            }
        }

        private async Task FillThumbAsync(RowParts parts)
        {
            var e = parts.Entry;
            e.Thumb = e.Thumb ?? await PhotoAsync(e, 96);
            parts.Thumb.Child = e.Thumb != null ? new Image { Source = e.Thumb, Stretch = Stretch.UniformToFill } : NoPhoto(12);
        }

        private static FrameworkElement NoPhoto(double size)
        {
            var path = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M3,5 H21 V19 H3 Z M3,16 L8,11 L12,15 L15,12 L21,18 M4,4 L20,20"),
                Stroke = B("FaintTextBrush"), StrokeThickness = 1.5, Stretch = Stretch.Uniform, Width = size, Height = size,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            };
            return path;
        }

        private static FrameworkElement TypeBadge(string key)
        {
            var type = Types.FirstOrDefault(t => t.Key == key);
            if (type.Key == null)
                return null;
            return Badge(type.Name, B(type.Brush));
        }

        private static Border Badge(string text, Brush tint)
        {
            var fill = tint.Clone();
            fill.Opacity = 0.22;
            return new Border
            {
                CornerRadius = new CornerRadius(9), Padding = new Thickness(7, 1, 7, 2), Margin = new Thickness(6, 0, 0, 0),
                Background = fill, VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = text, FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = tint },
            };
        }

        private static string Updated(UgcList.Job job)
        {
            var d = job.Updated;
            if (d == null || d[0] < 2000 || d[1] < 1 || d[1] > 12 || d[2] < 1 || d[2] > 31)
                return "";
            try
            {
                var at = new DateTime(d[0], d[1], d[2], Clamp(d[3], 23), Clamp(d[4], 59), Clamp(d[5], 59), DateTimeKind.Utc).ToLocalTime();
                return at.ToString("g", CultureInfo.CurrentCulture);
            }
            catch (ArgumentException)
            {
                return "";
            }
        }

        private static int Clamp(int v, int max) => Math.Max(0, Math.Min(max, v));

        // ----- right: detail -----

        private void ShowDetail()
        {
            if (!_built)
                return;
            var e = _current;
            if (e == null)
            {
                _detailScroll.Content = new TextBlock { Text = T("mj_pick", "Pick a job."), Margin = new Thickness(8), Foreground = B("MutedTextBrush") };
                return;
            }
            _ = LoadFileAsync(e);

            var root = new StackPanel();
            var top = new Grid();
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var left = new StackPanel();
            var photo = new Border { MaxWidth = 320, Height = 180, CornerRadius = new CornerRadius(7), HorizontalAlignment = HorizontalAlignment.Left, ClipToBounds = true };
            photo.Width = 320;
            photo.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            _ = FillPhotoAsync(e, photo);
            left.Children.Add(photo);

            var title = new TextBlock { Text = Name(e), FontSize = 18, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 4) };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            left.Children.Add(title);

            var tags = new WrapPanel();
            var type = TypeBadge(e.TypeKey);
            if (type != null)
            {
                type.Margin = new Thickness(0, 0, 5, 4);
                tags.Children.Add(type);
            }
            bool published = (e.Job.Flags & UgcList.JobFlags.Published) != 0;
            tags.Children.Add(Tag(published ? T("mj_published", "published") : T("mj_saved", "saved"), published ? "OkBrush" : "WarnBrush"));
            int tested = (int?)e.Mission?["gen"]?["testcomplete"] ?? -1;
            if (tested >= 0)
                tags.Children.Add(Tag(tested == 1 ? "✓ " + T("mj_tested", "tested") : T("mj_untested", "not tested"), tested == 1 ? "OkBrush" : "FaintTextBrush"));
            if ((e.Job.Flags & UgcList.JobFlags.Verified) != 0)
                tags.Children.Add(Tag(T("mj_verified", "verified"), "AccentBrush"));
            left.Children.Add(tags);

            var facts = new List<string> { T("mj_updated", "Updated") + " " + Updated(e.Job) };
            if ((e.Job.Flags & UgcList.JobFlags.Mine) == 0 && !string.IsNullOrEmpty(e.Job.Creator))
                facts.Insert(0, string.Format(CultureInfo.CurrentCulture, T("mj_by", "by {0}"), e.Job.Creator));
            if (published && e.Job.RatingCount > 0)
                facts.Add($"👍 {e.Job.Positive} · 👎 {e.Job.RatingCount - e.Job.Positive} ({Math.Round(e.Job.Rating * 100)} %)");
            left.Children.Add(Muted(string.Join("    ", facts), new Thickness(0, 4, 0, 0)));

            string description = string.Concat((e.Mission?["gen"]?["dec"] as JArray)?.Select(x => (string)x) ?? Enumerable.Empty<string>());
            if (description.Length > 0)
                left.Children.Add(Muted(WebUtility.HtmlDecode(description), new Thickness(0, 10, 0, 0)));

            left.Children.Add(Actions(e));
            left.Children.Add(Muted($"{e.Job.ContentId} · {System.IO.Path.GetFileNameWithoutExtension(e.Job.Path0 ?? "")}", new Thickness(0, 4, 0, 0), mono: true));
            top.Children.Add(left);

            // The map: as high as the left side.
            _mapHost.Children.Clear();
            if (_map.Parent is Panel old)
                old.Children.Remove(_map);
            _mapHost.Children.Add(_map);
            _mapHost.Children.Add(MapControls(false));
            _mapHost.MinHeight = 260;
            SetColumn(_mapHost, 2);
            if (_mapHost.Parent is Panel host)
                host.Children.Remove(_mapHost);
            top.Children.Add(_mapHost);
            FillMap(e, keepView: false);

            root.Children.Add(top);
            root.Children.Add(Tables(e));
            _detailScroll.Content = root;
        }

        private async Task FillPhotoAsync(Entry e, Border frame)
        {
            var photo = await PhotoAsync(e, 640);
            if (e != _current)
                return;
            if (photo != null)
                frame.Child = new Image { Source = photo, Stretch = Stretch.UniformToFill };
            else
            {
                var none = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                none.Children.Add(NoPhoto(30));
                none.Children.Add(Muted(T("mj_nophoto", "No photo for this job"), new Thickness(0, 4, 0, 0)));
                frame.Child = none;
            }
        }

        private static Border Tag(string text, string brush)
        {
            var b = Badge(text, B(brush));
            b.Margin = new Thickness(0, 0, 5, 4);
            return b;
        }

        private static TextBlock Muted(string text, Thickness margin, bool mono = false)
        {
            var t = new TextBlock { Text = text, FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = margin };
            if (mono)
                t.FontFamily = new FontFamily("Consolas");
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            return t;
        }

        private FrameworkElement Actions(Entry e)
        {
            var panel = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            Button Make(string text, bool primary, Action click)
            {
                var b = new Button { Content = text, Height = 32, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 6, 6) };
                b.SetResourceReference(StyleProperty, "FormButton");
                if (primary)
                    b.SetResourceReference(BackgroundProperty, "AccentBrush");
                b.Click += (_, __) => click();
                panel.Children.Add(b);
                return b;
            }
            Make(T("mj_load", "Load into creator"), true, async () => await LoadIntoCreatorAsync(e));
            Make(T("mj_copyjob", "Open in Copy Job"), false, () => MainWindow.Instance?.OpenInCopyJob(Cloud + e.Job.Path0));
            Make(T("mj_link", "Copy link"), false, () =>
            {
                try { Clipboard.SetText("https://socialclub.rockstargames.com/job/gtav/" + e.Job.ContentId); }
                catch (Exception ex) { Log.Debug("my jobs: clipboard: " + ex.Message, source: "myjobs"); }
            });
            if (e.Mission?["gen"]?["start"] is JObject start)
                Make("✈ " + T("mj_fly", "Fly to start"), false, () =>
                    GTA.Teleport(new XenVector3((float)start["x"], (float)start["y"], (float)start["z"] + 2)));
            return panel;
        }

        private async Task LoadIntoCreatorAsync(Entry e)
        {
            var window = MainWindow.Instance;
            if (window == null)
                return;
            int type = (int?)e.Mission?["gen"]?["type"] ?? -1, subtype = (int?)e.Mission?["gen"]?["subtype"] ?? -1;
            string wanted = e.Mission == null ? null : JobLoader.CreatorFor(type, subtype);
            string open = CreatorMap.CurrentCreator();
            if (wanted != null && open != wanted)
            {
                await window.ConfirmAsync(T("mj_wrong_creator_title", "Other creator"),
                    string.Format(CultureInfo.CurrentCulture, T("mj_wrong_creator", "This job belongs in the {0}. Open it in GTA, then load the job."), MainWindow.CreatorDisplayName(wanted)),
                    "OK", null);
                return;
            }
            bool go = await window.ConfirmAsync(T("mj_load_title", "Load this job?"),
                string.Format(CultureInfo.CurrentCulture, T("mj_load_text", "GTA opens \"{0}\" in the creator. What is open there now and not saved is lost."), Name(e)),
                T("mj_load", "Load into creator"), T("dialog_cancel", "Cancel"));
            if (!go)
                return;
            var result = await JobLoader.LoadAsync(e.Job.ContentId, Cloud + e.Job.Path0);
            if (result != JobLoader.Result.Loaded)
                await window.ConfirmAsync(T("mj_load_failed_title", "Not loaded"),
                    result == JobLoader.Result.NotEditing
                        ? T("copy_full_busy", "The creator is busy right now (test, saving, menu change). Try again in a moment.")
                        : T("copy_full_timeout", "The creator did not load the job. Look in GTA for a message."),
                    "OK", null);
        }

        // ----- map -----

        private void FillMap(Entry e, bool keepView)
        {
            var markers = new List<JobMap.Marker>();
            var mission = e.Mission;
            if (mission != null)
            {
                foreach (var layer in Layers)
                {
                    if (_layersOff.Contains(layer.Section))
                        continue;
                    foreach (var p in Points(mission, layer.Section, layer.Pos))
                        markers.Add(new JobMap.Marker(p.X, p.Y, B(layer.Brush), 4));
                }
                if (mission["gen"]?["start"] is JObject start)
                    markers.Add(new JobMap.Marker((double)start["x"], (double)start["y"], B("StartPointBrush"), 9));
            }
            _map.SetMarkers(markers, keepView);
        }

        private static IEnumerable<Point> Points(JObject mission, string section, string key)
        {
            if (!(mission[section]?[key] is JArray list))
                yield break;
            foreach (var item in list.OfType<JObject>())
            {
                double x = (double?)item["x"] ?? 0, y = (double?)item["y"] ?? 0;
                if (x != 0 || y != 0)
                    yield return new Point(x, y);
            }
        }

        private FrameworkElement MapControls(bool big)
        {
            var layer = new Grid();
            var buttons = new StackPanel { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(8) };
            Button Small(string text, string tip, Action click)
            {
                var b = new Button { Content = text, Width = 30, Height = 30, FontSize = 15, Margin = new Thickness(0, 0, 0, 4), ToolTip = tip, Padding = new Thickness(0) };
                b.SetResourceReference(StyleProperty, "FormButton");
                b.Click += (_, __) => click();
                buttons.Children.Add(b);
                return b;
            }
            Small("+", T("mj_zoom_in", "Zoom in"), () => _map.ZoomBy(1.5));
            Small("−", T("mj_zoom_out", "Zoom out"), () => _map.ZoomBy(1 / 1.5));
            Small("◎", T("mj_fit", "Show the whole job"), () => _map.Fit());
            Small(big ? "×" : "⛶", big ? T("mj_map_close", "Close") : T("mj_map_big", "Full size"), () => ExpandMap(!big));
            layer.Children.Add(buttons);

            var legend = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(8) };
            var legendCard = new Border { CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 5, 8, 5), Child = legend, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(8) };
            legendCard.Background = new SolidColorBrush(Color.FromArgb(0xE0, 0x20, 0x22, 0x25));
            var e = _current;
            foreach (var l in Layers)
            {
                int count = Points(e?.Mission ?? new JObject(), l.Section, l.Pos).Count();
                if (count == 0)
                    continue;
                var box = new CheckBox { IsChecked = !_layersOff.Contains(l.Section), FontSize = big ? 13 : 11.5, Margin = new Thickness(0, 1, 0, 1) };
                box.Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Ellipse { Width = 8, Height = 8, Fill = B(l.Brush), Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center },
                        new TextBlock { Text = $"{T(l.Key, l.Fallback)} {count}", Foreground = Brushes.White },
                    }
                };
                string section = l.Section;
                box.Click += (_, __) =>
                {
                    if (box.IsChecked == true) _layersOff.Remove(section); else _layersOff.Add(section);
                    if (_current != null)
                        FillMap(_current, keepView: true);
                };
                legend.Children.Add(box);
            }
            if (legend.Children.Count > 0)
                layer.Children.Add(legendCard);
            return layer;
        }

        private void ExpandMap(bool big)
        {
            if (_map.Parent is Panel from)
                from.Children.Remove(_map);
            if (big)
            {
                _overlay.Children.Clear();
                var frame = new Grid { Margin = new Thickness(10) };
                frame.Children.Add(_map);
                frame.Children.Add(MapControls(true));
                _overlay.Children.Add(frame);
                _overlay.Visibility = Visibility.Visible;
            }
            else
            {
                _overlay.Children.Clear();
                _overlay.Visibility = Visibility.Collapsed;
                ShowDetail();
            }
        }

        // ----- bottom: general and map objects -----

        private FrameworkElement Tables(Entry e)
        {
            var grid = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var line = new Border { Height = 1, Margin = new Thickness(0, 0, 0, 12), VerticalAlignment = VerticalAlignment.Top };
            line.SetResourceReference(Border.BackgroundProperty, "LineBrush");
            SetColumnSpan(line, 3);
            grid.Children.Add(line);

            var mission = e.Mission;
            if (mission == null)
            {
                grid.Children.Add(Muted(T("mj_file_loading", "Loading the job file …"), new Thickness(0, 14, 0, 0)));
                return grid;
            }
            var gen = mission["gen"] as JObject ?? new JObject();
            var rule = mission["rule"] as JObject ?? new JObject();

            var general = Table(T("mj_general", "General"));
            Row(general, T("copy_players", "Players"), $"{(int?)gen["min"] ?? 0}–{(int?)gen["num"] ?? 0}");
            if (((int?)gen["tnum"] ?? 0) > 0)
                Row(general, T("teams", "Teams"), ((int?)gen["tnum"]).ToString());
            Row(general, T("mj_rank", "Rank"), ((int?)gen["rank"])?.ToString() ?? "–");
            // Same names as Copy Job (rule.weth and rule.tod are indices into the creator's lists).
            string current = T("cj_w_current", "Current");
            Row(general, T("cj_weather", "Weather"), Pick(rule["weth"], current, "EXTRASUNNY", T("cj_w_rain", "Rain"), T("cj_w_snow", "Snow"), "SMOG", "Halloween", "Halloween 2",
                T("cj_w_clear", "Clear"), T("cj_w_clouds", "Clouds"), T("cj_w_overcast", "Overcast"), T("cj_w_thunder", "Thunder"), T("cj_w_foggy", "Foggy")));
            Row(general, T("cj_time", "Time of day"), Pick(rule["tod"], current, T("cj_t_morning", "Morning"), T("cj_t_noon", "Noon"), T("cj_t_night", "Night"), T("cj_t_dark", "Really dark")));
            Row(general, T("mj_traffic", "Traffic"), Level(rule["traf"]));
            Row(general, T("mj_peds", "Pedestrians"), Level(rule["apeds"]));
            Row(general, "dlcrel", ((int?)gen["dlcrel"])?.ToString() ?? "–");
            SetColumn(general, 0);
            general.Margin = new Thickness(0, 14, 0, 0);
            grid.Children.Add(general);

            var objects = Table(T("mj_mapobjects", "Map objects"));
            string creator = JobLoader.CreatorFor((int?)gen["type"] ?? -1, (int?)gen["subtype"] ?? -1) ?? "";
            Bar(objects, T("map_part_prop", "Props"), "PropBrush", Count(mission, "prop"), Xenvious.AdvancedPlacement.PropPlacementService.PropLimit);
            Bar(objects, T("dash_dynprops", "Dynamic Props"), "DynamicPropBrush", Count(mission, "dprop"), 32);
            Bar(objects, T("editnav_fixtures", "Fixtures"), "FixtureBrush", Count(mission, "dhprop"), CreatorLimits.Fixtures);
            Bar(objects, T("rl_m_vehs", "Vehicles"), "VehicleBrush", Count(mission, "veh"), CreatorLimits.Vehicles);
            Bar(objects, T("missionrspweapons", "Weapons"), "WeaponBrush", Count(mission, "weap"), CreatorLimits.Weapons);
            Bar(objects, T("dash_actors", "Actors"), "ActorBrush", Count(mission, "ene"), CreatorLimits.Actors(creator));
            Bar(objects, T("mj_zones", "Zones"), "ZoneBrush", Count(mission, "zone"), CreatorLimits.Zones);
            Bar(objects, T("mj_objects", "Objects"), "ZoneBrush", Count(mission, "obj"), 0);
            SetColumn(objects, 2);
            objects.Margin = new Thickness(0, 14, 0, 0);
            grid.Children.Add(objects);
            return grid;
        }

        private static int Count(JObject mission, string section) => (int?)mission[section]?["no"] ?? 0;

        private static string Pick(JToken value, params string[] names)
        {
            int i = (int?)value ?? 0;
            return i >= 0 && i < names.Length ? names[i] : i.ToString(CultureInfo.CurrentCulture);
        }

        // traf and apeds: an index the mission controller turns into a density (MainWindow.MissionDensityIndex).
        private static readonly string[] Densities = { "0%", "20%", "50%", "100%", "85%" };

        private static string Level(JToken value)
        {
            int? i = (int?)value;
            return i == null ? "–" : i >= 0 && i < Densities.Length ? Densities[i.Value] : i.Value.ToString(CultureInfo.CurrentCulture);
        }

        private static Grid Table(string title)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var t = new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            SetColumnSpan(t, 2);
            g.Children.Add(t);
            return g;
        }

        private static void Row(Grid table, string label, string value) => AddRow(table, Label(label, null), new TextBlock { Text = value, Foreground = B("TextColor") });

        private static void Bar(Grid table, string label, string brush, int count, int max)
        {
            var value = new StackPanel();
            string text = max > 0 ? $"{count}/{max}" : count.ToString(CultureInfo.CurrentCulture);
            var number = new TextBlock { Text = count == 0 ? "–" : text, Foreground = B(count > max && max > 0 ? "BadBrush" : "TextColor") };
            value.Children.Add(number);
            if (max > 0 && count > 0)
            {
                var track = new Grid { Height = 5, Margin = new Thickness(0, 3, 0, 0) };
                track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Min(count, max), GridUnitType.Star) });
                track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Max(0, max - count), GridUnitType.Star) });
                var back = new Border { CornerRadius = new CornerRadius(2.5) };
                back.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                SetColumnSpan(back, 2);
                track.Children.Add(back);
                track.Children.Add(new Border { CornerRadius = new CornerRadius(2.5), Background = B(brush) });
                value.Children.Add(track);
            }
            AddRow(table, Label(label, brush), value);
        }

        private static FrameworkElement Label(string text, string brush)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            if (brush != null)
                panel.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = B(brush), Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center });
            var t = new TextBlock { Text = text };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            panel.Children.Add(t);
            return panel;
        }

        private static void AddRow(Grid table, FrameworkElement label, FrameworkElement value)
        {
            int row = table.RowDefinitions.Count;
            table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            label.Margin = new Thickness(0, 5, 8, 5);
            value.Margin = new Thickness(0, 5, 0, 5);
            SetRow(label, row);
            SetRow(value, row);
            SetColumn(value, 1);
            table.Children.Add(label);
            table.Children.Add(value);
            var line = new Border { Height = 1, VerticalAlignment = VerticalAlignment.Bottom, Opacity = 0.5 };
            line.SetResourceReference(Border.BackgroundProperty, "LineBrush");
            SetRow(line, row);
            SetColumnSpan(line, 2);
            table.Children.Add(line);
        }
    }
}
