using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Play area 1 and 2 on one page (mockup: https://claude.ai/artifact/9atfQY2DhBMzuCuPFaaS7q):
    /// area, team, rule and "apply to" on top, then what the area does, its shape and position
    /// (AreaEditor), what it follows (EntityPicker), leaving, look and more. Memory: PlayAreas.
    /// </summary>
    public class PlayAreaView : DockPanel
    {
        private int _area = 1, _team, _rule;
        private bool _allRules, _allTeams;
        private bool _loading;
        private bool _built;

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        private readonly List<Action> _loaders = new List<Action>();
        private readonly List<ToggleButton> _areaTabs = new List<ToggleButton>(), _teamTabs = new List<ToggleButton>(), _scopeTabs = new List<ToggleButton>();
        private readonly TextBlock _ruleText = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 15, MinWidth = 28, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        private FrameworkElement _explain;
        private Border _explain1, _explain2;

        // Position boxes: the AreaEditors read and write them, the TextChanged handlers write the game.
        private readonly TextBox _cx = Box(), _cy = Box(), _cz = Box(), _radius = Box();
        private readonly TextBox _ex = Hidden(), _ey = Hidden(), _ez = Hidden();
        private readonly TextBox _p1x = Box(), _p1y = Box(), _p1z = Box(), _p2x = Box(), _p2y = Box(), _p2z = Box(), _width = Box();
        private readonly AreaEditor _sphereEditor = new AreaEditor { Kind = AreaShape.Sphere };
        private readonly AreaEditor _boxEditor = new AreaEditor { Kind = AreaShape.AngledBox };
        private FrameworkElement _sphereFields, _boxFields;
        private readonly EntityPicker _follow = new EntityPicker();
        private FrameworkElement _followNote, _followBody, _spawnAheadRow;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public PlayAreaView()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Load(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => Load();
        }

        private static TextBox Box()
        {
            var box = new TextBox { Height = 30 };
            box.SetResourceReference(StyleProperty, "Watermark");
            return box;
        }

        private static TextBox Hidden() => new TextBox { Visibility = Visibility.Collapsed };

        private bool Live => _built && MainWindow.m != null && MainWindow.m.IsProcOpen && PlayAreas.Ready;

        // ----- writing: to this rule, all rules or all teams -----

        private IEnumerable<(int Team, int Rule)> Targets => PlayAreas.Targets(_team, _rule, _allRules, _allTeams);

        private void WriteInt(int field, int value)
        {
            if (_loading || !Live) return;
            foreach (var (t, r) in Targets)
                new Global(PlayAreas.Field(_area, t, r, field)).SetInt(value);
        }

        private void WriteFloat(int field, float value)
        {
            if (_loading || !Live) return;
            foreach (var (t, r) in Targets)
                new Global(PlayAreas.Field(_area, t, r, field)).SetFloat(value);
        }

        private void WriteBit(int bit, bool on)
        {
            if (_loading || !Live) return;
            foreach (var (t, r) in Targets)
            {
                var g = new Global(PlayAreas.Field(_area, t, r, PlayAreas.BoundsBS));
                int v = g.Get<int>();
                g.SetInt(on ? v | (1 << bit) : v & ~(1 << bit));
            }
        }

        private void ForTargets(Action<int, int> write)
        {
            if (_loading || !Live) return;
            foreach (var (t, r) in Targets)
                write(t, r);
        }

        // ----- building -----

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            LastChildFill = true;

            var top = TopBar();
            DockPanel.SetDock(top, Dock.Top);
            Children.Add(top);

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            var page = new StackPanel { Margin = new Thickness(0, 12, 0, 12) };
            _explain = Explanations();
            page.Children.Add(_explain);
            var masonry = new MasonryPanel();
            masonry.Children.Add(Column(ModeCard(), ShapeCard()));
            masonry.Children.Add(Column(FollowCard(), LeaveCard()));
            masonry.Children.Add(Column(LookCard(), MoreCard()));
            page.Children.Add(masonry);
            scroll.Content = page;
            Children.Add(scroll);

            _sphereEditor.Attach(_cx, _cy, _cz, _ex, _ey, _ez, _radius);
            _boxEditor.Attach(_p1x, _p1y, _p1z, _p2x, _p2y, _p2z, _width);
            SelectTabs();
            Load();
        }

        private static StackPanel Column(params FrameworkElement[] cards)
        {
            var col = new StackPanel();
            foreach (var c in cards)
                col.Children.Add(c);
            return col;
        }

        private Border Card(string key, string fallback, FrameworkElement body, FrameworkElement headerRight = null)
        {
            var header = new DockPanel();
            if (headerRight != null)
            {
                DockPanel.SetDock(headerRight, Dock.Right);
                header.Children.Add(headerRight);
            }
            header.Children.Add(new TextBlock { Style = (Style)FindResource("DashCardTitle"), Text = T(key, fallback) });
            var dock = new DockPanel();
            var head = new Border { Style = (Style)FindResource("DashCardHeader"), Child = header };
            DockPanel.SetDock(head, Dock.Top);
            dock.Children.Add(head);
            body.Margin = new Thickness(14, 12, 14, 8);
            dock.Children.Add(body);
            return new Border { Style = (Style)FindResource("DashCard"), Margin = new Thickness(0, 0, 0, 12), Child = dock };
        }

        private TextBlock Label(string key, string fallback) => new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = T(key, fallback) };

        private TextBlock Hint(string key, string fallback)
        {
            var hint = new TextBlock { Text = T(key, fallback), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return hint;
        }

        private ToggleButton Tile(string text, string icon = null)
        {
            object content = text;
            if (icon != null)
            {
                var panel = new StackPanel();
                var path = new System.Windows.Shapes.Path { Data = Geometry.Parse(icon), StrokeThickness = 1.5, Width = 26, Height = 20, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center };
                path.SetBinding(System.Windows.Shapes.Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(ToggleButton), 1) });
                panel.Children.Add(path);
                panel.Children.Add(new TextBlock { Text = text, FontSize = 12.5, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 4, 0, 0) });
                content = panel;
            }
            return new ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = content, Margin = new Thickness(0, 0, 6, 6) };
        }

        // A labelled switch; load reads it, click writes it.
        private FrameworkElement Toggle(string key, string fallback, Func<bool> read, Action<bool> write, string tipKey = null, string tipFallback = null)
        {
            var row = new Grid { Style = (Style)FindResource("FormRow") };
            if (tipKey != null)
                row.ToolTip = T(tipKey, tipFallback);
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new TextBlock { Style = (Style)FindResource("FormLabel"), Text = T(key, fallback) });
            var box = new CheckBox { Style = (Style)FindResource("FormToggle") };
            box.Click += (_, __) => write(box.IsChecked == true);
            Grid.SetColumn(box, 1);
            row.Children.Add(box);
            _loaders.Add(() => box.IsChecked = read());
            return row;
        }

        private FrameworkElement BitToggle(int bit, string key, string fallback, string tipKey = null, string tipFallback = null)
            => Toggle(key, fallback, () => (PlayAreas.GetInt(_area, _team, _rule, PlayAreas.BoundsBS) & (1 << bit)) != 0, on => WriteBit(bit, on), tipKey, tipFallback);

        // A number box bound to an int field.
        private FrameworkElement IntField(string key, string fallback, int field, string hintKey = null, string hintFallback = null)
        {
            var panel = new StackPanel();
            panel.Children.Add(Label(key, fallback));
            var box = Box();
            box.Margin = new Thickness(0, 0, 0, hintKey == null ? 10 : 4);
            box.TextChanged += (_, __) => { if (int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v)) WriteInt(field, v); };
            panel.Children.Add(box);
            if (hintKey != null)
                panel.Children.Add(Hint(hintKey, hintFallback));
            _loaders.Add(() => { if (!box.IsKeyboardFocused) box.Text = PlayAreas.GetInt(_area, _team, _rule, field).ToString(CultureInfo.InvariantCulture); });
            return panel;
        }

        private FrameworkElement TopBar()
        {
            var bar = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };

            FrameworkElement Segment(List<ToggleButton> list, string[] labels, Action<int> pick)
            {
                var seg = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 16, 6) };
                for (int i = 0; i < labels.Length; i++)
                {
                    int n = i;
                    var b = new ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = labels[i], MinHeight = 32, Height = 32, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 4, 0), FontSize = 13.5 };
                    b.Click += (_, __) => { pick(n); SelectTabs(); Load(); };
                    list.Add(b);
                    seg.Children.Add(b);
                }
                return seg;
            }

            bar.Children.Add(Segment(_areaTabs, new[] { T("pa_area1", "Play area 1"), T("pa_area2", "Play area 2") }, n => _area = n + 1));
            var help = new Button { Style = (Style)FindResource("NavButton"), Content = new TextBlock { Text = "?", FontWeight = FontWeights.Bold, FontSize = 15, Margin = new Thickness(4, 0, 4, 0) },
                ToolTip = T("pa_help_tip", "What are play area 1 and 2?"), Margin = new Thickness(-10, 0, 16, 6), VerticalAlignment = VerticalAlignment.Center };
            help.Click += (_, __) =>
            {
                bool show = _explain.Visibility != Visibility.Visible;
                _explain.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "pahelp", show ? 1 : 0);
            };
            bar.Children.Add(help);
            bar.Children.Add(Segment(_teamTabs, Enumerable.Range(1, PlayAreas.Teams).Select(i => T("actorteam", "Team") + " " + i).ToArray(), n => _team = n));

            var step = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 16, 6) };
            var ruleLabel = new TextBlock { Text = T("pa_rule", "Rule"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), FontSize = 14 };
            ruleLabel.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
            step.Children.Add(ruleLabel);
            foreach (var (glyph, delta) in new[] { ("‹", -1), ("›", 1) })
            {
                var b = new Button { Style = (Style)FindResource("EntryStepButton"), Content = glyph };
                b.Click += (_, __) => { _rule = Math.Max(0, Math.Min(PlayAreas.Rules - 1, _rule + delta)); SelectTabs(); Load(); };
                if (delta < 0) { step.Children.Add(b); step.Children.Add(_ruleText); }
                else step.Children.Add(b);
            }
            bar.Children.Add(step);
            bar.Children.Add(Segment(_scopeTabs, new[] { T("pa_scope_rule", "This rule only"), T("pa_scope_rules", "All rules"), T("pa_scope_teams", "All teams") }, n =>
            {
                _allRules = n >= 1;
                _allTeams = n == 2;
            }));
            return bar;
        }

        private void SelectTabs()
        {
            for (int i = 0; i < _areaTabs.Count; i++) _areaTabs[i].IsChecked = i == _area - 1;
            for (int i = 0; i < _teamTabs.Count; i++) _teamTabs[i].IsChecked = i == _team;
            int scope = _allTeams ? 2 : _allRules ? 1 : 0;
            for (int i = 0; i < _scopeTabs.Count; i++) _scopeTabs[i].IsChecked = i == scope;
            _ruleText.Text = (_rule + 1).ToString(CultureInfo.CurrentCulture);
            if (_explain1 != null)
            {
                _explain1.SetResourceReference(Border.BorderBrushProperty, _area == 1 ? "AccentBrush" : "LineBrush");
                _explain2.SetResourceReference(Border.BorderBrushProperty, _area == 2 ? "AccentBrush" : "LineBrush");
            }
        }

        private FrameworkElement Explanations()
        {
            var grid = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, -12, 0) };
            Border Explain(string titleKey, string titleFb, string key, string fallback)
            {
                var body = new TextBlock { Text = T(key, fallback), TextWrapping = TextWrapping.Wrap, FontSize = 13.5, LineHeight = 20 };
                body.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                var card = Card(titleKey, titleFb, body);
                card.Margin = new Thickness(0, 0, 12, 12);
                card.BorderThickness = new Thickness(1);
                return card;
            }
            _explain1 = Explain("pa_area1", "Play area 1", "pa_explain1",
                "The main area of a rule: where the team may be while the rule runs.\n• Stay inside: whoever leaves gets a countdown, then fails or dies.\n• Stay outside: a no-go zone nobody may enter.\n• Respawn here: players who die come back inside it.\nTypical: limit the map of an LTS or a capture.");
            _explain2 = Explain("pa_area2", "Play area 2", "pa_explain2",
                "A second area for the same rule, on top of area 1, with its own mode, timer and wanted level.\n• No-go zone inside the play area: area 1 \"stay inside\", area 2 \"stay outside\", e.g. a roof or a base.\n• A second region where players may or must be.\n• Respawn only: no border, just the place players restart at (hence the old name \"Respawn area\").\nIf you are not sure you need it: you don't.");
            grid.Children.Add(_explain1);
            grid.Children.Add(_explain2);
            grid.Visibility = new ini_reader(Functions.getRoamingConfigFilePath()).ReadInteger("Settings", "pahelp", 1) == 1 ? Visibility.Visible : Visibility.Collapsed;
            return grid;
        }

        // ----- cards -----

        private Border ModeCard()
        {
            var body = new StackPanel();
            var tiles = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, -6, 4) };
            var modes = new[]
            {
                (PlayAreas.Mode.Off, T("pa_mode_off", "Off"), "M6,10 A7,7 0 1 1 20,10 A7,7 0 1 1 6,10 M8,15 L18,5"),
                (PlayAreas.Mode.StayInside, T("pa_mode_in", "Stay inside"), "M3,3 H23 V17 H3 Z M11,10 A2,2 0 1 1 15,10 A2,2 0 1 1 11,10"),
                (PlayAreas.Mode.StayOutside, T("pa_mode_out", "Stay outside"), "M3,3 H23 V17 H3 Z M8,6 L18,14 M18,6 L8,14"),
                (PlayAreas.Mode.SpawnOnly, T("pa_mode_spawn", "Respawn only"), "M13,3 V13 M9,9 L13,13 L17,9 M5,17 H21"),
            };
            var hint = Hint("pa_mode_hint_in", "Whoever leaves the area gets a countdown.");
            var buttons = new List<(PlayAreas.Mode, ToggleButton)>();
            string HintFor(PlayAreas.Mode m) => m == PlayAreas.Mode.StayInside ? T("pa_mode_hint_in", "Whoever leaves the area gets a countdown.")
                : m == PlayAreas.Mode.StayOutside ? T("pa_mode_hint_out", "Whoever enters the area gets a countdown. Good for no-go zones.")
                : m == PlayAreas.Mode.SpawnOnly ? T("pa_mode_hint_spawn", "No border and no countdown: only where players restart.")
                : T("pa_mode_hint_off", "This area does nothing on this rule.");
            foreach (var (mode, text, icon) in modes)
            {
                var b = Tile(text, icon);
                var m = mode;
                b.Click += (_, __) =>
                {
                    ForTargets((t, r) => PlayAreas.SetMode(_area, t, r, m));
                    foreach (var (bm, bb) in buttons) bb.IsChecked = bm == m;
                    hint.Text = HintFor(m);
                };
                buttons.Add((mode, b));
                tiles.Children.Add(b);
            }
            _loaders.Add(() =>
            {
                var m = PlayAreas.RuleBitsReady ? PlayAreas.GetMode(_area, _team, _rule) : PlayAreas.Mode.Off;
                foreach (var (bm, bb) in buttons) { bb.IsChecked = bm == m; bb.IsEnabled = PlayAreas.RuleBitsReady; }
                hint.Text = HintFor(m);
            });
            body.Children.Add(tiles);
            body.Children.Add(hint);

            // Return timer: one per team for each area, in ms (default 60000).
            body.Children.Add(Label("pa_timer", "Time to get back (seconds, per team)"));
            var timer = Box();
            timer.Margin = new Thickness(0, 0, 0, 10);
            timer.TextChanged += (_, __) =>
            {
                if (_loading || !Live || !double.TryParse(timer.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double sec)) return;
                for (int t = 0; t < PlayAreas.Teams; t++)
                    if (_allTeams || t == _team)
                        new Global(PlayAreas.Timer(_area, t)).SetInt((int)Math.Round(sec * 1000));
            };
            _loaders.Add(() => { if (!timer.IsKeyboardFocused) timer.Text = (new Global(PlayAreas.Timer(_area, _team)).Get<int>() / 1000.0).ToString("0.#", CultureInfo.InvariantCulture); });
            body.Children.Add(timer);
            body.Children.Add(Toggle("pa_spawnhere", "Respawn in this area", () => PlayAreas.GetSpawn(_area, _team, _rule), on => ForTargets((t, r) => PlayAreas.SetSpawn(_area, t, r, on))));
            body.Children.Add(Toggle("pa_wanted", "Wanted level for leaving", () => PlayAreas.GetWanted(_area, _team, _rule), on => ForTargets((t, r) => PlayAreas.SetWanted(_area, t, r, on))));

            var stars = new UniformGrid { Columns = 6, Margin = new Thickness(0, 4, -6, 6) };
            var starButtons = new List<ToggleButton>();
            for (int w = -1; w <= 5; w++)
            {
                if (w == 0) continue;
                var b = Tile(w < 0 ? T("pa_nowanted", "None") : new string('★', w));
                b.MinHeight = 30; b.Height = 30; b.Tag = w;
                int level = w;
                b.Click += (_, __) => { WriteInt(PlayAreas.WantedToGive, level); foreach (var sb in starButtons) sb.IsChecked = (int)sb.Tag == level; };
                starButtons.Add(b);
                stars.Children.Add(b);
            }
            _loaders.Add(() => { int wl = PlayAreas.GetInt(_area, _team, _rule, PlayAreas.WantedToGive); foreach (var sb in starButtons) sb.IsChecked = (int)sb.Tag == (wl <= 0 ? -1 : wl); });
            body.Children.Add(stars);
            return Card("pa_card_mode", "What the area does", body);
        }

        private Border ShapeCard()
        {
            var body = new StackPanel();
            var tiles = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, -6, 6) };
            var sphere = Tile(T("sphere", "Sphere"), "M5,10 A8,8 0 1 1 21,10 A8,8 0 1 1 5,10 M5,10 A8,3 0 1 0 21,10");
            var box = Tile(T("zs_boxrot", "Box"), "M8,3 L24,8 L20,19 L4,14 Z");
            sphere.Click += (_, __) => { WriteInt(PlayAreas.Type, 0); ShowShape(0); };
            box.Click += (_, __) => { WriteInt(PlayAreas.Type, 1); ShowShape(1); };
            tiles.Children.Add(sphere);
            tiles.Children.Add(box);
            body.Children.Add(tiles);
            body.Children.Add(Hint("pa_shape_hint", "Sphere: centre and radius, can follow a target. Box: two points, width and height."));

            // Sphere: vPos + fRadius.
            var sphereFields = new StackPanel();
            sphereFields.Children.Add(Xyz("pa_centre", "Centre", _cx, _cy, _cz, () => _sphereEditor.StartPicked()));
            sphereFields.Children.Add(Label("pa_radius", "Radius"));
            _radius.Margin = new Thickness(0, 0, 0, 6);
            sphereFields.Children.Add(_radius);
            sphereFields.Children.Add(_sphereEditor);
            _sphereFields = sphereFields;
            Bind3(_cx, _cy, _cz, PlayAreas.Pos);
            BindFloat(_radius, PlayAreas.Radius);
            // The sphere editor needs an end point; it is the centre.
            _cx.TextChanged += (_, __) => _ex.Text = _cx.Text;
            _cy.TextChanged += (_, __) => _ey.Text = _cy.Text;
            _cz.TextChanged += (_, __) => _ez.Text = _cz.Text;

            // Box: vPos1, vPos2, fWidth.
            var boxFields = new StackPanel();
            boxFields.Children.Add(Xyz("pastart", "Startpoint", _p1x, _p1y, _p1z, () => _boxEditor.StartPicked()));
            boxFields.Children.Add(Xyz("paend", "Endpoint", _p2x, _p2y, _p2z, () => _boxEditor.EndPicked()));
            boxFields.Children.Add(Label("width", "Width"));
            _width.Margin = new Thickness(0, 0, 0, 6);
            boxFields.Children.Add(_width);
            boxFields.Children.Add(_boxEditor);
            _boxFields = boxFields;
            Bind3(_p1x, _p1y, _p1z, PlayAreas.Pos1);
            Bind3(_p2x, _p2y, _p2z, PlayAreas.Pos2);
            BindFloat(_width, PlayAreas.Width);

            body.Children.Add(sphereFields);
            body.Children.Add(boxFields);
            _loaders.Add(() =>
            {
                int type = PlayAreas.GetInt(_area, _team, _rule, PlayAreas.Type);
                sphere.IsChecked = type == 0;
                box.IsChecked = type == 1;
                ShowShape(type);
            });
            return Card("pa_card_shape", "Shape and position", body, new TextBlock { Text = "outbt", FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Foreground = MainWindow.ThemeBrush("FaintTextBrush") });
        }

        private void ShowShape(int type)
        {
            _sphereFields.Visibility = type == 1 ? Visibility.Collapsed : Visibility.Visible;
            _boxFields.Visibility = type == 1 ? Visibility.Visible : Visibility.Collapsed;
            // Only the centre is replaced by the target's position, so only a sphere can follow.
            _followNote.Visibility = type == 1 ? Visibility.Visible : Visibility.Collapsed;
            _followBody.IsEnabled = type != 1;
        }

        private FrameworkElement Xyz(string key, string fallback, TextBox x, TextBox y, TextBox z, Action picked)
        {
            var panel = new StackPanel();
            panel.Children.Add(Label(key, fallback));
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            foreach (var w in new[] { "*", "6", "*", "6", "*", "6", "30" })
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = w == "*" ? new GridLength(1, GridUnitType.Star) : new GridLength(double.Parse(w, CultureInfo.InvariantCulture)) });
            int col = 0;
            foreach (var (axis, box) in new[] { ("X", x), ("Y", y), ("Z", z) })
            {
                var cell = new StackPanel();
                cell.Children.Add(new TextBlock { Style = (Style)FindResource("FieldAxis"), Text = axis });
                cell.Children.Add(box);
                Grid.SetColumn(cell, col);
                grid.Children.Add(cell);
                col += 2;
            }
            var pick = new Button { Style = (Style)FindResource("FieldIconButton"), Width = 30, Height = 30, VerticalAlignment = VerticalAlignment.Bottom, ToolTip = T("getcursorloc", "Get Cursor Location") };
            var icon = new System.Windows.Shapes.Path { Data = Geometry.Parse("M8,1.5 L8,4.5 M8,11.5 L8,14.5 M1.5,8 L4.5,8 M11.5,8 L14.5,8 M8,5 A3,3 0 1 1 7.99,5"), StrokeThickness = 1.6, Width = 16, Height = 16 };
            icon.SetBinding(System.Windows.Shapes.Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(Button), 1) });
            pick.Content = icon;
            pick.Click += (_, __) =>
            {
                if (!Live) return;
                var loc = Functions.Read.GetLocationVec();
                x.Text = loc.X.ToString(CultureInfo.CurrentCulture);
                y.Text = loc.Y.ToString(CultureInfo.CurrentCulture);
                z.Text = loc.Z.ToString(CultureInfo.CurrentCulture);
                picked();
            };
            Grid.SetColumn(pick, 6);
            grid.Children.Add(pick);
            panel.Children.Add(grid);
            return panel;
        }

        private static bool ParseFloat(string text, out float v)
            => float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out v) || float.TryParse(text?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out v);

        private void BindFloat(TextBox box, int field)
        {
            box.TextChanged += (_, __) => { if (ParseFloat(box.Text, out float v)) WriteFloat(field, v); };
            _loaders.Add(() => { if (!box.IsKeyboardFocused) box.Text = PlayAreas.GetFloat(_area, _team, _rule, field).ToString(CultureInfo.CurrentCulture); });
        }

        private void Bind3(TextBox x, TextBox y, TextBox z, int field)
        {
            BindFloat(x, field);
            BindFloat(y, field + 1);
            BindFloat(z, field + 2);
        }

        private Border FollowCard()
        {
            var body = new StackPanel();
            var note = new Border { CornerRadius = new CornerRadius(0, 6, 6, 0), BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 10) };
            note.SetResourceReference(Border.BorderBrushProperty, "WarnBrush");
            note.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            var noteText = new TextBlock { Text = T("pa_follow_box", "Following only works with the sphere: the game only moves the centre; for a box both points would sit on the target."), TextWrapping = TextWrapping.Wrap, FontSize = 12.5 };
            noteText.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            note.Child = noteText;
            _followNote = note;
            body.Children.Add(note);

            var inner = new StackPanel();
            inner.Children.Add(_follow);
            _follow.Changed += (type, id) =>
            {
                WriteInt(PlayAreas.EntityType, type);
                WriteInt(PlayAreas.EntityId, id);
                _spawnAheadRow.Visibility = type == EntityPicker.Team ? Visibility.Visible : Visibility.Collapsed;
            };
            inner.Children.Add(BitToggle(5, "pa_look", "Look towards the target"));
            _spawnAheadRow = IntField("pa_ahead", "Respawn ahead of the nearest team member (%)", PlayAreas.SpawnAhead);
            inner.Children.Add(_spawnAheadRow);
            _followBody = inner;
            body.Children.Add(inner);
            _loaders.Add(() =>
            {
                int type = PlayAreas.GetInt(_area, _team, _rule, PlayAreas.EntityType);
                _follow.Set(type, PlayAreas.GetInt(_area, _team, _rule, PlayAreas.EntityId));
                _spawnAheadRow.Visibility = type == EntityPicker.Team ? Visibility.Visible : Visibility.Collapsed;
            });
            return Card("pa_card_follow", "Follow", body, new TextBlock { Text = "outety / outeid", FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Foreground = MainWindow.ThemeBrush("FaintTextBrush") });
        }

        private Border LeaveCard()
        {
            var body = new StackPanel();
            // Leave text: one string per team and rule (bfm), not part of the bounds struct.
            body.Children.Add(Label("pa_leavetext", "Text when leaving"));
            var text = Box();
            text.Margin = new Thickness(0, 0, 0, 10);
            text.TextChanged += (_, __) =>
            {
                if (_loading || !Live || GTA.Offsets.Editor.bfm == 0 || _area != 1) return;
                ForTargets((t, r) => new Global(GTA.Offsets.Editor.bfm + t * GTA.Offsets.Editor.team_NEXT + r * GTA.Offsets.Editor.txt_NEXT).SetString(text.Text));
            };
            var textPanel = new StackPanel();
            textPanel.Children.Add(text);
            _loaders.Add(() =>
            {
                textPanel.Visibility = _area == 1 ? Visibility.Visible : Visibility.Collapsed;
                if (!text.IsKeyboardFocused && GTA.Offsets.Editor.bfm != 0)
                    text.Text = new Global(GTA.Offsets.Editor.bfm + _team * GTA.Offsets.Editor.team_NEXT + _rule * GTA.Offsets.Editor.txt_NEXT).GetString();
            });
            body.Children.Add(textPanel);
            body.Children.Add(BitToggle(13, "pa_noblock", "Objective stays active outside"));
            body.Children.Add(BitToggle(11, "pa_oobtext", "Hint text on screen"));
            body.Children.Add(BitToggle(12, "pa_shard", "Big message (shard)"));
            body.Children.Add(IntField("pa_ignoreveh", "Leaving is fine in this vehicle (model hash)", PlayAreas.IgnoreLeaveVeh, "pa_hash_hint", "0 = none. Model hashes are shown in the converter."));
            body.Children.Add(IntField("pa_onlyveh", "Only players in this vehicle count (model hash)", PlayAreas.OnlyAffectVeh));
            return Card("pa_card_leave", "Leaving", body);
        }

        private Border LookCard()
        {
            var body = new StackPanel();
            body.Children.Add(BitToggle(6, "pzvisible", "Visible in game"));
            // "Hide after entering" is a rule bit (iRuleBitsetEight), not a bounds bit.
            body.Children.Add(Toggle("pzvisibleinside", "Hide after entering",
                () => GTA.Offsets.Editor.irbs8 != 0 && Functions.Read.checkbinary(15, GTA.Offsets.Editor.irbs8 + _team * GTA.Offsets.Editor.team_NEXT + _rule),
                on => ForTargets((t, r) => Functions.Write.writebinary(15, GTA.Offsets.Editor.irbs8 + t * GTA.Offsets.Editor.team_NEXT + r, on))));
            body.Children.Add(BitToggle(10, "pa_gps", "GPS route to the blip"));
            body.Children.Add(IntField("pa_colour", "Colour (HUD colour number)", PlayAreas.Colour));

            body.Children.Add(Label("outmm", "Minimap alpha"));
            var alpha = new Slider { Minimum = 0, Maximum = 255, Margin = new Thickness(0, 0, 0, 10) };
            alpha.ValueChanged += (_, __) => WriteInt(PlayAreas.MinimapAlpha, (int)alpha.Value);
            _loaders.Add(() => alpha.Value = PlayAreas.GetInt(_area, _team, _rule, PlayAreas.MinimapAlpha));
            body.Children.Add(alpha);

            // Idle blip timer: one per team and rule, outside the struct (pribt).
            body.Children.Add(Label("pribt", "Idle blip timer"));
            var idle = Box();
            idle.Margin = new Thickness(0, 0, 0, 10);
            idle.TextChanged += (_, __) =>
            {
                if (_loading || !Live || GTA.Offsets.Editor.PlayArea.pribt == 0 || !int.TryParse(idle.Text, out int v)) return;
                ForTargets((t, r) => new Global(GTA.Offsets.Editor.PlayArea.pribt + t * GTA.Offsets.Editor.team_NEXT + r).SetInt(v));
            };
            _loaders.Add(() => { if (!idle.IsKeyboardFocused && GTA.Offsets.Editor.PlayArea.pribt != 0) idle.Text = new Global(GTA.Offsets.Editor.PlayArea.pribt + _team * GTA.Offsets.Editor.team_NEXT + _rule).Get<int>().ToString(CultureInfo.InvariantCulture); });
            body.Children.Add(idle);
            return Card("pa_card_look", "Look", body);
        }

        private Border MoreCard()
        {
            var body = new StackPanel();
            body.Children.Add(Label("pa_onemember", "One player of these teams is enough"));
            var teams = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, -6, 6) };
            for (int t = 0; t < PlayAreas.Teams; t++)
            {
                int bit = t;
                var b = Tile(T("actorteam", "Team") + " " + (t + 1));
                b.MinHeight = 30; b.Height = 30;
                b.Click += (_, __) => WriteBit(bit, b.IsChecked == true);
                _loaders.Add(() => b.IsChecked = (PlayAreas.GetInt(_area, _team, _rule, PlayAreas.BoundsBS) & (1 << bit)) != 0);
                teams.Children.Add(b);
            }
            body.Children.Add(teams);
            body.Children.Add(BitToggle(8, "pa_ignorez", "Ignore height for spheres (cylinder)"));
            body.Children.Add(BitToggle(9, "pa_keepspawn", "Spawn entities inside the area"));
            body.Children.Add(BitToggle(4, "pa_delayed", "Delayed spawn area position"));
            body.Children.Add(BitToggle(7, "pa_overtime", "Only the active overtime player"));
            body.Children.Add(IntField("pa_spawngroup", "Spawn group", PlayAreas.SpawnGroup));
            body.Children.Add(IntField("pa_blipheight", "Blip height threshold", PlayAreas.BlipHeight));
            body.Children.Add(IntField("pa_maxplayers", "Max players inside (0 = no limit)", PlayAreas.MaxPlayers));
            body.Children.Add(IntField("bits", "Flags (raw)", PlayAreas.BoundsBS, "pa_bits_hint", "iBoundsBS: the switches on this page are bits of it."));
            return Card("pa_card_more", "More", body);
        }

        // ----- loading -----

        public void Load()
        {
            if (!Live || !IsVisible)
                return;
            _loading = true;
            try
            {
                foreach (var load in _loaders)
                    load();
                _sphereEditor.Refresh();
                _boxEditor.Refresh();
            }
            finally
            {
                _loading = false;
            }
        }
    }
}
