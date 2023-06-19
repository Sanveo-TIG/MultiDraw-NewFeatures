using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
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

                double offsetdistance = Math.Sqrt(Math.Pow((Pickpoint.X - Intersectionpoint.X), 2) + Math.Pow((Pickpoint.Y - Intersectionpoint.Y), 2));
                Line referenceline = Line.CreateBound(pt1, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, pt1.Z));
                XYZ directionopfreferenecline = referenceline.Direction;

              //  secondaryElements = new List<Element>();
                double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offsetdistance / Math.Tan(angle);
               // primaryElements = primaryElements.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();

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
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
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
