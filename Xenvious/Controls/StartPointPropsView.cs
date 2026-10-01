using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// The Mission Creator's spawn point properties for the picked start point: start / respawn
    /// point, checkpoints, the rules it is usable on and the personal vehicle options, with the
    /// creator's locks between them (func_2700 / func_2701, see <see cref="StartPoints"/>).
    /// </summary>
    public class StartPointPropsView : SectionCard
    {
        private readonly ComboBox _teamBox;
        private readonly ComboBox _indexBox;
        private readonly StackPanel _body = new StackPanel();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private string _shown;
        private bool _loading;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public StartPointPropsView(ComboBox teamBox, ComboBox indexBox)
        {
            _teamBox = teamBox;
            _indexBox = indexBox;
            Style = (Style)MainWindow.Instance.FindResource(typeof(SectionCard));
            Title = T("sp_props", "Spawn point");
            Icon = Geometry.Parse("M4,6 L20,6 M4,12 L20,12 M4,18 L20,18 M8,4 L8,8 M15,10 L15,14 M10,16 L10,20");
            Content = _body;
            Margin = new Thickness(0, 0, 0, 12);
            IsVisibleChanged += (_, __) => { if (IsVisible) { Refresh(true); _timer.Start(); } else _timer.Stop(); };
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Refresh(false); };
            _teamBox.SelectionChanged += (_, __) => Refresh(true);
            _indexBox.SelectionChanged += (_, __) => Refresh(true);
        }

        private int Team => Math.Max(0, _teamBox.SelectedIndex);
        private int Index => _indexBox.SelectedIndex;
        private bool Live => MainWindow.m != null && MainWindow.m.IsProcOpen && StartPoints.Ready && Index >= 0 && Index < StartPoints.Count(Team);

        private void Refresh(bool force)
        {
            if (!Live)
            {
                _body.Children.Clear();
                _shown = null;
                _body.Children.Add(Faint(T("sp_pick", "Pick a start point in the list."), 13));
                return;
            }
            int bits = StartPoints.Bits(Team, Index);
            int from = StartPoints.Get(GTA.Offsets.Editor.player_vfrs, Team, Index);
            int until = StartPoints.Get(GTA.Offsets.Editor.player_vfre, Team, Index);
            int veh = StartPoints.Get(GTA.Offsets.Editor.player_veh, Team, Index);
            int rules = Rules.Ready ? Rules.Count(Team) : 0;
            string key = Rules.PublicCreator + "|" + bits + "|" + from + "|" + until + "|" + veh + "|" + rules + "|" + Team + "|" + Index;
            if (!force && key == _shown)
                return;
            _shown = key;
            Summary = string.Format(CultureInfo.CurrentCulture, T("sp_point_n", "Point {0}"), Index + 1);

            _loading = true;
            _body.Children.Clear();
            _body.Children.Add(Toggle(T("sp_isstart", "Treat as a start point"), T("sp_isstart_h", "Players can be put here when the mission starts."), bits, StartPoints.BitStart, true));
            _body.Children.Add(Toggle(T("sp_isrespawn", "Treat as a respawn point"), T("sp_isrespawn_h", "Players can respawn here after dying. By default they respawn near where they died."), bits, StartPoints.BitRespawn, true));
            // Mission checkpoints (restart after a failed attempt) only exist in the Mission Creator.
            bool pmc = Rules.PublicCreator;
            string cpHint = pmc ? T("sp_cp_h", "Used as a start point when restarting from this checkpoint.") : T("sp_cp_mission", "Only in the Mission Creator.");
            _body.Children.Add(Toggle(string.Format(CultureInfo.CurrentCulture, T("sp_cp", "Checkpoint {0}"), 1), cpHint, bits, StartPoints.BitCheckpoint1, pmc));
            _body.Children.Add(Toggle(string.Format(CultureInfo.CurrentCulture, T("sp_cp", "Checkpoint {0}"), 2), cpHint, bits, StartPoints.BitCheckpoint2, pmc));

            _body.Children.Add(Label(T("sp_active", "Usable")));
            var range = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            range.ColumnDefinitions.Add(new ColumnDefinition());
            range.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            range.ColumnDefinitions.Add(new ColumnDefinition());
            var fromBox = RuleBox(T("sp_from", "From"), T("sp_mstart", "Mission start"), from, rules, GTA.Offsets.Editor.player_vfrs);
            var untilBox = RuleBox(T("sp_until", "Until"), T("sp_mend", "Mission end"), until, rules, GTA.Offsets.Editor.player_vfre);
            Grid.SetColumn(untilBox, 2);
            range.Children.Add(fromBox);
            range.Children.Add(untilBox);
            _body.Children.Add(range);

            // The creator's locks: not in the personal vehicle when a placed vehicle is assigned or
            // creation is blocked; block only when neither spawn in nor force is on.
            bool spawnIn = (bits & (1 << StartPoints.BitSpawnInPv)) != 0, forcePv = (bits & (1 << StartPoints.BitForcePv)) != 0, block = (bits & (1 << StartPoints.BitBlockPv)) != 0;
            _body.Children.Add(Label(T("sp_pv", "Personal vehicle")));
            _body.Children.Add(Toggle(T("sp_pv_spawn", "Spawn in personal vehicle"), veh > -1 ? T("sp_pv_lockveh", "Not with a placed vehicle assigned.") : block ? T("sp_pv_lockblock", "Not while creation is blocked.") : T("sp_pv_spawn_h", "In the creator a standard vehicle stands in for it."),
                bits, StartPoints.BitSpawnInPv, veh < 0 && !block));
            _body.Children.Add(Toggle(T("sp_pv_force", "Force personal vehicle creation"), block ? T("sp_pv_lockblock", "Not while creation is blocked.") : T("sp_pv_force_h", "Creates it here even if the mission turns it off."), bits, StartPoints.BitForcePv, !block));
            _body.Children.Add(Toggle(T("sp_pv_block", "Block personal vehicle creation"), spawnIn || forcePv ? T("sp_pv_lockon", "Not while spawn in or force is on.") : T("sp_pv_block_h", "No personal vehicle here even if the mission creates one."), bits, StartPoints.BitBlockPv, !spawnIn && !forcePv));
            _loading = false;
        }

        private FrameworkElement Toggle(string text, string hint, int bits, int bit, bool enabled)
        {
            var row = new DockPanel { Margin = new Thickness(0, 4, 0, 6) };
            var box = new CheckBox { Style = (Style)MainWindow.Instance.FindResource("FormToggle"), IsChecked = (bits & (1 << bit)) != 0, IsEnabled = enabled, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
            box.Click += (_, __) =>
            {
                if (_loading || !Live) return;
                StartPoints.SetBit(Team, Index, bit, box.IsChecked == true);
                Refresh(true);
            };
            DockPanel.SetDock(box, Dock.Right);
            row.Children.Add(box);
            var text2 = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var title = new TextBlock { Text = text, FontSize = 13.5, TextWrapping = TextWrapping.Wrap };
            title.SetResourceReference(TextBlock.ForegroundProperty, enabled ? "TextColor" : "FaintTextBrush");
            text2.Children.Add(title);
            text2.Children.Add(Faint(hint, 11.5));
            row.Children.Add(text2);
            return row;
        }

        // -1 is mission start / end; the rest are the team's rules.
        private FrameworkElement RuleBox(string caption, string none, int value, int rules, long field)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = caption }.Also(t => t.SetResourceReference(StyleProperty, "FieldAxis")));
            var box = new ComboBox { Height = 30, IsEnabled = field != 0 };
            box.Items.Add(new ComboBoxItem { Content = none, Tag = -1 });
            for (int r = 0; r < Math.Max(rules, value + 1); r++)
                box.Items.Add(new ComboBoxItem { Content = T("rl_rule_n", "Rule {0}").Replace("{0}", (r + 1).ToString(CultureInfo.CurrentCulture)), Tag = r });
            box.SelectedItem = box.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == value) ?? box.Items[0];
            box.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(box.SelectedItem is ComboBoxItem item)) return;
                StartPoints.Set(field, Team, Index, (int)item.Tag);
            };
            stack.Children.Add(box);
            return stack;
        }

        private static TextBlock Label(string text)
        {
            var label = new TextBlock { Text = text, Margin = new Thickness(0, 10, 0, 2) };
            label.SetResourceReference(StyleProperty, "FieldLabel");
            return label;
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }
    }
}
