using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// Where a team goes after each rule, and which rules it can reach. A ✓ jump on an entity wins
    /// over the rule's next-objective override (seen in game: rule 3 with next = 4 and ✓ → 6 went
    /// to 6), the override wins over the next rule, and ✗ jumps branch off on failure. Only forward
    /// jumps count; the Mission Controller drops the others (func_147).
    /// </summary>
    internal sealed class RuleFlow
    {
        public enum Kind { Straight, Unused, Pass, Fail, Next, NextIgnored }
        public enum Reach { Main, Branch, Never }

        public sealed class Edge
        {
            public int From, To;  // To == rule count: the end
            public Kind Kind;
            public int Lane;      // gutter lane of a jump, 0 = closest to the rules

            public bool IsJump => Kind != Kind.Straight && Kind != Kind.Unused;
        }

        public readonly List<Edge> Edges = new List<Edge>();
        public Reach[] Reached;
        /// <summary>For a Never rule: the rule that jumps past it (-1 = none) and where to.</summary>
        public int[] SkippedBy, SkippedTo;
        /// <summary>For a Branch rule: the rule whose ✗ jump leads here.</summary>
        public int[] BranchFrom;
        public int Lanes;
        public int Count;

        public bool HasJumps => Edges.Any(e => e.IsJump);

        public static RuleFlow Build(IReadOnlyList<Rules.Rule> rules)
        {
            int n = rules.Count;
            var flow = new RuleFlow { Count = n };
            for (int i = 0; i < n; i++)
            {
                var rule = rules[i];
                var pass = rule.Links.Select(l => l.PassJump).Where(j => j > i && j < n).Distinct().OrderBy(j => j).ToList();
                var fail = rule.Links.Select(l => l.FailJump).Where(j => j > i && j < n).Distinct().OrderBy(j => j).ToList();
                var next = Enumerable.Range(0, 32).Where(b => (rule.NextRules & (1 << b)) != 0 && b > i && b < n).ToList();
                if (pass.Count > 0)
                {
                    pass.ForEach(j => flow.Add(i, j, Kind.Pass));
                    next.ForEach(j => flow.Add(i, j, Kind.NextIgnored));
                    if (!pass.Contains(i + 1)) flow.Add(i, i + 1, Kind.Unused);
                }
                else if (next.Count > 0)
                {
                    next.ForEach(j => flow.Add(i, j, Kind.Next));
                    if (!next.Contains(i + 1)) flow.Add(i, i + 1, Kind.Unused);
                }
                else
                    flow.Add(i, i + 1, Kind.Straight);
                fail.ForEach(j => flow.Add(i, j, Kind.Fail));
            }
            flow.FindReach();
            flow.AssignLanes();
            return flow;
        }

        private void Add(int from, int to, Kind kind) => Edges.Add(new Edge { From = from, To = to, Kind = kind });

        private static bool Taken(Kind k) => k == Kind.Straight || k == Kind.Pass || k == Kind.Next;

        private HashSet<int> Walk(bool withFail)
        {
            var seen = new HashSet<int>();
            var queue = new Queue<int>();
            if (Count > 0) { seen.Add(0); queue.Enqueue(0); }
            while (queue.Count > 0)
            {
                int at = queue.Dequeue();
                foreach (var e in Edges.Where(e => e.From == at && e.To < Count && (Taken(e.Kind) || withFail && e.Kind == Kind.Fail)))
                    if (seen.Add(e.To))
                        queue.Enqueue(e.To);
            }
            return seen;
        }

        private void FindReach()
        {
            var main = Walk(false);
            var any = Walk(true);
            Reached = new Reach[Count];
            SkippedBy = Enumerable.Repeat(-1, Count).ToArray();
            SkippedTo = Enumerable.Repeat(-1, Count).ToArray();
            BranchFrom = Enumerable.Repeat(-1, Count).ToArray();
            for (int r = 0; r < Count; r++)
            {
                Reached[r] = main.Contains(r) ? Reach.Main : any.Contains(r) ? Reach.Branch : Reach.Never;
                if (Reached[r] == Reach.Never)
                {
                    // The nearest reachable rule that jumps past this one names the reason.
                    var by = Edges.Where(e => e.IsJump && e.Kind != Kind.NextIgnored && e.From < r && e.To > r && any.Contains(e.From))
                        .OrderByDescending(e => e.From).FirstOrDefault();
                    if (by != null) { SkippedBy[r] = by.From; SkippedTo[r] = by.To; }
                }
                else if (Reached[r] == Reach.Branch)
                {
                    var by = Edges.Where(e => e.Kind == Kind.Fail && e.To <= r && main.Contains(e.From))
                        .OrderByDescending(e => e.To).FirstOrDefault();
                    if (by != null) BranchFrom[r] = by.From;
                }
            }
        }

        // Short jumps inside, long jumps outside, so the lines do not cross each other.
        private void AssignLanes()
        {
            var lanes = new List<List<Edge>>();
            foreach (var e in Edges.Where(e => e.IsJump).OrderBy(e => e.To - e.From))
            {
                int lane = 0;
                while (lane < lanes.Count && lanes[lane].Any(o => !(e.To <= o.From || e.From >= o.To)))
                    lane++;
                if (lane == lanes.Count) lanes.Add(new List<Edge>());
                lanes[lane].Add(e);
                e.Lane = lane;
            }
            Lanes = lanes.Count;
        }

        /// <summary>
        /// The rules a team goes through from a rule until the end, with the kind of each step
        /// (a ✓ jump or a next-objective override; several of them: the first).
        /// </summary>
        public List<(int Rule, Kind Via)> Path(int start)
        {
            var path = new List<(int, Kind)>();
            var seen = new HashSet<int>();
            int at = start;
            var via = Kind.Straight;
            while (at < Count && seen.Add(at))
            {
                path.Add((at, via));
                var step = Edges.Where(e => e.From == at && Taken(e.Kind)).OrderBy(e => e.To).FirstOrDefault();
                if (step == null) break;
                at = step.To;
                via = step.Kind;
            }
            if (at >= Count) path.Add((Count, via));
            return path;
        }
    }
}
