using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// A team's rules as a list (mockup variant A: https://claude.ai/artifact/KQX7zDCkNu1m71utNs6dMF):
    /// every rule with its text, what points at it, jumps and limits; the selected rule's settings on
    /// the right. Rules move by their grip (RowDragger, RuleMover); adding and deleting rules is not
    /// here yet. Memory: Rules.
    /// </summary>
    public partial class RulesView : DockPanel
    {
        private int _team, _selected;
        private bool _built, _loading;
        private string _shownKey;
        private List<Rules.Rule> _rules = new List<Rules.Rule>();

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        private readonly StackPanel _teamButtons = new StackPanel { Orientation = Orientation.Horizontal };
        private readonly TextBlock _meta = new TextBlock { FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock _listTitle = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 14 };
        private readonly TextBlock _detailTitle = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 14 };
        private readonly TextBlock _detailTeam = new TextBlock { FontSize = 12 };
        private readonly StackPanel _list = new StackPanel { Margin = new Thickness(12) };
        private readonly StackPanel _detail = new StackPanel { Margin = new Thickness(14, 12, 14, 14) };

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public RulesView()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Reload(true); _timer.Start(); }
                else _timer.Stop();
            };
            // Picks up changes made in the creator; skipped while something here has the keyboard.
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Reload(false); };
        }

        private bool Live => _built && Rules.Ready;

        /// <summary>Shows a team's rule selected and scrolled into view (from an entity page).</summary>
        public void ShowRule(int team, int rule)
        {
            _team = Math.Max(0, team);
            _selected = Math.Max(0, rule);
            _cp = -1;
            _shownKey = null;
            if (_built)
                Reload(true);
            // After the page is shown and laid out; before that the row has no place to scroll to.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_selected < _parts.Count)
                    _parts[_selected].Row.BringIntoView();
            }), DispatcherPriority.Loaded);
        }

        // ----- building -----

        private void Build()
        {
            if (_built)
                return;
            _built = true;

            // Top bar: team switch like the dashboard's, counts on the right.
            var teamPanel = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(3), HorizontalAlignment = HorizontalAlignment.Left };
            teamPanel.SetResourceReference(Border.BackgroundProperty, "TextBoxBackground");
            var teamRow = new StackPanel { Orientation = Orientation.Horizontal };
            var teamLabel = new TextBlock { Text = T("dash_team", "Team"), FontSize = 11, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 6, 0) };
            teamLabel.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
            teamRow.Children.Add(teamLabel);
            teamRow.Children.Add(_teamButtons);
            teamPanel.Child = teamRow;

            var bar = new DockPanel();
            _meta.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            _meta.Margin = new Thickness(12, 0, 0, 0);
            DockPanel.SetDock(_meta, Dock.Right);
            bar.Children.Add(_meta);
            bar.Children.Add(teamPanel);
            var top = Panel(bar, new Thickness(10, 8, 14, 8));
            top.Margin = new Thickness(0, 12, 0, 14);
            DockPanel.SetDock(top, Dock.Top);
            Children.Add(top);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            // The detail column grows with the window: its boxes need room for labels and editors.
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 430, MaxWidth = 640 });

            var listHint = new TextBlock { Text = "pri / rule · endcon.txt", FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center };
            listHint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var listCard = Card(_listTitle, listHint, new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = _list });
            grid.Children.Add(listCard);

            _detailTeam.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var detailCard = Card(_detailTitle, DetailHeaderRight(), new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = _detail });
            detailCard.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(detailCard, 2);
            grid.Children.Add(detailCard);
            Children.Add(grid);
            Reload(true);
        }

        private static Border Panel(UIElement child, Thickness padding)
        {
            var b = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Padding = padding, Child = child };
            b.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            b.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            return b;
        }

        private static Border Card(TextBlock title, UIElement right, UIElement body)
        {
            var head = new DockPanel();
            if (right != null)
            {
                DockPanel.SetDock(right, Dock.Right);
                head.Children.Add(right);
            }
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            head.Children.Add(title);
            var headBorder = new Border { Padding = new Thickness(14, 10, 14, 10), BorderThickness = new Thickness(0, 0, 0, 1), Child = head };
            headBorder.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            var dock = new DockPanel();
            DockPanel.SetDock(headBorder, Dock.Top);
            dock.Children.Add(headBorder);
            dock.Children.Add(body);
            return Panel(dock, new Thickness(0));
        }

        private void BuildTeamButtons(int teams)
        {
            if (_teamButtons.Children.Count != teams)
            {
                _teamButtons.Children.Clear();
                for (int i = 0; i < teams; i++)
                {
                    int n = i;
                    var b = new ToggleButton { Content = (i + 1).ToString(CultureInfo.InvariantCulture), Style = (Style)FindResource("DashTeamButton") };
                    b.Click += (_, __) => { _team = n; _selected = 0; Reload(true); };
                    _teamButtons.Children.Add(b);
                }
            }
            for (int i = 0; i < _teamButtons.Children.Count; i++)
                ((ToggleButton)_teamButtons.Children[i]).IsChecked = i == _team;
        }

        // ----- loading -----

        private void Reload(bool force)
        {
            if (!_built)
                return;
            if (!Live)
            {
                _list.Children.Clear();
                _detail.Children.Clear();
                _list.Children.Add(Faint(T("rl_nocreator", "Open a mission (LTS, Capture or Mission Creator) to see its rules."), 13.5));
                _listTitle.Text = T("rl_page", "Rules");
                _detailTitle.Text = "";
                _meta.Text = "";
                _shownKey = null;
                return;
            }
            int teams = Rules.Teams();
            if (_team >= teams) _team = 0;
            BuildTeamButtons(teams);

            _rules = Rules.Read(_team);
            if (_selected >= _rules.Count) _selected = Math.Max(0, _rules.Count - 1);
            LoadCheckpoints();
            string key = Signature();
            if (!force && key == _shownKey)
                return;
            _shownKey = key;

            ShowMeta();
            _listTitle.Text = string.Format(CultureInfo.CurrentCulture, T("rl_in_order", "Team {0} · rules in order"), _team + 1);
            _list.Children.Clear();
            if (_rules.Count == 0)
                _list.Children.Add(Faint(T("rl_none", "This team has no rules yet. Place entities with an objective in the creator."), 13.5));
            else
                ShowFlow();
            ShowDetail();
        }

        private void ShowMeta()
        {
            _meta.Inlines.Clear();
            void Part(int n, string key, string fallback)
            {
                if (_meta.Inlines.Count > 0)
                    _meta.Inlines.Add(new Run(" · "));
                _meta.Inlines.Add(new Run(n.ToString(CultureInfo.CurrentCulture)) { FontWeight = FontWeights.Bold }.Also(r => r.SetResourceReference(TextElement.ForegroundProperty, "TextColor")));
                _meta.Inlines.Add(new Run(" " + T(key, fallback)));
            }
            var links = _rules.SelectMany(r => r.Links).Where(l => !l.Extra).ToList();
            Part(_rules.Count, "rl_m_rules", "rules");
            Part(links.Where(l => l.Kind == Rules.Kind.Ped).Select(l => l.Index).Distinct().Count(), "rl_m_peds", "actors");
            Part(links.Where(l => l.Kind == Rules.Kind.Vehicle).Select(l => l.Index).Distinct().Count(), "rl_m_vehs", "vehicles");
            Part(links.Where(l => l.Kind == Rules.Kind.Object).Select(l => l.Index).Distinct().Count(), "rl_m_objs", "objects");
            Part(links.Where(l => l.Kind == Rules.Kind.GoTo).Select(l => l.Index).Distinct().Count(), "rl_m_locs", "locations");
        }

        private string Signature()
            => _team + "|" + _selected + "|" + _cp + "|" + Rules.PublicCreator + "|" + RestartCheckpoints.Signature(_team, _rules.Count) + "|" + string.Join(";", _rules.Select(r =>
                $"{r.Text}/{r.NextRules}/{r.TargetScore}/{r.ObjectiveScore}/{r.TakeoverMs}/{r.TimeLimit}/{r.FailsMission}/{string.Join(".", r.DropZones)}/{r.DropRadius}/" +
                string.Join(",", r.Links.Select(l => $"{l.Kind}{l.Index}:{l.Type}:{l.Extra}:{l.PassJump}:{l.FailJump}:{Rules.FailJumpBlocked(l, _team, out _)}"))));

        // ----- the list -----

        private FrameworkElement RuleRow(Rules.Rule rule, out FrameworkElement numOut)
        {
            bool selected = rule.Index == _selected;
            var reach = _flow.Reached[rule.Index];
            var row = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(selected ? 2 : 1), Padding = new Thickness(selected ? 11 : 12, selected ? 9 : 10, 12, 10),
                Cursor = System.Windows.Input.Cursors.Hand };
            row.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            row.SetResourceReference(Border.BorderBrushProperty, selected ? "AccentBrush" : "LineBrush");
            row.MouseLeftButtonUp += (_, __) => { _selected = rule.Index; _cp = -1; _joinError = null; Reload(true); };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var num = new Border { Width = 36, Height = 36, CornerRadius = new CornerRadius(18), BorderThickness = new Thickness(1), VerticalAlignment = VerticalAlignment.Top };
            num.SetResourceReference(Border.BackgroundProperty, selected ? "AccentBrush" : "DeepBrush");
            num.SetResourceReference(Border.BorderBrushProperty, selected ? "AccentBrush" : "LineBrush");
            var numText = new TextBlock { Text = (rule.Index + 1).ToString(CultureInfo.CurrentCulture), FontWeight = FontWeights.Bold, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (selected) numText.Foreground = Brushes.White;
            else numText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            num.Child = numText;
            if (reach == RuleFlow.Reach.Branch && !selected)
            {
                num.SetResourceReference(Border.BorderBrushProperty, "BadBrush");
                numText.SetResourceReference(TextBlock.ForegroundProperty, "BadBrush");
            }
            grid.Children.Add(num);
            numOut = num;

            var middle = new StackPanel { Margin = new Thickness(0, 1, 0, 0) };
            middle.Children.Add(ObjectiveText(rule.Text, 14, Rules.DefaultTextFor(_team, rule)));
            var chips = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
            if (rule.Links.Count == 0)
                chips.Children.Add(Chip(T("rl_empty", "nothing points at this rule"), ChipKind.Warn));
            foreach (var group in rule.Links.GroupBy(l => TypeName(l)))
            {
                chips.Children.Add(Chip(group.Key, ChipKind.Type));
                foreach (var kind in group.GroupBy(l => l.Kind))
                {
                    var ids = kind.Select(l => l.Index + 1).Distinct().ToList();
                    string list = ids.Count > 4 ? ids.Count.ToString(CultureInfo.CurrentCulture) + "×" : string.Join(", ", ids);
                    bool extra = kind.All(l => l.Extra);
                    chips.Children.Add(Chip(KindName(kind.Key) + " " + list + (extra ? "  +" : ""), ChipKind.Plain,
                        extra ? T("rl_extra_tip", "Extra objective: this entity has another rule as well") : null));
                }
            }
            foreach (int zone in rule.DropZones)
                chips.Children.Add(DropChip(string.Format(CultureInfo.CurrentCulture, T("rl_drop_zone", "Drop-off zone {0}"), zone),
                    T("rl_drop_zone_tip", "Drop-off zone of this rule. In the creator: the rule's Specify Drop-off Zones.")));
            if (rule.DropRadius > 0)
                chips.Children.Add(DropChip(string.Format(CultureInfo.CurrentCulture, T("rl_drop_point", "Drop-off point · {0} m"), rule.DropRadius.ToString("0.#", CultureInfo.CurrentCulture)),
                    T("rl_drop_point_tip", "Drop-off point of this rule and its radius.")));
            // Text messages this rule sends; the tooltip shows their texts.
            var sms = Sms.ForRule(_team, rule.Index);
            if (sms.Count > 0)
                chips.Children.Add(Chip("✉ " + string.Format(CultureInfo.CurrentCulture, T("rl_sms_chip", "SMS {0}"), string.Join(", ", sms.Select(i => i + 1))), ChipKind.Plain,
                    string.Join("\n", sms.Select(i => (i + 1).ToString(CultureInfo.CurrentCulture) + ": " + Sms.Text(i)))));
            if (Rules.PublicCreator && rule.DropZones.Count == 0 && rule.Links.Any(Rules.IsDelivery))
                chips.Children.Add(Chip(T("rl_drop_none", "no drop-off zone"), ChipKind.Warn,
                    T("rl_drop_none_tip", "Collect & deliver needs a drop-off. In the creator: the rule's Specify Drop-off Zones.")));
            // An extra objective on the rule the entity already has does nothing new: the entity is
            // done with this rule once, so the extra one belongs on a later rule.
            foreach (var twice in rule.Links.Where(l => l.Extra && rule.Links.Any(o => !o.Extra && o.Kind == l.Kind && o.Index == l.Index)))
                chips.Children.Add(Chip(string.Format(CultureInfo.CurrentCulture, T("rl_extra_same", "{0}: extra objective on its own rule"), KindName(twice.Kind) + " " + (twice.Index + 1)),
                    ChipKind.Warn, T("rl_extra_same_tip", "The entity's extra objective sits on the same rule as its own objective. Give it a later rule, then the team gets it after this one.")));
            AddRestartChips(chips, rule.Index);
            middle.Children.Add(chips);
            var flowChips = FlowChips(rule.Index);
            if (flowChips != null)
                middle.Children.Add(flowChips);
            Grid.SetColumn(middle, 1);
            grid.Children.Add(middle);

            var side = new StackPanel { Margin = new Thickness(12, 2, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };
            void Side(string text, string brush = "FaintTextBrush")
            {
                var t = new TextBlock { Text = text, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 0, 3) };
                t.SetResourceReference(TextBlock.ForegroundProperty, brush);
                side.Children.Add(t);
            }
            if (rule.TargetScore > 0)
                Side(string.Format(CultureInfo.CurrentCulture, T("rl_chip_target", "{0} needed"), rule.TargetScore));
            if (rule.TimeLimit != 0)
                Side("⏱ " + TimeText(rule.TimeLimit));
            if (rule.FailsMission)
                Side(T("rl_chip_fail", "fail = mission over"), "BadBrush");
            Grid.SetColumn(side, 2);
            grid.Children.Add(side);
            if (_dragger != null)
            {
                var grip = _dragger.Grip(rule.Index, RuleMover.Available, MoveTip());
                grip.Margin = new Thickness(8, 0, -4, 0);
                grip.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetColumn(grip, 3);
                grid.Children.Add(grip);
            }
            // A rule the team never reaches is greyed out; the note why stays readable.
            if (reach == RuleFlow.Reach.Never && !selected)
                foreach (var part in new UIElement[] { num, middle.Children[0], chips, side })
                    part.Opacity = 0.5;
            row.Child = grid;
            return WithCheckpointLines(Indented(row, reach, selected), rule.Index);
        }

        /// <summary>What the move grip says: how to use it, or why it is greyed out.</summary>
        public static string MoveTip()
        {
            if (RuleMover.Available)
                return T("rl_move_tip", "Drag to move the rule. Everything that points at it moves along: entities, player rules, extra objectives, jumps and the rule's settings.");
            if (Rules.Ready && Rules.PublicCreator)
                return T("rl_move_pmc", "In the Mission Creator the creator moves rules itself (its rule list, Move Rule).");
            return T("rl_move_na", "Moving rules works in the LTS and Capture Creator.");
        }

        /// <summary>
        /// Moves a rule of the shown team and rebuilds the creator so it shows the new order. The
        /// game data changes in many places; the selection follows the moved rule.
        /// </summary>
        private void MoveRule(int from, int to)
        {
            int team = _team;
            _selected = to;
            System.Threading.Tasks.Task.Run(() =>
            {
                string error = null;
                try { RuleMover.Move(team, from, to); }
                catch (Exception ex) { error = ex.Message; Xenvious.Logging.Log.Error("move rule: " + ex); }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (error != null)
                        MainWindow.Instance?.displayScreenMessage(error);
                    MainWindow.Instance?.creatorRefresh();
                    Reload(true);
                }));
            });
        }

        private enum ChipKind { Plain, Type, Warn }

        private static Border Chip(string text, ChipKind kind, string tip = null)
        {
            var chip = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 1, 7, 2), Margin = new Thickness(0, 0, 5, 5), BorderThickness = new Thickness(1), ToolTip = tip };
            var t = new TextBlock { Text = text, FontSize = 11.5, FontWeight = FontWeights.Bold };
            if (kind == ChipKind.Warn)
            {
                chip.Background = new SolidColorBrush(Color.FromArgb(0x2E, 0xFA, 0xA6, 0x1A));
                chip.BorderBrush = Brushes.Transparent;
                t.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
            }
            else
            {
                chip.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                chip.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                t.SetResourceReference(TextBlock.ForegroundProperty, kind == ChipKind.Type ? "TextColor" : "MutedTextBrush");
            }
            chip.Child = t;
            return chip;
        }

        /// <summary>A drop-off as a pin chip, yellow like the game's drop-off marker text.</summary>
        private static Border DropChip(string text, string tip)
        {
            var chip = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(6, 1, 7, 2), Margin = new Thickness(0, 0, 5, 5), BorderThickness = new Thickness(1), ToolTip = tip };
            chip.SetResourceReference(Border.BorderBrushProperty, "WarnBrush");
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var pin = new Path { Data = Geometry.Parse("M6,11 C6,11 10,7.4 10,4.5 A4,4 0 0 0 2,4.5 C2,7.4 6,11 6,11 Z M6,3.2 A1.3,1.3 0 1 1 6,5.8 A1.3,1.3 0 1 1 6,3.2 Z"),
                StrokeThickness = 1.6, Width = 10, Height = 11, Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) };
            pin.SetResourceReference(Shape.StrokeProperty, "WarnBrush");
            var t = new TextBlock { Text = text, FontSize = 11.5, FontWeight = FontWeights.Bold };
            t.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
            row.Children.Add(pin);
            row.Children.Add(t);
            chip.Child = row;
            return chip;
        }

        private static TextBlock Faint(string text, double size)
        {
            var t = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = size };
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return t;
        }

        /// <summary>The objective text with the game's colour codes, or a note that the default text is shown.</summary>
        /// <summary>
        /// The objective text with the game's codes. Without own text the game's default text for
        /// the rule type is shown (defaultText, marked as such); when that cannot be told, a note.
        /// </summary>
        internal static TextBlock ObjectiveText(string text, double size, string defaultText = null)
        {
            var block = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = size, FontWeight = FontWeights.Bold };
            bool own = !string.IsNullOrWhiteSpace(text);
            if (!own && defaultText == null)
            {
                block.Text = T("rl_default_unknown", "No own text; the game's default text for this rule was not found");
                block.FontStyle = FontStyles.Italic;
                block.FontWeight = FontWeights.SemiBold;
                block.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                return block;
            }
            block.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            AddCodes(block, own ? text : defaultText, size);
            if (!own)
            {
                var mark = new Run("  " + T("rl_default_mark", "default text")) { FontSize = size - 2.5, FontWeight = FontWeights.SemiBold, FontStyle = FontStyles.Italic };
                mark.SetResourceReference(TextElement.ForegroundProperty, "FaintTextBrush");
                block.Inlines.Add(mark);
            }
            return block;
        }

        private static void AddCodes(TextBlock block, string text, double size)
        {
            Color? colour = null;
            bool italic = false;
            foreach (var part in Regex.Split(text, "(~[A-Za-z_0-9]+~)"))
            {
                if (part.Length == 0)
                    continue;
                if (Regex.IsMatch(part, "^~[A-Za-z_0-9]+~$"))
                {
                    string code = part.ToLowerInvariant();
                    if (code == "~n~") block.Inlines.Add(new LineBreak());
                    else if (code == "~italic~") italic = !italic;
                    else if (code == "~ws~") block.Inlines.Add(Icon(code, size, colour));
                    // The list shows every text bold already, so ~bold~ and ~h~ change nothing here.
                    else if (code.Length == 3) colour = GtaTextAssist.ColourOf(code);
                    continue;
                }
                foreach (var piece in Regex.Split(part, "([¦‹›∑Ω])"))
                    if (piece.Length > 0)
                        block.Inlines.Add(GtaTextAssist.IsIcon(piece) ? Icon(piece, size, colour) : Styled(piece, colour, italic));
            }
        }

        private static Inline Icon(string code, double size, Color? colour)
        {
            var icon = GtaTextAssist.IconImage(code, size + 4, null);
            if (colour.HasValue) ((Shape)icon).Fill = new SolidColorBrush(colour.Value);
            else icon.SetResourceReference(Shape.FillProperty, "TextColor");
            return new InlineUIContainer(icon) { BaselineAlignment = BaselineAlignment.Center };
        }

        private static Run Styled(string text, Color? colour, bool italic)
        {
            var run = new Run(text);
            if (colour.HasValue)
                run.Foreground = new SolidColorBrush(colour.Value);
            if (italic)
                run.FontStyle = FontStyles.Italic;
            return run;
        }

        // ----- the selected rule -----

        // Shown under "What points at this rule" while the "?" is on.

        private static int EoType(Rules.Kind kind)
        {
            switch (kind)
            {
                case Rules.Kind.Ped: return ExtraObjectives.TypePed;
                case Rules.Kind.Vehicle: return ExtraObjectives.TypeVehicle;
                case Rules.Kind.Object: return ExtraObjectives.TypeObject;
                case Rules.Kind.GoTo: return ExtraObjectives.TypeGoTo;
                default: return 0;
            }
        }

        private static FrameworkElement ExtraChip()
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center, ToolTip = T("rl_extra_tip", "Extra objective: this entity has another rule as well") };
            var frame = new Rectangle { RadiusX = 5, RadiusY = 5, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 2 } };
            frame.SetResourceReference(Shape.StrokeProperty, "AccentBrush");
            grid.Children.Add(frame);
            var t = new TextBlock { Text = T("er_extra_short", "Extra"), FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(6, 1, 6, 2) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            grid.Children.Add(t);
            return grid;
        }

        private Button IconButton(string geometry, string tip, Action click)
        {
            var icon = new Path { Data = Geometry.Parse(geometry), StrokeThickness = 1.5, Width = 11, Height = 11, Stretch = Stretch.Uniform,
                StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            icon.SetResourceReference(Shape.StrokeProperty, "MutedTextBrush");
            var b = new Button { Style = (Style)FindResource("FieldIconButton"), Width = 24, Height = 24, Margin = new Thickness(4, 0, 0, 0), Content = icon, ToolTip = tip, VerticalAlignment = VerticalAlignment.Center };
            b.Click += (_, __) => click();
            return b;
        }

        // Other creators: an extra objective's own type, stored with it (eoir).
        private FrameworkElement ExtraTypeBox(Rules.Rule rule, Rules.Link link, int type)
        {
            var box = new ComboBox { Height = 26, MinWidth = 100, FontSize = 12, Margin = new Thickness(8, 0, 0, 0) };
            foreach (var r in ExtraObjectives.RuleTypesFor(type))
            {
                var item = new ComboBoxItem { Content = T("eo_r_" + r.Value, r.Name), Tag = r.Value };
                box.Items.Add(item);
                if (r.Value == link.Type)
                    box.SelectedItem = item;
            }
            box.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(box.SelectedItem is ComboBoxItem item))
                    return;
                var entry = EntityRules.Read(type, link.Index, _team).FirstOrDefault(e => !e.Main && e.Rule == rule.Index);
                if (entry != null)
                    ExtraObjectives.SetRule(entry.Slot, entry.Place, _team, (int)item.Tag, rule.Index);
                Changed(null);
            };
            return box;
        }

        private void LeaveRule(Rules.Rule rule, Rules.Link link, int type)
        {
            if (!Live)
                return;
            if (EntityRules.UsesRuleList)
            {
                Changed(EntityRules.SetMember(type, link.Index, _team, rule.Index, false));
                return;
            }
            var entry = EntityRules.Read(type, link.Index, _team).FirstOrDefault(e => !e.Main && e.Rule == rule.Index);
            if (entry != null)
                ExtraObjectives.Remove(entry.Slot, entry.Place, _team);
            Changed(null);
        }

        private string _joinError;

        private void Changed(string error)
        {
            _joinError = error;
            _shownKey = null;
            Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
        }

        private FrameworkElement JumpBox(Rules.Link link, bool pass)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(8, 0, 0, 0) };
            var mark = new TextBlock { Text = pass ? "✓" : "✗", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 3, 0) };
            mark.SetResourceReference(TextBlock.ForegroundProperty, pass ? "OkBrush" : "BadBrush");
            panel.Children.Add(mark);
            var combo = new ComboBox { Width = 60, Height = 26, FontSize = 12 };
            combo.Items.Add(new ComboBoxItem { Content = "–", Tag = -1 });
            for (int r = 0; r < _rules.Count; r++)
            {
                if (Rules.PublicCreator && r <= _selected)
                    continue;
                combo.Items.Add(new ComboBoxItem { Content = (r + 1).ToString(CultureInfo.CurrentCulture), Tag = r });
            }
            int current = pass ? link.PassJump : link.FailJump;
            combo.SelectedItem = combo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == current) ?? combo.Items[0];
            combo.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(combo.SelectedItem is ComboBoxItem item))
                    return;
                Rules.SetJump(link, _team, pass, (int)item.Tag);
                _shownKey = null;
                Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
            };
            panel.Children.Add(combo);
            return panel;
        }

        private FrameworkElement IntBox(long offset, int rule, int value)
        {
            var box = new TextBox { Height = 28, Text = value.ToString(CultureInfo.InvariantCulture), IsEnabled = offset != 0 };
            box.SetResourceReference(StyleProperty, "Watermark");
            box.TextChanged += (_, __) =>
            {
                if (!_loading && Live && int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    Rules.SetField(offset, _team, rule, v);
                    _shownKey = null;
                }
            };
            return box;
        }

        // ----- names -----

        private static string KindName(Rules.Kind kind)
        {
            switch (kind)
            {
                case Rules.Kind.Ped: return T("eo_k_ped", "Actor");
                case Rules.Kind.Vehicle: return T("eo_k_veh", "Vehicle");
                case Rules.Kind.Object: return T("eo_k_obj", "Object");
                case Rules.Kind.GoTo: return T("rl_k_loc", "Location");
                default: return T("rl_k_player", "Player rule");
            }
        }

        private static string EntityName(Rules.Link link)
        {
            int type = link.Kind == Rules.Kind.Ped ? EntityPicker.Actor : link.Kind == Rules.Kind.Vehicle ? EntityPicker.Vehicle
                : link.Kind == Rules.Kind.Object ? EntityPicker.Object : 0;
            string label = type == 0 ? "#" + (link.Index + 1).ToString(CultureInfo.CurrentCulture) : EntityPicker.Label(type, link.Index);
            return KindName(link.Kind) + " " + label;
        }

        /// <summary>The rule type as the creator names it for this kind of entity (a vehicle is destroyed, not killed).</summary>
        private static string TypeName(Rules.Link link) => Rules.TypeName(link);

        internal static string TimeLabel(int sec)
            => sec >= 60 ? TimeSpan.FromSeconds(sec).ToString(@"m\:ss", CultureInfo.InvariantCulture) + " min" : sec.ToString(CultureInfo.CurrentCulture) + " s";

        private static string TimeText(int selection) => TimeLabel(Rules.TimeLimits.FirstOrDefault(t => t.Selection == selection).Seconds);

        private static IEnumerable<int> Bits(int value)
        {
            for (int b = 0; b < 32; b++)
                if ((value & (1 << b)) != 0)
                    yield return b;
        }
    }

    internal static class RulesViewExtensions
    {
        public static T Also<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }
    }
}
