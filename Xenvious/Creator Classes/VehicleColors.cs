using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class VehicleColors
    {
        public string Name { get; set; }
        public int id { get; set; }

        public VehicleColors(string name, int ID)
        {
            Name = name;
            id = ID;
        }
    }
}
