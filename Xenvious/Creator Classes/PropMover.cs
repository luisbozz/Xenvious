using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xenvious
{
    /// <summary>
    /// Moves a prop between the static list (Global_5242880) and the dynamic list, which
    /// the creator offers no way to do.
    ///
    /// The prop is appended to the other list with every field both lists share
    /// (position, rotation, heading, model, team rules, colour, LOD, ...) on top of the
    /// creator's working entry, then removed from its list the way the creator deletes a
    /// prop (the entries after it move down one slot). A rebuild makes the creator show
    /// the result; it deletes and recreates every entity from the job data.
    /// </summary>
    public static class PropMover
    {
        /// <summary>Static field and the dynamic field holding the same value.</summary>
        private static IEnumerable<(long Static, long Dynamic)> SharedFields()
        {
            var p = typeof(GTA.Offsets.Editor.Props);
            var d = typeof(GTA.Offsets.Editor.DProps);
            string[] same =
            {
                "loc", "vrot", "head", "model", "asst", "asso", "asss", "pasc",
                "asst2", "asso2", "asss2", "pasc2", "asst3", "asso3", "asss3", "pasc3",
                "asst4", "asso4", "asss4", "pasc4", "prpct", "prcra", "prpcr", "prpbs"
            };
            var pairs = same.Select(n => (n, n)).ToList();
            pairs.Add(("prpclr", "prpdclr"));    // colour
            pairs.Add(("ploddist", "dpLODd"));   // LOD distance

            long p0 = GTA.Offsets.Editor.Props.loc;
            long d0 = GTA.Offsets.Editor.DProps.loc;
            foreach (var (sp, dp) in pairs)
            {
                var sf = p.GetField(sp);
                var df = d.GetField(dp);
                if (sf == null || df == null)
                    continue;
                long sv = (long)sf.GetValue(null);
                long dv = (long)df.GetValue(null);
                if (sv == 0 || dv == 0)
                    continue;
                int width = sp == "loc" || sp == "vrot" ? 3 : 1;
                for (int k = 0; k < width; k++)
                    yield return (sv - p0 + k, dv - d0 + k);
            }
        }

        private sealed class List
        {
            public long Count, First, Stride;
            public long BitsField;   // prpbs: set to 0 on the moved entry
            public int Capacity => new Global(First - 1).Get<int>();
        }

        private static List StaticList() => new List
        {
            Count = GTA.Offsets.Editor.Props.number, First = GTA.Offsets.Editor.Props.loc,
            Stride = GTA.Offsets.Editor.Props.NEXT,
            BitsField = GTA.Offsets.Editor.Props.prpbs - GTA.Offsets.Editor.Props.loc
        };

        private static List DynamicList() => new List
        {
            Count = GTA.Offsets.Editor.DProps.number, First = GTA.Offsets.Editor.DProps.loc,
            Stride = GTA.Offsets.Editor.DProps.NEXT,
            BitsField = GTA.Offsets.Editor.DProps.prpbs - GTA.Offsets.Editor.DProps.loc
        };

        /// <summary>Why the move cannot happen, or null.</summary>
        public static string Check(bool toDynamic, int index)
        {
            var from = toDynamic ? StaticList() : DynamicList();
            var to = toDynamic ? DynamicList() : StaticList();
            int count = new Global(from.Count).Get<int>();
            if (index < 0 || index >= count)
                return "index";
            int limit = toDynamic ? to.Capacity : Math.Min(to.Capacity, AdvancedPlacement.PropPlacementService.PropLimit);
            if (new Global(to.Count).Get<int>() >= limit)
                return "full";
            return null;
        }

        /// <summary>Moves entry <paramref name="index"/>; returns whether the creator rebuilt the map.</summary>
        public static async Task<bool> MoveAsync(bool toDynamic, int index)
        {
            if (Check(toDynamic, index) != null)
                return false;
            var from = toDynamic ? StaticList() : DynamicList();
            var to = toDynamic ? DynamicList() : StaticList();

            int fromCount = new Global(from.Count).Get<int>();
            int toCount = new Global(to.Count).Get<int>();
            byte[] source = CreatorMap.ReadSlots(from.First + index * from.Stride, (int)from.Stride);

            // The slot after the last entry is the creator's working entry: what it copies
            // into the list when the player places a prop, already holding the creator's
            // defaults (-1 for "none" in several fields). The new entry starts from it,
            // takes every shared field from the moved prop, and drops the bit fields.
            byte[] entry = CreatorMap.ReadSlots(to.First + toCount * to.Stride, (int)to.Stride);
            foreach (var (s, d) in SharedFields())
            {
                long src = toDynamic ? s : d;
                long dst = toDynamic ? d : s;
                if (src < from.Stride && dst < to.Stride)
                    Buffer.BlockCopy(source, (int)src * 8, entry, (int)dst * 8, 8);
            }
            Array.Clear(entry, (int)to.BitsField * 8, 8);
            // A dynamic prop's obref is the index of a linked mission object, -1 for none.
            // 0 links it to object 0, and the creator hangs when it cleans that link up on
            // leaving the job.
            if (toDynamic && GTA.Offsets.Editor.DProps.obref != 0)
                Buffer.BlockCopy(BitConverter.GetBytes(-1L), 0, entry,
                    (int)(GTA.Offsets.Editor.DProps.obref - GTA.Offsets.Editor.DProps.loc) * 8, 8);
            CreatorMap.WriteSlots(to.First + toCount * to.Stride, entry);
            new Global(to.Count).SetInt(toCount + 1);

            // Remove from the source list the way the creator deletes a prop: later entries
            // move down one slot. The freed last slot gets the working entry, which moves
            // down with them, so the defaults stay behind the last entry.
            int capacity = from.Capacity;
            byte[] working = fromCount < capacity
                ? CreatorMap.ReadSlots(from.First + fromCount * from.Stride, (int)from.Stride)
                : null;
            int after = fromCount - index - 1;
            if (after > 0)
            {
                byte[] rest = CreatorMap.ReadSlots(from.First + (index + 1) * from.Stride, (int)(after * from.Stride));
                CreatorMap.WriteSlots(from.First + index * from.Stride, rest);
            }
            CreatorMap.WriteSlots(from.First + (fromCount - 1) * from.Stride, working ?? new byte[from.Stride * 8]);
            new Global(from.Count).SetInt(fromCount - 1);

            return await CreatorMap.RebuildAsync().ConfigureAwait(true);
        }
    }
}
