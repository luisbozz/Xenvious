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
    /// A team's rules as a list (mockup variant A: https://claude.ai/artifact/KQX7zDCkNu1m71utNs6dMF):
    /// every rule with its text, what points at it and its jumps and limits; the selected rule's
    /// settings on the right. Moving, adding and deleting rules is not here yet (every pointer would
    /// have to be renumbered). Memory: Rules.
    /// </summary>
    public class RulesView : DockPanel
    {
        private int _team, _selected;
        private bool _built, _loading;
        private string _shownKey;
        private List<Rules.Rule> _rules = new List<Rules.Rule>();

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        private readonly List<ToggleButton> _teamTabs = new List<ToggleButton>();
        private readonly WrapPanel _teamBar = new WrapPanel();
        private readonly TextBlock _summary = new TextBlock { FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
        private readonly StackPanel _list = new StackPanel();
        private readonly StackPanel _detail = new StackPanel();

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public RulesView()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Reload(true); _timer.Start(); }
                else _timer.Stop();
            };
            // Picks up changes made in the creator; skipped while something here has the keyboard.
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Reload(false); };
        }

        private bool Live => _built && Rules.Ready;

        // ----- building -----

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            var top = new DockPanel { Margin = new Thickness(0, 12, 0, 10) };
            _summary.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
            DockPanel.SetDock(_summary, Dock.Right);
            top.Children.Add(_summary);
            top.Children.Add(_teamBar);
            DockPanel.SetDock(top, Dock.Top);
            Children.Add(top);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(11, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(9, GridUnitType.Star), MinWidth = 320 });
            var left = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = _list };
            var right = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = _detail };
            Grid.SetColumn(right, 2);
            grid.Children.Add(left);
            grid.Children.Add(right);
            Children.Add(grid);
            Reload(true);
        }

        private void BuildTeamTabs(int teams)
        {
            if (_teamTabs.Count == teams)
                return;
            _teamTabs.Clear();
            _teamBar.Children.Clear();
            for (int i = 0; i < teams; i++)
            {
                int n = i;
                var b = new ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = T("actorteam", "Team") + " " + (i + 1), MinHeight = 32, Height = 32,
                    Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 4, 0), FontSize = 13.5 };
                b.Click += (_, __) => { _team = n; _selected = 0; Reload(true); };
                _teamTabs.Add(b);
                _teamBar.Children.Add(b);
            }
        }

        // ----- loading -----

        private void Reload(bool force)
        {
            if (!_built)
                return;
            if (!Live)
            {
                _list.Children.Clear();
                _detail.Children.Clear();
                _list.Children.Add(Muted(T("rl_nocreator", "Open a mission (LTS, Capture or Mission Creator) to see its rules.")));
                _shownKey = null;
                return;
            }
            int teams = Rules.Teams();
            BuildTeamTabs(teams);
            if (_team >= teams) _team = 0;
            for (int i = 0; i < _teamTabs.Count; i++) _teamTabs[i].IsChecked = i == _team;

            _rules = Rules.Read(_team);
            if (_selected >= _rules.Count) _selected = Math.Max(0, _rules.Count - 1);
            string key = Signature();
            if (!force && key == _shownKey)
                return;
            _shownKey = key;

            _summary.Text = string.Format(CultureInfo.CurrentCulture, T("rl_summary", "{0} rules · {1} links"), _rules.Count, _rules.Sum(r => r.Links.Count));
            _list.Children.Clear();
            if (_rules.Count == 0)
                _list.Children.Add(Muted(T("rl_none", "This team has no rules yet. Place entities with an objective in the creator.")));
            foreach (var rule in _rules)
                _list.Children.Add(RuleCard(rule));
            ShowDetail();
        }

        private string Signature()
            => _team + "|" + _selected + "|" + Rules.PublicCreator + "|" + string.Join(";", _rules.Select(r =>
                $"{r.Text}/{r.NextRules}/{r.TargetScore}/{r.ObjectiveScore}/{r.TakeoverMs}/{r.TimeLimit}/{r.FailsMission}/" +
                string.Join(",", r.Links.Select(l => $"{l.Kind}{l.Index}:{l.Type}:{l.Extra}:{l.PassJump}:{l.FailJump}"))));

        // ----- the list -----

        private Border RuleCard(Rules.Rule rule)
        {
            bool selected = rule.Index == _selected;
            var card = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 9, 12, 9), Margin = new Thickness(0, 0, 0, 6),
                BorderThickness = new Thickness(selected ? 2 : 1), Cursor = System.Windows.Input.Cursors.Hand };
            card.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            card.SetResourceReference(Border.BorderBrushProperty, selected ? "AccentBrush" : "LineBrush");
            card.MouseLeftButtonUp += (_, __) => { _selected = rule.Index; _shownKey = null; Reload(true); };

            var body = new StackPanel();
            var head = new DockPanel();
            var number = new TextBlock { Text = (rule.Index + 1).ToString(CultureInfo.CurrentCulture), FontWeight = FontWeights.Bold, FontSize = 15, MinWidth = 26, VerticalAlignment = VerticalAlignment.Top };
            number.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            head.Children.Add(number);
            var text = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 14, VerticalAlignment = VerticalAlignment.Center,
                Text = string.IsNullOrWhiteSpace(rule.Text) ? T("rl_notext", "(no objective text)") : CleanText(rule.Text) };
            if (string.IsNullOrWhiteSpace(rule.Text))
                text.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            head.Children.Add(text);
            body.Children.Add(head);

            var chips = new WrapPanel { Margin = new Thickness(26, 6, 0, 0) };
            if (rule.Links.Count == 0)
                chips.Children.Add(Chip(T("rl_empty", "nothing points here"), "WarnBrush"));
            foreach (var group in rule.Links.GroupBy(l => (l.Kind, l.Type, l.Extra)))
            {
                var ids = group.Select(l => l.Index + 1).ToList();
                string who = KindName(group.Key.Kind) + " " + (ids.Count > 4 ? ids.Count.ToString(CultureInfo.CurrentCulture) + "×" : string.Join(", ", ids));
                chips.Children.Add(Chip(who + " · " + TypeName(group.Key.Kind, group.Key.Type, group.Key.Extra) + (group.Key.Extra ? " +" : ""), null));
            }
            foreach (var jump in rule.Links.Where(l => l.PassJump >= 0).Select(l => l.PassJump).Distinct())
                chips.Children.Add(Chip("✓ → " + (jump + 1), "OkBrush"));
            foreach (var jump in rule.Links.Where(l => l.FailJump >= 0).Select(l => l.FailJump).Distinct())
                chips.Children.Add(Chip("✗ → " + (jump + 1), "BadBrush"));
            if (rule.NextRules != 0)
                chips.Children.Add(Chip("⇢ " + string.Join(" / ", Bits(rule.NextRules).Select(b => (b + 1).ToString(CultureInfo.CurrentCulture))), "AccentBrush"));
            if (rule.TargetScore > 0)
                chips.Children.Add(Chip(string.Format(CultureInfo.CurrentCulture, T("rl_chip_target", "{0} needed"), rule.TargetScore), null));
            if (rule.TimeLimit != 0)
                chips.Children.Add(Chip("⏱ " + TimeText(rule.TimeLimit), null));
            if (rule.FailsMission)
                chips.Children.Add(Chip(T("rl_chip_fail", "fail = mission over"), "BadBrush"));
            body.Children.Add(chips);
            card.Child = body;
            return card;
        }

        private Border Chip(string text, string brush)
        {
            var chip = new Border { CornerRadius = new CornerRadius(10), Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(0, 0, 6, 4), BorderThickness = new Thickness(1) };
            chip.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            chip.SetResourceReference(Border.BorderBrushProperty, brush ?? "LineBrush");
            var t = new TextBlock { Text = text, FontSize = 12.5 };
            t.SetResourceReference(TextBlock.ForegroundProperty, brush ?? "MutedTextBrush");
            chip.Child = t;
            return chip;
        }

        private TextBlock Muted(string text)
        {
            var t = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = 13.5, Margin = new Thickness(0, 6, 0, 6) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
            return t;
        }

        // ----- the selected rule -----

        private void ShowDetail()
        {
            _detail.Children.Clear();
            if (_selected < 0 || _selected >= _rules.Count)
                return;
            var rule = _rules[_selected];
            _loading = true;
            try
            {
                _detail.Children.Add(Card(string.Format(CultureInfo.CurrentCulture, T("rl_rule_n", "Rule {0}"), rule.Index + 1), TextBody(rule)));
                _detail.Children.Add(Card(T("rl_links", "What points here"), LinksBody(rule)));
                _detail.Children.Add(Card(T("rl_next", "Next objective"), NextBody(rule)));
                _detail.Children.Add(Card(T("rl_limits", "Objective limits"), LimitsBody(rule)));
            }
            finally { _loading = false; }
        }

        private Border Card(string title, FrameworkElement body)
        {
            var dock = new DockPanel();
            var head = new Border { Style = (Style)FindResource("DashCardHeader"), Child = new TextBlock { Style = (Style)FindResource("DashCardTitle"), Text = title } };
            DockPanel.SetDock(head, Dock.Top);
            dock.Children.Add(head);
            body.Margin = new Thickness(14, 10, 14, 10);
            dock.Children.Add(body);
            return new Border { Style = (Style)FindResource("DashCard"), Margin = new Thickness(0, 0, 0, 12), Child = dock };
        }

        private TextBlock Label(string key, string fallback) => new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = T(key, fallback) };

        private TextBlock Hint(string text)
        {
            var hint = new TextBlock { Text = text, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return hint;
        }

        private static TextBox Box(string text)
        {
            var box = new TextBox { Height = 30, Text = text, Margin = new Thickness(0, 0, 0, 10) };
            box.SetResourceReference(StyleProperty, "Watermark");
            return box;
        }

        private FrameworkElement TextBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            panel.Children.Add(Label("rl_text", "Objective text"));
            var box = Box(rule.Text ?? "");
            box.MaxLength = 63;
            box.LostKeyboardFocus += (_, __) => { if (!_loading && Live && box.Text != rule.Text) { Rules.SetText(_team, rule.Index, box.Text); rule.Text = box.Text; _shownKey = null; } };
            panel.Children.Add(box);
            panel.Children.Add(Hint(T("rl_text_hint", "Colour codes like ~y~ ... ~s~ work as in the creator. Saved when you leave the field.")));
            return panel;
        }

        private FrameworkElement LinksBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            if (rule.Links.Count == 0)
            {
                panel.Children.Add(Hint(T("rl_links_none", "Nothing points at this rule. The game reaches it only through a jump or the next-objective override, otherwise the team gets stuck here.")));
                return panel;
            }
            foreach (var link in rule.Links)
            {
                var row = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
                var name = new TextBlock { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, FontSize = 13.5,
                    Text = EntityName(link) + "  ·  " + TypeName(link.Kind, link.Type, link.Extra) + (link.Extra ? "  (" + T("rl_extra", "extra objective") + ")" : "") };
                var jumps = new StackPanel { Orientation = Orientation.Horizontal };
                if (link.HasPass) jumps.Children.Add(JumpBox(link, true));
                if (link.HasFail) jumps.Children.Add(JumpBox(link, false));
                DockPanel.SetDock(jumps, Dock.Right);
                row.Children.Add(jumps);
                row.Children.Add(name);
                panel.Children.Add(row);
            }
            panel.Children.Add(Hint(Rules.PublicCreator
                ? T("rl_jump_hint_pmc", "✓ / ✗: where the team goes when this objective is passed or failed. The Mission Creator only jumps forward.")
                : T("rl_jump_hint", "✓ / ✗: where the team goes when this objective is passed or failed.")));
            return panel;
        }

        private FrameworkElement JumpBox(Rules.Link link, bool pass)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(8, 0, 0, 0) };
            var mark = new TextBlock { Text = pass ? "✓" : "✗", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) };
            mark.SetResourceReference(TextBlock.ForegroundProperty, pass ? "OkBrush" : "BadBrush");
            panel.Children.Add(mark);
            var combo = new ComboBox { Width = 92, Height = 30 };
            combo.Items.Add(new ComboBoxItem { Content = "–", Tag = -1 });
            for (int r = 0; r < _rules.Count; r++)
            {
                if (Rules.PublicCreator && r <= _selected)
                    continue;
                combo.Items.Add(new ComboBoxItem { Content = (r + 1).ToString(CultureInfo.CurrentCulture), Tag = r });
            }
            int current = pass ? link.PassJump : link.FailJump;
            combo.SelectedItem = combo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == current) ?? combo.Items[0];
            combo.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(combo.SelectedItem is ComboBoxItem item))
                    return;
                Rules.SetJump(link, _team, pass, (int)item.Tag);
                if (pass) link.PassJump = (int)item.Tag; else link.FailJump = (int)item.Tag;
                _shownKey = null;
                Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
            };
            panel.Children.Add(combo);
            return panel;
        }

        private FrameworkElement NextBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            panel.Children.Add(Hint(T("rl_next_hint", "Normally the team goes on with the next rule. Pick one rule to go there instead, or several to pick one at random.")));
            var tiles = new WrapPanel();
            // The Mission Creator only accepts later rules, and none from rule 16 on.
            bool pmc = Rules.PublicCreator;
            for (int r = 0; r < _rules.Count; r++)
            {
                if (pmc && (r <= rule.Index || rule.Index >= 15))
                    continue;
                int bit = r;
                var tile = new ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = (r + 1).ToString(CultureInfo.CurrentCulture), MinWidth = 40, Height = 32,
                    Margin = new Thickness(0, 0, 4, 4), IsChecked = (rule.NextRules & (1 << r)) != 0 };
                tile.Click += (_, __) =>
                {
                    if (!Live) return;
                    int v = Rules.GetField(GTA.Offsets.Editor.nxtrulb, _team, rule.Index);
                    v = tile.IsChecked == true ? v | (1 << bit) : v & ~(1 << bit);
                    Rules.SetField(GTA.Offsets.Editor.nxtrulb, _team, rule.Index, v);
                    rule.NextRules = v;
                    _shownKey = null;
                    Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
                };
                tiles.Children.Add(tile);
            }
            if (tiles.Children.Count == 0)
                panel.Children.Add(Hint(T("rl_next_none", "No later rule to pick.")));
            panel.Children.Add(tiles);
            return panel;
        }

        private FrameworkElement LimitsBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            panel.Children.Add(IntRow("rl_target", "Needed to pass (0 = all)", GTA.Offsets.Editor.tsc, rule.Index, rule.TargetScore,
                T("rl_target_hint", "How many of the objective's entities must be done, e.g. destroy 6 of 10.")));
            panel.Children.Add(IntRow("rl_points", "Points per objective", GTA.Offsets.Editor.tms, rule.Index, rule.ObjectiveScore, null));
            panel.Children.Add(IntRow("rl_takeover", "Capture / hack time (ms)", GTA.Offsets.Editor.ttime, rule.Index, rule.TakeoverMs, null));

            panel.Children.Add(Label("rl_time", "Time limit"));
            var time = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
            foreach (var (sel, sec) in Rules.TimeLimits)
                time.Items.Add(new ComboBoxItem { Content = sel == 0 ? T("rl_time_off", "No limit") : TimeSpan.FromSeconds(sec).ToString(sec >= 60 ? @"m\:ss" : @"s\ \s", CultureInfo.InvariantCulture), Tag = sel });
            time.SelectedItem = time.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == rule.TimeLimit);
            time.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(time.SelectedItem is ComboBoxItem item)) return;
                Rules.SetField(GTA.Offsets.Editor.tmt, _team, rule.Index, (int)item.Tag);
                rule.TimeLimit = (int)item.Tag;
                _shownKey = null;
            };
            panel.Children.Add(time);

            var row = new Grid { Style = (Style)FindResource("FormRow") };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new TextBlock { Style = (Style)FindResource("FormLabel"), Text = T("rl_failmission", "Failing this rule ends the mission") });
            var fail = new CheckBox { Style = (Style)FindResource("FormToggle"), IsChecked = rule.FailsMission };
            fail.Click += (_, __) => { if (Live) { Rules.SetFailsMission(_team, rule.Index, fail.IsChecked == true); rule.FailsMission = fail.IsChecked == true; _shownKey = null; } };
            Grid.SetColumn(fail, 1);
            row.Children.Add(fail);
            panel.Children.Add(row);
            panel.Children.Add(Hint(T("rl_limits_hint", "A rule nothing completes on its own (no entities, no needed count) needs a time limit, or the team stays on it.")));
            return panel;
        }

        private FrameworkElement IntRow(string key, string fallback, long offset, int rule, int value, string hint)
        {
            var panel = new StackPanel();
            panel.Children.Add(Label(key, fallback));
            var box = Box(value.ToString(CultureInfo.InvariantCulture));
            box.IsEnabled = offset != 0;
            box.TextChanged += (_, __) =>
            {
                if (!_loading && Live && int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    Rules.SetField(offset, _team, rule, v);
                    _shownKey = null;
                }
            };
            panel.Children.Add(box);
            if (hint != null)
                panel.Children.Add(Hint(hint));
            return panel;
        }

        // ----- names -----

        private static string KindName(Rules.Kind kind)
        {
            switch (kind)
            {
                case Rules.Kind.Ped: return T("eo_k_ped", "Actor");
                case Rules.Kind.Vehicle: return T("eo_k_veh", "Vehicle");
                case Rules.Kind.Object: return T("eo_k_obj", "Object");
                case Rules.Kind.GoTo: return T("eo_k_goto", "Go-to");
                default: return T("rl_k_player", "Player rule");
            }
        }

        private static string EntityName(Rules.Link link)
        {
            int type = link.Kind == Rules.Kind.Ped ? EntityPicker.Actor : link.Kind == Rules.Kind.Vehicle ? EntityPicker.Vehicle
                : link.Kind == Rules.Kind.Object ? EntityPicker.Object : link.Kind == Rules.Kind.GoTo ? EntityPicker.GoTo : 0;
            string label = type == 0 ? "#" + (link.Index + 1).ToString(CultureInfo.CurrentCulture) : EntityPicker.Label(type, link.Index);
            return KindName(link.Kind) + " " + label;
        }

        private static string TypeName(Rules.Kind kind, int type, bool extra)
        {
            if (extra)
            {
                int eoType = kind == Rules.Kind.Ped ? ExtraObjectives.TypePed : kind == Rules.Kind.Vehicle ? ExtraObjectives.TypeVehicle
                    : kind == Rules.Kind.Object ? ExtraObjectives.TypeObject : ExtraObjectives.TypeGoTo;
                string name = ExtraObjectives.RuleTypesFor(eoType).FirstOrDefault(r => r.Value == type).Name ?? type.ToString(CultureInfo.InvariantCulture);
                return T("eo_r_" + type, name);
            }
            return type >= 0 && type < Rules.LogicNames.Length ? T("rl_logic_" + type, Rules.LogicNames[type]) : type.ToString(CultureInfo.InvariantCulture);
        }

        private static string TimeText(int selection)
        {
            int sec = Rules.TimeLimits.FirstOrDefault(t => t.Selection == selection).Seconds;
            return sec >= 60 ? TimeSpan.FromSeconds(sec).ToString(@"m\:ss", CultureInfo.InvariantCulture) : sec.ToString(CultureInfo.CurrentCulture) + " s";
        }

        private static IEnumerable<int> Bits(int value)
        {
            for (int b = 0; b < 32; b++)
                if ((value & (1 << b)) != 0)
                    yield return b;
        }

        // ~y~, ~s~, ~r~ ... are the game's colour codes; the list shows the plain text.
        private static string CleanText(string text) => System.Text.RegularExpressions.Regex.Replace(text, "~[a-zA-Z0-9]~", "");
    }
}
