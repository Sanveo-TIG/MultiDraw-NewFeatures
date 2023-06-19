using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDraw
{
    public class ConduitsCollection
    {
        public ConduitsCollection(List<Element> a_Conduits)
        {
            Conduits = a_Conduits;
        }
        public List<Element> Conduits
        {
            get; set;
        }
    }
}
