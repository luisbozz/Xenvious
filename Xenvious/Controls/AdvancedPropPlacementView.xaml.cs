using System.Windows;
using System.Windows.Controls;
using Xenvious.ViewModels;

namespace Xenvious.Controls
{
    public partial class AdvancedPropPlacementView : UserControl
    {
        public AdvancedPropPlacementView()
        {
            InitializeComponent();
            // Status and hovered prop are polled only while the page is on screen.
            IsVisibleChanged += (_, e) => (DataContext as AdvancedPropPlacementViewModel)?.SetActive((bool)e.NewValue);
        }
    }
}
