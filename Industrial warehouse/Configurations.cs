using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrial_warehouse
{  
    static internal class Configurations
    {
        public static int MaxSameSizeBoxes;
        public static int CriticalValSameSizeBoxes;
        public static double MaxSizeDeviation;
        public static TimeSpan MaxInactivityPeriod;
        public static int MaxDiffrentSizes;

         static Configurations()
        {
            MaxSameSizeBoxes = 100;
            CriticalValSameSizeBoxes = 5;
            MaxSizeDeviation = 0.50;
            MaxInactivityPeriod = TimeSpan.FromDays(100);
            MaxDiffrentSizes = 5;
            
        }
    }    
}
