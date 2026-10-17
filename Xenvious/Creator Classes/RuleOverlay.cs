using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// Shapes for the injected drawer (ScriptDrawer) that the creators do not draw themselves:
    /// the spawn points of the restart checkpoints (RestartCheckpoints) and the go-to ranges of
    /// vehicles and actors (GotoRanges). PlayAreaOverlay puts them after the entity marks and
    /// before the play areas; each has its own switch on the "Show in game" card.
    /// </summary>
    public static class RuleOverlay
    {
        private static readonly Color CheckpointColour = Color.FromRgb(0xB0, 0x6E, 0xF0);
        private static readonly Color RangeColour = Color.FromRgb(0x2F, 0xB5, 0xB5);

        public static List<ScriptDrawer.Shape> Shapes()
        {
            var shapes = new List<ScriptDrawer.Shape>();
            if (!MainWindow.m.IsProcOpen)
                return shapes;
            if (VisibilityGroups.IsOn(VisibilityGroups.CheckpointBit) && RestartCheckpoints.Available)
                AddCheckpoints(shapes);
            if (VisibilityGroups.IsOn(VisibilityGroups.GotoRangeBit))
            {
                AddRanges(shapes, Rules.Kind.Vehicle, GTA.Offsets.Editor.Vehicle.number, Text("eo_k_veh", "Vehicle"));
                AddRanges(shapes, Rules.Kind.Ped, GTA.Offsets.Editor.Actor.number, Text("eo_k_ped", "Actor"));
            }
            return shapes;
        }

        // One small sphere per spawn point, labelled "T1 CP1 #2".
        private static void AddCheckpoints(List<ScriptDrawer.Shape> shapes)
        {
            for (int team = 0; team < Math.Max(1, Rules.Teams()); team++)
                for (int cp = 0; cp < RestartCheckpoints.Count; cp++)
                    for (int slot = 0; slot < RestartCheckpoints.Slots; slot++)
                    {
                        var s = RestartCheckpoints.Slot(team, cp, slot);
                        if (s == null)
                            continue;
                        var shape = ScriptDrawer.Shape.Sphere(s.Value.Pos + new Vector3(0, 0, 0.5f), 0.6f);
                        shape.Colour = CheckpointColour;
                        shape.FillAlpha = 150;
                        shape.Text = string.Format(CultureInfo.InvariantCulture, "T{0} CP{1} #{2}", team + 1, cp + 1, slot + 1);
                        shape.TextPosition = s.Value.Pos + new Vector3(0, 0, 1.6f);
                        shape.TextColour = CheckpointColour;
                        shapes.Add(shape);
                    }
        }

        private static void AddRanges(List<ScriptDrawer.Shape> shapes, Rules.Kind kind, long number, string name)
        {
            if (!GotoRanges.Available(kind) || number == 0)
                return;
            int count = Math.Max(0, Math.Min(new Global(number).Get<int>(), 200));
            for (int i = 0; i < count; i++)
            {
                int metres = GotoRanges.Get(kind, i);
                var at = metres > 0 ? GotoRanges.Position(kind, i) : null;
                if (at == null)
                    continue;
                var shape = ScriptDrawer.Shape.Sphere(at.Value, metres);
                shape.Colour = RangeColour;
                shape.FillAlpha = 35;
                shape.Text = name + " " + (i + 1) + " - " + metres.ToString(CultureInfo.InvariantCulture) + " m";
                shape.TextPosition = at.Value + new Vector3(0, 0, 2);
                shape.TextColour = RangeColour;
                shapes.Add(shape);
            }
        }

        private static string Text(string key, string fallback)
        {
            string t = MainWindow.Instance?.Translation?[key];
            return string.IsNullOrEmpty(t) || t == "missing translation" ? fallback : t;
        }
    }
}
