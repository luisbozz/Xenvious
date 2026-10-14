using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class Weapons
    {

        public static List<WeaponInventory> Pistols = new List<WeaponInventory>()
        {
            new WeaponInventory("Pistol"                        , 1 , InventoryOffset._1, WeaponCategory.Pistol, 1),
            new WeaponInventory("Pistol + Silencer"             , 25, InventoryOffset._1, WeaponCategory.Pistol, 25),
            new WeaponInventory("Pistol + Silencer + Flash"     , 11, InventoryOffset._2, WeaponCategory.Pistol, 43),
            new WeaponInventory("Combat Pistol"                 , 2 , InventoryOffset._1, WeaponCategory.Pistol, 2),
            new WeaponInventory("Combat Pistol + Silencer"      , 22, InventoryOffset._3, WeaponCategory.Pistol, 86),
            new WeaponInventory("AP-Pistol"                     , 3 , InventoryOffset._1, WeaponCategory.Pistol, 3),
            new WeaponInventory("AP-Pistol + Extended Clip"     , 12, InventoryOffset._2, WeaponCategory.Pistol, 44),
            new WeaponInventory("Pistol-50"                     , 9, InventoryOffset._2, WeaponCategory.Pistol, 41),
            new WeaponInventory("Pistol-50 + Flash"             , 4 , InventoryOffset._2, WeaponCategory.Pistol, 36),
            new WeaponInventory("Pistol-50 + Silencer"          , 2 , InventoryOffset._4, WeaponCategory.Pistol, 98),
            new WeaponInventory("Pistol-50 + Silencer + Flash"  , 16, InventoryOffset._4, WeaponCategory.Pistol, 112),
            new WeaponInventory("Flare Gun"                     , 1 , InventoryOffset._2, WeaponCategory.Pistol, 33),
            new WeaponInventory("Marksman Pistol"               , 17, InventoryOffset._2, WeaponCategory.Pistol, 49),
            new WeaponInventory("Heavy Revolver"                , 22, InventoryOffset._2, WeaponCategory.Pistol, 54),
            new WeaponInventory("Machine Pistol"                , 16, InventoryOffset._2, WeaponCategory.Pistol, 48),
            new WeaponInventory("Machine Pistol + Extended Mag" , 0 , InventoryOffset._4, WeaponCategory.Pistol, 96),
            new WeaponInventory("Machine Pistol + Silencer"     , 15, InventoryOffset._4, WeaponCategory.Pistol, 111),
            new WeaponInventory("Pistol MK II"                  , 6 , InventoryOffset._3, WeaponCategory.Pistol, 70),
            new WeaponInventory("Heavy Revolver MK II"          , 15, InventoryOffset._3, WeaponCategory.Pistol, 79),
            new WeaponInventory("SNS Pistol MK II"              , 16, InventoryOffset._3, WeaponCategory.Pistol, 80),
            new WeaponInventory("Heavy Pistol"                  , 24, InventoryOffset._3, WeaponCategory.Pistol, 88),
            new WeaponInventory("Heavy Pistol + Silencer"       , 25, InventoryOffset._3, WeaponCategory.Pistol, 89),
            new WeaponInventory("Heavy Pistol + Silencer + Flash" , 17, InventoryOffset._4, WeaponCategory.Pistol, 113),
            new WeaponInventory("Vintage Pistol + Silencer"     , 14, InventoryOffset._4, WeaponCategory.Pistol, 110),
            new WeaponInventory("Up-n-Atomizer"                 , 18, InventoryOffset._3, WeaponCategory.Pistol, 82),
            new WeaponInventory("Stungun"                       , 21, InventoryOffset._3, WeaponCategory.Pistol, 85),
            new WeaponInventory("Keramikpistol"                 , 7 , InventoryOffset._4, WeaponCategory.Pistol, 103)
        };

        public static List<WeaponInventory> Meele = new List<WeaponInventory>()
        {
            new WeaponInventory("Knife"                  , 22, InventoryOffset._1, WeaponCategory.Meele, 22),
            new WeaponInventory("Baseball Bat"           , 23, InventoryOffset._1, WeaponCategory.Meele, 23),
            new WeaponInventory("Parachute"              , 30, InventoryOffset._1, WeaponCategory.Meele, 29),
            new WeaponInventory("Crowbar"                , 3 , InventoryOffset._2, WeaponCategory.Meele, 35),
            new WeaponInventory("Nightstick"             , 7 , InventoryOffset._2, WeaponCategory.Meele, 39),
            new WeaponInventory("Pool Cue"               , 30, InventoryOffset._2, WeaponCategory.Meele, 62),
            new WeaponInventory("Hatchet"                , 18, InventoryOffset._2, WeaponCategory.Meele, 50),
            new WeaponInventory("Battle Axe"             , 28, InventoryOffset._2, WeaponCategory.Meele, 60),
            new WeaponInventory("Pipe Wrench"            , 31, InventoryOffset._2, WeaponCategory.Meele, 63),
            new WeaponInventory("Flashlight"             , 14, InventoryOffset._2, WeaponCategory.Meele, 46),
            new WeaponInventory("Machete"                , 15, InventoryOffset._2, WeaponCategory.Meele, 47),
            new WeaponInventory("Hammer"                 , 0 , InventoryOffset._3, WeaponCategory.Meele, 64),
            new WeaponInventory("Golf Club"              , 1 , InventoryOffset._3, WeaponCategory.Meele, 65),
            new WeaponInventory("Bottle"                 , 2 , InventoryOffset._3, WeaponCategory.Meele, 66),
            new WeaponInventory("Antique Cavalry Dagger" , 3 , InventoryOffset._3, WeaponCategory.Meele, 67),
            new WeaponInventory("Knuckle Duster"         , 4 , InventoryOffset._3, WeaponCategory.Meele, 68),
            new WeaponInventory("Switchblade"            , 5 , InventoryOffset._3, WeaponCategory.Meele, 69),
            new WeaponInventory("Molotov"                , 25, InventoryOffset._4, WeaponCategory.Meele, 121)
        };

        public static List<WeaponInventory> MG = new List<WeaponInventory>()
        {
            new WeaponInventory("Gusenberg Sweeper"        , 21, InventoryOffset._2, WeaponCategory.MG, 53),
            new WeaponInventory("Mini SMG"                 , 26, InventoryOffset._2, WeaponCategory.MG, 58),
            new WeaponInventory("Micro SMG"                , 4 , InventoryOffset._1, WeaponCategory.MG, 4),
            new WeaponInventory("Micro SMG + Extended Mag" , 4 , InventoryOffset._4, WeaponCategory.MG, 100),
            new WeaponInventory("Micro SMG + Silencer"     , 13, InventoryOffset._4, WeaponCategory.MG, 109),
            new WeaponInventory("SMG"                      , 5 , InventoryOffset._1, WeaponCategory.MG, 5),
            new WeaponInventory("SMG + Clip 1"             , 11, InventoryOffset._4, WeaponCategory.MG, 107),
            new WeaponInventory("SMG + Clip 2"             , 12, InventoryOffset._4, WeaponCategory.MG, 108),
            new WeaponInventory("Assault SMG"              , 19, InventoryOffset._2, WeaponCategory.MG, 51),
            new WeaponInventory("LMG"                      , 6 , InventoryOffset._1, WeaponCategory.MG, 6),
            new WeaponInventory("Combat LMG"               , 7 , InventoryOffset._1, WeaponCategory.MG, 7),
            new WeaponInventory("Combat LMG + Extended Mag", 28, InventoryOffset._1, WeaponCategory.MG, 28),
            new WeaponInventory("SMG MK II"                , 7 , InventoryOffset._3, WeaponCategory.MG, 71),
            new WeaponInventory("SMG MK II + Hollowpoint"  , 9, InventoryOffset._4, WeaponCategory.MG, 105),
            new WeaponInventory("Combat MG MK II"          , 8 , InventoryOffset._3, WeaponCategory.MG, 72),
            new WeaponInventory("Unholy Hellbringer"       , 19, InventoryOffset._3, WeaponCategory.MG, 83),
            new WeaponInventory("SMG + Silencer"           , 23, InventoryOffset._3, WeaponCategory.MG, 87),
            new WeaponInventory("Assault SMG + Silencer"   , 26, InventoryOffset._3, WeaponCategory.MG, 90),
            new WeaponInventory("Micro SMG + Silencer"     , 27, InventoryOffset._3, WeaponCategory.MG, 91)
        };

        public static List<WeaponInventory> SG = new List<WeaponInventory>()
        {
            new WeaponInventory("Sawed-Off Shotgun"             , 14, InventoryOffset._1, WeaponCategory.SG, 14),
            new WeaponInventory("Assault Shotgun"               , 15, InventoryOffset._1, WeaponCategory.SG, 15),
            new WeaponInventory("Pump Shotgun"                  , 13, InventoryOffset._1, WeaponCategory.SG, 13),
            new WeaponInventory("Pump Shotgun + Tactic Light"   , 2,  InventoryOffset._2, WeaponCategory.SG, 34),
            new WeaponInventory("Pump Shotgun + Silencer"       , 26, InventoryOffset._1, WeaponCategory.SG, 26),
            new WeaponInventory("Assault Shotgun + Silencer"    , 6,  InventoryOffset._2, WeaponCategory.SG, 38),
            new WeaponInventory("Bullpup Shotgun"               , 8,  InventoryOffset._2, WeaponCategory.SG, 40),
            new WeaponInventory("Double Barrel Shotgun"         , 23, InventoryOffset._2, WeaponCategory.SG, 55),
            new WeaponInventory("Automatic Shotgun"             , 25, InventoryOffset._2, WeaponCategory.SG, 57),
            new WeaponInventory("Pump Shotgun MK II"            , 14, InventoryOffset._3, WeaponCategory.SG, 78),
            new WeaponInventory("Assault Shotgun + Silencer"    , 30, InventoryOffset._3, WeaponCategory.SG, 94),
            new WeaponInventory("Sweeper Shotgun"               , 1,  InventoryOffset._4, WeaponCategory.SG, 97),
            new WeaponInventory("Pump Shotgun MK II + Silencer" , 6,  InventoryOffset._4, WeaponCategory.SG, 102),
            new WeaponInventory("Pump Shotgun MK II + AMMOPIERCING" , 21,  InventoryOffset._4, WeaponCategory.SG, 117),
            new WeaponInventory("Pump Shotgun MK II + Silencer + AMMOPIERCING" , 22,  InventoryOffset._4, WeaponCategory.SG, 118),
        };

        public static List<WeaponInventory> Special = new List<WeaponInventory>()
        {
            new WeaponInventory("Grenade Launcher"         , 16, InventoryOffset._1, WeaponCategory.Special, 16),
            new WeaponInventory("RPG"                      , 17, InventoryOffset._1, WeaponCategory.Special, 17),
            new WeaponInventory("Minigun"                  , 18, InventoryOffset._1, WeaponCategory.Special, 18),
            new WeaponInventory("Homing Launcher"          , 13, InventoryOffset._2, WeaponCategory.Special, 45),
            new WeaponInventory("Compact Grenade Launcher" , 27, InventoryOffset._2, WeaponCategory.Special, 59),
            new WeaponInventory("Railgun"                  , 20, InventoryOffset._2, WeaponCategory.Special, 52),
            new WeaponInventory("Widowmaker"               , 20, InventoryOffset._3, WeaponCategory.Special, 84)
        };

        public static List<WeaponInventory> Rifle = new List<WeaponInventory>()
        {
            new WeaponInventory("Assault Rifle"                   , 8 , InventoryOffset._1, WeaponCategory.Rifle, 8),
            new WeaponInventory("Carbine Rifle"                   , 9, InventoryOffset._1, WeaponCategory.Rifle, 9),
            new WeaponInventory("Carbine Rifle + Silencer"        , 20, InventoryOffset._4, WeaponCategory.Rifle, 116),
            new WeaponInventory("Advanced Rifle"                  , 10, InventoryOffset._1, WeaponCategory.Rifle, 10),
            new WeaponInventory("Compact Rifle"                   , 24, InventoryOffset._2, WeaponCategory.Rifle, 56),
            new WeaponInventory("Combat PDW"                      , 18, InventoryOffset._4, WeaponCategory.Rifle, 114),
            new WeaponInventory("Special Carbine"                 , 24, InventoryOffset._1, WeaponCategory.Rifle, 24),
            new WeaponInventory("Special Carbine + Silencer"      , 27, InventoryOffset._1, WeaponCategory.Rifle, 27),
            new WeaponInventory("Assault Rifle MK II"             , 9, InventoryOffset._3, WeaponCategory.Rifle, 73),
            new WeaponInventory("Carbine Rifle MK II"             , 10, InventoryOffset._3, WeaponCategory.Rifle, 74),
            new WeaponInventory("Carbine Rifle MK II + Silencer"  , 28, InventoryOffset._3, WeaponCategory.Rifle, 92),
            new WeaponInventory("Carbine Rifle MK II + ARMORPIERCING" , 10, InventoryOffset._4, WeaponCategory.Rifle, 106),
            new WeaponInventory("Carbine Rifle MK II + Silencer + ARMORPIERCING" , 24, InventoryOffset._4, WeaponCategory.Rifle, 120),
            new WeaponInventory("Bullpop Rifle MK II"             , 12, InventoryOffset._3, WeaponCategory.Rifle, 76),
            new WeaponInventory("Special Carbine MK II"           , 17, InventoryOffset._3, WeaponCategory.Rifle, 81),
            new WeaponInventory("Assault Rifle + Silencer"        , 31, InventoryOffset._3, WeaponCategory.Rifle, 95),
            new WeaponInventory("Compact Rifle + Extended Mag"    , 3 , InventoryOffset._4, WeaponCategory.Rifle, 99)
        };

        public static List<WeaponInventory> Sniper = new List<WeaponInventory>()
        {
            new WeaponInventory("Sniper Rifle"         , 11, InventoryOffset._1, WeaponCategory.Sniper, 11),
            new WeaponInventory("Heavy Sniper"         , 12, InventoryOffset._1, WeaponCategory.Sniper, 12),
            new WeaponInventory("Heavy Sniper MK II"   , 11, InventoryOffset._3, WeaponCategory.Sniper, 75),
            new WeaponInventory("Marksman Rifle MK II" , 13, InventoryOffset._3, WeaponCategory.Sniper, 77)
        };

        public static List<WeaponInventory> Explosive = new List<WeaponInventory>()
        {
            new WeaponInventory("Grande"                  , 19, InventoryOffset._1, WeaponCategory.Explosive, 19),
            new WeaponInventory("Sticky Bomb"             , 20, InventoryOffset._1, WeaponCategory.Explosive, 20),
            new WeaponInventory("Jerry Can"               , 21, InventoryOffset._1, WeaponCategory.Explosive, 21),
            new WeaponInventory("Proximity Mine"          , 31, InventoryOffset._1, WeaponCategory.Explosive, 31),
            new WeaponInventory("Pipe Bomb"               , 29, InventoryOffset._2, WeaponCategory.Explosive, 61),
            new WeaponInventory("Smoke Grenade"           , 8 , InventoryOffset._4, WeaponCategory.Explosive, 104)
        };
    }
}
