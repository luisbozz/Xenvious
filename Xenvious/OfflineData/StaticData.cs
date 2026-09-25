namespace Xenvious
{
    /// <summary>
    /// Values that do not change with a game update. They used to sit in offsets.ini and
    /// were read with one GetPrivateProfileString call each on every start, although
    /// nothing about them is build-specific. Build-specific data stays in the ini.
    /// </summary>
    public static class StaticData
    {
        public static class Links
        {
            public const string SocialClub = "https://de.socialclub.rockstargames.com/member/luisbozz_rsg/jobs?dateRange=any&platform=pc&sort=likes&title=gtav";
            public const string Discord = "https://discordapp.com/invite/f2Uwzwr";
            public const string YouTube = "https://www.youtube.com/channel/UCGsCGsOpaw8D4NU25CuOzLQ";
            public const string Twitter = "https://twitter.com/luis___g";
            public const string Props = "https://www.se7ensins.com/forums/threads/1-44-gta-v-prop-list.1728127/";
            public const string PropsWithPictures = "https://cdn.rage.mp/public/odb/index.html#0";
            public const string Vehicles = "https://www.se7ensins.com/forums/threads/ref-gta-v-vehicle-hashes-list-2.1615439/";
            public const string Weapons = "https://www.se7ensins.com/forums/threads/weapon-and-explosion-hashes-list.1045035/";
            public const string Peds = "https://wiki.gtanet.work/index.php?title=Peds";
            public const string ColorCodes = "https://docs.google.com/spreadsheets/d/1Uo4pRpXZsCaV1NFRP3H0AK1XTV3U7WDx8GAaWqOrVj0/edit#gid=0";
        }

        public static readonly string[] CountryCodes = { "en", "de", "es", "fr", "it", "ja", "ko", "pl", "ru", "zh", "zh-cn", "pt", "pt-pt", "en-gb", "es-mx" };

        public static readonly string[] StartWeapons =
        {
            "weapon_pistol", "weapon_combatpistol", "weapon_appistol", "weapon_pistol50", "weapon_heavypistol", "weapon_snspistol",
            "weapon_vintagepistol", "weapon_flaregun", "weapon_marksmanpistol", "weapon_pistol_mk2", "weapon_revolver_mk2", "weapon_snspistol_mk2",
            "weapon_revolver", "weapon_raypistol", "weapon_gadgetpistol", "weapon_microsmg", "weapon_smg", "weapon_assaultsmg",
            "weapon_combatpdw", "weapon_minismg", "weapon_machinepistol", "weapon_smg_mk2", "weapon_assaultrifle", "weapon_carbinerifle",
            "weapon_advancedrifle", "weapon_bullpuprifle", "weapon_marksmanrifle", "weapon_compactrifle", "weapon_gusenberg", "weapon_musket",
            "weapon_raycarbine", "weapon_specialcarbine", "weapon_assaultrifle_mk2", "weapon_carbinerifle_mk2", "weapon_combatmg_mk2", "weapon_bullpuprifle_mk2",
            "weapon_marksmanrifle_mk2", "weapon_specialcarbine_mk2", "weapon_militaryrifle", "weapon_mg", "weapon_combatmg", "weapon_pumpshotgun",
            "weapon_sawnoffshotgun", "weapon_assaultshotgun", "weapon_bullpupshotgun", "weapon_heavyshotgun", "weapon_dbshotgun", "weapon_autoshotgun",
            "weapon_doubleaction", "weapon_pumpshotgun_mk2", "weapon_combatshotgun", "weapon_sniperrifle", "weapon_heavysniper", "weapon_heavysniper_mk2",
            "weapon_grenadelauncher", "weapon_rpg", "weapon_minigun", "weapon_rayminigun", "weapon_firework", "weapon_hominglauncher",
            "weapon_railgun", "weapon_compactlauncher", "weapon_grenade", "weapon_smokegrenade", "weapon_stickybomb", "weapon_molotov",
            "weapon_proxmine", "weapon_pipebomb", "weapon_stungun_mp", "weapon_petrolcan", "weapon_knife", "weapon_nightstick",
            "weapon_hammer", "weapon_bat", "weapon_crowbar", "weapon_golfclub", "weapon_bottle", "weapon_dagger",
            "weapon_knuckle", "weapon_hatchet", "weapon_machete", "weapon_flashlight", "weapon_switchblade", "weapon_battleaxe",
            "weapon_poolcue", "weapon_wrench", "weapon_stone_hatchet", "weapon_unarmed", "weapon_fertilizercan", "weapon_emplaunchercase",
            "weapon_tacticalrifle", "weapon_precisionrifle", "weapon_heavyrifle",
        };

        /// <summary>Hand-picked props shown in the Special category.</summary>
        public static readonly GTA.Prop[] SpecialProps =
        {
            new GTA.Prop("p_spinning_anus_s", "UFO (Big Hitbox)", "Special"),
            new GTA.Prop("imp_prop_ship_01a", "UFO (Normal Hitbox)", "Special"),
            new GTA.Prop("gr_prop_damship_01a", "UFO Destroyed", "Special"),
            new GTA.Prop("prop_jet_bloodsplat_01", "Bloodsplat from Jet", "Special"),
            new GTA.Prop("prop_chall_lamp_01n", "Blue Light (Only Nighttime)", "Special"),
            new GTA.Prop("prop_chall_lamp_02", "White Light", "Special"),
            new GTA.Prop("prop_traffic_02a", "Wierd Traffic Light", "Special"),
            new GTA.Prop("p_tram_crash_s", "Train", "Special"),
            new GTA.Prop("p_hw1_22_table_s", "Bright Green Animated Table", "Special"),
            new GTA.Prop("ch_prop_casino_diamonds_02a", "Diamonds", "Special"),
            new GTA.Prop("prop_skid_pillar_01", "Pillar", "Special"),
            new GTA.Prop("prop_temp_block_blocker", "Wooden Block", "Special"),
        };
    }
}
