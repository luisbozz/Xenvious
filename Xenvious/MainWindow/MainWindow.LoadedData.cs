using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xenvious
{
    // Part of MainWindow: putting the data OffsetLoader read into the controls.
    public partial class MainWindow
    {
        /// <summary>Runs after OffsetLoader.Load(): links, vehicle and weapon lists, scr patches.</summary>
        private void BindLoadedData()
        {
            links_props.NavigateUri = new Uri(settings.props);
            links_propswp.NavigateUri = new Uri(settings.propswpic);
            links_veh.NavigateUri = new Uri(settings.vehicles);
            links_weap.NavigateUri = new Uri(settings.weapons);
            links_peds.NavigateUri = new Uri(settings.peds);
            links_cc.NavigateUri = new Uri(settings.colorcodes);

            ObservableCollection<Vehicle> vehlist = new ObservableCollection<Vehicle>();

            vehlist.Add(new Vehicle("None", 0));

            foreach (string name in GTA.Editor.Vehiclenames)
                vehlist.Add(new Vehicle(name, unchecked((int)Functions.joaat(name))));

            try
            {
                ddtrfmvm.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = vehlist });
            }
            catch (Exception) { }

            try
            {
                ddmissionrspmodel.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = vehlist });
            }
            catch (Exception) { }

            var stringlist = StaticData.StartWeapons;
            List<GTA.Weapon> wplist = new List<GTA.Weapon>();

            foreach (var item in stringlist)
            {
                int var1 = Functions.int_parse(item);
                uint var2 = unchecked((uint)var1);
                wplist.Add(new GTA.Weapon(item, "Start Weapon", var1.ToString("X"), var1, var2));
            }

            ddinvgsw.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = wplist });

            LoadScrPatchesPage();
        }
    }
}
