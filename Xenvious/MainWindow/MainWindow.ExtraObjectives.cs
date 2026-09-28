using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xenvious
{
    // Part of MainWindow: extra objectives (extra rules of mission entities) and their explanation.
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

        /// <summary>Opens the rules page at a team's rule.</summary>
        public void OpenRules(int team, int rule)
        {
            OpenRules();
            rulesView.ShowRule(team, rule);
        }

        /// <summary>Opens the overview: the explanation and all 30 slots.</summary>
        private void OpenExtraObjectives()
        {
            BtnSectionMission_Click(null, null);
            PageInnerMission.SelectedItem = PageInnerMissionExtra;
            RefreshExtraOverview();
        }

        private static readonly string[] TeamColors = { "#FF5865F2", "#FF43B581", "#FFFAA61A", "#FFEB459E" };

        private void RefreshExtraOverview()
        {
            ExtraOverviewList.Children.Clear();
            ExtraOverviewCount.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("eo_slots", "{0} of 30 used"), ExtraObjectives.UsedSlots());
            string[] kinds = { "", TranslateOr("eo_k_ped", "Actor"), TranslateOr("eo_k_veh", "Vehicle"), TranslateOr("eo_k_obj", "Object"), TranslateOr("eo_k_goto", "Go-to") };
            bool any = false;
            for (int slot = 0; slot < ExtraObjectives.Slots; slot++)
            {
                int id = ExtraObjectives.EntityId(slot);
                int type = ExtraObjectives.EntityType(slot);
                if (id < 0)
                    continue;
                any = true;
                var row = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
                row.Children.Add(new TextBlock { Width = 150, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center,
                    Text = $"{slot + 1}.  {(type > 0 && type < kinds.Length ? kinds[type] : type.ToString(CultureInfo.InvariantCulture))} {id + 1}" });
                for (int team = 0; team < ExtraObjectives.Teams; team++)
                    foreach (var rule in ExtraObjectives.Rules(slot, team))
                    {
                        string name = System.Linq.Enumerable.FirstOrDefault(ExtraObjectives.RuleTypesFor(type), r => r.Value == rule.Type).Name ?? rule.Type.ToString(CultureInfo.InvariantCulture);
                        var chip = new Border { CornerRadius = new CornerRadius(10), Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(0, 0, 6, 4),
                            Background = (Brush)FindResource("SeactionHeaderBackgroundBrush") };
                        var text = new TextBlock { FontSize = 12.5 };
                        text.Inlines.Add(new System.Windows.Documents.Run($"T{team + 1} · {rule.Priority + 1} ") { FontWeight = FontWeights.Bold,
                            Foreground = (Brush)new BrushConverter().ConvertFromString(TeamColors[team]) });
                        text.Inlines.Add(new System.Windows.Documents.Run(TranslateOr("eo_r_" + rule.Type, name)));
                        chip.Child = text;
                        row.Children.Add(chip);
                    }
                ExtraOverviewList.Children.Add(row);
            }
            if (!any)
                ExtraOverviewList.Children.Add(new TextBlock { Text = TranslateOr("eo_empty", "No entity has extra rules yet. Add them on the page of an actor, vehicle, object or go-to."),
                    TextWrapping = TextWrapping.Wrap, Foreground = (Brush)FindResource("NavMutedBrush") });
        }
    }
}
