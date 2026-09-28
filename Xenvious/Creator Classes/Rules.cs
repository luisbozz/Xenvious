using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// A team's rules (objectives) as the game stores them: a rule is no object of its own, peds,
    /// vehicles, objects, go-tos, player rules and extra objectives point at it with a priority
    /// (= rule index) and a rule type. Per-rule settings sit in the team's end-condition arrays.
    /// Research and value meanings: docs/handoff/RULES.md.
    /// </summary>
    public static class Rules
    {
        public const int MaxRules = 17;   // FMMC_MAX_RULES
        public const int PriorityNone = 99999;

        public enum Kind { Ped, Vehicle, Object, GoTo, Player }

        /// <summary>Something that points at a rule.</summary>
        public sealed class Link
        {
            public Kind Kind;
            public int Index;          // entity or player rule index
            public int Type;           // FMMC_OBJECTIVE_LOGIC_*, or the creator selection for extra objectives
            public bool Extra;         // an extra objective (eoir / eoep), not the entity's own rule
            public int PassJump = -1;  // rule index, -1 = none (already decoded for the running creator)
            public int FailJump = -1;
            public bool HasPass, HasFail;
        }

        public sealed class Rule
        {
            public int Index;
            public string Text;
            public readonly List<Link> Links = new List<Link>();
            public int NextRules;      // nxtrulb bitset
            public int TargetScore;    // tsc
            public int ObjectiveScore; // tms
            public int TakeoverMs;     // ttime
            public int TimeLimit;      // tmt selection index
            public bool FailsMission;  // bit in the team fail bitset
        }

        public static bool Ready => MainWindow.m != null && MainWindow.m.IsProcOpen
            && GTA.Offsets.Editor.nrl != 0 && GTA.Offsets.Editor.team_NEXT != 0;

        /// <summary>
        /// The Public Mission Creator's content runs in public_mission_controller: -1 = no jump and
        /// only later rules count. The older creators: 0 = no jump, -1 = rule 0, N = rule N.
        /// </summary>
        public static bool PublicCreator => GTA.CurrentCreatorName() == "public_mission_creator";

        public static int DecodeJump(int raw)
        {
            if (PublicCreator)
                return raw < 0 ? -1 : raw;
            return raw == 0 ? -1 : raw == -1 ? 0 : raw;
        }

        public static int EncodeJump(int index)
        {
            if (PublicCreator)
                return index < 0 ? -1 : index;
            return index < 0 ? 0 : index == 0 ? -1 : index;
        }

        private static long Team(int team) => team * GTA.Offsets.Editor.team_NEXT;

        public static int Teams()
        {
            int n = GTA.Offsets.Editor.tnum != 0 ? new Global(GTA.Offsets.Editor.tnum).Get<int>() : 1;
            return n < 1 ? 1 : n > 4 ? 4 : n;
        }

        public static int Count(int team)
        {
            int n = new Global(GTA.Offsets.Editor.nrl + Team(team)).Get<int>();
            return n < 0 ? 0 : n > MaxRules ? MaxRules : n;
        }

        private static long TextAddr(int team, int rule) => GTA.Offsets.Editor.txt0 + Team(team) + rule * GTA.Offsets.Editor.txt_NEXT;

        public static string Text(int team, int rule) => GTA.Offsets.Editor.txt0 == 0 ? "" : new Global(TextAddr(team, rule)).GetString(63);

        public static void SetText(int team, int rule, string text)
        {
            if (GTA.Offsets.Editor.txt0 != 0)
                new Global(TextAddr(team, rule)).SetString(text ?? "");
        }

        private static long RuleField(long offset, int team, int rule) => offset + Team(team) + rule;

        public static int GetField(long offset, int team, int rule) => offset == 0 ? 0 : new Global(RuleField(offset, team, rule)).Get<int>();

        public static void SetField(long offset, int team, int rule, int value)
        {
            if (offset != 0)
                new Global(RuleField(offset, team, rule)).SetInt(value);
        }

        public static bool FailsMission(int team, int rule)
            => GTA.Offsets.Editor.teamfail != 0 && (new Global(GTA.Offsets.Editor.teamfail + Team(team)).Get<int>() & (1 << rule)) != 0;

        public static void SetFailsMission(int team, int rule, bool on)
        {
            if (GTA.Offsets.Editor.teamfail == 0)
                return;
            var g = new Global(GTA.Offsets.Editor.teamfail + Team(team));
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << rule) : v & ~(1 << rule));
        }

        // ----- entities -----

        private struct Source
        {
            public Kind Kind; public long Number, Next, Pri, Type, Pass, Fail;
        }

        private static IEnumerable<Source> Sources()
        {
            yield return new Source { Kind = Kind.Ped, Number = GTA.Offsets.Editor.Actor.number, Next = GTA.Offsets.Editor.Actor.NEXT,
                Pri = GTA.Offsets.Editor.Actor.pri, Type = GTA.Offsets.Editor.Actor.rule, Pass = GTA.Offsets.Editor.Actor.jtp, Fail = GTA.Offsets.Editor.Actor.jtf };
            yield return new Source { Kind = Kind.Vehicle, Number = GTA.Offsets.Editor.Vehicle.number, Next = GTA.Offsets.Editor.Vehicle.NEXT,
                Pri = GTA.Offsets.Editor.Vehicle.pri, Type = GTA.Offsets.Editor.Vehicle.rule, Pass = GTA.Offsets.Editor.Vehicle.jtp, Fail = GTA.Offsets.Editor.Vehicle.jtf };
            yield return new Source { Kind = Kind.Object, Number = GTA.Offsets.Editor.Objects.number, Next = GTA.Offsets.Editor.Objects.NEXT,
                Pri = GTA.Offsets.Editor.Objects.pri, Type = GTA.Offsets.Editor.Objects.rule, Pass = GTA.Offsets.Editor.Objects.jtp, Fail = GTA.Offsets.Editor.Objects.jtf };
            // Go-tos have no fail jump in the Mission Creator (a location cannot fail).
            yield return new Source { Kind = Kind.GoTo, Number = GTA.Offsets.Editor.Locations.number, Next = GTA.Offsets.Editor.Locations.NEXT,
                Pri = GTA.Offsets.Editor.Locations.pri, Type = GTA.Offsets.Editor.Locations.rule, Pass = GTA.Offsets.Editor.Locations.jtp, Fail = 0 };
        }

        private static Source SourceOf(Kind kind) => Sources().FirstOrDefault(s => s.Kind == kind);

        /// <summary>Where a link's pass or fail jump is stored, 0 when it has none.</summary>
        public static long JumpAddress(Link link, int team, bool pass)
        {
            if (link.Extra)
                return 0;
            if (link.Kind == Kind.Player)
            {
                long o = pass ? GTA.Offsets.Editor.Kill.jtop : GTA.Offsets.Editor.Kill.jtof;
                return o == 0 ? 0 : o + team + link.Index * GTA.Offsets.Editor.Kill.NEXT;
            }
            var s = SourceOf(link.Kind);
            long field = pass ? s.Pass : s.Fail;
            return field == 0 ? 0 : field + team + link.Index * s.Next;
        }

        public static void SetJump(Link link, int team, bool pass, int ruleIndex)
        {
            long addr = JumpAddress(link, team, pass);
            if (addr != 0)
                new Global(addr).SetInt(EncodeJump(ruleIndex));
        }

        /// <summary>Every rule of the team with what points at it and its settings.</summary>
        public static List<Rule> Read(int team)
        {
            var rules = new List<Rule>();
            if (!Ready)
                return rules;
            int count = Count(team);
            for (int r = 0; r < count; r++)
                rules.Add(new Rule
                {
                    Index = r,
                    Text = Text(team, r),
                    NextRules = GetField(GTA.Offsets.Editor.nxtrulb, team, r),
                    TargetScore = GetField(GTA.Offsets.Editor.tsc, team, r),
                    ObjectiveScore = GetField(GTA.Offsets.Editor.tms, team, r),
                    TakeoverMs = GetField(GTA.Offsets.Editor.ttime, team, r),
                    TimeLimit = GetField(GTA.Offsets.Editor.tmt, team, r),
                    FailsMission = FailsMission(team, r),
                });

            bool publicCreator = PublicCreator;
            foreach (var s in Sources())
            {
                if (s.Number == 0 || s.Pri == 0)
                    continue;
                int n = new Global(s.Number).Get<int>();
                for (int i = 0; i < n && i < 1000; i++)
                {
                    int pri = new Global(s.Pri + team + i * s.Next).Get<int>();
                    if (pri < 0 || pri >= count)
                        continue;
                    var link = new Link { Kind = s.Kind, Index = i, Type = s.Type == 0 ? 0 : new Global(s.Type + team + i * s.Next).Get<int>() };
                    if (s.Pass != 0) { link.HasPass = true; link.PassJump = DecodeJump(new Global(s.Pass + team + i * s.Next).Get<int>()); }
                    if (s.Fail != 0) { link.HasFail = true; link.FailJump = DecodeJump(new Global(s.Fail + team + i * s.Next).Get<int>()); }
                    rules[pri].Links.Add(link);
                }
            }

            if (GTA.Offsets.Editor.Kill.number != 0 && GTA.Offsets.Editor.Kill.pri != 0)
            {
                int n = new Global(GTA.Offsets.Editor.Kill.number + team).Get<int>();
                for (int i = 0; i < n && i < MaxRules; i++)
                {
                    long at = team + i * GTA.Offsets.Editor.Kill.NEXT;
                    int pri = new Global(GTA.Offsets.Editor.Kill.pri + at).Get<int>();
                    if (pri < 0 || pri >= count)
                        continue;
                    var link = new Link { Kind = Kind.Player, Index = i, Type = new Global(GTA.Offsets.Editor.Kill.rule + at).Get<int>() };
                    // The Mission Creator does not save player rule jumps.
                    if (!publicCreator && GTA.Offsets.Editor.Kill.jtop != 0)
                    {
                        link.HasPass = link.HasFail = true;
                        link.PassJump = DecodeJump(new Global(GTA.Offsets.Editor.Kill.jtop + at).Get<int>());
                        link.FailJump = DecodeJump(new Global(GTA.Offsets.Editor.Kill.jtof + at).Get<int>());
                    }
                    rules[pri].Links.Add(link);
                }
            }

            for (int slot = 0; slot < ExtraObjectives.Slots; slot++)
            {
                int id = ExtraObjectives.EntityId(slot);
                if (id < 0)
                    continue;
                Kind kind;
                switch (ExtraObjectives.EntityType(slot))
                {
                    case ExtraObjectives.TypePed: kind = Kind.Ped; break;
                    case ExtraObjectives.TypeVehicle: kind = Kind.Vehicle; break;
                    case ExtraObjectives.TypeObject: kind = Kind.Object; break;
                    case ExtraObjectives.TypeGoTo: kind = Kind.GoTo; break;
                    default: continue;
                }
                foreach (var extra in ExtraObjectives.Rules(slot, team))
                    if (extra.Priority >= 0 && extra.Priority < count)
                        rules[extra.Priority].Links.Add(new Link { Kind = kind, Index = id, Type = extra.Type, Extra = true });
            }
            return rules;
        }

        // ----- names -----

        /// <summary>FMMC_OBJECTIVE_LOGIC_* names (key rl_logic_&lt;n&gt;).</summary>
        public static readonly string[] LogicNames =
        {
            "None", "Get and deliver", "Kill", "Protect", "Go to", "Capture", "Kill players", "Kill team 1", "Kill team 2", "Kill team 3", "Kill team 4",
            "Get and hold", "Photo", "Minigame", "Get masks", "Crowd control", "Scripted cutscene", "Charm", "Arrest", "Arrest", "Arrest", "Arrest", "Arrest",
            "Destroy", "Leave location", "Phone", "Phone", "Go to team 1", "Go to team 2", "Go to team 3", "Go to team 4", "Damage", "Leave entity",
            "Loot threshold", "Points threshold", "Ped goes to location", "Holding rule",
        };

        /// <summary>Time limit selections (tmt) in seconds, from public_mission_controller 1.73, sorted by time.</summary>
        public static readonly (int Selection, int Seconds)[] TimeLimits =
        {
            (0, 0), (20, 1), (21, 2), (22, 3), (23, 4), (24, 5), (25, 6), (26, 7), (27, 8), (28, 9), (1, 10), (29, 11), (30, 12), (31, 13), (32, 14),
            (33, 15), (34, 16), (35, 17), (36, 18), (37, 19), (2, 20), (63, 25), (3, 30), (64, 35), (4, 40), (65, 45), (5, 50), (66, 55), (6, 60),
            (67, 65), (47, 70), (68, 75), (48, 80), (69, 85), (38, 90), (70, 95), (49, 100), (71, 105), (50, 110), (72, 115), (7, 120), (51, 130),
            (52, 140), (39, 150), (53, 160), (54, 170), (8, 180), (55, 190), (56, 200), (40, 210), (57, 220), (58, 230), (9, 240), (59, 250),
            (60, 260), (41, 270), (61, 280), (62, 290), (10, 300), (42, 330), (11, 360), (43, 390), (12, 420), (44, 450), (13, 480), (45, 510),
            (14, 540), (46, 570), (15, 600), (16, 900), (17, 1200), (18, 1500), (19, 1800),
        };
    }
}
