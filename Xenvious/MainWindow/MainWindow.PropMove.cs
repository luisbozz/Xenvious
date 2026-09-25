using System.Globalization;
using System.Windows;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: moving props between the static and the dynamic list.
    public partial class MainWindow
    {
        private async void BtnPropsToDynamic_Click(object sender, RoutedEventArgs e)
        {
            await MoveProp(toDynamic: true, index: ddpropno.SelectedIndex);
        }

        private async void BtnDPropsToStatic_Click(object sender, RoutedEventArgs e)
        {
            await MoveProp(toDynamic: false, index: dddpropno.SelectedIndex);
        }

        private async System.Threading.Tasks.Task MoveProp(bool toDynamic, int index)
        {
            if (!m.IsProcOpen || !IsCreatorRunning())
                return;
            string problem = PropMover.Check(toDynamic, index);
            if (problem != null)
            {
                await ConfirmAsync(
                    TranslateOr("props_move_title", "Verschieben nicht möglich"),
                    problem == "full"
                        ? TranslateOr("props_move_full", "Die andere Liste ist voll.")
                        : TranslateOr("props_move_select", "Wähle zuerst einen Prop aus."),
                    "OK", null);
                return;
            }

            bool rebuilt = await PropMover.MoveAsync(toDynamic, index);
            Log.Info(string.Format(CultureInfo.InvariantCulture, "prop {0} moved to the {1} list (rebuild {2})",
                index, toDynamic ? "dynamic" : "static", rebuilt ? "ok" : "not done"), source: "props");
            if (!rebuilt)
            {
                await ConfirmAsync(
                    TranslateOr("props_move_title_done", "Verschoben"),
                    TranslateOr("map_restored_norebuild", "Daten geladen, aber der Creator hat die Map nicht neu aufgebaut."),
                    "OK", null);
            }
        }
    }
}
