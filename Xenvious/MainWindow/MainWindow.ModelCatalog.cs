using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: the model catalog on the Props, Dynamic Props, Vehicle, Weapon and Actor pages, and its picture cache in Settings.
    public partial class MainWindow
    {
        private List<CatalogItem> _propCatalog;
        private List<CatalogItem> _actorCatalog;
        private List<CatalogItem> _vehicleCatalog;
        private List<CatalogItem> _weaponCatalog;

        // Props and dynamic props share one list, and with it the same pictures. An empty list
        // (asked for before the offline data was loaded) is built again next time.
        private List<CatalogItem> PropCatalog => _propCatalog?.Count > 0 ? _propCatalog : (_propCatalog = GTA.Editor.PropList
            .Select(p => new CatalogItem(p.Name, p.Native, p.UInt, p.Category, "prop")).ToList());

        // actors.json has display names only; the model name and the category come from the
        // creator's own ped lists (ActorCategories). Models the creator offers that are missing
        // from actors.json are added under their model name. No picture source for peds yet.
        private List<CatalogItem> ActorCatalog => _actorCatalog?.Count > 0 ? _actorCatalog : (_actorCatalog = BuildActorCatalog());

        private List<CatalogItem> BuildActorCatalog()
        {
            string CategoryName(uint hash) => ActorCategories.ByHash.TryGetValue(hash, out var entry)
                ? TranslateOr("actorcat_" + entry.Category, "")
                : TranslateOr("actorcat_other", "Other");

            var items = GTA.Editor.ActorList
                .Select(a => new CatalogItem(a.Name, ActorCategories.ByHash.TryGetValue(a.UInt32, out var e) ? e.Model : null, a.UInt32, CategoryName(a.UInt32), "actor"))
                .ToList();
            if (items.Count == 0)
                return items;
            var known = new HashSet<uint>(items.Select(i => i.Hash));
            items.AddRange(ActorCategories.ByHash
                .Where(p => !known.Contains(p.Key))
                .Select(p => new CatalogItem(p.Value.Model, p.Value.Model, p.Key, CategoryName(p.Key), "actor")));
            return items;
        }

        // vehicles.json has model names only; there is no picture source for vehicles yet.
        private List<CatalogItem> VehicleCatalog => _vehicleCatalog?.Count > 0 ? _vehicleCatalog : (_vehicleCatalog = GTA.Editor.VehList
            .Select(v => new CatalogItem(v.Native, v.Native, v.Uint32, v.Category, "vehicle")).ToList());

        // weapons.json has weapon names only, no pictures either.
        private List<CatalogItem> WeaponCatalog => _weaponCatalog?.Count > 0 ? _weaponCatalog : (_weaponCatalog = GTA.Editor.WeaponList
            .Select(w => new CatalogItem(w.Native, w.Native, w.UInt32, w.Category, "weapon")).ToList());

        private void InitModelCards()
        {
            Wire(PropModelCard, ddpropno, () => PropCatalog, TranslateOr("prop", "Props"), () => (GTA.Offsets.Editor.Props.model, GTA.Offsets.Editor.Props.NEXT, ddpropno.SelectedIndex));
            Wire(DPropModelCard, dddpropno, () => PropCatalog, TranslateOr("dprop", "Dynamic Props"), () => (GTA.Offsets.Editor.DProps.model, GTA.Offsets.Editor.DProps.NEXT, dddpropno.SelectedIndex));
            Wire(ObjModelCard, ddobjno, () => PropCatalog, TranslateOr("capobjects", "Objects"), () => (GTA.Offsets.Editor.Objects.model, GTA.Offsets.Editor.Objects.NEXT, ddobjno.SelectedIndex));
            Wire(VehModelCard, ddvehno, () => VehicleCatalog, TranslateOr("vehicles", "Vehicles"), () => (GTA.Offsets.Editor.Vehicle.model, GTA.Offsets.Editor.Vehicle.NEXT, ddvehno.SelectedIndex));
            Wire(WeapModelCard, ddweapno, () => WeaponCatalog, TranslateOr("weapons", "Weapons"), () => (GTA.Offsets.Editor.Weapon.model, GTA.Offsets.Editor.Weapon.NEXT, ddweapno.SelectedIndex));
            Wire(ActorModelCard, ddactorno, () => ActorCatalog, TranslateOr("actor", "Actors"), () => (GTA.Offsets.Editor.Actor.model, GTA.Offsets.Editor.Actor.NEXT, ddactorno.SelectedIndex));
        }

        // Offsets are read when used, because OffsetLoader can load them again for the other edition.
        private void Wire(ModelCard card, ComboBox entries, Func<List<CatalogItem>> items, string title, Func<(long Model, long Stride, int Index)> target)
        {
            card.Items = () => items();

            // The page only refreshes the card for a selected entry, so the empty states follow the
            // entry dropdown: no entries means nothing is placed, no selection means none is picked.
            void UpdateEmpty()
            {
                if (entries.Items.Count == 0)
                    card.ShowEmpty(nothingPlaced: true);
                else if (entries.SelectedIndex < 0)
                    card.ShowEmpty(nothingPlaced: false);
            }
            entries.SelectionChanged += (_, __) => UpdateEmpty();
            ((INotifyCollectionChanged)entries.Items).CollectionChanged += (_, __) => UpdateEmpty();
            UpdateEmpty();
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
            // The creator only shows the new model after a refresh, like a pick from its own list.
            creatorRefresh();
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
