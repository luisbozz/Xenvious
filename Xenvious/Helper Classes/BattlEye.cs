using System.Diagnostics;

namespace Xenvious
{
    /// <summary>
    /// Tells whether GTA runs with BattlEye. Xenvious must not open the game then: BattlEye
    /// watches for other processes reading or writing the game's memory. Only the process list is
    /// looked at, nothing of GTA itself, so the check is safe to run before connecting.
    /// </summary>
    public static class BattlEye
    {
        // The BattlEye service, and the launchers GTA starts when BattlEye is on.
        private static readonly string[] Processes = { "BEService", "BEService_x64", "GTA5_BE", "GTA5_Enhanced_BE" };

        public static string RunningProcess()
        {
            foreach (string name in Processes)
            {
                var found = Process.GetProcessesByName(name);
                bool any = found.Length > 0;
                foreach (var p in found)
                    p.Dispose();
                if (any)
                    return name;
            }
            return null;
        }
    }
}
