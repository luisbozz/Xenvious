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
    /// the right. Moving, adding and deleting rules is not here yet (every pointer would have to be
    /// renumbered). Memory: Rules.
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(380) });

            var listHint = new TextBlock { Text = "pri / rule · endcon.txt", FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center };
            listHint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var listCard = Card(_listTitle, listHint, new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = _list });
            grid.Children.Add(listCard);

            _detailTeam.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var detailCard = Card(_detailTitle, _detailTeam, new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = _detail });
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
            => _team + "|" + _selected + "|" + Rules.PublicCreator + "|" + string.Join(";", _rules.Select(r =>
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
            row.MouseLeftButtonUp += (_, __) => { _selected = rule.Index; Reload(true); };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
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
                chips.Children.Add(DropChip(string.Format(CultureInfo.CurrentCulture, T("rl_drop_zone", "Drop-off zone {0}"), zone + 1),
                    T("rl_drop_zone_tip", "Drop-off zone of this rule. In the creator: the rule's Specify Drop-off Zones.")));
            if (rule.DropRadius > 0)
                chips.Children.Add(DropChip(string.Format(CultureInfo.CurrentCulture, T("rl_drop_point", "Drop-off point · {0} m"), rule.DropRadius.ToString("0.#", CultureInfo.CurrentCulture)),
                    T("rl_drop_point_tip", "Drop-off point of this rule and its radius.")));
            if (Rules.PublicCreator && rule.DropZones.Count == 0 && rule.Links.Any(Rules.IsDelivery))
                chips.Children.Add(Chip(T("rl_drop_none", "no drop-off zone"), ChipKind.Warn,
                    T("rl_drop_none_tip", "Collect & deliver needs a drop-off. In the creator: the rule's Specify Drop-off Zones.")));
            // An extra objective on the rule the entity already has does nothing new: the entity is
            // done with this rule once, so the extra one belongs on a later rule.
            foreach (var twice in rule.Links.Where(l => l.Extra && rule.Links.Any(o => !o.Extra && o.Kind == l.Kind && o.Index == l.Index)))
                chips.Children.Add(Chip(string.Format(CultureInfo.CurrentCulture, T("rl_extra_same", "{0}: extra objective on its own rule"), KindName(twice.Kind) + " " + (twice.Index + 1)),
                    ChipKind.Warn, T("rl_extra_same_tip", "The entity's extra objective sits on the same rule as its own objective. Give it a later rule, then the team gets it after this one.")));
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
            // A rule the team never reaches is greyed out; the note why stays readable.
            if (reach == RuleFlow.Reach.Never && !selected)
                foreach (var part in new UIElement[] { num, middle.Children[0], chips, side })
                    part.Opacity = 0.5;
            row.Child = grid;
            return Indented(row, reach, selected);
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

        private void ShowDetail()
        {
            _detail.Children.Clear();
            if (_selected < 0 || _selected >= _rules.Count)
            {
                _detailTitle.Text = "";
                _detailTeam.Text = "";
                return;
            }
            var rule = _rules[_selected];
            _detailTitle.Text = string.Format(CultureInfo.CurrentCulture, T("rl_rule_n", "Rule {0}"), rule.Index + 1);
            _detailTeam.Text = T("dash_team", "Team") + " " + (_team + 1);
            _loading = true;
            try
            {
                TextSection(rule);
                LinksSection(rule);
                NextSection(rule);
                LimitsSection(rule);
                OrderSection();
            }
            finally { _loading = false; }
        }

        private void Section(string text)
        {
            var t = new TextBlock { Text = text, FontSize = 12, Margin = new Thickness(0, _detail.Children.Count == 0 ? 0 : 14, 0, 6) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            _detail.Children.Add(t);
        }

        private void Hint(string text)
        {
            var t = Faint(text, 12);
            t.Margin = new Thickness(0, 4, 0, 0);
            _detail.Children.Add(t);
        }

        private static Border Field(UIElement child)
        {
            var b = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Padding = new Thickness(9, 6, 9, 6), Margin = new Thickness(0, 0, 0, 6), Child = child };
            b.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            b.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            return b;
        }

        private void TextSection(Rules.Rule rule)
        {
            Section(T("rl_text", "Objective text"));
            var box = new TextBox { Text = rule.Text ?? "", MaxLength = 63, BorderThickness = new Thickness(0), Background = Brushes.Transparent, Padding = new Thickness(0), FontSize = 13.5 };
            box.SetResourceReference(Control.ForegroundProperty, "TextColor");
            box.SetResourceReference(TextBoxBase.CaretBrushProperty, "TextColor");
            GtaTextAssist.Attach(box);
            // Codes count against the game's limit, so the length stays in view.
            var count = Faint("", 11.5);
            count.VerticalAlignment = VerticalAlignment.Center;
            count.Margin = new Thickness(8, 0, 0, 0);
            void Count() => count.Text = box.Text.Length + "/" + box.MaxLength;
            Count();
            box.TextChanged += (_, __) => Count();
            var line = new DockPanel();
            DockPanel.SetDock(count, Dock.Right);
            line.Children.Add(count);
            line.Children.Add(box);
            box.LostKeyboardFocus += (_, __) =>
            {
                if (_loading || !Live || box.Text == (rule.Text ?? "")) return;
                Rules.SetText(_team, rule.Index, box.Text);
                rule.Text = box.Text;
                _shownKey = null;
                Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
            };
            _detail.Children.Add(Field(line));
            Hint(string.IsNullOrWhiteSpace(rule.Text)
                ? T("rl_text_empty_hint", "Empty: the game shows its default text for this objective. Select text for colours, Ctrl+Space for icons and codes.")
                : T("rl_text_hint", "Select text for colours and font, Ctrl+Space for icons and codes. Saved when you leave the field."));
        }

        private void LinksSection(Rules.Rule rule)
        {
            string types = string.Join(", ", rule.Links.Select(TypeName).Distinct());
            Section(T("rl_links", "What points at this rule") + (types.Length > 0 ? " · " + types : ""));
            if (rule.Links.Count == 0)
            {
                Hint(T("rl_links_none", "Nothing points at this rule. The game reaches it only through a jump or the next-objective override, otherwise the team gets stuck here."));
                return;
            }
            foreach (var link in rule.Links)
            {
                var row = new DockPanel();
                var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                if (link.HasPass) right.Children.Add(JumpBox(link, true));
                if (link.HasFail) right.Children.Add(JumpBox(link, false));
                if (right.Children.Count == 0)
                {
                    var t = Faint(TypeName(link) + (link.Extra ? "  +" : ""), 12);
                    t.VerticalAlignment = VerticalAlignment.Center;
                    right.Children.Add(t);
                }
                DockPanel.SetDock(right, Dock.Right);
                row.Children.Add(right);
                var name = new TextBlock { Text = EntityName(link), FontSize = 13, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = EntityName(link) + " · " + TypeName(link) + (link.Extra ? " (" + T("rl_extra", "extra objective") + ")" : "") };
                name.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                row.Children.Add(name);
                _detail.Children.Add(Field(row));
            }
            if (rule.Links.Any(l => l.HasPass || l.HasFail))
                Hint(Rules.PublicCreator
                    ? T("rl_jump_hint_pmc", "✓ / ✗: where the team goes when this objective is passed or failed. The Mission Creator only jumps forward.")
                    : T("rl_jump_hint", "✓ / ✗: where the team goes when this objective is passed or failed."));
            foreach (int to in rule.Links.Where(l => Rules.FailJumpBlocked(l, _team, out _)).Select(l => l.FailJump).Distinct())
            {
                var warn = Faint("⚠ " + CriticalTip(rule, to), 12);
                warn.Margin = new Thickness(0, 6, 0, 0);
                warn.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                _detail.Children.Add(warn);
            }
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

        private void NextSection(Rules.Rule rule)
        {
            Section(T("rl_next", "Next objective"));
            var tiles = new WrapPanel();
            // The Mission Creator only accepts later rules, and none from rule 16 on.
            bool pmc = Rules.PublicCreator;
            for (int r = 0; r < _rules.Count; r++)
            {
                if (pmc && (r <= rule.Index || rule.Index >= 15))
                    continue;
                int bit = r;
                var tile = new ToggleButton { Style = (Style)FindResource("DashTeamButton"), Content = (r + 1).ToString(CultureInfo.CurrentCulture), FontSize = 13,
                    Margin = new Thickness(0, 0, 3, 3), IsChecked = (rule.NextRules & (1 << r)) != 0 };
                tile.Click += (_, __) =>
                {
                    if (!Live) return;
                    int v = Rules.GetField(GTA.Offsets.Editor.nxtrulb, _team, rule.Index);
                    v = tile.IsChecked == true ? v | (1 << bit) : v & ~(1 << bit);
                    Rules.SetField(GTA.Offsets.Editor.nxtrulb, _team, rule.Index, v);
                    _shownKey = null;
                    Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
                };
                tiles.Children.Add(tile);
            }
            if (tiles.Children.Count == 0)
            {
                Hint(T("rl_next_none", "No later rule to pick."));
                return;
            }
            var holder = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(3, 3, 0, 0), Child = tiles, HorizontalAlignment = HorizontalAlignment.Left };
            holder.SetResourceReference(Border.BackgroundProperty, "TextBoxBackground");
            _detail.Children.Add(holder);
            Hint(T("rl_next_hint", "Normally the team goes on with the next rule. Pick one rule to go there instead, or several to pick one at random."));
            if (rule.NextRules != 0 && rule.Links.Any(l => l.PassJump > rule.Index))
            {
                var warn = Faint(T("rl_flow_noeffect_tip", "A ✓ jump is set on this rule. In the game the jump wins, so the next objective does nothing."), 12);
                warn.Margin = new Thickness(0, 4, 0, 0);
                warn.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                _detail.Children.Add(warn);
            }
        }

        private void LimitsSection(Rules.Rule rule)
        {
            Section(T("rl_limits", "Objective limits"));
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            int row = 0;
            void Row(string key, string fallback, FrameworkElement editor, string tipKey = null, string tipFallback = null)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var label = new TextBlock { Text = T(key, fallback), FontSize = 13, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 6), TextWrapping = TextWrapping.Wrap };
                if (tipKey != null) label.ToolTip = T(tipKey, tipFallback);
                label.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                Grid.SetRow(label, row);
                grid.Children.Add(label);
                editor.Margin = new Thickness(0, 0, 0, 6);
                Grid.SetRow(editor, row);
                Grid.SetColumn(editor, 1);
                grid.Children.Add(editor);
                row++;
            }
            Row("rl_target", "Needed to pass (0 = all)", IntBox(GTA.Offsets.Editor.tsc, rule.Index, rule.TargetScore), "rl_target_hint", "How many of the objective's entities must be done, e.g. destroy 6 of 10.");
            Row("rl_points", "Points per objective", IntBox(GTA.Offsets.Editor.tms, rule.Index, rule.ObjectiveScore));
            Row("rl_takeover", "Capture / hack time (ms)", IntBox(GTA.Offsets.Editor.ttime, rule.Index, rule.TakeoverMs));

            var time = new ComboBox { Height = 28 };
            foreach (var (sel, sec) in Rules.TimeLimits)
                time.Items.Add(new ComboBoxItem { Content = sel == 0 ? T("rl_time_off", "No limit") : TimeLabel(sec), Tag = sel });
            time.SelectedItem = time.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == rule.TimeLimit);
            time.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(time.SelectedItem is ComboBoxItem item)) return;
                Rules.SetField(GTA.Offsets.Editor.tmt, _team, rule.Index, (int)item.Tag);
                _shownKey = null;
                Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
            };
            Row("rl_time", "Time limit", time);

            var fail = new CheckBox { Style = (Style)FindResource("FormToggle"), IsChecked = rule.FailsMission };
            fail.Click += (_, __) =>
            {
                if (!Live) return;
                Rules.SetFailsMission(_team, rule.Index, fail.IsChecked == true);
                _shownKey = null;
                Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
            };
            Row("rl_failmission", "Failing this rule ends the mission", fail);
            _detail.Children.Add(grid);
            Hint(T("rl_limits_hint", "A rule nothing completes on its own (no entities, no needed count) needs a time limit, or the team stays on it."));
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

        private void OrderSection()
        {
            Section(T("rl_order", "Order (later)"));
            var acts = new WrapPanel();
            foreach (var (key, fallback) in new[] { ("", "↑"), ("", "↓"), ("rl_insert", "Insert before"), ("rl_delete", "Delete") })
            {
                // Bordered like the mockup's buttons; disabled until reordering renumbers every pointer.
                var t = new TextBlock { Text = key.Length == 0 ? fallback : T(key, fallback), FontWeight = FontWeights.Bold, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
                t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                var b = new Border { Height = 30, Padding = new Thickness(11, 0, 11, 0), Margin = new Thickness(0, 0, 6, 6), CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
                    Child = t, Opacity = 0.45, ToolTip = T("rl_order_later", "Comes later: every jump and link has to be renumbered") };
                b.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                b.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                ToolTipService.SetShowOnDisabled(b, true);
                acts.Children.Add(b);
            }
            _detail.Children.Add(acts);
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

        private static string TimeLabel(int sec)
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
