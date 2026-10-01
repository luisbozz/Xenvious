using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: "Back to Story Mode" on the dashboard, for a creator or creator test that is stuck.
    public partial class MainWindow
    {
        // Story mode only starts once these are gone (maintransition's return to single player
        // waits for every creator and the mission controller).
        private static readonly string[] StoryModeBlockers =
        {
            "fm_lts_creator", "fm_capture_creator", "fm_deathmatch_creator", "fm_race_creator",
            "fm_survival_creator", "public_mission_creator",
            "fm_mission_controller", "public_mission_controller", "fm_survival_controller",
        };

        /// <summary>
        /// Ends a running test, then sets the flag every creator checks each frame to clean up
        /// and end itself (the same as "force quit" when leaving the creator); GTA then goes
        /// back to story mode. No question in the game about unsaved changes.
        /// </summary>
        private async void BtnStoryMode_Click(object sender, RoutedEventArgs e)
        {
            if (_leaving || !m.IsProcOpen || GTA.Offsets.Editor.creator_quit_flag == 0)
                return;
            bool go = await ConfirmAsync(
                TranslateOr("storymode_title", "Zurück in den Story Mode?"),
                TranslateOr("storymode_text", "Xenvious beendet einen laufenden Test und den Creator hart; GTA räumt auf und geht zurück in den Story Mode. Nicht gespeicherte Änderungen gehen verloren."),
                TranslateOr("storymode_confirm", "Zum Story Mode"), TranslateOr("dialog_cancel", "Abbrechen"), danger: true);
            if (!go || !m.IsProcOpen)
                return;

            _leaving = true;
            UpdateDashboardStatus();
            try
            {
                if (GTA.Offsets.Editor.endtest != 0 && TestControllers.Any(GTA.IsScriptRunning))
                {
                    new Global(GTA.Offsets.Editor.endtest).SetInt(1);
                    Log.Info("story mode: test end requested", source: "dashboard");
                }
                new Global(GTA.Offsets.Editor.creator_quit_flag).SetInt(1);
                Log.Info("story mode: quit flag set", source: "dashboard");

                bool gone = false;
                var until = DateTime.UtcNow + LeaveWait;
                while (DateTime.UtcNow < until)
                {
                    await Task.Delay(500);
                    if (!m.IsProcOpen || !await Task.Run(() => StoryModeBlockers.Any(GTA.IsScriptRunning)))
                    {
                        gone = true;
                        break;
                    }
                }
                Log.Info($"story mode: {(gone ? "creator scripts ended" : "creator scripts still running")}", source: "dashboard");
                if (!gone)
                {
                    await ConfirmAsync(TranslateOr("leave_failed_title", "Creator läuft noch"),
                        TranslateOr("leave_failed_text", "Der Creator hat sich nicht beendet. Lade in GTA den Story Mode über das Pausemenü neu."),
                        "OK", null, danger: true);
                }
            }
            finally
            {
                // The next creator must not see the flag.
                if (m.IsProcOpen)
                    new Global(GTA.Offsets.Editor.creator_quit_flag).SetInt(0);
                _leaving = false;
                UpdateDashboardStatus();
            }
        }
    }
}
