using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Xenvious.Logging;
using Xenvious.Translation;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Misc / ScrPatches page.
    public partial class MainWindow
    {
        /// <summary>
        /// Fill the patch list from what the app actually runs.
        ///
        /// Both lists are shown together because both are applied together, and
        /// a patch missing from the page while it is being applied in the
        /// background is worse than a longer list.
        /// </summary>
        private void LoadScrPatchesPage()
        {
            var all = new List<GTA.ScrPatches>();
            if (GTA.Editor.ScrPatches != null)
                all.AddRange(GTA.Editor.ScrPatches);
            if (GTA.Editor.ScrPatchesDev != null)
                all.AddRange(GTA.Editor.ScrPatchesDev);

            // Entries shipped with "enabled": false are parked (a broken or replaced pattern kept
            // for later). They stay out of the cards: a card is "on" only when all its entries
            // are, and switching it on would wake the parked ones too.
            foreach (var p in all)
                if (ScrPatchSeen.Add(p) && !p.enabled)
                    ScrPatchParked.Add(p);

            // One card per patch name: most patches exist once per creator script and are
            // one feature, so they are shown and switched together.
            _scrPatchGroups = all
                .Where(p => !string.IsNullOrEmpty(p.patch_name) && !ScrPatchParked.Contains(p))
                .GroupBy(p => p.patch_name)
                .Select(g => new ScrPatchGroup(g.Key, g.ToList(), ScrPatchDescription(g.Key),
                    TranslateOr, OnScrPatchToggled))
                .OrderBy(g => ScrPatchName(g.Name), StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            _patchesShownKey = null;
            RenderPatchesPage();
        }

        private List<ScrPatchGroup> _scrPatchGroups = new List<ScrPatchGroup>();
        private static readonly HashSet<GTA.ScrPatches> ScrPatchSeen = new HashSet<GTA.ScrPatches>();
        private static readonly HashSet<GTA.ScrPatches> ScrPatchParked = new HashSet<GTA.ScrPatches>();
        private string _scrPatchScript;   // null: all scripts
        private string _patchesQuery = "";
        private string _patchesShownKey;
        private BareSearchBox _patchesSearch;
        private DispatcherTimer _patchesTimer;

        private static readonly string[] ScrPatchScriptOrder =
        {
            "fm_race_creator", "fm_lts_creator", "fm_capture_creator", "public_mission_creator", "fm_deathmatch_creator", "fm_survival_creator", "fmmc_launcher"
        };

        private static string ScrPatchScriptLabel(string script)
        {
            switch (script)
            {
                case "fm_race_creator": return "Race";
                case "fm_lts_creator": return "LTS";
                case "fm_capture_creator": return "Capture";
                case "public_mission_creator": return "Mission";
                case "fm_deathmatch_creator": return "Deathmatch";
                case "fm_survival_creator": return "Survival";
                case "fmmc_launcher": return "Launcher";
                default: return script;
            }
        }

        private static string ScrPatchSlug(string patchName)
        {
            string slug = Regex.Replace((patchName ?? "").ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
            return slug.Length > 48 ? slug.Substring(0, 48).TrimEnd('_') : slug;
        }

        // Readable name from "scrpatchname_<slug>", the JSON name when there is none.
        private string ScrPatchName(string patchName) => TranslateOr("scrpatchname_" + ScrPatchSlug(patchName), patchName);

        // Which group a patch is listed under; unknown names go to "more".
        private static string ScrPatchGroupKey(string patchName)
        {
            switch (ScrPatchSlug(patchName))
            {
                case "dont_force_plyl_to_1":
                case "dont_force_kill_rule_to_6_and_number_to_1":
                case "dont_force_round_pa_with_dev_mode":
                case "nrl_fix":
                case "blue_fence_props_fix":
                case "selected_spawn_veicle_stays_after_testing_ends":
                    return "fixes";
                case "dev_mode":
                case "cam_fix":
                case "show_stunt_prop_item_cycle":
                case "fm_capture_creator_80_actors_patch":
                case "precise_templates":
                case "access_ch_planning_board_solo_gameplay_itself_re":
                    return "unlock";
                case "show_placed_zones_brighter":
                case "show_placed_angled_zones_brighter":
                case "set_switch_camera_to_caps_lock":
                    return "display";
                default:
                    return "more";
            }
        }

        private static readonly (string Key, string Fallback)[] ScrPatchGroups =
        {
            ("fixes", "Fixes"), ("unlock", "Unlocks"), ("display", "Display and controls"), ("more", "More"),
        };

        // ----- page -----

        private void EnsurePatchesPage()
        {
            if (_patchesSearch != null)
                return;
            _patchesSearch = new BareSearchBox(TranslateOr("patches_search", "Search patches…")) { Width = 240, VerticalAlignment = VerticalAlignment.Center };
            _patchesSearch.Changed += q => { _patchesQuery = q.Trim(); _patchesShownKey = null; RenderPatchesPage(); };
            patchesSearchHost.Child = _patchesSearch;
            _patchesTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
            _patchesTimer.Tick += (_, __) => RenderPatchesPage();
            patchesPage.IsVisibleChanged += (_, __) =>
            {
                if (patchesPage.IsVisible) { _patchesShownKey = null; RenderPatchesPage(); _patchesTimer.Start(); }
                else _patchesTimer.Stop();
            };
        }

        private void BtnPatchesApply_Click(object sender, RoutedEventArgs e)
        {
            LoadScrPatchesPage();
        }

        // Rebuilt only when something visible changed, so a toggle under the mouse is not
        // replaced every tick.
        private void RenderPatchesPage()
        {
            EnsurePatchesPage();
            string creator = m != null && m.IsProcOpen ? GTA.CurrentCreatorName() ?? "" : "";
            var natives = GamePatchRows();
            string key = string.Join(",", natives.Select(n => n.State)) + "|" + creator + "|" + _scrPatchScript + "|" + _patchesQuery + "|"
                + ScriptVars.FeaturesOn + ScriptVars.CameraKey + ScriptDrawer.Running(creator) + ScriptDrawer.ShownCount + "|"
                + string.Join(",", _scrPatchGroups.Select(g => (g.Enabled ? "1" : "0") + string.Concat(g.Patches.Select(p => ScrPatchesRunner.IsApplied(p) ? "a" : "-"))));
            if (key == _patchesShownKey)
                return;
            _patchesShownKey = key;

            RenderPatchesHeader(creator);
            patchesExe.Text = GameVariant.IsEnhanced ? "GTA5_Enhanced.exe" : "GTA5.exe";

            patchesGame.Children.Clear();
            foreach (var row in natives)
                if (MatchesPatchQuery(row.Name + " " + row.Description))
                    patchesGame.Children.Add(GamePatchCard(row));

            RenderCustomFuncs(creator);

            RenderScrPatchFilter();
            patchesScripts.Children.Clear();
            var visible = _scrPatchGroups
                .Where(g => !IsCustomFuncGroup(g))
                .Where(g => _scrPatchScript == null || g.Patches.Any(p => p.script_name == _scrPatchScript))
                .Where(g => MatchesPatchQuery(ScrPatchName(g.Name) + " " + g.Description + " " + g.Name))
                .ToList();
            foreach (var (groupKey, fallback) in ScrPatchGroups)
            {
                var items = visible.Where(g => ScrPatchGroupKey(g.Name) == groupKey).ToList();
                if (items.Count == 0)
                    continue;
                patchesScripts.Children.Add(PatchGroupLabel(string.Format(CultureInfo.CurrentCulture, "{0} · {1}/{2} {3}",
                    TranslateOr("patches_grp_" + groupKey, fallback), items.Count(g => g.Enabled), items.Count, TranslateOr("patches_on", "on"))));
                var grid = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2, Margin = new Thickness(0, 0, -10, 8) };
                foreach (var group in items)
                    grid.Children.Add(ScrPatchCard(group, creator));
                patchesScripts.Children.Add(grid);
            }
            if (visible.Count == 0)
                patchesScripts.Children.Add(new TextBlock { Text = TranslateOr("patches_none", "No patch matches the search."), Foreground = ThemeBrush("NavMutedBrush"), Margin = new Thickness(0, 4, 0, 8) });

            int on = _scrPatchGroups.Count(g => !IsCustomFuncGroup(g) && g.Enabled), total = _scrPatchGroups.Count(g => !IsCustomFuncGroup(g));
            patchesScriptCount.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("patches_count", "{0} of {1} on"), on, total);
        }

        // Edition as a segmented control (the running one raised), then build and open creator.
        private void RenderPatchesHeader(string creator)
        {
            // Only information: the edition follows the game Xenvious is attached to.
            var edition = new StackPanel { Orientation = Orientation.Horizontal };
            var dot = new System.Windows.Shapes.Ellipse { Width = 8, Height = 8, Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center };
            dot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, m != null && m.IsProcOpen ? "OkBrush" : "FaintTextBrush");
            edition.Children.Add(dot);
            var name = new TextBlock { Text = "GTA V " + GameVariant.DisplayName(GameVariant.Current), FontWeight = FontWeights.Bold, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
            name.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            edition.Children.Add(name);
            var track = new Border { CornerRadius = new CornerRadius(12), Padding = new Thickness(10, 4, 12, 5), BorderThickness = new Thickness(1), Child = edition,
                ToolTip = TranslateOr("patches_edition_tip", "The game edition Xenvious is connected to"), UseLayoutRounding = true };
            track.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            track.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            patchesEdition.Child = track;

            // "1.73-3889": online version and the build number without its ".0" / ".16" part.
            string build = GTA.GameVersion();
            patchesMeta.Inlines.Clear();
            patchesMeta.Inlines.Add(new Run(TranslateOr("patches_build", "Build") + " "));
            patchesMeta.Inlines.Add(new Run(build.Length == 0 ? "–" : build) { FontFamily = new FontFamily("Consolas"), FontWeight = FontWeights.Bold, Foreground = ThemeBrush("TextColor") });
            patchesMeta.Inlines.Add(new Run(" · " + TranslateOr("patches_creatoropen", "Creator open:") + " "));
            patchesMeta.Inlines.Add(new Run(creator.Length == 0 ? "–" : ScrPatchScriptLabel(creator)) { FontWeight = FontWeights.Bold, Foreground = ThemeBrush("TextColor") });
        }

        private bool MatchesPatchQuery(string text)
            => _patchesQuery.Length == 0 || (text ?? "").IndexOf(_patchesQuery, StringComparison.CurrentCultureIgnoreCase) >= 0;

        private TextBlock PatchGroupLabel(string text)
        {
            var label = new TextBlock { Text = text.ToUpper(CultureInfo.CurrentCulture), FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 6, 0, 6) };
            label.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            return label;
        }

        private void RenderScrPatchFilter()
        {
            var scripts = _scrPatchGroups.Where(g => !IsCustomFuncGroup(g)).SelectMany(g => g.Patches).Select(p => p.script_name).Where(n => !string.IsNullOrEmpty(n)).Distinct()
                .OrderBy(n => Array.IndexOf(ScrPatchScriptOrder, n) < 0 ? int.MaxValue : Array.IndexOf(ScrPatchScriptOrder, n)).ToList();
            if (_scrPatchScript != null && !scripts.Contains(_scrPatchScript))
                _scrPatchScript = null;
            scrPatchesFilter.Children.Clear();
            int Count(string script) => _scrPatchGroups.Count(g => !IsCustomFuncGroup(g) && (script == null || g.Patches.Any(p => p.script_name == script)));
            scrPatchesFilter.Children.Add(ScrPatchFilterChip(TranslateOr("scrpatch_filter_all", "All"), null, Count(null)));
            foreach (string script in scripts)
                scrPatchesFilter.Children.Add(ScrPatchFilterChip(ScrPatchScriptLabel(script), script, Count(script)));
        }

        // Chips as in the mockup: outlined, the chosen one with the accent outline and a soft fill.
        private Border ScrPatchFilterChip(string label, string script, int count)
        {
            bool selected = _scrPatchScript == script;
            var text = new TextBlock { FontSize = 12.5 };
            text.SetResourceReference(TextBlock.ForegroundProperty, selected ? "TextColor" : "MutedTextBrush");
            text.Inlines.Add(new Run(label));
            text.Inlines.Add(new Run("  " + count.ToString(CultureInfo.CurrentCulture)) { FontSize = 11.5, Foreground = ThemeBrush("FaintTextBrush") });
            var fill = new Grid();
            fill.Children.Add(new Border { CornerRadius = new CornerRadius(12) }.Also(b => b.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush")));
            if (selected)
                fill.Children.Add(new Border { CornerRadius = new CornerRadius(12), Opacity = 0.16 }.Also(b => b.SetResourceReference(Border.BackgroundProperty, "AccentBrush")));
            fill.Children.Add(new Border { Padding = new Thickness(11, 3, 11, 4), Child = text });
            var chip = new Border { Margin = new Thickness(0, 0, 6, 6), Cursor = Cursors.Hand, Child = Framed(fill, 12, selected) };
            chip.MouseLeftButtonUp += (_, __) => { _scrPatchScript = script; _patchesShownKey = null; RenderPatchesPage(); };
            return chip;
        }

        // A card: title and description on the left, the switch on the right, status chips below.
        private Border PatchCard(string title, string description, bool on, bool canSwitch, Action<bool> toggled, IEnumerable<FrameworkElement> chips)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var text = new StackPanel();
            text.Children.Add(new TextBlock { Text = title, FontSize = 14.5, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap, Foreground = ThemeBrush("TextColor") });
            if (!string.IsNullOrEmpty(description))
            {
                var d = new TextBlock { Text = description, FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
                d.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                text.Children.Add(d);
            }
            var foot = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
            foreach (var chip in chips)
                foot.Children.Add(chip);
            text.Children.Add(foot);
            grid.Children.Add(text);
            if (toggled != null)
            {
                var box = new CheckBox { Style = (Style)FindResource("FormToggle"), IsChecked = on, IsEnabled = canSwitch, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(12, 2, 0, 0) };
                box.Click += (_, __) => { toggled(box.IsChecked == true); _patchesShownKey = null; RenderPatchesPage(); };
                Grid.SetColumn(box, 1);
                grid.Children.Add(box);
            }
            var body = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(14, 12, 14, 10), Child = grid };
            body.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            return new Border { Margin = new Thickness(0, 0, 10, 10), Child = Framed(body, 8, on) };
        }

        /// <summary>
        /// The outline goes over the content as its own layer, so rounded corners stay clean; an
        /// active item gets the accent outline and a soft accent ring around it.
        /// </summary>
        internal static Grid Framed(UIElement content, double radius, bool active)
        {
            var grid = new Grid();
            if (active)
            {
                var ring = new Border { Margin = new Thickness(-3), CornerRadius = new CornerRadius(radius + 3), BorderThickness = new Thickness(2), Opacity = 0.22, IsHitTestVisible = false };
                ring.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
                grid.Children.Add(ring);
            }
            grid.Children.Add(content);
            var frame = new Border { CornerRadius = new CornerRadius(radius), BorderThickness = new Thickness(active ? 1.5 : 1), IsHitTestVisible = false, UseLayoutRounding = true };
            frame.SetResourceReference(Border.BorderBrushProperty, active ? "AccentBrush" : "LineBrush");
            grid.Children.Add(frame);
            return grid;
        }

        // A small rounded status chip with a coloured dot (null: no dot).
        private Border StatusChip(string text, string dotBrush, string tooltip = null)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            if (dotBrush != null)
            {
                var dot = new System.Windows.Shapes.Ellipse { Width = 7, Height = 7, Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
                dot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, dotBrush);
                panel.Children.Add(dot);
            }
            var t = new TextBlock { Text = text, FontSize = 11.5, FontWeight = FontWeights.SemiBold };
            t.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            panel.Children.Add(t);
            var chip = new Border { CornerRadius = new CornerRadius(9), Padding = new Thickness(8, 1, 9, 2), Margin = new Thickness(0, 0, 6, 4), BorderThickness = new Thickness(1), Child = panel, ToolTip = tooltip,
                UseLayoutRounding = true, SnapsToDevicePixels = true };
            chip.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            chip.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            return chip;
        }

        private Border ScrPatchCard(ScrPatchGroup group, string creator)
            => PatchCard(ScrPatchName(group.Name), group.Description, group.Enabled, true, on => group.Enabled = on, ScrPatchChips(group, creator, false));

        // Per script: green = written into the running creator, yellow = on but not found in it
        // (pattern no longer matches), grey = that creator is not open (applied when opened).
        // Custom func patches count as on: they follow the script features switch.
        private List<FrameworkElement> ScrPatchChips(ScrPatchGroup group, string creator, bool customFunc)
        {
            var chips = new List<FrameworkElement>();
            foreach (var script in group.Patches.Select(p => p.script_name).Where(n => !string.IsNullOrEmpty(n)).Distinct()
                .OrderBy(n => Array.IndexOf(ScrPatchScriptOrder, n) < 0 ? int.MaxValue : Array.IndexOf(ScrPatchScriptOrder, n)))
            {
                var patches = group.Patches.Where(p => p.script_name == script).ToList();
                bool applied = patches.Any(ScrPatchesRunner.IsApplied);
                bool enabled = customFunc ? ScriptVars.FeaturesOn || !patches.Any(p => p.dev) : patches.All(p => p.enabled);
                // Triggered patches (precise templates) are only in the script while their feature runs.
                bool waitsForTrigger = !applied && patches.All(p => !string.IsNullOrEmpty(p.trigger));
                // Found written by an earlier session without its original bytes: only a GTA restart takes it out.
                bool stuck = patches.Any(ScrPatchesRunner.IsStuck);
                string brush = !enabled ? (stuck ? "WarnBrush" : null) : applied ? "OkBrush" : script == creator && !waitsForTrigger ? "WarnBrush" : "FaintTextBrush";
                string tip = !enabled && stuck ? TranslateOr("patches_st_stuckoff", "Off, but still in the script from an earlier Xenvious session: restart GTA to take it out.")
                    : !enabled ? TranslateOr("patches_st_off", "Off")
                    : applied ? TranslateOr("patches_st_applied", "Applied")
                    : waitsForTrigger ? TranslateOr("patches_st_trigger", "Only written while the feature is in use (templates category).")
                    : script == creator ? TranslateOr("patches_st_notfound", "On, but not found in the script: the pattern may need updating after a game update.")
                    : TranslateOr("patches_st_notloaded", "Creator not open: applied when it is opened.");
                chips.Add(StatusChip(ScrPatchScriptLabel(script), brush, tip));
            }
            if (group.Patches.Any(p => p.trigger == "templates"))
                chips.Add(StatusChip(TranslateOr("scrpatch_tag_templates", "templates only"), null));
            return chips;
        }

        // ----- custom funcs -----

        // The patches that inject and call the custom funcs: the dev patches (switched by the
        // script features setting) and the one that clears room for them.
        private static bool IsCustomFuncGroup(ScrPatchGroup group)
            => group.Patches.Any(p => p.dev) || ScrPatchSlug(group.Name) == "nop_some_func_for_custom_funcs";

        private bool _customFuncPatchesOpen;

        // What the custom funcs give, and in which creators: null = all of them.
        private static readonly (string Key, string Fallback, string[] Scripts)[] CustomFuncFeatures =
        {
            ("patches_cf_f_dims", "Model sizes", null),
            ("patches_cf_f_hover", "Hovered model", null),
            ("patches_cf_f_drawer", "Play area drawer", null),
            ("patches_cf_f_hidden", "Hidden categories", new[] { "fm_lts_creator", "fm_capture_creator" }),
            ("patches_cf_f_race", "Race: tuning and garage", new[] { "fm_race_creator" }),
            ("patches_cf_f_camera", "Camera with Caps Lock", null),
        };

        private void RenderCustomFuncs(string creator)
        {
            var groups = _scrPatchGroups.Where(IsCustomFuncGroup).ToList();
            var patches = groups.SelectMany(g => g.Patches).ToList();
            var root = new StackPanel();

            // Main switch: the script features setting itself.
            var head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition());
            head.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var text = new StackPanel();
            text.Children.Add(new TextBlock { Text = TranslateOr("patches_cf_switch", "Script features"), FontSize = 14.5, FontWeight = FontWeights.Bold, Foreground = ThemeBrush("TextColor") });
            var desc = new TextBlock { Text = TranslateOr("patches_cf_switch_desc", "Switches all custom funcs on. Same switch as in the settings."), FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) };
            desc.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            text.Children.Add(desc);
            head.Children.Add(text);
            var main = new CheckBox { Style = (Style)FindResource("SwitchToggle"), IsChecked = cbsettingsexpscrfeat.IsChecked == true, Margin = new Thickness(12, 0, 0, 0) };
            main.Click += (_, __) => { cbsettingsexpscrfeat.IsChecked = main.IsChecked == true; _patchesShownKey = null; RenderPatchesPage(); };
            Grid.SetColumn(main, 1);
            head.Children.Add(main);
            root.Children.Add(head);

            // One tile per creator that has custom funcs.
            var scripts = patches.Select(p => p.script_name).Where(n => !string.IsNullOrEmpty(n)).Distinct()
                .OrderBy(n => Array.IndexOf(ScrPatchScriptOrder, n) < 0 ? int.MaxValue : Array.IndexOf(ScrPatchScriptOrder, n)).ToList();
            var tiles = new System.Windows.Controls.Primitives.UniformGrid { Columns = Math.Max(1, scripts.Count), Margin = new Thickness(0, 12, -8, 0) };
            foreach (string script in scripts)
                tiles.Children.Add(CustomFuncTile(script, creator, patches.Where(p => p.script_name == script).ToList()));
            root.Children.Add(tiles);

            // What they give; green where it works in the open creator (or anywhere, with none open).
            var features = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            foreach (var (key, fallback, only) in CustomFuncFeatures)
            {
                bool on = key == "patches_cf_f_camera" ? ScriptVars.CameraKey
                    : key == "patches_cf_f_drawer" && creator.Length > 0 ? ScriptDrawer.Running(creator)
                    : ScriptVars.FeaturesOn && (creator.Length == 0 || only == null || only.Contains(creator));
                string tip = only == null ? null : string.Join(", ", only.Select(ScrPatchScriptLabel));
                features.Children.Add(StatusChip(TranslateOr(key, fallback), on ? "OkBrush" : "FaintTextBrush", tip));
            }
            root.Children.Add(features);

            // The script patches behind them, folded away.
            var list = new Grid { Margin = new Thickness(14, 10, 0, 0) };
            list.ColumnDefinitions.Add(new ColumnDefinition());
            list.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            int row = 0;
            foreach (var group in groups.OrderBy(g => ScrPatchName(g.Name), StringComparer.CurrentCultureIgnoreCase))
            {
                list.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var name = new TextBlock { Text = ScrPatchName(group.Name), FontSize = 12.5, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 4), ToolTip = string.IsNullOrEmpty(group.Description) ? null : group.Description };
                name.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                Grid.SetRow(name, row);
                list.Children.Add(name);
                var chips = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
                foreach (var chip in ScrPatchChips(group, creator, true))
                    chips.Children.Add(chip);
                Grid.SetRow(chips, row);
                Grid.SetColumn(chips, 1);
                list.Children.Add(chips);
                row++;
            }
            var header = new TextBlock { FontWeight = FontWeights.SemiBold, Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("patches_cf_patches", "Script patches for them ({0})"), groups.Count) };
            header.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            var fold = new Expander { Style = (Style)FindResource("CardExpander"), Header = header, Content = list, IsExpanded = _customFuncPatchesOpen, Margin = new Thickness(0, 10, 0, 0) };
            fold.Expanded += (_, __) => _customFuncPatchesOpen = true;
            fold.Collapsed += (_, __) => _customFuncPatchesOpen = false;
            root.Children.Add(fold);

            patchesCustomFuncs.Child = root;
            patchesCustomFuncs.Visibility = groups.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // A creator: whether its custom funcs run, and where they live in the script.
        private Border CustomFuncTile(string script, string creator, List<GTA.ScrPatches> patches)
        {
            bool here = script == creator;
            bool applied = patches.Where(p => p.dev).Any(ScrPatchesRunner.IsApplied);
            string status, dot;
            if (!here) { status = TranslateOr("patches_cf_notopen", "not open"); dot = "FaintTextBrush"; }
            else if (!ScriptVars.FeaturesOn) { status = TranslateOr("patches_st_off", "Off"); dot = "FaintTextBrush"; }
            else if (applied)
            {
                status = TranslateOr("patches_cf_running", "running");
                if (ScriptDrawer.Running(script))
                    status += " · " + string.Format(CultureInfo.CurrentCulture, TranslateOr("patches_cf_drawer", "drawer {0} shapes"), ScriptDrawer.ShownCount);
                dot = "OkBrush";
            }
            else { status = TranslateOr("patches_cf_notwritten", "not written"); dot = "WarnBrush"; }

            var page = patches.FirstOrDefault(p => p.page);
            string where = page != null
                ? string.Format(CultureInfo.InvariantCulture, TranslateOr("patches_cf_page", "Page {0}"), "0x" + page.page_base.ToString("X", CultureInfo.InvariantCulture))
                : TranslateOr("patches_cf_inline", "in a script function");

            var panel = new StackPanel();
            var title = new StackPanel { Orientation = Orientation.Horizontal };
            var d = new System.Windows.Shapes.Ellipse { Width = 8, Height = 8, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center };
            d.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, dot);
            title.Children.Add(d);
            title.Children.Add(new TextBlock { Text = ScrPatchScriptLabel(script), FontWeight = FontWeights.Bold, FontSize = 13, Foreground = ThemeBrush("TextColor") });
            panel.Children.Add(title);
            var st = new TextBlock { Text = status, FontSize = 11.5, Margin = new Thickness(0, 3, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = status };
            st.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            panel.Children.Add(st);
            var w = new TextBlock { Text = where, FontSize = 11, FontFamily = new FontFamily("Consolas"), Margin = new Thickness(0, 2, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
            w.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            panel.Children.Add(w);

            var body = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(10, 8, 10, 8), Child = panel };
            body.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            return new Border { Margin = new Thickness(0, 0, 8, 0), Child = Framed(body, 8, here) };
        }

        // ----- patches in the game executable -----

        private enum GamePatchState { Active, Found, Stuck, NoPattern, Offline }

        private sealed class GamePatchRow
        {
            public string Name, Description, Key;
            public GamePatchState State;
            public Action<bool> Set;
        }

        private List<GamePatchRow> GamePatchRows()
        {
            var rows = new List<GamePatchRow>();
            bool open = m != null && m.IsProcOpen;
            // Developer mode: one int at the dev pointer (also on the dashboard).
            bool devFound = open && GTA.Offsets.Editor.dev != 0;
            rows.Add(new GamePatchRow
            {
                Name = TranslateOr("developermode", "Developer Mode"), Key = "devptr",
                Description = TranslateOr("patches_dev_desc", "Unlocks the game's developer functions that several script patches need."),
                State = !open ? GamePatchState.Offline : !devFound ? GamePatchState.NoPattern : m.memory(GTA.Offsets.Editor.dev).Get<int>() == GTA.DevPatched ? GamePatchState.Active : GamePatchState.Found,
                Set = SetDevMode,
            });
            rows.Add(GamePatchRowFor(GamePatches.CameraNoCollision, TranslateOr("np_camnocol", "Creator camera without collision"), TranslateOr("np_camnocol_tip", "The creator camera passes through walls and the ground."),
                "creator_cam_nocollision", GTA.Offsets.Editor.AOB_creator_cam_nocollision));
            rows.Add(GamePatchRowFor(GamePatches.NoBudget, TranslateOr("np_nobudget", "Ignore creator budget"), TranslateOr("patches_budget_desc", "The budget bar stays empty."),
                "creator_budget", GTA.Offsets.Editor.AOB_creator_budget));
            rows.Add(GamePatchRowFor(GamePatches.TestMode, TranslateOr("np_testmode", "Test Mode"), TranslateOr("patches_testmode_desc", "Enables test mode features."),
                "testmode", GTA.Offsets.Editor.AOB_testmode));
            return rows;
        }

        private GamePatchRow GamePatchRowFor(GamePatch patch, string name, string description, string key, string pattern)
        {
            var state = string.IsNullOrWhiteSpace(pattern) ? GamePatchState.NoPattern
                : !(m != null && m.IsProcOpen) ? GamePatchState.Offline
                : !patch.Available ? GamePatchState.Stuck
                : patch.IsOn ? GamePatchState.Active : GamePatchState.Found;
            return new GamePatchRow { Name = name, Description = description, Key = key, State = state, Set = patch.Set };
        }

        private Border GamePatchCard(GamePatchRow row)
        {
            string text, brush;
            switch (row.State)
            {
                case GamePatchState.Active: text = TranslateOr("patches_st_active", "active"); brush = "OkBrush"; break;
                case GamePatchState.Found: text = TranslateOr("patches_st_found", "found"); brush = null; break;
                case GamePatchState.Stuck: text = TranslateOr("patches_st_stuck", "not found – still patched? Restart GTA"); brush = "WarnBrush"; break;
                case GamePatchState.Offline: text = TranslateOr("patches_st_offline", "GTA not connected"); brush = "FaintTextBrush"; break;
                default: text = string.Format(CultureInfo.CurrentCulture, TranslateOr("patches_st_nopattern", "no pattern for {0}"), GameVariant.DisplayName(GameVariant.Current)); brush = "BadBrush"; break;
            }
            // One slim row: state dot, name, switch; the description and state are in the tooltip.
            bool usable = row.State == GamePatchState.Active || row.State == GamePatchState.Found;
            bool on = row.State == GamePatchState.Active;
            var grid = new Grid { ToolTip = row.Description + "\n" + text };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var dot = new System.Windows.Shapes.Ellipse { Width = 8, Height = 8, Margin = new Thickness(0, 0, 9, 0), VerticalAlignment = VerticalAlignment.Center };
            dot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, brush ?? "FaintTextBrush");
            grid.Children.Add(dot);
            var name = new TextBlock { Text = row.Name, FontSize = 13, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Foreground = ThemeBrush("TextColor") };
            Grid.SetColumn(name, 1);
            grid.Children.Add(name);
            var box = new CheckBox { Style = (Style)FindResource("SwitchToggle"), IsChecked = on, IsEnabled = usable, Margin = new Thickness(10, 0, 0, 0) };
            box.Click += (_, __) =>
            {
                row.Set(box.IsChecked == true);
                RememberGamePatch(row.Key, box.IsChecked == true);
                _patchesShownKey = null;
                RenderPatchesPage();
            };
            Grid.SetColumn(box, 2);
            grid.Children.Add(box);
            var body = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 8, 10, 8), Child = grid };
            body.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            return new Border { Margin = new Thickness(0, 0, 8, 8), Child = Framed(body, 8, on) };
        }

        private static void SetDevMode(bool on)
        {
            if (m.IsProcOpen && GTA.Offsets.Editor.dev != 0)
                m.memory(GTA.Offsets.Editor.dev).SetInt(on ? GTA.DevPatched : GTA.DevOriginal);
        }

        // ----- remembered game patches -----

        // The game patch switches are kept in config.ini and written again whenever Xenvious
        // connects, so they survive a GTA or Xenvious restart. A switch that was never touched
        // has no key, and the game keeps what it has. Test Mode is not remembered: it is only
        // meant for one session.
        private const string GamePatchSection = "GamePatches";

        private static Action<bool> RememberedGamePatch(string key)
        {
            switch (key)
            {
                case "devptr": return SetDevMode;
                case "creator_cam_nocollision": return GamePatches.CameraNoCollision.Set;
                case "creator_budget": return GamePatches.NoBudget.Set;
                case "testmode": return GamePatches.TestMode.Set;
                default: return null;
            }
        }

        internal static void RememberGamePatch(string key, bool on)
        {
            if (RememberedGamePatch(key) == null)
                return;
            try
            {
                new ini_reader(Functions.getRoamingConfigFilePath()).Write(GamePatchSection, key, on);
            }
            catch (Exception e)
            {
                Log.Error("Saving game patch " + key + " failed", e, "scrpatches");
            }
        }

        /// <summary>Writes the remembered game patch switches; call once the patches are resolved.</summary>
        internal static void RestoreGamePatches()
        {
            try
            {
                var ini = new ini_reader(Functions.getRoamingConfigFilePath());
                foreach (string key in new[] { "devptr", "creator_cam_nocollision", "creator_budget", "testmode" })
                {
                    if (!bool.TryParse(ini.ReadString(GamePatchSection, key), out bool on))
                        continue;
                    RememberedGamePatch(key)(on);
                    Log.Info($"Game patch {key} {(on ? "on" : "off")} (remembered)", source: "scrpatches");
                }
            }
            catch (Exception e)
            {
                Log.Error("Restoring game patches failed", e, "scrpatches");
            }
        }

        internal string TranslateOr(string key, string fallback)
        {
            if (Translation != null && Translation.TryGetValue(key, out string text))
                return text;
            return fallback;
        }

        /// <summary>
        /// The translated description of a patch: key "scrpatch_" + the name in lower case,
        /// every run of other characters as "_", at most 48 characters. English when the
        /// current language has none, "" when neither has one.
        /// </summary>
        private string ScrPatchDescription(string patchName)
        {
            string slug = Regex.Replace((patchName ?? "").ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
            if (slug.Length > 48)
                slug = slug.Substring(0, 48).TrimEnd('_');
            string key = "scrpatch_" + slug;

            if (Translation != null && Translation.TryGetValue(key, out string text))
                return text;
            return _Language.eng.TryGetValue(key, out text) ? text : "";
        }

        private void BtnScrPatchesRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadScrPatchesPage();
        }

        // A patch was switched on the page. Switching on only clears the way for the
        // runner; switching off has to put the original bytes back, because the runner
        // never touches a patch it already wrote.
        private void OnScrPatchToggled(GTA.ScrPatches patch, bool enabled)
        {
            if (!enabled)
                ScrPatchesRunner.Revert(patch);
            Log.Info($"Patch {(enabled ? "on" : "off")}: {patch.patch_name} [{patch.script_name}]",
                source: "scrpatches");
            Dispatcher.BeginInvoke(new Action(() => { _patchesShownKey = null; RenderPatchesPage(); }));
        }
    }
}
