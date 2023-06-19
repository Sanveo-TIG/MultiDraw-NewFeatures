using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDraw
{
    public class Settings
    {
        public double SupportSpacingAsDouble { get; set; }
        public string SupportSpacingAsString { get; set; }
        public double RodDiaAsDouble { get; set; }
        public string RodDiaAsString { get; set; }

        public double RodExtensionAsDouble { get; set; }
        public string RodExtensionAsString { get; set; }
        public bool IsSupportNeeded { get; set; }
        public string StrutType { get; set; }
    }
}
