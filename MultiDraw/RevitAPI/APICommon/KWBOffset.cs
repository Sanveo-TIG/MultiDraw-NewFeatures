﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TIGUtility;

namespace MultiDraw
{
    public class KWBOffset
    {
        public static void GetSecondaryElements(Document doc, ref List<Element> pickedElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar,XYZ pickpoint)
        {
            secondaryElements = new List<Element>();
            XYZ orgin = null;
            foreach (Element item in pickedElements)
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
                Line line = Utility.CrossProductLine(pickedElements[0], pickpoint, 1, true);
                line = Utility.CrossProductLine(line, pickpoint, 1, true);
                Line line1 = Utility.CrossProductLine(pickedElements[0], Utility.GetXYvalue(orgin), 1, true);
                XYZ ip = FindIntersectionPoint(line, line1);
                if (ip != null)
                    pickpoint = ip;
            }

            Line lineOne = (pickedElements[0].Location as LocationCurve).Curve as Line;
            XYZ pt1 = lineOne.GetEndPoint(0);
            XYZ pt2 = lineOne.GetEndPoint(1);
            XYZ midpoint = (pt1 + pt2) / 2;
            XYZ PrimaryConduitDirection = lineOne.Direction;
            XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
            XYZ PickPointTwo = pickpoint + CrossProduct.Multiply(1);

            XYZ Intersectionpoint = Utility.FindIntersectionPoint(pt1, pt2, pickpoint, PickPointTwo);
            Line PerpendicularconduitLine = Line.CreateBound(new XYZ(Intersectionpoint.X, Intersectionpoint.Y,0), new XYZ(pickpoint.X, pickpoint.Y,0));
            XYZ PerpendicularconduitLinedir = PerpendicularconduitLine.Direction;
            PerpendicularconduitLinedir = new XYZ(PerpendicularconduitLinedir.X,PerpendicularconduitLinedir.Y, 0);

            Line ConduitDirectionLine = Line.CreateBound(midpoint, Intersectionpoint);
            XYZ DirectionPfconduit = ConduitDirectionLine.Direction;
            DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z);

            //Set Line direction
            XYZ pickpointst1 = pickpoint + PrimaryConduitDirection.Multiply(1);
            XYZ midpoint2 = midpoint + CrossProduct.Multiply(1);
            XYZ intersectiompointTwo = Utility.FindIntersectionPoint(pickpoint, pickpointst1, midpoint, midpoint2);
            Line Linefoeoffset = Line.CreateBound(midpoint, intersectiompointTwo);
            XYZ Directionforoffset = Linefoeoffset.Direction;


            Dictionary<double, List<Element>> groupedElements = new Dictionary<double, List<Element>>();
            Utility.GroupByElevation(pickedElements, offSetVar, ref groupedElements);
            pickedElements = new List<Element>();
            groupedElements = groupedElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);
            double basedistance = offSet / Math.Tan(angle);
            if (offSet < 0)
            {
                basedistance = -basedistance;
            }

            foreach (KeyValuePair<double, List<Element>> valuePair in groupedElements)
            {
                XYZ Intersectionpoint_point = null;
                List<Element> primaryElementsforOrder = valuePair.Value.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                XYZ firstStartPoint = ((primaryElementsforOrder[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ firstEndPoint = ((primaryElementsforOrder[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                //oredred the element based on the pickpoint direction
                List<double> distancecollection = new List<double>();
                foreach (Element ele in primaryElementsforOrder)
                {
                    Conduit con = ele as Conduit;
                    XYZ Conpt1 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ Conpt2 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                    XYZ intersectiopointforOrderelement = Utility.FindIntersectionPoint(pickpoint, PickPointTwo, Conpt1, Conpt2);

                    double distance = Math.Sqrt(Math.Pow((pickpoint.X - intersectiopointforOrderelement.X), 2) + Math.Pow((pickpoint.Y - intersectiopointforOrderelement.Y), 2));
                    distancecollection.Add(distance);
                }
                distancecollection = distancecollection.OrderBy(x => x).ToList();
                List<Element> primaryElements = new List<Element>();
                foreach (double val in distancecollection)
                {
                    foreach (Element ele in primaryElementsforOrder)
                    {
                        Conduit con = ele as Conduit;
                        XYZ Conpt1 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                        XYZ Conpt2 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                        XYZ intersectiopointforOrderelement = Utility.FindIntersectionPoint(pickpoint, PickPointTwo, Conpt1, Conpt2);

                        double distance = Math.Sqrt(Math.Pow((pickpoint.X - intersectiopointforOrderelement.X), 2) + Math.Pow((pickpoint.Y - intersectiopointforOrderelement.Y), 2));
                        if (distance == val)
                        {
                            primaryElements.Add(ele);
                        }
                    }
                }
                primaryElements = primaryElements.Distinct().ToList();
                for (int i = 0; i < primaryElements.Count; i++)
                {
                    Line Mainl_Line = (primaryElements[0].Location as LocationCurve).Curve as Line;
                    XYZ MainStartPoint = Mainl_Line.GetEndPoint(0);
                    XYZ MainEndPoint = Mainl_Line.GetEndPoint(1);

                    Line l_Line = (primaryElements[i].Location as LocationCurve).Curve as Line;
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

                    Line LineForspacing = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                    XYZ LineForspacingDir = LineForspacing.Direction;

                    if (i == 0)
                    {
                        Intersectionpoint_point = Utility.FindIntersectionPoint(StartPoint, EndPoint, pickpoint, PickPointTwo);
                        XYZ refStartPoint = ConduitStartpt + LineForspacingDir.Multiply(0.1);
                        refStartPoint = ConduitStartpt + PerpendicularconduitLinedir.Multiply(basedistance);
                        XYZ refEndPoint = refStartPoint + PerpendicularconduitLinedir.Multiply(10);

                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Element ele = doc.GetElement(newCon.Id);
                        SetElevation(ele, elevation, offSet, offSetVar);
                        pickedElements.Add(primaryElements[i]);
                        secondaryElements.Add(ele);
                    }
                    else
                    {
                        XYZ IntersectionpointSub = Utility.FindIntersectionPoint(StartPoint, EndPoint, pickpoint, PickPointTwo);
                        double Spacing = Math.Sqrt(Math.Pow((Intersectionpoint_point.X - IntersectionpointSub.X), 2) + Math.Pow((Intersectionpoint_point.Y - IntersectionpointSub.Y), 2));

                        XYZ refStartPoint = ConduitStartpt + LineForspacingDir.Multiply(Spacing);
                        refStartPoint += PerpendicularconduitLinedir.Multiply(basedistance);
                        XYZ refEndPoint = refStartPoint + PerpendicularconduitLinedir.Multiply(10 + Spacing);

                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Element ele = doc.GetElement(newCon.Id);
                        SetElevation(ele, elevation, offSet, offSetVar);
                        pickedElements.Add(primaryElements[i]);
                        secondaryElements.Add(ele);

                    }
                }

                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }
        public static void GetSecondaryPointElements(Document doc, ref List<Element> pickedElements, double angle, double offSet, out List<Element> secondaryElements, string offSetVar, XYZ pickpoint)
        {
            secondaryElements = new List<Element>();
            XYZ orgin = null;
            foreach (Element item in pickedElements)
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
            using (SubTransaction transaction = new SubTransaction(doc))
            {
                XYZ removingPoint = null;
                transaction.Start();
                XYZ trimPoint = null;
                try
                {
                    trimPoint = pickpoint;
                }
                catch (Exception)
                {
                    transaction.Dispose();
                }
                if (pickedElements.TrueForAll(x => IsBothSideUnConnectors(x) == true))
                {
                    try
                    {
                        bool isAllNull = true;
                        foreach (Element element in pickedElements)
                        {
                            Line zLine = Utility.GetLineFromConduit(element);
                            Line xyLine = Utility.GetLineFromConduit(element, true);
                            XYZ startPoint = xyLine.GetEndPoint(0);
                            XYZ endPoint = xyLine.GetEndPoint(1);
                            Line trimLine = Utility.CrossProductLine(element, trimPoint, 50, true);
                            XYZ ipTrim = Utility.GetIntersection(xyLine, trimLine);
                            removingPoint = ipTrim;
                            if (ipTrim == null && isAllNull)
                                isAllNull = true;
                            else
                                isAllNull = false;
                        }
                        if (!isAllNull)
                        {
                            foreach (Element element in pickedElements)
                            {
                                Line zLine = Utility.GetLineFromConduit(element);
                                Line xyLine = Utility.GetLineFromConduit(element, true);
                                XYZ startPoint = xyLine.GetEndPoint(0);
                                XYZ endPoint = xyLine.GetEndPoint(1);
                                Line trimLine = Utility.CrossProductLine(element, trimPoint, 50, true);
                                XYZ ipTrim = Utility.GetIntersection(xyLine, trimLine);
                                if (ipTrim != null)
                                {
                                    Line firstLine = Line.CreateBound(startPoint, ipTrim);
                                    Line secondLine = Line.CreateBound(endPoint, ipTrim);
                                    Line removeLine = Utility.CrossProductLine(element, removingPoint, 50, true);

                                    XYZ ipRemove = Utility.FindIntersectionPoint(firstLine, removeLine);
                                    XYZ maxDisPoint = Utility.GetMaximumXYZ(xyLine.GetEndPoint(0), xyLine.GetEndPoint(1), ipRemove);
                                    maxDisPoint = Utility.SetZvalue(maxDisPoint, zLine.GetEndPoint(0));
                                    ipTrim = Utility.SetZvalue(ipTrim, zLine.GetEndPoint(0));

                                    (element.Location as LocationCurve).Curve = maxDisPoint.IsAlmostEqualTo(zLine.GetEndPoint(0)) ? Line.CreateBound(maxDisPoint, ipTrim) : Line.CreateBound(ipTrim, maxDisPoint);
                                }
                                else
                                {
                                    ipTrim = Utility.FindIntersectionPoint(xyLine, trimLine);
                                    XYZ maxDisPoint = Utility.GetMaximumXYZ(xyLine.GetEndPoint(0), xyLine.GetEndPoint(1), ipTrim);
                                    maxDisPoint = Utility.SetZvalue(maxDisPoint, zLine.GetEndPoint(0));
                                    ipTrim = Utility.SetZvalue(ipTrim, zLine.GetEndPoint(0));
                                    (element.Location as LocationCurve).Curve = maxDisPoint.IsAlmostEqualTo(zLine.GetEndPoint(0)) ? Line.CreateBound(maxDisPoint, ipTrim) : Line.CreateBound(ipTrim, maxDisPoint);
                                }
                            }
                        }
                        else
                        {
                            foreach (Element element in pickedElements)
                            {
                                Line xyLine = Utility.GetLineFromConduit(element, true);
                                Line trimLine = Utility.CrossProductLine(element, trimPoint, 50, true);
                                XYZ ipTrim = Utility.FindIntersection(element, trimLine);
                                XYZ maxDisPoint = Utility.GetMaximumXYZ(xyLine.GetEndPoint(0), xyLine.GetEndPoint(1), ipTrim);
                                Line line = Utility.GetLineFromConduit(element);
                                maxDisPoint = Utility.SetZvalue(maxDisPoint, line.GetEndPoint(0));
                                ipTrim = Utility.SetZvalue(ipTrim, line.GetEndPoint(0));
                                (element.Location as LocationCurve).Curve = maxDisPoint.IsAlmostEqualTo(line.GetEndPoint(0)) ? Line.CreateBound(maxDisPoint, ipTrim) : Line.CreateBound(ipTrim, maxDisPoint);
                                Color color = new Color(124, 252, 0);
                            }
                        }

                    }
                    catch (Exception)
                    {

                        transaction.Dispose();
                        ParentUserControl.Instance._window.Close();
                    }
                }
                else if (pickedElements.TrueForAll(x => IsOneSideConnectors(x) == true))
                {

                    foreach (Connector connector in (pickedElements[0] as Conduit).ConnectorManager.UnusedConnectors)
                        removingPoint = Utility.GetXYvalue(connector.Origin);

                    removingPoint = (removingPoint + trimPoint) / 2;
                    Line line_ = Utility.GetLineFromConduit(pickedElements[0]);
                    if (Utility.FindDifferentAxis(line_.GetEndPoint(0), line_.GetEndPoint(1)) == "Z")
                    {
                        transaction.Dispose();
                        Utility.AlertMessage("Feature not available for conduit stubs", false, MainWindow.Instance.SnackbarSeven);

                    }
                    else
                    {
                        //foreach (Element element in pickedElements)
                        //{
                        //    Line zLine = Utility.GetLineFromConduit(element);
                        //    Line xyLine = Utility.GetLineFromConduit(element, true);

                        //    Line trimLine = Utility.CrossProductLine(element, trimPoint, 50, true);
                        //    XYZ interSecPoint = Utility.FindIntersectionPoint(xyLine, trimLine);
                        //    if (interSecPoint != null)
                        //    {
                        //        ConnectorSet connectorSet = Utility.GetConnectors(element);
                        //        foreach (Connector con in connectorSet)
                        //        {
                        //            if (con.IsConnected)
                        //            {
                        //                if (con.Origin.IsAlmostEqualTo(zLine.GetEndPoint(0)))
                        //                {
                        //                    if (zLine.GetEndPoint(0).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)) > zLine.GetEndPoint(1).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)))
                        //                    {
                        //                        (element.Location as LocationCurve).Curve = Line.CreateBound(con.Origin, Utility.SetZvalue(interSecPoint, con.Origin));
                        //                        break;
                        //                    }
                        //                    else
                        //                    {
                        //                        Line l1 = Line.CreateBound(con.Origin, zLine.GetEndPoint(1));
                        //                        Line l2 = Line.CreateBound(new XYZ(con.Origin.X, con.Origin.Y, 0), interSecPoint);
                        //                        if (Math.Sign(Math.Round(l1.Direction.X, 5)) == Math.Sign(Math.Round(l2.Direction.X, 5)) && (Math.Sign(Math.Round(l1.Direction.Y, 5)) == Math.Sign(Math.Round(l2.Direction.Y, 5))))
                        //                        {

                        //                            (element.Location as LocationCurve).Curve = Line.CreateBound(con.Origin, Utility.SetZvalue(interSecPoint, con.Origin));
                        //                        }
                        //                        else
                        //                        {
                        //                            System.Windows.MessageBox.Show("Warning. \n" + "Cannot extend the conduits in opposite direction. Please pick another point", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //                            //return false;
                        //                        }
                        //                        break;
                        //                    }
                        //                }
                        //                if (con.Origin.IsAlmostEqualTo(zLine.GetEndPoint(1)))
                        //                {
                        //                    if (zLine.GetEndPoint(1).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)) > zLine.GetEndPoint(0).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)))
                        //                    {
                        //                        (element.Location as LocationCurve).Curve = Line.CreateBound(Utility.SetZvalue(interSecPoint, con.Origin), con.Origin);
                        //                        break;
                        //                    }
                        //                    else
                        //                    {
                        //                        Line l1 = Line.CreateBound(con.Origin, zLine.GetEndPoint(0));
                        //                        Line l2 = Line.CreateBound(new XYZ(con.Origin.X, con.Origin.Y, 0), interSecPoint);
                        //                        if (Math.Sign(Math.Round(l1.Direction.X, 5)) == Math.Sign(Math.Round(l2.Direction.X, 5)) && (Math.Sign(Math.Round(l1.Direction.Y, 5)) == Math.Sign(Math.Round(l2.Direction.Y, 5))))
                        //                        {

                        //                            (element.Location as LocationCurve).Curve = Line.CreateBound(Utility.SetZvalue(interSecPoint, con.Origin), con.Origin);
                        //                        }
                        //                        else
                        //                        {
                        //                            System.Windows.MessageBox.Show("Warning. \n" + "Cannot extend the conduits in opposite direction. Please pick another point", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //                            //return false;
                        //                        }
                        //                        break;

                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {

                        //    }
                        //}
                    }

                }
                transaction.Commit();
            }

            Line lineOne = (pickedElements[0].Location as LocationCurve).Curve as Line;
            XYZ pt1 = lineOne.GetEndPoint(0);
            XYZ pt2 = lineOne.GetEndPoint(1);
            XYZ midpoint = (pt1 + pt2) / 2;
            XYZ PrimaryConduitDirection = lineOne.Direction;
            XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
            XYZ PickPointTwo = pickpoint + CrossProduct.Multiply(1);


            //crossline pickpoint location
            Line baselineforpickpoint = Line.CreateBound(pt1, pt2);
            XYZ baselineforpickpoint_Dir = baselineforpickpoint.Direction;
            XYZ pickpoint_second_point = pickpoint + baselineforpickpoint_Dir.Multiply(5);


            XYZ Intersectionpoint = Utility.FindIntersectionPoint(pt1, pt2, pickpoint, PickPointTwo);
            Line PerpendicularconduitLine = Line.CreateBound(new XYZ(Intersectionpoint.X, Intersectionpoint.Y, 0), new XYZ(pickpoint.X, pickpoint.Y, 0));
            XYZ PerpendicularconduitLinedir = PerpendicularconduitLine.Direction;
            PerpendicularconduitLinedir = new XYZ(PerpendicularconduitLinedir.X, PerpendicularconduitLinedir.Y, 0);

            Line ConduitDirectionLine = Line.CreateBound(midpoint, Intersectionpoint);
            XYZ DirectionPfconduit = ConduitDirectionLine.Direction;
            DirectionPfconduit = new XYZ(DirectionPfconduit.X, DirectionPfconduit.Y, PrimaryConduitDirection.Z);

            //Set Line direction
            XYZ pickpointst1 = pickpoint + PrimaryConduitDirection.Multiply(1);
            XYZ midpoint2 = midpoint + CrossProduct.Multiply(1);
            XYZ intersectiompointTwo = Utility.FindIntersectionPoint(pickpoint, pickpointst1, midpoint, midpoint2);
            Line Linefoeoffset = Line.CreateBound(midpoint, intersectiompointTwo);
            XYZ Directionforoffset = Linefoeoffset.Direction;


            Dictionary<double, List<Element>> groupedElements = new Dictionary<double, List<Element>>();
            Utility.GroupByElevation(pickedElements, offSetVar, ref groupedElements);
            pickedElements = new List<Element>();
            groupedElements = groupedElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);
            double basedistance = offSet / Math.Tan(angle);
            if (offSet < 0)
            {
                basedistance = -basedistance;
            }

            foreach (KeyValuePair<double, List<Element>> valuePair in groupedElements)
            {
                XYZ Intersectionpoint_point = null;
                List<Element> primaryElementsforOrder = valuePair.Value.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                XYZ firstStartPoint = ((primaryElementsforOrder[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ firstEndPoint = ((primaryElementsforOrder[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                //oredred the element based on the pickpoint direction
                List<double> distancecollection = new List<double>();
                foreach (Element ele in primaryElementsforOrder)
                {
                    Conduit con = ele as Conduit;
                    XYZ Conpt1 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ Conpt2 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                    XYZ intersectiopointforOrderelement = Utility.FindIntersectionPoint(pickpoint, PickPointTwo, Conpt1, Conpt2);

                    double distance = Math.Sqrt(Math.Pow((pickpoint.X - intersectiopointforOrderelement.X), 2) + Math.Pow((pickpoint.Y - intersectiopointforOrderelement.Y), 2));
                    distancecollection.Add(distance);
                }
                distancecollection = distancecollection.OrderBy(x => x).ToList();
                List<Element> primaryElements = new List<Element>();
                foreach (double val in distancecollection)
                {
                    foreach (Element ele in primaryElementsforOrder) ///
                    {
                        Conduit con = ele as Conduit;
                        XYZ Conpt1 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                        XYZ Conpt2 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                        XYZ intersectiopointforOrderelement = Utility.FindIntersectionPoint(pickpoint, PickPointTwo, Conpt1, Conpt2);

                        double distance = Math.Sqrt(Math.Pow((pickpoint.X - intersectiopointforOrderelement.X), 2) + Math.Pow((pickpoint.Y - intersectiopointforOrderelement.Y), 2));
                        if (distance == val)
                        {
                            primaryElements.Add(ele);
                        }
                    }
                }
                primaryElements = primaryElements.Distinct().ToList();
                for (int i = 0; i < primaryElements.Count; i++)
                {
                    Line Mainl_Line = (primaryElements[0].Location as LocationCurve).Curve as Line;
                    XYZ MainStartPoint = Mainl_Line.GetEndPoint(0);
                    XYZ MainEndPoint = Mainl_Line.GetEndPoint(1);

                    Line l_Line = (primaryElements[i].Location as LocationCurve).Curve as Line;
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
                    Line LineForspacing = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                    XYZ LineForspacingDir = LineForspacing.Direction;
                    if (i == 0)
                    {
                        Intersectionpoint_point = Utility.FindIntersectionPoint(StartPoint, EndPoint, pickpoint, PickPointTwo);
                        XYZ refStartPoint = ConduitStartpt + LineForspacingDir.Multiply(0.1);
                        refStartPoint = ConduitStartpt + PerpendicularconduitLinedir.Multiply(basedistance);
                        XYZ refEndPoint = refStartPoint + PerpendicularconduitLinedir.Multiply(10);

                        refEndPoint = FindIntersectionPoint(refStartPoint, refEndPoint,pickpoint, pickpoint_second_point);
                        refEndPoint = new XYZ(refEndPoint.X, refEndPoint.Y, refStartPoint.Z);


                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Element ele = doc.GetElement(newCon.Id);
                        SetElevation(ele, elevation, offSet, offSetVar);
                        pickedElements.Add(primaryElements[i]);
                        secondaryElements.Add(ele);
                    }
                    else
                    {
                        XYZ IntersectionpointSub = Utility.FindIntersectionPoint(StartPoint, EndPoint, pickpoint, PickPointTwo);
                        double Spacing = Math.Sqrt(Math.Pow((Intersectionpoint_point.X - IntersectionpointSub.X), 2) + Math.Pow((Intersectionpoint_point.Y - IntersectionpointSub.Y), 2));

                        XYZ refStartPoint = ConduitStartpt + LineForspacingDir.Multiply(Spacing);
                        refStartPoint += PerpendicularconduitLinedir.Multiply(basedistance);
                        XYZ refEndPoint = refStartPoint + PerpendicularconduitLinedir.Multiply(10 + Spacing);

                        refEndPoint = FindIntersectionPoint(refStartPoint, refEndPoint, pickpoint, pickpoint_second_point);
                        refEndPoint = new XYZ(refEndPoint.X, refEndPoint.Y, refStartPoint.Z);

                        Conduit newCon = Utility.CreateConduit(doc, primaryElements[i] as Conduit, refStartPoint, refEndPoint);
                        double elevation = primaryElements[i].LookupParameter(offSetVar).AsDouble();
                        Element ele = doc.GetElement(newCon.Id);
                        SetElevation(ele, elevation, offSet, offSetVar);
                        pickedElements.Add(primaryElements[i]);
                        secondaryElements.Add(ele);
                    }
                }
                
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
            }
        }
        private static void SetElevation(Element Ele, double elevation, double offSet, string offSetVar)
        {
            Parameter newElevation = Ele.LookupParameter(offSetVar);
            newElevation.Set(elevation + offSet);
        }
        public static bool IsBothSideUnConnectors(Element e)
        {
            if (e is Conduit conduit)
            {
                return conduit.ConnectorManager.UnusedConnectors.Size == 2;
            }
            return false;
        }
        public static bool IsOneSideConnectors(Element e)
        {
            if (e is Conduit conduit)
            {
                return conduit.ConnectorManager.UnusedConnectors.Size == 1;
            }
            return false;
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

