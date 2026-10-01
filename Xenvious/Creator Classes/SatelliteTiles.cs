using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        /// <summary>The map under Xenvious' top views; off in config.ini ("realmap" = 0) leaves them plain.</summary>
        public static bool Enabled
        {
            get => new ini_reader(Functions.getRoamingConfigFilePath()).ReadInteger("Settings", "realmap", 1) == 1;
            set => new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "realmap", value ? 1 : 0);
        }

        /// <summary>
        /// Adds the map under a top view drawn north up as screen = (w/2 + (x - cx) * scale,
        /// h/2 - (y - cy) * scale), scale in pixels per metre: the loaded tiles, a dark wash so
        /// the drawing on top stays readable, and the credit. Call it right after clearing the
        /// canvas so everything drawn later lies on top. Tiles still loading are requested and
        /// raise <see cref="TileLoaded"/>; returns false when none is there (plain background).
        /// </summary>
        public static bool Draw(Canvas canvas, double w, double h, double cx, double cy, double scale, double wash = 0x40 / 255.0)
        {
            if (!Enabled || w <= 0 || h <= 0 || scale <= 0 || double.IsNaN(cx) || double.IsNaN(cy))
                return false;
            int z = MaxZoom;
            // Coarser tiles for a wide view: no more than about two tile pixels per screen pixel.
            while (z > 0 && MetresPerPixel(z) * scale < 0.5)
                z--;
            var tl = ToPixel(cx - w / 2 / scale, cy + h / 2 / scale, z);
            var br = ToPixel(cx + w / 2 / scale, cy - h / 2 / scale, z);
            int x0 = (int)Math.Floor(tl.X / TileSize), x1 = (int)Math.Floor(br.X / TileSize);
            int y0 = (int)Math.Floor(tl.Y / TileSize), y1 = (int)Math.Floor(br.Y / TileSize);
            // A view far too wide for the zoom levels would ask for hundreds of tiles.
            if ((x1 - x0 + 1) * (y1 - y0 + 1) > 64)
                return false;
            double size = TileSize * MetresPerPixel(z) * scale;
            bool any = false;
            for (int ty = y0; ty <= y1; ty++)
                for (int tx = x0; tx <= x1; tx++)
                {
                    var tile = Get(z, tx, ty);
                    if (tile == null)
                        continue;
                    var corner = ToWorld(tx * TileSize, ty * TileSize, z);
                    var image = new Image { Source = tile, Width = size + 0.5, Height = size + 0.5, Stretch = Stretch.Fill, IsHitTestVisible = false };
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                    Canvas.SetLeft(image, w / 2 + (corner.X - cx) * scale);
                    Canvas.SetTop(image, h / 2 - (corner.Y - cy) * scale);
                    canvas.Children.Add(image);
                    any = true;
                }
            if (!any)
                return false;
            if (wash > 0)
                canvas.Children.Add(new System.Windows.Shapes.Rectangle { Width = w, Height = h, Fill = new SolidColorBrush(Color.FromArgb((byte)(wash * 255), 0, 0, 0)), IsHitTestVisible = false });
            var credit = new TextBlock { Text = "© Pleb Masters Forge", FontSize = 9.5, Foreground = Brushes.White, Opacity = 0.8, IsHitTestVisible = false };
            Canvas.SetLeft(credit, 6);
            Canvas.SetBottom(credit, 4);
            canvas.Children.Add(credit);
            return true;
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
