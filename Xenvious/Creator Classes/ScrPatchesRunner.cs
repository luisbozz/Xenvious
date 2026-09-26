using System;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Xenvious.GTA;
using Xenvious.Logging;

namespace Xenvious
{

    public static class ScrPatchesRunner
    {
        public static HashSet<ulong> appliedPatches = new HashSet<ulong>();

        // Where each patch actually landed, so it can be put back. appliedPatches
        // alone only says that *some* patch owns an address, not which one.
        private static readonly Dictionary<ScrPatches, Dictionary<ulong, byte[]>> written =
            new Dictionary<ScrPatches, Dictionary<ulong, byte[]>>();

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
                    ScrPatchesRunner.ApplyPatches(GTA.Editor.ScrPatchesDev);
                    ScrPatchesRunner.ApplyPatches(GTA.Editor.ScrPatches);
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

        public static void ApplyPatches(List<ScrPatches> patches)
        {
            if (patches != null || patches.Count > 0)
            {
                foreach (var patch in patches)
                {
                    if (!patch.enabled)
                    {
                        continue;
                    }

                    if (!TriggerActive(patch.trigger))
                    {
                        continue;
                    }

                    if (patch.dev && !Functions.Read.checkbinary(30, GTA.Offsets.Editor.custom_check))
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

                    // Pattern-Scan durchführen
                    var foundAddrs = ScrProgramScanner.ScanScrProgramForPattern(scrProgramPtr, patch.pattern);
                    if (foundAddrs == null || foundAddrs.Count == 0)
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

                    // Patches anwenden
                    foreach (var addr in foundAddrs)
                    {
                        ulong patchAddress = addr + (ulong)patch.offset;

                        if (appliedPatches.Contains(patchAddress))
                            continue;

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
                                    perAddress[patchAddress] = patch.original_bytes;
                            }
                        }
                    }
                }
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
                if (!written.TryGetValue(patch, out perAddress))
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
