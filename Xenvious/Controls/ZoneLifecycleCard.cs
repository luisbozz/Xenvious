using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    /// <summary>
    /// Lifecycle of a zone: per team a start rule (znpr, "iznstrp", f_12[team]) and an end rule
    /// (znepr, "iznendp", f_17[team]; 99999 = never). Same look as the entities' lifecycle card.
    /// The page's team box and its two rule boxes stay (hidden) and keep writing the game.
    /// </summary>
    public class ZoneLifecycleCard : LifecycleCard
    {
        private const int Never = 99999;
        private ComboBox _team;
        private TextBox _start, _end;
        private bool _zoneSync;

        private readonly ComboBox _teamPick = new ComboBox { Height = 30 };
        private readonly ComboBox _startPick = new ComboBox { Height = 30 };
        private readonly ComboBox _endPick = new ComboBox { Height = 30 };
        private readonly RuleStrip _zoneStrip = new RuleStrip();
        private readonly TextBlock _startText = new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold };
        private readonly TextBlock _endText = new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold };

        public void AttachZone(ComboBox team, TextBox start, TextBox end, FrameworkElement before)
        {
            _team = team; _start = start; _end = end;
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            IsExpanded = true;

            var body = new StackPanel();
            body.Children.Add(_zoneStrip);
            var legend = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
            legend.Children.Add(LegendItem(T("lc_lg_there", "there"), "OkBrush"));
            legend.Children.Add(LegendItem(T("lc_lg_gone", "not there"), "BadBrush"));
            body.Children.Add(legend);
            body.Children.Add(Separator());
            body.Children.Add(Section("lc_spawn", "Appears", _startText, "OkBrush", "M12,19 L12,5 M5,12 L12,5 L19,12",
                Row((T("lc_team", "Team"), _teamPick), (T("lc_rule", "Rule"), _startPick))));
            body.Children.Add(Separator());
            body.Children.Add(Section("lc_clear", "Disappears", _endText, "BadBrush", "M4,12 A8,8 0 1 0 20,12 A8,8 0 1 0 4,12 M8,8 L16,16",
                Row((T("lc_rule", "Rule"), _endPick))));
            Content = body;

            foreach (var item in team.Items)
                _teamPick.Items.Add((item as ComboBoxItem)?.Content?.ToString() ?? item?.ToString());
            team.SelectionChanged += (_, __) => { if (!_zoneSync) RefreshZone(); };
            start.TextChanged += (_, __) => { if (!_zoneSync) RefreshZone(); };
            end.TextChanged += (_, __) => { if (!_zoneSync) RefreshZone(); };
            team.IsEnabledChanged += (_, __) => RefreshZone();
            IsVisibleChanged += (_, __) => { if (IsVisible) RefreshZone(); };
            _teamPick.SelectionChanged += (_, __) => { if (!_zoneSync) { _team.SelectedIndex = _teamPick.SelectedIndex; RefreshZone(); } };
            _startPick.SelectionChanged += (_, __) => { if (!_zoneSync) Write(_start, _startPick); };
            _endPick.SelectionChanged += (_, __) => { if (!_zoneSync) Write(_end, _endPick); };

            if (before.Parent is Panel panel)
                panel.Children.Insert(panel.Children.IndexOf(before), this);
            Margin = new Thickness(0, 0, 0, 12);
            RefreshZone();
        }

        private void Write(TextBox box, ComboBox pick)
        {
            if (pick.SelectedItem is ComboBoxItem item)
            {
                string text = ((int)item.Tag).ToString(CultureInfo.InvariantCulture);
                if (box.Text != text)
                    box.Text = text;
            }
            RefreshZone();
        }

        private static int Parse(string text, int fallback)
            => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;

        private void RefreshZone()
        {
            if (_team == null)
                return;
            _zoneSync = true;
            try
            {
                bool enabled = _team.IsEnabled && _team.SelectedIndex >= 0;
                _teamPick.IsEnabled = _startPick.IsEnabled = _endPick.IsEnabled = enabled;
                if (!_teamPick.IsDropDownOpen)
                    _teamPick.SelectedIndex = _team.SelectedIndex;
                int team = System.Math.Max(0, _team.SelectedIndex);
                int start = Parse(_start.Text, -1);
                int end = Parse(_end.Text, Never);

                // "Never" for the start: the zone is not used by this team.
                FillRules(_startPick, team, start < 0 ? -1 : start, true);
                FillRules(_endPick, team, end >= Never || end < 0 ? -1 : end, true);
                // The rule pickers' "never" item is -1; the end rule stores "never" as 99999.
                foreach (ComboBoxItem item in _endPick.Items)
                    if ((int)item.Tag == -1) item.Tag = Never;
                if (end >= Never || end < 0)
                    _endPick.SelectedIndex = 0;

                int count = enabled && Rules.Ready ? Rules.Count(team) : 0;
                var states = new List<RuleStrip.State>();
                for (int r = 0; r < count; r++)
                    states.Add(start >= 0 && r >= start && (end >= Never || end < 0 || r < end) ? RuleStrip.State.Live : RuleStrip.State.Gone);
                _zoneStrip.Show(states, -1, new int[0], r => string.Format(CultureInfo.CurrentCulture,
                    states[r] == RuleStrip.State.Live ? T("lc_tip_live_e", "Rule {0}: it is there") : T("lc_tip_gone_e", "Rule {0}: it is not there"), r + 1));

                _startText.Text = start < 0 ? T("lc_never", "never") : string.Format(CultureInfo.CurrentCulture, T("lc_with_rule", "with rule {0}"), start + 1);
                _endText.Text = end >= Never || end < 0 ? T("lc_never", "never") : string.Format(CultureInfo.CurrentCulture, T("lc_with_rule", "with rule {0}"), end + 1);
                Summary = T("dash_team", "Team") + " " + (team + 1) + " · " + (start < 0 ? "–" : "R" + (start + 1)) + " → " + (end >= Never || end < 0 ? T("lc_sum_end", "end") : "R" + (end + 1));
            }
            finally { _zoneSync = false; }
        }
    }
}
