using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// "Show rule changes in the game" (Mission › Test): a line in the game whenever a team reaches
    /// its next rule, which the creator test shows nowhere.
    ///
    /// The rule each team is on lives only in the controller's own statics
    /// (fm_mission_controller static mc_team_rule + 1 + team, Legacy Local_28722, Enhanced
    /// Local_29326; the controller's server data, so the tester is the host and reads it live). It
    /// counts up to the team's rule count once every rule is done. Xenvious reads it from the
    /// controller thread and keeps up to <see cref="MaxLines"/> lines for <see cref="ShowFor"/>; the
    /// payload in the creator (ruleticker.py in ysc-global-updater) draws them every frame bottom
    /// left above the radar, one box per line as wide as its text, like the game's tickers.
    /// Only the classic controller (dlcrel below 2), the one LTS and Capture jobs run.
    /// </summary>
    public static class RuleNotes
    {
        private const string Controller = "fm_mission_controller";
        private static readonly string[] Creators = { "fm_lts_creator", "fm_capture_creator" };

        // ruleticker.py: the line count in the data page, the lines in the drawer space.
        private const int CountAt = 0x3780, LinesAt = 0xF800, LineBytes = 0x80, MaxLines = 4;
        private static readonly TimeSpan ShowFor = TimeSpan.FromSeconds(6);
        // The controller's rule numbers stop below this (FMMC_MAX_RULES).
        private const int MaxRules = 17;
        // Game colour codes for teams 1 to 4 (orange, green, pink, purple, as the creators colour them).
        private static readonly string[] TeamColour = { "~o~", "~g~", "~q~", "~p~" };

        private static int[] _last;
        private static readonly List<(string Text, DateTime Until)> _lines = new List<(string, DateTime)>();
        private static string _written = "";

        /// <summary>Called by the patch loop (every 250 ms).</summary>
        public static void Tick()
        {
            string creator = null;
            long[] thread = null;
            if (OnlineTest.RuleNotesOn && ScriptVars.FeaturesOn && GTA.Offsets.Editor.mc_team_rule != 0 && MainWindow.m.IsProcOpen)
            {
                creator = Creators.FirstOrDefault(GTA.IsScriptRunning);
                // The controller keeps running in the creator between two tests, so only a
                // running test counts; leaving it makes the next test start fresh.
                bool testing = MainWindow.Instance?.CurrentGameState == MainWindow.GameState.Testing;
                thread = creator == null || !testing ? null : GTA.getLocalScriptAddy(Controller);
            }
            if (thread == null)
            {
                _last = null;
                _lines.Clear();
                if (creator != null)
                    Write(creator);
                return;
            }

            int teams = Math.Max(1, Math.Min(Rules.Teams(), 4));
            var now = new int[teams];
            for (int t = 0; t < teams; t++)
                now[t] = ReadStatic(thread, GTA.Offsets.Editor.mc_team_rule + 1 + t);

            // A new test: every team at the rule it starts on (later than the first when the test
            // starts from a checkpoint).
            bool start = _last == null || _last.Length != teams;
            for (int t = 0; t < teams; t++)
            {
                if (now[t] < 0)
                    continue;
                if (start)
                {
                    Add(Line(t, -1, now[t]));
                }
                else if (now[t] != _last[t])
                {
                    Add(Line(t, _last[t], now[t]));
                }
            }
            _last = now;
            _lines.RemoveAll(l => l.Until <= DateTime.UtcNow);
            Write(creator);
        }

        private static void Add(string line)
        {
            _lines.Add((line, DateTime.UtcNow + ShowFor));
            if (_lines.Count > MaxLines)
                _lines.RemoveAt(0);
            Log.Debug("rule note: " + line, source: "RuleNotes");
        }

        private static int ReadStatic(long[] thread, long index)
        {
            try
            {
                return MainWindow.m.memory(thread[0], new long[] { thread[1], GTA.Offsets.Editor.OFFSET_script_local_start, index * 8 }).Get<int>();
            }
            catch
            {
                return -1;
            }
        }

        // "Team 1  Rule 1 done > Rule 2: <what the team reads>", or that every rule is done.
        private static string Line(int team, int from, int to)
        {
            string head = TeamColour[team] + string.Format(CultureInfo.CurrentCulture, T("rn_team", "Team {0}"), team + 1) + "~s~  ";
            int count = Rules.Count(team);
            if (to >= MaxRules || (count > 0 && to >= count))
                return head + "~g~" + T("rn_all_done", "every rule done") + "~s~";
            string text = (Rules.Text(team, to) ?? "").Trim();
            string next = string.Format(CultureInfo.CurrentCulture, T("rn_rule", "Rule {0}"), to + 1) + (text.Length > 0 ? ": " + text + "~s~" : "");
            if (from < 0)
                return head + next;
            return head + "~g~" + string.Format(CultureInfo.CurrentCulture, T("rn_done", "Rule {0} done"), from + 1) + "~s~ > " + next;
        }

        // Count to 0 first, so the payload never draws a line being rewritten.
        private static void Write(string creator)
        {
            string state = string.Join("\n", _lines.Select(l => l.Text));
            if (state == _written)
                return;
            var space = ScrPatchesRunner.PageReady(creator) ? ScriptSpace.Get(creator) : null;
            if (space == null || space.Data == 0 || space.Extra == 0)
                return;
            try
            {
                MainWindow.m.memory((space.Data + CountAt).ToString("X")).SetBytes(BitConverter.GetBytes(0L));
                if (_lines.Count > 0)
                {
                    var block = new byte[_lines.Count * LineBytes];
                    for (int i = 0; i < _lines.Count; i++)
                    {
                        byte[] b = Bytes(_lines[i].Text);
                        Array.Copy(b, 0, block, i * LineBytes, b.Length);
                    }
                    MainWindow.m.memory((space.Extra + LinesAt).ToString("X")).SetBytes(block);
                    MainWindow.m.memory((space.Data + CountAt).ToString("X")).SetBytes(BitConverter.GetBytes((long)_lines.Count));
                }
                _written = state;
            }
            catch (Exception ex)
            {
                Log.Debug("rule notes: " + ex.Message, source: "RuleNotes");
            }
        }

        // UTF-8, cut at a character border so the last byte of the line always stays NUL.
        private static byte[] Bytes(string text)
        {
            byte[] all = Encoding.UTF8.GetBytes(text);
            int n = Math.Min(all.Length, LineBytes - 1);
            while (n > 0 && n < all.Length && (all[n] & 0xC0) == 0x80)
                n--;
            return all.Take(n).ToArray();
        }

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;
    }
}
