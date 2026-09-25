using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class Kill
    {
        public int[] number;
        public Values[] values;

        public Kill(int[] number, Values[] values)
        {
            this.number = number;
            this.values = values;
        }

        public class Values
        {
            public int[] rule;
            public int[] prio;
            public int[] lim;
            public int[] jtop;
            public int[] jtof;
            public int[] prbs;
            public int mcf;
            public int mcp;

            public Values(int[] rule, int[] prio, int[] lim, int[] jtop, int[] jtof, int[] prbs, int mcf, int mcp)
            {
                this.rule = rule;
                this.prio = prio;
                this.lim = lim;
                this.jtop = jtop;
                this.jtof = jtof;
                this.prbs = prbs;
                this.mcf = mcf;
                this.mcp = mcp;
            }
        }
    }
}
