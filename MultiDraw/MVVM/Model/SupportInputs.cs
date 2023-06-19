using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDraw
{
    public class ConduitDetail
    {
        public ConduitDetail()
        {

        }
        public ConduitDetail(Element element, bool isNewConduit = false)
        {
            Element = element;
            OldSizeAsString = element.LookupParameter("Diameter(Trade Size)").AsValueString();
            OldSizeAsDouble = element.LookupParameter("Diameter(Trade Size)").AsDouble();
            OldConduitType = new ElementId(Convert.ToInt32(element.LookupParameter("Type Id").AsValueString()));
            IsNewConduit = isNewConduit;
        }
        public ElementId Id { get; set; }
        public Element Element { get; set; }
        public ElementId OldConduitType { get; set; }
        public ElementId NewConduitType { get; set; }
        public string OldSizeAsString { get; set; }
        public string NewSizeAsString { get; set; }
        public double OldSizeAsDouble { get; set; }
        public double NewSizeAsDouble { get; set; }
        public bool IsNewConduit { get; set; }
    }
    public class SupportInputs
    {
        private string _RodDia = "3/8";
        private double _RodSpacing = 30;
        public double _RackTwoElevationTop = 0;
        public double _RackTwoElevationBottom = 0;
        public double _RackOneElevationTop = 0;
        public double _RackOneElevationBottom = 0;
        public double _RackThreeElevationTop = 0;
        public double _RackThreeElevationBottom = 0;
        public double _RackFourElevationTop = 0;
        public double _RackFourElevationBottom = 0;
        public int _TotalRack { get; set; }

        public string RodDia
        {
            get
            {
                return _RodDia;
            }
            set
            {
                _RodDia = value;
            }
        }
        public double RodSpacing
        {
            get
            {
                return _RodSpacing;
            }
            set
            {
                _RodSpacing = value;
            }
        }
        public bool IsRightRodChecked { get; set; }
        public bool IsLeftRodChecked { get; set; }
        public string RodSpacingLeft { get; set; }
        public string RodSpacingRight { get; set; }
        public ElementId LevelId { get; set; }
        public string R4Size { get; set; }
        public string R3Size { get; set; }
        public string R2Size { get; set; }
        public string R1Size { get; set; }

        public List<string> R4SizeList
        {
            get
            {
                double spaceOfElevation = RackFourElevationTop - RackFourElevationBottom;
                return GetRackList(spaceOfElevation);
            }
        }
        public List<string> R3SizeList
        {
            get
            {
                double spaceOfElevation = RackThreeElevationTop - RackThreeElevationBottom;
                return GetRackList(spaceOfElevation);
            }
        }
        public List<string> R2SizeList
        {
            get
            {
                double spaceOfElevation = RackTwoElevationTop - RackTwoElevationBottom;
                return GetRackList(spaceOfElevation);
            }
        }
        public List<string> R1SizeList
        {
            get
            {
                double spaceOfElevation = RackOneElevationTop - RackOneElevationBottom;
                return GetRackList(spaceOfElevation);
            }
        }
        public double R1Spacing { get { if (RackOneElevationBottom != 0 && RackTwoElevationTop != 0) { return ((RackOneElevationBottom - RackTwoElevationTop)); } return (10.0 / 12); } }
        public double R2Spacing { get { if (RackTwoElevationBottom != 0 && RackThreeElevationTop != 0) { return ((RackTwoElevationBottom - RackThreeElevationTop)); } return (10.0 / 12); } }
        public double R3Spacing { get { if (RackThreeElevationBottom != 0 && RackFourElevationTop != 0) { return ((RackThreeElevationBottom - RackFourElevationTop)); } return (10.0 / 12); } }

        public double RackOneElevationTop
        {
            get
            {
                if (_RackOneElevationTop == 0)
                    return (ConduitTopRackOne != null && ConduitTopRackOne.Count() > 0) ?
                        (ConduitTopRackOne.FirstOrDefault().Element.LookupParameter("Bottom Elevation") != null ?
                        ConduitTopRackOne.FirstOrDefault().Element.LookupParameter("Bottom Elevation").AsDouble() :
                        ConduitTopRackOne.FirstOrDefault().Element.LookupParameter("Upper End Bottom Elevation").AsDouble()) : 0;
                return _RackOneElevationTop;
            }
            set
            {
                _RackOneElevationTop = value;
            }

        }
        public double RackOneElevationBottom
        {
            get
            {
                if (_RackOneElevationBottom == 0)
                    return (ConduitBottomRackOne != null && ConduitBottomRackOne.Count() > 0) ?
                        (ConduitBottomRackOne.FirstOrDefault().Element.LookupParameter("Top Elevation") != null ?
                        ConduitBottomRackOne.FirstOrDefault().Element.LookupParameter("Top Elevation").AsDouble() :
                        ConduitBottomRackOne.FirstOrDefault().Element.LookupParameter("Upper End Top Elevation").AsDouble()) : 0;
                return _RackOneElevationBottom;
            }
            set
            {
                _RackOneElevationBottom = value;
            }
        }
        public double RackTwoElevationTop
        {

            get
            {
                if (_RackTwoElevationTop == 0)
                    return (ConduitTopRackTwo != null && ConduitTopRackTwo.Count() > 0) ?
                        (ConduitTopRackTwo.FirstOrDefault().Element.LookupParameter("Bottom Elevation") != null ?
                        ConduitTopRackTwo.FirstOrDefault().Element.LookupParameter("Bottom Elevation").AsDouble() :
                        ConduitTopRackTwo.FirstOrDefault().Element.LookupParameter("Upper End Bottom Elevation").AsDouble()) : 0;

                return _RackTwoElevationTop;
            }
            set
            {
                _RackTwoElevationTop = value;
            }

        }
        public double RackTwoElevationBottom
        {

            get
            {
                if (_RackTwoElevationBottom == 0)
                    return (ConduitBottomRackTwo != null && ConduitBottomRackTwo.Count() > 0) ?
                        (ConduitBottomRackTwo.FirstOrDefault().Element.LookupParameter("Top Elevation") != null ?
                        ConduitBottomRackTwo.FirstOrDefault().Element.LookupParameter("Top Elevation").AsDouble() :
                        ConduitBottomRackTwo.FirstOrDefault().Element.LookupParameter("Upper End Top Elevation").AsDouble()) : 0;
                return _RackTwoElevationBottom;
            }
            set
            {
                _RackTwoElevationBottom = value;
            }
        }
        public double RackThreeElevationTop
        {
            get
            {
                if (_RackThreeElevationTop == 0)
                    return (ConduitTopRackThree != null && ConduitTopRackThree.Count() > 0) ?
                         (ConduitTopRackThree.FirstOrDefault().Element.LookupParameter("Bottom Elevation") != null ?
                        ConduitTopRackThree.FirstOrDefault().Element.LookupParameter("Bottom Elevation").AsDouble() :
                        ConduitTopRackThree.FirstOrDefault().Element.LookupParameter("Upper End Bottom Elevation").AsDouble()) : 0;

                return _RackThreeElevationTop;
            }
            set
            {
                _RackThreeElevationTop = value;
            }
        }
        public double RackThreeElevationBottom
        {
            get
            {
                if (_RackThreeElevationBottom == 0)
                    return (ConduitBottomRackThree != null && ConduitBottomRackThree.Count() > 0) ?
                        (ConduitBottomRackThree.FirstOrDefault().Element.LookupParameter("Top Elevation") != null ?
                        ConduitBottomRackThree.FirstOrDefault().Element.LookupParameter("Top Elevation").AsDouble() :
                        ConduitBottomRackThree.FirstOrDefault().Element.LookupParameter("Upper End Top Elevation").AsDouble()) : 0;
                return _RackThreeElevationBottom;
            }
            set
            {
                _RackThreeElevationBottom = value;
            }
        }
        public double RackFourElevationTop
        {
            get
            {
                if (_RackFourElevationTop == 0)
                    return (ConduitTopRackFour != null && ConduitTopRackFour.Count() > 0) ?
                        (ConduitTopRackFour.FirstOrDefault().Element.LookupParameter("Bottom Elevation") != null ?
                        ConduitTopRackFour.FirstOrDefault().Element.LookupParameter("Bottom Elevation").AsDouble() :
                        ConduitTopRackFour.FirstOrDefault().Element.LookupParameter("Upper End Bottom Elevation").AsDouble()) : 0;

                return _RackFourElevationTop;
            }
            set
            {
                _RackFourElevationTop = value;
            }
        }
        public double RackFourElevationBottom
        {
            get
            {
                if (_RackFourElevationBottom == 0)
                    return (ConduitBottomRackFour != null && ConduitBottomRackFour.Count() > 0) ?
                         (ConduitBottomRackFour.FirstOrDefault().Element.LookupParameter("Top Elevation") != null ?
                        ConduitBottomRackFour.FirstOrDefault().Element.LookupParameter("Top Elevation").AsDouble() :
                        ConduitBottomRackFour.FirstOrDefault().Element.LookupParameter("Upper End Top Elevation").AsDouble()) : 0;
                return _RackFourElevationBottom;
            }
            set
            {
                _RackFourElevationBottom = value;
            }
        }
        public List<ConduitDetail> ConduitTopRackOne { get; set; }
        public List<ConduitDetail> ConduitBottomRackOne { get; set; }
        public List<ConduitDetail> ConduitTopRackTwo { get; set; }
        public List<ConduitDetail> ConduitBottomRackTwo { get; set; }
        public List<ConduitDetail> ConduitTopRackThree { get; set; }
        public List<ConduitDetail> ConduitBottomRackThree { get; set; }
        public List<ConduitDetail> ConduitTopRackFour { get; set; }
        public List<ConduitDetail> ConduitBottomRackFour { get; set; }
        public int MaxConduits
        {
            get
            {
                int maxCount = ConduitTopRackOne != null ? ConduitTopRackOne.Count : 0;
                if (ConduitBottomRackOne != null && ConduitBottomRackOne.Count > maxCount)
                    maxCount = ConduitBottomRackOne.Count;
                if (ConduitTopRackTwo != null && ConduitTopRackTwo.Count > maxCount)
                    maxCount = ConduitTopRackTwo.Count;
                if (ConduitBottomRackTwo != null && ConduitBottomRackTwo.Count > maxCount)
                    maxCount = ConduitBottomRackTwo.Count;
                if (ConduitTopRackThree != null && ConduitTopRackThree.Count > maxCount)
                    maxCount = ConduitTopRackThree.Count;
                if (ConduitBottomRackThree != null && ConduitBottomRackThree.Count > maxCount)
                    maxCount = ConduitBottomRackThree.Count;
                if (ConduitTopRackFour != null && ConduitTopRackFour.Count > maxCount)
                    maxCount = ConduitTopRackFour.Count;
                if (ConduitBottomRackFour != null && ConduitBottomRackFour.Count > maxCount)
                    maxCount = ConduitBottomRackFour.Count;

                return maxCount;
            }
        }
        public int TotalRacks
        {
            get
            {
                if (_TotalRack == 0)
                {
                    int count = ((ConduitTopRackOne != null && ConduitTopRackOne.Count > 0) || (ConduitBottomRackOne != null && ConduitBottomRackOne.Count > 0)) ? 1 : 0;
                    count = ((ConduitTopRackTwo != null && ConduitTopRackTwo.Count > 0) || (ConduitBottomRackTwo != null && ConduitBottomRackTwo.Count > 0)) ? count + 1 : count;
                    count = ((ConduitTopRackThree != null && ConduitTopRackThree.Count > 0) || (ConduitBottomRackThree != null && ConduitBottomRackThree.Count > 0)) ? count + 1 : count;
                    count = ((ConduitTopRackFour != null && ConduitTopRackFour.Count > 0) || (ConduitBottomRackFour != null && ConduitBottomRackFour.Count > 0)) ? count + 1 : count;
                    return count;
                }
                else
                {
                    return _TotalRack;
                }
            }

        }

        public static double GetRackSize(string a_SelectedSize)
        {
            double RackSize;
            switch (a_SelectedSize)
            {
                case "7/8 S":
                    RackSize = FractionToDouble("7/8") / 12;
                    break;
                case "7/8 SD":
                    RackSize = FractionToDouble("7/8") / 12;
                    break;
                case "7/8 BB":
                    RackSize = FractionToDouble("1 5/8") / 12;
                    break;
                case "1 5/8 S":
                    RackSize = FractionToDouble("1 5/8") / 12;
                    break;
                case "1 5/8 SD":
                    RackSize = FractionToDouble("1 5/8") / 12;
                    break;
                case "1 5/8 BB":
                    RackSize = FractionToDouble("3 1/4") / 12;
                    break;
                case "1S":
                    RackSize = FractionToDouble("1 5/8") / 12;
                    break;
                case "1SD":
                    RackSize = FractionToDouble("1 5/8") / 12;
                    break;
                case "2S":
                    RackSize = FractionToDouble("3 1/4") / 12;
                    break;
                case "1S7":
                    RackSize = FractionToDouble("7/8") / 12;
                    break;
                case "4D22":
                    RackSize = FractionToDouble("2 3/32") / 12;
                    break;
                case "4D22D":
                    RackSize = FractionToDouble("2 3/32") / 12;
                    break;
                default:
                    RackSize = 0;
                    break;
            }
            return RackSize;
        }

        public static double FractionToDouble(string a_Value)
        {
            try
            {
                if (double.TryParse(a_Value, out double l_OutValue))
                {
                    return l_OutValue;
                }
                string[] splitValues = a_Value.Split(new char[] { ' ', '/' });
                if (splitValues.Length == 2 || splitValues.Length == 3)
                {
                    if (int.TryParse(splitValues[0], out int a) && int.TryParse(splitValues[1], out int b))
                    {
                        if (splitValues.Length == 2)
                        {
                            l_OutValue = (double)a / b;
                        }
                        if (splitValues.Length == 3)
                        {
                            if (int.TryParse(splitValues[2], out int c))
                            {
                                l_OutValue = a + (double)b / c;
                            }
                        }
                    }
                }
                return l_OutValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<string> GetRackList(double spaceOfElevation)
        {

            double minSpaceOfElevation = Math.Round(spaceOfElevation - 0.0052083333333333, 4); // tolarence 1/16 = val/12  '' 0.0052083333333333
            double maxSpaceOfElevation = Math.Round(spaceOfElevation + 0.0052083333333333, 4);// tolarence 1/16  = val/12 '' 0.0052083333333333


            List<string> racksize = new List<string>
                {
                      "7/8 S","7/8 SD", "7/8 BB","1 5/8 S","1 5/8 SD","1 5/8 BB"
                };
            if (minSpaceOfElevation <= Math.Round(GetRackSize("7/8 S"), 4) && maxSpaceOfElevation >= Math.Round(GetRackSize("7/8 S"), 4))
            {
                racksize = new List<string> { "7/8 S", "7/8 SD" };

            }
            else if (minSpaceOfElevation <= Math.Round(GetRackSize("7/8 BB"), 4) && maxSpaceOfElevation >= Math.Round(GetRackSize("7/8 BB"), 4))
            {
                racksize = new List<string> { "7/8 BB", "1 5/8 S", "1 5/8 SD" };

            }
            else if (minSpaceOfElevation <= Math.Round(GetRackSize("1 5/8 BB"), 4) && maxSpaceOfElevation >= Math.Round(GetRackSize("1 5/8 BB"), 4))
            {
                racksize = new List<string> { "1 5/8 BB" };

            }
            return racksize;
        }


    }
}
