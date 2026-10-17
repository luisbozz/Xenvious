using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of RulesView: the team's beast abilities on the selected rule, as the Mission Creator's
    // team abilities menu sets them (public_mission_creator func_4997 / func_4999): per rule bits
    // in irbs5 (f_8504) and f_8720, numbers from rule_options.json (1.73: 7 active, 11 health,
    // 19 damage, 27 movement in f_8720, 13 jump, 12 speed, 14 invisibility), the values in the
    // team's beast struct per rule (f_5064[rule]).
    // All three controllers read them. Mission › Team Settings shows the same values.
    public partial class RulesView
    {
        // Label (rule_options.json, read from the Mission Creator by ysc-global-updater), fallback,
        // and whether the bit is in f_8720 (rl_boc) instead of irbs5.
        private static readonly (string Label, string Fallback, bool Move)[] BeastAbilities =
        {
            ("BST_HELTH", "Increased Health", false),
            ("BST_DMG", "Increased Damage", false),
            ("BST_MOV", "Beast Movement", true),
            ("BST_JMP", "Super Jump", false),
            ("BST_SPD", "Increased Speed", false),
            ("BST_STELO", "Invisibility", false),
        };

        private static int BeastBitOf(string label) => BuildData.CurrentRuleOptions().TryGetValue(label, out var b) ? b.Bit : -1;

        // Job options the older Team Settings page switches together with beast mode; Rockstar's
        // beast jobs have them on.
        private static readonly int[] BeastJobBits = { 2, 19, 23, 26 };

        private bool BeastAvailable => BeastBitOf("AB_BSTS_ON") >= 0 && GTA.Offsets.Editor.irbs5 != 0 && GTA.Offsets.Editor.rl_boc != 0 && GTA.Offsets.Editor.team_NEXT != 0
            && GTA.Offsets.Editor.bmmxh != 0 && GTA.Offsets.Editor.beast_next != 0;

        private long BeastBits(int rule, bool move) => (move ? GTA.Offsets.Editor.rl_boc : GTA.Offsets.Editor.irbs5) + _team * GTA.Offsets.Editor.team_NEXT + rule;
        private long BeastValue(long field, int rule) => field + _team * GTA.Offsets.Editor.team_NEXT + rule * GTA.Offsets.Editor.beast_next;

        private bool BeastBit(int rule, int bit, bool move) => bit >= 0 && Functions.Read.checkbinary(bit, BeastBits(rule, move));
        private bool BeastOn(int rule) => BeastBit(rule, BeastBitOf("AB_BSTS_ON"), false);

        private string BeastSummary(Rules.Rule rule)
        {
            if (!BeastOn(rule.Index))
                return T("rl_beast_off", "off");
            int n = BeastAbilities.Count(a => BeastBit(rule.Index, BeastBitOf(a.Label), a.Move));
            return string.Format(CultureInfo.CurrentCulture, T("rl_beast_on", "on · {0} abilities"), n);
        }

        private FrameworkElement BeastBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            int r = rule.Index;
            int active = BeastBitOf("AB_BSTS_ON");
            OptionRow(panel, RuleOptions.GameText("PMCT_AB_BSTS_ON", "Active Objectives"),
                Toggle(BeastOn(r), on => Functions.Write.writebinary(active, BeastBits(r, false), on)), RuleOptions.Help("AB_BSTS_ON"));
            if (!BeastOn(r))
            {
                panel.Children.Add(Say(T("rl_beast_hint", "The team becomes the beast on this rule: more health, damage, jump and speed, as in Rockstar's Hunting Pack and Beast jobs.")));
                return panel;
            }
            foreach (var a in BeastAbilities)
            {
                var ability = a;
                int bit = BeastBitOf(ability.Label);
                if (bit < 0)
                    continue;
                OptionRow(panel, RuleOptions.GameText("PMCT_" + ability.Label, ability.Fallback),
                    Toggle(BeastBit(r, bit, ability.Move), on => Functions.Write.writebinary(bit, BeastBits(r, ability.Move), on)),
                    RuleOptions.Help(ability.Label));
            }
            OptionRow(panel, T("beastmodehealth", "Health"),
                NumberBox(new Global(BeastValue(GTA.Offsets.Editor.bmmxh, r)).Get<int>(), null, v => new Global(BeastValue(GTA.Offsets.Editor.bmmxh, r)).SetInt(v)), null);
            if (GTA.Offsets.Editor.bmspm != 0)
                OptionRow(panel, RuleOptions.GameText("PMCT_BST_SPDVAR", T("beastmoderunspd", "Run Speed")) + " (%)",
                    NumberBox((int)Math.Round(new Global(BeastValue(GTA.Offsets.Editor.bmspm, r)).Get<float>() * 100), "%",
                        v => new Global(BeastValue(GTA.Offsets.Editor.bmspm, r)).SetFloat(v / 100f)), RuleOptions.Help("BST_SPDVAR"));
            if (GTA.Offsets.Editor.bmsjd != 0)
                OptionRow(panel, RuleOptions.GameText("PMCT_BST_DJMP", T("beastmodejmpdmg", "Jump Damage")),
                    NumberBox(new Global(BeastValue(GTA.Offsets.Editor.bmsjd, r)).Get<int>(), "%", v => new Global(BeastValue(GTA.Offsets.Editor.bmsjd, r)).SetInt(v)), RuleOptions.Help("BST_DJMP"));
            if (GTA.Offsets.Editor.bmstd != 0)
                OptionRow(panel, T("beastmodeinvdmg", "Invisibility Damage"),
                    NumberBox(new Global(BeastValue(GTA.Offsets.Editor.bmstd, r)).Get<int>(), null, v => new Global(BeastValue(GTA.Offsets.Editor.bmstd, r)).SetInt(v)), null);
            if (GTA.Offsets.Editor.bmmdm != 0)
                OptionRow(panel, T("beastmodemeeledmg", "Melee Damage"),
                    NumberBox(new Global(BeastValue(GTA.Offsets.Editor.bmmdm, r)).Get<int>(), null, v => new Global(BeastValue(GTA.Offsets.Editor.bmmdm, r)).SetInt(v)), null);
            if (GTA.Offsets.Editor.menubs5 != 0)
            {
                bool job = BeastJobBits.All(b => Functions.Read.checkbinary(b, GTA.Offsets.Editor.menubs5));
                OptionRow(panel, T("rl_beast_job", "Job options of Rockstar's beast jobs"),
                    Toggle(job, on => { foreach (int b in BeastJobBits) Functions.Write.writebinary(b, GTA.Offsets.Editor.menubs5, on); }),
                    T("rl_beast_job_help", "menubs5 bits 2, 19, 23 and 26, for the whole job. The older Team Settings page switches them with beast mode."));
            }
            return panel;
        }
    }
}
