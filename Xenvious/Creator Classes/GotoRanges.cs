using System;
using System.Numerics;

namespace Xenvious
{
    /// <summary>
    /// Go-to range of vehicles and actors (job key gotor, metres). On a "Go to" rule the target
    /// counts as reached as soon as a player of the team is this close, without driving up to it
    /// (fm_mission_controller: vehicles func_5153(player, vehicle) &lt;= f_56, actors VDIST2 &lt;= f_100²;
    /// the 2020 and v3 controllers read the same fields). 0 = the normal go-to. Handy as an
    /// invisible trigger: arriving in the area starts the next rule with its text messages.
    /// Vehicles ignore it when the team's rule has "be in vehicle" (f_8504 bit 22): then only
    /// getting in counts. The creators save and load the value but show no menu for it.
    /// Objects have a gotor too, with another meaning (collect from that distance), not here.
    /// </summary>
    public static class GotoRanges
    {
        public const int Max = 10000;
        private const int InVehicleBit = 22;

        private static long Field(Rules.Kind kind, int index)
        {
            if (index < 0)
                return 0;
            switch (kind)
            {
                case Rules.Kind.Vehicle:
                    return GTA.Offsets.Editor.Vehicle.gotor == 0 || GTA.Offsets.Editor.Vehicle.NEXT == 0 ? 0
                        : GTA.Offsets.Editor.Vehicle.gotor + index * GTA.Offsets.Editor.Vehicle.NEXT;
                case Rules.Kind.Ped:
                    return GTA.Offsets.Editor.Actor.gotor == 0 || GTA.Offsets.Editor.Actor.NEXT == 0 ? 0
                        : GTA.Offsets.Editor.Actor.gotor + index * GTA.Offsets.Editor.Actor.NEXT;
                default:
                    return 0;
            }
        }

        public static bool Available(Rules.Kind kind) => MainWindow.m != null && MainWindow.m.IsProcOpen && Field(kind, 0) != 0;

        public static int Get(Rules.Kind kind, int index)
        {
            long at = Field(kind, index);
            return at == 0 || !MainWindow.m.IsProcOpen ? 0 : new Global(at).Get<int>();
        }

        public static void Set(Rules.Kind kind, int index, int metres)
        {
            long at = Field(kind, index);
            if (at != 0 && MainWindow.m.IsProcOpen)
                new Global(at).SetInt(Math.Max(0, Math.Min(Max, metres)));
        }

        /// <summary>Where the entity is placed, or null.</summary>
        public static Vector3? Position(Rules.Kind kind, int index)
        {
            if (index < 0 || !MainWindow.m.IsProcOpen)
                return null;
            long at = kind == Rules.Kind.Vehicle && GTA.Offsets.Editor.Vehicle.loc != 0 ? GTA.Offsets.Editor.Vehicle.loc + index * GTA.Offsets.Editor.Vehicle.NEXT
                : kind == Rules.Kind.Ped && GTA.Offsets.Editor.Actor.locx != 0 ? GTA.Offsets.Editor.Actor.locx + index * GTA.Offsets.Editor.Actor.NEXT
                : 0;
            if (at == 0)
                return null;
            var v = new Global(at).GetVector3();
            return v == Vector3.Zero ? (Vector3?)null : v;
        }

        /// <summary>Whether the team's rule has "be in vehicle", which makes vehicles ignore the range.</summary>
        public static bool InVehicleRule(int team, int rule)
        {
            if (GTA.Offsets.Editor.irbs5 == 0 || team < 0 || rule < 0 || !MainWindow.m.IsProcOpen)
                return false;
            int v = new Global(GTA.Offsets.Editor.irbs5 + team * GTA.Offsets.Editor.team_NEXT + rule).Get<int>();
            return (v & (1 << InVehicleBit)) != 0;
        }
    }
}
