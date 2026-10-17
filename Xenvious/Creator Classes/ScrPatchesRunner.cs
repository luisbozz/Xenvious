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
        // The program each patch was last written into. A pass skips the full scan of a patch
        // that still sits there: the mission controller is 7 MB, scanned per pattern every 250 ms.
        private static readonly Dictionary<ScrPatches, ulong> writtenProgram = new Dictionary<ScrPatches, ulong>();
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
            var onlineTestWasActive = new HashSet<string>();
            bool oneTeamWasActive = false;
            bool testCheckpointWasActive = false;
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
                // "Test like online": a switch that went off (or the creator was left) takes its
                // patches out; one that went on gets a pass at once.
                var onlineTestActive = new HashSet<string>(OnlineTest.Features.Where(OnlineTest.Active));
                bool onlineTestChanged = !onlineTestActive.SetEquals(onlineTestWasActive);
                if (onlineTestChanged)
                    Log.Debug("online test active: " + (onlineTestActive.Count == 0 ? "none" : string.Join(", ", onlineTestActive)), source: "OnlineTest");
                foreach (string feature in onlineTestWasActive.Except(onlineTestActive))
                    RevertTriggered(OnlineTest.TriggerPrefix + feature);
                onlineTestWasActive = onlineTestActive;
                // "test with one team": out again once the creator is left.
                bool oneTeamActive = TriggerActive("test_oneteam");
                if (oneTeamWasActive && !oneTeamActive)
                    RevertTriggered("test_oneteam");
                bool oneTeamChanged = oneTeamActive != oneTeamWasActive;
                oneTeamWasActive = oneTeamActive;
                // Test from a checkpoint: the controller keeps running between tests, so its
                // spawn patches must come out again before a test from the start.
                bool testCheckpointActive = TriggerActive("rcp_test");
                if (testCheckpointWasActive && !testCheckpointActive)
                    RevertTriggered("rcp_test");
                testCheckpointWasActive = testCheckpointActive;

                // A pass scans the script bytecode for every patch's pattern,
                // and a patch is only ever written once -- appliedPatches sees
                // to that. So the pass exists to notice a script that has just
                // been loaded, which is not something that happens hundreds of
                // times a second: every 250 ms, or at once when a triggered
                // patch set was switched on.
                if (MainWindow.m.IsProcOpen && (tick % 5 == 0 || templatesChanged || onlineTestChanged || oneTeamChanged))
                {
                    try { ScriptSpace.Reserve(); }
                    catch (Exception ex) { Log.Debug("ScriptSpace: " + ex.Message, source: "ScrPatchesRunner"); }
                    ScrPatchesRunner.ApplyPatches(GTA.Editor.ScrPatchesDev);
                    ScrPatchesRunner.ApplyPatches(GTA.Editor.ScrPatches);
                    try { OnlineTest.Tick(); }
                    catch (Exception ex) { Log.Debug("OnlineTest: " + ex.Message, source: "OnlineTest"); }
                    // After the patch pass, which sets up the data page the rule note goes into.
                    try { RuleNotes.Tick(); }
                    catch (Exception ex) { Log.Debug("RuleNotes: " + ex.Message, source: "RuleNotes"); }
                    try { SaveResult.Tick(); }
                    catch (Exception ex) { Log.Debug("SaveResult: " + ex.Message, source: "SaveResult"); }
                    try { RestartCheckpoints.Tick(); }
                    catch (Exception ex) { Log.Debug("RestartCheckpoints: " + ex.Message, source: "RestartCheckpoints"); }
                    // Same reason: a creator script that was just loaded gets the kept prop changes.
                    ModdedPropMemory.Tick();
                    // Same thread: "nrl fix" leaves nrl as it is, which must never be below 1 at save.
                    try { Rules.KeepNrl(); }
                    catch (Exception ex) { Log.Debug("KeepNrl: " + ex.Message, source: "ScrPatchesRunner"); }
                    // After the patch pass, which sets up the data page the drawer reads.
                    if (tick % 10 == 0)
                    {
                        try { PlayAreaOverlay.Tick(); }
                        catch (Exception ex) { Log.Debug("PlayAreaOverlay: " + ex.Message, source: "ScrPatchesRunner"); }
                        try { ScriptDrawer.SetZoneNames(GTA.CurrentCreatorName(), ZoneTypes.UsedNames()); }
                        catch (Exception ex) { Log.Debug("SetZoneNames: " + ex.Message, source: "ScrPatchesRunner"); }
                    }
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
                // The classic controller ends a job with one team at once (func_893: "tnum == 1"),
                // in the creator test too; the patch asks for tnum -1 instead. Only while the LTS or
                // Capture creator runs, so a real job stays as Rockstar made it.
                case "test_oneteam":
                    return MainWindow.m.IsProcOpen && OnlineTest.InCreator();
                // A creator test armed to start at a restart checkpoint: in the creator test the
                // classic controller sends the player to the team start and skips its restart branch
                // (func_17931); the patches take that branch and use its warp to the checkpoint.
                case "rcp_test":
                    return MainWindow.m.IsProcOpen && OnlineTest.InCreator() && RestartCheckpoints.TestCheckpoint() >= 0;
                default:
                    return trigger.StartsWith(OnlineTest.TriggerPrefix, StringComparison.Ordinal)
                        && OnlineTest.Active(trigger.Substring(OnlineTest.TriggerPrefix.Length));
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
                && (!patch.dev || ScriptVars.FeaturesOn)
                && !string.IsNullOrEmpty(patch.script_name) && !string.IsNullOrEmpty(patch.bytes_to_patch);
        }

        // Scripts already warned about an older payload in their space, by program.
        private static readonly HashSet<ulong> foreignPages = new HashSet<ulong>();

        // The scripts whose space holds this build's payload, from the last patch pass.
        private static volatile HashSet<string> pagesReady = new HashSet<string>();

        /// <summary>
        /// Whether the script runs this build's customfuncs payload, so requests in its data page
        /// (teleport, datafile probe) are answered.
        /// </summary>
        public static bool PageReady(string script) => pagesReady.Contains(script);

        /// <summary>
        /// Writes the payloads for the extra code pages and returns the scripts whose payloads are
        /// all in. The pages are code nobody else runs, so a payload is written whenever its bytes
        /// are not there in a space this process set up (a freshly loaded script) and never taken
        /// out. A space an earlier Xenvious set up keeps what it has: its hooks CALL into labels of
        /// that payload, and a new payload of another size moved them (GTA crashed in the race
        /// creator when Xenvious started with a changed payload). Then the script runs without the
        /// new payload until it is loaded again.
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
                if (fits && !space.OwnSetup && !HasBytes((ulong)address, patch.bytes_to_patch))
                {
                    ulong prog = ScrProgramScanner.GetScrProgramByName(patch.script_name);
                    if (foreignPages.Add(prog))
                        Log.Warn($"{patch.script_name}: an earlier Xenvious put another payload in; it stays until the script is loaded again (leave the creator or restart GTA)", source: "ScrPatchesRunner");
                    missing.Add(patch.script_name);
                    continue;
                }
                if (!fits || (!HasBytes((ulong)address, patch.bytes_to_patch) && !PatchScrProgramBytecode((ulong)address, patch.bytes_to_patch)))
                {
                    missing.Add(patch.script_name);
                    continue;
                }
                ready.Add(patch.script_name);
            }
            ready.ExceptWith(missing);
            // The dev list has no page entries; only the list with them says which are in.
            if (patches.Any(p => p.page))
                pagesReady = ready;
            return ready;
        }

        public static void ApplyPatches(List<ScrPatches> patches)
        {
            if (patches != null && patches.Count > 0)
            {
                var ready = ApplyPagePatches(patches);
                foreach (var patch in patches)
                {
                    if (patch.page || !Wanted(patch))
                    {
                        continue;
                    }

                    if (patch.needs_page && !ready.Contains(patch.script_name))
                    {
                        continue;
                    }

                    if (patch.patch_name.Equals("set switch camera to caps lock"))
                    {
                        if (!ScriptVars.CameraKey)
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

                    if (StillWritten(patch, scrProgramPtr))
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
                    int wrote = 0;
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
                            wrote++;
                            appliedPatches.Add(patchAddress);
                            lock (written)
                            {
                                writtenProgram[patch] = scrProgramPtr;
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
                    // Switched patches are the ones testers ask about ("is it on?").
                    if (wrote > 0 && !string.IsNullOrEmpty(patch.trigger))
                        Log.Debug($"'{patch.patch_name}' written in {patch.script_name} ({wrote}x)", source: "ScrPatchesRunner");
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

        // Whether the bytes at the address already are the patch ("?" matches any byte).
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

        // Written into this very program and every address still holds the patch: nothing to do.
        // Only for patches without values, whose bytes are known without a scan.
        private static bool StillWritten(ScrPatches patch, ulong scrProgramPtr)
        {
            if (patch.values != null && patch.values.Count > 0)
                return false;
            Dictionary<ulong, byte[]> perAddress;
            lock (written)
            {
                if (!writtenProgram.TryGetValue(patch, out ulong program) || program != scrProgramPtr
                    || !written.TryGetValue(patch, out perAddress) || perAddress.Count == 0)
                    return false;
                perAddress = new Dictionary<ulong, byte[]>(perAddress);
            }
            return perAddress.Keys.All(a => HasBytes(a, patch.bytes_to_patch));
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

            lock (written)
                writtenProgram.Remove(patch);
            // Put the original back only over our own bytes: a script that has ended since (the
            // mission controller after a test) may have left that memory to something else.
            bool known = patch.values == null || patch.values.Count == 0;
            foreach (var entry in perAddress)
            {
                try
                {
                    if (!known || HasBytes(entry.Key, patch.bytes_to_patch))
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
