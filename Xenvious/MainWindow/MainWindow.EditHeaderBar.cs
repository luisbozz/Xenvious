using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
            SelectFirstEntry(bar);
        }

        // Opening a page shows its first entry right away instead of an empty page. Entries that
        // arrive later (the worker fills the list) are picked up as long as the page is open.
        private void SelectFirstEntry(DependencyObject bar)
        {
            foreach (var entries in EntryLists(bar))
            {
                if (entries.SelectedIndex < 0 && entries.Items.Count > 0)
                    entries.SelectedIndex = 0;
                if (_watchedEntryLists.Add(entries))
                    ((INotifyCollectionChanged)entries.Items).CollectionChanged += (_, __) =>
                    {
                        if (_headerBar != null && EntryLists(_headerBar).Contains(entries) && entries.SelectedIndex < 0 && entries.Items.Count > 0)
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (entries.SelectedIndex < 0 && entries.Items.Count > 0)
                                    entries.SelectedIndex = 0;
                            }));
                    };
            }
        }

        private readonly HashSet<ComboBox> _watchedEntryLists = new HashSet<ComboBox>();

        // The ‹ › buttons of a bar carry their entry list in Tag.
        private static IEnumerable<ComboBox> EntryLists(object node)
        {
            if (node is Button button && button.Tag is ComboBox entries)
            {
                yield return entries;
                yield break;
            }
            if (!(node is DependencyObject element))
                yield break;
            var seen = new HashSet<ComboBox>();
            foreach (object child in LogicalTreeHelper.GetChildren(element))
                foreach (var found in EntryLists(child))
                    if (seen.Add(found))
                        yield return found;
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
