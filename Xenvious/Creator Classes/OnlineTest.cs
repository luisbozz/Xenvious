using System;
using System.Collections.Generic;
using System.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// "Test like online" (Mission › Test): the LTS / Capture creator test behaves like the real
    /// job. Each switch is a set of script patches with the trigger TriggerPrefix + feature:
    /// - phone: the creator switches the phone off every frame while its test runs
    ///   (main loop, "if test running: disable cellphone"); the patch skips that call.
    /// - all others: places where fm_mission_controller asks "is this a creator test?"
    ///   (func_18070 in 1.73, Global_1575066 / Enhanced 1575068) get "no". Online that question
    ///   is answered "no" anyway, so a patch that ever ran in a real job would change nothing.
    /// - mortal: the creator makes the player invincible in three places, the patches pass
    ///   false to each: SET_ENTITY_INVINCIBLE and SET_ENTITY_PROOFS (bullet, fire, explosion,
    ///   collision, melee) in its launch code, and SET_PLAYER_INVINCIBLE every frame while the
    ///   controller runs ("if not invincible: make invincible"). The LTS creator also sets
    ///   SET_ENTITY_INVINCIBLE and the proofs again every frame of the test (func_41 > func_94,
    ///   for a mission-type job with f_2 5); Capture has no such call. The controller leaves
    ///   invincibility alone in a test (func_13055 checks the test flag), so without them the
    ///   player can die as online. Its own switch, not part of the master.
    /// What stays as in the test, on purpose: XP / RP / stats and unlocks (the stat writers check
    /// the test flag themselves), and the team minimum players.
    /// Only the classic controller (dlcrel below 2) has patches so far.
    /// </summary>
    public static class OnlineTest
    {
        public const string TriggerPrefix = "onlinetest_";

        /// <summary>The switches, in the order the page shows them.</summary>
        public static readonly string[] Features = { "inv", "looks", "phone", "death", "celeb", "time", "pick", "traffic", "pv", "mortal" };

        public static readonly string[] Controllers = { "fm_mission_controller", "fm_mission_controller_2020", "fm_mission_controller_v3" };

        private static volatile bool _enabled;
        private static volatile bool _invincible = true;
        private static volatile bool _ruleNotes = true;
        private static readonly HashSet<string> _off = new HashSet<string>();
        private static bool _loaded;

        public static bool Enabled
        {
            get { Load(); return _enabled; }
            set { Load(); _enabled = value; Save(); }
        }

        /// <summary>Invincible in the creator test, as the creator makes it (the default).</summary>
        public static bool Invincible
        {
            get { Load(); return _invincible; }
            set { Load(); _invincible = value; Save(); }
        }

        /// <summary>Show each team's rule change in the test (<see cref="RuleNotes"/>).</summary>
        public static bool RuleNotesOn
        {
            get { Load(); return _ruleNotes; }
            set { Load(); _ruleNotes = value; Save(); }
        }

        public static bool IsOn(string feature)
        {
            Load();
            lock (_off)
                return !_off.Contains(feature);
        }

        public static void Set(string feature, bool on)
        {
            Load();
            lock (_off)
            {
                if (on) _off.Remove(feature);
                else _off.Add(feature);
            }
            Save();
        }

        private static bool _inCreator;
        private static DateTime _inCreatorAt = DateTime.MinValue;

        /// <summary>
        /// LTS or Capture creator thread live: the only creators whose test this covers. Asks for
        /// the live thread, not GTA.CurrentCreatorName(): that one finds no creator while its test
        /// runs, exactly when the patches are needed (the creator thread goes on, it is what turns
        /// the phone off). Not the script program either: a program can outlive its thread.
        /// Cached for half a second, the patch loop asks every 50 ms for each switch.
        /// </summary>
        public static bool InCreator()
        {
            if ((DateTime.UtcNow - _inCreatorAt).TotalMilliseconds < 500)
                return _inCreator;
            bool found = false;
            try
            {
                found = GTA.IsScriptRunning("fm_lts_creator") || GTA.IsScriptRunning("fm_capture_creator");
            }
            catch
            {
            }
            _inCreator = found;
            _inCreatorAt = DateTime.UtcNow;
            return found;
        }

        /// <summary>Whether this switch's patches belong in the game right now.</summary>
        public static bool Active(string feature) =>
            (feature == "mortal" ? !Invincible : Enabled && IsOn(feature)) && MainWindow.m.IsProcOpen && InCreator();

        /// <summary>
        /// Called by the patch loop. With "phone" on and a test running: the creator leaves the
        /// phone state at 1 (switched off, func_5516(1)) whenever it is back in edit mode, and
        /// only its menu path turns it back to 3; the patch alone then helps only the first test.
        /// Does what that path does (state 1 to 3, cellphone_controller's normal "available").
        /// </summary>
        public static void Tick()
        {
            if (!Active("phone") || GTA.Offsets.Editor.phone_state == 0)
                return;
            if (!Controllers.Any(GTA.IsScriptRunning))
                return;
            var state = new Global(GTA.Offsets.Editor.phone_state);
            if (state.Get<int>() == 1)
                state.SetInt(3);
        }

        /// <summary>The mission controller running now, or null.</summary>
        public static string RunningController()
        {
            if (!MainWindow.m.IsProcOpen)
                return null;
            return Controllers.FirstOrDefault(c => ScrProgramScanner.GetScrProgramByName(c) != 0);
        }

        /// <summary>How many of the switch's patches are written into the running scripts.</summary>
        public static int AppliedCount(string feature)
        {
            var list = GTA.Editor.ScrPatches;
            if (list == null)
                return 0;
            return list.Count(p => p.trigger == TriggerPrefix + feature && ScrPatchesRunner.IsApplied(p));
        }

        public static bool HasPatches(string feature)
        {
            var list = GTA.Editor.ScrPatches;
            return list != null && list.Any(p => p.trigger == TriggerPrefix + feature);
        }

        private static void Load()
        {
            if (_loaded)
                return;
            _loaded = true;
            try
            {
                var ini = new ini_reader(Functions.getRoamingConfigFilePath());
                _enabled = ini.ReadBoolean("OnlineTest", "enabled", false);
                _invincible = ini.ReadBoolean("OnlineTest", "invincible", true);
                _ruleNotes = ini.ReadBoolean("OnlineTest", "rulenotes", true);
                string off = ini.ReadString("OnlineTest", "off", "");
                lock (_off)
                    foreach (string f in off.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        _off.Add(f.Trim());
            }
            catch (Exception ex)
            {
                Log.Debug("OnlineTest settings: " + ex.Message, source: "OnlineTest");
            }
        }

        private static void Save()
        {
            try
            {
                var ini = new ini_reader(Functions.getRoamingConfigFilePath());
                ini.Write("OnlineTest", "enabled", _enabled);
                ini.Write("OnlineTest", "invincible", _invincible);
                ini.Write("OnlineTest", "rulenotes", _ruleNotes);
                lock (_off)
                    ini.Write("OnlineTest", "off", string.Join(",", _off));
            }
            catch (Exception ex)
            {
                Log.Debug("OnlineTest settings: " + ex.Message, source: "OnlineTest");
            }
        }
    }
}
