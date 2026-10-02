using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of MainWindow: the Vehicles page layout (rules card, lifecycle strip, rule pickers, card summaries).
    public partial class MainWindow
    {
        private bool _vehPickSync;

        private void InitVehicleLayout()
        {
            VehModelCard.IsHero = true;
            VehModelCard.HeaderIcon = Geometry.Parse("M5,17 L19,17 M3,13 L5,7 L19,7 L21,13 L21,17 L3,17 Z M5.7,17 A1.8,1.8 0 1 1 9.3,17 A1.8,1.8 0 1 1 5.7,17 M14.7,17 A1.8,1.8 0 1 1 18.3,17 A1.8,1.8 0 1 1 14.7,17");
            VehAdvancedCard.Summary = TranslateOr("card_adv_sum", "raw values");

            // One team for the rules card and the per-team spawn fields.
            VehExtraRules.TeamChanged += (_, __) =>
            {
                if (ddvehteamrlprio.SelectedIndex != VehExtraRules.Team)
                    ddvehteamrlprio.SelectedIndex = VehExtraRules.Team;
            };
            ddvehteamrlprio.SelectionChanged += (_, __) => { if (ddvehteamrlprio.SelectedIndex >= 0) VehExtraRules.SetTeam(ddvehteamrlprio.SelectedIndex); };
            VehExtraRules.Refreshed += (_, __) => UpdateVehicleMission();

            // Rule pickers stand in for the raw spawn and clean-up rule boxes, which stay hidden
            // because the page code reads and writes them.
            VehSpawnRulePick.SelectionChanged += (_, __) => PickToBox(VehSpawnRulePick, tbvehspawnrule);
            VehClearRulePick.SelectionChanged += (_, __) => PickToBox(VehClearRulePick, tbvehclearrule);
            foreach (var box in new Control[] { tbvehspawnrule, tbvehclearrule, ddvehspawnon, ddvehspawnteam, ddvehteamclear, ddvehrsp })
            {
                if (box is TextBox t) t.TextChanged += (_, __) => QueueVehicleMission();
                if (box is ComboBox c) c.SelectionChanged += (_, __) => QueueVehicleMission();
            }

            // Mission Creator despawn trigger: -1 none, 1 at a rule of a team, 2 points, 3 team, 4 prerequisite
            // (func_2537 lists these; func_2536 sets the defaults when the type changes).
            foreach (var (value, key, fallback) in new[] { (-1, "lc_never", "never"), (1, "lc_trg_rule", "at a rule"), (2, "lc_trg_points", "points"),
                (3, "lc_trg_team", "team"), (4, "lc_trg_prereq", "prerequisite") })
                VehDespawnType.Items.Add(new ComboBoxItem { Content = TranslateOr(key, fallback), Tag = value });
            for (int t = 0; t < 4; t++)
                VehDespawnTeam.Items.Add(new ComboBoxItem { Content = TranslateOr("dash_team", "Team") + " " + (t + 1), Tag = t });
            VehDespawnType.SelectionChanged += (_, __) =>
            {
                if (_vehPickSync || !(VehDespawnType.SelectedItem is ComboBoxItem item)) return;
                int type = (int)item.Tag;
                if (DespawnField(0) == type) return;
                SetDespawnField(4, 0);
                SetDespawnField(3, -1);
                SetDespawnField(2, type == 1 ? 0 : -1);
                SetDespawnField(1, type == 1 ? 0 : -1);
                SetDespawnField(0, type);
                QueueVehicleMission();
            };
            VehDespawnTeam.SelectionChanged += (_, __) => { if (!_vehPickSync && VehDespawnTeam.SelectedItem is ComboBoxItem i) { SetDespawnField(2, (int)i.Tag); QueueVehicleMission(); } };
            VehDespawnFrom.SelectionChanged += (_, __) => { if (!_vehPickSync && VehDespawnFrom.SelectedItem is ComboBoxItem i) { SetDespawnField(1, (int)i.Tag); QueueVehicleMission(); } };
            VehDespawnTo.SelectionChanged += (_, __) => { if (!_vehPickSync && VehDespawnTo.SelectedItem is ComboBoxItem i) { SetDespawnField(3, (int)i.Tag); QueueVehicleMission(); } };
            VehDespawnValue.TextChanged += (_, __) =>
            {
                if (!_vehPickSync && int.TryParse(VehDespawnValue.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                    SetDespawnField(1, v);
            };
            VehDespawnMid.Checked += (_, __) => { if (!_vehPickSync) SetDespawnField(4, DespawnField(4) | 1); };
            VehDespawnMid.Unchecked += (_, __) => { if (!_vehPickSync) SetDespawnField(4, DespawnField(4) & ~1); };

            foreach (var cb in VehFlags().SelectMany(g => g))
            {
                cb.Checked += (_, __) => QueueVehicleSummaries();
                cb.Unchecked += (_, __) => QueueVehicleSummaries();
            }
            foreach (var box in new[] { ddvehcol1, ddvehcol2, ddvehicon })
                box.SelectionChanged += (_, __) => QueueVehicleSummaries();
            tbvehhlth.TextChanged += (_, __) => QueueVehicleSummaries();

            // Icon size: slider and box stay in step; the box writes the game.
            bool iconSync = false;
            VehIconSizeSlider.ValueChanged += (_, __) =>
            {
                if (iconSync) return;
                iconSync = true;
                tbvehiconsize.Text = Math.Round(VehIconSizeSlider.Value, 1).ToString(CultureInfo.CurrentCulture);
                iconSync = false;
            };
            tbvehiconsize.TextChanged += (_, __) =>
            {
                if (iconSync || !float.TryParse(tbvehiconsize.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float v)) return;
                iconSync = true;
                VehIconSizeSlider.Value = Math.Max(VehIconSizeSlider.Minimum, Math.Min(VehIconSizeSlider.Maximum, v));
                iconSync = false;
            };

            foreach (var box in new[] { tbvehlocx, tbvehlocy })
                box.TextChanged += (_, __) => QueueVehicleMap();
            ddvehno.SelectionChanged += (_, __) => { QueueVehicleSummaries(); QueueVehicleMission(); QueueVehicleMap(); };
        }

        private CheckBox[][] VehFlags() => new[]
        {
            new[] { cb_veh_neon, cb_veh_remove_windows, cb_veh_lights, cb_veh_sirens, cb_veh_sirens_audio, cb_veh_box, cb_veh_blipoff },
            new[] { cb_veh_godmode, cb_veh_bptires, cb_veh_engine, cb_veh_explodeinwater, cb_veh_nottargetable, cb_veh_mark },
            new[] { cb_veh_lockteam1, cb_veh_lockteam2, cb_veh_lockteam3, cb_veh_lockteam4, cb_veh_locveh, cb_veh_locvehforp, cb_veh_freeze },
            new[] { cb_veh_door_open_fl, cb_veh_door_close_fl, cb_veh_door_open_fr, cb_veh_door_close_fr, cb_veh_door_open_rl, cb_veh_door_close_rl,
                cb_veh_door_open_rr, cb_veh_door_close_rr, cb_veh_door_open_hood, cb_veh_door_close_hood, cb_veh_door_open_trunk, cb_veh_door_close_trunk },
        };

        private bool _vehSummaryQueued, _vehMissionQueued, _vehMapQueued;

        private void QueueVehicleMap()
        {
            if (_vehMapQueued) return;
            _vehMapQueued = true;
            Dispatcher.BeginInvoke(new Action(() => { _vehMapQueued = false; UpdateVehicleMap(); }), DispatcherPriority.Background);
        }

        /// <summary>All vehicles of the job on the map, the selected one larger in the accent colour.</summary>
        private void UpdateVehicleMap()
        {
            var markers = new List<JobMap.Marker>();
            if (m.IsProcOpen && GTA.Offsets.Editor.Vehicle.loc != 0)
            {
                int count = ddvehno.Items.Count, selected = ddvehno.SelectedIndex;
                var faint = ThemeBrush("FaintTextBrush");
                for (int i = 0; i < count && i < 200; i++)
                {
                    if (i == selected) continue;
                    long at = GTA.Offsets.Editor.Vehicle.loc + GTA.Offsets.Editor.Vehicle.NEXT * i;
                    markers.Add(new JobMap.Marker(new Global(at).Get<float>(), new Global(at + 1).Get<float>(), faint, 5));
                }
                if (selected >= 0 && float.TryParse(tbvehlocx.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float x)
                    && float.TryParse(tbvehlocy.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out float y))
                    markers.Add(new JobMap.Marker(x, y, ThemeBrush("AccentBrush"), 11));
            }
            VehMap.SetMarkers(markers);
        }

        private void QueueVehicleSummaries()
        {
            if (_vehSummaryQueued) return;
            _vehSummaryQueued = true;
            Dispatcher.BeginInvoke(new Action(() => { _vehSummaryQueued = false; UpdateVehicleSummaries(); }), DispatcherPriority.Background);
        }

        private void QueueVehicleMission()
        {
            if (_vehPickSync || _vehMissionQueued) return;
            _vehMissionQueued = true;
            Dispatcher.BeginInvoke(new Action(() => { _vehMissionQueued = false; UpdateVehicleMission(); }), DispatcherPriority.Background);
        }

        private static string FlagName(CheckBox cb) => cb.Content?.ToString() ?? "";

        /// <summary>The short line on the right of every folding card, so it can stay closed.</summary>
        private void UpdateVehicleSummaries()
        {
            var flags = VehFlags();
            string On(IEnumerable<CheckBox> list) => string.Join(", ", list.Where(c => c.IsChecked == true).Select(FlagName));

            string look = ddvehcol1.Text + " / " + ddvehcol2.Text;
            if (ddvehicon.SelectedIndex > 0) look += " · " + TranslateOr("icon", "Icon") + " " + ((ddvehicon.SelectedItem as ComboBoxItem)?.Content ?? "");
            string lookFlags = On(flags[0]);
            VehLookCard.Summary = lookFlags.Length > 0 ? look + " · " + lookFlags : look;

            string state = On(flags[1]);
            VehStateCard.Summary = tbvehhlth.Text + " HP · " + (state.Length > 0 ? state : TranslateOr("card_state_normal", "all normal"));

            var locked = new[] { cb_veh_lockteam1, cb_veh_lockteam2, cb_veh_lockteam3, cb_veh_lockteam4 }
                .Select((c, i) => (c, i)).Where(p => p.c.IsChecked == true).Select(p => (p.i + 1).ToString(CultureInfo.CurrentCulture)).ToList();
            string locks = locked.Count == 0 ? TranslateOr("card_locks_open", "open for all")
                : string.Format(CultureInfo.CurrentCulture, TranslateOr("card_locks_teams", "locked for team {0}"), string.Join(", ", locked));
            string lockFlags = On(new[] { cb_veh_locveh, cb_veh_locvehforp, cb_veh_freeze });
            VehLocksCard.Summary = lockFlags.Length > 0 ? locks + " · " + lockFlags : locks;

            int doors = flags[3].Count(c => c.IsChecked == true);
            VehDoorsCard.Summary = doors == 0 ? TranslateOr("card_doors_default", "default")
                : string.Format(CultureInfo.CurrentCulture, TranslateOr("card_doors_set", "{0} set"), doors);
        }

        private bool VehUsesDespawnTrigger => Rules.PublicCreator && GTA.Offsets.Editor.Vehicle.dspwn != 0 && m.IsProcOpen && ddvehno.SelectedIndex >= 0;

        private long DespawnAddr(int field) => GTA.Offsets.Editor.Vehicle.dspwn + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex + field;
        private int DespawnField(int field) => VehUsesDespawnTrigger ? new Global(DespawnAddr(field)).Get<int>() : -1;
        private void SetDespawnField(int field, int value) { if (VehUsesDespawnTrigger) new Global(DespawnAddr(field)).SetInt(value); }

        private static UIElement LegendItem(string text, Func<UIElement> marker)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 12, 2) };
            row.Children.Add(marker());
            var t = new TextBlock { Text = text, FontSize = 11.5, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            row.Children.Add(t);
            return row;
        }

        private static UIElement Swatch(string brush)
        {
            var grid = new Grid { Width = 12, Height = 12, VerticalAlignment = VerticalAlignment.Center };
            grid.Children.Add(new Border { CornerRadius = new CornerRadius(3), Opacity = 0.16 }.Also(b => b.SetResourceReference(Border.BackgroundProperty, brush)));
            grid.Children.Add(new Border { CornerRadius = new CornerRadius(3), BorderThickness = new Thickness(1) }.Also(b => b.SetResourceReference(Border.BorderBrushProperty, brush)));
            return grid;
        }

        private static UIElement Dot(bool dashed)
        {
            var e = new System.Windows.Shapes.Ellipse { Width = 10, Height = 10, VerticalAlignment = VerticalAlignment.Center };
            if (dashed)
            {
                e.StrokeThickness = 2;
                e.StrokeDashArray = new DoubleCollection { 1.5, 1 };
                e.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty, "AccentBrush");
            }
            else
                e.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "AccentBrush");
            return e;
        }

        private void BuildStripLegend(int team)
        {
            VehStripLegend.Children.Clear();
            var head = new TextBlock { Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_legend_team", "Rules of team {0}"), team + 1),
                FontSize = 11.5, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 12, 2), VerticalAlignment = VerticalAlignment.Center };
            head.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            VehStripLegend.Children.Add(head);
            VehStripLegend.Children.Add(LegendItem(TranslateOr("lc_lg_there", "there"), () => Swatch("OkBrush")));
            VehStripLegend.Children.Add(LegendItem(TranslateOr("lc_lg_gone", "not there"), () => Swatch("BadBrush")));
            VehStripLegend.Children.Add(LegendItem(TranslateOr("er_main", "Own rule"), () => Dot(false)));
            VehStripLegend.Children.Add(LegendItem(TranslateOr("er_extra", "Extra objective"), () => Dot(true)));
        }

        private static int ParseRule(string text, int fallback)
            => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;

        /// <summary>Rules card, lifecycle strip, rule pickers and the model card chips, from the current values.</summary>
        private void UpdateVehicleMission()
        {
            int team = VehExtraRules.Team;
            bool ready = Rules.Ready && ddvehno.SelectedIndex >= 0;
            int count = ready ? VehExtraRules.RuleCount : 0;

            int spawnOn = ddvehspawnon.SelectedIndex;
            int spawnTeam = ddvehspawnteam.SelectedIndex - 1;
            int spawnRule = ParseRule(tbvehspawnrule.Text, 0);
            int clearTeam = ddvehteamclear.SelectedIndex - 1;
            int clearRule = ParseRule(tbvehclearrule.Text, -1);

            FillRulePicker(VehSpawnRulePick, spawnTeam >= 0 ? spawnTeam : team, spawnRule, false);
            FillRulePicker(VehClearRulePick, clearTeam >= 0 ? clearTeam : team, clearRule, true);

            // The Mission Creator despawns vehicles through its own trigger, not team and rule.
            bool trigger = VehUsesDespawnTrigger;
            VehClearOld.Visibility = trigger ? Visibility.Collapsed : Visibility.Visible;
            VehClearTrigger.Visibility = trigger ? Visibility.Visible : Visibility.Collapsed;
            int dType = -1, dValue = -1, dTeam = -1, dEnd = -1, dBits = 0;
            if (trigger)
            {
                dType = DespawnField(0); dValue = DespawnField(1); dTeam = DespawnField(2); dEnd = DespawnField(3); dBits = DespawnField(4);
                if (!VehClearTrigger.IsKeyboardFocusWithin)
                {
                    _vehPickSync = true;
                    try
                    {
                        VehDespawnType.SelectedItem = VehDespawnType.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == dType);
                        VehDespawnTeam.SelectedItem = VehDespawnTeam.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == dTeam);
                        VehDespawnMid.IsChecked = (dBits & 1) != 0;
                        VehDespawnValue.Text = dValue.ToString(CultureInfo.InvariantCulture);
                    }
                    finally { _vehPickSync = false; }
                    FillRulePicker(VehDespawnFrom, Math.Max(0, dTeam), dValue, false);
                    FillRulePicker(VehDespawnTo, Math.Max(0, dTeam), dEnd, true, TranslateOr("lc_to_end", "to the end"));
                }
                VehDespawnTeamBox.Visibility = dType >= 1 && dType <= 3 ? Visibility.Visible : Visibility.Hidden;
                VehDespawnRules.Visibility = VehDespawnMidBox.Visibility = dType == 1 ? Visibility.Visible : Visibility.Collapsed;
                VehDespawnValueBox.Visibility = dType == 2 || dType == 4 ? Visibility.Visible : Visibility.Collapsed;
                VehDespawnValueLabel.Text = dType == 2 ? TranslateOr("lc_trg_points", "points") : TranslateOr("lc_trg_prereq", "prerequisite");
            }

            // Which rules of the shown team the vehicle exists in. Spawning after another team's
            // rule cannot be mapped onto this team's rules.
            bool unknown = spawnOn > 0 && spawnTeam >= 0 && spawnTeam != team;
            int from = spawnOn <= 0 ? 0 : spawnRule;
            int until = !trigger && clearTeam == team && clearRule >= 0 ? clearRule : count;
            int goneFrom = trigger && dType == 1 && dTeam == team && dValue >= 0 ? dValue : int.MaxValue;
            int goneTo = dEnd >= 0 ? dEnd : int.MaxValue;
            var states = new List<RuleStrip.State>();
            for (int r = 0; r < count; r++)
            {
                if (unknown) { states.Add(RuleStrip.State.Unknown); continue; }
                bool live = r >= from && r < until && (spawnOn != 3 || r == from) && !(r >= goneFrom && r <= goneTo);
                states.Add(live ? RuleStrip.State.Live : RuleStrip.State.Gone);
            }
            var extras = new HashSet<int>(VehExtraRules.Extras.Select(e => e.Rule));
            VehRuleStrip.Show(states, VehExtraRules.MainRule, extras, r => string.Format(CultureInfo.CurrentCulture,
                states[r] == RuleStrip.State.Live ? TranslateOr("lc_tip_live", "Rule {0}: the vehicle is there") :
                states[r] == RuleStrip.State.Gone ? TranslateOr("lc_tip_gone", "Rule {0}: the vehicle is not there") : TranslateOr("lc_tip_unknown", "Rule {0}: depends on another team"), r + 1));
            BuildStripLegend(team);

            VehSpawnText.Text = "  " + (spawnOn <= 0 ? TranslateOr("lc_at_start", "at mission start")
                : string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_with_rule", "with rule {0}"), spawnRule + 1)
                  + (spawnTeam >= 0 ? " · " + TranslateOr("dash_team", "Team") + " " + (spawnTeam + 1) : ""));
            VehRespawnText.Text = "  " + ((ddvehrsp.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");
            VehClearText.Text = "  " + (trigger
                ? (dType == 1 ? string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_with_rule", "with rule {0}"), dValue + 1) + " · " + TranslateOr("dash_team", "Team") + " " + (dTeam + 1)
                   : (VehDespawnType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? TranslateOr("lc_never", "never"))
                : clearRule < 0 || clearTeam < 0 ? TranslateOr("lc_never", "never")
                : string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_with_rule", "with rule {0}"), clearRule + 1) + " · " + TranslateOr("dash_team", "Team") + " " + (clearTeam + 1));

            string start = spawnOn <= 0 ? TranslateOr("lc_sum_start", "start") : "R" + (spawnRule + 1);
            string end = trigger ? (dType == 1 ? "R" + (dValue + 1) : dType < 0 ? TranslateOr("lc_sum_end", "end") : "…")
                : clearRule < 0 || clearTeam < 0 ? TranslateOr("lc_sum_end", "end") : "R" + (clearRule + 1);
            VehLifecycle.Summary = start + " → " + end;
            var goals = new[] { VehExtraRules.MainRule }.Concat(extras).Where(r => r >= 0 && r < states.Count && states[r] == RuleStrip.State.Gone).Select(r => r + 1).ToList();
            VehLifecycle.HeaderRight = goals.Count == 0 ? null : EntityRulesCard.SummaryChip(string.Format(CultureInfo.CurrentCulture,
                TranslateOr("lc_warn_goal", "objective without vehicle: R{0}"), string.Join(", R", goals)), "warn");

            var chips = new List<UIElement>();
            if (ready)
            {
                if (VehExtraRules.MainRule >= 0)
                    chips.Add(EntityRulesCard.SummaryChip("R" + (VehExtraRules.MainRule + 1) + " " + VehExtraRules.MainName, "main"));
                foreach (var e in VehExtraRules.Extras)
                    chips.Add(EntityRulesCard.SummaryChip("R" + (e.Rule + 1) + " " + e.Name, "extra"));
                chips.Add(EntityRulesCard.SummaryChip(spawnOn <= 0 ? TranslateOr("lc_chip_start", "from mission start")
                    : string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_chip_rule", "from rule {0}"), spawnRule + 1), "ok"));
                chips.Add(EntityRulesCard.SummaryChip(TranslateOr("dash_team", "Team") + " " + (team + 1), "plain"));
            }
            VehModelCard.SetInfo(chips);
        }

        /// <summary>
        /// Writes "spawn on" like the creator's menu: spawning with a rule needs a team and a rule,
        /// so empty ones (-1) get the shown team and rule 1 first; mission start clears the rule in
        /// the Mission Creator. Otherwise the creator sees a broken setting and resets the vehicle.
        /// Only runs when the value really changes (the page also sets the box while reading).
        /// </summary>
        private void SetVehicleSpawnOn(long spwn, int value)
        {
            long at = GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex;
            long team, objt;
            if (spwn == GTA.Offsets.Editor.Vehicle.spwn2) { team = GTA.Offsets.Editor.Vehicle.team2; objt = GTA.Offsets.Editor.Vehicle.objt2; }
            else if (spwn == GTA.Offsets.Editor.Vehicle.spwn3) { team = GTA.Offsets.Editor.Vehicle.team3; objt = GTA.Offsets.Editor.Vehicle.objt3; }
            else if (spwn == GTA.Offsets.Editor.Vehicle.spwn4) { team = GTA.Offsets.Editor.Vehicle.team4; objt = GTA.Offsets.Editor.Vehicle.objt4; }
            else { team = GTA.Offsets.Editor.Vehicle.team; objt = GTA.Offsets.Editor.Vehicle.objt; }
            if (spwn == 0 || new Global(spwn + at).Get<int>() == value)
                return;
            if (value > 0)
            {
                if (team != 0 && new Global(team + at).Get<int>() < 0)
                    new Global(team + at).SetInt(Math.Max(0, VehExtraRules.Team));
                if (objt != 0 && new Global(objt + at).Get<int>() < 0)
                    new Global(objt + at).SetInt(0);
            }
            else if (value == 0 && Rules.PublicCreator && objt != 0)
                new Global(objt + at).SetInt(-1);
            new Global(spwn + at).SetInt(value);
        }

        private static string PlainText(string text) => Regex.Replace(text ?? "", "~[A-Za-z_0-9]+~", "").Trim();

        /// <summary>"Rule N · objective text" entries; value -1 is "never" where allowed.</summary>
        private void FillRulePicker(ComboBox pick, int team, int value, bool allowNever, string neverText = null)
        {
            if (pick.IsDropDownOpen)
                return;
            _vehPickSync = true;
            try
            {
                pick.Items.Clear();
                if (allowNever)
                    pick.Items.Add(new ComboBoxItem { Content = neverText ?? TranslateOr("lc_never", "never"), Tag = -1 });
                int count = Rules.Ready ? Rules.Count(team) : 0;
                for (int r = 0; r < count; r++)
                {
                    string text = PlainText(Rules.Text(team, r));
                    if (text.Length > 26) text = text.Substring(0, 25) + "…";
                    pick.Items.Add(new ComboBoxItem { Content = TranslateOr("rl_rule_n", "Rule {0}").Replace("{0}", (r + 1).ToString(CultureInfo.CurrentCulture)) + (text.Length > 0 ? " · " + text : ""), Tag = r });
                }
                if ((value >= count || (value < 0 && !allowNever)) && !(allowNever && value < 0))
                    pick.Items.Add(new ComboBoxItem { Content = TranslateOr("rl_rule_n", "Rule {0}").Replace("{0}", (value + 1).ToString(CultureInfo.CurrentCulture)) + " " + TranslateOr("lc_missing", "(missing)"), Tag = value });
                pick.SelectedItem = pick.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == (allowNever && value < 0 ? -1 : value));
            }
            finally { _vehPickSync = false; }
        }

        private void PickToBox(ComboBox pick, TextBox box)
        {
            if (_vehPickSync || !(pick.SelectedItem is ComboBoxItem item))
                return;
            string text = ((int)item.Tag).ToString(CultureInfo.InvariantCulture);
            if (box.Text != text)
                box.Text = text;
        }
    }
}
