using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrial_warehouse
{
    internal class BoxPileByDateComparer : IComparer<BoxPile>
    {
        public int Compare(BoxPile bp1, BoxPile bp2)
        {
            return bp1.LastActivityDate.CompareTo(bp2.LastActivityDate);
        }
    }
}
