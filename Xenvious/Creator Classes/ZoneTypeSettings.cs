using System.Collections.Generic;

namespace Xenvious
{
    /// <summary>
    /// The settings the Mission Creator offers for single zone types (public_mission_creator
    /// "Type Specific Options", PMCT_ZNE_TSO, 1.73), with the fields they write. Labels are the
    /// creator's own (PMCT_* / PMCH_*); the zones page builds its "Type settings" card from this.
    /// </summary>
    public static class ZoneTypeSettings
    {
        /// <summary>Zone fields the settings use: znbstwo (f_23), znbs3 (f_24), znwd (f_28, float), znVl3 (f_30).</summary>
        public enum Field { Znbs2, Znbs3, Value1, Value3 }

        public enum Kind { Bit, Choice, Slider, Number }

        public sealed class Setting
        {
            public Kind Kind;
            public Field Field;
            public int Bit;
            public string Key, Fallback, TipKey, TipFallback;
            /// <summary>Choice: (value, label key, English label).</summary>
            public (int Value, string Key, string Fallback)[] Options;
            /// <summary>Slider and number: shown value = raw * Scale (or 100 - raw with Invert), between Min and Max.</summary>
            public double Min, Max, Step = 1, Scale = 1;
            public bool Invert;
            public string Unit = "";
            /// <summary>Shown only while this bit of znbs3 is set (true) or clear (false); null = always.</summary>
            public (int Bit, bool Set)? When;
        }

        private static Setting Bit(Field field, int bit, string key, string fallback, string tip = null, string tipFallback = null)
            => new Setting { Kind = Kind.Bit, Field = field, Bit = bit, Key = key, Fallback = fallback, TipKey = tip, TipFallback = tipFallback };

        private static Setting Choice(Field field, string key, string fallback, params (int, string, string)[] options)
            => new Setting { Kind = Kind.Choice, Field = field, Key = key, Fallback = fallback, Options = options };

        private static Setting Slider(string key, string fallback, double min, double max, double step, double scale = 1, string unit = "%")
            => new Setting { Kind = Kind.Slider, Field = Field.Value1, Key = key, Fallback = fallback, Min = min, Max = max, Step = step, Scale = scale, Unit = unit };

        private static readonly (int, string, string)[] Stars =
        {
            (0, "zs_wanted_0", "No change"), (1, "zs_wanted_1", "1 star"), (2, "zs_wanted_2", "2 stars"),
            (3, "zs_wanted_3", "3 stars"), (4, "zs_wanted_4", "4 stars"), (5, "zs_wanted_5", "5 stars"),
        };

        // Bit 31 of znbs3: an existing higher wanted level stays (fm_mission_controller type 103, FMMC menu "TYGDR").
        private static Setting KeepHigher => Bit(Field.Znbs3, 31, "zs_keephigher", "Keep a higher wanted level", "zs_keephigher_tip", "Players who already have more stars keep them.");

        public static readonly IReadOnlyDictionary<int, Setting[]> ByType = new Dictionary<int, Setting[]>
        {
            [17] = new[] { Slider("zs_17_speed", "Force movement speed", 50, 200, 10, 100) },
            [18] = new[]
            {
                Bit(Field.Znbs3, 16, "zs_18_melee", "Disable melee"),
                Bit(Field.Value3, 0, "zs_18_weapons", "Disable weapons", "zs_18_weapons_tip", "Players cannot use or pull out their main weapons."),
            },
            [30] = new[] { Bit(Field.Znbs3, 14, "zs_30_para", "Unequip parachute", "zs_30_para_tip", "An equipped parachute is stowed away until the player leaves the zone.") },
            [52] = new[] { Bit(Field.Znbs2, 7, "zs_52_oob", "No out of bounds blip", "zs_52_oob_tip", "No GPS blip is added to this zone when a player goes out of bounds.") },
            [61] = new[] { Bit(Field.Znbs3, 12, "zs_ladders", "Block ladders"), Bit(Field.Znbs3, 13, "zs_jumping", "Block jumping") },
            [63] = new[]
            {
                Bit(Field.Znbs3, 11, "zs_63_air", "Air drop-off", "zs_63_air_tip", "Collect & deliver passes without landing when the players are in a helicopter or plane."),
                new Setting
                {
                    Kind = Kind.Choice, Field = Field.Value3, Key = "zs_63_marker", Fallback = "Air marker", When = (11, true),
                    Options = new (int, string, string)[] { (0, "zs_63_marker_0", "Hide marker"), (1, "zs_63_marker_1", "Show marker") },
                },
                new Setting
                {
                    Kind = Kind.Choice, Field = Field.Value3, Key = "zs_63_corona", Fallback = "Corona", When = (11, false),
                    Options = new (int, string, string)[]
                    {
                        (0, "zs_63_corona_0", "Hide corona"), (1, "zs_63_corona_1", "Actual size"), (2, "zs_63_corona_2", "On foot"),
                        (3, "zs_63_corona_3", "In vehicle"), (4, "zs_63_corona_4", "On foot / in vehicle"),
                    },
                },
            },
            [68] = new[]
            {
                new Setting { Kind = Kind.Number, Field = Field.Value3, Key = "zs_68_flag", Fallback = "Mission flag", Min = 0, Max = 31 },
                Bit(Field.Znbs2, 11, "zs_68_clear", "Clear the flag on exit", "zs_68_clear_tip", "The mission flag of this zone is cleared when players leave it."),
            },
            [70] = new[]
            {
                Choice(Field.Value3, "zs_70_style", "Walk style", (0, "zs_70_0", "Default"), (1, "zs_70_1", "Femme"), (2, "zs_70_2", "Gangster"),
                    (3, "zs_70_3", "Posh"), (4, "zs_70_4", "Tough guy"), (5, "zs_70_5", "Grooving")),
            },
            [75] = new[] { Choice(Field.Value1, "zs_wanted", "Minimum wanted level", Stars), KeepHigher },
            [81] = new[]
            {
                new Setting { Kind = Kind.Slider, Field = Field.Value1, Key = "zs_81_speed", Fallback = "Transition speed", Min = 50, Max = 100, Step = 5, Invert = true, Unit = "%" },
                Choice(Field.Value3, "zs_81_filter", "Filter",
                    (0, "zs_81_0", "No filter"), (1, "zs_81_1", "Blue tint"), (2, "zs_81_2", "Old TV"), (3, "zs_81_3", "Sepia"),
                    (4, "zs_81_4", "Black & white"), (5, "zs_81_5", "Trippy"), (6, "zs_81_6", "Low saturation"),
                    (7, "zs_81_7", "Blue-purple tint"), (8, "zs_81_8", "Noir"), (9, "zs_81_9", "High saturation"),
                    (10, "zs_81_10", "Night vision (green)")),
            },
            [91] = new[] { Slider("zs_91_zoom", "Zoom amount", 10, 100, 2.5) },
            [96] = new[]
            {
                Bit(Field.Znbs3, 5, "zs_96_peds", "Block pedestrians"),
                Bit(Field.Znbs3, 6, "zs_96_traffic", "Block traffic"),
                Bit(Field.Znbs3, 7, "zs_96_animals", "Block animals"),
            },
            [97] = new[] { Choice(Field.Value1, "zs_wanted", "Wanted level", Stars) },
            [98] = new[] { Choice(Field.Value1, "zs_wanted", "Wanted level", Stars) },
            [103] = new[] { Choice(Field.Value1, "zs_wanted", "Wanted level", Stars), KeepHigher },
            [106] = new[]
            {
                Bit(Field.Value3, 0, "zs_106_sprint", "Disable sprinting"),
                Bit(Field.Value3, 1, "zs_106_roll", "Disable combat roll"),
                Bit(Field.Value3, 2, "zs_106_enter", "Disable entering vehicles"),
                Bit(Field.Value3, 3, "zs_106_exit", "Disable exiting vehicles"),
                Bit(Field.Value3, 4, "zs_106_climb", "Disable climbing"),
                Bit(Field.Znbs3, 12, "zs_106_ladders", "Disable ladders"),
                Bit(Field.Znbs3, 13, "zs_106_jump", "Disable jumping"),
                Slider("zs_106_speed", "Movement speed", 50, 150, 10, 100),
            },
        };

        public static Setting[] For(int type) => ByType.TryGetValue(type, out var settings) ? settings : null;

        /// <summary>The global of a field for zone <paramref name="zone"/>; 0 when the edition lacks it.</summary>
        public static long Address(Field field, int zone)
        {
            long at = field switch
            {
                Field.Znbs2 => GTA.Offsets.Editor.Zones.znbs2,
                Field.Znbs3 => GTA.Offsets.Editor.Zones.znbs3,
                Field.Value1 => GTA.Offsets.Editor.Zones.znwd,
                _ => GTA.Offsets.Editor.Zones.znVl3,
            };
            return at == 0 || GTA.Offsets.Editor.Zones.NEXT == 0 || zone < 0 ? 0 : at + zone * GTA.Offsets.Editor.Zones.NEXT;
        }
    }
}
