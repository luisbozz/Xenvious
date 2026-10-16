using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Xenvious
{
    /// <summary>
    /// Drop-offs of a rule, per team. Two systems:
    /// - LTS and Capture: one drop-off per rule in the team data (dpost type, dpos and dpos2 points,
    ///   drpr radius or width, drph height). Types as fm_mission_controller reads them (func_4xxx
    ///   "drop-off centre"): 6 sphere around dpos (radius drpr), 0 cylinder around dpos (radius drpr,
    ///   height drph, 0 = 1000 m), 1 area from dpos to dpos2 (width drpr). dpos2 is a second, alternative
    ///   point for 0 and 6. Others (2, 3, 4, 7: base, vehicle, ...) are set by the creators' own
    ///   menus and only shown here. The LTS creator itself only places rule 1's drop-off; the
    ///   controller reads every rule's.
    /// - Mission Creator: zones of type 63 (Drop Off) chosen per rule (dozn bitset, "Specify
    ///   Drop-off Zones", public_mission_creator func_1322).
    /// </summary>
    public static class DropOffs
    {
        public const int Sphere = 6, Cylinder = 0, Area = 1;
        public const int ZoneType = 63;

        private static long Team(int team) => team * GTA.Offsets.Editor.team_NEXT;

        public static bool PointsAvailable => MainWindow.m != null && MainWindow.m.IsProcOpen && !Rules.PublicCreator
            && GTA.Offsets.Editor.dpos != 0 && GTA.Offsets.Editor.dpos2 != 0 && GTA.Offsets.Editor.dpost != 0
            && GTA.Offsets.Editor.drpr != 0 && GTA.Offsets.Editor.drph != 0 && GTA.Offsets.Editor.team_NEXT != 0;

        public static bool ZonesAvailable => MainWindow.m != null && MainWindow.m.IsProcOpen && Rules.PublicCreator
            && GTA.Offsets.Editor.dozn != 0 && GTA.Offsets.Editor.dozn_NEXT != 0
            && GTA.Offsets.Editor.Zones.zntp != 0 && GTA.Offsets.Editor.Zones.NEXT != 0 && GTA.Offsets.Editor.Zones.number != 0;

        // ----- LTS / Capture -----

        public static int Type(int team, int rule) => PointsAvailable ? new Global(GTA.Offsets.Editor.dpost + Team(team) + rule).Get<int>() : 0;
        public static void SetType(int team, int rule, int type) { if (PointsAvailable) new Global(GTA.Offsets.Editor.dpost + Team(team) + rule).SetInt(type); }

        public static Vector3 Point(int team, int rule, bool second) =>
            PointsAvailable ? new Global((second ? GTA.Offsets.Editor.dpos2 : GTA.Offsets.Editor.dpos) + Team(team) + rule * 3).GetVector3() : Vector3.Zero;

        public static void SetPoint(int team, int rule, bool second, Vector3 v)
        {
            if (PointsAvailable)
                new Global((second ? GTA.Offsets.Editor.dpos2 : GTA.Offsets.Editor.dpos) + Team(team) + rule * 3).WriteVector3(v);
        }

        public static float Radius(int team, int rule) => PointsAvailable ? new Global(GTA.Offsets.Editor.drpr + Team(team) + rule).Get<float>() : 0;
        public static void SetRadius(int team, int rule, float r) { if (PointsAvailable) new Global(GTA.Offsets.Editor.drpr + Team(team) + rule).SetFloat(Math.Max(0.5f, r)); }

        public static float Height(int team, int rule) => PointsAvailable ? new Global(GTA.Offsets.Editor.drph + Team(team) + rule).Get<float>() : 0;
        public static void SetHeight(int team, int rule, float h) { if (PointsAvailable) new Global(GTA.Offsets.Editor.drph + Team(team) + rule).SetFloat(Math.Max(0, h)); }

        public static bool Placed(int team, int rule) => Point(team, rule, false) != Vector3.Zero;

        /// <summary>A new drop-off at this point: a sphere with 5 m unless the rule has one already.</summary>
        public static void Place(int team, int rule, Vector3 at)
        {
            if (!PointsAvailable)
                return;
            bool had = Placed(team, rule);
            SetPoint(team, rule, false, at);
            if (!had)
            {
                SetType(team, rule, Sphere);
                if (Radius(team, rule) < 0.5f)
                    SetRadius(team, rule, 5);
            }
        }

        public static void Remove(int team, int rule)
        {
            if (!PointsAvailable)
                return;
            SetPoint(team, rule, false, Vector3.Zero);
            SetPoint(team, rule, true, Vector3.Zero);
            SetType(team, rule, Cylinder);
        }

        // ----- Mission Creator -----

        /// <summary>The job's Drop Off zones: index and the number the creator shows.</summary>
        public static List<(int Index, int Number)> Zones()
        {
            var list = new List<(int, int)>();
            if (!ZonesAvailable)
                return list;
            int count = Math.Min(96, new Global(GTA.Offsets.Editor.Zones.number).Get<int>());
            for (int z = 0; z < count; z++)
                if (new Global(GTA.Offsets.Editor.Zones.zntp + z * GTA.Offsets.Editor.Zones.NEXT).Get<int>() == ZoneType)
                    list.Add((z, Rules.ZoneNumber(z)));
            return list.OrderBy(z => z.Item2).ToList();
        }

        public static void SetZone(int team, int rule, int zone, bool on)
        {
            if (!ZonesAvailable || zone < 0 || zone >= 96)
                return;
            var g = new Global(GTA.Offsets.Editor.dozn + Team(team) + rule * GTA.Offsets.Editor.dozn_NEXT + zone / 32);
            int v = g.Get<int>();
            int bit = 1 << (zone % 32);
            g.SetInt(on ? v | bit : v & ~bit);
        }
    }
}
