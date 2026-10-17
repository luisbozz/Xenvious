using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: Navigation between the main pages.
    public partial class MainWindow
    {
        private void Btndashboard_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageDashboard;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageEdit;
        }


        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageSettings;
        }

        private void BtnConverter_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageConverter;
        }

        private void BtnTeleport_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageTeleport;
        }

        private void BtnMod_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageMod;
        }

        private void MainPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // NavButton (Controls/NavBar.xaml) draws the button tagged "active" as the open page.
            var pages = new (Button Button, TabItem Page)[]
            {
                (Btndashboard, PageDashboard), (BtnEdit, PageEdit), (BtnCopyJobs, PageCopyJobs),
                (BtnConverter, PageConverter), (BtnTeleport, PageTeleport), (BtnGEditor, PageGEditor),
                (BtnMod, PageMod), (BtnSettings, PageSettings),
            };
            foreach (var (button, page) in pages)
                button.Tag = MainPages.SelectedItem == page ? "active" : null;
            // Only the main pages themselves: inner tab controls bubble this event up too.
            if (e.OriginalSource == MainPages)
                CountMainPage(MainPages.SelectedItem);
            // Not while the window is being built: the Settings controls may not exist yet.
            if (MainPages.SelectedItem == PageSettings && IsLoaded)
                UpdateModelCacheSettings();

            UpdateGtaMessage();
        }

        // The "Start GTA" overlay while not connected; pages that need no game never get it.
        private void UpdateGtaMessage()
        {
            if (m.IsProcOpen || SGTAMessage == null)
                return;
            bool noGame = MainPages.SelectedItem == PageSettings || MainPages.SelectedItem == PageConverter
                || MainPages.SelectedItem == PageMod || MainPages.SelectedItem == PageCopyJobs;
            SGTAMessage.Visibility = HideGtaMessage() || noGame ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnGEditor_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageGEditor;
        }
    }
}
