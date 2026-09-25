using System;
using System.Collections.Generic;

namespace Xenvious.AdvancedPlacement
{
    /// <summary>Double-precision 3D vector. Long chains of steps drift visibly in float.</summary>
    public readonly struct V3
    {
        public readonly double X, Y, Z;
        public V3(double x, double y, double z) { X = x; Y = y; Z = z; }

        public static readonly V3 Zero = new V3(0, 0, 0);
        public static readonly V3 UnitX = new V3(1, 0, 0);
        public static readonly V3 UnitY = new V3(0, 1, 0);
        public static readonly V3 UnitZ = new V3(0, 0, 1);

        public static V3 operator +(V3 a, V3 b) => new V3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static V3 operator -(V3 a, V3 b) => new V3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static V3 operator -(V3 a) => new V3(-a.X, -a.Y, -a.Z);
        public static V3 operator *(V3 a, double s) => new V3(a.X * s, a.Y * s, a.Z * s);
        public static V3 operator *(double s, V3 a) => a * s;

        public double Dot(V3 b) => X * b.X + Y * b.Y + Z * b.Z;
        public V3 Cross(V3 b) => new V3(Y * b.Z - Z * b.Y, Z * b.X - X * b.Z, X * b.Y - Y * b.X);
        public double Length => Math.Sqrt(Dot(this));
        public V3 Normalized() { double l = Length; return l < 1e-12 ? Zero : this * (1.0 / l); }
        public bool IsFinite => !(double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z) || double.IsInfinity(X) || double.IsInfinity(Y) || double.IsInfinity(Z));

        public override string ToString() => $"({X:0.###}, {Y:0.###}, {Z:0.###})";
    }

    /// <summary>
    /// Row-major 3x3 rotation. GTA props store their rotation as euler angles in
    /// rotation order 2: R = Rz(yaw) · Rx(pitch) · Ry(roll), X = pitch, Y = roll, Z = yaw.
    /// Verified against placed wallrides and spirals: with this order the step between
    /// consecutive pieces is one constant rotation about one axis.
    /// </summary>
    public readonly struct Rot3
    {
        public readonly double M00, M01, M02, M10, M11, M12, M20, M21, M22;

        public Rot3(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
        {
            M00 = m00; M01 = m01; M02 = m02;
            M10 = m10; M11 = m11; M12 = m12;
            M20 = m20; M21 = m21; M22 = m22;
        }

        public static readonly Rot3 Identity = new Rot3(1, 0, 0, 0, 1, 0, 0, 0, 1);

        public static Rot3 operator *(Rot3 a, Rot3 b) => new Rot3(
            a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20, a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21, a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22,
            a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20, a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21, a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22,
            a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20, a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21, a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22);

        public static V3 operator *(Rot3 m, V3 v) => new V3(
            m.M00 * v.X + m.M01 * v.Y + m.M02 * v.Z,
            m.M10 * v.X + m.M11 * v.Y + m.M12 * v.Z,
            m.M20 * v.X + m.M21 * v.Y + m.M22 * v.Z);

        public Rot3 Transposed() => new Rot3(M00, M10, M20, M01, M11, M21, M02, M12, M22);

        public V3 Right => new V3(M00, M10, M20);    // local +X in world
        public V3 Forward => new V3(M01, M11, M21);  // local +Y in world
        public V3 Up => new V3(M02, M12, M22);       // local +Z in world

        /// <summary>Rotation of <paramref name="degrees"/> about <paramref name="axis"/> (right-handed).</summary>
        public static Rot3 AxisAngle(V3 axis, double degrees)
        {
            V3 a = axis.Normalized();
            double t = degrees * Math.PI / 180.0, c = Math.Cos(t), s = Math.Sin(t), k = 1 - c;
            return new Rot3(
                c + a.X * a.X * k, a.X * a.Y * k - a.Z * s, a.X * a.Z * k + a.Y * s,
                a.Y * a.X * k + a.Z * s, c + a.Y * a.Y * k, a.Y * a.Z * k - a.X * s,
                a.Z * a.X * k - a.Y * s, a.Z * a.Y * k + a.X * s, c + a.Z * a.Z * k);
        }

        /// <summary>From GTA euler degrees (X = pitch, Y = roll, Z = yaw), rotation order 2.</summary>
        public static Rot3 FromGtaEuler(V3 deg)
        {
            return AxisAngle(V3.UnitZ, deg.Z) * AxisAngle(V3.UnitX, deg.X) * AxisAngle(V3.UnitY, deg.Y);
        }

        /// <summary>To GTA euler degrees (X = pitch in [-90, 90], Y = roll, Z = yaw, both in (-180, 180]).</summary>
        public V3 ToGtaEuler()
        {
            // R = Rz Rx Ry:  M21 = sin x,  M20 = -cos x sin y,  M22 = cos x cos y,
            //                M01 = -sin z cos x,  M11 = cos z cos x
            double sx = Math.Max(-1.0, Math.Min(1.0, M21));
            double x = Math.Asin(sx), y, z;
            if (Math.Abs(sx) < 0.999999)
            {
                y = Math.Atan2(-M20, M22);
                z = Math.Atan2(-M01, M11);
            }
            else
            {
                // Pitch ±90°: roll and yaw share an axis; put it all into yaw.
                y = 0;
                z = Math.Atan2(M10, M00);
            }
            const double r2d = 180.0 / Math.PI;
            return new V3(x * r2d, y * r2d, z * r2d);
        }

        /// <summary>Axis (unit) and angle in degrees [0, 180] of this rotation.</summary>
        public void ToAxisAngle(out V3 axis, out double degrees)
        {
            double cos = Math.Max(-1.0, Math.Min(1.0, (M00 + M11 + M22 - 1) / 2));
            degrees = Math.Acos(cos) * 180.0 / Math.PI;
            axis = new V3(M21 - M12, M02 - M20, M10 - M01).Normalized();
            if (axis.Length < 0.5)
            {
                if (degrees < 1e-6)
                {
                    axis = V3.UnitZ;
                    degrees = 0;
                    return;
                }
                // 180°: axis from the largest diagonal term.
                double xx = Math.Sqrt(Math.Max(0, (M00 + 1) / 2)), yy = Math.Sqrt(Math.Max(0, (M11 + 1) / 2)), zz = Math.Sqrt(Math.Max(0, (M22 + 1) / 2));
                if (xx >= yy && xx >= zz) axis = new V3(xx, M01 / (2 * xx), M02 / (2 * xx));
                else if (yy >= zz) axis = new V3(M01 / (2 * yy), yy, M12 / (2 * yy));
                else axis = new V3(M02 / (2 * zz), M12 / (2 * zz), zz);
                axis = axis.Normalized();
            }
        }
    }

    /// <summary>A prop's world position and orientation.</summary>
    public readonly struct PropPose
    {
        public PropPose(V3 position, Rot3 rotation) { Position = position; Rotation = rotation; }

        public V3 Position { get; }
        public Rot3 Rotation { get; }

        public static PropPose FromGta(V3 position, V3 eulerDeg) => new PropPose(position, Rot3.FromGtaEuler(eulerDeg));
        public V3 EulerDeg => Rotation.ToGtaEuler();

        /// <summary>Heading the creator stores next to the rotation (f_6): the yaw, 0..360.</summary>
        public double Heading
        {
            get
            {
                double h = EulerDeg.Z % 360.0;
                return h < 0 ? h + 360.0 : h;
            }
        }

        public V3 ToWorld(V3 local) => Position + Rotation * local;
        public V3 ToLocal(V3 world) => Rotation.Transposed() * (world - Position);
    }

    public enum StepAxis
    {
        /// <summary>No rotation; the piece only moves by the offsets.</summary>
        None,
        /// <summary>World up (Z). Spirals and flat curves: the tilt of the piece does not matter.</summary>
        WorldUp,
        /// <summary>The piece's own X (right) axis. Loops.</summary>
        LocalX,
        /// <summary>The piece's own Y (forward) axis. Corkscrews around the direction of travel.</summary>
        LocalY,
        /// <summary>The piece's own Z (up) axis. Wallrides from a tilted first piece.</summary>
        LocalZ,
        /// <summary>Any axis in the piece's frame (<see cref="RepeatStep.CustomAxis"/>).</summary>
        Custom
    }

    /// <summary>
    /// One step of a repeated placement: how piece k+1 follows from piece k. It is a
    /// screw motion: rotate by <see cref="AngleDeg"/> about an axis through a pivot
    /// that is fixed relative to the piece, then move <see cref="Advance"/> along that
    /// axis. Every wallride, spiral, loop and curve in real jobs is one such step
    /// applied over and over.
    /// </summary>
    public sealed class RepeatStep
    {
        public StepAxis Axis { get; set; } = StepAxis.None;

        /// <summary>Axis in the piece's local frame, used when <see cref="Axis"/> is Custom.</summary>
        public V3 CustomAxis { get; set; } = V3.UnitZ;

        /// <summary>Point the piece turns around, in the piece's local frame (meters).</summary>
        public V3 Pivot { get; set; } = V3.Zero;

        /// <summary>Signed turn per piece in degrees (right-handed about the axis).</summary>
        public double AngleDeg { get; set; }

        /// <summary>Move along the axis per piece (meters). Rise of a spiral, drift of a loop.</summary>
        public double Advance { get; set; }

        /// <summary>Extra move per piece in the piece's local frame, applied after the turn (meters).</summary>
        public V3 Offset { get; set; } = V3.Zero;

        public RepeatStep Clone() => (RepeatStep)MemberwiseClone();

        /// <summary>Axis direction in world space for a piece with the given pose.</summary>
        public V3 WorldAxis(in PropPose pose)
        {
            switch (Axis)
            {
                case StepAxis.WorldUp: return V3.UnitZ;
                case StepAxis.LocalX: return pose.Rotation.Right;
                case StepAxis.LocalY: return pose.Rotation.Forward;
                case StepAxis.LocalZ: return pose.Rotation.Up;
                case StepAxis.Custom: return (pose.Rotation * CustomAxis).Normalized();
                default: return V3.UnitZ;
            }
        }

        /// <summary>Pose of the next piece.</summary>
        public PropPose Apply(in PropPose pose)
        {
            V3 position = pose.Position;
            Rot3 rotation = pose.Rotation;

            if (Axis != StepAxis.None && Math.Abs(AngleDeg) > 1e-9)
            {
                V3 axis = WorldAxis(pose);
                V3 center = pose.ToWorld(Pivot);
                Rot3 turn = Rot3.AxisAngle(axis, AngleDeg);
                position = center + turn * (position - center);
                rotation = turn * rotation;
            }

            if (Axis != StepAxis.None && Math.Abs(Advance) > 1e-12)
            {
                position = position + WorldAxis(pose) * Advance;
            }

            if (Offset.Length > 1e-12)
            {
                position = position + rotation * Offset;
            }

            return new PropPose(position, rotation);
        }
    }

    /// <summary>Builds and analyses repeated placements.</summary>
    public static class RepeatPlanner
    {
        /// <summary>
        /// <paramref name="count"/> poses. With <paramref name="includeStart"/> the first is
        /// <paramref name="start"/> itself; otherwise the chain begins one step after it
        /// (continuing from an already placed prop).
        /// </summary>
        public static List<PropPose> Build(in PropPose start, RepeatStep step, int count, bool includeStart)
        {
            var result = new List<PropPose>(Math.Max(count, 0));
            PropPose current = start;
            if (includeStart && count > 0)
            {
                result.Add(current);
            }
            while (result.Count < count)
            {
                current = step.Apply(current);
                if (!current.Position.IsFinite)
                {
                    break;
                }
                result.Add(current);
            }
            return result;
        }

        /// <summary>
        /// The step that turns <paramref name="a"/> into <paramref name="b"/>, expressed
        /// in a's frame: a custom local axis, pivot, angle and advance. With
        /// <paramref name="snapAxes"/> an axis within about 0.5° of world up or a local
        /// axis is reported as that axis, which reads better and absorbs the rounding of
        /// hand-placed props; without it, applying the result to a gives b exactly.
        /// </summary>
        public static RepeatStep Detect(in PropPose a, in PropPose b, bool snapAxes = true)
        {
            Rot3 local = a.Rotation.Transposed() * b.Rotation;
            V3 t = a.ToLocal(b.Position);
            local.ToAxisAngle(out V3 axis, out double angle);

            if (angle < 0.01)
            {
                return new RepeatStep { Axis = StepAxis.None, Offset = t };
            }

            double advance = t.Dot(axis);
            V3 tPerp = t - axis * advance;
            // The origin moves by (I - R)(-c) around the pivot c (perpendicular to the axis):
            // c = ½ (t⊥ + cot(θ/2) · axis × t⊥).
            double cot = 1.0 / Math.Tan(angle * Math.PI / 360.0);
            V3 pivot = (tPerp + axis.Cross(tPerp) * cot) * 0.5;

            var step = new RepeatStep
            {
                Axis = StepAxis.Custom,
                CustomAxis = axis,
                Pivot = pivot,
                AngleDeg = angle,
                Advance = advance
            };

            if (!snapAxes)
            {
                return step;
            }

            // Snap to the named axes when they fit, so the user sees "Z" rather than (0, 0, 1).
            V3 worldAxis = a.Rotation * axis;
            if (Math.Abs(worldAxis.Z) > 0.99996)
            {
                step.Axis = StepAxis.WorldUp;
                step.AngleDeg = worldAxis.Z > 0 ? angle : -angle;
                step.Advance = worldAxis.Z > 0 ? advance : -advance;
            }
            else if (Math.Abs(axis.X) > 0.99996) { step.Axis = StepAxis.LocalX; step.AngleDeg = Math.Sign(axis.X) * angle; step.Advance = Math.Sign(axis.X) * advance; }
            else if (Math.Abs(axis.Y) > 0.99996) { step.Axis = StepAxis.LocalY; step.AngleDeg = Math.Sign(axis.Y) * angle; step.Advance = Math.Sign(axis.Y) * advance; }
            else if (Math.Abs(axis.Z) > 0.99996) { step.Axis = StepAxis.LocalZ; step.AngleDeg = Math.Sign(axis.Z) * angle; step.Advance = Math.Sign(axis.Z) * advance; }

            return step;
        }

        /// <summary>
        /// Extent of a box (<paramref name="size"/> along local X/Y/Z) measured along the
        /// local direction <paramref name="dir"/>.
        /// </summary>
        public static double ExtentAlong(V3 size, V3 dir)
        {
            V3 d = dir.Normalized();
            return Math.Abs(d.X) * size.X + Math.Abs(d.Y) * size.Y + Math.Abs(d.Z) * size.Z;
        }

        /// <summary>
        /// Turn per piece (degrees, positive) at which consecutive pieces sit
        /// <paramref name="spacing"/> meters apart while turning around a pivot at
        /// <paramref name="radius"/> meters and moving <paramref name="advance"/> along the
        /// axis. 0 when the spacing cannot be reached.
        /// </summary>
        public static double AngleForSpacing(double spacing, double radius, double advance)
        {
            if (radius < 1e-6)
            {
                return 0;
            }
            double chordSq = spacing * spacing - advance * advance;
            if (chordSq <= 0)
            {
                return 0;
            }
            double s = Math.Sqrt(chordSq) / (2 * radius);
            if (s >= 1)
            {
                return 180;
            }
            return 2 * Math.Asin(s) * 180.0 / Math.PI;
        }

        /// <summary>Direction the piece travels in at the start, in its local frame.</summary>
        public static V3 TravelDirection(RepeatStep step)
        {
            var origin = new PropPose(V3.Zero, Rot3.Identity);
            PropPose next = step.Apply(origin);
            V3 d = next.Position.Normalized();
            return d.Length > 0.5 ? d : V3.UnitY;
        }
    }
}
