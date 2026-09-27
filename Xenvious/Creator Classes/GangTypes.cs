using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// Gang chase types (iGangBackupType of a rule). Numbers and factions from Rockstar's enum
    /// ciBACKUP_TYPE_* (0-52, the current creator allows the same 53); vehicle, peds and weapons
    /// from GET_GANG_CHASE_UNIT_* in the mission controller. The four custom types take their
    /// units from the job's own gang chase unit setup.
    /// </summary>
    public static class GangTypes
    {
        public sealed class GangType
        {
            public int Id;
            public string Group;
            public string Faction;
            public string Vehicle;
            public string Ped;
            public string Weapon;
            public int Peds;
        }

        /// <summary>Groups of the picker, in the order it shows them (key gt_grp_<name>).</summary>
        public static readonly string[] Groups = { "none", "gangs", "security", "air", "sea", "custom" };

        private static GangType G(int id, string group, string faction, string vehicle, string ped, string weapon, int peds)
            => new GangType { Id = id, Group = group, Faction = faction, Vehicle = vehicle, Ped = ped, Weapon = weapon, Peds = peds };

        public static readonly IReadOnlyList<GangType> All = new[]
        {
            G(0, "none", "none", null, null, null, 0),
            G(1, "security", "merryweather", "Patriot", "s_m_y_blackops_01", "Micro SMG", 2),
            G(2, "gangs", "lost", "Daemon", "g_m_y_lost_01", "Pistol", 2),
            G(3, "gangs", "vagos", "Cavalcade", "g_m_y_mexgoon_02", "Pistol", 2),
            G(4, "gangs", "families", "Baller", "g_m_y_famca_01", "Micro SMG", 2),
            G(5, "security", "pros", "Granger", "mp_g_m_pros_01", "Micro SMG", 2),
            G(6, "gangs", "ballas", "Baller", "g_m_y_ballaorig_01", "Micro SMG", 2),
            G(7, "gangs", "salva", "Emperor", "g_m_y_salvagoon_01", "Micro SMG", 2),
            G(8, "gangs", "altruists", "Bodhi", "a_m_o_acult_02", "Micro SMG", 2),
            G(9, "security", "fib", "FIB", "s_m_m_fibsec_01", "Pistol", 2),
            G(10, "gangs", "oneil", "Sandking", "a_m_m_hillbilly_01", "Pistol", 2),
            G(11, "gangs", "koreans", "Fugitive", "g_m_y_korean_01", "Pistol", 2),
            G(12, "security", "merryweather", "Mesa", "s_m_y_blackops_01", "Micro SMG", 2),
            G(13, "gangs", "koreans", "Feltzer", "g_m_y_korean_01", "Micro SMG", 2),
            G(14, "security", "army", "Mesa", "s_m_y_marine_03", "Pistol", 2),
            G(15, "gangs", "lost", "Slamvan", "g_m_y_lost_01", "Pistol", 2),
            G(16, "security", "army", "Crusader", "s_m_y_marine_03", "Pistol", 2),
            G(17, "gangs", "vagos", "Blade", "g_m_y_mexgoon_02", "Pistol", 2),
            G(18, "gangs", "vagos", "Emperor", "g_m_y_mexgoon_02", "Pistol", 2),
            G(19, "gangs", "ballas", "Buccaneer", "g_m_y_ballaorig_01", "Pistol", 2),
            G(20, "air", "merryweather", "Buzzard", "s_m_y_blackops_01", "Micro SMG", 2),
            G(21, "air", "noose", "Buzzard", "s_m_y_swat_01", "Micro SMG", 2),
            G(22, "air", "pros", "Buzzard", "mp_g_m_pros_01", "Micro SMG", 2),
            G(23, "air", "noose", "Maverick", "s_m_y_swat_01", "Micro SMG", 2),
            G(24, "air", "police", "Maverick", "s_m_y_cop_01", "Micro SMG", 2),
            G(25, "air", "merryweather", "Frogger", "s_m_y_blackops_01", "Micro SMG", 2),
            G(26, "air", "pros", "Frogger", "mp_g_m_pros_01", "Micro SMG", 2),
            G(27, "air", "lost", "Frogger", "g_m_y_lost_01", "Pistol", 2),
            G(28, "air", "vagos", "Frogger", "g_m_y_mexgoon_02", "Pistol", 2),
            G(29, "air", "koreans", "Frogger", "g_m_y_korean_01", "Pistol", 2),
            G(30, "air", "merryweather", "Valkyrie", "s_m_y_blackops_01", "Micro SMG", 4),
            G(31, "air", "pros", "Valkyrie", "mp_g_m_pros_01", "Micro SMG", 4),
            G(32, "air", "merryweather", "Hunter", "s_m_y_blackops_01", "Micro SMG", 2),
            G(33, "air", "pros", "Hunter", "mp_g_m_pros_01", "Micro SMG", 2),
            G(34, "air", "merryweather", "Savage", "s_m_y_blackops_01", "Micro SMG", 2),
            G(35, "air", "pros", "Savage", "mp_g_m_pros_01", "Micro SMG", 2),
            G(36, "air", "merryweather", "Lazer", "mp_g_m_pros_01", "Micro SMG", 2),
            G(37, "air", "merryweather", "Molotok", "mp_g_m_pros_01", "Micro SMG", 2),
            G(38, "security", "bogdan", "Nightshark", "mp_m_bogdangoon", "Micro SMG", 2),
            G(39, "security", "bogdan", "Insurgent Pick-Up", "mp_m_bogdangoon", "Micro SMG", 2),
            G(40, "security", "bogdan", "Insurgent", "mp_m_bogdangoon", "Micro SMG", 2),
            G(41, "air", "avon", "Buzzard", "mp_m_avongoon", "Micro SMG", 2),
            G(42, "security", "avon", "Nightshark", "mp_m_avongoon", "Micro SMG", 2),
            G(43, "security", "avon", "Dubsta 6x6", "mp_m_avongoon", "Micro SMG", 2),
            G(44, "security", "pros", "Kamacho", "mp_g_m_pros_01", "Micro SMG", 2),
            G(45, "security", "pros", "Contender", "mp_g_m_pros_01", "Micro SMG", 2),
            G(46, "gangs", "hillbillies", "Caracara", "g_m_m_casrn_01", "Micro SMG", 2),
            G(47, "sea", "koreans", "Dinghy", "g_m_y_korean_01", "Machine Pistol", 2),
            G(48, "sea", "pros", "Dinghy", "mp_g_m_pros_01", "Machine Pistol", 2),
            G(49, "custom", "custom1", null, null, null, 0),
            G(50, "custom", "custom2", null, null, null, 0),
            G(51, "custom", "custom3", null, null, null, 0),
            G(52, "custom", "custom4", null, null, null, 0),
        };

        public static GangType Find(int id) => All.FirstOrDefault(t => t.Id == id);
    }
}
