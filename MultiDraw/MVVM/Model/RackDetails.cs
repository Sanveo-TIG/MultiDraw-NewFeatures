using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDraw
{
    public class RackDetails
    {
        public List<ConduitCollection> RackTop { get; set; }
        public List<ConduitCollection> RackBottom { get; set; }
        public double RackSizeAsDouble { get; set; }
        public string RackSizeAsString { get; set; }
        public double SpacingAsDouble { get; set; }
        public string SpacingAsString { get; set; }
        public int ConduitTypeId { get; set; }
    }
}
