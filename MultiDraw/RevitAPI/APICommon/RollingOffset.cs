using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TIGUtility;

namespace MultiDraw
{
    public class RollingOffset
    {
        public static void GetSecondaryElements(Document doc, ref List<Element> pickedElements, double angle, double offSet, double rollingOffset, out List<Element> secondaryElements, string offSetVar, XYZ PickPoint)
        {
            secondaryElements = new List<Element>();
            XYZ orgin = null;
            foreach (Conduit item in pickedElements)
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
                Line line = Utility.CrossProductLine(pickedElements[0], PickPoint, 1, true);
                line = Utility.CrossProductLine(line, PickPoint, 1, true);
                Line line1 = Utility.CrossProductLine(pickedElements[0], Utility.GetXYvalue(orgin), 1, true);
                XYZ ip = FindIntersectionPoint(line, line1);
                if (ip != null)
                    PickPoint = ip;
            }

            //ConduitElevation identification
            XYZ e1pt1 = ((pickedElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
            XYZ e1pt2 = ((pickedElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

            double Z1 = Math.Round(e1pt1.Z, 2);
            double Z2 = Math.Round(e1pt2.Z, 2);

            if (Z1 == Z2)
            {
                //Indentifing the direction for conduit creation
                XYZ pt1 = (((pickedElements[0] as Conduit).Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ pt2 = (((pickedElements[0] as Conduit).Location as LocationCurve).Curve as Line).GetEndPoint(1);
                XYZ direction = (((pickedElements[0] as Conduit).Location as LocationCurve).Curve as Line).Direction;
                XYZ crossproduct = direction.CrossProduct(XYZ.BasisZ);
                XYZ pickpoint2 = PickPoint + crossproduct.Multiply(1);
                XYZ intersectionpoint = null;
                double shortestditance = 100;
                XYZ ShorestPoint = null;
                foreach (Element ele in pickedElements)
                {
                    XYZ Subpt1 = (((ele as Conduit).Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ Subpt2 = (((ele as Conduit).Location as LocationCurve).Curve as Line).GetEndPoint(1);

                    intersectionpoint = FindIntersectionPoint(Subpt1, Subpt2, PickPoint, pickpoint2);
                    double Subshortestditance = Math.Sqrt(Math.Pow(PickPoint.X - intersectionpoint.X, 2) + Math.Pow(PickPoint.Y - intersectionpoint.Y, 2));
                    if (Subshortestditance < shortestditance)
                    {
                        shortestditance = Subshortestditance;
                        ShorestPoint = intersectionpoint;
                    }
                }
                XYZ DirectionofLine = new XYZ(0, 0, 0);
                double DistanceBetweenTwopoint = Math.Sqrt(Math.Pow(PickPoint.X - ShorestPoint.X, 2) + Math.Pow(PickPoint.Y - ShorestPoint.Y, 2));
                if (DistanceBetweenTwopoint != 0)
                {
                    try
                    {
                        Line CrossLine = Line.CreateBound(ShorestPoint, new XYZ(PickPoint.X, PickPoint.Y, 0));
                        DirectionofLine = CrossLine.Direction;
                        DirectionofLine = new XYZ(DirectionofLine.X, DirectionofLine.Y, direction.Z);
                    }
                    catch
                    {

                    }

                }

                if (DirectionofLine.X == 0 && DirectionofLine.Y == 0)
                {
                    Line SecondaryLine = Line.CreateBound(pt1, pt2);
                    DirectionofLine = SecondaryLine.Direction;
                    XYZ crossDirectin = DirectionofLine.CrossProduct(XYZ.BasisZ);
                    DirectionofLine = new XYZ(Math.Abs(crossDirectin.X), Math.Abs(crossDirectin.Y), Math.Abs(crossDirectin.Z));
                }

                Dictionary<double, List<Element>> groupedElements = new Dictionary<double, List<Element>>();
                Utility.GroupByElevation(pickedElements, offSetVar, ref groupedElements);
                pickedElements = new List<Element>();
                groupedElements = groupedElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);
                double BaseDistance = GetBaseDistance(offSet, rollingOffset, angle);
                double conduitLength = 10;
                foreach (KeyValuePair<double, List<Element>> valuePair in groupedElements)
                {
                    List<Element> primaryElements = valuePair.Value.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                    double zSpace = (groupedElements.FirstOrDefault().Key - valuePair.Key) * Math.Tan(angle / 2);
                    XYZ firstStartPoint = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ firstEndPoint = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                    for (int i = 0; i < primaryElements.Count; i++)
                    {
                        LocationCurve curve = primaryElements[i].Location as LocationCurve;
                        Line l_Line = curve.Curve as Line;
                        XYZ StartPoint = l_Line.GetEndPoint(0);
                        XYZ EndPoint = l_Line.GetEndPoint(1);

                        //finding the shortest point
                        double SubdistanceOne = Math.Sqrt(Math.Pow((StartPoint.X - ShorestPoint.X), 2) + Math.Pow((StartPoint.Y - ShorestPoint.Y), 2));
                        double SubdistanceTwo = Math.Sqrt(Math.Pow((EndPoint.X - ShorestPoint.X), 2) + Math.Pow((EndPoint.Y - ShorestPoint.Y), 2));
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
                        Line LineforDirectionofSecondary = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                        XYZ l_LineEnd = ConduitStartpt;
                        double space = GetConduitSpace(primaryElements.FirstOrDefault(), primaryElements[i], l_LineEnd) * Math.Tan(angle / 3);
                        XYZ refStartPoint = l_LineEnd + (LineforDirectionofSecondary.Direction * (BaseDistance + zSpace + space)) /*: l_LineEnd - (l_Line.Direction * (BaseDistance + zSpace + space))*/;
                        XYZ refEndPoint = refStartPoint + (LineforDirectionofSecondary.Direction * (conduitLength - zSpace - space)) /*: refStartPoint - (l_Line.Direction * (conduitLength - zSpace - space))*/;
                        XYZ cross = l_Line.Direction.CrossProduct(XYZ.BasisZ);
                        refStartPoint = refStartPoint + DirectionofLine.Multiply(rollingOffset);
                        refEndPoint = refEndPoint + DirectionofLine.Multiply(rollingOffset);
                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Element ele = doc.GetElement(newCon.Id);
                        SetElevation(ele, elevation, offSet, offSetVar);
                        Line newLine = GetLineWithSpace(ele, primaryElements[i], zSpace + space);
                        if (StartPoint.DistanceTo(newLine.Origin) > EndPoint.DistanceTo(newLine.Origin))
                        {
                            curve.Curve = Line.CreateBound(StartPoint, newLine.Origin);
                        }
                        else
                        {
                            curve.Curve = Line.CreateBound(newLine.Origin, EndPoint);
                        }
                        pickedElements.Add(primaryElements[i]);
                        secondaryElements.Add(ele);
                    }
                    ParentUserControl.Instance.Secondaryelst.Clear();
                    ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                    ParentUserControl.Instance.Primaryelst.Clear();
                    ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                }

            }
            else
            {
                double BaseDistance = GetBaseDistance(offSet, rollingOffset, angle);
                XYZ Ae1pt1 = ((pickedElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ Ae2pt1 = ((pickedElements[1].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                Line ImageLine = Line.CreateBound(new XYZ(Ae1pt1.X, Ae1pt1.Y, Ae1pt1.Z), new XYZ(Ae2pt1.X, Ae2pt1.Y, Ae1pt1.Z));

                XYZ VerticalLineDirection = ImageLine.Direction;
                XYZ CrossforVerticalLine = VerticalLineDirection.CrossProduct(XYZ.BasisZ);

                for (int i = 0; i < pickedElements.Count; i++)
                {

                    XYZ ConduitStartpt = null;
                    XYZ conduitEndpoint = null;

                    XYZ E1pt1 = ((pickedElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ E1pt2 = ((pickedElements[i].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                    double EFirstZvalue = e1pt1.Z;
                    double ESceondZvalue = e1pt2.Z;

                    double EFirstDistance = (Math.Pow(EFirstZvalue - PickPoint.Z, 2));
                    double ESceondDistance = (Math.Pow(ESceondZvalue - PickPoint.Z, 2));

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
                    XYZ refStartPoint = ConduitStartpt + ConduitlineDir.Multiply(Math.Abs(BaseDistance));
                    refStartPoint = refStartPoint + VerticalLineDirection.Multiply(rollingOffset);
                    refStartPoint = refStartPoint + CrossforVerticalLine.Multiply(offSet);

                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(10);
                    Conduit newCon = Utility.CreateConduit(doc, pickedElements[i] as Conduit, refStartPoint, refEndPoint);
                    //double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    //newElevation.Set(elevation + offSet);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);


                }
            }

        }
        public static void GetSecondarypointElements(Document doc, ref List<Element> pickedElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar, XYZ PickPoint)
        {
            secondaryElements = new List<Element>();
            XYZ basepointone = null;
            XYZ basepointtwo = null;
            bool breakfunction = false;
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

                for (int i = 0; i < primaryElements.Count; i++)
                {
                    Line lineOne = (primaryElements[i].Location as LocationCurve).Curve as Line;
                    XYZ pt1 = lineOne.GetEndPoint(0);
                    XYZ pt2 = lineOne.GetEndPoint(1);
                    XYZ midpoint = (pt1 + pt2) / 2;
                    XYZ PrimaryConduitDirection = lineOne.Direction;
                    XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                    XYZ PickPointTwo = PickPoint + CrossProduct.Multiply(1);

                    XYZ Intersectionpoint = FindIntersectionPoint(pt1, pt2, PickPoint, PickPointTwo);

                    double offsetdistance = Math.Sqrt(Math.Pow((PickPoint.X - Intersectionpoint.X), 2) + Math.Pow((PickPoint.Y - Intersectionpoint.Y), 2));
                    Line referenceline = Line.CreateBound(pt1, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, pt1.Z));
                    XYZ directionopfreferenecline = referenceline.Direction;

                    Line line2 = Line.CreateBound(new XYZ(Intersectionpoint.X, Intersectionpoint.Y, pt1.Z), new XYZ(PickPoint.X, PickPoint.Y, pt1.Z));
                    XYZ perdiculardir = line2.Direction;

                    //double BaseDistance = (angle * (180 / Math.PI)) == 90 ? 1 : offsetdistance / Math.Tan(angle);
                    // pickedElements = pickedElements.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();

                    double BaseDistance = GetBaseDistance(offSet, offsetdistance, angle);


                    if (i == 0)
                    {

                        LocationCurve curve = primaryElements[i].Location as LocationCurve;
                        Line l_Line = curve.Curve as Line;
                        //XYZ StartPoint = l_Line.GetEndPoint(0);
                        //XYZ EndPoint = l_Line.GetEndPoint(1);

                        XYZ strpt1 = l_Line.GetEndPoint(0);
                        XYZ endpt1 = l_Line.GetEndPoint(1);

                        //finding the shortest point
                        double SubdistanceOne = Math.Sqrt(Math.Pow((strpt1.X - Intersectionpoint.X), 2) + Math.Pow((strpt1.Y - Intersectionpoint.Y), 2));
                        double SubdistanceTwo = Math.Sqrt(Math.Pow((endpt1.X - Intersectionpoint.X), 2) + Math.Pow((endpt1.Y - Intersectionpoint.Y), 2));
                        XYZ ConduitStartpt = null;
                        XYZ ConduitEndpoint = null;
                        if (SubdistanceOne < SubdistanceTwo)
                        {
                            ConduitStartpt = strpt1;
                            ConduitEndpoint = endpt1;
                        }
                        else
                        {
                            ConduitStartpt = endpt1;
                            ConduitEndpoint = strpt1;
                        }
                        Line LineforDirectionofSecondary = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                        XYZ l_LineEnd = ConduitStartpt;
                        double space = GetConduitSpace(primaryElements.FirstOrDefault(), primaryElements[i], l_LineEnd)/* * Math.Tan(angle / 3)*/;
                        XYZ refStartPoint = l_LineEnd + (LineforDirectionofSecondary.Direction * (BaseDistance + space)) /*: l_LineEnd - (l_Line.Direction * (BaseDistance + zSpace + space))*/;
                        XYZ alignpointpt1 = refStartPoint;
                        refStartPoint = refStartPoint + perdiculardir.Multiply(offsetdistance);
                        basepointtwo = new XYZ(PickPoint.X, PickPoint.Y, refStartPoint.Z);

                        //point lie on the line or not 
                        XYZ alignpointsub1 = alignpointpt1 + CrossProduct.Multiply(2);
                        XYZ alignpointsub2 = alignpointpt1 - CrossProduct.Multiply(2);

                        XYZ Intersectionforroll = Utility.GetIntersection(Line.CreateBound(ConduitStartpt, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, ConduitStartpt.Z)),Line.CreateBound(alignpointsub1, alignpointsub2));


                        if (Intersectionforroll == null)
                        {
                            TaskDialog.Show("Result", "Please keep the picked point away").ToString();
                            breakfunction = true;
                            break;
                        }
                           


                        basepointone = refStartPoint;
                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, basepointtwo, basepointone);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Parameter newElevation = newCon.LookupParameter(offSetVar);
                        newElevation.Set(elevation);
                        Element ele = doc.GetElement(newCon.Id);
                        SetElevation(ele, elevation, offSet, offSetVar);
                        secondaryElements.Add(ele);

                        pickedElements.Add(primaryElements[i]);
                        // secondaryElements.Add(ele);
                    }
                    else
                    {
                        //direction indetification
                        Line reflineOne = (primaryElements[0].Location as LocationCurve).Curve as Line;
                        XYZ refpt1 = reflineOne.GetEndPoint(0);
                        XYZ refpt2 = reflineOne.GetEndPoint(1);
                        XYZ ref1Intersectionpoint = FindIntersectionPoint(refpt1, refpt2, PickPoint, PickPointTwo);

                        Line ref2lineOne = (primaryElements[1].Location as LocationCurve).Curve as Line;
                        XYZ ref2pt1 = ref2lineOne.GetEndPoint(0);
                        XYZ ref2pt2 = ref2lineOne.GetEndPoint(1);
                        XYZ ref2Intersectionpoint = FindIntersectionPoint(ref2pt1, ref2pt2, PickPoint, PickPointTwo);

                        Line refline2 = Line.CreateBound(new XYZ(ref1Intersectionpoint.X, ref1Intersectionpoint.Y, pt1.Z), new XYZ(ref2Intersectionpoint.X, ref2Intersectionpoint.Y, pt1.Z));
                        XYZ refperdiculardir = refline2.Direction;

                        LocationCurve curve = primaryElements[i].Location as LocationCurve;
                        Line l_Line = curve.Curve as Line;
                        //XYZ StartPoint = l_Line.GetEndPoint(0);
                        //XYZ EndPoint = l_Line.GetEndPoint(1);

                        XYZ strpt1 = l_Line.GetEndPoint(0);
                        XYZ endpt1 = l_Line.GetEndPoint(1);

                        //finding the shortest point
                        double SubdistanceOne = Math.Sqrt(Math.Pow((strpt1.X - Intersectionpoint.X), 2) + Math.Pow((strpt1.Y - Intersectionpoint.Y), 2));
                        double SubdistanceTwo = Math.Sqrt(Math.Pow((endpt1.X - Intersectionpoint.X), 2) + Math.Pow((endpt1.Y - Intersectionpoint.Y), 2));
                        XYZ ConduitStartpt = null;
                        XYZ ConduitEndpoint = null;
                        if (SubdistanceOne < SubdistanceTwo)
                        {
                            ConduitStartpt = strpt1;
                            ConduitEndpoint = endpt1;
                        }
                        else
                        {
                            ConduitStartpt = endpt1;
                            ConduitEndpoint = strpt1;
                        }

                        Line LineforDirectionofSecondary = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                        XYZ l_LineEnd = ConduitStartpt;
                        double space = GetConduitSpace(primaryElements.FirstOrDefault(), primaryElements[i], l_LineEnd) /** Math.Tan(angle / 3)*/;
                        //basepointone = basepointone + perdiculardir.Multiply(space);
                        //basepointtwo = basepointtwo + perdiculardir.Multiply(space);
                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, basepointone + refperdiculardir.Multiply(space), basepointtwo + refperdiculardir.Multiply(space));
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Parameter newElevation = newCon.LookupParameter(offSetVar);
                        newElevation.Set(elevation);
                        Element ele = doc.GetElement(newCon.Id);
                        SetElevation(ele, elevation, offSet, offSetVar);
                        secondaryElements.Add(ele);


                        //Line SublineOne = (pickedElements[i].Location as LocationCurve).Curve as Line;
                        //XYZ Subpt1 = SublineOne.GetEndPoint(0);
                        //XYZ Subpt2 = SublineOne.GetEndPoint(1);
                        //XYZ Secondintersections = FindIntersectionPoint(Subpt1, Subpt2, PickPoint, PickPointTwo);
                        //XYZ perpendicularlinedir = Line.CreateBound(new XYZ(Intersectionpoint.X, Intersectionpoint.Y, 0), new XYZ(Secondintersections.X, Secondintersections.Y, 0)).Direction;
                        //double speacing = Math.Sqrt(Math.Pow((Intersectionpoint.X - Secondintersections.X), 2) + Math.Pow((Intersectionpoint.Y - Secondintersections.Y), 2));
                        //XYZ strpt1 = PickPoint + perpendicularlinedir.Multiply(speacing);
                        //XYZ endpt1 = strpt1 - directionopfreferenecline.Multiply(2);
                        //Conduit newCon = Utility.CreateConduit(doc, pickedElements[i] as Conduit, strpt1, endpt1);
                        //double elevation = pickedElements[i].LookupParameter(offSetVar).AsDouble();
                        //Parameter newElevation = newCon.LookupParameter(offSetVar);
                        //newElevation.Set(elevation);
                        //Element ele = doc.GetElement(newCon.Id);
                        //SetElevation(ele, elevation, offSet, offSetVar);
                        //secondaryElements.Add(ele);

                        pickedElements.Add(primaryElements[i]);
                        //secondaryElements.Add(ele);

                    }

                }
                if (breakfunction == false)
                {
                    ParentUserControl.Instance.Secondaryelst.Clear();
                    ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                    ParentUserControl.Instance.Primaryelst.Clear();
                    ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                }
              
            }




        }
        public static double GetBaseDistance(double offSet, double rollingOffset, double angle)
        {
            double Roll = Math.Sqrt(Math.Pow(offSet, 2) + Math.Pow(rollingOffset, 2));
            double BendSpacing = Roll / Math.Cos((Math.PI / 2) - angle);
            double BaseDistance = Math.Sqrt(Math.Pow(BendSpacing, 2) - Math.Pow(Roll, 2));
            return BaseDistance;
        }
        public static double GetConduitSpace(Element firstElement, Element secondElement, XYZ Origin)
        {
            Line firstLine = (firstElement.Location as LocationCurve).Curve as Line;
            Line secondLine = (secondElement.Location as LocationCurve).Curve as Line;
            XYZ direction = secondLine.Direction;
            XYZ cross = direction.CrossProduct(XYZ.BasisZ);
            Line lineThree = Line.CreateBound(Origin, Origin + cross.Multiply(5));
            XYZ interSecPoint = Utility.FindIntersectionPoint(firstLine.GetEndPoint(0), firstLine.GetEndPoint(1), lineThree.GetEndPoint(0), lineThree.GetEndPoint(1));
            XYZ newEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, Origin.Z);
            return Origin.DistanceTo(newEndPoint);
        }
        private static void SetElevation(Element Ele, double elevation, double offSet, string offSetVar)
        {
            Parameter newElevation = Ele.LookupParameter(offSetVar);
            newElevation.Set(elevation + offSet);
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
        public static Line GetLineWithSpace(Element firstElement, Element secondElement, double space)
        {
            Line firstLine = (firstElement.Location as LocationCurve).Curve as Line;
            Line secondLine = (secondElement.Location as LocationCurve).Curve as Line;
            Utility.GetClosestConnectors(firstElement, secondElement, out Connector ConnectorOne, out Connector ConnectorTwo);
            XYZ direction = firstLine.Direction;
            XYZ cross = direction.CrossProduct(XYZ.BasisZ);
            Line lineThree = Line.CreateBound(ConnectorOne.Origin, ConnectorOne.Origin + cross.Multiply(ConnectorOne.Origin.DistanceTo(ConnectorTwo.Origin)));
            XYZ interSecPoint = Utility.FindIntersectionPoint(secondLine.GetEndPoint(0), secondLine.GetEndPoint(1), lineThree.GetEndPoint(0), lineThree.GetEndPoint(1));
            XYZ newEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, ConnectorTwo.Origin.Z);
            Line newLine = Line.CreateBound(ConnectorTwo.Origin, newEndPoint);
            return Line.CreateBound(ConnectorTwo.Origin + (newLine.Direction * space), newEndPoint);
        }

    }
}
