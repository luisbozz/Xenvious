using System.Collections.Generic;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// The rules one entity (ped, vehicle, object or go-to) takes part in for one team: its own rule
    /// (priority and type) and its extra objectives.
    ///
    /// In the Mission Creator both come from the creator's rule list: every rule has a selection
    /// (its type) and a bitset of the entities in it. func_1349 gives the entity the first of its
    /// rules as its own and the later ones as extra objectives (eoir = the rule's selection,
    /// eoep = the rule); func_5301 rebuilds every own rule from the list when a test starts. So
    /// Xenvious changes the bitset and repeats func_1349 here, and the result survives the test.
    /// Function numbers: public_mission_creator 1.73-3889 Legacy (docs/handoff/RULES.md).
    ///
    /// The other creators have no such list; there the priority and ExtraObjectives are written
    /// directly.
    /// </summary>
    public static class EntityRules
    {
        public sealed class Entry
        {
            public int Rule;          // 0-based rule index of the team
            public int Selection;     // creator selection (ciSELECTION_*), -1 when unknown
            public bool Main;         // the entity's own rule
            public int Slot = -1;     // extra objective slot and place, for removing
            public int Place = -1;
        }

        private static long PriOffset(int eoType)
        {
            switch (eoType)
            {
                case ExtraObjectives.TypePed: return GTA.Offsets.Editor.Actor.pri;
                case ExtraObjectives.TypeVehicle: return GTA.Offsets.Editor.Vehicle.pri;
                case ExtraObjectives.TypeObject: return GTA.Offsets.Editor.Objects.pri;
                case ExtraObjectives.TypeGoTo: return GTA.Offsets.Editor.Locations.pri;
                default: return 0;
            }
        }

        private static long TypeOffset(int eoType)
        {
            switch (eoType)
            {
                case ExtraObjectives.TypePed: return GTA.Offsets.Editor.Actor.rule;
                case ExtraObjectives.TypeVehicle: return GTA.Offsets.Editor.Vehicle.rule;
                case ExtraObjectives.TypeObject: return GTA.Offsets.Editor.Objects.rule;
                case ExtraObjectives.TypeGoTo: return GTA.Offsets.Editor.Locations.rule;
                default: return 0;
            }
        }

        private static long Next(int eoType)
        {
            switch (eoType)
            {
                case ExtraObjectives.TypePed: return GTA.Offsets.Editor.Actor.NEXT;
                case ExtraObjectives.TypeVehicle: return GTA.Offsets.Editor.Vehicle.NEXT;
                case ExtraObjectives.TypeObject: return GTA.Offsets.Editor.Objects.NEXT;
                case ExtraObjectives.TypeGoTo: return GTA.Offsets.Editor.Locations.NEXT;
                default: return 0;
            }
        }

        public static Rules.Kind KindOf(int eoType)
            => eoType == ExtraObjectives.TypePed ? Rules.Kind.Ped : eoType == ExtraObjectives.TypeVehicle ? Rules.Kind.Vehicle
                : eoType == ExtraObjectives.TypeObject ? Rules.Kind.Object : Rules.Kind.GoTo;

        /// <summary>True when the rules come from the Mission Creator's rule list.</summary>
        public static bool UsesRuleList => Rules.PublicCreator && GTA.Offsets.Editor.rulelist != 0 && GTA.Offsets.Editor.rulelist_NEXT != 0;

        // ----- the Mission Creator's rule list: Global_1837536.f_6[team /*104*/][rule /*6*/] -----

        private static long EntryAddr(int team, int rule) => GTA.Offsets.Editor.rulelist + team * GTA.Offsets.Editor.rulelist_NEXT + rule * GTA.Offsets.Editor.rulelist_rule_NEXT;

        /// <summary>The rule's creator selection (its type).</summary>
        public static int RuleSelection(int team, int rule) => new Global(EntryAddr(team, rule)).Get<int>();

        // .f_2 is an int[3] with its size slot in front: 96 entity bits.
        private static Global BitWord(int team, int rule, int index) => new Global(EntryAddr(team, rule) + 3 + index / 32);

        public static bool InRule(int team, int rule, int index) => index >= 0 && index < 96 && (BitWord(team, rule, index).Get<int>() & (1 << (index % 32))) != 0;

        private static void SetInRule(int team, int rule, int index, bool on)
        {
            var word = BitWord(team, rule, index);
            int v = word.Get<int>();
            word.SetInt(on ? v | (1 << (index % 32)) : v & ~(1 << (index % 32)));
        }

        /// <summary>
        /// Entity class of a creator selection (func_956): 1 ped, 2 vehicle, 3 object, 4 go-to,
        /// 5 and 6 player rules, 0 none. Only rules of the entity's own class can take it.
        /// </summary>
        public static int ClassOf(int selection)
        {
            switch (selection)
            {
                case 1: case 2: case 3: case 4: case 5: case 23: case 26: case 32: case 33: case 46: case 47: case 55: return ExtraObjectives.TypePed;
                case 6: case 7: case 8: case 9: case 10: case 24: case 27: case 48: case 50: return ExtraObjectives.TypeVehicle;
                case 11: case 12: case 13: case 14: case 15: case 25: case 28: case 30: case 49: case 51: return ExtraObjectives.TypeObject;
                case 16: case 17: case 29: case 40: case 41: return ExtraObjectives.TypeGoTo;
                case 31: return 6;
                case 0: return 0;
                default: return selection > 0 ? 5 : 0;
            }
        }

        /// <summary>The rule logic the entity gets for a selection (func_1356).</summary>
        public static int LogicOf(int selection)
        {
            switch (selection)
            {
                case 1: case 6: case 11: return 1;
                case 2: case 7: case 12: return 2;
                case 3: case 8: case 13: return 3;
                case 4: case 9: case 14: case 16: return 4;
                case 5: case 10: case 15: case 17: return 5;
                case 23: case 24: case 25: return 11;
                case 26: case 27: case 28: case 29: return 12;
                case 30: return 13;
                case 31: return 14;
                case 32: return 15;
                case 33: return 17;
                case 34: return 16;
                case 40: return 23;
                case 41: return 24;
                case 46: case 50: case 51: return 31;
                case 47: case 48: case 49: return 32;
                case 55: return 35;
                case 18: return 6; case 19: return 7; case 20: return 8; case 21: return 9; case 22: return 10;
                case 35: return 18; case 36: return 19; case 37: return 20; case 38: return 21; case 39: return 22;
                case 42: return 27; case 43: return 28; case 44: return 29; case 45: return 30;
                case 53: return 33; case 54: return 34; case 56: return 36;
                default: return 0;
            }
        }

        // ----- reading -----

        public static int OwnPriority(int eoType, int index, int team)
        {
            long pri = PriOffset(eoType);
            return pri == 0 || index < 0 ? -1 : new Global(pri + team + Next(eoType) * index).Get<int>();
        }

        public static int OwnLogic(int eoType, int index, int team)
        {
            long type = TypeOffset(eoType);
            return type == 0 || index < 0 ? 0 : new Global(type + team + Next(eoType) * index).Get<int>();
        }

        /// <summary>The entity's own rule and its extra objectives for the team, sorted by rule.</summary>
        public static List<Entry> Read(int eoType, int index, int team)
        {
            var list = new List<Entry>();
            if (!Rules.Ready || index < 0)
                return list;
            int pri = OwnPriority(eoType, index, team);
            if (pri >= 0 && pri < Rules.PriorityNone)
                list.Add(new Entry { Rule = pri, Main = true, Selection = Rules.Selection(KindOf(eoType), OwnLogic(eoType, index, team)) });
            int slot = ExtraObjectives.FindSlot(eoType, index);
            foreach (var extra in ExtraObjectives.Rules(slot, team))
                list.Add(new Entry { Rule = extra.Priority, Selection = extra.Type, Slot = slot, Place = extra.Index });
            return list.OrderBy(e => e.Rule).ThenBy(e => e.Main ? 0 : 1).ToList();
        }

        // ----- Mission Creator: join or leave a rule like its rules menu (func_1348 / func_1349) -----

        /// <summary>
        /// Puts the entity into the rule or takes it out, then sets its own rule and extra
        /// objectives from the rule list. Returns an error key or null.
        /// </summary>
        public static string SetMember(int eoType, int index, int team, int rule, bool on)
        {
            if (!UsesRuleList || index < 0 || index >= 96)
                return "er_err_list";
            if (ClassOf(RuleSelection(team, rule)) != eoType)
                return "er_err_class";
            bool was = InRule(team, rule, index);
            SetInRule(team, rule, index, on);
            string error = Sync(eoType, index, team);
            if (error != null)
                SetInRule(team, rule, index, was);
            return error;
        }

        /// <summary>
        /// Mission Creator: changes the type of a whole rule (its selection), then gives every
        /// entity in it the new type again, like the rule list does at the next test.
        /// </summary>
        public static string SetRuleType(int team, int rule, int selection)
        {
            if (!UsesRuleList)
                return "er_err_list";
            int cls = ClassOf(RuleSelection(team, rule));
            if (ClassOf(selection) != cls)
                return "er_err_class";
            new Global(EntryAddr(team, rule)).SetInt(selection);
            for (int i = 0; i < 96; i++)
                if (InRule(team, rule, i))
                {
                    string error = Sync(cls, i, team);
                    if (error != null)
                        return error;
                }
            return null;
        }

        /// <summary>Other creators: the entity's own rule type (the rule logic of the selection).</summary>
        public static void SetOwnType(int eoType, int index, int team, int selection)
        {
            long type = TypeOffset(eoType);
            if (type != 0 && index >= 0)
                new Global(type + team + Next(eoType) * index).SetInt(LogicOf(selection));
        }

        /// <summary>
        /// func_1349 for one team: the first rule of the entity's class that has its bit becomes
        /// its own rule, the later ones its extra objectives. A slot without extra objectives in
        /// every team is freed.
        /// </summary>
        public static string Sync(int eoType, int index, int team)
        {
            if (!UsesRuleList)
                return "er_err_list";
            int count = Rules.Count(team);
            var member = new List<int>();
            for (int r = 0; r < count; r++)
                if (ClassOf(RuleSelection(team, r)) == eoType && InRule(team, r, index))
                    member.Add(r);

            long pri = PriOffset(eoType), type = TypeOffset(eoType), next = Next(eoType);
            if (pri == 0 || type == 0)
                return "er_err_list";
            // Checked before anything is written, so a refused change leaves the job as it was.
            if (member.Count > ExtraObjectives.RulesPerEntity + 1)
                return "eo_err_rules";
            if (member.Count > 1 && ExtraObjectives.FindSlot(eoType, index) < 0 && ExtraObjectives.UsedSlots() >= ExtraObjectives.Slots)
                return "eo_err_slots";
            if (member.Count == 0)
            {
                new Global(pri + team + next * index).SetInt(Rules.PriorityNone);
                new Global(type + team + next * index).SetInt(0);
            }
            else
            {
                new Global(pri + team + next * index).SetInt(member[0]);
                new Global(type + team + next * index).SetInt(LogicOf(RuleSelection(team, member[0])));
            }

            int slot = ExtraObjectives.FindSlot(eoType, index);
            if (slot >= 0)
                foreach (var old in ExtraObjectives.Rules(slot, team))
                    ExtraObjectives.SetRule(slot, old.Index, team, -1, -1);
            foreach (int r in member.Skip(1))
            {
                string error = ExtraObjectives.Add(eoType, index, team, r, RuleSelection(team, r));
                if (error != null)
                    return error;
            }
            ExtraObjectives.FreeIfEmpty(eoType, index);
            return null;
        }
    }
}
