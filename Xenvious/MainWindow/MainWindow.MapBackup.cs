using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>Misc → Map Backup: save the placed map of the current job and put it back.</summary>
    public partial class MainWindow
    {
        /// <summary>One saved map in the list.</summary>
        public sealed class MapBackupEntry
        {
            public string File { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
        }

        private void BtnModMapBackup_Click(object sender, RoutedEventArgs e)
        {
            LoadMapBackupList();
            PageInnerMod.SelectedItem = PageInnerModMapBackup;
        }

        private void LoadMapBackupList()
        {
            var entries = new List<MapBackupEntry>();
            try
            {
                if (Directory.Exists(CreatorMap.MapFolder))
                {
                    // Safety copies of an earlier version, one per load; the single undo
                    // file replaces them.
                    foreach (string old in Directory.GetFiles(CreatorMap.MapFolder, "_before-restore *" + CreatorMap.FileExtension))
                    {
                        try { File.Delete(old); } catch { }
                    }

                    foreach (string file in Directory.GetFiles(CreatorMap.MapFolder, "*" + CreatorMap.FileExtension)
                                 .Where(f => !Path.GetFileName(f).StartsWith("_"))
                                 .OrderByDescending(File.GetLastWriteTime))
                    {
                        try
                        {
                            var snap = CreatorMap.Load(file);
                            entries.Add(new MapBackupEntry
                            {
                                File = file,
                                Title = string.IsNullOrWhiteSpace(snap.Name) ? Path.GetFileNameWithoutExtension(file) : snap.Name,
                                Subtitle = DescribeSnapshot(snap)
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Warn($"map backup unreadable: {file}", ex, source: "mapbackup");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("map backup list", ex, source: "mapbackup");
            }
            mapBackupList.ItemsSource = entries;
            BtnMapUndoContainer.Visibility = File.Exists(UndoFile) ? Visibility.Visible : Visibility.Collapsed;
        }

        // What the job looked like before the last load; one file, overwritten each time.
        private static string UndoFile => Path.Combine(CreatorMap.MapFolder, "_undo" + CreatorMap.FileExtension);

        private string DescribeSnapshot(CreatorMap.Snapshot snap)
        {
            var parts = new List<string>
            {
                snap.SavedAt.ToString("g", CultureInfo.CurrentCulture),
                CreatorDisplayName(snap.Creator),
                $"Props {snap.CountOf("props")}"
            };
            if (snap.CountOf("dprops") > 0 && snap.Version >= 2) parts.Add($"Dynamic {snap.CountOf("dprops")}");
            if (snap.CountOf("checkpoints") > 0) parts.Add($"Checkpoints {snap.CountOf("checkpoints")}");
            if (snap.CountOf("templates") > 0) parts.Add($"Templates {snap.CountOf("templates")}");
            parts.Add($"{snap.Edition} {snap.Build}".Trim());
            return string.Join(" · ", parts);
        }

        private bool MapBackupReady()
        {
            if (!m.IsProcOpen || !IsCreatorRunning())
            {
                tbMapStatus.Text = TranslateOr("map_need_creator", "Dafür muss ein Creator laufen.");
                return false;
            }
            return true;
        }

        /// <summary>Saves the open job's map under its name and the time; returns the snapshot.</summary>
        private CreatorMap.Snapshot SaveCurrentMap()
        {
            string name = new Global(GTA.Offsets.Editor.nm).GetString()?.Trim();
            if (string.IsNullOrEmpty(name))
                name = CreatorDisplayName(CreatorMap.CurrentCreator());
            var snap = CreatorMap.Capture(name);
            string safe = Regex.Replace(name, @"[^\w\- ]+", "").Trim();
            if (safe.Length == 0) safe = "map";
            if (safe.Length > 40) safe = safe.Substring(0, 40);
            string file = Path.Combine(CreatorMap.MapFolder,
                $"{safe} {DateTime.Now:yyyy-MM-dd HH-mm-ss}{CreatorMap.FileExtension}");
            CreatorMap.Save(snap, file);
            Log.Info($"map saved: {file}", source: "mapbackup");
            LoadMapBackupList();
            return snap;
        }

        private void BtnMapSave_Click(object sender, RoutedEventArgs e)
        {
            if (!MapBackupReady())
                return;
            try
            {
                var snap = SaveCurrentMap();
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("map_saved", "Gesichert: {0}"), DescribeSnapshot(snap));
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("map_save_failed", "Sichern fehlgeschlagen.") + " " + ex.Message;
                Log.Error("map save", ex, "mapbackup");
            }
        }

        private void BtnMapOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = CreatorMap.FileFilter,
                InitialDirectory = Directory.Exists(CreatorMap.MapFolder) ? CreatorMap.MapFolder : null
            };
            if (dialog.ShowDialog(this) == true)
                RestoreMapFile(dialog.FileName);
        }

        private void BtnMapFolder_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(CreatorMap.MapFolder);
            Process.Start("explorer.exe", CreatorMap.MapFolder);
        }

        private void BtnMapRestoreEntry_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is MapBackupEntry entry)
                RestoreMapFile(entry.File);
        }

        private async void BtnMapDeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.Tag is MapBackupEntry entry))
                return;
            bool confirmed = await ConfirmAsync(
                string.Format(CultureInfo.CurrentCulture, TranslateOr("map_delete_confirm", "„{0}“ löschen?"), entry.Title),
                TranslateOr("map_delete_text", "Die Sicherung wird endgültig gelöscht."),
                TranslateOr("map_delete", "Löschen"), TranslateOr("dialog_cancel", "Abbrechen"), danger: true);
            if (!confirmed)
                return;
            try
            {
                File.Delete(entry.File);
            }
            catch (Exception ex)
            {
                Log.Warn($"map delete: {entry.File}", ex, source: "mapbackup");
            }
            LoadMapBackupList();
        }

        private async void BtnMapUndo_Click(object sender, RoutedEventArgs e)
        {
            if (!MapBackupReady() || !File.Exists(UndoFile))
                return;
            try
            {
                var snap = CreatorMap.Load(UndoFile);
                var all = new HashSet<string>(snap.Sections.Select(x => x.Name));
                bool rebuilt = await CreatorMap.RestoreAsync(snap, all);
                File.Delete(UndoFile);
                tbMapStatus.Text = rebuilt
                    ? TranslateOr("map_undone", "Letztes Laden rückgängig gemacht.")
                    : TranslateOr("map_restored_norebuild", "Daten geladen, aber der Creator hat die Map nicht neu aufgebaut.");
                Log.Info("map restore undone", source: "mapbackup");
                LoadMapBackupList();
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("map_restore_failed", "Laden fehlgeschlagen.") + " " + ex.Message;
                Log.Error("map undo", ex, "mapbackup");
            }
        }

        private async void RestoreMapFile(string file)
        {
            if (!MapBackupReady())
                return;
            try
            {
                var snap = CreatorMap.Load(file);
                string problem = CreatorMap.CheckCompatible(snap);
                if (problem != null)
                {
                    tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                        TranslateOr("map_incompatible", "Diese Sicherung stammt von {0} {1} und passt nicht zur laufenden Spielversion."),
                        snap.Edition, snap.Build);
                    return;
                }

                var sections = new HashSet<string>();
                if (cbMapProps.IsChecked == true) sections.Add("props");
                if (cbMapDProps.IsChecked == true) sections.Add("dprops");
                if (cbMapCheckpoints.IsChecked == true) sections.Add("checkpoints");
                if (cbMapTemplates.IsChecked == true) sections.Add("templates");
                if (sections.Count == 0)
                    return;

                bool confirmed = await ConfirmAsync(
                    string.Format(CultureInfo.CurrentCulture, TranslateOr("map_restore_title", "„{0}“ laden?"), snap.Name),
                    TranslateOr("map_restore_text", "Die gewählten Teile des aktuellen Jobs werden ersetzt. Mit „Rückgängig“ holst du den jetzigen Stand zurück."),
                    TranslateOr("map_load", "Laden"), TranslateOr("dialog_cancel", "Abbrechen"));
                if (!confirmed)
                    return;

                // The current state first, so a wrong click can be undone.
                CreatorMap.Save(CreatorMap.Capture(TranslateOr("map_before_restore", "Vor dem Laden")), UndoFile);

                bool rebuilt = await CreatorMap.RestoreAsync(snap, sections);
                tbMapStatus.Text = rebuilt
                    ? string.Format(CultureInfo.CurrentCulture, TranslateOr("map_restored", "Geladen: {0}"), snap.Name)
                    : TranslateOr("map_restored_norebuild", "Daten geladen, aber der Creator hat die Map nicht neu aufgebaut (nur im Bearbeiten-Modus von Race, LTS und Capture möglich). Einmal testen oder den Job neu laden zeigt sie an.");
                Log.Info($"map restored: {file} (rebuild {(rebuilt ? "ok" : "not done")})", source: "mapbackup");
                LoadMapBackupList();
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("map_restore_failed", "Laden fehlgeschlagen.") + " " + ex.Message;
                Log.Error("map restore", ex, "mapbackup");
            }
        }
    }
}
