using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using static Xenvious.GTA;
using Newtonsoft.Json;
using Xenvious.Logging;

namespace Xenvious
{

    public static class ScrPatchesRunner
    {
        public static HashSet<ulong> appliedPatches = new HashSet<ulong>();

        // Where each patch actually landed, so it can be put back. appliedPatches
        // alone only says that *some* patch owns an address, not which one.
        // A null entry is a patch found already written (an earlier Xenvious session) whose
        // original bytes are not known: it stays in until GTA restarts.
        private static readonly Dictionary<ScrPatches, Dictionary<ulong, byte[]>> written =
            new Dictionary<ScrPatches, Dictionary<ulong, byte[]>>();

        // Original bytes of every address this session or an earlier one patched in the running
        // game, kept in %AppData%\Xenvious so a restarted Xenvious can still take patches out.
        private static readonly string JournalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "scrpatches-session.json");
        private static Dictionary<ulong, string> journal;
        private static string journalSession;

        private class Journal
        {
            public string session { get; set; }
            public Dictionary<string, string> originals { get; set; }
        }

        // Startet den automatischen Patcher
        public static void RunPatcher()
        {
            int tick = 0;
            bool templatesWereActive = false;
            while (true)
            {
                // Precise templates reacts to the creator menu, so it ticks every 50 ms.
                // It runs on this thread so that switching its patches in and out never
                // races with a patch pass.
                PreciseTemplates.Tick();
                bool templatesChanged = PreciseTemplates.Active != templatesWereActive;
                // LTS test end: on foot where the test ended (needs the 50 ms to catch the creator's take over).
                try { TestReturn.Tick(); }
                catch (Exception ex) { Log.Debug("TestReturn: " + ex.Message, source: "TestReturn"); }
                templatesWereActive = PreciseTemplates.Active;
                if (templatesChanged && !PreciseTemplates.Active)
                {
                    RevertTriggered("templates");
                }

                // A pass scans the script bytecode for every patch's pattern,
                // and a patch is only ever written once -- appliedPatches sees
                // to that. So the pass exists to notice a script that has just
                // been loaded, which is not something that happens hundreds of
                // times a second: every 250 ms, or at once when a triggered
                // patch set was switched on.
                if (MainWindow.m.IsProcOpen && (tick % 5 == 0 || templatesChanged))
                {
                    try { ScriptSpace.Reserve(); }
                    catch (Exception ex) { Log.Debug("ScriptSpace: " + ex.Message, source: "ScrPatchesRunner"); }
                    ScrPatchesRunner.ApplyPatches(GTA.Editor.ScrPatchesDev);
                    ScrPatchesRunner.ApplyPatches(GTA.Editor.ScrPatches);
                    // Same reason: a creator script that was just loaded gets the kept prop changes.
                    ModdedPropMemory.Tick();
                    // Same thread: "nrl fix" leaves nrl as it is, which must never be below 1 at save.
                    try { Rules.KeepNrl(); }
                    catch (Exception ex) { Log.Debug("KeepNrl: " + ex.Message, source: "ScrPatchesRunner"); }
                }
                tick++;
                Thread.Sleep(50);
            }
        }

        private static bool TriggerActive(string trigger)
        {
            switch (trigger)
            {
                case null:
                case "":
                    return true;
                case "templates":
                    return PreciseTemplates.Active;
                default:
                    return false;
            }
        }

        /// <summary>Takes out every applied patch of a trigger whose feature went inactive.</summary>
        private static void RevertTriggered(string trigger)
        {
            foreach (var list in new[] { GTA.Editor.ScrPatches, GTA.Editor.ScrPatchesDev })
            {
                if (list == null)
                    continue;
                foreach (var patch in list)
                {
                    if (patch.trigger == trigger)
                        Revert(patch);
                }
            }
        }

        private static bool Wanted(ScrPatches patch)
        {
            return patch.enabled && TriggerActive(patch.trigger)
                && (!patch.dev || Functions.Read.checkbinary(30, GTA.Offsets.Editor.custom_check))
                && !string.IsNullOrEmpty(patch.script_name) && !string.IsNullOrEmpty(patch.bytes_to_patch);
        }

        /// <summary>
        /// Writes the payloads for the extra code pages and returns the scripts whose payloads are
        /// all in. The pages are code nobody else runs, so a payload is simply written again
        /// whenever its bytes are not there (a freshly loaded script) and never taken out.
        /// </summary>
        private static HashSet<string> ApplyPagePatches(List<ScrPatches> patches)
        {
            var ready = new HashSet<string>();
            var missing = new HashSet<string>();
            foreach (var patch in patches.Where(p => p.page && Wanted(p)))
            {
                var space = ScriptSpace.Get(patch.script_name);
                long address = space == null || space.Base != patch.page_base ? 0 : space.Address(patch.page_base + patch.offset);
                int length = patch.bytes_to_patch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                // The payload must not run past its page's end into the next page, which is elsewhere in memory.
                bool fits = address != 0 && space.Address(patch.page_base + patch.offset + length - 1) == address + length - 1;
                if (!fits || (!HasBytes((ulong)address, patch.bytes_to_patch) && !PatchScrProgramBytecode((ulong)address, patch.bytes_to_patch)))
                {
                    missing.Add(patch.script_name);
                    continue;
                }
                ready.Add(patch.script_name);
            }
            ready.ExceptWith(missing);
            return ready;
        }

        public static void ApplyPatches(List<ScrPatches> patches)
        {
            if (patches != null && patches.Count > 0)
            {
                var pagesReady = ApplyPagePatches(patches);
                foreach (var patch in patches)
                {
                    if (patch.page || !Wanted(patch))
                    {
                        continue;
                    }

                    if (patch.needs_page && !pagesReady.Contains(patch.script_name))
                    {
                        continue;
                    }

                    if (patch.patch_name.Equals("set switch camera to caps lock"))
                    {
                        if (!Functions.Read.checkbinary(31, GTA.Offsets.Editor.custom_check))
                        {
                            continue;
                        }
                    }

                    if (string.IsNullOrEmpty(patch.script_name) ||
                        string.IsNullOrEmpty(patch.patch_name) ||
                        string.IsNullOrEmpty(patch.bytes_to_patch))
                        continue;

                    // scrProgram Pointer holen
                    ulong scrProgramPtr = ScrProgramScanner.GetScrProgramByName(patch.script_name);
                    if (scrProgramPtr == 0)
                        continue;

                    // falls values vorhanden -> Platzhalter {{ n }} ersetzen
                    string finalBytesToPatch = patch.bytes_to_patch;
                    if (patch.values != null && patch.values.Count > 0)
                    {
                        foreach (var val in patch.values)
                        {
                            // Pattern-Scan für value
                            var valueAddrs = ScrProgramScanner.ScanScrProgramForPattern(scrProgramPtr, val.pattern);
                            if (valueAddrs == null || valueAddrs.Count == 0)
                                continue;

                            // nur erste Adresse nehmen (oder mehrere verarbeiten, je nach Bedarf)
                            ulong valAddr = valueAddrs[0] + (ulong)val.offset;

                            // Bytes lesen
                            byte[] readBytes = MainWindow.m.memory(valAddr.ToString("X")).GetBytes(val.bytes_to_read);

                            // In Hex-String umwandeln
                            string hexString = BitConverter.ToString(readBytes).Replace("-", " ");
                            // Platzhalter {{ id }} ersetzen
                            finalBytesToPatch = finalBytesToPatch.Replace($"{{{{ {val.id} }}}}", hexString);
                        }
                    }

                    // Pattern-Scan durchführen
                    var foundAddrs = ScrProgramScanner.ScanScrProgramForPattern(scrProgramPtr, patch.pattern);
                    if (foundAddrs == null || foundAddrs.Count == 0)
                    {
                        // The pattern is gone once the patch is in: look for it patched instead.
                        if (!IsApplied(patch))
                            AdoptWritten(patch, scrProgramPtr, finalBytesToPatch);
                        continue;
                    }

                    // Patches anwenden
                    foreach (var addr in foundAddrs)
                    {
                        ulong patchAddress = addr + (ulong)patch.offset;

                        if (appliedPatches.Contains(patchAddress))
                        {
                            // Still ours, unless the script was loaded again over the same address.
                            if (HasBytes(patchAddress, finalBytesToPatch))
                                continue;
                            Forget(patchAddress);
                        }

                        patch.original_bytes = ReadOrigScrProgramBytecode(patchAddress, finalBytesToPatch);

                        bool success = PatchScrProgramBytecode(patchAddress, finalBytesToPatch);
                        if (success)
                        {
                            appliedPatches.Add(patchAddress);
                            lock (written)
                            {
                                if (!written.TryGetValue(patch, out var perAddress))
                                    written[patch] = perAddress = new Dictionary<ulong, byte[]>();
                                if (patch.original_bytes != null)
                                {
                                    perAddress[patchAddress] = patch.original_bytes;
                                    Remember(patchAddress, patch.original_bytes);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds a patch that is already in its script (written before Xenvious restarted): the
        /// pattern with the patch bytes laid over it. Its original bytes come from the journal, or
        /// from the pattern where it covers them; without both it cannot be taken out again.
        /// </summary>
        private static void AdoptWritten(ScrPatches patch, ulong scrProgramPtr, string finalBytes)
        {
            string[] pattern = patch.pattern.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] bytes = Unresolved(patch, finalBytes).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int o = patch.offset;
            if (o < 0 || bytes.Length == 0)
                return;
            var signature = new string[Math.Max(pattern.Length, o + bytes.Length)];
            for (int i = 0; i < signature.Length; i++)
                signature[i] = i >= o && i < o + bytes.Length ? bytes[i - o] : i < pattern.Length ? pattern[i] : "?";
            if (signature.All(t => t.StartsWith("?")))
                return;

            var found = ScrProgramScanner.ScanScrProgramForPattern(scrProgramPtr, string.Join(" ", signature));
            if (found == null || found.Count == 0)
                return;
            // A patch of only zero bytes with nothing of its pattern around them looks like every
            // other run of NOPs (our own NOP patches leave plenty); claiming those would let
            // switching it off write into code that is not its own.
            bool onlyZeros = bytes.All(b => b == "00")
                && Enumerable.Range(0, signature.Length).All(i => (i >= o && i < o + bytes.Length) || signature[i].StartsWith("?"));
            if (onlyZeros && found.Count > 1)
            {
                Log.Trace($"'{patch.patch_name}' in {patch.script_name}: its zero bytes match {found.Count} places, none taken as written", source: "ScrPatchesRunner");
                return;
            }

            LoadJournal();
            var perAddress = new Dictionary<ulong, byte[]>();
            foreach (ulong addr in found)
            {
                ulong patchAddress = addr + (ulong)o;
                byte[] original = null;
                if (journal.TryGetValue(patchAddress, out string hex))
                    original = hex.Split(' ').Select(b => Convert.ToByte(b, 16)).ToArray();
                else if (Enumerable.Range(o, bytes.Length).All(i => i < pattern.Length && !pattern[i].StartsWith("?")))
                    original = Enumerable.Range(o, bytes.Length).Select(i => Convert.ToByte(pattern[i], 16)).ToArray();
                perAddress[patchAddress] = original;
                appliedPatches.Add(patchAddress);
            }
            lock (written)
                written[patch] = perAddress;
            Log.Debug($"Found '{patch.patch_name}' already written in {patch.script_name} ({found.Count}x)", source: "ScrPatchesRunner");
        }

        // Placeholders whose value could not be read match anything.
        private static string Unresolved(ScrPatches patch, string bytes)
        {
            return Regex.Replace(bytes, @"\{\{\s*(\w+)\s*\}\}", m =>
            {
                var val = patch.values?.FirstOrDefault(v => v.id.ToString() == m.Groups[1].Value);
                return string.Join(" ", Enumerable.Repeat("?", Math.Max(1, val?.bytes_to_read ?? 1)));
            });
        }

        private static bool HasBytes(ulong address, string bytes)
        {
            try
            {
                string[] tokens = bytes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                byte[] now = MainWindow.m.memory(address.ToString("X")).GetBytes(tokens.Length);
                for (int i = 0; i < tokens.Length; i++)
                    if (!tokens[i].StartsWith("?") && Convert.ToByte(tokens[i], 16) != now[i])
                        return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // The script at this address was loaded again: what was written there is gone.
        private static void Forget(ulong address)
        {
            appliedPatches.Remove(address);
            lock (written)
            {
                foreach (var patch in written.Keys.ToList())
                {
                    if (written[patch].Remove(address) && written[patch].Count == 0)
                        written.Remove(patch);
                }
                LoadJournal();
                if (journal.Remove(address))
                    SaveJournal();
            }
        }

        // GTA's process id and start time: an address only means something in that process.
        private static string GameSession()
        {
            try
            {
                var p = Process.GetProcessesByName(GameVariant.ProcessName).FirstOrDefault();
                return p == null ? null : p.Id.ToString(CultureInfo.InvariantCulture) + "@" + p.StartTime.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        private static void LoadJournal()
        {
            string session = GameSession();
            if (journal != null && session == journalSession)
                return;
            journalSession = session;
            journal = new Dictionary<ulong, string>();
            try
            {
                if (session == null || !File.Exists(JournalPath))
                    return;
                var data = JsonConvert.DeserializeObject<Journal>(File.ReadAllText(JournalPath));
                if (data?.session != session || data.originals == null)
                    return;
                foreach (var entry in data.originals)
                    journal[ulong.Parse(entry.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture)] = entry.Value;
            }
            catch (Exception ex)
            {
                Log.Debug("Patch journal unreadable: " + ex.Message, source: "ScrPatchesRunner");
            }
        }

        private static void SaveJournal()
        {
            if (journalSession == null)
                return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(JournalPath));
                var data = new Journal { session = journalSession, originals = journal.ToDictionary(e => e.Key.ToString("X", CultureInfo.InvariantCulture), e => e.Value) };
                File.WriteAllText(JournalPath, JsonConvert.SerializeObject(data));
            }
            catch (Exception ex)
            {
                Log.Debug("Patch journal not saved: " + ex.Message, source: "ScrPatchesRunner");
            }
        }

        private static void Remember(ulong address, byte[] original)
        {
            LoadJournal();
            journal[address] = BitConverter.ToString(original).Replace("-", " ");
            SaveJournal();
        }

        /// <summary>Whether the patch is in its script but cannot be taken out before GTA restarts.</summary>
        public static bool IsStuck(ScrPatches patch)
        {
            lock (written)
            {
                return patch != null && written.TryGetValue(patch, out var perAddress) && perAddress.Values.Any(b => b == null);
            }
        }

        /// <summary>Whether the runner has written this patch into its script.</summary>
        public static bool IsApplied(ScrPatches patch)
        {
            lock (written)
            {
                return patch != null && written.ContainsKey(patch);
            }
        }

        /// <summary>
        /// Put a patch's original bytes back and let it be applied again later.
        ///
        /// Clearing the flag alone only stops the runner from writing the patch
        /// *again*; whatever it already wrote stays in the script. Without this,
        /// unticking a patch would look like it did nothing until the game is
        /// restarted.
        /// </summary>
        public static void Revert(ScrPatches patch)
        {
            if (patch == null)
                return;

            Dictionary<ulong, byte[]> perAddress;
            lock (written)
            {
                // Without its original bytes a patch can only leave with a GTA restart.
                if (!written.TryGetValue(patch, out perAddress) || perAddress.Values.Any(b => b == null))
                    return;
                written.Remove(patch);
            }

            foreach (var entry in perAddress)
            {
                try
                {
                    MainWindow.m.memory(entry.Key.ToString("X")).SetBytes(entry.Value);
                }
                catch (Exception)
                {
                    // The process can go away between the tick and this call.
                }
                appliedPatches.Remove(entry.Key);
                lock (written)
                {
                    LoadJournal();
                    if (journal.Remove(entry.Key))
                        SaveJournal();
                }
            }
        }

        private static bool PatchScrProgramBytecode(ulong patchAddress, string patchedBytes)
        {
            try
            {
                byte[] bytes = patchedBytes
                .Split(' ')
                .Select(b => Convert.ToByte(b, 16))
                .ToArray();

                MainWindow.m.memory(patchAddress.ToString("X")).SetBytes(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static byte[] ReadOrigScrProgramBytecode(ulong patchAddress, string patchedBytes)
        {
            try
            {
                return MainWindow.m.memory(patchAddress.ToString("X")).GetBytes(patchedBytes.Split(' ').Count());
            }
            catch
            {
                return null;
            }
        }
    }
}
