namespace Xenvious
{
    public enum InventoryOffset
    {
        _1,
        _2,
        _3,
        _4
    }

    public enum WeaponCategory
    {
        Pistol,
        Meele,
        MG,
        SG,
        Rifle,
        Shotgun,
        Sniper,
        Explosive,
        Special,
        MK2
    }

    public class WeaponInventory
    {
        public string Name;
        public int Bit;
        public int Sia;
        public InventoryOffset InventoryOffset;
        public WeaponCategory WeaponCategory;

        public WeaponInventory(string name, int bit, InventoryOffset inventoryOffset, WeaponCategory weaponCategory, int sia = 99999)
        {
            Name = name;
            Bit = bit;
            Sia = sia;
            InventoryOffset = inventoryOffset;
            WeaponCategory = weaponCategory;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
