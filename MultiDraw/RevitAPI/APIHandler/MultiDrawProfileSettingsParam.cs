using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TIGUtility;

namespace MultiDraw.RevitAPI.APIHandler
{
    public class MultiDrawProfileSettingsParam
    {

        public string VoffsetValue { get; set; }
        public string HoffsetValue { get; set; }
        public string RoffsetValue { get; set; }
        public string KoffsetValue { get; set; }
        public string Straight { get; set; }
        public string NinetyKick { get; set; }
        public string Ninetystub { get; set; }

        public string VoffsetValuecolor { get; set; }

        public string HoffsetValuecolor { get; set; }
        public string RoffsetValuecolor { get; set; }
        public string KoffsetValuecolor { get; set; }
        public string Straightcolor { get; set; }
        public string NinetyKickcolor { get; set; }
        public string Ninetystubcolor { get; set; }

    }
}
