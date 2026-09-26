using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: Edit page.
    public partial class MainWindow
    {
        private void BtnSectionProps_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageProps;
        }

        private void BtnSectionVeh_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageVehicle;
        }

        private void BtnSectionWeap_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageWeapon;
        }

        private void BtnSectionRace_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageRace;
            if (m.IsProcOpen && ddcpssg.SelectedIndex == -1 && ddcpssg.Items.Count > 0)
            {
                ddcpssg.SelectedIndex = 0;
                ddRaceCPTransform.SelectedIndex = 0;
                if (ddtrfmvmno.SelectedIndex == -1)
                {
                    ddtrfmvmno.SelectedIndex = 0;
                }
            }
        }

        private void BtnSectionMission_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageMission;
        }

        private void BtnSectionActor_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageActor;
        }

        private void EditPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkSection();
            // Selection changes of dropdowns inside the pages bubble up to here as well.
            if (e.Source == EditPages)
                OnEditPageChanged();
        }

        private void BtnSectionZone_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageZone;
        }

        private void BtnSectionDoors_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageDoors;
        }

        private void BtnSectionDeathmatch_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageDeathmatch;
        }

        private void BtnSectionSurvival_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageSurvival;
        }

        private void BtnSectionObj_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = PageCapture;
        }

        private void BtnSectioncentity_Click(object sender, RoutedEventArgs e)
        {
            EditPages.SelectedItem = Pagecentity;
        }
    }
}
