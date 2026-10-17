using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: copies the selected actor's goto route (points and route-wide settings)
    // to other actors of the job.
    public partial class MainWindow
    {
        /// <summary>
        /// The panel under the route settings: "Copy to other actors" opens a row of actor chips,
        /// an option to move the route along with each actor and the copy button.
        /// </summary>
        private FrameworkElement BuildGotoCopyPanel(Action copied)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            var open = new Button { Content = TranslateOr("gc_open", "Copy to other actors…"), Height = 30, Padding = new Thickness(12, 0, 12, 0), HorizontalAlignment = HorizontalAlignment.Left };
            open.SetResourceReference(StyleProperty, "FormButton");
            panel.Children.Add(open);

            var body = new StackPanel { Visibility = Visibility.Collapsed, Margin = new Thickness(0, 10, 0, 0) };
            var hint = new TextBlock { Text = TranslateOr("gc_hint", "Their goto points and route settings are replaced by this actor's."), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            body.Children.Add(hint);
            var chips = new WrapPanel();
            body.Children.Add(chips);
            var tools = new WrapPanel { Margin = new Thickness(0, 4, 0, 0) };
            var all = new Button { Content = TranslateOr("gc_all", "All"), Height = 26, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 6) };
            all.SetResourceReference(StyleProperty, "FormButton");
            var none = new Button { Content = TranslateOr("gc_none", "None"), Height = 26, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 6) };
            none.SetResourceReference(StyleProperty, "FormButton");
            tools.Children.Add(all);
            tools.Children.Add(none);
            body.Children.Add(tools);
            // A label left, the switch right, like the other option rows.
            var relative = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
            relative.SetResourceReference(StyleProperty, "FormToggle");
            var relativeRow = new DockPanel { Margin = new Thickness(0, 6, 0, 4) };
            DockPanel.SetDock(relative, Dock.Right);
            relativeRow.Children.Add(relative);
            var relativeText = new TextBlock { Text = TranslateOr("gc_relative", "Move the route with each actor"), FontSize = 13, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            relativeText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            relativeRow.Children.Add(relativeText);
            body.Children.Add(relativeRow);
            var relativeHint = new TextBlock { Text = TranslateOr("gc_relative_tip", "Off: every actor goes to the same places. On: the route keeps its shape and starts from each actor's own position."), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
            relativeHint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            body.Children.Add(relativeHint);
            var copy = new Button { Content = TranslateOr("gc_copy", "Copy"), Height = 30, Padding = new Thickness(14, 0, 14, 0), HorizontalAlignment = HorizontalAlignment.Left, IsEnabled = false };
            copy.SetResourceReference(StyleProperty, "FormButtonPrimary");
            body.Children.Add(copy);
            panel.Children.Add(body);

            void UpdateCopy() => copy.IsEnabled = chips.Children.OfType<CheckBox>().Any(c => c.IsChecked == true);

            void Fill()
            {
                chips.Children.Clear();
                int count = m.IsProcOpen && GTA.Offsets.Editor.Actor.number != 0 ? new Global(GTA.Offsets.Editor.Actor.number).Get<int>() : 0;
                for (int i = 0; i < count && i < 1000; i++)
                {
                    if (i == ddactorno.SelectedIndex)
                        continue;
                    var chip = new CheckBox { Content = (i + 1).ToString(CultureInfo.CurrentCulture), Tag = i, Margin = new Thickness(0, 0, 6, 6), MinWidth = 40 };
                    chip.SetResourceReference(StyleProperty, "ChipToggle");
                    chip.Click += (_, __) => UpdateCopy();
                    chips.Children.Add(chip);
                }
                if (chips.Children.Count == 0)
                {
                    var t = new TextBlock { Text = TranslateOr("gc_noother", "There is no other actor."), FontSize = 12 };
                    t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                    chips.Children.Add(t);
                }
                UpdateCopy();
            }

            open.Click += (_, __) =>
            {
                body.Visibility = body.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                if (body.Visibility == Visibility.Visible)
                    Fill();
            };
            all.Click += (_, __) => { foreach (var c in chips.Children.OfType<CheckBox>()) c.IsChecked = true; UpdateCopy(); };
            none.Click += (_, __) => { foreach (var c in chips.Children.OfType<CheckBox>()) c.IsChecked = false; UpdateCopy(); };
            ddactorno.SelectionChanged += (_, __) => { if (body.Visibility == Visibility.Visible) Fill(); };
            copy.Click += async (_, __) =>
            {
                int from = ddactorno.SelectedIndex;
                var targets = chips.Children.OfType<CheckBox>().Where(c => c.IsChecked == true).Select(c => (int)c.Tag).ToList();
                if (from < 0 || targets.Count == 0 || !m.IsProcOpen)
                    return;
                if (!await ConfirmAsync(TranslateOr("gc_title", "Copy goto route"),
                    string.Format(CultureInfo.CurrentCulture, TranslateOr("gc_confirm", "Copy the goto route of actor {0} to {1} actors? Their own goto points are replaced."), from + 1, targets.Count),
                    TranslateOr("gc_copy", "Copy"), TranslateOr("dialog_cancel", "Cancel"), danger: true))
                    return;
                foreach (int to in targets)
                    CopyGotoRoute(from, to, relative.IsChecked == true);
                body.Visibility = Visibility.Collapsed;
                displayScreenMessage(string.Format(CultureInfo.CurrentCulture, TranslateOr("gc_done", "Goto route copied to {0} actors."), targets.Count));
                copied?.Invoke();
            };
            return panel;
        }

        /// <summary>
        /// Copies every goto point (all slots of the 12 points, the bit array's size slot left
        /// alone) and the route-wide settings: vehicle speed (gtds), loop / heli at destination /
        /// rappel at destination (pedbs bits 15, 13, 14) and the pbs13 bit 9 option. With
        /// <paramref name="relative"/> the point and trigger positions move by the offset between
        /// the two actors.
        /// </summary>
        private static void CopyGotoRoute(int from, int to, bool relative)
        {
            long next = GTA.Offsets.Editor.Actor.NEXT, pointNext = GTA.Offsets.Editor.Actor.actv_NEXT;
            if (next == 0 || pointNext == 0 || GTA.Offsets.Editor.Actor.actv_bs == 0 || from == to)
                return;
            float dx = 0, dy = 0, dz = 0;
            if (relative)
            {
                dx = new Global(GTA.Offsets.Editor.Actor.locx + next * to).Get<float>() - new Global(GTA.Offsets.Editor.Actor.locx + next * from).Get<float>();
                dy = new Global(GTA.Offsets.Editor.Actor.locy + next * to).Get<float>() - new Global(GTA.Offsets.Editor.Actor.locy + next * from).Get<float>();
                dz = new Global(GTA.Offsets.Editor.Actor.locz + next * to).Get<float>() - new Global(GTA.Offsets.Editor.Actor.locz + next * from).Get<float>();
            }
            long start = GTA.Offsets.Editor.Actor.actv_bs - 1, awl = GTA.Offsets.Editor.Actor.awl;
            var d = new[] { dx, dy, dz };
            for (int p = 0; p < GotoRouteView.Points; p++)
            {
                long src = next * from + p * pointNext, dst = next * to + p * pointNext;
                // A position 0,0,0 means "not set" and stays so.
                bool point = relative && (new Global(GTA.Offsets.Editor.Actor.actvx + src).Get<float>() != 0 || new Global(GTA.Offsets.Editor.Actor.actvy + src).Get<float>() != 0);
                bool trigger = relative && awl != 0 && (new Global(awl + src).Get<float>() != 0 || new Global(awl + 1 + src).Get<float>() != 0);
                for (long f = 1; f < pointNext; f++)
                {
                    long field = start + f;
                    int axis = field == GTA.Offsets.Editor.Actor.actvx ? 0 : field == GTA.Offsets.Editor.Actor.actvy ? 1 : field == GTA.Offsets.Editor.Actor.actvz ? 2 : -1;
                    bool shifted = axis >= 0 ? point : awl != 0 && field >= awl && field <= awl + 2 && trigger;
                    if (axis < 0 && shifted)
                        axis = (int)(field - awl);
                    if (shifted)
                        new Global(field + dst).SetFloat(new Global(field + src).Get<float>() + d[axis]);
                    else
                        new Global(field + dst).SetLong(new Global(field + src).Get<long>());
                }
            }
            if (GTA.Offsets.Editor.Actor.gtds != 0)
                new Global(GTA.Offsets.Editor.Actor.gtds + next * to).SetInt(new Global(GTA.Offsets.Editor.Actor.gtds + next * from).Get<int>());
            CopyBit(GTA.Offsets.Editor.Actor.pedbs, next, from, to, 13);
            CopyBit(GTA.Offsets.Editor.Actor.pedbs, next, from, to, 14);
            CopyBit(GTA.Offsets.Editor.Actor.pedbs, next, from, to, 15);
            CopyBit(GTA.Offsets.Editor.Actor.pbs13, next, from, to, 9);
        }

        private static void CopyBit(long field, long next, int from, int to, int bit)
        {
            if (field == 0)
                return;
            bool on = (new Global(field + next * from).Get<int>() & (1 << bit)) != 0;
            var g = new Global(field + next * to);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << bit) : v & ~(1 << bit));
        }
    }
}
