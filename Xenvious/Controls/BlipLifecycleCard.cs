using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    /// <summary>
    /// Lifecycle of a free blip in the look of the entities' lifecycle card: the team whose rule counts
    /// (iTeam), the rule it appears with and the rule it disappears with. The controller shows the blip
    /// for iRule -2 (every rule) or inside the window sTeamBlip[0] start..end (end -1 = to the end), so
    /// "rule 1 until never" is written as -2 and every other choice as that window. A single rule set by
    /// the creator (iRule n) shows as the window n..n.
    /// </summary>
    public class BlipLifecycleCard : LifecycleCard
    {
        /// <summary>Writes team, iRule, window start and window end (inclusive, -1 = to the end).</summary>
        public Action<int, int, int, int> Write;

        private readonly ComboBox _teamPick = new ComboBox { Height = 30 };
        private readonly ComboBox _startPick = new ComboBox { Height = 30 };
        private readonly ComboBox _endPick = new ComboBox { Height = 30 };
        private readonly RuleStrip _blipStrip = new RuleStrip();
        private readonly TextBlock _startText = new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold };
        private readonly TextBlock _endText = new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold };
        private bool _blipSync;
        private int _team, _start, _end;

        public BlipLifecycleCard()
        {
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            IsExpanded = true;
            Margin = new Thickness(0, 0, 0, 12);
            var body = new StackPanel();
            body.Children.Add(_blipStrip);
            var legend = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
            legend.Children.Add(LegendItem(T("bl_lg_shown", "on the map"), "OkBrush"));
            legend.Children.Add(LegendItem(T("bl_lg_hidden", "hidden"), "BadBrush"));
            body.Children.Add(legend);
            body.Children.Add(Separator());
            body.Children.Add(Section("lc_spawn", "Appears", _startText, "OkBrush", "M12,19 L12,5 M5,12 L12,5 L19,12",
                Row((T("lc_team", "Team"), _teamPick), (T("lc_rule", "Rule"), _startPick))));
            body.Children.Add(Separator());
            body.Children.Add(Section("lc_clear", "Disappears", _endText, "BadBrush", "M4,12 A8,8 0 1 0 20,12 A8,8 0 1 0 4,12 M8,8 L16,16",
                Row((T("lc_rule", "Rule"), _endPick))));
            Content = body;

            _teamPick.SelectionChanged += (_, __) => { if (!_blipSync && _teamPick.SelectedIndex >= 0) Save(_teamPick.SelectedIndex, _start, _end); };
            _startPick.SelectionChanged += (_, __) => { if (!_blipSync && _startPick.SelectedItem is ComboBoxItem item) Save(_team, (int)item.Tag, _end); };
            _endPick.SelectionChanged += (_, __) => { if (!_blipSync && _endPick.SelectedItem is ComboBoxItem item) Save(_team, _start, (int)item.Tag); };
        }

        /// <summary>Shows a blip's stored values.</summary>
        public void Show(int team, int rule, int from, int to)
        {
            _team = team >= 0 && team < 4 ? team : 0;
            int count = Rules.Ready ? Rules.Count(_team) : 0;
            // start: -1 = never; end: the first rule it is gone on, -1 = never.
            if (rule == -2 || from == -2) { _start = 0; _end = -1; }
            else if (from >= 0) { _start = from; _end = to < 0 ? -1 : to + 1; }
            else if (rule >= 0) { _start = rule; _end = rule + 1; }
            else { _start = -1; _end = -1; }
            if (_end >= count) _end = -1;

            _blipSync = true;
            try
            {
                if (_teamPick.Items.Count != Math.Max(1, Rules.Teams()))
                {
                    _teamPick.Items.Clear();
                    for (int t = 0; t < Math.Max(1, Rules.Teams()); t++)
                        _teamPick.Items.Add(T("dash_team", "Team") + " " + (t + 1));
                }
                _teamPick.SelectedIndex = Math.Min(_team, _teamPick.Items.Count - 1);
                FillRules(_startPick, _team, _start, true);
                FillRules(_endPick, _team, _end, true);
                _endPick.IsEnabled = _start >= 0;

                var states = new List<RuleStrip.State>();
                for (int r = 0; r < count; r++)
                    states.Add(_start >= 0 && r >= _start && (_end < 0 || r < _end) ? RuleStrip.State.Live : RuleStrip.State.Gone);
                _blipStrip.Show(states, -1, new int[0], r => string.Format(CultureInfo.CurrentCulture,
                    states[r] == RuleStrip.State.Live ? T("bl_tip_shown", "Rule {0}: on the map") : T("bl_tip_hidden", "Rule {0}: hidden"), r + 1));

                _startText.Text = _start < 0 ? T("lc_never", "never") : string.Format(CultureInfo.CurrentCulture, T("lc_with_rule", "with rule {0}"), _start + 1);
                _endText.Text = _end < 0 ? T("lc_never", "never") : string.Format(CultureInfo.CurrentCulture, T("lc_with_rule", "with rule {0}"), _end + 1);
                Summary = T("dash_team", "Team") + " " + (_team + 1) + " · " + (_start < 0 ? "–" : "R" + (_start + 1)) + " → " + (_end < 0 ? T("lc_sum_end", "end") : "R" + (_end + 1));
            }
            finally { _blipSync = false; }
        }

        private void Save(int team, int start, int end)
        {
            if (end >= 0 && end <= start)
                end = start + 1;
            if (start < 0)
                Write?.Invoke(team, -1, -1, -1);
            else if (start == 0 && end < 0)
                Write?.Invoke(team, -2, -1, -1);
            else
                Write?.Invoke(team, -1, start, end < 0 ? -1 : end - 1);
        }
    }
}
