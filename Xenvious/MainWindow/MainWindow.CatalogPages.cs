using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: catalog pages layout (position card under the model card, heading slider).
    public partial class MainWindow
    {
        private void InitCatalogPages()
        {
            PositionUnderModel(PropModelCard, tbpropslocx, tbpropsrotz);
            PositionUnderModel(DPropModelCard, tbdpropslocx, tbdpropsrotz);
            PositionUnderModel(VehModelCard, tbvehlocx, tbvehhead);
            PositionUnderModel(WeapModelCard, tbweaplocx, tbweapheading);
            PositionUnderModel(ActorModelCard, tbactorlocx, tbactorhead);
            PositionUnderModel(ObjModelCard, tbobjlocx, tbobjhead);
        }

        /// <summary>
        /// Moves the card holding the location boxes right under the model card (after its
        /// "add to menu" button, if any) and gives the heading box a slider.
        /// </summary>
        private void PositionUnderModel(ModelCard model, TextBox locX, TextBox heading)
        {
            var dashCard = (Style)FindResource("DashCard");
            FrameworkElement card = locX;
            while (card != null && !(card is Border b && b.Style == dashCard))
                card = card.Parent as FrameworkElement;
            if (card?.Parent is Panel from && model.Parent is Panel to && from != to)
            {
                from.Children.Remove(card);
                int at = to.Children.IndexOf(model) + 1;
                if (at < to.Children.Count && to.Children[at] is Button)
                    at++;
                to.Children.Insert(at, card);
            }
            HeadingSlider.Attach(heading, creatorRefresh);
        }
    }
}
