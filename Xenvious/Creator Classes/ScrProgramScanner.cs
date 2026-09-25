using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Provides helpers to inspect and scan script program bytecode pages.
    /// </summary>
    public static class ScrProgramScanner
    {
        public static ulong GetScrProgramByName(string scriptName)
        {
            if (!MainWindow.m.IsProcOpen || string.IsNullOrWhiteSpace(scriptName))
            {
                return 0;
            }

            // Without the scrProgram pointer there is no script table to walk.
            // Enhanced ships with that AOB pattern still empty, so the address
            // is 0, the count below reads whatever happens to sit at 0x18, and
            // the loop runs for however many billion that turns out to be --
            // twice a memory read each, 43 patches deep, inside RunPatcher's
            // unthrottled while(true). The app comes up and then wedges.
            if (GTA.Offsets.Editor.scrProgram_addy == 0)
            {
                return 0;
            }

            uint scriptHash = Functions.joaat(scriptName);
            ulong scriptTableBase = MainWindow.m.memory(GTA.Offsets.Editor.scrProgram_addy).Get<ulong>();
            int scriptCount = MainWindow.m.memory(GTA.Offsets.Editor.scrProgram_addy + 0x18).Get<int>();

            // A plausible table is a few hundred entries. Anything else means
            // the pointer resolved to something that is not the script table,
            // and walking it would be a very long way to arrive at nothing.
            if (scriptTableBase == 0 || scriptCount <= 0 || scriptCount > 4096)
            {
                Log.Debug($"Script table looks wrong (base 0x{scriptTableBase:X}, " +
                          $"count {scriptCount}) -- not scanning", source: "ScrProgramScanner");
                return 0;
            }

            for (int i = 0; i < scriptCount; i++)
            {
                ulong entryPtr = scriptTableBase + (ulong)(i * 0x10);
                ulong programPtr = MainWindow.m.memory(entryPtr.ToString("X")).Get<ulong>();

                uint programHash;
                try
                {
                    programHash = MainWindow.m.memory((entryPtr + 0x0C).ToString("X")).Get<uint>();
                }
                catch (Exception ex)
                {
                    Log.Debug($"Could not read hash for script table entry 0x{entryPtr:X}: {ex.Message}", source: "ScrProgramScanner");
                    continue;
                }

                if (programHash == scriptHash)
                {
                    return programPtr;
                }
            }

            return 0;
        }

        public static List<ulong> ScanScrProgramForPattern(ulong scrProgramPtr, string pattern)
        {
            if (scrProgramPtr == 0 || string.IsNullOrWhiteSpace(pattern))
            {
                return null;
            }

            var codePages = GetScrProgramByteCodeRegion(scrProgramPtr);
            if (codePages == null || codePages.Count == 0)
            {
                return null;
            }

            pattern = pattern.Trim();
            var foundAddresses = new List<ulong>();

            foreach (var (pageAddr, pageSize) in codePages)
            {
                byte[] pageData;
                try
                {
                    pageData = MainWindow.m.memory(pageAddr.ToString("X")).GetBytes(pageSize);
                }
                catch (Exception ex)
                {
                    Log.Debug($"Failed to read page 0x{pageAddr:X}: {ex.Message}", source: "ScrProgramScanner");
                    continue;
                }

                ulong addr = MainWindow.m.AOBScanBytes(pattern, (long)pageAddr, pageData);
                if (addr != 0uL)
                {
                    foundAddresses.Add(addr);
                }
            }

            return foundAddresses.Count > 0 ? foundAddresses : null;
        }

        public static List<(ulong PagePtr, int PageSize)> GetScrProgramByteCodeRegion(ulong scrProgramPtr)
        {
            if (scrProgramPtr == 0)
            {
                return null;
            }

            int numCodePages = MainWindow.m.memory((scrProgramPtr + 0x1C).ToString("X")).Get<int>();
            long codeBlocksPtr = MainWindow.m.memory((scrProgramPtr + 0x10).ToString("X")).Get<long>();

            if (!(codeBlocksPtr > 0x10000 && codeBlocksPtr < 0x7FFFFFFFFFFF))
            {
                Debug.WriteLine($"Invalid code blocks pointer: 0x{codeBlocksPtr:X}");
                return null;
            }

            var pages = new List<(ulong, int)>();
            int totalPages = (numCodePages + 0x3FFF) >> 14;

            for (int i = 0; i < totalPages; i++)
            {
                ulong pagePtr = MainWindow.m.memory((codeBlocksPtr + (long)(i * 8)).ToString("X")).Get<ulong>();
                if (!(pagePtr > 0x10000 && pagePtr < 0x7FFFFFFFFFFF))
                {
                    Debug.WriteLine($"Skipping invalid page at index {i}: 0x{pagePtr:X}");
                    continue;
                }

                int pageSize = (i < (numCodePages >> 14)) ? 0x4000 : (numCodePages & 0x3FFF);
                pages.Add((pagePtr, pageSize));
            }

            return pages.Count == 0 ? null : pages;
        }
    }
}
