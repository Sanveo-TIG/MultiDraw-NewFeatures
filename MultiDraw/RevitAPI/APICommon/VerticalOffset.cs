using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TIGUtility;

namespace MultiDraw
{
    public class VerticalOffset
    {
        public static void VOffsetDrawPointWithPickOffset(Document _doc, UIDocument _uiDoc, UIApplication uiApp, List<Element> PrimaryElements, string offsetVariable, int RevitVersion, XYZ Pickpoint, ref List<Element> SecondaryElements)
        {
            try
            {
                VerticalOffsetGP globalParam = new VerticalOffsetGP
                {
                    OffsetValue = VOffsetUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : VOffsetUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = VOffsetUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };
                using (SubTransaction substrans2 = new SubTransaction(_doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        _doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }

                    substrans2.Commit();
                }

                List<Element> thirdElements = new List<Element>();
                double l_angle = Convert.ToDouble(VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                double l_offSet = VOffsetUserControl.Instance.txtOffsetFeet.AsDouble;

                GetSecondaryPointElementsWithPickOffset(_doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offsetVariable, Pickpoint);

                ConnectorSet PrimaryConnectors = null;
                ConnectorSet SecondaryConnectors = null;
                for (int j = 0; j < PrimaryElements.Count; j++)
                {
                    PrimaryConnectors = Utility.GetConnectors(PrimaryElements[j]);
                    SecondaryConnectors = Utility.GetConnectors(SecondaryElements[j]);
                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector COne, out Connector CTwo);
                    XYZ newenpt = new XYZ(CTwo.Origin.X, CTwo.Origin.Y, COne.Origin.Z);
                    Conduit newCon = Utility.CreateConduit(_doc, PrimaryElements[j], COne.Origin, newenpt);
                    Element e = _doc.GetElement(newCon.Id);
                    thirdElements.Add(e);
                    Utility.RetainParameters(PrimaryElements[j], SecondaryElements[j], uiApp);
                    Utility.RetainParameters(PrimaryElements[j], e, uiApp);

                    Parameter bendtype = SecondaryElements[j].LookupParameter("TIG-Bend Type");
                    Parameter bendangle = SecondaryElements[j].LookupParameter("TIG-Bend Angle");
                    Parameter bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                    Parameter bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                    bendtype.Set("V Offset");
                    bendangle.Set(l_angle);
                    bendtype2.Set("V Offset");
                    bendangle2.Set(l_angle);
                }

                //Rotate Elements at Once
                Element ElementOne = PrimaryElements[0];
                Element ElementTwo = SecondaryElements[0];
                Utility.GetClosestConnectors(ElementOne, ElementTwo, out Connector ConnectorOne, out Connector ConnectorTwo);
                LocationCurve newconcurve = thirdElements[0].Location as LocationCurve;
                Line ncl1 = newconcurve.Curve as Line;
                XYZ direction = ncl1.Direction;
                XYZ axisStart = ConnectorOne.Origin;
                XYZ axisEnd = axisStart.Add(XYZ.BasisZ.CrossProduct(direction));
                Line axisLine = Line.CreateBound(axisStart, axisEnd);
                if (l_offSet < 0)
                {
                    l_angle = -l_angle;
                }
                ElementTransformUtils.RotateElements(_doc, thirdElements.Select(r => r.Id).ToList(), axisLine, -l_angle);
                DeleteSupports(_doc, PrimaryElements);
                List<FamilyInstance> bOTTOMForAddtags = new List<FamilyInstance>();
                List<FamilyInstance> TOPForAddtags = new List<FamilyInstance>();
                for (int j = 0; j < PrimaryElements.Count; j++)
                {
                    Element firstElement = PrimaryElements[j];
                    Element secondElement = SecondaryElements[j];
                    Element thirdElement = thirdElements[j];
                    ConnectorSet thirdConnectors = Utility.GetConnectors(thirdElement);
                    ConnectorSet SecondConnectors = Utility.GetConnectors(secondElement);
                    ConnectorSet firstConnectors = Utility.GetConnectors(firstElement);
                    FamilyInstance fittings1 = Utility.CreateElbowFittings(thirdConnectors, SecondConnectors, _doc, uiApp, PrimaryElements[j], true);
                    FamilyInstance fittings2 = Utility.CreateElbowFittings(thirdConnectors, firstConnectors, _doc, uiApp, PrimaryElements[j], true);
                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                    bendtype.Set("V Offset");
                    bendangle.Set(l_angle);
                    bendtype2.Set("V Offset");
                    bendangle2.Set(l_angle);
                    bOTTOMForAddtags.Add(fittings1);
                    TOPForAddtags.Add(fittings2);
                }
            }
            catch
            {

            }
        }
        private static void DeleteSupports(Document doc, List<Element> Elements)
        {
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
                        if (Elements.TrueForAll(r => InterSecElements.Any(x => x.Id == r.Id)))
                        {
                            SupportTobeDeleted.Add(support);
                        }
                    }
                }
            }
            if (SupportTobeDeleted.Any())
            {
                doc.Delete(SupportTobeDeleted.Select(r => r.Id).ToList());
            }
        }
        public static void GetSecondaryElements(Document doc, ref List<Element> primaryElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar, XYZ pickpoint)
        {
            secondaryElements = new List<Element>();

            XYZ orgin = null;
            foreach (Conduit item in primaryElements)
            {
                ConnectorSet PrimaryConnectors = Utility.GetUnusedConnectors(item);
                if (PrimaryConnectors.Size == 1)
                {
                    foreach (Connector con in PrimaryConnectors)
                    {
                        orgin = con.Origin;
                        break;
                    }
                    break;
                }
            }
            if (orgin != null)
            {
                Line line = Utility.CrossProductLine(primaryElements[0], pickpoint, 1, true);
                line = Utility.CrossProductLine(line, pickpoint, 1, true);
                Line line1 = Utility.CrossProductLine(primaryElements[0], Utility.GetXYvalue(orgin), 1, true);
                XYZ ip = FindIntersectionPoint(line, line1);
                if (ip != null)
                    pickpoint = ip;
            }

            //ConduitElevation identification
            XYZ e1pt1 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
            XYZ e1pt2 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

            double Z1 = Math.Round(e1pt1.Z, 2);
            double Z2 = Math.Round(e1pt2.Z, 2);

            if (Z1 == Z2)
            {
                Line lineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
                XYZ pt1 = lineOne.GetEndPoint(0);
                XYZ pt2 = lineOne.GetEndPoint(1);
                XYZ midpoint = (pt1 + pt2) / 2;
                XYZ PrimaryConduitDirection = lineOne.Direction;
                XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ PickPointTwo = pickpoint + CrossProduct.Multiply(1);
                XYZ Intersectionpoint = Utility.FindIntersectionPoint(pt1, pt2, pickpoint, PickPointTwo);

                Line ConduitDirectionLine = Line.CreateBound(midpoint, Intersectionpoint);
                XYZ DirectionPfconduit = ConduitDirectionLine.Direction;
                DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z);

                secondaryElements = new List<Element>();
                double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offSet / Math.Tan(angle);
                primaryElements = primaryElements.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();

                for (int i = 0; i < primaryElements.Count; i++)
                {
                    LocationCurve curve = primaryElements[i].Location as LocationCurve;
                    Line l_Line = curve.Curve as Line;
                    XYZ StartPoint = l_Line.GetEndPoint(0);
                    XYZ EndPoint = l_Line.GetEndPoint(1);
                    double SubdistanceOne = Math.Sqrt(Math.Pow((StartPoint.X - Intersectionpoint.X), 2) + Math.Pow((StartPoint.Y - Intersectionpoint.Y), 2));
                    double SubdistanceTwo = Math.Sqrt(Math.Pow((EndPoint.X - Intersectionpoint.X), 2) + Math.Pow((EndPoint.Y - Intersectionpoint.Y), 2));
                    XYZ ConduitStartpt = null;
                    XYZ conduitEndpoint = null;
                    if (SubdistanceOne < SubdistanceTwo)
                    {
                        ConduitStartpt = StartPoint;
                        conduitEndpoint = EndPoint;
                    }
                    else
                    {
                        ConduitStartpt = EndPoint;
                        conduitEndpoint = StartPoint;
                    }
                    Line ConduitLine = Line.CreateBound(conduitEndpoint, ConduitStartpt);
                    XYZ ConduitlineDir = ConduitLine.Direction;
                    XYZ refStartPoint = ConduitStartpt + ConduitlineDir.Multiply(Math.Abs(basedistance));
                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(10);
                    Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                    double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    newElevation.Set(elevation + offSet);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }

            else
            {
                double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offSet / Math.Tan(angle);
                XYZ Ae1pt1 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ Ae2pt1 = ((primaryElements[1].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                Line ImageLine = Line.CreateBound(new XYZ(Ae1pt1.X, Ae1pt1.Y, Ae1pt1.Z), new XYZ(Ae2pt1.X, Ae2pt1.Y, Ae1pt1.Z));

                XYZ VerticalLineDirection = ImageLine.Direction;
                XYZ CrossforVerticalLine = VerticalLineDirection.CrossProduct(XYZ.BasisZ);



                for (int i = 0; i < primaryElements.Count; i++)
                {

                    XYZ ConduitStartpt = null;
                    XYZ conduitEndpoint = null;

                    XYZ E1pt1 = ((primaryElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ E1pt2 = ((primaryElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                    double EFirstZvalue = e1pt1.Z;
                    double ESceondZvalue = e1pt2.Z;

                    double EFirstDistance = (Math.Pow(EFirstZvalue - pickpoint.Z, 2));
                    double ESceondDistance = (Math.Pow(ESceondZvalue - pickpoint.Z, 2));

                    if (EFirstDistance < ESceondDistance)
                    {
                        ConduitStartpt = E1pt1;
                        conduitEndpoint = E1pt2;
                    }
                    else
                    {
                        ConduitStartpt = E1pt2;
                        conduitEndpoint = E1pt1;
                    }

                    Line ConduitLine = Line.CreateBound(conduitEndpoint, ConduitStartpt);
                    XYZ ConduitlineDir = ConduitLine.Direction;
                    XYZ refStartPoint = ConduitStartpt + ConduitlineDir.Multiply(Math.Abs(basedistance));
                    refStartPoint += CrossforVerticalLine.Multiply(offSet);

                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(10);
                    Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                    //double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    //newElevation.Set(elevation + offSet);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }
        public static void GetSecondaryPointElements(Document doc, ref List<Element> primaryElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar, XYZ pickpoint)
        {
            secondaryElements = new List<Element>();

            XYZ orgin = null;
            foreach (Conduit item in primaryElements)
            {
                ConnectorSet PrimaryConnectors = Utility.GetUnusedConnectors(item);
                if (PrimaryConnectors.Size == 1)
                {
                    foreach (Connector con in PrimaryConnectors)
                    {
                        orgin = con.Origin;
                        break;
                    }
                    break;
                }
            }
            //ConduitElevation identification
            XYZ e1pt1 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
            XYZ e1pt2 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

            double Z1 = Math.Round(e1pt1.Z, 2);
            double Z2 = Math.Round(e1pt2.Z, 2);

            //Based on pickedpoint location
            LocationCurve Rcurve = primaryElements[0].Location as LocationCurve;
            Line Rl_Line = Rcurve.Curve as Line;
            XYZ RStartPoint = Rl_Line.GetEndPoint(0);
            XYZ REndPoint = Rl_Line.GetEndPoint(1);
            XYZ Rl_Line_direction = Rl_Line.Direction;
            XYZ crossforvertical = Rl_Line_direction.CrossProduct(XYZ.BasisZ);
            XYZ Pickedpointreftwo = pickpoint + crossforvertical.Multiply(2);
            XYZ intersectionpointforvertical = Utility.FindIntersectionPoint(RStartPoint, REndPoint, pickpoint, Pickedpointreftwo);

            if (Z1 == Z2)
            {
                Line lineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
                XYZ pt1 = lineOne.GetEndPoint(0);
                XYZ pt2 = lineOne.GetEndPoint(1);
                XYZ midpoint = (pt1 + pt2) / 2;
                XYZ PrimaryConduitDirection = lineOne.Direction;
                XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ PickPointTwo = pickpoint + CrossProduct.Multiply(1);
                XYZ Intersectionpoint = Utility.FindIntersectionPoint(pt1, pt2, pickpoint, PickPointTwo);

                Line ConduitDirectionLine = Line.CreateBound(midpoint, Intersectionpoint);
                XYZ DirectionPfconduit = ConduitDirectionLine.Direction;
                DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z);

                secondaryElements = new List<Element>();
                double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offSet / Math.Tan(angle);
                primaryElements = primaryElements.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                double conduitlength = 0;
                for (int i = 0; i < primaryElements.Count; i++)
                {
                    LocationCurve curve = primaryElements[i].Location as LocationCurve;
                    Line l_Line = curve.Curve as Line;
                    XYZ StartPoint = l_Line.GetEndPoint(0);
                    XYZ EndPoint = l_Line.GetEndPoint(1);
                    double SubdistanceOne = Math.Sqrt(Math.Pow((StartPoint.X - Intersectionpoint.X), 2) + Math.Pow((StartPoint.Y - Intersectionpoint.Y), 2));
                    double SubdistanceTwo = Math.Sqrt(Math.Pow((EndPoint.X - Intersectionpoint.X), 2) + Math.Pow((EndPoint.Y - Intersectionpoint.Y), 2));
                    XYZ ConduitStartpt = null;
                    XYZ conduitEndpoint = null;
                    if (SubdistanceOne < SubdistanceTwo)
                    {
                        ConduitStartpt = StartPoint;
                        conduitEndpoint = EndPoint;
                    }
                    else
                    {
                        ConduitStartpt = EndPoint;
                        conduitEndpoint = StartPoint;
                    }
                    Line ConduitLine = Line.CreateBound(conduitEndpoint, ConduitStartpt);
                    XYZ ConduitlineDir = ConduitLine.Direction;
                    XYZ refStartPoint = ConduitStartpt + ConduitlineDir.Multiply(Math.Abs(basedistance));

                    if (i == 0)
                    {
                        conduitlength = Math.Sqrt(Math.Pow((refStartPoint.X - intersectionpointforvertical.X), 2) + Math.Pow((refStartPoint.Y - intersectionpointforvertical.Y), 2));
                    }
                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(conduitlength);

                    ////Based on pickedpoint location
                    ////XYZ crossforvertical = ConduitlineDir.CrossProduct(pickpoint);
                    ////XYZ Pickedpointreftwo = pickpoint + crossforvertical.Multiply(2);
                    //XYZ intersectionpointforvertical = Utility.FindIntersectionPoint(refStartPoint, refEndPoint, pickpoint, Pickedpointreftwo);

                    Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                    double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    newElevation.Set(elevation + offSet);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
            else
            {
                double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offSet / Math.Tan(angle);
                XYZ Ae1pt1 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ Ae2pt1 = ((primaryElements[1].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                Line ImageLine = Line.CreateBound(new XYZ(Ae1pt1.X, Ae1pt1.Y, Ae1pt1.Z), new XYZ(Ae2pt1.X, Ae2pt1.Y, Ae1pt1.Z));

                XYZ VerticalLineDirection = ImageLine.Direction;
                XYZ CrossforVerticalLine = VerticalLineDirection.CrossProduct(XYZ.BasisZ);

                for (int i = 0; i < primaryElements.Count; i++)
                {
                    XYZ ConduitStartpt = null;
                    XYZ conduitEndpoint = null;

                    XYZ E1pt1 = ((primaryElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ E1pt2 = ((primaryElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                    double EFirstZvalue = e1pt1.Z;
                    double ESceondZvalue = e1pt2.Z;

                    double EFirstDistance = (Math.Pow(EFirstZvalue - pickpoint.Z, 2));
                    double ESceondDistance = (Math.Pow(ESceondZvalue - pickpoint.Z, 2));

                    if (EFirstDistance < ESceondDistance)
                    {
                        ConduitStartpt = E1pt1;
                        conduitEndpoint = E1pt2;
                    }
                    else
                    {
                        ConduitStartpt = E1pt2;
                        conduitEndpoint = E1pt1;
                    }

                    Line ConduitLine = Line.CreateBound(conduitEndpoint, ConduitStartpt);
                    XYZ ConduitlineDir = ConduitLine.Direction;
                    XYZ refStartPoint = ConduitStartpt + ConduitlineDir.Multiply(Math.Abs(basedistance));
                    refStartPoint += CrossforVerticalLine.Multiply(offSet);

                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(10);
                    Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                    //double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    //newElevation.Set(elevation + offSet);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }

        public static void GetSecondaryPointElementsWithPickOffset(Document doc, ref List<Element> primaryElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar, XYZ pickpoint)
        {
            secondaryElements = new List<Element>();

            XYZ orgin = null;
            foreach (Conduit item in primaryElements)
            {
                ConnectorSet PrimaryConnectors = Utility.GetUnusedConnectors(item);
                if (PrimaryConnectors.Size == 1)
                {
                    foreach (Connector con in PrimaryConnectors)
                    {
                        orgin = con.Origin;
                        break;
                    }
                    break;
                }
            }
            //ConduitElevation identification
            XYZ e1pt1 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
            XYZ e1pt2 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

            double Z1 = Math.Round(e1pt1.Z, 2);
            double Z2 = Math.Round(e1pt2.Z, 2);

            //Based on pickedpoint location
            LocationCurve Rcurve = primaryElements[0].Location as LocationCurve;
            Line Rl_Line = Rcurve.Curve as Line;
            XYZ RStartPoint = Rl_Line.GetEndPoint(0);
            XYZ REndPoint = Rl_Line.GetEndPoint(1);
            XYZ Rl_Line_direction = Rl_Line.Direction;
            XYZ crossforvertical = Rl_Line_direction.CrossProduct(XYZ.BasisZ);
            XYZ Pickedpointreftwo = pickpoint + crossforvertical.Multiply(2);
            XYZ intersectionpointforvertical = Utility.FindIntersectionPoint(RStartPoint, REndPoint, pickpoint, Pickedpointreftwo);

            if (Z1 == Z2)
            {
                Line lineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
                XYZ pt1 = lineOne.GetEndPoint(0);
                XYZ pt2 = lineOne.GetEndPoint(1);
                XYZ midpoint = (pt1 + pt2) / 2;
                XYZ PrimaryConduitDirection = lineOne.Direction;
                XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ PickPointTwo = pickpoint + CrossProduct.Multiply(1);
                XYZ Intersectionpoint = Utility.FindIntersectionPoint(pt1, pt2, pickpoint, PickPointTwo);

                Line ConduitDirectionLine = Line.CreateBound(midpoint, Intersectionpoint);
                XYZ DirectionPfconduit = ConduitDirectionLine.Direction;
                DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z);

                secondaryElements = new List<Element>();
                double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offSet / Math.Tan(angle);
                primaryElements = primaryElements.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                double conduitlength = 0;
                for (int i = 0; i < primaryElements.Count; i++)
                {
                    LocationCurve curve = primaryElements[i].Location as LocationCurve;
                    Line l_Line = curve.Curve as Line;
                    XYZ StartPoint = l_Line.GetEndPoint(0);
                    XYZ EndPoint = l_Line.GetEndPoint(1);
                    double SubdistanceOne = Math.Sqrt(Math.Pow((StartPoint.X - Intersectionpoint.X), 2) + Math.Pow((StartPoint.Y - Intersectionpoint.Y), 2));
                    double SubdistanceTwo = Math.Sqrt(Math.Pow((EndPoint.X - Intersectionpoint.X), 2) + Math.Pow((EndPoint.Y - Intersectionpoint.Y), 2));
                    XYZ ConduitStartpt = null;
                    XYZ conduitEndpoint = null;
                    if (SubdistanceOne < SubdistanceTwo)
                    {
                        ConduitStartpt = StartPoint;
                        conduitEndpoint = EndPoint;
                    }
                    else
                    {
                        ConduitStartpt = EndPoint;
                        conduitEndpoint = StartPoint;
                    }
                    Line ConduitLine = Line.CreateBound(conduitEndpoint, ConduitStartpt);
                    XYZ ConduitlineDir = ConduitLine.Direction;
                    XYZ refStartPoint = ConduitStartpt + ConduitlineDir.Multiply(Math.Abs(basedistance));

                    if (i == 0)
                    {
                        conduitlength = Math.Sqrt(Math.Pow((refStartPoint.X - intersectionpointforvertical.X), 2) + Math.Pow((refStartPoint.Y - intersectionpointforvertical.Y), 2));
                    }
                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(conduitlength);

                    ////Based on pickedpoint location
                    ////XYZ crossforvertical = ConduitlineDir.CrossProduct(pickpoint);
                    ////XYZ Pickedpointreftwo = pickpoint + crossforvertical.Multiply(2);
                    //XYZ intersectionpointforvertical = Utility.FindIntersectionPoint(refStartPoint, refEndPoint, pickpoint, Pickedpointreftwo);

                    //per pickppoint  Line1
                    XYZ perpendicularDir = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                    XYZ perpendicularStartPoint = pickpoint;
                    XYZ perpendicularEndPoint = pickpoint + perpendicularDir.Multiply(15);
                    Line PickPointPerpendicularLine = Line.CreateBound(perpendicularStartPoint, perpendicularEndPoint);
                    //DetailCurve perpendicularDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, PickPointPerpendicularLine);

                    //Line 2 first conduitline Line2
                    XYZ extendedStartPoint = StartPoint - PrimaryConduitDirection.Multiply(15);
                    XYZ extendedEndPoint = EndPoint + PrimaryConduitDirection.Multiply(15);
                    Line parallelLine = Line.CreateBound(extendedStartPoint, extendedEndPoint);
                    //DetailCurve horizontalDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, parallelLine);

                    XYZ IpforOffset = Utility.FindIntersectionPoint(PickPointPerpendicularLine, parallelLine);
                    if (IpforOffset != null)
                    {
                        XYZ NewGetPoint = null;
                        if (IpforOffset != null)
                        {
                            XYZ SP = l_Line.GetEndPoint(0);
                            XYZ EP = l_Line.GetEndPoint(1);
                            if (IpforOffset.DistanceTo(SP) < IpforOffset.DistanceTo(EP))
                            {
                                NewGetPoint = SP;
                            }
                            else
                            {
                                NewGetPoint = EP;
                            }
                        }

                        XYZ conduitStartPoint = new XYZ(IpforOffset.X, IpforOffset.Y, NewGetPoint.Z);
                        XYZ conduitDirection = Line.CreateBound(NewGetPoint, new XYZ(IpforOffset.X, IpforOffset.Y, NewGetPoint.Z)).Direction;
                        XYZ conduitEndPoint = conduitStartPoint + conduitDirection.Multiply(10);

                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, conduitStartPoint, conduitEndPoint);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Parameter newElevation = newCon.LookupParameter(offSetVar);
                        newElevation.Set(elevation + offSet);
                        Element ele = doc.GetElement(newCon.Id);
                        secondaryElements.Add(ele);
                    }
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
            else
            {
                double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offSet / Math.Tan(angle);
                XYZ Ae1pt1 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ Ae2pt1 = ((primaryElements[1].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                Line ImageLine = Line.CreateBound(new XYZ(Ae1pt1.X, Ae1pt1.Y, Ae1pt1.Z), new XYZ(Ae2pt1.X, Ae2pt1.Y, Ae1pt1.Z));

                XYZ VerticalLineDirection = ImageLine.Direction;
                XYZ CrossforVerticalLine = VerticalLineDirection.CrossProduct(XYZ.BasisZ);

                for (int i = 0; i < primaryElements.Count; i++)
                {
                    XYZ ConduitStartpt = null;
                    XYZ conduitEndpoint = null;

                    XYZ E1pt1 = ((primaryElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ E1pt2 = ((primaryElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                    double EFirstZvalue = e1pt1.Z;
                    double ESceondZvalue = e1pt2.Z;

                    double EFirstDistance = (Math.Pow(EFirstZvalue - pickpoint.Z, 2));
                    double ESceondDistance = (Math.Pow(ESceondZvalue - pickpoint.Z, 2));

                    if (EFirstDistance < ESceondDistance)
                    {
                        ConduitStartpt = E1pt1;
                        conduitEndpoint = E1pt2;
                    }
                    else
                    {
                        ConduitStartpt = E1pt2;
                        conduitEndpoint = E1pt1;
                    }

                    Line ConduitLine = Line.CreateBound(conduitEndpoint, ConduitStartpt);
                    XYZ ConduitlineDir = ConduitLine.Direction;
                    XYZ refStartPoint = ConduitStartpt + ConduitlineDir.Multiply(Math.Abs(basedistance));
                    refStartPoint += CrossforVerticalLine.Multiply(offSet);

                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(10);
                    Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                    //double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    //newElevation.Set(elevation + offSet);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }
        public static XYZ FindIntersectionPoint(Line lineOne, Line lineTwo)
        {
            return FindIntersectionPoint(lineOne.GetEndPoint(0), lineOne.GetEndPoint(1), lineTwo.GetEndPoint(0), lineTwo.GetEndPoint(1));
        }
        public static XYZ FindIntersectionPoint(XYZ s1, XYZ e1, XYZ s2, XYZ e2)
        {
            double a1 = e1.Y - s1.Y;
            double b1 = s1.X - e1.X;
            double c1 = a1 * s1.X + b1 * s1.Y;

            double a2 = e2.Y - s2.Y;
            double b2 = s2.X - e2.X;
            double c2 = a2 * s2.X + b2 * s2.Y;

            double delta = a1 * b2 - a2 * b1;
            //If lines are parallel, the result will be (NaN, NaN).
            return delta == 0 ? null
                : new XYZ((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta, 0);
        }
    }
}




