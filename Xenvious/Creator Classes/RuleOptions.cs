using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Xenvious
{
    /// <summary>
    /// Per-rule options of the Mission Creator's objective menu that Xenvious shows on the Rules
    /// page, with the game's own titles and help texts (PMCT_* / PMCH_*, OfflineData
    /// rule_option_texts.json, six languages). Fields and bit numbers come from the menu code of
    /// public_mission_creator 1.73 (the func_983 / func_1136 / func_1166 calls with each label) and
    /// are the same in both editions. A bit option's "on" is what the creator shows as on: some
    /// bits switch a feature off (mechanic, radar, weapons, helicopters, end near drop-off).
    /// The controllers read these fields from the job; whether LTS and Capture honour each of them
    /// is not checked one by one.
    /// </summary>
    public static class RuleOptions
    {
        public enum Kind { Bit, RuleBit, Int, Choice }

        public sealed class Option
        {
            public string Label;            // PMCT_<Label> / PMCH_<Label>
            public string Fallback;
            public Kind Kind;
            public Func<long> Field;
            public bool OnWhenClear;        // the option is on while the bit is clear
            public int Min, Max;
            public string Unit;             // shown after a number
            public Func<IEnumerable<(int Value, string Text)>> Choices;
        }

        public sealed class Group
        {
            public string Key, Fallback, HelpLabel;
            public Option[] Options;
        }

        // ----- texts -----

        private static readonly Lazy<Dictionary<string, Dictionary<string, string>>> TextData =
            new Lazy<Dictionary<string, Dictionary<string, string>>>(() =>
            {
                var d = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in JObject.Parse(OfflineData.RuleOptionTexts).Properties())
                    d[p.Name] = ((JObject)p.Value).Properties().ToDictionary(x => x.Name, x => (string)x.Value);
                return d;
            });

        private static string Language() => Translation._Language.GetCode(MainWindow.Instance?.Translation);

        /// <summary>A game text by label in the current language (English when missing), or the fallback.</summary>
        public static string GameText(string label, string fallback = null)
        {
            if (label != null && TextData.Value.TryGetValue(label, out var texts))
            {
                if (texts.TryGetValue(Language(), out var t) || texts.TryGetValue("en", out t))
                    return Clean(t);
            }
            return fallback;
        }

        // GXT line breaks and colour codes.
        private static string Clean(string text) =>
            text.Replace("~n~", "\n").Replace("~s~", "").Replace("~y~", "").Replace("~r~", "").Replace("~g~", "").Replace("~b~", "");

        public static string Title(Option o) => GameText("PMCT_" + o.Label, o.Fallback);
        public static string Help(Option o) => GameText("PMCH_" + o.Label);
        public static string Help(string label) => GameText("PMCH_" + label);

        private static IEnumerable<(int, string)> Labels(string prefix, int from, int to)
        {
            for (int v = from; v <= to; v++)
            {
                string t = GameText(prefix + v);
                if (t != null)
                    yield return (v, t);
            }
        }

        // ----- reading and writing -----

        private static long Address(Option o, int team, int rule)
        {
            long f = o.Field();
            if (f == 0)
                return 0;
            return f + team * GTA.Offsets.Editor.team_NEXT + (o.Kind == Kind.RuleBit ? 0 : rule);
        }

        public static bool Available(Option o) => o.Field() != 0 && BitOf(o, out _, out _);

        /// <summary>Whether a yes/no option is on while its bit is clear (mechanic, radar, helicopters ...).</summary>
        public static bool OnWhenClear(Option o) => BitOf(o, out _, out bool clear) && clear;

        public static int Get(Option o, int team, int rule)
        {
            long at = Address(o, team, rule);
            if (at == 0 || !MainWindow.m.IsProcOpen)
                return 0;
            int v = new Global(at).Get<int>();
            switch (o.Kind)
            {
                case Kind.Bit:
                    if (!BitOf(o, out int b, out bool clear))
                        return 0;
                    return ((v >> b) & 1) != 0 ^ clear ? 1 : 0;
                case Kind.RuleBit:
                    return ((v >> rule) & 1) != 0 ^ o.OnWhenClear ? 1 : 0;
                default:
                    return v;
            }
        }

        public static void Set(Option o, int team, int rule, int value)
        {
            long at = Address(o, team, rule);
            if (at == 0 || !MainWindow.m.IsProcOpen)
                return;
            var g = new Global(at);
            switch (o.Kind)
            {
                case Kind.Bit:
                case Kind.RuleBit:
                    if (!BitOf(o, out int dataBit, out bool onClear))
                        return;
                    int bit = o.Kind == Kind.Bit ? dataBit : rule;
                    bool set = (value != 0) ^ onClear;
                    int v = g.Get<int>();
                    g.SetInt(set ? v | (1 << bit) : v & ~(1 << bit));
                    break;
                default:
                    g.SetInt(Math.Max(o.Min, Math.Min(o.Max, value)));
                    break;
            }
        }

        // ----- the options -----

        // The bit and whether "on" is the clear bit come from rule_options.json (BuildData), which
        // ysc-global-updater reads out of the Mission Creator's menu for every build.
        private static Option Bit(string label, string fallback, Func<long> field) =>
            new Option { Label = label, Fallback = fallback, Kind = Kind.Bit, Field = field };

        private static bool BitOf(Option o, out int bit, out bool onWhenClear)
        {
            bit = 0;
            onWhenClear = o.OnWhenClear;
            if (o.Kind != Kind.Bit)
                return true;
            if (!BuildData.CurrentRuleOptions().TryGetValue(o.Label, out var b))
                return false;
            bit = b.Bit;
            onWhenClear = b.OnWhenClear;
            return true;
        }

        private static Option Number(string label, string fallback, Func<long> field, int min, int max, string unit = null) =>
            new Option { Label = label, Fallback = fallback, Kind = Kind.Int, Field = field, Min = min, Max = max, Unit = unit };

        private static Option Choice(string label, string fallback, Func<long> field, Func<IEnumerable<(int, string)>> choices) =>
            new Option { Label = label, Fallback = fallback, Kind = Kind.Choice, Field = field, Choices = choices, Min = int.MinValue, Max = int.MaxValue };

        /// <summary>Extra ways to end the objective, shown in "When the rule ends".</summary>
        public static readonly Option[] EndOptions =
        {
            Bit("OL_TLSC", "Use Team Total Score", () => GTA.Offsets.Editor.rl_score),
            Choice("OL_MTIME", "Cross-Objective Time Limit", () => GTA.Offsets.Editor.rl_mtime, () => Labels("PMCT_MR_TM_", -1, 19)),
            Bit("OL_WANT", "Lose Wanted Limit", () => GTA.Offsets.Editor.irbs),
            Bit("OL_WANTF", "Gain Wanted Limit (Fail)", () => GTA.Offsets.Editor.irbs),
            Number("OJT_MOC", "Max Carried Objects Per Player", () => GTA.Offsets.Editor.rl_moc, 0, 10),
            Bit("OJT_DOD", "Drop Objects on Delivery", () => GTA.Offsets.Editor.rl_dod),
            Bit("OJT_L_OUT", "Require Out of All Locations", () => GTA.Offsets.Editor.rl_lout),
            Bit("OJT_L_REQ", "Allow Players In Any Location", () => GTA.Offsets.Editor.rl_lreq),
            Bit("OJT_ATL", "Require All Team To Leave", () => GTA.Offsets.Editor.rl_score),
        };

        public static readonly Group[] MoreGroups =
        {
            new Group
            {
                Key = "rl_g_wanted", Fallback = "Wanted level", HelpLabel = "LAW",
                Options = new[]
                {
                    Number("LAW_DELAY", "Delay", () => GTA.Offsets.Editor.rl_wdelay, 0, 600000, "ms"),
                    Bit("LAW_NOLOSE", "Can't Lose", () => GTA.Offsets.Editor.irbs),
                    Bit("LAW_HELI", "Helicopter Units", () => GTA.Offsets.Editor.rl_heli),
                },
            },
            new Group
            {
                Key = "rl_g_chase", Fallback = "Chase units", HelpLabel = "GC_T",
                Options = new[]
                {
                    Choice("GC_UT", "Unit", () => GTA.Offsets.Editor.rl_gcunit, () => Labels("PMC_SS_GB_", 0, 60)),
                    Number("GC_I", "Max Active", () => GTA.Offsets.Editor.rl_gcmax, 1, 6),
                    Number("GC_DELAY", "Activation Delay", () => GTA.Offsets.Editor.rl_gcdelay, 0, 180, "s"),
                    Number("GC_RDELAY", "Respawn Delay", () => GTA.Offsets.Editor.rl_gcrdelay, 0, 60, "s"),
                    new Option { Label = "GC_MID", Fallback = "Start On Midpoint", Kind = Kind.RuleBit, Field = () => GTA.Offsets.Editor.rl_gcmid },
                    Bit("GC_ALRTD", "Require Alerted Actors", () => GTA.Offsets.Editor.rl_chase),
                    Bit("GC_AGGRO", "Require Team Aggro", () => GTA.Offsets.Editor.rl_aggro),
                    Bit("GC_ODIST", "End Near Drop-off", () => GTA.Offsets.Editor.rl_chase),
                    Bit("GC_FLEEOBJ", "Flee On Next Objective", () => GTA.Offsets.Editor.rl_chase),
                    Bit("OB_CHASE", "Block while chase units are active", () => GTA.Offsets.Editor.rl_block),
                },
            },
            new Group
            {
                Key = "rl_g_hud", Fallback = "HUD & disable", HelpLabel = "ODO",
                Options = new[]
                {
                    Bit("HO_TAS", "Disable Objective HUD", () => GTA.Offsets.Editor.irbs),
                    Bit("HUD_HB", "Show Health Bars", () => GTA.Offsets.Editor.irbs),
                    Choice("HO_SC", "Score HUD Color", () => GTA.Offsets.Editor.rl_hudcol, () => Labels("PMC_HUDC_", -1, 24)),
                    Bit("ODO_MCH", "Mechanic", () => GTA.Offsets.Editor.irbs),
                    Bit("ODO_NWEP", "Player Weapons", () => GTA.Offsets.Editor.rl_nwep),
                    Bit("ODO_RDAR", "Player Radar", () => GTA.Offsets.Editor.rl_radar),
                    // "Press R to toggle focus": the controller offers the hint cam on the rule's target
                    // (30 to 400 m, 800 for aircraft) unless bit 0 of irbs13 is set (all three controllers).
                    Bit("OOHCAM", "Hint Cam", () => GTA.Offsets.Editor.rl_aggro),
                },
            },
        };

        // ----- wanted level (two bitsets and two arrays, func_1308 / func_1305) -----

        public static IEnumerable<(int Value, string Text)> WantedLevels() => Labels("PMCT_LAW_LVL_", -1, 9);
        public static IEnumerable<(int Value, string Text)> WantedTriggers() => Labels("PMCT_LAW_TRIG_", 0, 2);

        private static long TeamAt(long field, int team) => field == 0 ? 0 : field + team * GTA.Offsets.Editor.team_NEXT;

        public static bool WantedAvailable =>
            GTA.Offsets.Editor.rl_wtrig1 != 0 && GTA.Offsets.Editor.rl_wtrig2 != 0 && GTA.Offsets.Editor.rl_wlvl != 0 && GTA.Offsets.Editor.rl_wlvlx != 0;

        public static int WantedTrigger(int team, int rule)
        {
            if (!WantedAvailable)
                return 0;
            if ((new Global(TeamAt(GTA.Offsets.Editor.rl_wtrig2, team)).Get<int>() & (1 << rule)) != 0)
                return 2;
            return (new Global(TeamAt(GTA.Offsets.Editor.rl_wtrig1, team)).Get<int>() & (1 << rule)) != 0 ? 1 : 0;
        }

        public static int WantedLevel(int team, int rule)
        {
            if (!WantedAvailable)
                return 0;
            int x = new Global(TeamAt(GTA.Offsets.Editor.rl_wlvlx, team) + rule).Get<int>();
            int l = new Global(TeamAt(GTA.Offsets.Editor.rl_wlvl, team) + rule).Get<int>();
            if (x == 1)
                return -1;
            if (l > 0)
                return l;
            return x == 0 ? 0 : x + 4;
        }

        public static void SetWanted(int team, int rule, int trigger, int level)
        {
            if (!WantedAvailable || !MainWindow.m.IsProcOpen)
                return;
            void SetBit(long field, bool on)
            {
                var g = new Global(TeamAt(field, team));
                int v = g.Get<int>();
                g.SetInt(on ? v | (1 << rule) : v & ~(1 << rule));
            }
            SetBit(GTA.Offsets.Editor.rl_wtrig2, trigger == 2);
            SetBit(GTA.Offsets.Editor.rl_wtrig1, trigger == 1);
            if (GTA.Offsets.Editor.rl_heli != 0)
            {
                var h = new Global(TeamAt(GTA.Offsets.Editor.rl_heli, team) + rule);
                h.SetInt(h.Get<int>() & ~(1 << 20));
            }
            var lvl = new Global(TeamAt(GTA.Offsets.Editor.rl_wlvl, team) + rule);
            var lvlx = new Global(TeamAt(GTA.Offsets.Editor.rl_wlvlx, team) + rule);
            if (level == -1) { lvl.SetInt(0); lvlx.SetInt(1); }
            else if (level <= 0) { lvl.SetInt(0); lvlx.SetInt(0); }
            else if (level < 6) { lvl.SetInt(level); lvlx.SetInt(0); }
            else { lvl.SetInt(0); lvlx.SetInt(level - 4); }
        }

        // ----- other teams' progress (PMCT_OJT_OPL) -----

        public static bool OtherTeamsAvailable => GTA.Offsets.Editor.rl_opl != 0 && GTA.Offsets.Editor.rl_oplbs != 0;

        // f_18890[rule /*5*/][team]: rule index the other team has to reach, -1 none.
        private static long OplAt(int team, int rule, int other) =>
            TeamAt(GTA.Offsets.Editor.rl_opl, team) + rule * 5 + 1 + other;

        public static int OtherTeamRule(int team, int rule, int other) =>
            OtherTeamsAvailable ? new Global(OplAt(team, rule, other)).Get<int>() : -1;

        public static void SetOtherTeamRule(int team, int rule, int other, int value)
        {
            if (OtherTeamsAvailable && MainWindow.m.IsProcOpen)
                new Global(OplAt(team, rule, other)).SetInt(value);
        }

        // f_18976[rule]: bit other = wait for its midpoint, bit 4 + other = later objectives count too.
        public static bool OtherTeamFlag(int team, int rule, int bit) =>
            OtherTeamsAvailable && (new Global(TeamAt(GTA.Offsets.Editor.rl_oplbs, team) + rule).Get<int>() & (1 << bit)) != 0;

        public static void SetOtherTeamFlag(int team, int rule, int bit, bool on)
        {
            if (!OtherTeamsAvailable || !MainWindow.m.IsProcOpen)
                return;
            var g = new Global(TeamAt(GTA.Offsets.Editor.rl_oplbs, team) + rule);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << bit) : v & ~(1 << bit));
        }

        /// <summary>PMCT_OJT_TXC: set = all set teams must meet their condition, clear = any.</summary>
        public static readonly Option AllTeams = Bit("OJT_TXC", "Pass Condition", () => GTA.Offsets.Editor.rl_chase);
    
        // ----- Mission Flags -----

        /// <summary>
        /// Mission Flags (the scripts call them prerequisites): 80 on/off switches for the whole
        /// mission. A rule can set or clear them when it starts or reaches its midpoint, and end
        /// early once all flags of its "Mission Flag Limit" are set. Entities, doors, zones and
        /// interactables set and check them too (not read here). Each rule keeps a list as a small
        /// bitset array: size slot, then the words (func_1243, 80 flags).
        /// </summary>
        public const int FlagCount = 80;

        public enum FlagList { SetOnStart, ClearOnStart, SetOnMidpoint, ClearOnMidpoint, Limit }

        private static (long Field, long Stride) FlagField(FlagList list)
        {
            switch (list)
            {
                case FlagList.SetOnStart: return (GTA.Offsets.Editor.rl_flset, GTA.Offsets.Editor.rl_flset_NEXT);
                case FlagList.ClearOnStart: return (GTA.Offsets.Editor.rl_flclr, GTA.Offsets.Editor.rl_flclr_NEXT);
                case FlagList.SetOnMidpoint: return (GTA.Offsets.Editor.rl_flmset, GTA.Offsets.Editor.rl_flmset_NEXT);
                case FlagList.ClearOnMidpoint: return (GTA.Offsets.Editor.rl_flmclr, GTA.Offsets.Editor.rl_flmclr_NEXT);
                default: return (GTA.Offsets.Editor.rl_fllimit, GTA.Offsets.Editor.rl_fllimit_NEXT);
            }
        }

        public static bool FlagsAvailable => Enum.GetValues(typeof(FlagList)).Cast<FlagList>().All(l => FlagField(l).Field != 0 && FlagField(l).Stride != 0);

        private static long FlagArray(FlagList list, int team, int rule)
        {
            var (field, stride) = FlagField(list);
            return field + team * GTA.Offsets.Editor.team_NEXT + rule * stride;
        }

        private static int FlagWords(long array)
        {
            int n = new Global(array).Get<int>();
            int needed = (FlagCount + 31) / 32;
            return n <= 0 || n > 8 ? needed : n;
        }

        public static List<int> Flags(FlagList list, int team, int rule)
        {
            var flags = new List<int>();
            if (!FlagsAvailable || !MainWindow.m.IsProcOpen)
                return flags;
            long array = FlagArray(list, team, rule);
            int words = FlagWords(array);
            for (int w = 0; w < words; w++)
            {
                int v = new Global(array + 1 + w).Get<int>();
                for (int b = 0; b < 32; b++)
                    if ((v & (1 << b)) != 0 && w * 32 + b < FlagCount)
                        flags.Add(w * 32 + b);
            }
            return flags;
        }

        public static void SetFlag(FlagList list, int team, int rule, int flag, bool on)
        {
            if (!FlagsAvailable || !MainWindow.m.IsProcOpen || flag < 0 || flag >= FlagCount)
                return;
            long array = FlagArray(list, team, rule);
            var g = new Global(array + 1 + flag / 32);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << (flag % 32)) : v & ~(1 << (flag % 32)));
        }

        /// <summary>Chase units do not deploy while this flag is set (one flag, -1 none, 0-31).</summary>
        public static readonly Option ChaseBlockFlag = Choice("GC_BLOCK", "Blocking Mission Flag", () => GTA.Offsets.Editor.rl_gcblock,
            () => new[] { (-1, GameText("PMC_SEL_OFF", "Off")) }.Concat(Enumerable.Range(0, 32).Select(f => (f, FlagName(f)))));

        public static readonly Option FlagLimitFails = Bit("OL_MO", "Mission Flag Limit Fails Team", () => GTA.Offsets.Editor.rl_chase);
        public static readonly Option BlockOnComplete = Bit("OMF_BOC", "Block On Objective Complete Mission Flags", () => GTA.Offsets.Editor.rl_boc);

        public static string FlagName(int flag) =>
            string.Format(System.Globalization.CultureInfo.CurrentCulture, MainWindow.Instance?.TranslateOr("rl_flag_n", "Flag {0}") ?? "Flag {0}", flag + 1);

        // ----- bounds (zones as play area, Mission Creator; f_2389[rule][0]) -----

        // rl_bnd is the rule's element of f_2389, which starts with the size slot of its [2] array
        // of bounds structs; the first struct follows. Its fields (menu code func_1201 / func_1205):
        // f_0 type, f_1 zone bitset (size slot, then words), f_5 time out of bounds (ms), f_10 bits.
        private const int FirstBounds = 1;
        private const int BoundsType = FirstBounds + 0, BoundsZoneBits = FirstBounds + 1, BoundsTime = FirstBounds + 5, BoundsFlags = FirstBounds + 10;
        // Consequence: f_8486[rule] bit 16 together with the struct's bits 11 / 12 (func_1205, func_1206).
        private const int ConsequenceRuleBit = 16, ConsequenceInstruct = 11, ConsequenceTimer = 12;
        /// <summary>Zone type of a bounds zone (func_1200(52) in the bounds menu).</summary>
        public const int BoundsZoneType = 52;

        public static bool BoundsAvailable => GTA.Offsets.Editor.rl_bnd != 0 && GTA.Offsets.Editor.rl_bnd_NEXT != 0 && GTA.Offsets.Editor.rl_nwep != 0;

        private static Global BoundsField(int team, int rule, int field) =>
            new Global(GTA.Offsets.Editor.rl_bnd + team * GTA.Offsets.Editor.team_NEXT + rule * GTA.Offsets.Editor.rl_bnd_NEXT + field);

        public static IEnumerable<(int Value, string Text)> BoundsTypes() => Labels("PMCT_BNDS_T", 0, 2);
        public static IEnumerable<(int Value, string Text)> BoundsConsequences() => Labels("PMC_BND_CON_", 0, 2);

        public static int BoundsTypeOf(int team, int rule) => BoundsAvailable ? BoundsField(team, rule, BoundsType).Get<int>() : 0;
        public static void SetBoundsType(int team, int rule, int type) { if (BoundsAvailable) BoundsField(team, rule, BoundsType).SetInt(type); }

        /// <summary>Seconds a player may stay outside; -1 when the consequence only instructs.</summary>
        public static int BoundsSeconds(int team, int rule)
        {
            int ms = BoundsAvailable ? BoundsField(team, rule, BoundsTime).Get<int>() : -1;
            return ms < 0 ? -1 : ms / 1000;
        }

        public static void SetBoundsSeconds(int team, int rule, int seconds)
        {
            if (BoundsAvailable)
                BoundsField(team, rule, BoundsTime).SetInt(Math.Max(0, Math.Min(300, seconds)) * 1000);
        }

        public static int BoundsConsequence(int team, int rule)
        {
            if (!BoundsAvailable)
                return 0;
            bool ruleBit = (new Global(GTA.Offsets.Editor.rl_nwep + team * GTA.Offsets.Editor.team_NEXT + rule).Get<int>() & (1 << ConsequenceRuleBit)) != 0;
            bool timer = (BoundsField(team, rule, BoundsFlags).Get<int>() & (1 << ConsequenceTimer)) != 0;
            return !ruleBit && !timer ? 0 : ruleBit && timer ? 1 : 2;
        }

        public static void SetBoundsConsequence(int team, int rule, int consequence)
        {
            if (!BoundsAvailable || !MainWindow.m.IsProcOpen)
                return;
            var rb = new Global(GTA.Offsets.Editor.rl_nwep + team * GTA.Offsets.Editor.team_NEXT + rule);
            var fl = BoundsField(team, rule, BoundsFlags);
            int r = rb.Get<int>(), f = fl.Get<int>();
            r = consequence == 1 ? r | (1 << ConsequenceRuleBit) : r & ~(1 << ConsequenceRuleBit);
            f = consequence == 0 ? (f & ~(1 << ConsequenceTimer)) | (1 << ConsequenceInstruct) : (f | (1 << ConsequenceTimer)) & ~(1 << ConsequenceInstruct);
            rb.SetInt(r);
            fl.SetInt(f);
            var time = BoundsField(team, rule, BoundsTime);
            if (consequence == 0)
                time.SetInt(-1);
            else if (time.Get<int>() < 0)
                time.SetInt(5000);      // the creator's default when a timer is switched on
        }

        /// <summary>The job's bounds zones: zone index and the number the creator shows.</summary>
        public static List<(int Index, int Number)> BoundsZones()
        {
            var list = new List<(int, int)>();
            var z = GTA.Offsets.Editor.Zones.zntp;
            if (z == 0 || GTA.Offsets.Editor.Zones.number == 0 || !MainWindow.m.IsProcOpen)
                return list;
            int count = Math.Max(0, Math.Min(128, new Global(GTA.Offsets.Editor.Zones.number).Get<int>()));
            int shown = 0;
            for (int i = 0; i < count; i++)
                if (new Global(z + (long)i * GTA.Offsets.Editor.Zones.NEXT).Get<int>() == BoundsZoneType)
                    list.Add((i, ++shown));
            return list;
        }

        public static bool BoundsUsesZone(int team, int rule, int zone)
        {
            if (!BoundsAvailable)
                return false;
            int words = BoundsField(team, rule, BoundsZoneBits).Get<int>();
            if (zone / 32 >= Math.Max(words, 1))
                return false;
            return (BoundsField(team, rule, BoundsZoneBits + 1 + zone / 32).Get<int>() & (1 << (zone % 32))) != 0;
        }

        public static void SetBoundsZone(int team, int rule, int zone, bool on)
        {
            if (!BoundsAvailable || !MainWindow.m.IsProcOpen)
                return;
            int words = BoundsField(team, rule, BoundsZoneBits).Get<int>();
            if (zone / 32 >= Math.Max(words, 1))
                return;
            var g = BoundsField(team, rule, BoundsZoneBits + 1 + zone / 32);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << (zone % 32)) : v & ~(1 << (zone % 32)));
        }
    }
}
