using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xenvious.AdvancedPlacement
{
    /// <summary>One prop of a creator template, relative to the template's anchor.</summary>
    public readonly struct TemplateEntry
    {
        public TemplateEntry(int model, V3 offset, V3 rotation, int tint = 0)
        {
            Model = model;
            Offset = offset;
            Rotation = rotation;
            Tint = tint;
        }

        public int Model { get; }
        public V3 Offset { get; }
        public V3 Rotation { get; }
        public int Tint { get; }
    }

    /// <summary>What a template slot holds right now.</summary>
    public sealed class TemplateSlotInfo
    {
        public int Index { get; set; }
        public bool InUse { get; set; }
        public string Name { get; set; } = "";
        public int PropCount { get; set; }
        public int FirstModel { get; set; }
        public int MainModel { get; set; }
    }

    /// <summary>
    /// The creator's prop templates ("Templates" in the prop menu).
    ///
    /// Layout, from fm_*_creator (func "ptemp" save/load): an array of 10 templates of
    /// 248 slots each, the number in use in a separate global.
    ///   +1+3j    offset of prop j from the anchor (prop 0), in the anchor's heading frame
    ///   +92+3j   rotation of prop j minus (0, 0, anchor heading), per component
    ///   +183+j   model, 0 = unused
    ///   +214+j   tint
    ///   +244     name, text label (empty = "Template N" in the menu)
    /// The creator stores templates in the job itself (block "ptemp"), so a written
    /// template is kept with the next save of the job.
    /// </summary>
    public sealed class CreatorTemplateService
    {
        public const int MaxTemplates = 10;
        public const int MaxPropsPerTemplate = 30;
        public const int NameLength = 15;
        private const int Stride = 248;

        public bool IsAvailable => GTA.Offsets.Editor.templates != 0 && GTA.Offsets.Editor.templates_count != 0;

        private static long SlotBase(int template) => GTA.Offsets.Editor.templates + 1 + (long)template * Stride;

        public int Count => Math.Max(0, Math.Min(MaxTemplates, new Global(GTA.Offsets.Editor.templates_count).Get<int>()));

        public IReadOnlyList<TemplateSlotInfo> ReadSlots()
        {
            int count = Count;
            var result = new List<TemplateSlotInfo>(MaxTemplates);
            for (int t = 0; t < MaxTemplates; t++)
            {
                long b = SlotBase(t);
                var info = new TemplateSlotInfo { Index = t, InUse = t < count };
                if (info.InUse)
                {
                    var models = new List<int>();
                    for (int j = 0; j < MaxPropsPerTemplate; j++)
                    {
                        int model = new Global(b + 183 + j).Get<int>();
                        if (model != 0)
                        {
                            models.Add(model);
                        }
                    }
                    info.PropCount = models.Count;
                    info.FirstModel = models.FirstOrDefault();
                    info.MainModel = models.GroupBy(m => m).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                    info.Name = ReadName(b + 244);
                }
                result.Add(info);
            }
            return result;
        }

        /// <summary>
        /// Writes <paramref name="entries"/> into template <paramref name="slot"/>. A slot
        /// past the used ones becomes the next free one (the creator keeps them packed).
        /// </summary>
        public void Write(int slot, string name, IReadOnlyList<TemplateEntry> entries)
        {
            if (entries == null || entries.Count == 0 || entries.Count > MaxPropsPerTemplate)
            {
                throw new ArgumentException($"A template holds 1 to {MaxPropsPerTemplate} props.");
            }
            int count = Count;
            if (slot < 0 || slot >= MaxTemplates)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }
            if (slot > count)
            {
                slot = count;
            }

            long b = SlotBase(slot);
            for (int j = 0; j < MaxPropsPerTemplate; j++)
            {
                bool used = j < entries.Count;
                TemplateEntry e = used ? entries[j] : default;
                new Global(b + 1 + 3 * j + 0).SetFloat(used ? (float)e.Offset.X : 0f);
                new Global(b + 1 + 3 * j + 1).SetFloat(used ? (float)e.Offset.Y : 0f);
                new Global(b + 1 + 3 * j + 2).SetFloat(used ? (float)e.Offset.Z : 0f);
                new Global(b + 92 + 3 * j + 0).SetFloat(used ? (float)e.Rotation.X : 0f);
                new Global(b + 92 + 3 * j + 1).SetFloat(used ? (float)e.Rotation.Y : 0f);
                new Global(b + 92 + 3 * j + 2).SetFloat(used ? (float)e.Rotation.Z : 0f);
                new Global(b + 183 + j).SetInt(used ? e.Model : 0);
                new Global(b + 214 + j).SetInt(used ? e.Tint : 0);
            }
            WriteName(b + 244, name);

            if (slot == count)
            {
                new Global(GTA.Offsets.Editor.templates_count).SetInt(count + 1);
            }
        }

        /// <summary>
        /// Template entries for a chain of pieces, split into templates of at most 30.
        /// Every template starts with the same anchor prop at the same place, followed
        /// by the next run of pieces. Placing template 1 on the anchor spot, deleting the
        /// anchor and placing template 2 on the same spot continues the chain seamlessly.
        /// </summary>
        public static List<List<TemplateEntry>> BuildChunks(int anchorModel, in PropPose anchor, IReadOnlyList<(int Model, PropPose Pose)> pieces)
        {
            // The creator measures from a helper with the anchor's position and heading
            // only (no pitch or roll), and stores rotations as plain euler differences.
            double heading = anchor.EulerDeg.Z;
            Rot3 frame = Rot3.AxisAngle(V3.UnitZ, heading);
            Rot3 toFrame = frame.Transposed();
            V3 anchorEuler = anchor.EulerDeg;

            var chunks = new List<List<TemplateEntry>>();
            int perChunk = MaxPropsPerTemplate - 1;
            for (int start = 0; start < pieces.Count; start += perChunk)
            {
                var entries = new List<TemplateEntry>
                {
                    new TemplateEntry(anchorModel, V3.Zero, new V3(anchorEuler.X, anchorEuler.Y, 0))
                };
                for (int i = start; i < Math.Min(pieces.Count, start + perChunk); i++)
                {
                    PropPose p = pieces[i].Pose;
                    V3 offset = toFrame * (p.Position - anchor.Position);
                    V3 e = p.EulerDeg;
                    entries.Add(new TemplateEntry(pieces[i].Model, offset, new V3(e.X, e.Y, WrapSigned(e.Z - heading))));
                }
                chunks.Add(entries);
            }
            return chunks;
        }

        private static double WrapSigned(double deg)
        {
            deg %= 360.0;
            if (deg > 180) deg -= 360;
            if (deg <= -180) deg += 360;
            return deg;
        }

        private static string ReadName(long index)
        {
            var bytes = new List<byte>(32);
            for (int k = 0; k < 4; k++)
            {
                bytes.AddRange(BitConverter.GetBytes(new Global(index + k).Get<long>()));
            }
            int end = bytes.IndexOf(0);
            return Encoding.ASCII.GetString(bytes.Take(end < 0 ? bytes.Count : end).ToArray());
        }

        private static void WriteName(long index, string name)
        {
            var bytes = new byte[32];
            string clean = new string((name ?? "").Where(c => c >= 32 && c < 127).Take(NameLength).ToArray());
            Encoding.ASCII.GetBytes(clean, 0, clean.Length, bytes, 0);
            for (int k = 0; k < 4; k++)
            {
                new Global(index + k).SetLong(BitConverter.ToInt64(bytes, 8 * k));
            }
        }
    }
}
