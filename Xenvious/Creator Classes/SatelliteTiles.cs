using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Top-down map tiles of the Pleb Masters Forge map viewer (forge.plebmasters.de/map, layer
    /// "Realmap", a render of the game world): 256 px tiles at
    /// maps.plebmasters.de/gta5/realmap/M{z}/mapC_{x}_{y}.png, zoom 0-8. The viewer's "big" Leaflet
    /// CRS (CRS.Simple, Transformation(0.04444, 157.94, -0.04444, 341.66)) puts world (x, y) at
    /// pixel (2^z * (0.04444 x + 157.94), 2^z * (341.66 - 0.04444 y)); at zoom 8 that is about
    /// 11 pixels per metre. (Their "Satellite" layer only reaches zoom 6, about 2 pixels per metre.) Tiles are kept in memory and in %AppData%\Xenvious\maptiles so
    /// each one is downloaded once; a tile that fails is not asked for again this session.
    /// </summary>
    public static class SatelliteTiles
    {
        public const int MaxZoom = 8;
        public const int TileSize = 256;
        private const double Scale = 0.04444, OffsetX = 157.94, OffsetY = 341.66;

        private static readonly HttpClient Http = CreateClient();
        private static readonly ConcurrentDictionary<string, ImageSource> Loaded = new ConcurrentDictionary<string, ImageSource>();
        private static readonly ConcurrentDictionary<string, bool> Pending = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<string, bool> Failed = new ConcurrentDictionary<string, bool>();

        /// <summary>Raised on a background thread when a tile has arrived.</summary>
        public static event Action TileLoaded;

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Xenvious");
            client.DefaultRequestHeaders.Referrer = new Uri("https://forge.plebmasters.de/");
            return client;
        }

        /// <summary>Tile pixel (at zoom z) of a world position.</summary>
        public static (double X, double Y) ToPixel(double x, double y, int z)
        {
            double s = Math.Pow(2, z);
            return (s * (Scale * x + OffsetX), s * (OffsetY - Scale * y));
        }

        /// <summary>World position of a tile pixel (at zoom z).</summary>
        public static (double X, double Y) ToWorld(double px, double py, int z)
        {
            double s = Math.Pow(2, z);
            return ((px / s - OffsetX) / Scale, (OffsetY - py / s) / Scale);
        }

        /// <summary>World metres per tile pixel at zoom z.</summary>
        public static double MetresPerPixel(int z) => 1 / (Scale * Math.Pow(2, z));

        /// <summary>The tile if it is loaded; otherwise starts loading it and returns null.</summary>
        public static ImageSource Get(int z, int x, int y)
        {
            string key = z + "/" + x + "_" + y;
            if (Loaded.TryGetValue(key, out var image))
                return image;
            if (x < 0 || y < 0 || x >= (1 << z) * 4 || y >= (1 << z) * 4 || Failed.ContainsKey(key) || !Pending.TryAdd(key, true))
                return null;
            Task.Run(() => Load(key, z, x, y));
            return null;
        }

        private static async Task Load(string key, int z, int x, int y)
        {
            try
            {
                string file = Path.Combine(Path.GetDirectoryName(Functions.getRoamingConfigFilePath()), "maptiles", "realmap", z.ToString(), x + "_" + y + ".png");
                byte[] data;
                if (File.Exists(file))
                {
                    data = File.ReadAllBytes(file);
                }
                else
                {
                    data = await Http.GetByteArrayAsync($"https://maps.plebmasters.de/gta5/realmap/M{z}/mapC_{x}_{y}.png").ConfigureAwait(false);
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                    File.WriteAllBytes(file, data);
                }
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(data))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                bitmap.Freeze();
                Loaded[key] = bitmap;
                TileLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                // Outside the map (404) or offline: the caller keeps its plain background.
                Failed[key] = true;
                Log.Debug($"map tile {key}: {ex.Message}", source: "SatelliteTiles");
            }
            finally
            {
                Pending.TryRemove(key, out _);
            }
        }
    }
}
