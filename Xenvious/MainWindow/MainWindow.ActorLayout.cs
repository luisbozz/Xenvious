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

            // Goto: the route view gets the page's own fields, regrouped. Route-wide settings
            // (loop, vehicle speed, hover/rappel) go on top; the point's fields under its step.
            var gotoBody = BodyOf(gotoCard);
            ((FrameworkElement)ddActorgoto.Parent).Visibility = Visibility.Collapsed;
            FrameworkElement CellOf(FrameworkElement box, string key = null, string fallback = null)
            {
                var cell = box.Parent is StackPanel c && c.Children.Count <= 2 ? (FrameworkElement)c : box;
                if (key != null && cell is StackPanel sc && sc.Children.Count == 2 && sc.Children[0] is TextBlock label)
                {
                    label.Text = TranslateOr(key, fallback);
                    label.SetResourceReference(StyleProperty, "FieldLabel");
                }
                return Detach(cell);
            }
            Grid Pair(FrameworkElement a, FrameworkElement b)
            {
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition());
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                g.ColumnDefinitions.Add(new ColumnDefinition());
                // The cells keep the column they had in the page's grid; reset it.
                Grid.SetColumn(a, 0);
                g.Children.Add(a);
                if (b != null) { Grid.SetColumn(b, 2); g.Children.Add(b); }
                return g;
            }
            var routeOptions = new StackPanel();
            foreach (var flag in new FrameworkElement[] { cbactoractvloop, cbactoractvhadest, cbactoractvradest, cbactoractvrpadest })
                routeOptions.Children.Add(Detach(RowOf(flag)));
            var vehSpeed = CellOf(tbactoractvvehspeed, "gr_vehspeed", "Vehicle speed (whole route)");
            var sizeCell = CellOf(tbactoractvsize, "gr_radius", "Arrival radius");
            routeOptions.Children.Add(Pair(vehSpeed, null));

            TextBlock Heading(string key, string fallback)
            {
                var t = new TextBlock { Text = TranslateOr(key, fallback) };
                t.SetResourceReference(StyleProperty, "FieldLabel");
                return t;
            }
            TextBlock Note(string key, string fallback)
            {
                var t = new TextBlock { Text = TranslateOr(key, fallback), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) };
                t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                return t;
            }
            var details = new StackPanel();
            details.Children.Add(Heading("gr_pos", "Position"));
            details.Children.Add(Detach(RowOf(tbactoractvx)));
            details.Children.Add(Pair(sizeCell, CellOf(tbactoractvspeed, "gr_speed", "Speed")));
            details.Children.Add(Pair(CellOf(tbactoractvawt, "gr_waitms", "Wait (ms)"), null));
            // Trigger area (FMMC_AOGT_TAP / _TAR): position and radius belong together.
            details.Children.Add(Heading("gr_trigger", "Trigger area"));
            details.Children.Add(Note("gr_trigger_hint", "Optional. Position 0,0,0 = no trigger area."));
            details.Children.Add(Detach(RowOf(tbactoractvawlx)));
            details.Children.Add(Pair(CellOf(tbactoractvawlr, "gr_trigr", "Trigger radius"), null));
            var rawGoto = new Expander { Header = TranslateOr("gr_raw", "Raw values"), IsExpanded = false, Margin = new Thickness(0, 4, 0, 0) };
            rawGoto.SetResourceReference(StyleProperty, "CardExpander");
            var rawPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
            rawPanel.Children.Add(Pair(CellOf(tbactoractvachf), CellOf(tbactoractvawr)));
            rawPanel.Children.Add(Pair(CellOf(tbactoractvags), CellOf(tbactoractvbs)));
            rawGoto.Content = rawPanel;
            details.Children.Add(rawGoto);
            // Whatever is left of the old card body (separators, labels) stays out of sight.
            gotoBody.Visibility = Visibility.Collapsed;

            long Point(long field, int i) => field + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + i * GTA.Offsets.Editor.Actor.actv_NEXT;
            bool GotoLive() => m.IsProcOpen && ddactorno.SelectedIndex >= 0 && GTA.Offsets.Editor.Actor.actv_NEXT > 0;
            _actorRoute = new GotoRouteView(ddActorgoto, GotoLive,
                i => new GotoPoint
                {
                    X = new Global(Point(GTA.Offsets.Editor.Actor.actvx, i)).Get<float>(),
                    Y = new Global(Point(GTA.Offsets.Editor.Actor.actvy, i)).Get<float>(),
                    Z = new Global(Point(GTA.Offsets.Editor.Actor.actvz, i)).Get<float>(),
                    WaitMs = new Global(Point(GTA.Offsets.Editor.Actor.awt, i)).Get<int>(),
                    Bits = new Global(Point(GTA.Offsets.Editor.Actor.actv_bs, i)).Get<int>(),
                },
                () => (new Global(GTA.Offsets.Editor.Actor.locx + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).Get<float>(),
                       new Global(GTA.Offsets.Editor.Actor.locy + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).Get<float>()),
                i =>
                {
                    ddActorgoto.SelectedIndex = i;
                    Btnactorgetlocgoto.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                },
                (a, b) =>
                {
                    // A point is 28 slots from f_0 (the one-int bit array's size slot, left alone) on.
                    long start = GTA.Offsets.Editor.Actor.actv_bs - 1;
                    for (int f = 1; f < GTA.Offsets.Editor.Actor.actv_NEXT; f++)
                    {
                        var ga = new Global(Point(start + f, a));
                        var gb = new Global(Point(start + f, b));
                        int va = ga.Get<int>(), vb = gb.Get<int>();
                        ga.SetInt(vb);
                        gb.SetInt(va);
                    }
                    GetActorACTVValues();
                },
                (i, bit, on) =>
                {
                    var g = new Global(Point(GTA.Offsets.Editor.Actor.actv_bs, i));
                    int v = g.Get<int>();
                    g.SetInt(on ? v | (1 << bit) : v & ~(1 << bit));
                    GetActorACTVValues();
                },
                routeOptions, details)
            {
                DeleteLast = i =>
                {
                    foreach (var f in new[] { GTA.Offsets.Editor.Actor.actvx, GTA.Offsets.Editor.Actor.actvy, GTA.Offsets.Editor.Actor.actvz })
                        new Global(Point(f, i)).SetFloat(0f);
                    GetActorACTVValues();
                },
            };
            gotoBody = new StackPanel();
            gotoBody.Children.Add(_actorRoute);
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
            foreach (var box in new[] { tbactoractvx, tbactoractvy, tbactoractvz, tbactorlocx, tbactorlocy, tbactoractvawt, tbactoractvbs })
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
            gotoSection.Content = gotoBody;
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
