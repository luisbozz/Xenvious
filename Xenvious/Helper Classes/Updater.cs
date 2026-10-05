using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>A release on GitHub that is newer than the running Xenvious.</summary>
    public sealed class UpdateInfo
    {
        public Version Version { get; set; }
        public string Tag { get; set; }
        public string Notes { get; set; }
        public string ExeUrl { get; set; }
        public string ChecksumUrl { get; set; }
        public bool Nightly { get; set; }
    }

    /// <summary>
    /// Updates Xenvious from the latest GitHub release: the release must carry
    /// Xenvious.exe and Xenvious.exe.sha256. The new exe is downloaded next to the running
    /// one, checked against the hash and swapped in by renaming: Windows lets a running
    /// exe be renamed, not overwritten. Nothing here needs a token; the GitHub API allows
    /// 60 unauthenticated requests per hour and IP, which is plenty for one check per start.
    ///
    /// Nightly builds (opt-in) sit in one pre-release with the tag "nightly", which the
    /// nightly workflow replaces each time. Their version has a fourth part, the run number
    /// (1.73.3.12): newer than release 1.73.3, older than the next release 1.73.4, so a nightly
    /// user moves on to that release by itself. Pre-releases never show up as "latest".
    /// </summary>
    public static class Updater
    {
        public const string ExeAsset = "Xenvious.exe";
        public const string ChecksumAsset = "Xenvious.exe.sha256";
        public const string NightlyTag = "nightly";

        private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

        // No client-wide timeout: the check has its own, a download may take a while.
        private static readonly HttpClient Http = new HttpClient { Timeout = System.Threading.Timeout.InfiniteTimeSpan };

        /// <summary>
        /// The running version: three parts for a release (1.73.3.0 is v1.73.3), four for a
        /// nightly build (1.73.3.12). As a Version, 1.73.3 is older than 1.73.3.12.
        /// </summary>
        public static Version CurrentVersion => Normalize(Assembly.GetExecutingAssembly().GetName().Version);

        /// <summary>True for a nightly build (its version has a fourth part).</summary>
        public static bool IsNightly => CurrentVersion.Revision > 0;

        private static string ExePath => Process.GetCurrentProcess().MainModule.FileName;

        private static Version Normalize(Version v) => v.Revision > 0
            ? new Version(v.Major, v.Minor, Math.Max(0, v.Build), v.Revision)
            : new Version(v.Major, v.Minor, Math.Max(0, v.Build));

        public static bool TryParseTag(string tag, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(tag))
                return false;
            if (!Version.TryParse(tag.Trim().TrimStart('v', 'V'), out var parsed))
                return false;
            version = Normalize(parsed);
            return true;
        }

        private static HttpRequestMessage Request(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // GitHub rejects API requests without a User-Agent.
            request.Headers.TryAddWithoutValidation("User-Agent", "Xenvious/" + CurrentVersion);
            request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
            return request;
        }

        /// <summary>
        /// The newest build the player may get, if it is newer than the running one and
        /// complete, otherwise null: the latest release, and with <paramref name="nightly"/>
        /// also the nightly build. Being offline, the rate limit or a release without the two
        /// files only end up in the log.
        /// </summary>
        public static async Task<UpdateInfo> CheckAsync(bool nightly)
        {
            var release = await FetchAsync("latest").ConfigureAwait(false);
            var build = nightly ? await FetchAsync("tags/" + NightlyTag).ConfigureAwait(false) : null;
            var newest = build != null && (release == null || build.Version > release.Version) ? build : release;
            if (newest == null)
                return null;
            if (newest.Version <= CurrentVersion)
            {
                Log.Info($"update check: {CurrentVersion} is current (newest {newest.Version})", source: "updater");
                return null;
            }
            Log.Info($"update check: {newest.Version}{(newest.Nightly ? " (nightly)" : "")} available (running {CurrentVersion})", source: "updater");
            return newest;
        }

        /// <summary>
        /// The latest release whatever the running version, for going back from a nightly
        /// build to the release. Null when it cannot be read.
        /// </summary>
        public static Task<UpdateInfo> LatestReleaseAsync() => FetchAsync("latest");

        // One release from the GitHub API ("latest" or "tags/<tag>"), complete with both files.
        private static async Task<UpdateInfo> FetchAsync(string which)
        {
            string url = $"https://api.github.com/repos/{settings.UpdateOwner}/{settings.UpdateRepo}/releases/{which}";
            try
            {
                using (var timeout = new CancellationTokenSource(CheckTimeout))
                using (var request = Request(url))
                using (var response = await Http.SendAsync(request, timeout.Token).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Info($"update check ({which}): HTTP {(int)response.StatusCode}", source: "updater");
                        return null;
                    }

                    var release = JObject.Parse(body);
                    string tag = (string)release["tag_name"];
                    bool nightly = tag == NightlyTag;
                    // The nightly release keeps its tag; its version is in the title ("Nightly 1.73.3.12").
                    string named = nightly ? Regex.Match((string)release["name"] ?? "", @"\d+\.\d+\.\d+\.\d+").Value : tag;
                    if (!TryParseTag(named, out var version))
                    {
                        Log.Info($"update check: release '{tag}' has no version", source: "updater");
                        return null;
                    }

                    var assets = (release["assets"] as JArray ?? new JArray()).OfType<JObject>().ToList();
                    string Asset(string name) => (string)assets.FirstOrDefault(a => (string)a["name"] == name)?["browser_download_url"];
                    var info = new UpdateInfo
                    {
                        Version = version,
                        Tag = tag,
                        Notes = (string)release["body"] ?? "",
                        ExeUrl = Asset(ExeAsset),
                        ChecksumUrl = Asset(ChecksumAsset),
                        Nightly = nightly,
                    };
                    if (info.ExeUrl == null || info.ChecksumUrl == null)
                    {
                        Log.Warn($"update check: release {tag} lacks {ExeAsset} or {ChecksumAsset}", source: "updater");
                        return null;
                    }
                    return info;
                }
            }
            catch (Exception ex)
            {
                Log.Info($"update check ({which}) failed: " + ex.Message, source: "updater");
                return null;
            }
        }

        /// <summary>
        /// Downloads the new exe next to the running one and checks it against the
        /// published hash. Returns the downloaded file; a wrong hash deletes it and throws.
        /// </summary>
        public static async Task<string> DownloadAsync(UpdateInfo info, IProgress<double> progress, CancellationToken cancel)
        {
            string target = ExePath + ".download";
            try
            {
                string published;
                using (var request = Request(info.ChecksumUrl))
                using (var response = await Http.SendAsync(request, cancel).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    published = (await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                        .Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                }

                using (var request = Request(info.ExeUrl))
                using (var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    long? total = response.Content.Headers.ContentLength;
                    using (var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var file = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[81920];
                        long done = 0;
                        int read;
                        while ((read = await source.ReadAsync(buffer, 0, buffer.Length, cancel).ConfigureAwait(false)) > 0)
                        {
                            await file.WriteAsync(buffer, 0, read, cancel).ConfigureAwait(false);
                            done += read;
                            if (total > 0)
                                progress?.Report((double)done / total.Value);
                        }
                    }
                }

                string actual;
                using (var sha = SHA256.Create())
                using (var file = File.OpenRead(target))
                    actual = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
                if (!string.Equals(actual, published, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"SHA256 mismatch: expected {published}, got {actual}");

                Log.Info($"update {info.Tag} downloaded and verified", source: "updater");
                return target;
            }
            catch
            {
                TryDelete(target);
                throw;
            }
        }

        /// <summary>
        /// Puts the downloaded exe in place of the running one and starts it. The caller
        /// shuts Xenvious down afterwards. The old exe stays as .old until the next start.
        /// </summary>
        public static void SwapAndRestart(string download)
        {
            string exe = ExePath;
            string old = exe + ".old";
            TryDelete(old);
            File.Move(exe, old);
            try
            {
                File.Move(download, exe);
            }
            catch
            {
                File.Move(old, exe);
                throw;
            }
            Log.Info("update installed, restarting", source: "updater");
            Process.Start(exe);
        }

        /// <summary>Removes what an earlier update left next to the exe.</summary>
        public static void DeleteLeftovers()
        {
            TryDelete(ExePath + ".old");
            TryDelete(ExePath + ".download");
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Debug($"could not delete {path}: {ex.Message}", source: "updater");
            }
        }
    }
}
