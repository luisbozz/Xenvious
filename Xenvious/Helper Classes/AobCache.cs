using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Xenvious
{
    /// <summary>
    /// Where each AOB pattern hit, remembered.
    ///
    /// A pattern scan reads the whole game module (~91 MB) and searches it. Without a
    /// buffer, every call did that again: GetJobImageBytes() every 3 s through
    /// getIMGPointer(), and getLocalScriptAddy() for every local freeze lookup. But a
    /// hit only moves when the game binary changes, so:
    ///
    ///  - per process, each pattern is scanned once and the hit reused;
    ///  - across starts, the hit's offset from the module base is stored in the roaming
    ///    config, keyed by the binary (variant, file size, write time, image size) and
    ///    the pattern text. A stored offset is used only after the bytes there are
    ///    checked against the pattern again, so a stale entry costs a rescan, never a
    ///    wrong pointer.
    /// </summary>
    public static class AobCache
    {
        private const string Section = "AOBCACHE";
        private static readonly ConcurrentDictionary<string, ulong> memo = new ConcurrentDictionary<string, ulong>();
        private static string identityFor;      // module base the identity below belongs to
        private static string identity;

        private static string Identity(ProcessModule mod)
        {
            string key = mod.BaseAddress.ToInt64().ToString("X");
            if (identityFor == key && identity != null)
                return identity;
            string file = "";
            try
            {
                var fi = new FileInfo(mod.FileName);
                file = fi.Length + "|" + fi.LastWriteTimeUtc.Ticks;
            }
            catch { }
            identity = GameVariant.Current + "|" + file + "|" + mod.ModuleMemorySize;
            identityFor = key;
            return identity;
        }

        private static string StoreKey(ProcessModule mod, string pattern)
        {
            using (var sha = SHA1.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(Identity(mod) + "\n" + pattern));
                return string.Concat(h.Take(8).Select(b => b.ToString("x2")));
            }
        }

        /// <summary>Do the bytes at <paramref name="address"/> still match the pattern?</summary>
        public static bool Matches(ulong address, string pattern)
        {
            string[] tok = pattern.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] bytes;
            try { bytes = MainWindow.m.memory(address.ToString("X")).GetBytes(tok.Length); }
            catch { return false; }
            if (bytes == null || bytes.Length < tok.Length)
                return false;
            for (int i = 0; i < tok.Length; i++)
            {
                if (tok[i] == "?" || tok[i] == "??")
                    continue;
                if (bytes[i] != Convert.ToByte(tok[i], 16))
                    return false;
            }
            return true;
        }

        /// <summary>Hit address from this process or from the stored cache, verified.</summary>
        public static bool TryGet(string pattern, out ulong hit)
        {
            hit = 0;
            ProcessModule mod = MainWindow.m.getMainModule();
            if (mod == null)
                return false;
            long baseAddr = mod.BaseAddress.ToInt64();
            string mkey = baseAddr.ToString("X") + "|" + pattern;
            if (memo.TryGetValue(mkey, out hit))
                return true;

            try
            {
                string stored = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString(Section, StoreKey(mod, pattern));
                long off;
                if (!string.IsNullOrEmpty(stored) && long.TryParse(stored, System.Globalization.NumberStyles.HexNumber, null, out off))
                {
                    ulong candidate = (ulong)(baseAddr + off);
                    if (Matches(candidate, pattern))
                    {
                        memo[mkey] = candidate;
                        hit = candidate;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// The stored hit without checking the bytes there: for a patch site whose bytes a patch
        /// changed, so the pattern no longer matches. The caller checks what it finds.
        /// </summary>
        public static bool TryGetStored(string pattern, out ulong hit)
        {
            hit = 0;
            ProcessModule mod = MainWindow.m.getMainModule();
            if (mod == null)
                return false;
            try
            {
                string stored = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString(Section, StoreKey(mod, pattern));
                if (!long.TryParse(stored, System.Globalization.NumberStyles.HexNumber, null, out long off))
                    return false;
                hit = (ulong)(mod.BaseAddress.ToInt64() + off);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Remembers a patch site and its original bytes for this game binary.</summary>
        public static void PutOriginal(string pattern, ulong address, byte[] original)
        {
            ProcessModule mod = MainWindow.m.getMainModule();
            if (mod == null || address == 0 || original == null)
                return;
            try
            {
                string value = ((long)address - mod.BaseAddress.ToInt64()).ToString("X") + "|" + BitConverter.ToString(original).Replace("-", "");
                new ini_reader(Functions.getRoamingConfigFilePath()).Write(Section, StoreKey(mod, "original\n" + pattern), value);
            }
            catch { }
        }

        /// <summary>A patch site and its original bytes stored by <see cref="PutOriginal"/>.</summary>
        public static bool TryGetOriginal(string pattern, out ulong address, out byte[] original)
        {
            address = 0;
            original = null;
            ProcessModule mod = MainWindow.m.getMainModule();
            if (mod == null)
                return false;
            try
            {
                string[] parts = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString(Section, StoreKey(mod, "original\n" + pattern)).Split('|');
                if (parts.Length != 2 || parts[1].Length == 0 || parts[1].Length % 2 != 0
                    || !long.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out long off))
                    return false;
                original = Enumerable.Range(0, parts[1].Length / 2).Select(i => Convert.ToByte(parts[1].Substring(i * 2, 2), 16)).ToArray();
                address = (ulong)(mod.BaseAddress.ToInt64() + off);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Put(string pattern, ulong hit)
        {
            if (hit == 0)
                return;
            ProcessModule mod = MainWindow.m.getMainModule();
            if (mod == null)
                return;
            long baseAddr = mod.BaseAddress.ToInt64();
            memo[baseAddr.ToString("X") + "|" + pattern] = hit;
            try
            {
                new ini_reader(Functions.getRoamingConfigFilePath())
                    .Write(Section, StoreKey(mod, pattern), ((long)hit - baseAddr).ToString("X"));
            }
            catch { }
        }
    }
}
