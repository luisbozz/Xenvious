using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// Zone types (zntp): names from Rockstar's enum ciFMMC_ZONE_TYPE__* (0-78) and what the
    /// value fields mean from GET_ZONE_VALUE_ONE/TWO_* in the creator source; the current
    /// public_mission_creator (Enhanced) still uses the same numbers and value labels. The game
    /// knows 112 types; 79-111 are newer than that source and can only be typed in as a number.
    /// Value 1 is znwd (f_28), value 2 is znwvd (f_29).
    /// </summary>
    public static class ZoneTypes
    {
        public const int Count = 112;

        public sealed class ZoneType
        {
            public int Id;
            public string Name;
            public string Description;
            public string Group;
            public string Value1Key;
            public string Value1Fallback;
            public string Value2Key;
            public string Value2Fallback;
        }

        /// <summary>Groups of the picker, in the order it shows them (key zt_grp_<name>).</summary>
        public static readonly string[] Groups = { "npc", "veh", "player", "air", "obj", "mission", "other" };

        private static ZoneType T(int id, string group, string name, string description, string value1 = null, string value1Fallback = null, string value2 = null, string value2Fallback = null)
            => new ZoneType { Id = id, Group = group, Name = name, Description = description, Value1Key = value1, Value1Fallback = value1Fallback, Value2Key = value2, Value2Fallback = value2Fallback };

        // Names and descriptions are English fallbacks; the page shows zt_name_<id> and zt_desc_<id>.
        public static readonly IReadOnlyList<ZoneType> All = new[]
        {
            T(0, "npc", "Peds and vehicles", "No pedestrians and no traffic in the zone."), // CLEAR_ALL
            T(1, "npc", "Peds", "No pedestrians in the zone."), // CLEAR_PEDS
            T(2, "npc", "Vehicles", "No traffic in the zone."), // CLEAR_VEHICLES
            T(3, "player", "Clear players", "Clears players out of the zone. Rockstar's label for it is \"Contact Mission Area\"."), // CLEAR_PLAYERS
            T(4, "npc", "Peds, vehicles and personal vehicles", "Like type 0, and personal vehicles are removed too."), // CLEAR_ALL_AND_PV
            T(5, "player", "No cover", "Players and NPCs cannot take cover in the zone."), // CLEAR_COVER
            T(6, "veh", "Rocket fire", "Rockets are fired at players in the zone."), // FIRE_ROCKETS_AT_PLAYER
            T(7, "player", "Block GPS", "The GPS plans no route through the zone."), // GPS
            T(8, "player", "No player spawning", "Players do not respawn inside the zone."), // NO_PLAYER_SPAWNING
            T(9, "air", "Calm water", "Calms the water in the zone. Value 1 calms the water, value 2 the waves (0 to 1 each). At most 8 per job.", "zt_value_calm", "Water calm", "zt_value_waves", "Wave strength"), // WATER_DAMPENING
            T(10, "player", "Explosion area", "Area for explosions. Value 1 is the threshold (0.1 to 1000).", "zt_value_threshold", "Threshold"), // EXPLOSION_AREA
            T(11, "other", "Menu clear", "Rockstar's internal type MENU_CLEAR; not offered in the creator."), // MENU_CLEAR
            T(12, "npc", "No emergency services", "Police, army and other emergency services do not spawn in the zone."), // DISPATCH_SPAWN_BLOCK
            T(13, "obj", "Force object", "Rockstar's type FORCE_OBJECT; what it does in a job is not checked yet."), // FORCE_OBJECT
            T(14, "player", "No wanted level", "No wanted level in the zone."), // BLOCK_WANTED_LEVEL
            T(15, "air", "Bomber arena", "Arena area for bomber modes."), // BOMBER_ARENA
            T(16, "player", "No jumping and climbing", "Players cannot jump or climb in the zone."), // BLOCK_JUMPING_AND_CLIMBING
            T(17, "player", "No running", "Players cannot run in the zone. Value 1 is the move speed (0.1 to 4).", "zt_value_movespeed", "Move speed"), // BLOCK_RUNNING
            T(18, "player", "No weapons", "Players cannot use weapons in the zone."), // BLOCK_WEAPONS
            T(19, "air", "Open parachute", "Opens the parachute in free fall.", "zt_value_value", "Value 1"), // AUTO_PARACHUTE
            T(20, "player", "Spawn protection", "Protects the spawn area of a team. Value 1 is the team (1 to 4).", "zt_value_team", "Team"), // SPAWN_PROTECTION
            T(21, "player", "Show player on map", "Players in the zone are shown on the map."), // BLIP_PLAYER
            T(22, "npc", "Alert NPCs", "NPCs are alerted when a player enters the zone."), // ALERT_PEDS
            T(23, "veh", "Air drag", "Changes the air drag of the player's vehicle. Value 1 is the factor (0.5 to 15, 1 is normal).", "zt_value_drag", "Drag factor"), // SET_AIR_DRAG_MULTIPLIER
            T(24, "veh", "Can't leave vehicle", "Players cannot get out of their vehicle in the zone."), // PLAYER_CANT_EXIT_VEHICLE
            T(25, "obj", "Object respawn", "Objects respawn in this zone."), // OBJECT_RESPAWN
            T(26, "veh", "Refill special ability", "Refills the special ability, for example the rocket boost."), // REFILL_SPECIAL_ABILITY
            T(27, "obj", "Object respawn inside", "Objects respawn at a point inside this zone."), // OBJECT_RESPAWN_WITHIN
            T(28, "air", "No VTOL switch", "VTOL aircraft cannot switch their mode in the zone."), // BLOCK_VTOL_TOGGLE
            T(29, "npc", "Road switch", "Switches the roads in the zone (Rockstar: ACTIVATE_ROADS)."), // ACTIVATE_ROADS
            T(30, "air", "No parachute", "The parachute cannot be opened in the zone."), // BLOCK_PARACHUTE
            T(31, "player", "No electrocution", "Players cannot be electrocuted in the zone."), // BLOCK_ELECTROCUTION
            T(32, "veh", "Can't drive", "Players cannot drive vehicles in the zone."), // PLAYER_CANT_DRIVE_VEHICLE
            T(33, "veh", "Speed limit", "Limits the speed of the player's vehicle. Value 1 is the top speed (1 to 100).", "zt_value_maxspeed", "Top speed"), // PLAYER_VEHICLE_MAX_SPEED
            T(34, "player", "No control", "Players lose control in the zone."), // BLOCK_PLAYER_CONTROL
            T(35, "player", "Kill zone", "Players who enter the zone die."), // KILL_ZONE
            T(36, "mission", "Goal area", "Goal for a team; the creator sets the team and the points."), // GOAL_AREA
            T(37, "veh", "Pit stop", "Pit area of a race: repairs the vehicle. Value 1: 5 to 150, value 2: 0.1 to 15.", "zt_value_pit1", "Value 1", "zt_value_pit2", "Value 2"), // RACE_PIT_ZONE
            T(38, "veh", "Stunt score", "Stunts in the zone give points."), // STUNT_SCORE
            T(39, "obj", "Return object", "Objects that get into the zone go back to their start position."), // RETURN_OBJECT
            T(40, "player", "Explode player", "The player explodes after the set time in the zone. Value 1 is the time in seconds (1 to 20).", "zt_value_time", "Time (s)"), // EXPLODE_PLAYER
            T(41, "obj", "Search area", "Objects in the zone are hidden until they are found."), // HIDE_OBJECTS_SEARCH_AREA
            T(42, "other", "Blank", "Does nothing."), // BLANK
            T(43, "npc", "Aggro NPCs", "NPCs in the zone turn hostile when a player comes in."), // AGGRO_PEDS_IN_THIS_ZONE
            T(44, "npc", "Block NPC paths", "NPCs do not walk through the zone."), // NAV_BLOCKING
            T(45, "player", "Health drain", "Players lose health in the zone. Value 1 is the amount (1 to 100).", "zt_value_drain", "Amount"), // HEALTH_DRAIN
            T(46, "player", "Metal detector", "Carrying weapons into the zone raises an alarm."), // METAL_DETECTOR
            T(47, "player", "Explosion danger area", "Danger area around explosions a player sets off."), // PLAYER_TRIGGERED_EXPLOSION_DANGER_AREA
            T(48, "air", "Air defence", "Air defence shoots at aircraft in the zone. Value 1: up to 1000000, value 2: up to 10000. At most 10 per job.", "zt_value_value", "Value 1", "zt_value_value2", "Value 2"), // AIR_DEFENCE
            T(49, "mission", "Objective text", "Shows a different objective text while a player is in the zone."), // OBJECTIVE_TEXT_OVERRIDE
            T(50, "air", "Altimeter", "Shows the altimeter in the zone."), // ALTIMETER_SYSTEM_ZONE
            T(51, "npc", "Roads off", "Switches the roads in the zone off, so no traffic drives into it."), // BLOCK_ROAD_NODES
            T(52, "mission", "Bounds", "Play area bounds; only in a special creator mode."), // BOUNDS
            T(53, "npc", "Spook NPCs", "NPCs in the zone get spooked."), // SPOOK_PED_IN_RADIUS
            T(54, "other", "Anti-zone", "Cancels another zone where both overlap."), // ANTI_ZONE
            T(55, "npc", "Leave vehicle", "NPCs and players get out of their vehicle in the zone."), // PED_PLAYER_LEAVE_VEHICLE
            T(56, "mission", "Blip zone", "Shows the zone on the map."), // BLIP_ZONE
            T(57, "veh", "Stay in vehicle", "NPCs and players cannot get out of their vehicle in the zone."), // PED_PLAYER_PREVENT_LEAVE_VEHICLE
            T(58, "other", "Sound zone", "Plays a sound in the zone."), // SOUND_ZONE
            T(59, "player", "Own spawn points", "Only the job's own spawn points inside the zone are used."), // EXCLUSIVE_CUSTOM_SPAWN_POINTS
            T(60, "npc", "NPCs flee", "NPCs flee from the zone."), // FLEE_PEDS
            T(61, "player", "No climbing", "Players cannot climb in the zone."), // BLOCK_CLIMBING
            T(62, "npc", "No gang chase spawns", "Gang chase units do not spawn in the zone."), // BLOCK_GANG_CHASE_SPAWN
            T(63, "mission", "Drop-off zone", "Drop-off point of a rule; may be very large."), // DROPOFF_ZONE
            T(64, "obj", "Door switch", "Switches the door settings while a player is in the zone."), // DOOR_CONFIG_SWITCH
            T(65, "player", "Wanted at once", "The wanted level comes at once, without the usual delay."), // SKIP_WANTED_DELAY
            T(66, "npc", "No dispatch", "Emergency services are not dispatched to the zone."), // BLOCK_DISPATCH
            T(67, "obj", "Clear entities", "Removes entities from the zone."), // CLEAR_ENTITY
            T(68, "mission", "Prerequisite zone", "Entering the zone completes a prerequisite."), // PREREQ_ZONE
            T(69, "obj", "Petrol pool", "Puts a pool of petrol in the zone."), // PETROL_POOL
            T(70, "player", "Walk style", "Changes how players walk in the zone. The style is value 3, which Xenvious does not show."), // CHANGE_WALK_STYLE
            T(71, "npc", "Gang chase flees", "Gang chase units flee from the zone."), // GANG_CHASE_FLEE
            T(72, "mission", "Lose lives", "Players lose lives in the zone. Value 1: 1 to 120, value 2: 0.5 to 30.", "zt_value_value", "Value 1", "zt_value_value2", "Value 2"), // LIVES_DEPLETION_ZONE
            T(73, "mission", "Aggro check", "Checks the aggro prerequisites while a player is in the zone."), // CHECK_AGGRO_PREREQS
            T(74, "mission", "Entity prerequisite zone", "Bringing the rule's entity into the zone completes a prerequisite."), // RULE_ENTITY_PREREQ_ZONE
            T(75, "player", "Keep wanted level", "Gives a wanted level and keeps it while in the zone. Value 1 is the stars (0 to 5).", "zt_value_stars", "Wanted stars"), // GIVE_AND_MAINTAIN_WANTED_LEVEL
            T(76, "player", "No ladders", "Players cannot climb ladders in the zone."), // BLOCK_LADDER_CLIMBING
            T(77, "air", "Remove parachute", "Takes the parachute away and blocks it in the zone."), // REMOVE_AND_BLOCK_PARACHUTE
            T(78, "npc", "Roads on", "Switches the roads in the zone on."), // ENABLE_ROAD_NODES
        };

        public static ZoneType Find(int id) => All.FirstOrDefault(t => t.Id == id);
    }
}
