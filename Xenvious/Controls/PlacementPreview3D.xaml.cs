using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xenvious.AdvancedPlacement;
using Xenvious.ViewModels;

namespace Xenvious.Controls
{
    /// <summary>
    /// Live 3D preview of the placement plan: one box per prop, built from the prop's
    /// real bounding box around its pivot, with an orbit camera (left drag: rotate,
    /// right drag: pan, wheel: zoom). The camera frames the plan until the user moves
    /// it; "fit" frames it again.
    /// </summary>
    public partial class PlacementPreview3D : UserControl
    {
        private AdvancedPropPlacementViewModel _vm;

        // Orbit camera around _target, in preview coordinates (plan centre = origin).
        private Point3D _target = new Point3D(0, 0, 0);
        private double _yaw = -35, _pitch = 30, _distance = 60;
        private bool _userMoved;
        private Point _lastMouse;
        private Rect3D _bounds = Rect3D.Empty;
        private int _framedVersion = -1;

        public PlacementPreview3D()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += (_, __) => Rebuild();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm != null)
            {
                _vm.PreviewInvalidated -= OnPreviewInvalidated;
            }
            _vm = DataContext as AdvancedPropPlacementViewModel;
            if (_vm != null)
            {
                _vm.PreviewInvalidated += OnPreviewInvalidated;
            }
            Rebuild();
        }

        private void OnPreviewInvalidated(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess()) Rebuild();
            else Dispatcher.BeginInvoke(new Action(Rebuild));
        }

        private void Rebuild()
        {
            // The preview must never take the app down; a failed rebuild keeps the last frame.
            try
            {
                RebuildCore();
            }
            catch
            {
            }
        }

        private void RebuildCore()
        {
            if (_vm == null || PreviewModels == null)
            {
                return;
            }

            List<PropPose> plan = _vm.BuildPlan();
            var group = new Model3DGroup();
            group.Children.Add(new AmbientLight(Color.FromRgb(85, 85, 92)));
            group.Children.Add(new DirectionalLight(Color.FromRgb(190, 190, 200), new Vector3D(-1, -1.5, -3)));
            group.Children.Add(new DirectionalLight(Color.FromRgb(80, 80, 95), new Vector3D(1, 1, 1)));

            if (plan.Count == 0)
            {
                PreviewModels.Content = group;
                return;
            }

            // Draw relative to the start point (world coordinates are in the thousands).
            // A fixed origin keeps the view still while the build turns around it.
            V3 centre = _vm.StartPose.Position;

            MeshGeometry3D mesh = CreateBoxMesh(_vm.BoxMin, _vm.BoxMax);
            mesh.Freeze();
            var bounds = Rect3D.Empty;

            // The prop the plan continues from, if any, in grey.
            if (!_vm.IncludeStart)
            {
                group.Children.Add(BuildBox(mesh, _vm.StartPose, centre, Color.FromRgb(110, 112, 120), ref bounds));
            }

            for (int i = 0; i < plan.Count; i++)
            {
                if (!plan[i].Position.IsFinite) continue;
                double t = plan.Count <= 1 ? 0 : (double)i / (plan.Count - 1);
                var colour = Color.FromRgb((byte)(40 + t * 200), (byte)(200 - t * 140), (byte)(180 + t * 40));
                group.Children.Add(BuildBox(mesh, plan[i], centre, colour, ref bounds));
            }

            // Start point of the build (where a template's start prop sits), as a yellow marker,
            // and the ground at that height: anything below the grid ends up underground.
            PropPose anchor = _vm.TemplateAnchor(plan);
            V3 a = anchor.Position - centre;
            group.Children.Add(Marker(new Point3D(a.X, a.Y, a.Z), 0.9, Color.FromRgb(250, 200, 40)));
            AddGrid(group, bounds, a.Z);

            // Pivot and axis of the turn: the pieces circle this line.
            if (_vm.TryGetStepGuide(out V3 pivot, out V3 axis))
            {
                V3 c = pivot - centre;
                double len = Math.Max(10, Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ)) * 0.6);
                V3 p0 = c - axis * len, p1 = c + axis * len;
                group.Children.Add(Line(new Point3D(p0.X, p0.Y, p0.Z), new Point3D(p1.X, p1.Y, p1.Z), 0.15, Color.FromArgb(200, 120, 170, 255)));
                group.Children.Add(Marker(new Point3D(c.X, c.Y, c.Z), 0.7, Color.FromRgb(120, 170, 255)));
            }

            group.Freeze();
            PreviewModels.Content = group;
            _bounds = bounds;

            // Frame the build only when it is new (first draw, other prop, other shape) or
            // on request; otherwise keep the camera where it is.
            if (_framedVersion != _vm.PreviewFrameVersion)
            {
                _framedVersion = _vm.PreviewFrameVersion;
                _userMoved = false;
                Fit();
            }
            else
            {
                UpdateCamera();
            }
        }

        private static GeometryModel3D BuildBox(MeshGeometry3D mesh, in PropPose pose, V3 centre, Color colour, ref Rect3D bounds)
        {
            var brush = new SolidColorBrush(colour);
            brush.Freeze();
            var material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(brush));
            material.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), 20));

            Rot3 r = pose.Rotation;
            V3 p = pose.Position - centre;
            // WPF multiplies row vectors (p · M), so M is the transpose of our column rotation.
            var matrix = new Matrix3D(
                r.M00, r.M10, r.M20, 0,
                r.M01, r.M11, r.M21, 0,
                r.M02, r.M12, r.M22, 0,
                p.X, p.Y, p.Z, 1);
            var model = new GeometryModel3D(mesh, material)
            {
                BackMaterial = new DiffuseMaterial(brush),
                Transform = new MatrixTransform3D(matrix)
            };
            bounds.Union(model.Bounds);
            return model;
        }

        /// <summary>Small double pyramid (diamond) as a point marker.</summary>
        private static GeometryModel3D Marker(Point3D c, double size, Color colour)
        {
            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    new Point3D(c.X, c.Y, c.Z + size), new Point3D(c.X, c.Y, c.Z - size),
                    new Point3D(c.X + size, c.Y, c.Z), new Point3D(c.X - size, c.Y, c.Z),
                    new Point3D(c.X, c.Y + size, c.Z), new Point3D(c.X, c.Y - size, c.Z)
                },
                TriangleIndices = new Int32Collection { 0,2,4, 0,4,3, 0,3,5, 0,5,2, 1,4,2, 1,3,4, 1,5,3, 1,2,5 }
            };
            var brush = new SolidColorBrush(colour);
            return new GeometryModel3D(mesh, new EmissiveMaterial(brush)) { BackMaterial = new EmissiveMaterial(brush) };
        }

        /// <summary>Thin square rod from p0 to p1.</summary>
        private static GeometryModel3D Line(Point3D p0, Point3D p1, double thickness, Color colour)
        {
            Vector3D d = p1 - p0;
            if (d.Length < 1e-6) d = new Vector3D(0, 0, 1);
            d.Normalize();
            Vector3D u = Vector3D.CrossProduct(d, Math.Abs(d.Z) < 0.9 ? new Vector3D(0, 0, 1) : new Vector3D(1, 0, 0));
            u.Normalize();
            Vector3D v = Vector3D.CrossProduct(d, u);
            u *= thickness / 2; v *= thickness / 2;
            var positions = new Point3DCollection
            {
                p0 - u - v, p0 + u - v, p0 + u + v, p0 - u + v,
                p1 - u - v, p1 + u - v, p1 + u + v, p1 - u + v
            };
            var mesh = new MeshGeometry3D
            {
                Positions = positions,
                TriangleIndices = new Int32Collection { 0,1,5, 0,5,4, 1,2,6, 1,6,5, 2,3,7, 2,7,6, 3,0,4, 3,4,7 }
            };
            var brush = new SolidColorBrush(colour);
            return new GeometryModel3D(mesh, new EmissiveMaterial(brush)) { BackMaterial = new EmissiveMaterial(brush) };
        }

        /// <summary>Ground grid at height <paramref name="z"/>, a bit larger than the build.</summary>
        private static void AddGrid(Model3DGroup group, Rect3D bounds, double z)
        {
            if (bounds.IsEmpty) return;
            double extent = Math.Max(bounds.SizeX, bounds.SizeY) * 0.75 + 10;
            double step = extent < 40 ? 5 : extent < 120 ? 10 : extent < 300 ? 25 : 50;
            double cx = Math.Round((bounds.X + bounds.SizeX / 2) / step) * step;
            double cy = Math.Round((bounds.Y + bounds.SizeY / 2) / step) * step;
            double half = Math.Ceiling(extent / step) * step;
            var colour = Color.FromArgb(70, 200, 205, 215);
            for (double x = cx - half; x <= cx + half + 1e-6; x += step)
                group.Children.Add(Line(new Point3D(x, cy - half, z), new Point3D(x, cy + half, z), step / 60, colour));
            for (double y = cy - half; y <= cy + half + 1e-6; y += step)
                group.Children.Add(Line(new Point3D(cx - half, y, z), new Point3D(cx + half, y, z), step / 60, colour));
        }

        /// <summary>Box from min to max around the pivot, so props with an off-centre pivot sit right.</summary>
        private static MeshGeometry3D CreateBoxMesh(V3 min, V3 max)
        {
            double x0 = min.X, y0 = min.Y, z0 = min.Z, x1 = max.X, y1 = max.Y, z1 = max.Z;
            if (x1 - x0 < 0.05) { x0 -= 0.025; x1 += 0.025; }
            if (y1 - y0 < 0.05) { y0 -= 0.025; y1 += 0.025; }
            if (z1 - z0 < 0.05) { z0 -= 0.025; z1 += 0.025; }

            var positions = new Point3DCollection
            {
                new Point3D(x0, y0, z0), new Point3D(x1, y0, z0), new Point3D(x1, y1, z0), new Point3D(x0, y1, z0),
                new Point3D(x0, y0, z1), new Point3D(x1, y0, z1), new Point3D(x1, y1, z1), new Point3D(x0, y1, z1)
            };
            var indices = new Int32Collection
            {
                0,2,1, 0,3,2,   // bottom
                4,5,6, 4,6,7,   // top
                0,1,5, 0,5,4,   // back (-Y)
                3,6,2, 3,7,6,   // front (+Y)
                1,2,6, 1,6,5,   // right (+X)
                0,4,7, 0,7,3    // left (-X)
            };
            return new MeshGeometry3D { Positions = positions, TriangleIndices = indices };
        }

        // ------------------------------------------------------------------
        // Camera
        // ------------------------------------------------------------------
        private void Fit()
        {
            if (_bounds.IsEmpty)
            {
                return;
            }
            _target = new Point3D(_bounds.X + _bounds.SizeX / 2, _bounds.Y + _bounds.SizeY / 2, _bounds.Z + _bounds.SizeZ / 2);
            double radius = Math.Sqrt(_bounds.SizeX * _bounds.SizeX + _bounds.SizeY * _bounds.SizeY + _bounds.SizeZ * _bounds.SizeZ) / 2;
            _distance = Math.Max(5, radius / Math.Tan(25 * Math.PI / 180) * 1.1);
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            double yaw = _yaw * Math.PI / 180, pitch = _pitch * Math.PI / 180;
            var dir = new Vector3D(Math.Cos(pitch) * Math.Sin(yaw), -Math.Cos(pitch) * Math.Cos(yaw), Math.Sin(pitch));
            Point3D position = _target + dir * _distance;
            PreviewCamera.Position = position;
            PreviewCamera.LookDirection = _target - position;
            PreviewCamera.UpDirection = new Vector3D(0, 0, 1);
            PreviewCamera.NearPlaneDistance = Math.Max(0.05, _distance / 500);
            PreviewCamera.FarPlaneDistance = _distance * 20 + 1000;
        }

        private void OnFitClick(object sender, RoutedEventArgs e)
        {
            _userMoved = false;
            Fit();
        }

        private void OnViewTop(object sender, RoutedEventArgs e) { _yaw = 0; _pitch = 89; _userMoved = false; Fit(); }
        private void OnViewSide(object sender, RoutedEventArgs e) { _yaw = 0; _pitch = 2; _userMoved = false; Fit(); }
        private void OnView3D(object sender, RoutedEventArgs e) { _yaw = -35; _pitch = 30; _userMoved = false; Fit(); }

        // Left (or middle) drag: grab and move the scene with the mouse. Right drag: orbit
        // around the target. Wheel: zoom towards the point under the mouse.
        // W/A/S/D + Q/E fly once the preview has focus (Shift: faster).
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastMouse = e.GetPosition(InputSurface);
            InputSurface.Focus();
            if (!InputSurface.IsMouseCaptured)
            {
                InputSurface.CaptureMouse();
            }
            e.Handled = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _lastMouse = e.GetPosition(InputSurface);
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released)
            {
                InputSurface.ReleaseMouseCapture();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool orbit = e.RightButton == MouseButtonState.Pressed;
            bool pan = !orbit && (e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed);
            Point now = e.GetPosition(InputSurface);
            Vector delta = now - _lastMouse;
            _lastMouse = now;
            if (!orbit && !pan)
            {
                return;
            }
            _userMoved = true;

            if (orbit)
            {
                _yaw -= delta.X * 0.4;
                _pitch = Math.Max(-89, Math.Min(89, _pitch + delta.Y * 0.4));
            }
            else
            {
                CameraAxes(out _, out Vector3D sideways, out Vector3D up);
                double scale = WorldPerPixel();
                _target = _target - sideways * (delta.X * scale) + up * (delta.Y * scale);
            }
            UpdateCamera();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _userMoved = true;
            double factor = e.Delta > 0 ? 0.85 : 1.18;
            double newDistance = Math.Max(1, Math.Min(5000, _distance * factor));

            // Keep the point under the mouse where it is: move the target towards it by
            // the same share the distance shrinks.
            Point m = e.GetPosition(InputSurface);
            CameraAxes(out _, out Vector3D sideways, out Vector3D up);
            double scale = WorldPerPixel();
            Vector3D offset = sideways * ((m.X - InputSurface.ActualWidth / 2) * scale)
                            - up * ((m.Y - InputSurface.ActualHeight / 2) * scale);
            _target = _target + offset * (1 - newDistance / _distance);
            _distance = newDistance;
            UpdateCamera();
            e.Handled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            CameraAxes(out Vector3D look, out Vector3D sideways, out _);
            var flat = new Vector3D(look.X, look.Y, 0);
            if (flat.Length < 1e-6) flat = new Vector3D(0, 1, 0);
            flat.Normalize();
            double step = Math.Max(0.5, _distance * 0.05) * ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? 4 : 1);
            Vector3D move;
            switch (e.Key)
            {
                case Key.W: move = flat * step; break;
                case Key.S: move = -flat * step; break;
                case Key.A: move = -sideways * step; break;
                case Key.D: move = sideways * step; break;
                case Key.E: move = new Vector3D(0, 0, step); break;
                case Key.Q: move = new Vector3D(0, 0, -step); break;
                default: return;
            }
            _userMoved = true;
            _target = _target + move;
            UpdateCamera();
            e.Handled = true;
        }

        private void CameraAxes(out Vector3D look, out Vector3D sideways, out Vector3D up)
        {
            look = PreviewCamera.LookDirection;
            look.Normalize();
            sideways = Vector3D.CrossProduct(look, new Vector3D(0, 0, 1));
            if (sideways.Length < 1e-6) sideways = new Vector3D(1, 0, 0);
            sideways.Normalize();
            up = Vector3D.CrossProduct(sideways, look);
        }

        /// <summary>World metres per screen pixel at the target's depth.</summary>
        private double WorldPerPixel()
        {
            return _distance * Math.Tan(PreviewCamera.FieldOfView / 2 * Math.PI / 180) * 2 / Math.Max(1, InputSurface.ActualHeight);
        }
    }
}
