using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using TIGUtility;

namespace MultiDraw
{
    public class Support
    {
        public static void AddSupport(UIApplication uiApp, Document doc, List<ConduitsCollection> PrimConduits, List<ConduitsCollection> SecConduits = null, List<ConduitsCollection> ThirdConduits = null)
        {
            string FamilyName = "TIG HANGER STRUT v1-";
            string family_folder = Path.GetDirectoryName(typeof(Command).Assembly.Location);

            int.TryParse(uiApp.Application.VersionNumber, out int RevitVersion);
            string offsetVariable = RevitVersion < 2020 ? "Offset" : ((RevitVersion < 2023) ? "Middle Elevation" : "Upper End Centerline Elevation");

            FamilyName = FamilyName.Replace("v1-", "v1-" + RevitVersion.ToString());

            string familyPath = Path.Combine(family_folder, FamilyName);
            FilteredElementCollector SupportCollector = new FilteredElementCollector(doc);
            SupportCollector.OfCategory(BuiltInCategory.OST_ElectricalFixtures);

            Settings settings = ParentUserControl.Instance.GetSettings();
            if (settings != null && settings.IsSupportNeeded)
            {
                if (!settings.StrutType.ToLower().Contains("v1"))
                {
                    FamilyName = "TIG HANGER STRUT v2-";
                    FamilyName = FamilyName.Replace("v2-", "v2-" + RevitVersion.ToString());
                    familyPath = Path.Combine(family_folder, FamilyName);
                }
                FamilySymbol Symbol = SupportCollector.FirstOrDefault(r => r.Name == FamilyName) as FamilySymbol;
                Family family = null;
                try
                {
                    if (Symbol == null)
                    {
                        FamilyName += ".rfa";
                        familyPath = Path.Combine(family_folder, FamilyName);
                        // It is not present, so check for
                        // the file to load it from:
                        if (!File.Exists(familyPath))
                        {
                            TaskDialog.Show(
                              "Please ensure that the sample table "
                              + "family file '{0}' exists in '{1}'.",
                              FamilyName + family_folder);
                        }
                        // Load family from file:
                        if (doc.LoadFamily(familyPath, new FamilyOption(), out family))
                        {
                            ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();
                            foreach (ElementId id in familySymbolIds)
                            {
                                Symbol = doc.GetElement(id) as FamilySymbol;
                                if (!Symbol.IsActive)
                                {
                                    Symbol.Activate();
                                    doc.Regenerate();
                                }
                            }
                        }
                    }
                    if (Symbol != null)
                    {
                        if (!Symbol.IsActive)
                        {
                            Symbol.Activate();
                            doc.Regenerate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }
                AddSupport(uiApp, doc, settings, Symbol, PrimConduits, SecConduits, ThirdConduits);
            }
        }

        public static CoRakDetails ArrangeRack(Document doc, Dictionary<double, List<Element>> groupElements, string offsetVar, string StrutType)
        {
            List<List<Element>> elements = groupElements.Values.ToList();
            SupportInputs supportInputs = new SupportInputs();
            List<ConduitDetail> l_ConduitTopRackOne = new List<ConduitDetail>();
            List<ConduitDetail> l_ConduitBottomRackOne = new List<ConduitDetail>();
            List<ConduitDetail> l_ConduitTopRackTwo = new List<ConduitDetail>();
            List<ConduitDetail> l_ConduitBottomRackTwo = new List<ConduitDetail>();
            List<ConduitDetail> l_ConduitTopRackThree = new List<ConduitDetail>();
            List<ConduitDetail> l_ConduitBottomRackThree = new List<ConduitDetail>();
            List<ConduitDetail> l_ConduitTopRackFour = new List<ConduitDetail>();
            List<ConduitDetail> l_ConduitBottomRackFour = new List<ConduitDetail>();
            CoRakDetails _CoRakDetails = new CoRakDetails();
            FilteredElementCollector conduitTypeCollector = new FilteredElementCollector(doc);
            conduitTypeCollector.OfClass(typeof(ConduitType)).ToElements();
            if (elements.Count > 0)
            {
                GroupeByRackTheConduit(elements, "Rack 1", ref l_ConduitTopRackOne, ref l_ConduitBottomRackOne, offsetVar);
                elements.RemoveAt(0);
                supportInputs.ConduitTopRackOne = l_ConduitTopRackOne;
                supportInputs.ConduitBottomRackOne = l_ConduitBottomRackOne;
                if (l_ConduitTopRackOne != null && l_ConduitTopRackOne.Count > 0 && l_ConduitBottomRackOne != null && l_ConduitBottomRackOne.Count > 0)
                {
                    elements.RemoveAt(0);
                }
                _CoRakDetails.RackOne = new RackDetails
                {
                    RackTop = l_ConduitTopRackOne.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList(),
                    RackBottom = l_ConduitBottomRackOne.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList()
                };
                if (l_ConduitTopRackOne.Any())
                {
                    _CoRakDetails.RackOne.ConduitTypeId = l_ConduitTopRackOne.FirstOrDefault().OldConduitType.IntegerValue;
                }
                else if (l_ConduitBottomRackOne.Any())
                {
                    _CoRakDetails.RackOne.ConduitTypeId = l_ConduitBottomRackOne.FirstOrDefault().OldConduitType.IntegerValue;
                }
                if (l_ConduitTopRackOne.Any() && l_ConduitBottomRackOne.Any())
                {
                    _CoRakDetails.RackOne.RackSizeAsDouble = supportInputs.RackOneElevationTop - supportInputs.RackOneElevationBottom;
                    _CoRakDetails.RackOne.RackSizeAsString = GetRackSize(_CoRakDetails.RackOne.RackSizeAsDouble, StrutType);
                }
                else
                {
                    _CoRakDetails.RackOne.RackSizeAsDouble = (7 / 8) / 12;
                    _CoRakDetails.RackOne.RackSizeAsString = StrutType.ToLower().Contains("strut") ? (l_ConduitTopRackOne.Any() ? "7/8 S" : "7/8 SD") : "1S7";
                }
                _CoRakDetails.TOSR1AsDouble = supportInputs.RackOneElevationTop != 0 ? supportInputs.RackOneElevationTop : supportInputs.RackOneElevationBottom;
            }
            //if (_isStoped)
            //    return supportInputs;
            if (elements.Count > 0)
            {
                GroupeByRackTheConduit(elements, "Rack 2", ref l_ConduitTopRackTwo, ref l_ConduitBottomRackTwo, offsetVar);
                elements.RemoveAt(0);
                supportInputs.ConduitTopRackTwo = l_ConduitTopRackTwo;
                supportInputs.ConduitBottomRackTwo = l_ConduitBottomRackTwo;
                if (l_ConduitTopRackTwo != null && l_ConduitTopRackTwo.Count > 0 && l_ConduitBottomRackTwo != null && l_ConduitBottomRackTwo.Count > 0)
                {
                    elements.RemoveAt(0);
                }
                _CoRakDetails.RackTwo = new RackDetails
                {
                    RackTop = l_ConduitTopRackTwo.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList(),
                    RackBottom = l_ConduitBottomRackTwo.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList()
                };
                if (l_ConduitTopRackTwo.Any())
                {
                    _CoRakDetails.RackTwo.ConduitTypeId = l_ConduitTopRackTwo.FirstOrDefault().OldConduitType.IntegerValue;
                }
                else if (l_ConduitBottomRackTwo.Any())
                {
                    _CoRakDetails.RackTwo.ConduitTypeId = l_ConduitBottomRackTwo.FirstOrDefault().OldConduitType.IntegerValue;
                }
                if (l_ConduitTopRackTwo.Any() && l_ConduitBottomRackTwo.Any())
                {
                    _CoRakDetails.RackTwo.RackSizeAsDouble = supportInputs.RackTwoElevationTop - supportInputs.RackTwoElevationBottom;
                    _CoRakDetails.RackTwo.RackSizeAsString = GetRackSize(_CoRakDetails.RackTwo.RackSizeAsDouble, StrutType);
                }
                else
                {
                    _CoRakDetails.RackTwo.RackSizeAsDouble = (7 / 8) / 12;
                    _CoRakDetails.RackTwo.RackSizeAsString = StrutType.ToLower().Contains("strut") ? (l_ConduitTopRackOne.Any() ? "7/8 S" : "7/8 SD") : "1S7";
                }

            }
            //if (_isStoped)
            //    return supportInputs;
            if (elements.Count > 0)
            {
                GroupeByRackTheConduit(elements, "Rack 3", ref l_ConduitTopRackThree, ref l_ConduitBottomRackThree, offsetVar);
                elements.RemoveAt(0);
                supportInputs.ConduitTopRackThree = l_ConduitTopRackThree;
                supportInputs.ConduitBottomRackThree = l_ConduitBottomRackThree;
                if (l_ConduitTopRackThree != null && l_ConduitTopRackThree.Count > 0 && l_ConduitBottomRackThree != null && l_ConduitBottomRackThree.Count > 0)
                {
                    elements.RemoveAt(0);
                }
                _CoRakDetails.RackThree = new RackDetails
                {
                    RackTop = l_ConduitTopRackThree.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList(),
                    RackBottom = l_ConduitBottomRackThree.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList()
                };
                if (l_ConduitTopRackThree.Any())
                {
                    _CoRakDetails.RackThree.ConduitTypeId = l_ConduitTopRackThree.FirstOrDefault().OldConduitType.IntegerValue;
                }
                else if (l_ConduitBottomRackThree.Any())
                {
                    _CoRakDetails.RackThree.ConduitTypeId = l_ConduitBottomRackThree.FirstOrDefault().OldConduitType.IntegerValue;
                }
                if (l_ConduitTopRackThree.Any() && l_ConduitBottomRackThree.Any())
                {
                    _CoRakDetails.RackThree.RackSizeAsDouble = supportInputs.RackThreeElevationTop - supportInputs.RackThreeElevationBottom;
                    _CoRakDetails.RackThree.RackSizeAsString = GetRackSize(_CoRakDetails.RackThree.RackSizeAsDouble, StrutType);
                }
                else
                {
                    _CoRakDetails.RackThree.RackSizeAsDouble = (7 / 8) / 12;
                    _CoRakDetails.RackThree.RackSizeAsString = StrutType.ToLower().Contains("strut") ? (l_ConduitTopRackOne.Any() ? "7/8 S" : "7/8 SD") : "1S7";
                }
            }
            //if (_isStoped)
            //    return supportInputs;
            if (elements.Count > 0)
            {
                GroupeByRackTheConduit(elements, "Rack 4", ref l_ConduitTopRackFour, ref l_ConduitBottomRackFour, offsetVar);
                elements.RemoveAt(0);
                supportInputs.ConduitTopRackFour = l_ConduitTopRackFour;
                supportInputs.ConduitBottomRackFour = l_ConduitBottomRackFour;
                if (l_ConduitTopRackFour != null && l_ConduitTopRackFour.Count > 0 && l_ConduitBottomRackFour != null && l_ConduitBottomRackFour.Count > 0)
                {
                    elements.RemoveAt(0);
                }
                _CoRakDetails.RackFour = new RackDetails
                {
                    RackTop = l_ConduitTopRackFour.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList(),
                    RackBottom = l_ConduitBottomRackFour.Select(r => new ConduitCollection { SizeAsDouble = r.OldSizeAsDouble }).ToList()
                };
                if (l_ConduitTopRackFour.Any())
                {
                    _CoRakDetails.RackFour.ConduitTypeId = l_ConduitTopRackFour.FirstOrDefault().OldConduitType.IntegerValue;
                }
                else if (l_ConduitBottomRackFour.Any())
                {
                    _CoRakDetails.RackFour.ConduitTypeId = l_ConduitBottomRackFour.FirstOrDefault().OldConduitType.IntegerValue;
                }
                if (l_ConduitTopRackFour.Any() && l_ConduitBottomRackFour.Any())
                {
                    _CoRakDetails.RackFour.RackSizeAsDouble = supportInputs.RackFourElevationTop - supportInputs.RackFourElevationBottom;
                    _CoRakDetails.RackFour.RackSizeAsString = GetRackSize(_CoRakDetails.RackFour.RackSizeAsDouble, StrutType);
                }
                else
                {
                    _CoRakDetails.RackFour.RackSizeAsDouble = (7 / 8) / 12;
                    _CoRakDetails.RackFour.RackSizeAsString = StrutType.ToLower().Contains("strut") ? (l_ConduitTopRackOne.Any() ? "7/8 S" : "7/8 SD") : "1S7";
                }
            }
            if (elements.Count > 0)
            {
                TaskDialog.Show("Warning", "More than 4 racks detected. Please check and retry");

            }
            if (_CoRakDetails != null && _CoRakDetails.RackTwo != null)
                _CoRakDetails.RackTwo.SpacingAsDouble = supportInputs.R1Spacing;
            if (_CoRakDetails != null && _CoRakDetails.RackThree != null)
                _CoRakDetails.RackThree.SpacingAsDouble = supportInputs.R2Spacing;
            if (_CoRakDetails != null && _CoRakDetails.RackFour != null)
                _CoRakDetails.RackFour.SpacingAsDouble = supportInputs.R3Spacing;
            return _CoRakDetails;
        }

        public static string GetRackSize(double value, string strutType)
        {
            string RackSize = string.Empty;
            if (value >= ((FractionToDouble("7/8") / 12) - 0.0052083333333333) && value <= ((FractionToDouble("7/8") / 12) + 0.0052083333333333))
            {
                if (strutType.ToLower().Contains("strut"))
                {
                    RackSize = "7/8 SD";
                }
                else
                    RackSize = "1S7";
            }
            if (value >= ((FractionToDouble("1 5/8") / 12) - 0.0052083333333333) && value <= ((FractionToDouble("1 5/8") / 12) + 0.0052083333333333))
            {
                if (strutType.ToLower().Contains("strut"))
                {
                    RackSize = "7/8 BB";
                }
                else
                {
                    RackSize = "1SD";
                }
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

        public static void GroupeByRackTheConduit(List<List<Element>> elements, string rackName, ref List<ConduitDetail> topRackConduits, ref List<ConduitDetail> bottomRackConduits, string offsetVar)
        {
            topRackConduits = new List<ConduitDetail>();
            bottomRackConduits = new List<ConduitDetail>();
            if (elements.Count > 1)
            {
                List<Element> firstElement = elements[0];
                List<Element> secElement = elements[1];
                double firstTopElevation = firstElement.FirstOrDefault().LookupParameter("Top Elevation") != null ? firstElement.FirstOrDefault().LookupParameter("Top Elevation").AsDouble() : firstElement.FirstOrDefault().LookupParameter("Upper End Top Elevation").AsDouble();
                double firstBottomElevation = firstElement.FirstOrDefault().LookupParameter("Bottom Elevation") != null ? firstElement.FirstOrDefault().LookupParameter("Bottom Elevation").AsDouble() : firstElement.FirstOrDefault().LookupParameter("Upper End Bottom Elevation").AsDouble(); ;
                bool isFirstTopElevation = firstElement.TrueForAll(x => Math.Round(x.LookupParameter("Top Elevation") != null ? x.LookupParameter("Top Elevation").AsDouble() : x.LookupParameter("Upper End Top Elevation").AsDouble(), 5) == Math.Round(firstTopElevation, 5));
                double secTopElevation = secElement.FirstOrDefault().LookupParameter("Top Elevation") != null ? secElement.FirstOrDefault().LookupParameter("Top Elevation").AsDouble() : secElement.FirstOrDefault().LookupParameter("Upper End Top Elevation").AsDouble();
                double secBottomElevation = secElement.FirstOrDefault().LookupParameter("Bottom Elevation") != null ? secElement.FirstOrDefault().LookupParameter("Bottom Elevation").AsDouble() : secElement.FirstOrDefault().LookupParameter("Upper End Bottom Elevation").AsDouble();
                bool isSecTopElevation = secElement.TrueForAll(x => Math.Round(x.LookupParameter("Top Elevation") != null ? x.LookupParameter("Top Elevation").AsDouble() : x.LookupParameter("Upper End Top Elevation").AsDouble(), 5) == Math.Round(secTopElevation, 5));
                double firstElevation = firstElement.FirstOrDefault().LookupParameter(offsetVar).AsDouble();
                double secElevation = secElement.FirstOrDefault().LookupParameter(offsetVar).AsDouble();
                if (!isFirstTopElevation && isSecTopElevation)
                {
                    topRackConduits = firstElement.Select(x => new ConduitDetail(x)).ToList();
                    double spaceOfElevation = Math.Round(firstBottomElevation - secTopElevation, 4);
                    double minSpaceOfElevation = Math.Round(spaceOfElevation - 0.0052083333333333, 4); // tolarence 1/16 = val/12  '' 0.0052083333333333
                    double maxSpaceOfElevation = Math.Round(spaceOfElevation + 0.0052083333333333, 4);// tolarence 1/16  = val/12 '' 0.0052083333333333
                    if ((minSpaceOfElevation <= Math.Round(SupportInputs.GetRackSize("7/8 S"), 4) && maxSpaceOfElevation >= Math.Round(SupportInputs.GetRackSize("7/8 S"), 4))
                        ||
                        (minSpaceOfElevation <= Math.Round(SupportInputs.GetRackSize("7/8 BB"), 4) && maxSpaceOfElevation >= Math.Round(SupportInputs.GetRackSize("7/8 BB"), 4))
                        ||
                        (minSpaceOfElevation <= Math.Round(SupportInputs.GetRackSize("1 5/8 BB"), 4) && maxSpaceOfElevation >= Math.Round(SupportInputs.GetRackSize("1 5/8 BB"), 4))
                        )
                    {
                        bottomRackConduits = secElement.Select(x => new ConduitDetail(x)).ToList();
                    }
                    else
                    {
                        if (Math.Round(spaceOfElevation, 3) < 0.333)
                        {

                            TaskDialog.Show("Warning", "Strut '" + rackName + "' size is not available. Kindly check the spacing between two layers");

                            return;

                        }
                    }
                }
                else if (isFirstTopElevation && !isSecTopElevation)
                {
                    bottomRackConduits = firstElement.Select(x => new ConduitDetail(x)).ToList();
                }
                else if (isFirstTopElevation && isSecTopElevation)
                {
                    bottomRackConduits = firstElement.Select(x => new ConduitDetail(x)).ToList();
                }
                else
                {
                    bottomRackConduits = firstElement.Select(x => new ConduitDetail(x)).ToList();
                }
            }
            else if (elements.Count == 1)
            {
                List<Element> firstElement = elements[0];
                double firstTopElevation = firstElement.FirstOrDefault().LookupParameter("Top Elevation") != null ? firstElement.FirstOrDefault().LookupParameter("Top Elevation").AsDouble() : firstElement.FirstOrDefault().LookupParameter("Upper End Top Elevation").AsDouble();
                double firstBottomElevation = firstElement.FirstOrDefault().LookupParameter("Bottom Elevation") != null ? firstElement.FirstOrDefault().LookupParameter("Bottom Elevation").AsDouble() : firstElement.FirstOrDefault().LookupParameter("Upper End Bottom Elevation").AsDouble(); ;
                bool isFirstTopElevation = firstElement.TrueForAll(x => Math.Round(x.LookupParameter("Top Elevation") != null ? x.LookupParameter("Top Elevation").AsDouble() : x.LookupParameter("Upper End Top Elevation").AsDouble(), 5) == Math.Round(firstTopElevation, 5));
                bool isFirstBottomElevation = firstElement.TrueForAll(x => Math.Round(x.LookupParameter("Bottom Elevation") != null ? x.LookupParameter("Bottom Elevation").AsDouble() : x.LookupParameter("Upper End Bottom Elevation").AsDouble(), 5) == Math.Round(firstBottomElevation, 5));

                if ((isFirstTopElevation && isFirstBottomElevation) || (!isFirstTopElevation && isFirstBottomElevation))
                {
                    topRackConduits = firstElement.Select(x => new ConduitDetail(x)).ToList();
                }
                else
                {
                    bottomRackConduits = firstElement.Select(x => new ConduitDetail(x)).ToList();
                }
            }

        }

        public static void SetSupport(UIApplication uiApp, Document doc, List<ConduitsCollection> PrimConduits, Conduit MinLengthConduit, XYZ SupportPointStart, XYZ SupportPointEnd, double StructLength, FamilySymbol ColumnType, Settings settings)
        {
            int.TryParse(uiApp.Application.VersionNumber, out int RevitVersion);
            string offsetVar = RevitVersion < 2020 ? "Offset" : "Middle Elevation";
            List<Element> _lstElements = new List<Element>();
            PrimConduits.ForEach(r => _lstElements.AddRange(r.Conduits));
            Dictionary<double, List<Element>> groupElements = new Dictionary<double, List<Element>>();
            Utility.GroupByElevation(_lstElements, offsetVar, ref groupElements);
            CoRakDetails _supportInputs = ArrangeRack(doc, groupElements, offsetVar, settings.StrutType);
            Line ConduitLine = (MinLengthConduit.Location as LocationCurve).Curve as Line;
            XYZ Direction = ConduitLine.Direction;
            XYZ StartPoint = ConduitLine.GetEndPoint(0);
            XYZ EndPoint = ConduitLine.GetEndPoint(1);
            if (ConduitLine.Length >= 3)
            {
                SetSupport(uiApp, doc, MinLengthConduit, StartPoint, EndPoint, StructLength, ColumnType, SupportPointStart, settings, _supportInputs);
                SetSupport(uiApp, doc, MinLengthConduit, StartPoint, EndPoint, StructLength, ColumnType, SupportPointEnd, settings, _supportInputs);

                double totalDistance = SupportPointStart.DistanceTo(SupportPointEnd);
                double TotalLength = 0;
                XYZ LP = SupportPointStart;
                for (int k = 0; k <= Math.Round(totalDistance / settings.SupportSpacingAsDouble); k++)
                {
                    if (TotalLength < totalDistance)
                    {
                        if ((TotalLength + settings.SupportSpacingAsDouble + 1) < totalDistance)
                        {
                            if (TotalLength + (settings.SupportSpacingAsDouble * 2) <= totalDistance)
                            {
                                if (Math.Abs(Math.Round(Direction.X)) == 1)
                                {
                                    if (SupportPointStart.X < SupportPointEnd.X)
                                    {
                                        SupportPointStart += Direction * settings.SupportSpacingAsDouble;
                                        LP = SupportPointStart;
                                    }
                                    else
                                    {
                                        SupportPointEnd -= Direction * settings.SupportSpacingAsDouble;
                                        LP = SupportPointEnd;
                                    }
                                }
                                else
                                {
                                    if (SupportPointStart.Y > SupportPointEnd.Y)
                                    {
                                        SupportPointStart += Direction * settings.SupportSpacingAsDouble;
                                        LP = SupportPointStart;
                                    }
                                    else
                                    {
                                        SupportPointEnd -= Direction * settings.SupportSpacingAsDouble;
                                        LP = SupportPointEnd;
                                    }
                                }
                                SetSupport(uiApp, doc, MinLengthConduit, StartPoint, EndPoint, StructLength, ColumnType, LP, settings, _supportInputs);
                                TotalLength += settings.SupportSpacingAsDouble;
                            }
                            else
                            {
                                if (totalDistance - TotalLength >= 3)
                                {
                                    if (Math.Abs(Math.Round(Direction.X)) == 1)
                                    {
                                        if (SupportPointStart.X < SupportPointEnd.X)
                                        {
                                            SupportPointStart += Direction * ((totalDistance - TotalLength) / 2);
                                            LP = SupportPointStart;
                                        }
                                        else
                                        {
                                            SupportPointEnd -= Direction * ((totalDistance - TotalLength) / 2);
                                            LP = SupportPointEnd;
                                        }
                                    }
                                    else
                                    {
                                        if (SupportPointStart.Y > SupportPointEnd.Y)
                                        {
                                            SupportPointStart += Direction * ((totalDistance - TotalLength) / 2);
                                            LP = SupportPointStart;
                                        }
                                        else
                                        {
                                            SupportPointEnd -= Direction * ((totalDistance - TotalLength) / 2);
                                            LP = SupportPointEnd;
                                        }
                                    }
                                    SetSupport(uiApp, doc, MinLengthConduit, StartPoint, EndPoint, StructLength, ColumnType, LP, settings, _supportInputs);
                                }
                                TotalLength += (totalDistance - TotalLength) / 2;
                            }
                        }
                    }
                }
            }
        }

        private static void AddSupport(UIApplication uiApp, Document doc, Settings settings, FamilySymbol Symbol, List<ConduitsCollection> PrimConduits, List<ConduitsCollection> SecConduits = null, List<ConduitsCollection> ThirdConduits = null)
        {
            XYZ SupportPointStart = XYZ.Zero;
            XYZ SupportPointEnd = XYZ.Zero;
            Conduit MinLengthConduit = null;
            double StructLength = 0;
            double extraSupportSize = 30;

            FilteredElementCollector SupportCollector = new FilteredElementCollector(doc);
            SupportCollector.OfCategory(BuiltInCategory.OST_ElectricalFixtures);

            List<Element> SupportTobeDeleted = new List<Element>();
            foreach (Element support in SupportCollector)
            {
                BoundingBoxXYZ boxXYZ = support.get_BoundingBox(doc.ActiveView);
                if (boxXYZ != null)
                {
                    Outline outline = new Outline(boxXYZ.Min, boxXYZ.Max);
                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);
                    List<Element> InterSecElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType().WherePasses(filter).OfClass(typeof(Conduit)).ToList();
                    if (InterSecElements.Any())
                    {
                        for (int i = 0; i < PrimConduits.Count; i++)
                        {
                            if (PrimConduits[i].Conduits.TrueForAll(r => InterSecElements.Any(x => x.Id == r.Id)))
                            {
                                SupportTobeDeleted.Add(support);
                            }
                        }
                    }
                }
            }
            if (SupportTobeDeleted.Any())
            {
                doc.Delete(SupportTobeDeleted.Select(r => r.Id).ToList());
            }

            for (int i = 0; i < PrimConduits.Count; i++)
            {
                List<Element> OrderedConduits = new List<Element>();
                OrderedConduits = Utility.ConduitInOrder(PrimConduits[i].Conduits);
                if (OrderedConduits.Any())
                    StructLength = GetStructLength(OrderedConduits, StructLength, extraSupportSize);
            }
            for (int i = 0; i < PrimConduits.Count; i++)
            {
                List<Element> OrderedConduits = new List<Element>();
                OrderedConduits = Utility.ConduitInOrder(PrimConduits[i].Conduits);
                if (OrderedConduits.Any())
                    GetSupportPoint(OrderedConduits, StructLength, extraSupportSize, ref SupportPointStart, ref SupportPointEnd, ref MinLengthConduit);
            }
            SetSupport(uiApp, doc, PrimConduits, MinLengthConduit, SupportPointStart, SupportPointEnd, StructLength, Symbol, settings);
            SupportPointStart = XYZ.Zero;
            SupportPointEnd = XYZ.Zero;
            MinLengthConduit = null;
            if (SecConduits != null)
            {
                for (int i = 0; i < SecConduits.Count; i++)
                {
                    List<Element> OrderedConduits = new List<Element>();
                    OrderedConduits = Utility.ConduitInOrder(SecConduits[i].Conduits);
                    if (OrderedConduits.Any())
                        GetSupportPoint(OrderedConduits, StructLength, extraSupportSize, ref SupportPointStart, ref SupportPointEnd, ref MinLengthConduit);
                }
                SetSupport(uiApp, doc, SecConduits, MinLengthConduit, SupportPointStart, SupportPointEnd, StructLength, Symbol, settings);
            }
            SupportPointStart = XYZ.Zero;
            SupportPointEnd = XYZ.Zero;
            MinLengthConduit = null;
            if (ThirdConduits != null)
            {
                for (int i = 0; i < ThirdConduits.Count; i++)
                {
                    List<Element> OrderedConduits = new List<Element>();
                    OrderedConduits = Utility.ConduitInOrder(ThirdConduits[i].Conduits);
                    if (OrderedConduits.Any())
                        GetSupportPoint(OrderedConduits, StructLength, extraSupportSize, ref SupportPointStart, ref SupportPointEnd, ref MinLengthConduit);
                }
                SetSupport(uiApp, doc, ThirdConduits, MinLengthConduit, SupportPointStart, SupportPointEnd, StructLength, Symbol, settings);
            }
        }

        public static double GetStructLength(List<Element> l_Conduits, double MaxDistance, double extraSupportSize)
        {
            if (l_Conduits.Count > 0)
            {
                Element FirstConduit = l_Conduits.FirstOrDefault();
                Element FinalConduit = l_Conduits.LastOrDefault();
                //3 Inch Hanger width should be included
                Line lineOne = (FirstConduit.Location as LocationCurve).Curve as Line;
                Line lineTwo = (FinalConduit.Location as LocationCurve).Curve as Line;
                XYZ firstConduitDirection = lineOne.Direction;
                XYZ startPointOne = ((FirstConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ normal = XYZ.BasisZ.CrossProduct(firstConduitDirection).Normalize();
                XYZ translation = normal.Multiply(10);
                Line verticalLine = Line.CreateBound(startPointOne, startPointOne + translation);
                XYZ IP = Utility.FindIntersectionPoint(lineTwo, verticalLine);
                double distance;
                if (IP != null)
                {
                    distance = IP.DistanceTo(new XYZ(startPointOne.X, startPointOne.Y, 0));
                    distance += ((FirstConduit.LookupParameter("Outside Diameter").AsDouble() + FinalConduit.LookupParameter("Outside Diameter").AsDouble()) / 2) + (3.0 / 12);
                }
                else
                {
                    distance = ((FirstConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0).DistanceTo(((FinalConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0)) +
                        ((FirstConduit.LookupParameter("Outside Diameter").AsDouble() + FinalConduit.LookupParameter("Outside Diameter").AsDouble()) / 2) + (3.0 / 12);
                }
                distance += distance * (extraSupportSize / 100);
                if (distance > MaxDistance)
                {
                    MaxDistance = distance;
                }
            }
            return MaxDistance;
        }

        public static void GetSupportPoint(List<Element> l_Conduits, double StructLength, double extraSupportSize, ref XYZ SupportPointStart, ref XYZ SupportPointEnd, ref Conduit MinLenghConduit)
        {
            if (l_Conduits.Count > 0)
            {
                Element FirstConduit = l_Conduits.FirstOrDefault();
                Element FinalConduit = l_Conduits.LastOrDefault();
                Line lineOne = (FirstConduit.Location as LocationCurve).Curve as Line;
                Line lineTwo = (FinalConduit.Location as LocationCurve).Curve as Line;
                MinLenghConduit = FirstConduit as Conduit;
                XYZ firstConduitDirection = lineOne.Direction;
                XYZ startPointOne = ((FirstConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ normal = XYZ.BasisZ.CrossProduct(firstConduitDirection).Normalize();
                XYZ translation = normal.Multiply(10);
                Line verticalLine = Line.CreateBound(startPointOne, startPointOne + translation);
                XYZ IP = Utility.FindIntersectionPoint(lineTwo, verticalLine);
                double distance;
                if (IP != null)
                {
                    distance = IP.DistanceTo(new XYZ(startPointOne.X, startPointOne.Y, 0));
                    distance += ((FirstConduit.LookupParameter("Outside Diameter").AsDouble() + FinalConduit.LookupParameter("Outside Diameter").AsDouble()) / 2) + (3.0 / 12);
                }
                else
                {
                    distance = ((FirstConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0).DistanceTo(((FinalConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0)) +
                        ((FirstConduit.LookupParameter("Outside Diameter").AsDouble() + FinalConduit.LookupParameter("Outside Diameter").AsDouble()) / 2) + (3.0 / 12);
                }
                distance += distance * (extraSupportSize / 100);
                if (Math.Round(distance, 3) >= Math.Round(StructLength, 3))
                {
                    firstConduitDirection = ((FirstConduit.Location as LocationCurve).Curve as Line).Direction;
                    XYZ lastConduitDirection = ((FinalConduit.Location as LocationCurve).Curve as Line).Direction;
                    startPointOne = ((FirstConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ startPointTwo = ((FinalConduit.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    normal = XYZ.BasisZ.CrossProduct(firstConduitDirection).Normalize();
                    translation = normal.Multiply(10);
                    XYZ pntStart = startPointOne + (firstConduitDirection * 1);
                    Line verticalLineOne = Line.CreateBound(pntStart, pntStart + translation);
                    XYZ intersecPointOne = Utility.FindIntersectionPoint(lineOne, verticalLineOne);
                    intersecPointOne = new XYZ(intersecPointOne.X, intersecPointOne.Y, startPointOne.Z);
                    XYZ intersecPointTwo = Utility.FindIntersectionPoint(lineTwo, verticalLineOne);
                    intersecPointTwo = new XYZ(intersecPointTwo.X, intersecPointTwo.Y, startPointTwo.Z);
                    XYZ pntEnd = new XYZ(intersecPointTwo.X, intersecPointTwo.Y, startPointTwo.Z);
                    double distanceOne = startPointOne.DistanceTo(intersecPointOne);
                    double distanceTwo = startPointTwo.DistanceTo(intersecPointTwo);
                    XYZ newStartPointOne = startPointOne + (firstConduitDirection * distanceOne);
                    XYZ newStartPointTwo = startPointTwo + (lastConduitDirection * distanceTwo);
                    if (!((Math.Round(newStartPointOne.X, 8) == Math.Round(intersecPointOne.X, 8)) && (Math.Round(newStartPointOne.Y, 8) == Math.Round(intersecPointOne.Y, 8))
                        && (Math.Round(newStartPointTwo.X, 8) == Math.Round(intersecPointTwo.X, 8)) && (Math.Round(newStartPointTwo.Y, 8) == Math.Round(intersecPointTwo.Y, 8)) && distanceOne > 0.99 && distanceTwo > 0.99))
                    {
                        pntStart = startPointTwo + (lastConduitDirection * 1);
                        verticalLineOne = Line.CreateBound(pntStart, pntStart + translation);
                        intersecPointTwo = Utility.FindIntersectionPoint(lineOne, verticalLineOne);
                        pntEnd = new XYZ(intersecPointTwo.X, intersecPointTwo.Y, startPointOne.Z);
                    }
                    SupportPointStart = (pntStart + pntEnd) / 2;
                    XYZ EndPointOne = ((FirstConduit.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                    XYZ EndPointTwo = ((FinalConduit.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                    pntStart = EndPointOne + (firstConduitDirection * -1);
                    verticalLineOne = Line.CreateBound(pntStart, pntStart + translation);
                    intersecPointOne = Utility.FindIntersectionPoint(lineOne, verticalLineOne);
                    intersecPointOne = new XYZ(intersecPointOne.X, intersecPointOne.Y, EndPointOne.Z);
                    intersecPointTwo = Utility.FindIntersectionPoint(lineTwo, verticalLineOne);
                    intersecPointTwo = new XYZ(intersecPointTwo.X, intersecPointTwo.Y, EndPointTwo.Z);
                    pntEnd = new XYZ(intersecPointTwo.X, intersecPointTwo.Y, EndPointTwo.Z);
                    distanceOne = EndPointOne.DistanceTo(intersecPointOne);
                    distanceTwo = EndPointTwo.DistanceTo(intersecPointTwo);
                    XYZ newEndPointOne = EndPointOne - (firstConduitDirection * distanceOne);
                    XYZ newEndPointTwo = EndPointTwo - (lastConduitDirection * distanceTwo);
                    if (!((Math.Round(newEndPointOne.X, 8) == Math.Round(intersecPointOne.X, 8)) && (Math.Round(newEndPointOne.Y, 8) == Math.Round(intersecPointOne.Y, 8))
                        && (Math.Round(newEndPointTwo.X, 8) == Math.Round(intersecPointTwo.X, 8)) && (Math.Round(newEndPointTwo.Y, 8) == Math.Round(intersecPointTwo.Y, 8)) && distanceOne > 0.99 && distanceTwo > 0.99))
                    {
                        pntStart = EndPointTwo + (lastConduitDirection * -1);
                        verticalLineOne = Line.CreateBound(pntStart, pntStart + translation);
                        intersecPointTwo = Utility.FindIntersectionPoint(lineOne, verticalLineOne);
                        pntEnd = new XYZ(intersecPointTwo.X, intersecPointTwo.Y, EndPointOne.Z);
                    }
                    SupportPointEnd = (pntStart + pntEnd) / 2;
                }
            }
        }

        public static double GetRackSizeNo(string a_SelectedSize)
        {
            double RackSizeNo;
            switch (a_SelectedSize)
            {
                case "7/8 S":
                    RackSizeNo = 1;
                    break;
                case "7/8 SD":
                    RackSizeNo = 2;
                    break;
                case "7/8 BB":
                    RackSizeNo = 3;
                    break;
                case "1 5/8 S":
                    RackSizeNo = 4;
                    break;
                case "1 5/8 SD":
                    RackSizeNo = 5;
                    break;
                case "1 5/8 BB":
                    RackSizeNo = 6;
                    break;
                case "1S":
                    RackSizeNo = 1;
                    break;
                case "1SD":
                    RackSizeNo = 2;
                    break;
                case "1S7":
                    RackSizeNo = 4;
                    break;
                case "2S":
                    RackSizeNo = 3;
                    break;
                case "4D22":
                    RackSizeNo = 5;
                    break;
                case "4D22D":
                    RackSizeNo = 6;
                    break;
                default:
                    RackSizeNo = 0;
                    break;
            }
            return RackSizeNo;
        }

        public static void SetSupport(UIApplication uiApp, Document doc, Conduit MinLengthConduit, XYZ StartPoint, XYZ EndPoint, double StructLength, FamilySymbol ColumnType, XYZ SupportPoint,
         Settings settings, CoRakDetails RakDetails)
        {
            FamilyInstance Instance = doc.Create.NewFamilyInstance(SupportPoint, ColumnType, MinLengthConduit.ReferenceLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            Parameter SupportLength = Instance.LookupParameter("STRUT LENGTH");
            double floatnumber = StructLength - Math.Truncate(StructLength);
            if (floatnumber <= 0.25)
            {
                StructLength = Math.Truncate(StructLength) + 0.25;
            }
            if (floatnumber > 0.25 && floatnumber <= 0.5)
            {
                StructLength = Math.Truncate(StructLength) + 0.5;
            }
            if (floatnumber > 0.5 && floatnumber <= 0.75)
            {
                StructLength = Math.Truncate(StructLength) + 0.75;
            }
            if (floatnumber > 0.75 && floatnumber <= 1.0)
            {
                StructLength = Math.Truncate(StructLength) + 1.0;
            }
            SupportLength.Set((StructLength));
            Parameter RodSize = Instance.LookupParameter("ROD SIZE");
            Utility.GetProjectUnits(uiApp, settings.RodDiaAsString, out double givenRodSize, out _);
            RodSize?.Set(givenRodSize);
            if (RakDetails.RackOne != null && RakDetails.RackTwo != null && RakDetails.RackThree != null && RakDetails.RackFour != null)
            {
                if (!string.IsNullOrEmpty(RakDetails.RackFour.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 1 SELECTION") ?? Instance.LookupParameter("TIER 1 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackFour.RackSizeAsString));
                }
                if (!string.IsNullOrEmpty(RakDetails.RackThree.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 2 SELECTION") ?? Instance.LookupParameter("TIER 2 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackThree.RackSizeAsString));
                }
                if (!string.IsNullOrEmpty(RakDetails.RackTwo.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 3 SELECTION") ?? Instance.LookupParameter("TIER 3 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackTwo.RackSizeAsString));
                }
                if (!string.IsNullOrEmpty(RakDetails.RackOne.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 4 SELECTION") ?? Instance.LookupParameter("TIER 4 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackOne.RackSizeAsString));
                }
                Parameter TierOneOffset = Instance.LookupParameter("TOP OF TIER 4");
                double TOS = RakDetails.TOSR1AsDouble;
                TierOneOffset.Set(TOS);
                Parameter TierTwoOffset = Instance.LookupParameter("TOP OF TIER 3");
                double OffsetTwo = TOS - RakDetails.RackOne.RackSizeAsDouble - RakDetails.RackTwo.SpacingAsDouble;
                TierTwoOffset.Set(OffsetTwo);
                Parameter TierThreeOffset = Instance.LookupParameter("TOP OF TIER 2");
                double OffsetThree = OffsetTwo - RakDetails.RackTwo.RackSizeAsDouble - RakDetails.RackThree.SpacingAsDouble;
                TierThreeOffset.Set(OffsetThree);
                Parameter TierFourOffset = Instance.LookupParameter("TOP OF TIER 1");
                double OffsetFour = OffsetThree - RakDetails.RackThree.RackSizeAsDouble - RakDetails.RackFour.SpacingAsDouble;
                TierFourOffset.Set(OffsetFour);
            }
            else if (RakDetails.RackOne != null && RakDetails.RackTwo != null && RakDetails.RackThree != null)
            {
                if (!string.IsNullOrEmpty(RakDetails.RackThree.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 1 SELECTION") ?? Instance.LookupParameter("TIER 1 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackThree.RackSizeAsString));
                }
                if (!string.IsNullOrEmpty(RakDetails.RackTwo.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 2 SELECTION") ?? Instance.LookupParameter("TIER 2 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackTwo.RackSizeAsString));
                }
                if (!string.IsNullOrEmpty(RakDetails.RackOne.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 3 SELECTION") ?? Instance.LookupParameter("TIER 3 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackOne.RackSizeAsString));
                }
                Parameter TierOneOffset = Instance.LookupParameter("TOP OF TIER 3");
                double TOS = RakDetails.TOSR1AsDouble;
                TierOneOffset.Set(TOS);
                Parameter TierTwoOffset = Instance.LookupParameter("TOP OF TIER 2");
                double OffsetTwo = TOS - RakDetails.RackOne.RackSizeAsDouble - RakDetails.RackTwo.SpacingAsDouble;
                TierTwoOffset.Set(OffsetTwo);
                Parameter TierThreeOffset = Instance.LookupParameter("TOP OF TIER 1");
                double OffsetThree = OffsetTwo - RakDetails.RackTwo.RackSizeAsDouble - RakDetails.RackThree.SpacingAsDouble;
                TierThreeOffset.Set(OffsetThree);
            }
            else if (RakDetails.RackOne != null && RakDetails.RackTwo != null)
            {
                if (!string.IsNullOrEmpty(RakDetails.RackTwo.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 1 SELECTION") ?? Instance.LookupParameter("TIER 1 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackTwo.RackSizeAsString));
                }
                if (!string.IsNullOrEmpty(RakDetails.RackOne.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 2 SELECTION") ?? Instance.LookupParameter("TIER 2 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackOne.RackSizeAsString));
                }
                Parameter TierOneOffset = Instance.LookupParameter("TOP OF TIER 2");
                double TOS = RakDetails.TOSR1AsDouble;
                TierOneOffset.Set(TOS);
                Parameter TierTwoOffset = Instance.LookupParameter("TOP OF TIER 1");
                double OffsetTwo = TOS - RakDetails.RackOne.RackSizeAsDouble - RakDetails.RackTwo.SpacingAsDouble;
                TierTwoOffset.Set(OffsetTwo);
            }
            else
            {
                if (!string.IsNullOrEmpty(RakDetails.RackOne.RackSizeAsString))
                {
                    Parameter RackSize = Instance.LookupParameter("TIER 1 SELECTION") ?? Instance.LookupParameter("TIER 1 TYPE");
                    RackSize?.Set(GetRackSizeNo(RakDetails.RackOne.RackSizeAsString));
                }
                Parameter TierOneOffset = Instance.LookupParameter("TOP OF TIER 1");
                TierOneOffset.Set(RakDetails.TOSR1AsDouble);
            }
            LocationPoint LP = Instance.Location as LocationPoint;
            XYZ PPT = new XYZ(LP.Point.X, LP.Point.Y, 0);
            Line l_ConduitLine = (MinLengthConduit.Location as LocationCurve).Curve as Line;
            XYZ vector_from_pt1_to_pt2 = l_ConduitLine.GetEndPoint(1) - l_ConduitLine.GetEndPoint(0);
            double angle = vector_from_pt1_to_pt2.AngleTo(XYZ.BasisX);
            if (Math.Round(StartPoint.X, 4) != Math.Round(EndPoint.X, 4) && StartPoint.Y > EndPoint.Y)
            {
                angle = -angle;
            }
            Line Axis = Line.CreateBound(PPT, new XYZ(PPT.X, PPT.Y, PPT.Z + 10));
            ElementTransformUtils.RotateElement(doc, Instance.Id, Axis, (Math.PI / 2) + angle);
        }
    }
}
