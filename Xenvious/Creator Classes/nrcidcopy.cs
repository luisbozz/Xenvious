using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class nrcidcopy
    {
        public long start; 
        public long range; 
        public long end;
        public List<int> values;

        public nrcidcopy(long start, long end)
        {
            this.start = start;
            this.end = end;
            this.range = end - start;
        }
    }
}
