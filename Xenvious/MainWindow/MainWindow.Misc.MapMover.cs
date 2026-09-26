using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;
using Xenvious.Logging;
using static mry.mem;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Misc / MapMover page.
    public partial class MainWindow
    {
        // Moving a whole job is the same write the Ctrl+Shift and +/- shortcut does in a
        // location field, only applied to every selected family at once. The difference is
        // how the amount is chosen: you say where the job should end up, not how far it
        // should travel. The starting point is never asked for -- it is the centre of what
        // is loaded and ticked, which Xenvious works out itself.
        //
        // A slot is one axis of one family: where its first element sits, how far apart the
        // elements are, and which global holds how many of them exist.
        private readonly struct MoveSlot
        {
            public readonly long Offset;
            public readonly long Next;
            public readonly long CountIndex;

            public MoveSlot(long offset, long next, long countIndex)
            {
                Offset = offset;
                Next = next;
                CountIndex = countIndex;
            }
        }

        private sealed class MoveFamily
        {
            public string Label;
            public CheckBox Toggle;
            public Func<int, MoveSlot[]> Slots;
            public Func<long> CountIndex;
        }

        private List<MoveFamily> _mapMoverFamilies;
        private Vector3? _mapMoverTo;
        private Vector3 _mapMoverLastDelta;
        private bool _mapMoverHasUndo;
        private bool _mapMoverUpdating;

        // --- Karte <-> Welt ------------------------------------------------------
        //
        // Images/gtav_map.jpg is a straight top-down render, so world and image are related
        // by a scale and an origin per axis, nothing else. Y is inverted because world Y
        // grows north while image rows grow downwards. These defaults are an estimate;
        // "Kalibrieren" replaces them with numbers measured from two real camera positions
        // and stores them in the roaming config.
        private static double _mapOriginX = -4150;      // Welt-X am linken Bildrand
        private static double _mapUnitsPerPxX = 8750.0 / 420.0;
        private static double _mapOriginY = 7750;       // Welt-Y am oberen Bildrand
        private static double _mapUnitsPerPxY = 12050.0 / 578.0;

        private const string MapCalibSection = "MAPMOVER";

        // Also used by JobMap (Copy Jobs), so both follow the same calibration.
        internal static Point WorldToMap(double wx, double wy)
        {
            return new Point((wx - _mapOriginX) / _mapUnitsPerPxX,
                             (_mapOriginY - wy) / _mapUnitsPerPxY);
        }

        private Point MapToWorld(double px, double py)
        {
            return new Point(_mapOriginX + px * _mapUnitsPerPxX,
                             _mapOriginY - py * _mapUnitsPerPxY);
        }

        private void LoadMapCalibration()
        {
            try
            {
                string path = Functions.getRoamingConfigFilePath();
                if (!File.Exists(path))
                    return;
                var ini = new ini_reader(path);
                double ox, ux, oy, uy;
                if (double.TryParse(ini.ReadString(MapCalibSection, "originX"), NumberStyles.Float, CultureInfo.InvariantCulture, out ox) &&
                    double.TryParse(ini.ReadString(MapCalibSection, "unitsPerPxX"), NumberStyles.Float, CultureInfo.InvariantCulture, out ux) &&
                    double.TryParse(ini.ReadString(MapCalibSection, "originY"), NumberStyles.Float, CultureInfo.InvariantCulture, out oy) &&
                    double.TryParse(ini.ReadString(MapCalibSection, "unitsPerPxY"), NumberStyles.Float, CultureInfo.InvariantCulture, out uy) &&
                    ux != 0 && uy != 0)
                {
                    _mapOriginX = ox; _mapUnitsPerPxX = ux;
                    _mapOriginY = oy; _mapUnitsPerPxY = uy;
                    Log.Info("Map Mover: Kalibrierung aus config.ini geladen");
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Map Mover: Kalibrierung nicht lesbar: " + ex.Message);
            }
        }

        private void SaveMapCalibration()
        {
            try
            {
                var ini = new ini_reader(Functions.getRoamingConfigFilePath());
                ini.Write(MapCalibSection, "originX", _mapOriginX.ToString("R", CultureInfo.InvariantCulture));
                ini.Write(MapCalibSection, "unitsPerPxX", _mapUnitsPerPxX.ToString("R", CultureInfo.InvariantCulture));
                ini.Write(MapCalibSection, "originY", _mapOriginY.ToString("R", CultureInfo.InvariantCulture));
                ini.Write(MapCalibSection, "unitsPerPxY", _mapUnitsPerPxY.ToString("R", CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                Log.Warn("Map Mover: Kalibrierung nicht speicherbar: " + ex.Message);
            }
        }

        // --- Kamera / Cursor -----------------------------------------------------

        // Where the creator camera currently is. That is the point the user can fly to,
        // which is a far more direct way of saying "put the job here" than a map click.
        private Vector3? ReadCreatorCam()
        {
            try
            {
                if (!m.IsProcOpen || GTA.Offsets.Editor.creator_camptr == 0)
                    return null;
                long addy = m.memory(GTA.Offsets.Editor.creator_camptr, GTA.Offsets.Editor.OFFSET_creator_cam_loc).GetAddress();
                if (addy == 0)
                    return null;
                var v = new Vector3(
                    m.memory(addy.ToString("X")).Get<float>(),
                    m.memory((addy + 4).ToString("X")).Get<float>(),
                    m.memory((addy + 8).ToString("X")).Get<float>());
                // Genau 0/0/0 ist keine Kameraposition, sondern ein Zeiger, der ins Leere
                // greift. Als Wert durchgereicht macht er jede Rechnung darauf kaputt.
                if (v.X == 0f && v.Y == 0f && v.Z == 0f)
                    return null;
                return v;
            }
            catch
            {
                return null;
            }
        }

        // --- Familien ------------------------------------------------------------

        private static long AxisOf(long x, long y, long z, int axis) => axis == 0 ? x : axis == 1 ? y : z;

        private List<MoveFamily> MapMoverFamilies()
        {
            if (_mapMoverFamilies != null)
                return _mapMoverFamilies;

            var f = new List<MoveFamily>
            {
                new MoveFamily { Label = "Props",
                    CountIndex = () => GTA.Offsets.Editor.Props.number,
                    Slots = a => new[] { new MoveSlot(GTA.Offsets.Editor.Props.loc + a, GTA.Offsets.Editor.Props.NEXT, GTA.Offsets.Editor.Props.number) } },
                new MoveFamily { Label = "Dynamic Props",
                    CountIndex = () => GTA.Offsets.Editor.DProps.number,
                    Slots = a => new[] { new MoveSlot(GTA.Offsets.Editor.DProps.loc + a, GTA.Offsets.Editor.DProps.NEXT, GTA.Offsets.Editor.DProps.number) } },
                new MoveFamily { Label = "Vehicles",
                    CountIndex = () => GTA.Offsets.Editor.Vehicle.number,
                    Slots = a => new[] { new MoveSlot(GTA.Offsets.Editor.Vehicle.loc + a, GTA.Offsets.Editor.Vehicle.NEXT, GTA.Offsets.Editor.Vehicle.number) } },
                new MoveFamily { Label = "Objects",
                    CountIndex = () => GTA.Offsets.Editor.Objects.number,
                    Slots = a => new[] { new MoveSlot(GTA.Offsets.Editor.Objects.loc + a, GTA.Offsets.Editor.Objects.NEXT, GTA.Offsets.Editor.Objects.number) } },
                new MoveFamily { Label = "Actors",
                    CountIndex = () => GTA.Offsets.Editor.Actor.number,
                    Slots = a => new[] { new MoveSlot(AxisOf(GTA.Offsets.Editor.Actor.locx, GTA.Offsets.Editor.Actor.locy, GTA.Offsets.Editor.Actor.locz, a),
                                                     GTA.Offsets.Editor.Actor.NEXT, GTA.Offsets.Editor.Actor.number) } },
                new MoveFamily { Label = "Weapons",
                    CountIndex = () => GTA.Offsets.Editor.Weapon.number,
                    Slots = a => new[] { new MoveSlot(AxisOf(GTA.Offsets.Editor.Weapon.locx, GTA.Offsets.Editor.Weapon.locy, GTA.Offsets.Editor.Weapon.locz, a),
                                                     GTA.Offsets.Editor.Weapon.NEXT, GTA.Offsets.Editor.Weapon.number) } },
                new MoveFamily { Label = "Locations",
                    CountIndex = () => GTA.Offsets.Editor.Locations.number,
                    Slots = a => new[] { new MoveSlot(AxisOf(GTA.Offsets.Editor.Locations.locx, GTA.Offsets.Editor.Locations.locy, GTA.Offsets.Editor.Locations.locz, a),
                                                     GTA.Offsets.Editor.Locations.NEXT, GTA.Offsets.Editor.Locations.number) } },
                new MoveFamily { Label = "Locations 2",
                    CountIndex = () => GTA.Offsets.Editor.Locations.number,
                    Slots = a => new[] { new MoveSlot(AxisOf(GTA.Offsets.Editor.Locations.locaa1x, GTA.Offsets.Editor.Locations.locaa1y, GTA.Offsets.Editor.Locations.locaa1z, a),
                                                     GTA.Offsets.Editor.Locations.NEXT, GTA.Offsets.Editor.Locations.number) } },
                new MoveFamily { Label = "Zones",
                    CountIndex = () => GTA.Offsets.Editor.Zones.number,
                    Slots = a => new[] { new MoveSlot(AxisOf(GTA.Offsets.Editor.Zones.vtox, GTA.Offsets.Editor.Zones.vtoy, GTA.Offsets.Editor.Zones.vtoz, a),
                                                     GTA.Offsets.Editor.Zones.NEXT, GTA.Offsets.Editor.Zones.number) } },
                // A race checkpoint carries three vehicle spawn positions that have to travel
                // with it, otherwise the starting grid stays behind. vspn holds them as nine
                // consecutive floats: three x, then three y, then three z.
                new MoveFamily { Label = "Race Checkpoints",
                    CountIndex = () => GTA.Offsets.Editor.Race.Checkpoints.number,
                    Slots = a => new[]
                    {
                        new MoveSlot(AxisOf(GTA.Offsets.Editor.Race.Checkpoints.locx, GTA.Offsets.Editor.Race.Checkpoints.locy, GTA.Offsets.Editor.Race.Checkpoints.locz, a),
                                     GTA.Offsets.Editor.Race.Checkpoints.NEXT, GTA.Offsets.Editor.Race.Checkpoints.number),
                        new MoveSlot(GTA.Offsets.Editor.Race.Checkpoints.vspn + (a * 3) + 0, GTA.Offsets.Editor.Race.Checkpoints.NEXT, GTA.Offsets.Editor.Race.Checkpoints.number),
                        new MoveSlot(GTA.Offsets.Editor.Race.Checkpoints.vspn + (a * 3) + 1, GTA.Offsets.Editor.Race.Checkpoints.NEXT, GTA.Offsets.Editor.Race.Checkpoints.number),
                        new MoveSlot(GTA.Offsets.Editor.Race.Checkpoints.vspn + (a * 3) + 2, GTA.Offsets.Editor.Race.Checkpoints.NEXT, GTA.Offsets.Editor.Race.Checkpoints.number),
                    } },
            };

            // Zeilen bauen. Das CheckBox-Template des Tools zeigt kein Content, das Label
            // ist darum immer ein eigener TextBlock. Zwei Spalten, damit zehn Familien
            // nicht die halbe Seite hoch sind.
            int half = (f.Count + 1) / 2;
            for (int i = 0; i < f.Count; i++)
            {
                MoveFamily fam = f[i];
                var row = new Grid { Height = 32 };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

                var label = new TextBlock { Text = fam.Label, FontSize = 16, VerticalAlignment = VerticalAlignment.Center };
                var cb = new CheckBox
                {
                    IsChecked = true,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Style = (Style)Resources["FormToggle"]
                };
                Grid.SetColumn(cb, 1);
                row.Children.Add(label);
                row.Children.Add(cb);
                fam.Toggle = cb;

                (i < half ? panelMapMoverFamiliesLeft : panelMapMoverFamiliesRight).Children.Add(row);
            }

            _mapMoverFamilies = f;
            return f;
        }

        // --- Startpunkt ----------------------------------------------------------

        // Centre of everything that is loaded and ticked. This is the job's position, so it
        // is never asked for -- asking would only invite a wrong answer.
        private Vector3? CurrentJobCentre()
        {
            if (!m.IsProcOpen)
                return null;

            double sx = 0, sy = 0, sz = 0;
            int n = 0;
            foreach (MoveFamily fam in MapMoverFamilies())
            {
                if (fam.Toggle?.IsChecked != true)
                    continue;
                int count = new Global(fam.CountIndex()).Get<int>();
                if (count <= 0 || count > 10000)
                    continue;
                MoveSlot sx0 = fam.Slots(0)[0], sy0 = fam.Slots(1)[0], sz0 = fam.Slots(2)[0];
                for (int i = 0; i < count; i++)
                {
                    float x = new Global(sx0.Offset + (i * sx0.Next)).Get<float>();
                    float y = new Global(sy0.Offset + (i * sy0.Next)).Get<float>();
                    float z = new Global(sz0.Offset + (i * sz0.Next)).Get<float>();
                    if (x == 0f && y == 0f)     // leerer Slot
                        continue;
                    sx += x; sy += y; sz += z; n++;
                }
            }
            return n == 0 ? (Vector3?)null : new Vector3((float)(sx / n), (float)(sy / n), (float)(sz / n));
        }

        // --- Zeichnen ------------------------------------------------------------

        private void DrawMapMoverMarkers(Vector3? from)
        {
            canvasMapMover.Children.Clear();

            if (from.HasValue && _mapMoverTo.HasValue)
            {
                Point a = WorldToMap(from.Value.X, from.Value.Y);
                Point b = WorldToMap(_mapMoverTo.Value.X, _mapMoverTo.Value.Y);
                if (double.IsNaN(a.X) || double.IsNaN(b.X) || double.IsInfinity(a.X) || double.IsInfinity(b.X))
                    return;
                canvasMapMover.Children.Add(new System.Windows.Shapes.Line
                {
                    X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y,
                    Stroke = Brushes.White, StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 4, 3 }, Opacity = 0.8
                });
            }
            if (from.HasValue)
                AddMapMarker(WorldToMap(from.Value.X, from.Value.Y), Brushes.White);
            if (_mapMoverTo.HasValue)
                AddMapMarker(WorldToMap(_mapMoverTo.Value.X, _mapMoverTo.Value.Y), Brushes.OrangeRed);
        }

        private static bool IsUsableScale(double v)
        {
            return !double.IsNaN(v) && !double.IsInfinity(v) && Math.Abs(v) > 0.0001;
        }

        private void AddMapMarker(Point p, Brush fill)
        {
            if (double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsInfinity(p.X) || double.IsInfinity(p.Y))
                return;

            double r = 6 / Math.Max(mapMoverZoom.ScaleX, 0.1);   // bleibt beim Zoomen gleich gross
            var dot = new System.Windows.Shapes.Ellipse
            {
                Width = r * 2, Height = r * 2, Fill = fill,
                Stroke = Brushes.Black, StrokeThickness = 1.5 / Math.Max(mapMoverZoom.ScaleX, 0.1)
            };
            Canvas.SetLeft(dot, p.X - r);
            Canvas.SetTop(dot, p.Y - r);
            canvasMapMover.Children.Add(dot);
        }

        // --- Karte bedienen ------------------------------------------------------

        private void MapMoverMap_Wheel(object sender, MouseWheelEventArgs e)
        {
            double z = mapMoverZoom.ScaleX * (e.Delta > 0 ? 1.2 : 1 / 1.2);
            z = Math.Max(1.0, Math.Min(6.0, z));
            mapMoverZoom.ScaleX = mapMoverZoom.ScaleY = z;
            RefreshMapMover(false);
            e.Handled = true;       // sonst scrollt der ScrollViewer mit
        }

        private void MapMoverMap_Move(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(gridMapMoverCanvas);
            Point w = MapToWorld(p.X, p.Y);
            tbmapmoverhover.Text = _mapCalibStep > 0
                ? $"Kalibrieren {_mapCalibStep}/2 -- hier klicken   (X {w.X:0}  Y {w.Y:0})"
                : $"X {w.X:0}   Y {w.Y:0}   ({mapMoverZoom.ScaleX:0.#}x)";
        }

        private void MapMoverMap_Click(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(gridMapMoverCanvas);

            if (_mapCalibStep > 0)
            {
                HandleCalibrationClick(p);
                return;
            }

            Point w = MapToWorld(p.X, p.Y);
            // Ein Kartenklick kennt keine Hoehe -- Z bleibt, was es war.
            _mapMoverTo = new Vector3((float)w.X, (float)w.Y, _mapMoverTo?.Z ?? 0f);
            RefreshMapMover(true);
        }

        // Where the user currently is in GTA. Preferably the creator camera; when that
        // pointer does not resolve -- creator_camptr is a data pattern and misses on
        // Enhanced -- the player position does just as well, because the creator carries
        // the player ped along with the camera.
        private Vector3? ReadGamePosition(out string source)
        {
            source = "Kamera";
            Vector3? cam = ReadCreatorCam();
            if (cam != null)
                return cam;

            source = "Spieler";
            if (!m.IsProcOpen)
                return null;
            try
            {
                XenVector3 p = GTA.GetLocation();
                if (p.X == 0f && p.Y == 0f && p.Z == 0f)
                    return null;
                return new Vector3(p.X, p.Y, p.Z);
            }
            catch
            {
                return null;
            }
        }

        private void BtnMapMoverToCam_Click(object sender, RoutedEventArgs e)
        {
            string source;
            Vector3? v = ReadGamePosition(out source);
            if (v == null)
            {
                tbmapmoverstatus.Text = "Position nicht lesbar. Laeuft GTA und ist der Creator offen?";
                return;
            }
            _mapMoverTo = v;
            RefreshMapMover(true);
            tbmapmoverstatus.Text = $"Ziel aus {source}-Position uebernommen.";
        }

        // --- Diagnose ------------------------------------------------------------
        //
        // Cursor and camera both come from a chain of pointers, and when the result is 0
        // there is no way to tell from the outside which step gave up. This prints every
        // step, so one screenshot replaces a round of guessing.
        private void BtnMapMoverDiag_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            try
            {
                sb.AppendLine($"GTA offen: {m.IsProcOpen}   Build: {GameVariant.DisplayName(GameVariant.Current)}");

                long[] lp = GTA.Offsets.Editor.localptr;
                sb.AppendLine($"localptr: {(lp == null ? "null" : $"0x{lp[0]:X} + 0x{lp[1]:X}")}");
                if (lp == null)
                {
                    lp = GTA.getCurrentCreatorAddy();
                    sb.AppendLine($"  neu gesucht: {(lp == null ? "nichts gefunden" : $"0x{lp[0]:X} + 0x{lp[1]:X}")}");
                    GTA.Offsets.Editor.localptr = lp;
                }
                if (lp != null)
                {
                    string name = GTA.ReadScriptName(lp[0], lp[1]);
                    sb.AppendLine($"Script-Name (Offset 0x{GTA.Offsets.Editor.OFFSET_script_name:X}): \"{name}\"");

                    long worker = getCurrentCreatorWorkerOffset();
                    sb.AppendLine($"Worker-Index: {worker}   Feld pos: {GTA.Offsets.Editor.OFFSET_current_creator_worker_pos}");

                    long statics = m.memory(lp[0], new long[] { lp[1], GTA.Offsets.Editor.OFFSET_script_local_start }).GetAddress();
                    sb.AppendLine($"Statics-Basis (0x{GTA.Offsets.Editor.OFFSET_script_local_start:X}): 0x{statics:X}");

                    long wbase = getCreatorScriptLocalWorkerBase();
                    sb.AppendLine($"Worker-Basis: 0x{wbase:X}");
                    if (wbase != 0)
                    {
                        long posAddy = wbase + GTA.Offsets.Editor.OFFSET_current_creator_worker_pos * 8;
                        sb.AppendLine($"pos-Adresse: 0x{posAddy:X}");
                        sb.AppendLine($"pos-Werte: {m.memory(posAddy.ToString("X")).Get<float>():0.###} / "
                                    + $"{m.memory((posAddy + 8).ToString("X")).Get<float>():0.###} / "
                                    + $"{m.memory((posAddy + 16).ToString("X")).Get<float>():0.###}");
                        // Umgebung: zeigt, ob die Koordinaten ein Feld daneben liegen.
                        var around = new StringBuilder("Nachbarfelder f_0..f_9: ");
                        for (int i = 0; i < 10; i++)
                            around.Append($"{m.memory((wbase + i * 8).ToString("X")).Get<float>():0.#} ");
                        sb.AppendLine(around.ToString());
                    }
                }

                if (lp == null)
                {
                    sb.AppendLine("--- Suche ueber den joaat-Hash des Scripts ---");
                    foreach (string name in CreatorScriptNames)
                    {
                        uint hash = Joaat(name);
                        List<long> hhits = ScanForUInt(hash);
                        if (hhits.Count == 0)
                            continue;
                        int shown = 0;
                        foreach (long hh in hhits)
                        {
                            // vtable-Zeiger in den Modulbereich = echtes Objekt, keine Tabelle
                            long vt = ReadPointer(hh - 0x10);
                            if (vt < 0x7FF000000000L || vt > 0x7FFFFFFFFFFFL)
                                continue;
                            long at;
                            long statics = FindStaticsPointerInThread(hh, name, out at);
                            sb.AppendLine($"\"{name}\" Thread 0x{hh - 0x10:X}  (Hash bei 0x{hh:X}, vtable 0x{vt:X})");
                            if (statics != 0)
                            {
                                long posAddy = statics + (WorkerOffsetFor(name) + GTA.Offsets.Editor.OFFSET_current_creator_worker_pos) * 8;
                                sb.AppendLine($"   STATICS 0x{statics:X} gefunden bei Hash{(at < 0 ? "-" : "+")}0x{Math.Abs(at):X}");
                                sb.AppendLine($"   Cursor: {m.memory(posAddy.ToString("X")).Get<float>():0.##} / "
                                            + $"{m.memory((posAddy + 8).ToString("X")).Get<float>():0.##} / "
                                            + $"{m.memory((posAddy + 16).ToString("X")).Get<float>():0.##}");
                            }
                            else
                            {
                                sb.AppendLine($"   kein Statics-Zeiger gefunden{DumpQwords(hh - 0x40, 16)}");
                            }
                            if (++shown >= 2) break;
                        }
                    }

                    sb.AppendLine("--- Suche ueber den Namen (Strings) ---");
                    foreach (string name in CreatorScriptNames)
                    {
                        List<long> hits = ScanForText(name);
                        if (hits.Count == 0)
                            continue;
                        sb.AppendLine($"\"{name}\": {hits.Count} Treffer");
                        foreach (long hit in hits.Take(3))
                        {
                            long threadBase = hit - GTA.Offsets.Editor.OFFSET_script_name;
                            long staticsPtr = ReadPointer(threadBase + GTA.Offsets.Editor.OFFSET_script_local_start);
                            string back = "";
                            try { back = m.memory((threadBase + GTA.Offsets.Editor.OFFSET_script_name).ToString("X")).GetString(32, false); } catch { }
                            sb.AppendLine($"   Treffer 0x{hit:X}  Thread 0x{threadBase:X}");
                            sb.AppendLine($"      [0x{GTA.Offsets.Editor.OFFSET_script_local_start:X}] = 0x{staticsPtr:X} {(LooksLikePointer(staticsPtr) ? "(Zeiger)" : "(kein Zeiger)")}   Name zurueck: \"{back}\"");
                        }
                    }
                }

                sb.AppendLine($"creator_camptr: 0x{GTA.Offsets.Editor.creator_camptr:X}");
                if (GTA.Offsets.Editor.creator_camptr != 0)
                {
                    long camAddy = m.memory(GTA.Offsets.Editor.creator_camptr, GTA.Offsets.Editor.OFFSET_creator_cam_loc).GetAddress();
                    sb.AppendLine($"  Kamera-Adresse: 0x{camAddy:X}");
                    if (camAddy != 0)
                        sb.AppendLine($"  Kamera-Werte: {m.memory(camAddy.ToString("X")).Get<float>():0.###} / "
                                    + $"{m.memory((camAddy + 4).ToString("X")).Get<float>():0.###} / "
                                    + $"{m.memory((camAddy + 8).ToString("X")).Get<float>():0.###}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Abbruch: " + ex.Message);
            }

            tbStuffTestOutput.Text = sb.ToString();
            Log.Info("Map Mover Diagnose:\n" + sb);
        }

        // The creator thread, found by its own name instead of by a pointer.
        //
        // getCurrentCreatorAddy() walks a global that the localptr AOB resolves, and on
        // Enhanced that global is not the thread list -- the scan finds no creator and the
        // whole local chain dies. The thread carries its script name as plain ASCII, so
        // searching writable memory for it locates the thread directly. Slower than a
        // pointer, but it does not depend on a pattern being right.
        private static readonly string[] CreatorScriptNames =
        {
            "fm_lts_creator", "fm_capture_creator", "fm_deathmatch_creator",
            "fm_race_creator", "fm_survival_creator", "fm_mission_creator"
        };

        private static string AsciiPattern(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text)
                sb.Append(((byte)c).ToString("X2")).Append(' ');
            sb.Append("00");        // nullterminiert, sonst trifft auch fm_lts_creator_xyz
            return sb.ToString();
        }

        private List<long> ScanForText(string text)
        {
            try
            {
                var hits = Task.Run(() => m.AoBScan(AsciiPattern(text), true, false)).GetAwaiter().GetResult();
                return hits == null ? new List<long>() : hits.Take(8).ToList();
            }
            catch
            {
                return new List<long>();
            }
        }

        private static long WorkerOffsetFor(string script)
        {
            switch (script)
            {
                case "fm_survival_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_survival;
                case "fm_capture_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_capture;
                case "fm_deathmatch_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_dm;
                case "fm_race_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_race;
                default: return GTA.Offsets.Editor.OFFSET_current_creator_worker_lts;
            }
        }

        private static bool PlausibleWorldCoord(float x, float y, float z)
        {
            return Math.Abs(x) > 0.01f && Math.Abs(x) < 9000f
                && Math.Abs(y) > 0.01f && Math.Abs(y) < 9000f
                && z > -300f && z < 3000f;
        }

        // Find the statics pointer inside a thread object without guessing its offset.
        //
        // The test is the data itself: a candidate pointer is the statics array only if
        // reading the creator's worker struct through it yields a position that could be a
        // place in the world. Anything else is some other pointer in the object.
        private long FindStaticsPointerInThread(long hashAddress, string script, out long foundAtOffset)
        {
            foundAtOffset = 0;
            long worker = WorkerOffsetFor(script);
            long posField = GTA.Offsets.Editor.OFFSET_current_creator_worker_pos;
            if (worker <= 0)
                return 0;

            for (long off = -0x40; off <= 0x400; off += 8)
            {
                long candidate = ReadPointer(hashAddress + off);
                if (!LooksLikePointer(candidate))
                    continue;
                try
                {
                    long posAddy = candidate + (worker + posField) * 8;
                    float x = m.memory(posAddy.ToString("X")).Get<float>();
                    float y = m.memory((posAddy + 8).ToString("X")).Get<float>();
                    float z = m.memory((posAddy + 16).ToString("X")).Get<float>();
                    if (PlausibleWorldCoord(x, y, z))
                    {
                        foundAtOffset = off;
                        return candidate;
                    }
                }
                catch { }
            }
            return 0;
        }

        // A running script thread carries the joaat hash of its script. Unlike the name,
        // that hash lives in the thread itself, so searching for it finds threads and not
        // string tables.
        public static uint Joaat(string text)
        {
            uint h = 0;
            foreach (char c in text.ToLower())
            {
                h += c;
                h += h << 10;
                h ^= h >> 6;
            }
            h += h << 3;
            h ^= h >> 11;
            h += h << 15;
            return h;
        }

        private static string BytePattern(uint value)
        {
            byte[] b = BitConverter.GetBytes(value);
            return string.Join(" ", b.Select(x => x.ToString("X2")));
        }

        private List<long> ScanForUInt(uint value)
        {
            try
            {
                var hits = Task.Run(() => m.AoBScan(BytePattern(value), true, false)).GetAwaiter().GetResult();
                return hits == null ? new List<long>() : hits.Take(40).ToList();
            }
            catch
            {
                return new List<long>();
            }
        }

        // Hex window around an address, so the structure can be read by eye.
        private string DumpQwords(long start, int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                long a = start + i * 8;
                long v = ReadPointer(a);
                if (i % 4 == 0)
                    sb.Append($"\n   +{(i * 8):X3}: ");
                sb.Append($"{v:X16} ");
            }
            return sb.ToString();
        }

        // The running creator thread, found by the joaat hash of its script.
        //
        // Enhanced needs this because its localptr pattern resolves to something that is
        // not the thread list, so getCurrentCreatorAddy() never finds a creator. The name
        // is no help either: in Enhanced it lives in a {hash, name pointer} registry, not
        // in the thread. The hash does live in the thread, right behind the vtable:
        //
        //     +0x00 vtable   +0x08 thread id   +0x10 script hash   +0xB8 statics
        //
        // The 0xB8 is not assumed. A candidate pointer only counts as the statics array if
        // reading the creator's worker struct through it yields a position that could be a
        // place in the world -- measured, not guessed, and it survives the next update.
        private long _creatorHashAddr;
        private long _creatorStatics;
        private string _creatorScript;

        private long ReadPointer(long address)
        {
            try { return m.memory(address.ToString("X")).Get<long>(); }
            catch { return 0; }
        }

        private static bool LooksLikePointer(long p)
        {
            return p > 0x10000 && p < 0x7FFFFFFFFFFF;
        }

        private static bool LooksLikeModulePointer(long p)
        {
            return p >= 0x7FF000000000L && p <= 0x7FFFFFFFFFFFL;
        }

        private bool CreatorCacheStillValid()
        {
            if (_creatorHashAddr == 0 || _creatorStatics == 0 || _creatorScript == null)
                return false;
            try
            {
                if (m.memory(_creatorHashAddr.ToString("X")).Get<uint>() != Joaat(_creatorScript))
                    return false;
                return LooksLikeModulePointer(ReadPointer(_creatorHashAddr - 0x10));
            }
            catch { return false; }
        }

        private bool TryLocateCreatorThread()
        {
            if (CreatorCacheStillValid())
                return true;

            _creatorHashAddr = 0; _creatorStatics = 0; _creatorScript = null;
            foreach (string name in CreatorScriptNames)
            {
                foreach (long hit in ScanForUInt(Joaat(name)))
                {
                    if (!LooksLikeModulePointer(ReadPointer(hit - 0x10)))
                        continue;   // keine vtable -> Namens-/Hash-Tabelle, kein Objekt
                    long at;
                    long statics = FindStaticsPointerInThread(hit, name, out at);
                    if (statics == 0)
                        continue;
                    _creatorHashAddr = hit;
                    _creatorStatics = statics;
                    _creatorScript = name;
                    Log.Info($"Creator-Thread gefunden: {name}, Thread 0x{hit - 0x10:X}, Statics 0x{statics:X} (Hash+0x{at:X})");
                    return true;
                }
            }
            return false;
        }

        public long CreatorStaticsBase()
        {
            return TryLocateCreatorThread() ? _creatorStatics : 0;
        }

        public long CreatorWorkerBase()
        {
            if (!TryLocateCreatorThread())
                return 0;
            return _creatorStatics + WorkerOffsetFor(_creatorScript) * 8;
        }

        public long CreatorHeadingBase()
        {
            if (!TryLocateCreatorThread())
                return 0;
            return _creatorStatics + HeadingOffsetFor(_creatorScript) * 8;
        }

        private static long HeadingOffsetFor(string script)
        {
            switch (script)
            {
                case "fm_survival_creator": return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_survival;
                case "fm_capture_creator": return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_capture;
                case "fm_deathmatch_creator": return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_dm;
                case "fm_race_creator": return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_race;
                default: return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_lts;
            }
        }

        // Derive the thread-list global the way Legacy already has one, instead of scanning
        // memory at runtime forever.
        //
        //   1. locate a real creator thread (hash scan, as before)
        //   2. find the array slot that holds its address
        //   3. the array base is that slot minus i*8 for some small i -- try all of them
        //      and look for whichever value the module's data section actually stores
        //   4. that storage location is the global; its RVA is what an AOB has to resolve to
        //
        // Step 4's RVA is stable for a build, so the pattern can be built from it offline
        // and shipped in offsets.ini. After that Enhanced reads locals exactly like Legacy.
        private void BtnMapMoverDeriveptr_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            try
            {
                if (!TryLocateCreatorThread())
                {
                    tbStuffTestOutput.Text = "Kein Creator-Thread gefunden. Ist der Creator offen?";
                    return;
                }

                long thread = _creatorHashAddr - 0x10;
                sb.AppendLine($"Thread {_creatorScript} @ 0x{thread:X}");

                // 2. Wer haelt diesen Zeiger?
                byte[] tb = BitConverter.GetBytes(thread);
                string pat = string.Join(" ", tb.Select(x => x.ToString("X2")));
                var slots = Task.Run(() => m.AoBScan(pat, true, false)).GetAwaiter().GetResult()?.Take(12).ToList()
                            ?? new List<long>();
                sb.AppendLine($"Slots mit diesem Zeiger: {slots.Count}");

                // 3./4. Modul einmal einlesen, dann linear nach einer der moeglichen
                //       Array-Basen suchen.
                ProcessModule mod = m.getMainModule();
                long modBase = mod.BaseAddress.ToInt64();
                byte[] buf = m.memory(modBase.ToString("X")).GetBytes(mod.ModuleMemorySize);
                sb.AppendLine($"Modul 0x{modBase:X}, {buf.Length / 1024 / 1024} MB gelesen");

                bool found = false;
                foreach (long slot in slots)
                {
                    var bases = new HashSet<long>();
                    for (int i = 0; i < 2048; i++)
                        bases.Add(slot - i * 8L);

                    for (int off = 0; off + 8 <= buf.Length; off += 8)
                    {
                        long v = BitConverter.ToInt64(buf, off);
                        if (!bases.Contains(v))
                            continue;
                        long idx = (slot - v) / 8;
                        sb.AppendLine($"  TREFFER: Global bei RVA 0x{off:X}  ->  Array 0x{v:X}, Thread ist Index {idx}");
                        found = true;
                        break;
                    }
                    if (found) break;
                }
                if (!found)
                    sb.AppendLine("  kein Global im Modul gefunden, das auf dieses Array zeigt");

                DeriveImagePointer(sb, buf);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Abbruch: " + ex.Message);
            }

            tbStuffTestOutput.Text = sb.ToString();
            Log.Info("Pointer-Herleitung:\n" + sb);
        }

        // Same derivation for the job image. Legacy reads it as
        //
        //     X = object in the module (lea)   P = [X+0x18]   length = [P+0x18]   JPEG at P+0x1C
        //
        // so: find the job photo in memory by its JPEG header, confirm it by the length in
        // front of it and the FF D9 at its end, step back to P, and look for the module
        // location that stores P. That location minus 0x18 is X, and X's RVA is what the
        // img_ptr pattern has to resolve to.
        private void DeriveImagePointer(StringBuilder sb, byte[] modBuf)
        {
            sb.AppendLine("--- Job-Bild ---");
            long imgOff = GTA.Offsets.Editor.Image.img;           // 0x1C
            var heads = new List<long>();
            foreach (string head in new[] { "FF D8 FF E0 00 10 4A 46 49 46 00", "FF D8 FF E1" })
            {
                try
                {
                    var h = Task.Run(() => m.AoBScan(head, true, false)).GetAwaiter().GetResult();
                    if (h != null) heads.AddRange(h.Take(200));
                }
                catch { }
            }
            sb.AppendLine($"JPEG-Koepfe im Speicher: {heads.Count}");

            long modBase = m.getMainModule().BaseAddress.ToInt64();
            int shown = 0;
            foreach (long jpeg in heads)
            {
                int len = 0;
                try { len = m.memory((jpeg - 4).ToString("X")).Get<int>(); } catch { }
                if (len < 1024 || len > 4 * 1024 * 1024)
                    continue;
                byte[] tail = null;
                try { tail = m.memory((jpeg + len - 2).ToString("X")).GetBytes(2); } catch { }
                if (tail == null || tail.Length < 2 || tail[0] != 0xFF || tail[1] != 0xD9)
                    continue;

                long buffer = jpeg - imgOff;
                bool hit = false;
                for (int off = 0; off + 8 <= modBuf.Length; off += 8)
                {
                    if (BitConverter.ToInt64(modBuf, off) != buffer)
                        continue;
                    sb.AppendLine($"  TREFFER: JPEG {len} Bytes @ 0x{jpeg:X}, Puffer 0x{buffer:X}");
                    sb.AppendLine($"           Zeiger im Modul bei RVA 0x{off:X}  ->  Objekt X = RVA 0x{off - 0x18:X}");
                    hit = true;
                    break;
                }
                if (!hit && shown < 3)
                {
                    sb.AppendLine($"  JPEG {len} Bytes @ 0x{jpeg:X}: gueltig, aber kein Modul-Zeiger auf Puffer 0x{buffer:X}");
                    shown++;
                }
                if (hit)
                    return;
            }
            sb.AppendLine("  kein Job-Bild mit Modul-Zeiger gefunden (Job mit Bild geladen?)");
        }

        // --- Kalibrierung --------------------------------------------------------
        //
        // Two reference points are enough, because the map is a plain linear projection.
        // For each one: fly the creator camera there, then click that same spot on the map.
        private int _mapCalibStep;
        private Point _calibWorldA, _calibPixelA;

        private void BtnMapMoverCalib_Click(object sender, RoutedEventArgs e)
        {
            if (_mapCalibStep > 0)
            {
                _mapCalibStep = 0;
                tbmapmoverstatus.Text = "Kalibrierung abgebrochen.";
                return;
            }
            _mapCalibStep = 1;
            tbmapmoverstatus.Text = "Kalibrierung 1 von 2: mit der Creator-Kamera an eine gut erkennbare Stelle fliegen, "
                                  + "dann genau diese Stelle auf der Karte anklicken. Je weiter die zwei Punkte auseinander "
                                  + "liegen, desto genauer wird es.";
        }

        private void HandleCalibrationClick(Point pixel)
        {
            string src;
            Vector3? cam = ReadGamePosition(out src);
            if (cam == null)
            {
                tbmapmoverstatus.Text = "Position nicht lesbar -- Kalibrierung abgebrochen.";
                _mapCalibStep = 0;
                return;
            }

            if (_mapCalibStep == 1)
            {
                _calibWorldA = new Point(cam.Value.X, cam.Value.Y);
                _calibPixelA = pixel;
                _mapCalibStep = 2;
                tbmapmoverstatus.Text = $"Punkt 1 gesetzt (X {cam.Value.X:0}, Y {cam.Value.Y:0}). "
                                      + "Jetzt an eine zweite, weit entfernte Stelle fliegen und dort auf die Karte klicken.";
                return;
            }

            double dpx = pixel.X - _calibPixelA.X;
            double dpy = pixel.Y - _calibPixelA.Y;
            if (Math.Abs(dpx) < 20 || Math.Abs(dpy) < 20)
            {
                tbmapmoverstatus.Text = "Die zwei Punkte liegen zu dicht beieinander. Nimm einen deutlich weiter entfernten.";
                return;
            }

            double uppX = (cam.Value.X - _calibWorldA.X) / dpx;
            double uppY = (_calibWorldA.Y - cam.Value.Y) / dpy;
            if (!IsUsableScale(uppX) || !IsUsableScale(uppY))
            {
                _mapCalibStep = 0;
                tbmapmoverstatus.Text = "Die zwei Kamera-Positionen liegen zu dicht beieinander "
                                      + "oder sind identisch. Kalibrierung verworfen.";
                return;
            }

            _mapUnitsPerPxX = uppX;
            _mapOriginX = _calibWorldA.X - _calibPixelA.X * uppX;
            _mapUnitsPerPxY = uppY;
            _mapOriginY = _calibWorldA.Y + _calibPixelA.Y * uppY;

            _mapCalibStep = 0;
            SaveMapCalibration();
            RefreshMapMover(false);
            tbmapmoverstatus.Text = $"Kalibriert: {_mapUnitsPerPxX:0.##} Einheiten je Pixel in X, {_mapUnitsPerPxY:0.##} in Y. Gespeichert.";
            Log.Info($"Map Mover kalibriert: originX={_mapOriginX:0.##} uppX={_mapUnitsPerPxX:0.####} originY={_mapOriginY:0.##} uppY={_mapUnitsPerPxY:0.####}");
        }

        // --- Anzeige -------------------------------------------------------------

        private void RefreshMapMover(bool recomputeDelta)
        {
            MapMoverFamilies();

            Vector3? from = CurrentJobCentre();
            tbmapmoverfrom.Text = from.HasValue
                ? $"X {from.Value.X:0.0}   Y {from.Value.Y:0.0}   Z {from.Value.Z:0.0}"
                : "nichts geladen";
            tbmapmoverto.Text = _mapMoverTo.HasValue
                ? $"X {_mapMoverTo.Value.X:0.0}   Y {_mapMoverTo.Value.Y:0.0}   Z {_mapMoverTo.Value.Z:0.0}"
                : "noch nicht gesetzt";

            if (recomputeDelta && from.HasValue && _mapMoverTo.HasValue)
            {
                _mapMoverUpdating = true;
                Vector3 a = from.Value, b = _mapMoverTo.Value;
                tbmapmoverdx.Text = (b.X - a.X).ToString("0.###");
                tbmapmoverdy.Text = (b.Y - a.Y).ToString("0.###");
                if (b.Z != 0f)
                    tbmapmoverdz.Text = (b.Z - a.Z).ToString("0.###");
                _mapMoverUpdating = false;
                MapMoverDelta_Changed(null, null);
            }

            DrawMapMoverMarkers(from);
        }

        private void MapMoverDelta_Changed(object sender, TextChangedEventArgs e)
        {
            if (_mapMoverUpdating || tbmapmoverstatus == null || _mapCalibStep > 0)
                return;
            Vector3 d = ReadMapMoverDelta();
            double dist = Math.Sqrt(d.X * d.X + d.Y * d.Y);
            tbmapmoverstatus.Text = dist > 0 ? $"Distanz {dist:0} m" : "";
        }

        private Vector3 ReadMapMoverDelta()
        {
            float Parse(TextBox tb)
            {
                float v;
                return float.TryParse(tb.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out v) ? v : 0f;
            }
            return new Vector3(Parse(tbmapmoverdx), Parse(tbmapmoverdy), Parse(tbmapmoverdz));
        }

        // --- Ausfuehren ----------------------------------------------------------

        private int ApplyMapMove(Vector3 delta)
        {
            int moved = 0;
            foreach (MoveFamily fam in MapMoverFamilies())
            {
                if (fam.Toggle?.IsChecked != true)
                    continue;
                for (int axis = 0; axis < 3; axis++)
                {
                    float d = axis == 0 ? delta.X : axis == 1 ? delta.Y : delta.Z;
                    if (d == 0f)
                        continue;
                    foreach (MoveSlot slot in fam.Slots(axis))
                    {
                        int count = new Global(slot.CountIndex).Get<int>();
                        if (count <= 0 || count > 10000)
                            continue;
                        for (int i = 0; i < count; i++)
                        {
                            Global g = new Global(slot.Offset + (i * slot.Next));
                            g.SetFloat(g.Get<float>() + d);
                            moved++;
                        }
                    }
                }
            }
            return moved;
        }

        private void BtnMapMoverApply_Click(object sender, RoutedEventArgs e)
        {
            if (!m.IsProcOpen) { tbmapmoverstatus.Text = "GTA laeuft nicht."; return; }

            Vector3 delta = ReadMapMoverDelta();
            if (delta.X == 0f && delta.Y == 0f && delta.Z == 0f)
            {
                tbmapmoverstatus.Text = "Versatz ist 0 -- nichts zu tun.";
                return;
            }

            int moved = ApplyMapMove(delta);
            if (moved == 0)
            {
                tbmapmoverstatus.Text = "Nichts verschoben. Ist etwas angehakt und ein Job geladen?";
                return;
            }

            _mapMoverLastDelta = delta;
            _mapMoverHasUndo = true;
            BtnMapMoverUndo.IsEnabled = true;
            BtnMapMoverUndo.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];

            _mapMoverUpdating = true;
            tbmapmoverdx.Text = "0"; tbmapmoverdy.Text = "0"; tbmapmoverdz.Text = "0";
            _mapMoverUpdating = false;
            RefreshMapMover(false);

            tbmapmoverstatus.Text = $"{moved} Werte verschoben  (X {delta.X:+0.#;-0.#;0}  Y {delta.Y:+0.#;-0.#;0}  Z {delta.Z:+0.#;-0.#;0}).";
            Log.Info($"Map Mover: {moved} Werte  dX={delta.X} dY={delta.Y} dZ={delta.Z}");
        }

        private void BtnMapMoverUndo_Click(object sender, RoutedEventArgs e)
        {
            if (!_mapMoverHasUndo) return;
            if (!m.IsProcOpen) { tbmapmoverstatus.Text = "GTA laeuft nicht."; return; }

            int moved = ApplyMapMove(new Vector3(-_mapMoverLastDelta.X, -_mapMoverLastDelta.Y, -_mapMoverLastDelta.Z));
            _mapMoverHasUndo = false;
            BtnMapMoverUndo.IsEnabled = false;
            BtnMapMoverUndo.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            RefreshMapMover(false);
            tbmapmoverstatus.Text = $"{moved} Werte zurueckgesetzt.";
        }

        private void SetMapMoverChecks(bool state)
        {
            foreach (MoveFamily fam in MapMoverFamilies())
                if (fam.Toggle != null)
                    fam.Toggle.IsChecked = state;
            RefreshMapMover(false);
        }

        private void BtnModMapMover_Click(object sender, RoutedEventArgs e)
        {
            PageInnerMod.SelectedItem = PageInnerModMapMover;
            RefreshMapMover(false);
        }

        private void BtnMapMoverAll_Click(object sender, RoutedEventArgs e) => SetMapMoverChecks(true);
        private void BtnMapMoverNone_Click(object sender, RoutedEventArgs e) => SetMapMoverChecks(false);

        private void tbsettingsincrementsize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbsettingsincrementsize.Text))
                incrementsize = Convert.ToSingle(tbsettingsincrementsize.Text);
        }
    }
}
