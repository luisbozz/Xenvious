using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class JobTime
    {
        public string Time { get; set; }
        public int Value { get; set; }

        public JobTime(string time, int value)
        {
            Time = time;
            Value = value;
        }
    }
}
