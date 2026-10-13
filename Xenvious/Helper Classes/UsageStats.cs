using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Anonymous usage statistics, on unless switched off in Settings. Counts per day which
    /// pages and actions were used, how long Xenvious and each creator were open, and sends
    /// that to xenvious.com a few times a day. The only identifier is a random install ID;
    /// nothing about the job, the player or the PC is counted (see xenvious.com/privacy).
    ///
    /// Counts live in %AppData%\Xenvious\usage.json until the server has them, so days
    /// without a connection are sent later. The running day is resent as it grows; the
    /// server replaces what it had for that day.
    /// </summary>
    public static class UsageStats
    {
        private const string DefaultEndpoint = "https://xenvious.com/api/metrics";
        private const string SettingEnabled = "usage_stats";
        private const string SettingInstallId = "usage_id";
        // Not in the UI: points the reports at a test server, e.g. http://localhost:8080/api/metrics.
        private const string SettingEndpoint = "usage_endpoint";
        private const int MaxDays = 14;         // the server takes at most 14 days per report
        private static readonly TimeSpan FirstSend = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan SendEvery = TimeSpan.FromHours(3);
        private static readonly TimeSpan ExitSendTimeout = TimeSpan.FromSeconds(3);

        // The server accepts lower case names of up to 48 characters.
        private static readonly Regex NameChars = new Regex("[^a-z0-9_.-]+", RegexOptions.Compiled);

        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        private static readonly object Gate = new object();
        private static Dictionary<string, Day> days = new Dictionary<string, Day>();
        private static bool dirty;
        private static bool enabled;
        private static string installId = "";
        private static string endpoint = DefaultEndpoint;
        private static Timer timer;
        private static DateTime nextSend;
        private static int sending;

        private sealed class Day
        {
            [JsonProperty("minutes")] public int Minutes;
            [JsonProperty("pages")] public Dictionary<string, int> Pages = new Dictionary<string, int>();
            [JsonProperty("actions")] public Dictionary<string, int> Actions = new Dictionary<string, int>();
            [JsonProperty("creators")] public Dictionary<string, int> Creators = new Dictionary<string, int>();
        }

        private static string StorePath =>
            Path.Combine(Path.GetDirectoryName(Functions.getRoamingConfigFilePath()), "usage.json");

        /// <summary>Whether statistics are collected and sent (Settings, on by default).</summary>
        public static bool Enabled => enabled;

        /// <summary>The random ID the reports carry, shown in Settings for data requests.</summary>
        public static string InstallId => installId;

        /// <summary>Reads the setting and the counts kept so far, counts this start and starts the timer.</summary>
        public static void Start()
        {
            var ini = new ini_reader(Functions.getRoamingConfigFilePath());
            installId = ini.ReadString("Settings", SettingInstallId, "");
            if (!Guid.TryParse(installId, out _))
            {
                installId = Guid.NewGuid().ToString("D");
                ini.Write("Settings", SettingInstallId, installId);
            }
            enabled = ini.ReadBoolean("Settings", SettingEnabled, true);
            string custom = ini.ReadString("Settings", SettingEndpoint, "");
            if (Uri.TryCreate(custom, UriKind.Absolute, out _))
                endpoint = custom;
            if (enabled)
                Load();
            Action("app.start");
            nextSend = DateTime.UtcNow + FirstSend;
            timer = new Timer(_ => Tick(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        /// <summary>Switches statistics on or off. Off also forgets the counts not sent yet.</summary>
        public static void SetEnabled(bool on)
        {
            new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", SettingEnabled, on);
            lock (Gate)
            {
                enabled = on;
                days.Clear();
                dirty = false;
            }
            if (!on)
            {
                try { File.Delete(StorePath); }
                catch (Exception ex) { Log.Debug("usage stats: " + ex.Message, source: "usage"); }
            }
            Log.Info("usage statistics " + (on ? "on" : "off"), source: "usage");
        }

        /// <summary>A page was opened, e.g. "page.copyjobs" or "edit.race.checkpoints".</summary>
        public static void Page(string name) => Count(d => d.Pages, name);

        /// <summary>An action was done, e.g. "creator.rebuild".</summary>
        public static void Action(string name) => Count(d => d.Actions, name);

        /// <summary>A patch was switched on or off by hand; counted under its name.</summary>
        public static void Patch(string patchName, bool on) => Action((on ? "patch.on." : "patch.off.") + patchName);

        /// <summary>An unhandled exception: only its type is counted, never message or stack.</summary>
        public static void Crash(Exception ex)
        {
            Action("error.crash." + (ex?.GetType().Name ?? "unknown"));
            Save();
        }

        /// <summary>
        /// When Xenvious closes: saves, then sends once, waiting at most a few seconds so
        /// closing does not hang. The day is complete on the server at once instead of with
        /// the next start; whatever does not get through goes with that start as before.
        /// </summary>
        public static void SaveAndSend()
        {
            Save();
            try
            {
                using (var cancel = new CancellationTokenSource(ExitSendTimeout))
                    SendAsync(cancel.Token).Wait(ExitSendTimeout);
            }
            catch (Exception ex)
            {
                Log.Debug("usage stats exit send: " + ex.Message, source: "usage");
            }
        }

        public static void Save()
        {
            string json;
            lock (Gate)
            {
                if (!enabled || !dirty)
                    return;
                json = JsonConvert.SerializeObject(new { days });
                dirty = false;
            }
            try
            {
                File.WriteAllText(StorePath, json, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Log.Debug("usage stats save: " + ex.Message, source: "usage");
            }
        }

        private static void Count(Func<Day, Dictionary<string, int>> pick, string name)
        {
            string key = Key(name);
            if (key == "")
                return;
            lock (Gate)
            {
                if (!enabled)
                    return;
                var map = pick(Today());
                map.TryGetValue(key, out int n);
                map[key] = n + 1;
                dirty = true;
            }
        }

        // Lower case, other characters as "_", at most 48 long: what the server accepts.
        private static string Key(string name)
        {
            string key = NameChars.Replace((name ?? "").ToLowerInvariant(), "_").Trim('_', '.', '-');
            return key.Length > 48 ? key.Substring(0, 48).TrimEnd('_', '.', '-') : key;
        }

        private static Day Today()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (!days.TryGetValue(today, out var day))
                days[today] = day = new Day();
            return day;
        }

        private static void Load()
        {
            try
            {
                if (!File.Exists(StorePath))
                    return;
                var stored = JsonConvert.DeserializeAnonymousType(File.ReadAllText(StorePath, Encoding.UTF8),
                    new { days = new Dictionary<string, Day>() });
                lock (Gate)
                    days = Newest(stored?.days ?? new Dictionary<string, Day>());
            }
            catch (Exception ex)
            {
                Log.Debug("usage stats load: " + ex.Message, source: "usage");
            }
        }

        private static Dictionary<string, Day> Newest(Dictionary<string, Day> all) =>
            all.Where(p => p.Value != null && DateTime.TryParseExact(p.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
               .OrderByDescending(p => p.Key, StringComparer.Ordinal)
               .Take(MaxDays)
               .ToDictionary(p => p.Key, p => p.Value);

        // Once a minute: the minute counts for the day and for the creator Xenvious is in.
        private static void Tick()
        {
            try
            {
                if (!enabled)
                    return;
                string creator = "";
                bool attached = MainWindow.m != null && MainWindow.m.IsProcOpen;
                if (attached)
                    creator = CreatorKey(GTA.CurrentCreatorName());
                lock (Gate)
                {
                    var day = Today();
                    day.Minutes = Math.Min(day.Minutes + 1, 1440);
                    if (attached)
                    {
                        day.Actions.TryGetValue("game.attached", out int a);
                        day.Actions["game.attached"] = a + 1;
                    }
                    if (creator != "")
                    {
                        day.Creators.TryGetValue(creator, out int c);
                        day.Creators[creator] = c + 1;
                    }
                    dirty = true;
                }
                Save();
                if (DateTime.UtcNow >= nextSend)
                {
                    nextSend = DateTime.UtcNow + SendEvery;
                    _ = SendAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Debug("usage stats tick: " + ex.Message, source: "usage");
            }
        }

        private static string CreatorKey(string script)
        {
            switch (script)
            {
                case "fm_race_creator": return "race";
                case "fm_lts_creator": return "lts";
                case "fm_capture_creator": return "capture";
                case "fm_deathmatch_creator": return "deathmatch";
                case "fm_survival_creator": return "survival";
                case "public_mission_creator": return "mission";
                default: return "";
            }
        }

        private static async Task SendAsync(CancellationToken cancel = default)
        {
            if (Interlocked.Exchange(ref sending, 1) == 1)
                return;
            try
            {
                string body;
                string today = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                lock (Gate)
                {
                    if (!enabled || days.Count == 0)
                        return;
                    body = JsonConvert.SerializeObject(new
                    {
                        install_id = installId,
                        version = Updater.CurrentVersion.ToString(),
                        edition = GameVariant.IsEnhanced ? "enhanced" : "legacy",
                        language = MainWindow.Instance?.LanguageCode ?? Translation._Language.DefaultCode,
                        days = days.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new
                        {
                            day = p.Key,
                            minutes = p.Value.Minutes,
                            pages = p.Value.Pages,
                            actions = p.Value.Actions,
                            creators = p.Value.Creators,
                        }),
                    });
                }

                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    request.Headers.UserAgent.ParseAdd("Xenvious/" + Updater.CurrentVersion);
                    using (var response = await Http.SendAsync(request, cancel).ConfigureAwait(false))
                    {
                        // Sent, or refused for good (400): either way those days are done.
                        // Only the running day stays, it is sent again as it grows.
                        bool done = response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest;
                        if (!response.IsSuccessStatusCode)
                            Log.Debug($"usage stats: HTTP {(int)response.StatusCode}", source: "usage");
                        if (done)
                        {
                            lock (Gate)
                            {
                                foreach (string key in days.Keys.Where(k => string.CompareOrdinal(k, today) < 0).ToList())
                                    days.Remove(key);
                                if (response.StatusCode == HttpStatusCode.BadRequest)
                                    days.Remove(today);
                                dirty = true;
                            }
                            Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Offline or the server is down: the days stay for the next try.
                Log.Debug("usage stats send: " + ex.Message, source: "usage");
            }
            finally
            {
                Interlocked.Exchange(ref sending, 0);
            }
        }
    }
}
