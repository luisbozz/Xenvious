using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>An entry of a searchable list: its text and the current search, for highlighting.</summary>
    public class SearchItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Group { get; set; }
        public string Query { get; set; } = "";
        public override string ToString() => Text;
    }

    /// <summary>Shows Text with the part that matches Query in bold and the accent colour.</summary>
    public static class Highlight
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text", typeof(string), typeof(Highlight), new PropertyMetadata("", Update));
        public static readonly DependencyProperty QueryProperty = DependencyProperty.RegisterAttached(
            "Query", typeof(string), typeof(Highlight), new PropertyMetadata("", Update));

        public static void SetText(DependencyObject d, string v) => d.SetValue(TextProperty, v);
        public static string GetText(DependencyObject d) => (string)d.GetValue(TextProperty);
        public static void SetQuery(DependencyObject d, string v) => d.SetValue(QueryProperty, v);
        public static string GetQuery(DependencyObject d) => (string)d.GetValue(QueryProperty);

        private static Brush Accent => MainWindow.ThemeBrush("WarnBrush");

        private static void Update(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBlock block))
                return;
            string text = GetText(block) ?? "", query = GetQuery(block) ?? "";
            block.Inlines.Clear();
            int at = query.Length == 0 ? -1 : text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
            if (at < 0)
            {
                block.Inlines.Add(new Run(text));
                return;
            }
            block.Inlines.Add(new Run(text.Substring(0, at)));
            block.Inlines.Add(new Run(text.Substring(at, query.Length)) { FontWeight = FontWeights.Bold, Foreground = Accent });
            block.Inlines.Add(new Run(text.Substring(at + query.Length)));
        }
    }

    /// <summary>
    /// A grouped list (disabled header rows + SearchItem rows) with a search box above it that
    /// filters by name or number and highlights the match. The list is flat on purpose: a grouped
    /// CollectionView showed only the headers in the window's ComboBox template.
    /// </summary>
    public sealed class SearchableCombo
    {
        private readonly ComboBox _combo;
        private readonly TextBox _search;
        private List<SearchItem> _all = new List<SearchItem>();
        private Func<string, string> _groupName = g => g;
        private string[] _groupOrder = new string[0];
        public bool Syncing { get; private set; }

        public SearchableCombo(ComboBox combo, TextBox search)
        {
            _combo = combo;
            _search = search;
            _combo.ItemTemplate = (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:l='clr-namespace:Xenvious;assembly=" + typeof(Highlight).Assembly.GetName().Name + "'>" +
                "<TextBlock TextTrimming='CharacterEllipsis' l:Highlight.Text='{Binding Text}' l:Highlight.Query='{Binding Query}'/></DataTemplate>");
            _search.TextChanged += (_, __) =>
            {
                Rebuild();
                if (_search.IsKeyboardFocusWithin && _search.Text.Length > 0)
                    _combo.IsDropDownOpen = true;
            };
        }

        public void SetItems(IEnumerable<SearchItem> items, string[] groupOrder, Func<string, string> groupName)
        {
            _all = items.ToList();
            _groupOrder = groupOrder;
            _groupName = groupName;
            Rebuild();
        }

        public SearchItem Selected => _combo.SelectedItem as SearchItem;

        public void Select(int id)
        {
            Syncing = true;
            _combo.SelectedItem = _combo.Items.OfType<SearchItem>().FirstOrDefault(i => i.Id == id);
            Syncing = false;
        }

        private void Rebuild()
        {
            string q = _search.Text.Trim();
            int keep = Selected?.Id ?? int.MinValue;
            var list = new List<object>();
            foreach (var group in _all.GroupBy(i => i.Group).OrderBy(g => Array.IndexOf(_groupOrder, g.Key)))
            {
                var hits = group.Where(i => q.Length == 0 || i.Text.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                    || i.Id.ToString(CultureInfo.InvariantCulture) == q).ToList();
                if (hits.Count == 0)
                    continue;
                string name = _groupName(group.Key);
                if (!string.IsNullOrEmpty(name))
                    list.Add(new ComboBoxItem
                    {
                        Content = name, IsEnabled = false, FontSize = 11, FontWeight = FontWeights.Bold,
                        Foreground = (Brush)_combo.FindResource("NavMutedBrush"), Margin = new Thickness(0, list.Count == 0 ? 2 : 8, 0, 0),
                    });
                foreach (var hit in hits)
                    hit.Query = q;
                list.AddRange(hits);
            }
            Syncing = true;
            _combo.ItemsSource = list;
            _combo.SelectedItem = list.OfType<SearchItem>().FirstOrDefault(i => i.Id == keep);
            Syncing = false;
        }
    }
}
