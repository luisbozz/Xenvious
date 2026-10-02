using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows.Media;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Shapes and labels drawn in the game world by the injected drawer ("custom funcs in extra
    /// page", dl_main), which runs every frame inside the creator script once the script features
    /// are on. Xenvious only fills the data page of the script's extra space (ScriptSpace); the
    /// payload reads it as string page 63, so neither globals nor statics are involved.
    ///
    /// Data page (byte offsets): 0x00 "STRING" (the text label BEGIN_TEXT_COMMAND_DISPLAY_TEXT
    /// needs), 0x08 entry count, 0x10 a heartbeat the drawer counts up every frame, 0x100 the
    /// entries, 0x100 bytes each. An entry is 8 byte slots with the value in the low 4 bytes:
    /// 0 kind, 1 parts, 2..9 prism corners (x, y) or marker type, position and scale, 10/11 prism
    /// bottom and top, 12..16 r, g, b, fill alpha, line alpha, 17..19 label position, 20 label
    /// scale, 21..24 label r, g, b, a, 25..31 the label (56 bytes, NUL terminated).
    /// </summary>
    public static class ScriptDrawer
    {
        public const int MaxShapes = 63;
        private const int CountAt = 0x08, BeatAt = 0x10, EntriesAt = 0x100, EntrySize = 0x100;
        private const int TextAt = 25 * 8, TextBytes = 56;
        private static readonly byte[] Header = Encoding.ASCII.GetBytes("STRING\0\0");

        public enum Kind { Off = 0, Prism = 1, Marker = 2, Label = 3 }

        [Flags]
        public enum Parts { None = 0, Fill = 1, Outline = 2, Caps = 4, Label = 8 }

        public class Shape
        {
            public Kind Kind;
            public Parts Parts;
            /// <summary>Prism: the four corners of its base, in order around it.</summary>
            public Vector2[] Corners = new Vector2[4];
            public float Bottom, Top;
            /// <summary>Marker: DRAW_MARKER type (28 is a sphere whose scale is its radius).</summary>
            public int MarkerType;
            public Vector3 Position, Scale;
            public Color Colour = Colors.White;
            public byte FillAlpha = 60, LineAlpha = 200;
            public string Text;
            public Vector3 TextPosition;
            public float TextScale = 0.35f;
            public Color TextColour = Colors.White;

            /// <summary>
            /// A box the way the game tests angled areas: p1 and p2 are the middles of two opposite
            /// sides, width is measured across them, and the height runs from p1.z to p2.z.
            /// </summary>
            public static Shape AngledArea(Vector3 p1, Vector3 p2, float width)
            {
                var axis = new Vector2(p2.X - p1.X, p2.Y - p1.Y);
                float length = axis.Length();
                if (length < 0.01f || width <= 0)
                    return null;
                var side = new Vector2(-axis.Y, axis.X) / length * (width / 2);
                var a = new Vector2(p1.X, p1.Y);
                var b = new Vector2(p2.X, p2.Y);
                float bottom = Math.Min(p1.Z, p2.Z), top = Math.Max(p1.Z, p2.Z);
                return new Shape
                {
                    Kind = Kind.Prism,
                    // A flat area has no sides to see, so it gets its top and bottom.
                    Parts = Parts.Fill | Parts.Outline | (top - bottom < 0.1f ? Parts.Caps : Parts.None),
                    Corners = new[] { a + side, b + side, b - side, a - side },
                    Bottom = bottom,
                    Top = top,
                    TextPosition = (p1 + p2) / 2,
                };
            }

            public static Shape Sphere(Vector3 centre, float radius)
            {
                if (radius <= 0)
                    return null;
                return new Shape
                {
                    Kind = Kind.Marker,
                    Parts = Parts.Fill,
                    MarkerType = 28,
                    Position = centre,
                    Scale = new Vector3(radius),
                    TextPosition = centre,
                };
            }

            internal byte[] ToBytes()
            {
                var b = new byte[EntrySize];
                void I(int slot, int v) => Array.Copy(BitConverter.GetBytes(v), 0, b, slot * 8, 4);
                void F(int slot, float v) => Array.Copy(BitConverter.GetBytes(v), 0, b, slot * 8, 4);
                var parts = Parts;
                if (!string.IsNullOrEmpty(Text))
                    parts |= Parts.Label;
                I(0, (int)Kind);
                I(1, (int)parts);
                if (Kind == Kind.Prism)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        F(2 + 2 * i, Corners[i].X);
                        F(3 + 2 * i, Corners[i].Y);
                    }
                    F(10, Bottom);
                    F(11, Top);
                }
                else if (Kind == Kind.Marker)
                {
                    I(2, MarkerType);
                    F(3, Position.X); F(4, Position.Y); F(5, Position.Z);
                    F(6, Scale.X); F(7, Scale.Y); F(8, Scale.Z);
                }
                I(12, Colour.R); I(13, Colour.G); I(14, Colour.B); I(15, FillAlpha); I(16, LineAlpha);
                F(17, TextPosition.X); F(18, TextPosition.Y); F(19, TextPosition.Z);
                F(20, TextScale);
                I(21, TextColour.R); I(22, TextColour.G); I(23, TextColour.B); I(24, TextColour.A);
                byte[] text = TextBytesOf(Text);
                Array.Copy(text, 0, b, TextAt, text.Length);
                return b;
            }
        }

        // UTF-8, cut at a character border so the last byte of the field always stays NUL. "~"
        // starts a format code in game text.
        private static byte[] TextBytesOf(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new byte[0];
            text = text.Replace('~', '-');
            byte[] all = Encoding.UTF8.GetBytes(text);
            int n = Math.Min(all.Length, TextBytes - 1);
            while (n > 0 && n < all.Length && (all[n] & 0xC0) == 0x80)
                n--;
            var cut = new byte[n];
            Array.Copy(all, cut, n);
            return cut;
        }

        // What the data page holds, so unchanged entries are not written again.
        private static long page;
        private static byte[][] written = new byte[MaxShapes][];
        private static int writtenCount;
        private static bool warnedFull;
        private static int lastBeat;
        private static DateTime beatSeen;
        private static volatile string running = "";

        /// <summary>
        /// Makes the drawer of a creator script draw these shapes (at most <see cref="MaxShapes"/>);
        /// false when the script has no data page.
        /// </summary>
        public static bool Show(string script, IList<Shape> shapes)
        {
            if (!MainWindow.m.IsProcOpen || string.IsNullOrEmpty(script))
                return false;
            var space = ScriptSpace.Get(script);
            if (space == null || space.Data == 0)
            {
                running = "";
                return false;
            }
            try
            {
                // Another script, or a page that was set up again (its header is gone): start over.
                if (space.Data != page || !Same(Read(space.Data, Header.Length), Header))
                {
                    page = space.Data;
                    written = new byte[MaxShapes][];
                    writtenCount = 0;
                    lastBeat = 0;
                    Write(page, Header);
                    Write(page + CountAt, BitConverter.GetBytes(0));
                }

                int n = Math.Min(shapes.Count, MaxShapes);
                if (shapes.Count > MaxShapes && !warnedFull)
                    Log.Warn($"{shapes.Count} shapes to draw, the drawer takes {MaxShapes}", source: "ScriptDrawer");
                warnedFull = shapes.Count > MaxShapes;

                // Fewer entries: the count goes down first, so the drawer never reads one being rewritten.
                if (n < writtenCount)
                {
                    Write(page + CountAt, BitConverter.GetBytes(n));
                    writtenCount = n;
                }
                for (int i = 0; i < n; i++)
                {
                    byte[] entry = shapes[i].ToBytes();
                    if (written[i] != null && Same(written[i], entry))
                        continue;
                    Write(page + EntriesAt + i * EntrySize, entry);
                    written[i] = entry;
                }
                if (n != writtenCount)
                {
                    Write(page + CountAt, BitConverter.GetBytes(n));
                    writtenCount = n;
                }

                // The heartbeat tells whether the payload really runs (script features on, hook in).
                int beat = BitConverter.ToInt32(Read(page + BeatAt, 4), 0);
                if (beat != lastBeat)
                {
                    lastBeat = beat;
                    beatSeen = DateTime.UtcNow;
                }
                running = DateTime.UtcNow - beatSeen < TimeSpan.FromSeconds(3) ? script : "";
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug("Show " + script + ": " + ex.Message, source: "ScriptDrawer");
                page = 0;
                running = "";
                return false;
            }
        }

        /// <summary>True while the drawer of this script ran in the last seconds (as seen by <see cref="Show"/>).</summary>
        public static bool Running(string script) => !string.IsNullOrEmpty(script) && running == script;

        private static bool Same(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }

        private static byte[] Read(long address, int length) => MainWindow.m.memory(address.ToString("X")).GetBytes(length);

        private static void Write(long address, byte[] bytes) => MainWindow.m.memory(address.ToString("X")).SetBytes(bytes);
    }
}
