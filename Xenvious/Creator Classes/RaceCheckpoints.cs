using System;
using System.Collections.Generic;
using System.Linq;
using static Xenvious.GTA.Offsets.Editor.Race;

namespace Xenvious
{
    /// <summary>
    /// The race creator's checkpoints (Global_4718592.f_115324[i], 65 slots each). Every checkpoint
    /// has a primary and an optional secondary point (split route); most fields exist twice, the
    /// three option bitsets are shared and hold one bit per point. Bit numbers are the script's
    /// (0-based), checked against fm_race_creator.c.
    /// </summary>
    public static class RaceCheckpoints
    {
        public const int Max = 100;

        /// <summary>
        /// One option bit. Set = cpbs1/2/3; Secondary = the bit of the secondary point, or the
        /// primary bit again where the option is shared by both points.
        /// </summary>
        public sealed class Flag
        {
            public Flag(string key, string fallback, int set, int primary, int secondary)
            {
                Key = key;
                Fallback = fallback;
                Set = set;
                Primary = primary;
                Secondary = secondary;
            }

            public string Key { get; }
            public string Fallback { get; }
            public int Set { get; }
            public int Primary { get; }
            public int Secondary { get; }
            public int Bit(bool secondary) => secondary ? Secondary : Primary;
        }

        // How the checkpoint looks and is collected.
        public static readonly Flag[] LookFlags =
        {
            new Flag("cp_f_fake", "Fake", 1, 10, 11),
            new Flag("cp_f_small", "Small", 1, 5, 5),
            new Flag("cp_f_big", "Big", 1, 9, 13),
            new Flag("cp_f_round", "Round", 1, 1, 2),
            new Flag("cp_f_warp", "Warp", 1, 27, 28),
            new Flag("cp_f_pitstop", "Pit stop", 2, 16, 17),
            new Flag("cp_f_lowicon", "Lower icon", 2, 18, 19),
            new Flag("cp_f_onwater", "On water", 1, 7, 8),
            new Flag("cp_f_underwater", "Under water", 2, 5, 6),
            new Flag("cp_f_lowcollect", "Low collect height", 2, 13, 14),
        };

        // What the checkpoint changes in the race.
        public static readonly Flag[] RaceFlags =
        {
            new Flag("cp_f_nocatchup", "No catch-up", 1, 3, 3),
            new Flag("cp_f_noslip", "No slipstream", 1, 6, 6),
            new Flag("cp_f_nitro", "Recharge nitro", 3, 0, 1),
            new Flag("cp_f_moretraffic", "More traffic", 2, 28, 29),
            new Flag("cp_f_lesstraffic", "Less traffic", 2, 30, 31),
        };

        public static readonly Flag Fake = LookFlags[0];
        public static readonly Flag Warp = LookFlags[4];
        public static readonly Flag Tall = new Flag("cp_f_tall", "Tall", 2, 20, 21);
        public static readonly Flag Pitch = new Flag("cp_f_pitch", "Pitch", 1, 18, 19);

        public static bool Ready => MainWindow.m.IsProcOpen && Checkpoints.locx != 0 && Checkpoints.NEXT > 0;

        public static int Count
        {
            get
            {
                if (!Ready)
                    return 0;
                int n = new Global(Checkpoints.number).Get<int>();
                return n < 0 ? 0 : Math.Min(n, Max);
            }
        }

        /// <summary>Lap race (route type 0) or point to point.</summary>
        public static bool IsLap => Ready && new Global(Checkpoints.ptp).Get<int>() == 0;

        private static long At(long field, int index) => field + index * Checkpoints.NEXT;

        public static float GetFloat(long field, int index) => new Global(At(field, index)).Get<float>();
        public static void SetFloat(long field, int index, float value) { if (Ready && field != 0) new Global(At(field, index)).SetFloat(value); }
        public static int GetInt(long field, int index) => new Global(At(field, index)).Get<int>();
        public static void SetInt(long field, int index, int value) { if (Ready && field != 0) new Global(At(field, index)).SetInt(value); }

        // ----- fields of one point -----

        public static long PosField(bool secondary) => secondary ? Checkpoints.sndchk : Checkpoints.locx;
        public static long HeadingField(bool secondary) => secondary ? Checkpoints.sndrsp : Checkpoints.chh;
        public static long RespawnField(bool secondary) => secondary ? Checkpoints.vspns : Checkpoints.vspn;
        public static long CollectSizeField(bool secondary) => secondary ? Checkpoints.chs2 : Checkpoints.chs;
        public static long ShrinkField(bool secondary) => secondary ? Checkpoints.chstRs : Checkpoints.chstR;
        public static long PitchField(bool secondary) => secondary ? Checkpoints.chpps : Checkpoints.chpp;
        public static long TransformField(bool secondary) => secondary ? Checkpoints.cptfrms : Checkpoints.cptfrm;
        public static long DeluxoField(bool secondary) => secondary ? Checkpoints.chdlos : Checkpoints.chdlo;
        public static long StrombergField(bool secondary) => secondary ? Checkpoints.chstos : Checkpoints.chsto;

        public static long BitsetField(int set)
        {
            switch (set)
            {
                case 1: return Checkpoints.cpbs1;
                case 2: return Checkpoints.cpbs2;
                case 3: return Checkpoints.cpbs3;
                default: return 0;
            }
        }

        public static (float X, float Y, float Z) GetPos(int index, bool secondary)
        {
            long f = PosField(secondary);
            return (GetFloat(f, index), GetFloat(f + 1, index), GetFloat(f + 2, index));
        }

        public static void SetPos(int index, bool secondary, float x, float y, float z)
        {
            long f = PosField(secondary);
            SetFloat(f, index, x);
            SetFloat(f + 1, index, y);
            SetFloat(f + 2, index, z);
        }

        /// <summary>A secondary point exists when its position is not 0,0,0 (the creator's own test).</summary>
        public static bool HasSecondary(int index)
        {
            var p = GetPos(index, true);
            return p.X != 0 || p.Y != 0 || p.Z != 0;
        }

        // Three respawn points per point, X/Y/Z each.
        public static (float X, float Y, float Z) GetRespawn(int index, bool secondary, int n)
        {
            long f = RespawnField(secondary) + n * 3;
            return (GetFloat(f, index), GetFloat(f + 1, index), GetFloat(f + 2, index));
        }

        public static void SetRespawn(int index, bool secondary, int n, float x, float y, float z)
        {
            long f = RespawnField(secondary) + n * 3;
            SetFloat(f, index, x);
            SetFloat(f + 1, index, y);
            SetFloat(f + 2, index, z);
        }

        public static bool GetFlag(Flag flag, int index, bool secondary)
        {
            long field = BitsetField(flag.Set);
            return field != 0 && (GetInt(field, index) & (1 << flag.Bit(secondary))) != 0;
        }

        public static void SetFlag(Flag flag, int index, bool secondary, bool on)
        {
            long field = BitsetField(flag.Set);
            if (field == 0 || !Ready)
                return;
            int value = GetInt(field, index), mask = 1 << flag.Bit(secondary);
            SetInt(field, index, on ? value | mask : value & ~mask);
        }

        /// <summary>
        /// Plane turn (cppsst): one of four bits, 0-3 for the primary point and 4-7 for the
        /// secondary. The script draws them as checkpoint types 37, 39, 40 and 38. Returns 0 for
        /// none, else 1-4.
        /// </summary>
        public static int GetPlaneTurn(int index, bool secondary)
        {
            int value = GetInt(Checkpoints.cppsst, index), shift = secondary ? 4 : 0;
            for (int i = 0; i < 4; i++)
                if ((value & (1 << (i + shift))) != 0)
                    return i + 1;
            return 0;
        }

        public static void SetPlaneTurn(int index, bool secondary, int option)
        {
            if (!Ready || Checkpoints.cppsst == 0)
                return;
            int value = GetInt(Checkpoints.cppsst, index), shift = secondary ? 4 : 0;
            value &= ~(0xF << shift);
            if (option > 0)
                value |= 1 << (option - 1 + shift);
            SetInt(Checkpoints.cppsst, index, value);
        }

        /// <summary>Random transform of the whole race: 0 off, 1 the checkpoints set to "random", 2 every checkpoint.</summary>
        public static int RandomTransform
        {
            get => Ready && Checkpoints.trntype != 0 ? new Global(Checkpoints.trntype).Get<int>() : 0;
            set { if (Ready && Checkpoints.trntype != 0) new Global(Checkpoints.trntype).SetInt(value); }
        }

        /// <summary>
        /// The transform vehicles of the race: slot 0 is the lobby vehicle, 1-12 the vehicles
        /// picked on Race › General (0 = empty slot). A checkpoint's transform holds the slot,
        /// -1 for none or -2 for random.
        /// </summary>
        public static List<(int Slot, int Hash)> TransformVehicles()
        {
            var list = new List<(int, int)>();
            if (!Ready || Checkpoints.trfmvm == 0)
                return list;
            for (int i = 0; i < GTA.Editor.TransformVehiclesCount; i++)
                list.Add((i + 1, new Global(Checkpoints.trfmvm + i).Get<int>()));
            return list;
        }

        /// <summary>
        /// Puts a new checkpoint right after <paramref name="index"/>: the ones behind it move up one
        /// place (whole 65-slot blocks, as the creator's own delete moves them down), the new one
        /// gets the creator's defaults for an empty checkpoint, the given position and the heading
        /// of the one before. The creator must rebuild afterwards to show it. Returns the new
        /// index, or -1 when the race is full.
        /// </summary>
        public static int InsertAfter(int index, float x, float y, float z)
        {
            int count = Count;
            if (!Ready || count >= Max || index < 0 || index >= count)
                return -1;
            int at = index + 1, size = (int)Checkpoints.NEXT * 8;
            for (int j = count - 1; j >= at; j--)
                new Global(At(Checkpoints.locx, j + 1)).SetBytes(new Global(At(Checkpoints.locx, j)).GetBytes(size));

            // The creator's reset of an empty checkpoint (fm_race_creator.c, the function that
            // clears f_115324[i]); the respawn arrays keep their size slots.
            long b = At(Checkpoints.locx, at);
            void F(int slot, float v) => new Global(b + slot).SetFloat(v);
            void I(int slot, int v) => new Global(b + slot).SetInt(v);
            for (int s = 0; s < 6; s++)
                F(s, 0);
            for (int s = 7; s < 18; s++)
                F(s, 0);
            for (int s = 19; s < 28; s++)
                F(s, 0);
            F(28, 0); F(29, 0);
            I(30, 0); I(31, 0); I(32, 0);
            F(33, GetFloat(HeadingField(false), index)); F(34, 0);
            for (int s = 35; s < 41; s++)
                F(s, 0);
            I(41, -1); I(42, -1); I(43, 0); I(44, 0);
            F(45, 500); F(46, 500); F(47, 0); F(48, 0);
            F(49, 1); F(50, 1); F(51, 0); F(52, 0);
            I(53, -1); I(54, -1); I(55, -1); I(56, -1);
            F(57, 1); I(58, -1); I(59, 0); I(60, -1); I(61, 0); I(62, 0); I(63, -1); I(64, -1);
            SetPos(at, false, x, y, z);
            new Global(Checkpoints.number).SetInt(count + 1);
            return at;
        }

        public static string VehicleName(int hash)
        {
            if (hash == 0)
                return null;
            return GTA.Editor.Vehiclenames.FirstOrDefault(x => Functions.int_parse(Functions.joaat(x).ToString()) == hash);
        }
    }
}
