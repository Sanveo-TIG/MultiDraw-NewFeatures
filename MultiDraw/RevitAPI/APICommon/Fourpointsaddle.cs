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

        ///@Vertical method of fourpoint saddle in ZAxis
        public static void GetSecondaryZPointElements(Document doc, ref List<Element> primaryElements, double angle, double offSet, out List<Element> secondaryElements, out List<Element> fourthElements, string offSetVar, XYZ pickpoint)
        {
            secondaryElements = new List<Element>();
            fourthElements = new List<Element>();

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

                    double conduitDefaultLength = 2.0;
                    XYZ refEndPoint = refStartPoint + ConduitlineDir.Multiply(conduitDefaultLength);
                    Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                    double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                    Parameter newElevation = newCon.LookupParameter(offSetVar);
                    newElevation.Set(elevation + offSet);
                    Element ele = doc.GetElement(newCon.Id);
                    secondaryElements.Add(ele);


                    // Create the FourthConduit 
                    XYZ fourthConduitStartPoint = refEndPoint + ConduitlineDir.Multiply(Math.Abs(basedistance));
                    XYZ fourthConduitEndPoint = fourthConduitStartPoint + ConduitlineDir.Multiply(5);
                    Conduit fourthConduit = Utility.CreateConduit(doc, primaryElements[i] as Conduit, fourthConduitStartPoint, fourthConduitEndPoint);
                    Element fourthElement = doc.GetElement(fourthConduit.Id);
                    fourthElements.Add(fourthElement);
                }

                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(fourthElements);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(fourthElements);
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

        //4 point
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

            bool isVerticalConduits = false;
            XYZ parallelDirection = XYZ.Zero;
            for (int j = 0; j < PrimaryElements.Count; j++)
            {
                //Create 4th Conduit
                Line newLine = Utility.GetParallelLine(PrimaryElements[j], SecondaryElements[j], ref isVerticalConduits);
                parallelDirection = newLine.Direction;
                XYZ start = newLine.GetEndPoint(0);
                XYZ direction = (newLine.GetEndPoint(1) - start).Normalize();
                XYZ end = start + direction * 2;
                Conduit thirdConduit = Utility.CreateConduit(_doc, PrimaryElements[j] as Conduit, start, end);

                //Conduit thirdConduit = Utility.CreateConduit(_doc, PrimaryElements[j] as Conduit, newLine.GetEndPoint(0), newLine.GetEndPoint(1));
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

            for (int i = 0; i < SecondaryElements.Count; i++)
            {
                Element firstElement = PrimaryElements[i];
                Element secondElement = SecondaryElements[i];
                Element thirdElement = thirdElements[i];

                Line firstLine = (secondElement.Location as LocationCurve).Curve as Line;
                Line thirdLine = (thirdElement.Location as LocationCurve).Curve as Line;
                XYZ IP = Utility.FindIntersectionPoint(firstLine, thirdLine);
                if (IP != null && (secondElement.Location as LocationCurve).Curve.Length < 5.0)
                {
                    Line thirdlineNew = Line.CreateBound(new XYZ(IP.X, IP.Y, thirdLine.GetEndPoint(0).Z), new XYZ(IP.X, IP.Y, thirdLine.GetEndPoint(0).Z) + parallelDirection.Multiply(2));
                    (secondElement.Location as LocationCurve).Curve = thirdlineNew;
                }

                Utility.CreateElbowFittings(firstElement, thirdElement, _doc, uiApp);
                Utility.CreateElbowFittings(secondElement, thirdElement, _doc, uiApp);
            }
        }

        //4 point
        public static void GetSecondaryPointElements(Document doc, ref List<Element> primaryElements, out List<Element> secondaryElements, out List<Element> thirdConduits, string offSetVar, XYZ pickpoint)
        {
            secondaryElements = new List<Element>();
            thirdConduits = new List<Element>();
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

            LocationCurve Curve = primaryElements[0].Location as LocationCurve;
            Line l_Line = Curve.Curve as Line;
            XYZ startPoint1 = l_Line.GetEndPoint(0);
            XYZ endPoint1 = l_Line.GetEndPoint(1);
            XYZ PrimaryConduitDirection = l_Line.Direction;

            for (int i = 0; i < primaryElements.Count; i++)
            {
                // Retrieve the primary conduit and its geometry
                Conduit primaryConduit = primaryElements[i] as Conduit;
                LocationCurve primaryCurve = primaryElements[i].Location as LocationCurve;
                Line primaryLine = primaryCurve.Curve as Line;
                XYZ startPoint = primaryLine.GetEndPoint(0);
                XYZ endPoint = primaryLine.GetEndPoint(1);

                //LINE1
                XYZ perpendicularDir = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                XYZ perpendicularStartPoint = pickpoint;
                XYZ perpendicularEndPoint = pickpoint + perpendicularDir.Multiply(15);
                Line PickPointPerpendicularLine = Line.CreateBound(perpendicularStartPoint, perpendicularEndPoint);
                //DetailCurve perpendicularDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, PickPointPerpendicularLine);

                //LINE2
                XYZ extendedStartPoint = startPoint - PrimaryConduitDirection.Multiply(15);
                XYZ extendedEndPoint = endPoint + PrimaryConduitDirection.Multiply(15);
                Line parallelLine = Line.CreateBound(extendedStartPoint, extendedEndPoint);
                XYZ IpforOffset = Utility.FindIntersectionPoint(PickPointPerpendicularLine, parallelLine);

                //conduitDirection 
                XYZ NewGetPoint = null;
                if (IpforOffset != null)
                {
                    XYZ SP = primaryLine.GetEndPoint(0);
                    XYZ EP = primaryLine.GetEndPoint(1);

                    if (IpforOffset.DistanceTo(SP) < IpforOffset.DistanceTo(EP))
                    {
                        NewGetPoint = SP; //left side
                    }
                    else
                    {
                        NewGetPoint = EP; //right side 
                    }
                }

                Line conLine = Line.CreateBound(NewGetPoint, new XYZ(IpforOffset.X, IpforOffset.Y, NewGetPoint.Z));
                XYZ offsetStartPoint = NewGetPoint + conLine.Direction.Multiply(1.0);
                // Create the second conduit (5 feet long)
                Conduit secondConduit = Utility.CreateConduit(doc, primaryConduit, offsetStartPoint, offsetStartPoint + conLine.Direction.Multiply(4));
                Element secondElement = doc.GetElement(secondConduit.Id);
                secondaryElements.Add(secondElement);

                // Create the third conduit (5 feet long), starting 1 foot after the second conduit
                XYZ thirdConduitStartPoint = offsetStartPoint + conLine.Direction.Multiply(5); // 5 feet + 1 foot
                XYZ thirdConduitEndPoint = thirdConduitStartPoint + conLine.Direction.Multiply(6);
                Line thirdConduitLine = Line.CreateBound(thirdConduitStartPoint, thirdConduitEndPoint);
                Conduit thirdConduit = Utility.CreateConduit(doc, primaryConduit, thirdConduitStartPoint, thirdConduitEndPoint);
                Element thirdElement = doc.GetElement(thirdConduit.Id);
                thirdConduits.Add(thirdElement);
            }

            ParentUserControl.Instance.Secondaryelst.Clear();
            ParentUserControl.Instance.Secondaryelst.AddRange(thirdConduits);
            ParentUserControl.Instance.Primaryelst = new List<Element>();
            ParentUserControl.Instance.Primaryelst.AddRange(thirdConduits);
        }
    }
}





