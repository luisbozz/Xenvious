using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: the entry bar on top of the edit pages (‹ number › stepping).
    public partial class MainWindow
    {
        // Tag is the page's number dropdown, CommandParameter the step (-1 or 1). Selecting
        // through the dropdown runs the page's own SelectionChanged, as a pick from the list does.
        private void EntryStep_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is ComboBox entries) || entries.Items.Count == 0)
                return;
            int step = int.Parse((string)button.CommandParameter, CultureInfo.InvariantCulture);
            int index = entries.SelectedIndex < 0 ? (step > 0 ? 0 : entries.Items.Count - 1) : entries.SelectedIndex + step;
            entries.SelectedIndex = Math.Max(0, Math.Min(entries.Items.Count - 1, index));
        }
    }
}
