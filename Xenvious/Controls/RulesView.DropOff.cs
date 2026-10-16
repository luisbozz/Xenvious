using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Xenvious
{
    // Part of RulesView: the selected rule's drop-off (DropOffs). LTS and Capture: shape, points,
    // radius and height. Mission Creator: which Drop Off zones count for the rule.
    public partial class RulesView
    {
        private bool ShowsDropOff(Rules.Rule rule) =>
            (DropOffs.PointsAvailable || DropOffs.ZonesAvailable) && (rule.Links.Any(Rules.IsDelivery) || rule.DropZones.Count > 0 || rule.DropRadius > 0);

        private string DropSummary(Rules.Rule rule)
        {
            if (DropOffs.ZonesAvailable)
                return rule.DropZones.Count == 0 ? T("rl_drop_none", "no drop-off zone")
                    : string.Join(", ", rule.DropZones.Select(z => Rules.ZoneNumber(z).ToString(CultureInfo.CurrentCulture)));
            if (!DropOffs.Placed(_team, rule.Index))
                return T("dof_none", "not placed");
            return ShapeName(DropOffs.Type(_team, rule.Index)) + " · " + DropOffs.Radius(_team, rule.Index).ToString("0.#", CultureInfo.CurrentCulture) + " m";
        }

        private static string ShapeName(int type)
        {
            switch (type)
            {
                case DropOffs.Sphere: return T("dof_sphere", "Sphere");
                case DropOffs.Cylinder: return T("dof_cylinder", "Cylinder");
                case DropOffs.Area: return T("dof_area", "Area");
                default: return string.Format(CultureInfo.CurrentCulture, T("dof_other", "Other ({0})"), type);
            }
        }

        private FrameworkElement DropBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            int team = _team, r = rule.Index;
            if (DropOffs.ZonesAvailable)
            {
                var zones = DropOffs.Zones();
                if (zones.Count == 0)
                    panel.Children.Add(Say(T("dof_no_zones", "The job has no zone of type Drop Off yet. Place one on the zones page (type Drop Off), then tick it here.")));
                foreach (var (index, number) in zones)
                {
                    int zone = index;
                    OptionRow(panel, string.Format(CultureInfo.CurrentCulture, T("rl_drop_zone", "Drop-off zone {0}"), number),
                        Toggle(rule.DropZones.Contains(zone), on => DropOffs.SetZone(team, r, zone, on)), null);
                }
                return panel;
            }

            bool placed = DropOffs.Placed(team, r);
            int type = DropOffs.Type(team, r);
            if (!placed)
                panel.Children.Add(Say(T("dof_hint", "Where the team delivers on this rule. Place it at your position or at the cursor.")));
            var place = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
            place.Children.Add(TextButton(T("dof_me", "At my position"), null, () =>
            {
                var me = PlayerPosition();
                if (me != null) { DropOffs.Place(team, r, me.Value); Reread(); }
            }));
            place.Children.Add(TextButton(T("dof_cursor", "At the cursor"), null, () =>
            {
                var at = CursorPosition();
                if (at != null) { DropOffs.Place(team, r, at.Value); Reread(); }
            }));
            if (placed)
            {
                place.Children.Add(TextButton(T("rcp_go", "Teleport there"), null, () =>
                {
                    var p = DropOffs.Point(team, r, false);
                    GTA.Teleport(new XenVector3(p.X, p.Y, p.Z + 1));
                }));
                place.Children.Add(TextButton(T("dof_remove", "Remove"), null, () => { DropOffs.Remove(team, r); Reread(); }));
            }
            panel.Children.Add(place);
            if (!placed)
                return panel;

            bool known = type == DropOffs.Sphere || type == DropOffs.Cylinder || type == DropOffs.Area;
            if (known)
                OptionRow(panel, T("dof_shape", "Shape"),
                    Combo(new[] { (DropOffs.Sphere, ShapeName(DropOffs.Sphere)), (DropOffs.Cylinder, ShapeName(DropOffs.Cylinder)), (DropOffs.Area, ShapeName(DropOffs.Area)) }, type,
                        v => DropOffs.SetType(team, r, v)),
                    _help ? T("dof_shape_help", "Sphere: around the point. Cylinder: around the point, up to the height. Area: a box from the first to the second point, as wide as the width.") : null);
            else
                panel.Children.Add(Say(string.Format(CultureInfo.CurrentCulture, T("dof_other_hint", "Type {0} is set by the creator's own menu (for example a vehicle or a base as drop-off); only position and radius are shown here."), type)));

            PointRow(panel, type == DropOffs.Area ? T("dof_from", "From") : T("dof_point", "Point"), team, r, false);
            if (type == DropOffs.Area || DropOffs.Point(team, r, true) != Vector3.Zero)
                PointRow(panel, type == DropOffs.Area ? T("dof_to", "To") : T("dof_second", "Second point"), team, r, true);
            OptionRow(panel, type == DropOffs.Area ? T("dof_width", "Width") : T("dof_radius", "Radius"),
                FloatBox(DropOffs.Radius(team, r), "m", v => DropOffs.SetRadius(team, r, v)), null);
            if (type == DropOffs.Cylinder)
                OptionRow(panel, T("dof_height", "Height"), FloatBox(DropOffs.Height(team, r), "m", v => DropOffs.SetHeight(team, r, v)),
                    _help ? T("dof_height_help", "0 = 1000 m.") : null);
            return panel;
        }

        private void PointRow(Panel panel, string label, int team, int rule, bool second)
        {
            var p = DropOffs.Point(team, rule, second);
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var acts = new StackPanel { Orientation = Orientation.Horizontal };
            acts.Children.Add(IconButton("M6,1 A2.2,2.2 0 1 1 5.99,1 M1.5,11 C1.5,7 10.5,7 10.5,11", T("dof_me", "At my position"), () =>
            {
                var me = PlayerPosition();
                if (me != null) { DropOffs.SetPoint(team, rule, second, me.Value); Reread(); }
            }));
            acts.Children.Add(IconButton("M6,0.5 V3 M6,9 V11.5 M0.5,6 H3 M9,6 H11.5 M6,3.5 A2.5,2.5 0 1 1 5.99,3.5", T("dof_cursor", "At the cursor"), () =>
            {
                var at = CursorPosition();
                if (at != null) { DropOffs.SetPoint(team, rule, second, at.Value); Reread(); }
            }));
            DockPanel.SetDock(acts, Dock.Right);
            row.Children.Add(acts);
            var l = new TextBlock { Text = label, FontSize = 13, Width = 110, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            l.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            DockPanel.SetDock(l, Dock.Left);
            row.Children.Add(l);
            var grid = new UniformGrid { Columns = 3 };
            var boxes = new TextBox[3];
            float[] v = { p.X, p.Y, p.Z };
            for (int i = 0; i < 3; i++)
            {
                var box = new TextBox { Height = 28, Margin = new Thickness(0, 0, 4, 0), Text = p == Vector3.Zero ? "" : v[i].ToString("0.###", CultureInfo.InvariantCulture), Tag = "XYZ".Substring(i, 1) };
                box.SetResourceReference(StyleProperty, "Watermark");
                boxes[i] = box;
                grid.Children.Add(box);
            }
            void Commit()
            {
                if (_loading || !Live)
                    return;
                var n = new float[3];
                for (int i = 0; i < 3; i++)
                    if (!float.TryParse(boxes[i].Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out n[i]))
                        return;
                DropOffs.SetPoint(team, rule, second, new Vector3(n[0], n[1], n[2]));
                _shownKey = null;
            }
            foreach (var box in boxes)
            {
                box.LostKeyboardFocus += (_, __) => Commit();
                box.KeyDown += (_, e) => { if (e.Key == Key.Enter) Commit(); };
            }
            row.Children.Add(grid);
            panel.Children.Add(row);
        }

        private TextBox FloatBox(float value, string unit, Action<float> write)
        {
            var box = new TextBox { Height = 28, Text = value.ToString("0.##", CultureInfo.InvariantCulture), Tag = unit, ToolTip = unit };
            box.SetResourceReference(StyleProperty, "Watermark");
            void Commit()
            {
                if (!_loading && Live && float.TryParse(box.Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                {
                    write(v);
                    _shownKey = null;
                }
            }
            box.LostKeyboardFocus += (_, __) => Commit();
            box.KeyDown += (_, e) => { if (e.Key == Key.Enter) Commit(); };
            return box;
        }
    }
}
