using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// "Rules" card of the entity pages (mockup: https://claude.ai/artifact/9VmM5o1Y3BupjHnRSMqyMa):
    /// the team's rules in order and where the selected entity takes part, as its own rule or as an
    /// extra objective. In the Mission Creator joining a rule goes through its rule list
    /// (EntityRules), elsewhere extra objectives are written directly. Replaces ExtraRulesCard.
    /// </summary>
    public class EntityRulesCard : SectionCard
    {
        private ComboBox _entries;
        private int _type;
        private int _team;
        private int _adding = -1;
        private string _shownKey;
        private string _error;
        private bool _built;

        private readonly StackPanel _body = new StackPanel();
        private readonly StackPanel _teamButtons = new StackPanel { Orientation = Orientation.Horizontal };
        private readonly TextBlock _slots = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
        private readonly StackPanel _messages = new StackPanel();
        private readonly WrapPanel _legend = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
        private readonly StackPanel _track = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        private readonly StackPanel _footer = new StackPanel();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

        /// <summary>Opens the explanation (on the rules page).</summary>
        public event EventHandler HelpRequested;
        /// <summary>The team shown changed (the page follows with its per-team fields).</summary>
        public event EventHandler TeamChanged;
        /// <summary>After every redraw: the page updates its summary chips and the lifecycle strip.</summary>
        public event EventHandler Refreshed;

        public int Team => _team;
        public int RuleCount { get; private set; }
        public int MainRule { get; private set; } = -1;
        public string MainName { get; private set; }
        public List<(int Rule, string Name)> Extras { get; } = new List<(int, string)>();

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public EntityRulesCard()
        {
            Title = T("er_card", "Rules");
            Icon = Geometry.Parse("M5,21 L5,4 M5,4 L16,4 L14,8 L16,12 L5,12");
            Content = _body;
            // An implicit style only matches its exact type, so this subclass asks for SectionCard's look.
            SetResourceReference(StyleProperty, typeof(SectionCard));
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Refresh(); _timer.Start(); }
                else _timer.Stop();
            };
            // Picks up changes made in the creator; skipped while something here has the keyboard.
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Redraw(false); };
        }

        public void Attach(ComboBox entries, int entityType)
        {
            _entries = entries;
            _type = entityType;
            entries.SelectionChanged += (_, __) => { _adding = -1; _error = null; Refresh(); };
        }

        /// <summary>Shows a team (the page's team selection calls this).</summary>
        public void SetTeam(int team)
        {
            if (team == _team || team < 0 || team > 3)
                return;
            _team = team;
            _adding = -1;
            Refresh();
            TeamChanged?.Invoke(this, EventArgs.Empty);
        }

        private int Index => _entries?.SelectedIndex ?? -1;

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            var help = new Button { Style = (Style)FindResource("NavButton"), Width = 26, Height = 26, Padding = new Thickness(0), Content = "?",
                ToolTip = T("eo_help_tip", "How extra rules work"), VerticalAlignment = VerticalAlignment.Center };
            help.Click += (_, __) => HelpRequested?.Invoke(this, EventArgs.Empty);
            HeaderRight = help;

            var top = new DockPanel();
            _slots.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            DockPanel.SetDock(_slots, Dock.Right);
            top.Children.Add(_slots);
            // Segmented control as in the mockup: the chosen team raised, with an accent line under it.
            var holder = new Border { CornerRadius = new CornerRadius(7), Padding = new Thickness(3), BorderThickness = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Left, Child = _teamButtons };
            holder.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            holder.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            top.Children.Add(holder);
            _body.Children.Add(top);

            _body.Children.Add(_legend);

            _body.Children.Add(_messages);
            _body.Children.Add(_track);
            _body.Children.Add(_footer);
            Refresh();
        }

        public void Refresh() => Redraw(true);

        private void Redraw(bool force)
        {
            if (!_built)
                return;
            bool ready = Rules.Ready && Index >= 0;
            int teams = ready ? Rules.Teams() : 1;
            if (_team >= teams) _team = 0;
            BuildTeamButtons(teams);

            var rules = ready ? Rules.Read(_team) : new List<Rules.Rule>();
            var entries = ready ? EntityRules.Read(_type, Index, _team) : new List<EntityRules.Entry>();
            bool list = ready && EntityRules.UsesRuleList;
            string key = string.Join("|", Index, _team, _adding, _error, list, string.Join(";", rules.Select(r => r.Text + "/" + string.Join(",", r.Links.Select(l => $"{l.Kind}{l.Index}:{l.Type}:{l.Extra}")))),
                string.Join(";", entries.Select(e => $"{e.Rule}:{e.Selection}:{e.Main}")), list ? string.Join(",", Enumerable.Range(0, rules.Count).Select(r => EntityRules.RuleSelection(_team, r) + ":" + (EntityRules.InRule(_team, r, Index) ? 1 : 0))) : "",
                EntityRules.UsesRuleList);
            if (!force && key == _shownKey)
                return;
            _shownKey = key;

            _messages.Children.Clear();
            _track.Children.Clear();
            _legend.Children.Clear();
            _legend.Children.Add(LegendItem(Marker(true, false, 12), T("er_main", "Own rule")));
            _legend.Children.Add(LegendItem(Marker(false, true, 12), T("er_extra", "Extra objective")));
            _legend.Children.Add(LegendItem(Marker(false, false, 12).Also(m => m.Opacity = 0.45), T("er_before", "before the own rule")));
            if (list)
                _legend.Children.Add(LegendItem(Marker(false, false, 12).Also(m => m.Opacity = 0.45), T("er_other_class", "rule for other entities")));
            _footer.Children.Clear();
            _slots.Text = ready ? string.Format(CultureInfo.CurrentCulture, T("er_slots", "Extra objectives: {0} of 30 entities"), ExtraObjectives.UsedSlots()) : "";

            RuleCount = rules.Count;
            var main = entries.FirstOrDefault(e => e.Main);
            MainRule = main?.Rule ?? -1;
            MainName = main == null ? null : Rules.SelectionName(main.Selection) ?? Rules.TypeName(new Rules.Link { Kind = EntityRules.KindOf(_type), Type = EntityRules.OwnLogic(_type, Index, _team) });
            Extras.Clear();
            foreach (var e in entries.Where(e => !e.Main))
                Extras.Add((e.Rule, Rules.SelectionName(e.Selection) ?? e.Selection.ToString(CultureInfo.InvariantCulture)));

            if (!ready)
            {
                _track.Children.Add(Faint(Index < 0 ? T("er_noentity", "Select an entry above.") : T("rl_nocreator", "Open a mission (LTS, Capture or Mission Creator) to see its rules."), 13));
                Summary = "";
                Refreshed?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (_error != null)
                _messages.Children.Add(Message(T(_error, _error), true));

            if (list)
                DrawRuleList(rules, entries);
            else
                DrawDirect(rules, entries);

            Summary = MainRule < 0 ? T("er_sum_none", "no own rule")
                : string.Format(CultureInfo.CurrentCulture, T("er_sum", "Own rule R{0} {1}"), MainRule + 1, MainName)
                  + (Extras.Count > 0 ? " · " + string.Format(CultureInfo.CurrentCulture, T("er_sum_extra", "{0} extra"), Extras.Count) : "");

            var add = new Button { Style = (Style)FindResource("FormButton"), Height = 28, Padding = new Thickness(10, 0, 10, 0), IsEnabled = false,
                Content = "+ " + string.Format(CultureInfo.CurrentCulture, T("er_newrule", "New rule at the end (rule {0})"), rules.Count + 1),
                ToolTip = T("er_newrule_later", "Comes next: a new rule needs the creator's presets, their offsets are not in Xenvious yet.") };
            ToolTipService.SetShowOnDisabled(add, true);
            var foot = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            foot.Children.Add(add);
            foot.Children.Add(Faint(T("er_newrule_hint", "Mission Creator only"), 12).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(10, 0, 0, 0); }));
            _footer.Children.Add(foot);
            _footer.Children.Add(Faint(list
                ? T("er_hint_list", "Mission Creator: the rule decides the type. The first rule an entity is in is its own rule, later ones become extra objectives, like in the creator's rules menu.")
                : T("er_hint_direct", "Extra objectives run after the own rule, one after the other. On the own rule itself the game skips them."), 12).Also(t => t.Margin = new Thickness(0, 8, 0, 0)));
            Refreshed?.Invoke(this, EventArgs.Empty);
        }

        private void BuildTeamButtons(int teams)
        {
            if (_teamButtons.Children.Count != teams)
            {
                _teamButtons.Children.Clear();
                for (int i = 0; i < teams; i++)
                {
                    int n = i;
                    var b = new ToggleButton { Content = T("dash_team", "Team") + " " + (i + 1).ToString(CultureInfo.InvariantCulture), Style = (Style)FindResource("TeamSegButton") };
                    b.Click += (_, __) => { if (n == _team) Redraw(true); else SetTeam(n); };
                    _teamButtons.Children.Add(b);
                }
            }
            for (int i = 0; i < _teamButtons.Children.Count; i++)
                ((ToggleButton)_teamButtons.Children[i]).IsChecked = i == _team;
        }

        // ----- Mission Creator: membership in the rule list -----

        private void DrawRuleList(List<Rules.Rule> rules, List<EntityRules.Entry> entries)
        {
            int index = Index;
            // Extra objectives the creator would not make: set by hand, dropped the next time the entity joins or leaves a rule.
            var stray = entries.Where(e => !e.Main && (e.Rule >= rules.Count || EntityRules.ClassOf(EntityRules.RuleSelection(_team, e.Rule)) != _type
                || !EntityRules.InRule(_team, e.Rule, index))).ToList();
            if (stray.Count > 0)
            {
                var fix = SmallButton(T("er_fix", "Tidy up"), () => Apply(EntityRules.Sync(_type, index, _team)));
                _messages.Children.Add(Message(string.Format(CultureInfo.CurrentCulture, T("er_warn_stray", "Extra objective on rule {0} is not in the creator's rule list. The game skips or loses it."),
                    string.Join(", ", stray.Select(s => s.Rule + 1))), false, fix));
            }
            for (int r = 0; r < rules.Count; r++)
            {
                int rule = r;
                int selection = EntityRules.RuleSelection(_team, r);
                int cls = EntityRules.ClassOf(selection);
                // The bitset holds entity numbers of the rule's own kind: vehicle 2 and actor 2 share a bit.
                bool member = cls == _type && EntityRules.InRule(_team, r, index);
                string type = Rules.SelectionName(selection) ?? T("er_type_player", "Player rule");
                var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                RowKind kind;
                if (member && r == MainRule)
                {
                    kind = RowKind.Main;
                    right.Children.Add(Chip(T("er_main", "Own rule"), ChipKind.Main));
                    right.Children.Add(TypeCombo(selection, value => Apply(EntityRules.SetRuleType(_team, rule, value)), T("er_type_rule_tip", "Changes the type of the whole rule, for every entity in it.")));
                    right.Children.Add(Remove(() => Apply(EntityRules.SetMember(_type, index, _team, rule, false))));
                }
                else if (member)
                {
                    kind = RowKind.Extra;
                    right.Children.Add(Chip(T("er_extra_short", "Extra"), ChipKind.Extra));
                    right.Children.Add(TypeCombo(selection, value => Apply(EntityRules.SetRuleType(_team, rule, value)), T("er_type_rule_tip", "Changes the type of the whole rule, for every entity in it.")));
                    right.Children.Add(Remove(() => Apply(EntityRules.SetMember(_type, index, _team, rule, false))));
                }
                else if (cls == _type && MainRule >= 0 && r < MainRule)
                {
                    kind = RowKind.Faded;
                    right.Children.Add(Faint(T("er_before", "before the own rule"), 12).Also(t => t.ToolTip = T("er_before_tip", "Extra objectives only run after the own rule.")));
                }
                else if (cls == _type)
                {
                    kind = RowKind.Free;
                    bool becomesMain = MainRule < 0;
                    right.Children.Add(AddButton(becomesMain ? T("er_add_main", "Own rule") : T("er_extra_short", "Extra"), null,
                        () => Apply(EntityRules.SetMember(_type, index, _team, rule, true))));
                }
                else
                {
                    kind = RowKind.Faded;
                    right.Children.Add(Faint(string.Format(CultureInfo.CurrentCulture, T("er_for_class", "{0} rule"), ClassName(cls)), 12)
                        .Also(t => t.ToolTip = T("er_for_class_tip", "In the Mission Creator a rule takes one kind of entity only.")));
                }
                _track.Children.Add(Row(rules[r], r, rules.Count, kind, type, right, entries.Count(e => e.Rule == r)));
            }
        }

        // ----- other creators: priority and ExtraObjectives written directly -----

        private void DrawDirect(List<Rules.Rule> rules, List<EntityRules.Entry> entries)
        {
            int index = Index;
            int last = Math.Max(rules.Count - 1, entries.Count == 0 ? -1 : entries.Max(e => e.Rule));
            if (MainRule < 0)
                _messages.Children.Add(Message(string.Format(CultureInfo.CurrentCulture, T("er_warn_nomain", "No own rule for team {0}: extra objectives of this entity do not run."), _team + 1), false));
            if (entries.Any(e => !e.Main && e.Rule == MainRule))
                _messages.Children.Add(Message(string.Format(CultureInfo.CurrentCulture, T("er_warn_own", "Extra objective on rule {0} sits on the own rule. The game skips it; put it on a later rule."), MainRule + 1), false));
            var missing = entries.Where(e => e.Rule >= rules.Count).Select(e => e.Rule + 1).ToList();
            if (missing.Count > 0)
                _messages.Children.Add(Message(string.Format(CultureInfo.CurrentCulture, T("er_warn_missing", "Rule {0} does not exist. The extra objective never runs; remove it or add the rule."), string.Join(", ", missing)), true));

            for (int r = 0; r <= last && r < Rules.MaxRules; r++)
            {
                int rule = r;
                var extra = entries.FirstOrDefault(e => !e.Main && e.Rule == r);
                var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                RowKind kind;
                string type = r < rules.Count ? string.Join(", ", rules[r].Links.Select(Rules.TypeName).Distinct()) : "";
                if (r == MainRule)
                {
                    kind = extra != null ? RowKind.Warn : RowKind.Main;
                    right.Children.Add(Chip(T("er_main", "Own rule"), ChipKind.Main));
                    right.Children.Add(TypeCombo(Rules.Selection(EntityRules.KindOf(_type), EntityRules.OwnLogic(_type, index, _team)),
                        value => { EntityRules.SetOwnType(_type, index, _team, value); Redraw(true); }));
                    if (extra != null)
                    {
                        right.Children.Add(Chip(string.Format(CultureInfo.CurrentCulture, T("er_twice", "+ extra {0} twice"), Rules.SelectionName(extra.Selection)), ChipKind.Warn,
                            T("er_twice_tip", "The game skips an extra objective on the own rule.")));
                        right.Children.Add(Remove(() => { ExtraObjectives.Remove(extra.Slot, extra.Place, _team); Redraw(true); }));
                    }
                }
                else if (extra != null)
                {
                    bool gone = r >= rules.Count;
                    kind = gone ? RowKind.Missing : r < MainRule ? RowKind.Warn : RowKind.Extra;
                    right.Children.Add(Chip(gone ? T("er_missing", "rule missing") : r < MainRule ? T("er_never", "never runs") : T("er_extra_short", "Extra"),
                        gone ? ChipKind.Bad : r < MainRule ? ChipKind.Warn : ChipKind.Extra));
                    right.Children.Add(TypeCombo(extra.Selection, value => { ExtraObjectives.SetRule(extra.Slot, extra.Place, _team, value, rule); Redraw(true); }));
                    right.Children.Add(Remove(() => { ExtraObjectives.Remove(extra.Slot, extra.Place, _team); Redraw(true); }));
                }
                else if (MainRule < 0 || r < MainRule)
                {
                    kind = RowKind.Faded;
                    right.Children.Add(Faint(MainRule < 0 ? "" : T("er_before", "before the own rule"), 12)
                        .Also(t => t.ToolTip = T("er_before_tip", "Extra objectives only run after the own rule.")));
                }
                else if (_adding == r)
                {
                    kind = RowKind.Free;
                    var types = ExtraObjectives.RuleTypesFor(_type);
                    int picked = types.Length > 0 ? types[0].Value : -1;
                    right.Children.Add(TypeCombo(picked, value => picked = value));
                    right.Children.Add(SmallButton(T("eo_add", "Add"), () =>
                    {
                        string error = ExtraObjectives.Add(_type, index, _team, rule, picked);
                        _adding = -1;
                        Apply(error);
                    }));
                    right.Children.Add(Remove(() => { _adding = -1; Redraw(true); }));
                }
                else
                {
                    kind = RowKind.Free;
                    right.Children.Add(AddButton(T("er_extra_short", "Extra"), null, () => { _adding = rule; Redraw(true); }));
                }
                _track.Children.Add(Row(r < rules.Count ? rules[r] : null, r, last + 1, kind, type, right, r < rules.Count ? rules[r].Links.Count : 0));
            }
        }

        private void Apply(string error)
        {
            _error = error;
            Redraw(true);
        }

        // ----- drawing -----

        private enum RowKind { Main, Extra, Free, Faded, Warn, Missing }
        private enum ChipKind { Main, Extra, Plain, Warn, Bad }

        private UIElement Row(Rules.Rule rule, int r, int rows, RowKind kind, string type, UIElement right, int links)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 2) };
            if (kind == RowKind.Main || kind == RowKind.Warn || kind == RowKind.Missing)
            {
                var tint = new Border { CornerRadius = new CornerRadius(7), Opacity = 0.14 };
                tint.SetResourceReference(Border.BackgroundProperty, kind == RowKind.Main ? "AccentBrush" : kind == RowKind.Warn ? "WarnBrush" : "BadBrush");
                grid.Children.Add(tint);
            }
            // The dashed rail through the number circles, cut at the first and last row.
            var rail = new Line { X1 = 0, X2 = 0, Y1 = 0, Y2 = 400, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 2, 1.5 } };
            rail.SetResourceReference(Shape.StrokeProperty, "LineBrush");
            var railHolder = new Canvas { ClipToBounds = true, HorizontalAlignment = HorizontalAlignment.Left, Width = 2, Margin = new Thickness(22, r == 0 ? 20 : 0, 0, r == rows - 1 ? 20 : 0) };
            railHolder.Children.Add(rail);
            grid.Children.Add(railHolder);

            var dock = new DockPanel { Margin = new Thickness(8, 6, 8, 6) };
            var num = NumberCircle(r, kind);
            DockPanel.SetDock(num, Dock.Left);
            dock.Children.Add(num);
            if (rule != null)
            {
                var open = ShowInRules(r);
                DockPanel.SetDock(open, Dock.Right);
                dock.Children.Add(open);
            }
            DockPanel.SetDock(right, Dock.Right);
            dock.Children.Add(right);
            var middle = new StackPanel { Margin = new Thickness(10, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center };
            if (rule == null)
                middle.Children.Add(Faint(T("er_norule", "This rule does not exist"), 13).Also(t => t.FontStyle = FontStyles.Italic));
            else
                middle.Children.Add(RulesView.ObjectiveText(rule.Text, 13, Rules.DefaultTextFor(_team, rule)));
            string sub = rule == null ? string.Format(CultureInfo.CurrentCulture, T("er_only_rules", "The team has {0} rules"), RuleCount)
                : (type.Length > 0 ? type : T("er_empty_rule", "nothing points at it"));
            middle.Children.Add(Faint(sub, 11.5));
            dock.Children.Add(middle);
            if (kind == RowKind.Faded)
                dock.Opacity = 0.5;
            grid.Children.Add(dock);
            return grid;
        }

        private static UIElement NumberCircle(int r, RowKind kind)
        {
            var grid = new Grid { Width = 28, Height = 28, VerticalAlignment = VerticalAlignment.Center };
            var circle = new Ellipse();
            var text = new TextBlock { Text = (r + 1).ToString(CultureInfo.CurrentCulture), FontSize = 12.5, FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (kind == RowKind.Main)
            {
                circle.SetResourceReference(Shape.FillProperty, "AccentBrush");
                text.Foreground = OnAccent();
            }
            else if (kind == RowKind.Extra || kind == RowKind.Missing)
            {
                circle.StrokeThickness = 2;
                circle.StrokeDashArray = new DoubleCollection { 2, 1.5 };
                string brush = kind == RowKind.Missing ? "BadBrush" : "AccentBrush";
                circle.SetResourceReference(Shape.StrokeProperty, brush);
                circle.SetResourceReference(Shape.FillProperty, "SectionBackgroundBrush");
                text.SetResourceReference(TextBlock.ForegroundProperty, brush);
            }
            else
            {
                circle.StrokeThickness = 1;
                circle.SetResourceReference(Shape.StrokeProperty, "LineBrush");
                circle.SetResourceReference(Shape.FillProperty, "DeepBrush");
                text.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            }
            grid.Children.Add(circle);
            grid.Children.Add(text);
            return grid;
        }

        /// <summary>White or black, whichever reads better on the theme's accent.</summary>
        public static Brush OnAccent()
        {
            var c = (MainWindow.ThemeBrush("AccentBrush") as SolidColorBrush)?.Color ?? Colors.SteelBlue;
            double l = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            return l > 0.62 ? new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x1B)) : Brushes.White;
        }

        private static FrameworkElement Chip(string text, ChipKind kind, string tip = null)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center, ToolTip = tip };
            var t = new TextBlock { Text = text, FontSize = 11.5, FontWeight = FontWeights.Bold, Margin = new Thickness(8, 2, 8, 3) };
            switch (kind)
            {
                case ChipKind.Main:
                    grid.Children.Add(new Border { CornerRadius = new CornerRadius(5) }.Also(b => b.SetResourceReference(Border.BackgroundProperty, "AccentBrush")));
                    t.Foreground = OnAccent();
                    break;
                case ChipKind.Extra:
                    grid.Children.Add(new Rectangle { RadiusX = 5, RadiusY = 5, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 2 } }
                        .Also(x => x.SetResourceReference(Shape.StrokeProperty, "AccentBrush")));
                    t.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
                    break;
                case ChipKind.Warn:
                case ChipKind.Bad:
                    string brush = kind == ChipKind.Warn ? "WarnBrush" : "BadBrush";
                    grid.Children.Add(new Border { CornerRadius = new CornerRadius(5), Opacity = 0.18 }.Also(b => b.SetResourceReference(Border.BackgroundProperty, brush)));
                    t.SetResourceReference(TextBlock.ForegroundProperty, brush);
                    break;
                default:
                    grid.Children.Add(new Border { CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(1) }.Also(b =>
                    {
                        b.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                        b.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                    }));
                    t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                    break;
            }
            grid.Children.Add(t);
            return grid;
        }

        /// <summary>A summary chip for the model card: own rule, extra objective, lifecycle.</summary>
        public static FrameworkElement SummaryChip(string text, string kind)
        {
            var chip = Chip(text, kind == "main" ? ChipKind.Main : kind == "extra" ? ChipKind.Extra : kind == "warn" ? ChipKind.Warn : ChipKind.Plain);
            chip.Margin = new Thickness(0, 0, 5, 5);
            if (kind == "ok")
            {
                var grid = (Grid)chip;
                grid.Children.Clear();
                grid.Children.Add(new Border { CornerRadius = new CornerRadius(5), Opacity = 0.18 }.Also(b => b.SetResourceReference(Border.BackgroundProperty, "OkBrush")));
                var t = new TextBlock { Text = text, FontSize = 11.5, FontWeight = FontWeights.Bold, Margin = new Thickness(8, 2, 8, 3) };
                t.SetResourceReference(TextBlock.ForegroundProperty, "OkBrush");
                grid.Children.Add(t);
            }
            return chip;
        }

        private static UIElement Marker(bool main, bool extra, double size)
        {
            var e = new Ellipse { Width = size, Height = size, VerticalAlignment = VerticalAlignment.Center };
            if (main)
                e.SetResourceReference(Shape.FillProperty, "AccentBrush");
            else if (extra)
            {
                e.StrokeThickness = 2;
                e.StrokeDashArray = new DoubleCollection { 1.5, 1 };
                e.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
            }
            else
            {
                e.StrokeThickness = 1;
                e.SetResourceReference(Shape.StrokeProperty, "LineBrush");
                e.SetResourceReference(Shape.FillProperty, "DeepBrush");
            }
            return e;
        }

        private static UIElement LegendItem(UIElement marker, string text)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 14, 0) };
            row.Children.Add(marker);
            var t = new TextBlock { Text = text, FontSize = 12, Margin = new Thickness(6, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            row.Children.Add(t);
            return row;
        }

        private UIElement Message(string text, bool bad, UIElement action = null)
        {
            var grid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
            var bg = new Border { CornerRadius = new CornerRadius(0, 6, 6, 0) };
            bg.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            grid.Children.Add(bg);
            var bar = new Border { Width = 3, HorizontalAlignment = HorizontalAlignment.Left };
            bar.SetResourceReference(Border.BackgroundProperty, bad ? "BadBrush" : "WarnBrush");
            grid.Children.Add(bar);
            var dock = new DockPanel { Margin = new Thickness(12, 7, 8, 7) };
            if (action != null)
            {
                DockPanel.SetDock(action, Dock.Right);
                dock.Children.Add(action);
            }
            var t = new TextBlock { Text = text, FontSize = 12.5, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            dock.Children.Add(t);
            grid.Children.Add(dock);
            return grid;
        }

        private Button SmallButton(string text, Action click)
        {
            var b = new Button { Style = (Style)FindResource("FormButton"), Height = 26, Padding = new Thickness(9, 0, 9, 0), Content = text, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center };
            b.Click += (_, __) => click();
            return b;
        }

        /// <summary>The dashed "+ …" button of a free row.</summary>
        private static Button AddButton(string text, string tip, Action click)
        {
            var label = new TextBlock { Text = "+ " + text, FontSize = 12.5, FontWeight = FontWeights.Bold, Margin = new Thickness(9, 3, 9, 4) };
            label.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            var frame = new Rectangle { RadiusX = 6, RadiusY = 6, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 2 }, Fill = Brushes.Transparent };
            frame.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
            var content = new Grid();
            content.Children.Add(frame);
            content.Children.Add(label);
            var b = new Button { Content = content, ToolTip = tip, Cursor = System.Windows.Input.Cursors.Hand, VerticalAlignment = VerticalAlignment.Center, FocusVisualStyle = null };
            b.Template = new ControlTemplate(typeof(Button)) { VisualTree = new FrameworkElementFactory(typeof(ContentPresenter)) };
            b.MouseEnter += (_, __) => frame.StrokeThickness = 1.6;
            b.MouseLeave += (_, __) => frame.StrokeThickness = 1;
            b.Click += (_, __) => click();
            return b;
        }

        // Opens the rules page at this rule, where its entities, jumps and texts are.
        private Button ShowInRules(int rule)
        {
            var icon = new Path { Data = Geometry.Parse("M5,3 H1 V11 H9 V7 M7,1 H11 V5 M11,1 L5,7"), StrokeThickness = 1.5, Width = 12, Height = 12, Stretch = Stretch.Uniform,
                StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            icon.SetResourceReference(Shape.StrokeProperty, "MutedTextBrush");
            var b = new Button { Style = (Style)FindResource("FieldIconButton"), Width = 26, Height = 26, Margin = new Thickness(4, 0, 0, 0), Content = icon,
                ToolTip = T("er_show_rule", "Show in the rules"), VerticalAlignment = VerticalAlignment.Center };
            int team = _team;
            b.Click += (_, __) => MainWindow.Instance?.OpenRules(team, rule);
            return b;
        }

        private Button Remove(Action click)
        {
            var b = new Button { Style = (Style)FindResource("FieldIconButton"), Width = 26, Height = 26, ToolTip = T("eo_remove", "Remove"),
                Content = new TextBlock { Text = "✕", FontSize = 11 }, VerticalAlignment = VerticalAlignment.Center };
            b.Click += (_, __) => click();
            return b;
        }

        private ComboBox TypeCombo(int selected, Action<int> changed, string tip = null)
        {
            var box = new ComboBox { Height = 26, MinWidth = 110, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center, ToolTip = tip };
            foreach (var r in ExtraObjectives.RuleTypesFor(_type))
            {
                var item = new ComboBoxItem { Content = T("eo_r_" + r.Value, r.Name), Tag = r.Value };
                box.Items.Add(item);
                if (r.Value == selected)
                    box.SelectedItem = item;
            }
            box.SelectionChanged += (_, __) => { if (box.SelectedItem is ComboBoxItem i) changed((int)i.Tag); };
            return box;
        }

        private static string ClassName(int cls)
        {
            switch (cls)
            {
                case ExtraObjectives.TypePed: return T("eo_k_ped", "Actor");
                case ExtraObjectives.TypeVehicle: return T("eo_k_veh", "Vehicle");
                case ExtraObjectives.TypeObject: return T("eo_k_obj", "Object");
                case ExtraObjectives.TypeGoTo: return T("rl_k_loc", "Location");
                default: return T("rl_k_player", "Player rule");
            }
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }
    }
}
