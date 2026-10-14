using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Xenvious
{
    /// <summary>
    /// Reads a decompiler path ("Global_4980736.f_71084[i /*631*/].f_14") into the index it names,
    /// with the decompiler's meaning: the parts after the dots add up, and every [..] skips the slot
    /// in front of element 0 that holds the array size. A number as index ([2 /*11*/]) adds its
    /// elements too (2 * 11; without the stride comment an element is one slot); a name (i, j, team)
    /// is the caller's to add at runtime. Same rules as tools/builddata/builddata.py, which turns
    /// offsets.ini into Generated/*.g.cs; here for paths typed into the Global/Local editor.
    /// </summary>
    public static class GlobalPath
    {
        private static readonly Regex Part = new Regex(@"^(?:(?:Global|[a-z]?Local)_)?(?:f_)?(-?\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex Index = new Regex(@"\G\[\s*([^\]/\s]+)\s*(?:/\*\s*(\d+)\s*\*/)?\s*\]",
            RegexOptions.CultureInvariant);

        /// <summary>The index the path names; false when it is not a path.</summary>
        public static bool TryEvaluate(string path, out long index)
        {
            index = 0;
            string p = (path ?? "").Trim();
            if (p.Length == 0)
                return false;
            int depth = 0, start = 0;
            for (int i = 0; i <= p.Length; i++)
            {
                if (i < p.Length && p[i] == '[') depth++;
                else if (i < p.Length && p[i] == ']') depth--;
                else if (i == p.Length || (p[i] == '.' && depth == 0))
                {
                    if (!AddPart(p.Substring(start, i - start), ref index))
                        return false;
                    start = i + 1;
                }
            }
            return true;
        }

        /// <summary>The index the path names, 0 when it is not a path.</summary>
        public static long Evaluate(string path) => TryEvaluate(path, out long index) ? index : 0;

        private static bool AddPart(string part, ref long index)
        {
            int bracket = part.IndexOf('[');
            string head = (bracket < 0 ? part : part.Substring(0, bracket)).Trim();
            Match m = Part.Match(head);
            if (!m.Success)
                return false;
            index += long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            if (bracket < 0)
                return true;
            for (int pos = bracket; pos < part.Length;)
            {
                Match ix = Index.Match(part, pos);
                if (!ix.Success)
                    return false;
                index += 1;
                if (long.TryParse(ix.Groups[1].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long n))
                {
                    long stride = ix.Groups[2].Success ? long.Parse(ix.Groups[2].Value, CultureInfo.InvariantCulture) : 1;
                    index += n * stride;
                }
                pos = ix.Index + ix.Length;
            }
            return true;
        }
    }
}
