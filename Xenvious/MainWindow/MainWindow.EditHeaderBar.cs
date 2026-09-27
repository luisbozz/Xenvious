using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Xenvious
{
    // Part of MainWindow: moves the entry bar of the open edit page (Tag="EntryBar") into the header row.
    public partial class MainWindow
    {
        private Border _headerBar;
        private Panel _headerBarHome;
        private int _headerBarIndex;
        private Thickness _headerBarMargin;

        /// <summary>
        /// Puts the entry bar of the page that is open now into the header, next to the title. The
        /// bar stays the same element (names, handlers and bindings keep working); it only changes
        /// its parent and goes back when another page opens.
        /// </summary>
        private void MoveEntryBarToHeader()
        {
            RestoreEntryBar();
            var bar = FindEntryBar(EditPages.SelectedItem as TabItem);
            if (bar == null || !(bar.Parent is Panel home))
                return;
            _headerBarHome = home;
            _headerBarIndex = home.Children.IndexOf(bar);
            _headerBarMargin = bar.Margin;
            home.Children.Remove(bar);
            bar.Margin = new Thickness(0);
            EditHeaderBar.Content = bar;
            _headerBar = bar;
        }

        private void RestoreEntryBar()
        {
            if (_headerBar == null)
                return;
            EditHeaderBar.Content = null;
            _headerBar.Margin = _headerBarMargin;
            _headerBarHome.Children.Insert(Math.Min(_headerBarIndex, _headerBarHome.Children.Count), _headerBar);
            _headerBar = null;
        }

        // Only the selected tab of nested TabControls counts, so Props finds the bar of Normal or Dynamic.
        private static Border FindEntryBar(object node)
        {
            if (node is TabItem tab)
                return FindEntryBar(tab.Content);
            if (node is TabControl tabs)
                return FindEntryBar(tabs.SelectedItem);
            if (node is Border border && Equals(border.Tag, "EntryBar"))
                return border;
            if (!(node is DependencyObject element))
                return null;
            foreach (object child in LogicalTreeHelper.GetChildren(element))
            {
                var found = FindEntryBar(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void QueueEntryBarMove()
        {
            // After the tab switch has settled, so the new page's content is the selected one.
            Dispatcher.BeginInvoke(new Action(MoveEntryBarToHeader), DispatcherPriority.Loaded);
        }
    }
}
