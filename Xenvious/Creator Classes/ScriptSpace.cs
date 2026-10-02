using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Code space of our own behind a creator script, allocated by the game itself.
    ///
    /// The game sizes a script resource from the virtual page flags of its entry in the
    /// script_rel.rpf table of contents, which it keeps in memory. The chunks are laid out
    /// largest first, so a layout that ends behind the shipped one keeps every offset as long as
    /// each of its chunk borders inside the shipped data is a shipped border too. The game
    /// allocates the layout, lists it in the resource's page map and frees it with the script.
    /// The extra bytes hold whatever the heap held before.
    ///
    /// Two limits, both found on Enhanced 1.0.1158 (DM):
    /// - The page map is a placeholder inside the script data, sized for the shipped chunk
    ///   count; every chunk more overwrites the string page array behind it (crash).
    /// - With two or more chunks wholly behind the shipped data the load never finishes.
    /// So the layouts use bigger chunks (base 0x2000: one 0x80000, then 0x40000s) and the
    /// last one reaches past the shipped data; DM loaded that way with 17 chunks.
    ///
    /// Once the script is loaded, the first 16 KB slot holds a longer copy of the code page array
    /// (scrProgram+0x10 is pointed at it), and every slot between the first and the last becomes a
    /// code page after the script's own: page N starts at pc N &lt;&lt; 14. The VM does not check pc
    /// against the code size, so CALLs into these pages run. Measured on Enhanced 1.0.1158
    /// (Capture, DM, Race).
    ///
    /// The last slot is a data page for Xenvious and the payload (ScriptVars, ScriptDrawer). The script reaches
    /// it as string page 63 of a string page array of our own (scrProgram+0x68 switched the same
    /// way): PUSH_CONST_U24 0xFC000 + offset; STRING gives a pointer into it, and LOAD and STORE
    /// work on any pointer. The VM's STRING handler takes pages[index &gt;&gt; 14] + (index &amp; 0x3FFF)
    /// with no bounds check (Enhanced 1.0.1158), so the page index only has to exist in our array.
    /// Statics cannot serve here: a thread copies them into its stack when it starts, and their
    /// count is fixed by the program.
    ///
    /// The flags are only read when a script is loaded fresh. A creator the game still has
    /// cached from before keeps its old size until a session change frees it; a flag changed
    /// while the script was cached was harmless when the game freed it.
    /// </summary>
    public static class ScriptSpace
    {
        private const int PageSize = 0x4000;
        private const long ProgramPageMap = 0x08, ProgramCodeBlocks = 0x10, ProgramCodeSize = 0x1C;
        private const long ProgramStrings = 0x68, ProgramStringsSize = 0x70;
        // The string page array sits at the end of the first slot, behind the code page array.
        private const int StringArray = 0x3E00;
        /// <summary>String page index of the data page; STRING index = DataStringPage &lt;&lt; 14 + offset.</summary>
        public const int DataStringPage = 63;
        private const long PageMapCount = 0x8, PageMapEntries = 0x10;

        private class Reservation
        {
            public string Script;
            public uint Shipped;
            public uint Target;
            // Address of the virtual flags in the table of contents, 0 until found.
            public long Entry;
            public bool Warned;
        }

        public class Space
        {
            /// <summary>pc of the first extra page.</summary>
            public int Base;
            /// <summary>Address of each extra page, in pc order.</summary>
            public List<long> Pages = new List<long>();
            /// <summary>Address of the data page (string page <see cref="DataStringPage"/>).</summary>
            public long Data;

            /// <summary>Address of a pc in the extra pages; 0 outside them.</summary>
            public long Address(int pc)
            {
                int i = (pc - Base) / PageSize;
                return pc >= Base && i < Pages.Count ? Pages[i] + (pc - Base) % PageSize : 0;
            }
        }

        private static readonly object Gate = new object();
        private static List<Reservation> reservations;
        private static long scriptPack;

        /// <summary>Absolute address of the game's packfile table; 0 without the pattern.</summary>
        public static long PackfileTable;

        private static void Load()
        {
            if (reservations != null)
                return;
            var list = new List<Reservation>();
            foreach (var kv in GTA.Offsets.Editor.ScriptSpaceReservations)
            {
                // "<shipped virtual flags>, <virtual flags to load with>"
                string[] parts = kv.Value.Split(',');
                if (parts.Length != 2
                    || !uint.TryParse(parts[0].Trim().Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint shipped)
                    || !uint.TryParse(parts[1].Trim().Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint target))
                {
                    Log.Warn("Bad script space entry " + kv.Key + " = " + kv.Value, source: "ScriptSpace");
                    continue;
                }
                string problem = CheckLayout(shipped, target);
                if (problem != null)
                {
                    Log.Warn($"Script space for {kv.Key}: 0x{target:X8} does not fit 0x{shipped:X8}: {problem}", source: "ScriptSpace");
                    continue;
                }
                list.Add(new Reservation { Script = kv.Key, Shipped = shipped, Target = target });
            }
            // Published whole: other threads read it without the lock.
            reservations = list;
        }

        /// <summary>Forgets the addresses of the last game process.</summary>
        public static void Reset()
        {
            scriptPack = 0;
            if (reservations == null)
                return;
            foreach (var r in reservations)
            {
                r.Entry = 0;
                r.Warned = false;
            }
        }

        /// <summary>
        /// Makes the game allocate the extra chunks for every configured script the next time it
        /// loads one fresh. Once done it costs one read per script.
        /// </summary>
        public static void Reserve()
        {
            if (!MainWindow.m.IsProcOpen || PackfileTable == 0)
                return;
            Load();
            foreach (var r in reservations)
            {
                try
                {
                    if (r.Entry == 0)
                        r.Entry = FindEntry(r.Script + ".ysc");
                    if (r.Entry == 0)
                        continue;
                    uint now = Read<uint>(r.Entry);
                    if (now == r.Target)
                        continue;
                    if (now != r.Shipped)
                    {
                        if (!r.Warned)
                            Log.Warn($"{r.Script}.ysc has page flags 0x{now:X8}, expected 0x{r.Shipped:X8}; left as it is", source: "ScriptSpace");
                        r.Warned = true;
                        continue;
                    }
                    MainWindow.m.memory(r.Entry.ToString("X")).SetBytes(BitConverter.GetBytes(r.Target));
                    Log.Debug($"{r.Script}: page flags 0x{r.Target:X8} from its next load on", source: "ScriptSpace");
                }
                catch (Exception ex)
                {
                    Log.Debug("Reserve " + r.Script + ": " + ex.Message, source: "ScriptSpace");
                    r.Entry = 0;
                }
            }
        }

        /// <summary>
        /// The extra pages of a loaded script, with its page array switched over; null when the
        /// script is not loaded or was loaded without the extra chunks.
        /// </summary>
        public static Space Get(string script)
        {
            if (!MainWindow.m.IsProcOpen)
                return null;
            // Several threads ask; two setting up the same script at once could clear the data
            // page after the first one already handed it out.
            lock (Gate)
                return GetLocked(script);
        }

        /// <summary>Whether this edition gives the script extra pages (it may still be loaded without them).</summary>
        public static bool Configured(string script)
        {
            Load();
            return reservations.Any(x => x.Script == script);
        }

        private static Space GetLocked(string script)
        {
            Load();
            var r = reservations.FirstOrDefault(x => x.Script == script);
            if (r == null)
                return null;
            long prog = (long)ScrProgramScanner.GetScrProgramByName(script);
            if (prog == 0)
                return null;
            try
            {
                // The page map lists the chunks in layout order; a script loaded with other flags
                // (cached from before, or not raised in time) has no extra space.
                long map = Read<long>(prog + ProgramPageMap);
                List<long> layout = Layout(r.Target);
                if (Read<int>(map + PageMapCount) != layout.Count)
                    return null;
                byte[] entries = MainWindow.m.memory((map + PageMapEntries).ToString("X")).GetBytes(layout.Count * 8);

                // Every 16 KB slot behind the shipped data that lies inside one chunk.
                long shippedEnd = Layout(r.Shipped).Sum();
                var slots = new List<long>();
                long start = 0;
                for (int i = 0; i < layout.Count; i++)
                {
                    long e = BitConverter.ToInt64(entries, i * 8);
                    if ((0x2000L << (int)(e & 0xFFF)) != layout[i])
                        return null;
                    long end = start + layout[i];
                    for (long o = Math.Max(start, (shippedEnd + PageSize - 1) & ~(long)(PageSize - 1)); o + PageSize <= end; o += PageSize)
                        slots.Add((e & ~0xFFFL) + (o - start));
                    start = end;
                }

                int pages = (Read<int>(prog + ProgramCodeSize) + PageSize - 1) / PageSize;
                // The first slot holds the page arrays, the last one is the data page, every slot
                // between them is a code page.
                if (slots.Count < 3 || (pages + slots.Count - 2) * 8 > StringArray)
                    return null;
                var space = new Space { Base = pages * PageSize, Data = slots[slots.Count - 1] };
                long array = slots[0];
                space.Pages.AddRange(slots.Skip(1).Take(slots.Count - 2));

                // Strings first: the payload that uses the data page is only written once Get
                // returns, so it never runs with the script's own string array.
                long strings = Read<long>(prog + ProgramStrings);
                if (strings != array + StringArray)
                {
                    int stringPages = (Read<int>(prog + ProgramStringsSize) + PageSize - 1) / PageSize;
                    if (stringPages >= DataStringPage)
                        return null;
                    MainWindow.m.memory(space.Data.ToString("X")).SetBytes(new byte[PageSize]);
                    // The script's own string pages, then the data page for every index up to ours.
                    var table = new byte[(DataStringPage + 1) * 8];
                    Array.Copy(MainWindow.m.memory(strings.ToString("X")).GetBytes(stringPages * 8), table, stringPages * 8);
                    for (int i = stringPages; i <= DataStringPage; i++)
                        Array.Copy(BitConverter.GetBytes(space.Data), 0, table, i * 8, 8);
                    MainWindow.m.memory((array + StringArray).ToString("X")).SetBytes(table);
                    MainWindow.m.memory((prog + ProgramStrings).ToString("X")).SetBytes(BitConverter.GetBytes(array + StringArray));
                    Log.Debug($"{script}: data page at string page {DataStringPage}", source: "ScriptSpace");
                }

                long blocks = Read<long>(prog + ProgramCodeBlocks);
                if (blocks != array)
                {
                    // The script's own page pointers plus ours; the program is switched over in one
                    // 8 byte write, so the VM never reads half an array.
                    var table = new byte[(pages + space.Pages.Count) * 8];
                    Array.Copy(MainWindow.m.memory(blocks.ToString("X")).GetBytes(pages * 8), table, pages * 8);
                    for (int i = 0; i < space.Pages.Count; i++)
                        Array.Copy(BitConverter.GetBytes(space.Pages[i]), 0, table, (pages + i) * 8, 8);
                    MainWindow.m.memory(array.ToString("X")).SetBytes(table);
                    MainWindow.m.memory((prog + ProgramCodeBlocks).ToString("X")).SetBytes(BitConverter.GetBytes(array));
                    Log.Debug($"{script}: {space.Pages.Count} extra page(s) from pc 0x{space.Base:X}", source: "ScriptSpace");
                }
                return space;
            }
            catch (Exception ex)
            {
                Log.Debug("Space " + script + ": " + ex.Message, source: "ScriptSpace");
                return null;
            }
        }

        // Chunk sizes the flags describe, in layout order: base << 8 down to base << 0, with
        // base = 0x200 << (flags & 0xF) and the count of each bucket in its own bit field.
        private static List<long> Layout(uint flags)
        {
            long chunkBase = 0x200L << (int)(flags & 0xF);
            int[] shift = { 4, 5, 7, 11, 17, 24, 25, 26, 27 };
            int[] bits = { 1, 2, 4, 6, 7, 1, 1, 1, 1 };
            var sizes = new List<long>();
            for (int b = 0; b < shift.Length; b++)
            {
                int count = (int)((flags >> shift[b]) & ((1u << bits[b]) - 1));
                for (int i = 0; i < count; i++)
                    sizes.Add(chunkBase << (8 - b));
            }
            return sizes;
        }

        // Why the game cannot load the shipped data with the target flags; null when it can.
        private static string CheckLayout(uint shipped, uint target)
        {
            List<long> before = Layout(shipped), after = Layout(target);
            if ((shipped & 0xF0000000) != (target & 0xF0000000))
                return "different resource version bits";
            if (after.Count > before.Count)
                return $"{after.Count} chunks, the page map has room for {before.Count}";
            long shippedEnd = before.Sum();
            if (after.Sum() - after[after.Count - 1] >= shippedEnd)
                return "a chunk lies wholly behind the shipped data";
            if (after.Sum() < shippedEnd + 3 * PageSize)
                return "less than three extra pages";
            var borders = new HashSet<long>();
            long offset = 0;
            foreach (long size in before)
                borders.Add(offset += size);
            offset = 0;
            foreach (long size in after)
            {
                offset += size;
                if (offset < shippedEnd && !borders.Contains(offset))
                    return $"chunk border 0x{offset:X} cuts through a shipped chunk";
            }
            return null;
        }

        // The virtual flags of a file in script_rel.rpf. Its table of contents is the RPF7 one:
        // 16 byte entries (u16 name offset first, virtual flags at +8), the names behind them.
        private static long FindEntry(string file)
        {
            if (scriptPack == 0)
            {
                // The game masks the table index to 11 bits.
                for (int i = 0; i < 0x800 && scriptPack == 0; i++)
                {
                    long pack = Read<long>(PackfileTable + i * 8);
                    if (pack != 0 && ReadString(pack + GTA.Offsets.Editor.OFFSET_packfile_name, 32) == "script_rel.rpf")
                        scriptPack = pack;
                }
                if (scriptPack == 0)
                    return 0;
            }
            long names = Read<long>(scriptPack + GTA.Offsets.Editor.OFFSET_packfile_names);
            long entries = Read<long>(scriptPack + GTA.Offsets.Editor.OFFSET_packfile_entries);
            int count = Read<int>(scriptPack + GTA.Offsets.Editor.OFFSET_packfile_count);
            if (count <= 0 || count > 0x10000)
                return 0;
            byte[] table = MainWindow.m.memory(entries.ToString("X")).GetBytes(count * 16);
            for (int i = 0; i < count; i++)
            {
                int nameOffset = BitConverter.ToUInt16(table, i * 16);
                if (ReadString(names + nameOffset, file.Length + 1) == file)
                    return entries + i * 16 + 8;
            }
            return 0;
        }

        // Heap addresses lie below the module, so they go to mry as hex strings (a long below the
        // module base counts as module relative).
        private static T Read<T>(long address) where T : struct => MainWindow.m.memory(address.ToString("X")).Get<T>();

        private static string ReadString(long address, int max)
        {
            byte[] b = MainWindow.m.memory(address.ToString("X")).GetBytes(max);
            int end = Array.IndexOf(b, (byte)0);
            return Encoding.ASCII.GetString(b, 0, end < 0 ? b.Length : end);
        }
    }
}
