using System.Collections.Generic;

namespace Xenvious
{
    /// <summary>
    /// Extra objectives of a mission: up to 30 entities (ped, vehicle, object, go-to) that take part
    /// in more rules than their own one, up to 13 extra rules per team. Layout and logic from
    /// Rockstar's FMMC_header.sch (GIVE_EXTRA_OBJECTIVE_ENTITY_NEW_RULE,
    /// MOVE_EXTRA_OBJECTIVE_ENTITIES_DOWN_1). eoid / eoet are loaded at element 0; eoir / eoep are
    /// [30][13][4] arrays whose every level keeps its size slot in front.
    /// </summary>
    public static class ExtraObjectives
    {
        public const int Slots = 30;
        public const int RulesPerEntity = 13;
        public const int Teams = 4;

        // ciRULE_TYPE_*
        public const int TypePed = 1, TypeVehicle = 2, TypeObject = 3, TypeGoTo = 4;

        /// <summary>
        /// Rules the creator offers per entity type. eoir stores the creator's selection value
        /// (ciSELECTION_*, MP_globals_FM.sch); the controller turns it into the rule logic with
        /// GET_FMMC_RULE_FROM_CREATOR_RULE. Rockstar's labels (FMMC_RLM_*) are not in any dump,
        /// so the names are ours (key eo_r_&lt;value&gt;).
        /// </summary>
        public static (int Value, string Name)[] RuleTypesFor(int type)
        {
            switch (type)
            {
                case TypePed:
                    return new[] { (1, "Collect"), (2, "Kill"), (3, "Protect"), (4, "Go to"), (5, "Capture"), (23, "Collect and hold"),
                        (26, "Photograph"), (32, "Crowd control"), (33, "Charm"), (46, "Damage"), (47, "Leave"), (55, "Ped goes to location") };
                case TypeVehicle:
                    return new[] { (6, "Collect"), (7, "Destroy"), (8, "Protect"), (9, "Go to"), (10, "Capture"), (24, "Collect and hold"),
                        (27, "Photograph"), (48, "Leave"), (50, "Damage") };
                case TypeObject:
                    return new[] { (11, "Collect"), (12, "Destroy"), (13, "Protect"), (14, "Go to"), (15, "Capture"), (25, "Collect and hold"),
                        (28, "Photograph"), (30, "Hack"), (49, "Leave"), (51, "Damage") };
                case TypeGoTo:
                    return new[] { (16, "Go to"), (17, "Capture"), (29, "Photograph"), (41, "Leave"), (40, "Destroy") };
                default:
                    return new (int, string)[0];
            }
        }

        public sealed class Rule
        {
            public int Index;      // 0-12 inside the entity
            public int Type;       // ciSELECTION_*
            public int Priority;   // rule number in the team's rule list (0-based)
        }

        private const int InnerStride = 1 + Teams;                         // [4] with size slot
        private const int EntityStride = 1 + RulesPerEntity * InnerStride; // [13][4] with size slots

        private static long RuleAddr(long baseOffset, int slot, int n, int team)
            => baseOffset + slot * EntityStride + 1 + n * InnerStride + 1 + team;

        private static bool Ready => MainWindow.m != null && MainWindow.m.IsProcOpen
            && GTA.Offsets.Editor.eoid != 0 && GTA.Offsets.Editor.eoet != 0 && GTA.Offsets.Editor.eoir != 0 && GTA.Offsets.Editor.eoep != 0;

        public static int EntityId(int slot) => new Global(GTA.Offsets.Editor.eoid + slot).Get<int>();
        public static int EntityType(int slot) => new Global(GTA.Offsets.Editor.eoet + slot).Get<int>();

        /// <summary>Slot of the entity, -1 when it has no extra objectives.</summary>
        public static int FindSlot(int type, int index)
        {
            if (!Ready || index < 0)
                return -1;
            for (int s = 0; s < Slots; s++)
                if (EntityId(s) == index && EntityType(s) == type)
                    return s;
            return -1;
        }

        public static int UsedSlots()
        {
            if (!Ready)
                return 0;
            int used = 0;
            for (int s = 0; s < Slots; s++)
                if (EntityId(s) != -1)
                    used++;
            return used;
        }

        /// <summary>Number of rules in the team's list (nrl).</summary>
        public static int TeamRuleCount(int team)
            => Ready && GTA.Offsets.Editor.nrl != 0 ? new Global(GTA.Offsets.Editor.nrl + team * GTA.Offsets.Editor.team_NEXT).Get<int>() : 0;

        /// <summary>The entity's own rule number (0-based) for the team, -1 when unknown.</summary>
        public static int OwnPriority(int type, int index, int team)
        {
            if (!Ready || index < 0)
                return -1;
            long pri, next;
            switch (type)
            {
                case TypePed: pri = GTA.Offsets.Editor.Actor.pri; next = GTA.Offsets.Editor.Actor.NEXT; break;
                case TypeVehicle: pri = GTA.Offsets.Editor.Vehicle.pri; next = GTA.Offsets.Editor.Vehicle.NEXT; break;
                case TypeObject: pri = GTA.Offsets.Editor.Objects.pri; next = GTA.Offsets.Editor.Objects.NEXT; break;
                case TypeGoTo: pri = GTA.Offsets.Editor.Locations.pri; next = GTA.Offsets.Editor.Locations.NEXT; break;
                default: return -1;
            }
            return pri == 0 ? -1 : new Global(pri + team + next * index).Get<int>();
        }

        public static List<Rule> Rules(int slot, int team)
        {
            var list = new List<Rule>();
            if (!Ready || slot < 0)
                return list;
            for (int n = 0; n < RulesPerEntity; n++)
            {
                int type = new Global(RuleAddr(GTA.Offsets.Editor.eoir, slot, n, team)).Get<int>();
                if (type != -1)
                    list.Add(new Rule { Index = n, Type = type, Priority = new Global(RuleAddr(GTA.Offsets.Editor.eoep, slot, n, team)).Get<int>() });
            }
            return list;
        }

        /// <summary>
        /// Adds an extra rule like the creator does: the entity's slot or the first free one, then
        /// the first free rule place of the team. Returns an error key or null.
        /// </summary>
        public static string Add(int type, int index, int team, int priority, int rule)
        {
            if (!Ready || index < 0)
                return "eo_err_noentity";
            int slot = FindSlot(type, index);
            if (slot < 0)
            {
                for (int s = 0; s < Slots && slot < 0; s++)
                    if (EntityId(s) == -1)
                        slot = s;
                if (slot < 0)
                    return "eo_err_slots";
            }
            for (int n = 0; n < RulesPerEntity; n++)
            {
                var ruleAddr = new Global(RuleAddr(GTA.Offsets.Editor.eoir, slot, n, team));
                if (ruleAddr.Get<int>() != -1)
                    continue;
                ruleAddr.SetInt(rule);
                new Global(RuleAddr(GTA.Offsets.Editor.eoep, slot, n, team)).SetInt(priority);
                new Global(GTA.Offsets.Editor.eoet + slot).SetInt(type);
                new Global(GTA.Offsets.Editor.eoid + slot).SetInt(index);
                return null;
            }
            return "eo_err_rules";
        }

        public static void SetRule(int slot, int n, int team, int rule, int priority)
        {
            if (!Ready || slot < 0)
                return;
            new Global(RuleAddr(GTA.Offsets.Editor.eoir, slot, n, team)).SetInt(rule);
            new Global(RuleAddr(GTA.Offsets.Editor.eoep, slot, n, team)).SetInt(priority);
        }

        /// <summary>Removes one rule; a slot left without rules is freed.</summary>
        public static void Remove(int slot, int n, int team)
        {
            if (!Ready || slot < 0)
                return;
            SetRule(slot, n, team, -1, -1);
            for (int t = 0; t < Teams; t++)
                if (Rules(slot, t).Count > 0)
                    return;
            ClearSlot(slot);
        }

        private static void ClearSlot(int slot)
        {
            for (int n = 0; n < RulesPerEntity; n++)
                for (int t = 0; t < Teams; t++)
                    SetRule(slot, n, t, -1, -1);
            new Global(GTA.Offsets.Editor.eoid + slot).SetInt(-1);
            new Global(GTA.Offsets.Editor.eoet + slot).SetInt(-1);
        }

        /// <summary>
        /// Call after an entity of this type was deleted: its slot is freed and later entities of
        /// the same type move down by one, the same as the creator. Otherwise the extra rules of
        /// the following entities would point to their neighbour.
        /// </summary>
        public static void OnEntityDeleted(int type, int index)
        {
            if (!Ready || index < 0)
                return;
            for (int s = 0; s < Slots; s++)
            {
                if (EntityType(s) != type)
                    continue;
                int id = EntityId(s);
                if (id == index)
                    ClearSlot(s);
                else if (id > index)
                    new Global(GTA.Offsets.Editor.eoid + s).SetInt(id - 1);
            }
        }
    }
}
