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

            RenderScrPatchFilter();
            patchesScripts.Children.Clear();
            var visible = _scrPatchGroups
                .Where(g => _scrPatchScript == null || g.Patches.Any(p => p.script_name == _scrPatchScript))
                .Where(g => MatchesPatchQuery(ScrPatchName(g.Name) + " " + g.Description + " " + g.Name))
                .ToList();
            foreach (var (groupKey, fallback) in ScrPatchGroups)
            {
                var items = visible.Where(g => !g.Patches.Any(p => p.dev) && ScrPatchGroupKey(g.Name) == groupKey).ToList();
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

            // Script features: needed by other functions, so they are listed but not switchable.
            var dev = visible.Where(g => g.Patches.Any(p => p.dev)).ToList();
            patchesDevList.Children.Clear();
            foreach (var group in dev)
                patchesDevList.Children.Add(ScrPatchCard(group, creator, readOnly: true));
            patchesDevHeader.Inlines.Clear();
            patchesDevHeader.Inlines.Add(new Run(TranslateOr("patches_internal", "Internal: needed by other functions")));
            patchesDevHeader.Inlines.Add(new Run("  (" + dev.Count + ")") { FontWeight = FontWeights.Normal, Foreground = ThemeBrush("FaintTextBrush") });
            patchesDev.Visibility = dev.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            int on = _scrPatchGroups.Count(g => !g.Patches.Any(p => p.dev) && g.Enabled), total = _scrPatchGroups.Count(g => !g.Patches.Any(p => p.dev));
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
            var scripts = _scrPatchGroups.SelectMany(g => g.Patches).Where(p => !p.dev).Select(p => p.script_name).Where(n => !string.IsNullOrEmpty(n)).Distinct()
                .OrderBy(n => Array.IndexOf(ScrPatchScriptOrder, n) < 0 ? int.MaxValue : Array.IndexOf(ScrPatchScriptOrder, n)).ToList();
            if (_scrPatchScript != null && !scripts.Contains(_scrPatchScript))
                _scrPatchScript = null;
            scrPatchesFilter.Children.Clear();
            int Count(string script) => _scrPatchGroups.Count(g => !g.Patches.Any(p => p.dev) && (script == null || g.Patches.Any(p => p.script_name == script)));
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
        private Border PatchCard(string title, string description, string warning, bool on, bool canSwitch, Action<bool> toggled, IEnumerable<FrameworkElement> chips)
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
            if (!string.IsNullOrEmpty(warning))
            {
                var w = new TextBlock { Text = warning, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
                w.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                text.Children.Add(w);
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

        private Border ScrPatchCard(ScrPatchGroup group, string creator, bool readOnly = false)
        {
            // Per script: green = written into the running creator, yellow = on but not found in
            // it (pattern no longer matches), grey = that creator is not open (applied when opened).
            var chips = new List<FrameworkElement>();
            foreach (var script in group.Patches.Select(p => p.script_name).Where(n => !string.IsNullOrEmpty(n)).Distinct()
                .OrderBy(n => Array.IndexOf(ScrPatchScriptOrder, n) < 0 ? int.MaxValue : Array.IndexOf(ScrPatchScriptOrder, n)))
            {
                var patches = group.Patches.Where(p => p.script_name == script).ToList();
                bool applied = patches.Any(ScrPatchesRunner.IsApplied);
                bool enabled = readOnly || patches.All(p => p.enabled);
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
            if (readOnly)
            {
                var head = new DockPanel();
                var always = StatusChip(TranslateOr("patches_always", "always on"), "OkBrush", group.Description);
                always.Margin = new Thickness(10, 0, 0, 0);
                always.VerticalAlignment = VerticalAlignment.Top;
                DockPanel.SetDock(always, Dock.Right);
                head.Children.Add(always);
                head.Children.Add(new TextBlock { Text = ScrPatchName(group.Name), FontSize = 14.5, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap, Foreground = ThemeBrush("TextColor"), ToolTip = group.Description });
                var body = new StackPanel();
                body.Children.Add(head);
                var foot = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
                foreach (var chip in chips)
                    foot.Children.Add(chip);
                body.Children.Add(foot);
                var card = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(14, 12, 14, 10), Margin = new Thickness(0, 0, 10, 10), BorderThickness = new Thickness(1), Child = body };
                card.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
                card.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                return card;
            }
            return PatchCard(ScrPatchName(group.Name), group.Description, null, group.Enabled, true, on => group.Enabled = on, chips);
        }

        // ----- patches in the game executable -----

        private enum GamePatchState { Active, Found, Stuck, NoPattern, Offline }

        private sealed class GamePatchRow
        {
            public string Name, Description, Warning, Key;
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
                Set = on => { if (m.IsProcOpen && GTA.Offsets.Editor.dev != 0) m.memory(GTA.Offsets.Editor.dev).SetInt(on ? GTA.DevPatched : GTA.DevOriginal); },
            });
            rows.Add(GamePatchRowFor(GamePatches.CameraNoCollision, TranslateOr("np_camnocol", "Creator camera without collision"), TranslateOr("np_camnocol_tip", "The creator camera passes through walls and the ground."), null,
                "creator_cam_nocollision", GTA.Offsets.Editor.AOB_creator_cam_nocollision));
            rows.Add(GamePatchRowFor(GamePatches.NoBudget, TranslateOr("np_nobudget", "Ignore creator budget"), TranslateOr("patches_budget_desc", "The budget bar stays empty."),
                TranslateOr("patches_budget_warn", "Too many entities can make the job fail to save or load."), "creator_budget", GTA.Offsets.Editor.AOB_creator_budget));
            rows.Add(GamePatchRowFor(GamePatches.TestMode, TranslateOr("np_testmode", "Test Mode"), TranslateOr("patches_testmode_desc", "Enables test mode features."),
                TranslateOr("patches_testmode_warn", "Enable Test mode in Creator with BE disabled"), "testmode", GTA.Offsets.Editor.AOB_testmode));
            return rows;
        }

        private GamePatchRow GamePatchRowFor(GamePatch patch, string name, string description, string warning, string key, string pattern)
        {
            var state = string.IsNullOrWhiteSpace(pattern) ? GamePatchState.NoPattern
                : !(m != null && m.IsProcOpen) ? GamePatchState.Offline
                : !patch.Available ? GamePatchState.Stuck
                : patch.IsOn ? GamePatchState.Active : GamePatchState.Found;
            return new GamePatchRow { Name = name, Description = description, Warning = warning, Key = key, State = state, Set = patch.Set };
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
            var key = new TextBlock { Text = row.Key, FontSize = 11, FontFamily = new FontFamily("Consolas"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 0, 4) };
            key.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            bool usable = row.State == GamePatchState.Active || row.State == GamePatchState.Found;
            return PatchCard(row.Name, row.Description, row.Warning, row.State == GamePatchState.Active, usable, row.Set, new FrameworkElement[] { StatusChip(text, brush), key });
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
