using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Tunes the vehicle the player sits in while testing a race. The race creator itself never
    /// mods its test vehicle (it only creates it and sets proofs, CREATE_VEHICLE in the test
    /// start), so the injected race customfuncs carry fn7: one request per frame on custom_check
    /// bit 5 (helper bit 6). Xenvious writes slot and value to custom_tune, sets the bit, and the
    /// function sets the mod (or only reads it for value -99), writes back the option count, the
    /// current value, model and vehicle handle, and clears the bit.
    ///
    /// The function is part of the dev customfuncs payload, so it only exists while the script
    /// features are on (custom_check bit 30). Without it the bit never clears; Request then
    /// times out and reports false.
    /// </summary>
    public static class TestVehicleTune
    {
        public const int ReadOnly = -99;
        /// <summary>A wanted value meaning "the highest option of this model", resolved when applied.</summary>
        public const int Max = -2;
        public const int ModSlots = 50;
        public const int WheelType = 50, Primary = 51, Secondary = 52, Pearl = 53, WheelColour = 54, WindowTint = 55, XenonColour = 56, Livery = 57;
        public const int Turbo = 18, TyreSmoke = 20, Xenon = 22, FrontWheels = 23, BackWheels = 24, LiveryMod = 48;

        // custom_check bit 5 of the bytecode; the helpers count from 1.
        private const int RequestBit = 6;
        private const int ScriptFeaturesBit = 30;

        private static readonly object Gate = new object();

        public struct Answer
        {
            public int Count, Current, Model, Vehicle;
        }

        /// <summary>The slots that switch on and off instead of picking an option.</summary>
        public static bool IsToggle(int slot) => slot == Turbo || slot == TyreSmoke || slot == Xenon;

        public static bool Ready => MainWindow.m != null && MainWindow.m.IsProcOpen
            && GTA.Offsets.Editor.custom_check != 0 && GTA.Offsets.Editor.custom_tune != 0;

        public static bool InRaceCreator => Ready && GTA.CurrentCreatorName() == "fm_race_creator";

        public static bool ScriptFeaturesOn => Ready && Functions.Read.checkbinary(ScriptFeaturesBit, GTA.Offsets.Editor.custom_check);

        /// <summary>
        /// Sends one request and waits for the script to answer (it runs once per frame).
        /// Blocks the calling thread, so call it off the UI thread.
        /// </summary>
        public static bool Request(int slot, int value, out Answer answer)
        {
            answer = default;
            long tune = GTA.Offsets.Editor.custom_tune;
            if (!Run(RequestBit, () =>
                {
                    new Global(tune).SetInt(slot);
                    new Global(tune + 1).SetInt(value);
                }))
                return false;
            answer.Count = new Global(tune + 2).Get<int>();
            answer.Current = new Global(tune + 3).Get<int>();
            answer.Model = new Global(tune + 4).Get<int>();
            answer.Vehicle = new Global(tune + 5).Get<int>();
            return true;
        }

        // Writes the inputs, sets the function's bit and waits until the function cleared it.
        private static bool Run(int bit, Action write)
        {
            if (!InRaceCreator)
                return false;
            lock (Gate)
            {
                long check = GTA.Offsets.Editor.custom_check;
                // A request still pending (the function is not there, or the game is paused)
                // is dropped first, so it does not apply a stale value later.
                Functions.Write.writebinary(bit, check, false);
                write();
                Functions.Write.writebinary(bit, check, true);

                var clock = Stopwatch.StartNew();
                while (Functions.Read.checkbinary(bit, check))
                {
                    if (clock.ElapsedMilliseconds > 2000 || !MainWindow.m.IsProcOpen)
                    {
                        Functions.Write.writebinary(bit, check, false);
                        return false;
                    }
                    Thread.Sleep(4);
                }
                return true;
            }
        }

        // ----- online vehicles -----
        //
        // The creator does not load the garage (freemode fills the personal vehicle array, and
        // freemode does not run in the creator), but the MPSV stats are there. pvscan reads the
        // model stat of 100 garage slots per request; pvload reads one slot's MPSV stats into a
        // struct in the custom globals, the way maintransition loads the garage, and puts it on
        // the vehicle the player sits in with the creator's own personal vehicle setup
        // (func_489: mods, paint, wheels, neon, tint, liveries). Neither writes a stat.

        private const int ScanBit = 7, LoadBit = 8;
        public const int GarageSlots = 600;

        public struct OnlineVehicle
        {
            public int Slot, Model;
        }

        public enum LoadResult { Applied, OtherModel, NoVehicle, NoAnswer }

        public static bool OnlineReady => InRaceCreator && GTA.Offsets.Editor.custom_pv_slot != 0
            && GTA.Offsets.Editor.custom_pv_result != 0 && GTA.Offsets.Editor.custom_pv_list != 0;

        /// <summary>The garage slots that hold a vehicle; null when the script does not answer.</summary>
        public static List<OnlineVehicle> ScanGarage()
        {
            if (!OnlineReady)
                return null;
            var list = new List<OnlineVehicle>();
            for (int start = 0; start < GarageSlots; start += 100)
            {
                if (!Run(ScanBit, () => new Global(GTA.Offsets.Editor.custom_pv_slot).SetInt(start)))
                    return null;
                for (int i = 0; i < 100; i++)
                {
                    int model = new Global(GTA.Offsets.Editor.custom_pv_list + i).Get<int>();
                    if (model != 0)
                        list.Add(new OnlineVehicle { Slot = start + i, Model = model });
                }
            }
            return list;
        }

        /// <summary>Loads a garage slot and puts it on the current vehicle when the model matches.</summary>
        public static LoadResult LoadOnto(int slot)
        {
            if (!OnlineReady || !Run(LoadBit, () => new Global(GTA.Offsets.Editor.custom_pv_slot).SetInt(slot)))
                return LoadResult.NoAnswer;
            switch (new Global(GTA.Offsets.Editor.custom_pv_result).Get<int>())
            {
                case 1: return LoadResult.Applied;
                case 0: return LoadResult.OtherModel;
                default: return LoadResult.NoVehicle;
            }
        }

        /// <summary>Reads every slot of the current vehicle; null when the script does not answer.</summary>
        public static Dictionary<int, Answer> ReadAll()
        {
            var all = new Dictionary<int, Answer>();
            for (int slot = 0; slot <= Livery; slot++)
            {
                if (!Request(slot, ReadOnly, out var a))
                    return null;
                if (a.Vehicle == 0)
                    return all;
                all[slot] = a;
            }
            return all;
        }

        /// <summary>
        /// Writes several slots; the wheel type goes first, because it decides which wheels
        /// the wheel slots pick from.
        /// </summary>
        public static bool Apply(IEnumerable<KeyValuePair<int, int>> values)
        {
            var list = new List<KeyValuePair<int, int>>(values);
            // SET_VEHICLE_WHEEL_TYPE takes the wheels off, even for the type the vehicle already
            // has; kept changes and presets put on after the online vehicle would strip its wheels.
            int type = list.FindIndex(kv => kv.Key == WheelType);
            if (type >= 0 && Request(WheelType, ReadOnly, out var now) && now.Current == list[type].Value)
                list.RemoveAt(type);
            list.Sort((a, b) => (a.Key == WheelType ? -1 : a.Key).CompareTo(b.Key == WheelType ? -1 : b.Key));
            foreach (var kv in list)
            {
                int value = kv.Value;
                if (value == Max)
                {
                    if (!Request(kv.Key, ReadOnly, out var a))
                        return false;
                    if (a.Count <= 0)
                        continue;
                    value = a.Count - 1;
                }
                if (!Request(kv.Key, value, out _))
                    return false;
            }
            return true;
        }
    }

    /// <summary>A named set of tuning values (slot -> value) for a model.</summary>
    public class TunePreset
    {
        public string Name { get; set; }
        public int Model { get; set; }
        public Dictionary<int, int> Values { get; set; } = new Dictionary<int, int>();
        /// <summary>File name of the picture in the presets' image folder, null without one.</summary>
        public string Image { get; set; }
        public DateTime Saved { get; set; }
    }

    /// <summary>
    /// What the tuning page keeps in %AppData%\Xenvious: the user's presets, and the option counts
    /// of every model driven in a test, so the page can offer a model's parts before the test.
    /// </summary>
    public static class TuneStore
    {
        private static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious");
        private static string PresetFile => Path.Combine(Folder, "tune_presets.json");
        private static string CountFile => Path.Combine(Folder, "tune_counts.json");

        public static List<TunePreset> LoadPresets() => Read<List<TunePreset>>(PresetFile) ?? new List<TunePreset>();

        public static void SavePresets(IEnumerable<TunePreset> presets) => Write(PresetFile, presets.ToList());

        private static string ImageFolder => Path.Combine(Folder, "tune_presets");

        /// <summary>Stores a picture as a small JPEG and returns its file name; null on failure.</summary>
        public static string SaveImage(System.Windows.Media.Imaging.BitmapSource image)
        {
            if (image == null)
                return null;
            try
            {
                const double maxWidth = 480;
                if (image.PixelWidth > maxWidth)
                {
                    double scale = maxWidth / image.PixelWidth;
                    image = new System.Windows.Media.Imaging.TransformedBitmap(image, new System.Windows.Media.ScaleTransform(scale, scale));
                }
                // The JPEG encoder takes no alpha channel.
                image = new System.Windows.Media.Imaging.FormatConvertedBitmap(image, System.Windows.Media.PixelFormats.Bgr24, null, 0);
                var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder { QualityLevel = 85 };
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                Directory.CreateDirectory(ImageFolder);
                string name = Guid.NewGuid().ToString("N") + ".jpg";
                using (var file = File.Create(Path.Combine(ImageFolder, name)))
                    encoder.Save(file);
                return name;
            }
            catch (Exception e)
            {
                Log.Error("Could not save a preset picture", e, source: "tune");
                return null;
            }
        }

        public static System.Windows.Media.Imaging.BitmapSource LoadImage(string name)
        {
            try
            {
                string file = string.IsNullOrEmpty(name) ? null : Path.Combine(ImageFolder, name);
                if (file == null || !File.Exists(file))
                    return null;
                var image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(file);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public static void DeleteImage(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                    File.Delete(Path.Combine(ImageFolder, name));
            }
            catch { }
        }

        public static Dictionary<int, Dictionary<int, int>> LoadCounts() => Read<Dictionary<int, Dictionary<int, int>>>(CountFile) ?? new Dictionary<int, Dictionary<int, int>>();

        public static void SaveCounts(Dictionary<int, Dictionary<int, int>> counts) => Write(CountFile, counts);

        private static T Read<T>(string file) where T : class
        {
            try
            {
                return File.Exists(file) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(file)) : null;
            }
            catch (Exception e)
            {
                Log.Error("Could not read " + file, e, source: "tune");
                return null;
            }
        }

        private static void Write(string file, object value)
        {
            try
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(file, JsonConvert.SerializeObject(value, Formatting.Indented));
            }
            catch (Exception e)
            {
                Log.Error("Could not write " + file, e, source: "tune");
            }
        }
    }
}
