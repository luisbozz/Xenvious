using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of RulesView: the detail column of the selected rule. One box per question (what the
    // team reads, what it has to do, when the rule ends, where it goes next), then the Mission
    // Creator's further options in groups. "? Help" shows the creator's own help text (PMCH_*)
    // under every option.
    public partial class RulesView
    {
        private static bool _help;
        private static readonly HashSet<string> _openGroups = new HashSet<string>();
        private ToggleButton _helpButton;
        private Button _deleteButton;
        private bool _addAsNewRule;

        private FrameworkElement DetailHeaderRight()
        {
            // DashTeamButton has the team buttons' fixed 28 px width and ignores Padding.
            _helpButton = new ToggleButton { Style = (Style)FindResource("DashTeamButton"), Width = double.NaN, Height = 24,
                Content = new TextBlock { Text = "? " + T("rl_help", "Help"), Margin = new Thickness(8, 0, 8, 0) },
                FontSize = 12, IsChecked = _help, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center,
                ToolTip = T("rl_help_tip", "Shows the Mission Creator's help text under every option.") };
            _helpButton.Click += (_, __) => { _help = _helpButton.IsChecked == true; ShowDetail(); };
            _deleteButton = IconButton("M1,3 H11 M4.5,3 V1 H7.5 V3 M2.5,3 L3.2,11 H8.8 L9.5,3", T("rl_delete_tip", "Delete this rule"), DeleteRule);
            _deleteButton.Margin = new Thickness(6, 0, 0, 0);
            ToolTipService.SetShowOnDisabled(_deleteButton, true);
            var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            _detailTeam.VerticalAlignment = VerticalAlignment.Center;
            right.Children.Add(_detailTeam);
            right.Children.Add(_helpButton);
            right.Children.Add(_deleteButton);
            return right;
        }

        private void UpdateDeleteButton()
        {
            if (_deleteButton == null)
                return;
            bool can = RuleMover.Available && _selected >= 0 && _selected < _rules.Count && _rules.Count > 1;
            _deleteButton.Visibility = Visibility.Visible;
            _deleteButton.IsEnabled = can;
            _deleteButton.Opacity = can ? 1 : 0.4;
            _deleteButton.ToolTip = can ? T("rl_delete_tip", "Delete this rule")
                : !RuleMover.Available ? T("rl_delete_na", "Only in LTS and Capture. In the Mission Creator delete rules in its own rules menu.")
                : T("rl_delete_last", "A team keeps at least one rule.");
        }

        /// <summary>
        /// Deletes the selected rule after asking: later rules move up by one, its entities lose
        /// this rule and jumps to it become "no jump". Then the creator rebuilds.
        /// </summary>
        private async void DeleteRule()
        {
            if (_selected < 0 || _selected >= _rules.Count || MainWindow.Instance == null)
                return;
            int team = _team, rule = _selected;
            if (!await MainWindow.Instance.ConfirmAsync(T("rl_delete", "Delete"),
                string.Format(CultureInfo.CurrentCulture, T("rl_delete_text", "Delete rule {0}? Its entities leave it, later rules move up by one and jumps to it become \"no jump\"."), rule + 1),
                T("rl_delete", "Delete"), T("dialog_cancel", "Cancel"), danger: true))
                return;
            _selected = Math.Max(0, rule - 1);
            string notAvailable = T("rl_delete_na", "Only in LTS and Capture. In the Mission Creator delete rules in its own rules menu.");
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                string error = null;
                try
                {
                    if (!RuleMover.Delete(team, rule))
                        error = notAvailable;
                }
                catch (Exception ex) { error = ex.Message; Xenvious.Logging.Log.Error("delete rule: " + ex); }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (error != null)
                        MainWindow.Instance?.displayScreenMessage(error);
                    MainWindow.Instance?.creatorRefresh();
                    Reload(true);
                }));
            });
        }

        private void ShowDetail()
        {
            _detail.Children.Clear();
            UpdateDeleteButton();
            if (_selected < 0 || _selected >= _rules.Count)
            {
                _detailTitle.Text = "";
                _detailTeam.Text = "";
                return;
            }
            if (_cp >= 0 && RestartCheckpoints.Available)
            {
                _loading = true;
                try { ShowCheckpointDetail(); }
                finally { _loading = false; }
                return;
            }
            var rule = _rules[_selected];
            _detailTitle.Text = string.Format(CultureInfo.CurrentCulture, T("rl_rule_of", "Rule {0} of {1}"), rule.Index + 1, _rules.Count);
            _detailTeam.Text = T("dash_team", "Team") + " " + (_team + 1);
            _loading = true;
            try
            {
                _detail.Children.Add(StatusRow(rule));
                _detail.Children.Add(Box("T", T("rl_b_text", "What the team reads"), null, TextBody(rule)));
                _detail.Children.Add(Box("✔", T("rl_b_tasks", "What the team has to do"),
                    rule.Links.Count > 1 ? Faint(T("rl_b_tasks_same", "all at the same time"), 12) : null, TasksBody(rule)));
                _detail.Children.Add(Box("⏱", T("rl_b_end", "When the rule ends"), null, EndBody(rule)));
                _detail.Children.Add(Box("→", T("rl_b_next", "Where it goes next"), null, NextBody(rule)));
                if (Sms.Ready)
                    _detail.Children.Add(Group("sms", "✉", T("rl_g_sms", "Text messages"), SmsSummary(rule), () => SmsBody(rule)));
                if (Rules.Teams() > 1 && RuleOptions.OtherTeamsAvailable)
                    _detail.Children.Add(Group("teams", "⇄", RuleOptions.GameText("PMCT_OJT_OPL", "Other Team Progress Limit"), OtherTeamsSummary(rule), () => OtherTeamsBody(rule)));
                if (RuleOptions.FlagsAvailable)
                    _detail.Children.Add(Group("flags", "◆", RuleOptions.GameText("PMCT_OMF", "Mission Flags"), FlagsSummary(rule), () => FlagsBody(rule)));
                if (RuleOptions.BoundsAvailable)
                    _detail.Children.Add(Group("bounds", "▢", RuleOptions.GameText("PMCT_BNDS", "Bounds"), BoundsSummary(rule), () => BoundsBody(rule)));
                if (RuleOptions.WantedAvailable)
                    _detail.Children.Add(Group("wanted", "★", T("rl_g_wanted", "Wanted level"), WantedSummary(rule), () => WantedBody(rule)));
                foreach (var g in RuleOptions.MoreGroups.Skip(1))
                {
                    var group = g;
                    _detail.Children.Add(Group(group.Key, group.Key == "rl_g_chase" ? "⚑" : "◫", T(group.Key, group.Fallback), OptionsSummary(group.Options, rule),
                        () => OptionsBody(group.Options, rule, group.HelpLabel)));
                }
            }
            finally { _loading = false; }
        }

        // ----- text messages -----

        private string SmsSummary(Rules.Rule rule)
        {
            int n = Sms.ForRule(_team, rule.Index).Count;
            return n == 0 ? T("rl_sms_none", "none") : string.Format(CultureInfo.CurrentCulture, T("rl_sms_count", "{0} SMS"), n);
        }

        // The messages this rule sends, each with its text and when it arrives; open one on the
        // SMS page or add a new one for this rule.
        private FrameworkElement SmsBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            var list = Sms.ForRule(_team, rule.Index);
            if (list.Count == 0)
                panel.Children.Add(Say(T("rl_sms_empty", "This rule sends no text message.")));
            foreach (int i in list)
            {
                var card = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Padding = new Thickness(10, 8, 8, 8), Margin = new Thickness(0, 0, 0, 6) };
                card.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                var dock = new DockPanel();
                int slot = i;
                var open = IconButton("M5,3 H1 V11 H9 V7 M7,1 H11 V5 M11,1 L5,7", T("rl_sms_open", "Open on the SMS page"), () => MainWindow.Instance?.OpenSms(slot));
                open.VerticalAlignment = VerticalAlignment.Top;
                DockPanel.SetDock(open, Dock.Right);
                dock.Children.Add(open);
                var stack = new StackPanel();
                string text = Sms.Text(i);
                var msg = new TextBlock { Text = text.Length == 0 ? T("rl_sms_notext", "(no text)") : "„" + text + "“", FontSize = 13, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap };
                msg.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                stack.Children.Add(msg);
                var parts = new List<string>
                {
                    string.Format(CultureInfo.CurrentCulture, T("rl_sms_slot", "SMS {0}"), i + 1),
                    Sms.AtMidPoint(i) ? RuleOptions.GameText("FMMC_ASSRL3_2", "Objective Mid Point") : RuleOptions.GameText("FMMC_ASSRL3_1", "Objective Start"),
                };
                if (Sms.Delay(i) > 0)
                    parts.Add(string.Format(CultureInfo.CurrentCulture, T("rl_sms_delay", "+{0} s"), Sms.Delay(i)));
                if (Sms.ToAllTeams(i))
                    parts.Add(RuleOptions.GameText("FMMC_SMSSTA_T", "Send To All Teams"));
                int points = Sms.Get(GTA.Offsets.Editor.SMS.ptsreq, i);
                if (points >= 0)
                    parts.Add(string.Format(CultureInfo.CurrentCulture, T("rl_sms_points", "from {0} points"), points));
                stack.Children.Add(Faint(string.Join(" · ", parts), 12));
                dock.Children.Add(stack);
                card.Child = dock;
                panel.Children.Add(card);
            }
            var add = new Button { Style = (Style)FindResource("FormButton"), Height = 28, Padding = new Thickness(12, 0, 12, 0), HorizontalAlignment = HorizontalAlignment.Left,
                Content = T("rl_sms_add", "+ SMS for this rule"), Margin = new Thickness(0, 4, 0, 0) };
            bool free = Sms.FreeSlot() >= 0;
            add.IsEnabled = free;
            add.Opacity = free ? 1 : 0.5;
            ToolTipService.SetShowOnDisabled(add, true);
            add.ToolTip = free ? T("rl_sms_add_tip", "Takes the first free of the ten SMS slots and opens it on the SMS page.") : T("rl_sms_full", "All ten SMS slots are in use.");
            int team = _team, ruleIndex = rule.Index;
            add.Click += (_, __) =>
            {
                int slot = Sms.Add(team, ruleIndex);
                if (slot >= 0)
                    MainWindow.Instance?.OpenSms(slot);
            };
            panel.Children.Add(add);
            return panel;
        }

        // ----- building blocks -----

        private Border Box(string icon, string title, FrameworkElement right, FrameworkElement body)
        {
            var head = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
            var badge = new Border { Width = 24, Height = 24, CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 9, 0),
                Child = new TextBlock { Text = icon, FontSize = 12, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
            badge.SetResourceReference(Border.BackgroundProperty, "TextBoxBackground");
            ((TextBlock)badge.Child).SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            DockPanel.SetDock(badge, Dock.Left);
            head.Children.Add(badge);
            if (right != null)
            {
                right.VerticalAlignment = VerticalAlignment.Center;
                DockPanel.SetDock(right, Dock.Right);
                head.Children.Add(right);
            }
            var t = new TextBlock { Text = title, FontSize = 14, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            head.Children.Add(t);
            var stack = new StackPanel();
            stack.Children.Add(head);
            stack.Children.Add(body);
            var box = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Padding = new Thickness(12, 11, 12, 11), Margin = new Thickness(0, 0, 0, 10), Child = stack };
            box.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            box.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            return box;
        }

        // A box that opens and closes; closed it shows one line of what is set.
        private Border Group(string key, string icon, string title, string summary, Func<FrameworkElement> body)
        {
            bool open = _openGroups.Contains(key);
            var right = new TextBlock { Text = (summary ?? "") + (open ? "  ▾" : "  ▸"), FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 260 };
            right.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var box = Box(icon, title, right, open ? body() : new Border());
            var head = (DockPanel)((StackPanel)box.Child).Children[0];
            if (!open)
                head.Margin = new Thickness(0);
            head.Cursor = System.Windows.Input.Cursors.Hand;
            head.Background = Brushes.Transparent;
            head.MouseLeftButtonUp += (_, __) =>
            {
                if (!_openGroups.Remove(key))
                    _openGroups.Add(key);
                ShowDetail();
            };
            return box;
        }

        private TextBlock Say(string text, double top = 0)
        {
            var t = new TextBlock { Text = text, FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, top, 0, 6) };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            return t;
        }

        // The creator's help text, only while "? Help" is on.
        private FrameworkElement HelpLine(string text)
        {
            if (!_help || string.IsNullOrWhiteSpace(text))
                return null;
            var t = new TextBlock { Text = text, FontSize = 12, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            var b = new Border { Child = t, BorderThickness = new Thickness(3, 0, 0, 0), CornerRadius = new CornerRadius(3), Padding = new Thickness(8, 5, 8, 5), Margin = new Thickness(0, 2, 0, 8) };
            b.SetResourceReference(Border.BackgroundProperty, "AccentSoftBrush");
            b.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
            return b;
        }

        private static void AddIf(Panel panel, UIElement element)
        {
            if (element != null)
                panel.Children.Add(element);
        }

        private void Reread()
        {
            _shownKey = null;
            Dispatcher.BeginInvoke(new Action(() => Reload(true)), DispatcherPriority.Background);
        }

        // A label on the left, an editor on the right, the help text under both.
        private void OptionRow(Panel panel, string label, FrameworkElement editor, string help, string tip = null)
        {
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            editor.VerticalAlignment = VerticalAlignment.Center;
            if (!(editor is CheckBox))
                editor.Width = Math.Max(editor.Width is double w && !double.IsNaN(w) ? w : 0, 150);
            DockPanel.SetDock(editor, Dock.Right);
            row.Children.Add(editor);
            var l = new TextBlock { Text = label, FontSize = 13, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), ToolTip = tip };
            l.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(l);
            panel.Children.Add(row);
            AddIf(panel, HelpLine(help));
        }

        private CheckBox Toggle(bool on, Action<bool> write)
        {
            var c = new CheckBox { Style = (Style)FindResource("FormToggle"), IsChecked = on };
            c.Click += (_, __) =>
            {
                if (_loading || !Live) return;
                write(c.IsChecked == true);
                Reread();
            };
            return c;
        }

        private ComboBox Combo(IEnumerable<(int Value, string Text)> items, int current, Action<int> write)
        {
            var box = new ComboBox { Height = 28 };
            foreach (var (v, text) in items)
                box.Items.Add(new ComboBoxItem { Content = text, Tag = v });
            var pick = box.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == current);
            if (pick == null)
            {
                pick = new ComboBoxItem { Content = current.ToString(CultureInfo.CurrentCulture), Tag = current };
                box.Items.Add(pick);
            }
            box.SelectedItem = pick;
            box.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(box.SelectedItem is ComboBoxItem item)) return;
                write((int)item.Tag);
                Reread();
            };
            return box;
        }

        private TextBox NumberBox(int value, string unit, Action<int> write)
        {
            var box = new TextBox { Height = 28, Text = value.ToString(CultureInfo.InvariantCulture), Tag = unit, ToolTip = unit };
            box.SetResourceReference(StyleProperty, "Watermark");
            box.TextChanged += (_, __) =>
            {
                if (!_loading && Live && int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    write(v);
                    _shownKey = null;
                }
            };
            return box;
        }

        // ----- status -----

        private FrameworkElement StatusRow(Rules.Rule rule)
        {
            var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
            var reach = _flow.Reached[rule.Index];
            if (reach == RuleFlow.Reach.Never)
                wrap.Children.Add(Chip(T("rl_st_never", "never reached"), ChipKind.Warn));
            else if (reach == RuleFlow.Reach.Branch)
                wrap.Children.Add(Chip(T("rl_st_branch", "only reached through a jump"), ChipKind.Plain));
            else
                wrap.Children.Add(Chip("✓ " + (rule.Index == 0 ? T("rl_st_first", "first rule") : string.Format(CultureInfo.CurrentCulture, T("rl_st_after", "comes after rule {0}"), rule.Index)), ChipKind.Plain));
            wrap.Children.Add(Chip(rule.Links.Count == 1 ? T("rl_st_task1", "1 task") : string.Format(CultureInfo.CurrentCulture, T("rl_st_tasks", "{0} tasks"), rule.Links.Count),
                rule.Links.Count == 0 ? ChipKind.Warn : ChipKind.Plain));
            wrap.Children.Add(Chip(rule.TimeLimit != 0 ? "⏱ " + TimeText(rule.TimeLimit) : T("rl_time_off", "No limit"), ChipKind.Plain));
            if (rule.FailsMission)
                wrap.Children.Add(Chip(T("rl_chip_fail", "fail = mission over"), ChipKind.Warn));
            return wrap;
        }

        // ----- 1: text -----

        private FrameworkElement TextBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            var box = new TextBox { Text = rule.Text ?? "", MaxLength = 63, BorderThickness = new Thickness(0), Background = Brushes.Transparent, Padding = new Thickness(0), FontSize = 13.5 };
            box.SetResourceReference(Control.ForegroundProperty, "TextColor");
            box.SetResourceReference(System.Windows.Controls.Primitives.TextBoxBase.CaretBrushProperty, "TextColor");
            GtaTextAssist.Attach(box);
            var count = Faint("", 11.5);
            count.VerticalAlignment = VerticalAlignment.Center;
            count.Margin = new Thickness(8, 0, 0, 0);
            void Count() => count.Text = box.Text.Length + "/" + box.MaxLength;
            Count();
            box.TextChanged += (_, __) => Count();
            box.LostKeyboardFocus += (_, __) =>
            {
                if (_loading || !Live || box.Text == (rule.Text ?? "")) return;
                Rules.SetText(_team, rule.Index, box.Text);
                rule.Text = box.Text;
                Reread();
            };
            var line = new DockPanel();
            DockPanel.SetDock(count, Dock.Right);
            line.Children.Add(count);
            line.Children.Add(box);
            var field = new Border { CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(1), Padding = new Thickness(9, 6, 9, 6), Margin = new Thickness(0, 0, 0, 6), Child = line };
            field.SetResourceReference(Border.BackgroundProperty, "TextBoxBackground");
            field.SetResourceReference(Border.BorderBrushProperty, "TextBoxBorder");
            panel.Children.Add(field);
            panel.Children.Add(Say(string.IsNullOrWhiteSpace(rule.Text)
                ? T("rl_text_empty_hint2", "Empty: the game shows its default text. Select text for colours, Ctrl+Space for icons and codes.")
                : T("rl_text_hint2", "Shown at the bottom of the screen while the team is on this rule. Select text for colours, Ctrl+Space for icons.")));
            return panel;
        }

        // ----- 2: tasks -----

        private static string KindBrush(Rules.Kind kind)
        {
            switch (kind)
            {
                case Rules.Kind.Ped: return "ActorBrush";
                case Rules.Kind.Vehicle: return "VehicleBrush";
                case Rules.Kind.Object: return "PropBrush";
                case Rules.Kind.GoTo: return "ZoneBrush";
                default: return "AccentBrush";
            }
        }

        private FrameworkElement TasksBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            panel.Children.Add(Say(rule.Links.Count == 0
                ? T("rl_links_none", "Nothing points at this rule. The game reaches it only through a jump or the next-objective override, otherwise the team gets stuck here.")
                : T("rl_tasks_say", "The rule is done when all of these are done, or earlier when a limit below ends it.")));
            AddIf(panel, HelpLine(RuleOptions.GameText("PMCH_EO_T")));
            foreach (var link in rule.Links)
                panel.Children.Add(TaskCard(rule, link));
            foreach (int to in rule.Links.Where(l => Rules.FailJumpBlocked(l, _team, out _)).Select(l => l.FailJump).Distinct())
            {
                var warn = Faint("⚠ " + CriticalTip(rule, to), 12);
                warn.Margin = new Thickness(0, 2, 0, 6);
                warn.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                panel.Children.Add(warn);
            }
            panel.Children.Add(AddTaskArea(rule));
            return panel;
        }

        private FrameworkElement TaskCard(Rules.Rule rule, Rules.Link link)
        {
            int type = EoType(link.Kind);
            var stack = new StackPanel();

            // Name line: colour of the entity kind, name, task, own/extra, open, remove.
            var top = new DockPanel();
            var icons = new StackPanel { Orientation = Orientation.Horizontal };
            if (type != 0)
            {
                icons.Children.Add(IconButton("M5,3 H1 V11 H9 V7 M7,1 H11 V5 M11,1 L5,7", T("rl_open_entity", "Open its page"),
                    () => MainWindow.Instance?.OpenEntity(type, link.Index)));
                if (EntityRules.UsesRuleList || link.Extra)
                    icons.Children.Add(IconButton("M2,2 L10,10 M10,2 L2,10", T("rl_leave_rule", "Take it out of this rule"), () => LeaveRule(rule, link, type)));
            }
            DockPanel.SetDock(icons, Dock.Right);
            top.Children.Add(icons);
            if (link.Extra && !EntityRules.UsesRuleList)
            {
                var tb = ExtraTypeBox(rule, link, type);
                DockPanel.SetDock(tb, Dock.Right);
                top.Children.Add(tb);
            }
            else
            {
                var what = Faint(TypeName(link), 12);
                what.VerticalAlignment = VerticalAlignment.Center;
                what.Margin = new Thickness(8, 0, 0, 0);
                DockPanel.SetDock(what, Dock.Right);
                top.Children.Add(what);
            }
            var dot = new Rectangle { Width = 10, Height = 10, RadiusX = 3, RadiusY = 3, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
            dot.SetResourceReference(Shape.FillProperty, KindBrush(link.Kind));
            DockPanel.SetDock(dot, Dock.Left);
            top.Children.Add(dot);
            if (link.Extra)
            {
                var chip = ExtraChip();
                DockPanel.SetDock(chip, Dock.Left);
                top.Children.Add(chip);
            }
            var name = new TextBlock { Text = EntityName(link), FontSize = 13, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
            name.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            top.Children.Add(name);
            stack.Children.Add(top);

            // What happens when it is done or failed, in words.
            if (link.HasPass)
                stack.Children.Add(JumpLine(link, true));
            if (link.HasFail)
                stack.Children.Add(JumpLine(link, false));

            // The entity's way through the rules, so an extra reads as "also in this rule".
            if (type != 0)
            {
                var entries = EntityRules.Read(type, link.Index, _team).OrderBy(e => e.Rule).ToList();
                if (entries.Count > 1)
                {
                    var way = new WrapPanel { Margin = new Thickness(0, 7, 0, 0) };
                    var label = Faint(T("rl_way", "Its way:") + " ", 12);
                    label.VerticalAlignment = VerticalAlignment.Center;
                    way.Children.Add(label);
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var e = entries[i];
                        if (i > 0)
                            way.Children.Add(Faint(" → ", 12).Also(t => t.VerticalAlignment = VerticalAlignment.Center));
                        string what = Rules.SelectionName(e.Selection) ?? "";
                        way.Children.Add(Chip(string.Format(CultureInfo.CurrentCulture, "R{0} {1}", e.Rule + 1, what).Trim(),
                            e.Rule == rule.Index ? ChipKind.Type : ChipKind.Plain));
                    }
                    stack.Children.Add(way);
                }
            }
            if (link.Extra)
            {
                stack.Children.Add(Say(T("rl_extra_same_time", "Runs at the same time as the other tasks of this rule, not after them."), 6));
                if (RulePresets.Ready)
                {
                    var move = new Button { Style = (Style)FindResource("FormButton"), Height = 26, Padding = new Thickness(10, 0, 10, 0), HorizontalAlignment = HorizontalAlignment.Left,
                        Content = T("rl_extra_move_new", "Move into a new rule after this one"),
                        ToolTip = T("rl_extra_move_new_tip", "For \"first this rule, then that\": takes it out of this rule and puts it into a new rule right after it.") };
                    move.Click += (_, __) => MoveToNewRule(rule, link, type);
                    stack.Children.Add(move);
                }
            }

            var card = new Border { CornerRadius = new CornerRadius(7), BorderThickness = new Thickness(1), Padding = new Thickness(10, 8, 10, 9), Margin = new Thickness(0, 0, 0, 6), Child = stack };
            card.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            card.SetResourceReference(Border.BorderBrushProperty, link.Extra ? "AccentBrush" : "LineBrush");
            return card;
        }

        private FrameworkElement JumpLine(Rules.Link link, bool pass)
        {
            var row = new DockPanel { Margin = new Thickness(0, 6, 0, 0) };
            var mark = new TextBlock { Text = pass ? "✓" : "✗", FontWeight = FontWeights.Bold, Width = 16, VerticalAlignment = VerticalAlignment.Center };
            mark.SetResourceReference(TextBlock.ForegroundProperty, pass ? "OkBrush" : "BadBrush");
            DockPanel.SetDock(mark, Dock.Left);
            row.Children.Add(mark);
            var combo = new ComboBox { Width = 150, Height = 26, FontSize = 12 };
            combo.Items.Add(new ComboBoxItem { Content = pass ? T("rl_jump_normal", "as normal") : T("rl_jump_stay", "nothing special"), Tag = -1 });
            for (int r = 0; r < _rules.Count; r++)
            {
                if (Rules.PublicCreator && r <= _selected)
                    continue;
                combo.Items.Add(new ComboBoxItem { Content = string.Format(CultureInfo.CurrentCulture, T("rl_rule_n", "Rule {0}"), r + 1), Tag = r });
            }
            int current = pass ? link.PassJump : link.FailJump;
            combo.SelectedItem = combo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == current) ?? combo.Items[0];
            combo.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(combo.SelectedItem is ComboBoxItem item))
                    return;
                Rules.SetJump(link, _team, pass, (int)item.Tag);
                Reread();
            };
            DockPanel.SetDock(combo, Dock.Right);
            row.Children.Add(combo);
            var text = new TextBlock { Text = pass ? T("rl_jump_pass_say", "When done, go to") : T("rl_jump_fail_say", "When it fails, go to"), FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };
            text.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            row.Children.Add(text);
            return row;
        }

        // Takes the extra out of this rule and gives the entity a new rule right after it.
        private void MoveToNewRule(Rules.Rule rule, Rules.Link link, int type)
        {
            if (!Live)
                return;
            int selection = EntityRules.UsesRuleList ? EntityRules.RuleSelection(_team, rule.Index) : link.Type;
            if (EntityRules.UsesRuleList)
                EntityRules.SetMember(type, link.Index, _team, rule.Index, false);
            else
            {
                var entry = EntityRules.Read(type, link.Index, _team).FirstOrDefault(e => !e.Main && e.Rule == rule.Index);
                if (entry != null)
                    ExtraObjectives.Remove(entry.Slot, entry.Place, _team);
            }
            AddAsNewRuleAfter(rule, type, link.Index, selection);
        }

        // A new rule with the entity in it, placed right after this rule where rules can be moved
        // (LTS, Capture); in the Mission Creator it stays at the end.
        private void AddAsNewRuleAfter(Rules.Rule rule, int type, int index, int selection)
        {
            string error = EntityRules.AddToNewRule(type, index, _team, selection);
            if (error != null)
            {
                Changed(error);
                return;
            }
            int added = Rules.Count(_team) - 1;
            if (RuleMover.Available && added > rule.Index + 1)
                MoveRule(added, rule.Index + 1);
            else
                Changed(EntityRules.UsesRuleList ? "rl_new_at_end" : null);
        }

        private FrameworkElement AddTaskArea(Rules.Rule rule)
        {
            bool list = EntityRules.UsesRuleList;
            int cls = list ? EntityRules.ClassOf(EntityRules.RuleSelection(_team, rule.Index)) : 0;
            var panel = new StackPanel();
            var head = new TextBlock { Text = "+ " + T("rl_add_task", "Add a task"), FontWeight = FontWeights.SemiBold, FontSize = 13, Margin = new Thickness(0, 0, 0, 6) };
            head.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            panel.Children.Add(head);
            if (list && (cls < ExtraObjectives.TypePed || cls > ExtraObjectives.TypeGoTo))
            {
                panel.Children.Add(Say(T("rl_add_list_none", "Mission Creator: this rule takes no entities (player rule).")));
                return Dashed(panel);
            }

            var kinds = new ComboBox { Height = 28, Width = 110, Margin = new Thickness(0, 0, 6, 0), IsEnabled = !list };
            foreach (var (t, key, fallback) in new[] { (ExtraObjectives.TypePed, "eo_k_ped", "Actor"), (ExtraObjectives.TypeVehicle, "eo_k_veh", "Vehicle"),
                (ExtraObjectives.TypeObject, "eo_k_obj", "Object"), (ExtraObjectives.TypeGoTo, "rl_k_loc", "Location") })
                kinds.Items.Add(new ComboBoxItem { Content = T(key, fallback), Tag = t });
            kinds.SelectedIndex = list ? cls - 1 : 1;
            var entities = new ComboBox { Height = 28, MinWidth = 140 };
            var types = new ComboBox { Height = 28, Width = 150, Margin = new Thickness(6, 0, 0, 0), Visibility = list ? Visibility.Collapsed : Visibility.Visible };
            int Kind() => kinds.SelectedItem is ComboBoxItem k ? (int)k.Tag : 0;
            void Fill()
            {
                int type = Kind();
                entities.Items.Clear();
                var taken = new HashSet<int>(rule.Links.Where(l => EoType(l.Kind) == type).Select(l => l.Index));
                for (int i = 0; i < EntityPicker.Count(type); i++)
                    if (!taken.Contains(i))
                        entities.Items.Add(new ComboBoxItem { Content = EntityPicker.Label(type, i), Tag = i });
                entities.SelectedIndex = entities.Items.Count > 0 ? 0 : -1;
                types.Items.Clear();
                foreach (var r in ExtraObjectives.RuleTypesFor(type))
                    types.Items.Add(new ComboBoxItem { Content = T("eo_r_" + r.Value, r.Name), Tag = r.Value });
                types.SelectedIndex = 0;
            }
            Fill();
            kinds.SelectionChanged += (_, __) => Fill();
            var line = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
            DockPanel.SetDock(kinds, Dock.Left);
            line.Children.Add(kinds);
            DockPanel.SetDock(types, Dock.Right);
            line.Children.Add(types);
            line.Children.Add(entities);
            panel.Children.Add(line);

            // Same time or after: the question the old row never asked.
            var choices = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, 0, 8) };
            choices.Children.Add(Choice(T("rl_add_same", "In this rule"), T("rl_add_same_sub", "at the same time as the tasks above"), !_addAsNewRule, () => { _addAsNewRule = false; ShowDetail(); }));
            choices.Children.Add(Choice(T("rl_add_after", "As a new rule after this"),
                RuleMover.Available ? T("rl_add_after_sub", "only once this rule is done") : T("rl_add_after_end", "added at the end (Mission Creator)"),
                _addAsNewRule, () => { _addAsNewRule = true; ShowDetail(); }));
            panel.Children.Add(choices);

            var add = new Button { Style = (Style)FindResource("FormButton"), Height = 28, Padding = new Thickness(12, 0, 12, 0), Content = T("eo_add", "Add"), HorizontalAlignment = HorizontalAlignment.Left };
            add.Click += (_, __) =>
            {
                if (!Live || !(entities.SelectedItem is ComboBoxItem e))
                    return;
                int type = Kind(), index = (int)e.Tag;
                int selection = list ? EntityRules.RuleSelection(_team, rule.Index) : types.SelectedItem is ComboBoxItem t ? (int)t.Tag : -1;
                if (selection < 0)
                    return;
                if (_addAsNewRule)
                    AddAsNewRuleAfter(rule, type, index, selection);
                else
                    Changed(list ? EntityRules.SetMember(type, index, _team, rule.Index, true) : ExtraObjectives.Add(type, index, _team, rule.Index, selection));
            };
            var bottom = new DockPanel();
            DockPanel.SetDock(add, Dock.Left);
            bottom.Children.Add(add);
            var note = Faint(string.Format(CultureInfo.CurrentCulture, T("rl_add_note", "An entity that already has a rule becomes an extra objective here. Extra objectives: {0} of 30 entities."),
                ExtraObjectives.UsedSlots()), 12);
            note.Margin = new Thickness(10, 0, 0, 0);
            note.VerticalAlignment = VerticalAlignment.Center;
            note.TextWrapping = TextWrapping.Wrap;
            bottom.Children.Add(note);
            panel.Children.Add(bottom);
            if (_joinError != null)
            {
                var error = Faint(T(_joinError, _joinError), 12);
                error.Margin = new Thickness(0, 6, 0, 0);
                error.SetResourceReference(TextBlock.ForegroundProperty, _joinError == "rl_new_at_end" ? "WarnBrush" : "BadBrush");
                panel.Children.Add(error);
            }
            return Dashed(panel);
        }

        private static Border Dashed(UIElement child)
        {
            var grid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            var frame = new Rectangle { RadiusX = 7, RadiusY = 7, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 4, 3 } };
            frame.SetResourceReference(Shape.StrokeProperty, "LineBrush");
            grid.Children.Add(frame);
            grid.Children.Add(new Border { Padding = new Thickness(10), Child = child });
            return new Border { Child = grid };
        }

        private Border Choice(string title, string sub, bool on, Action pick)
        {
            var stack = new StackPanel();
            var t = new TextBlock { Text = title, FontSize = 12.5, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            stack.Children.Add(t);
            stack.Children.Add(Faint(sub, 11.5).Also(x => x.TextWrapping = TextWrapping.Wrap));
            var b = new Border { CornerRadius = new CornerRadius(7), BorderThickness = new Thickness(on ? 2 : 1), Padding = new Thickness(on ? 8 : 9, on ? 6 : 7, 8, 7),
                Margin = new Thickness(0, 0, 6, 0), Child = stack, Cursor = System.Windows.Input.Cursors.Hand };
            b.SetResourceReference(Border.BackgroundProperty, on ? "AccentSoftBrush" : "SectionBackgroundBrush");
            b.SetResourceReference(Border.BorderBrushProperty, on ? "AccentBrush" : "LineBrush");
            b.MouseLeftButtonUp += (_, __) => pick();
            return b;
        }

        // ----- 3: end -----

        private FrameworkElement EndBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            OptionRow(panel, RuleOptions.GameText("PMCT_OL_SCORE", "Score Limit"),
                NumberBox(rule.TargetScore, null, v => Rules.SetField(GTA.Offsets.Editor.tsc, _team, rule.Index, v)),
                RuleOptions.Help("OL_SCORE"), T("rl_target_hint", "How many of the objective's entities must be done, e.g. destroy 6 of 10."));
            OptionRow(panel, RuleOptions.GameText("PMCT_OL_POINTS", "Objective Score"),
                NumberBox(rule.ObjectiveScore, null, v => Rules.SetField(GTA.Offsets.Editor.tms, _team, rule.Index, v)), RuleOptions.Help("OL_POINTS"));

            var time = new ComboBox { Height = 28 };
            foreach (var (sel, sec) in Rules.TimeLimits)
                time.Items.Add(new ComboBoxItem { Content = sel == 0 ? T("rl_time_off", "No limit") : TimeLabel(sec), Tag = sel });
            time.SelectedItem = time.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == rule.TimeLimit);
            time.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(time.SelectedItem is ComboBoxItem item)) return;
                Rules.SetField(GTA.Offsets.Editor.tmt, _team, rule.Index, (int)item.Tag);
                Reread();
            };
            OptionRow(panel, RuleOptions.GameText("PMCT_OL_TIME", "Time Limit"), time, RuleOptions.Help("OL_TIME"));
            var expired = Combo(new[] { (0, RuleOptions.GameText("PMC_OL_TO_0", "Objective fails")), (1, RuleOptions.GameText("PMC_OL_TO_1", "Team fails")) },
                rule.FailsMission ? 1 : 0, v => Rules.SetFailsMission(_team, rule.Index, v == 1));
            OptionRow(panel, RuleOptions.GameText("PMCT_OL_TO", "Time Limit Expired Fails Team"), expired, RuleOptions.Help("OL_TO"));
            OptionRow(panel, RuleOptions.GameText("PMCT_OBJ_CAPT", "Capture Time") + " (ms)",
                NumberBox(rule.TakeoverMs, "ms", v => Rules.SetField(GTA.Offsets.Editor.ttime, _team, rule.Index, v)), T("rl_takeover_help", "How long a capture or hack takes on this objective, in milliseconds."));
            foreach (var o in RuleOptions.EndOptions.Where(RuleOptions.Available))
                OptionEditor(panel, o, rule);
            if (rule.Links.Count == 0 && rule.TargetScore == 0 && rule.TimeLimit == 0)
            {
                var warn = Faint("⚠ " + T("rl_limits_hint", "A rule nothing completes on its own (no entities, no needed count) needs a time limit, or the team stays on it."), 12);
                warn.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                panel.Children.Add(warn);
            }
            return panel;
        }

        private void OptionEditor(Panel panel, RuleOptions.Option o, Rules.Rule rule)
        {
            int value = RuleOptions.Get(o, _team, rule.Index);
            FrameworkElement editor;
            switch (o.Kind)
            {
                case RuleOptions.Kind.Bit:
                case RuleOptions.Kind.RuleBit:
                    editor = Toggle(value != 0, on => RuleOptions.Set(o, _team, rule.Index, on ? 1 : 0));
                    break;
                case RuleOptions.Kind.Choice:
                    editor = Combo(o.Choices(), value, v => RuleOptions.Set(o, _team, rule.Index, v));
                    break;
                default:
                    editor = NumberBox(value, o.Unit, v => RuleOptions.Set(o, _team, rule.Index, v));
                    break;
            }
            string title = RuleOptions.Title(o) + (o.Kind == RuleOptions.Kind.Int && o.Unit != null ? " (" + o.Unit + ")" : "");
            OptionRow(panel, title, editor, RuleOptions.Help(o));
        }

        // ----- 4: next -----

        private FrameworkElement NextBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            panel.Children.Add(Say(rule.Index + 1 < _rules.Count
                ? string.Format(CultureInfo.CurrentCulture, T("rl_next_normal", "Normally: rule {0}. Only pick something here to go elsewhere."), rule.Index + 2)
                : T("rl_next_last", "This is the last rule: when it is done, the team has passed.")));
            var tiles = new WrapPanel();
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
                    Reread();
                };
                tiles.Children.Add(tile);
            }
            if (tiles.Children.Count > 0)
            {
                var row = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
                var label = new TextBlock { Text = RuleOptions.GameText("PMCT_OJT_OVRNXT", "Override Next Objective"), FontSize = 13, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
                label.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                DockPanel.SetDock(label, Dock.Left);
                row.Children.Add(label);
                var holder = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(3, 3, 0, 0), Child = tiles, HorizontalAlignment = HorizontalAlignment.Right };
                holder.SetResourceReference(Border.BackgroundProperty, "TextBoxBackground");
                row.Children.Add(holder);
                panel.Children.Add(row);
                panel.Children.Add(Say(T("rl_next_hint2", "One number: go there. Several: one of them at random. A ✓ jump of a task always wins.")));
                AddIf(panel, HelpLine(RuleOptions.Help("OJT_OVRNXT")));
            }
            if (rule.NextRules != 0 && rule.Links.Any(l => l.PassJump > rule.Index))
            {
                var warn = Faint("⚠ " + T("rl_flow_noeffect_tip", "A ✓ jump is set on this rule. In the game the jump wins, so the next objective does nothing."), 12);
                warn.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                panel.Children.Add(warn);
            }
            return panel;
        }

        // ----- further groups -----

        private string OptionsSummary(RuleOptions.Option[] options, Rules.Rule rule)
        {
            int set = options.Where(RuleOptions.Available).Count(o =>
                o.Kind == RuleOptions.Kind.Bit || o.Kind == RuleOptions.Kind.RuleBit
                    ? (RuleOptions.Get(o, _team, rule.Index) != 0) != RuleOptions.OnWhenClear(o)
                    : RuleOptions.Get(o, _team, rule.Index) > 0);
            return set == 0 ? T("rl_g_default", "default") : string.Format(CultureInfo.CurrentCulture, T("rl_g_set", "{0} changed"), set);
        }

        private FrameworkElement OptionsBody(RuleOptions.Option[] options, Rules.Rule rule, string helpLabel)
        {
            var panel = new StackPanel();
            AddIf(panel, HelpLine(RuleOptions.Help(helpLabel)));
            foreach (var o in options.Where(RuleOptions.Available))
                OptionEditor(panel, o, rule);
            if (helpLabel == "GC_T" && RuleOptions.Available(RuleOptions.ChaseBlockFlag))
                OptionEditor(panel, RuleOptions.ChaseBlockFlag, rule);
            panel.Children.Add(Say(T("rl_g_source", "Options of the Mission Creator's objective menu. LTS and Capture keep them in the job too."), 4));
            return panel;
        }

        private string WantedSummary(Rules.Rule rule)
        {
            int level = RuleOptions.WantedLevel(_team, rule.Index);
            string text = RuleOptions.WantedLevels().FirstOrDefault(l => l.Value == level).Text ?? level.ToString(CultureInfo.CurrentCulture);
            return text + " · " + OptionsSummary(RuleOptions.MoreGroups[0].Options, rule);
        }

        private FrameworkElement WantedBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            AddIf(panel, HelpLine(RuleOptions.Help("LAW")));
            int trigger = RuleOptions.WantedTrigger(_team, rule.Index), level = RuleOptions.WantedLevel(_team, rule.Index);
            OptionRow(panel, RuleOptions.GameText("PMCT_LAW_LVL", "Set Level"),
                Combo(RuleOptions.WantedLevels(), level, v => RuleOptions.SetWanted(_team, rule.Index, RuleOptions.WantedTrigger(_team, rule.Index), v)), RuleOptions.Help("LAW_LVL"));
            OptionRow(panel, RuleOptions.GameText("PMCT_LAW_TRIG", "Trigger Condition"),
                Combo(RuleOptions.WantedTriggers(), trigger, v => RuleOptions.SetWanted(_team, rule.Index, v, RuleOptions.WantedLevel(_team, rule.Index))), RuleOptions.Help("LAW_TRIG"));
            foreach (var o in RuleOptions.MoreGroups[0].Options.Where(RuleOptions.Available))
                OptionEditor(panel, o, rule);
            panel.Children.Add(Say(T("rl_g_source", "Options of the Mission Creator's objective menu. LTS and Capture keep them in the job too."), 4));
            return panel;
        }

        private string OtherTeamsSummary(Rules.Rule rule)
        {
            var parts = new List<string>();
            for (int t = 0; t < Rules.Teams(); t++)
            {
                if (t == _team) continue;
                int r = RuleOptions.OtherTeamRule(_team, rule.Index, t);
                if (r >= 0)
                    parts.Add(string.Format(CultureInfo.CurrentCulture, T("rl_opl_sum", "team {0} → rule {1}"), t + 1, r + 1));
            }
            return parts.Count == 0 ? T("rl_g_default", "default") : string.Join(", ", parts);
        }

        private FrameworkElement OtherTeamsBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            AddIf(panel, HelpLine(RuleOptions.Help("OJT_OPL")));
            panel.Children.Add(Say(T("rl_opl_say", "This rule also passes when another team reaches a rule.")));
            for (int t = 0; t < Rules.Teams(); t++)
            {
                if (t == _team) continue;
                int other = t;
                var items = new List<(int, string)> { (-1, RuleOptions.GameText("PMC_OPL_UF", "None")) };
                for (int r = 0; r < Rules.MaxRules && r < Math.Max(Rules.Count(other), 1); r++)
                    items.Add((r, string.Format(CultureInfo.CurrentCulture, T("rl_rule_n", "Rule {0}"), r + 1)));
                OptionRow(panel, string.Format(CultureInfo.CurrentCulture, T("rl_opl_team", "Team {0} reaches"), t + 1),
                    Combo(items, RuleOptions.OtherTeamRule(_team, rule.Index, other), v => RuleOptions.SetOtherTeamRule(_team, rule.Index, other, v)), null);
                if (RuleOptions.OtherTeamRule(_team, rule.Index, other) >= 0)
                {
                    OptionRow(panel, "   " + RuleOptions.GameText("PMCT_OPL_TM", "Wait for Midpoint"),
                        Toggle(RuleOptions.OtherTeamFlag(_team, rule.Index, other), on => RuleOptions.SetOtherTeamFlag(_team, rule.Index, other, on)), RuleOptions.Help("OPL_TM"));
                    OptionRow(panel, "   " + RuleOptions.GameText("PMCT_OPL_AFR", "Consider Later Objectives"),
                        Toggle(RuleOptions.OtherTeamFlag(_team, rule.Index, 4 + other), on => RuleOptions.SetOtherTeamFlag(_team, rule.Index, 4 + other, on)), RuleOptions.Help("OPL_AFR"));
                }
            }
            if (Rules.Teams() > 2)
                OptionRow(panel, RuleOptions.Title(RuleOptions.AllTeams),
                    Combo(new[] { (0, RuleOptions.GameText("PMC_OJT_TXCN", "Any Team")), (1, RuleOptions.GameText("PMC_OJT_TXCY", "All Teams")) },
                        RuleOptions.Get(RuleOptions.AllTeams, _team, rule.Index), v => RuleOptions.Set(RuleOptions.AllTeams, _team, rule.Index, v)),
                    RuleOptions.Help(RuleOptions.AllTeams));
            return panel;
        }
    
        // ----- Mission Flags -----

        private static readonly (RuleOptions.FlagList List, string Label, string Fallback)[] FlagLists =
        {
            (RuleOptions.FlagList.SetOnStart, "OMF_SOO", "Set Mission Flags on Objective Start"),
            (RuleOptions.FlagList.ClearOnStart, "OMF_COO", "Clear Mission Flags on Objective Start"),
            (RuleOptions.FlagList.SetOnMidpoint, "OMF_SOM", "Set Mission Flags on Objective Midpoint"),
            (RuleOptions.FlagList.ClearOnMidpoint, "OMF_COM", "Clear Mission Flags on Objective Midpoint"),
        };

        private string FlagsSummary(Rules.Rule rule)
        {
            int changes = FlagLists.Sum(l => RuleOptions.Flags(l.List, _team, rule.Index).Count);
            int checks = RuleOptions.Flags(RuleOptions.FlagList.Limit, _team, rule.Index).Count;
            if (changes == 0 && checks == 0)
                return T("rl_g_default", "default");
            return string.Format(CultureInfo.CurrentCulture, T("rl_fl_sum", "changes {0} · waits for {1}"), changes, checks);
        }

        private FrameworkElement FlagsBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            // What they are, always shown: nobody knows the word.
            var intro = new Border { CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 10) };
            intro.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            var introText = new StackPanel();
            introText.Children.Add(new TextBlock { Text = T("rl_fl_what", "Mission Flags are 80 on/off switches for the whole mission. A rule can switch them on or off when it starts or reaches its midpoint. Other parts of the mission react to them: a rule ends once its flags are on, a door opens, an actor spawns, chase units stay away. That links events without a fixed order."),
                FontSize = 12.5, TextWrapping = TextWrapping.Wrap }.Also(t => t.SetResourceReference(TextBlock.ForegroundProperty, "TextColor")));
            introText.Children.Add(Faint(T("rl_fl_example", "Example: rule 2 switches flag 1 on at its start; rule 4 waits for flag 1 and ends as soon as it is on."), 12)
                .Also(t => { t.TextWrapping = TextWrapping.Wrap; t.Margin = new Thickness(0, 5, 0, 0); }));
            intro.Child = introText;
            panel.Children.Add(intro);
            AddIf(panel, HelpLine(RuleOptions.Help("OMF")));

            foreach (var (list, label, fallback) in FlagLists)
                FlagRow(panel, RuleOptions.GameText("PMCT_" + label, fallback), list, rule, RuleOptions.Help(label));

            var limit = new TextBlock { Text = T("rl_fl_wait", "This rule waits for"), FontSize = 13, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 4) };
            limit.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            panel.Children.Add(limit);
            FlagRow(panel, RuleOptions.GameText("PMCT_OJT_OLP", "Mission Flag Limit"), RuleOptions.FlagList.Limit, rule, RuleOptions.Help("OJT_OLP"));
            if (RuleOptions.Available(RuleOptions.FlagLimitFails))
                OptionEditor(panel, RuleOptions.FlagLimitFails, rule);
            if (RuleOptions.Available(RuleOptions.BlockOnComplete))
                OptionEditor(panel, RuleOptions.BlockOnComplete, rule);

            panel.Children.Add(FlagOverview());
            return panel;
        }

        // One list of flags: chips to remove, a box to add.
        private void FlagRow(Panel panel, string title, RuleOptions.FlagList list, Rules.Rule rule, string help)
        {
            var flags = RuleOptions.Flags(list, _team, rule.Index);
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
            var add = new ComboBox { Height = 26, Width = 110, FontSize = 12 };
            add.Items.Add(new ComboBoxItem { Content = "+ " + T("rl_fl_add", "Flag"), Tag = -1 });
            for (int f = 0; f < RuleOptions.FlagCount; f++)
                if (!flags.Contains(f))
                    add.Items.Add(new ComboBoxItem { Content = RuleOptions.FlagName(f), Tag = f });
            add.SelectedIndex = 0;
            add.SelectionChanged += (_, __) =>
            {
                if (_loading || !Live || !(add.SelectedItem is ComboBoxItem item) || (int)item.Tag < 0) return;
                RuleOptions.SetFlag(list, _team, rule.Index, (int)item.Tag, true);
                Reread();
            };
            DockPanel.SetDock(add, Dock.Right);
            row.Children.Add(add);
            var label = new TextBlock { Text = title, FontSize = 13, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            label.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(label);
            panel.Children.Add(row);
            var chips = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
            if (flags.Count == 0)
                chips.Children.Add(Faint(RuleOptions.GameText("PMC_BS_EMPT", "none"), 12));
            foreach (int f in flags)
            {
                int flag = f;
                var chip = new Button { Style = (Style)FindResource("FormButton"), Height = 24, Padding = new Thickness(8, 0, 8, 0), Margin = new Thickness(0, 0, 4, 4),
                    Content = RuleOptions.FlagName(flag) + "  ✕", FontSize = 12, ToolTip = T("rl_fl_remove", "Remove") };
                chip.Click += (_, __) =>
                {
                    if (!Live) return;
                    RuleOptions.SetFlag(list, _team, rule.Index, flag, false);
                    Reread();
                };
                chips.Children.Add(chip);
            }
            panel.Children.Add(chips);
            AddIf(panel, HelpLine(help));
        }

        // Where each flag is switched and where a rule waits for it, over all teams' rules.
        private FrameworkElement FlagOverview()
        {
            var changed = new Dictionary<int, List<string>>();
            var waited = new Dictionary<int, List<string>>();
            for (int t = 0; t < Rules.Teams(); t++)
                for (int r = 0; r < Math.Min(Rules.Count(t), Rules.MaxRules); r++)
                {
                    string where = (Rules.Teams() > 1 ? "T" + (t + 1) + " " : "") + "R" + (r + 1);
                    foreach (var (list, _, _) in FlagLists)
                        foreach (int f in RuleOptions.Flags(list, t, r))
                        {
                            string what = list == RuleOptions.FlagList.SetOnStart || list == RuleOptions.FlagList.SetOnMidpoint ? T("rl_fl_on", "on") : T("rl_fl_off", "off");
                            string when = list == RuleOptions.FlagList.SetOnStart || list == RuleOptions.FlagList.ClearOnStart ? T("rl_fl_start", "start") : T("rl_fl_mid", "midpoint");
                            (changed.TryGetValue(f, out var l) ? l : changed[f] = new List<string>()).Add($"{where} {when} → {what}");
                        }
                    foreach (int f in RuleOptions.Flags(RuleOptions.FlagList.Limit, t, r))
                        (waited.TryGetValue(f, out var l) ? l : waited[f] = new List<string>()).Add(where);
                    int block = RuleOptions.Available(RuleOptions.ChaseBlockFlag) ? RuleOptions.Get(RuleOptions.ChaseBlockFlag, t, r) : -1;
                    if (block >= 0)
                        (waited.TryGetValue(block, out var b) ? b : waited[block] = new List<string>()).Add(where + " " + T("rl_fl_chase", "(chase)"));
                }
            var panel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
            var head = new TextBlock { Text = T("rl_fl_overview", "Where each flag is used (all rules)"), FontSize = 13, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) };
            head.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            panel.Children.Add(head);
            var used = changed.Keys.Union(waited.Keys).OrderBy(f => f).ToList();
            if (used.Count == 0)
            {
                panel.Children.Add(Say(T("rl_fl_none", "No rule uses Mission Flags yet.")));
                return panel;
            }
            foreach (int f in used)
            {
                string line = RuleOptions.FlagName(f) + ":  "
                    + (changed.TryGetValue(f, out var c) ? T("rl_fl_switched", "switched") + " " + string.Join(", ", c) : "")
                    + (changed.ContainsKey(f) && waited.ContainsKey(f) ? "  ·  " : "")
                    + (waited.TryGetValue(f, out var w) ? T("rl_fl_waited", "waited for in") + " " + string.Join(", ", w) : "");
                var t = Faint(line, 12);
                t.TextWrapping = TextWrapping.Wrap;
                t.Margin = new Thickness(0, 0, 0, 3);
                panel.Children.Add(t);
                // The creator's own warnings, for rules; entities, doors and zones may still use the flag.
                if (!changed.ContainsKey(f) || !waited.ContainsKey(f))
                {
                    var warn = Faint("   ⚠ " + RuleOptions.GameText(changed.ContainsKey(f) ? "PMCW_PO_NC" : "PMCW_PO_NM", "") + " " + T("rl_fl_rulesonly", "(Only rules are looked at; entities, doors and zones may use it too.)"), 11.5);
                    warn.TextWrapping = TextWrapping.Wrap;
                    warn.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                    warn.Margin = new Thickness(0, 0, 0, 4);
                    panel.Children.Add(warn);
                }
            }
            return panel;
        }

        // ----- bounds -----

        private string BoundsSummary(Rules.Rule rule)
        {
            int type = RuleOptions.BoundsTypeOf(_team, rule.Index);
            string text = RuleOptions.BoundsTypes().FirstOrDefault(b => b.Value == type).Text ?? type.ToString(CultureInfo.CurrentCulture);
            int zones = RuleOptions.BoundsZones().Count(z => RuleOptions.BoundsUsesZone(_team, rule.Index, z.Index));
            return type == 0 ? text : text + " · " + string.Format(CultureInfo.CurrentCulture, T("rl_bnd_zones", "{0} zones"), zones);
        }

        private FrameworkElement BoundsBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            panel.Children.Add(Say(T("rl_bnd_say", "Where the team may be during this rule, made of bounds zones. Leaving them warns, kills or fails the team, as set below.")));
            AddIf(panel, HelpLine(RuleOptions.Help("BNDS")));
            int type = RuleOptions.BoundsTypeOf(_team, rule.Index);
            OptionRow(panel, RuleOptions.GameText("PMCT_BNDS_TYPE", "Bounds"),
                Combo(RuleOptions.BoundsTypes(), type, v => RuleOptions.SetBoundsType(_team, rule.Index, v)), RuleOptions.Help("BNDS_TYPE"));
            if (type != 0)
            {
                var zones = RuleOptions.BoundsZones();
                var title = new TextBlock { Text = RuleOptions.GameText("PMCT_BNDS_ZNE", "Specify Bounds Zones"), FontSize = 13, Margin = new Thickness(0, 2, 0, 4) };
                title.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                panel.Children.Add(title);
                if (zones.Count == 0)
                    panel.Children.Add(Say(RuleOptions.GameText("PMCT_BNDS_NOZNE", "No Bounds Zones placed") + ". " + T("rl_bnd_place", "Place zones of the type \"Bounds\" (Zones page or the creator), then pick them here.")));
                var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
                foreach (var (index, number) in zones)
                {
                    int zone = index;
                    var tile = new ToggleButton { Style = (Style)FindResource("DashTeamButton"), Content = number.ToString(CultureInfo.CurrentCulture), FontSize = 13,
                        Margin = new Thickness(0, 0, 3, 3), IsChecked = RuleOptions.BoundsUsesZone(_team, rule.Index, zone),
                        ToolTip = RuleOptions.GameText("PMCT_BNDS_INDEX", "Bounds Zone ~1~").Replace("~1~", number.ToString(CultureInfo.CurrentCulture)) };
                    tile.Click += (_, __) =>
                    {
                        if (!Live) return;
                        RuleOptions.SetBoundsZone(_team, rule.Index, zone, tile.IsChecked == true);
                        Reread();
                    };
                    wrap.Children.Add(tile);
                }
                panel.Children.Add(wrap);
                int consequence = RuleOptions.BoundsConsequence(_team, rule.Index);
                OptionRow(panel, RuleOptions.GameText("PMCT_BNDS_CON", "Bound Consequences"),
                    Combo(RuleOptions.BoundsConsequences(), consequence, v => RuleOptions.SetBoundsConsequence(_team, rule.Index, v)),
                    RuleOptions.Help("BNDS_CON") + "\n" + RuleOptions.Help("BNDS_CON" + consequence));
                if (consequence != 0)
                    OptionRow(panel, RuleOptions.GameText("PMCT_BNDS_TIME", "Max Time Out Of Bounds") + " (s)",
                        NumberBox(Math.Max(0, RuleOptions.BoundsSeconds(_team, rule.Index)), "s", v => RuleOptions.SetBoundsSeconds(_team, rule.Index, v)), RuleOptions.Help("BNDS_TIME"));
            }
            // The other play area system: shapes per rule, on its own page, in every creator.
            if (PlayAreas.Ready)
            {
                var modes = new[] { 1, 2 }.Select(a => PlayAreas.GetMode(a, _team, rule.Index)).ToList();
                string text = modes.All(m => m == PlayAreas.Mode.Off)
                    ? T("rl_bnd_pa_off", "Play Areas page (shapes): none for this rule.")
                    : string.Format(CultureInfo.CurrentCulture, T("rl_bnd_pa_on", "Play Areas page (shapes): area 1 {0}, area 2 {1}."), PlayAreaModeName(modes[0]), PlayAreaModeName(modes[1]));
                panel.Children.Add(Say(text, 6));
            }
            return panel;
        }

        private static string PlayAreaModeName(PlayAreas.Mode mode)
        {
            switch (mode)
            {
                case PlayAreas.Mode.StayInside: return T("rl_pa_inside", "stay inside");
                case PlayAreas.Mode.StayOutside: return T("rl_pa_outside", "stay outside");
                case PlayAreas.Mode.SpawnOnly: return T("rl_pa_spawn", "respawn only");
                default: return T("rl_pa_off", "off");
            }
        }
    }
}
