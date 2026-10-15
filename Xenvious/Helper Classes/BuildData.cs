using System;
using System.Collections.Generic;

namespace Xenvious
{
    /// <summary>
    /// The build data of each game edition: offsets.ini, scrpatches.json and prop_lists.json of
    /// OfflineData/&lt;edition&gt;, compiled in by tools/builddata/gen.py (Generated/*.g.cs).
    ///
    /// Nothing is parsed at start: the generator resolved every path already, with the
    /// decompiler's meaning (each [..] counts the array's size slot, see GlobalPath). An edition's
    /// data is only built when it is first asked for, and never stands in for the other one:
    /// Legacy addresses in an Enhanced process would point into unrelated memory.
    /// </summary>
    public sealed partial class BuildData
    {
        private static readonly Lazy<BuildData> legacy = new Lazy<BuildData>(CreateLegacy);
        private static readonly Lazy<BuildData> enhanced = new Lazy<BuildData>(CreateEnhanced);

        public static BuildData Legacy => legacy.Value;
        public static BuildData Enhanced => enhanced.Value;

        /// <summary>The data of the edition <see cref="GameVariant.Current"/> names.</summary>
        public static BuildData Current => For(GameVariant.Current);

        public static BuildData For(GameEdition edition) =>
            edition == GameEdition.Enhanced ? Enhanced : Legacy;

        /// <summary>The script patches of the current edition, a new list on every call.</summary>
        public static List<GTA.ScrPatches> CurrentScrPatches() =>
            GameVariant.IsEnhanced ? ScrPatchesEnhanced() : ScrPatchesLegacy();

        /// <summary>The developer script patches of the current edition, a new list on every call.</summary>
        public static List<GTA.ScrPatches> CurrentScrPatchesDev() =>
            GameVariant.IsEnhanced ? ScrPatchesDevEnhanced() : ScrPatchesDevLegacy();

        private static readonly Lazy<RuleFields> ruleFieldsLegacy = new Lazy<RuleFields>(RuleFieldsLegacy);
        private static readonly Lazy<RuleFields> ruleFieldsEnhanced = new Lazy<RuleFields>(RuleFieldsEnhanced);

        private static readonly Lazy<Dictionary<string, (int Bit, bool OnWhenClear)>> ruleOptionsLegacy =
            new Lazy<Dictionary<string, (int Bit, bool OnWhenClear)>>(RuleOptionsLegacy);
        private static readonly Lazy<Dictionary<string, (int Bit, bool OnWhenClear)>> ruleOptionsEnhanced =
            new Lazy<Dictionary<string, (int Bit, bool OnWhenClear)>>(RuleOptionsEnhanced);

        /// <summary>Bit numbers of the Rules page's yes/no options in the current edition (rule_options.json, RuleOptions).</summary>
        public static Dictionary<string, (int Bit, bool OnWhenClear)> CurrentRuleOptions() =>
            GameVariant.IsEnhanced ? ruleOptionsEnhanced.Value : ruleOptionsLegacy.Value;

        /// <summary>What moves with a rule in the current edition (rule_fields.json, RuleMover).</summary>
        public static RuleFields CurrentRuleFields() =>
            GameVariant.IsEnhanced ? ruleFieldsEnhanced.Value : ruleFieldsLegacy.Value;

        /// <summary>The model hash lists of the current edition (prop_lists.json), a new object on every call.</summary>
        public static PropLists CurrentPropLists() =>
            GameVariant.IsEnhanced ? PropListsEnhanced() : PropListsLegacy();
    }
}
