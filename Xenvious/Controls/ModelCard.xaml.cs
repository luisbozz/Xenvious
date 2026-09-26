using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    /// <summary>
    /// Compact model display for the Props, Dynamic Props and Actor pages (replaces the old
    /// category / list column). The page tells it the current model with SetModel; it raises
    /// CatalogRequested for the catalog and Picked when a chip is clicked.
    /// </summary>
    public partial class ModelCard : UserControl
    {
        private uint _hash;
        private bool _hasModel;

        public ModelCard()
        {
            InitializeComponent();
        }

        /// <summary>The list the card looks models up in ("prop" or "actor" items).</summary>
        public Func<IReadOnlyList<CatalogItem>> Items { get; set; }

        public event EventHandler CatalogRequested;
        public event EventHandler<CatalogItem> Picked;

        /// <summary>Shows a model; called by the page refresh, so it only redraws on a change.</summary>
        public void SetModel(uint hash)
        {
            if (_hasModel && hash == _hash)
                return;
            _hasModel = true;
            _hash = hash;
            var item = Items?.Invoke()?.FirstOrDefault(i => i.Hash == hash);
            NameText.Text = item?.Name ?? (hash == 0 ? "–" : hash.ToString(CultureInfo.InvariantCulture));
            DetailText.Text = item?.Detail ?? "0x" + hash.ToString("X8", CultureInfo.InvariantCulture);
            CategoryText.Text = item?.Category ?? "";
            Thumb.Source = item?.Thumb;
            if (item != null)
                item.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CatalogItem.Thumb) && _hash == item.Hash) Thumb.Source = item.Thumb; };
            RefreshChips();
        }

        /// <summary>Rebuilds the recent and favourite chips (after a pick or a favourite change).</summary>
        public void RefreshChips()
        {
            var items = Items?.Invoke();
            if (items == null || items.Count == 0)
                return;
            string kind = items[0].ImageKind;
            var byHash = new Dictionary<uint, CatalogItem>();
            foreach (var item in items)
                byHash[item.Hash] = item;

            Fill(RecentChips, RecentLabel, ModelCatalogStore.RecentHashes(kind).Take(6), byHash);
            Fill(FavoriteChips, FavoriteLabel, ModelCatalogStore.FavoriteHashes(kind).Take(12), byHash);
        }

        private void Fill(Panel panel, FrameworkElement label, IEnumerable<uint> hashes, Dictionary<uint, CatalogItem> byHash)
        {
            panel.Children.Clear();
            foreach (uint hash in hashes)
            {
                if (!byHash.TryGetValue(hash, out var item))
                    continue;
                var chip = new Button { Style = (Style)FindResource("Chip"), Content = item.Name, ToolTip = item.Detail };
                chip.Click += (_, __) => Picked?.Invoke(this, item);
                panel.Children.Add(chip);
            }
            label.Visibility = panel.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e) => CatalogRequested?.Invoke(this, EventArgs.Empty);
    }
}
