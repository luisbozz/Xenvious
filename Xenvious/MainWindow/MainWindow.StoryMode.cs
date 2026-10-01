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

        private const int HubStageQuit = 6;

        /// <summary>
        /// Goes back to story mode the way the creator hub's "Quit to Story Mode" does
        /// (creator.c func_13(-1)): the joining game mode becomes story mode (-1) and the hub's
        /// menu stage becomes quit, which swoops the sky camera up, starts the transition and
        /// ends the hub. A creator or test still running is ended first: the test through its
        /// end global, the creator through the flag every creator checks each frame. No
        /// question in the game about unsaved changes.
        /// </summary>
        private async void BtnStoryMode_Click(object sender, RoutedEventArgs e)
        {
            if (_leaving || !m.IsProcOpen)
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
                var live = await Task.Run(() => GTA.LiveScriptHashes());
                Log.Info("story mode: live scripts " + string.Join(" ", live.Select(h => h.ToString("X8"))), source: "dashboard");

                if (GTA.Offsets.Editor.launch_creator_local_3 != 0)
                    new Global(GTA.Offsets.Editor.launch_creator_local_3).SetInt(-1);

                if (await Task.Run(() => StoryModeBlockers.Any(GTA.IsScriptRunning)))
                {
                    if (GTA.Offsets.Editor.endtest != 0 && TestControllers.Any(GTA.IsScriptRunning))
                    {
                        new Global(GTA.Offsets.Editor.endtest).SetInt(1);
                        Log.Info("story mode: test end requested", source: "dashboard");
                    }
                    if (GTA.Offsets.Editor.creator_quit_flag != 0)
                    {
                        new Global(GTA.Offsets.Editor.creator_quit_flag).SetInt(1);
                        Log.Info("story mode: quit flag set", source: "dashboard");
                    }
                    bool ended = await WaitGoneAsync(() => !StoryModeBlockers.Any(GTA.IsScriptRunning));
                    Log.Info($"story mode: {(ended ? "creator scripts ended" : "creator scripts still running")}", source: "dashboard");
                }

                bool done = false;
                if (await Task.Run(() => GTA.IsScriptRunning("creator")))
                {
                    long stage = await Task.Run(() => HubLocal(GTA.Offsets.Editor.OFFSET_creator_hub_stage));
                    if (stage != 0)
                    {
                        Log.Info($"story mode: hub stage {m.memory(stage.ToString("X")).Get<int>()} -> {HubStageQuit}", source: "dashboard");
                        m.memory(stage.ToString("X")).SetInt(HubStageQuit);
                        done = await WaitGoneAsync(() => !GTA.IsScriptRunning("creator"));
                        Log.Info($"story mode: {(done ? "hub ended" : "hub still running")}", source: "dashboard");
                    }
                    else
                    {
                        Log.Warn("story mode: no hub stage offset for this edition", source: "dashboard");
                    }
                }
                else
                {
                    done = true;
                }

                if (!done)
                {
                    await ConfirmAsync(TranslateOr("leave_failed_title", "Creator läuft noch"),
                        TranslateOr("leave_failed_text", "Der Creator hat sich nicht beendet. Lade in GTA den Story Mode über das Pausemenü neu."),
                        "OK", null, danger: true);
                }
            }
            finally
            {
                // The next creator must not see the flag.
                if (m.IsProcOpen && GTA.Offsets.Editor.creator_quit_flag != 0)
                    new Global(GTA.Offsets.Editor.creator_quit_flag).SetInt(0);
                _leaving = false;
                UpdateDashboardStatus();
            }
        }

        private static async Task<bool> WaitGoneAsync(Func<bool> gone)
        {
            var until = DateTime.UtcNow + LeaveWait;
            while (DateTime.UtcNow < until)
            {
                await Task.Delay(500);
                if (!m.IsProcOpen || await Task.Run(gone))
                    return true;
            }
            return false;
        }

        // Address of a local of the creator hub script, 0 when it is not running.
        private static long HubLocal(long index)
        {
            long[] hub = GTA.getLocalScriptAddy("creator");
            if (hub == null || index == 0)
                return 0;
            try
            {
                return m.memory(hub[0], new long[] { hub[1], GTA.Offsets.Editor.OFFSET_script_local_start, index * 8 }).GetAddress();
            }
            catch
            {
                return 0;
            }
        }
    }
}
