using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    // The rule list's flow (mockup: https://claude.ai/artifact/A2uzrBQee93oqKx8ue9P2m): jumps as
    // arrows in lanes left of the rules, rules the team only reaches on failure or never reaches
    // indented, and the team's way through the rules on top.
    public partial class RulesView
    {
        private const double LaneWidth = 16, Indent = 46, RowGap = 22;

        private RuleFlow _flow;
        private bool _flowWired;
        private readonly Grid _flowGrid = new Grid();
        private readonly StackPanel _rows = new StackPanel();
        private readonly Canvas _arrows = new Canvas { IsHitTestVisible = false };
        private readonly List<(FrameworkElement Row, FrameworkElement Num)> _parts = new List<(FrameworkElement, FrameworkElement)>();

        private void ShowFlow()
        {
            if (!_flowWired)
            {
                _flowWired = true;
                _flowGrid.Children.Add(_rows);
                _flowGrid.Children.Add(_arrows);
                // Rows wrap and grow with the window; the arrows follow them.
                _rows.SizeChanged += (_, __) => DrawArrows();
            }
            _flow = RuleFlow.Build(_rules);
            _rows.Children.Clear();
            _arrows.Children.Clear();
            _parts.Clear();

            if (_flow.HasJumps)
                _list.Children.Add(PathSummary());
            _rows.Margin = new Thickness(_flow.Lanes == 0 ? 0 : _flow.Lanes * LaneWidth + 22, 0, 0, 0);
            foreach (var rule in _rules)
            {
                var row = RuleRow(rule, out var num);
                row.Margin = new Thickness(row.Margin.Left, 0, 0, RowGap);
                _rows.Children.Add(row);
                _parts.Add((row, num));
            }
            var end = EndRow(out var endNum);
            _rows.Children.Add(end);
            _parts.Add((end, endNum));
            _list.Children.Add(_flowGrid);
            Dispatcher.BeginInvoke(new Action(DrawArrows), DispatcherPriority.Loaded);
        }

        /// <summary>Rules off the team's way sit further right; a never reached one gets a dashed frame.</summary>
        private static FrameworkElement Indented(Border row, RuleFlow.Reach reach, bool selected)
        {
            if (reach == RuleFlow.Reach.Main)
                return row;
            var holder = new Grid { Margin = new Thickness(Indent, 0, 0, 0) };
            if (reach == RuleFlow.Reach.Never && !selected)
            {
                row.Background = Brushes.Transparent;
                row.BorderBrush = Brushes.Transparent;
                var dashed = new Rectangle { RadiusX = 8, RadiusY = 8, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 4, 3 } };
                dashed.SetResourceReference(Shape.StrokeProperty, "LineBrush");
                holder.Children.Add(dashed);
            }
            holder.Children.Add(row);
            return holder;
        }

        private FrameworkElement EndRow(out FrameworkElement num)
        {
            var circle = new Grid { Width = 36, Height = 36 };
            var tint = new Border { CornerRadius = new CornerRadius(18), Opacity = 0.16 };
            tint.SetResourceReference(Border.BackgroundProperty, "OkBrush");
            var ring = new Border { CornerRadius = new CornerRadius(18), BorderThickness = new Thickness(1) };
            ring.SetResourceReference(Border.BorderBrushProperty, "OkBrush");
            var mark = new TextBlock { Text = "✓", FontWeight = FontWeights.Bold, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            mark.SetResourceReference(TextBlock.ForegroundProperty, "OkBrush");
            circle.Children.Add(tint);
            circle.Children.Add(ring);
            circle.Children.Add(mark);
            var text = new TextBlock { Text = T("rl_flow_end", "End"), FontWeight = FontWeights.Bold, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
            text.SetResourceReference(TextBlock.ForegroundProperty, "OkBrush");
            // Same inset as a rule's number, so the line from the last rule runs straight down.
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(13, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
            row.Children.Add(circle);
            row.Children.Add(text);
            num = circle;
            return row;
        }

        // ----- chips under a rule -----

        private WrapPanel FlowChips(int rule)
        {
            var panel = new WrapPanel { Margin = new Thickness(0, 2, 0, 0) };
            string Rule(int index) => string.Format(CultureInfo.CurrentCulture, T("rl_rule_n", "Rule {0}"), index + 1);
            switch (_flow.Reached[rule])
            {
                case RuleFlow.Reach.Never:
                    string why = _flow.SkippedBy[rule] < 0 ? T("rl_skip_never", "never reached")
                        : string.Format(CultureInfo.CurrentCulture, T("rl_skip_by", "skipped: rule {0} goes on with rule {1}"), _flow.SkippedBy[rule] + 1, _flow.SkippedTo[rule] + 1);
                    panel.Children.Add(FlowChip("↷ " + why, "WarnBrush", T("rl_skip_tip", "The team never gets here: an earlier rule jumps past it and nothing leads back.")));
                    break;
                case RuleFlow.Reach.Branch:
                    string from = _flow.BranchFrom[rule] < 0 ? "?" : (_flow.BranchFrom[rule] + 1).ToString(CultureInfo.CurrentCulture);
                    panel.Children.Add(FlowChip(string.Format(CultureInfo.CurrentCulture, T("rl_branch_by", "only through ✗ of rule {0}"), from), "BadBrush",
                        T("rl_branch_tip", "The team only gets here when an earlier objective fails.")));
                    break;
            }
            var edges = _flow.Edges.Where(e => e.From == rule).ToList();
            foreach (var e in edges.Where(e => e.Kind == RuleFlow.Kind.Pass))
                panel.Children.Add(FlowChip("✓ → " + Rule(e.To), "OkBrush", T("rl_flow_pass_tip", "Jump when the objective is passed")));
            foreach (var kind in new[] { RuleFlow.Kind.Next, RuleFlow.Kind.NextIgnored })
            {
                var next = edges.Where(e => e.Kind == kind).Select(e => e.To).ToList();
                if (next.Count == 0)
                    continue;
                string text = "⇢ " + (next.Count == 1 ? Rule(next[0])
                    : string.Format(CultureInfo.CurrentCulture, T("rl_flow_random", "one of {0} at random"), string.Join(" / ", next.Select(b => (b + 1).ToString(CultureInfo.CurrentCulture)))));
                panel.Children.Add(kind == RuleFlow.Kind.Next
                    ? FlowChip(text, "AccentBrush", T("rl_flow_next_tip", "Next-objective override: the team goes on with this rule"))
                    : IgnoredChip(text));
            }
            foreach (var e in edges.Where(e => e.Kind == RuleFlow.Kind.Fail))
                panel.Children.Add(FlowChip("✗ → " + Rule(e.To), "BadBrush", T("rl_flow_fail_tip", "Jump when the objective is failed")));
            return panel.Children.Count == 0 ? null : panel;
        }

        private static FrameworkElement FlowChip(string text, string brush, string tip)
        {
            var chip = new Grid { Margin = new Thickness(0, 0, 6, 4), ToolTip = tip };
            var tint = new Border { CornerRadius = new CornerRadius(11), Opacity = 0.16 };
            tint.SetResourceReference(Border.BackgroundProperty, brush);
            var t = new TextBlock { Text = text, FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(9, 1, 9, 2) };
            t.SetResourceReference(TextBlock.ForegroundProperty, brush);
            chip.Children.Add(tint);
            chip.Children.Add(t);
            return chip;
        }

        /// <summary>A next-objective override that a ✓ jump on the same rule beats in game.</summary>
        private static FrameworkElement IgnoredChip(string text)
        {
            var chip = new Grid { Margin = new Thickness(0, 0, 6, 4),
                ToolTip = T("rl_flow_noeffect_tip", "A ✓ jump is set on this rule. In the game the jump wins, so the next objective does nothing.") };
            var frame = new Rectangle { RadiusX = 11, RadiusY = 11, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 2 } };
            frame.SetResourceReference(Shape.StrokeProperty, "FaintTextBrush");
            var t = new TextBlock { FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(9, 1, 9, 2) };
            t.Inlines.Add(new Run(text) { TextDecorations = TextDecorations.Strikethrough });
            t.Inlines.Add(new Run("  " + T("rl_flow_noeffect", "no effect")));
            t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            chip.Children.Add(frame);
            chip.Children.Add(t);
            return chip;
        }

        // ----- the team's way on top -----

        private enum Pill { Plain, Ok, Bad, End }

        private FrameworkElement PathSummary()
        {
            var box = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };
            WrapPanel Line(string label)
            {
                var line = new WrapPanel { Margin = new Thickness(0, 0, 0, 5) };
                var t = new TextBlock { Text = label, FontSize = 12.5, Width = 120, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
                t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                line.Children.Add(t);
                box.Children.Add(line);
                return line;
            }
            void Steps(WrapPanel line, List<(int Rule, RuleFlow.Kind Via)> path, Pill kind, bool first)
            {
                foreach (var (rule, via) in path)
                {
                    if (!first)
                        line.Children.Add(Sep(via));
                    first = false;
                    line.Children.Add(rule >= _flow.Count ? PathPill(T("rl_flow_end", "End"), Pill.End) : PathPill((rule + 1).ToString(CultureInfo.CurrentCulture), kind));
                }
            }

            var main = _flow.Path(0);
            Steps(Line(T("rl_path_pass", "Passed")), main, Pill.Ok, true);
            // One line per ✗ jump on the way, at most three.
            foreach (var fail in main.Where(p => p.Rule < _flow.Count)
                .SelectMany(p => _flow.Edges.Where(e => e.From == p.Rule && e.Kind == RuleFlow.Kind.Fail)).Take(3))
            {
                var line = Line(string.Format(CultureInfo.CurrentCulture, T("rl_path_fail", "Fail at {0}"), fail.From + 1));
                line.Children.Add(PathPill((fail.From + 1).ToString(CultureInfo.CurrentCulture), Pill.Bad));
                line.Children.Add(Sep(RuleFlow.Kind.Fail));
                var rest = _flow.Path(fail.To);
                line.Children.Add(PathPill((fail.To + 1).ToString(CultureInfo.CurrentCulture), Pill.Bad));
                Steps(line, rest.Skip(1).ToList(), Pill.Plain, false);
            }
            var never = Enumerable.Range(0, _flow.Count).Where(r => _flow.Reached[r] == RuleFlow.Reach.Never).ToList();
            if (never.Count > 0)
            {
                var line = Line(T("rl_path_never", "Never reached"));
                foreach (int r in never)
                    line.Children.Add(PathPill((r + 1).ToString(CultureInfo.CurrentCulture), Pill.Plain));
            }
            return box;
        }

        private static FrameworkElement PathPill(string text, Pill kind)
        {
            string brush = kind == Pill.Ok || kind == Pill.End ? "OkBrush" : kind == Pill.Bad ? "BadBrush" : null;
            var pill = new Grid { MinWidth = 24, Height = 22, Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center };
            var bg = new Border { CornerRadius = new CornerRadius(11) };
            if (kind == Pill.End)
                bg.SetResourceReference(Border.BackgroundProperty, "OkBrush");
            else if (brush != null)
            {
                bg.Opacity = 0.16;
                bg.SetResourceReference(Border.BackgroundProperty, brush);
            }
            else
                bg.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            var frame = new Border { CornerRadius = new CornerRadius(11), BorderThickness = new Thickness(1) };
            frame.SetResourceReference(Border.BorderBrushProperty, brush ?? "LineBrush");
            var t = new TextBlock { Text = text, FontSize = 12, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(7, 0, 7, 1) };
            if (kind == Pill.End) t.Foreground = Brushes.White;
            else t.SetResourceReference(TextBlock.ForegroundProperty, brush ?? "TextColor");
            pill.Children.Add(bg);
            pill.Children.Add(frame);
            pill.Children.Add(t);
            return pill;
        }

        private static TextBlock Sep(RuleFlow.Kind via)
        {
            var t = new TextBlock { FontSize = 12, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(1, 0, 5, 0) };
            t.Text = via == RuleFlow.Kind.Pass ? "✓" : via == RuleFlow.Kind.Next ? "⇢" : via == RuleFlow.Kind.Fail ? "✗" : "›";
            t.SetResourceReference(TextBlock.ForegroundProperty, via == RuleFlow.Kind.Pass ? "OkBrush" : via == RuleFlow.Kind.Next ? "AccentBrush" : via == RuleFlow.Kind.Fail ? "BadBrush" : "FaintTextBrush");
            return t;
        }

        // ----- the lines -----

        private void DrawArrows()
        {
            _arrows.Children.Clear();
            if (_flow == null || _parts.Count != _flow.Count + 1 || !_flowGrid.IsVisible)
                return;
            var boxes = _parts.Select(p =>
            {
                var top = p.Row.TranslatePoint(new Point(0, 0), _flowGrid);
                var c = p.Num.TranslatePoint(new Point(p.Num.ActualWidth / 2, p.Num.ActualHeight / 2), _flowGrid);
                return (Left: top.X, Top: top.Y, Bottom: top.Y + p.Row.ActualHeight, Cx: c.X, Cy: c.Y, NumTop: c.Y - p.Num.ActualHeight / 2, NumBottom: c.Y + p.Num.ActualHeight / 2);
            }).ToList();
            // The end sits under the rules' numbers, so the line to it runs straight down.
            var endRow = _parts[_flow.Count].Row;
            double shift = boxes[0].Cx - boxes[_flow.Count].Cx;
            if (Math.Abs(shift) > 0.5 && _flow.Reached[0] == RuleFlow.Reach.Main)
            {
                endRow.Margin = new Thickness(endRow.Margin.Left + shift, 0, 0, 0);
                Dispatcher.BeginInvoke(new Action(DrawArrows), DispatcherPriority.Loaded);
                return;
            }
            bool anySelected = _selected >= 0 && _selected < _flow.Count;
            double gutter = _rows.Margin.Left;
            string F(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);

            foreach (var e in _flow.Edges)
            {
                var a = boxes[e.From];
                var b = boxes[e.To];
                bool hot = e.From == _selected || e.To == _selected;
                double dim = anySelected && !hot ? 0.35 : 1;
                if (!e.IsJump)
                {
                    // Down to the next rule through the gap; faint when the team never takes it.
                    bool unused = e.Kind == RuleFlow.Kind.Unused || _flow.Reached[e.From] == RuleFlow.Reach.Never;
                    double y1 = a.Bottom, y2 = e.To == _flow.Count ? b.NumTop : b.Top, mid = (y1 + y2) / 2;
                    var line = Stroke($"M{F(a.Cx)},{F(y1)} C{F(a.Cx)},{F(mid)} {F(b.Cx)},{F(mid)} {F(b.Cx)},{F(y2)}", "FaintTextBrush",
                        unused ? new DoubleCollection { 1, 2 } : new DoubleCollection { 1.5, 1.5 }, (unused ? 0.35 : 0.8) * dim);
                    _arrows.Children.Add(line);
                    continue;
                }
                string brush = e.Kind == RuleFlow.Kind.Pass ? "OkBrush" : e.Kind == RuleFlow.Kind.Fail ? "BadBrush" : e.Kind == RuleFlow.Kind.Next ? "AccentBrush" : "FaintTextBrush";
                bool ignored = e.Kind == RuleFlow.Kind.NextIgnored;
                double x = gutter - 14 - e.Lane * LaneWidth, r = 7;
                double ys = a.Cy + (e.Kind == RuleFlow.Kind.Fail ? 8 : ignored ? -8 : 0), ye = b.Cy;
                double xe = e.To == _flow.Count ? b.Cx - 20 : b.Left - 2;
                var path = Stroke($"M{F(a.Left)},{F(ys)} H{F(x + r)} Q{F(x)},{F(ys)} {F(x)},{F(ys + r)} V{F(ye - r)} Q{F(x)},{F(ye)} {F(x + r)},{F(ye)} H{F(xe)}",
                    brush, ignored ? new DoubleCollection { 2, 2 } : null, (ignored ? 0.7 : 1) * dim);
                _arrows.Children.Add(path);
                var head = new Polygon { Points = new PointCollection { new Point(xe - 7, ye - 4), new Point(xe, ye), new Point(xe - 7, ye + 4) }, Opacity = path.Opacity };
                head.SetResourceReference(Shape.FillProperty, brush);
                _arrows.Children.Add(head);
                if (ignored)
                {
                    double my = (ys + ye) / 2;
                    _arrows.Children.Add(Stroke($"M{F(x - 4)},{F(my - 4)} L{F(x + 4)},{F(my + 4)} M{F(x + 4)},{F(my - 4)} L{F(x - 4)},{F(my + 4)}", "FaintTextBrush", null, dim));
                }
            }
        }

        private static Path Stroke(string data, string brush, DoubleCollection dash, double opacity)
        {
            var path = new Path { Data = Geometry.Parse(data), StrokeThickness = 2, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round, Opacity = opacity };
            if (dash != null) path.StrokeDashArray = dash;
            path.SetResourceReference(Shape.StrokeProperty, brush);
            return path;
        }
    }
}
