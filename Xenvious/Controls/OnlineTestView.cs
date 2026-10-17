using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Mission › Test: "Test like online" (<see cref="OnlineTest"/>) with one switch per patch set,
    /// what stays as in the test, and the solo / controller card. Greyed out outside the LTS and
    /// Capture creators, never locked.
    /// </summary>
    public class OnlineTestView : StackPanel
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private readonly Dictionary<string, (CheckBox Toggle, TextBlock Badge, FrameworkElement Row)> _rows = new Dictionary<string, (CheckBox, TextBlock, FrameworkElement)>();
        private CheckBox _master, _invincible, _ruleNotes;
        private TextBlock _invincibleBadge;
        private WrapPanel _status;
        private FrameworkElement _body, _page;
        private bool _built, _sync;

        public OnlineTestView()
        {
            Margin = new Thickness(10, 12, 10, 12);
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Refresh(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => Refresh();
        }

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static (string Key, string Name, string Help)[] Switches => new[]
        {
            ("inv", T("olt_inv", "Inventory like online"), T("olt_inv_help", "The team inventory with \"Remove Weapons before Spawn\" and Lock Weapons, instead of only the start weapon or the fixed test loadout.")),
            ("looks", T("olt_looks", "Own tints & outfit"), T("olt_looks_help", "Weapon tints and looks from your account when the job sets none. Only read, nothing is written.")),
            ("phone", T("olt_phone", "Phone & SMS"), T("olt_phone_help", "The phone opens and the job's text messages arrive.")),
            ("death", T("olt_death", "Death by play area, zones & rules"), T("olt_death_help2", "An expired play area timer, kill zones, gas and drowning (team option) kill like online. To die from them, switch off \"Invincible in the test\" too.")),
            ("celeb", T("olt_celeb", "End screen"), T("olt_celeb_help", "The end cutscene and the celebration screen with the result, instead of only fading out. Experimental: if the test does not end afterwards, switch it off.")),
            ("time", T("olt_time", "Job time limit"), T("olt_time_help", "The job's overall time limit counts, not only the rule's.")),
            ("pick", T("olt_pick", "Weapon pickups like online"), T("olt_pick_help", "LTS and Capture pickups choose their weapon like online instead of using the raw type.")),
            ("traffic", T("olt_traffic", "Traffic & pedestrians"), T("olt_traffic_help", "The density the job sets, not the fixed test density.")),
            ("pv", T("olt_pv", "Personal vehicle like online"), T("olt_pv_help", "Jobs with \"Use Personal Vehicle\" or respawn in the personal vehicle use yours, as online. Only when the creator has your vehicle data loaded; not tested yet.")),
        };

        /// <summary>Everything this page can be searched for: key, name, help (side list search).</summary>
        public static IEnumerable<(string Key, string Name, string Help)> SearchEntries() =>
            Switches.Concat(new[]
            {
                ("invincible", T("olt_invincible", "Invincible in the test"), T("olt_invincible_help", "")),
                ("rulenotes", T("olt_rulenotes", "Show rule changes in the game"), T("olt_rulenotes_help", "")),
                ("master", T("olt_title", "Test like online"), T("olt_hint", "")),
            });

        /// <summary>Scrolls to a switch of <see cref="SearchEntries"/> and flashes it.</summary>
        public void Reveal(string key)
        {
            Build();
            FrameworkElement target = key == "invincible" ? _invincible?.Parent as FrameworkElement
                : key == "rulenotes" ? _ruleNotes?.Parent as FrameworkElement
                : key == "master" ? _master?.Parent as FrameworkElement
                : _rows.TryGetValue(key, out var row) ? row.Row : null;
            if (target == null)
                return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                target.BringIntoView();
                FlashOutline.Show(target, null);
            }), DispatcherPriority.Loaded);
        }

        private Border Card(string key, string fallback, UIElement content)
        {
            var title = new TextBlock { Text = T(key, fallback) };
            title.SetResourceReference(StyleProperty, "DashCardTitle");
            var header = new Border { Child = title };
            header.SetResourceReference(StyleProperty, "DashCardHeader");
            var dock = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            dock.Children.Add(content);
            var card = new Border { Child = dock, Margin = new Thickness(0, 0, 0, 12) };
            card.SetResourceReference(StyleProperty, "DashCard");
            return card;
        }

        private static TextBlock Text(string text, double size, string brush, FontWeight? weight = null)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            if (weight.HasValue) t.FontWeight = weight.Value;
            t.SetResourceReference(TextBlock.ForegroundProperty, brush);
            return t;
        }

        private static CheckBox Toggle()
        {
            var c = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
            c.SetResourceReference(StyleProperty, "FormToggle");
            return c;
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            // Master switch.
            var master = new DockPanel { Margin = new Thickness(14, 12, 14, 12) };
            _master = Toggle();
            _master.Margin = new Thickness(14, 2, 0, 0);
            _master.VerticalAlignment = VerticalAlignment.Top;
            _master.Click += (_, __) => { if (!_sync) { OnlineTest.Enabled = _master.IsChecked == true; Refresh(); } };
            DockPanel.SetDock(_master, Dock.Right);
            master.Children.Add(_master);
            var text = new StackPanel();
            text.Children.Add(Text(T("olt_title", "Test like online"), 15, "TextColor", FontWeights.Bold));
            var hint = Text(T("olt_hint", "The creator test behaves like a real job. Only in the creator test of LTS and Capture; real online jobs stay as they are, because every change only answers the controller's \"is this a creator test?\" with no. Classic controller only so far."), 12.5, "MutedTextBrush");
            hint.Margin = new Thickness(0, 4, 0, 8);
            text.Children.Add(hint);
            _status = new WrapPanel();
            text.Children.Add(_status);
            master.Children.Add(text);
            var masterCard = new Border { Child = master, Margin = new Thickness(0, 0, 0, 12) };
            masterCard.SetResourceReference(StyleProperty, "DashCard");

            // Switches.
            var list = new StackPanel { Margin = new Thickness(14, 6, 14, 6) };
            foreach (var (key, name, help) in Switches)
            {
                string feature = key;
                var row = new DockPanel { Margin = new Thickness(0, 6, 0, 6) };
                var toggle = Toggle();
                toggle.Click += (_, __) => { if (!_sync) { OnlineTest.Set(feature, toggle.IsChecked == true); Refresh(); } };
                DockPanel.SetDock(toggle, Dock.Right);
                row.Children.Add(toggle);
                var left = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
                var line = new StackPanel { Orientation = Orientation.Horizontal };
                line.Children.Add(Text(name, 13, "TextColor"));
                var badge = Text("", 11, "FaintTextBrush");
                badge.Margin = new Thickness(8, 1, 0, 0);
                line.Children.Add(badge);
                left.Children.Add(line);
                var h = Text(help, 12, "FaintTextBrush");
                h.Margin = new Thickness(0, 2, 0, 0);
                left.Children.Add(h);
                row.Children.Add(left);
                list.Children.Add(row);
                _rows[feature] = (toggle, badge, row);
            }

            // Right column: simulated players (not built yet), what stays, solo & controller.
            var players = new StackPanel { Margin = new Thickness(14, 12, 14, 12), IsEnabled = false, Opacity = 0.5 };
            players.Children.Add(Text(T("olt_players", "Players in the job"), 13, "TextColor"));
            var ph = Text(T("olt_players_help", "Vehicles, objects, peds and pickups with \"minimum players\" spawn as with this many players. Coming later: the test fixes this number to 1 in more places than one switch can cover."), 12, "FaintTextBrush");
            ph.Margin = new Thickness(0, 4, 0, 0);
            players.Children.Add(ph);

            var keep = new StackPanel { Margin = new Thickness(14, 12, 14, 8) };
            // Invincibility has its own switch, independent of "Test like online".
            var inv = new DockPanel { Margin = new Thickness(14, 12, 14, 12) };
            _invincible = Toggle();
            _invincible.Click += (_, __) => { if (!_sync) { OnlineTest.Invincible = _invincible.IsChecked == true; Refresh(); } };
            DockPanel.SetDock(_invincible, Dock.Right);
            inv.Children.Add(_invincible);
            var invText = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
            var invLine = new StackPanel { Orientation = Orientation.Horizontal };
            invLine.Children.Add(Text(T("olt_invincible", "Invincible in the test"), 13, "TextColor"));
            _invincibleBadge = Text("", 11, "FaintTextBrush");
            _invincibleBadge.Margin = new Thickness(8, 1, 0, 0);
            invLine.Children.Add(_invincibleBadge);
            invText.Children.Add(invLine);
            var invHelp = Text(T("olt_invincible_help", "On: as the creator does it, you cannot die. Off: the creator no longer makes you invincible when the test starts, so you take damage and die as online. Takes effect at the next test start."), 12, "FaintTextBrush");
            invHelp.Margin = new Thickness(0, 2, 0, 0);
            invText.Children.Add(invHelp);
            inv.Children.Add(invText);
            // Outside the switches' grid, which is greyed out while "Test like online" is off.
            var healthCard = Card("olt_card_health", "Health in the test", inv);

            // Rule changes in the game (RuleNotes), also outside "Test like online".
            var notes = new DockPanel { Margin = new Thickness(14, 12, 14, 12) };
            _ruleNotes = Toggle();
            _ruleNotes.Click += (_, __) => { if (!_sync) OnlineTest.RuleNotesOn = _ruleNotes.IsChecked == true; };
            DockPanel.SetDock(_ruleNotes, Dock.Right);
            notes.Children.Add(_ruleNotes);
            var notesText = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
            notesText.Children.Add(Text(T("olt_rulenotes", "Show rule changes in the game"), 13, "TextColor"));
            var notesHelp = Text(T("olt_rulenotes_help", "Bottom left above the radar, one line per change for 6 s: which rule the team has reached and what it reads, e.g. \"Team 1  Rule 1 done > Rule 2: Go to the vehicle\". LTS and Capture with the classic controller; needs the script features on."), 12, "FaintTextBrush");
            notesHelp.Margin = new Thickness(0, 2, 0, 0);
            notesText.Children.Add(notesHelp);
            notes.Children.Add(notesText);
            var notesCard = Card("olt_card_notes", "Notes in the test", notes);
            foreach (var (k, f) in new[]
            {
                ("olt_keep_stats", "No XP, RP, stats or unlocks: a test writes nothing into your account."),
                ("olt_keep_teams", "Team minimum players are not checked; alone your team would be out at once."),
            })
            {
                var r = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
                var tick = Text("✓", 12.5, "OkBrush");
                tick.Margin = new Thickness(0, 0, 8, 0);
                DockPanel.SetDock(tick, Dock.Left);
                r.Children.Add(tick);
                r.Children.Add(Text(T(k, f), 12.5, "MutedTextBrush"));
                keep.Children.Add(r);
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var leftCol = new StackPanel();
            leftCol.Children.Add(Card("olt_card_switches", "What runs like online", list));
            var rightCol = new StackPanel();
            rightCol.Children.Add(Card("olt_card_players", "Simulate players", players));
            rightCol.Children.Add(Card("olt_card_keep", "Stays as in the test", keep));
            Grid.SetColumn(leftCol, 0);
            Grid.SetColumn(rightCol, 2);
            grid.Children.Add(leftCol);
            grid.Children.Add(rightCol);
            _body = grid;

            var page = new StackPanel();
            page.Children.Add(masterCard);
            page.Children.Add(healthCard);
            page.Children.Add(notesCard);
            page.Children.Add(grid);
            // Solo start and controller choice, moved here from General.
            page.Children.Add(new SoloTestCard());
            _page = page;
            Children.Add(page);
            Refresh();
        }

        private void Refresh()
        {
            if (!_built)
                return;
            bool creator = MainWindow.m.IsProcOpen && OnlineTest.InCreator();
            bool on = OnlineTest.Enabled;
            string controller = creator ? OnlineTest.RunningController() : null;
            bool classic = controller == "fm_mission_controller";

            _sync = true;
            try
            {
                _master.IsChecked = on;
                _invincible.IsChecked = OnlineTest.Invincible;
                _ruleNotes.IsChecked = OnlineTest.RuleNotesOn;
                bool mortal = OnlineTest.AppliedCount("mortal") > 0;
                _invincibleBadge.Text = mortal ? T("olt_badge_active", "active") : T("olt_badge_creator", "creator");
                _invincibleBadge.SetResourceReference(TextBlock.ForegroundProperty, mortal ? "OkBrush" : "FaintTextBrush");
                _body.IsEnabled = on;
                _body.Opacity = on ? 1 : 0.5;
                foreach (var entry in _rows)
                {
                    string feature = entry.Key;
                    var (toggle, badge, row) = entry.Value;
                    toggle.IsChecked = OnlineTest.IsOn(feature);
                    bool inController = feature != "phone";
                    // The 2020 and v3 controllers have no patches yet: their switches do nothing there.
                    row.Opacity = controller != null && !classic && inController ? 0.45 : 1;
                    int applied = OnlineTest.AppliedCount(feature);
                    badge.Text = applied > 0 ? T("olt_badge_active", "active")
                        : inController ? T("olt_badge_controller", "controller") : T("olt_badge_creator", "creator");
                    badge.SetResourceReference(TextBlock.ForegroundProperty, applied > 0 ? "OkBrush" : "FaintTextBrush");
                }
            }
            finally
            {
                _sync = false;
            }

            _page.Opacity = creator ? 1 : 0.6;
            _status.Children.Clear();
            if (!creator)
                Chip(T("olt_status_creator", "Only in the LTS and Capture creator"), "FaintTextBrush");
            else if (controller == null)
                Chip(T("olt_status_wait", "Waiting for a creator test"), "FaintTextBrush");
            else
            {
                Chip(string.Format(CultureInfo.CurrentCulture, T("olt_status_running", "Running: {0}"), controller), classic ? "OkBrush" : "WarnBrush");
                if (!classic)
                    Chip(T("olt_status_classic", "Only the classic controller has these changes so far"), "WarnBrush");
                else if (on)
                {
                    var switches = OnlineTest.Features.Where(f => f != "mortal").ToList();
                    int active = switches.Count(f => OnlineTest.IsOn(f) && OnlineTest.AppliedCount(f) > 0);
                    int wanted = switches.Count(OnlineTest.IsOn);
                    Chip(string.Format(CultureInfo.CurrentCulture, T("olt_status_count", "{0} of {1} active"), active, wanted), "OkBrush");
                }
            }
        }

        private void Chip(string text, string brush)
        {
            var t = Text("● " + text, 11.5, brush);
            var b = new Border { Child = t, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(9, 1, 9, 1), Margin = new Thickness(0, 0, 6, 0) };
            b.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            b.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            _status.Children.Add(b);
        }
    }
}
