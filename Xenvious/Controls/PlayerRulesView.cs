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
    /// Kill page (mockup https://claude.ai/artifact/VifG8TGRs71syLT1EsWBmP, tab "LTS Kill"): the
    /// team's player rules (Global_4718592.f_114365, count f_115318[team]) as goals.
    ///
    /// The LTS creator forces player rule 0 to "kill players" on rule 1 every frame (func_4836) and
    /// sets nrl to 1 at test and save (func_904). The patches "dont force kill rule to 6 and number
    /// to 1" and "nrl fix" switch both off; this page shows whether they are active. A new player
    /// rule is filled like the Mission Creator does it (func_1371): type, rule, limit 5 for the
    /// kill types, 0 for the others (-1 for the cutscene).
    /// </summary>
    public class PlayerRulesView : StackPanel
    {
        private int _team;
        private bool _built, _sync;
        private readonly ComboBox _teamBox = new ComboBox { Height = 30, Width = 120 };
        private readonly StackPanel _status = new StackPanel();
        private readonly UniformGrid _goals = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, -6, 4) };
        private readonly TextBlock _goalHint = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) };
        private readonly StackPanel _list = new StackPanel();
        private readonly SectionCard _listCard = new SectionCard();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        private string _shown;

        // Player rule logics (FMMC_OBJECTIVE_LOGIC_*) and the creator selection each comes from.
        private static readonly (int Logic, int Selection)[] Types =
        {
            (6, 18), (7, 19), (8, 20), (9, 21), (10, 22), (34, 54), (33, 53), (36, 56), (16, 34), (27, 42), (28, 43), (29, 44), (30, 45),
        };

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public PlayerRulesView()
        {
            Margin = new Thickness(0, 12, 0, 12);
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Refresh(false); };
        }

        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && Rules.Ready
            && GTA.Offsets.Editor.Kill.number != 0 && GTA.Offsets.Editor.Kill.rule != 0 && GTA.Offsets.Editor.Kill.pri != 0 && GTA.Offsets.Editor.Kill.NEXT != 0;

        private static long At(long field, int team, int i) => field + team + i * GTA.Offsets.Editor.Kill.NEXT;
        private static int Get(long field, int team, int i) => field == 0 ? 0 : new Global(At(field, team, i)).Get<int>();
        private static void Set(long field, int team, int i, int value) { if (field != 0) new Global(At(field, team, i)).SetInt(value); }
        private static int Number(int team) => new Global(GTA.Offsets.Editor.Kill.number + team).Get<int>();
        private static void SetNumber(int team, int n) => new Global(GTA.Offsets.Editor.Kill.number + team).SetInt(n);

        private static string LogicName(int logic) => logic >= 0 && logic < Rules.LogicNames.Length ? T("rl_logic_" + logic, Rules.LogicNames[logic]) : logic.ToString(CultureInfo.InvariantCulture);

        private static int DefaultLimit(int logic)
        {
            // func_1371: these logics start without a limit, the cutscene with -1, the kill types with 5.
            if (logic == 16) return -1;
            return new[] { 14, 22, 18, 19, 20, 21, 33, 34, 36 }.Contains(logic) ? 0 : 5;
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            var bar = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
            var teamLabel = new TextBlock { Text = T("dash_team", "Team"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), FontSize = 14 };
            teamLabel.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
            bar.Children.Add(teamLabel);
            _teamBox.SelectionChanged += (_, __) => { if (!_sync && _teamBox.SelectedIndex >= 0) { _team = _teamBox.SelectedIndex; Refresh(true); } };
            bar.Children.Add(_teamBox);
            Children.Add(bar);

            var grid = new MasonryPanel { MaxColumns = 2, MinColumnWidth = 420 };
            var left = new StackPanel();
            left.Children.Add(Card("pr_goal", "How does the team win the round?", Goals()));
            left.Children.Add(Card("pr_force", "LTS rules", _status));
            var right = new StackPanel();
            _listCard.Style = (Style)FindResource(typeof(SectionCard));
            _listCard.Title = T("pr_list", "Player rules");
            _listCard.Margin = new Thickness(0, 0, 0, 12);
            _listCard.Content = _list;
            right.Children.Add(_listCard);
            grid.Children.Add(left);
            grid.Children.Add(right);
            Children.Add(grid);
            Refresh(true);
        }

        private SectionCard Card(string key, string fallback, FrameworkElement body)
            => new SectionCard { Style = (Style)FindResource(typeof(SectionCard)), Title = T(key, fallback), Content = body, Margin = new Thickness(0, 0, 0, 12) };

        private FrameworkElement Goals()
        {
            var body = new StackPanel();
            _goalHint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            body.Children.Add(_goals);
            body.Children.Add(_goalHint);
            return body;
        }

        // ----- reading and drawing -----

        private void Refresh(bool force)
        {
            if (!_built)
                return;
            if (!Live)
            {
                _goals.Children.Clear();
                _list.Children.Clear();
                _status.Children.Clear();
                _list.Children.Add(Faint(T("rl_nocreator", "Open a mission (LTS, Capture or Mission Creator) to see its rules."), 13));
                _shown = null;
                return;
            }
            int teams = Rules.Teams();
            if (_teamBox.Items.Count != teams)
            {
                _sync = true;
                _teamBox.Items.Clear();
                for (int t = 0; t < teams; t++) _teamBox.Items.Add(T("dash_team", "Team") + " " + (t + 1));
                if (_team >= teams) _team = 0;
                _teamBox.SelectedIndex = _team;
                _sync = false;
            }

            int n = Math.Max(0, Math.Min(Number(_team), Rules.MaxRules));
            var rows = Enumerable.Range(0, n).Select(i => (Logic: Get(GTA.Offsets.Editor.Kill.rule, _team, i), Pri: Get(GTA.Offsets.Editor.Kill.pri, _team, i), Lim: Get(GTA.Offsets.Editor.Kill.lim, _team, i))).ToList();
            string key = string.Join(";", rows) + "|" + Rules.Nrl(_team) + "|" + Rules.Count(_team) + "|" + PatchState();
            if (!force && key == _shown)
                return;
            _shown = key;

            DrawStatus();
            DrawGoals(rows);
            DrawList(rows);
        }

        private string PatchState() => string.Join(",", ForcePatches().Select(p => p.Name + "=" + p.Active));

        /// <summary>The two patches that stop the LTS creator from forcing its kill rule and rule count.</summary>
        private static IEnumerable<(string Name, bool Known, bool Active)> ForcePatches()
        {
            string script = GTA.CurrentCreatorName();
            foreach (string name in new[] { "dont force kill rule to 6 and number to 1", "nrl fix" })
            {
                var patch = (GTA.Editor.ScrPatches ?? new List<GTA.ScrPatches>()).FirstOrDefault(p => p.patch_name == name && p.script_name == script);
                yield return (name, patch != null, patch != null && ScrPatchesRunner.IsApplied(patch));
            }
        }

        private void DrawStatus()
        {
            _status.Children.Clear();
            bool lts = GTA.CurrentCreatorName() == "fm_lts_creator";
            foreach (var (name, known, active) in ForcePatches())
            {
                if (!known && !(lts || name == "nrl fix"))
                    continue;
                string text = name == "nrl fix" ? T("pr_p_nrl", "Rule count stays as set (nrl fix)") : T("pr_p_kill", "Kill rule no longer forced");
                _status.Children.Add(StatusRow(text, active, known ? (active ? T("pr_on", "active") : T("pr_off", "not active")) : T("pr_none", "no patch for this creator")));
            }
            int nrl = Rules.Nrl(_team), used = Rules.UsedCount(_team);
            var row = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
            var text2 = Faint(string.Format(CultureInfo.CurrentCulture, T("pr_nrl", "Rule count (nrl): {0} · rules in use: {1}"), nrl, used), 12.5);
            text2.VerticalAlignment = VerticalAlignment.Center;
            if (nrl != used && used > 0)
            {
                var fix = SmallButton(string.Format(CultureInfo.CurrentCulture, T("pr_nrl_fix", "Set to {0}"), used), () => { Rules.SetNrl(_team, used); Refresh(true); });
                DockPanel.SetDock(fix, Dock.Right);
                row.Children.Add(fix);
                text2.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
            }
            row.Children.Add(text2);
            _status.Children.Add(row);
            _status.Children.Add(Faint(T("pr_force_hint", "Without these patches the LTS creator turns rule 1 back into \"kill players\" and the rule count back to 1."), 12)
                .Also(t => t.Margin = new Thickness(0, 6, 0, 0)));
        }

        private FrameworkElement StatusRow(string text, bool ok, string state)
        {
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var chip = new Border { CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(1), Padding = new Thickness(8, 2, 8, 2), VerticalAlignment = VerticalAlignment.Center };
            chip.SetResourceReference(Border.BorderBrushProperty, ok ? "OkBrush" : "WarnBrush");
            var label = new TextBlock { Text = state, FontSize = 12, FontWeight = FontWeights.SemiBold };
            label.SetResourceReference(TextBlock.ForegroundProperty, ok ? "OkBrush" : "WarnBrush");
            chip.Child = label;
            DockPanel.SetDock(chip, Dock.Right);
            row.Children.Add(chip);
            var t = new TextBlock { Text = text, FontSize = 13.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(t);
            return row;
        }

        // The first player rule of the team decides how it wins; the tiles set its type.
        private void DrawGoals(List<(int Logic, int Pri, int Lim)> rows)
        {
            _goals.Children.Clear();
            int first = rows.Count == 0 ? -1 : Enumerable.Range(0, rows.Count).OrderBy(i => rows[i].Pri).First();
            int logic = first < 0 ? -1 : rows[first].Logic;
            var goals = new List<(int Logic, string Text)>
            {
                (6, T("pr_g_all", "Kill everyone")),
                (34, T("pr_g_points", "Reach points")),
                (36, T("pr_g_hold", "Hold out (time)")),
            };
            for (int t = 0; t < Rules.Teams(); t++)
                if (t != _team)
                    goals.Add((7 + t, string.Format(CultureInfo.CurrentCulture, T("pr_g_team", "Kill team {0}"), t + 1)));
            foreach (var (l, text) in goals)
            {
                var tile = new ToggleButton { Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, FontWeight = FontWeights.SemiBold, FontSize = 13 },
                    IsChecked = l == logic, Margin = new Thickness(0, 0, 6, 6), Padding = new Thickness(6, 5, 6, 5) };
                tile.SetResourceReference(StyleProperty, "ChoiceTile");
                tile.MinHeight = 44;
                int pick = l;
                tile.Click += (_, __) =>
                {
                    if (first < 0)
                        AddPlayerRule(pick, 0);
                    else
                    {
                        Set(GTA.Offsets.Editor.Kill.rule, _team, first, pick);
                        Set(GTA.Offsets.Editor.Kill.lim, _team, first, DefaultLimit(pick));
                    }
                    Refresh(true);
                };
                _goals.Children.Add(tile);
            }
            _goalHint.Text = first < 0 ? T("pr_g_none", "The team has no player rule yet: pick a goal to add one on rule 1.")
                : string.Format(CultureInfo.CurrentCulture, T("pr_g_first", "Set on player rule {0} (rule {1}). More goals: player rules on the right."), first + 1, rows[first].Pri + 1);
        }

        private void DrawList(List<(int Logic, int Pri, int Lim)> rows)
        {
            _list.Children.Clear();
            _listCard.Summary = string.Format(CultureInfo.CurrentCulture, "{0} / {1}", rows.Count, Rules.MaxRules);
            int ruleCount = Math.Max(Rules.Count(_team), 1);
            for (int i = 0; i < rows.Count; i++)
            {
                int index = i;
                var (logic, pri, lim) = rows[i];
                var card = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 8) };
                card.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                card.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                var stack = new StackPanel();
                var head = new DockPanel();
                var remove = new Button { Width = 26, Height = 26, Content = new TextBlock { Text = "✕", FontSize = 11 }, ToolTip = T("pr_remove", "Remove this player rule") };
                remove.SetResourceReference(StyleProperty, "FieldIconButton");
                remove.Click += (_, __) => { RemovePlayerRule(index); Refresh(true); };
                DockPanel.SetDock(remove, Dock.Right);
                head.Children.Add(remove);
                var title = new TextBlock { Text = string.Format(CultureInfo.CurrentCulture, T("pr_row", "Player rule {0} · rule {1}"), i + 1, pri + 1), FontWeight = FontWeights.Bold, FontSize = 13.5, VerticalAlignment = VerticalAlignment.Center };
                title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                head.Children.Add(title);
                stack.Children.Add(head);

                var fields = new Grid { Margin = new Thickness(0, 8, 0, 0) };
                fields.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                fields.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                fields.ColumnDefinitions.Add(new ColumnDefinition());
                fields.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                fields.ColumnDefinitions.Add(new ColumnDefinition());

                var type = new ComboBox { Height = 30 };
                foreach (var (l, _) in Types)
                    type.Items.Add(new ComboBoxItem { Content = LogicName(l), Tag = l });
                type.SelectedItem = type.Items.Cast<ComboBoxItem>().FirstOrDefault(c => (int)c.Tag == logic);
                type.SelectionChanged += (_, __) =>
                {
                    if (type.SelectedItem is ComboBoxItem c) { Set(GTA.Offsets.Editor.Kill.rule, _team, index, (int)c.Tag); Refresh(true); }
                };
                fields.Children.Add(Field(T("pr_type", "Goal"), type, 0));

                var rule = new ComboBox { Height = 30 };
                for (int r = 0; r < Math.Max(ruleCount, pri + 1) && r < Rules.MaxRules; r++)
                    rule.Items.Add(new ComboBoxItem { Content = T("rl_rule_n", "Rule {0}").Replace("{0}", (r + 1).ToString(CultureInfo.CurrentCulture)), Tag = r });
                rule.SelectedItem = rule.Items.Cast<ComboBoxItem>().FirstOrDefault(c => (int)c.Tag == pri);
                rule.SelectionChanged += (_, __) =>
                {
                    if (rule.SelectedItem is ComboBoxItem c) { Set(GTA.Offsets.Editor.Kill.pri, _team, index, (int)c.Tag); Refresh(true); }
                };
                fields.Children.Add(Field(T("lc_rule", "Rule"), rule, 2));

                var limit = new TextBox { Height = 30, Text = lim.ToString(CultureInfo.InvariantCulture), ToolTip = T("pr_limit_tip", "Kills or points the rule needs; 0 = no limit.") };
                limit.SetResourceReference(StyleProperty, "Watermark");
                limit.TextChanged += (_, __) =>
                {
                    if (int.TryParse(limit.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                        Set(GTA.Offsets.Editor.Kill.lim, _team, index, v);
                };
                fields.Children.Add(Field(T("pr_limit", "Limit"), limit, 4));
                stack.Children.Add(fields);
                card.Child = stack;
                _list.Children.Add(card);
            }
            if (rows.Count == 0)
                _list.Children.Add(Faint(T("pr_empty", "No player rules for this team."), 13).Also(t => t.Margin = new Thickness(0, 0, 0, 8)));

            var add = new WrapPanel { Margin = new Thickness(0, 4, 0, 0) };
            var onRule = new ComboBox { Height = 30, MinWidth = 150, Margin = new Thickness(0, 0, 6, 6) };
            for (int r = 0; r < ruleCount; r++)
                onRule.Items.Add(new ComboBoxItem { Content = T("rl_rule_n", "Rule {0}").Replace("{0}", (r + 1).ToString(CultureInfo.CurrentCulture)), Tag = r });
            bool canNew = RulePresets.Ready && Rules.Count(_team) < Rules.MaxRules;
            if (canNew)
                onRule.Items.Add(new ComboBoxItem { Content = string.Format(CultureInfo.CurrentCulture, T("pr_newrule", "new rule {0}"), Rules.Count(_team) + 1), Tag = -1 });
            onRule.SelectedIndex = onRule.Items.Count - 1;
            var button = new Button { Content = T("pr_add", "+ Player rule"), Height = 30, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 6, 6), IsEnabled = rows.Count < Rules.MaxRules };
            button.SetResourceReference(StyleProperty, "FormButtonPrimary");
            button.Click += (_, __) =>
            {
                if (!(onRule.SelectedItem is ComboBoxItem c))
                    return;
                AddPlayerRule(6, (int)c.Tag);
                Refresh(true);
            };
            add.Children.Add(button);
            add.Children.Add(Faint(T("pr_onrule", "on"), 13).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(0, 0, 6, 6); }));
            add.Children.Add(onRule);
            _list.Children.Add(add);
        }

        private static FrameworkElement Field(string label, FrameworkElement box, int column)
        {
            var cell = new StackPanel();
            var l = new TextBlock { Text = label };
            l.SetResourceReference(StyleProperty, "FieldLabel");
            cell.Children.Add(l);
            cell.Children.Add(box);
            Grid.SetColumn(cell, column);
            return cell;
        }

        // ----- writing -----

        /// <summary>Appends a player rule like func_1371. rule -1 = a new rule at the end first.</summary>
        private void AddPlayerRule(int logic, int rule)
        {
            int n = Number(_team);
            if (n < 0) n = 0;
            if (n >= Rules.MaxRules)
                return;
            if (rule < 0)
            {
                int selection = Types.First(t => t.Logic == logic).Selection;
                rule = RulePresets.AddRule(_team, selection, out _);
                if (rule < 0)
                    return;
            }
            Set(GTA.Offsets.Editor.Kill.rule, _team, n, logic);
            Set(GTA.Offsets.Editor.Kill.pri, _team, n, rule);
            Set(GTA.Offsets.Editor.Kill.lim, _team, n, DefaultLimit(logic));
            Set(GTA.Offsets.Editor.Kill.prbs, _team, n, 0);
            SetNumber(_team, n + 1);
            // LTS and Capture count rules by nrl: make sure it covers the player rule's rule.
            if (!Rules.PublicCreator && Rules.Nrl(_team) < rule + 1)
                Rules.SetNrl(_team, rule + 1);
        }

        /// <summary>Removes a player rule of this team; the later ones move up.</summary>
        private void RemovePlayerRule(int index)
        {
            int n = Number(_team);
            if (index < 0 || index >= n)
                return;
            var fields = new[] { GTA.Offsets.Editor.Kill.rule, GTA.Offsets.Editor.Kill.pri, GTA.Offsets.Editor.Kill.lim, GTA.Offsets.Editor.Kill.jtop, GTA.Offsets.Editor.Kill.jtof, GTA.Offsets.Editor.Kill.prbs };
            for (int i = index; i < n - 1; i++)
                foreach (long f in fields)
                    Set(f, _team, i, Get(f, _team, i + 1));
            Set(GTA.Offsets.Editor.Kill.rule, _team, n - 1, 0);
            Set(GTA.Offsets.Editor.Kill.pri, _team, n - 1, Rules.PriorityNone);
            Set(GTA.Offsets.Editor.Kill.lim, _team, n - 1, 0);
            SetNumber(_team, n - 1);
        }

        // ----- small parts -----

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }

        private static Button SmallButton(string text, Action click)
        {
            var b = new Button { Content = text, Height = 26, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(8, 0, 0, 0) };
            b.SetResourceReference(StyleProperty, "FormButton");
            b.Click += (_, __) => click();
            return b;
        }
    }
}
