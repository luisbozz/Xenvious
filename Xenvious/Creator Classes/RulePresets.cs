using System.Collections.Generic;

namespace Xenvious
{
    /// <summary>
    /// A new rule at the end of a team's rules, filled like the Mission Creator fills one.
    ///
    /// The creator (public_mission_creator func_1369 Legacy / func_1372 Enhanced) writes the
    /// rule's selection into its rule list, sets a few values by type and calls func_1373 /
    /// func_1376 for about 40 defaults (bitsets, limits, both play areas' minimap alpha 60, ...);
    /// deleting a rule resets the freed slot with the same function. The per-team field numbers are
    /// the same in both editions; only the rule list global and the two team bitsets differ, which
    /// offsets.ini (OFFSET_rp_*) takes care of.
    ///
    /// Mission Creator: the rule goes into the rule list and its count (nrl follows at the next test).
    /// LTS and Capture have no rule list for this: their priorities are written directly and the
    /// count is nrl itself (kept by the "nrl fix" patch, else the creator sets it back to 1).
    /// </summary>
    public static class RulePresets
    {
        public static readonly string[] RuleArrayKeys =
        {
            "f1083", "f6203", "f6221", "f6239", "f19261", "f6275", "f6293", "f8648", "f2349", "f7342", "f1845", "f8432", "f8468", "f8504",
            "f8684", "f6329", "f6311", "f19063", "f7166", "f8450", "f8486", "f8558", "f7324", "f7306", "f6914", "f8594", "f7004", "f7022",
            "f6968", "f8612", "f7112", "f6720", "f8522", "f7679", "f2389",
        };

        public static readonly string[] TeamKeys = { "f6349", "f6348", "f6347", "teambits1", "teambits2" };

        private static Dictionary<string, long> O => GTA.Offsets.Editor.RulePreset;

        /// <summary>All preset offsets are known for this edition.</summary>
        public static bool Ready
        {
            get
            {
                foreach (string key in RuleArrayKeys) if (!O.TryGetValue(key, out long v) || v == 0) return false;
                foreach (string key in TeamKeys) if (!O.TryGetValue(key, out long v) || v == 0) return false;
                return O.TryGetValue("f2389_NEXT", out long n) && n > 0 && O.TryGetValue("f2389_sub_NEXT", out long m) && m > 0
                    && GTA.Offsets.Editor.team_NEXT != 0 && GTA.Offsets.Editor.nrl != 0;
            }
        }

        private static long Team(int team) => team * GTA.Offsets.Editor.team_NEXT;
        private static Global At(string key, int team, int rule) => new Global(O[key] + Team(team) + rule);

        private static void Int(string key, int team, int rule, int value) => At(key, team, rule).SetInt(value);
        private static void Float(string key, int team, int rule, float value) => At(key, team, rule).SetFloat(value);

        private static void Bit(Global g, int bit, bool on)
        {
            int v = g.Get<int>();
            g.SetInt(on ? v | (1 << bit) : v & ~(1 << bit));
        }

        // f_2389[rule /*45*/][k /*22*/]: an array of two 22-slot structs, with its own size slot.
        private static Global Sub(int team, int rule, int k, int field)
            => new Global(O["f2389"] + Team(team) + rule * O["f2389_NEXT"] + 1 + k * O["f2389_sub_NEXT"] + field);

        // func_1375 / func_1378
        private static int F1083(int selection) => selection == 3 || selection == 8 || selection == 13 || selection == 56 ? 7 : 0;
        // func_1330 / func_1333
        private static int F6203(int selection) => selection == 3 || selection == 8 || selection == 13 || selection == 34 || selection == 56 ? 0 : 1;
        // func_1374 / func_1377
        private static int F6275(int selection) => selection == 5 || selection == 10 || selection == 15 || selection == 17 ? 60000 : 0;

        /// <summary>Writes the creator's values for a new entity rule (not a player rule) into rule slot <paramref name="rule"/>.</summary>
        public static void Apply(int team, int rule, int selection)
        {
            // func_1369, entity branch.
            Int("f1083", team, rule, F1083(selection));
            Int("f6203", team, rule, F6203(selection));
            Int("f6221", team, rule, 0);
            Int("f6239", team, rule, 1);
            Int("f19261", team, rule, -1);
            Int("f6275", team, rule, F6275(selection));
            Int("f6293", team, rule, 0);
            Bit(new Global(O["teambits1"] + team), rule, false);
            Bit(new Global(O["teambits2"] + team), rule, false);

            // func_1373, in its order.
            Bit(At("f8648", team, rule), 18, true);
            Int("f2349", team, rule, 0);
            Int("f7342", team, rule, 7);
            Int("f1845", team, rule, -1);
            Bit(At("f8432", team, rule), 13, true);
            Bit(At("f8432", team, rule), 18, true);
            Bit(At("f8468", team, rule), 19, true);
            Bit(At("f8504", team, rule), 4, true);
            Bit(At("f8684", team, rule), 3, true);
            Int("f6329", team, rule, 0);
            Int("f6311", team, rule, 0);
            Bit(new Global(O["f6349"] + Team(team)), rule, false);
            Bit(new Global(O["f6348"] + Team(team)), rule, false);
            Bit(At("f8450", team, rule), 20, false);
            Bit(new Global(O["f6347"] + Team(team)), rule, false);
            Bit(At("f8432", team, rule), 4, false);
            Bit(At("f8432", team, rule), 12, false);
            Int("f19063", team, rule, 0);
            Int("f7166", team, rule, 0);
            for (int k = 0; k <= 1; k++)
            {
                Sub(team, rule, k, 0).SetInt(0);
                Bit(Sub(team, rule, k, 10), 13, false);
                Bit(Sub(team, rule, k, 10), 12, true);
                Sub(team, rule, k, 15).SetInt(0);
            }
            Bit(At("f8450", team, rule), 11, true);
            Bit(At("f8486", team, rule), 16, true);
            Bit(At("f8558", team, rule), 14, true);
            Bit(At("f8468", team, rule), 11, true);
            if (PlayAreas.Ready)
            {
                new Global(PlayAreas.Field(1, team, rule, PlayAreas.MinimapAlpha)).SetInt(60);
                new Global(PlayAreas.Field(2, team, rule, PlayAreas.MinimapAlpha)).SetInt(60);
            }
            Bit(Sub(team, rule, 0, 10), 12, true);
            Bit(Sub(team, rule, 0, 10), 11, false);
            Sub(team, rule, 0, 5).SetInt(5000);
            Float("f7324", team, rule, 5f);
            Float("f7306", team, rule, -1f);
            Int("f6914", team, rule, -1);
            Bit(At("f8594", team, rule), 22, true);
            Int("f7004", team, rule, 50);
            Int("f7022", team, rule, 0);
            Int("f6968", team, rule, 1);
            Bit(At("f8612", team, rule), 8, true);
            Bit(At("f8504", team, rule), 1, true);
            Float("f7112", team, rule, 300f);
            Int("f6720", team, rule, 3);
            Bit(At("f8522", team, rule), 28, false);
            Int("f7679", team, rule, -2);
        }

        /// <summary>
        /// Adds a rule of <paramref name="selection"/> at the end of the team's rules. Returns the new
        /// rule's index, or -1 with an error key.
        /// </summary>
        public static int AddRule(int team, int selection, out string error)
        {
            error = null;
            if (!Ready || !Rules.Ready)
            {
                error = "er_err_presets";
                return -1;
            }
            int rule = Rules.Count(team);
            if (rule >= Rules.MaxRules)
            {
                error = "er_err_full";
                return -1;
            }
            if (EntityRules.UsesRuleList)
            {
                // The creator's rule list: the selection, no entities yet, then the count.
                EntityRules.InitListEntry(team, rule, selection);
                Apply(team, rule, selection);
                new Global(GTA.Offsets.Editor.rulelist_count + team * GTA.Offsets.Editor.rulelist_NEXT).SetInt(rule + 1);
            }
            else
            {
                Apply(team, rule, selection);
                new Global(GTA.Offsets.Editor.nrl + Team(team)).SetInt(rule + 1);
            }
            return rule;
        }
    }
}
