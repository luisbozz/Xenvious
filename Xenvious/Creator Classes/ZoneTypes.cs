using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// Zone types (zntp) with a name and what they do. Taken from fm_mission_controller: the
    /// switch over the zone type that runs while a player is in a zone, and the one that
    /// cleans up. Types 0-4 and 7 carry Rockstar's own names (FMMC_ZN_TY0..7); the names of
    /// the later DLC types are not in any public label dump, so those are described by what
    /// the controller does. Types not listed here can still be typed in as a number.
    /// </summary>
    public static class ZoneTypes
    {
        /// <summary>Which of the two zone values a type reads, so the page can name the field.</summary>
        public enum Value { None, Znwd, Znwvd }

        public sealed class ZoneType
        {
            public int Id;
            public string Name;
            public string Description;
            public Value UsesValue;
            public string ValueKey;
            public string ValueFallback;
        }

        private static ZoneType T(int id, string name, string description, Value value = Value.None, string valueKey = null, string valueFallback = null)
            => new ZoneType { Id = id, Name = name, Description = description, UsesValue = value, ValueKey = valueKey, ValueFallback = valueFallback };

        // Names and descriptions are English fallbacks; the page shows zt_name_<id> and zt_desc_<id>.
        public static readonly IReadOnlyList<ZoneType> All = new[]
        {
            T(0, "Peds and vehicles", "No pedestrians and no traffic in the zone."),
            T(1, "Peds", "No pedestrians in the zone."),
            T(2, "Vehicles", "No traffic in the zone."),
            T(3, "Contact mission area", "Rockstar's name for this type; the controller does nothing of its own with it."),
            T(4, "Player vehicle", "Rockstar's name for this type; the controller does nothing of its own with it."),
            T(5, "No cover", "Players and NPCs cannot take cover in the zone."),
            T(6, "Rocket fire", "Vehicles in the zone are shot at with rockets."),
            T(7, "Block GPS", "The GPS plans no route through the zone."),
            T(9, "Waves", "Sets the height of the ocean waves and calms the water in the zone. Value 2 is the strength, 1 is normal.", Value.Znwvd, "zt_value_waves", "Wave strength"),
            T(12, "No emergency services", "Police, army and other emergency services do not spawn in the zone."),
            T(14, "No wanted level", "Clears the wanted level and keeps it at zero while in the zone."),
            T(19, "Open parachute", "Opens the parachute in free fall; vehicles with a parachute and the Oppressor start to glide."),
            T(20, "Air defence", "Air defence shoots at aircraft in the zone."),
            // SET_AIR_DRAG_MULTIPLIER_FOR_PLAYERS_VEHICLE with znwd, reset to 1 when leaving.
            T(23, "Air drag", "Changes the air drag of the player's vehicle. Value 1 is the factor, 1 is normal, higher slows down more.", Value.Znwd, "zt_value_drag", "Drag factor"),
            T(26, "Oppressor boost", "Refills the rocket boost of the Oppressor."),
            T(29, "Roads off", "Switches the roads in the zone off, so no traffic drives into it."),
            // SET_VEHICLE_FORWARD_SPEED lowers the speed by a share taken from znwvd.
            T(33, "Slow down", "Slows vehicles down in the zone. Value 2 is the strength.", Value.Znwvd, "zt_value_brake", "Brake strength"),
            // Only sets a flag in the controller; the effect and the znwd use are from the old field help and players.
            T(37, "Repair", "Repairs the vehicle while it is in the zone. Value 1 is the repair speed.", Value.Znwd, "zt_value_repair", "Repair speed"),
            T(39, "Reset objects", "Objects that get into the zone jump back to their start position."),
            // Runs a timer of znwd seconds while the player is inside, then fires its effect.
            T(40, "Explosion", "Explodes after the set time while a player is inside. Value 1 is the time in seconds.", Value.Znwd, "zt_value_time", "Time (s)"),
            T(41, "Hide objects", "Objects in the zone turn transparent and their blips fade out."),
            T(44, "Block NPC paths", "NPCs do not walk through the zone."),
        };

        public static ZoneType Find(int id) => All.FirstOrDefault(t => t.Id == id);
    }
}
