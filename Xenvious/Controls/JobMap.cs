using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Xenvious
{
    /// <summary>
    /// A top-down view of a job: the GTA map, zoomed to the placed objects, with one dot
    /// per object. Positions are world coordinates; the conversion to the map image is the
    /// one Map Mover uses (MainWindow.WorldToMap), so calibrating there fixes both.
    /// </summary>
    public class JobMap : Border
    {
        /// <summary>One dot: world X/Y and its colour.</summary>
        public readonly struct Marker
        {
            public Marker(double x, double y, Brush fill, double size = 4)
            {
                X = x;
                Y = y;
                Fill = fill;
                Size = size;
            }

            public double X { get; }
            public double Y { get; }
            public Brush Fill { get; }
            public double Size { get; }
        }

        // The map image is laid out on the same 420 x 578 surface as on the Map Mover page;
        // WorldToMap returns positions on that surface.
        private const double SurfaceWidth = 420;
        private const double SurfaceHeight = 578;
        // Never zoom in further than this many surface pixels across, or a single prop
        // would fill the whole view.
        private const double MinSpan = 12;

        private static readonly ImageSource MapImage =
            new BitmapImage(new Uri("pack://application:,,,/Images/gtav_map.jpg", UriKind.Absolute));

        private readonly Canvas _surface = new Canvas { ClipToBounds = true };
        private readonly Image _image = new Image { Source = MapImage, Stretch = Stretch.Fill, Width = SurfaceWidth, Height = SurfaceHeight };
        private readonly Canvas _dots = new Canvas();
        private List<Marker> _markers = new List<Marker>();

        public JobMap()
        {
            Background = new SolidColorBrush(Color.FromRgb(0x1B, 0x1D, 0x20));
            CornerRadius = new CornerRadius(4);
            ClipToBounds = true;
            _surface.Children.Add(_image);
            _surface.Children.Add(_dots);
            Child = _surface;
            SizeChanged += (_, __) => Redraw();
        }

        public void SetMarkers(IEnumerable<Marker> markers)
        {
            _markers = markers?.ToList() ?? new List<Marker>();
            Redraw();
        }

        private void Redraw()
        {
            _dots.Children.Clear();
            double width = ActualWidth, height = ActualHeight;
            if (width <= 0 || height <= 0)
                return;

            if (_markers.Count == 0)
            {
                // Nothing placed: the whole map.
                ApplyView(0, 0, SurfaceWidth, SurfaceHeight, width, height);
                return;
            }

            var points = _markers.Select(m => MainWindow.WorldToMap(m.X, m.Y)).ToList();
            double minX = points.Min(p => p.X), maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y), maxY = points.Max(p => p.Y);
            double span = Math.Max(MinSpan, Math.Max(maxX - minX, maxY - minY)) * 1.25;
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            double scale = ApplyView(cx - span / 2, cy - span / 2, span, span, width, height);
            double offsetX = (width - span * scale) / 2 - (cx - span / 2) * scale;
            double offsetY = (height - span * scale) / 2 - (cy - span / 2) * scale;

            for (int i = 0; i < points.Count; i++)
            {
                var marker = _markers[i];
                var dot = new Ellipse { Width = marker.Size, Height = marker.Size, Fill = marker.Fill };
                Canvas.SetLeft(dot, points[i].X * scale + offsetX - marker.Size / 2);
                Canvas.SetTop(dot, points[i].Y * scale + offsetY - marker.Size / 2);
                _dots.Children.Add(dot);
            }
        }

        // Scales and moves the map so the given part of the surface fills the view, centred;
        // returns the scale. The dots are placed in view coordinates, so they keep their size.
        private double ApplyView(double left, double top, double spanX, double spanY, double width, double height)
        {
            double scale = Math.Min(width / spanX, height / spanY);
            double offsetX = (width - spanX * scale) / 2 - left * scale;
            double offsetY = (height - spanY * scale) / 2 - top * scale;
            var transform = new TransformGroup();
            transform.Children.Add(new ScaleTransform(scale, scale));
            transform.Children.Add(new TranslateTransform(offsetX, offsetY));
            _image.RenderTransform = transform;
            return scale;
        }
    }
}
