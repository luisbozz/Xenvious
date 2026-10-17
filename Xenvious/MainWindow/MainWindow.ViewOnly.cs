// Part of MainWindow: view-only mode, the window without any connection to GTA (for looking at the pages while playing).
using System.Windows;
using Xenvious.Logging;

namespace Xenvious
{
    public partial class MainWindow
    {
        /// <summary>
        /// Xenvious does not open the game process while this is on, BattlEye or not
        /// (Timercheckgta_Tick and the worker check it before OpenProcess). The window turns
        /// purple (Themes.ViewOnly) and a banner says so on every page, so nobody takes it for a
        /// connected one. Only for this launch; it starts from the BattlEye dialog or Settings.
        /// </summary>
        public bool ViewOnly { get; private set; }

        private Themes.Theme _themeBeforeViewOnly;

        public void EnterViewOnly(bool battlEye)
        {
            if (!ViewOnly)
            {
                ViewOnly = true;
                Log.Info("View-only mode on" + (battlEye ? " (BattlEye)" : ""), source: "ViewOnly");
                // Switched on by hand while connected: let go of the game like when it closes.
                if (m.IsProcOpen)
                {
                    if (mWorker.IsBusy)
                        mWorker.CancelAsync();
                    m.CloseProcess();
                    pid = 0;
                }
                _themeBeforeViewOnly = CurrentTheme;
                ApplyTheme(Themes.ViewOnly);
            }
            ViewOnlyBannerText.Text = battlEye
                ? TranslateOr("vo_banner_be", "Not connected to GTA (BattlEye is on). Nothing here reads or writes the game.")
                : TranslateOr("vo_banner", "Not connected to GTA. Nothing here reads or writes the game.");
            ViewOnlyBanner.Visibility = Visibility.Visible;
            MainPages.Margin = new Thickness(5, 46, 5, 5);
            cbsettingsviewonly.IsChecked = true;
            SGTAMessage.Visibility = Visibility.Collapsed;
        }

        public void LeaveViewOnly()
        {
            if (!ViewOnly)
                return;
            ViewOnly = false;
            Log.Info("View-only mode off", source: "ViewOnly");
            ApplyTheme(_themeBeforeViewOnly ?? Themes.All[0]);
            ViewOnlyBanner.Visibility = Visibility.Collapsed;
            MainPages.Margin = new Thickness(5);
            cbsettingsviewonly.IsChecked = false;
            // Still BattlEye: no new dialog for this game, the "Start GTA" overlay comes back.
            UpdateGtaMessage();
        }

        private void BtnLeaveViewOnly_Click(object sender, RoutedEventArgs e) => LeaveViewOnly();

        private void cbsettingsviewonly_Click(object sender, RoutedEventArgs e)
        {
            if (cbsettingsviewonly.IsChecked == true)
                EnterViewOnly(BattlEye.RunningProcess() != null);
            else
                LeaveViewOnly();
        }
    }
}
