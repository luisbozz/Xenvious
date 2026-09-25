using System;
using System.Collections.Generic;
using System.Globalization;

namespace Xenvious
{
    /// <summary>
    /// Read-only ini parsed once from a string.
    ///
    /// offsets.ini used to be written to a temp file and read with one
    /// GetPrivateProfileString call per key; each call opens and scans the whole file,
    /// which for ~1100 keys cost about 0.4 s per start. This parses once into a
    /// dictionary and keeps the Win32 semantics the loader relies on: the first
    /// occurrence of a key wins, section and key names are case-insensitive, a value
    /// wrapped in matching quotes loses them, and an integer may be written as 0x...
    /// </summary>
    public sealed class IniText
    {
        private readonly Dictionary<string, string> values =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IniText(string content)
        {
            string section = "";
            foreach (string raw in (content ?? "").Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == ';')
                    continue;
                if (line[0] == '[')
                {
                    int end = line.IndexOf(']');
                    section = end > 0 ? line.Substring(1, end - 1).Trim() : line.Substring(1).Trim();
                    continue;
                }
                int eq = line.IndexOf('=');
                if (eq <= 0)
                    continue;
                string key = section + "\u0001" + line.Substring(0, eq).Trim();
                if (!values.ContainsKey(key))
                    values[key] = Unquote(line.Substring(eq + 1).Trim());
            }
        }

        private static string Unquote(string v)
        {
            if (v.Length >= 2 && (v[0] == '"' || v[0] == '\'') && v[v.Length - 1] == v[0])
                return v.Substring(1, v.Length - 2);
            return v;
        }

        public string ReadString(string section, string key, string defVal)
        {
            string v;
            return values.TryGetValue(section + "\u0001" + key, out v) ? v : defVal;
        }

        public string ReadString(string section, string key)
        {
            return ReadString(section, key, "");
        }

        public int ReadInteger(string section, string key, int defVal)
        {
            string v = ReadString(section, key, null);
            if (string.IsNullOrWhiteSpace(v))
                return defVal;
            v = v.Trim();
            int r;
            // Sign before the prefix is allowed: OFFSET_cam_zoom = -0x24.
            bool neg = v.StartsWith("-");
            string h = neg || v.StartsWith("+") ? v.Substring(1) : v;
            if (h.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(h.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r))
                    return defVal;
                return neg ? -r : r;
            }
            // GetPrivateProfileInt reads the leading integer and ignores the rest.
            int i = 0;
            if (i < v.Length && (v[i] == '-' || v[i] == '+')) i++;
            while (i < v.Length && char.IsDigit(v[i])) i++;
            return int.TryParse(v.Substring(0, i), NumberStyles.Integer, CultureInfo.InvariantCulture, out r) ? r : defVal;
        }

        public int ReadInteger(string section, string key)
        {
            return ReadInteger(section, key, 0);
        }
    }
}
