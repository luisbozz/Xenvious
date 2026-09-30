using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    /// <summary>
    /// The Race Creator's available vehicles. Each class has base vehicles, one bit each in
    /// aveh[class] (bit set = blocked), then DLC vehicles, 31 per word in adlc[class][0..3]
    /// (bit set = allowed). clbs has one bit per class that is in the race. The lists come
    /// from the creator script (ysc-global-updater tools/extract_race_vehicles.py), so list
    /// index = bit index, per edition.
    /// </summary>
    public static class RaceVehicles
    {
        public class Vehicle
        {
            [JsonProperty("hash")] public uint Hash;
            [JsonProperty("model")] public string Model;
            [JsonProperty("names")] public Dictionary<string, string> Names;

            [JsonIgnore] public bool Dlc;
            [JsonIgnore] public int Bit;            // index inside base or dlc
            [JsonIgnore] public int StartIndex;     // index inside base + dlc, what ivm stores

            public string Name(string language)
            {
                if (Names != null && Names.TryGetValue(language, out var name) && !string.IsNullOrEmpty(name))
                    return name;
                return Model ?? "0x" + Hash.ToString("X8");
            }
        }

        public class VehicleClass
        {
            [JsonProperty("class")] public int Index;
            [JsonProperty("base")] public List<Vehicle> Base = new List<Vehicle>();
            [JsonProperty("dlc")] public List<Vehicle> Dlc = new List<Vehicle>();

            [JsonIgnore] public IEnumerable<Vehicle> All => Base.Concat(Dlc);
        }

        // DLC bits per adlc word; bit 31 is not used by the script.
        private const int BitsPerWord = 31;
        public const int DlcWords = 4;

        private static List<VehicleClass> _classes;

        public static IReadOnlyList<VehicleClass> Classes
        {
            get
            {
                if (_classes == null)
                {
                    _classes = JsonConvert.DeserializeObject<List<VehicleClass>>(OfflineData.RaceVehicles) ?? new List<VehicleClass>();
                    foreach (var c in _classes)
                    {
                        for (int i = 0; i < c.Base.Count; i++)
                        {
                            c.Base[i].Bit = i;
                            c.Base[i].StartIndex = i;
                        }
                        for (int i = 0; i < c.Dlc.Count; i++)
                        {
                            c.Dlc[i].Dlc = true;
                            c.Dlc[i].Bit = i;
                            c.Dlc[i].StartIndex = c.Base.Count + i;
                        }
                    }
                }
                return _classes;
            }
        }

        /// <summary>Xenvious language code (de, en, ...) to the key in the vehicle names.</summary>
        public static string NameLanguage(string code)
        {
            switch (code)
            {
                case "de": return "ger";
                case "en": return "eng";
                default: return code;
            }
        }

        public static bool Ready => MainWindow.m != null && MainWindow.m.IsProcOpen && Race.aveh != 0 && Race.adlc != 0 && Race.adlc_NEXT != 0 && Race.Checkpoints.clbs != 0;

        private static long AvehAddress(int cls) => Race.aveh + cls;
        private static long AdlcAddress(int cls, int word) => Race.adlc + cls * Race.adlc_NEXT + word;

        public static int ClassBits => new Global(Race.Checkpoints.clbs).Get<int>();

        public static bool ClassOn(int cls) => (ClassBits & (1 << cls)) != 0;

        public static void SetClass(int cls, bool on) => SetBit(Race.Checkpoints.clbs, cls, on);

        /// <summary>A class's vehicle bits, read once so a whole class needs five reads.</summary>
        public class ClassState
        {
            public int Aveh;
            public int[] Adlc = new int[DlcWords];

            public bool Allowed(Vehicle v)
            {
                if (!v.Dlc)
                    return (Aveh & (1 << v.Bit)) == 0;
                int word = v.Bit / BitsPerWord;
                return word < DlcWords && (Adlc[word] & (1 << (v.Bit % BitsPerWord))) != 0;
            }
        }

        public static ClassState Read(int cls)
        {
            var state = new ClassState { Aveh = new Global(AvehAddress(cls)).Get<int>() };
            for (int word = 0; word < DlcWords; word++)
                state.Adlc[word] = new Global(AdlcAddress(cls, word)).Get<int>();
            return state;
        }

        public static void SetAllowed(int cls, Vehicle v, bool allowed)
        {
            if (!v.Dlc)
                SetBit(AvehAddress(cls), v.Bit, !allowed);
            else
                SetBit(AdlcAddress(cls, v.Bit / BitsPerWord), v.Bit % BitsPerWord, allowed);
        }

        /// <summary>Allows or blocks every vehicle of a class with one write per word.</summary>
        public static void SetAllAllowed(VehicleClass c, bool allowed)
        {
            if (c.Base.Count > 0)
            {
                var g = new Global(AvehAddress(c.Index));
                int mask = c.Base.Count >= 32 ? -1 : (1 << c.Base.Count) - 1;
                int v = g.Get<int>();
                g.SetInt(allowed ? v & ~mask : v | mask);
            }
            for (int word = 0; word * BitsPerWord < c.Dlc.Count && word < DlcWords; word++)
            {
                int count = Math.Min(BitsPerWord, c.Dlc.Count - word * BitsPerWord);
                int mask = (1 << count) - 1;
                var g = new Global(AdlcAddress(c.Index, word));
                int v = g.Get<int>();
                g.SetInt(allowed ? v | mask : v & ~mask);
            }
        }

        private static void SetBit(long address, int bit, bool on)
        {
            var g = new Global(address);
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << bit) : v & ~(1 << bit));
        }

        public static int StartClass => new Global(Race.Checkpoints.icv).Get<int>();

        /// <summary>
        /// Sets the start class the way the creator's menu does: the start vehicle becomes the
        /// class's first allowed vehicle (the script sets ivm to 0 and skips blocked ones).
        /// </summary>
        public static void SetStartClass(int cls)
        {
            new Global(Race.Checkpoints.icv).SetInt(cls);
            var c = Classes.FirstOrDefault(x => x.Index == cls);
            if (c == null)
                return;
            var state = Read(cls);
            var first = c.All.FirstOrDefault(state.Allowed) ?? c.All.FirstOrDefault();
            SetStartVehicle(first?.StartIndex ?? 0);
        }

        public static int StartVehicle => ivm != 0 ? new Global(ivm).Get<int>() : -1;

        // ivm is the index into the class's base vehicles followed by its DLC vehicles. The
        // creator swaps its preview vehicle when the value no longer matches it.
        public static void SetStartVehicle(int index)
        {
            if (ivm != 0)
                new Global(ivm).SetInt(index);
        }

        public static int RaceType => racetype != 0 ? new Global(racetype).Get<int>() : -1;

        public static int GridSize => Race.Checkpoints.gridty != 0 ? new Global(Race.Checkpoints.gridty).Get<int>() : -1;
    }
}
