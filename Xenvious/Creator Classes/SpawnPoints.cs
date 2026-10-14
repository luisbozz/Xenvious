using System;

namespace Xenvious
{
    /// <summary>
    /// The job's spawn points (JSON "ene": no, loc, head, team, dmSPTR, dmSPMR), which every
    /// creator keeps in Global_4980736.f_197027[i /*71*/] with the count in f_197020, at most 60
    /// (fm_deathmatch_creator 1.73 func_261 saves, func_7138 loads).
    ///
    /// f_23 (dmSPTR) holds a bit per team that may use the point, 0 for every team; f_24 (dmSPMR)
    /// bits 0-24, 0 for all (what they pick is not known yet). f_4 is the team the creator had
    /// selected when it placed the point.
    ///
    /// The creator shows each point as an arrow decal; adding or deleting one from outside needs a
    /// rebuild so the creator makes its decals and blips again.
    /// </summary>
    public static class SpawnPoints
    {
        public const int Max = 60;

        private static long Next => GTA.Offsets.Editor.SpawnPoints.NEXT;

        public static bool Ready => MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.SpawnPoints.number != 0
            && GTA.Offsets.Editor.SpawnPoints.loc != 0 && Next > 0;

        public static long At(long field, int i) => field + i * Next;

        public static int Count => Ready ? Math.Max(0, Math.Min(new Global(GTA.Offsets.Editor.SpawnPoints.number).Get<int>(), Max)) : 0;

        public static (float X, float Y, float Z) Pos(int i)
        {
            long at = At(GTA.Offsets.Editor.SpawnPoints.loc, i);
            return (new Global(at).Get<float>(), new Global(at + 1).Get<float>(), new Global(at + 2).Get<float>());
        }

        public static void SetPos(int i, float x, float y, float z)
        {
            if (!Ready)
                return;
            long at = At(GTA.Offsets.Editor.SpawnPoints.loc, i);
            new Global(at).SetFloat(x);
            new Global(at + 1).SetFloat(y);
            new Global(at + 2).SetFloat(z);
        }

        public static float Heading(int i) => new Global(At(GTA.Offsets.Editor.SpawnPoints.head, i)).Get<float>();

        public static void SetHeading(int i, float value)
        {
            if (Ready && GTA.Offsets.Editor.SpawnPoints.head != 0)
                new Global(At(GTA.Offsets.Editor.SpawnPoints.head, i)).SetFloat(value);
        }

        public static int Get(long field, int i) => field == 0 ? 0 : new Global(At(field, i)).Get<int>();

        public static void Set(long field, int i, int value)
        {
            if (Ready && field != 0)
                new Global(At(field, i)).SetInt(value);
        }

        /// <summary>Adds a point at the end, for every team. Its index, or -1 when there is no room.</summary>
        public static int Add(float x, float y, float z, float heading)
        {
            int i = Count;
            if (!Ready || i >= Max)
                return -1;
            // A slot left over from a deleted point would carry its old values.
            for (int k = 0; k < Next; k++)
                new Global(At(GTA.Offsets.Editor.SpawnPoints.loc, i) + k).SetLong(0);
            SetPos(i, x, y, z);
            SetHeading(i, heading);
            new Global(GTA.Offsets.Editor.SpawnPoints.number).SetInt(i + 1);
            return i;
        }

        /// <summary>Deletes a point; the ones after it move up, like the creator's own delete (func_6438).</summary>
        public static void Delete(int i)
        {
            int n = Count;
            if (!Ready || i < 0 || i >= n)
                return;
            long first = GTA.Offsets.Editor.SpawnPoints.loc;
            for (int j = i; j < n - 1; j++)
            {
                for (int k = 0; k < Next; k++)
                    new Global(At(first, j) + k).SetLong(new Global(At(first, j + 1) + k).Get<long>());
            }
            for (int k = 0; k < Next; k++)
                new Global(At(first, n - 1) + k).SetLong(0);
            new Global(GTA.Offsets.Editor.SpawnPoints.number).SetInt(n - 1);
        }
    }
}
