using System;

namespace Xenvious
{
    /// <summary>
    /// Team start points (Global_4980736.f_201288[team][i /*71*/], count f_197021[team]) as the
    /// Mission Creator's spawn point menu sees them (public_mission_creator 1.73, func_2700 /
    /// func_2701 / func_2702).
    ///
    /// Bits of f_5: 7 treat as a start point, 2 treat as a respawn point, 16 / 17 checkpoint 1 / 2,
    /// 24 spawn in the personal vehicle, 23 force and 31 block personal vehicle creation.
    /// f_12 / f_13 are the rules the point is usable from / until, -1 for mission start / end.
    ///
    /// Adding a point writes the slot the way the creator's JSON loader fills it and raises the
    /// count; the creator only draws it after a rebuild. Its own placement also gives the point an
    /// id in a creator local (f_11), which a write from outside cannot do.
    /// </summary>
    public static class StartPoints
    {
        public const int BitRespawn = 2;
        public const int BitStart = 7;
        public const int BitCheckpoint1 = 16;
        public const int BitCheckpoint2 = 17;
        public const int BitForcePv = 23;
        public const int BitSpawnInPv = 24;
        public const int BitBlockPv = 31;

        // 4261 slots per team / 71 per point.
        public const int Max = 60;

        public static bool Ready => GTA.Offsets.Editor.player_number != 0 && GTA.Offsets.Editor.player_loc != 0 && GTA.Offsets.Editor.player_bit != 0
            && GTA.Offsets.Editor.next_settings != 0 && GTA.Offsets.Editor.team_NEXT_settings != 0;

        public static long Field(long field, int team, int i) => field + team * GTA.Offsets.Editor.team_NEXT_settings + i * GTA.Offsets.Editor.next_settings;

        public static int Count(int team) => new Global(GTA.Offsets.Editor.player_number + team).Get<int>();

        public static int Bits(int team, int i) => new Global(Field(GTA.Offsets.Editor.player_bit, team, i)).Get<int>();

        public static bool Bit(int team, int i, int bit) => (Bits(team, i) & (1 << bit)) != 0;

        public static void SetBit(int team, int i, int bit, bool on)
        {
            int v = Bits(team, i);
            new Global(Field(GTA.Offsets.Editor.player_bit, team, i)).SetInt(on ? v | (1 << bit) : v & ~(1 << bit));
        }

        public static int Get(long field, int team, int i) => field == 0 ? 0 : new Global(Field(field, team, i)).Get<int>();

        public static void Set(long field, int team, int i, int value)
        {
            if (field != 0)
                new Global(Field(field, team, i)).SetInt(value);
        }

        /// <summary>
        /// Adds a point at the end of the team's points with the creator's defaults. Returns its
        /// index, or -1 when the team is full or offsets are missing.
        /// </summary>
        public static int Add(int team, float x, float y, float z, float heading, bool start, bool respawn)
        {
            if (!Ready || team < 0 || team > 3)
                return -1;
            int i = Count(team);
            if (i < 0 || i >= Max)
                return -1;

            // A slot left over from a deleted point would carry its old values.
            for (int k = 0; k < GTA.Offsets.Editor.next_settings; k++)
                new Global(Field(GTA.Offsets.Editor.player_loc, team, i) + k).SetInt(0);

            var loc = Field(GTA.Offsets.Editor.player_loc, team, i);
            new Global(loc).SetFloat(x);
            new Global(loc + 1).SetFloat(y);
            new Global(loc + 2).SetFloat(z);
            new Global(Field(GTA.Offsets.Editor.player_head, team, i)).SetFloat(heading);
            Set(GTA.Offsets.Editor.player_team, team, i, team);
            Set(GTA.Offsets.Editor.player_bit, team, i, (start ? 1 << BitStart : 0) | (respawn ? 1 << BitRespawn : 0));
            // The JSON loader's values for a point without them (func_7593).
            Set(GTA.Offsets.Editor.player_veh, team, i, -1);
            Set(GTA.Offsets.Editor.player_seat, team, i, -3);
            Set(GTA.Offsets.Editor.player_vfrs, team, i, -1);
            Set(GTA.Offsets.Editor.player_vfre, team, i, -1);
            Set(GTA.Offsets.Editor.player_ttm, team, i, -2);
            Set(GTA.Offsets.Editor.player_tspr, team, i, team);
            Set(GTA.Offsets.Editor.player_lcet, team, i, -1);
            Set(GTA.Offsets.Editor.player_lcid, team, i, -1);
            if (GTA.Offsets.Editor.player_pvhead != 0)
                new Global(Field(GTA.Offsets.Editor.player_pvhead, team, i)).SetFloat(999f);

            new Global(GTA.Offsets.Editor.player_number + team).SetInt(i + 1);
            return i;
        }
    }
}
