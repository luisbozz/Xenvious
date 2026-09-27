using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// "Extra rules" card of an entity page (actor, vehicle, object, go-to): the extra objectives
    /// of the selected entity per team, add and remove. Built in code so the four pages share it;
    /// the page attaches its entry dropdown with Attach().
    /// </summary>
    public class ExtraRulesCard : Border
    {
        private ComboBox _entries;
        private int _type;
        private int _team;
        private readonly TextBlock _slotInfo = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
        private readonly UniformGrid _teamTabs = new UniformGrid { Columns = ExtraObjectives.Teams, Margin = new Thickness(0, 0, 0, 10) };
        private readonly StackPanel _list = new StackPanel();
        private readonly TextBox _newNumber = new TextBox { Height = 30, Text = "1" };
        private readonly ComboBox _newType = new ComboBox { Height = 30, Margin = new Thickness(6, 0, 6, 0) };
        private readonly TextBlock _error = new TextBlock { FontSize = 12, Foreground = MainWindow.ThemeBrush("WarnBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };

        /// <summary>Opens the explanation (the extra objectives overview).</summary>
        public event EventHandler HelpRequested;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public ExtraRulesCard()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) => { if (IsVisible) Refresh(); };
        }

        public void Attach(ComboBox entries, int entityType)
        {
            _entries = entries;
            _type = entityType;
            entries.SelectionChanged += (_, __) => Refresh();
        }

        private bool _built;

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            Style = (Style)FindResource("DashCard");
            Margin = new Thickness(0, 0, 0, 12);
            _slotInfo.Foreground = (Brush)FindResource("NavMutedBrush");
            _newNumber.Style = (Style)FindResource("Watermark");

            var help = new Button { Style = (Style)FindResource("NavButton"), Margin = new Thickness(8, 0, 0, 0), ToolTip = T("eo_help_tip", "How extra rules work") };
            help.Content = new TextBlock { Text = "?", FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(4, 0, 4, 0) };
            help.Click += (_, __) => HelpRequested?.Invoke(this, EventArgs.Empty);

            var header = new DockPanel();
            DockPanel.SetDock(help, Dock.Right);
            DockPanel.SetDock(_slotInfo, Dock.Right);
            header.Children.Add(help);
            header.Children.Add(_slotInfo);
            header.Children.Add(new TextBlock { Style = (Style)FindResource("DashCardTitle"), Text = T("eo_card", "Extra rules") });
            var headerBorder = new Border { Style = (Style)FindResource("DashCardHeader"), Child = header };

            for (int t = 0; t < ExtraObjectives.Teams; t++)
            {
                int team = t;
                var tab = new ToggleButton { Style = (Style)FindResource("ChoiceTile"), MinHeight = 30, Height = 30, Padding = new Thickness(4, 0, 4, 0), Margin = new Thickness(0, 0, 4, 0), FontSize = 13 };
                tab.Click += (_, __) => { _team = team; Refresh(); };
                _teamTabs.Children.Add(tab);
            }

            foreach (var r in ExtraObjectives.RuleTypesFor(_type))
                _newType.Items.Add(new ComboBoxItem { Content = T("eo_r_" + r.Value, r.Name), Tag = r.Value });
            _newType.SelectedIndex = 0;

            var add = new Button { Style = (Style)FindResource("FormButtonPrimary"), Height = 30, Padding = new Thickness(12, 0, 12, 0), Content = "+ " + T("eo_add", "Add") };
            add.Click += (_, __) => AddRule();
            var addRow = new Grid();
            addRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
            addRow.ColumnDefinitions.Add(new ColumnDefinition());
            addRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(_newType, 1);
            Grid.SetColumn(add, 2);
            addRow.Children.Add(_newNumber);
            addRow.Children.Add(_newType);
            addRow.Children.Add(add);

            var body = new StackPanel { Margin = new Thickness(14, 12, 14, 14) };
            body.Children.Add(_teamTabs);
            body.Children.Add(new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = T("eo_cols", "Rule no. · type") });
            body.Children.Add(_list);
            body.Children.Add(new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = T("eo_new", "New extra rule"), Margin = new Thickness(0, 8, 0, 4) });
            body.Children.Add(addRow);
            body.Children.Add(_error);
            body.Children.Add(new TextBlock { FontSize = 12, Foreground = (Brush)FindResource("NavMutedBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0), Text = T("eo_card_hint", "Rule no. = the rule of the team's list in which this entity also takes part.") });

            var dock = new DockPanel();
            DockPanel.SetDock(headerBorder, Dock.Top);
            dock.Children.Add(headerBorder);
            dock.Children.Add(body);
            Child = dock;
            Refresh();
        }

        private int Index => _entries?.SelectedIndex ?? -1;

        public void Refresh()
        {
            if (!_built)
                return;
            _error.Text = "";
            int slot = ExtraObjectives.FindSlot(_type, Index);
            _slotInfo.Text = string.Format(CultureInfo.CurrentCulture, T("eo_slots", "{0} of 30 used"), ExtraObjectives.UsedSlots());
            for (int t = 0; t < ExtraObjectives.Teams; t++)
            {
                var tab = (ToggleButton)_teamTabs.Children[t];
                tab.Content = $"{T("team", "Team")} {t + 1} · {ExtraObjectives.Rules(slot, t).Count}";
                tab.IsChecked = t == _team;
            }

            _list.Children.Clear();
            var rules = ExtraObjectives.Rules(slot, _team);
            if (rules.Count == 0)
                _list.Children.Add(new TextBlock { Text = T("eo_none", "No extra rules for this team."), FontSize = 13, Foreground = (Brush)FindResource("NavMutedBrush"), Margin = new Thickness(0, 2, 0, 4) });
            int count = ExtraObjectives.TeamRuleCount(_team);
            foreach (var rule in rules)
            {
                _list.Children.Add(RuleRow(slot, rule));
                if (count > 0 && rule.Priority >= count)
                    _list.Children.Add(Warning(string.Format(CultureInfo.CurrentCulture, T("eo_warn_count", "Team {0} has only {1} rules."), _team + 1, count)));
            }
            int own = ExtraObjectives.OwnPriority(_type, Index, _team);
            if (rules.Count > 0 && (own < 0 || (count > 0 && own >= count)))
                _list.Children.Add(Warning(string.Format(CultureInfo.CurrentCulture, T("eo_warn_own", "This entity has no own rule for team {0}; its extra rules will not run."), _team + 1)));
            IsEnabled = MainWindow.m != null && MainWindow.m.IsProcOpen && Index >= 0;
        }

        private static UIElement Warning(string text) => new TextBlock
        {
            Text = text, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, -2, 0, 6),
            Foreground = MainWindow.ThemeBrush("WarnBrush"),
        };

        private UIElement RuleRow(int slot, ExtraObjectives.Rule rule)
        {
            var number = new TextBox { Style = (Style)FindResource("Watermark"), Height = 30, Text = (rule.Priority + 1).ToString(CultureInfo.InvariantCulture) };
            var type = new ComboBox { Height = 30, Margin = new Thickness(6, 0, 6, 0) };
            foreach (var r in ExtraObjectives.RuleTypesFor(_type))
                type.Items.Add(new ComboBoxItem { Content = T("eo_r_" + r.Value, r.Name), Tag = r.Value });
            type.SelectedItem = type.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == rule.Type);
            if (type.SelectedItem == null)
            {
                type.Items.Add(new ComboBoxItem { Content = rule.Type.ToString(CultureInfo.InvariantCulture), Tag = rule.Type });
                type.SelectedIndex = type.Items.Count - 1;
            }
            void Save()
            {
                if (int.TryParse(number.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int no) && no > 0 && type.SelectedItem is ComboBoxItem item)
                    ExtraObjectives.SetRule(slot, rule.Index, _team, (int)item.Tag, no - 1);
            }
            number.LostFocus += (_, __) => Save();
            type.SelectionChanged += (_, __) => Save();
            var delete = new Button { Style = (Style)FindResource("FieldIconButton"), ToolTip = T("eo_remove", "Remove"), Content = new TextBlock { Text = "✕", FontSize = 12 } };
            delete.Click += (_, __) => { ExtraObjectives.Remove(slot, rule.Index, _team); Refresh(); };

            var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(type, 1);
            Grid.SetColumn(delete, 2);
            row.Children.Add(number);
            row.Children.Add(type);
            row.Children.Add(delete);
            return row;
        }

        private void AddRule()
        {
            if (!int.TryParse(_newNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int no) || no < 1)
            {
                _error.Text = T("eo_err_number", "Enter the rule number (1 or higher).");
                return;
            }
            if (!(_newType.SelectedItem is ComboBoxItem item))
                return;
            string error = ExtraObjectives.Add(_type, Index, _team, no - 1, (int)item.Tag);
            Refresh();
            if (error != null)
                _error.Text = T(error, error == "eo_err_slots" ? "All 30 slots are used." : error == "eo_err_rules" ? "This entity has 13 extra rules for the team already." : "Select an entry first.");
        }
    }
}
