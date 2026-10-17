using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// Marks placed entities in the game world through the injected drawer (LTS and Capture):
    /// - every actor, vehicle, object and weapon near the cursor gets its number as a small label
    ///   ("Actor 3", the number the Xenvious page shows), the nearest first, at most
    ///   ScriptDrawer.MaxLabels;
    /// - the entry selected on the open Xenvious page gets a cone above it and its name, however
    ///   far away it is.
    /// The page sets <see cref="Select"/>; the patch loop calls <see cref="Shapes"/> and
    /// <see cref="Labels"/> with the play areas.
    /// </summary>
    public static class EntityMarks
    {
        public enum Kind { None, Actor, Vehicle, Object, Weapon, Prop, DynamicProp, Door }

        private const float LabelRange = 150f;
        private static readonly Color SelectedColour = Color.FromRgb(0x5B, 0x9B, 0xE6);

        private static volatile int _kind;
        private static volatile int _index = -1;

        /// <summary>The entry the open page shows; Kind.None or -1 for nothing.</summary>
        public static void Select(Kind kind, int index)
        {
            _kind = (int)kind;
            _index = index;
        }

        public static bool ForCreator(string script) => script == "fm_lts_creator" || script == "fm_capture_creator";

        private static (long Number, long Next, long X, long Y, long Z) Fields(Kind kind)
        {
            switch (kind)
            {
                case Kind.Actor: return (GTA.Offsets.Editor.Actor.number, GTA.Offsets.Editor.Actor.NEXT, GTA.Offsets.Editor.Actor.locx, GTA.Offsets.Editor.Actor.locy, GTA.Offsets.Editor.Actor.locz);
                case Kind.Vehicle: return Loc(GTA.Offsets.Editor.Vehicle.number, GTA.Offsets.Editor.Vehicle.NEXT, GTA.Offsets.Editor.Vehicle.loc);
                case Kind.Object: return Loc(GTA.Offsets.Editor.Objects.number, GTA.Offsets.Editor.Objects.NEXT, GTA.Offsets.Editor.Objects.loc);
                case Kind.Weapon: return (GTA.Offsets.Editor.Weapon.number, GTA.Offsets.Editor.Weapon.NEXT, GTA.Offsets.Editor.Weapon.locx, GTA.Offsets.Editor.Weapon.locy, GTA.Offsets.Editor.Weapon.locz);
                case Kind.Prop: return Loc(GTA.Offsets.Editor.Props.number, GTA.Offsets.Editor.Props.NEXT, GTA.Offsets.Editor.Props.loc);
                case Kind.DynamicProp: return Loc(GTA.Offsets.Editor.DProps.number, GTA.Offsets.Editor.DProps.NEXT, GTA.Offsets.Editor.DProps.loc);
                case Kind.Door: return Loc(GTA.Offsets.Editor.Doors.number, GTA.Offsets.Editor.Doors.NEXT, GTA.Offsets.Editor.Doors.loc);
                default: return (0, 0, 0, 0, 0);
            }
        }

        private static (long, long, long, long, long) Loc(long number, long next, long loc) => (number, next, loc, loc == 0 ? 0 : loc + 1, loc == 0 ? 0 : loc + 2);

        private static int Count(Kind kind)
        {
            var f = Fields(kind);
            if (f.Number == 0 || f.X == 0 || f.Next == 0)
                return 0;
            int n = new Global(f.Number).Get<int>();
            return n < 0 ? 0 : Math.Min(n, 1000);
        }

        private static Vector3? Position(Kind kind, int index)
        {
            var f = Fields(kind);
            if (f.X == 0 || f.Next == 0 || index < 0)
                return null;
            long at = f.Next * index;
            var p = new Vector3(new Global(f.X + at).Get<float>(), new Global(f.Y + at).Get<float>(), new Global(f.Z + at).Get<float>());
            return p == Vector3.Zero || float.IsNaN(p.X) ? (Vector3?)null : p;
        }

        // How far above its position a label or the cone sits: positions are at the feet / the
        // ground, the label goes over the model.
        private static float Above(Kind kind) => kind == Kind.Actor ? 1.3f : kind == Kind.Vehicle ? 1.8f : 1.0f;

        private static Color ColourOf(Kind kind)
        {
            switch (kind)
            {
                case Kind.Actor: return Color.FromRgb(0xF0, 0x80, 0x70);
                case Kind.Vehicle: return Color.FromRgb(0x7C, 0xB8, 0xFF);
                case Kind.Object: return Color.FromRgb(0xF2, 0xCC, 0x60);
                case Kind.Weapon: return Color.FromRgb(0x84, 0xD8, 0x84);
                default: return Colors.White;
            }
        }

        private static string Name(Kind kind, int index)
        {
            string key, fallback;
            switch (kind)
            {
                case Kind.Actor: key = "em_actor"; fallback = "Actor"; break;
                case Kind.Vehicle: key = "em_vehicle"; fallback = "Vehicle"; break;
                case Kind.Object: key = "em_object"; fallback = "Object"; break;
                case Kind.Weapon: key = "em_weapon"; fallback = "Weapon"; break;
                case Kind.Prop: key = "em_prop"; fallback = "Prop"; break;
                case Kind.DynamicProp: key = "em_dprop"; fallback = "Dynamic prop"; break;
                case Kind.Door: key = "em_door"; fallback = "Door"; break;
                default: return "";
            }
            string t = MainWindow.Instance?.Translation?[key];
            return (string.IsNullOrEmpty(t) || t == "missing translation" ? fallback : t) + " " + (index + 1);
        }

        /// <summary>The cone and name over the selected entry, or nothing.</summary>
        public static List<ScriptDrawer.Shape> Shapes()
        {
            var list = new List<ScriptDrawer.Shape>();
            var kind = (Kind)_kind;
            int index = _index;
            if (kind == Kind.None || index < 0 || index >= Count(kind))
                return list;
            var p = Position(kind, index);
            if (p == null)
                return list;
            var top = p.Value + new Vector3(0, 0, Above(kind) + 0.9f);
            list.Add(new ScriptDrawer.Shape
            {
                Kind = ScriptDrawer.Kind.Marker,
                Parts = ScriptDrawer.Parts.Fill,
                MarkerType = 0,   // upside-down cone
                Position = top,
                Scale = new Vector3(0.7f),
                Colour = SelectedColour,
                FillAlpha = 220,
                Text = Name(kind, index),
                TextPosition = top + new Vector3(0, 0, 0.7f),
                TextScale = 0.38f,
            });
            return list;
        }

        /// <summary>Numbers of the actors, vehicles, objects and weapons around the cursor.</summary>
        public static List<ScriptDrawer.Label> Labels()
        {
            var centre = Functions.Read.GetLocationVec();
            var selKind = (Kind)_kind;
            int selIndex = _index;
            var all = new List<(float Distance, ScriptDrawer.Label Label)>();
            foreach (var kind in new[] { Kind.Actor, Kind.Vehicle, Kind.Object, Kind.Weapon })
            {
                int n = Count(kind);
                for (int i = 0; i < n; i++)
                {
                    if (kind == selKind && i == selIndex)
                        continue;   // it has the cone and its own label
                    var p = Position(kind, i);
                    if (p == null)
                        continue;
                    float d = Vector3.Distance(p.Value, centre);
                    if (d > LabelRange)
                        continue;
                    all.Add((d, new ScriptDrawer.Label { Position = p.Value + new Vector3(0, 0, Above(kind)), Colour = ColourOf(kind), Text = Name(kind, i) }));
                }
            }
            return all.OrderBy(x => x.Distance).Take(ScriptDrawer.MaxLabels).Select(x => x.Label).ToList();
        }
    }
}
