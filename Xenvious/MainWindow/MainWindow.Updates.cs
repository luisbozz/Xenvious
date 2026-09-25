using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: update check, update dialog, "what's new" and the update settings.
    public partial class MainWindow
    {
        private const string SettingCheckUpdates = "checkupdates";
        private const string SettingLastVersion = "lastversion";

        private bool _updateRunning;

        /// <summary>
        /// Runs once after the window has loaded: cleans up after an earlier update, shows
        /// what changed if this version starts for the first time, then looks for a newer
        /// release unless the player turned that off. Offering the update only here (or on
        /// the button) keeps it from interrupting work in the creator.
        /// </summary>
        private async Task StartupUpdateCheckAsync()
        {
            Updater.DeleteLeftovers();
            ShowUpdateVersion();
            await ShowWhatsNewAsync();

            var ini = new ini_reader(Functions.getRoamingConfigFilePath());
            if (!ini.ReadBoolean("Settings", SettingCheckUpdates, true))
                return;
            var update = await Updater.CheckAsync();
            if (update != null)
                await OfferUpdateAsync(update);
        }

        private void ShowUpdateVersion()
        {
            tbUpdateVersion.Text = string.Format(CultureInfo.CurrentCulture,
                TranslateOr("update_version", "Version {0}"), Updater.CurrentVersion);
            var ini = new ini_reader(Functions.getRoamingConfigFilePath());
            cbsettingscheckupdates.IsChecked = ini.ReadBoolean("Settings", SettingCheckUpdates, true);
        }

        /// <summary>After an update, once: this version's part of the built-in changelog.</summary>
        private async Task ShowWhatsNewAsync()
        {
            var ini = new ini_reader(Functions.getRoamingConfigFilePath());
            string last = ini.ReadString("Settings", SettingLastVersion, "");
            string current = Updater.CurrentVersion.ToString();
            if (last == current)
                return;
            ini.Write("Settings", SettingLastVersion, current);
            if (string.IsNullOrEmpty(last))
                return;   // first start ever: nothing "new" to show

            var lines = ChangelogText.Parse(ChangelogText.Section(ChangelogText.Embedded(), Updater.CurrentVersion));
            if (lines.Count == 0)
                return;
            await ConfirmAsync(
                string.Format(CultureInfo.CurrentCulture, TranslateOr("update_whatsnew_title", "Neu in Xenvious {0}"), current),
                null, "OK", null, details: lines);
        }

        private async Task OfferUpdateAsync(UpdateInfo update)
        {
            bool install = await ConfirmAsync(
                string.Format(CultureInfo.CurrentCulture, TranslateOr("update_available_title", "Xenvious {0} ist verfügbar"), update.Version),
                string.Format(CultureInfo.CurrentCulture, TranslateOr("update_available_text", "Du hast {0}. Xenvious lädt die neue Version, prüft sie und startet neu."), Updater.CurrentVersion),
                TranslateOr("update_install", "Jetzt aktualisieren"), TranslateOr("update_later", "Später"),
                details: ChangelogText.Parse(update.Notes));
            if (install)
                await InstallUpdateAsync(update);
        }

        private async Task InstallUpdateAsync(UpdateInfo update)
        {
            if (_updateRunning)
                return;
            _updateRunning = true;
            try
            {
                ShowBusyDialog(
                    string.Format(CultureInfo.CurrentCulture, TranslateOr("update_downloading", "Lade Xenvious {0} …"), update.Version),
                    null);
                var progress = new Progress<double>(SetDialogProgress);
                string download = await Updater.DownloadAsync(update, progress, CancellationToken.None);
                Updater.SwapAndRestart(download);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Log.Error("update failed", ex, "updater");
                CloseDialog(false);
                await ConfirmAsync(TranslateOr("update_failed_title", "Update fehlgeschlagen"),
                    TranslateOr("update_failed_text", "Xenvious bleibt auf der bisherigen Version. Grund:") + " " + ex.Message,
                    "OK", null, danger: true);
            }
            finally
            {
                _updateRunning = false;
            }
        }

        private async void BtnCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            BtnCheckUpdates.IsEnabled = false;
            try
            {
                var update = await Updater.CheckAsync();
                if (update != null)
                    await OfferUpdateAsync(update);
                else
                    await ConfirmAsync(TranslateOr("update_none_title", "Kein Update"),
                        string.Format(CultureInfo.CurrentCulture,
                            TranslateOr("update_none_text", "Xenvious {0} ist die neueste Version, oder GitHub war gerade nicht erreichbar."),
                            Updater.CurrentVersion),
                        "OK", null);
            }
            finally
            {
                BtnCheckUpdates.IsEnabled = true;
            }
        }

        private void cbsettingscheckupdates_Checked(object sender, RoutedEventArgs e)
        {
            new ini_reader(Functions.getRoamingConfigFilePath())
                .Write("Settings", SettingCheckUpdates, cbsettingscheckupdates.IsChecked ?? true);
        }

        // Clicking the version shows the whole built-in changelog, also offline.
        private async void TbUpdateVersion_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var lines = ChangelogText.Parse(ChangelogText.Embedded());
            if (lines.Count == 0)
                return;
            await ConfirmAsync("Changelog", null, "OK", null, details: lines);
        }
    }
}
