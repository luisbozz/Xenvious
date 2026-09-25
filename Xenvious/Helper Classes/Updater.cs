using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
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
    }

    /// <summary>
    /// Updates Xenvious from the latest GitHub release: the release must carry
    /// Xenvious.exe and Xenvious.exe.sha256. The new exe is downloaded next to the running
    /// one, checked against the hash and swapped in by renaming: Windows lets a running
    /// exe be renamed, not overwritten. Nothing here needs a token; the GitHub API allows
    /// 60 unauthenticated requests per hour and IP, which is plenty for one check per start.
    /// </summary>
    public static class Updater
    {
        public const string ExeAsset = "Xenvious.exe";
        public const string ChecksumAsset = "Xenvious.exe.sha256";

        private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

        // No client-wide timeout: the check has its own, a download may take a while.
        private static readonly HttpClient Http = new HttpClient { Timeout = System.Threading.Timeout.InfiniteTimeSpan };

        /// <summary>The running version, compared on three parts (2.71.11.0 is v2.71.11).</summary>
        public static Version CurrentVersion => ThreeParts(Assembly.GetExecutingAssembly().GetName().Version);

        private static string ExePath => Process.GetCurrentProcess().MainModule.FileName;

        private static Version ThreeParts(Version v) => new Version(v.Major, v.Minor, Math.Max(0, v.Build));

        public static bool TryParseTag(string tag, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(tag))
                return false;
            if (!Version.TryParse(tag.Trim().TrimStart('v', 'V'), out var parsed))
                return false;
            version = ThreeParts(parsed);
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
        /// The latest release if it is newer and complete, otherwise null. Being offline,
        /// the rate limit or a release without the two files only end up in the log.
        /// </summary>
        public static Task<UpdateInfo> CheckAsync() => CheckAsync(settings.UpdateOwner, settings.UpdateRepo);

        internal static async Task<UpdateInfo> CheckAsync(string owner, string repo)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            try
            {
                using (var timeout = new CancellationTokenSource(CheckTimeout))
                using (var request = Request(url))
                using (var response = await Http.SendAsync(request, timeout.Token).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Info($"update check: HTTP {(int)response.StatusCode}", source: "updater");
                        return null;
                    }

                    var release = JObject.Parse(body);
                    string tag = (string)release["tag_name"];
                    if (!TryParseTag(tag, out var version))
                    {
                        Log.Info($"update check: tag '{tag}' is no version", source: "updater");
                        return null;
                    }
                    if (version <= CurrentVersion)
                    {
                        Log.Info($"update check: {CurrentVersion} is current (latest {tag})", source: "updater");
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
                    };
                    if (info.ExeUrl == null || info.ChecksumUrl == null)
                    {
                        Log.Warn($"update check: release {tag} lacks {ExeAsset} or {ChecksumAsset}", source: "updater");
                        return null;
                    }
                    Log.Info($"update check: {tag} available (running {CurrentVersion})", source: "updater");
                    return info;
                }
            }
            catch (Exception ex)
            {
                Log.Info("update check failed: " + ex.Message, source: "updater");
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
