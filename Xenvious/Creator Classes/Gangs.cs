using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class Gang
    {
        public Gang(string name, string infoName, string weapon, string randonizedWeapons, string vehicle, string enemy, int enemyNumber, int value)
        {
            Name = name;
            InfoName = infoName;
            Weapon = weapon;
            RandonizedWeapons = randonizedWeapons;
            Vehicle = vehicle;
            Enemy = enemy;
            EnemyNumber = enemyNumber;
            Value = value;
        }

        public string Name { get; set; }
        public string InfoName { get; set; }
        public string Weapon { get; set; }
        public string RandonizedWeapons { get; set; }
        public string Vehicle { get; set; }
        public string Enemy { get; set; }
        public int EnemyNumber { get; set; }
        public int Value { get; set; }

    }
}
