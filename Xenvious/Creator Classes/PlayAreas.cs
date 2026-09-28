using System;
using System.Collections.Generic;

namespace Xenvious
{
    /// <summary>
    /// Play area 1 and 2 of a team's rule: two LEGACY_BOUNDS_STRUCTs (sBoundsStruct, f_3155, and
    /// sBoundsStruct2, f_3768; 17 rules of 36 slots each) plus the per-team rule bitsets that say
    /// what each area does (stay inside, stay outside, respawn in it) and the return timer.
    /// Field numbers are the struct's (docs/handoff/PLAY-AREA.md); addresses come from offsets.ini.
    /// </summary>
    public static class PlayAreas
    {
        public const int Teams = 4;
        public const int Rules = 17;

        // Struct fields (LEGACY_BOUNDS_STRUCT).
        public const int IgnoreWantedVeh = 0, IgnoreWantedOutfit = 1, WantedToGive = 2, BoundsBS = 3, MinimapAlpha = 4,
            SpawnAhead = 5, SpawnGroup = 6, BlipHeight = 7, MaxPlayers = 8, VehicleRegen = 9, IgnoreLeaveVeh = 10,
            OnlyAffectVeh = 11, Colour = 12, Pos = 13, Pos1 = 16, Pos2 = 19, Radius = 22, Width = 23, Type = 29,
            EntityType = 30, EntityId = 31;

        // f_0 of area 1 for team 0 / rule 0: outbs is f_3, loaded with the array size slots already.
        private static long Base => GTA.Offsets.Editor.PlayArea.outbs - BoundsBS;

        // Distance between the two structs, taken from two keys that name the same field in each.
        private static long Area2Delta => GTA.Offsets.Editor.PlayArea.out2bs - GTA.Offsets.Editor.PlayArea.outbs;

        public static bool Ready => GTA.Offsets.Editor.PlayArea.outbs != 0 && GTA.Offsets.Editor.PlayArea.out2bs != 0 && GTA.Offsets.Editor.PlayArea.NEXT != 0;

        public static long Field(int area, int team, int rule, int field)
            => Base + (area == 2 ? Area2Delta : 0) + team * GTA.Offsets.Editor.team_NEXT + rule * GTA.Offsets.Editor.PlayArea.NEXT + field;

        public static int GetInt(int area, int team, int rule, int field) => new Global(Field(area, team, rule, field)).Get<int>();
        public static float GetFloat(int area, int team, int rule, int field) => new Global(Field(area, team, rule, field)).Get<float>();

        // ----- per team, one bit per rule -----

        public enum Mode { Off, StayInside, StayOutside, SpawnOnly }

        private static long PlayBits(int area) => area == 2 ? GTA.Offsets.Editor.PlayArea.playbs2 : GTA.Offsets.Editor.PlayArea.playbs;
        private static long LeaveBits(int area) => area == 2 ? GTA.Offsets.Editor.PlayArea.leavebs2 : GTA.Offsets.Editor.PlayArea.leavebs;
        private static long SpawnBits(int area) => area == 2 ? GTA.Offsets.Editor.PlayArea.spawnbs2 : GTA.Offsets.Editor.PlayArea.spawnbs;
        private static long WantedBits(int area) => area == 2 ? GTA.Offsets.Editor.PlayArea.wantedbs2 : GTA.Offsets.Editor.PlayArea.wantedbs;
        public static long Timer(int area, int team) => (area == 2 ? GTA.Offsets.Editor.PlayArea.timer2 : GTA.Offsets.Editor.PlayArea.timer) + team * GTA.Offsets.Editor.team_NEXT;

        public static bool RuleBitsReady => GTA.Offsets.Editor.PlayArea.playbs != 0 && GTA.Offsets.Editor.PlayArea.playbs2 != 0;

        private static bool Bit(long offset, int team, int rule)
            => offset != 0 && (new Global(offset + team * GTA.Offsets.Editor.team_NEXT).Get<int>() & (1 << rule)) != 0;

        private static void SetBit(long offset, int team, int rule, bool on)
        {
            if (offset == 0)
                return;
            var g = new Global(offset + team * GTA.Offsets.Editor.team_NEXT);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << rule) : v & ~(1 << rule));
        }

        public static Mode GetMode(int area, int team, int rule)
        {
            if (Bit(PlayBits(area), team, rule)) return Mode.StayInside;
            if (Bit(LeaveBits(area), team, rule)) return Mode.StayOutside;
            if (Bit(SpawnBits(area), team, rule)) return Mode.SpawnOnly;
            return Mode.Off;
        }

        /// <summary>Stay inside and stay outside exclude each other; "spawn only" is the spawn bit alone.</summary>
        public static void SetMode(int area, int team, int rule, Mode mode)
        {
            SetBit(PlayBits(area), team, rule, mode == Mode.StayInside);
            SetBit(LeaveBits(area), team, rule, mode == Mode.StayOutside);
            if (mode == Mode.SpawnOnly)
                SetBit(SpawnBits(area), team, rule, true);
        }

        public static bool GetSpawn(int area, int team, int rule) => Bit(SpawnBits(area), team, rule);
        public static void SetSpawn(int area, int team, int rule, bool on) => SetBit(SpawnBits(area), team, rule, on);
        public static bool GetWanted(int area, int team, int rule) => Bit(WantedBits(area), team, rule);
        public static void SetWanted(int area, int team, int rule, bool on) => SetBit(WantedBits(area), team, rule, on);

        /// <summary>The (team, rule) pairs a change goes to.</summary>
        public static IEnumerable<(int Team, int Rule)> Targets(int team, int rule, bool allRules, bool allTeams)
        {
            for (int t = 0; t < Teams; t++)
            {
                if (!allTeams && t != team)
                    continue;
                for (int r = 0; r < Rules; r++)
                {
                    if (!allRules && r != rule)
                        continue;
                    yield return (t, r);
                }
            }
        }
    }
}
