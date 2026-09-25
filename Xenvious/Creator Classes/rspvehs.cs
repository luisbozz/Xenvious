using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xenvious
{
    public class rspvehs
    {
        public static rspveh tampa3 = new rspveh("tampa3", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Forward Facing Minigun", -1),
            new rspveh.Item("Dual Remote Minigun", 0)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Level 1", 0),
            new rspveh.Item("Level 2", 1),
            new rspveh.Item("Level 3", 2)
        });
        public static rspveh dune3 = new rspveh("dune3", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Passenger Machine Gun", -1),
            new rspveh.Item("Passenger Grenade Launcher", 0),
            new rspveh.Item("Passenger Minigun", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Level 1", 0),
            new rspveh.Item("Level 2", 1)
        });
        public static rspveh halftrack = new rspveh("halftrack", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Flatbad Machine Gun", -1),
            new rspveh.Item("Quad 20 mm Autocannon", 0)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Level 1", 0),
            new rspveh.Item("Level 2", 1),
            new rspveh.Item("Level 3", 2)
        });
        public static rspveh insurgent3 = new rspveh("insurgent3", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Top Mounted Machine Gun", -1),
            new rspveh.Item("Top Mounted Minigun", 0)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Level 1", 0),
            new rspveh.Item("Level 2", 1),
            new rspveh.Item("Level 3", 2)
        });
        public static rspveh technical3 = new rspveh("technical3", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Top Mounted Machine Gun", -1),
            new rspveh.Item("Top Mounted Minigun", 0)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Level 1", 0),
            new rspveh.Item("Level 2", 1),
            new rspveh.Item("Level 3", 2)
        });
        public static rspveh apc = new rspveh("apc", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Single Fire Cannon", -1),
            new rspveh.Item("SAM Battery", 0)
        });
        public static rspveh oppressor = new rspveh("oppressor", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Explosive Rounds", 0)
        });
        public static rspveh trailersmall2 = new rspveh("trailersmall2", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Anti Aircraft Gun Turret", -1),
            new rspveh.Item("Homing Rocket Barrage Turret", 0),
            new rspveh.Item("Anti Aircraft Cannon", 1)
        });
        public static rspveh microlight = new rspveh("microlight", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Mounted Gimbal Turret", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh havok = new rspveh("havok", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item(".50 Cal Minigun", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh seabreeze = new rspveh("seabreeze", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });
        public static rspveh starling = new rspveh("starling", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Missiles", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });
        public static rspveh seasparrow = new rspveh("seasparrow", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Forward Facing Minigun", 0),
            new rspveh.Item("Homing Missiles", 1)
        });
        public static rspveh cuban800 = new rspveh("cuban800", null,null,null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });
        public static rspveh pyro = new rspveh("pyro", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Missiles", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh alphaz1 = new rspveh("alphaz1", null, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh howard = new rspveh("howard", null, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh mogul = new rspveh("mogul", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Dual Machine Gun", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });
        public static rspveh rogue = new rspveh("rogue", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0),
            new rspveh.Item("Explosive Rounds", 1)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });
        public static rspveh molotok = new rspveh("molotok", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Missiles", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh nokota = new rspveh("nokota", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Missiles", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh hunter = new rspveh("hunter", null, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });
        public static rspveh tula = new rspveh("tula", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Dual Machine Gun", 0),
            new rspveh.Item("Minigun", 1)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3),
            new rspveh.Item("Bomb 5", 4)
        });
        public static rspveh bombushka = new rspveh("bombushka", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Dual .50 Cal Turret", -1),
            new rspveh.Item("30mm Explosive Cannon", 0)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3),
            new rspveh.Item("Bomb 5", 4)
        });
        public static rspveh thruster = new rspveh("thruster", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0),
            new rspveh.Item("Missiles", 1)
        });
        public static rspveh riot2 = new rspveh("riot2", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Main Water Cannon", -1),
            new rspveh.Item("Main & Passenger Water Cannon", 0)
        });
        public static rspveh volatol = new rspveh("volatol", null, null, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });
        public static rspveh khanjali = new rspveh("khanjali", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Standard Cannon", -1),
            new rspveh.Item("Rail Gun", 0)
        });
        public static rspveh deluxo = new rspveh("deluxo", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Missiles and MG", 0)
        });
        public static rspveh barrage = new rspveh("barrage", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item(".50 Cal Turret", -1),
            new rspveh.Item("Minigun", 0)
        });
        public static rspveh avenger = new rspveh("avenger", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Nose Missile Turret", 0),
            new rspveh.Item("Nose, Top & Rear Missile Turrets", 1)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3),
            new rspveh.Item("Bomb 5", 4)
        });
        public static rspveh comet4 = new rspveh("comet4", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0)
        });
        public static rspveh viseris = new rspveh("viseris", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0)
        });
        public static rspveh revolter = new rspveh("revolter", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0)
        });
        public static rspveh savestra = new rspveh("savestra", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0)
        });
        public static rspveh oppressor2 = new rspveh("oppressor2", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Explosive Rounds", 0),
            new rspveh.Item("Homing Missile", 1)
        }, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        });
        public static rspveh strikeforce = new rspveh("strikeforce", null, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Chaff", 0),
            new rspveh.Item("Decoy Flares", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3),
            new rspveh.Item("Bomb 5", 4)
        });
        public static rspveh speedo4 = new rspveh("speedo4", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0),
            new rspveh.Item("Top Mounted Machine Gun", -1),
            new rspveh.Item("Top Mounted Minigun", 0)
        });
        public static rspveh mule4 = new rspveh("mule4", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0),
            new rspveh.Item("Missiles", 1)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Level 1", 0),
            new rspveh.Item("Level 2", 1),
            new rspveh.Item("Level 3", 2)
        });
        public static rspveh pounder2 = new rspveh("pounder2", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0),
            new rspveh.Item("Missiles", 1),
            new rspveh.Item("SAM Battery", 2)
        }, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Level 1", 0),
            new rspveh.Item("Level 2", 1),
            new rspveh.Item("Level 3", 2)
        });
        public static rspveh jb7002 = new rspveh("jb7002", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Machine Gun", 0),
            new rspveh.Item("Dual Machine Gun", 1)
        });
        public static rspveh minitank = new rspveh("minitank", new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("Machine Gun", -1),
            new rspveh.Item("Flamethrower", 0),
            new rspveh.Item("Charged Rockets", 1),
            new rspveh.Item("Plasma Cannon", 2)
        });
        public static rspveh alkonost = new rspveh("alkonost", null, null, null, new ObservableCollection<rspveh.Item>
        {
            new rspveh.Item("None", -1),
            new rspveh.Item("Bomb 1", 0),
            new rspveh.Item("Bomb 2", 1),
            new rspveh.Item("Bomb 3", 2),
            new rspveh.Item("Bomb 4", 3)
        });

        public static List<rspveh> ArmoredVehicles = new List<rspveh>()
        {
            tampa3,
            dune3,
            halftrack,
            insurgent3,
            technical3,
            apc,
            oppressor,
            trailersmall2,
            microlight,
            havok,
            seabreeze,
            starling,
            seasparrow,
            cuban800,
            pyro,
            alphaz1,
            howard,
            mogul,
            rogue,
            molotok,
            nokota,
            hunter,
            tula,
            bombushka,
            thruster,
            riot2,
            volatol,
            khanjali,
            deluxo,
            barrage,
            avenger,
            comet4,
            savestra,
            revolter,
            viseris,
            oppressor2,
            strikeforce,
            speedo4,
            mule4,
            pounder2,
            jb7002,
            minitank,
            alkonost
        };

        public class rspveh
        {
            public string _native;
            public bool _hasweapons = false;
            public bool _hasarmor = false;
            public bool _hasacm = false;
            public bool _hasbombs = false;
            
            public ObservableCollection<Item> _weapons { get; set; }
            public ObservableCollection<Item> _armor { get; set; }
            public ObservableCollection<Item> _acm { get; set; }
            public ObservableCollection<Item> _bombs { get; set; }

            public rspveh(string native, ObservableCollection<Item> weapons)
            {
                _native = native;
                if (weapons != null)
                {
                    _weapons = weapons;
                    _hasweapons = true;
                }
            }
            public rspveh(string native, ObservableCollection<Item> weapons, ObservableCollection<Item> armor)
            {
                _native = native;
                if (weapons != null)
                {
                    _weapons = weapons;
                    _hasweapons = true;
                }
                if (armor != null)
                {
                    _armor = armor;
                    _hasarmor = true;
                }
            }
            public rspveh(string native, ObservableCollection<Item> weapons, ObservableCollection<Item> armor, ObservableCollection<Item> acm)
            {
                _native = native;
                if (weapons != null)
                {
                    _weapons = weapons;
                    _hasweapons = true;
                }
                if (armor != null)
                {
                    _armor = armor;
                    _hasarmor = true;
                }
                if (acm != null)
                {
                    _acm = acm;
                    _hasacm = true;
                }
            }
            public rspveh(string native, ObservableCollection<Item> weapons, ObservableCollection<Item> armor, ObservableCollection<Item> acm, ObservableCollection<Item> bombs)
            {
                _native = native;
                if (weapons != null)
                {
                    _weapons = weapons;
                    _hasweapons = true;
                }
                if (armor != null)
                {
                    _armor = armor;
                    _hasarmor = true;
                }
                if (acm != null)
                {
                    _acm = acm;
                    _hasacm = true;
                }
                if (bombs != null)
                {
                    _bombs = bombs;
                    _hasbombs = true;
                }
            }

            public class Item
            {
                public string Name { get; set; }
                public int Value { get; set; }

                public Item(string name, int value)
                {
                    Name = name;
                    Value = value;
                }
            }
        }
    }
}
