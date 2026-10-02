using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: catalog pages layout (position card under the model card, heading slider, heading = rotation Z).
    public partial class MainWindow
    {
        private void InitCatalogPages()
        {
            PositionUnderModel(PropModelCard, tbpropslocx, tbpropshead);
            PositionUnderModel(DPropModelCard, tbdpropslocx, tbdpropshead);
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

        /// <summary>
        /// Props, dynamic props and vehicles keep a heading and a rotation. When the creator builds
        /// them, a set rotation (not 0,0,0 or 999,999,999) wins over the heading (fm_lts_creator
        /// func_1161 for props, func_1019 for vehicles), so a new heading also goes into the
        /// rotation's Z, and a new rotation Z into the heading.
        /// </summary>
        public static void WriteHeading(long head, long rot, string text, bool toHeading = true, bool toRotation = true)
        {
            if (!m.IsProcOpen || !float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out float value))
                return;
            if (toHeading && head != 0)
                new Global(head).SetFloat(value);
            if (!toRotation || rot == 0)
                return;
            float x = new Global(rot).Get<float>(), y = new Global(rot + 1).Get<float>(), z = new Global(rot + 2).Get<float>();
            if ((x == 0 && y == 0 && z == 0) || (x == 999 && y == 999 && z == 999))
                return;
            value %= 360;
            if (value > 180) value -= 360;
            if (value < -180) value += 360;
            new Global(rot + 2).SetFloat(value);
        }
    }
}
