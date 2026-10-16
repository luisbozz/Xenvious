using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// "Go-to range" card of the vehicle and actor pages (GotoRanges; mockup
    /// https://claude.ai/artifact/SgSyu4ZDzMyg6DKABv3irT): the selected entity's own range for a
    /// "Go to" rule, with presets and how far away you are now. Greyed out while the entity's
    /// own rule for the team is no "Go to"; the team and rule come from the page's rules card.
    /// </summary>
    public class GotoRangeCard : SectionCard
    {
        private static readonly int[] Presets = { 10, 25, 50, 100, 250, 500, 1000 };

        private EntityRulesCard _rules;
        private ComboBox _entries;
        private Rules.Kind _kind;
        private bool _built, _sync, _isGoto;

        private readonly StackPanel _body = new StackPanel();
        private readonly TextBlock _hint = Text("", 12.5, "MutedTextBrush");
        private readonly CheckBox _on = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
        private readonly TextBox _metres = new TextBox { Width = 90, Height = 30, Tag = "m" };
        private readonly WrapPanel _presets = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        private readonly FrameworkElement _editRow;
        private readonly Border _warn = new Border { CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 7, 10, 7), Margin = new Thickness(0, 10, 0, 0) };
        private readonly TextBlock _live = Text("", 12.5, "MutedTextBrush");
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        private static TextBlock Text(string text, double size, string brush)
        {
            var t = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            t.SetResourceReference(TextBlock.ForegroundProperty, brush);
            return t;
        }

        public GotoRangeCard()
        {
            Title = T("gr_card", "Go-to range");
            Icon = Geometry.Parse("M12,2 A10,10 0 1 0 12.01,2 Z M12,8 A4,4 0 1 0 12.01,8 Z");
            Content = _body;
            // An implicit style only matches its exact type, so this subclass asks for SectionCard's look.
            SetResourceReference(StyleProperty, typeof(SectionCard));

            _on.SetResourceReference(StyleProperty, "FormToggle");
            _metres.SetResourceReference(StyleProperty, "Watermark");
            var row = new DockPanel { Margin = new Thickness(0, 10, 0, 0) };
            DockPanel.SetDock(_on, Dock.Right);
            row.Children.Add(_on);
            var label = Text(T("gr_own", "Own range"), 13, "TextColor");
            label.VerticalAlignment = VerticalAlignment.Center;
            row.Children.Add(label);

            var edit = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
            var line = new StackPanel { Orientation = Orientation.Horizontal };
            line.Children.Add(_metres);
            var unit = Text("m", 13, "MutedTextBrush");
            unit.VerticalAlignment = VerticalAlignment.Center;
            unit.Margin = new Thickness(8, 0, 0, 0);
            line.Children.Add(unit);
            edit.Children.Add(line);
            foreach (int p in Presets)
            {
                int metres = p;
                var b = new Button { Content = p.ToString(CultureInfo.CurrentCulture), Height = 26, MinWidth = 40, Padding = new Thickness(8, 0, 8, 0), Margin = new Thickness(0, 0, 5, 5) };
                b.SetResourceReference(StyleProperty, "FormButton");
                b.Click += (_, __) => Write(metres);
                _presets.Children.Add(b);
            }
            edit.Children.Add(_presets);
            _editRow = edit;

            var warnText = Text(T("gr_inveh", "The rule has \"be in vehicle\" on: then only getting in counts, the range is ignored."), 12.5, "WarnBrush");
            _warn.Child = warnText;
            _warn.Background = new SolidColorBrush(Color.FromArgb(0x2E, 0xFA, 0xA6, 0x1A));

            _body.Children.Add(_hint);
            _body.Children.Add(row);
            _body.Children.Add(_editRow);
            _body.Children.Add(_warn);
            _live.Margin = new Thickness(0, 10, 0, 0);
            _body.Children.Add(_live);
            var note = Text(T("gr_note", "Job field gotor. The creator has no menu for it, but saves and loads it with the job."), 11.5, "FaintTextBrush");
            note.Margin = new Thickness(0, 10, 0, 0);
            _body.Children.Add(note);

            _on.Click += (_, __) =>
            {
                if (_sync) return;
                Write(_on.IsChecked == true ? Math.Max(Current(), 50) : 0);
            };
            _metres.LostKeyboardFocus += (_, __) => CommitText();
            _metres.KeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Enter) CommitText(); };

            Loaded += (_, __) => { _built = true; Refresh(); };
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Refresh(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => { if (!IsKeyboardFocusWithin) Refresh(); else ShowLive(); };
        }

        /// <summary>The page's entity list, its rules card (team and rule) and what kind of entity it lists.</summary>
        public void Attach(ComboBox entries, EntityRulesCard rules, Rules.Kind kind)
        {
            _entries = entries;
            _rules = rules;
            _kind = kind;
            entries.SelectionChanged += (_, __) => Refresh();
            rules.Refreshed += (_, __) => Refresh();
            rules.TeamChanged += (_, __) => Refresh();
        }

        private int Index => _entries?.SelectedIndex ?? -1;
        private int Current() => GotoRanges.Get(_kind, Index);

        private void Write(int metres)
        {
            if (!GotoRanges.Available(_kind) || Index < 0)
                return;
            GotoRanges.Set(_kind, Index, metres);
            Refresh();
        }

        private void CommitText()
        {
            if (_sync)
                return;
            string text = _metres.Text.Trim();
            if (text.Length == 0)
                Write(0);
            else if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                Write(v);
        }

        private void Refresh()
        {
            if (!_built || _entries == null)
                return;
            bool ready = GotoRanges.Available(_kind) && Index >= 0 && Rules.Ready;
            int team = _rules?.Team ?? 0, rule = _rules?.MainRule ?? -1;
            _isGoto = false;
            if (ready && rule >= 0)
            {
                var rules = Rules.Read(team);
                var link = rule < rules.Count ? rules[rule].Links.FirstOrDefault(l => l.Kind == _kind && l.Index == Index && !l.Extra) : null;
                _isGoto = link != null && link.Type == 4;
            }
            int metres = ready ? Current() : 0;

            _sync = true;
            try
            {
                _on.IsChecked = metres > 0;
                if (!_metres.IsKeyboardFocusWithin)
                    _metres.Text = metres > 0 ? metres.ToString(CultureInfo.InvariantCulture) : "";
            }
            finally { _sync = false; }

            _hint.Text = !ready ? T("gor_none", "Open a creator and pick a vehicle or actor.")
                : _isGoto ? T("gor_hint", "On a \"Go to\" rule the target counts as reached as soon as a player of the team is this close, without driving up to it. Good as an invisible trigger: arriving in the area starts the next rule with its text messages.")
                : string.Format(CultureInfo.CurrentCulture, T("gr_not_goto", "Only for \"Go to\" rules. For team {0} this entity has no \"Go to\" rule, so the range does nothing."), team + 1);
            _body.IsEnabled = ready;
            foreach (UIElement part in _body.Children)
                if (part != _hint)
                    part.Opacity = _isGoto ? 1 : 0.5;
            _editRow.Opacity = _isGoto && metres > 0 ? 1 : 0.5;
            _warn.Visibility = _isGoto && metres > 0 && _kind == Rules.Kind.Vehicle && GotoRanges.InVehicleRule(team, rule) ? Visibility.Visible : Visibility.Collapsed;
            HeaderRight = !ready ? null : EntityRulesCard.SummaryChip(metres > 0 ? metres.ToString(CultureInfo.CurrentCulture) + " m" : T("gr_normal", "normal"), metres > 0 && _isGoto ? "ok" : "plain");
            ShowLive();
        }

        // How far you are from the entity now, and whether that counts.
        private void ShowLive()
        {
            int metres = Current();
            var target = GotoRanges.Position(_kind, Index);
            var me = PlayerPosition();
            if (!_isGoto || metres <= 0 || target == null || me == null)
            {
                _live.Visibility = Visibility.Collapsed;
                return;
            }
            float d = Vector3.Distance(me.Value, target.Value);
            bool inside = d <= metres;
            _live.Visibility = Visibility.Visible;
            _live.Text = "● " + string.Format(CultureInfo.CurrentCulture, inside
                ? T("gr_live_in", "You are {0:0} m away: in range, the rule would count as reached.")
                : T("gr_live_out", "You are {0:0} m away: out of range."), d);
            _live.SetResourceReference(TextBlock.ForegroundProperty, inside ? "OkBrush" : "MutedTextBrush");
        }

        private static Vector3? PlayerPosition()
        {
            try
            {
                long at = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_player_loc).GetAddress();
                var v = new Vector3(MainWindow.m.memory(at.ToString("X")).Get<float>(), MainWindow.m.memory((at + 4).ToString("X")).Get<float>(), MainWindow.m.memory((at + 8).ToString("X")).Get<float>());
                return v == Vector3.Zero ? (Vector3?)null : v;
            }
            catch
            {
                return null;
            }
        }
    }
}
