namespace Xenvious
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Float-only Math-Utility. Verwendet überall diese Klasse (Alias: using MathF = Xenvious.Maths.MathF;).
    /// </summary>
    public static class MathF
    {
        public static float PI => (float)Math.PI;
        public static float Tau = 2f * PI;

        public static float Sin(float x) => (float)Math.Sin(x);
        public static float Cos(float x) => (float)Math.Cos(x);
        public static float Tan(float x) => (float)Math.Tan(x);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Sqrt(float x) => (float)Math.Sqrt(x);

        public static float Max(float x, float y) => Math.Max(x, y);
        public static float Min(float x, float y) => Math.Min(x, y);
        public static float Abs(float x) => Math.Abs(x);

        public static int Clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
        public static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
        public static double Clamp(double value, double min, double max) => value < min ? min : (value > max ? max : value);
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

        // Winkel & Konvertierung
        public static float Deg2Rad(float d) => d * (PI / 180f);
        public static float Rad2Deg(float r) => r * (180f / PI);
        public static float WrapAngleDeg(float a)
        {
            float r = a % 360f;
            if (r < 0f) r += 360f;
            return r;
        }
        public static float AngleDeltaDeg(float a, float b)
        {
            float d = WrapAngleDeg(b - a);
            if (d > 180f) d -= 360f;
            return d;
        }

        // Vektor-Utilities
        public static float Dot(in Vector3 a, in Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vector3 Cross(in Vector3 a, in Vector3 b)
            => new Vector3(a.Y * b.Z - a.Z * b.Y,
                           a.Z * b.X - a.X * b.Z,
                           a.X * b.Y - a.Y * b.X);

        public static float Length(in Vector3 v) => Sqrt(Dot(v, v));
        public static Vector3 NormalizeSafe(in Vector3 v, float eps = 1e-6f)
        {
            var len = Length(v);
            if (len < eps || float.IsNaN(len) || float.IsInfinity(len)) return new Vector3(0f, 0f, 0f);
            return v / len;
        }

        // Rotate v um Z-Achse (Grad)
        public static Vector3 RotateAroundZ(in Vector3 v, float deg)
        {
            float r = Deg2Rad(deg);
            float c = Cos(r), s = Sin(r);
            return new Vector3(v.X * c - v.Y * s, v.X * s + v.Y * c, v.Z);
        }

        // Remap
        public static float Remap(float v, float inMin, float inMax, float outMin, float outMax)
        {
            if (Abs(inMax - inMin) < 1e-6f) return outMin;
            float t = (v - inMin) / (inMax - inMin);
            return Lerp(outMin, outMax, t);
        }

        // -------- Polyfills/Guards für IsFinite --------
        public static bool IsFiniteScalar(float f) => !(float.IsNaN(f) || float.IsInfinity(f));
        public static bool IsFinite(Vector3 v) => IsFiniteScalar(v.X) && IsFiniteScalar(v.Y) && IsFiniteScalar(v.Z);
        public static float CoerceFinite(float f, float fallback = 0f) => (float.IsNaN(f) || float.IsInfinity(f)) ? fallback : f;
        public static Vector3 Sanitize(Vector3 v) => new Vector3(CoerceFinite(v.X), CoerceFinite(v.Y), CoerceFinite(v.Z));
        public static float AsinF(float x) => (float)System.Math.Asin(Clamp(x, -1.0, 1.0));
        public static float AtanF(float x) => (float)System.Math.Atan(x);
    }
}
