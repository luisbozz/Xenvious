using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xenvious.AdvancedPlacement
{
    /// <summary>
    /// A saved setup for advanced placement: a prop plus either the quick-start choices
    /// ("quick") or the exact repeat step ("expert", for shapes the quick start cannot
    /// express, like a tilted plane or a skewed axis). Never a position: a preset is
    /// placed wherever the user starts it.
    /// </summary>
    public sealed class PlacementPreset
    {
        [JsonProperty("name")] public string Name { get; set; } = "";
        /// <summary>Translation key of a built-in preset's name; user presets have none.</summary>
        [JsonProperty("nameKey", NullValueHandling = NullValueHandling.Ignore)] public string NameKey { get; set; }
        [JsonProperty("kind")] public string Kind { get; set; } = "quick";
        [JsonProperty("shape")] public string Shape { get; set; } = "Curve";
        [JsonProperty("model")] public int Model { get; set; }
        [JsonProperty("count")] public int Count { get; set; } = 10;

        // quick
        [JsonProperty("turnLeft", NullValueHandling = NullValueHandling.Ignore)] public bool? TurnLeft { get; set; }
        [JsonProperty("radius", NullValueHandling = NullValueHandling.Ignore)] public double? Radius { get; set; }
        [JsonProperty("rise", NullValueHandling = NullValueHandling.Ignore)] public double? Rise { get; set; }
        [JsonProperty("bank", NullValueHandling = NullValueHandling.Ignore)] public double? Bank { get; set; }
        [JsonProperty("overlap", NullValueHandling = NullValueHandling.Ignore)] public double? Overlap { get; set; }
        [JsonProperty("autoPitch", NullValueHandling = NullValueHandling.Ignore)] public bool? AutoPitch { get; set; }

        // first piece (both kinds)
        [JsonProperty("pitch", NullValueHandling = NullValueHandling.Ignore)] public double? Pitch { get; set; }
        [JsonProperty("roll", NullValueHandling = NullValueHandling.Ignore)] public double? Roll { get; set; }

        // expert
        [JsonProperty("axis", NullValueHandling = NullValueHandling.Ignore)] public string Axis { get; set; }
        [JsonProperty("customAxis", NullValueHandling = NullValueHandling.Ignore)] public double[] CustomAxis { get; set; }
        [JsonProperty("pivot", NullValueHandling = NullValueHandling.Ignore)] public double[] Pivot { get; set; }
        [JsonProperty("angle", NullValueHandling = NullValueHandling.Ignore)] public double? Angle { get; set; }
        [JsonProperty("advance", NullValueHandling = NullValueHandling.Ignore)] public double? Advance { get; set; }
        [JsonProperty("offset", NullValueHandling = NullValueHandling.Ignore)] public double[] Offset { get; set; }

        /// <summary>Shipped with Xenvious (read-only), as opposed to the user's own.</summary>
        [JsonIgnore] public bool BuiltIn { get; set; }

        [JsonIgnore] public bool IsExpert => string.Equals(Kind, "expert", StringComparison.OrdinalIgnoreCase);

        public RepeatStep ToStep()
        {
            var step = new RepeatStep
            {
                Axis = Enum.TryParse(Axis, out StepAxis axis) ? axis : StepAxis.None,
                AngleDeg = Angle ?? 0,
                Advance = Advance ?? 0,
                Pivot = Vec(Pivot),
                Offset = Vec(Offset)
            };
            if (CustomAxis != null && CustomAxis.Length == 3)
            {
                step.CustomAxis = Vec(CustomAxis);
            }
            return step;
        }

        public static double[] Arr(V3 v) => new[] { Math.Round(v.X, 5), Math.Round(v.Y, 5), Math.Round(v.Z, 5) };

        private static V3 Vec(double[] a) => a != null && a.Length == 3 ? new V3(a[0], a[1], a[2]) : V3.Zero;
    }

    /// <summary>Built-in presets from OfflineData plus the user's own in %AppData%\Xenvious.</summary>
    public static class PlacementPresetStore
    {
        public const string FileFilter = "Xenvious presets (*.xvpresets)|*.xvpresets|JSON (*.json)|*.json";

        private static string UserFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "placement_presets.json");

        public static List<PlacementPreset> LoadBuiltIn()
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<PlacementPreset>>(OfflineData.PlacementPresets) ?? new List<PlacementPreset>();
                list.ForEach(p => p.BuiltIn = true);
                return list;
            }
            catch
            {
                return new List<PlacementPreset>();
            }
        }

        public static List<PlacementPreset> LoadUser()
        {
            try
            {
                return File.Exists(UserFile)
                    ? JsonConvert.DeserializeObject<List<PlacementPreset>>(File.ReadAllText(UserFile)) ?? new List<PlacementPreset>()
                    : new List<PlacementPreset>();
            }
            catch
            {
                return new List<PlacementPreset>();
            }
        }

        public static void SaveUser(IEnumerable<PlacementPreset> presets)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UserFile));
            File.WriteAllText(UserFile, JsonConvert.SerializeObject(presets.Where(p => !p.BuiltIn).ToList(), Formatting.Indented));
        }

        public static void Export(string file, IEnumerable<PlacementPreset> presets)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(presets.ToList(), Formatting.Indented));
        }

        /// <summary>Presets from a file; entries without a model or name are dropped.</summary>
        public static List<PlacementPreset> Import(string file)
        {
            var list = JsonConvert.DeserializeObject<List<PlacementPreset>>(File.ReadAllText(file)) ?? new List<PlacementPreset>();
            return list.Where(p => p != null && p.Model != 0 && !string.IsNullOrWhiteSpace(p.Name)).ToList();
        }
    }
}
