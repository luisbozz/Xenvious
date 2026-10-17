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

        // ----- minimum players per role (trcmn) -----

        public const int Roles = 4;
        private const int RolesBit = 6;   // menubs3 bit 6, FMMC_MC_RLE "Roles"

        public static bool RolesAvailable => MainWindow.m != null && MainWindow.m.IsProcOpen
            && GTA.Offsets.Editor.rcp_minrole != 0 && GTA.Offsets.Editor.rcp_minrole_NEXT != 0 && GTA.Offsets.Editor.menubs3 != 0;

        /// <summary>
        /// The job option "Roles": players pick a role in the lobby (Global_1925996). Only then the
        /// classic controller counts players per role and fails a team below trcmn (func_806).
        /// </summary>
        public static bool RolesOn => RolesAvailable && Functions.Read.checkbinary(RolesBit, GTA.Offsets.Editor.menubs3);

        public static void SetRolesOn(bool on)
        {
            if (RolesAvailable)
                Functions.Write.writebinary(RolesBit, GTA.Offsets.Editor.menubs3, on);
        }

        private static Global RoleField(int team, int role) =>
            new Global(GTA.Offsets.Editor.rcp_minrole + team * GTA.Offsets.Editor.rcp_minrole_NEXT + role);

        public static int MinPlayers(int team, int role) => RolesAvailable ? RoleField(team, role).Get<int>() : 0;

        public static void SetMinPlayers(int team, int role, int players)
        {
            if (RolesAvailable)
                RoleField(team, role).SetInt(Math.Max(0, Math.Min(32, players)));
        }

        // ----- test from a checkpoint -----

        /// <summary>
        /// The reached checkpoints of the running job (Global_2686119.f_3930, bits 0-6; the
        /// controller sets them when the job ends and clears them when it cleans up). Set before a
        /// creator test, the controller spawns the team at the checkpoint's points
        /// (fm_mission_controller_2020 func_794). The restart rule and the rules before it only
        /// count when the job also runs as a restart (rcp_restart bit 4, func_626 in func_789), so
        /// both are set, as the Mission Creator's "Test from checkpoint" does
        /// (public_mission_controller func_22055 and func_22217). Checkpoints only apply at the
        /// start: a death during the job respawns the normal way.
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
            bool on = cp >= 0 && cp < Count;
            g.SetInt(on ? v | (1 << cp) : v);
            SetRestart(on);
            _armedIn = on ? Creator() : null;
        }

        // The creator the checkpoint was armed in. The bits and the checkpoint positions are
        // globals: armed in Capture, a test in LTS would start at a Capture job's checkpoint.
        private static string _armedIn;

        private static string Creator() =>
            GTA.IsScriptRunning("fm_capture_creator") ? "fm_capture_creator"
            : GTA.IsScriptRunning("fm_lts_creator") ? "fm_lts_creator"
            : null;

        private const int RestartBit = 4;
        private static bool _restartSet;

        private static void SetRestart(bool on)
        {
            if (GTA.Offsets.Editor.rcp_restart == 0)
                return;
            var g = new Global(GTA.Offsets.Editor.rcp_restart);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << RestartBit) : v & ~(1 << RestartBit));
            _restartSet = on;
        }

        /// <summary>
        /// Clears what this class armed once it no longer applies: the restart bit when no
        /// checkpoint is armed, both when the player left or switched the creator. The LTS and Capture
        /// creators and the controller in a creator test never clear them (only the launcher and
        /// the Mission Creator do), and the next job online would start at the checkpoint.
        /// </summary>
        public static void Tick()
        {
            // The controller keeps running in the creator between two tests, so only a running
            // test is left alone.
            if (!TestAvailable || MainWindow.Instance?.CurrentGameState == MainWindow.GameState.Testing)
                return;
            string creator = Creator();
            if (creator != null)
                ClearKeepPosition();
            if (TestCheckpoint() < 0)
            {
                if (_restartSet)
                    SetRestart(false);
                _armedIn = null;
                return;
            }
            // Armed before Xenvious started: it belongs to the creator that is open now.
            if (_armedIn == null && creator != null && !_restartSet)
                _armedIn = creator;
            if (_armedIn != null && creator != _armedIn)
            {
                Logging.Log.Info($"test checkpoint cleared: armed in {_armedIn}, now {creator ?? "no creator"}", source: "RestartCheckpoints");
                SetTestCheckpoint(-1);
            }
        }

        // Every job end sets the keep-position bit and only the launcher clears it, so without
        // this every creator test after the first starts where the player stood.
        private static void ClearKeepPosition()
        {
            if (GTA.Offsets.Editor.rcp_keep_pos == 0)
                return;
            var g = new Global(GTA.Offsets.Editor.rcp_keep_pos);
            int v = g.Get<int>();
            if ((v & 1) != 0)
                g.SetInt(v & ~1);
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
            if (RolesAvailable)
                parts.Add((RolesOn ? "R" : "r") + string.Join(",", Enumerable.Range(0, Roles).Select(r => MinPlayers(team, r))));
            return string.Join(";", parts) + "|t" + TestCheckpoint();
        }
    }
}
