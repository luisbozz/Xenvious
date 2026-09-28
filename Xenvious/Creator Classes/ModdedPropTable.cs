using System;
using System.Collections.Generic;

namespace Xenvious
{
    /// <summary>
    /// The prop menu table in a creator script's bytecode: a run of "PUSH_CONST_U32 &lt;hash&gt;;
    /// LEAVE 2,1" entries, one function per category. Finds the table and splits it into its
    /// categories; each creator has its own shape, so offsets never carry over between creators.
    /// </summary>
    public static class ModdedPropTable
    {
        public static readonly byte[] Placeholder = { 0x2E, 0x02, 0x01, 0x28 };

        // Checked against all five creators on both builds: the first run of 3 or more
        // placeholders 7-8 bytes apart is the table start in every case, and still is at 8.
        public const int RunLength = 4;

        // How much of the script is read from the table start; the table ends well before.
        public const int ReadLength = 50000;

        public static bool PlaceholderAt(byte[] buffer, int at)
        {
            if (at < 0 || at + Placeholder.Length > buffer.Length)
                return false;
            for (int k = 0; k < Placeholder.Length; k++)
                if (buffer[at + k] != Placeholder[k])
                    return false;
            return true;
        }

        public static int IndexOfPlaceholderRun(byte[] buffer, int runLength)
        {
            for (int i = IndexOfPattern(buffer, Placeholder); i >= 0; i = IndexOfPattern(buffer, Placeholder, i + 1))
            {
                // Entries are normally 8 bytes apart, occasionally 7.
                int n = 1, at = i;
                while (n < runLength)
                {
                    if (PlaceholderAt(buffer, at + 8)) at += 8;
                    else if (PlaceholderAt(buffer, at + 7)) at += 7;
                    else break;
                    n++;
                }
                if (n >= runLength)
                    return i;
            }
            return -1;
        }

        public static int IndexOfPattern(byte[] buffer, byte[] pattern, int startIndex = 0)
        {
            if (buffer == null || pattern == null || pattern.Length == 0 || buffer.Length < pattern.Length || startIndex < 0)
                return -1;
            for (int i = startIndex; i <= buffer.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }
            return -1;
        }

        /// <summary>The table start in a loaded script program, or 0.</summary>
        public static ulong Locate(ulong scriptPointer)
        {
            var codePages = ScrProgramScanner.GetScrProgramByteCodeRegion(scriptPointer);
            if (codePages == null)
                return 0;
            foreach (var (pagePtr, pageSize) in codePages)
            {
                byte[] pageBytes;
                try
                {
                    pageBytes = MainWindow.m.memory(pagePtr.ToString("X")).GetBytes(pageSize);
                }
                catch
                {
                    continue;
                }

                // The placeholder alone also occurs in ordinary code -- in fm_race_creator
                // eleven times before the table, which put the region 165 KB too early and
                // filled the list with CALL operands. Those stray matches stand alone; the
                // table is the first place where they follow each other. That is structure,
                // not a value, so it still holds after the props were edited.
                int idx = IndexOfPlaceholderRun(pageBytes, RunLength);
                if (idx < 0)
                    continue;
                if (idx < 4 && pagePtr < (ulong)(4 - idx))
                    continue;
                return idx >= 4 ? pagePtr + (ulong)(idx - 4) : pagePtr - (ulong)(4 - idx);
            }
            return 0;
        }

        /// <summary>
        /// The categories of a table read from its start: per entry the offset of its hash from
        /// the start, and the hash. A gap of more than 40 bytes starts the next category.
        /// </summary>
        public static List<List<(int Offset, int Hash)>> Parse(byte[] buffer)
        {
            var categories = new List<List<(int, int)>>();
            var current = new List<(int, int)>();
            int previous = -Placeholder.Length;
            for (int offset = IndexOfPattern(buffer, Placeholder); offset >= 0; offset = IndexOfPattern(buffer, Placeholder, offset + Placeholder.Length))
            {
                if (offset < 4)
                    continue;
                if (offset - previous > 40 && current.Count > 0)
                {
                    categories.Add(current);
                    current = new List<(int, int)>();
                }
                previous = offset;
                current.Add((offset - 4, BitConverter.ToInt32(buffer, offset - 4)));
            }
            if (current.Count > 0)
                categories.Add(current);
            return categories;
        }
    }
}
