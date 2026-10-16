using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Xenvious
{
    /// <summary>
    /// A team's restart checkpoints (job keys trsrl, trstp, trsth, cmrt). When the team fails a
    /// job online after reaching a checkpoint, it can restart from there instead of from the
    /// start: it spawns at the checkpoint's points and goes on at its restart rule, the rules
    /// before count as done. The checkpoint is saved when the team reaches a rule that has the
    /// checkpoint's bit (fm_mission_controller: f_8432 bit 28, f_8468 bit 15, f_8594 bits 27 and
    /// 28 for checkpoints 1 to 4; the last such rule wins). The job data has room for 7
    /// checkpoints, the controllers save only these 4.
    /// The LTS creator saves and loads all of it, but its menus for it are hidden; the Mission
    /// Creator has its own.
    /// Not here: the facing point (trstf; the creator saves it as trstf{t}{cp}n{slot}, but it and
    /// Rockstar's launcher only load trstf{t}{cp}, so it never reaches a job) and the restart
    /// cutscene (trsct, only fm_mission_controller_2020 / _v3).
    /// </summary>
    public static class RestartCheckpoints
    {
        public const int Count = 4;
        public const int Slots = 4;

        // The rule option bit that saves checkpoint 1 to 4.
        private static readonly (Func<long> Field, int Bit)[] SaveBits =
        {
            (() => GTA.Offsets.Editor.irbs, 28),
            (() => GTA.Offsets.Editor.irbs3, 15),
            (() => GTA.Offsets.Editor.irbs10, 27),
            (() => GTA.Offsets.Editor.irbs10, 28),
        };

        private static IEnumerable<long> Fields => new[]
        {
            GTA.Offsets.Editor.rcp_rule, GTA.Offsets.Editor.rcp_rule_NEXT,
            GTA.Offsets.Editor.rcp_pos, GTA.Offsets.Editor.rcp_pos_team_NEXT, GTA.Offsets.Editor.rcp_pos_cp_NEXT,
            GTA.Offsets.Editor.rcp_head, GTA.Offsets.Editor.rcp_head_team_NEXT, GTA.Offsets.Editor.rcp_head_cp_NEXT,
            GTA.Offsets.Editor.rcp_time, GTA.Offsets.Editor.rcp_time_NEXT,
        };

        public static bool Available
        {
            get
            {
                return MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.team_NEXT != 0
                    && SaveBits.All(b => b.Field() != 0)
                    && Fields.All(f => f != 0);
            }
        }

        /// <summary>Which rule option saves this checkpoint, for tooltips ("irbs bit 28").</summary>
        public static string BitName(int cp)
        {
            string[] names = { "irbs", "irbs3", "irbs10", "irbs10" };
            return names[cp] + " bit " + SaveBits[cp].Bit.ToString(CultureInfo.InvariantCulture);
        }

        private static Global RuleBits(int cp, int team, int rule) =>
            new Global(SaveBits[cp].Field() + team * GTA.Offsets.Editor.team_NEXT + rule);

        /// <summary>The rule that saves the checkpoint, or -1. Of several, the last one (as the controller reads it).</summary>
        public static int SaveRule(int team, int cp, int rules)
        {
            if (!Available)
                return -1;
            int found = -1;
            for (int r = 0; r < rules; r++)
                if ((RuleBits(cp, team, r).Get<int>() & (1 << SaveBits[cp].Bit)) != 0)
                    found = r;
            return found;
        }

        /// <summary>Puts the checkpoint's bit on this rule only; -1 takes it off every rule.</summary>
        public static void SetSaveRule(int team, int cp, int rule, int rules)
        {
            if (!Available)
                return;
            int mask = 1 << SaveBits[cp].Bit;
            for (int r = 0; r < rules; r++)
            {
                var g = RuleBits(cp, team, r);
                int v = g.Get<int>();
                int want = r == rule ? v | mask : v & ~mask;
                if (want != v)
                    g.SetInt(want);
            }
        }

        private static Global RuleField(int team, int cp) =>
            new Global(GTA.Offsets.Editor.rcp_rule + team * GTA.Offsets.Editor.rcp_rule_NEXT + cp);

        /// <summary>The rule the team goes on at after the restart (0-based; trsrl).</summary>
        public static int RestartRule(int team, int cp) => Available ? RuleField(team, cp).Get<int>() : 0;

        public static void SetRestartRule(int team, int cp, int rule)
        {
            if (Available)
                RuleField(team, cp).SetInt(Math.Max(0, rule));
        }

        private static long PosAt(int team, int cp, int slot) =>
            GTA.Offsets.Editor.rcp_pos + team * GTA.Offsets.Editor.rcp_pos_team_NEXT + cp * GTA.Offsets.Editor.rcp_pos_cp_NEXT + slot * 3;

        private static long HeadAt(int team, int cp, int slot) =>
            GTA.Offsets.Editor.rcp_head + team * GTA.Offsets.Editor.rcp_head_team_NEXT + cp * GTA.Offsets.Editor.rcp_head_cp_NEXT + slot;

        /// <summary>A spawn point after the restart, or null while it is 0/0/0 (not set).</summary>
        public static (Vector3 Pos, float Heading)? Slot(int team, int cp, int slot)
        {
            if (!Available)
                return null;
            var pos = new Global(PosAt(team, cp, slot)).GetVector3();
            if (pos == Vector3.Zero)
                return null;
            return (pos, new Global(HeadAt(team, cp, slot)).Get<float>());
        }

        public static void SetSlot(int team, int cp, int slot, Vector3 pos, float heading)
        {
            if (!Available)
                return;
            new Global(PosAt(team, cp, slot)).WriteVector3(pos);
            new Global(HeadAt(team, cp, slot)).SetFloat(heading);
        }

        public static void ClearSlot(int team, int cp, int slot) => SetSlot(team, cp, slot, Vector3.Zero, 0);

        public static int SlotCount(int team, int cp) => Enumerable.Range(0, Slots).Count(s => Slot(team, cp, s) != null);

        // cmrt: [checkpoint][team], the other way round than the rest.
        private static Global TimeField(int team, int cp) =>
            new Global(GTA.Offsets.Editor.rcp_time + cp * GTA.Offsets.Editor.rcp_time_NEXT + team);

        /// <summary>
        /// Least time left for the team after the restart, in ms (cmrt): the controller gives the
        /// team this much when less is left (func_803). 0 = no minimum.
        /// </summary>
        public static int MinTimeMs(int team, int cp) => Available ? TimeField(team, cp).Get<int>() : 0;

        public static void SetMinTimeMs(int team, int cp, int ms)
        {
            if (Available)
                TimeField(team, cp).SetInt(Math.Max(0, ms));
        }

        // ----- test from a checkpoint -----

        /// <summary>
        /// The reached checkpoints of the running job (Global_2686119.f_3930, bits 0-6; the
        /// controller sets them, func_18950, and clears them when it cleans up, func_156). Set
        /// before a creator test, the controller starts as after a restart: it completes the rules
        /// before (func_948) and spawns at the checkpoint (func_16980). The Mission Creator's
        /// "Test from checkpoint" does the same through public_mission_controller.
        /// </summary>
        public static bool TestAvailable => MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.rcp_test != 0;

        /// <summary>The checkpoint the next test starts at, or -1.</summary>
        public static int TestCheckpoint()
        {
            if (!TestAvailable)
                return -1;
            int v = new Global(GTA.Offsets.Editor.rcp_test).Get<int>();
            for (int cp = Count - 1; cp >= 0; cp--)
                if ((v & (1 << cp)) != 0)
                    return cp;
            return -1;
        }

        /// <summary>Lets the next test start at this checkpoint; -1 = from the start again.</summary>
        public static void SetTestCheckpoint(int cp)
        {
            if (!TestAvailable)
                return;
            var g = new Global(GTA.Offsets.Editor.rcp_test);
            int v = g.Get<int>() & ~((1 << Count) - 1);
            g.SetInt(cp >= 0 && cp < Count ? v | (1 << cp) : v);
        }

        /// <summary>For change detection on the Rules page.</summary>
        public static string Signature(int team, int rules)
        {
            if (!Available)
                return "";
            var parts = new List<string>();
            for (int cp = 0; cp < Count; cp++)
            {
                parts.Add(SaveRule(team, cp, rules) + ">" + RestartRule(team, cp) + ":" + MinTimeMs(team, cp));
                for (int s = 0; s < Slots; s++)
                {
                    var slot = Slot(team, cp, s);
                    parts.Add(slot == null ? "-" : string.Format(CultureInfo.InvariantCulture, "{0:0.00},{1:0.00},{2:0.00},{3:0.0}", slot.Value.Pos.X, slot.Value.Pos.Y, slot.Value.Pos.Z, slot.Value.Heading));
                }
            }
            return string.Join(";", parts) + "|t" + TestCheckpoint();
        }
    }
}
