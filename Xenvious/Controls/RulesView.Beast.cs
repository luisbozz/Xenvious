using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of RulesView: the team's beast abilities on the selected rule, as the Mission Creator's
    // team abilities menu sets them (public_mission_creator func_4997 / func_4999): per rule bits
    // in irbs5 (f_8504: 7 abilities on, 11 health, 19 damage, 13 jump, 12 speed, 14 stealth) and
    // f_8720 bit 27 (movement), the values in the team's beast struct per rule (f_5064[rule]).
    // All three controllers read them. Mission › Team Settings shows the same values.
    public partial class RulesView
    {
        private static readonly (string Label, string Fallback, int Bit, bool Move)[] BeastAbilities =
        {
            ("PMCT_BST_HELTH", "Health", 11, false),
            ("PMCT_BST_DMG", "Damage", 19, false),
            ("PMCT_BST_MOV", "Movement", 27, true),
            ("PMCT_BST_JMP", "Jump", 13, false),
            ("PMCT_BST_SPD", "Speed", 12, false),
            ("PMCT_BST_STELO", "Stealth", 14, false),
        };

        // Job options the older Team Settings page switches together with beast mode; Rockstar's
        // beast jobs have them on.
        private static readonly int[] BeastJobBits = { 2, 19, 23, 26 };

        private bool BeastAvailable => GTA.Offsets.Editor.irbs5 != 0 && GTA.Offsets.Editor.rl_boc != 0 && GTA.Offsets.Editor.team_NEXT != 0
            && GTA.Offsets.Editor.bmmxh != 0 && GTA.Offsets.Editor.beast_next != 0;

        private long BeastBits(int rule, bool move) => (move ? GTA.Offsets.Editor.rl_boc : GTA.Offsets.Editor.irbs5) + _team * GTA.Offsets.Editor.team_NEXT + rule;
        private long BeastValue(long field, int rule) => field + _team * GTA.Offsets.Editor.team_NEXT + rule * GTA.Offsets.Editor.beast_next;

        private bool BeastBit(int rule, int bit, bool move) => Functions.Read.checkbinary(bit, BeastBits(rule, move));

        private string BeastSummary(Rules.Rule rule)
        {
            if (!BeastBit(rule.Index, 7, false))
                return T("rl_beast_off", "off");
            int n = BeastAbilities.Count(a => BeastBit(rule.Index, a.Bit, a.Move));
            return string.Format(CultureInfo.CurrentCulture, T("rl_beast_on", "on · {0} abilities"), n);
        }

        private FrameworkElement BeastBody(Rules.Rule rule)
        {
            var panel = new StackPanel();
            int r = rule.Index;
            OptionRow(panel, RuleOptions.GameText("PMCT_AB_BSTS_ON", "Beast Abilities"),
                Toggle(BeastBit(r, 7, false), on => Functions.Write.writebinary(7, BeastBits(r, false), on)), RuleOptions.Help("AB_BSTS_ON"));
            if (!BeastBit(r, 7, false))
            {
                panel.Children.Add(Say(T("rl_beast_hint", "The team becomes the beast on this rule: more health, damage, jump and speed, as in Rockstar's Hunting Pack and Beast jobs.")));
                return panel;
            }
            foreach (var a in BeastAbilities)
            {
                var ability = a;
                OptionRow(panel, RuleOptions.GameText(ability.Label, ability.Fallback),
                    Toggle(BeastBit(r, ability.Bit, ability.Move), on => Functions.Write.writebinary(ability.Bit, BeastBits(r, ability.Move), on)),
                    RuleOptions.Help(ability.Label.Substring(5)));
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
