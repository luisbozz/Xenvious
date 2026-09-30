using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xenvious
{
    // Part of MainWindow: Actors page layout (mockup https://claude.ai/artifact/VifG8TGRs71syLT1EsWBmP):
    // who (model, position, combat, advanced groups), goto route, when (lifecycle, rules).
    public partial class MainWindow
    {
        private GotoRouteView _actorRoute;

        /// <summary>
        /// Rearranges the existing actor cards into three columns. The page's controls and their
        /// handlers stay; only their place changes. Runs after InitCatalogPages (placement card
        /// already under the model card).
        /// </summary>
        private void InitActorLayout()
        {
            var dashCard = (Style)FindResource("DashCard");
            Border CardOf(FrameworkElement c)
            {
                while (c != null && !(c is Border b && b.Style == dashCard))
                    c = c.Parent as FrameworkElement;
                return (Border)c;
            }
            Panel BodyOf(Border card) => ((DockPanel)card.Child).Children.OfType<Panel>().Last();
            // The row a field sits in: past the grid cells around it.
            FrameworkElement RowOf(FrameworkElement c)
            {
                while (c.Parent is Grid || c.Parent is StackPanel cell && cell.Children.Count <= 2 && cell.Parent is Grid)
                    c = (FrameworkElement)c.Parent;
                return c;
            }
            T Detach<T>(T e) where T : FrameworkElement
            {
                if (e.Parent is Panel p) p.Children.Remove(e);
                else if (e.Parent is ContentControl cc) cc.Content = null;
                else if (e.Parent is Decorator d) d.Child = null;
                return e;
            }

            FrameworkElement start = ActorModelCard;
            while (start != null && !(start is MasonryPanel))
                start = start.Parent as FrameworkElement;
            if (!(start is MasonryPanel masonry) || !(ActorModelCard.Parent is StackPanel who))
                return;
            var allCards = masonry.Children.OfType<Panel>().SelectMany(p => p.Children.OfType<Border>()).Where(b => b.Style == dashCard).ToList();

            var combat = CardOf(ddActorweap);
            var behaviour = CardOf(cbactorstationary);
            var proofs = CardOf(cbactorbulletproof);
            var respawn = CardOf(ddActorrsp);
            var gotoCard = CardOf(ddActorgoto);
            var oldRule = CardOf(ddactorspawnon);
            var placement = CardOf(tbactorlocx);

            // Advanced groups: relations and vehicle come out of the combat card, the rest are
            // whole card bodies.
            var combatBody = BodyOf(combat);
            var carLabelAt = combatBody.Children.IndexOf(ddActorcar) - 1;
            var vehicle = new StackPanel();
            var relations = new StackPanel();
            var moved = combatBody.Children.Cast<UIElement>().Skip(carLabelAt).ToList();
            foreach (FrameworkElement e in moved)
            {
                Detach(e);
                if (e == ddActorcar || moved.IndexOf(e) == 0)
                    vehicle.Children.Add(e);
                else if (!(e is System.Windows.Shapes.Rectangle))
                    relations.Children.Add(e);
            }
            var death = new StackPanel();
            foreach (var field in new FrameworkElement[] { tbactorpcash, tbactorblipsize })
            {
                var row = RowOf(field);
                if (row.Parent != death)
                    death.Children.Add(Detach(row));
            }
            foreach (FrameworkElement e in BodyOf(proofs).Children.Cast<FrameworkElement>().ToList())
                death.Children.Add(Detach(e));

            var advanced = new GroupCard("actorgroup") { Title = TranslateOr("advanced", "Advanced"), Margin = new Thickness(0, 0, 0, 12), IsExpanded = true };
            advanced.Add("ag_relations", "Relations", relations);
            advanced.Add("ag_behaviour", "Behaviour", Detach(BodyOf(behaviour)));
            advanced.Add("ag_vehicle", "Vehicle", vehicle);
            advanced.Add("ag_death", "Proofs & death", death);

            // When: lifecycle with respawn and "action on", then the rules card.
            var lifecycle = new LifecycleCard();
            lifecycle.Attach(ddactorspawnon, ddactorspawnteam, tbactorspawnrule, ddactorteamclear, tbactorclearrule, true);
            Detach(lifecycle);
            lifecycle.Margin = new Thickness(0, 0, 0, 12);
            lifecycle.AddSection("asrlactionon", "Action on", "AccentBrush", "M5,12 L19,12 M13,6 L19,12 L13,18", Detach(RowOf(ddactoractionon)));
            lifecycle.AddSection("actorrsp", "Respawn", "AccentBrush", "M4,12 A8,8 0 0 1 18,7 L20,9 M20,12 A8,8 0 0 1 6,17 L4,15 M20,4 L20,9 L15,9 M4,20 L4,15 L9,15", Detach(BodyOf(respawn)));
            // The old rule card keeps its raw fields (priority bits, rule per team) as a group.
            oldRule.Visibility = Visibility.Visible;
            var raw = BodyOf(oldRule);
            advanced.Add("ag_raw", "Raw rule data", Detach(raw));

            // Goto: route above the page's point fields; the number box is replaced by the list.
            var gotoBody = BodyOf(gotoCard);
            ((FrameworkElement)ddActorgoto.Parent).Visibility = Visibility.Collapsed;
            _actorRoute = new GotoRouteView(ddActorgoto, () => ddactorno.SelectedIndex,
                i => (new Global(GTA.Offsets.Editor.Actor.actvx + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + i * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>(),
                      new Global(GTA.Offsets.Editor.Actor.actvy + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + i * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>()),
                () => (new Global(GTA.Offsets.Editor.Actor.locx + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).Get<float>(),
                       new Global(GTA.Offsets.Editor.Actor.locy + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).Get<float>()),
                () => Btnactorgetlocgoto.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));
            gotoBody.Children.Insert(0, _actorRoute);
            // Goto points in a collapsible card, closed by default: most actors never move.
            var gotoSection = new SectionCard
            {
                Style = (Style)FindResource(typeof(SectionCard)),
                Title = TranslateOr("actorgoto", "Goto Points"),
                Icon = System.Windows.Media.Geometry.Parse("M5,19 L9,11 L14,14 L19,5"),
                CanCollapse = true,
                IsExpanded = false,
                Margin = new Thickness(0, 0, 0, 12),
            };
            _actorRoute.Changed += n => gotoSection.Summary = n == 0 ? TranslateOr("gr_none", "none") : string.Format(TranslateOr("gr_count", "{0} points"), n);

            // Combat like the mockup: style as tiles, accuracy and health as stepped sliders.
            TextBlock LabelOf(ComboBox box) => box.Parent is Panel p && p.Children.IndexOf(box) > 0 ? p.Children[p.Children.IndexOf(box) - 1] as TextBlock : null;
            ComboViews.Tiles(ddActorcombat, 3);
            // Combat style first and full width; idle then gets its row alone.
            if (ddActorcombat.Parent is StackPanel styleCell && styleCell.Parent is Grid styleGrid)
            {
                styleGrid.Children.Remove(styleCell);
                Grid.SetColumn(styleCell, 0);
                combatBody.Children.Insert(0, styleCell);
                foreach (FrameworkElement rest in styleGrid.Children)
                {
                    Grid.SetColumn(rest, 0);
                    Grid.SetColumnSpan(rest, Math.Max(1, styleGrid.ColumnDefinitions.Count));
                }
            }
            ComboViews.Steps(ddActoraccu, LabelOf(ddActoraccu));
            ComboViews.Steps(ddActorhealth, LabelOf(ddActorhealth));
            foreach (var box in new[] { tbactoractvx, tbactoractvy, tbactorlocx, tbactorlocy })
                box.TextChanged += (_, __) => { if (!box.IsKeyboardFocused) _actorRoute.Refresh(); };
            ddactorno.SelectionChanged += (_, __) => _actorRoute.Refresh();

            // Columns. Cards not placed above go into the advanced card, so nothing gets lost.
            var handled = new HashSet<Border> { combat, behaviour, proofs, respawn, gotoCard, oldRule, placement };
            foreach (var card in allCards)
                Detach(card);
            foreach (var card in allCards.Where(c => !handled.Contains(c)))
                advanced.Add("ag_more", ((card.Child as DockPanel)?.Children.OfType<Border>().FirstOrDefault()?.Child as TextBlock)?.Text ?? "…", card);
            Detach(ActorExtraRules);
            masonry.Children.Clear();

            // Left: who. Middle: rules, then lifecycle. Right: goto points, then advanced.
            who.Children.Add(placement);
            who.Children.Add(combat);
            var when = new StackPanel();
            when.Children.Add(ActorExtraRules);
            when.Children.Add(lifecycle);
            gotoSection.Content = Detach(gotoBody);
            var more = new StackPanel();
            more.Children.Add(gotoSection);
            more.Children.Add(advanced);
            masonry.MaxColumns = 3;
            masonry.Children.Add(who);
            masonry.Children.Add(when);
            masonry.Children.Add(more);
        }
    }
}
