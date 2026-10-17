using System;
using System.Globalization;
using System.Text;

namespace Xenvious
{
    /// <summary>The Global Editor's value types, in the order of its type dropdowns.</summary>
    public enum EditorKind { Byte, Short, Int, Long, Float, Double, String }

    /// <summary>
    /// Turns the Global Editor's text into the bytes of a type and back, for reading, writing and
    /// freezing. Numbers are read with the invariant culture (the UI thread is de-DE); a comma in
    /// a float is taken as the decimal point. Whole numbers may be given signed or unsigned, so
    /// 65535 fits a Short and 4294967295 an Integer, as the bits show them.
    /// </summary>
    public static class EditorValue
    {
        /// <summary>Bytes read for a string: GTA's longest text labels are 64 bytes.</summary>
        public const int StringBytes = 64;

        public static int Size(EditorKind kind)
        {
            switch (kind)
            {
                case EditorKind.Byte: return 1;
                case EditorKind.Short: return 2;
                case EditorKind.Int: return 4;
                case EditorKind.Float: return 4;
                case EditorKind.String: return StringBytes;
                default: return 8;
            }
        }

        /// <summary>Bits the Bits card shows for the type, 0 for types without bits.</summary>
        public static int Bits(EditorKind kind) =>
            kind == EditorKind.Byte ? 8 : kind == EditorKind.Short ? 16 : kind == EditorKind.Int ? 32 : 0;

        /// <summary>The bytes to write. Throws FormatException or OverflowException for text that does not fit.</summary>
        public static byte[] Encode(EditorKind kind, string text)
        {
            string s = (text ?? "").Trim();
            var inv = CultureInfo.InvariantCulture;
            switch (kind)
            {
                case EditorKind.Byte:
                    return new[] { (byte)Whole(s, sbyte.MinValue, byte.MaxValue) };
                case EditorKind.Short:
                    return BitConverter.GetBytes((short)Whole(s, short.MinValue, ushort.MaxValue));
                case EditorKind.Int:
                    return BitConverter.GetBytes((int)Whole(s, int.MinValue, uint.MaxValue));
                case EditorKind.Long:
                    if (ulong.TryParse(s, NumberStyles.None, inv, out ulong u))
                        return BitConverter.GetBytes(u);
                    return BitConverter.GetBytes(long.Parse(s, NumberStyles.Integer, inv));
                case EditorKind.Float:
                    return BitConverter.GetBytes(float.Parse(s.Replace(',', '.'), NumberStyles.Float, inv));
                case EditorKind.Double:
                    return BitConverter.GetBytes(double.Parse(s.Replace(',', '.'), NumberStyles.Float, inv));
                default:
                    // The terminating zero ends the old text when the new one is shorter.
                    byte[] t = Encoding.UTF8.GetBytes(text ?? "");
                    var b = new byte[t.Length + 1];
                    Array.Copy(t, b, t.Length);
                    return b;
            }
        }

        private static long Whole(string s, long min, long max)
        {
            long v = long.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (v < min || v > max)
                throw new OverflowException();
            return v;
        }

        public static string Decode(EditorKind kind, byte[] b)
        {
            var inv = CultureInfo.InvariantCulture;
            switch (kind)
            {
                case EditorKind.Byte: return b[0].ToString(inv);
                case EditorKind.Short: return BitConverter.ToInt16(b, 0).ToString(inv);
                case EditorKind.Int: return BitConverter.ToInt32(b, 0).ToString(inv);
                case EditorKind.Long: return BitConverter.ToInt64(b, 0).ToString(inv);
                case EditorKind.Float: return BitConverter.ToSingle(b, 0).ToString("R", inv);
                case EditorKind.Double: return BitConverter.ToDouble(b, 0).ToString("R", inv);
                default:
                    int end = Array.IndexOf(b, (byte)0);
                    return Encoding.UTF8.GetString(b, 0, end < 0 ? b.Length : end);
            }
        }

        /// <summary>The value as an unsigned whole number, for the Bits card.</summary>
        public static ulong Raw(byte[] b, EditorKind kind)
        {
            ulong v = 0;
            for (int i = Math.Min(Size(kind), b.Length) - 1; i >= 0; i--)
                v = (v << 8) | b[i];
            return v;
        }

        // Names in the saved freeze lists (config.ini); older versions saved a byte as "bytes".
        private static readonly string[] ConfigNames = { "bytes", "short", "int", "long", "float", "double", "string" };

        public static string ConfigName(EditorKind kind) => ConfigNames[(int)kind];

        public static EditorKind FromConfig(string name)
        {
            int i = Array.IndexOf(ConfigNames, name);
            return i < 0 ? EditorKind.Int : (EditorKind)i;
        }
    }
}
