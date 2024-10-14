using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TIGUtility;

namespace MultiDraw
{
    public class Threepointsaddle
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

                    double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();//

                    Parameter newElevation = newCon.LookupParameter(offSetVar);

                    newElevation.Set(elevation + offSet);  //

                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);
                }

                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }

        public static void GetSecondaryFourPointElements(Document doc, ref List<Element> primaryElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar, XYZ pickpoint)
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

                DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z); //PrimaryConduitDirection.Z
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
                        conduitlength = Math.Sqrt(Math.Pow((refStartPoint.X - intersectionpointforvertical.X), 2) +
                                        Math.Pow((refStartPoint.Y - intersectionpointforvertical.Y), 2));
                    }
                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(conduitlength);
                    Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);  //@
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
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }

        public static void HorizontalOffsetPrimary(Document _doc, UIApplication uiApp, List<Element> PrimaryElements, List<Element> SecondaryElements, double l_angle)
        {
            List<Element> thirdElements = new List<Element>();
            XYZ orgin = null;
            foreach (Conduit item in PrimaryElements)
            {
                ConnectorSet PrimaryConnector = Utility.GetUnusedConnectors(item);
                if (PrimaryConnector.Size == 1)
                {
                    foreach (Connector con in PrimaryConnector)
                    {
                        orgin = con.Origin;
                        break;
                    }
                    break;
                }
            }

            ConnectorSet PrimaryConnectors = null;
            ConnectorSet SecondaryConnectors = null;
            bool isVerticalConduits = false;

            for (int j = 0; j < PrimaryElements.Count; j++)
            {
                //Create 4th Conduit
                Line newLine = Utility.GetParallelLine(PrimaryElements[j], SecondaryElements[j], ref isVerticalConduits);
                Conduit thirdConduit = Utility.CreateConduit(_doc, PrimaryElements[j] as Conduit, newLine.GetEndPoint(0), newLine.GetEndPoint(1));
                thirdElements.Add(thirdConduit);
            }

            Element firstConduitElement = PrimaryElements[0];
            Element SecondConduitElement = SecondaryElements[0];
            Utility.GetClosestConnectors(firstConduitElement, SecondConduitElement, out Connector ConnectorOne, out Connector ConnectorTwo);
            XYZ axisStart = ConnectorOne.Origin;
            XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
            Line axisLine = Line.CreateBound(axisStart, axisEnd);

            //Rotate 4th Conduit Element
            if (isVerticalConduits)
            {
                axisEnd = new XYZ(ConnectorTwo.Origin.X, ConnectorTwo.Origin.Y, ConnectorOne.Origin.Z);
                axisLine = Line.CreateBound(axisStart, axisEnd);
                XYZ dir = axisLine.Direction;
                dir = new XYZ(dir.X, dir.Y, 0);
                XYZ cross = dir.CrossProduct(XYZ.BasisZ);
                Element ele = thirdElements[0];
                LocationCurve newconcurve = ele.Location as LocationCurve;
                Line ncl1 = newconcurve.Curve as Line;
                XYZ MidPoint = ncl1.GetEndPoint(0);
                axisLine = Line.CreateBound(MidPoint, MidPoint + cross.Multiply(10));
                ElementTransformUtils.RotateElements(_doc, thirdElements.Select(r => r.Id).ToList(), axisLine, -l_angle);
            }
            else
            {
                ElementTransformUtils.RotateElements(_doc, thirdElements.Select(r => r.Id).ToList(), axisLine, l_angle);
                //find right angle
                Conduit refcondone = thirdElements[0] as Conduit;
                Line refcondoneline = (refcondone.Location as LocationCurve).Curve as Line;
                XYZ refcondonelinedir = refcondoneline.Direction;
                XYZ refcondonelinemidept = (refcondoneline.GetEndPoint(0) + refcondoneline.GetEndPoint(1)) / 2;
                XYZ addedpt1 = refcondonelinemidept + refcondonelinedir.Multiply(250);
                XYZ addedpt2 = refcondonelinemidept - refcondonelinedir.Multiply(250);
                Line addedline = Line.CreateBound(addedpt1, addedpt2);

                Line refcondtwoline = (SecondConduitElement.Location as LocationCurve).Curve as Line;
                XYZ intersectionpoint = Utility.GetIntersection(addedline, refcondtwoline);
                if (intersectionpoint == null)
                {
                    ElementTransformUtils.RotateElements(_doc, thirdElements.Select(r => r.Id).ToList(), axisLine, -2 * l_angle);
                }
            }

            for (int i = 0; i < PrimaryElements.Count; i++)
            {
                Element firstElement = PrimaryElements[i];
                Element secondElement = SecondaryElements[i];
                Element thirdElement = thirdElements[i];

                Utility.CreateElbowFittings(firstElement, thirdElement, _doc, uiApp);
                Utility.CreateElbowFittings(secondElement, thirdElement, _doc, uiApp);
            }
        }

        public static void ThreePointSaddleConnect(Document _doc, UIApplication uiApp, List<Element> PrimaryElements, List<Element> SecondaryElements, double l_angle,out List<Element> thirdElements)
        {
           thirdElements = new List<Element>();
            XYZ orgin = null;
            foreach (Conduit item in PrimaryElements)
            {
                ConnectorSet PrimaryConnector = Utility.GetUnusedConnectors(item);
                if (PrimaryConnector.Size == 1)
                {
                    foreach (Connector con in PrimaryConnector)
                    {
                        orgin = con.Origin;
                        break;
                    }
                    break;
                }
            }
            ConnectorSet PrimaryConnectors = null;
            ConnectorSet SecondaryConnectors = null;
            bool isVerticalConduits = false;
           
            Conduit thirdConduit = null;
           
            for (int j = 0; j < PrimaryElements.Count; j++)
            {
                //Create 4th Conduit
                Line newLine = Utility.GetParallelLine(PrimaryElements[j], SecondaryElements[j], ref isVerticalConduits);
                thirdConduit = Utility.CreateConduit(_doc, PrimaryElements[j] as Conduit, newLine.GetEndPoint(0), newLine.GetEndPoint(1));
                thirdElements.Add(thirdConduit);
            }
            Element firstConduitElement = PrimaryElements[0];
            Element SecondConduitElement = SecondaryElements[0];
            Utility.GetClosestConnectors(firstConduitElement, SecondConduitElement, out Connector ConnectorOne, out Connector ConnectorTwo);
            XYZ axisStart = ConnectorOne.Origin;
            XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
            Line axisLine = Line.CreateBound(axisStart, axisEnd);

            //Rotate 4th Conduit Element
            if (isVerticalConduits)
            {
                axisEnd = new XYZ(ConnectorTwo.Origin.X, ConnectorTwo.Origin.Y, ConnectorOne.Origin.Z);
                axisLine = Line.CreateBound(axisStart, axisEnd);
                XYZ dir = axisLine.Direction;
                dir = new XYZ(dir.X, dir.Y, 0);
                XYZ cross = dir.CrossProduct(XYZ.BasisZ);
                Element ele = thirdElements[0];
                LocationCurve newconcurve = ele.Location as LocationCurve;
                Line ncl1 = newconcurve.Curve as Line;
                XYZ MidPoint = ncl1.GetEndPoint(0);
                axisLine = Line.CreateBound(MidPoint, MidPoint + cross.Multiply(10));
                ElementTransformUtils.RotateElements(_doc, thirdElements.Select(r => r.Id).ToList(), axisLine, -l_angle);
            }
            else
            {
                ElementTransformUtils.RotateElements(_doc, thirdElements.Select(r => r.Id).ToList(), axisLine, l_angle);
                //find right angle
                Conduit refcondone = thirdElements[0] as Conduit;
                Line refcondoneline = (refcondone.Location as LocationCurve).Curve as Line;
                XYZ refcondonelinedir = refcondoneline.Direction;
                XYZ refcondonelinemidept = (refcondoneline.GetEndPoint(0) + refcondoneline.GetEndPoint(1)) / 2;
                XYZ addedpt1 = refcondonelinemidept + refcondonelinedir.Multiply(250);
                XYZ addedpt2 = refcondonelinemidept - refcondonelinedir.Multiply(250);
                Line addedline = Line.CreateBound(addedpt1, addedpt2);
                Line refcondtwoline = (SecondConduitElement.Location as LocationCurve).Curve as Line;
                XYZ intersectionpoint = Utility.GetIntersection(addedline, refcondtwoline);
                if (intersectionpoint == null)
                {
                    ElementTransformUtils.RotateElements(_doc, thirdElements.Select(r => r.Id).ToList(), axisLine, -2 * l_angle);
                }
            }
        }

        public static void GetSecondaryPointElements(Document doc, ref List<Element> primaryElements, double basedistance, out List<Element> secondaryElements, string offSetVar, XYZ pickpoint)
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

            for (int i = 0; i < primaryElements.Count; i++)
            {
                Line lineOne = (primaryElements[i].Location as LocationCurve).Curve as Line;

                XYZ pt1 = lineOne.GetEndPoint(0);
                XYZ pt2 = lineOne.GetEndPoint(1);
                XYZ PrimaryConduitDirection = lineOne.Direction;
                XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ pickPointTwo = pickpoint + CrossProduct.Multiply(10);
                XYZ pointOne = pickpoint;
                XYZ pointTwo = pickPointTwo;
                Line PickLine = Line.CreateBound(pointOne, pointTwo);
                XYZ Intersectionpoint = Utility.FindIntersectionPoint(lineOne, PickLine);

                //DISTANCE
                XYZ ConduitStartpt = null;
                if (Intersectionpoint.DistanceTo(pt1) < Intersectionpoint.DistanceTo(pt2))
                {
                    ConduitStartpt = pt1;
                }
                else
                {
                    ConduitStartpt = pt2;
                }

                Line conduitLine = Line.CreateBound(ConduitStartpt, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, ConduitStartpt.Z));

                //CREATE SECONDARY CONDUIT 
                Conduit secondaryConduit = Utility.CreateConduit(doc, primaryElements[i] as Conduit, ConduitStartpt + conduitLine.Direction.Multiply(basedistance),
                                        (ConduitStartpt + conduitLine.Direction.Multiply(basedistance)) + conduitLine.Direction.Multiply(10));
                double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                Parameter newElevation = secondaryConduit.LookupParameter(offSetVar);
                newElevation.Set(elevation);
                Element ele = doc.GetElement(secondaryConduit.Id);
                secondaryElements.Add(ele);
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

            double basedistance = (angle * (180 / Math.PI)) == 90 ? 1 : offSet / Math.Tan(angle);

            //ConduitElevation identification
            XYZ e1pt1 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
            XYZ e1pt2 = ((primaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

            double Z1 = Math.Round(e1pt1.Z, 2);
            double Z2 = Math.Round(e1pt2.Z, 2);

            for (int i = 0; i < primaryElements.Count; i++)
            {
                Line lineOne = (primaryElements[i].Location as LocationCurve).Curve as Line;

                XYZ pt1 = lineOne.GetEndPoint(0);
                XYZ pt2 = lineOne.GetEndPoint(1);
                XYZ PrimaryConduitDirection = lineOne.Direction;
                XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ pickPointTwo = pickpoint + CrossProduct.Multiply(10);
                XYZ pointOne = pickpoint;
                XYZ pointTwo = pickPointTwo;
                Line PickLine = Line.CreateBound(pointOne, pointTwo);
                XYZ Intersectionpoint = Utility.FindIntersectionPoint(lineOne, PickLine);

                //DISTANCE
                XYZ ConduitStartpt = null;
                if (Intersectionpoint.DistanceTo(pt1) < Intersectionpoint.DistanceTo(pt2))
                {
                    ConduitStartpt = pt1;
                }
                else
                {
                    ConduitStartpt = pt2;
                }

                Line conduitLine = Line.CreateBound(ConduitStartpt, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, ConduitStartpt.Z));
                Line newLine = Line.CreateBound(ConduitStartpt, ConduitStartpt + conduitLine.Direction.Multiply(basedistance * 2));

                //CREATE SECONDARY CONDUIT 
                Conduit secondaryConduit = Utility.CreateConduit(doc, primaryElements[i] as Conduit, ConduitStartpt + conduitLine.Direction.Multiply(basedistance * 2),
                                        (ConduitStartpt + conduitLine.Direction.Multiply(basedistance * 2)) + conduitLine.Direction.Multiply(10));
                double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                Parameter newElevation = secondaryConduit.LookupParameter(offSetVar);
                newElevation.Set(elevation);
                Element ele = doc.GetElement(secondaryConduit.Id);
                secondaryElements.Add(ele);
            }
        }
        
        public static void GetSecondaryFourPointElementZAxiz(Document doc, ref List<Element> primaryElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar, XYZ pickpoint)
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
                        conduitlength = Math.Sqrt(Math.Pow((refStartPoint.X - intersectionpointforvertical.X), 2) + 
                            Math.Pow((refStartPoint.Y - intersectionpointforvertical.Y), 2));
                    }
                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(conduitlength);
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
            return delta == 0 ? null : new XYZ((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta, 0);
        }
    }
}

