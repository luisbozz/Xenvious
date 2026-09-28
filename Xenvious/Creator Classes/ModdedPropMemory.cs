using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Prop menu changes kept for every creator. The game loads a creator's script only while
    /// that creator is open, and loads it fresh from disk each time, so a change written into
    /// one creator's table is gone when another creator (or the same one again) is entered.
    /// With this on, each change is kept in config.ini and written into every creator script as
    /// soon as it is loaded (ScrPatchesRunner calls Tick).
    ///
    /// The tables differ between creators (other categories, other lengths), so a change is found
    /// again by its category and the model it replaced, never by a byte offset.
    /// </summary>
    public static class ModdedPropMemory
    {
        public sealed class Change
        {
            public string Script;   // creator it was made in
            public int Table;       // category index in the table
            public int Slot;        // index in that category
            public int? Original;   // model the slot had in that creator; null: unknown
            public int Hash;        // model written instead
        }

        private const string ConfigKey = "mpremember", EnabledKey = "mprememberon";
        private static readonly object Sync = new object();
        private static List<Change> _changes;
        private static bool? _enabled;
        // Script program each creator had when its changes were last written; a new one is a fresh load.
        private static readonly Dictionary<string, ulong> Seen = new Dictionary<string, ulong>();
        // Set by Record, handled by the next Tick: a whole imported table is one save and one pass.
        private static bool _dirty;
        private static string _recordedIn;

        /// <summary>Raised (on the patcher thread) after changes were written into a creator script.</summary>
        public static event Action<string> Applied;

        public static bool Enabled
        {
            get
            {
                if (_enabled == null)
                    _enabled = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString("Settings", EnabledKey, "1") != "0";
                return _enabled.Value;
            }
            set
            {
                _enabled = value;
                new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", EnabledKey, value ? "1" : "0");
            }
        }

        public static int Count
        {
            get { lock (Sync) return Changes.Count; }
        }

        private static List<Change> Changes
        {
            get
            {
                if (_changes != null)
                    return _changes;
                _changes = new List<Change>();
                try
                {
                    string stored = ConfigText.Decode(new ini_reader(Functions.getRoamingConfigFilePath()).ReadString("Settings", ConfigKey, ""));
                    if (!string.IsNullOrWhiteSpace(stored))
                        _changes = JsonConvert.DeserializeObject<List<Change>>(stored) ?? new List<Change>();
                }
                catch (Exception ex)
                {
                    Log.Error("Reading the remembered prop changes failed", ex, source: "mprops");
                }
                return _changes;
            }
        }

        private static void Save()
            => new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", ConfigKey, ConfigText.Encode(JsonConvert.SerializeObject(Changes)));

        /// <summary>
        /// Keeps a slot written in <paramref name="script"/>; the next Tick saves it and writes it
        /// into the other creators loaded right now. Putting the original back forgets the slot.
        /// </summary>
        public static void Record(string script, int table, int slot, int? original, int hash)
        {
            if (!Enabled)
                return;
            lock (Sync)
            {
                var old = Changes.FirstOrDefault(c => c.Table == table && (c.Slot == slot && c.Script == script || original.HasValue && c.Original == original));
                if (old != null)
                {
                    // The first creator's original stays: it is what the slot is found by.
                    Changes.Remove(old);
                    original = old.Original ?? original;
                    script = old.Original.HasValue ? old.Script : script;
                    slot = old.Original.HasValue ? old.Slot : slot;
                }
                if (!original.HasValue || original.Value != hash)
                    Changes.Add(new Change { Script = script, Table = table, Slot = slot, Original = original, Hash = hash });
                _dirty = true;
                _recordedIn = script;
            }
        }

        public static void Clear()
        {
            lock (Sync)
            {
                Changes.Clear();
                _dirty = false;
                Save();
            }
        }

        /// <summary>Writes the changes into every creator script loaded since the last call.</summary>
        public static void Tick()
        {
            if (!MainWindow.m.IsProcOpen)
                return;
            string recordedIn = null;
            lock (Sync)
            {
                if (_dirty)
                {
                    Save();
                    _dirty = false;
                    recordedIn = _recordedIn;
                }
            }
            foreach (var source in GTA.Editor.ModdedPropSources)
            {
                ulong pointer = ScrProgramScanner.GetScrProgramByName(source.ScriptName);
                bool fresh;
                lock (Sync)
                {
                    fresh = !Seen.TryGetValue(source.ScriptName, out var known) || known != pointer;
                    Seen[source.ScriptName] = pointer;
                }
                // A creator just loaded, or another one than where the user changed something.
                bool other = recordedIn != null && source.ScriptName != recordedIn;
                if (pointer != 0 && (fresh || other) && Enabled && Count > 0)
                    ApplyTo(source.ScriptName, pointer);
            }
        }

        private static void ApplyTo(string script, ulong pointer)
        {
            ulong region = ModdedPropTable.Locate(pointer);
            if (region == 0)
                return;
            byte[] buffer;
            try
            {
                buffer = MainWindow.m.memory(region.ToString("X")).GetBytes(ModdedPropTable.ReadLength);
            }
            catch
            {
                return;
            }
            var table = ModdedPropTable.Parse(buffer);
            int written = 0;
            List<Change> changes;
            lock (Sync)
                changes = Changes.ToList();
            foreach (var change in changes)
            {
                if (change.Table >= table.Count)
                    continue;
                var category = table[change.Table];
                int target = -1;
                if (change.Original.HasValue)
                {
                    if (change.Slot < category.Count && (category[change.Slot].Hash == change.Original.Value || category[change.Slot].Hash == change.Hash))
                        target = change.Slot;
                    else
                        target = category.FindIndex(e => e.Hash == change.Original.Value);
                }
                // Without the original the slot is only certain in the creator it was made in.
                else if (change.Script == script && change.Slot < category.Count)
                    target = change.Slot;
                if (target < 0 || category[target].Hash == change.Hash)
                    continue;
                MainWindow.m.memory((region + (ulong)category[target].Offset).ToString("X")).SetInt(change.Hash);
                written++;
            }
            if (written > 0)
            {
                Log.Debug($"Wrote {written} remembered prop change(s) into {script}", source: "mprops");
                Applied?.Invoke(script);
            }
        }
    }
}
