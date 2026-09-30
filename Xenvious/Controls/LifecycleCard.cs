using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xenvious
{
    /// <summary>
    /// "Lifecycle" card for pages whose entity only has spawn and clean-up rules (props, dynamic
    /// props, weapons): a rule strip that shows in which rules the entity exists, then "appears"
    /// and "disappears" with rule pickers. It replaces the old "Associated rule" card: the page's
    /// own boxes stay (hidden) and keep writing the game; this card only reads and sets them,
    /// the same way the Vehicles page's lifecycle does.
    /// </summary>
    public class LifecycleCard : SectionCard
    {
        private ComboBox _spawnOn, _spawnTeam, _clearTeam;
        private TextBox _spawnRule, _clearRule;
        private bool _sync;

        private readonly ComboBox _when = new ComboBox { Height = 30 };
        private readonly ComboBox _spawnRulePick = new ComboBox { Height = 30 };
        private readonly ComboBox _spawnTeamPick = new ComboBox { Height = 30 };
        private readonly ComboBox _clearRulePick = new ComboBox { Height = 30 };
        private readonly ComboBox _clearTeamPick = new ComboBox { Height = 30 };
        private readonly RuleStrip _strip = new RuleStrip();
        private readonly TextBlock _spawnText = new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold };
        private readonly TextBlock _clearText = new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold };
        private readonly TextBlock _stripTeam = new TextBlock { FontSize = 12, Margin = new Thickness(0, 0, 0, 2) };

        protected static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public LifecycleCard()
        {
            Title = T("lc_card", "Lifecycle");
            Icon = Geometry.Parse("M3,12 L7,12 L10,5 L14,19 L17,12 L21,12");
            CanCollapse = true;
            VerticalAlignment = VerticalAlignment.Top;
            IsVisibleChanged += (_, __) => { if (IsVisible) Refresh(); };
        }

        /// <summary>
        /// Takes the page's boxes and puts this card where their old card was. The old card
        /// (the DashCard around <paramref name="spawnOn"/>) is hidden, not removed.
        /// </summary>
        public void Attach(ComboBox spawnOn, ComboBox spawnTeam, TextBox spawnRule, ComboBox clearTeam, TextBox clearRule, bool expanded)
        {
            _spawnOn = spawnOn; _spawnTeam = spawnTeam; _spawnRule = spawnRule; _clearTeam = clearTeam; _clearRule = clearRule;
            // The window's implicit style is keyed to SectionCard and does not reach a subclass.
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            IsExpanded = expanded;
            Content = Body();

            foreach (var box in new[] { spawnOn, spawnTeam, clearTeam })
                box.SelectionChanged += (_, __) => { if (!_sync) Refresh(); };
            foreach (var box in new[] { spawnRule, clearRule })
                box.TextChanged += (_, __) => { if (!_sync) Refresh(); };
            spawnOn.IsEnabledChanged += (_, __) => Refresh();

            _when.SelectionChanged += (_, __) => { if (!_sync) Push(() => _spawnOn.SelectedIndex = _when.SelectedIndex); };
            _spawnTeamPick.SelectionChanged += (_, __) => { if (!_sync) Push(() => _spawnTeam.SelectedIndex = _spawnTeamPick.SelectedIndex); };
            _clearTeamPick.SelectionChanged += (_, __) => { if (!_sync) Push(() => _clearTeam.SelectedIndex = _clearTeamPick.SelectedIndex); };
            _spawnRulePick.SelectionChanged += (_, __) => { if (!_sync) Push(() => SetText(_spawnRule, _spawnRulePick)); };
            _clearRulePick.SelectionChanged += (_, __) => { if (!_sync) Push(() => SetText(_clearRule, _clearRulePick)); };

            var dashCard = (Style)MainWindow.Instance?.TryFindResource("DashCard");
            FrameworkElement old = spawnOn;
            while (old != null && !(old is Border b && b.Style == dashCard))
                old = old.Parent as FrameworkElement;
            if (old?.Parent is Panel panel)
            {
                Margin = old.Margin;
                panel.Children.Insert(panel.Children.IndexOf(old), this);
                old.Visibility = Visibility.Collapsed;
            }
            Refresh();
        }

        private void Push(Action write)
        {
            // The page's handlers write the game; Refresh then redraws from the page's boxes.
            write();
            Refresh();
        }

        private static void SetText(TextBox box, ComboBox pick)
        {
            if (pick.SelectedItem is ComboBoxItem item)
            {
                string text = ((int)item.Tag).ToString(CultureInfo.InvariantCulture);
                if (box.Text != text)
                    box.Text = text;
            }
        }

        private FrameworkElement Body()
        {
            var body = new StackPanel();
            body.Children.Add(_strip);
            // Same legend as the Vehicles page's lifecycle.
            var legend = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
            legend.Children.Add(LegendItem(T("lc_lg_there", "there"), "OkBrush"));
            legend.Children.Add(LegendItem(T("lc_lg_gone", "not there"), "BadBrush"));
            _stripTeam.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _stripTeam.FontSize = 11.5;
            _stripTeam.VerticalAlignment = VerticalAlignment.Center;
            legend.Children.Add(_stripTeam);
            body.Children.Add(legend);
            body.Children.Add(Separator());
            body.Children.Add(Section("lc_spawn", "Appears", _spawnText, "OkBrush", "M12,19 L12,5 M5,12 L12,5 L19,12",
                Row((T("lc_when", "When"), _when), (T("lc_rule", "Rule"), _spawnRulePick), (T("lc_team", "Team"), _spawnTeamPick))));
            body.Children.Add(Separator());
            body.Children.Add(Section("lc_clear", "Disappears", _clearText, "BadBrush", "M4,12 A8,8 0 1 0 20,12 A8,8 0 1 0 4,12 M8,8 L16,16",
                Row((T("lc_rule", "Rule"), _clearRulePick), (T("lc_team", "Team"), _clearTeamPick))));
            return body;
        }

        /// <summary>Adds a section under "disappears" (the Actors page puts its respawn and action settings here).</summary>
        public void AddSection(string key, string fallback, string brush, string icon, FrameworkElement content)
        {
            if (Content is Panel body)
            {
                body.Children.Add(Separator());
                body.Children.Add(Section(key, fallback, new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold }, brush, icon, content));
            }
        }

        protected static UIElement LegendItem(string text, string brush)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 12, 2) };
            var swatch = new Grid { Width = 12, Height = 12, VerticalAlignment = VerticalAlignment.Center };
            var tint = new Border { CornerRadius = new CornerRadius(3), Opacity = 0.16 };
            tint.SetResourceReference(Border.BackgroundProperty, brush);
            var frame = new Border { CornerRadius = new CornerRadius(3), BorderThickness = new Thickness(1) };
            frame.SetResourceReference(Border.BorderBrushProperty, brush);
            swatch.Children.Add(tint);
            swatch.Children.Add(frame);
            row.Children.Add(swatch);
            var t = new TextBlock { Text = text, FontSize = 11.5, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            row.Children.Add(t);
            return row;
        }

        protected static Rectangle Separator()
        {
            var line = new Rectangle { Height = 1, Margin = new Thickness(0, 12, 0, 12) };
            line.SetResourceReference(Shape.FillProperty, "LineBrush");
            return line;
        }

        protected FrameworkElement Section(string key, string fallback, TextBlock status, string brush, string icon, FrameworkElement content)
        {
            var dock = new DockPanel();
            var badge = new Grid { Width = 30, Height = 30, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 0, 12, 0) };
            var tint = new Border { CornerRadius = new CornerRadius(7), Opacity = 0.16 };
            tint.SetResourceReference(Border.BackgroundProperty, brush);
            var path = new Path { Data = Geometry.Parse(icon), StrokeThickness = 1.8, StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
                Stretch = Stretch.Uniform, Width = 14, Height = 14 };
            path.SetResourceReference(Shape.StrokeProperty, brush);
            badge.Children.Add(tint);
            badge.Children.Add(path);
            DockPanel.SetDock(badge, Dock.Left);
            dock.Children.Add(badge);

            var stack = new StackPanel();
            var head = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var title = new TextBlock { Text = T(key, fallback), FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 8, 0) };
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            status.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            status.VerticalAlignment = VerticalAlignment.Bottom;
            head.Children.Add(title);
            head.Children.Add(status);
            stack.Children.Add(head);
            stack.Children.Add(content);
            dock.Children.Add(stack);
            return dock;
        }

        protected FrameworkElement Row(params (string Label, ComboBox Box)[] fields)
        {
            var grid = new Grid();
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                var cell = new StackPanel();
                cell.Children.Add(new TextBlock { Style = (Style)MainWindow.Instance.FindResource("FieldLabel"), Text = fields[i].Label });
                cell.Children.Add(fields[i].Box);
                Grid.SetColumn(cell, i * 2);
                grid.Children.Add(cell);
            }
            return grid;
        }

        private static int Parse(string text, int fallback)
            => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;

        private static string Plain(string text) => Regex.Replace(text ?? "", "~[A-Za-z_0-9]+~", "").Trim();

        /// <summary>Redraws from the page's boxes (after the page loaded another entity).</summary>
        public void Refresh()
        {
            if (_spawnOn == null || !IsLoaded && !IsVisible)
                return;
            _sync = true;
            try
            {
                bool enabled = _spawnOn.IsEnabled;
                foreach (var box in new[] { _when, _spawnRulePick, _spawnTeamPick, _clearRulePick, _clearTeamPick })
                    box.IsEnabled = enabled;

                Copy(_spawnOn, _when);
                Copy(_spawnTeam, _spawnTeamPick);
                Copy(_clearTeam, _clearTeamPick);

                int spawnOn = _spawnOn.SelectedIndex;
                int spawnTeam = _spawnTeam.SelectedIndex - 1;
                int spawnRule = Parse(_spawnRule.Text, 0);
                int clearTeam = _clearTeam.SelectedIndex - 1;
                int clearRule = Parse(_clearRule.Text, -1);
                // The strip shows the team the entity belongs to: its spawn team, else its
                // clean-up team, else team 1.
                int team = spawnTeam >= 0 ? spawnTeam : clearTeam >= 0 ? clearTeam : 0;

                FillRules(_spawnRulePick, spawnTeam >= 0 ? spawnTeam : team, spawnRule, false);
                FillRules(_clearRulePick, clearTeam >= 0 ? clearTeam : team, clearRule, true);
                _spawnRulePick.IsEnabled = _clearRulePick.IsEnabled = enabled;
                // At mission start the spawn rule is not used.
                _spawnRulePick.IsEnabled = enabled && spawnOn > 0;

                int count = enabled && Rules.Ready ? Rules.Count(team) : 0;
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
                _strip.Show(states, -1, new int[0], r => string.Format(CultureInfo.CurrentCulture,
                    states[r] == RuleStrip.State.Live ? T("lc_tip_live_e", "Rule {0}: it is there") : states[r] == RuleStrip.State.Gone ? T("lc_tip_gone_e", "Rule {0}: it is not there")
                    : T("lc_tip_unknown", "Rule {0}: depends on another team"), r + 1));
                _stripTeam.Text = "· " + T("dash_team", "Team") + " " + (team + 1);

                _spawnText.Text = spawnOn <= 0 ? T("lc_at_start", "at mission start")
                    : string.Format(CultureInfo.CurrentCulture, T("lc_with_rule", "with rule {0}"), spawnRule + 1) + (spawnTeam >= 0 ? " · " + T("dash_team", "Team") + " " + (spawnTeam + 1) : "");
                _clearText.Text = clearRule < 0 || clearTeam < 0 ? T("lc_never", "never")
                    : string.Format(CultureInfo.CurrentCulture, T("lc_with_rule", "with rule {0}"), clearRule + 1) + " · " + T("dash_team", "Team") + " " + (clearTeam + 1);
                string start = spawnOn <= 0 ? T("lc_sum_start", "start") : "R" + (spawnRule + 1);
                string end = clearRule < 0 || clearTeam < 0 ? T("lc_sum_end", "end") : "R" + (clearRule + 1);
                Summary = start + " → " + end;
            }
            finally { _sync = false; }
        }

        // The card's box shows the same items as the page's box.
        private static void Copy(ComboBox from, ComboBox to)
        {
            if (to.IsDropDownOpen)
                return;
            if (to.Items.Count != from.Items.Count)
            {
                to.Items.Clear();
                foreach (var item in from.Items)
                    to.Items.Add(new ComboBoxItem { Content = (item as ComboBoxItem)?.Content?.ToString() ?? item?.ToString() });
            }
            to.SelectedIndex = from.SelectedIndex;
        }

        protected static void FillRules(ComboBox pick, int team, int value, bool allowNever)
        {
            if (pick.IsDropDownOpen)
                return;
            pick.Items.Clear();
            if (allowNever)
                pick.Items.Add(new ComboBoxItem { Content = T("lc_never", "never"), Tag = -1 });
            int count = Rules.Ready ? Rules.Count(team) : 0;
            for (int r = 0; r < count; r++)
            {
                string text = Plain(Rules.Text(team, r));
                if (text.Length > 26) text = text.Substring(0, 25) + "…";
                pick.Items.Add(new ComboBoxItem { Content = T("rl_rule_n", "Rule {0}").Replace("{0}", (r + 1).ToString(CultureInfo.CurrentCulture)) + (text.Length > 0 ? " · " + text : ""), Tag = r });
            }
            if (value >= count || (value < 0 && !allowNever))
                pick.Items.Add(new ComboBoxItem { Content = T("rl_rule_n", "Rule {0}").Replace("{0}", (value + 1).ToString(CultureInfo.CurrentCulture)) + " " + T("lc_missing", "(missing)"), Tag = value });
            pick.SelectedItem = pick.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == (allowNever && value < 0 ? -1 : value));
        }
    }
}
