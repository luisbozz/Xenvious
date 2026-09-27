using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
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
    /// A grouped list (disabled header rows + SearchItem rows) with a search box at the top of its drop-down that
    /// filters by name or number and highlights the match. The list is flat on purpose: a grouped
    /// CollectionView showed only the headers in the window's ComboBox template.
    /// </summary>
    public sealed class SearchableCombo
    {
        private readonly ComboBox _combo;
        private readonly TextBox _search;
        private readonly TextBlock _hint;
        private List<SearchItem> _all = new List<SearchItem>();
        private Func<string, string> _groupName = g => g;
        private string[] _groupOrder = new string[0];
        public bool Syncing { get; private set; }

        public SearchableCombo(ComboBox combo)
        {
            _combo = combo;
            // A bare text line, no box: the grey hint sits in it until something is typed.
            _search = new TextBox { Background = Brushes.Transparent, BorderThickness = new Thickness(0), Padding = new Thickness(8, 7, 8, 7), FontSize = 14 };
            _search.SetResourceReference(Control.ForegroundProperty, "TextColor");
            _search.SetResourceReference(TextBoxBase.CaretBrushProperty, "TextColor");
            _hint = new TextBlock
            {
                Text = MainWindow.Instance?.TranslateOr("search_type", "Search: name or number") ?? "Search: name or number",
                Margin = new Thickness(11, 7, 8, 7), FontSize = 14, IsHitTestVisible = false,
            };
            _hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _search.TextChanged += (_, __) => _hint.Visibility = _search.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            // Typing goes to the search box, not to the combo's own jump-to-letter search.
            _combo.IsTextSearchEnabled = false;
            _combo.ItemTemplate = (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:l='clr-namespace:Xenvious;assembly=" + typeof(Highlight).Assembly.GetName().Name + "'>" +
                "<TextBlock TextTrimming='CharacterEllipsis' l:Highlight.Text='{Binding Text}' l:Highlight.Query='{Binding Query}'/></DataTemplate>");
            _search.TextChanged += (_, __) => Rebuild();
            _combo.DropDownOpened += (_, __) =>
            {
                PutSearchIntoPopup();
                _search.Dispatcher.BeginInvoke(new Action(() => { _search.Focus(); Keyboard.Focus(_search); }), System.Windows.Threading.DispatcherPriority.Input);
            };
            _combo.DropDownClosed += (_, __) =>
            {
                if (_search.Text.Length > 0)
                    _search.Text = "";
            };
        }

        // The search box sits at the top of the drop-down itself (ComboBox.xaml: DropDownBorder), so
        // the list and the box share the frame. Done once per combo, each has its own template copy.
        private void PutSearchIntoPopup()
        {
            if (_search.Parent != null || !(_combo.Template?.FindName("DropDownBorder", _combo) is Border border) || border.Child == null)
                return;
            var list = border.Child;
            border.Child = null;
            var field = new Grid();
            field.Children.Add(_search);
            field.Children.Add(_hint);
            var line = new Border { BorderThickness = new Thickness(0, 0, 0, 1), Child = field };
            line.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            var panel = new DockPanel();
            DockPanel.SetDock(line, Dock.Top);
            panel.Children.Add(line);
            panel.Children.Add(list);
            border.Child = panel;
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
