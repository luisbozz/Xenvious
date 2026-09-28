using System;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: the rules cards of the entity pages, and opening the rules page or an entity from them.
    public partial class MainWindow
    {
        private void InitExtraRules()
        {
            // "?" on a rules card: the explanation is on the rules page, at the entity's own rule.
            void Help(EntityRulesCard card) => card.HelpRequested += (_, __) => OpenRules(card.Team, Math.Max(0, card.MainRule));
            VehExtraRules.Attach(ddvehno, ExtraObjectives.TypeVehicle);
            Help(VehExtraRules);
            InitVehicleLayout();
            ActorExtraRules.Attach(ddactorno, ExtraObjectives.TypePed);
            Help(ActorExtraRules);
            ObjExtraRules.Attach(ddobjno, ExtraObjectives.TypeObject);
            Help(ObjExtraRules);
            GotoExtraRules.Attach(ddgotono, ExtraObjectives.TypeGoTo);
            Help(GotoExtraRules);
        }

        /// <summary>Opens the rules page (Controls/RulesView.cs).</summary>
        private void OpenRules()
        {
            BtnSectionMission_Click(null, null);
            PageInnerMission.SelectedItem = PageInnerMissionRules;
        }

        /// <summary>Opens the page of an entity (ExtraObjectives.Type*) with it selected.</summary>
        public void OpenEntity(int type, int index)
        {
            ComboBox list;
            switch (type)
            {
                case ExtraObjectives.TypePed: BtnSectionActor_Click(null, null); list = ddactorno; break;
                case ExtraObjectives.TypeVehicle: BtnSectionVeh_Click(null, null); list = ddvehno; break;
                case ExtraObjectives.TypeObject: BtnSectionObj_Click(null, null); ClickButton(BtnCaptureObjects); list = ddobjno; break;
                case ExtraObjectives.TypeGoTo: BtnSectionMission_Click(null, null); ClickButton(BtnSectionGoto); list = ddgotono; break;
                default: return;
            }
            if (index >= 0 && index < list.Items.Count)
                list.SelectedIndex = index;
        }

        /// <summary>Opens the rules page at a team's rule.</summary>
        public void OpenRules(int team, int rule)
        {
            OpenRules();
            rulesView.ShowRule(team, rule);
        }
    }
}
