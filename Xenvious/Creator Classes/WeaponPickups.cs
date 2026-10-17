using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Xenvious
{
    /// <summary>
    /// What a placed weapon gives when it is collected, beyond the plain weapon: script pickups
    /// (power-ups, mission items, ammo), vehicle pickups, the ammo amount and weapon components.
    ///
    /// The mission controller (Mission, LTS, Capture) reads (fm_mission_controller_2020, 1.73):
    /// - type (f_16) pickup_custom_script: rput (f_24) picks the script pickup, wcpm (f_25) its
    ///   variant. The pickup's model, blip and name follow from these.
    /// - type pickup_vehicle_custom_script(_low_glow): vput (f_26) picks the vehicle pickup.
    /// - clip (f_7): magazines; the player gets clip x the weapon's clip size, 0 = the weapon's
    ///   default ammo. For the ammo pickup (rput 15) it is the amount, bit 28 of bits = for all
    ///   weapons instead of the one in hand.
    /// - comps (f_17, 6 slots): components given to the player's weapon on collection.
    /// Of the creators only the Deathmatch Creator offers the script and vehicle pickups in its
    /// menu; the others keep them as they are in the job.
    /// </summary>
    public static class WeaponPickups
    {
        public enum Kind { Weapon, Script, Vehicle }

        public static readonly int CustomScript = unchecked((int)Functions.joaat("pickup_custom_script"));
        public static readonly int VehicleScript = unchecked((int)Functions.joaat("pickup_vehicle_custom_script"));
        public static readonly int VehicleScriptLowGlow = unchecked((int)Functions.joaat("pickup_vehicle_custom_script_low_glow"));
        private static readonly int Pistol = unchecked((int)Functions.joaat("pickup_weapon_pistol"));
        private static readonly int Stungun = unchecked((int)Functions.joaat("pickup_weapon_stungun"));
        private static readonly int MarksmanRifle = unchecked((int)Functions.joaat("pickup_weapon_marksmanrifle"));

        public const int ComponentSlots = 6;
        private const int AllWeaponsBit = 28;

        /// <summary>Where the effect of a pickup comes from (checked in the 1.73 scripts).</summary>
        public enum Effect
        {
            /// <summary>Every mission controller runs it.</summary>
            Any,
            /// <summary>freemode sends the event only while dlcrel is below 2, and only the classic
            /// fm_mission_controller handles it (func_451 in freemode, -1484757921 / -1054853926).</summary>
            Classic,
            /// <summary>As Classic, and the controller drops it for teams 3 and 4 (Power Play had two).</summary>
            ClassicTwoTeams,
            /// <summary>Only in heist jobs (fm_mission_controller_2020 checks the heist bag option).</summary>
            Heist,
        }

        /// <summary>One choice for rput or vput: its value, the translation key of its game name, the
        /// group it is listed in, its blip icon (Images/pickups) and the variants (wcpm) it has.</summary>
        public sealed class PickupType
        {
            public int Value;
            public string Key;
            public string Fallback;
            public string Group;
            public string Model;
            public string Icon;
            public Effect Effect;
            public (int Value, string Key, string Fallback)[] Variants = new (int, string, string)[0];
        }

        // rput: names and icons are the controller's blips (func_10132 sprites, BLIP_* labels).
        public static readonly PickupType[] ScriptTypes =
        {
            Type(10, "Life", "item", "stt_prop_lives_bottle", "weapon_lives", Effect.Any,
                (0, "wp_var_bottle", "Bottle"), (1, "wp_var_other", "Other model")),
            Type(12, "Equipment", "item", "h4_prop_h4_bolt_cutter_01a", "nhp_bag", Effect.Any,
                Enumerable.Range(0, 7).Select(i => (i, "wp_me_" + i, "Equipment " + (i + 1))).ToArray()),
            Type(13, "Extra Take", "item", "h4_prop_h4_cash_stack_02a", "cash_pickup", Effect.Any),
            Type(14, "USB stick", "item", "hei_prop_hst_usb_drive", "hacking_device", Effect.Any),
            Type(15, "Weapon Ammo", "item", "prop_box_ammo02a", "weapon_ammo", Effect.Any),
            Type(11, "Weapon Bag", "item", "prop_cs_duffel_01", "nhp_bag", Effect.Heist,
                (0, "wp_var_duffel", "Duffel bag"), (1, "wp_var_binbag", "Bin bag")),
            Type(0, "Raging", "power", "prop_ex_b_shark", "testosterone", Effect.ClassicTwoTeams),
            Type(1, "Beasted", "power", "prop_ex_bmd", "pickup_beast", Effect.ClassicTwoTeams),
            Type(2, "Thermal Vision", "power", "prop_mk_thermal", "pickup_thermal", Effect.ClassicTwoTeams),
            Type(3, "Flipped", "power", "prop_ex_swap", "pickup_swap", Effect.ClassicTwoTeams),
            Type(4, "Doped", "power", "prop_ex_weed", "pickup_weed", Effect.ClassicTwoTeams),
            Type(5, "Slow Time", "power", "prop_mk_s_time", "pickup_slow_time", Effect.ClassicTwoTeams),
            Type(6, "Zoned", "power", "prop_ex_b_time", "pickup_zoned", Effect.ClassicTwoTeams),
            Type(7, "Dark", "power", "prop_ex_hidden", "pickup_hidden", Effect.ClassicTwoTeams),
            Type(8, "Random", "power", "prop_mk_random", "pickup_random", Effect.ClassicTwoTeams),
        };

        // vput: names and icons are the controller's blips (func_10133).
        public static readonly PickupType[] VehicleTypes =
        {
            VType(0, "Missiles", "weapon", "prop_ic_rock", "rockets"),
            VType(13, "Machine Gun", "weapon", "prop_ic_mguns_b", "pickup_machinegun"),
            VType(8, "Bomb", "weapon", "prop_ic_bomb", "pickup_bomb"),
            VType(7, "Detonator", "weapon", "prop_ic_deton", "pickup_detonator"),
            VType(10, "Deadline", "weapon", "prop_hx_deadl", "pickup_deadline"),
            VType(17, "Blast size", "weapon", null, "pickup_dtb_blast_increase"),
            VType(18, "Bomb count", "weapon", null, "pickup_dtb_bomb_increase"),
            VType(1, "Boost", "drive", "prop_ic_boost", "boost"),
            VType(4, "Jammed", "drive", "prop_ic_accel", "pickup_accelerator"),
            VType(9, "Hop", "drive", "prop_ic_hop", "pickup_jump"),
            VType(2, "Ghost", "drive", "prop_ic_ghost", "pickup_ghost"),
            VType(3, "Beast", "drive", "prop_hx_arm", "pickup_armoured"),
            VType(11, "Repair", "drive", "prop_ic_repair", "pickup_repair"),
            VType(19, "Health", "drive", null, "pickup_dtb_health"),
            VType(5, "Flipped", "other", "prop_ex_swap", "pickup_swap"),
            VType(6, "Zoned", "other", "prop_ex_b_time", "pickup_zoned"),
            VType(12, "Random", "other", "prop_ex_random", "pickup_random"),
            VType(14, "Random Special Vehicle", "other", "prop_hx_special_vehicle", "steeringwheel"),
            VType(15, "Ruiner 2000", "other", "prop_hx_special_ruiner", "ex_vech_3"),
            VType(16, "Ramp Buggy", "other", "prop_hx_special_buggy", "ex_vech_4"),
        };

        private static PickupType Type(int value, string name, string group, string model, string icon, Effect effect, params (int, string, string)[] variants) =>
            new PickupType { Value = value, Key = "wp_cs_" + value, Fallback = name, Group = group, Model = model, Icon = icon, Effect = effect, Variants = variants };

        private static PickupType VType(int value, string name, string group, string model, string icon) =>
            new PickupType { Value = value, Key = "wp_vp_" + value, Fallback = name, Group = group, Model = model, Icon = icon, Effect = Effect.Classic };

        /// <summary>The blip icon of a type as a pack URI; vput 17/18 show their lowering icon when they lower.</summary>
        public static string IconUri(PickupType type, bool lowers = false)
        {
            string icon = type.Icon;
            if (lowers && icon != null)
                icon = icon.Replace("_increase", "_decrease");
            return icon == null ? null : "pack://application:,,,/Images/pickups/radar_" + icon + ".png";
        }

        // ----- special vehicles (vput 14 to 16) -----

        /// <summary>How long the player stays a Ruiner 2000 (vput 15) or Ramp Buggy (vput 16), in
        /// seconds: job options vwp15 / vwp16. The classic controller turns the vehicle back after
        /// that many seconds (func_14064), so 0 turns it back at once. A new LTS starts with 15; a job
        /// file without the keys loads as 0.</summary>
        public static long SpecialSecondsField(int vehicleType) =>
            vehicleType == 15 ? GTA.Offsets.Editor.vwp15 : vehicleType == 16 ? GTA.Offsets.Editor.vwp16 : 0;

        public static int SpecialSeconds(int vehicleType)
        {
            long field = SpecialSecondsField(vehicleType);
            return field == 0 || !MainWindow.m.IsProcOpen ? -1 : new Global(field).Get<int>();
        }

        public static void SetSpecialSeconds(int vehicleType, int seconds)
        {
            long field = SpecialSecondsField(vehicleType);
            if (field != 0 && MainWindow.m.IsProcOpen)
                new Global(field).SetInt(Math.Max(0, seconds));
        }

        // ----- which mission controller runs the job -----

        /// <summary>dlcrel (-1 when unknown). Below 2 the classic fm_mission_controller runs the job.</summary>
        public static int ControllerRelease() =>
            GTA.Offsets.Editor.dlcrel == 0 || !MainWindow.m.IsProcOpen ? -1 : new Global(GTA.Offsets.Editor.dlcrel).Get<int>();

        public static bool ClassicController() => ControllerRelease() < 2;

        /// <summary>Sets dlcrel to 1: the classic controller. The Mission Creator only replaces -1 with
        /// 14 when it starts a new job, so 1 stays.</summary>
        public static void UseClassicController()
        {
            if (GTA.Offsets.Editor.dlcrel != 0 && MainWindow.m.IsProcOpen)
                new Global(GTA.Offsets.Editor.dlcrel).SetInt(1);
        }

        /// <summary>
        /// Job option menubs9 bit 5 (FMMC_VWO_ENB, vehicle weapon options). The classic controller
        /// loads the vehicle pickup models (func_17597), the power play effects and the vehicle
        /// weapon controls only with it (or with "disable vehicle weapons: pickups"). Without, the
        /// pickup is created before its model is in memory and shows the pickup type's own model,
        /// which looks like a health pack. Rockstar's jobs with these pickups have it on.
        /// </summary>
        public static bool VehiclePickupsLoaded() =>
            GTA.Offsets.Editor.menubs9 == 0 || !MainWindow.m.IsProcOpen
            || Functions.Read.checkbinary(5, GTA.Offsets.Editor.menubs9)
            || GTA.Offsets.Editor.menubs13 != 0 && Functions.Read.checkbinary(4, GTA.Offsets.Editor.menubs13);

        public static void LoadVehiclePickups()
        {
            if (GTA.Offsets.Editor.menubs9 != 0 && MainWindow.m.IsProcOpen)
                Functions.Write.writebinary(5, GTA.Offsets.Editor.menubs9, true);
        }

        // ----- the selected weapon in memory -----

        private static long At(long field, int index) => field + (long)index * GTA.Offsets.Editor.Weapon.NEXT;

        private static int Get(long field, int index) => field == 0 ? 0 : new Global(At(field, index)).Get<int>();

        private static void Set(long field, int index, int value)
        {
            if (field != 0 && MainWindow.m.IsProcOpen)
                new Global(At(field, index)).SetInt(value);
        }

        public static int Model(int index) => Get(GTA.Offsets.Editor.Weapon.model, index);

        public static Kind KindOf(int index)
        {
            int model = Model(index);
            if (model == CustomScript)
                return Kind.Script;
            if (model == VehicleScript || model == VehicleScriptLowGlow)
                return Kind.Vehicle;
            return Kind.Weapon;
        }

        /// <summary>Turns the weapon into a script pickup, a vehicle pickup or back into a pistol,
        /// with the first type of its kind when it had none.</summary>
        public static void SetKind(int index, Kind kind)
        {
            if (KindOf(index) == kind)
                return;
            var w = GTA.Offsets.Editor.Weapon.model;
            switch (kind)
            {
                case Kind.Script:
                    Set(w, index, CustomScript);
                    Set(GTA.Offsets.Editor.Weapon.vput, index, -1);
                    if (ScriptType(index) < 0)
                        Set(GTA.Offsets.Editor.Weapon.rput, index, ScriptTypes[0].Value);
                    break;
                case Kind.Vehicle:
                    Set(w, index, VehicleScript);
                    Set(GTA.Offsets.Editor.Weapon.rput, index, -1);
                    if (VehicleType(index) < 0)
                        Set(GTA.Offsets.Editor.Weapon.vput, index, VehicleTypes[0].Value);
                    break;
                default:
                    Set(w, index, Pistol);
                    Set(GTA.Offsets.Editor.Weapon.rput, index, -1);
                    Set(GTA.Offsets.Editor.Weapon.vput, index, -1);
                    break;
            }
        }

        public static int ScriptType(int index) => Get(GTA.Offsets.Editor.Weapon.rput, index);
        public static int VehicleType(int index) => Get(GTA.Offsets.Editor.Weapon.vput, index);
        public static int Variant(int index) => Get(GTA.Offsets.Editor.Weapon.wcpm, index);
        public static int Clip(int index) => Get(GTA.Offsets.Editor.Weapon.clip, index);

        public static void SetScriptType(int index, int value)
        {
            Set(GTA.Offsets.Editor.Weapon.rput, index, value);
            var type = ScriptTypes.FirstOrDefault(t => t.Value == value);
            // A variant the new type does not have would give no model.
            if (type != null && type.Variants.Length > 0 && !type.Variants.Any(v => v.Value == Variant(index)))
                SetVariant(index, type.Variants[0].Value);
        }

        public static void SetVehicleType(int index, int value) => Set(GTA.Offsets.Editor.Weapon.vput, index, value);
        public static void SetVariant(int index, int value) => Set(GTA.Offsets.Editor.Weapon.wcpm, index, value);
        public static void SetClip(int index, int value) => Set(GTA.Offsets.Editor.Weapon.clip, index, Math.Max(0, value));

        public static bool LowGlow(int index) => Model(index) == VehicleScriptLowGlow;

        public static void SetLowGlow(int index, bool on)
        {
            if (KindOf(index) == Kind.Vehicle)
                Set(GTA.Offsets.Editor.Weapon.model, index, on ? VehicleScriptLowGlow : VehicleScript);
        }

        /// <summary>vput 17 and 18 raise or lower (blast size, bomb count): vbmbl / vbmbm below 0 lowers.</summary>
        public static bool Lowers(int index, int vehicleType)
        {
            long field = vehicleType == 17 ? GTA.Offsets.Editor.Weapon.vbmbl : GTA.Offsets.Editor.Weapon.vbmbm;
            return Get(field, index) < 0;
        }

        public static void SetLowers(int index, int vehicleType, bool lower)
        {
            long field = vehicleType == 17 ? GTA.Offsets.Editor.Weapon.vbmbl : GTA.Offsets.Editor.Weapon.vbmbm;
            int now = Get(field, index);
            int size = Math.Max(1, Math.Abs(now));
            Set(field, index, lower ? -size : size);
        }

        public static bool AmmoForAll(int index) => (Get(GTA.Offsets.Editor.Weapon.bits, index) & (1 << AllWeaponsBit)) != 0;

        public static void SetAmmoForAll(int index, bool on)
        {
            int bits = Get(GTA.Offsets.Editor.Weapon.bits, index);
            Set(GTA.Offsets.Editor.Weapon.bits, index, on ? bits | (1 << AllWeaponsBit) : bits & ~(1 << AllWeaponsBit));
        }

        // brest1..4: bit 0 hides the weapon for that team.
        public static bool HiddenFor(int index, int team) => (Get(GTA.Offsets.Editor.Weapon.brest + team, index) & 1) != 0;

        public static void SetHiddenFor(int index, int team, bool hidden)
        {
            int v = Get(GTA.Offsets.Editor.Weapon.brest + team, index);
            Set(GTA.Offsets.Editor.Weapon.brest + team, index, hidden ? v | 1 : v & ~1);
        }

        public static int Component(int index, int slot) => Get(GTA.Offsets.Editor.Weapon.comps + slot, index);

        public static int[] Components(int index) =>
            Enumerable.Range(0, ComponentSlots).Select(s => Component(index, s)).ToArray();

        /// <summary>Puts the component into the first free comps slot, or takes it out.</summary>
        public static bool SetComponent(int index, int component, bool on)
        {
            var now = Components(index);
            int at = Array.IndexOf(now, component);
            if (on == (at >= 0))
                return true;
            if (on)
            {
                int free = Array.FindIndex(now, c => c == 0 || c == -1);
                if (free < 0)
                    return false;
                Set(GTA.Offsets.Editor.Weapon.comps + free, index, component);
            }
            else
            {
                Set(GTA.Offsets.Editor.Weapon.comps + at, index, 0);
            }
            return true;
        }

        // ----- weapon data (weapon_components.json) -----

        public sealed class ComponentInfo
        {
            public string Name;
            public int Hash;
            public int ClipSize;
            public Dictionary<string, string> Names = new Dictionary<string, string>();

            public string Label(string language) =>
                Names.TryGetValue(language, out var n) ? n : Names.TryGetValue("en", out n) ? n : Name;
        }

        public sealed class Slot
        {
            public string Bone;
            public string Default;
            public string[] Items;
        }

        public sealed class WeaponInfo
        {
            public string Name;
            public int ClipSize;
            public Slot[] Slots;
        }

        private static readonly Lazy<(Dictionary<string, WeaponInfo> Weapons, Dictionary<string, ComponentInfo> Components)> Data =
            new Lazy<(Dictionary<string, WeaponInfo>, Dictionary<string, ComponentInfo>)>(Load);

        private static (Dictionary<string, WeaponInfo>, Dictionary<string, ComponentInfo>) Load()
        {
            var root = JObject.Parse(OfflineData.WeaponComponents);
            var weapons = new Dictionary<string, WeaponInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in ((JObject)root["weapons"]).Properties())
                weapons[p.Name] = new WeaponInfo
                {
                    Name = p.Name,
                    ClipSize = (int?)p.Value["clip"] ?? 0,
                    Slots = p.Value["slots"].Select(s => new Slot
                    {
                        Bone = (string)s["bone"],
                        Default = (string)s["default"],
                        Items = s["items"].Select(i => (string)i).ToArray(),
                    }).ToArray(),
                };
            var comps = new Dictionary<string, ComponentInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in ((JObject)root["components"]).Properties())
                comps[p.Name] = new ComponentInfo
                {
                    Name = p.Name,
                    Hash = (int)p.Value["hash"],
                    ClipSize = (int?)p.Value["clip"] ?? 0,
                    Names = ((JObject)p.Value["name"]).Properties().ToDictionary(n => n.Name, n => (string)n.Value),
                };
            return (weapons, comps);
        }

        public static ComponentInfo ComponentByHash(int hash) => Data.Value.Components.Values.FirstOrDefault(c => c.Hash == hash);

        public static ComponentInfo ComponentByName(string name) =>
            name != null && Data.Value.Components.TryGetValue(name, out var c) ? c : null;

        /// <summary>The weapon behind a pickup type (PICKUP_WEAPON_X -> WEAPON_X), or null.</summary>
        public static WeaponInfo WeaponOf(int pickup)
        {
            var entry = GTA.Editor.WeaponList.FirstOrDefault(w => w.Int32 == pickup);
            if (entry == null || string.IsNullOrEmpty(entry.Native))
                return null;
            string name = entry.Native.ToUpperInvariant()
                .Replace("PICKUP_VEHICLE_WEAPON_", "WEAPON_")
                .Replace("PICKUP_WEAPON_", "WEAPON_");
            return Data.Value.Weapons.TryGetValue(name, out var w) ? w : null;
        }

        /// <summary>Rounds per magazine of the weapon (the default clip component's, else the weapon's).</summary>
        public static int ClipSize(WeaponInfo weapon)
        {
            if (weapon == null)
                return 0;
            foreach (var slot in weapon.Slots)
            {
                var clip = ComponentByName(slot.Default);
                if (clip != null && clip.ClipSize > 0)
                    return clip.ClipSize;
            }
            return weapon.ClipSize;
        }

        // ----- what the creators change when they load the job -----

        /// <summary>
        /// Why the creator will change this pickup type when it loads the job, or null. The
        /// creators keep only the types in weapon_pickup_models (prop_lists.json); others become a
        /// pistol, a marksman rifle a sniper rifle. A stun gun becomes a pistol in a player's own
        /// job (content category 0) as well.
        /// </summary>
        public static string LoadWarning(int pickup, Func<string, string, string> t)
        {
            if (pickup == 0 || pickup == CustomScript || pickup == VehicleScript || pickup == VehicleScriptLowGlow)
                return null;
            if (pickup == Stungun)
                return t("wp_warn_stungun", "In your own jobs the creator turns the stun gun into a pistol when it loads the job.");
            var allowed = GTA.Editor.weapon_pickup_models;
            if (allowed.Count == 0 || allowed.Contains(pickup.ToString(System.Globalization.CultureInfo.InvariantCulture)))
                return null;
            return pickup == MarksmanRifle
                ? t("wp_warn_marksman", "The creator turns this into a sniper rifle when it loads the job.")
                : t("wp_warn_pistol", "This pickup is not on the creators' list: when the job loads it becomes a pistol.");
        }
    }
}
