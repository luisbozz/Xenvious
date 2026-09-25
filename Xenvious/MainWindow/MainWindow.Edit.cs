using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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


            BtnSectionProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionActor.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectioncentity.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionDeathmatch.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionSurvival.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionMission.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionObj.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionRace.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionDoors.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionVeh.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionWeap.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnSectionZone.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];

            if (EditPages.SelectedItem == PageProps)
            {
                BtnSectionProps.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionProps.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageActor)
            {
                BtnSectionActor.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionActor.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == Pagecentity)
            {
                BtnSectioncentity.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (EditPages.SelectedItem == PageDeathmatch)
            {
                BtnSectionDeathmatch.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionDeathmatch.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageSurvival)
            {
                BtnSectionSurvival.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionSurvival.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageMission)
            {
                BtnSectionMission.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionMission.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageCapture)
            {
                BtnSectionObj.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionObj.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageRace)
            {
                BtnSectionRace.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionRace.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageDoors)
            {
                BtnSectionDoors.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionDoors.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageVehicle)
            {
                BtnSectionVeh.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionVeh.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageWeapon)
            {
                BtnSectionWeap.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionWeap.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }
            else if (EditPages.SelectedItem == PageZone)
            {
                BtnSectionZone.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
                HeaderLabel.SetBinding(Label.ContentProperty, BtnSectionZone.GetBindingExpression(Button.ContentProperty).ParentBinding);
            }

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
