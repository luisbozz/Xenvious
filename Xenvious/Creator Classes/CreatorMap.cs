using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Xenvious
{
    /// <summary>
    /// Saving and restoring what is placed in a creator job (props, dynamic props,
    /// checkpoints, templates), and making the creator show changed data.
    ///
    /// Rebuild: the creator's main state (worker.f_565) is 3 while editing. Setting it
    /// to 7 runs the "back from testing" path, which deletes every placed entity
    /// *without* touching the job data and builds checkpoints, props, dynamic props,
    /// vehicles and markers again from the globals, then returns to 3. Anything that
    /// writes job data (restore, copy, moving props between lists) uses it to become
    /// visible. Race, LTS and capture share that numbering; deathmatch and survival
    /// do not, so they are left out until checked.
    ///
    /// That path also sends the menu back to the creator's main menu (func_71) and
    /// sets the camera anchor to (0, 0, 10), which makes the camera jump to the player
    /// looking straight down -- right after testing that is wanted, after a rebuild it
    /// is not. For the one frame the rebuild takes, both statements are replaced with
    /// NOPs in the creator's bytecode and put back afterwards.
    ///
    /// Snapshots are the raw global arrays. That keeps every field (a JSON round trip
    /// would only keep the fields Xenvious knows), but ties a file to the game edition
    /// and build it was taken on; restoring refuses anything else.
    /// </summary>
    public static class CreatorMap
    {
        public const string FileExtension = ".xvmap";
        public const string FileFilter = "Xenvious map (*.xvmap)|*.xvmap";

        internal const int StateEditing = 3;
        private const int StateRebuild = 7;

        public static string MapFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "maps");

        /// <summary>One array of job data: its count global and the array itself.</summary>
        private sealed class Section
        {
            public string Name;
            public long CountGlobal;
            public long FirstElement;   // element 0; the array header (capacity) sits right before it
            public long Stride;
            public Func<string, bool> AppliesTo;
        }

        private static IEnumerable<Section> Sections()
        {
            yield return new Section
            {
                Name = "props", CountGlobal = GTA.Offsets.Editor.Props.number,
                FirstElement = GTA.Offsets.Editor.Props.loc, Stride = GTA.Offsets.Editor.Props.NEXT,
                AppliesTo = c => true
            };
            yield return new Section
            {
                Name = "dprops", CountGlobal = GTA.Offsets.Editor.DProps.number,
                FirstElement = GTA.Offsets.Editor.DProps.loc, Stride = GTA.Offsets.Editor.DProps.NEXT,
                AppliesTo = c => true
            };
            yield return new Section
            {
                Name = "checkpoints", CountGlobal = GTA.Offsets.Editor.Race.Checkpoints.number,
                FirstElement = GTA.Offsets.Editor.Race.Checkpoints.locx, Stride = GTA.Offsets.Editor.Race.Checkpoints.NEXT,
                AppliesTo = c => c == "fm_race_creator"
            };
            // PTemp.pto is the position list inside element 0, one header further in.
            yield return new Section
            {
                Name = "templates", CountGlobal = GTA.Offsets.Editor.PTemp.number,
                FirstElement = GTA.Offsets.Editor.PTemp.pto - 1, Stride = GTA.Offsets.Editor.PTemp.NEXT,
                AppliesTo = c => true
            };
        }

        public sealed class SectionData
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("count")] public int Count { get; set; }
            [JsonProperty("start")] public long Start { get; set; }
            [JsonProperty("slots")] public int Slots { get; set; }
            /// <summary>The raw global slots (8 bytes each), gzip, base64.</summary>
            [JsonProperty("data")] public string Data { get; set; }
        }

        public sealed class Snapshot
        {
            [JsonProperty("format")] public string Format { get; set; } = "xenvious-map";
            /// <summary>
            /// 2 since the dynamic prop count is read from the right global; version 1
            /// files saved an unrelated value (32) as that count.
            /// </summary>
            [JsonProperty("version")] public int Version { get; set; } = 2;
            [JsonProperty("edition")] public string Edition { get; set; }
            [JsonProperty("build")] public string Build { get; set; }
            [JsonProperty("creator")] public string Creator { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("savedAt")] public DateTime SavedAt { get; set; }
            [JsonProperty("sections")] public List<SectionData> Sections { get; set; } = new List<SectionData>();

            public int CountOf(string section) => Sections.FirstOrDefault(s => s.Name == section)?.Count ?? 0;
        }

        // ---- rebuild -----------------------------------------------------------------

        internal static long WorkerOffset(string creator)
        {
            switch (creator)
            {
                case "fm_race_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_race;
                case "fm_lts_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_lts;
                case "fm_capture_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_capture;
                default: return 0;   // deathmatch / survival: other state numbering
            }
        }

        public static string CurrentCreator()
        {
            string creator = GTA.CurrentCreatorName();
            if (string.IsNullOrEmpty(creator))
            {
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
                creator = GTA.CurrentCreatorName();
            }
            return creator ?? "";
        }

        public static bool CanRebuild(string creator) =>
            WorkerOffset(creator) != 0 && GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh != 0;

        /// <summary>
        /// Makes the creator rebuild every placed entity from the job data. Returns once
        /// the creator is back in its editing state, or false after a few seconds.
        /// </summary>
        public static async Task<bool> RebuildAsync()
        {
            string creator = CurrentCreator();
            if (!CanRebuild(creator))
                return false;
            long state = WorkerOffset(creator) + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh;
            if (ReadLocal(state) != StateEditing)
                return false;   // testing, a menu transition, ... -- not ours to interrupt

            var kept = KeepMenuAndCamera(creator);
            try
            {
                WriteLocal(state, StateRebuild);
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(50).ConfigureAwait(true);
                    if (ReadLocal(state) == StateEditing)
                        return true;
                }
                return false;
            }
            finally
            {
                // The state-7 code has run by now (or never will); a frame more to be safe.
                await Task.Delay(60).ConfigureAwait(true);
                foreach (var (address, original) in kept)
                    MainWindow.m.memory(address.ToString("X")).SetBytes(original);
            }
        }

        private static long PreOffset(string creator)
        {
            switch (creator)
            {
                case "fm_race_creator": return GTA.Offsets.Editor.OFFSET_current_creator_pre_race;
                case "fm_lts_creator": return GTA.Offsets.Editor.OFFSET_current_creator_pre_lts;
                case "fm_capture_creator": return GTA.Offsets.Editor.OFFSET_current_creator_pre_capture;
                default: return 0;
            }
        }

        /// <summary>
        /// NOPs "func_71(&pre, 0)" (menu to main menu) and "camera.f_9 = {0, 0, 10}" in the
        /// state-7 path of the running creator. Returns what to put back. Empty when the
        /// code is not found (another build or opcode table): the rebuild still works, it
        /// just resets menu and camera like after a test.
        /// </summary>
        private static List<(ulong Address, byte[] Original)> KeepMenuAndCamera(string creator)
        {
            var kept = new List<(ulong, byte[])>();
            try
            {
                long pre = PreOffset(creator);
                long worker = WorkerOffset(creator);
                long state = GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh;
                if (pre <= 0 || pre > 0xFFFF || worker <= 0 || worker > 0xFFFF || state <= 0 || state > 0xFFFF)
                    return kept;
                string U16(long v) => $"{v & 0xFF:X2} {(v >> 8) & 0xFF:X2}";

                ulong program = ScrProgramScanner.GetScrProgramByName(creator);
                if (program == 0)
                    return kept;
                // Race: LOCAL_U16 &pre, PUSH_CONST_0, CALL (menu reset) | PUSH_CONST_M1,
                // LOCAL_U16_STORE x | PUSH_CONST_3, LOCAL_U16 &worker, IOFFSET_U16_STORE 565
                string raceMenu = $"4F {U16(pre)} 71 5D ? ? ? 70 51 ? ? 74 4F {U16(worker)} 48 {U16(state)}";
                // LTS and Capture: the menu reset takes only &pre, then the state goes to 3.
                string menu = $"4F {U16(pre)} 5D ? ? ? 74 4F {U16(worker)} 48 {U16(state)}";
                // PUSH_CONST_F0, PUSH_CONST_F0, PUSH_CONST_F 10.0, PUSH_CONST_3, LOCAL_U16 &cam, IOFFSET_U8 9, STORE_N
                const string camera = "7A 7A 29 00 00 20 41 74 4F ? ? 40 09 33";

                // Only the push(es) and the call are cleared: 8 bytes for race, 7 otherwise.
                int menuLength = 8;
                var menuHits = ScrProgramScanner.ScanScrProgramForPattern(program, raceMenu);
                if (menuHits == null || menuHits.Count != 1)
                {
                    menuLength = 7;
                    menuHits = ScrProgramScanner.ScanScrProgramForPattern(program, menu);
                }
                if (menuHits == null || menuHits.Count != 1)
                    return kept;
                ulong menuAt = menuHits[0];
                var cameraAt = (ScrProgramScanner.ScanScrProgramForPattern(program, camera) ?? new List<ulong>())
                    .Where(a => a > menuAt && a - menuAt < 0x100).ToList();

                kept.Add((menuAt, MainWindow.m.memory(menuAt.ToString("X")).GetBytes(menuLength)));
                if (cameraAt.Count == 1)
                    kept.Add((cameraAt[0], MainWindow.m.memory(cameraAt[0].ToString("X")).GetBytes(14)));
                foreach (var (address, original) in kept)
                    MainWindow.m.memory(address.ToString("X")).SetBytes(new byte[original.Length]);
            }
            catch (Exception)
            {
                foreach (var (address, original) in kept)
                    MainWindow.m.memory(address.ToString("X")).SetBytes(original);
                kept.Clear();
            }
            return kept;
        }

        private static long LocalAddress(long index)
        {
            var e = GTA.Offsets.Editor.localptr;
            return MainWindow.m.memory(e[0], new long[] { e[1], GTA.Offsets.Editor.OFFSET_script_local_start, index * 8 }).GetAddress();
        }

        internal static int ReadLocal(long index) => MainWindow.m.memory(LocalAddress(index).ToString("X")).Get<int>();

        internal static void WriteLocal(long index, int value) => MainWindow.m.memory(LocalAddress(index).ToString("X")).SetInt(value);

        // ---- snapshots ---------------------------------------------------------------

        public static Snapshot Capture(string name)
        {
            string creator = CurrentCreator();
            var snap = new Snapshot
            {
                Edition = GameVariant.IsEnhanced ? "Enhanced" : "Legacy",
                Build = (GTA.getBuildVersion() ?? "").Trim(),
                Creator = creator,
                Name = name,
                SavedAt = DateTime.Now
            };
            foreach (var section in Sections().Where(s => s.AppliesTo(creator) && s.CountGlobal != 0 && s.FirstElement != 0 && s.Stride > 0))
            {
                long header = section.FirstElement - 1;
                int capacity = new Global(header).Get<int>();
                if (capacity <= 0 || capacity > 5000)
                    continue;   // not an array header: offsets do not fit this build
                int slots = 1 + (int)(capacity * section.Stride);
                snap.Sections.Add(new SectionData
                {
                    Name = section.Name,
                    Count = new Global(section.CountGlobal).Get<int>(),
                    Start = header,
                    Slots = slots,
                    Data = Pack(ReadSlots(header, slots))
                });
            }
            return snap;
        }

        /// <summary>Why a snapshot cannot go into the running creator, or null.</summary>
        public static string CheckCompatible(Snapshot snap)
        {
            string edition = GameVariant.IsEnhanced ? "Enhanced" : "Legacy";
            if (!string.Equals(snap.Edition, edition, StringComparison.OrdinalIgnoreCase))
                return $"edition:{snap.Edition}";
            string build = (GTA.getBuildVersion() ?? "").Trim();
            if (!string.IsNullOrEmpty(snap.Build) && !string.IsNullOrEmpty(build) && snap.Build != build)
                return $"build:{snap.Build}";
            return null;
        }

        /// <summary>
        /// Writes the chosen sections back and rebuilds the map. A section is only
        /// restored when its array sits where it sat when the file was saved.
        /// </summary>
        public static async Task<bool> RestoreAsync(Snapshot snap, ISet<string> sections)
        {
            string creator = CurrentCreator();
            foreach (var section in Sections().Where(s => sections.Contains(s.Name) && s.AppliesTo(creator)))
            {
                if (section.Name == "dprops" && snap.Version < 2)
                    continue;   // its count is not the dynamic prop count, see Snapshot.Version
                var data = snap.Sections.FirstOrDefault(d => d.Name == section.Name);
                if (data == null || data.Start != section.FirstElement - 1)
                    continue;
                byte[] bytes = Unpack(data.Data);
                if (bytes.Length != data.Slots * 8)
                    continue;
                WriteSlots(data.Start, bytes);
                new Global(section.CountGlobal).SetInt(data.Count);
            }
            return await RebuildAsync().ConfigureAwait(true);
        }

        public static void Save(Snapshot snap, string file)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllText(file, JsonConvert.SerializeObject(snap, Formatting.Indented));
        }

        public static Snapshot Load(string file)
        {
            var snap = JsonConvert.DeserializeObject<Snapshot>(File.ReadAllText(file));
            if (snap == null || snap.Format != "xenvious-map")
                throw new InvalidDataException("not a Xenvious map file");
            return snap;
        }

        // Globals are paged in blocks of 2^18 slots; a read or write must not cross one.
        internal static byte[] ReadSlots(long first, int slots)
        {
            var result = new byte[slots * 8];
            int done = 0;
            while (done < slots)
            {
                long index = first + done;
                int inPage = (int)Math.Min(slots - done, 0x40000 - (index & 0x3FFFF));
                byte[] part = new Global(index).GetBytes(inPage * 8);
                Buffer.BlockCopy(part, 0, result, done * 8, Math.Min(part.Length, inPage * 8));
                done += inPage;
            }
            return result;
        }

        internal static void WriteSlots(long first, byte[] bytes)
        {
            int slots = bytes.Length / 8;
            int done = 0;
            while (done < slots)
            {
                long index = first + done;
                int inPage = (int)Math.Min(slots - done, 0x40000 - (index & 0x3FFFF));
                var part = new byte[inPage * 8];
                Buffer.BlockCopy(bytes, done * 8, part, 0, part.Length);
                new Global(index).SetBytes(part);
                done += inPage;
            }
        }

        private static string Pack(byte[] raw)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
                    gzip.Write(raw, 0, raw.Length);
                return Convert.ToBase64String(output.ToArray());
            }
        }

        private static byte[] Unpack(string packed)
        {
            using (var input = new MemoryStream(Convert.FromBase64String(packed)))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}
