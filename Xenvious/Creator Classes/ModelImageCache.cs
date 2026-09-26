using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Preview pictures for the model catalog. A picture is downloaded the first time it is
    /// shown, shrunk to a thumbnail and kept in %AppData%\Xenvious\cache\models, so the next
    /// time it comes from disk. Props and dynamic props use the same pictures. The cache can be
    /// switched off and cleared under Settings.
    /// </summary>
    public static class ModelImageCache
    {
        private const string ConfigSection = "MODELCACHE";
        private const int ThumbWidth = 192;
        // A thumbnail is about this big on disk; used for the estimate in Settings.
        public const long AverageThumbBytes = 9 * 1024;

        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        // Few parallel downloads: scrolling through the catalog should not flood the source.
        private static readonly SemaphoreSlim Downloads = new SemaphoreSlim(4);
        private static readonly Dictionary<string, ImageSource> Memory = new Dictionary<string, ImageSource>();
        private static readonly HashSet<string> Missing = new HashSet<string>();
        private static bool? _enabled;

        public static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "cache", "models");

        /// <summary>Whether thumbnails are kept on disk (config.ini, on by default).</summary>
        public static bool Enabled
        {
            get
            {
                if (_enabled == null)
                {
                    try
                    {
                        string path = Functions.getRoamingConfigFilePath();
                        string value = File.Exists(path) ? new ini_reader(path).ReadString(ConfigSection, "enabled") : "";
                        _enabled = value != "0";
                    }
                    catch
                    {
                        _enabled = true;
                    }
                }
                return _enabled.Value;
            }
            set
            {
                _enabled = value;
                try
                {
                    new ini_reader(Functions.getRoamingConfigFilePath()).Write(ConfigSection, "enabled", value ? "1" : "0");
                }
                catch (Exception ex)
                {
                    Log.Warn("model cache: setting not saved: " + ex.Message);
                }
            }
        }

        /// <summary>Where a prop's picture can be found online. Actors have no source yet.</summary>
        private static string SourceUrl(string kind, string name, uint hash)
        {
            return kind == "prop" ? $"https://cdn.rage.mp/public/odb/imgs/{name}-{hash}.jpg" : null;
        }

        /// <summary>The thumbnail, or null when there is none (no source, offline, unknown model).</summary>
        public static async Task<ImageSource> GetAsync(string kind, string name, uint hash)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            string key = kind + "/" + name;
            lock (Memory)
            {
                if (Memory.TryGetValue(key, out var known))
                    return known;
                if (Missing.Contains(key))
                    return null;
            }

            string file = Path.Combine(Folder, kind, Sanitize(name) + ".jpg");
            ImageSource image = null;
            try
            {
                if (File.Exists(file))
                    image = Load(File.ReadAllBytes(file));
                else
                    image = await DownloadAsync(kind, name, hash, file);
            }
            catch (Exception ex)
            {
                Log.Debug($"model cache: {key}: {ex.Message}", source: "catalog");
            }

            lock (Memory)
            {
                if (image != null)
                    Memory[key] = image;
                else
                    Missing.Add(key);
            }
            return image;
        }

        private static async Task<ImageSource> DownloadAsync(string kind, string name, uint hash, string file)
        {
            string url = SourceUrl(kind, name, hash);
            if (url == null)
                return null;

            await Downloads.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var response = await Http.GetAsync(url).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                        return null;
                    byte[] full = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    byte[] thumb = Shrink(full);
                    if (Enabled)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                        File.WriteAllBytes(file, thumb);
                    }
                    return Load(thumb);
                }
            }
            finally
            {
                Downloads.Release();
            }
        }

        private static byte[] Shrink(byte[] picture)
        {
            var source = new BitmapImage();
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.DecodePixelWidth = ThumbWidth;
            source.StreamSource = new MemoryStream(picture);
            source.EndInit();
            var encoder = new JpegBitmapEncoder { QualityLevel = 80 };
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        private static ImageSource Load(byte[] data)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(data);
            image.EndInit();
            image.Freeze();   // created off the UI thread, shown on it
            return image;
        }

        private static string Sanitize(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        public static (int Files, long Bytes) Usage()
        {
            try
            {
                if (!Directory.Exists(Folder))
                    return (0, 0);
                var files = new DirectoryInfo(Folder).GetFiles("*.jpg", SearchOption.AllDirectories);
                return (files.Length, files.Sum(f => f.Length));
            }
            catch
            {
                return (0, 0);
            }
        }

        public static void Clear()
        {
            try
            {
                if (Directory.Exists(Folder))
                    Directory.Delete(Folder, true);
            }
            catch (Exception ex)
            {
                Log.Warn("model cache: not cleared: " + ex.Message);
            }
            lock (Memory)
            {
                Memory.Clear();
                Missing.Clear();
            }
        }
    }
}
