using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    /// <summary>
    /// SMS page: the message's further options with the Mission Creator's names instead of raw
    /// values (what each does: <see cref="Sms"/>). Sender, all teams, points, dialogue trigger,
    /// sending later, game text label (with the date), and the mission flag while it is open.
    /// </summary>
    public class SmsOptionsCard : Border
    {
        // Two columns when the card is wide (beside the message), one when narrow.
        private readonly System.Windows.Controls.Primitives.UniformGrid _body = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2, Margin = new Thickness(14, 12, 0, 6) };
        private Func<int> _index = () => -1;
        private string _shown;
        private bool _loading;

        public SmsOptionsCard()
        {
            SetResourceReference(StyleProperty, "DashCard");
            var title = new TextBlock { Text = T("sms_more", "When and how it arrives") };
            title.SetResourceReference(StyleProperty, "DashCardTitle");
            var header = new Border { Child = title };
            header.SetResourceReference(StyleProperty, "DashCardHeader");
            var dock = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            dock.Children.Add(_body);
            Child = dock;
            SizeChanged += (_, __) => _body.Columns = ActualWidth >= 640 ? 2 : 1;
        }

        public void Bind(Func<int> index) => _index = index;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public void Refresh(bool force = false)
        {
            int i = _index();
            bool ready = i >= 0 && i < Sms.Count && Sms.Ready;
            IsEnabled = ready;
            if (!ready)
            {
                _shown = null;
                _body.Children.Clear();
                return;
            }
            string state = string.Join("|", i, Sms.Get(GTA.Offsets.Editor.SMS.smsc, i), Sms.Get(GTA.Offsets.Editor.SMS.sndall, i), Sms.Get(GTA.Offsets.Editor.SMS.ptsreq, i),
                Sms.Get(GTA.Offsets.Editor.SMS.smsdt, i), Sms.Get(GTA.Offsets.Editor.SMS.smstl, i), Sms.Get(GTA.Offsets.Editor.SMS.smsei, i), Sms.Get(GTA.Offsets.Editor.SMS.smspwo, i));
            if (!force && (state == _shown || IsKeyboardFocusWithin))
                return;
            _shown = state;
            _loading = true;
            try { Build(i); }
            finally { _loading = false; }
        }

        private void Build(int i)
        {
            _body.Children.Clear();
            // Sender: -1 the job's contact, 0..24 the controller's character list.
            var sender = new ComboBox { Height = 28 };
            sender.Items.Add(new ComboBoxItem { Content = T("sms_sender_job", "Job contact (default)"), Tag = -1 });
            for (int c = 0; c <= 24; c++)
                sender.Items.Add(new ComboBoxItem { Content = T("sms_sender_n", "Character") + " " + c.ToString(CultureInfo.CurrentCulture), Tag = c });
            Select(sender, Sms.Get(GTA.Offsets.Editor.SMS.smsc, i));
            sender.SelectionChanged += (_, __) => { if (!_loading && sender.SelectedItem is ComboBoxItem it) Sms.Set(GTA.Offsets.Editor.SMS.smsc, i, (int)it.Tag); };
            Row(T("sms_sender", "Sender"), sender, T("sms_sender_help", "Unknown Contact (below, for all messages) wins over this. Characters 4 and up only in the 2020 mission controller."));

            Row(RuleOptions.GameText("FMMC_SMSSTA_T", "Send To All Teams"),
                Toggle(Sms.ToAllTeams(i), on => Sms.Set(GTA.Offsets.Editor.SMS.sndall, i, on ? 1 : 0)),
                T("sms_all_help", "The team's rule still decides when; every team gets the message."));

            int points = Sms.Get(GTA.Offsets.Editor.SMS.ptsreq, i);
            var pts = new TextBox { Height = 28, Text = points < 0 ? "" : points.ToString(CultureInfo.InvariantCulture), Tag = T("sms_off", "off") };
            pts.SetResourceReference(StyleProperty, "Watermark");
            pts.TextChanged += (_, __) =>
            {
                if (_loading) return;
                if (pts.Text.Trim().Length == 0)
                    Sms.Set(GTA.Offsets.Editor.SMS.ptsreq, i, -1);
                else if (int.TryParse(pts.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= 0)
                    Sms.Set(GTA.Offsets.Editor.SMS.ptsreq, i, v);
            };
            Row(RuleOptions.GameText("FMMC_SMSPT", "Points Required"), pts, T("sms_points_help", "Only once the team has at least this many points. Empty = off."));

            var trigger = new ComboBox { Height = 28 };
            trigger.Items.Add(new ComboBoxItem { Content = T("sms_off", "off"), Tag = -1 });
            for (int d = 0; d < 32; d++)
                trigger.Items.Add(new ComboBoxItem { Content = (d + 1).ToString(CultureInfo.CurrentCulture), Tag = d });
            Select(trigger, Sms.Get(GTA.Offsets.Editor.SMS.smsdt, i));
            trigger.SelectionChanged += (_, __) => { if (!_loading && trigger.SelectedItem is ComboBoxItem it) Sms.Set(GTA.Offsets.Editor.SMS.smsdt, i, (int)it.Tag); };
            Row(RuleOptions.GameText("FMMC_SMSDT", "Trigger from Dialogue Trigger"), trigger, T("sms_dt_help", "Also sent as soon as this dialogue trigger has played, whatever the rule."));

            Row(T("sms_later", "Send later when the phone is busy"),
                Toggle(Sms.Bit(GTA.Offsets.Editor.SMS.smstl, i, 2), on => Sms.SetBit(GTA.Offsets.Editor.SMS.smstl, i, 2, on)),
                T("sms_later_help", "Otherwise a message the phone cannot take at that moment (call, cutscene) is lost."));

            bool label = Sms.Bit(GTA.Offsets.Editor.SMS.smstl, i, 0);
            Row(T("sms_label", "Text is a game text label"),
                Toggle(label, on => { Sms.SetBit(GTA.Offsets.Editor.SMS.smstl, i, 0, on); Refresh(true); }),
                T("sms_label_help", "The message box then holds a label such as a Rockstar text name, and the game shows its text in the player's language."));
            if (label)
                Row(T("sms_date", "Add the date"),
                    Toggle(Sms.Get(GTA.Offsets.Editor.SMS.smsei, i) == 1, on => Sms.Set(GTA.Offsets.Editor.SMS.smsei, i, on ? 1 : 0)),
                    T("sms_date_help", "Puts today's date (dd-mm-yy) into the label's text. 2020 mission controller only."));

            var flag = new ComboBox { Height = 28 };
            flag.Items.Add(new ComboBoxItem { Content = T("sms_off", "off"), Tag = -1 });
            for (int m = 0; m < RuleOptions.FlagCount; m++)
                flag.Items.Add(new ComboBoxItem { Content = RuleOptions.FlagName(m), Tag = m });
            Select(flag, Sms.Get(GTA.Offsets.Editor.SMS.smspwo, i));
            flag.SelectionChanged += (_, __) => { if (!_loading && flag.SelectedItem is ComboBoxItem it) Sms.Set(GTA.Offsets.Editor.SMS.smspwo, i, (int)it.Tag); };
            Row(T("sms_flag", "Mission flag while it is open"), flag, T("sms_flag_help", "On while a player has this message open on the phone, off again when they close it. Rules can wait for it (Rules page, Mission Flags). 2020 mission controller only."));
        }

        private static void Select(ComboBox box, int value)
        {
            foreach (ComboBoxItem it in box.Items)
                if ((int)it.Tag == value) { box.SelectedItem = it; return; }
            var extra = new ComboBoxItem { Content = value.ToString(CultureInfo.InvariantCulture), Tag = value };
            box.Items.Add(extra);
            box.SelectedItem = extra;
        }

        private CheckBox Toggle(bool on, Action<bool> write)
        {
            var c = new CheckBox { IsChecked = on };
            c.SetResourceReference(StyleProperty, "FormToggle");
            c.Click += (_, __) => { if (!_loading && Sms.Ready) write(c.IsChecked == true); };
            return c;
        }

        // A label on the left, the editor on the right, the help text under both.
        private void Row(string label, FrameworkElement editor, string help)
        {
            var cell = new StackPanel { Margin = new Thickness(0, 0, 18, 8) };
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 2) };
            editor.VerticalAlignment = VerticalAlignment.Center;
            if (!(editor is CheckBox))
                editor.Width = 150;
            DockPanel.SetDock(editor, Dock.Right);
            row.Children.Add(editor);
            var l = new TextBlock { Text = label, FontSize = 13, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            l.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            row.Children.Add(l);
            cell.Children.Add(row);
            if (!string.IsNullOrEmpty(help))
            {
                var h = new TextBlock { Text = help, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) };
                h.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                cell.Children.Add(h);
            }
            _body.Children.Add(cell);
        }
    }
}
