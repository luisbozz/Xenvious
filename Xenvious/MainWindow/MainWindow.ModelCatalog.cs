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

        // Props and dynamic props share one list, and with it the same pictures.
        private List<CatalogItem> PropCatalog => _propCatalog ?? (_propCatalog = GTA.Editor.PropList
            .Select(p => new CatalogItem(p.Name, p.Native, p.UInt, p.Category, "prop")).ToList());

        // actors.json has display names only, no model names, so actors have no pictures yet.
        private List<CatalogItem> ActorCatalog => _actorCatalog ?? (_actorCatalog = GTA.Editor.ActorList
            .Select(a => new CatalogItem(a.Name, null, a.UInt32, "", "actor")).ToList());

        private void BtnPropCatalog_Click(object sender, RoutedEventArgs e)
        {
            OpenModelCatalog(TranslateOr("prop", "Props"), PropCatalog, GTA.Offsets.Editor.Props.model, GTA.Offsets.Editor.Props.NEXT, ddpropno.SelectedIndex);
        }

        private void BtnDPropCatalog_Click(object sender, RoutedEventArgs e)
        {
            OpenModelCatalog(TranslateOr("dprop", "Dynamic Props"), PropCatalog, GTA.Offsets.Editor.DProps.model, GTA.Offsets.Editor.DProps.NEXT, dddpropno.SelectedIndex);
        }

        private void BtnActorCatalog_Click(object sender, RoutedEventArgs e)
        {
            OpenModelCatalog(TranslateOr("actor", "Actors"), ActorCatalog, GTA.Offsets.Editor.Actor.model, GTA.Offsets.Editor.Actor.NEXT, ddactorno.SelectedIndex);
        }

        // Opens the catalog on the model of the selected entry and writes the chosen one to it;
        // the page shows it on its next refresh, the same as a pick from its own list.
        private void OpenModelCatalog(string title, List<CatalogItem> items, long modelOffset, long stride, int index)
        {
            uint current = 0;
            bool canWrite = m.IsProcOpen && index >= 0 && modelOffset != 0;
            if (canWrite)
                current = unchecked((uint)new Global(modelOffset + stride * index).Get<int>());

            ModelCatalogOverlay.Show(title, items, current, item =>
            {
                if (!m.IsProcOpen || index < 0 || modelOffset == 0)
                {
                    displayScreenMessage(TranslateOr("catalog_noentry", "Select an entry in the creator first."));
                    return;
                }
                new Global(modelOffset + stride * index).SetInt(item.Int32);
            });
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
