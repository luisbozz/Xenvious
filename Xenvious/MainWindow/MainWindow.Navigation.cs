using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            Btndashboard.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            BtnEdit.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            BtnSettings.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            BtnConverter.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            BtnTeleport.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            BtnGEditor.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            BtnMod.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];
            BtnCopyJobs.Background = (SolidColorBrush)Resources["SectionBackgroundBrush"];

            if (MainPages.SelectedItem == PageDashboard)
            {
                Btndashboard.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (MainPages.SelectedItem == PageEdit)
            {
                BtnEdit.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (MainPages.SelectedItem == PageSettings)
            {
                BtnSettings.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (MainPages.SelectedItem == PageConverter)
            {
                BtnConverter.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (MainPages.SelectedItem == PageTeleport)
            {
                BtnTeleport.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (MainPages.SelectedItem == PageGEditor)
            {
                BtnGEditor.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (MainPages.SelectedItem == PageMod)
            {
                BtnMod.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (MainPages.SelectedItem == PageCopyJobs)
            {
                BtnCopyJobs.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }

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
