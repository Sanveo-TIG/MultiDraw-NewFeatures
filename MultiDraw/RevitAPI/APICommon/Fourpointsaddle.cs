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
    public class Fourpointsaddle
    {
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

                    //XYZ dirrefline = Line.CreateUnbound(refEndPoint, refStartPoint).Direction;
                    //XYZ threesaddlept = refEndPoint + dirrefline.Multiply();

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