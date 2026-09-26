using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xenvious.AdvancedPlacement;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: dashboard parts that depend on the open creator (counts, script features, mission ambient).
    public partial class MainWindow
    {
        private sealed class DashCount
        {
            public string Label;
            public Func<string> Read;
            public Action Open;
            public Brush Dot;
            public TextBlock Value;
        }

        // Script patches that change what a creator lets you do, by patch_name in
        // scrpatches.json. The technical ones (cam fix, nrl fix, templates, custom
        // functions) stay on Misc > Scr Patches only.
        private static readonly (string Patch, string Key, string Fallback)[] DashFeaturePatches =
        {
            ("show stunt prop item cycle", "dash_feat_cycle", "Stunt prop item cycle"),
            ("selected spawn veicle stays after testing ends", "dash_feat_keepveh", "Keep spawn vehicle after test"),
            ("fm_capture_creator 80 actors patch", "dash_feat_actors80", "80 actors"),
            ("dont force plyl to 1", "dash_feat_lives", "Free player lives"),
            ("dont force kill rule to 6 and number to 1", "dash_feat_killrule", "Free kill rule"),
            ("dont force round pa with dev mode", "dash_feat_roundpa", "Free round play area"),
        };

        private string _dashCreator = "";   // creator script the creator-specific parts were built for
        private readonly List<DashCount> _dashCounts = new List<DashCount>();
        private readonly List<(ScrPatchGroup Group, Ellipse Dot, FrameworkElement Row)> _dashFeatures =
            new List<(ScrPatchGroup, Ellipse, FrameworkElement)>();

        private static bool IsMissionCreator(string creator)
        {
            return creator == "fm_lts_creator" || creator == "fm_capture_creator" || creator == "fm_mission_creator";
        }

        /// <summary>
        /// Rebuilds the creator-specific rows when another creator opens and refreshes the
        /// mission-only ambient values. <paramref name="creator"/> is null outside a creator.
        /// </summary>
        private void UpdateDashboardForCreator(string creator)
        {
            creator = creator ?? "";
            if (creator != _dashCreator)
            {
                _dashCreator = creator;
                BuildDashboardCounts(creator);
                BuildDashboardFeatures(creator);
                BuildDashboardTeams(creator);
                bool mission = IsMissionCreator(creator);
                DashMissionAmbient.Visibility = mission ? Visibility.Visible : Visibility.Collapsed;
                DashMissionLook.Visibility = mission ? Visibility.Visible : Visibility.Collapsed;
                // The map rebuild is only verified for race, LTS and capture (see CreatorMap).
                BtnDashReloadMap.IsEnabled = creator == "fm_race_creator" || creator == "fm_lts_creator" || creator == "fm_capture_creator";
            }

            if (MainPages.SelectedItem != PageDashboard || !m.IsProcOpen || creator.Length == 0)
                return;

            foreach (var count in _dashCounts)
                count.Value.Text = SafeRead(count.Read);
            UpdateDashboardFeatureStatus(creator);

            if (IsMissionCreator(creator))
                GetDashboardMissionAmbient();
        }

        private static string SafeRead(Func<string> read)
        {
            try { return read(); }
            catch { return "–"; }
        }

        private static string CountOf(long numberOffset)
        {
            return numberOffset == 0 ? "–" : new Global(numberOffset).Get<int>().ToString(CultureInfo.CurrentCulture);
        }

        // The tile in the status bar: the number that matters most in each creator.
        private void UpdateDashboardCreatorCounts(string creator)
        {
            string label, value;
            switch (creator)
            {
                case "fm_race_creator":
                    label = TranslateOr("checkpoints", "Checkpoints");
                    int checkpoints = new Global(GTA.Offsets.Editor.Race.Checkpoints.number).Get<int>();
                    value = checkpoints.ToString(CultureInfo.CurrentCulture) + " · " + string.Format(CultureInfo.CurrentCulture,
                        TranslateOr("dash_secondary", "{0} secondary"), CountSecondaryCheckpoints(checkpoints));
                    break;
                case "fm_lts_creator":
                    label = TranslateOr("dash_actors", "Actors");
                    value = CountOf(GTA.Offsets.Editor.Actor.number);
                    break;
                case "fm_capture_creator":
                    label = TranslateOr("dash_capobjects", "Capture objects");
                    value = CountOf(GTA.Offsets.Editor.Objects.number);
                    break;
                case "fm_deathmatch_creator":
                    label = TranslateOr("weapons", "Weapons");
                    value = CountOf(GTA.Offsets.Editor.Weapon.number);
                    break;
                case "fm_survival_creator":
                    label = TranslateOr("dash_waves", "Waves");
                    value = CountOf(GTA.Offsets.Editor.Survival.wave);
                    break;
                default:
                    DashTileMode.Visibility = Visibility.Collapsed;
                    return;
            }
            DashModeLabel.Text = label;
            DashModeValue.Text = value;
        }

        // A checkpoint has a secondary one when its secondary position is set.
        private static int CountSecondaryCheckpoints(int checkpoints)
        {
            long next = GTA.Offsets.Editor.Race.Checkpoints.NEXT;
            long secondary = GTA.Offsets.Editor.Race.Checkpoints.sndchk;
            if (secondary == 0 || next == 0)
                return 0;
            int found = 0;
            for (int i = 0; i < checkpoints && i < 100; i++)
            {
                float x = new Global(secondary + 0 + next * i).Get<float>();
                float y = new Global(secondary + 1 + next * i).Get<float>();
                float z = new Global(secondary + 2 + next * i).Get<float>();
                if (x != 0f || y != 0f || z != 0f)
                    found++;
            }
            return found;
        }

        private void OpenEditPage(TabItem page)
        {
            MainPages.SelectedItem = PageEdit;
            EditPages.SelectedItem = page;
        }

        // "Placed in this job": props and dynamic props first, then what the creator adds.
        private void BuildDashboardCounts(string creator)
        {
            _dashCounts.Clear();
            DashCounts.Children.Clear();
            if (creator.Length == 0)
                return;

            var propBrush = (Brush)DashTileProps.FindResource("DashPropBrush");
            var dynamicBrush = (Brush)DashTileProps.FindResource("DashDynamicBrush");

            _dashCounts.Add(new DashCount
            {
                Label = TranslateOr("props", "Props"), Dot = propBrush,
                Read = () => CountOf(GTA.Offsets.Editor.Props.number) + " / " + PropPlacementService.PropLimit.ToString(CultureInfo.CurrentCulture),
                Open = () => { OpenEditPage(PageProps); PageInnerProps.SelectedItem = PageInnerNormalProps; }
            });
            _dashCounts.Add(new DashCount
            {
                Label = TranslateOr("dash_dynprops", "Dynamic Props"), Dot = dynamicBrush,
                Read = () => CountOf(GTA.Offsets.Editor.DProps.number),
                Open = () => { OpenEditPage(PageProps); PageInnerProps.SelectedItem = PageInnerDynamicProps; }
            });

            var actors = new DashCount { Label = TranslateOr("dash_actors", "Actors"), Read = () => CountOf(GTA.Offsets.Editor.Actor.number), Open = () => OpenEditPage(PageActor) };
            var vehicles = new DashCount { Label = TranslateOr("vehicles", "Vehicles"), Read = () => CountOf(GTA.Offsets.Editor.Vehicle.number), Open = () => OpenEditPage(PageVehicle) };
            var weapons = new DashCount { Label = TranslateOr("weapons", "Weapons"), Read = () => CountOf(GTA.Offsets.Editor.Weapon.number), Open = () => OpenEditPage(PageWeapon) };
            var zones = new DashCount { Label = TranslateOr("zones", "Zones"), Read = () => CountOf(GTA.Offsets.Editor.Zones.number), Open = () => OpenEditPage(PageZone) };
            var doors = new DashCount { Label = TranslateOr("doors", "Doors"), Read = () => CountOf(GTA.Offsets.Editor.Doors.number), Open = () => OpenEditPage(PageDoors) };

            switch (creator)
            {
                // Actors and doors exist in every creator except the race creator.
                case "fm_race_creator": _dashCounts.AddRange(new[] { vehicles, weapons, zones }); break;
                case "fm_lts_creator": _dashCounts.AddRange(new[] { actors, doors, vehicles, weapons, zones }); break;
                case "fm_capture_creator": _dashCounts.AddRange(new[] { actors, doors, vehicles, weapons, zones }); break;
                case "fm_deathmatch_creator": _dashCounts.AddRange(new[] { actors, doors, vehicles, zones }); break;
                case "fm_survival_creator": _dashCounts.AddRange(new[] { actors, doors, weapons, vehicles }); break;
                default: _dashCounts.AddRange(new[] { actors, doors, vehicles, weapons, zones }); break;
            }

            foreach (var count in _dashCounts)
                DashCounts.Children.Add(CountTile(count));
        }

        private Border CountTile(DashCount count)
        {
            count.Value = new TextBlock { FontSize = 17, FontWeight = FontWeights.Bold, Text = "–" };
            var label = new StackPanel { Orientation = Orientation.Horizontal };
            if (count.Dot != null)
                label.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = count.Dot, Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });
            label.Children.Add(new TextBlock { Text = count.Label, FontSize = 12, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("NavMutedBrush") });

            var tile = new Border
            {
                Background = (Brush)FindResource("SeactionHeaderBackgroundBrush"),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 7, 10, 7),
                Margin = new Thickness(0, 0, 8, 8),
                MinWidth = 118,
                Cursor = Cursors.Hand,
                Child = new StackPanel { Children = { count.Value, label } }
            };
            tile.MouseEnter += (_, __) => tile.Background = (Brush)FindResource("ButtonHoverBackgroundBrush");
            tile.MouseLeave += (_, __) => tile.Background = (Brush)FindResource("SeactionHeaderBackgroundBrush");
            tile.MouseLeftButtonUp += (_, __) => count.Open?.Invoke();
            return tile;
        }

        // One status row per creator-relevant script patch. Switching stays on Misc >
        // Scr Patches; the dashboard only says whether a feature is working.
        private void BuildDashboardFeatures(string creator)
        {
            DashScriptFeatures.Children.Clear();
            _dashFeatures.Clear();
            DashScriptFeaturesFor.Text = creator.Length == 0 ? "" : CreatorDisplayName(creator);
            if (creator.Length == 0)
            {
                DashScriptFeatures.Children.Add(DashMutedText(TranslateOr("dash_features_nocreator", "Open a creator to see its features.")));
                return;
            }

            if (_scrPatchGroups.Count == 0)
                LoadScrPatchesPage();

            foreach (var (patch, key, fallback) in DashFeaturePatches)
            {
                var group = _scrPatchGroups.FirstOrDefault(g => g.Name == patch);
                if (group == null || !group.Patches.Any(p => p.script_name == creator))
                    continue;

                var dot = new Ellipse { Width = 9, Height = 9, Margin = new Thickness(0, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center, Fill = DotOff };
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 3), Background = Brushes.Transparent };
                row.Children.Add(dot);
                row.Children.Add(new TextBlock { Text = TranslateOr(key, fallback), FontSize = 14, Foreground = (Brush)FindResource("TextColor"), VerticalAlignment = VerticalAlignment.Center });
                DashScriptFeatures.Children.Add(row);
                _dashFeatures.Add((group, dot, row));
            }

            if (_dashFeatures.Count == 0)
                DashScriptFeatures.Children.Add(DashMutedText(TranslateOr("dash_features_none", "No script features for this creator.")));
            UpdateDashboardFeatureStatus(creator);
        }

        private TextBlock DashMutedText(string text)
        {
            return new TextBlock { Text = text, FontSize = 13, TextWrapping = TextWrapping.Wrap, Foreground = (Brush)FindResource("NavMutedBrush") };
        }

        // Green: written into the creator script. Yellow: switched on, but the runner has
        // not found its place in the script (yet, or no longer after a game update).
        // Grey: switched off. The reason sits in the tooltip, not next to the name.
        private void UpdateDashboardFeatureStatus(string creator)
        {
            foreach (var (group, dot, row) in _dashFeatures)
            {
                var patches = group.Patches.Where(p => p.script_name == creator).ToList();
                bool enabled = patches.All(p => p.enabled);
                bool applied = patches.Any(ScrPatchesRunner.IsApplied);
                dot.Fill = !enabled ? DotOff : applied ? DotOk : DotWarn;
                row.ToolTip = !enabled
                    ? TranslateOr("dash_feat_off", "Off. Switch it on in Misc › Script Patches.")
                    : applied
                        ? TranslateOr("dash_feat_on", "Active in this creator.")
                        : TranslateOr("dash_feat_pending", "On, but not in the script yet. If it stays like this, the pattern no longer matches this game build.");
            }
        }

        private void BtnDashScrPatches_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageMod;
            BtnModScrPatches_Click(sender, e);
        }

        // Team to test with. The race creator tests the primary or the secondary route
        // instead (index 0 / 1, see BtnTestMain_Click); the deathmatch creator has no team test.
        private void BuildDashboardTeams(string creator)
        {
            DashTeamButtons.Children.Clear();
            bool race = creator == "fm_race_creator";
            int count = creator == "fm_deathmatch_creator" || creator.Length == 0 ? 0 : race ? 2 : 4;
            DashTeamPanel.Visibility = count == 0 ? Visibility.Collapsed : Visibility.Visible;
            if (ddteamtest.SelectedIndex >= count)
                ddteamtest.SelectedIndex = 0;

            for (int i = 0; i < count; i++)
            {
                int index = i;
                var button = new ToggleButton
                {
                    Content = (i + 1).ToString(CultureInfo.InvariantCulture),
                    Style = (Style)DashTeamPanel.FindResource("DashTeamButton"),
                    IsChecked = ddteamtest.SelectedIndex == i,
                    ToolTip = race ? (i == 0 ? TranslateOr("dash_route_primary", "Primary route") : TranslateOr("dash_route_secondary", "Secondary route")) : null
                };
                button.Click += (_, __) =>
                {
                    ddteamtest.SelectedIndex = index;
                    foreach (ToggleButton other in DashTeamButtons.Children)
                        other.IsChecked = other == button;
                };
                DashTeamButtons.Children.Add(button);
            }
        }

        // The job id in the job card's header: a link once the job is published.
        private void UpdateDashboardJobId(bool inCreator)
        {
            string id = tbjobid.Text?.Trim() ?? "";
            if (!inCreator || id.Length == 0)
            {
                BtnDashJobId.Visibility = Visibility.Collapsed;
                return;
            }

            bool published = GTA.Offsets.Editor.jobpublished != 0 && new Global(GTA.Offsets.Editor.jobpublished).Get<int>() != 0;
            BtnDashJobId.Visibility = Visibility.Visible;
            BtnDashJobId.IsEnabled = published;
            DashJobIdIcon.Visibility = published ? Visibility.Visible : Visibility.Collapsed;
            BtnDashJobId.ToolTip = published
                ? TranslateOr("dash_jobid_open", "Open the job in the Social Club")
                : TranslateOr("dash_jobid_draft", "Not published yet");
            // A disabled button would ignore the tooltip otherwise.
            ToolTipService.SetShowOnDisabled(BtnDashJobId, true);
        }

        private void BtnDashJobId_Click(object sender, RoutedEventArgs e)
        {
            string id = tbjobid.Text?.Trim() ?? "";
            if (id.Length > 0)
                OpenInBrowser("https://socialclub.rockstargames.com/job/gtav/" + Uri.EscapeDataString(id));
        }

        private void BtnDashSC_Click(object sender, RoutedEventArgs e)
        {
            string name = Lbl_SCName.Text?.Trim() ?? "";
            if (name.Length > 0)
                OpenInBrowser("https://socialclub.rockstargames.com/member/" + Uri.EscapeDataString(name) + "/");
        }

        private static void OpenInBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Warn("Could not open " + url + ": " + ex.Message);
            }
        }

        private void BtnDashBackupMap_Click(object sender, RoutedEventArgs e)
        {
            // Saves on the Map Backup page, which shows the result and the list.
            MainPages.SelectedItem = PageMod;
            BtnMapSave_Click(sender, e);
            BtnModMapBackup_Click(sender, e);
        }

        private async void BtnDashReloadMap_Click(object sender, RoutedEventArgs e)
        {
            if (!m.IsProcOpen)
                return;
            BtnDashReloadMap.IsEnabled = false;
            try
            {
                await CreatorMap.RebuildAsync();
            }
            finally
            {
                BtnDashReloadMap.IsEnabled = true;
            }
        }

        // ----- mission-only ambient (LTS, Capture): same values as Mission > General -----

        private void GetDashboardMissionAmbient()
        {
            if (!dddashpolice.IsDropDownOpen)
            {
                int pol = new Global(GTA.Offsets.Editor.pol).Get<int>();
                dddashpolice.SelectedIndex = pol >= 0 && pol < dddashpolice.Items.Count ? pol : -1;
            }
            GetMissionDensity(dddashtraffic, GTA.Offsets.Editor.traf);
            GetMissionDensity(dddashpeds, GTA.Offsets.Editor.apeds);
            Functions.Read.checkbinary(21, GTA.Offsets.Editor.menubs2, cbdashptod);
            GetMissionExtraValues();
        }

        private void dddashpolice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // pol: 0 = normal police, 1 = no police, 2..6 = at most 1..5 stars.
            if (dddashpolice.SelectedIndex > -1 && m.IsProcOpen)
                new Global(GTA.Offsets.Editor.pol).SetInt(dddashpolice.SelectedIndex);
        }

        private void dddashtraffic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
                SetMissionDensity(dddashtraffic, GTA.Offsets.Editor.traf);
        }

        private void dddashpeds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
                SetMissionDensity(dddashpeds, GTA.Offsets.Editor.apeds);
        }

        private void cbdashptod_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(21, GTA.Offsets.Editor.menubs2, cbdashptod);
        }
    }
}
