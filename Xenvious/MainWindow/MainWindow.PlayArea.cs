using System.Globalization;
using System.Linq;

namespace Xenvious
{
    // Part of MainWindow: names and counts of the job's entities for the EntityPicker (play areas).
    public partial class MainWindow
    {
        private void InitEntityPicker()
        {
            EntityPicker.Count = type =>
            {
                if (!m.IsProcOpen)
                    return 0;
                long number = type == EntityPicker.Actor ? GTA.Offsets.Editor.Actor.number
                    : type == EntityPicker.Vehicle ? GTA.Offsets.Editor.Vehicle.number
                    : type == EntityPicker.Object ? GTA.Offsets.Editor.Objects.number
                    : type == EntityPicker.GoTo ? GTA.Offsets.Editor.Locations.number : 0;
                return number == 0 ? 0 : new Global(number).Get<int>();
            };
            EntityPicker.Label = (type, index) =>
            {
                string number = "#" + (index + 1).ToString(CultureInfo.CurrentCulture);
                if (!m.IsProcOpen)
                    return number;
                (long model, long next, System.Func<System.Collections.Generic.List<CatalogItem>> catalog) =
                    type == EntityPicker.Actor ? (GTA.Offsets.Editor.Actor.model, GTA.Offsets.Editor.Actor.NEXT, (System.Func<System.Collections.Generic.List<CatalogItem>>)(() => ActorCatalog))
                    : type == EntityPicker.Vehicle ? (GTA.Offsets.Editor.Vehicle.model, GTA.Offsets.Editor.Vehicle.NEXT, () => VehicleCatalog)
                    : type == EntityPicker.Object ? (GTA.Offsets.Editor.Objects.model, GTA.Offsets.Editor.Objects.NEXT, () => PropCatalog)
                    : (0L, 0L, null);
                if (model == 0 || catalog == null)
                    return type == EntityPicker.GoTo ? TranslateOr("ep_goto", "Go-to") + " " + number : number;
                uint hash = unchecked((uint)new Global(model + index * next).Get<int>());
                var item = catalog().FirstOrDefault(c => c.Hash == hash);
                return number + "  " + (item?.Name ?? "0x" + hash.ToString("X8", CultureInfo.InvariantCulture));
            };
        }
    }
}
