using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: Misc → Map Backup, map backups as JSON (JobParts), whole job files (JobDatafile) and old raw backups (CreatorMap).
    public partial class MainWindow
    {
        private enum MapEntryKind { Parts, Job, Old }

        /// <summary>One file in the backup list.</summary>
        private sealed class MapEntry
        {
            public string File;
            public MapEntryKind Kind;
            public string Name;
            public string Creator;
            public DateTime SavedAt;
            public Dictionary<string, int> Counts = new Dictionary<string, int>();
            public JObject Data;                    // the backup file, or the whole job
            public CreatorMap.Snapshot Snapshot;    // an old raw backup
            public string Problem;                  // old backups: why it does not fit the running game

            public JObject PartData(string key) =>
                Kind == MapEntryKind.Parts ? JobParts.PartOf(Data, key)
                : Kind == MapEntryKind.Job ? JobParts.Extract(Data, key)
                : null;
        }

        // The sections of an old raw backup by part key.
        private static readonly Dictionary<string, string> OldSections = new Dictionary<string, string>
        {
            ["prop"] = "props", ["dprop"] = "dprops", ["cp"] = "checkpoints", ["tpl"] = "templates"
        };

        private static string JobFileFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "jobs");

        // The job before the last "put in", one file each, overwritten each time: the whole
        // job for JSON backups, the raw arrays for old ones.
        private static string JsonUndoFile => Path.Combine(CreatorMap.MapFolder, "_undo" + JobParts.FileExtension);
        private static string UndoFile => Path.Combine(CreatorMap.MapFolder, "_undo" + CreatorMap.FileExtension);

        private bool mapBusy;

        private string PartLabel(string key)
        {
            switch (key)
            {
                case "prop": return TranslateOr("map_part_prop", "Props");
                case "dprop": return TranslateOr("map_part_dprop", "Dynamic Props");
                case "cp": return TranslateOr("map_part_cp", "Checkpoints");
                default: return TranslateOr("map_part_tpl", "Templates");
            }
        }

        private void SetMapBusy(bool busy)
        {
            mapBusy = busy;
            BtnMapSave.IsEnabled = !busy;
            BtnJobExport.IsEnabled = !busy;
            BtnMapOpen.IsEnabled = !busy;
            BtnMapUndo.IsEnabled = !busy;
            mapBackupList.IsEnabled = !busy;
        }

        private bool MapBackupReady()
        {
            if (mapBusy)
                return false;
            if (!m.IsProcOpen || !IsInCreator())
            {
                tbMapStatus.Text = TranslateOr("map_need_creator", "Dafür muss ein Creator laufen.");
                return false;
            }
            return true;
        }

        private string JobFileMessage(JobDatafile.Result result)
        {
            switch (result)
            {
                case JobDatafile.Result.Unsupported:
                    return TranslateOr("job_file_unsupported", "Geht nur im Race-, Deathmatch-, LTS-, Capture- und Survival-Creator dieser Spielversion.");
                case JobDatafile.Result.NotEditing:
                    return TranslateOr("job_file_not_editing", "Der Creator muss im Bearbeiten-Modus sein (kein Menü offen, kein Test).");
                case JobDatafile.Result.NoTitle:
                    return TranslateOr("job_file_no_title", "Der Job braucht zuerst einen Titel und eine Beschreibung.");
                case JobDatafile.Result.NoJob:
                    return TranslateOr("job_file_no_job", "Der Creator hat keine Job-Datei gebaut.");
                case JobDatafile.Result.TimedOut:
                    return TranslateOr("job_file_timeout", "Der Creator ist nicht in den Bearbeiten-Modus zurückgekehrt.");
                default:
                    return TranslateOr("job_file_failed", "Fehlgeschlagen.");
            }
        }

        private static string SafeFileName(string name, string fallback)
        {
            string safe = Regex.Replace(name ?? "", @"[^\w\- ]+", "").Trim();
            if (safe.Length == 0) safe = fallback;
            return safe.Length > 40 ? safe.Substring(0, 40) : safe;
        }

        // ---- list ---------------------------------------------------------------------------

        private void BtnModMapBackup_Click(object sender, RoutedEventArgs e)
        {
            LoadMapBackupList();
            PageInnerMod.SelectedItem = PageInnerModMapBackup;
        }

        private static MapEntry LoadMapEntry(string file)
        {
            if (file.EndsWith(CreatorMap.FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                var snap = CreatorMap.Load(file);
                var entry = new MapEntry
                {
                    File = file, Kind = MapEntryKind.Old, Snapshot = snap, Creator = snap.Creator, SavedAt = snap.SavedAt,
                    Name = string.IsNullOrWhiteSpace(snap.Name) ? Path.GetFileNameWithoutExtension(file) : snap.Name,
                    Problem = m.IsProcOpen ? CreatorMap.CheckCompatible(snap) : null
                };
                foreach (var pair in OldSections)
                    entry.Counts[pair.Key] = pair.Key == "dprop" && snap.Version < 2 ? 0 : snap.CountOf(pair.Value);
                return entry;
            }

            string text = File.ReadAllText(file);
            var parts = JobParts.ParseFile(text);
            if (parts != null)
            {
                var entry = new MapEntry
                {
                    File = file, Kind = MapEntryKind.Parts, Data = parts, Creator = (string)parts["creator"],
                    Name = (string)parts["name"] ?? Path.GetFileNameWithoutExtension(file),
                    SavedAt = DateTime.TryParse((string)parts["savedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at)
                        ? at : File.GetLastWriteTime(file)
                };
                foreach (var part in JobParts.All)
                    entry.Counts[part.Key] = JobParts.PartOf(parts, part.Key) is JObject data && data[part.CountKey]?.Type == JTokenType.Integer
                        ? (int)data[part.CountKey] : 0;
                return entry;
            }

            var job = JobDatafile.Parse(text);
            if (job == null)
                return null;
            var whole = new MapEntry
            {
                File = file, Kind = MapEntryKind.Job, Data = job, Creator = JobDatafile.CreatorOf(job),
                Name = (string)job["mission"]?["gen"]?["nm"] ?? Path.GetFileNameWithoutExtension(file),
                SavedAt = File.GetLastWriteTime(file)
            };
            foreach (var part in JobParts.All)
                whole.Counts[part.Key] = JobParts.Count(job, part.Key);
            return whole;
        }

        private void LoadMapBackupList()
        {
            var entries = new List<MapEntry>();
            void Add(string folder, string pattern)
            {
                if (!Directory.Exists(folder))
                    return;
                foreach (string file in Directory.GetFiles(folder, pattern).Where(f => !Path.GetFileName(f).StartsWith("_")))
                {
                    try
                    {
                        var entry = LoadMapEntry(file);
                        if (entry != null)
                            entries.Add(entry);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"map backup unreadable: {file}", ex, source: "mapbackup");
                    }
                }
            }
            try
            {
                Add(CreatorMap.MapFolder, "*" + JobParts.FileExtension);
                Add(CreatorMap.MapFolder, "*" + CreatorMap.FileExtension);
                Add(JobFileFolder, "*.json");
            }
            catch (Exception ex)
            {
                Log.Warn("map backup list", ex, source: "mapbackup");
            }

            mapBackupList.Children.Clear();
            foreach (var entry in entries.OrderByDescending(x => x.SavedAt))
                mapBackupList.Children.Add(MapEntryCard(entry));
            tbMapEmpty.Visibility = entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            BtnMapUndo.Visibility = File.Exists(JsonUndoFile) || File.Exists(UndoFile) ? Visibility.Visible : Visibility.Collapsed;
        }

        private static SolidColorBrush Tint(string themeBrush, byte alpha)
        {
            var color = ThemeBrush(themeBrush) is SolidColorBrush b ? b.Color : Colors.Gray;
            return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
        }

        private static TextBlock MapText(string text, double size, string brush, bool bold = false)
        {
            var block = new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap };
            if (bold)
                block.FontWeight = FontWeights.Bold;
            block.SetResourceReference(TextBlock.ForegroundProperty, brush);
            return block;
        }

        private Button MapButton(string text, bool primary, RoutedEventHandler click, double size = 13, double height = 28)
        {
            var button = new Button
            {
                Content = text, FontSize = size, Height = height, Padding = new Thickness(12, 0, 12, 0),
                Style = (Style)FindResource("FormButton")
            };
            if (primary)
            {
                button.SetResourceReference(BackgroundProperty, "HighlightBrush");
                button.SetResourceReference(ForegroundProperty, "HighlightForeground");
            }
            button.Click += click;
            return button;
        }

        private static void Explain(FrameworkElement element, string why)
        {
            element.ToolTip = why;
            ToolTipService.SetShowOnDisabled(element, true);
        }

        private UIElement MapEntryCard(MapEntry entry)
        {
            bool unfit = entry.Kind == MapEntryKind.Old && entry.Problem != null;
            string creator = m.IsProcOpen && IsInCreator() ? CreatorMap.CurrentCreator() : null;

            var head = new TextBlock { TextWrapping = TextWrapping.Wrap };
            head.Inlines.Add(new Run(entry.Name) { FontSize = 15, FontWeight = FontWeights.Bold });
            head.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            string tagText, tagBrush;
            switch (entry.Kind)
            {
                case MapEntryKind.Parts: tagText = TranslateOr("map_tag_json", "JSON"); tagBrush = "OkBrush"; break;
                case MapEntryKind.Job: tagText = TranslateOr("map_tag_job", "ganzer Job"); tagBrush = "AccentBrush"; break;
                default: tagText = TranslateOr("map_tag_old", "altes Format"); tagBrush = "WarnBrush"; break;
            }
            var tag = new Border
            {
                CornerRadius = new CornerRadius(4), Padding = new Thickness(7, 1, 7, 1), Margin = new Thickness(8, 0, 0, 0),
                Background = Tint(tagBrush, 0x22), VerticalAlignment = VerticalAlignment.Center,
                Child = MapText(tagText, 11, tagBrush, bold: true)
            };
            var title = new WrapPanel();
            title.Children.Add(head);
            title.Children.Add(tag);

            string meta = $"{CreatorKindName(entry.Creator)} · ";
            meta += entry.Kind == MapEntryKind.Old
                ? $"{entry.Snapshot.Edition} Build {GTA.BuildNumber(entry.Snapshot.Build)}"
                : entry.SavedAt.ToString("g", CultureInfo.CurrentCulture);
            if (unfit)
                meta += " · " + TranslateOr("map_old_build", "passt nur zu diesem Build");
            var metaText = MapText(meta, 12.5, "MutedTextBrush");
            metaText.Margin = new Thickness(0, 2, 0, 0);

            var chips = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
            foreach (var part in JobParts.All)
            {
                int count = entry.Counts.TryGetValue(part.Key, out int n) ? n : 0;
                bool off = count == 0 || (creator != null && !part.AppliesTo(creator)) || (creator == null && !part.AppliesTo(entry.Creator));
                var text = new TextBlock { FontSize = 12 };
                text.Inlines.Add(new Run(PartLabel(part.Key) + " ") { Foreground = ThemeBrush("MutedTextBrush") });
                text.Inlines.Add(new Run(count.ToString(CultureInfo.InvariantCulture)) { FontWeight = FontWeights.Bold, Foreground = ThemeBrush("TextColor") });
                var chip = new Border
                {
                    CornerRadius = new CornerRadius(10), Padding = new Thickness(9, 2, 9, 2), Margin = new Thickness(0, 0, 6, 4),
                    Opacity = off ? 0.4 : 1, Child = text
                };
                chip.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                chips.Children.Add(chip);
            }

            var left = new StackPanel { Opacity = unfit ? 0.5 : 1 };
            left.Children.Add(title);
            left.Children.Add(metaText);
            left.Children.Add(chips);

            var actions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            if (entry.Kind == MapEntryKind.Job)
                actions.Children.Add(MapButton(TranslateOr("map_load_whole", "Komplett laden …"), true, (s, e) => LoadWholeJob(entry, true)));
            var putIn = MapButton(TranslateOr("map_put_in", "Einspielen …"), entry.Kind != MapEntryKind.Job, (s, e) => PutInMapEntry(entry));
            actions.Children.Add(putIn);
            if (entry.Kind == MapEntryKind.Old)
            {
                var convert = MapButton(TranslateOr("map_convert", "In JSON umwandeln"), false, (s, e) => ConvertOldMapEntry(entry));
                Explain(convert, string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("map_convert_tip", "Spielt die alte Sicherung ein, speichert sie als JSON und stellt den Job wieder her. Geht nur in {0} Build {1}."),
                    entry.Snapshot.Edition, GTA.BuildNumber(entry.Snapshot.Build)));
                if (unfit)
                {
                    putIn.IsEnabled = false;
                    convert.IsEnabled = false;
                    Explain(putIn, TranslateOr("map_old_unfit", "Andere Spielversion: die Rohdaten passen nicht."));
                }
                actions.Children.Add(convert);
            }
            actions.Children.Add(MapButton(TranslateOr("map_rename", "Umbenennen"), false, (s, e) => RenameMapEntry(entry)));
            actions.Children.Add(MapButton(TranslateOr("map_delete", "Löschen"), false, (s, e) => DeleteMapEntry(entry)));
            foreach (FrameworkElement button in actions.Children)
                button.Margin = new Thickness(6, 0, 0, 6);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MaxWidth = 420 });
            grid.Children.Add(left);
            Grid.SetColumn(actions, 1);
            grid.Children.Add(actions);

            var card = new Border
            {
                CornerRadius = new CornerRadius(6), Padding = new Thickness(14, 12, 14, 12), Margin = new Thickness(0, 0, 0, 8),
                Child = grid, ToolTip = entry.File
            };
            card.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            ToolTipService.SetInitialShowDelay(card, 1500);
            return card;
        }

        // ---- save, export, open --------------------------------------------------------------

        /// <summary>
        /// Saves the open job's parts as a JSON backup and returns what was saved. When the
        /// creator cannot build its job file (no title yet, a menu open), the raw arrays are
        /// saved instead, which only fit this build.
        /// </summary>
        private async Task<string> SaveCurrentMapAsync(string name)
        {
            string creator = CreatorMap.CurrentCreator();
            if (string.IsNullOrWhiteSpace(name))
                name = new Global(GTA.Offsets.Editor.nm).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                name = CreatorDisplayName(creator);
            string stem = Path.Combine(CreatorMap.MapFolder, $"{SafeFileName(name, "map")} {DateTime.Now:yyyy-MM-dd HH-mm-ss}");

            var result = JobDatafile.Result.Unsupported;
            if (JobDatafile.CanExport(creator))
            {
                tbMapStatus.Text = TranslateOr("job_file_exporting", "Der Creator baut die Job-Datei …");
                JObject job;
                (result, job) = await JobDatafile.ExportAsync();
                if (result == JobDatafile.Result.Done)
                {
                    var file = JobParts.CreateFile(job, name, creator);
                    JobParts.Save(file, stem + JobParts.FileExtension);
                    Log.Info($"map saved: {stem}{JobParts.FileExtension}", source: "mapbackup");
                    LoadMapBackupList();
                    var counts = JobParts.All.Where(p => file["counts"]?[p.Key] != null)
                        .Select(p => $"{(int)file["counts"][p.Key]} {PartLabel(p.Key)}");
                    return string.Format(CultureInfo.CurrentCulture, TranslateOr("map_saved", "Gesichert: {0}"),
                        $"{name} ({string.Join(", ", counts)})");
                }
            }

            var snap = CreatorMap.Capture(name);
            CreatorMap.Save(snap, stem + CreatorMap.FileExtension);
            Log.Info($"map saved in the old format ({result}): {stem}{CreatorMap.FileExtension}", source: "mapbackup");
            LoadMapBackupList();
            return string.Format(CultureInfo.CurrentCulture,
                TranslateOr("map_saved_old", "Im alten Format gesichert (passt nur zu diesem Build): {0}"), JobFileMessage(result));
        }

        /// <summary>The dialog with a text box; null when cancelled.</summary>
        private async Task<string> AskTextAsync(string title, string text, string initial, string confirm)
        {
            var box = new TextBox { Text = initial ?? "", Height = 34, FontSize = 14, Padding = new Thickness(6, 0, 6, 0), Style = (Style)FindResource("Watermark") };
            var closed = ShowContentAsync(title, text, box, confirm, TranslateOr("dialog_cancel", "Abbrechen"), 460);
            await Dispatcher.BeginInvoke(new Action(() => { box.Focus(); box.SelectAll(); }), System.Windows.Threading.DispatcherPriority.Input);
            return await closed ? box.Text.Trim() : null;
        }

        private async void BtnMapSave_Click(object sender, RoutedEventArgs e)
        {
            if (!MapBackupReady())
                return;
            string name = await AskTextAsync(TranslateOr("map_save_title", "Teile sichern"),
                TranslateOr("map_save_text", "Speichert Props, Dynamic Props, Checkpoints und Templates des offenen Jobs."),
                new Global(GTA.Offsets.Editor.nm).GetString()?.Trim(), TranslateOr("map_save_confirm", "Sichern"));
            if (name == null || !MapBackupReady())
                return;
            SetMapBusy(true);
            try
            {
                tbMapStatus.Text = await SaveCurrentMapAsync(name);
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("map_save_failed", "Sichern fehlgeschlagen.") + " " + ex.Message;
                Log.Error("map save", ex, "mapbackup");
            }
            finally
            {
                SetMapBusy(false);
            }
        }

        private async void BtnJobExport_Click(object sender, RoutedEventArgs e)
        {
            if (!MapBackupReady())
                return;
            string title = new Global(GTA.Offsets.Editor.nm).GetString()?.Trim();
            Directory.CreateDirectory(JobFileFolder);
            var dialog = new SaveFileDialog
            {
                Filter = "Job file (*.json)|*.json",
                InitialDirectory = JobFileFolder,
                FileName = SafeFileName(title, "job") + ".json"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            SetMapBusy(true);
            tbMapStatus.Text = TranslateOr("job_file_exporting", "Der Creator baut die Job-Datei …");
            try
            {
                var (result, job) = await JobDatafile.ExportAsync();
                if (result != JobDatafile.Result.Done)
                {
                    tbMapStatus.Text = JobFileMessage(result);
                    return;
                }
                File.WriteAllText(dialog.FileName, JobDatafile.ToText(job));
                long size = new FileInfo(dialog.FileName).Length;
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("job_file_exported", "Exportiert: {0} ({1} KB)"), dialog.FileName, (size + 1023) / 1024);
                Log.Info($"job file exported: {dialog.FileName}", source: "jobfile");
                LoadMapBackupList();
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("job_file_failed", "Fehlgeschlagen.") + " " + ex.Message;
                Log.Error("job file export", ex, "jobfile");
            }
            finally
            {
                SetMapBusy(false);
            }
        }

        private async void BtnMapOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Xenvious (*.json;*.xvmap)|*.json;*.xvmap",
                InitialDirectory = Directory.Exists(CreatorMap.MapFolder) ? CreatorMap.MapFolder : null
            };
            if (dialog.ShowDialog(this) != true)
                return;
            MapEntry entry = null;
            try
            {
                entry = LoadMapEntry(dialog.FileName);
            }
            catch (Exception ex)
            {
                Log.Warn($"map file unreadable: {dialog.FileName}", ex, source: "mapbackup");
            }
            if (entry == null)
            {
                tbMapStatus.Text = TranslateOr("map_not_backup", "Das ist keine Sicherung und keine Job-Datei.");
                return;
            }
            if (entry.Kind != MapEntryKind.Job)
            {
                PutInMapEntry(entry);
                return;
            }
            var choice = await ChooseAsync(
                string.Format(CultureInfo.CurrentCulture, TranslateOr("map_load_whole_title", "„{0}“ komplett laden?"), entry.Name),
                TranslateOr("map_load_whole_text", "Der offene Job wird komplett ersetzt. Speichern legt danach einen neuen Job an. „Rückgängig“ holt den jetzigen Stand zurück."),
                TranslateOr("map_load_whole_confirm", "Komplett laden"), TranslateOr("map_parts_instead", "Nur Teile einspielen …"),
                TranslateOr("dialog_cancel", "Abbrechen"));
            if (choice == DialogChoice.Confirm)
                LoadWholeJob(entry, false);
            else if (choice == DialogChoice.Alternative)
                PutInMapEntry(entry);
        }

        /// <summary>
        /// Loads a whole job file in place of the open job. Saving afterwards creates a new job;
        /// the open job goes to the undo file first.
        /// </summary>
        private async void LoadWholeJob(MapEntry entry, bool confirm)
        {
            if (!MapBackupReady())
                return;
            string creator = CreatorMap.CurrentCreator();
            if (!JobDatafile.CanImport(creator))
            {
                tbMapStatus.Text = JobFileMessage(JobDatafile.Result.Unsupported);
                return;
            }
            if (entry.Creator != creator)
            {
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("map_wrong_creator", "Diese Datei gehört in den {0}."),
                    entry.Creator == null ? "?" : CreatorDisplayName(entry.Creator));
                return;
            }
            if (confirm && !await ConfirmAsync(
                    string.Format(CultureInfo.CurrentCulture, TranslateOr("map_load_whole_title", "„{0}“ komplett laden?"), entry.Name),
                    TranslateOr("map_load_whole_text", "Der offene Job wird komplett ersetzt. Speichern legt danach einen neuen Job an. „Rückgängig“ holt den jetzigen Stand zurück."),
                    TranslateOr("map_load_whole_confirm", "Komplett laden"), TranslateOr("dialog_cancel", "Abbrechen")))
                return;
            if (!MapBackupReady())
                return;

            SetMapBusy(true);
            try
            {
                // The open job for "Undo"; without a job file (no title yet) there is none.
                tbMapStatus.Text = TranslateOr("job_file_exporting", "Der Creator baut die Job-Datei …");
                if (JobDatafile.CanExport(creator))
                {
                    var (exported, current) = await JobDatafile.ExportAsync();
                    if (exported == JobDatafile.Result.Done)
                        SaveJsonUndo(current);
                    else
                        Log.Info($"whole job load: no undo, export {exported}", source: "mapbackup");
                }

                tbMapStatus.Text = TranslateOr("job_file_importing", "Der Creator lädt den Job …");
                var result = await JobDatafile.ImportAsync(entry.Data);
                if (result != JobDatafile.Result.Done)
                {
                    tbMapStatus.Text = JobFileMessage(result);
                    return;
                }
                ApplyJobFileTexts(entry.Data);
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("map_loaded_whole", "Geladen: {0}. Speichern legt einen neuen Job an."), entry.Name);
                Log.Info($"whole job loaded: {entry.File}", source: "mapbackup");
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("map_restore_failed", "Laden fehlgeschlagen.") + " " + ex.Message;
                Log.Error("whole job load", ex, "mapbackup");
            }
            finally
            {
                SetMapBusy(false);
                LoadMapBackupList();
            }
        }

        private static void SaveJsonUndo(JObject job)
        {
            var identity = JobDatafile.Identity();
            var undo = new JObject
            {
                ["format"] = "xenvious-undo",
                ["creator"] = CreatorMap.CurrentCreator(),
                ["id"] = identity.Id,
                ["updates"] = identity.Updates,
                ["job"] = job.DeepClone()
            };
            JobParts.Save(undo, JsonUndoFile);
        }

        private void BtnMapFolder_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(CreatorMap.MapFolder);
            Process.Start("explorer.exe", CreatorMap.MapFolder);
        }

        // ---- put in -------------------------------------------------------------------------

        /// <summary>What the dialog asks for, per part.</summary>
        private sealed class PutInChoice
        {
            public JobParts.Part Part;
            public int Backup;
            public int Job;
            public string Why;      // why the part cannot be put in, or null
            public bool On;
            public JobParts.Mode Mode = JobParts.Mode.Replace;
            public int After => Mode == JobParts.Mode.Append ? Job + Backup : Backup;
        }

        private async void PutInMapEntry(MapEntry entry)
        {
            if (!MapBackupReady())
                return;
            string creator = CreatorMap.CurrentCreator();
            bool old = entry.Kind == MapEntryKind.Old;
            if (old && entry.Problem != null)
            {
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("map_incompatible", "Diese Sicherung stammt von {0} {1} und passt nicht zur laufenden Spielversion."),
                    entry.Snapshot.Edition, entry.Snapshot.Build);
                return;
            }
            if (!old && !(JobDatafile.CanExport(creator) && JobDatafile.CanImport(creator)))
            {
                tbMapStatus.Text = JobFileMessage(JobDatafile.Result.Unsupported);
                return;
            }

            var choices = JobParts.All.Select(part =>
            {
                var choice = new PutInChoice
                {
                    Part = part,
                    Backup = entry.Counts.TryGetValue(part.Key, out int n) ? n : 0,
                    Job = CreatorMap.CurrentCount(OldSections[part.Key])
                };
                if (!part.AppliesTo(creator))
                    choice.Why = string.Format(CultureInfo.CurrentCulture, TranslateOr("map_only_in", "Gibt es nur im {0}-Creator."), CreatorKindName("fm_race_creator"));
                else if (choice.Backup == 0)
                    choice.Why = TranslateOr("map_empty_part", "In der Sicherung leer.");
                choice.On = choice.Why == null && part.Key != "cp";
                return choice;
            }).ToList();

            var identity = old ? (null, false) : JobDatafile.Identity();
            string jobTitle = new Global(GTA.Offsets.Editor.nm).GetString()?.Trim();
            if (string.IsNullOrEmpty(jobTitle))
                jobTitle = CreatorDisplayName(creator);
            bool otherCreator = !string.IsNullOrEmpty(entry.Creator) && entry.Creator != creator;

            var dialog = BuildPutInDialog(choices, old, otherCreator, identity, jobTitle, out var links, out var asNew);
            bool confirmed = await ShowContentAsync(
                string.Format(CultureInfo.CurrentCulture, TranslateOr("map_dlg_title", "„{0}“ in den offenen Job einspielen"), entry.Name),
                string.Format(CultureInfo.CurrentCulture, TranslateOr("map_dlg_sub", "Sicherung aus {0} · offener Job: „{1}“ ({2})"),
                    CreatorKindName(entry.Creator), jobTitle, CreatorKindName(creator)),
                dialog, TranslateOr("map_put_in_confirm", "Einspielen"), TranslateOr("dialog_cancel", "Abbrechen"));
            if (!confirmed || !MapBackupReady())
                return;

            var chosen = choices.Where(c => c.On).ToList();
            if (chosen.Count == 0)
                return;
            SetMapBusy(true);
            try
            {
                if (old)
                    await PutInOldAsync(entry, chosen);
                else
                    await PutInJsonAsync(entry, chosen, links.IsChecked == true, asNew.IsChecked == true ? default : identity);
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("map_restore_failed", "Laden fehlgeschlagen.") + " " + ex.Message;
                Log.Error("map put in", ex, "mapbackup");
            }
            finally
            {
                SetMapBusy(false);
                LoadMapBackupList();
            }
        }

        private UIElement BuildPutInDialog(List<PutInChoice> choices, bool old, bool otherCreator, (string Id, bool Updates) identity,
            string jobTitle, out CheckBox links, out CheckBox asNew)
        {
            var root = new StackPanel();

            var table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 150 });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(78) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            string[] heads =
            {
                TranslateOr("map_col_part", "Teil"), TranslateOr("map_col_backup", "Sicherung"),
                TranslateOr("map_col_job", "Im Job"), TranslateOr("map_col_action", "Aktion")
            };
            for (int c = 0; c < heads.Length; c++)
            {
                var head = MapText(heads[c].ToUpper(CultureInfo.CurrentCulture), 11, "FaintTextBrush", bold: true);
                head.Margin = new Thickness(6, 0, c == 1 || c == 2 ? 12 : 6, 6);
                if (c == 1 || c == 2)
                    head.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetColumn(head, c);
                table.Children.Add(head);
            }

            var limitBox = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 12, 0, 0), Background = Tint("BadBrush", 0x20) };
            var limitText = MapText("", 12.5, "BadBrush");
            limitBox.Child = limitText;

            var refreshers = new List<Action>();
            void Refresh()
            {
                foreach (var refresh in refreshers)
                    refresh();
                var over = choices.Where(c => c.On && c.After > c.Part.Limit).ToList();
                limitBox.Visibility = over.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                limitText.Text = string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("map_too_many", "Zu viele: {0}. Der Creator lädt nur bis zum Limit, der Rest fiele weg. Weniger anhängen oder ersetzen."),
                    string.Join(", ", over.Select(c => string.Format(CultureInfo.CurrentCulture, TranslateOr("map_of", "{0}: {1} von {2}"), PartLabel(c.Part.Key), c.After, c.Part.Limit))));
                SetDialogConfirmEnabled(choices.Any(c => c.On) && over.Count == 0);
            }

            int row = 1;
            foreach (var choice in choices)
            {
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                bool usable = choice.Why == null;
                var line = new Border { BorderThickness = new Thickness(0, 1, 0, 0) };
                line.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                Grid.SetRow(line, row);
                Grid.SetColumnSpan(line, 4);
                table.Children.Add(line);

                var check = new CheckBox { IsChecked = choice.On, IsEnabled = usable, VerticalAlignment = VerticalAlignment.Center };
                var label = MapText(PartLabel(choice.Part.Key), 14, "TextColor");
                label.Margin = new Thickness(8, 0, 0, 0);
                label.VerticalAlignment = VerticalAlignment.Center;
                var name = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(6, 8, 6, 8), Background = Brushes.Transparent };
                name.Children.Add(check);
                name.Children.Add(label);
                if (!usable)
                    Explain(name, choice.Why);

                var backup = MapText(choice.Backup.ToString(CultureInfo.InvariantCulture), 14, "TextColor");
                var job = MapText(choice.Job.ToString(CultureInfo.InvariantCulture), 14, "TextColor");
                foreach (var number in new[] { backup, job })
                {
                    number.HorizontalAlignment = HorizontalAlignment.Right;
                    number.VerticalAlignment = VerticalAlignment.Center;
                    number.Margin = new Thickness(6, 0, 12, 0);
                }

                var replace = SegmentButton(TranslateOr("map_replace", "Ersetzen"));
                var append = SegmentButton(TranslateOr("map_append", "Anhängen"));
                if (old)
                    Explain(append, TranslateOr("map_old_replace_only", "Altes Format: nur Ersetzen."));
                var segment = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(2), HorizontalAlignment = HorizontalAlignment.Left };
                segment.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                var buttons = new StackPanel { Orientation = Orientation.Horizontal };
                buttons.Children.Add(replace);
                buttons.Children.Add(append);
                segment.Child = buttons;
                var after = MapText("", 11.5, "FaintTextBrush");
                after.Margin = new Thickness(0, 3, 0, 0);
                var action = new StackPanel { Margin = new Thickness(6, 7, 6, 7), VerticalAlignment = VerticalAlignment.Center };
                action.Children.Add(segment);
                action.Children.Add(after);

                var cells = new FrameworkElement[] { name, backup, job, action };
                for (int c = 0; c < cells.Length; c++)
                {
                    Grid.SetRow(cells[c], row);
                    Grid.SetColumn(cells[c], c);
                    table.Children.Add(cells[c]);
                    if (!usable)
                        cells[c].Opacity = 0.38;
                }

                refreshers.Add(() =>
                {
                    replace.IsEnabled = choice.On;
                    append.IsEnabled = choice.On && !old;
                    MarkSegment(replace, choice.Mode == JobParts.Mode.Replace);
                    MarkSegment(append, choice.Mode == JobParts.Mode.Append);
                    bool over = choice.On && choice.After > choice.Part.Limit;
                    after.Text = choice.On
                        ? string.Format(CultureInfo.CurrentCulture, TranslateOr("map_after", "danach {0} / {1}"), choice.After, choice.Part.Limit)
                        : choice.Why ?? TranslateOr("map_keep", "bleibt wie im Job");
                    after.SetResourceReference(TextBlock.ForegroundProperty, over ? "BadBrush" : "FaintTextBrush");
                    after.FontWeight = over ? FontWeights.Bold : FontWeights.Normal;
                });
                check.Checked += (s, e) => { choice.On = true; Refresh(); };
                check.Unchecked += (s, e) => { choice.On = false; Refresh(); };
                replace.Click += (s, e) => { choice.Mode = JobParts.Mode.Replace; Refresh(); };
                append.Click += (s, e) => { choice.Mode = JobParts.Mode.Append; Refresh(); };
                row++;
            }
            root.Children.Add(table);

            CheckBox Option(string text, string hint, bool isChecked, bool enabled, out FrameworkElement line)
            {
                var box = new CheckBox { IsChecked = isChecked, IsEnabled = enabled, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) };
                var words = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
                words.Children.Add(MapText(text, 13, "TextColor"));
                if (hint != null)
                    words.Children.Add(MapText(hint, 12, "MutedTextBrush"));
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0), Opacity = enabled ? 1 : 0.38 };
                panel.Children.Add(box);
                panel.Children.Add(words);
                line = panel;
                return box;
            }

            links = Option(TranslateOr("map_links", "Verknüpfungen zu Teams und Regeln zurücksetzen"),
                otherCreator ? TranslateOr("map_links_other", "Sicherung stammt aus einem anderen Creator. Team- und Regel-Nummern passen dort nicht.")
                             : TranslateOr("map_links_same", "Nur bei einer Sicherung aus einem anderen Creator nötig."),
                otherCreator && !old, otherCreator && !old, out var linksLine);
            root.Children.Add(linksLine);
            root.Children.Add(limitBox);

            var info = new TextBlock { FontSize = 12.5, TextWrapping = TextWrapping.Wrap, LineHeight = 18 };
            info.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            var infoBox = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 12, 0, 0), Child = info };
            infoBox.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            root.Children.Add(infoBox);

            bool updates = identity.Id != null && identity.Updates;
            asNew = Option(TranslateOr("map_as_new", "Stattdessen als neuen Job speichern"), null, false, true, out var asNewLine);
            asNewLine.Visibility = updates ? Visibility.Visible : Visibility.Collapsed;
            root.Children.Add(asNewLine);

            var newBox = asNew;
            void Info()
            {
                info.Inlines.Clear();
                if (old)
                {
                    info.Inlines.Add(new Run(TranslateOr("map_old_info", "Der Creator baut die Map neu auf, ohne den Job neu zu laden. Die Job-ID bleibt.")));
                }
                else if (!updates)
                {
                    info.Inlines.Add(new Run(TranslateOr("map_reload_new", "Der Job lädt neu (ca. 5 s). Er wurde noch nie gespeichert, Speichern legt ihn neu an.")));
                }
                else if (newBox.IsChecked == true)
                {
                    info.Inlines.Add(new Run(string.Format(CultureInfo.CurrentCulture,
                        TranslateOr("map_reload_asnew", "Der Job lädt neu (ca. 5 s). Speichern legt einen neuen Job an, „{0}“ bleibt unverändert."), jobTitle)));
                }
                else
                {
                    info.Inlines.Add(new Run(TranslateOr("map_reload_overwrite", "Der Job lädt neu (ca. 5 s). Speichern überschreibt") + " "));
                    info.Inlines.Add(new Run($"„{jobTitle}“") { FontWeight = FontWeights.Bold, Foreground = ThemeBrush("TextColor") });
                    info.Inlines.Add(new Run("  " + identity.Id) { FontFamily = new FontFamily("Consolas"), Foreground = ThemeBrush("FaintTextBrush") });
                }
            }
            asNew.Checked += (s, e) => Info();
            asNew.Unchecked += (s, e) => Info();
            Info();

            // The dialog resets the confirm button when it opens; refresh once it is shown.
            Dispatcher.BeginInvoke(new Action(Refresh), System.Windows.Threading.DispatcherPriority.Loaded);
            return root;
        }

        private Button SegmentButton(string text) => new Button
        {
            Content = text, FontSize = 12.5, Height = 24, Padding = new Thickness(10, 0, 10, 0),
            Style = (Style)FindResource("FormButton"), Background = Brushes.Transparent
        };

        private static void MarkSegment(Button button, bool on)
        {
            if (on)
            {
                button.SetResourceReference(BackgroundProperty, "SeactionHeaderBackgroundBrush");
                button.SetResourceReference(ForegroundProperty, "TextColor");
            }
            else
            {
                button.Background = Brushes.Transparent;
                button.SetResourceReference(ForegroundProperty, "MutedTextBrush");
            }
        }

        private async Task PutInJsonAsync(MapEntry entry, List<PutInChoice> chosen, bool resetLinks, (string Id, bool Updates) identity)
        {
            tbMapStatus.Text = TranslateOr("job_file_exporting", "Der Creator baut die Job-Datei …");
            var (result, job) = await JobDatafile.ExportAsync();
            if (result != JobDatafile.Result.Done)
            {
                tbMapStatus.Text = JobFileMessage(result);
                return;
            }

            // The job may have changed while the dialog was open; check the limits again.
            foreach (var choice in chosen)
            {
                choice.Job = JobParts.Count(job, choice.Part.Key);
                if (choice.After > choice.Part.Limit)
                {
                    tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                        TranslateOr("map_too_many_now", "Zu viele {0}: {1} von {2}. Nichts eingespielt."), PartLabel(choice.Part.Key), choice.After, choice.Part.Limit);
                    return;
                }
            }

            SaveJsonUndo(job);

            var merged = (JObject)job.DeepClone();
            foreach (var choice in chosen)
                JobParts.Merge(merged, choice.Part.Key, entry.PartData(choice.Part.Key), choice.Mode, resetLinks);

            tbMapStatus.Text = TranslateOr("job_file_importing", "Der Creator lädt den Job …");
            result = await JobDatafile.ImportAsync(merged, identity);
            if (result != JobDatafile.Result.Done)
            {
                tbMapStatus.Text = JobFileMessage(result);
                return;
            }
            ApplyJobFileTexts(merged);
            string done = string.Join(", ", chosen.Select(c => string.Format(CultureInfo.CurrentCulture,
                c.Mode == JobParts.Mode.Append ? TranslateOr("map_done_appended", "{0} angehängt") : TranslateOr("map_done_replaced", "{0} ersetzt"),
                PartLabel(c.Part.Key))));
            tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("map_put_in_done", "Eingespielt aus „{0}“: {1}. Job neu geladen."), entry.Name, done);
            Log.Info($"map put in: {entry.File} ({string.Join(", ", chosen.Select(c => $"{c.Part.Key} {c.Mode}"))}, links {(resetLinks ? "reset" : "kept")}, id {identity.Id ?? "new"})", source: "mapbackup");
        }

        private async Task PutInOldAsync(MapEntry entry, List<PutInChoice> chosen)
        {
            var sections = new HashSet<string>(chosen.Select(c => OldSections[c.Part.Key]));
            // The current state first, so a wrong click can be undone.
            CreatorMap.Save(CreatorMap.Capture(TranslateOr("map_before_restore", "Vor dem Laden")), UndoFile);
            if (File.Exists(JsonUndoFile))
                File.Delete(JsonUndoFile);

            bool rebuilt = await CreatorMap.RestoreAsync(entry.Snapshot, sections);
            tbMapStatus.Text = rebuilt
                ? string.Format(CultureInfo.CurrentCulture, TranslateOr("map_restored", "Geladen: {0}"), entry.Name)
                : TranslateOr("map_restored_norebuild", "Daten geladen, aber der Creator hat die Map nicht neu aufgebaut (nur im Bearbeiten-Modus von Race, LTS und Capture möglich). Einmal testen oder den Job neu laden zeigt sie an.");
            Log.Info($"map restored: {entry.File} (rebuild {(rebuilt ? "ok" : "not done")})", source: "mapbackup");
        }

        /// <summary>
        /// The creator takes title and description from the job's cloud entry, not from the
        /// job file; for an id without one (an import) it shows "No translation.", so write both.
        /// </summary>
        private void ApplyJobFileTexts(JObject job)
        {
            var gen = job["mission"]?["gen"];
            string title = gen?["nm"]?.Type == JTokenType.String ? (string)gen["nm"] : null;
            var dec = gen?["dec"];
            string description = dec is JArray parts
                ? string.Concat(parts.Select(p => (string)p))
                : dec?.Type == JTokenType.String ? (string)dec : null;
            if (!string.IsNullOrEmpty(title))
                new Global(GTA.Offsets.Editor.nm).SetString(title);
            if (!string.IsNullOrEmpty(description))
                setDescribtionNew(description);
        }

        // ---- undo, convert, rename, delete ----------------------------------------------------

        private async void BtnMapUndo_Click(object sender, RoutedEventArgs e)
        {
            if (!MapBackupReady())
                return;
            bool json = File.Exists(JsonUndoFile)
                && (!File.Exists(UndoFile) || File.GetLastWriteTime(JsonUndoFile) >= File.GetLastWriteTime(UndoFile));
            SetMapBusy(true);
            try
            {
                if (json)
                {
                    var undo = JObject.Parse(File.ReadAllText(JsonUndoFile));
                    string creator = (string)undo["creator"];
                    if (creator != CreatorMap.CurrentCreator())
                    {
                        tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                            TranslateOr("map_undo_wrong_creator", "Der Stand vor dem Einspielen gehört in den {0}."), CreatorDisplayName(creator));
                        return;
                    }
                    var job = (JObject)undo["job"];
                    tbMapStatus.Text = TranslateOr("job_file_importing", "Der Creator lädt den Job …");
                    var result = await JobDatafile.ImportAsync(job, ((string)undo["id"], (bool?)undo["updates"] ?? false));
                    if (result != JobDatafile.Result.Done)
                    {
                        tbMapStatus.Text = JobFileMessage(result);
                        return;
                    }
                    ApplyJobFileTexts(job);
                    File.Delete(JsonUndoFile);
                    tbMapStatus.Text = TranslateOr("map_undone", "Stand vor dem Einspielen wiederhergestellt. Job neu geladen.");
                }
                else if (File.Exists(UndoFile))
                {
                    var snap = CreatorMap.Load(UndoFile);
                    var all = new HashSet<string>(snap.Sections.Select(x => x.Name));
                    bool rebuilt = await CreatorMap.RestoreAsync(snap, all);
                    File.Delete(UndoFile);
                    tbMapStatus.Text = rebuilt
                        ? TranslateOr("map_undone_old", "Stand vor dem Laden wiederhergestellt.")
                        : TranslateOr("map_restored_norebuild", "Daten geladen, aber der Creator hat die Map nicht neu aufgebaut (nur im Bearbeiten-Modus von Race, LTS und Capture möglich). Einmal testen oder den Job neu laden zeigt sie an.");
                }
                Log.Info($"map put in undone ({(json ? "job file" : "raw")})", source: "mapbackup");
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("map_restore_failed", "Laden fehlgeschlagen.") + " " + ex.Message;
                Log.Error("map undo", ex, "mapbackup");
            }
            finally
            {
                SetMapBusy(false);
                LoadMapBackupList();
            }
        }

        /// <summary>
        /// An old raw backup as JSON: put it into the open job, let the creator build the job
        /// file, and put the job back the way it was. Only in the build it was saved on.
        /// </summary>
        private async void ConvertOldMapEntry(MapEntry entry)
        {
            if (!MapBackupReady())
                return;
            string creator = CreatorMap.CurrentCreator();
            if (entry.Problem != null)
            {
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("map_incompatible", "Diese Sicherung stammt von {0} {1} und passt nicht zur laufenden Spielversion."),
                    entry.Snapshot.Edition, entry.Snapshot.Build);
                return;
            }
            if (entry.Creator != creator)
            {
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture,
                    TranslateOr("map_convert_creator", "Zum Umwandeln den {0} öffnen."), CreatorDisplayName(entry.Creator));
                return;
            }
            if (!JobDatafile.CanExport(creator))
            {
                tbMapStatus.Text = JobFileMessage(JobDatafile.Result.Unsupported);
                return;
            }

            SetMapBusy(true);
            var current = CreatorMap.Capture(entry.Name);
            var all = new HashSet<string>(current.Sections.Select(x => x.Name));
            try
            {
                await CreatorMap.RestoreAsync(entry.Snapshot, new HashSet<string>(entry.Snapshot.Sections.Select(x => x.Name)));
                tbMapStatus.Text = TranslateOr("job_file_exporting", "Der Creator baut die Job-Datei …");
                var (result, job) = await JobDatafile.ExportAsync();
                if (result != JobDatafile.Result.Done)
                {
                    tbMapStatus.Text = JobFileMessage(result);
                    return;
                }
                var file = JobParts.CreateFile(job, entry.Name, creator);
                file["savedAt"] = entry.SavedAt.ToString("o", CultureInfo.InvariantCulture);
                string path = Path.Combine(CreatorMap.MapFolder, Path.GetFileNameWithoutExtension(entry.File) + JobParts.FileExtension);
                JobParts.Save(file, path);
                tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("map_converted", "In JSON umgewandelt: {0}"), entry.Name);
                Log.Info($"map converted: {entry.File} -> {path}", source: "mapbackup");
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("job_file_failed", "Fehlgeschlagen.") + " " + ex.Message;
                Log.Error("map convert", ex, "mapbackup");
            }
            finally
            {
                await CreatorMap.RestoreAsync(current, all);
                SetMapBusy(false);
                LoadMapBackupList();
            }
        }

        private async void RenameMapEntry(MapEntry entry)
        {
            if (mapBusy)
                return;
            string name = await AskTextAsync(TranslateOr("map_rename_title", "Sicherung umbenennen"), null, entry.Name, TranslateOr("map_rename", "Umbenennen"));
            if (string.IsNullOrWhiteSpace(name) || name == entry.Name)
                return;
            try
            {
                switch (entry.Kind)
                {
                    case MapEntryKind.Parts:
                        entry.Data["name"] = name;
                        JobParts.Save(entry.Data, entry.File);
                        break;
                    case MapEntryKind.Old:
                        entry.Snapshot.Name = name;
                        CreatorMap.Save(entry.Snapshot, entry.File);
                        break;
                    default:
                        // A whole job keeps its title; the file gets the new name.
                        string target = Path.Combine(Path.GetDirectoryName(entry.File), SafeFileName(name, "job") + ".json");
                        if (File.Exists(target))
                        {
                            tbMapStatus.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("map_rename_exists", "„{0}“ gibt es schon."), Path.GetFileName(target));
                            return;
                        }
                        File.Move(entry.File, target);
                        break;
                }
            }
            catch (Exception ex)
            {
                tbMapStatus.Text = TranslateOr("job_file_failed", "Fehlgeschlagen.") + " " + ex.Message;
                Log.Warn($"map rename: {entry.File}", ex, source: "mapbackup");
            }
            LoadMapBackupList();
        }

        private async void DeleteMapEntry(MapEntry entry)
        {
            if (mapBusy)
                return;
            bool confirmed = await ConfirmAsync(
                string.Format(CultureInfo.CurrentCulture, TranslateOr("map_delete_confirm", "„{0}“ löschen?"), entry.Name),
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
    }
}
