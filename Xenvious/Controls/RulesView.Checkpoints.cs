using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Xenvious
{
    // Part of RulesView: the team's restart checkpoints (RestartCheckpoints; mockup
    // https://claude.ai/artifact/MBUpCQfCAAZRYPfhF8gkup). A purple line in the list where a
    // checkpoint is saved, a chip on its restart rule, four tiles under the list; a picked
    // checkpoint takes the detail column.
    public partial class RulesView
    {
        private int _cp = -1;
        private readonly int[] _cpSave = new int[RestartCheckpoints.Count];
        private readonly int[] _cpRestart = new int[RestartCheckpoints.Count];

        private static readonly SolidColorBrush CpBrush = Frozen(Color.FromRgb(0xB0, 0x6E, 0xF0));
        private static readonly SolidColorBrush CpSoftBrush = Frozen(Color.FromArgb(0x2E, 0xB0, 0x6E, 0xF0));

        private static SolidColorBrush Frozen(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        private void LoadCheckpoints()
        {
            bool on = RestartCheckpoints.Available;
            for (int cp = 0; cp < RestartCheckpoints.Count; cp++)
            {
                _cpSave[cp] = on ? RestartCheckpoints.SaveRule(_team, cp, _rules.Count) : -1;
                _cpRestart[cp] = on ? RestartCheckpoints.RestartRule(_team, cp) : 0;
            }
        }

        private void PickCheckpoint(int cp)
        {
            _cp = cp;
            _shownKey = null;
            Reload(true);
        }

        // ----- in the list -----

        /// <summary>The rule row, with a line above it for every checkpoint this rule saves.</summary>
        private FrameworkElement WithCheckpointLines(FrameworkElement row, int rule)
        {
            var saved = Enumerable.Range(0, RestartCheckpoints.Count).Where(cp => _cpSave[cp] == rule).ToList();
            if (saved.Count == 0)
                return row;
            var stack = new StackPanel();
            foreach (int cp in saved)
                stack.Children.Add(CheckpointLine(cp));
            stack.Children.Add(row);
            return stack;
        }

        private FrameworkElement CheckpointLine(int cp)
        {
            bool picked = cp == _cp;
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 6), Cursor = Cursors.Hand, Background = Brushes.Transparent,
                ToolTip = T("rcp_line_tip", "The team saves this checkpoint when it reaches the rule below. Click to edit it.") };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int c = 0; c < 3; c += 2)
            {
                var line = new Border { Height = picked ? 2 : 1, Background = CpBrush, Opacity = picked ? 1 : 0.5, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(line, c);
                grid.Children.Add(line);
            }
            var text = new TextBlock { Text = "⚑ " + string.Format(CultureInfo.CurrentCulture, T("rcp_line", "Checkpoint {0} · saved from here"), cp + 1),
                FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = CpBrush, Margin = new Thickness(8, 0, 8, 0) };
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
            grid.MouseLeftButtonUp += (_, e) => { e.Handled = true; PickCheckpoint(cp); };
            return grid;
        }

        /// <summary>"Restart CP n" chips for the checkpoints that go on at this rule.</summary>
        private void AddRestartChips(Panel chips, int rule)
        {
            for (int cp = 0; cp < RestartCheckpoints.Count; cp++)
            {
                if (_cpSave[cp] < 0 || _cpRestart[cp] != rule)
                    continue;
                var t = new TextBlock { Text = "↻ " + string.Format(CultureInfo.CurrentCulture, T("rcp_chip", "Restart CP {0}"), cp + 1), FontSize = 11.5, FontWeight = FontWeights.Bold, Foreground = CpBrush };
                chips.Children.Add(new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 1, 7, 2), Margin = new Thickness(0, 0, 5, 5), BorderThickness = new Thickness(1),
                    BorderBrush = CpBrush, Background = CpSoftBrush, Child = t,
                    ToolTip = T("rcp_chip_tip", "After a restart from this checkpoint the team goes on at this rule.") });
            }
        }

        /// <summary>The four checkpoints as tiles, under the rules.</summary>
        private FrameworkElement CheckpointTiles()
        {
            var stack = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            var head = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
            int set = _cpSave.Count(r => r >= 0);
            var count = new TextBlock { Text = string.Format(CultureInfo.CurrentCulture, T("rcp_count", "{0} of {1} set"), set, RestartCheckpoints.Count), FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
            count.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            DockPanel.SetDock(count, Dock.Right);
            head.Children.Add(count);
            var title = new TextBlock { Text = "⚑ " + T("rcp_title", "Restart checkpoints"), FontSize = 14, FontWeight = FontWeights.Bold };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            head.Children.Add(title);
            stack.Children.Add(head);
            stack.Children.Add(Faint(T("rcp_hint", "When the team fails the job online, it can restart from the last checkpoint it reached instead of from the start: it spawns at the checkpoint's points and goes on at its restart rule."), 12.5));

            var tiles = new UniformGrid { Columns = RestartCheckpoints.Count, Margin = new Thickness(-4, 6, -4, 0) };
            for (int cp = 0; cp < RestartCheckpoints.Count; cp++)
                tiles.Children.Add(CheckpointTile(cp));
            stack.Children.Add(tiles);
            if (RestartCheckpoints.RolesAvailable)
                stack.Children.Add(RolesRow());
            return stack;
        }

        // Minimum players per role (trcmn); greyed out while the job's Roles option is off.
        private FrameworkElement RolesRow()
        {
            int team = _team;
            bool on = RestartCheckpoints.RolesOn;
            var panel = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
            var head = new DockPanel();
            var toggle = new CheckBox { IsChecked = on, VerticalAlignment = VerticalAlignment.Center, ToolTip = T("rcp_roles_tip", "Job option \"Roles\" (menubs3 bit 6): players pick a role in the lobby.") };
            toggle.SetResourceReference(StyleProperty, "FormToggle");
            toggle.Click += (_, __) => { if (Live) { RestartCheckpoints.SetRolesOn(toggle.IsChecked == true); Reread(); } };
            DockPanel.SetDock(toggle, Dock.Right);
            head.Children.Add(toggle);
            var title = new TextBlock { Text = T("rcp_roles", "Minimum players per role"), FontSize = 13.5, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            head.Children.Add(title);
            panel.Children.Add(head);
            panel.Children.Add(Faint(T("rcp_roles_hint", "With the job option Roles on, a team with fewer players in a role than set here is out. 0 = no limit. Classic controller only."), 12));
            var boxes = new UniformGrid { Columns = RestartCheckpoints.Roles, Margin = new Thickness(-3, 6, -3, 0), Opacity = on ? 1 : 0.5 };
            for (int r = 0; r < RestartCheckpoints.Roles; r++)
            {
                int role = r;
                var cell = new StackPanel { Margin = new Thickness(3, 0, 3, 0) };
                var label = new TextBlock { Text = string.Format(CultureInfo.CurrentCulture, T("rcp_role_n", "Role {0}"), r + 1), FontSize = 11.5, Margin = new Thickness(0, 0, 0, 3) };
                label.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                cell.Children.Add(label);
                var box = NumberBox(RestartCheckpoints.MinPlayers(team, role), null, v => RestartCheckpoints.SetMinPlayers(team, role, v));
                cell.Children.Add(box);
                boxes.Children.Add(cell);
            }
            panel.Children.Add(boxes);
            return panel;
        }

        private FrameworkElement CheckpointTile(int cp)
        {
            bool on = _cpSave[cp] >= 0, picked = cp == _cp;
            var tile = new Border { CornerRadius = new CornerRadius(7), BorderThickness = new Thickness(picked ? 2 : 1), Padding = new Thickness(picked ? 9 : 10), Margin = new Thickness(4), Cursor = Cursors.Hand };
            tile.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            if (picked) tile.BorderBrush = CpBrush;
            else tile.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            var stack = new StackPanel();
            var name = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 13 };
            name.Inlines.Add(new System.Windows.Documents.Run("⚑ ") { Foreground = on ? CpBrush : (Brush)FindResource("FaintTextBrush") });
            name.Inlines.Add(new System.Windows.Documents.Run("CP " + (cp + 1)));
            if (RestartCheckpoints.TestCheckpoint() == cp)
                name.Inlines.Add(new System.Windows.Documents.Run("  ▶") { Foreground = (Brush)FindResource("OkBrush"), ToolTip = T("rcp_test_chip", "next test") });
            name.SetResourceReference(TextBlock.ForegroundProperty, on ? "TextColor" : "MutedTextBrush");
            stack.Children.Add(name);
            void Line(string text)
            {
                var t = new TextBlock { Text = text, FontSize = 11.5, Margin = new Thickness(0, 3, 0, 0), TextWrapping = TextWrapping.Wrap };
                t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                stack.Children.Add(t);
            }
            if (on)
            {
                Line(string.Format(CultureInfo.CurrentCulture, T("rcp_tile_save", "from rule {0}"), _cpSave[cp] + 1));
                Line(string.Format(CultureInfo.CurrentCulture, T("rcp_tile_restart", "restart at rule {0}"), _cpRestart[cp] + 1));
            }
            else
            {
                Line(T("rcp_tile_off", "not set"));
            }
            var dots = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 7, 0, 0) };
            for (int s = 0; s < RestartCheckpoints.Slots; s++)
            {
                var dot = new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 3, 0) };
                if (RestartCheckpoints.Slot(_team, cp, s) != null) dot.Background = CpBrush;
                else dot.SetResourceReference(Border.BackgroundProperty, "LineBrush");
                dots.Children.Add(dot);
            }
            stack.Children.Add(dots);
            tile.Child = stack;
            tile.ToolTip = T("rcp_tile_tip", "Dots: spawn points set for the restart. Click to edit.");
            tile.MouseLeftButtonUp += (_, __) => PickCheckpoint(cp);
            return tile;
        }

        // ----- the detail column -----

        private void ShowCheckpointDetail()
        {
            int cp = _cp, team = _team;
            _detailTitle.Text = string.Format(CultureInfo.CurrentCulture, T("rcp_detail", "Restart checkpoint {0}"), cp + 1);
            _detailTeam.Text = T("dash_team", "Team") + " " + (team + 1);
            if (_deleteButton != null)
                _deleteButton.Visibility = Visibility.Collapsed;

            var back = new Button { Content = "‹ " + string.Format(CultureInfo.CurrentCulture, T("rcp_back", "Back to rule {0}"), _selected + 1), Height = 26, Padding = new Thickness(10, 0, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 10) };
            back.SetResourceReference(StyleProperty, "FormButton");
            back.Click += (_, __) => PickCheckpoint(-1);
            _detail.Children.Add(back);

            int save = _cpSave[cp], restart = _cpRestart[cp], rules = _rules.Count;
            var rulesList = Enumerable.Range(0, rules).Select(r => (r, string.Format(CultureInfo.CurrentCulture, T("rl_rule_n", "Rule {0}"), r + 1))).ToList();

            // When it is saved and where the team goes on.
            var when = new StackPanel();
            when.Children.Add(Say(save < 0
                ? T("rcp_flow_off", "Off: after a fail the team can only restart from the start.")
                : string.Format(CultureInfo.CurrentCulture, T("rcp_flow", "The team reaches rule {0}: checkpoint {1} is saved. It fails, picks \"restart from checkpoint\", spawns at the points below and goes on at rule {2}."), save + 1, cp + 1, restart + 1)));
            OptionRow(when, T("rcp_save", "Saved when the team reaches"),
                Combo(new[] { (-1, T("rcp_off", "off")) }.Concat(rulesList), save, v =>
                {
                    RestartCheckpoints.SetSaveRule(team, cp, v, rules);
                    // A new checkpoint goes on where it is saved, unless a restart rule is set already.
                    if (v >= 0 && save < 0 && RestartCheckpoints.RestartRule(team, cp) == 0)
                        RestartCheckpoints.SetRestartRule(team, cp, v);
                }),
                null, string.Format(CultureInfo.CurrentCulture, T("rcp_save_tip", "Rule option {0}; only one rule of the team has it."), RestartCheckpoints.BitName(cp)));
            OptionRow(when, T("rcp_restart", "Restart at rule"),
                Combo(rulesList, restart, v => RestartCheckpoints.SetRestartRule(team, cp, v)),
                null, string.Format(CultureInfo.CurrentCulture, T("rcp_restart_tip", "trsrl{0}{1}: the rules before it count as done."), team, cp));
            _detail.Children.Add(Box("⚑", T("rcp_b_when", "When it is saved"), null, when));

            _detail.Children.Add(Box("⌖", T("rcp_b_spawn", "Spawn points after the restart"),
                Faint(string.Format(CultureInfo.CurrentCulture, "trstp{0}{1}n0–3", team, cp), 11.5), SpawnBody(team, cp)));

            var time = new StackPanel();
            OptionRow(time, T("rcp_time", "Least time left after the restart"),
                NumberBox(RestartCheckpoints.MinTimeMs(team, cp) / 1000, "s", v => RestartCheckpoints.SetMinTimeMs(team, cp, v * 1000)),
                null, string.Format(CultureInfo.CurrentCulture, T("rcp_time_tip", "{0}cmrt{1}: with less time left the team gets this much. 0 = as it is."), cp + 1, team));
            _detail.Children.Add(Box("⏱", T("rcp_b_time", "Time"), null, time));

            if (RestartCheckpoints.TestAvailable)
            {
                var test = new StackPanel();
                bool armed = RestartCheckpoints.TestCheckpoint() == cp;
                test.Children.Add(Say(armed
                    ? T("rcp_test_armed", "The next creator test starts at this checkpoint: the rules before it are done and the team spawns at its points. It is used up once the test ends.")
                    : T("rcp_test_hint", "Start the next creator test as after a restart from this checkpoint.")));
                var row = new WrapPanel();
                row.Children.Add(TextButton(armed ? T("rcp_test_off", "Start from the beginning again") : T("rcp_test_on", "Next test starts here"), null, () =>
                {
                    RestartCheckpoints.SetTestCheckpoint(armed ? -1 : cp);
                    Reread();
                }));
                test.Children.Add(row);
                _detail.Children.Add(Box("▶", T("rcp_b_test", "Test from this checkpoint"),
                    armed ? EntityRulesCard.SummaryChip(T("rcp_test_chip", "next test"), "ok") : null, test));
            }

            _detail.Children.Add(Faint(T("rcp_note2", "A real job offers the restart after a fail; the creator test ends there. The creator saves these values with the job."), 12));
        }

        private FrameworkElement SpawnBody(int team, int cp)
        {
            var stack = new StackPanel();
            var map = new JobMap { Height = 170, MinSpan = 20, Margin = new Thickness(0, 0, 0, 10) };
            void DrawMap()
            {
                var markers = Enumerable.Range(0, RestartCheckpoints.Slots).Select(s => RestartCheckpoints.Slot(team, cp, s))
                    .Where(s => s != null).Select(s => new JobMap.Marker(s.Value.Pos.X, s.Value.Pos.Y, CpBrush, 7)).ToList();
                map.SetMarkers(markers);
                map.Visibility = markers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            var head = SlotGrid();
            foreach (var (text, col) in new[] { ("X", 1), ("Y", 2), ("Z", 3), (T("rcp_heading", "Heading"), 4) })
            {
                var t = new TextBlock { Text = text, FontSize = 11.5, Margin = new Thickness(4, 0, 0, 3) };
                t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                Grid.SetColumn(t, col);
                head.Children.Add(t);
            }
            stack.Children.Add(head);
            for (int s = 0; s < RestartCheckpoints.Slots; s++)
                stack.Children.Add(SlotRow(team, cp, s, DrawMap));

            var buttons = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
            buttons.Children.Add(TextButton(T("rcp_ring", "4 points around me"), T("rcp_ring_tip", "Four points 2.5 m around your position, facing where you look."), () =>
            {
                var me = PlayerPosition();
                if (me == null) return;
                float heading = CameraHeading();
                for (int s = 0; s < RestartCheckpoints.Slots; s++)
                {
                    double a = s * Math.PI / 2;
                    RestartCheckpoints.SetSlot(team, cp, s, new Vector3(me.Value.X + (float)(Math.Cos(a) * 2.5), me.Value.Y + (float)(Math.Sin(a) * 2.5), me.Value.Z), heading);
                }
                SpawnsChanged(team, cp);
            }));
            buttons.Children.Add(TextButton(T("rcp_copy_start", "Copy team start points"), T("rcp_copy_start_tip", "The team's first four start points with their headings."), () =>
            {
                if (!StartPoints.Ready) return;
                var starts = Enumerable.Range(0, StartPoints.Count(team)).Where(i => StartPoints.Bit(team, i, StartPoints.BitStart)).Take(RestartCheckpoints.Slots).ToList();
                if (starts.Count == 0) return;
                for (int s = 0; s < RestartCheckpoints.Slots; s++)
                {
                    if (s < starts.Count)
                        RestartCheckpoints.SetSlot(team, cp, s, new Global(StartPoints.Field(GTA.Offsets.Editor.player_loc, team, starts[s])).GetVector3(),
                            new Global(StartPoints.Field(GTA.Offsets.Editor.player_head, team, starts[s])).Get<float>());
                    else
                        RestartCheckpoints.ClearSlot(team, cp, s);
                }
                SpawnsChanged(team, cp);
            }));
            buttons.Children.Add(TextButton(T("rcp_clear", "Clear"), T("rcp_clear_tip", "Removes all four spawn points of this checkpoint."), () =>
            {
                for (int s = 0; s < RestartCheckpoints.Slots; s++)
                    RestartCheckpoints.ClearSlot(team, cp, s);
                SpawnsChanged(team, cp);
            }));
            stack.Children.Add(buttons);
            if (RestartCheckpoints.SlotCount(team, cp) == 0)
                stack.Children.Add(Faint(T("rcp_no_spawns", "No spawn points: the team would restart where the job places it by default."), 12).Also(f => f.Margin = new Thickness(0, 6, 0, 0)));
            stack.Children.Insert(0, map);
            DrawMap();
            return stack;
        }

        private static Grid SlotGrid()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            for (int i = 0; i < 3; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            return grid;
        }

        private FrameworkElement SlotRow(int team, int cp, int slot, Action drawMap)
        {
            var grid = SlotGrid();
            grid.Margin = new Thickness(0, 0, 0, 5);
            var n = new TextBlock { Text = (slot + 1).ToString(CultureInfo.CurrentCulture), FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };
            n.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            grid.Children.Add(n);

            var now = RestartCheckpoints.Slot(team, cp, slot);
            float[] values = now == null ? new float[4] : new[] { now.Value.Pos.X, now.Value.Pos.Y, now.Value.Pos.Z, now.Value.Heading };
            var boxes = new TextBox[4];
            for (int i = 0; i < 4; i++)
            {
                var box = new TextBox { Height = 28, Margin = new Thickness(4, 0, 0, 0), Tag = "–",
                    Text = now == null ? "" : values[i].ToString(i == 3 ? "0.#" : "0.###", CultureInfo.InvariantCulture) };
                box.SetResourceReference(StyleProperty, "Watermark");
                Grid.SetColumn(box, i + 1);
                grid.Children.Add(box);
                boxes[i] = box;
            }
            void Commit()
            {
                if (_loading || !Live)
                    return;
                var v = new float[4];
                for (int i = 0; i < 4; i++)
                {
                    string text = boxes[i].Text.Trim().Replace(',', '.');
                    if (text.Length == 0 && i == 3) v[i] = 0;
                    else if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out v[i]))
                        return;
                }
                RestartCheckpoints.SetSlot(team, cp, slot, new Vector3(v[0], v[1], v[2]), v[3]);
                _shownKey = null;
                drawMap();
            }
            foreach (var box in boxes)
            {
                box.LostKeyboardFocus += (_, __) => Commit();
                box.KeyDown += (_, e) => { if (e.Key == Key.Enter) Commit(); };
            }

            var acts = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 0, 0, 0) };
            acts.Children.Add(IconButton("M6,1 A2.2,2.2 0 1 1 5.99,1 M1.5,11 C1.5,7 10.5,7 10.5,11", T("rcp_me", "My position and the direction I look"), () =>
            {
                var me = PlayerPosition();
                if (me == null) return;
                RestartCheckpoints.SetSlot(team, cp, slot, me.Value, CameraHeading());
                SpawnsChanged(team, cp);
            }));
            acts.Children.Add(IconButton("M6,0.5 V3 M6,9 V11.5 M0.5,6 H3 M9,6 H11.5 M6,3.5 A2.5,2.5 0 1 1 5.99,3.5", T("rcp_cursor", "Cursor position"), () =>
            {
                var at = CursorPosition();
                if (at == null) return;
                RestartCheckpoints.SetSlot(team, cp, slot, at.Value, RestartCheckpoints.Slot(team, cp, slot)?.Heading ?? CameraHeading());
                SpawnsChanged(team, cp);
            }));
            acts.Children.Add(IconButton("M2,10 L10,2 M5,2 H10 V7", T("rcp_go", "Teleport there"), () =>
            {
                var at = RestartCheckpoints.Slot(team, cp, slot);
                if (at != null)
                    GTA.Teleport(new XenVector3(at.Value.Pos.X, at.Value.Pos.Y, at.Value.Pos.Z + 1));
            }));
            acts.Children.Add(IconButton("M2,2 L10,10 M10,2 L2,10", T("rcp_clear_one", "Remove this point"), () =>
            {
                RestartCheckpoints.ClearSlot(team, cp, slot);
                SpawnsChanged(team, cp);
            }));
            Grid.SetColumn(acts, 5);
            grid.Children.Add(acts);
            return grid;
        }

        private void SpawnsChanged(int team, int cp)
        {
            // A checkpoint with spawn points but no rule saves nothing; the first set point turns it on at its own number.
            if (_cpSave[cp] < 0 && _rules.Count > 0 && RestartCheckpoints.SlotCount(team, cp) > 0)
            {
                int rule = Math.Min(_selected, _rules.Count - 1);
                RestartCheckpoints.SetSaveRule(team, cp, rule, _rules.Count);
                if (RestartCheckpoints.RestartRule(team, cp) == 0)
                    RestartCheckpoints.SetRestartRule(team, cp, rule);
            }
            Reread();
        }

        private Button TextButton(string text, string tip, Action click)
        {
            var b = new Button { Content = text, Height = 28, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 6), ToolTip = tip };
            b.SetResourceReference(StyleProperty, "FormButton");
            b.Click += (_, __) => { if (Live) click(); };
            return b;
        }

        private static Vector3? PlayerPosition()
        {
            try
            {
                long at = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_player_loc).GetAddress();
                var v = new Vector3(MainWindow.m.memory(at.ToString("X")).Get<float>(), MainWindow.m.memory((at + 4).ToString("X")).Get<float>(), MainWindow.m.memory((at + 8).ToString("X")).Get<float>());
                return v == Vector3.Zero ? (Vector3?)null : v;
            }
            catch
            {
                return null;
            }
        }

        private static Vector3? CursorPosition()
        {
            try
            {
                var v = Functions.Read.GetLocationVec();
                return v == Vector3.Zero ? (Vector3?)null : v;
            }
            catch
            {
                return null;
            }
        }

        private static float CameraHeading()
        {
            try { return Functions.Read.GetHeading(); }
            catch { return 0; }
        }
    }
}
