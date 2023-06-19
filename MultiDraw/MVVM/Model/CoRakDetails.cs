using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDraw
{
    public class CoRakDetails
    {
        public double TOSR1AsDouble { get; set; }
        public string TOSR1AsString { get; set; }

        public RackDetails RackOne { get; set; }
        public RackDetails RackTwo { get; set; }
        public RackDetails RackThree { get; set; }
        public RackDetails RackFour { get; set; }
    }
}
