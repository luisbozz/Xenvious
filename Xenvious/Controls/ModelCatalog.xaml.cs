using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>One model in the catalog. The picture loads the first time a row shows it.</summary>
    public sealed class CatalogItem : INotifyPropertyChanged
    {
        private ImageSource _thumb;
        private bool _thumbRequested;

        public CatalogItem(string name, string native, uint hash, string category, string imageKind)
        {
            Name = name;
            Native = native;
            Hash = hash;
            Category = category ?? "";
            ImageKind = imageKind;
        }

        public string Name { get; }
        public string Native { get; }
        public uint Hash { get; }
        public int Int32 => unchecked((int)Hash);
        public string Category { get; }
        public string ImageKind { get; }

        public string Detail => (string.IsNullOrEmpty(Native) || Native == Name ? "" : Native + " · ")
            + "0x" + Hash.ToString("X8", CultureInfo.InvariantCulture);

        public bool IsFavorite
        {
            get => ModelCatalogStore.IsFavorite(ImageKind, Hash);
            set
            {
                ModelCatalogStore.SetFavorite(ImageKind, Hash, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFavorite)));
            }
        }

        public ImageSource Thumb
        {
            get
            {
                if (!_thumbRequested)
                {
                    _thumbRequested = true;
                    LoadThumbAsync();
                }
                return _thumb;
            }
        }

        private async void LoadThumbAsync()
        {
            var image = await ModelImageCache.GetAsync(ImageKind, Native, Hash);
            if (image == null)
                return;
            _thumb = image;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thumb)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// The model catalog over the whole window (no separate window, so the game keeps focus).
    /// Show() fills it; the callback gets the chosen model.
    /// </summary>
    public partial class ModelCatalog : UserControl
    {
        private sealed class Category
        {
            public string Name { get; set; }
            public int Count { get; set; }
            public string Key { get; set; }   // null: all
        }

        private const string FavoritesKey = "\u0001favorites";
        private const string RecentKey = "\u0001recent";

        private List<CatalogItem> _items = new List<CatalogItem>();
        private string _kind = "";
        private Action<CatalogItem> _onPick;

        public ModelCatalog()
        {
            InitializeComponent();
        }

        public void Show(string title, IEnumerable<CatalogItem> items, uint currentHash, Action<CatalogItem> onPick)
        {
            _items = items.ToList();
            _onPick = onPick;
            _kind = _items.FirstOrDefault()?.ImageKind ?? "";
            TitleText.Text = title;

            var categories = new List<Category>
            {
                new Category { Name = Translate("catalog_all", "All"), Count = _items.Count },
                new Category { Name = "★ " + Translate("catalog_favorites", "Favourites"), Key = FavoritesKey, Count = _items.Count(i => i.IsFavorite) },
                new Category { Name = Translate("catalog_recent", "Recently used"), Key = RecentKey, Count = ModelCatalogStore.RecentHashes(_kind).Count },
            };
            categories.AddRange(_items.GroupBy(i => i.Category).Where(g => g.Key.Length > 0).OrderBy(g => g.Key)
                .Select(g => new Category { Name = g.Key, Key = g.Key, Count = g.Count() }));
            CategoryList.ItemsSource = categories;
            // Start on the favourites when there are any, otherwise on everything.
            CategoryList.SelectedIndex = categories[1].Count > 0 ? 1 : 0;

            SearchBox.Text = "";
            Filter();
            var current = _items.FirstOrDefault(i => i.Hash == currentHash);
            if (current != null)
            {
                ItemList.SelectedItem = current;
                ItemList.ScrollIntoView(current);
            }
            Visibility = Visibility.Visible;
            SearchBox.Focus();
        }

        private void Filter()
        {
            string query = SearchBox.Text.Trim();
            string category = (CategoryList.SelectedItem as Category)?.Key;
            IEnumerable<CatalogItem> result = _items;
            if (category == FavoritesKey)
                result = result.Where(i => i.IsFavorite);
            else if (category == RecentKey)
            {
                var recent = ModelCatalogStore.RecentHashes(_kind).ToList();
                result = result.Where(i => recent.Contains(i.Hash)).OrderBy(i => recent.IndexOf(i.Hash));
            }
            else if (category != null)
                result = result.Where(i => i.Category == category);
            if (query.Length > 0)
            {
                string hex = query.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? query.Substring(2) : query;
                result = result.Where(i =>
                    i.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                    || (i.Native ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                    || i.Hash.ToString(CultureInfo.InvariantCulture) == query
                    || i.Int32.ToString(CultureInfo.InvariantCulture) == query
                    || i.Hash.ToString("X8", CultureInfo.InvariantCulture).Equals(hex.PadLeft(8, '0'), StringComparison.OrdinalIgnoreCase));
            }
            var list = result.ToList();
            ItemList.ItemsSource = list;
            HitCount.Text = string.Format(CultureInfo.CurrentCulture, Translate("catalog_hits", "{0} models"), list.Count);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => Filter();

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e) => Filter();

        private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ItemList.SelectedItem as CatalogItem;
            TakeButton.IsEnabled = item != null;
            PreviewName.Text = item?.Name ?? "";
            PreviewNative.Text = item == null || item.Native == item.Name ? "" : item.Native;
            PreviewHash.Text = item == null ? "" : item.Hash.ToString(CultureInfo.InvariantCulture) + " · 0x" + item.Hash.ToString("X8", CultureInfo.InvariantCulture);
            PreviewCategory.Text = item?.Category ?? "";
            PreviewImage.Source = item?.Thumb;
            if (item != null)
                item.PropertyChanged += PreviewThumbArrived;
        }

        private void PreviewThumbArrived(object sender, PropertyChangedEventArgs e)
        {
            if (sender == ItemList.SelectedItem)
                PreviewImage.Source = ((CatalogItem)sender).Thumb;
            ((CatalogItem)sender).PropertyChanged -= PreviewThumbArrived;
        }

        private void Take()
        {
            if (!(ItemList.SelectedItem is CatalogItem item))
                return;
            Visibility = Visibility.Collapsed;
            ModelCatalogStore.AddRecent(item.ImageKind, item.Hash);
            _onPick?.Invoke(item);
        }

        private void Take_Click(object sender, RoutedEventArgs e) => Take();

        private void ItemList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Take();

        private void Close_Click(object sender, RoutedEventArgs e) => Visibility = Visibility.Collapsed;

        private void Catalog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (ItemList.SelectedItem == null && ItemList.Items.Count > 0)
                    ItemList.SelectedIndex = 0;
                Take();
                e.Handled = true;
            }
            else if ((e.Key == Key.Down || e.Key == Key.Up) && SearchBox.IsKeyboardFocused && ItemList.Items.Count > 0)
            {
                int index = ItemList.SelectedIndex + (e.Key == Key.Down ? 1 : -1);
                ItemList.SelectedIndex = Math.Max(0, Math.Min(ItemList.Items.Count - 1, index));
                ItemList.ScrollIntoView(ItemList.SelectedItem);
                e.Handled = true;
            }
        }

        // A click on the dimmed area closes; clicks inside the panel do not bubble up to it.
        private void Backdrop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Visibility = Visibility.Collapsed;

        private void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => e.Handled = true;

        /// <summary>Loads the pictures of the favourites and recently used models in the background,
        /// so the catalog opens with them ready. Nothing else is fetched ahead.</summary>
        public static void Preload(IEnumerable<CatalogItem> items)
        {
            foreach (var item in items)
            {
                if (item.IsFavorite || ModelCatalogStore.RecentHashes(item.ImageKind).Contains(item.Hash))
                    _ = item.Thumb;
            }
        }

        private static string Translate(string key, string fallback)
        {
            return Application.Current?.MainWindow is MainWindow window ? window.TranslateOr(key, fallback) : fallback;
        }
    }
}
