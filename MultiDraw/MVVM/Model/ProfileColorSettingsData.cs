using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDraw
{
    public class ProfileColorSettingsData
    {
        public string vOffsetValue { get; set; }
        public Color vOffsetColor { get; set; }
        public string hOffsetValue { get; set; }
        public Color  hOffsetColor { get; set; }
        public string rOffsetValue { get; set; }
        public Color  rOffsetColor { get; set; }
        public string kOffsetValue { get; set; }
        public Color kOffsetColor { get; set; }
        public string straightValue { get; set; }
        public Color straightColor { get; set; }
        public string nkOffsetValue { get; set; }
        public Color nkOffsetColor { get; set; }
        public string nsOffsetValue { get; set; }
        public Color nsOffsetColor { get; set; }
    }
}
