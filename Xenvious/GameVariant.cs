using System;
using System.Diagnostics;
using System.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>Which of the two GTA V builds is being edited.</summary>
    public enum GameEdition
    {
        /// <summary>The original build, process "GTA5".</summary>
        Legacy,

        /// <summary>The rebuilt engine released in 2025, process "GTA5_Enhanced".</summary>
        Enhanced
    }

    /// <summary>
    /// Detects which GTA V build is running and names the data that belongs to it.
    ///
    /// Rockstar ships GTA V as two separate games that can be installed side by
    /// side. They share the script *content* -- the same missions, the same
    /// creators, the same Online version number -- but they are compiled
    /// separately, so every memory address differs: the globals move, and the
    /// byte patterns used to find the engine pointers differ as well. Data for
    /// one build is therefore never valid for the other, and reading Legacy
    /// offsets out of an Enhanced process would write to unrelated memory.
    ///
    /// The edition is decided by which process is running, which is the only
    /// signal available before anything is read from the game.
    /// </summary>
    public static class GameVariant
    {
        /// <summary>Process name of the original build (no .exe suffix).</summary>
        public const string LegacyProcess = "GTA5";

        /// <summary>Process name of the Enhanced build (no .exe suffix).</summary>
        public const string EnhancedProcess = "GTA5_Enhanced";

        private static readonly object Gate = new object();
        private static GameEdition current = GameEdition.Legacy;

        /// <summary>
        /// The edition the loaded data belongs to. Defaults to Legacy so the
        /// application has a consistent state before any game has been seen;
        /// <see cref="Detect"/> corrects it as soon as one is running.
        /// </summary>
        public static GameEdition Current
        {
            get { lock (Gate) { return current; } }
        }

        /// <summary>True once a game process has actually been observed.</summary>
        public static bool Detected { get; private set; }

        public static bool IsEnhanced => Current == GameEdition.Enhanced;

        /// <summary>Process name to attach to for the current edition.</summary>
        public static string ProcessName =>
            IsEnhanced ? EnhancedProcess : LegacyProcess;

        /// <summary>Module name the pointer scans run against.</summary>
        public static string ModuleName => ProcessName + ".exe";

        /// <summary>
        /// Folder inside OfflineData that holds this edition's data. The names
        /// match the folders in the repository, so a resource name can be built
        /// from an edition without a lookup table.
        /// </summary>
        public static string DataFolder(GameEdition edition) =>
            edition == GameEdition.Enhanced ? "enhanced" : "legacy";

        /// <summary>Folder for the edition currently detected.</summary>
        public static string DataFolder() => DataFolder(Current);

        /// <summary>Human-readable name, for logs and the UI.</summary>
        public static string DisplayName(GameEdition edition) =>
            edition == GameEdition.Enhanced ? "Enhanced" : "Legacy";

        /// <summary>
        /// Look for a running game and return the edition it belongs to.
        ///
        /// Enhanced is checked first: both processes can be installed at once,
        /// and if both are somehow running, the newer build is the one a user
        /// means. When neither is running the last known edition is kept, so a
        /// game that is closed and reopened does not flip the loaded data.
        /// </summary>
        public static GameEdition Detect()
        {
            GameEdition? found = null;
            if (IsRunning(EnhancedProcess))
                found = GameEdition.Enhanced;
            else if (IsRunning(LegacyProcess))
                found = GameEdition.Legacy;

            if (found == null)
                return Current;

            lock (Gate)
            {
                if (!Detected || current != found.Value)
                {
                    Log.Info(
                        $"Detected GTA {DisplayName(found.Value)} (process {ProcessNameOf(found.Value)})",
                        source: "GameVariant");
                }
                current = found.Value;
            }
            Detected = true;
            return found.Value;
        }

        /// <summary>
        /// Detect and report whether the edition changed. Callers use the result
        /// to reload the offsets and patch data, which are edition-specific.
        /// </summary>
        public static bool DetectChanged()
        {
            bool wasDetected = Detected;
            GameEdition before = Current;
            GameEdition after = Detect();
            return after != before || (!wasDetected && Detected);
        }

        /// <summary>The running game process, or null if none is.</summary>
        public static Process FindProcess()
        {
            Detect();
            return Process.GetProcessesByName(ProcessName).FirstOrDefault();
        }

        /// <summary>Whether a game of the current edition is running.</summary>
        public static bool IsGameRunning() => FindProcess() != null;

        private static string ProcessNameOf(GameEdition edition) =>
            edition == GameEdition.Enhanced ? EnhancedProcess : LegacyProcess;

        private static bool IsRunning(string processName)
        {
            try
            {
                return Process.GetProcessesByName(processName).Length > 0;
            }
            catch (Exception)
            {
                // Enumerating processes can fail transiently (a process exiting
                // mid-enumeration, or an access error). Treat it as "not found"
                // rather than letting it escape into the UI thread.
                return false;
            }
        }
    }
}
