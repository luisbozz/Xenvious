using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: what happens after a job was copied into the creator.
    public partial class MainWindow
    {
        private enum CopyJobMode { CopyOnly = 0, CopyAndSave = 1, CopyAndPublish = 2 }

        private const int CreatorStateSave = 5;
        private const int CreatorStatePublish = 91;

        private void FillCopyJobModes()
        {
            int selected = ddCopyJobMode.SelectedIndex < 0 ? 0 : ddCopyJobMode.SelectedIndex;
            ddCopyJobMode.Items.Clear();
            ddCopyJobMode.Items.Add(TranslateOr("copy_mode_only", "Nur kopieren"));
            ddCopyJobMode.Items.Add(TranslateOr("copy_mode_save", "Kopieren + speichern"));
            ddCopyJobMode.Items.Add(TranslateOr("copy_mode_publish", "Kopieren + veröffentlichen"));
            ddCopyJobMode.SelectedIndex = selected;
            BtnCopyJobLoad.Content = TranslateOr("copy_load", "Laden");

            int scope = ddCopyScope.SelectedIndex < 0 ? 0 : ddCopyScope.SelectedIndex;
            ddCopyScope.Items.Clear();
            ddCopyScope.Items.Add(TranslateOr("copy_scope_full", "Kompletter Job"));
            ddCopyScope.Items.Add(TranslateOr("copy_scope_parts", "Nur ausgewählte Teile"));
            ddCopyScope.SelectedIndex = scope;
            UpdateCopyScope();
        }

        /// <summary>
        /// "Complete job": the creator loads the job itself (<see cref="JobLoader"/>), so
        /// nothing is left out. "Selected parts": Xenvious writes only the checked parts
        /// into the job that is open.
        /// </summary>
        private bool CopyCompleteJob => ddCopyScope.SelectedIndex <= 0;

        private void DdCopyScope_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateCopyScope();

        private void UpdateCopyScope()
        {
            if (CopyPartsPanel == null || tbCopyScopeHint == null || cbCopyEndcon == null)
                return;
            // The image is not part of the job data, so it is offered in both scopes.
            foreach (var box in new CheckBox[] { cbCopyProps, cbCopyDProps, cbCopyCP, cbCopyTemplates, cbCopyDHProps, cbCopyBasics, cbCopyGen, cbCopyMenubs, cbCopyEndcon })
            {
                var pair = (FrameworkElement)box.Parent;
                pair.IsEnabled = !CopyCompleteJob;
                pair.Opacity = CopyCompleteJob ? 0.4 : 1.0;
            }
            tbCopyScopeHint.Text = CopyCompleteJob
                ? TranslateOr("copy_scope_full_hint", "GTA lädt den Job selbst in den offenen Creator, mit allem, was dazugehört. Der Creator muss zum Job passen (Race, LTS oder Capture).")
                : TranslateOr("copy_scope_parts_hint", "Xenvious schreibt nur die angehakten Teile in den Job, der gerade offen ist.");
        }

        private async Task CopyCompleteJobAsync()
        {
            string creator = CreatorMap.CurrentCreator();
            string wanted = JobLoader.CreatorFor(ToInt(jobjson?.Mission?.Gen?.Type), ToInt(jobjson?.Mission?.Gen?.Subtype));
            if (wanted == null)
            {
                SetCopyStatus(TranslateOr("copy_full_unsupported", "Komplett laden geht nur für Races, LTS und Capture. Wähle „Nur ausgewählte Teile“."), true);
                return;
            }
            if (creator != wanted)
            {
                SetCopyStatus(string.Format(TranslateOr("copy_full_wrong_creator", "Dieser Job gehört in den {0}. Öffne ihn in GTA und kopiere dann."), CreatorDisplayName(wanted)), true);
                return;
            }

            bool replace = await ConfirmAsync(
                TranslateOr("copy_full_confirm_title", "Offenen Job ersetzen?"),
                string.Format(TranslateOr("copy_full_confirm_text", "GTA lädt „{0}“ und ersetzt dabei alles im offenen Creator. Die Map des offenen Jobs (Props, Dynamic Props, Checkpoints, Templates) sichert Xenvious vorher unter Map Backup."), tbCopyName.Text),
                TranslateOr("copy_full_confirm", "Sichern und ersetzen"), TranslateOr("dialog_cancel", "Abbrechen"));
            if (!replace)
                return;

            BtnCopyJob.IsEnabled = false;
            try
            {
                try
                {
                    var snap = SaveCurrentMap();
                    Log.Info($"copy job: open map saved first ({DescribeSnapshot(snap)})", source: "copyjob");
                }
                catch (System.Exception ex)
                {
                    Log.Error("copy job: saving the open map failed", ex, "copyjob");
                    SetCopyStatus(TranslateOr("copy_full_backup_failed", "Die offene Map ließ sich nicht sichern; nichts wurde ersetzt."), true);
                    return;
                }

                SetCopyStatus(TranslateOr("copy_full_loading", "GTA lädt den Job …"), false);
                var result = await JobLoader.LoadAsync(copyJobContentId);
                switch (result)
                {
                    case JobLoader.Result.Loaded:
                        break;
                    case JobLoader.Result.NotEditing:
                        SetCopyStatus(TranslateOr("copy_full_busy", "Der Creator ist gerade beschäftigt (Test, Speichern, Menüwechsel). Versuch es gleich noch einmal."), true);
                        return;
                    case JobLoader.Result.TimedOut:
                        SetCopyStatus(TranslateOr("copy_full_timeout", "Der Creator hat den Job nicht geladen. Schau in GTA nach einer Meldung."), true);
                        return;
                    default:
                        SetCopyStatus(TranslateOr("copy_full_unsupported", "Komplett laden geht nur für Races, LTS und Capture. Wähle „Nur ausgewählte Teile“."), true);
                        return;
                }

                // The job's photo belongs to the original job; without a copied image the
                // creator asks for a new one before saving.
                if (cbCopyImage.IsChecked == true)
                    CopyJobImage();
                else
                    Functions.Write.writebinary(1, GTA.Offsets.Editor.photo, false);

                await FinishCopyJobAsync(rebuild: false);
            }
            finally
            {
                BtnCopyJob.IsEnabled = true;
            }
        }

        private static int ToInt(object value)
        {
            try
            {
                return value == null ? -1 : System.Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Copying writes the job data straight into the globals, so the creator still
        /// shows the old entities; a rebuild makes the copy visible. Then the creator is
        /// told to save (main state 5) or publish (91) the job, as chosen. Publishing is
        /// visible to others, so it asks first.
        /// </summary>
        private async Task FinishCopyJobAsync(bool rebuild = true)
        {
            if (!m.IsProcOpen || !IsCreatorRunning())
                return;
            var mode = (CopyJobMode)(ddCopyJobMode.SelectedIndex < 0 ? 0 : ddCopyJobMode.SelectedIndex);

            if (mode == CopyJobMode.CopyAndPublish)
            {
                bool publish = await ConfirmAsync(
                    TranslateOr("copy_publish_title", "Job veröffentlichen?"),
                    TranslateOr("copy_publish_text", "Der kopierte Job wird unter deinem Social-Club-Konto veröffentlicht und ist dann für andere sichtbar."),
                    TranslateOr("copy_publish_confirm", "Veröffentlichen"), TranslateOr("dialog_cancel", "Abbrechen"));
                if (!publish)
                    mode = CopyJobMode.CopyOnly;
            }

            bool rebuilt = !rebuild || await CreatorMap.RebuildAsync();
            Log.Info($"copy job: rebuild {(rebuilt ? "ok" : "not done")}, then {mode}", source: "copyjob");
            if (!rebuilt)
            {
                SetCopyStatus(TranslateOr("copy_done_norebuild", "Kopiert. Der Creator hat die Map nicht neu aufgebaut; lade den Job neu, um sie zu sehen."), true);
                return;
            }
            SetCopyStatus(mode == CopyJobMode.CopyAndSave ? TranslateOr("copy_done_save", "Kopiert, der Creator speichert den Job.")
                : mode == CopyJobMode.CopyAndPublish ? TranslateOr("copy_done_publish", "Kopiert, der Creator veröffentlicht den Job.")
                : TranslateOr("copy_done", "Kopiert. Nicht gespeichert: speichere im Creator, wenn du den Job behalten willst."), false);
            if (mode == CopyJobMode.CopyOnly)
                return;

            m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh * 8).ToString("X"))
                .SetInt(mode == CopyJobMode.CopyAndSave ? CreatorStateSave : CreatorStatePublish);
        }
    }
}
