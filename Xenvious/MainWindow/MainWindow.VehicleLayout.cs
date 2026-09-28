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
            VehSpawnRulePick.DropDownOpened += (_, __) => UpdateVehicleMission();
            VehClearRulePick.DropDownOpened += (_, __) => UpdateVehicleMission();
            foreach (var box in new Control[] { tbvehspawnrule, tbvehclearrule, ddvehspawnon, ddvehspawnteam, ddvehteamclear, ddvehrsp })
            {
                if (box is TextBox t) t.TextChanged += (_, __) => QueueVehicleMission();
                if (box is ComboBox c) c.SelectionChanged += (_, __) => QueueVehicleMission();
            }

            foreach (var cb in VehFlags().SelectMany(g => g))
            {
                cb.Checked += (_, __) => QueueVehicleSummaries();
                cb.Unchecked += (_, __) => QueueVehicleSummaries();
            }
            foreach (var box in new[] { ddvehcol1, ddvehcol2, ddvehicon })
                box.SelectionChanged += (_, __) => QueueVehicleSummaries();
            tbvehhlth.TextChanged += (_, __) => QueueVehicleSummaries();
            ddvehno.SelectionChanged += (_, __) => { QueueVehicleSummaries(); QueueVehicleMission(); };
        }

        private CheckBox[][] VehFlags() => new[]
        {
            new[] { cb_veh_neon, cb_veh_remove_windows, cb_veh_lights, cb_veh_sirens, cb_veh_sirens_audio, cb_veh_box },
            new[] { cb_veh_godmode, cb_veh_bptires, cb_veh_engine, cb_veh_explodeinwater, cb_veh_nottargetable, cb_veh_mark },
            new[] { cb_veh_lockteam1, cb_veh_lockteam2, cb_veh_lockteam3, cb_veh_lockteam4, cb_veh_locveh, cb_veh_locvehforp, cb_veh_freeze },
            new[] { cb_veh_door_open_fl, cb_veh_door_close_fl, cb_veh_door_open_fr, cb_veh_door_close_fr, cb_veh_door_open_rl, cb_veh_door_close_rl,
                cb_veh_door_open_rr, cb_veh_door_close_rr, cb_veh_door_open_hood, cb_veh_door_close_hood, cb_veh_door_open_trunk, cb_veh_door_close_trunk },
        };

        private bool _vehSummaryQueued, _vehMissionQueued;

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

            // Which rules of the shown team the vehicle exists in. Spawning after another team's
            // rule cannot be mapped onto this team's rules.
            bool unknown = spawnOn > 0 && spawnTeam >= 0 && spawnTeam != team;
            int from = spawnOn <= 0 ? 0 : spawnRule;
            int until = clearTeam == team && clearRule >= 0 ? clearRule : count;
            var states = new List<RuleStrip.State>();
            for (int r = 0; r < count; r++)
            {
                if (unknown) { states.Add(RuleStrip.State.Unknown); continue; }
                bool live = r >= from && r < until && (spawnOn != 3 || r == from);
                states.Add(live ? RuleStrip.State.Live : RuleStrip.State.Gone);
            }
            var extras = new HashSet<int>(VehExtraRules.Extras.Select(e => e.Rule));
            VehRuleStrip.Show(states, VehExtraRules.MainRule, extras, r => string.Format(CultureInfo.CurrentCulture,
                states[r] == RuleStrip.State.Live ? TranslateOr("lc_tip_live", "Rule {0}: the vehicle is there") :
                states[r] == RuleStrip.State.Gone ? TranslateOr("lc_tip_gone", "Rule {0}: the vehicle is not there") : TranslateOr("lc_tip_unknown", "Rule {0}: depends on another team"), r + 1));
            VehStripLegend.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_legend", "Rules of team {0} · green: there · dot: its objective"), team + 1);

            VehSpawnText.Text = "  " + (spawnOn <= 0 ? TranslateOr("lc_at_start", "at mission start")
                : string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_with_rule", "with rule {0}"), spawnRule + 1)
                  + (spawnTeam >= 0 ? " · " + TranslateOr("dash_team", "Team") + " " + (spawnTeam + 1) : ""));
            VehRespawnText.Text = "  " + ((ddvehrsp.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");
            VehClearText.Text = "  " + (clearRule < 0 || clearTeam < 0 ? TranslateOr("lc_never", "never")
                : string.Format(CultureInfo.CurrentCulture, TranslateOr("lc_with_rule", "with rule {0}"), clearRule + 1) + " · " + TranslateOr("dash_team", "Team") + " " + (clearTeam + 1));

            string start = spawnOn <= 0 ? TranslateOr("lc_sum_start", "start") : "R" + (spawnRule + 1);
            string end = clearRule < 0 || clearTeam < 0 ? TranslateOr("lc_sum_end", "end") : "R" + (clearRule + 1);
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

        private static string PlainText(string text) => Regex.Replace(text ?? "", "~[A-Za-z_0-9]+~", "").Trim();

        /// <summary>"Rule N · objective text" entries; value -1 is "never" where allowed.</summary>
        private void FillRulePicker(ComboBox pick, int team, int value, bool allowNever)
        {
            _vehPickSync = true;
            try
            {
                pick.Items.Clear();
                if (allowNever)
                    pick.Items.Add(new ComboBoxItem { Content = TranslateOr("lc_never", "never"), Tag = -1 });
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
