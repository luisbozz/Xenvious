using System;
using System.Collections.Generic;
using System.Linq;

namespace Xenvious.AdvancedPlacement
{
    /// <summary>A run of identical props where each follows from the previous by the same step.</summary>
    public sealed class LearnedPattern
    {
        public int Model { get; set; }
        public List<int> Indices { get; set; } = new List<int>();
        public RepeatStep Step { get; set; }
        public PropPose First { get; set; }
        public PropPose Last { get; set; }
    }

    /// <summary>
    /// Finds wallrides, spirals, loops and rows in a job: props of one model where one
    /// constant step (same axis, pivot, angle, advance in the piece's frame) leads from
    /// piece to piece. The array order does not matter -- real jobs have pieces added
    /// out of order -- so every close pair is measured and the most common step wins.
    /// </summary>
    public static class JobPatternFinder
    {
        public static List<LearnedPattern> Find(IReadOnlyList<(int Model, PropPose Pose)> props, int minLength = 3)
        {
            var result = new List<LearnedPattern>();
            foreach (var group in props.Select((p, i) => (p.Model, p.Pose, Index: i)).GroupBy(p => p.Model))
            {
                var items = group.ToList();
                if (items.Count < minLength)
                {
                    continue;
                }

                // Every ordered pair that could be neighbours, keyed by its (rounded) step.
                double reach = NeighbourReach(items.Select(x => x.Pose).ToList());
                var edges = new Dictionary<string, List<(int From, int To, RepeatStep Step)>>();
                for (int a = 0; a < items.Count; a++)
                {
                    for (int b = 0; b < items.Count; b++)
                    {
                        if (a == b || (items[a].Pose.Position - items[b].Pose.Position).Length > reach)
                        {
                            continue;
                        }
                        RepeatStep step = RepeatPlanner.Detect(items[a].Pose, items[b].Pose, snapAxes: false);
                        string key = Key(step);
                        if (!edges.TryGetValue(key, out var list))
                        {
                            edges[key] = list = new List<(int, int, RepeatStep)>();
                        }
                        list.Add((a, b, step));
                    }
                }

                var used = new HashSet<int>();
                foreach (var kv in edges.OrderByDescending(e => e.Value.Count))
                {
                    if (kv.Value.Count < minLength - 1)
                    {
                        break;
                    }
                    // Chains along this step: each piece links to its nearest successor (the
                    // key ignores the angle, so pairs two steps apart share it too), and a
                    // chain starts where nothing points in.
                    var next = new Dictionary<int, int>();
                    var bestAngle = new Dictionary<int, double>();
                    foreach (var e in kv.Value)
                    {
                        double size = Math.Abs(e.Step.AngleDeg) + e.Step.Offset.Length;
                        if (!bestAngle.TryGetValue(e.From, out double best) || size < best)
                        {
                            bestAngle[e.From] = size;
                            next[e.From] = e.To;
                        }
                    }
                    var hasIncoming = new HashSet<int>(next.Values);
                    foreach (int start in next.Keys.Where(k => !hasIncoming.Contains(k)))
                    {
                        var chain = new List<int> { start };
                        int cur = start;
                        while (next.TryGetValue(cur, out int nxt) && !chain.Contains(nxt))
                        {
                            chain.Add(nxt);
                            cur = nxt;
                        }
                        if (chain.Count < minLength || chain.Any(used.Contains))
                        {
                            continue;
                        }
                        chain.ForEach(i => used.Add(i));
                        // Hand-built runs vary in angle, and a missing piece doubles one step:
                        // take the median.
                        RepeatStep step = RepeatPlanner.Detect(items[chain[0]].Pose, items[chain[1]].Pose);
                        var angles = new List<double>();
                        for (int k = 0; k + 1 < chain.Count; k++)
                        {
                            angles.Add(Math.Abs(RepeatPlanner.Detect(items[chain[k]].Pose, items[chain[k + 1]].Pose, snapAxes: false).AngleDeg));
                        }
                        angles.Sort();
                        double median = angles[angles.Count / 2];
                        if (Math.Abs(step.AngleDeg) > 1e-6)
                        {
                            double scale = median / Math.Abs(step.AngleDeg);
                            step.AngleDeg *= scale;
                            step.Advance *= scale;
                        }
                        result.Add(new LearnedPattern
                        {
                            Model = group.Key,
                            Indices = chain.Select(i => items[i].Index).ToList(),
                            Step = step,
                            First = items[chain[0]].Pose,
                            Last = items[chain[chain.Count - 1]].Pose
                        });
                    }
                }
            }
            return result.OrderByDescending(r => r.Indices.Count).ToList();
        }

        /// <summary>
        /// Rounded step signature without the angle: the same axis through the same pivot.
        /// Hand-placed props differ in the last digits; for turns the advance per degree
        /// is what stays constant, so it is keyed that way.
        /// </summary>
        private static string Key(RepeatStep s)
        {
            V3 a = s.CustomAxis;
            bool turns = Math.Abs(s.AngleDeg) > 0.01;
            double pitch = turns ? s.Advance / s.AngleDeg : s.Advance;
            return string.Join("|", turns ? "T" : "S",
                Math.Round(pitch * 10) / 10,
                Math.Round(s.Pivot.X), Math.Round(s.Pivot.Y), Math.Round(s.Pivot.Z),
                Math.Round(a.X * 20) / 20, Math.Round(a.Y * 20) / 20, Math.Round(a.Z * 20) / 20,
                Math.Round(s.Offset.X * 2) / 2, Math.Round(s.Offset.Y * 2) / 2, Math.Round(s.Offset.Z * 2) / 2);
        }

        /// <summary>Neighbours are closer than a few typical gaps: twice the median nearest distance.</summary>
        private static double NeighbourReach(List<PropPose> poses)
        {
            var nearest = poses.Select(p => poses.Where(q => (q.Position - p.Position).Length > 1e-3)
                                                 .Select(q => (q.Position - p.Position).Length)
                                                 .DefaultIfEmpty(0).Min())
                               .Where(d => d > 0).OrderBy(d => d).ToList();
            return nearest.Count == 0 ? 50 : Math.Max(1, nearest[nearest.Count / 2] * 2.5);
        }
    }
}
