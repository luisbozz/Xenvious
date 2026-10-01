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
    /// Kill page (mockup https://claude.ai/artifact/VifG8TGRs71syLT1EsWBmP, tab "LTS Kill", version 2):
    /// the team's rules as steps; a rule with a player rule (Global_4718592.f_114365, count
    /// f_115318[team]) shows its goal as icon tiles and its limit. The team comes from the page's
    /// header (the old Kill page's team box).
    ///
    /// The LTS creator forces player rule 0 to "kill players" on rule 1 every frame (func_4836) and
    /// sets nrl to 1 at test and save (func_904). The patches "dont force kill rule to 6 and number
    /// to 1" and "nrl fix" switch both off; the side card shows their state and links to the Script
    /// Patches page. A new player rule gets the Mission Creator's defaults (func_1371): limit 5 for
    /// the kill types, 0 for the others, -1 for the cutscene.
    ///
    /// How the Mission Controller reads a player rule (public_mission_controller, the player rule
    /// switch and its load loop):
    /// - kill (6, 7-10): the limit is a menu step (0-10, 11 = 15, 12 = 20, 13 = no count); the rule
    ///   passes at that many kills, or when the enemy team(s) are out. 0 and 13 only pass then.
    /// - go to team (27-30): passes when a player gets to a player of that team; no limit.
    /// - loot (33) and points (34): the limit is the raw amount. Loot reads two bits of f_46: bit 0
    ///   counts percent of all loot, bit 1 multiplies the limit by the team's players.
    /// - cutscene (16): the limit is the index of one of the creator's five scripted cutscenes.
    /// - hold (36): never passes by itself, only through a time limit or another rule's limit.
    /// - f_15 / f_41: the rule to jump to on pass / on fail; ignored unless it is after this rule.
    /// </summary>
    public class PlayerRulesView : Grid
    {
        private readonly ComboBox _teamBox;
        private readonly Action _openPatches;
        private int _team;
        private int _open = -1;           // player rule whose step is open
        private bool _built;
        private readonly StackPanel _steps = new StackPanel();
        private readonly WrapPanel _add = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
        private readonly StackPanel _status = new StackPanel();
        private readonly SectionCard _goalsCard = new SectionCard();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        private string _shown;

        // The goals as tiles: logic (FMMC_OBJECTIVE_LOGIC_*), creator selection, label, icon.
        // "Kill a team" and "go to team" stand for logic 7-10 / 27-30, the team is picked below.
        private static readonly (int Logic, int Selection, string Key, string Fallback, string Icon)[] Goals =
        {
            (6, 18, "pr_g_all", "Kill everyone", "M5,5 L19,19 M19,5 L5,19"),
            (7, 19, "pr_g_teamkill", "Kill a team", "M4,20 L14,10 M10,4 L20,14 M14,4 L20,10 M4,14 L10,20"),
            (27, 42, "pr_g_goteam", "Go to team", "M4,12 L18,12 M12,6 L18,12 L12,18"),
            (34, 54, "pr_g_points", "Points", "M4,12 A8,8 0 1 0 20,12 A8,8 0 1 0 4,12 M9,12 A3,3 0 1 0 15,12 A3,3 0 1 0 9,12"),
            (33, 53, "pr_g_loot", "Loot", "M6,9 L18,9 L20,21 L4,21 Z M9,9 C9,4 15,4 15,9"),
            (36, 56, "pr_g_hold", "Hold out", "M7,3 L17,3 M7,21 L17,21 M8,3 C8,9 16,9 16,12 C16,15 8,15 8,21 M16,3 C16,9 8,9 8,12 C8,15 16,15 16,21"),
            (16, 34, "pr_g_cutscene", "Cutscene", "M7,5 L19,12 L7,19 Z"),
        };

        // Kill limits are the creator's menu steps; the controller turns them into kills (func_666).
        private static readonly int[] KillSteps = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20 };

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public PlayerRulesView(ComboBox teamBox, Action openPatches)
        {
            _teamBox = teamBox;
            _openPatches = openPatches;
            Margin = new Thickness(0, 12, 0, 12);
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 240 });
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Refresh(false); };
            _teamBox.SelectionChanged += (_, __) => { _open = -1; Refresh(true); };
        }

        private static bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && Rules.Ready
            && GTA.Offsets.Editor.Kill.number != 0 && GTA.Offsets.Editor.Kill.rule != 0 && GTA.Offsets.Editor.Kill.pri != 0 && GTA.Offsets.Editor.Kill.NEXT != 0;

        private static long At(long field, int team, int i) => field + team + i * GTA.Offsets.Editor.Kill.NEXT;
        private static int Get(long field, int team, int i) => field == 0 ? 0 : new Global(At(field, team, i)).Get<int>();
        private static int Number(int team) => new Global(GTA.Offsets.Editor.Kill.number + team).Get<int>();

        // The old Kill page keeps its own copy of the player rules (MainWindow.kill): its raw
        // values show that copy, "Freeze values" writes it into the game all the time and "Save to
        // config file" keeps it. Every write here goes into the copy too, so freeze cannot undo it.
        private static void Set(long field, int team, int i, int value)
        {
            if (field == 0)
                return;
            new Global(At(field, team, i)).SetInt(value);
            var values = MainWindow.kill?.values;
            if (values == null || i < 0 || i >= values.Length || team < 0 || team > 3)
                return;
            int[] slot = field == GTA.Offsets.Editor.Kill.rule ? values[i].rule : field == GTA.Offsets.Editor.Kill.pri ? values[i].prio
                : field == GTA.Offsets.Editor.Kill.lim ? values[i].lim : field == GTA.Offsets.Editor.Kill.jtop ? values[i].jtop
                : field == GTA.Offsets.Editor.Kill.jtof ? values[i].jtof : field == GTA.Offsets.Editor.Kill.prbs ? values[i].prbs : null;
            if (slot != null)
                slot[team] = value;
        }

        private static void SetNumber(int team, int n)
        {
            new Global(GTA.Offsets.Editor.Kill.number + team).SetInt(n);
            if (MainWindow.kill?.number != null && team >= 0 && team < MainWindow.kill.number.Length)
                MainWindow.kill.number[team] = n;
        }

        private static string LogicName(int logic) => logic >= 0 && logic < Rules.LogicNames.Length ? T("rl_logic_" + logic, Rules.LogicNames[logic]) : logic.ToString(CultureInfo.InvariantCulture);

        // The goal tile a logic belongs to (7-10 kill team, 27-30 go to team).
        private static int GoalOf(int logic) => logic >= 7 && logic <= 10 ? 7 : logic >= 27 && logic <= 30 ? 27 : logic;

        private static int DefaultLimit(int logic)
        {
            if (logic == 16) return -1;
            return new[] { 14, 22, 18, 19, 20, 21, 33, 34, 36 }.Contains(logic) ? 0 : 5;
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            _goalsCard.Style = (Style)FindResource(typeof(SectionCard));
            _goalsCard.Icon = Geometry.Parse("M5,21 L5,4 M5,4 L16,4 L14,8 L16,12 L5,12");
            var body = new StackPanel();
            body.Children.Add(_steps);
            body.Children.Add(_add);
            _goalsCard.Content = body;
            _goalsCard.VerticalAlignment = VerticalAlignment.Top;
            Children.Add(_goalsCard);

            var side = new SectionCard { Style = (Style)FindResource(typeof(SectionCard)), Title = T("pr_force", "LTS rules"), Content = _status, VerticalAlignment = VerticalAlignment.Top };
            Grid.SetColumn(side, 2);
            Children.Add(side);
            Refresh(true);
        }

        // ----- reading and drawing -----

        private void Refresh(bool force)
        {
            if (!_built)
                return;
            if (!Live)
            {
                _steps.Children.Clear();
                _add.Children.Clear();
                _status.Children.Clear();
                _steps.Children.Add(Faint(T("rl_nocreator", "Open a mission (LTS, Capture or Mission Creator) to see its rules."), 13));
                _shown = null;
                return;
            }
            // Without freeze the old page's copy follows the game, so its raw values show the job.
            if (MainWindow.Instance?.cbmissionkillfreeze.IsChecked != true)
                MainWindow.Instance?.LoadKillFromGame();
            _team = Math.Max(0, Math.Min(_teamBox.SelectedIndex, Rules.Teams() - 1));
            int n = Math.Max(0, Math.Min(Number(_team), Rules.MaxRules));
            var rows = Enumerable.Range(0, n).Select(i => (Index: i, Logic: Get(GTA.Offsets.Editor.Kill.rule, _team, i), Pri: Get(GTA.Offsets.Editor.Kill.pri, _team, i), Lim: Get(GTA.Offsets.Editor.Kill.lim, _team, i))).ToList();
            string key = string.Join(";", rows) + "|" + _team + "|" + _open + "|" + Rules.Nrl(_team) + "|" + Rules.Count(_team) + "|" + string.Join(",", ForcePatches());
            if (!force && key == _shown)
                return;
            _shown = key;

            int count = Rules.Count(_team);
            _goalsCard.Title = string.Format(CultureInfo.CurrentCulture, T("pr_goals", "Goals · team {0}"), _team + 1);
            _goalsCard.Summary = string.Format(CultureInfo.CurrentCulture, T("pr_goals_sum", "{0} goals · {1} rules"), rows.Count, count);
            DrawSteps(rows, count);
            DrawAdd(rows.Count, count);
            DrawStatus();
        }

        private static IEnumerable<(string Name, bool Known, bool Active)> ForcePatches()
        {
            string script = GTA.CurrentCreatorName();
            foreach (string name in new[] { "dont force kill rule to 6 and number to 1", "nrl fix" })
            {
                var patch = (GTA.Editor.ScrPatches ?? new List<GTA.ScrPatches>()).FirstOrDefault(p => p.patch_name == name && p.script_name == script);
                yield return (name, patch != null, patch != null && ScrPatchesRunner.IsApplied(patch));
            }
        }

        private void DrawSteps(List<(int Index, int Logic, int Pri, int Lim)> rows, int count)
        {
            _steps.Children.Clear();
            int last = Math.Max(count, rows.Count == 0 ? 0 : rows.Max(r => r.Pri) + 1);
            for (int rule = 0; rule < last && rule < Rules.MaxRules; rule++)
            {
                var here = rows.Where(r => r.Pri == rule).ToList();
                bool more = rule < last - 1;
                if (here.Count == 0)
                    _steps.Children.Add(Step(rule, null, more));
                foreach (var row in here)
                    _steps.Children.Add(Step(rule, row, more || row.Index != here.Last().Index));
            }
            if (last == 0)
                _steps.Children.Add(Faint(T("pr_g_none2", "The team has no rule yet: add a goal below."), 13));
        }

        private FrameworkElement Step(int rule, (int Index, int Logic, int Pri, int Lim)? row, bool more)
        {
            bool open = row != null && row.Value.Index == _open;
            var grid = new Grid { MinHeight = 46 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            if (more)
            {
                var line = new Rectangle { Width = 2, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(15, 32, 0, 0) };
                line.SetResourceReference(Shape.FillProperty, "LineBrush");
                grid.Children.Add(line);
            }
            var dot = new Border { Width = 32, Height = 32, CornerRadius = new CornerRadius(16), BorderThickness = new Thickness(2), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            var num = new TextBlock { Text = "R" + (rule + 1), FontWeight = FontWeights.Bold, FontSize = 11.5, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (row == null)
            {
                dot.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                num.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            }
            else
            {
                dot.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
                dot.Background = new SolidColorBrush(Color.FromArgb(0x2E, 0x5B, 0x9B, 0xE6));
                num.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            }
            dot.Child = num;
            grid.Children.Add(dot);

            var body = new StackPanel { Margin = new Thickness(0, 4, 8, 10) };
            Grid.SetColumn(body, 1);
            grid.Children.Add(body);
            if (row == null)
            {
                body.Children.Add(Faint(T("pr_noplayer", "No player goal"), 13.5).Also(t => t.FontWeight = FontWeights.Bold));
                body.Children.Add(Faint(T("pr_noplayer_sub", "A rule for entities (actors, vehicles, objects, locations)."), 12));
                return grid;
            }

            var r = row.Value;
            var title = new TextBlock { FontSize = 14, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            title.Inlines.Add(new System.Windows.Documents.Run(LogicName(r.Logic)));
            string limitText = LimitText(r.Logic, r.Lim);
            if (limitText != null)
            {
                var lim = new System.Windows.Documents.Run("  · " + limitText) { FontSize = 12, FontWeight = FontWeights.SemiBold };
                lim.SetResourceReference(System.Windows.Documents.TextElement.ForegroundProperty, "FaintTextBrush");
                title.Inlines.Add(lim);
            }
            body.Children.Add(title);
            if (open)
                body.Children.Add(Editor(r));
            else
                body.Children.Add(Faint(GoalHint(r.Logic), 12));

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
            var chevron = new Path { Data = Geometry.Parse(open ? "M2,5 L8,11 L14,5" : "M5,2 L11,8 L5,14"), StrokeThickness = 1.8, Width = 12, Height = 12, Stretch = Stretch.Uniform,
                StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            chevron.SetResourceReference(Shape.StrokeProperty, "TextColor");
            var toggle = IconButton(chevron, open ? T("gr_close", "Close") : T("pr_edit", "Edit this goal"));
            toggle.Click += (_, __) => { _open = open ? -1 : r.Index; Refresh(true); };
            var remove = IconButton(new TextBlock { Text = "✕", FontSize = 11 }, T("pr_remove", "Remove this player rule"));
            remove.Margin = new Thickness(6, 0, 0, 0);
            remove.Click += (_, __) => { RemovePlayerRule(r.Index); _open = -1; Refresh(true); };
            buttons.Children.Add(toggle);
            buttons.Children.Add(remove);
            Grid.SetColumn(buttons, 2);
            grid.Children.Add(buttons);
            return grid;
        }

        private static string GoalHint(int logic)
        {
            switch (GoalOf(logic))
            {
                case 6: return T("pr_h2_all", "Passes at the kill count, or once no player of the other teams is left.");
                case 7: return T("pr_h2_team", "Passes at the kill count, or once no player of that team is left.");
                case 27: return T("pr_h2_goteam", "Passes when a player of the team gets to a player of the chosen team.");
                case 34: return T("pr_h2_points", "Passes when the team's score reaches the limit.");
                case 33: return T("pr_h2_loot", "Passes when the team has grabbed this much loot.");
                case 36: return T("pr_h2_hold", "Never passes by itself: a time limit or another rule's limit ends it (Rules page).");
                case 16: return T("pr_h2_cutscene", "Plays one of the creator's scripted cutscenes, then passes.");
                default: return T("pr_h2_other", "Not one of the goals above; edit it in the raw values below.");
            }
        }

        /// <summary>The limit as the game reads it, null when the goal has none.</summary>
        private static string LimitText(int logic, int lim)
        {
            switch (GoalOf(logic))
            {
                case 6:
                case 7:
                    return lim <= 0 || lim >= KillSteps.Length ? T("pr_kills_all", "until all are out")
                        : string.Format(CultureInfo.CurrentCulture, T("pr_kills_n", "{0} kills"), KillSteps[lim]);
                case 33:
                case 34:
                    return string.Format(CultureInfo.CurrentCulture, T("pr_limit_n", "limit {0}"), lim);
                case 16:
                    return lim < 0 ? T("pr_cut_none", "no cutscene") : string.Format(CultureInfo.CurrentCulture, T("pr_cut_n", "Cutscene {0}"), lim + 1);
                default:
                    return null;
            }
        }

        private static bool CutscenesKnown => GTA.Offsets.Editor.Kill.cutscene != 0 && GTA.Offsets.Editor.Kill.cutscene_NEXT != 0;

        // The creator's five scripted cutscene slots that are in use (a name is set).
        private static List<int> Cutscenes()
        {
            var list = new List<int>();
            if (!CutscenesKnown)
                return list;
            for (int i = 0; i < 5; i++)
            {
                byte[] name = new Global(GTA.Offsets.Editor.Kill.cutscene + i * GTA.Offsets.Editor.Kill.cutscene_NEXT).GetBytes(1);
                if (name != null && name.Length > 0 && name[0] != 0)
                    list.Add(i);
            }
            return list;
        }

        // The open step: goal tiles with icons, the team for team goals, then what the goal needs.
        private FrameworkElement Editor((int Index, int Logic, int Pri, int Lim) r)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
            var tiles = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, -6, 2) };
            int goal = GoalOf(r.Logic);
            foreach (var g in Goals)
            {
                // Only the Mission Creator places scripted cutscenes.
                bool usable = g.Logic != 16 || Rules.PublicCreator;
                var tile = new ToggleButton { IsChecked = g.Logic == goal, Margin = new Thickness(0, 0, 6, 6), IsEnabled = usable };
                if (!usable)
                {
                    ToolTipService.SetShowOnDisabled(tile, true);
                    tile.ToolTip = T("pr_cut_mission", "Only the Mission Creator has scripted cutscenes.");
                }
                var icon = new Path { Data = Geometry.Parse(g.Icon), StrokeThickness = 1.6, Width = 22, Height = 22, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center,
                    StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
                icon.SetBinding(Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { Source = tile });
                var content = new StackPanel();
                content.Children.Add(icon);
                content.Children.Add(new TextBlock { Text = T(g.Key, g.Fallback), FontSize = 12.5, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 4, 0, 0) });
                tile.Content = content;
                tile.SetResourceReference(StyleProperty, "ChoiceTile");
                tile.MinHeight = 62;
                int logic = g.Logic;
                tile.Click += (_, __) =>
                {
                    // Team goals keep their team, or take the first other team.
                    int pick = logic;
                    if (logic == 7 || logic == 27)
                        pick = logic + (GoalOf(r.Logic) == logic ? r.Logic - logic : FirstOtherTeam());
                    Set(GTA.Offsets.Editor.Kill.rule, _team, r.Index, pick);
                    Set(GTA.Offsets.Editor.Kill.lim, _team, r.Index, DefaultLimit(pick));
                    Refresh(true);
                };
                tiles.Children.Add(tile);
            }
            panel.Children.Add(tiles);

            if (goal == 7 || goal == 27)
                panel.Children.Add(TeamTabs(r.Logic - goal, t => { Set(GTA.Offsets.Editor.Kill.rule, _team, r.Index, goal + t); Refresh(true); }));

            switch (goal)
            {
                case 6:
                case 7:
                    int step = r.Lim <= 0 || r.Lim >= KillSteps.Length ? 0 : r.Lim;
                    panel.Children.Add(Label(T("pr_kills", "Kills needed")));
                    panel.Children.Add(SliderRow(0, KillSteps.Length - 1, step, v => LimitText(goal, v), false,
                        v => Set(GTA.Offsets.Editor.Kill.lim, _team, r.Index, v)));
                    break;
                case 34:
                    panel.Children.Add(Label(T("pr_points", "Score needed")));
                    panel.Children.Add(SliderRow(0, Math.Max(500, r.Lim), Math.Max(0, r.Lim), v => v.ToString(CultureInfo.InvariantCulture), true,
                        v => Set(GTA.Offsets.Editor.Kill.lim, _team, r.Index, v)));
                    break;
                case 33:
                    int bits = Get(GTA.Offsets.Editor.Kill.prbs, _team, r.Index);
                    bool percent = (bits & 1) != 0;
                    panel.Children.Add(Label(percent ? T("pr_loot_pct", "Loot needed (percent of all loot)") : T("pr_loot", "Loot needed")));
                    panel.Children.Add(SliderRow(0, percent ? 100 : Math.Max(1000, r.Lim), Math.Max(0, r.Lim), v => v.ToString(CultureInfo.InvariantCulture) + (percent ? " %" : ""), true,
                        v => Set(GTA.Offsets.Editor.Kill.lim, _team, r.Index, v)));
                    panel.Children.Add(BitBox(T("pr_loot_pctbox", "Count in percent of all loot"), r.Index, 0));
                    panel.Children.Add(BitBox(T("pr_loot_per", "Multiply by the team's players"), r.Index, 1));
                    break;
                case 36:
                    // Hold only ends through the rule's time limit (or another rule's limit).
                    int tmt = Rules.GetField(GTA.Offsets.Editor.tmt, _team, r.Pri);
                    int at = Math.Max(0, Array.FindIndex(Rules.TimeLimits, t => t.Selection == tmt));
                    panel.Children.Add(Label(T("pr_hold_time", "Time limit of rule {0}").Replace("{0}", (r.Pri + 1).ToString(CultureInfo.CurrentCulture))));
                    panel.Children.Add(SliderRow(0, Rules.TimeLimits.Length - 1, at,
                        v => v == 0 ? T("rl_time_off", "No limit") : RulesView.TimeLabel(Rules.TimeLimits[v].Seconds), false,
                        v => Rules.SetField(GTA.Offsets.Editor.tmt, _team, r.Pri, Rules.TimeLimits[v].Selection)));
                    if (tmt == 0)
                        panel.Children.Add(Faint(T("pr_hold_none", "Without a time limit the team stays on this rule."), 12).Also(t => { t.Margin = new Thickness(0, 4, 0, 0); t.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush"); }));
                    break;
                case 16:
                    panel.Children.Add(Label(T("pr_cut", "Cutscene")));
                    var cuts = new ComboBox { Height = 30, MinWidth = 180, HorizontalAlignment = HorizontalAlignment.Left };
                    cuts.Items.Add(new ComboBoxItem { Content = T("pr_cut_none", "no cutscene"), Tag = -1 });
                    var used = Cutscenes();
                    if (r.Lim >= 0 && !used.Contains(r.Lim))
                        used.Add(r.Lim);
                    foreach (int c in used.OrderBy(c => c))
                        cuts.Items.Add(new ComboBoxItem { Content = string.Format(CultureInfo.CurrentCulture, T("pr_cut_n", "Cutscene {0}"), c + 1), Tag = c });
                    cuts.SelectedItem = cuts.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == r.Lim) ?? cuts.Items[0];
                    cuts.SelectionChanged += (_, __) => { if (cuts.SelectedItem is ComboBoxItem i) { Set(GTA.Offsets.Editor.Kill.lim, _team, r.Index, (int)i.Tag); Refresh(true); } };
                    panel.Children.Add(cuts);
                    if (Cutscenes().Count == 0)
                        panel.Children.Add(Faint(T("pr_cut_empty", "The mission has no scripted cutscene yet; place one in the creator first."), 12).Also(t => t.Margin = new Thickness(0, 4, 0, 0)));
                    break;
            }

            panel.Children.Add(Label(T("pr_jump", "Afterwards")));
            var jumps = new WrapPanel();
            jumps.Children.Add(JumpBox(T("pr_jump_pass", "On pass"), GTA.Offsets.Editor.Kill.jtop, r));
            jumps.Children.Add(JumpBox(T("pr_jump_fail", "On fail"), GTA.Offsets.Editor.Kill.jtof, r));
            panel.Children.Add(jumps);
            return panel;
        }

        private static TextBlock Label(string text)
        {
            var label = new TextBlock { Text = text, Margin = new Thickness(0, 8, 0, 0) };
            label.SetResourceReference(StyleProperty, "FieldLabel");
            return label;
        }

        // A slider with its value beside it, like the zone sliders; editable keeps a text box there.
        private static FrameworkElement SliderRow(int min, int max, int value, Func<int, string> format, bool editable, Action<int> write)
        {
            var row = new DockPanel();
            var slider = new Slider { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), IsSnapToTickEnabled = true, TickFrequency = 1, VerticalAlignment = VerticalAlignment.Center };
            FrameworkElement side;
            bool sync = false;
            if (editable)
            {
                var box = new TextBox { Width = 86, Height = 30, Text = value.ToString(CultureInfo.InvariantCulture) };
                box.SetResourceReference(StyleProperty, "Watermark");
                slider.ValueChanged += (_, __) => { if (sync) return; sync = true; box.Text = ((int)slider.Value).ToString(CultureInfo.InvariantCulture); sync = false; };
                box.TextChanged += (_, __) =>
                {
                    if (!int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) || v < min) return;
                    write(v);
                    if (!sync) { sync = true; slider.Maximum = Math.Max(slider.Maximum, v); slider.Value = v; sync = false; }
                };
                side = box;
            }
            else
            {
                var text = new TextBlock { Text = format(value), FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right, MinWidth = 110 };
                text.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                slider.ValueChanged += (_, __) => { int v = (int)slider.Value; text.Text = format(v); write(v); };
                side = text;
            }
            side.Margin = new Thickness(10, 0, 0, 0);
            DockPanel.SetDock(side, Dock.Right);
            row.Children.Add(side);
            row.Children.Add(slider);
            return row;
        }

        // A switch with its text beside it, like the Rules page's rows.
        private FrameworkElement BitBox(string text, int index, int bit)
        {
            var row = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
            var box = new CheckBox { Style = (Style)MainWindow.Instance.FindResource("FormToggle"), IsChecked = (Get(GTA.Offsets.Editor.Kill.prbs, _team, index) & (1 << bit)) != 0, VerticalAlignment = VerticalAlignment.Center };
            box.Click += (_, __) =>
            {
                int v = Get(GTA.Offsets.Editor.Kill.prbs, _team, index);
                Set(GTA.Offsets.Editor.Kill.prbs, _team, index, box.IsChecked == true ? v | (1 << bit) : v & ~(1 << bit));
                Refresh(true);
            };
            DockPanel.SetDock(box, Dock.Right);
            row.Children.Add(box);
            var label = new TextBlock { Text = text, FontSize = 13, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            label.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(label);
            return row;
        }

        // The rule to jump to; the controller ignores targets that are not after this rule.
        private FrameworkElement JumpBox(string caption, long field, (int Index, int Logic, int Pri, int Lim) r)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 14, 6) };
            panel.Children.Add(Faint(caption, 13).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(0, 0, 8, 0); }));
            var box = new ComboBox { Height = 30, MinWidth = 140, IsEnabled = field != 0 };
            box.Items.Add(new ComboBoxItem { Content = T("pr_jump_next", "next rule"), Tag = 0 });
            int current = Get(field, _team, r.Index);
            int count = Rules.Count(_team);
            for (int rule = r.Pri + 1; rule < Math.Max(count, current + 1); rule++)
                box.Items.Add(new ComboBoxItem { Content = T("rl_rule_n", "Rule {0}").Replace("{0}", (rule + 1).ToString(CultureInfo.CurrentCulture)), Tag = rule });
            box.SelectedItem = box.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == current) ?? box.Items[0];
            box.SelectionChanged += (_, __) => { if (box.SelectedItem is ComboBoxItem i) Set(field, _team, r.Index, (int)i.Tag); };
            panel.Children.Add(box);
            return panel;
        }

        private int FirstOtherTeam()
        {
            for (int t = 0; t < Rules.Teams(); t++)
                if (t != _team) return t;
            return 0;
        }

        // Teams as nav tabs in their colours, in the header's dark box (team selector style).
        private static FrameworkElement TeamTabs(int selected, Action<int> pick)
        {
            var tabs = new StackPanel { Orientation = Orientation.Horizontal };
            for (int t = 0; t < Rules.Teams(); t++)
            {
                int team = t;
                var tab = new ToggleButton { Content = T("dash_team", "Team") + " " + (t + 1), IsChecked = t == selected };
                tab.SetResourceReference(StyleProperty, "NavTab");
                if (MainWindow.ThemeBrush("TeamBrush" + (t + 1)) is SolidColorBrush colour)
                {
                    tab.Resources["AccentBrush"] = colour;
                    tab.Resources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x70, colour.Color.R, colour.Color.G, colour.Color.B));
                }
                tab.Click += (_, __) => pick(team);
                tabs.Children.Add(tab);
            }
            var box = new Border { Child = tabs, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 4, 0, 4) };
            box.SetResourceReference(StyleProperty, "NavGroup");
            return box;
        }

        private void DrawAdd(int players, int count)
        {
            _add.Children.Clear();
            var onRule = new ComboBox { Height = 30, MinWidth = 150, Margin = new Thickness(0, 0, 6, 6) };
            for (int r = 0; r < count; r++)
                onRule.Items.Add(new ComboBoxItem { Content = T("rl_rule_n", "Rule {0}").Replace("{0}", (r + 1).ToString(CultureInfo.CurrentCulture)), Tag = r });
            if (RulePresets.Ready && count < Rules.MaxRules)
                onRule.Items.Add(new ComboBoxItem { Content = string.Format(CultureInfo.CurrentCulture, T("pr_newrule", "new rule {0}"), count + 1), Tag = -1 });
            onRule.SelectedIndex = onRule.Items.Count - 1;
            var button = new Button { Content = T("pr_add_goal", "+ Goal"), Height = 30, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 6, 6), IsEnabled = players < Rules.MaxRules && onRule.Items.Count > 0 };
            button.SetResourceReference(StyleProperty, "FormButtonPrimary");
            button.Click += (_, __) =>
            {
                if (!(onRule.SelectedItem is ComboBoxItem c))
                    return;
                _open = AddPlayerRule(6, (int)c.Tag);
                Refresh(true);
            };
            _add.Children.Add(button);
            _add.Children.Add(Faint(T("pr_onrule", "on"), 13).Also(t => { t.VerticalAlignment = VerticalAlignment.Center; t.Margin = new Thickness(0, 0, 6, 6); }));
            _add.Children.Add(onRule);
        }

        private void DrawStatus()
        {
            _status.Children.Clear();
            foreach (var (name, known, active) in ForcePatches())
            {
                string text = name == "nrl fix" ? T("pr_p_nrl", "Rule count stays as set (nrl fix)") : T("pr_p_kill", "Kill rule no longer forced");
                _status.Children.Add(StatusRow(text, active, known ? (active ? T("pr_on", "active") : T("pr_off", "not active")) : T("pr_none", "no patch for this creator")));
            }
            int nrl = Rules.Nrl(_team), used = Rules.UsedCount(_team);
            var row = new DockPanel { Margin = new Thickness(0, 6, 0, 0) };
            var text2 = Faint(string.Format(CultureInfo.CurrentCulture, T("pr_nrl", "Rule count (nrl): {0} · rules in use: {1}"), nrl, used), 12.5);
            text2.VerticalAlignment = VerticalAlignment.Center;
            if (nrl != used && used > 0)
            {
                var fix = new Button { Content = string.Format(CultureInfo.CurrentCulture, T("pr_nrl_fix", "Set to {0}"), used), Height = 26, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(8, 0, 0, 0) };
                fix.SetResourceReference(StyleProperty, "FormButton");
                fix.Click += (_, __) => { Rules.SetNrl(_team, used); Refresh(true); };
                DockPanel.SetDock(fix, Dock.Right);
                row.Children.Add(fix);
                text2.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
            }
            row.Children.Add(text2);
            _status.Children.Add(row);
            _status.Children.Add(Faint(T("pr_force_hint", "Without these patches the LTS creator turns rule 1 back into \"kill players\" and the rule count back to 1."), 12)
                .Also(t => t.Margin = new Thickness(0, 8, 0, 8)));
            var link = new Button { Content = T("pr_patches", "Open Script Patches"), Height = 30, Padding = new Thickness(12, 0, 12, 0), HorizontalAlignment = HorizontalAlignment.Left };
            link.SetResourceReference(StyleProperty, "FormButton");
            link.Click += (_, __) => _openPatches?.Invoke();
            _status.Children.Add(link);
        }

        private static FrameworkElement StatusRow(string text, bool ok, string state)
        {
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var chip = new Border { CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(1), Padding = new Thickness(8, 2, 8, 2), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
            chip.SetResourceReference(Border.BorderBrushProperty, ok ? "OkBrush" : "WarnBrush");
            var label = new TextBlock { Text = state, FontSize = 12, FontWeight = FontWeights.SemiBold };
            label.SetResourceReference(TextBlock.ForegroundProperty, ok ? "OkBrush" : "WarnBrush");
            chip.Child = label;
            DockPanel.SetDock(chip, Dock.Right);
            row.Children.Add(chip);
            var t = new TextBlock { Text = text, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(t);
            return row;
        }

        // ----- writing -----

        /// <summary>Appends a player rule like func_1371; rule -1 = a new rule at the end first. Returns its index or -1.</summary>
        private int AddPlayerRule(int logic, int rule)
        {
            int n = Math.Max(0, Number(_team));
            if (n >= Rules.MaxRules)
                return -1;
            if (rule < 0)
            {
                rule = RulePresets.AddRule(_team, Goals.First(g => g.Logic == logic).Selection, out _);
                if (rule < 0)
                    return -1;
            }
            Set(GTA.Offsets.Editor.Kill.rule, _team, n, logic);
            Set(GTA.Offsets.Editor.Kill.pri, _team, n, rule);
            Set(GTA.Offsets.Editor.Kill.lim, _team, n, DefaultLimit(logic));
            Set(GTA.Offsets.Editor.Kill.prbs, _team, n, 0);
            SetNumber(_team, n + 1);
            // LTS and Capture count rules by nrl: it has to cover the player rule's rule.
            if (!Rules.PublicCreator && Rules.Nrl(_team) < rule + 1)
                Rules.SetNrl(_team, rule + 1);
            return n;
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

        private static Button IconButton(object content, string tip)
        {
            var b = new Button { Content = content, Width = 30, Height = 30, ToolTip = tip };
            b.SetResourceReference(StyleProperty, "FieldIconButton");
            return b;
        }
    }
}
