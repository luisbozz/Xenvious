using System.Collections.Generic;

namespace Xenvious
{
    // Part of MainWindow: extra objectives (extra rules of mission entities) and their explanation.
    public partial class MainWindow
    {
        private void InitExtraRules()
        {
            VehExtraRules.Attach(ddvehno, ExtraObjectives.TypeVehicle);
            VehExtraRules.HelpRequested += (_, __) => ShowExtraRulesHelp();
            ActorExtraRules.Attach(ddactorno, ExtraObjectives.TypePed);
            ActorExtraRules.HelpRequested += (_, __) => ShowExtraRulesHelp();
        }

        /// <summary>How extra objectives work, with an example; shown from the card's "?".</summary>
        private async void ShowExtraRulesHelp()
        {
            await ConfirmAsync(
                TranslateOr("eo_help_title", "Extra rules"),
                TranslateOr("eo_help_text",
                    "Normally a ped, vehicle, object or go-to belongs to one rule of a team. Extra rules let the same entity take part in more rules of the team's rule list.\n\n" +
                    "Example: vehicle 3 has the rule 1 \"Go to\" for team 1. Add the extra rule 3 \"Collect\": in rule 3 team 1 has to take the same vehicle and bring it to the drop-off.\n\n" +
                    "Rule no. is the number of the rule in the team's list. The entity needs its own rule for the team, and the rule number has to exist. Up to 30 entities can have extra rules, each up to 13 per team.\n\n" +
                    "When you delete an entity in Xenvious, the extra rules of the following entities move along with them."),
                "OK", null);
        }
    }
}
