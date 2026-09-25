using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class Teamname
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Teamname(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
