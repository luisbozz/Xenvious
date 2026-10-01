using System;
using System.Collections.Generic;
using System.Globalization;
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
            public List<int> DropZones = new List<int>(); // Mission Creator: zone numbers of the drop-off as the creator shows them
            public float DropRadius;   // older creators: radius of the drop-off point, 0 = none
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
            // The Mission Creator's rule list is live; nrl only follows it at the next test.
            if (PublicCreator && GTA.Offsets.Editor.rulelist_count != 0)
            {
                int listed = new Global(GTA.Offsets.Editor.rulelist_count + team * GTA.Offsets.Editor.rulelist_NEXT).Get<int>();
                return listed < 0 ? 0 : listed > MaxRules ? MaxRules : listed;
            }
            // LTS and Capture: the creator sets nrl to 1 at test and save; with the "nrl fix" patch
            // it keeps whatever it had, often -1 or 0 in a new job while the forced kill rule
            // already sits on rule 1. Then the rules in use count.
            int n = Nrl(team);
            if (n < 1)
                n = UsedCount(team);
            return n > MaxRules ? MaxRules : n;
        }

        /// <summary>The team's stored rule count (nrl) as it is, -1 or 0 included.</summary>
        public static int Nrl(int team) => new Global(GTA.Offsets.Editor.nrl + Team(team)).Get<int>();

        public static void SetNrl(int team, int count) => new Global(GTA.Offsets.Editor.nrl + Team(team)).SetInt(count);

        /// <summary>1 + the highest rule anything points at (entities, player rules), 0 when nothing does.</summary>
        public static int UsedCount(int team)
        {
            int top = -1;
            foreach (var s in Sources())
            {
                if (s.Number == 0 || s.Pri == 0)
                    continue;
                int n = new Global(s.Number).Get<int>();
                for (int i = 0; i < n && i < 1000; i++)
                {
                    int pri = new Global(s.Pri + team + i * s.Next).Get<int>();
                    if (pri >= 0 && pri < MaxRules && pri > top) top = pri;
                }
            }
            if (GTA.Offsets.Editor.Kill.number != 0 && GTA.Offsets.Editor.Kill.pri != 0)
            {
                int n = new Global(GTA.Offsets.Editor.Kill.number + team).Get<int>();
                for (int i = 0; i < n && i < MaxRules; i++)
                {
                    int pri = new Global(GTA.Offsets.Editor.Kill.pri + team + i * GTA.Offsets.Editor.Kill.NEXT).Get<int>();
                    if (pri >= 0 && pri < MaxRules && pri > top) top = pri;
                }
            }
            return top + 1;
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

        /// <summary>
        /// The Mission Creator's drop-off zones of a rule ("Specify Drop-off Zones"): a bitset of
        /// zone indices per team and rule (public_mission_creator func_1322).
        /// </summary>
        public static List<int> DropOffZones(int team, int rule)
        {
            var zones = new List<int>();
            if (!PublicCreator || GTA.Offsets.Editor.dozn == 0 || GTA.Offsets.Editor.dozn_NEXT == 0)
                return zones;
            long at = GTA.Offsets.Editor.dozn + Team(team) + rule * GTA.Offsets.Editor.dozn_NEXT;
            for (int word = 0; word < 3; word++)
            {
                int bits = new Global(at + word).Get<int>();
                for (int b = 0; b < 32; b++)
                    if ((bits & (1 << b)) != 0)
                        zones.Add(word * 32 + b);
            }
            return zones;
        }

        /// <summary>
        /// The number the Mission Creator shows for a zone ("Drop-off Zone 1"): its zcid + 1, which
        /// is not the zone's index (func_1885 lists zones by zcid).
        /// </summary>
        public static int ZoneNumber(int zone)
        {
            if (GTA.Offsets.Editor.Zones.zcid == 0 || GTA.Offsets.Editor.Zones.NEXT == 0)
                return zone + 1;
            int id = new Global(GTA.Offsets.Editor.Zones.zcid + zone * GTA.Offsets.Editor.Zones.NEXT).Get<int>();
            return id < 0 ? zone + 1 : id + 1;
        }

        /// <summary>The older creators' drop-off point of a rule: its radius, 0 when none is placed.</summary>
        public static float DropOffRadius(int team, int rule)
        {
            if (PublicCreator || GTA.Offsets.Editor.dpos == 0 || GTA.Offsets.Editor.drpr == 0)
                return 0;
            long at = GTA.Offsets.Editor.dpos + Team(team) + rule * 3;
            bool placed = new Global(at).Get<float>() != 0 || new Global(at + 1).Get<float>() != 0;
            return placed ? Math.Max(0.1f, new Global(GTA.Offsets.Editor.drpr + Team(team) + rule).Get<float>()) : 0;
        }

        /// <summary>A collect-and-deliver objective (creator selections 1, 6, 11).</summary>
        public static bool IsDelivery(Link link)
            => link.Kind != Kind.Player && (link.Extra ? link.Type == 1 || link.Type == 6 || link.Type == 11 : link.Type == 1 && link.Kind != Kind.GoTo);

        public enum Critical { No, Auto, Yes }

        /// <summary>
        /// The Mission Creator's "Mission Critical" of an entity for a team (Critical for Team N:
        /// Auto / Yes / No). Auto is a bit on the entity, Yes a bit in a team bitset
        /// (public_mission_creator func_2287 and neighbours).
        /// </summary>
        public static Critical CriticalFor(Link link, int team)
        {
            long auto = 0, yes = 0;
            int autoBit = 0;
            switch (link.Kind)
            {
                case Kind.Vehicle:
                    auto = GTA.Offsets.Editor.Vehicle.vbs12 == 0 ? 0 : GTA.Offsets.Editor.Vehicle.vbs12 + link.Index * GTA.Offsets.Editor.Vehicle.NEXT; autoBit = 14;
                    yes = GTA.Offsets.Editor.mcvbs == 0 ? 0 : GTA.Offsets.Editor.mcvbs + team;
                    break;
                case Kind.Object:
                    auto = GTA.Offsets.Editor.Objects.bits6 == 0 ? 0 : GTA.Offsets.Editor.Objects.bits6 + link.Index * GTA.Offsets.Editor.Objects.NEXT; autoBit = 22;
                    yes = GTA.Offsets.Editor.mcobs == 0 ? 0 : GTA.Offsets.Editor.mcobs + team;
                    break;
                case Kind.Ped:
                    auto = GTA.Offsets.Editor.Actor.pbs25 == 0 ? 0 : GTA.Offsets.Editor.Actor.pbs25 + link.Index * GTA.Offsets.Editor.Actor.NEXT; autoBit = 13;
                    yes = GTA.Offsets.Editor.mcpbs == 0 || GTA.Offsets.Editor.mcpbs_NEXT == 0 ? 0 : GTA.Offsets.Editor.mcpbs + team * GTA.Offsets.Editor.mcpbs_NEXT + link.Index / 32;
                    break;
            }
            if (auto != 0 && (new Global(auto).Get<int>() & (1 << (autoBit + team))) != 0)
                return Critical.Auto;
            int bit = link.Kind == Kind.Ped ? link.Index % 32 : link.Index;
            if (yes != 0 && bit < 32 && (new Global(yes).Get<int>() & (1 << bit)) != 0)
                return Critical.Yes;
            return Critical.No;
        }

        /// <summary>
        /// A ✗ jump the Mission Controller never takes: when a critical entity dies the team fails
        /// the mission first (public_mission_controller func_596 / func_598). Auto only counts
        /// while the entity is needed, which it is on its own rule, and never on a kill objective.
        /// </summary>
        public static bool FailJumpBlocked(Link link, int team, out Critical critical)
        {
            critical = Critical.No;
            if (!PublicCreator || link.FailJump < 0 || link.Kind == Kind.Player || link.Kind == Kind.GoTo)
                return false;
            critical = CriticalFor(link, team);
            bool kill = link.Extra ? link.Type == 2 || link.Type == 7 || link.Type == 12 : link.Type == 2;
            return critical == Critical.Yes || critical == Critical.Auto && !kill;
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
                    DropZones = DropOffZones(team, r).Select(ZoneNumber).OrderBy(n => n).ToList(),
                    DropRadius = DropOffRadius(team, r),
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

        /// <summary>
        /// The creator's selection for a rule logic on this kind of entity (func_1378 in the Mission
        /// Creator), so a vehicle's "kill" reads "Destroy" like in the creator. -1 when there is none.
        /// </summary>
        public static int Selection(Kind kind, int logic)
        {
            int k = kind == Kind.Ped ? 0 : kind == Kind.Vehicle ? 1 : kind == Kind.Object ? 2 : kind == Kind.GoTo ? 3 : -1;
            if (k < 0)
                return -1;
            switch (logic)
            {
                case 1: return new[] { 1, 6, 11, -1 }[k];
                case 2: return new[] { 2, 7, 12, -1 }[k];
                case 3: return new[] { 3, 8, 13, -1 }[k];
                case 4: return new[] { 4, 9, 14, 16 }[k];
                case 5: return new[] { 5, 10, 15, 17 }[k];
                case 11: return new[] { 23, 24, 25, -1 }[k];
                case 12: return new[] { 26, 27, 28, 29 }[k];
                case 13: return k == 2 ? 30 : -1;
                case 23: return k == 3 ? 40 : -1;
                case 24: return k == 3 ? 41 : -1;
                case 31: return new[] { 46, 50, 51, -1 }[k];
                case 32: return new[] { 47, 48, 49, -1 }[k];
                case 35: return k == 0 ? 55 : -1;
                default: return -1;
            }
        }

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        /// <summary>Name of a creator selection as the creator shows it ("Destroy" for a vehicle), null when unknown.</summary>
        public static string SelectionName(int selection)
        {
            foreach (int eoType in new[] { ExtraObjectives.TypePed, ExtraObjectives.TypeVehicle, ExtraObjectives.TypeObject, ExtraObjectives.TypeGoTo })
            {
                var known = ExtraObjectives.RuleTypesFor(eoType).FirstOrDefault(r => r.Value == selection);
                if (known.Name != null)
                    return T("eo_r_" + selection, known.Name);
            }
            return null;
        }

        /// <summary>What a link asks for, in the creator's words where there are some.</summary>
        public static string TypeName(Link link)
        {
            int selection = link.Extra ? link.Type : Selection(link.Kind, link.Type);
            if (selection > 0 && link.Kind != Kind.Player && SelectionName(selection) is string name)
                return name;
            if (link.Extra)
                return link.Type.ToString(CultureInfo.InvariantCulture);
            return link.Type >= 0 && link.Type < LogicNames.Length ? T("rl_logic_" + link.Type, LogicNames[link.Type]) : link.Type.ToString(CultureInfo.InvariantCulture);
        }

        // ----- default objective texts -----

        private static Dictionary<string, Dictionary<string, string>> _defaultTexts;

        /// <summary>
        /// The label the Mission Controller shows for a rule without own text (func_3673 of
        /// public_mission_controller 1.73): per creator selection, plural when several entities
        /// share the rule. null when the game picks its text another way (photo, charm, ...).
        /// </summary>
        public static string DefaultLabel(int selection, int entities)
        {
            string k;
            switch (selection)
            {
                case 1: case 23: k = "C_PED"; break;
                case 2: k = "K_PED"; break;
                case 3: k = "P_PED"; break;
                case 4: k = "GOTO_PED"; break;
                case 5: k = "CON_PED"; break;
                case 46: k = "DMG_PED"; break;
                case 47: k = "LEAV_PED"; break;
                case 6: case 24: k = "C_VEH"; break;
                case 7: k = "K_VEH"; break;
                case 8: k = "P_VEH"; break;
                case 9: k = "GOTO_VEH"; break;
                case 10: k = "CON_VEH"; break;
                case 48: k = "LEAV_VEH"; break;
                case 50: k = "DMG_VEH"; break;
                case 11: case 25: k = "C_OBJ"; break;
                case 12: k = "K_OBJ"; break;
                case 13: k = "P_OBJ"; break;
                case 14: k = "GOTO_OBJ"; break;
                case 15: k = "CON_OBJ"; break;
                case 49: k = "LEAV_OBJ"; break;
                case 51: k = "DMG_OBJ"; break;
                case 16: k = "GOTO_LOC"; break;
                case 17: k = "GO_AREA"; break;
                case 30: return "PMC_MINIG";
                case 41: return "PMC_LVELOC";
                default: return null;
            }
            return "PMC_" + k + (entities > 1 ? "S" : "");
        }

        /// <summary>
        /// The default text of a rule in the app's language, null when it cannot be told. Collect
        /// and deliver rules get the second text too, shown once the entity is picked up.
        /// </summary>
        public static string DefaultText(int selection, int entities)
        {
            string first = LabelText(DefaultLabel(selection, entities));
            if (first == null)
                return null;
            string kind = selection == 1 ? "PED" : selection == 6 ? "VEH" : selection == 11 ? "OBJ" : null;
            // There is no plural label for delivering vehicles.
            string deliver = kind == null ? null : LabelText("PMC_D_" + kind + (entities > 1 && kind != "VEH" ? "S" : ""));
            return deliver == null ? first : first + "~n~" + deliver;
        }

        private static string LabelText(string label)
        {
            if (label == null)
                return null;
            if (_defaultTexts == null)
            {
                try { _defaultTexts = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(OfflineData.ObjectiveTexts); }
                catch { _defaultTexts = new Dictionary<string, Dictionary<string, string>>(); }
            }
            if (!_defaultTexts.TryGetValue(label, out var texts))
                return null;
            string lang = MainWindow.Instance?.LanguageCode ?? "en";
            return texts.TryGetValue(lang, out string text) ? text : texts.TryGetValue("en", out text) ? text : null;
        }

        /// <summary>
        /// The creator selection that decides a rule's default text: the Mission Creator's rule
        /// type, elsewhere the type of the first entity on the rule. Also returns how many entities
        /// of that kind share the rule. -1 when the rule has none.
        /// </summary>
        public static int DefaultSelection(int team, Rule rule, out int entities)
        {
            entities = 0;
            var own = rule.Links.Where(l => !l.Extra && l.Kind != Kind.Player).ToList();
            if (EntityRules.UsesRuleList)
            {
                int sel = EntityRules.RuleSelection(team, rule.Index);
                int cls = EntityRules.ClassOf(sel);
                entities = own.Count(l => EntityRules.KindOf(cls) == l.Kind);
                return sel;
            }
            if (own.Count == 0)
                return -1;
            var first = own[0];
            entities = own.Count(l => l.Kind == first.Kind && l.Type == first.Type);
            return Selection(first.Kind, first.Type);
        }

        public static string DefaultTextFor(int team, Rule rule)
        {
            int selection = DefaultSelection(team, rule, out int entities);
            return selection < 0 ? null : DefaultText(selection, entities);
        }

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
