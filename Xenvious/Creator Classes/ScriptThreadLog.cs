using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Xenvious
{
    /// <summary>
    /// Diagnosis for the game state detection: while %AppData%\Xenvious\threadlog.on exists, logs
    /// every slot of the script thread list once a second to logs\threads-yyyy-MM-dd.log, first in
    /// full and then only the slots that changed. Each slot shows the raw dwords 0x08-0x1C of the
    /// thread (id, script hash, thread state; the layout differs between Legacy and Enhanced) and,
    /// on Legacy, the script name. The names of Enhanced hashes are resolved afterwards.
    /// </summary>
    public static class ScriptThreadLog
    {
        private static readonly string Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious");
        private static readonly string FlagFile = Path.Combine(Root, "threadlog.on");
        private static Dictionary<int, string> _last;
        private static string _lastContext;

        public static bool Enabled => File.Exists(FlagFile);

        /// <summary>Called from the dashboard timer; <paramref name="context"/> is what Xenvious detects right now.</summary>
        public static void Tick(string context)
        {
            if (!Enabled)
            {
                _last = null;
                return;
            }
            var now = Snapshot();
            var sb = new StringBuilder();
            string time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            if (context != _lastContext)
                sb.AppendLine($"{time} context: {context}");
            if (_last == null)
            {
                sb.AppendLine($"{time} full snapshot, {now.Count} slots (slot: dwords 0x08 0x0C 0x10 0x14 0x18 0x1C | name)");
                foreach (var slot in now.OrderBy(p => p.Key))
                    sb.AppendLine($"  {slot.Key,3}: {slot.Value}");
            }
            else
            {
                foreach (int key in now.Keys.Union(_last.Keys).OrderBy(k => k))
                {
                    now.TryGetValue(key, out string a);
                    _last.TryGetValue(key, out string b);
                    if (a != b)
                        sb.AppendLine($"{time} {key,3}: {b ?? "-"}  =>  {a ?? "-"}");
                }
            }
            _last = now;
            _lastContext = context;
            if (sb.Length == 0)
                return;
            try
            {
                string folder = Path.Combine(Root, "logs");
                Directory.CreateDirectory(folder);
                File.AppendAllText(Path.Combine(folder, $"threads-{DateTime.Now:yyyy-MM-dd}.log"), sb.ToString());
            }
            catch (IOException)
            {
                // Next tick writes the difference again.
                _last = null;
            }
        }

        private static Dictionary<int, string> Snapshot()
        {
            var slots = new Dictionary<int, string>();
            if (MainWindow.m == null || !MainWindow.m.IsProcOpen)
                return slots;
            long list = GTA.getLocalPointer().ToInt64();
            if (list == 0)
                return slots;
            for (int d = 0; d < 0x800; d += 0x8)
            {
                try
                {
                    var words = new uint[6];
                    for (int i = 0; i < words.Length; i++)
                        words[i] = MainWindow.m.memory(list, new long[] { d, 0x08 + i * 4 }).Get<uint>();
                    if (words.All(w => w == 0))
                        continue;
                    string name = "";
                    if (GTA.Offsets.Editor.OFFSET_script_hash == 0 && GTA.Offsets.Editor.OFFSET_script_name != 0)
                    {
                        try { name = MainWindow.m.memory(list, new long[] { d, GTA.Offsets.Editor.OFFSET_script_name }).GetString(); } catch { }
                    }
                    slots[d / 8] = string.Join(" ", words.Select(w => w.ToString("X8", CultureInfo.InvariantCulture))) + (name.Length > 0 ? " | " + name : "");
                }
                catch
                {
                    // Empty slot: no thread behind the pointer.
                }
            }
            return slots;
        }
    }
}
