using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace Xenvious
{
    // Source: https://docs.fivem.net/docs/game-references/hud-colors/
    public sealed class HudColorDefinition
    {
        public HudColorDefinition(int index, string name, Color color, string sourceName = null, string displayName = null)
        {
            Index = index;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Color = color;
            SourceName = sourceName;
            DisplayName = displayName ?? name;

            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            Brush = brush;
        }

        public int Index { get; }

        public string Name { get; }

        public string DisplayName { get; }

        public Color Color { get; }

        public SolidColorBrush Brush { get; }

        public string SourceName { get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public static class HudColors
    {
        public static IReadOnlyList<HudColorDefinition> All => all;

        public static bool TryGetByIndex(int index, out HudColorDefinition definition)
        {
            return byIndex.TryGetValue(index, out definition);
        }

        public static bool TryGetByName(string name, out HudColorDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                definition = null;
                return false;
            }

            return byName.TryGetValue(name, out definition) || byName.TryGetValue(name.ToUpperInvariant(), out definition);
        }

        public static HudColorDefinition GetByIndex(int index)
        {
            if (!TryGetByIndex(index, out var definition))
            {
                throw new KeyNotFoundException($"Unknown HUD color index {index}.");
            }

            return definition;
        }

        public static HudColorDefinition GetByName(string name)
        {
            if (!TryGetByName(name, out var definition))
            {
                throw new KeyNotFoundException($"Unknown HUD color name '{name}'.");
            }

            return definition;
        }

        private static readonly ReadOnlyCollection<HudColorDefinition> all = Array.AsReadOnly(new[]
        {
            new HudColorDefinition(0, "HUD_COLOUR_PURE_WHITE", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(1, "HUD_COLOUR_WHITE", Color.FromArgb(255, 240, 240, 240)),
            new HudColorDefinition(2, "HUD_COLOUR_BLACK", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(3, "HUD_COLOUR_GREY", Color.FromArgb(255, 155, 155, 155)),
            new HudColorDefinition(4, "HUD_COLOUR_GREYLIGHT", Color.FromArgb(255, 205, 205, 205)),
            new HudColorDefinition(5, "HUD_COLOUR_GREYDARK", Color.FromArgb(255, 77, 77, 77)),
            new HudColorDefinition(6, "HUD_COLOUR_RED", Color.FromArgb(255, 224, 50, 50)),
            new HudColorDefinition(7, "HUD_COLOUR_REDLIGHT", Color.FromArgb(255, 240, 153, 153)),
            new HudColorDefinition(8, "HUD_COLOUR_REDDARK", Color.FromArgb(255, 112, 25, 25)),
            new HudColorDefinition(9, "HUD_COLOUR_BLUE", Color.FromArgb(255, 93, 182, 229)),
            new HudColorDefinition(10, "HUD_COLOUR_BLUELIGHT", Color.FromArgb(255, 174, 219, 242)),
            new HudColorDefinition(11, "HUD_COLOUR_BLUEDARK", Color.FromArgb(255, 47, 92, 115)),
            new HudColorDefinition(12, "HUD_COLOUR_YELLOW", Color.FromArgb(255, 240, 200, 80)),
            new HudColorDefinition(13, "HUD_COLOUR_YELLOWLIGHT", Color.FromArgb(255, 254, 235, 169)),
            new HudColorDefinition(14, "HUD_COLOUR_YELLOWDARK", Color.FromArgb(255, 126, 107, 41)),
            new HudColorDefinition(15, "HUD_COLOUR_ORANGE", Color.FromArgb(255, 255, 133, 85)),
            new HudColorDefinition(16, "HUD_COLOUR_ORANGELIGHT", Color.FromArgb(255, 255, 194, 170)),
            new HudColorDefinition(17, "HUD_COLOUR_ORANGEDARK", Color.FromArgb(255, 127, 66, 42)),
            new HudColorDefinition(18, "HUD_COLOUR_GREEN", Color.FromArgb(255, 114, 204, 114)),
            new HudColorDefinition(19, "HUD_COLOUR_GREENLIGHT", Color.FromArgb(255, 185, 230, 185)),
            new HudColorDefinition(20, "HUD_COLOUR_GREENDARK", Color.FromArgb(255, 57, 102, 57)),
            new HudColorDefinition(21, "HUD_COLOUR_PURPLE", Color.FromArgb(255, 132, 102, 226)),
            new HudColorDefinition(22, "HUD_COLOUR_PURPLELIGHT", Color.FromArgb(255, 192, 179, 239)),
            new HudColorDefinition(23, "HUD_COLOUR_PURPLEDARK", Color.FromArgb(255, 67, 57, 111)),
            new HudColorDefinition(24, "HUD_COLOUR_PINK", Color.FromArgb(255, 203, 54, 148)),
            new HudColorDefinition(25, "HUD_COLOUR_RADAR_HEALTH", Color.FromArgb(255, 53, 154, 71)),
            new HudColorDefinition(26, "HUD_COLOUR_RADAR_ARMOUR", Color.FromArgb(255, 93, 182, 229), "HUD_COLOUR_BLUE"),
            new HudColorDefinition(27, "HUD_COLOUR_RADAR_DAMAGE", Color.FromArgb(255, 235, 36, 39)),
            new HudColorDefinition(28, "HUD_COLOUR_NET_PLAYER1", Color.FromArgb(255, 194, 80, 80)),
            new HudColorDefinition(29, "HUD_COLOUR_NET_PLAYER2", Color.FromArgb(255, 156, 110, 175)),
            new HudColorDefinition(30, "HUD_COLOUR_NET_PLAYER3", Color.FromArgb(255, 255, 123, 196)),
            new HudColorDefinition(31, "HUD_COLOUR_NET_PLAYER4", Color.FromArgb(255, 247, 159, 123)),
            new HudColorDefinition(32, "HUD_COLOUR_NET_PLAYER5", Color.FromArgb(255, 178, 144, 132)),
            new HudColorDefinition(33, "HUD_COLOUR_NET_PLAYER6", Color.FromArgb(255, 141, 206, 167)),
            new HudColorDefinition(34, "HUD_COLOUR_NET_PLAYER7", Color.FromArgb(255, 113, 169, 175)),
            new HudColorDefinition(35, "HUD_COLOUR_NET_PLAYER8", Color.FromArgb(255, 211, 209, 231)),
            new HudColorDefinition(36, "HUD_COLOUR_NET_PLAYER9", Color.FromArgb(255, 144, 127, 153)),
            new HudColorDefinition(37, "HUD_COLOUR_NET_PLAYER10", Color.FromArgb(255, 106, 196, 191)),
            new HudColorDefinition(38, "HUD_COLOUR_NET_PLAYER11", Color.FromArgb(255, 214, 196, 153)),
            new HudColorDefinition(39, "HUD_COLOUR_NET_PLAYER12", Color.FromArgb(255, 234, 142, 80)),
            new HudColorDefinition(40, "HUD_COLOUR_NET_PLAYER13", Color.FromArgb(255, 152, 203, 234)),
            new HudColorDefinition(41, "HUD_COLOUR_NET_PLAYER14", Color.FromArgb(255, 178, 98, 135)),
            new HudColorDefinition(42, "HUD_COLOUR_NET_PLAYER15", Color.FromArgb(255, 144, 142, 122)),
            new HudColorDefinition(43, "HUD_COLOUR_NET_PLAYER16", Color.FromArgb(255, 166, 117, 94)),
            new HudColorDefinition(44, "HUD_COLOUR_NET_PLAYER17", Color.FromArgb(255, 175, 168, 168)),
            new HudColorDefinition(45, "HUD_COLOUR_NET_PLAYER18", Color.FromArgb(255, 232, 142, 155)),
            new HudColorDefinition(46, "HUD_COLOUR_NET_PLAYER19", Color.FromArgb(255, 187, 214, 91)),
            new HudColorDefinition(47, "HUD_COLOUR_NET_PLAYER20", Color.FromArgb(255, 12, 123, 86)),
            new HudColorDefinition(48, "HUD_COLOUR_NET_PLAYER21", Color.FromArgb(255, 123, 196, 255)),
            new HudColorDefinition(49, "HUD_COLOUR_NET_PLAYER22", Color.FromArgb(255, 171, 60, 230)),
            new HudColorDefinition(50, "HUD_COLOUR_NET_PLAYER23", Color.FromArgb(255, 206, 169, 13)),
            new HudColorDefinition(51, "HUD_COLOUR_NET_PLAYER24", Color.FromArgb(255, 71, 99, 173)),
            new HudColorDefinition(52, "HUD_COLOUR_NET_PLAYER25", Color.FromArgb(255, 42, 166, 185)),
            new HudColorDefinition(53, "HUD_COLOUR_NET_PLAYER26", Color.FromArgb(255, 186, 157, 125)),
            new HudColorDefinition(54, "HUD_COLOUR_NET_PLAYER27", Color.FromArgb(255, 201, 225, 255)),
            new HudColorDefinition(55, "HUD_COLOUR_NET_PLAYER28", Color.FromArgb(255, 240, 240, 150)),
            new HudColorDefinition(56, "HUD_COLOUR_NET_PLAYER29", Color.FromArgb(255, 237, 140, 161)),
            new HudColorDefinition(57, "HUD_COLOUR_NET_PLAYER30", Color.FromArgb(255, 249, 138, 138)),
            new HudColorDefinition(58, "HUD_COLOUR_NET_PLAYER31", Color.FromArgb(255, 252, 239, 166)),
            new HudColorDefinition(59, "HUD_COLOUR_NET_PLAYER32", Color.FromArgb(255, 240, 240, 240)),
            new HudColorDefinition(60, "HUD_COLOUR_SIMPLEBLIP_DEFAULT", Color.FromArgb(255, 159, 201, 166)),
            new HudColorDefinition(61, "HUD_COLOUR_MENU_BLUE", Color.FromArgb(255, 140, 140, 140)),
            new HudColorDefinition(62, "HUD_COLOUR_MENU_GREY_LIGHT", Color.FromArgb(255, 140, 140, 140), "HUD_COLOUR_MENU_BLUE"),
            new HudColorDefinition(63, "HUD_COLOUR_MENU_BLUE_EXTRA_DARK", Color.FromArgb(255, 40, 40, 40)),
            new HudColorDefinition(64, "HUD_COLOUR_MENU_YELLOW", Color.FromArgb(255, 240, 160, 0)),
            new HudColorDefinition(65, "HUD_COLOUR_MENU_YELLOW_DARK", Color.FromArgb(255, 240, 160, 0), "HUD_COLOUR_MENU_YELLOW"),
            new HudColorDefinition(66, "HUD_COLOUR_MENU_GREEN", Color.FromArgb(255, 240, 160, 0), "HUD_COLOUR_MENU_YELLOW"),
            new HudColorDefinition(67, "HUD_COLOUR_MENU_GREY", Color.FromArgb(255, 140, 140, 140)),
            new HudColorDefinition(68, "HUD_COLOUR_MENU_GREY_DARK", Color.FromArgb(255, 60, 60, 60)),
            new HudColorDefinition(69, "HUD_COLOUR_MENU_HIGHLIGHT", Color.FromArgb(255, 30, 30, 30)),
            new HudColorDefinition(70, "HUD_COLOUR_MENU_STANDARD", Color.FromArgb(255, 140, 140, 140)),
            new HudColorDefinition(71, "HUD_COLOUR_MENU_DIMMED", Color.FromArgb(255, 75, 75, 75)),
            new HudColorDefinition(72, "HUD_COLOUR_MENU_EXTRA_DIMMED", Color.FromArgb(255, 50, 50, 50)),
            new HudColorDefinition(73, "HUD_COLOUR_BRIEF_TITLE", Color.FromArgb(255, 95, 95, 95)),
            new HudColorDefinition(74, "HUD_COLOUR_MID_GREY_MP", Color.FromArgb(255, 100, 100, 100)),
            new HudColorDefinition(75, "HUD_COLOUR_NET_PLAYER1_DARK", Color.FromArgb(255, 93, 39, 39)),
            new HudColorDefinition(76, "HUD_COLOUR_NET_PLAYER2_DARK", Color.FromArgb(255, 77, 55, 89)),
            new HudColorDefinition(77, "HUD_COLOUR_NET_PLAYER3_DARK", Color.FromArgb(255, 124, 62, 99)),
            new HudColorDefinition(78, "HUD_COLOUR_NET_PLAYER4_DARK", Color.FromArgb(255, 120, 80, 80)),
            new HudColorDefinition(79, "HUD_COLOUR_NET_PLAYER5_DARK", Color.FromArgb(255, 87, 72, 66)),
            new HudColorDefinition(80, "HUD_COLOUR_NET_PLAYER6_DARK", Color.FromArgb(255, 74, 103, 83)),
            new HudColorDefinition(81, "HUD_COLOUR_NET_PLAYER7_DARK", Color.FromArgb(255, 60, 85, 88)),
            new HudColorDefinition(82, "HUD_COLOUR_NET_PLAYER8_DARK", Color.FromArgb(255, 105, 105, 64)),
            new HudColorDefinition(83, "HUD_COLOUR_NET_PLAYER9_DARK", Color.FromArgb(255, 72, 63, 76)),
            new HudColorDefinition(84, "HUD_COLOUR_NET_PLAYER10_DARK", Color.FromArgb(255, 53, 98, 95)),
            new HudColorDefinition(85, "HUD_COLOUR_NET_PLAYER11_DARK", Color.FromArgb(255, 107, 98, 76)),
            new HudColorDefinition(86, "HUD_COLOUR_NET_PLAYER12_DARK", Color.FromArgb(255, 117, 71, 40)),
            new HudColorDefinition(87, "HUD_COLOUR_NET_PLAYER13_DARK", Color.FromArgb(255, 76, 101, 117)),
            new HudColorDefinition(88, "HUD_COLOUR_NET_PLAYER14_DARK", Color.FromArgb(255, 65, 35, 47)),
            new HudColorDefinition(89, "HUD_COLOUR_NET_PLAYER15_DARK", Color.FromArgb(255, 72, 71, 61)),
            new HudColorDefinition(90, "HUD_COLOUR_NET_PLAYER16_DARK", Color.FromArgb(255, 85, 58, 47)),
            new HudColorDefinition(91, "HUD_COLOUR_NET_PLAYER17_DARK", Color.FromArgb(255, 87, 84, 84)),
            new HudColorDefinition(92, "HUD_COLOUR_NET_PLAYER18_DARK", Color.FromArgb(255, 116, 71, 77)),
            new HudColorDefinition(93, "HUD_COLOUR_NET_PLAYER19_DARK", Color.FromArgb(255, 93, 107, 45)),
            new HudColorDefinition(94, "HUD_COLOUR_NET_PLAYER20_DARK", Color.FromArgb(255, 6, 61, 43)),
            new HudColorDefinition(95, "HUD_COLOUR_NET_PLAYER21_DARK", Color.FromArgb(255, 61, 98, 127)),
            new HudColorDefinition(96, "HUD_COLOUR_NET_PLAYER22_DARK", Color.FromArgb(255, 85, 30, 115)),
            new HudColorDefinition(97, "HUD_COLOUR_NET_PLAYER23_DARK", Color.FromArgb(255, 103, 84, 6)),
            new HudColorDefinition(98, "HUD_COLOUR_NET_PLAYER24_DARK", Color.FromArgb(255, 35, 49, 86)),
            new HudColorDefinition(99, "HUD_COLOUR_NET_PLAYER25_DARK", Color.FromArgb(255, 21, 83, 92)),
            new HudColorDefinition(100, "HUD_COLOUR_NET_PLAYER26_DARK", Color.FromArgb(255, 93, 98, 62)),
            new HudColorDefinition(101, "HUD_COLOUR_NET_PLAYER27_DARK", Color.FromArgb(255, 100, 112, 127)),
            new HudColorDefinition(102, "HUD_COLOUR_NET_PLAYER28_DARK", Color.FromArgb(255, 120, 120, 75)),
            new HudColorDefinition(103, "HUD_COLOUR_NET_PLAYER29_DARK", Color.FromArgb(255, 152, 76, 93)),
            new HudColorDefinition(104, "HUD_COLOUR_NET_PLAYER30_DARK", Color.FromArgb(255, 124, 69, 69)),
            new HudColorDefinition(105, "HUD_COLOUR_NET_PLAYER31_DARK", Color.FromArgb(255, 10, 43, 50)),
            new HudColorDefinition(106, "HUD_COLOUR_NET_PLAYER32_DARK", Color.FromArgb(255, 95, 95, 10)),
            new HudColorDefinition(107, "HUD_COLOUR_BRONZE", Color.FromArgb(255, 180, 130, 97)),
            new HudColorDefinition(108, "HUD_COLOUR_SILVER", Color.FromArgb(255, 150, 153, 161)),
            new HudColorDefinition(109, "HUD_COLOUR_GOLD", Color.FromArgb(255, 214, 181, 99)),
            new HudColorDefinition(110, "HUD_COLOUR_PLATINUM", Color.FromArgb(255, 166, 221, 190)),
            new HudColorDefinition(111, "HUD_COLOUR_GANG1", Color.FromArgb(255, 29, 100, 153)),
            new HudColorDefinition(112, "HUD_COLOUR_GANG2", Color.FromArgb(255, 214, 116, 15)),
            new HudColorDefinition(113, "HUD_COLOUR_GANG3", Color.FromArgb(255, 135, 125, 142)),
            new HudColorDefinition(114, "HUD_COLOUR_GANG4", Color.FromArgb(255, 229, 119, 185)),
            new HudColorDefinition(115, "HUD_COLOUR_SAME_CREW", Color.FromArgb(255, 252, 239, 166)),
            new HudColorDefinition(116, "HUD_COLOUR_FREEMODE", Color.FromArgb(255, 45, 110, 185)),
            new HudColorDefinition(117, "HUD_COLOUR_PAUSE_BG", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(118, "HUD_COLOUR_FRIENDLY", Color.FromArgb(255, 93, 182, 229)),
            new HudColorDefinition(119, "HUD_COLOUR_ENEMY", Color.FromArgb(255, 194, 80, 80)),
            new HudColorDefinition(120, "HUD_COLOUR_LOCATION", Color.FromArgb(255, 240, 200, 80), "HUD_COLOUR_YELLOW"),
            new HudColorDefinition(121, "HUD_COLOUR_PICKUP", Color.FromArgb(255, 114, 204, 114), "HUD_COLOUR_GREEN"),
            new HudColorDefinition(122, "HUD_COLOUR_PAUSE_SINGLEPLAYER", Color.FromArgb(255, 114, 204, 114), "HUD_COLOUR_GREEN"),
            new HudColorDefinition(123, "HUD_COLOUR_FREEMODE_DARK", Color.FromArgb(255, 22, 55, 92)),
            new HudColorDefinition(124, "HUD_COLOUR_INACTIVE_MISSION", Color.FromArgb(255, 154, 154, 154)),
            new HudColorDefinition(125, "HUD_COLOUR_DAMAGE", Color.FromArgb(255, 194, 80, 80)),
            new HudColorDefinition(126, "HUD_COLOUR_PINKLIGHT", Color.FromArgb(255, 252, 115, 201)),
            new HudColorDefinition(127, "HUD_COLOUR_PM_MITEM_HIGHLIGHT", Color.FromArgb(255, 252, 177, 49)),
            new HudColorDefinition(128, "HUD_COLOUR_SCRIPT_VARIABLE", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(129, "HUD_COLOUR_YOGA", Color.FromArgb(255, 109, 247, 204)),
            new HudColorDefinition(130, "HUD_COLOUR_TENNIS", Color.FromArgb(255, 241, 101, 34)),
            new HudColorDefinition(131, "HUD_COLOUR_GOLF", Color.FromArgb(255, 214, 189, 97)),
            new HudColorDefinition(132, "HUD_COLOUR_SHOOTING_RANGE", Color.FromArgb(255, 112, 25, 25), "HUD_COLOUR_REDDARK"),
            new HudColorDefinition(133, "HUD_COLOUR_FLIGHT_SCHOOL", Color.FromArgb(255, 47, 92, 115), "HUD_COLOUR_BLUEDARK"),
            new HudColorDefinition(134, "HUD_COLOUR_NORTH_BLUE", Color.FromArgb(255, 93, 182, 229), "HUD_COLOUR_BLUE"),
            new HudColorDefinition(135, "HUD_COLOUR_SOCIAL_CLUB", Color.FromArgb(255, 234, 153, 28)),
            new HudColorDefinition(136, "HUD_COLOUR_PLATFORM_BLUE", Color.FromArgb(255, 11, 55, 123)),
            new HudColorDefinition(137, "HUD_COLOUR_PLATFORM_GREEN", Color.FromArgb(255, 146, 200, 62)),
            new HudColorDefinition(138, "HUD_COLOUR_PLATFORM_GREY", Color.FromArgb(255, 234, 153, 28)),
            new HudColorDefinition(139, "HUD_COLOUR_FACEBOOK_BLUE", Color.FromArgb(255, 66, 89, 148)),
            new HudColorDefinition(140, "HUD_COLOUR_INGAME_BG", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(141, "HUD_COLOUR_DARTS", Color.FromArgb(255, 114, 204, 114), "HUD_COLOUR_GREEN"),
            new HudColorDefinition(142, "HUD_COLOUR_WAYPOINT", Color.FromArgb(255, 164, 76, 242)),
            new HudColorDefinition(143, "HUD_COLOUR_MICHAEL", Color.FromArgb(255, 101, 180, 212)),
            new HudColorDefinition(144, "HUD_COLOUR_FRANKLIN", Color.FromArgb(255, 171, 237, 171)),
            new HudColorDefinition(145, "HUD_COLOUR_TREVOR", Color.FromArgb(255, 255, 163, 87)),
            new HudColorDefinition(146, "HUD_COLOUR_GOLF_P1", Color.FromArgb(255, 240, 240, 240), "HUD_COLOUR_WHITE"),
            new HudColorDefinition(147, "HUD_COLOUR_GOLF_P2", Color.FromArgb(255, 235, 239, 30)),
            new HudColorDefinition(148, "HUD_COLOUR_GOLF_P3", Color.FromArgb(255, 255, 149, 14)),
            new HudColorDefinition(149, "HUD_COLOUR_GOLF_P4", Color.FromArgb(255, 246, 60, 161)),
            new HudColorDefinition(150, "HUD_COLOUR_WAYPOINTLIGHT", Color.FromArgb(255, 210, 166, 249)),
            new HudColorDefinition(151, "HUD_COLOUR_WAYPOINTDARK", Color.FromArgb(255, 82, 38, 121)),
            new HudColorDefinition(152, "HUD_COLOUR_PANEL_LIGHT", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(153, "HUD_COLOUR_MICHAEL_DARK", Color.FromArgb(255, 72, 103, 116)),
            new HudColorDefinition(154, "HUD_COLOUR_FRANKLIN_DARK", Color.FromArgb(255, 85, 118, 85)),
            new HudColorDefinition(155, "HUD_COLOUR_TREVOR_DARK", Color.FromArgb(255, 127, 81, 43)),
            new HudColorDefinition(156, "HUD_COLOUR_OBJECTIVE_ROUTE", Color.FromArgb(255, 240, 200, 80), "HUD_COLOUR_YELLOW"),
            new HudColorDefinition(157, "HUD_COLOUR_PAUSEMAP_TINT", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(158, "HUD_COLOUR_PAUSE_DESELECT", Color.FromArgb(255, 100, 100, 100)),
            new HudColorDefinition(159, "HUD_COLOUR_PM_WEAPONS_PURCHASABLE", Color.FromArgb(255, 45, 110, 185), "HUD_COLOUR_FREEMODE"),
            new HudColorDefinition(160, "HUD_COLOUR_PM_WEAPONS_LOCKED", Color.FromArgb(255, 240, 240, 240)),
            new HudColorDefinition(161, "HUD_COLOUR_END_SCREEN_BG", Color.FromArgb(255, 0, 0, 0), "HUD_COLOUR_INGAME_BG"),
            new HudColorDefinition(162, "HUD_COLOUR_CHOP", Color.FromArgb(255, 224, 50, 50), "HUD_COLOUR_RED"),
            new HudColorDefinition(163, "HUD_COLOUR_PAUSEMAP_TINT_HALF", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(164, "HUD_COLOUR_NORTH_BLUE_OFFICIAL", Color.FromArgb(255, 0, 71, 133)),
            new HudColorDefinition(165, "HUD_COLOUR_SCRIPT_VARIABLE_2", Color.FromArgb(255, 0, 0, 0)),
            new HudColorDefinition(166, "HUD_COLOUR_H", Color.FromArgb(255, 33, 118, 37)),
            new HudColorDefinition(167, "HUD_COLOUR_HDARK", Color.FromArgb(255, 37, 102, 40)),
            new HudColorDefinition(168, "HUD_COLOUR_T", Color.FromArgb(255, 234, 153, 28)),
            new HudColorDefinition(169, "HUD_COLOUR_TDARK", Color.FromArgb(255, 225, 140, 8)),
            new HudColorDefinition(170, "HUD_COLOUR_HSHARD", Color.FromArgb(255, 20, 40, 0)),
            new HudColorDefinition(171, "HUD_COLOUR_CONTROLLER_MICHAEL", Color.FromArgb(255, 48, 255, 255)),
            new HudColorDefinition(172, "HUD_COLOUR_CONTROLLER_FRANKLIN", Color.FromArgb(255, 48, 255, 0)),
            new HudColorDefinition(173, "HUD_COLOUR_CONTROLLER_TREVOR", Color.FromArgb(255, 176, 80, 0)),
            new HudColorDefinition(174, "HUD_COLOUR_CONTROLLER_CHOP", Color.FromArgb(255, 127, 0, 0)),
            new HudColorDefinition(175, "HUD_COLOUR_VIDEO_EDITOR_VIDEO", Color.FromArgb(255, 53, 166, 224)),
            new HudColorDefinition(176, "HUD_COLOUR_VIDEO_EDITOR_AUDIO", Color.FromArgb(255, 162, 79, 157)),
            new HudColorDefinition(177, "HUD_COLOUR_VIDEO_EDITOR_TEXT", Color.FromArgb(255, 104, 192, 141)),
            new HudColorDefinition(178, "HUD_COLOUR_HB_BLUE", Color.FromArgb(255, 29, 100, 153)),
            new HudColorDefinition(179, "HUD_COLOUR_HB_YELLOW", Color.FromArgb(255, 234, 153, 28)),
            new HudColorDefinition(180, "HUD_COLOUR_VIDEO_EDITOR_SCORE", Color.FromArgb(255, 240, 160, 1)),
            new HudColorDefinition(181, "HUD_COLOUR_VIDEO_EDITOR_AUDIO_FADEOUT", Color.FromArgb(255, 59, 34, 57)),
            new HudColorDefinition(182, "HUD_COLOUR_VIDEO_EDITOR_TEXT_FADEOUT", Color.FromArgb(255, 41, 68, 53)),
            new HudColorDefinition(183, "HUD_COLOUR_VIDEO_EDITOR_SCORE_FADEOUT", Color.FromArgb(255, 82, 58, 10)),
            new HudColorDefinition(184, "HUD_COLOUR_HEIST_BACKGROUND", Color.FromArgb(255, 37, 102, 40)),
            new HudColorDefinition(185, "HUD_COLOUR_VIDEO_EDITOR_AMBIENT", Color.FromArgb(255, 240, 200, 80), "HUD_COLOUR_YELLOW"),
            new HudColorDefinition(186, "HUD_COLOUR_VIDEO_EDITOR_AMBIENT_FADEOUT", Color.FromArgb(255, 80, 70, 34)),
            new HudColorDefinition(187, "HUD_COLOUR_VIDEO_EDITOR_AMBIENT_DARK", Color.FromArgb(255, 255, 133, 85), "HUD_COLOUR_ORANGE"),
            new HudColorDefinition(188, "HUD_COLOUR_VIDEO_EDITOR_AMBIENT_LIGHT", Color.FromArgb(255, 255, 194, 170), "HUD_COLOUR_ORANGELIGHT"),
            new HudColorDefinition(189, "HUD_COLOUR_VIDEO_EDITOR_AMBIENT_MID", Color.FromArgb(255, 255, 133, 85), "HUD_COLOUR_ORANGE"),
            new HudColorDefinition(190, "HUD_COLOUR_LOW_FLOW", Color.FromArgb(255, 240, 200, 80), "HUD_COLOUR_YELLOW"),
            new HudColorDefinition(191, "HUD_COLOUR_LOW_FLOW_DARK", Color.FromArgb(255, 126, 107, 41), "HUD_COLOUR_YELLOWDARK"),
            new HudColorDefinition(192, "HUD_COLOUR_G1", Color.FromArgb(255, 247, 159, 123)),
            new HudColorDefinition(193, "HUD_COLOUR_G2", Color.FromArgb(255, 226, 134, 187)),
            new HudColorDefinition(194, "HUD_COLOUR_G3", Color.FromArgb(255, 239, 238, 151)),
            new HudColorDefinition(195, "HUD_COLOUR_G4", Color.FromArgb(255, 113, 169, 175)),
            new HudColorDefinition(196, "HUD_COLOUR_G5", Color.FromArgb(255, 160, 140, 193)),
            new HudColorDefinition(197, "HUD_COLOUR_G6", Color.FromArgb(255, 141, 206, 167)),
            new HudColorDefinition(198, "HUD_COLOUR_G7", Color.FromArgb(255, 181, 214, 234)),
            new HudColorDefinition(199, "HUD_COLOUR_G8", Color.FromArgb(255, 178, 144, 132)),
            new HudColorDefinition(200, "HUD_COLOUR_G9", Color.FromArgb(255, 0, 132, 114)),
            new HudColorDefinition(201, "HUD_COLOUR_G10", Color.FromArgb(255, 216, 85, 117)),
            new HudColorDefinition(202, "HUD_COLOUR_G11", Color.FromArgb(255, 30, 100, 152)),
            new HudColorDefinition(203, "HUD_COLOUR_G12", Color.FromArgb(255, 43, 181, 117)),
            new HudColorDefinition(204, "HUD_COLOUR_G13", Color.FromArgb(255, 233, 141, 79)),
            new HudColorDefinition(205, "HUD_COLOUR_G14", Color.FromArgb(255, 137, 210, 215)),
            new HudColorDefinition(206, "HUD_COLOUR_G15", Color.FromArgb(255, 134, 125, 141)),
            new HudColorDefinition(207, "HUD_COLOUR_ADVERSARY", Color.FromArgb(255, 109, 34, 33)),
            new HudColorDefinition(208, "HUD_COLOUR_DEGEN_RED", Color.FromArgb(255, 255, 0, 0)),
            new HudColorDefinition(209, "HUD_COLOUR_DEGEN_YELLOW", Color.FromArgb(255, 255, 255, 0)),
            new HudColorDefinition(210, "HUD_COLOUR_DEGEN_GREEN", Color.FromArgb(255, 0, 255, 0)),
            new HudColorDefinition(211, "HUD_COLOUR_DEGEN_CYAN", Color.FromArgb(255, 0, 255, 255)),
            new HudColorDefinition(212, "HUD_COLOUR_DEGEN_BLUE", Color.FromArgb(255, 0, 0, 255)),
            new HudColorDefinition(213, "HUD_COLOUR_DEGEN_MAGENTA", Color.FromArgb(255, 255, 0, 255)),
            new HudColorDefinition(214, "HUD_COLOUR_STUNT_1", Color.FromArgb(255, 38, 136, 234)),
            new HudColorDefinition(215, "HUD_COLOUR_STUNT_2", Color.FromArgb(255, 224, 50, 50), "HUD_COLOUR_RED"),
            new HudColorDefinition(216, "HUD_COLOUR_SPECIAL_RACE_SERIES", Color.FromArgb(255, 154, 178, 54)),
            new HudColorDefinition(217, "HUD_COLOUR_SPECIAL_RACE_SERIES_DARK", Color.FromArgb(255, 93, 107, 45)),
            new HudColorDefinition(218, "HUD_COLOUR_CS", Color.FromArgb(255, 206, 169, 13)),
            new HudColorDefinition(219, "HUD_COLOUR_CS_DARK", Color.FromArgb(255, 103, 84, 6)),
            new HudColorDefinition(220, "HUD_COLOUR_TECH_GREEN", Color.FromArgb(255, 0, 151, 151)),
            new HudColorDefinition(221, "HUD_COLOUR_TECH_GREEN_DARK", Color.FromArgb(255, 5, 119, 113)),
            new HudColorDefinition(222, "HUD_COLOUR_TECH_RED", Color.FromArgb(255, 151, 0, 0)),
            new HudColorDefinition(223, "HUD_COLOUR_TECH_GREEN_VERY_DARK", Color.FromArgb(255, 0, 40, 40)),
            new HudColorDefinition(224, "HUD_COLOUR_PLACEHOLDER_01", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(225, "HUD_COLOUR_PLACEHOLDER_02", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(226, "HUD_COLOUR_PLACEHOLDER_03", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(227, "HUD_COLOUR_PLACEHOLDER_04", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(228, "HUD_COLOUR_PLACEHOLDER_05", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(229, "HUD_COLOUR_PLACEHOLDER_06", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(230, "HUD_COLOUR_PLACEHOLDER_07", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(231, "HUD_COLOUR_PLACEHOLDER_08", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(232, "HUD_COLOUR_PLACEHOLDER_09", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(233, "HUD_COLOUR_PLACEHOLDER_10", Color.FromArgb(255, 255, 255, 255)),
            new HudColorDefinition(234, "HUD_COLOUR_JUNK_ENERGY", Color.FromArgb(255, 29, 237, 195)),
        });

        private static readonly IReadOnlyDictionary<int, HudColorDefinition> byIndex = all.ToDictionary(color => color.Index);

        private static readonly IReadOnlyDictionary<string, HudColorDefinition> byName = all.ToDictionary(color => color.Name, StringComparer.OrdinalIgnoreCase);
    }
}

