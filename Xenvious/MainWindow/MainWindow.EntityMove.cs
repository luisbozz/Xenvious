using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of MainWindow: position, rotation and heading edits of props, dynamic props, capture
    // objects, vehicles, actors, weapons and team start points show live in the creator (EntityMover)
    // instead of after a rebuild.
    public partial class MainWindow
    {
        // The entity a position, rotation or heading box edits, by its name; null for other boxes.
        private (EntityMover.Kind Kind, int Index, int Team)? MoveTarget(object sender)
        {
            if (!(sender is TextBox box) || box.Name == null)
                return null;
            string n = box.Name;
            bool Pose(string prefix) => n.StartsWith(prefix + "loc") || n.StartsWith(prefix + "rot") || n == prefix + "head";
            if (Pose("tbprops"))
                return (EntityMover.Kind.Prop, ddpropno.SelectedIndex, 0);
            if (Pose("tbdprops"))
                return (EntityMover.Kind.DynamicProp, dddpropno.SelectedIndex, 0);
            if (Pose("tbobj"))
                return (EntityMover.Kind.Object, ddobjno.SelectedIndex, 0);
            if (Pose("tbveh"))
                return (EntityMover.Kind.Vehicle, ddvehno.SelectedIndex, 0);
            if (Pose("tbactor"))
                return (EntityMover.Kind.Actor, ddactorno.SelectedIndex, 0);
            if (Pose("tbweap") || n == "tbweapheading")
                return (EntityMover.Kind.Weapon, ddweapno.SelectedIndex, 0);
            if (Pose("tbplyr"))
                return (EntityMover.Kind.StartPoint, ddplyrno.SelectedIndex, ddplyrteam.SelectedIndex);
            return null;
        }

        private void InitEntityMove()
        {
            // The boxes' own TextChanged handlers write the job data first (the event bubbles up
            // from the box); the move waits until they ran. Typing and heading slider drags end
            // here; "get location" calls RefreshAfterEdit itself. When a page loads another entity
            // its boxes change too: moving the entity then would replace the creator's own
            // placement for nothing.
            AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((sender, e) =>
            {
                if (!(e.OriginalSource is TextBox box) || !box.IsKeyboardFocusWithin && !HeadingSlider.Writing)
                    return;
                var t = MoveTarget(box);
                if (t != null)
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(async () =>
                        await EntityMover.MoveAsync(t.Value.Kind, t.Value.Index, t.Value.Team)));
            }));
        }

        /// <summary>Shows an edit of this box: moves its entity, or rebuilds when that does not work.</summary>
        private async void RefreshAfterEdit(object sender)
        {
            var t = MoveTarget(sender);
            if (t != null && await EntityMover.MoveAsync(t.Value.Kind, t.Value.Index, t.Value.Team))
                return;
            creatorRefresh();
        }
    }
}
