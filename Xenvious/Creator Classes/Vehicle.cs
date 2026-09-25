using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class Vehicle
    {
        public string Name { get; set; }
        public int Integer { get; set; }

        public Vehicle(string name, int integer)
        {
            Name = name;
            Integer = integer;
        }
    }
}
