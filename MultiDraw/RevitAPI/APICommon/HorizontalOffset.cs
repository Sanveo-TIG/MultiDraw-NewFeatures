using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TIGUtility;

namespace MultiDraw
{
    public class HorizontalOffset
    {
        public static void HOffsetDrawWithPickPoint(Document doc, UIApplication uiapp, List<Element> pickedElements, string offsetVariable, bool Refpiuckpoint, XYZ Pickpoint, ref List<Element> secondaryElements, ref bool fittingsfailure)
        {
            try
            {
                string json = Properties.Settings.Default.ProfileColorSettings;
                ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
                DateTime startDate = DateTime.UtcNow;

                HOffsetGP globalParam = new HOffsetGP
                {
                    OffsetValue = HOffsetUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : HOffsetUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = HOffsetUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };

                using (SubTransaction substrans2 = new SubTransaction(doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in pickedElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    substrans2.Commit();
                }

                using (SubTransaction tx = new SubTransaction(doc))
                {
                    tx.Start();
                    List<Element> thirdElements = new List<Element>();
                    double angle = Convert.ToDouble(HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    GetSecondaryElementsWithOffsetPoint(doc, ref pickedElements, angle, offsetVariable, out secondaryElements, Pickpoint, Refpiuckpoint);

                    //check the angle direction
                    LocationCurve curve = pickedElements[0].Location as LocationCurve;
                    Line l_Line = curve.Curve as Line;
                    XYZ StartPoint = l_Line.GetEndPoint(0);
                    XYZ EndPoint = l_Line.GetEndPoint(1);
                    XYZ PrimaryConduitDirection = l_Line.Direction;
                    XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                    XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1);

                    XYZ Intersectionpoint = Utility.FindIntersectionPoint(StartPoint, EndPoint, Pickpoint, PickPointTwo);  ///IP
                    double SubdistanceOne = Math.Sqrt(Math.Pow((StartPoint.X - Intersectionpoint.X), 2) + Math.Pow((StartPoint.Y - Intersectionpoint.Y), 2));
                    double SubdistanceTwo = Math.Sqrt(Math.Pow((EndPoint.X - Intersectionpoint.X), 2) + Math.Pow((EndPoint.Y - Intersectionpoint.Y), 2));
                    XYZ ConduitStartpt = null;
                    XYZ ConduitEndpoint = null;

                    if (SubdistanceOne < SubdistanceTwo)
                    {
                        ConduitStartpt = StartPoint;
                        ConduitEndpoint = EndPoint;
                    }
                    else
                    {
                        ConduitStartpt = EndPoint;
                        ConduitEndpoint = StartPoint;
                    }
                    Line baseline = Line.CreateBound(ConduitEndpoint, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, ConduitEndpoint.Z)); ///
                    double offsetDistance = 0.0;
                    //Line CrossLine = null;

                    for (int i = 0; i < pickedElements.Count; i++)
                    {
                        Element firstElement = pickedElements[i];
                        Element secondElement = secondaryElements[i];

                        Line firstLine = (firstElement.Location as LocationCurve).Curve as Line;
                        Line secondLine = (secondElement.Location as LocationCurve).Curve as Line;

                        XYZ StartPoint1 = firstLine.GetEndPoint(0);
                        XYZ EndPoint1 = firstLine.GetEndPoint(1);

                        //Line 1
                        XYZ perpendicularDir = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                        XYZ perpendicularStartPoint = Pickpoint;
                        XYZ perpendicularEndPoint = Pickpoint + perpendicularDir.Multiply(15);
                        Line PickPointPerpendicularLine = Line.CreateBound(perpendicularStartPoint, perpendicularEndPoint);
                        //DetailCurve perpendicularDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, PickPointPerpendicularLine);

                        //Line 2
                        XYZ extendedStartPoint = StartPoint1 - PrimaryConduitDirection.Multiply(15);
                        XYZ extendedEndPoint = EndPoint1 + PrimaryConduitDirection.Multiply(15);
                        Line parallelLine = Line.CreateBound(extendedStartPoint, extendedEndPoint);
                        //DetailCurve horizontalDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, parallelLine);
                        XYZ IpforOffset = Utility.FindIntersectionPoint(PickPointPerpendicularLine, parallelLine);

                        //conduitDirection 
                        XYZ NewGetPoint = null;
                        if (IpforOffset != null)
                        {
                            XYZ SP = firstLine.GetEndPoint(0);
                            XYZ EP = firstLine.GetEndPoint(1);

                            if (IpforOffset.DistanceTo(SP) < IpforOffset.DistanceTo(EP))
                            {
                                NewGetPoint = SP;
                            }
                            else
                            {
                                NewGetPoint = EP;
                            }
                        }

                        if (IpforOffset != null && offsetDistance == 0)
                        {
                            offsetDistance = Pickpoint.DistanceTo(IpforOffset);
                        }

                        ///direction
                        Line IpforOffsetLine = Line.CreateBound(NewGetPoint, new XYZ(IpforOffset.X, IpforOffset.Y, NewGetPoint.Z));
                        XYZ secondaryDirection = IpforOffsetLine.Direction;
                        XYZ offsetDirection = secondaryDirection.Multiply(offsetDistance);
                        XYZ secConduitStart = NewGetPoint;
                        XYZ endPoint = secConduitStart + offsetDirection;
                        XYZ conduitEndPoint = endPoint + secondaryDirection.Multiply(10);

                        //if (CrossLine == null)
                        Line CrossLine = Line.CreateBound(IpforOffset, new XYZ(Pickpoint.X, Pickpoint.Y, 0));
                        endPoint += CrossLine.Direction.Multiply(offsetDistance);
                        conduitEndPoint += CrossLine.Direction.Multiply(offsetDistance);

                        Conduit thirdConduit = Utility.CreateConduit(doc, firstElement as Conduit, endPoint, conduitEndPoint);
                        Element thirdElement = doc.GetElement(thirdConduit.Id);
                        thirdElements.Add(thirdElement);
                        Utility.RetainParameters(firstElement, secondElement, uiapp);
                        Utility.RetainParameters(firstElement, thirdElement, uiapp);
                        Parameter bendtype = secondElement.LookupParameter("TIG-Bend Type");
                        Parameter bendangle = secondElement.LookupParameter("TIG-Bend Angle");
                        Parameter bendtype2 = thirdConduit.LookupParameter("TIG-Bend Type");
                        Parameter bendangle2 = thirdConduit.LookupParameter("TIG-Bend Angle");
                        bendtype.Set("H Offset");
                        bendangle.Set(angle);
                        bendtype2.Set("H Offset");
                        bendangle2.Set(angle);
                    }
                    ParentUserControl.Instance.Secondaryelst.Clear();
                    ParentUserControl.Instance.Secondaryelst.AddRange(thirdElements); //
                    ParentUserControl.Instance.Primaryelst = new List<Element>();
                    ParentUserControl.Instance.Primaryelst.AddRange(thirdElements); //3ele

                    try
                    {
                        //Rotate Elements at Once
                        Element ElementOne = pickedElements[0];
                        Element ElementTwo = thirdElements[0];
                        Utility.GetClosestConnectors(ElementOne, ElementTwo, out Connector ConnectorOne, out Connector ConnectorTwo);
                        XYZ axisStart = ConnectorOne.Origin;
                        XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
                        Line axisLine = Line.CreateBound(axisStart, axisEnd);
                        ElementTransformUtils.RotateElements(doc, secondaryElements.Select(r => r.Id).ToList(), axisLine, angle);
                        //Conduit rotate angle indentification
                        Conduit SecondConduit = thirdElements[0] as Conduit;
                        Line SceondConduitLine = (SecondConduit.Location as LocationCurve).Curve as Line;
                        XYZ pt1 = SceondConduitLine.GetEndPoint(0);
                        XYZ pt2 = SceondConduitLine.GetEndPoint(1);
                        XYZ SecondLineDirection = SceondConduitLine.Direction;
                        pt1 -= SecondLineDirection.Multiply(SceondConduitLine.Length + 50);
                        pt2 += SecondLineDirection.Multiply(SceondConduitLine.Length + 50);
                        Line firstline = Line.CreateBound(pt1, pt2);

                        Conduit ThirdConduit = secondaryElements[0] as Conduit;
                        Line ThirdConduitLine = (ThirdConduit.Location as LocationCurve).Curve as Line;
                        XYZ pt3 = ThirdConduitLine.GetEndPoint(0);
                        XYZ pt4 = ThirdConduitLine.GetEndPoint(1);
                        XYZ ThirdLineDirection = ThirdConduitLine.Direction;
                        pt4 += ThirdLineDirection.Multiply(100);
                        Line secondline = Line.CreateBound(pt3, pt4);
                        XYZ IntersectionforangleConduit = Utility.GetIntersection(firstline, secondline);
                        if (IntersectionforangleConduit == null)
                        {
                            angle = 2 * angle;
                            ElementTransformUtils.RotateElements(doc, secondaryElements.Select(r => r.Id).ToList(), axisLine, -angle);
                        }
                        /// Connected Fitting Element
                        for (int i = 0; i < pickedElements.Count; i++)
                        {
                            Element firstElement = pickedElements[i];
                            Element secondElement = secondaryElements[i];
                            Element thirdElement = thirdElements[i];

                            ConnectorSet thirdConnectors = Utility.GetConnectors(thirdElement);
                            ConnectorSet SecondConnectors = Utility.GetConnectors(secondElement);
                            ConnectorSet firstConnectors = Utility.GetConnectors(firstElement);

                            FamilyInstance fittings1 = Utility.CreateElbowFittings(firstConnectors, SecondConnectors, doc, uiapp, pickedElements[i], true);
                            FamilyInstance fittings2 = Utility.CreateElbowFittings(SecondConnectors, thirdConnectors, doc, uiapp, pickedElements[i], true);

                            Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                            Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                            Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                            Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                            bendtype.Set("H Offset");
                            bendangle.Set(angle);
                            bendtype2.Set("H Offset");
                            bendangle2.Set(angle);
                        }
                        secondaryElements = new List<Element>();
                        secondaryElements.AddRange(thirdElements);
                    }
                    catch
                    {
                        DeleteElementsWithFittings(doc, thirdElements);
                        DeleteElementsWithFittings(doc, secondaryElements);

                        HOffsetDrawHandlerWithDefaultOffset(doc, uiapp, pickedElements, offsetVariable, Refpiuckpoint, Pickpoint, ref secondaryElements);

                        MessageBox.Show("Selected point is too close. XYZ Offset has been placed with a default offset of 1'");
                    }
                    tx.Commit();
                }
            }
            catch 
            {

            }
        }

        private static void DeleteElementsWithFittings(Document doc, List<Element> elements)
        {
            foreach (Element element in elements)
            {
                ConnectorManager connMgr = (element as MEPCurve)?.ConnectorManager;
                if (connMgr != null)
                {
                    foreach (Connector connector in connMgr.Connectors)
                    {
                        foreach (Connector refConnector in connector.AllRefs)
                        {
                            if (refConnector.Owner is FamilyInstance fitting)
                            {
                                doc.Delete(fitting.Id);
                            }
                        }
                    }
                }
                doc.Delete(element.Id);
            }
        }
        public static void GetSecondaryElementsWithOffsetPoint(Document doc, ref List<Element> pickedElements, double angle, string offSetVar, out List<Element> secondaryElements, XYZ Pickpoint, bool refpickpoint)
        {
            secondaryElements = new List<Element>();
            Dictionary<double, List<Element>> groupedElements = new Dictionary<double, List<Element>>();
            Utility.GroupByElevation(pickedElements, offSetVar, ref groupedElements);
            pickedElements = new List<Element>();
            groupedElements = groupedElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<double, List<Element>> valuePair in groupedElements)
            {
                List<Element> primaryElements = valuePair.Value.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                double zSpace = (groupedElements.FirstOrDefault().Key - valuePair.Key) * Math.Tan(angle / 2);
                XYZ firstStartPoint = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ firstEndPoint = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                Line lineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
                XYZ pt1 = lineOne.GetEndPoint(0);
                XYZ pt2 = lineOne.GetEndPoint(1);
                XYZ midpoint = (pt1 + pt2) / 2;
                XYZ PrimaryConduitDirection = lineOne.Direction;
                XYZ extendedStartPoint = pt1 + PrimaryConduitDirection.Multiply(10);
                XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1);
                XYZ Intersectionpoint = FindIntersectionPoint(pt1, pt2, Pickpoint, PickPointTwo);
                Line referenceline = Line.CreateBound(pt1, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, pt1.Z));
                XYZ directionopfreferenecline = referenceline.Direction; //
                for (int i = 0; i < primaryElements.Count; i++)
                {
                    if (i == 0)
                    {
                        XYZ strpt1 = extendedStartPoint;
                        XYZ endpt1 = strpt1 - directionopfreferenecline.Multiply(5);
                        XYZ ConduitStartpt = null;
                        if (Intersectionpoint.DistanceTo(pt1) < Intersectionpoint.DistanceTo(pt2))
                        {
                            ConduitStartpt = pt1;
                        }
                        else
                        {
                            ConduitStartpt = pt2;
                        }
                        XYZ ConduitDirection = referenceline.Direction; //
                        XYZ ConduitEndpt = ConduitStartpt + ConduitDirection.Multiply(2);

                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, ConduitStartpt, ConduitEndpt);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Parameter newElevation = newCon.LookupParameter(offSetVar);
                        newElevation.Set(elevation);
                        Element ele = doc.GetElement(newCon.Id);
                        secondaryElements.Add(ele);
                        pickedElements.Add(primaryElements[i]);
                    }
                    else
                    {
                        Line SublineOne = (primaryElements[i].Location as LocationCurve).Curve as Line;
                        XYZ Subpt1 = SublineOne.GetEndPoint(0);
                        XYZ Subpt2 = SublineOne.GetEndPoint(1);
                        XYZ Secondintersections = FindIntersectionPoint(Subpt1, Subpt2, Pickpoint, PickPointTwo);
                        XYZ perpendicularlinedir = Line.CreateBound(new XYZ(Intersectionpoint.X, Intersectionpoint.Y, 0), new XYZ(Secondintersections.X, Secondintersections.Y, 0)).Direction;
                        double speacing = Math.Sqrt(Math.Pow((Intersectionpoint.X - Secondintersections.X), 2) + Math.Pow((Intersectionpoint.Y - Secondintersections.Y), 2));

                        // Extend the start point of the secondary conduits similarly
                        XYZ strpt1 = Pickpoint + perpendicularlinedir.Multiply(speacing);
                        XYZ endpt1 = strpt1 - directionopfreferenecline.Multiply(5);

                        XYZ ConduitStartpt = null;
                        if (Secondintersections.DistanceTo(Subpt1) < Secondintersections.DistanceTo(Subpt2))
                        {
                            ConduitStartpt = Subpt1;
                        }
                        else
                        {
                            ConduitStartpt = Subpt2;
                        }
                        XYZ ConduitDirection = referenceline.Direction;
                        XYZ ConduitEndpt = ConduitStartpt + ConduitDirection.Multiply(2);

                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, ConduitStartpt, ConduitEndpt);

                        //Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, strpt1, endpt1);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Parameter newElevation = newCon.LookupParameter(offSetVar);
                        newElevation.Set(elevation);
                        Element ele = doc.GetElement(newCon.Id);
                        secondaryElements.Add(ele);
                        pickedElements.Add(primaryElements[i]);
                    }
                }
            }
        }
        public static void GetSecondaryElementsWithPoint(Document doc, ref List<Element> pickedElements, double angle, string offSetVar, out List<Element> secondaryElements, XYZ Pickpoint, bool refpickpoint)
        {
            secondaryElements = new List<Element>();
            Dictionary<double, List<Element>> groupedElements = new Dictionary<double, List<Element>>();
            Utility.GroupByElevation(pickedElements, offSetVar, ref groupedElements);
            pickedElements = new List<Element>();
            groupedElements = groupedElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<double, List<Element>> valuePair in groupedElements)
            {
                List<Element> primaryElements = valuePair.Value.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                double zSpace = (groupedElements.FirstOrDefault().Key - valuePair.Key) * Math.Tan(angle / 2);
                XYZ firstStartPoint = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ firstEndPoint = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                Line lineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
                XYZ pt1 = lineOne.GetEndPoint(0);
                XYZ pt2 = lineOne.GetEndPoint(1);
                XYZ midpoint = (pt1 + pt2) / 2;
                XYZ PrimaryConduitDirection = lineOne.Direction;
                XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1);
                XYZ Intersectionpoint = FindIntersectionPoint(pt1, pt2, Pickpoint, PickPointTwo);
                Line referenceline = Line.CreateBound(pt1, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, pt1.Z));
                XYZ directionopfreferenecline = referenceline.Direction;
                for (int i = 0; i < primaryElements.Count; i++)
                {
                    if (i == 0)
                    {
                        XYZ strpt1 = new XYZ(Pickpoint.X, Pickpoint.Y, Pickpoint.Z);
                        XYZ endpt1 = strpt1 - directionopfreferenecline.Multiply(5);
                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, strpt1, endpt1);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Parameter newElevation = newCon.LookupParameter(offSetVar);
                        newElevation.Set(elevation);
                        Element ele = doc.GetElement(newCon.Id);
                        secondaryElements.Add(ele);
                        pickedElements.Add(primaryElements[i]);
                    }
                    else
                    {
                        Line SublineOne = (primaryElements[i].Location as LocationCurve).Curve as Line;
                        XYZ Subpt1 = SublineOne.GetEndPoint(0);
                        XYZ Subpt2 = SublineOne.GetEndPoint(1);
                        XYZ Secondintersections = FindIntersectionPoint(Subpt1, Subpt2, Pickpoint, PickPointTwo);
                        XYZ perpendicularlinedir = Line.CreateBound(new XYZ(Intersectionpoint.X, Intersectionpoint.Y, 0), new XYZ(Secondintersections.X, Secondintersections.Y, 0)).Direction;
                        double speacing = Math.Sqrt(Math.Pow((Intersectionpoint.X - Secondintersections.X), 2) + Math.Pow((Intersectionpoint.Y - Secondintersections.Y), 2));
                        XYZ strpt1 = Pickpoint + perpendicularlinedir.Multiply(speacing);
                        XYZ endpt1 = strpt1 - directionopfreferenecline.Multiply(5);
                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, strpt1, endpt1);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Parameter newElevation = newCon.LookupParameter(offSetVar);
                        newElevation.Set(elevation);
                        Element ele = doc.GetElement(newCon.Id);
                        secondaryElements.Add(ele);
                        pickedElements.Add(primaryElements[i]);
                    }
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(secondaryElements);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }
        public static void GetSecondaryElements(Document doc, ref List<Element> primaryElements, double angle, double offSet, string offSetVar, out List<Element> secondaryElements, XYZ Pickpoint, bool refpickpoint)
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
                Line line = Utility.CrossProductLine(primaryElements[0], Pickpoint, 1, true);
                line = Utility.CrossProductLine(line, Pickpoint, 1, true);
                Line line1 = Utility.CrossProductLine(primaryElements[0], Utility.GetXYvalue(orgin), 1, true);
                XYZ ip = FindIntersectionPoint(line, line1);
                if (ip != null)
                    Pickpoint = ip;
            }
            Line lineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
            XYZ pt1 = lineOne.GetEndPoint(0);
            XYZ pt2 = lineOne.GetEndPoint(1);
            XYZ midpoint = (pt1 + pt2) / 2;
            XYZ PrimaryConduitDirection = lineOne.Direction;
            XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
            XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1);
            XYZ Intersectionpoint = FindIntersectionPoint(pt1, pt2, Pickpoint, PickPointTwo);

            Line ConduitDirectionLine = Line.CreateBound(pt1, pt2);
            XYZ DirectionPfconduit = ConduitDirectionLine.Direction;
            DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z);

            //line for perpendicular dir
            XYZ crossproduct = DirectionPfconduit.CrossProduct(XYZ.BasisZ);
            Line lineforperpenduicular = Line.CreateBound(Intersectionpoint, new XYZ(Pickpoint.X, Pickpoint.Y, 0));
            XYZ Perpendiculardir = lineforperpenduicular.Direction;
            Perpendiculardir = new XYZ(Perpendiculardir.X, Perpendiculardir.Y, PrimaryConduitDirection.Z);

            //Set Line direction
            XYZ pickpointst1 = Pickpoint + PrimaryConduitDirection.Multiply(1);
            XYZ midpoint2 = midpoint + CrossProduct.Multiply(1);
            XYZ intersectiompointTwo = Utility.FindIntersectionPoint(Pickpoint, pickpointst1, midpoint, midpoint2);
            Line Linefoeoffset = Line.CreateBound(midpoint, intersectiompointTwo);
            XYZ Directionforoffset = Linefoeoffset.Direction;

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
                XYZ ConduitEndpoint = null;
                if (SubdistanceOne < SubdistanceTwo)
                {
                    ConduitStartpt = StartPoint;
                    ConduitEndpoint = EndPoint;
                }
                else
                {
                    ConduitStartpt = EndPoint;
                    ConduitEndpoint = StartPoint;
                }
                Line ConduitLine = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                XYZ ConduitLinedir = ConduitLine.Direction;

                XYZ refStartPoint = ConduitStartpt + Perpendiculardir.Multiply(offSet);
                refStartPoint += ConduitLinedir.Multiply(Math.Abs(basedistance));
                XYZ refEndPoint = refStartPoint + ConduitLinedir.Multiply(10);

                Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                Parameter newElevation = newCon.LookupParameter(offSetVar);
                newElevation.Set(elevation);
                Element ele = doc.GetElement(newCon.Id);
                secondaryElements.Add(ele);
            }
            ParentUserControl.Instance.Secondaryelst.Clear();
            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
            ParentUserControl.Instance.Primaryelst.Clear();
            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
        }
        public static void HOffsetDrawHandlerWithDefaultOffset(Document doc, UIApplication uiapp, List<Element> pickedElements, string offsetVariable, bool Refpiuckpoint, XYZ Pickpoint, ref List<Element> secondaryElements)
        {
            DateTime startDate = DateTime.UtcNow;

            try
            {
                List<Element> thirdElements = new List<Element>();
                bool isVerticalConduits = false;
                double angle = 11.25 * (Math.PI / 180);

                string json = Properties.Settings.Default.ProfileColorSettings;
                ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);

                startDate = DateTime.UtcNow;
                HOffsetGP globalParam = new HOffsetGP
                {
                    OffsetValue = HOffsetUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : HOffsetUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = HOffsetUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };
                using (SubTransaction substrans2 = new SubTransaction(doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in pickedElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    substrans2.Commit();
                }
                GetSecondaryElementsWithDefaultOffset(doc, ref pickedElements, offsetVariable, out secondaryElements, Pickpoint, Refpiuckpoint);

                for (int i = 0; i < pickedElements.Count; i++)
                {
                    Element firstElement = pickedElements[i];
                    Element secondElement = secondaryElements[i];
                    Line firstLine = (firstElement.Location as LocationCurve).Curve as Line;
                    Line secondLine = (secondElement.Location as LocationCurve).Curve as Line;
                    Line newLine = Utility.GetParallelLine(firstElement, secondElement, ref isVerticalConduits);
                    double elevation = firstElement.LookupParameter(offsetVariable).AsDouble();

                    Conduit thirdConduit = Utility.CreateConduit(doc, firstElement as Conduit, newLine.GetEndPoint(0), newLine.GetEndPoint(1));
                    Element thirdElement = doc.GetElement(thirdConduit.Id);
                    thirdElements.Add(thirdElement);

                    Utility.RetainParameters(firstElement, secondElement, uiapp);
                    Utility.RetainParameters(firstElement, thirdElement, uiapp);

                    Parameter bendtype = secondElement.LookupParameter("TIG-Bend Type");
                    Parameter bendangle = secondElement.LookupParameter("TIG-Bend Angle");
                    Parameter bendtype2 = thirdConduit.LookupParameter("TIG-Bend Type");
                    Parameter bendangle2 = thirdConduit.LookupParameter("TIG-Bend Angle");
                    bendtype.Set("H Offset");
                    bendangle.Set(angle);
                    bendtype2.Set("H Offset");
                    bendangle2.Set(angle);
                }
                //Rotate Elements at Once
                Element ElementOne = pickedElements[0];
                Element ElementTwo = secondaryElements[0];
                Utility.GetClosestConnectors(ElementOne, ElementTwo, out Connector ConnectorOne, out Connector ConnectorTwo);
                XYZ axisStart = ConnectorOne.Origin;
                XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
                Line axisLine = Line.CreateBound(axisStart, axisEnd);
                ElementTransformUtils.RotateElements(doc, thirdElements.Select(r => r.Id).ToList(), axisLine, angle);

                //Conduit rotate angle indentification
                Conduit SecondConduit = secondaryElements[0] as Conduit;
                Line SceondConduitLine = (SecondConduit.Location as LocationCurve).Curve as Line;
                XYZ pt1 = SceondConduitLine.GetEndPoint(0);
                XYZ pt2 = SceondConduitLine.GetEndPoint(1);
                XYZ SecondLineDirection = SceondConduitLine.Direction;
                pt1 -= SecondLineDirection.Multiply(10);
                Line firstline = Line.CreateBound(pt1, pt2);

                Conduit ThirdConduit = thirdElements[0] as Conduit;
                Line ThirdConduitLine = (ThirdConduit.Location as LocationCurve).Curve as Line;
                XYZ pt3 = ThirdConduitLine.GetEndPoint(0);
                XYZ pt4 = ThirdConduitLine.GetEndPoint(1);
                XYZ ThirdLineDirection = ThirdConduitLine.Direction;
                pt4 += ThirdLineDirection.Multiply(10);
                Line secondline = Line.CreateBound(pt3, pt4);
                XYZ IntersectionforangleConduit = Utility.GetIntersection(firstline, secondline);

                if (IntersectionforangleConduit == null)
                {
                    angle = 2 * angle;
                    ElementTransformUtils.RotateElements(doc, thirdElements.Select(r => r.Id).ToList(), axisLine, -angle);
                }
                //DeleteSupports(doc, pickedElements);

                List<FamilyInstance> bOTTOMForAddtags = new List<FamilyInstance>();
                List<FamilyInstance> TOPForAddtags = new List<FamilyInstance>();
                for (int i = 0; i < pickedElements.Count; i++)
                {
                    Element firstElement = pickedElements[i];
                    Element secondElement = secondaryElements[i];
                    Element thirdElement = thirdElements[i];
                    ConnectorSet thirdConnectors = Utility.GetConnectors(thirdElement);
                    ConnectorSet SecondConnectors = Utility.GetConnectors(secondElement);
                    ConnectorSet firstConnectors = Utility.GetConnectors(firstElement);
                    FamilyInstance fittings1 = Utility.CreateElbowFittings(thirdConnectors, SecondConnectors, doc, uiapp, pickedElements[i], true);
                    FamilyInstance fittings2 = Utility.CreateElbowFittings(thirdConnectors, firstConnectors, doc, uiapp, pickedElements[i], true);

                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                    bendtype.Set("H Offset");
                    bendangle.Set(angle);
                    bendtype2.Set("H Offset");
                    bendangle2.Set(angle);
                    bOTTOMForAddtags.Add(fittings1);
                    TOPForAddtags.Add(fittings2);
                }
            }
            catch
            {
                _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Horizontal Offset", Util.ProductVersion, "Draw");
            }
        }
        public static void GetSecondaryElementsWithDefaultOffset(Document doc, ref List<Element> primaryElements, string offSetVar, out List<Element> secondaryElements, XYZ Pickpoint, bool refpickpoint)
        {
            double angle = 11.25 * (Math.PI / 180);
            double offSet = 1.0;

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
                Line line = Utility.CrossProductLine(primaryElements[0], Pickpoint, 1, true);
                line = Utility.CrossProductLine(line, Pickpoint, 1, true);
                Line line1 = Utility.CrossProductLine(primaryElements[0], Utility.GetXYvalue(orgin), 1, true);
                XYZ ip = FindIntersectionPoint(line, line1);
                if (ip != null)
                    Pickpoint = ip;
            }
            Line lineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
            XYZ pt1 = lineOne.GetEndPoint(0);
            XYZ pt2 = lineOne.GetEndPoint(1);
            XYZ midpoint = (pt1 + pt2) / 2;
            XYZ PrimaryConduitDirection = lineOne.Direction;
            XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
            XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1);
            XYZ Intersectionpoint = FindIntersectionPoint(pt1, pt2, Pickpoint, PickPointTwo);

            Line ConduitDirectionLine = Line.CreateBound(pt1, pt2);
            XYZ DirectionPfconduit = ConduitDirectionLine.Direction;
            DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z);

            //line for perpendicular dir
            XYZ crossproduct = DirectionPfconduit.CrossProduct(XYZ.BasisZ);
            Line lineforperpenduicular = Line.CreateBound(Intersectionpoint, new XYZ(Pickpoint.X, Pickpoint.Y, 0));
            XYZ Perpendiculardir = lineforperpenduicular.Direction;
            Perpendiculardir = new XYZ(Perpendiculardir.X, Perpendiculardir.Y, PrimaryConduitDirection.Z);

            //Set Line direction
            XYZ pickpointst1 = Pickpoint + PrimaryConduitDirection.Multiply(1);
            XYZ midpoint2 = midpoint + CrossProduct.Multiply(1);
            XYZ intersectiompointTwo = Utility.FindIntersectionPoint(Pickpoint, pickpointst1, midpoint, midpoint2);
            Line Linefoeoffset = Line.CreateBound(midpoint, intersectiompointTwo);
            XYZ Directionforoffset = Linefoeoffset.Direction;

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
                XYZ ConduitEndpoint = null;
                if (SubdistanceOne < SubdistanceTwo)
                {
                    ConduitStartpt = StartPoint;
                    ConduitEndpoint = EndPoint;
                }
                else
                {
                    ConduitStartpt = EndPoint;
                    ConduitEndpoint = StartPoint;
                }
                Line ConduitLine = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                XYZ ConduitLinedir = ConduitLine.Direction;

                XYZ refStartPoint = ConduitStartpt + Perpendiculardir.Multiply(offSet);
                refStartPoint += ConduitLinedir.Multiply(Math.Abs(basedistance));
                XYZ refEndPoint = refStartPoint + ConduitLinedir.Multiply(10);

                Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                Parameter newElevation = newCon.LookupParameter(offSetVar);
                newElevation.Set(elevation);
                Element ele = doc.GetElement(newCon.Id);
                secondaryElements.Add(ele);
            }
            ParentUserControl.Instance.Secondaryelst.Clear();
            ParentUserControl.Instance.Secondaryelst.AddRange(secondaryElements);
            ParentUserControl.Instance.Primaryelst.Clear();
            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
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



