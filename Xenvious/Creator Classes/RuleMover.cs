using System;
using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>An address with named indices: Base plus each name's value times its stride.</summary>
    public sealed class RuleIndex
    {
        public readonly long Base;
        public readonly string[] Vars;
        public readonly long[] Strides;

        public RuleIndex(long @base, string[] vars = null, long[] strides = null)
        {
            Base = @base;
            Vars = vars ?? new string[0];
            Strides = strides ?? new long[0];
        }

        public long At(IReadOnlyDictionary<string, long> values)
        {
            long at = Base;
            for (int i = 0; i < Vars.Length; i++)
                at += values[Vars[i]] * Strides[i];
            return at;
        }
    }

    /// <summary>How often a loop runs: a fixed number, the value at a path, or the team's rule count.</summary>
    public sealed class RuleCount
    {
        public int Number;
        public RuleIndex Path;
        public string RulesOf;

        public static RuleCount Fixed(int n) => new RuleCount { Number = n };
        public static RuleCount At(RuleIndex path) => new RuleCount { Path = path };
        public static RuleCount Rules(string team) => new RuleCount { RulesOf = team };
    }

    /// <summary>One entry of rule_fields.json (see ysc-global-updater tools/extract_rule_fields.py).</summary>
    public sealed class RuleField
    {
        public string Kind;            // data, bits, prio, val (a rule number) or bits (a bitset of rules)
        public RuleIndex Field;
        public int Size = 1;
        public string[] LoopVars = new string[0];
        public RuleCount[] LoopCounts = new RuleCount[0];
        public RuleIndex Team;         // refs: where the reference's team is
        public string TeamVar;         // refs: or the loop (or "t") that is its team
        public int TeamFixed = int.MinValue;
        public RuleIndex[] If = new RuleIndex[0];
        public bool[] IfEqual = new bool[0];
        public int[] IfValue = new int[0];
    }

    public sealed class RuleFields
    {
        public RuleField[] Bits = new RuleField[0], Data = new RuleField[0], Prio = new RuleField[0], Refs = new RuleField[0];
    }

    /// <summary>
    /// Moves a rule of one team to another position, the way the Public Mission Creator does it
    /// (its "move rule" button): step by step, each step swaps the rule with its neighbour. A swap
    /// of rules a and b exchanges
    /// - the per-rule data (text, time limit, points, play area, ...) and the per-rule bits,
    /// - the rule numbers that point at a or b: the entities' own rules (priority), player rules,
    ///   extra objectives, pass/fail jumps and the other references (spawn and cleanup rules, ...)
    ///   that belong to that team.
    /// What and where comes from rule_fields.json, which ysc-global-updater reads out of the
    /// Mission Creator's move code for every build. Jumps keep the running creator's encoding
    /// (Rules.DecodeJump).
    ///
    /// The Mission Creator keeps its own rule list besides the job data and moves rules itself;
    /// there this is not offered. LTS and Capture have no move of their own.
    /// </summary>
    public static class RuleMover
    {
        private const int MaxLoop = 512;
        private const int NoReference = -1000000;   // func_1401: values at or below are not rule numbers

        public static bool Available
        {
            get
            {
                if (!Rules.Ready || Rules.PublicCreator)
                    return false;
                string creator = GTA.CurrentCreatorName();
                return (creator == "fm_lts_creator" || creator == "fm_capture_creator") && Fields.Data.Length > 0;
            }
        }

        private static RuleFields Fields => BuildData.CurrentRuleFields();

        /// <summary>Moves rule <paramref name="from"/> of the team to position <paramref name="to"/>; the rules between shift by one.</summary>
        public static void Move(int team, int from, int to)
        {
            if (!Available || from == to)
                return;
            int count = Rules.Count(team);
            if (from < 0 || to < 0 || from >= count || to >= count)
                return;
            int step = to > from ? 1 : -1;
            for (int r = from; r != to; r += step)
                Swap(team, r, r + step);
        }

        /// <summary>
        /// Deletes a rule: it moves to the end first, so every later rule and every reference
        /// shifts down by one like a move, then it is emptied and nrl drops by one. A team keeps
        /// at least one rule.
        /// </summary>
        public static bool Delete(int team, int rule)
        {
            if (!Available)
                return false;
            int count = Rules.Count(team);
            if (count < 2 || rule < 0 || rule >= count)
                return false;
            Move(team, rule, count - 1);
            Rules.Unlink(team, count - 1);
            Rules.SetNrl(team, count - 1);
            return true;
        }

        /// <summary>Swaps rules a and b of the team with everything that belongs to them.</summary>
        public static void Swap(int team, int a, int b)
        {
            if (!Available || a == b || a < 0 || b < 0 || a > 31 || b > 31)
                return;
            var fields = Fields;
            var jumps = JumpFields().ToList();
            var jumpBases = new HashSet<long>(jumps.Select(j => j.Field));

            var done = new HashSet<long>();
            foreach (var f in fields.Data)
                ForEach(f, team, v =>
                {
                    v["r"] = a;
                    long at = f.Field.At(v);
                    v["r"] = b;
                    long bt = f.Field.At(v);
                    if (!done.Add(at))
                        return;
                    for (int k = 0; k < f.Size; k++)
                    {
                        var ga = new Global(at + k);
                        var gb = new Global(bt + k);
                        long x = ga.Get<long>();
                        ga.SetLong(gb.Get<long>());
                        gb.SetLong(x);
                    }
                });

            foreach (var f in fields.Bits)
                ForEach(f, team, v => SwapBits(f.Field.At(v), a, b));

            foreach (var f in fields.Prio)
                ForEach(f, team, v => SwapValue(f.Field.At(v), a, b));

            done.Clear();
            foreach (var f in fields.Refs)
            {
                if (jumpBases.Contains(f.Field.Base))
                    continue;   // below, with the creator's jump encoding
                ForEach(f, team, v =>
                {
                    if (!Holds(f, v) || TeamOf(f, v) != team)
                        return;
                    long at = f.Field.At(v);
                    if (!done.Add(at))
                        return;
                    if (f.Kind == "bits")
                        SwapBits(at, a, b);
                    else
                        SwapValue(at, a, b);
                });
            }

            SwapPlayerRules(team, a, b);
            foreach (var j in jumps)
                SwapJumps(j, team, a, b);
            SwapExtraObjectives(team, a, b);
        }

        // ----- loops -----

        // Runs the action for every combination of the entry's loops, t set to the team.
        private static void ForEach(RuleField f, int team, Action<Dictionary<string, long>> action)
        {
            var values = new Dictionary<string, long> { ["t"] = team, ["r"] = 0 };
            Loop(f, 0, values, action);
        }

        private static void Loop(RuleField f, int depth, Dictionary<string, long> values, Action<Dictionary<string, long>> action)
        {
            if (depth == f.LoopVars.Length)
            {
                action(values);
                return;
            }
            int n = CountOf(f.LoopCounts[depth], values);
            for (int i = 0; i < n; i++)
            {
                values[f.LoopVars[depth]] = i;
                Loop(f, depth + 1, values, action);
            }
            values.Remove(f.LoopVars[depth]);
        }

        private static int CountOf(RuleCount c, IReadOnlyDictionary<string, long> values)
        {
            int n;
            if (c.RulesOf != null)
                n = values.TryGetValue(c.RulesOf, out long t) ? Rules.Count((int)t) : 0;
            else if (c.Path != null)
                n = new Global(c.Path.At(values)).Get<int>();
            else
                n = c.Number;
            // A count read from memory that is out of range means the path does not fit this job.
            return n < 0 ? 0 : n > MaxLoop ? 0 : n;
        }

        private static bool Holds(RuleField f, IReadOnlyDictionary<string, long> values)
        {
            for (int i = 0; i < f.If.Length; i++)
            {
                bool equal = new Global(f.If[i].At(values)).Get<int>() == f.IfValue[i];
                if (equal != f.IfEqual[i])
                    return false;
            }
            return true;
        }

        private static long TeamOf(RuleField f, IReadOnlyDictionary<string, long> values)
        {
            if (f.TeamVar != null)
                return values[f.TeamVar];
            if (f.Team != null)
                return new Global(f.Team.At(values)).Get<int>();
            return f.TeamFixed;
        }

        // ----- values -----

        private static void SwapValue(long at, int a, int b)
        {
            var g = new Global(at);
            int v = g.Get<int>();
            if (v <= NoReference)
                return;
            if (v == a)
                g.SetInt(b);
            else if (v == b)
                g.SetInt(a);
        }

        private static void SwapBits(long at, int a, int b)
        {
            var g = new Global(at);
            int v = g.Get<int>();
            bool ba = (v & (1 << a)) != 0, bb = (v & (1 << b)) != 0;
            if (ba == bb)
                return;
            v ^= (1 << a) | (1 << b);
            g.SetInt(v);
        }

        private static void SwapPlayerRules(int team, int a, int b)
        {
            var k = GTA.Offsets.Editor.Kill.number;
            if (k == 0 || GTA.Offsets.Editor.Kill.pri == 0)
                return;
            int n = new Global(k + team).Get<int>();
            for (int i = 0; i < n && i < Rules.MaxRules; i++)
                SwapValue(GTA.Offsets.Editor.Kill.pri + team + i * GTA.Offsets.Editor.Kill.NEXT, a, b);
        }

        private struct Jump { public long Number, Next, Field; public bool PerTeamCount; }

        // Pass and fail jumps of entities and player rules, per team ([team] at the end).
        private static IEnumerable<Jump> JumpFields()
        {
            foreach (var (number, next, field) in new[]
            {
                (GTA.Offsets.Editor.Actor.number, GTA.Offsets.Editor.Actor.NEXT, GTA.Offsets.Editor.Actor.jtp),
                (GTA.Offsets.Editor.Actor.number, GTA.Offsets.Editor.Actor.NEXT, GTA.Offsets.Editor.Actor.jtf),
                (GTA.Offsets.Editor.Vehicle.number, GTA.Offsets.Editor.Vehicle.NEXT, GTA.Offsets.Editor.Vehicle.jtp),
                (GTA.Offsets.Editor.Vehicle.number, GTA.Offsets.Editor.Vehicle.NEXT, GTA.Offsets.Editor.Vehicle.jtf),
                (GTA.Offsets.Editor.Objects.number, GTA.Offsets.Editor.Objects.NEXT, GTA.Offsets.Editor.Objects.jtp),
                (GTA.Offsets.Editor.Objects.number, GTA.Offsets.Editor.Objects.NEXT, GTA.Offsets.Editor.Objects.jtf),
                (GTA.Offsets.Editor.Locations.number, GTA.Offsets.Editor.Locations.NEXT, GTA.Offsets.Editor.Locations.jtp),
            })
            {
                if (number != 0 && field != 0)
                    yield return new Jump { Number = number, Next = next, Field = field };
            }
            foreach (long field in new[] { GTA.Offsets.Editor.Kill.jtop, GTA.Offsets.Editor.Kill.jtof })
            {
                if (GTA.Offsets.Editor.Kill.number != 0 && field != 0)
                    yield return new Jump { Number = GTA.Offsets.Editor.Kill.number, Next = GTA.Offsets.Editor.Kill.NEXT, Field = field, PerTeamCount = true };
            }
        }

        private static void SwapJumps(Jump j, int team, int a, int b)
        {
            int n = new Global(j.Number + (j.PerTeamCount ? team : 0)).Get<int>();
            for (int i = 0; i < n && i < MaxLoop; i++)
            {
                var g = new Global(j.Field + team + i * j.Next);
                int rule = Rules.DecodeJump(g.Get<int>());
                if (rule == a)
                    g.SetInt(Rules.EncodeJump(b));
                else if (rule == b)
                    g.SetInt(Rules.EncodeJump(a));
            }
        }

        private static void SwapExtraObjectives(int team, int a, int b)
        {
            foreach (var (slot, n) in ExtraObjectives.PriorityCells())
            {
                long at = ExtraObjectives.PriorityAddress(slot, n, team);
                if (at != 0)
                    SwapValue(at, a, b);
            }
        }
    }
}
