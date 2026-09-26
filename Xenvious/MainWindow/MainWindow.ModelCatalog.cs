using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Xenvious
{
    // Part of MainWindow: the model catalog on the Props, Dynamic Props and Actor pages, and its picture cache in Settings.
    public partial class MainWindow
    {
        private List<CatalogItem> _propCatalog;
        private List<CatalogItem> _actorCatalog;

        // Props and dynamic props share one list, and with it the same pictures. An empty list
        // (asked for before the offline data was loaded) is built again next time.
        private List<CatalogItem> PropCatalog => _propCatalog?.Count > 0 ? _propCatalog : (_propCatalog = GTA.Editor.PropList
            .Select(p => new CatalogItem(p.Name, p.Native, p.UInt, p.Category, "prop")).ToList());

        // actors.json has display names only, no model names, so actors have no pictures yet.
        private List<CatalogItem> ActorCatalog => _actorCatalog?.Count > 0 ? _actorCatalog : (_actorCatalog = GTA.Editor.ActorList
            .Select(a => new CatalogItem(a.Name, null, a.UInt32, "", "actor")).ToList());

        private void InitModelCards()
        {
            Wire(PropModelCard, () => PropCatalog, TranslateOr("prop", "Props"), () => (GTA.Offsets.Editor.Props.model, GTA.Offsets.Editor.Props.NEXT, ddpropno.SelectedIndex));
            Wire(DPropModelCard, () => PropCatalog, TranslateOr("dprop", "Dynamic Props"), () => (GTA.Offsets.Editor.DProps.model, GTA.Offsets.Editor.DProps.NEXT, dddpropno.SelectedIndex));
            Wire(ActorModelCard, () => ActorCatalog, TranslateOr("actor", "Actors"), () => (GTA.Offsets.Editor.Actor.model, GTA.Offsets.Editor.Actor.NEXT, ddactorno.SelectedIndex));
        }

        // Offsets are read when used, because OffsetLoader can load them again for the other edition.
        private void Wire(ModelCard card, Func<List<CatalogItem>> items, string title, Func<(long Model, long Stride, int Index)> target)
        {
            card.Items = () => items();
            card.CatalogRequested += (_, __) =>
            {
                var (model, stride, index) = target();
                uint current = m.IsProcOpen && index >= 0 && model != 0 ? unchecked((uint)new Global(model + stride * index).Get<int>()) : 0;
                ModelCatalogOverlay.Show(title, items(), current, item => WriteModel(card, target, item));
            };
            card.Picked += (_, item) =>
            {
                ModelCatalogStore.AddRecent(item.ImageKind, item.Hash);
                WriteModel(card, target, item);
            };
        }

        // Writes the model into the selected entry; the page shows it on its next refresh,
        // the same as a pick from its own list.
        private void WriteModel(ModelCard card, Func<(long Model, long Stride, int Index)> target, CatalogItem item)
        {
            var (model, stride, index) = target();
            if (!m.IsProcOpen || index < 0 || model == 0)
            {
                displayScreenMessage(TranslateOr("catalog_noentry", "Select an entry in the creator first."));
                return;
            }
            new Global(model + stride * index).SetInt(item.Int32);
            card.SetModel(item.Hash);
            card.RefreshChips();
        }

        // ----- Settings: picture cache -----

        private bool _updatingCacheSettings;

        private void UpdateModelCacheSettings()
        {
            if (cbModelCache == null || tbModelCacheUsage == null)
                return;
            _updatingCacheSettings = true;
            cbModelCache.IsChecked = ModelImageCache.Enabled;
            _updatingCacheSettings = false;

            var (files, bytes) = ModelImageCache.Usage();
            long estimate = (long)PropCatalog.Count * ModelImageCache.AverageThumbBytes;
            tbModelCacheUsage.Text = string.Format(CultureInfo.CurrentCulture,
                TranslateOr("cache_usage", "{0:N0} pictures, {1:N1} MB. All props would take about {2:N0} MB."),
                files, bytes / 1048576.0, estimate / 1048576.0);
        }

        private void cbModelCache_Changed(object sender, RoutedEventArgs e)
        {
            if (_updatingCacheSettings)
                return;
            ModelImageCache.Enabled = cbModelCache.IsChecked == true;
        }

        private void BtnModelCacheClear_Click(object sender, RoutedEventArgs e)
        {
            ModelImageCache.Clear();
            UpdateModelCacheSettings();
        }
    }
}
