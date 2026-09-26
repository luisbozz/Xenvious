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

            if (!m.IsProcOpen && SGTAMessage != null)
            {
                if (HideGtaMessage() || MainPages.SelectedItem == PageSettings || MainPages.SelectedItem == PageConverter || MainPages.SelectedItem == PageMod || MainPages.SelectedItem == PageCopyJobs)
                {
                    SGTAMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SGTAMessage.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnGEditor_Click(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedItem = PageGEditor;
        }
    }
}
