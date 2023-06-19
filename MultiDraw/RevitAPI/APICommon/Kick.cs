using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIGUtility;
using System.Windows.Shapes;
using Line = Autodesk.Revit.DB.Line;

namespace MultiDraw
{
    public class Kick
    {
        public static void GetSecondaryElements(Document doc, ref List<Element> primaryElements, double angle, double offSet, double rise, out List<Element> secondaryElements, string offSetVar,XYZ pickedPoint)
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
                Line line = Utility.CrossProductLine(primaryElements[0], pickedPoint, 1, true);
                line = Utility.CrossProductLine(line, pickedPoint, 1, true);
                Line line1 = Utility.CrossProductLine(primaryElements[0], Utility.GetXYvalue(orgin), 1, true);
                XYZ ip = FindIntersectionPoint(line, line1);
                if (ip != null)
                    pickedPoint = ip;
            }


            Dictionary<double, List<Element>> groupElements = new Dictionary<double, List<Element>>();
            Utility.GroupByElevation(primaryElements, offSetVar, ref groupElements);
            groupElements = groupElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);
            primaryElements = new List<Element>();
            angle = 90 - angle;
            double l_Angle = angle * Math.PI / 180;
            double basedistance = offSet * Math.Tan(l_Angle);
            basedistance = angle == 90 ? 1 : basedistance;
            if(rise < 0)
            {
                basedistance *= -1;
            }

            int k = 0;
            double correctspace = 0;
            double Baseelevation = 0;
            foreach (KeyValuePair<double, List<Element>> valuePair in groupElements)
            {
                double Zspace = (groupElements.FirstOrDefault().Key - valuePair.Key);
                List<Element> pickedElements = Utility.GetConduitsInOrderByPoint(valuePair.Value, pickedPoint);
                double Elevation = pickedElements[0].LookupParameter(offSetVar).AsDouble();
                if (k == 0)
                {
                    Baseelevation = pickedElements[0].LookupParameter(offSetVar).AsDouble();
                }
                if (k > 0)
                {
                    correctspace = Math.Sqrt(Math.Pow((Elevation - Baseelevation), 2));
                }
                for (int i = 0; i < pickedElements.Count; i++)
                {
                    Conduit con = pickedElements[i] as Conduit;
                    double conduitsize = con.LookupParameter("Outside Diameter").AsDouble();
                    LocationCurve curve = pickedElements[i].Location as LocationCurve;
                    Line l_Line = curve.Curve as Line;
                    XYZ StartPoint = l_Line.GetEndPoint(0);
                    XYZ EndPoint = l_Line.GetEndPoint(1);
                    XYZ cross = l_Line.Direction.CrossProduct(XYZ.BasisZ);
                    Line perpendicularLine = Line.CreateBound(pickedPoint + cross.Multiply(10), pickedPoint - cross.Multiply(10));
                    XYZ ip = Utility.FindIntersectionPoint(l_Line, perpendicularLine);
                    XYZ ConduitStartpt = null;
                    XYZ ConduitEndpoint = null;
                    if (ip.DistanceTo(StartPoint) < ip.DistanceTo(EndPoint))
                    {
                        ConduitStartpt = StartPoint;
                        ConduitEndpoint = EndPoint;
                    }
                    else
                    {
                        ConduitStartpt = EndPoint;
                        ConduitEndpoint = StartPoint;
                    }
                    Line Linefordirection = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                    XYZ LinefordirectionDir = Linefordirection.Direction;

                    Line ConduitLine = Line.CreateBound(ConduitStartpt, ConduitEndpoint);
                    XYZ refStartPoint = ConduitLine.GetEndPoint(0);
                    Line newLine = Line.CreateBound(new XYZ(ip.X,ip.Y, refStartPoint.Z), new XYZ(pickedPoint.X,pickedPoint.Y,refStartPoint.Z));
                    refStartPoint = new XYZ(refStartPoint.X, refStartPoint.Y, refStartPoint.Z + basedistance);
                    XYZ refEndPoint = new XYZ(refStartPoint.X, refStartPoint.Y, (refStartPoint.Z + rise));
                    refStartPoint += newLine.Direction.Multiply(offSet);
                    refEndPoint += newLine.Direction.Multiply(offSet);

                    if (k > 0)
                    {
                        if (rise > 0)
                        {
                            refStartPoint = refStartPoint + LinefordirectionDir.Multiply(correctspace + conduitsize);
                        }
                        else
                        {
                            refStartPoint = refStartPoint - LinefordirectionDir.Multiply(correctspace + conduitsize);
                        }

                    }
                    refEndPoint = new XYZ(refStartPoint.X, refStartPoint.Y, refStartPoint.Z + rise);
                    if (k > 0)
                    {
                        refEndPoint = new XYZ(refStartPoint.X, refStartPoint.Y, refEndPoint.Z + correctspace);
                    }

                    Conduit newCon = Utility.CreateConduit(doc, pickedElements[i] as Conduit, refStartPoint, refEndPoint);
                    Element ele = doc.GetElement(newCon.Id);
                    primaryElements.Add(pickedElements[i]);
                    secondaryElements.Add(ele);
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                k++;
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
