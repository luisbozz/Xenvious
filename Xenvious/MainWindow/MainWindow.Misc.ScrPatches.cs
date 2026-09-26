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

            // One card per patch name: most patches exist once per creator script and are
            // one feature, so they are shown and switched together. Script features (dev)
            // come after the plain patches.
            var groups = all
                .Where(p => !string.IsNullOrEmpty(p.patch_name))
                .GroupBy(p => p.patch_name)
                .Select(g => new ScrPatchGroup(g.Key, g.ToList(), ScrPatchDescription(g.Key),
                    TranslateOr, OnScrPatchToggled))
                .OrderBy(g => g.Patches.Any(p => p.dev))
                .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _scrPatchGroups = groups;
            BuildScrPatchFilter(all.Select(p => p.script_name).Where(n => !string.IsNullOrEmpty(n)).Distinct());
            ApplyScrPatchFilter();
        }

        private List<ScrPatchGroup> _scrPatchGroups = new List<ScrPatchGroup>();
        private string _scrPatchScript;   // null: all scripts

        private static readonly string[] ScrPatchScriptOrder =
        {
            "fm_race_creator", "fm_lts_creator", "fm_capture_creator", "fm_deathmatch_creator", "fm_survival_creator", "fmmc_launcher"
        };

        private static string ScrPatchScriptLabel(string script)
        {
            switch (script)
            {
                case "fm_race_creator": return "Race";
                case "fm_lts_creator": return "LTS";
                case "fm_capture_creator": return "Capture";
                case "fm_deathmatch_creator": return "Deathmatch";
                case "fm_survival_creator": return "Survival";
                case "fmmc_launcher": return "Launcher";
                default: return script;
            }
        }

        // One chip per script plus "all"; the selected one is filled yellow.
        private void BuildScrPatchFilter(IEnumerable<string> scripts)
        {
            var ordered = scripts
                .OrderBy(n => Array.IndexOf(ScrPatchScriptOrder, n) < 0 ? int.MaxValue : Array.IndexOf(ScrPatchScriptOrder, n))
                .ThenBy(n => n)
                .ToList();
            if (_scrPatchScript != null && !ordered.Contains(_scrPatchScript))
                _scrPatchScript = null;

            scrPatchesFilter.Children.Clear();
            scrPatchesFilter.Children.Add(ScrPatchFilterChip(TranslateOr("scrpatch_filter_all", "Alle"), null));
            foreach (string script in ordered)
                scrPatchesFilter.Children.Add(ScrPatchFilterChip(ScrPatchScriptLabel(script), script));
        }

        private Border ScrPatchFilterChip(string label, string script)
        {
            bool selected = _scrPatchScript == script;
            var yellow = Color.FromRgb(0xFA, 0xC8, 0x28);
            // No outline: a 1 px border on a rounded chip renders blurry at non-100 % scaling.
            var chip = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12, 4, 12, 4),
                Margin = new Thickness(0, 0, 6, 6),
                Cursor = Cursors.Hand,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                Background = new SolidColorBrush(selected ? yellow : Color.FromRgb(0x2B, 0x2D, 0x31)),
                Child = new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = selected ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = new SolidColorBrush(selected ? Color.FromRgb(0x20, 0x22, 0x25) : yellow)
                }
            };
            chip.MouseLeftButtonUp += (_, __) =>
            {
                _scrPatchScript = script;
                BuildScrPatchFilter(_scrPatchGroups.SelectMany(g => g.Patches).Select(p => p.script_name)
                    .Where(n => !string.IsNullOrEmpty(n)).Distinct());
                ApplyScrPatchFilter();
            };
            return chip;
        }

        private void ApplyScrPatchFilter()
        {
            scrPatchesList.ItemsSource = _scrPatchScript == null
                ? _scrPatchGroups
                : _scrPatchGroups.Where(g => g.Patches.Any(p => p.script_name == _scrPatchScript)).ToList();
            UpdateScrPatchesInfo();
        }

        private void UpdateScrPatchesInfo()
        {
            if (!(scrPatchesList.ItemsSource is IEnumerable<ScrPatchGroup> groups))
                return;
            tbscrpatchesinfo.Text = string.Format(CultureInfo.CurrentCulture,
                TranslateOr("scrpatch_info", "{0} Patches, {1} aktiv"),
                groups.Count(), groups.Count(g => g.Enabled));
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
            Dispatcher.BeginInvoke(new Action(UpdateScrPatchesInfo));
        }
    }
}
