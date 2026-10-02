using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of MainWindow: Mission / Blips page (the page itself is Controls/DummyBlipsView).
    public partial class MainWindow
    {
        private void BtnmissionddblipAdd_Click(object sender, RoutedEventArgs e)
        {
            // New blips need a team and a rule or they never show in a test (see DummyBlipsView).
            _dummyBlips?.AddAtCursor();
        }

        private void BtnmissionddblipDelete_Click(object sender, RoutedEventArgs e)
        {
            _dummyBlips?.DeleteSelected();
        }

        // Dummy blips exist in the mission creators only (LTS, Capture, Mission); greyed out elsewhere.
        private void UpdateBlipButtons()
        {
            string letter = m.IsProcOpen ? CreatorLetter(GTA.CurrentCreatorName()) : "";
            bool blips = letter.Length == 1 && "LCM".Contains(letter) && GTA.Offsets.Editor.ddblip.number != 0;
            BtnVehBlip.IsEnabled = blips;
            BtnActorBlip.IsEnabled = blips;
            if (!_blipButtonsWired)
            {
                _blipButtonsWired = true;
                ddvehno.SelectionChanged += (_, __) => UpdateBlipButtons();
                ddactorno.SelectionChanged += (_, __) => UpdateBlipButtons();
            }
            // An entry that has a blip already opens it instead of adding a second one.
            BlipButtonText(BtnVehBlip, blips ? DummyBlipsView.FindFor(EntityPicker.Vehicle, ddvehno.SelectedIndex) : -1);
            BlipButtonText(BtnActorBlip, blips ? DummyBlipsView.FindFor(EntityPicker.Actor, ddactorno.SelectedIndex) : -1);
        }

        private bool _blipButtonsWired;

        private void BlipButtonText(Button button, int blip)
        {
            button.Content = blip < 0 ? TranslateOr("bl_create_for", "+ Blip")
                : string.Format(System.Globalization.CultureInfo.CurrentCulture, TranslateOr("bl_open_for", "Blip #{0} ›"), blip + 1);
            button.ToolTip = blip < 0 ? TranslateOr("bl_create_for_tip", "Creates a blip on the Blips page that follows this entry")
                : TranslateOr("bl_open_for_tip", "This entry has a blip already; opens it on the Blips page");
        }

        private void BtnVehBlip_Click(object sender, RoutedEventArgs e)
        {
            int i = ddvehno.SelectedIndex;
            if (!m.IsProcOpen || i < 0 || GTA.Offsets.Editor.Vehicle.loc == 0)
                return;
            long at = GTA.Offsets.Editor.Vehicle.loc + i * GTA.Offsets.Editor.Vehicle.NEXT;
            CreateBlipFor(EntityPicker.Vehicle, i, new Global(at).Get<float>(), new Global(at + 1).Get<float>(), new Global(at + 2).Get<float>());
        }

        private void BtnActorBlip_Click(object sender, RoutedEventArgs e)
        {
            int i = ddactorno.SelectedIndex;
            if (!m.IsProcOpen || i < 0 || GTA.Offsets.Editor.Actor.locx == 0)
                return;
            long next = GTA.Offsets.Editor.Actor.NEXT;
            CreateBlipFor(EntityPicker.Actor, i, new Global(GTA.Offsets.Editor.Actor.locx + i * next).Get<float>(),
                new Global(GTA.Offsets.Editor.Actor.locy + i * next).Get<float>(), new Global(GTA.Offsets.Editor.Actor.locz + i * next).Get<float>());
        }

        // Opens the Blips page and adds a blip there that follows the entity, picked for editing.
        private void CreateBlipFor(int entityType, int index, float x, float y, float z)
        {
            BtnSectionMission_Click(null, null);
            ClickButton(BtnMissionBlips);
            int existing = DummyBlipsView.FindFor(entityType, index);
            // The page builds its view on the first visit; act once it is laid out.
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (existing >= 0)
                    _dummyBlips?.Open(existing);
                else
                    _dummyBlips?.AddFollowing(entityType, index, x, y, z);
            }), DispatcherPriority.Loaded);
        }
    }
}
