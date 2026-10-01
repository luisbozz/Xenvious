using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Xenvious.AdvancedPlacement;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// The dashboard's status strip: which game, which creator, how full the job is,
    /// whether the script features are on. Starting the creator ("Launch Creator")
    /// shows its progress in the creator chip instead of a button that silently writes
    /// a few globals: requested, creator script loaded, ready -- or failed after a
    /// timeout.
    /// </summary>
    public partial class MainWindow
    {
        private enum LaunchPhase { None, WaitingForFocus, Requested, ScriptLoaded, Failed }

        private static readonly TimeSpan LaunchTimeout = TimeSpan.FromSeconds(120);
        private static readonly TimeSpan FailureShownFor = TimeSpan.FromSeconds(20);

        private static readonly Brush DotOk = Frozen(0x43, 0xB5, 0x81);
        private static readonly Brush DotWarn = Frozen(0xFA, 0xC8, 0x28);
        private static readonly Brush DotBad = Frozen(0xD9, 0x53, 0x4F);
        private static readonly Brush DotOff = Frozen(0x72, 0x76, 0x7D);

        private DispatcherTimer _dashboardTimer;
        private LaunchPhase _launchPhase = LaunchPhase.None;
        private DateTime _launchStarted;
        private DateTime _launchFailedAt;

        private static Brush Frozen(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private void StartDashboardStatus()
        {
            _dashboardTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(1) };
            _dashboardTimer.Tick += (_, __) => UpdateDashboardStatus();
            _dashboardTimer.Start();
            UpdateDashboardStatus();
        }

        private void UpdateDashboardStatus()
        {
            try
            {
                bool game = m != null && m.IsProcOpen;
                bool globals = game && globalPtrSanityCheck(GTA.Offsets.Editor.GlobalPTRversion);
                bool inCreator = globals && IsInCreator();
                string creator = inCreator ? GTA.CurrentCreatorName() : "";
                // The creator hub ("Load Creation", "Create a Race", ...) runs as the script
                // "creator"; from there the player picks a creator in the game itself.
                bool creatorMenu = globals && !inCreator && GTA.IsScriptRunning("creator");
                // Before the game state: it needs the test and creator script it finds.
                UpdateTestButton(globals);
                UpdateGameState(game, globals, inCreator, creatorMenu);
                if (inCreator)
                    VisibilityGroups.Apply();

                AdvanceLaunch(game, inCreator);
                UpdateLaunchButton(inCreator);
                UpdateStatusStrip(game, inCreator, creatorMenu, creator);
                UpdateEditNav(inCreator ? creator : "");
                OfferLaunchInStoryMode(globals, inCreator, creatorMenu);
                ScriptThreadLog.Tick($"state={CurrentGameState} creator={(inCreator ? creator : "-")} transition={(globals && GTA.Offsets.Editor.transitionState != 0 ? new Global(GTA.Offsets.Editor.transitionState).Get<int>() : -1)}");
            }
            catch (Exception ex)
            {
                Log.Debug("dashboard status: " + ex.Message, source: "dashboard");
            }
        }

        private void UpdateStatusStrip(bool game, bool inCreator, bool creatorMenu, string creator)
        {
            if (game)
            {
                string build = GTA.GameVersion(Lblonlineversion?.Text, Lblbuildversion?.Text);
                DashStatusGame.Text = "GTA V " + (GameVariant.IsEnhanced ? "Enhanced" : "Legacy")
                    + (string.IsNullOrWhiteSpace(build) ? "" : " · " + build.Trim());
                DashStatusGameDot.Fill = DotOk;
            }
            else
            {
                DashStatusGame.Text = TranslateOr("dash_game_none", "GTA V nicht gefunden");
                DashStatusGameDot.Fill = DotBad;
            }

            int seconds = (int)(DateTime.UtcNow - _launchStarted).TotalSeconds;
            string creatorLabel = TranslateOr("dash_creator_label", "Creator");
            string modeLabel = TranslateOr("dash_mode_label", "Modus");
            switch (_launchPhase)
            {
                case LaunchPhase.Requested:
                case LaunchPhase.ScriptLoaded:
                    ShowStatus(TranslateOr("dash_launch_label", "Creator-Start"), string.Format(CultureInfo.CurrentCulture, "{0} · {1} s",
                        _launchPhase == LaunchPhase.Requested
                            ? TranslateOr("launch_step1", "Start angefordert")
                            : TranslateOr("launch_step2", "Creator-Script wird geladen"),
                        seconds), DotWarn);
                    break;
                case LaunchPhase.Failed:
                    ShowStatus(TranslateOr("dash_launch_label", "Creator-Start"), TranslateOr("launch_failed_short2", "Fehlgeschlagen"), DotBad);
                    break;
                default:
                    // Where the game is right now, from UpdateGameState: the creator tiles only say
                    // "Creator" while one is open, otherwise the tile names the game mode.
                    switch (CurrentGameState)
                    {
                        case GameState.Creator:
                            ShowStatus(creatorLabel, CreatorKindName(creator), DotOk);
                            break;
                        case GameState.Testing:
                            ShowStatus(TranslateOr("dash_test_label", "Test läuft"), CreatorKindName(_runningCreatorScript ?? ""), DotWarn);
                            break;
                        case GameState.CreatorMenu:
                            ShowStatus(creatorLabel, TranslateOr("dash_creator_menu2", "Auswahlmenü"), DotWarn);
                            break;
                        case GameState.Loading:
                            ShowStatus(modeLabel, TranslateOr("dash_state_loading", "Lädt …"), DotWarn);
                            break;
                        case GameState.MainMenu:
                            ShowStatus(modeLabel, TranslateOr("dash_state_mainmenu", "Hauptmenü"), DotOff);
                            break;
                        case GameState.StoryMode:
                            ShowStatus(modeLabel, TranslateOr("dash_state_story", "Story Mode"), DotOff);
                            break;
                        case GameState.Online:
                            ShowStatus(modeLabel, TranslateOr("dash_state_online", "GTA Online"), DotOff);
                            break;
                        default:
                            ShowStatus(modeLabel, "–", DotOff);
                            break;
                    }
                    break;
            }

            DashTileGame.ToolTip = string.IsNullOrWhiteSpace(Lblonlineversion?.Text)
                ? null
                : TranslateOr("onlineversion", "Online Version") + ": " + Lblonlineversion.Text.Trim()
                    + " · " + TranslateOr("patches_build", "Build") + ": " + (Lblbuildversion?.Text ?? "").Trim();

            string scName = Lbl_SCName.Text;
            DashSCInitial.Text = string.IsNullOrWhiteSpace(scName) ? "" : scName.Trim().Substring(0, 1).ToUpperInvariant();
            BtnDashSC.IsEnabled = !string.IsNullOrWhiteSpace(scName);
            UpdateDashboardJobId(inCreator);

            // Props and the creator's own count only mean something inside a creator.
            DashTileProps.Visibility = inCreator ? Visibility.Visible : Visibility.Collapsed;
            DashTileMode.Visibility = inCreator ? Visibility.Visible : Visibility.Collapsed;
            if (inCreator)
            {
                int count = new Global(GTA.Offsets.Editor.Props.number).Get<int>();
                int limit = PropPlacementService.PropLimit;
                DashStatusProps.Text = string.Format(CultureInfo.CurrentCulture, "{0} / {1}", count, limit);
                DashStatusDynamic.Text = new Global(GTA.Offsets.Editor.DProps.number).Get<int>().ToString(CultureInfo.CurrentCulture);
                DashPropsBar.Width = 170.0 * Math.Max(0, Math.Min(count, limit)) / limit;
                DashPropsBar.Fill = count >= limit ? DotBad : count >= limit * 0.9 ? DotWarn : (Brush)DashTileProps.FindResource("DashPropBrush");
                UpdateDashboardCreatorCounts(creator);
            }

            bool script = cbsettingsexpscrfeat.IsChecked == true;
            DashStatusScript.Text = script
                ? TranslateOr("dash_script_on", "Script-Funktionen an")
                : TranslateOr("dash_script_off", "Script-Funktionen aus");
            DashStatusScriptDot.Fill = script ? DotOk : DotOff;

            UpdateDashboardForCreator(inCreator ? creator : null);
        }

        private void ShowStatus(string label, string value, Brush dot)
        {
            DashStatusCreatorLabel.Text = label;
            DashStatusCreator.Text = value;
            DashStatusCreatorDot.Fill = dot;
        }

        // What the tile shows under "Creator" / "Test": the job type without the word "Creator".
        private static string CreatorKindName(string script)
        {
            switch (script)
            {
                case "fm_race_creator": return "Race";
                case "fm_lts_creator": return "Last Team Standing";
                case "fm_capture_creator": return "Capture";
                case "fm_deathmatch_creator": return "Deathmatch";
                case "fm_survival_creator": return "Survival";
                case "public_mission_creator": return "Mission";
                default: return "–";
            }
        }

        private static string CreatorDisplayName(string script)
        {
            switch (script)
            {
                case "fm_race_creator": return "Race Creator";
                case "fm_lts_creator": return "LTS Creator";
                case "fm_capture_creator": return "Capture Creator";
                case "fm_deathmatch_creator": return "Deathmatch Creator";
                case "fm_survival_creator": return "Survival Creator";
                case "public_mission_creator": return "Mission Creator";
                default: return "Creator";
            }
        }

        private bool _storyModeChecked;

        /// <summary>Where the game is, as far as Xenvious can tell from the running scripts.</summary>
        public enum GameState { NoGame, Loading, MainMenu, StoryMode, Online, CreatorMenu, Creator, Testing }

        public GameState CurrentGameState { get; private set; } = GameState.NoGame;

        // Markers from the thread log (Enhanced 1.0.1158 and Legacy): story mode runs flow_controller (there
        // is no respawn_controller), a switch between modes runs maintransition, the creator hub
        // runs creator, GTA Online freemode. In the game's own main menu no thread is live. Only
        // live threads count (GTA.IsLiveThread); Legacy was checked the same way.
        private void UpdateGameState(bool game, bool globals, bool inCreator, bool creatorMenu)
        {
            GameState state = !game ? GameState.NoGame
                : !globals || GTA.IsScriptRunning("maintransition") ? GameState.Loading
                : inCreator ? GameState.Creator
                // In a test the creator does not count as open (IsInCreator); LTS, Capture,
                // Mission and Survival tests run a mission controller (TestControllers). Race and
                // deathmatch tests have none and still show as the creator menu.
                : _testRunning ? GameState.Testing
                : creatorMenu ? GameState.CreatorMenu
                : GTA.IsScriptRunning("freemode") ? GameState.Online
                // startup_positioning places the player while the loading screen is still up.
                : GTA.IsScriptRunning("flow_controller") ? (GTA.IsScriptRunning("startup_positioning") ? GameState.Loading : GameState.StoryMode)
                : GameState.MainMenu;
            if (state != CurrentGameState)
                Log.Info($"game state: {CurrentGameState} -> {state}", source: "dashboard");
            CurrentGameState = state;
        }

        /// <summary>
        /// Once per Xenvious start: in story mode the creator is one click away, so offer it.
        /// </summary>
        private async void OfferLaunchInStoryMode(bool globals, bool inCreator, bool creatorMenu)
        {
            if (_storyModeChecked || !globals)
                return;
            _storyModeChecked = true;
            if (_launchPhase != LaunchPhase.None || CurrentGameState != GameState.StoryMode)
            {
                // Story mode is still to come from the main menu or a loading screen; look again then.
                if (CurrentGameState == GameState.MainMenu || CurrentGameState == GameState.Loading)
                    _storyModeChecked = false;
                return;
            }

            bool start = await ConfirmAsync(
                TranslateOr("launch_story_title", "Story Mode erkannt"),
                TranslateOr("launch_story_text2", "Soll Xenvious den Creator direkt starten? Xenvious holt dazu GTA nach vorne und meldet sich, sobald der Creator läuft."),
                TranslateOr("launch_button", "Creator starten"),
                TranslateOr("launch_not_now", "Nicht jetzt"));
            if (start)
                StartLaunch();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        private static readonly TimeSpan FocusWait = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan AutoFocusWait = TimeSpan.FromSeconds(3);
        private DispatcherTimer _focusTimer;

        /// <summary>
        /// Brings the game window to the front. Windows only allows this to the process
        /// that has the focus, which Xenvious has right after its button was clicked.
        /// </summary>
        private static bool FocusGame()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(GameVariant.ProcessName))
                {
                    IntPtr window = process.MainWindowHandle;
                    if (window == IntPtr.Zero)
                        continue;
                    if (IsIconic(window))
                        ShowWindow(window, SW_RESTORE);
                    return SetForegroundWindow(window);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("focus game: " + ex.Message, source: "dashboard");
            }
            return false;
        }

        private static bool GameHasFocus()
        {
            try
            {
                GetWindowThreadProcessId(GetForegroundWindow(), out uint pid);
                return pid != 0 && Process.GetProcessById((int)pid).ProcessName
                    .Equals(GameVariant.ProcessName, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enhanced only takes the launch reliably while GTA has window focus (without it
        /// the transition ends up in GTA Online), so there Xenvious brings the game to the
        /// front first and starts once it has the focus. Only if Windows refuses that does
        /// a dialog ask the player to switch. Legacy starts right away.
        /// </summary>
        private void StartLaunch()
        {
            if (!m.IsProcOpen)
                return;
            _launchStarted = DateTime.UtcNow;
            if (!GameVariant.IsEnhanced || GameHasFocus())
            {
                FireLaunch();
                return;
            }

            _launchPhase = LaunchPhase.WaitingForFocus;
            bool focused = FocusGame();
            Log.Info($"Launch creator: bringing GTA to the front ({(focused ? "ok" : "refused")})", source: "dashboard");
            bool asked = false;
            _focusTimer?.Stop();
            _focusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _focusTimer.Tick += (_, __) =>
            {
                if (_launchPhase != LaunchPhase.WaitingForFocus)
                {
                    _focusTimer.Stop();
                }
                else if (GameHasFocus())
                {
                    _focusTimer.Stop();
                    if (asked)
                        CloseDialog(false);
                    FireLaunch();
                }
                else if (!asked && DateTime.UtcNow - _launchStarted > AutoFocusWait)
                {
                    // Windows did not hand over the focus; ask the player instead.
                    asked = true;
                    Log.Info("Launch creator: waiting for the player to switch to GTA", source: "dashboard");
                    ShowFocusDialog();
                }
                else if (DateTime.UtcNow - _launchStarted > FocusWait || !m.IsProcOpen)
                {
                    _focusTimer.Stop();
                    if (asked)
                        CloseDialog(false);
                    _launchPhase = LaunchPhase.None;
                    Log.Info("Launch creator: GTA never got focus, cancelled", source: "dashboard");
                    UpdateDashboardStatus();
                }
            };
            _focusTimer.Start();
            UpdateDashboardStatus();
        }

        // The dialog's only button cancels; it closes by itself once GTA has focus.
        private async void ShowFocusDialog()
        {
            bool cancel = await ConfirmAsync(
                TranslateOr("launch_focus_title", "Zu GTA wechseln"),
                TranslateOr("launch_focus_text", "In GTA V Enhanced startet der Creator nur, wenn das Spiel im Vordergrund ist. Wechsle jetzt zu GTA – der Start läuft dann automatisch."),
                TranslateOr("dialog_cancel", "Abbrechen"), null);
            if (cancel && _launchPhase == LaunchPhase.WaitingForFocus)
            {
                _focusTimer?.Stop();
                _launchPhase = LaunchPhase.None;
                Log.Info("Launch creator: cancelled", source: "dashboard");
                UpdateDashboardStatus();
            }
        }

        /// <summary>
        /// GTA has the focus while the creator starts, so Xenvious tells the player what
        /// happened when they come back: one dialog per launch.
        /// </summary>
        private void FireLaunch()
        {
            LaunchCreator();
            _launchPhase = LaunchPhase.Requested;
            _launchStarted = DateTime.UtcNow;
            Log.Info("Launch creator requested", source: "dashboard");
            UpdateDashboardStatus();
        }

        private void AdvanceLaunch(bool game, bool inCreator)
        {
            switch (_launchPhase)
            {
                case LaunchPhase.Requested:
                case LaunchPhase.ScriptLoaded:
                    if (!game)
                    {
                        _launchPhase = LaunchPhase.None;
                        return;
                    }
                    // The launch ends in the game's creator hub, where the player picks the
                    // creator; reaching the hub (or a creator) is the end of it.
                    if (inCreator || CurrentGameState == GameState.CreatorMenu)
                    {
                        _launchPhase = LaunchPhase.None;
                        double took = (DateTime.UtcNow - _launchStarted).TotalSeconds;
                        Log.Info($"{(inCreator ? "Creator" : "Creator menu")} ready after {took:0.0} s", source: "dashboard");
                        return;
                    }
                    if (_launchPhase == LaunchPhase.Requested && GTA.getCurrentCreatorAddy() != null)
                        _launchPhase = LaunchPhase.ScriptLoaded;
                    if (DateTime.UtcNow - _launchStarted > LaunchTimeout)
                    {
                        Log.Warn($"Creator did not start within {LaunchTimeout.TotalSeconds:0} s ({_launchPhase})", source: "dashboard");
                        _launchPhase = LaunchPhase.Failed;
                        _launchFailedAt = DateTime.UtcNow;
                    }
                    break;
                case LaunchPhase.Failed:
                    if (inCreator || !game || DateTime.UtcNow - _launchFailedAt > FailureShownFor)
                        _launchPhase = LaunchPhase.None;
                    break;
            }
        }
    


        public static void LaunchCreator()
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.launch_creator_local_2).SetInt(2);
                new Global(GTA.Offsets.Editor.launch_creator_local_1).SetInt(1);
                new Global(GTA.Offsets.Editor.transitionState).SetInt(3);
                new Global(GTA.Offsets.Editor.launch_creator_local_3).SetInt(2);
                new Global(GTA.Offsets.Editor.launch_creator_local_4).SetInt(1);
                new Global(GTA.Offsets.Editor.launch_creator_local_5).SetInt(32);
            }
        }


        private bool _inCreatorNow;
        private bool _leaving;

        // In a creator the button leaves it instead of starting one.
        private void UpdateLaunchButton(bool inCreator)
        {
            _inCreatorNow = inCreator;
            BtnLaunchCreator.Content = inCreator
                ? TranslateOr("leave_creator", "Creator verlassen")
                : TranslateOr("launchcreator", "Launch Creator");
            BtnLaunchCreator.IsEnabled = !_leaving;
        }

        private void BtnLaunchCreator_Click(object sender, RoutedEventArgs e)
        {
            if (_inCreatorNow)
                LeaveCreatorAsync();
            else
                StartLaunch();
        }

        private const int CreatorStateExit = 94;
        private static readonly TimeSpan LeaveWait = TimeSpan.FromSeconds(20);

        /// <summary>
        /// Leaves the creator for story mode. First the creator's own Exit (main state 94,
        /// which also asks in the game about unsaved changes); if the creator is stuck or
        /// does not leave, and the player agrees, the flag every creator checks each frame
        /// to clean up and end itself. Nothing but GTA Online's transition clears that
        /// flag, so Xenvious clears it once the creator is gone; left set, the next
        /// creator would end right after starting.
        /// </summary>
        private async void LeaveCreatorAsync()
        {
            if (_leaving || !m.IsProcOpen)
                return;
            _leaving = true;
            try
            {
                string creator = CreatorMap.CurrentCreator();
                long worker = CreatorMap.WorkerOffset(creator);
                long state = worker + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh;
                if (worker != 0 && CreatorMap.ReadLocal(state) == CreatorMap.StateEditing)
                {
                    CreatorMap.WriteLocal(state, CreatorStateExit);
                    Log.Info($"leave creator: exit requested ({creator})", source: "dashboard");
                    if (await WaitForCreatorGoneAsync())
                        return;
                }

                if (GTA.Offsets.Editor.creator_quit_flag == 0)
                    return;
                bool force = await ConfirmAsync(
                    TranslateOr("leave_force_title", "Creator hängt?"),
                    TranslateOr("leave_force_text", "Der Creator reagiert nicht auf Verlassen. Xenvious kann ihn hart beenden; GTA räumt dann auf und geht zurück in den Story Mode. Nicht gespeicherte Änderungen gehen verloren."),
                    TranslateOr("leave_force_confirm", "Hart beenden"), TranslateOr("dialog_cancel", "Abbrechen"), danger: true);
                if (!force)
                    return;

                new Global(GTA.Offsets.Editor.creator_quit_flag).SetInt(1);
                Log.Info($"leave creator: quit flag set ({creator})", source: "dashboard");
                bool gone = await WaitForCreatorGoneAsync();
                Log.Info($"leave creator: {(gone ? "creator ended" : "creator still running")}", source: "dashboard");
                if (!gone)
                {
                    await ConfirmAsync(TranslateOr("leave_failed_title", "Creator läuft noch"),
                        TranslateOr("leave_failed_text", "Der Creator hat sich nicht beendet. Lade in GTA den Story Mode über das Pausemenü neu."),
                        "OK", null, danger: true);
                }
            }
            finally
            {
                // Whether it worked or not, the next creator must not see the flag.
                if (m.IsProcOpen && GTA.Offsets.Editor.creator_quit_flag != 0)
                    new Global(GTA.Offsets.Editor.creator_quit_flag).SetInt(0);
                _leaving = false;
                UpdateDashboardStatus();
            }
        }

        private async System.Threading.Tasks.Task<bool> WaitForCreatorGoneAsync()
        {
            var until = DateTime.UtcNow + LeaveWait;
            while (DateTime.UtcNow < until)
            {
                await System.Threading.Tasks.Task.Delay(250);
                if (!m.IsProcOpen || !IsInCreator())
                    return true;
            }
            return false;
        }
}
}
