using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Xenvious
{
    // Tells EntityMarks which entry the open page shows, so the drawer marks it in game.
    public partial class MainWindow
    {
        private DispatcherTimer _entityMarkTimer;

        private void InitEntityMarks()
        {
            var pages = new (ComboBox Box, EntityMarks.Kind Kind)[]
            {
                (ddactorno, EntityMarks.Kind.Actor),
                (ddvehno, EntityMarks.Kind.Vehicle),
                (ddobjno, EntityMarks.Kind.Object),
                (ddweapno, EntityMarks.Kind.Weapon),
                (ddpropno, EntityMarks.Kind.Prop),
                (dddpropno, EntityMarks.Kind.DynamicProp),
                (dddoorsno, EntityMarks.Kind.Door),
            };
            // A visible index box means its page is open; only one is at a time.
            _entityMarkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _entityMarkTimer.Tick += (_, __) =>
            {
                foreach (var (box, kind) in pages)
                    if (box.IsVisible && box.SelectedIndex >= 0)
                    {
                        EntityMarks.Select(kind, box.SelectedIndex);
                        return;
                    }
                EntityMarks.Select(EntityMarks.Kind.None, -1);
            };
            _entityMarkTimer.Start();
        }
    }
}
