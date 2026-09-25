using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class Outfit
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public Outfit(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }
}
