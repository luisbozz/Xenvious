using System;
using System.Numerics;

namespace Xenvious
{
    /// <summary>
    /// The variables Xenvious shares with the injected creator functions (customfuncs): the
    /// dispatch bits, model dimensions, the hovered model, the race test tuning and the garage.
    ///
    /// A creator with a data page (ScriptSpace) keeps them there, variable n at 0x18 + n * 8, so
    /// every creator script has its own set and nothing else in the game uses that memory. The
    /// page starts empty whenever the game loads the script again. A creator without a data page
    /// (Legacy) uses the custom globals from offsets.ini, which the game resets when it rebuilds
    /// its globals. Either way the dispatch bits are put back every second by the worker.
    /// The numbering follows the custom globals (Global_2884084 + n), so the payload sources
    /// convert one to one (scrasm/customfuncs/page in ysc-global-updater).
    /// </summary>
    public static class ScriptVars
    {
        public const int Check = 0, HoveredModel = 1, DimensionModel = 2, DimensionMin = 3, DimensionMax = 6;
        /// <summary>Six slots: mod slot, value, option count, current value, model, vehicle.</summary>
        public const int Tune = 9;
        public const int GarageSlot = 15, GarageResult = 16;
        /// <summary>100 slots: the model of each garage slot of one scan request.</summary>
        public const int GarageList = 19;
        private const int VarsAt = 0x18;

        /// <summary>The "experimental script features" setting; the dev patches follow it.</summary>
        public static volatile bool FeaturesOn;
        /// <summary>The "switch camera with caps lock" setting.</summary>
        public static volatile bool CameraKey;

        // Finding a script's data page walks the script table, so it is looked up once a second
        // at most. A script freed and loaded again within that second would keep a stale page,
        // but leaving a creator and loading it fresh takes far longer.
        private static readonly object Gate = new object();
        private static string pageScript = "";
        private static long page;
        private static DateTime pageAt;

        /// <summary>Address of a variable (slot counts 8 byte slots) for the creator running now; 0 without one.</summary>
        public static long Address(int var, int slot = 0)
        {
            if (MainWindow.m == null || !MainWindow.m.IsProcOpen)
                return 0;
            string script = GTA.CurrentCreatorName();
            if (script == "")
                return 0;
            if (!ScriptSpace.Configured(script))
            {
                long global = CustomGlobal(var);
                return global == 0 ? 0 : new Global(global + slot).GetAddress();
            }
            lock (Gate)
            {
                if (script != pageScript || DateTime.UtcNow - pageAt > TimeSpan.FromSeconds(1))
                {
                    page = ScriptSpace.Get(script)?.Data ?? 0;
                    pageScript = script;
                    pageAt = DateTime.UtcNow;
                }
                return page == 0 ? 0 : page + VarsAt + (var + slot) * 8;
            }
        }

        // The custom global of a variable; 0 when offsets.ini has none.
        private static long CustomGlobal(int var)
        {
            switch (var)
            {
                case Check: return GTA.Offsets.Editor.custom_check;
                case HoveredModel: return GTA.Offsets.Editor.custom_hovered_model;
                case DimensionModel: return GTA.Offsets.Editor.custom_dimension_model;
                case DimensionMin: return GTA.Offsets.Editor.custom_dimension_min;
                case DimensionMax: return GTA.Offsets.Editor.custom_dimension_max;
                case Tune: return GTA.Offsets.Editor.custom_tune;
                case GarageSlot: return GTA.Offsets.Editor.custom_pv_slot;
                case GarageResult: return GTA.Offsets.Editor.custom_pv_result;
                case GarageList: return GTA.Offsets.Editor.custom_pv_list;
                default: return 0;
            }
        }

        public static T Get<T>(int var, int slot = 0) where T : struct
        {
            long address = Address(var, slot);
            return address == 0 ? default : MainWindow.m.memory(address.ToString("X")).Get<T>();
        }

        public static bool Set<T>(int var, T value, int slot = 0) where T : struct
        {
            long address = Address(var, slot);
            return address != 0 && MainWindow.m.memory(address.ToString("X")).Write(value);
        }

        public static Vector3 GetVector3(int var)
        {
            long address = Address(var);
            return address == 0 ? Vector3.Zero : MainWindow.m.memory(address.ToString("X")).GetVector3();
        }

        /// <summary>A dispatch bit of <see cref="Check"/>, counted from 1 like the helpers in Functions.</summary>
        public static bool IsBitSet(int bit) => (Get<int>(Check) & (1 << (bit - 1))) != 0;

        /// <summary>Sets or clears a dispatch bit (counted from 1); writes only when it changes.</summary>
        public static bool SetBit(int bit, bool on)
        {
            long address = Address(Check);
            if (address == 0)
                return false;
            var memory = MainWindow.m.memory(address.ToString("X"));
            int value = memory.Get<int>();
            int wanted = on ? value | (1 << (bit - 1)) : value & ~(1 << (bit - 1));
            return wanted == value || memory.Write(wanted);
        }
    }
}
