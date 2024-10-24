using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using MultiDraw;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Controls;
using System.Windows;
using TIGUtility;
using System.Net;
using System.Collections.ObjectModel;
using Color = Autodesk.Revit.DB.Color;
using System.Security.Cryptography;
using System.Runtime.Remoting.Channels;
using System.Drawing;

namespace MultiDraw
{
    public class APICommon
    {
        public static ICollection<ElementId> _elementSelected = new Collection<ElementId>();
        public static List<MultiSelect> _selectedSyncDataList = new List<MultiSelect>();
        public static List<SYNCDataGlobalParam> globalParamSync = new List<SYNCDataGlobalParam>();
        public static void AlertMessage(string msg, bool isSuccess, Snackbar SnackbarSeven)
        {
            if (isSuccess)
                SnackbarSeven.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D9A");
            else if (!isSuccess)
                SnackbarSeven.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#fbb511");
            else
                SnackbarSeven.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0000");
            SnackbarSeven.MessageQueue?.Enqueue(
                                 msg,
                                 "OK",
                                 param => SnackbarSeven.MessageQueue?.Clear(),
                                 null,
                                 false,
                                 true,
                                 TimeSpan.FromSeconds(15));
        }
        //private static void BOTTOMAddTags(Document doc, List<FamilyInstance> elements)
        //{
        //    int j = 1;
        //    for (int i = 0; i < elements.Count; i++)
        //    {
        //        Element ele = elements[i];
        //        TagMode tagmode = TagMode.TM_ADDBY_CATEGORY;
        //        TagOrientation tagorg = TagOrientation.Horizontal;
        //        Reference conref = new Reference(ele);
        //        XYZ locationpoint = (ele.Location as LocationPoint).Point;
        //        IndependentTag newtag = IndependentTag.Create(doc, doc.ActiveView.Id, conref, true, tagmode, tagorg, locationpoint);
        //        newtag.LeaderEndCondition = LeaderEndCondition.Free;

        //        newtag.LeaderEnd = locationpoint;
        //        XYZ elbowpnt = locationpoint + new XYZ(2, -j/2, 0);
        //        newtag.LeaderElbow = elbowpnt;


        //        XYZ headerpnt = elbowpnt + new XYZ(4, 0, 0);

        //        newtag.TagHeadPosition = headerpnt;
        //        j++;
        //    }
        //}
        //private static void TOPAddTags(Document doc, List<FamilyInstance> elements)
        //{
        //    int j = 1;
        //    for (int i = 0; i < elements.Count; i++)
        //    {
        //        Element ele = elements[i];
        //        TagMode tagmode = TagMode.TM_ADDBY_CATEGORY;
        //        TagOrientation tagorg = TagOrientation.Horizontal;
        //        Reference conref = new Reference(ele);
        //        XYZ locationpoint = (ele.Location as LocationPoint).Point;
        //        IndependentTag newtag = IndependentTag.Create(doc, doc.ActiveView.Id, conref, true, tagmode, tagorg, locationpoint);
        //        newtag.LeaderEndCondition = LeaderEndCondition.Free;

        //        newtag.LeaderEnd = locationpoint;
        //        XYZ elbowpnt = locationpoint + new XYZ(2,-j / 2, 0);
        //        newtag.LeaderElbow = elbowpnt;


        //        XYZ headerpnt = elbowpnt + new XYZ(4, 0, 0);

        //        newtag.TagHeadPosition = headerpnt;
        //        j++;
        //    }
        //}
        //public static void Conduitcoloroverride(ElementId eid, Document doc)
        //{
        //    var patternCollector = new FilteredElementCollector(doc);
        //    patternCollector.OfClass(typeof(FillPatternElement));
        //    FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");
        //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Hoffset = SetOverrideGraphicSettings(fpe, new Color(50, 205, 50));
        //    doc.ActiveView.SetElementOverrides(eid, ogs_Hoffset);
        //}
        //public static void PrimryConduitcoloroverride(ElementId eid, Document doc)
        //{
        //    var patternCollector = new FilteredElementCollector(doc);
        //    patternCollector.OfClass(typeof(FillPatternElement));
        //    FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");
        //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Hoffset = SetOverrideGraphicSettings(fpe, new Color(1, 50, 32));
        //    doc.ActiveView.SetElementOverrides(eid, ogs_Hoffset);
        //}


        public static bool HOffsetDrawHandler(Document doc, UIApplication uiapp, List<Element> pickedElements, string offsetVariable, bool Refpiuckpoint, XYZ Pickpoint, ref List<Element> secondaryElements)
        {
            /* string jsonParam = Utility.GetGlobalParametersManager(uiapp, "SyncDataParameters");
             if (!string.IsNullOrEmpty(jsonParam))
             {
                 globalParamSync = JsonConvert.DeserializeObject<List<SYNCDataGlobalParam>>(jsonParam);
             }*/

            DateTime startDate = DateTime.UtcNow;
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            try
            {
                List<Element> thirdElements = new List<Element>();
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

                if (HOffsetUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                HOffsetGP globalParam = new HOffsetGP
                {
                    OffsetValue = HOffsetUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : HOffsetUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = HOffsetUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };
                bool isVerticalConduits = false;
                double angle = Convert.ToDouble(HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                double offSet = HOffsetUserControl.Instance.txtOffsetFeet.AsDouble;

                using (SubTransaction tx = new SubTransaction(doc))
                {
                    tx.Start();
                    startDate = DateTime.UtcNow;
                    Properties.Settings.Default.HorizontalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    //
                    HorizontalOffset.GetSecondaryElements(doc, ref pickedElements, angle, offSet, offsetVariable, out secondaryElements, Pickpoint, Refpiuckpoint);

                    for (int i = 0; i < pickedElements.Count; i++)
                    {
                        Element firstElement = pickedElements[i];
                        Element secondElement = secondaryElements[i];
                        ///List<Element> elements = new List<Element>() { firstElement, secondElement };

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
                        /// ApplyParameters(doc, thirdConduit, elements);
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

                        //if (i == 0)
                        //{
                        //    PrimryConduitcoloroverride(secondaryElements[i].Id, doc);
                        //}
                        //else
                        //{
                        //    Conduitcoloroverride(secondaryElements[i].Id, doc);
                        //}

                        bOTTOMForAddtags.Add(fittings1);
                        TOPForAddtags.Add(fittings2);
                    }

                    // BOTTOMAddTags(doc, bOTTOMForAddtags);
                    // TOPAddTags(doc, TOPForAddtags);
                    // Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(pickedElements) }, new List<ConduitsCollection> { new ConduitsCollection(secondaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(thirdElements) });
                    tx.Commit();
                    using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in pickedElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, doc, uiapp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Horizontal Offset", Util.ProductVersion, "Draw");
                }
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Pick operation aborted.")
                {
                    ParentUserControl.Instance._window.Close();
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Horizontal Offset", Util.ProductVersion, "Draw");
                }
            }
            return true;
        }

        /*private static void ApplyParameters(Document doc, Conduit eleConduit, List<Element> elements)
        {
            foreach (SYNCDataGlobalParam param in globalParamSync)
            {

                Parameter ConduitParam = eleConduit.LookupParameter(param.Name);
                string paramValue = Utility.GetParameterValue(ConduitParam);
                if (eleConduit.RunId != ElementId.InvalidElementId)
                {
                    Element eleRun = doc.GetElement(eleConduit.RunId);
                    if (eleRun != null)
                    {
                        Parameter RunParam = eleRun.LookupParameter(param.Name);
                        if (RunParam != null && !RunParam.IsReadOnly)
                            Utility.SetParameterValue(RunParam, ConduitParam);
                    }

                    foreach (Element e in elements.Distinct())
                    {
                        if (e.GetType() == typeof(Conduit) || (e.GetType() == typeof(FamilyInstance)))
                        {
                            Parameter lookUpParam = e.LookupParameter(param.Name);
                            if (lookUpParam != null && ConduitParam != null)
                                Utility.SetParameterValue(lookUpParam, ConduitParam);
                        }
                    }
                }
            }
        }*/
        //backup old code
        /*        public static bool HOffsetDrawPointHandlerOLD(Document doc, UIApplication uiapp, List<Element> pickedElements, string offsetVariable, bool Refpiuckpoint, XYZ Pickpoint, ref List<Element> secondaryElements, ref bool fittingsfailure)
                {
                    string json = Properties.Settings.Default.ProfileColorSettings;
                    ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
                    DateTime startDate = DateTime.UtcNow;
                    fittingsfailure = false;
                    try
                    {
                        List<Element> thirdElements = new List<Element>();
                        if (HOffsetUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                        {
                            return false;
                        }
                        HOffsetGP globalParam = new HOffsetGP
                        {
                            OffsetValue = HOffsetUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : HOffsetUserControl.Instance.txtOffsetFeet.AsString,
                            AngleValue = HOffsetUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()
                        };
                        // bool isVerticalConduits = false;
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
                        double angle = Convert.ToDouble(HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                        using (SubTransaction tx = new SubTransaction(doc))
                        {
                            tx.Start();
                            startDate = DateTime.UtcNow;
                            List<Element> backupsele = new List<Element>();
                            foreach (Element element in pickedElements)
                            {
                                backupsele.Add(element);
                            }
                            Properties.Settings.Default.HorizontalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                            Properties.Settings.Default.Save();

                            HorizontalOffset.GetSecondaryElementsWithOffsetPoint(doc, ref pickedElements, angle, offsetVariable, out secondaryElements, Pickpoint, Refpiuckpoint);

                            //check the angle direction
                            LocationCurve curve = pickedElements[0].Location as LocationCurve;
                            Line l_Line = curve.Curve as Line;
                            XYZ StartPoint = l_Line.GetEndPoint(0);
                            XYZ EndPoint = l_Line.GetEndPoint(1);
                            XYZ PrimaryConduitDirection = l_Line.Direction;
                            XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                            XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1); //1

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
                            for (int i = 0; i < pickedElements.Count; i++)
                            {
                                double offsetDistance = 0.0;
                                Element firstElement = pickedElements[i];
                                Element secondElement = secondaryElements[i];

                                Line firstLine = (firstElement.Location as LocationCurve).Curve as Line;
                                Line secondLine = (secondElement.Location as LocationCurve).Curve as Line;

                                //XYZ sp = Pickpoint;//-
                                //XYZ dir = Pickpoint + secondLine.Direction.Multiply(10);
                                ///Line ipLine = Line.CreateBound(sp, dir);  //vLine 

                                //Line 1
                                XYZ perpendicularDir = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                                XYZ perpendicularStartPoint = Pickpoint;
                                XYZ perpendicularEndPoint = Pickpoint + perpendicularDir.Multiply(15);
                                Line PickPointPerpendicularLine = Line.CreateBound(perpendicularStartPoint, perpendicularEndPoint);
                                //DetailCurve perpendicularDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, PickPointPerpendicularLine);

                                //Line 2
                                XYZ extendedStartPoint = StartPoint - PrimaryConduitDirection.Multiply(15);
                                XYZ extendedEndPoint = EndPoint + PrimaryConduitDirection.Multiply(15);
                                Line parallelLine = Line.CreateBound(extendedStartPoint, extendedEndPoint);
                                //DetailCurve horizontalDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, parallelLine);

                                XYZ IpforOffset = Utility.FindIntersectionPoint(PickPointPerpendicularLine, parallelLine);

                                //conduitDirection 
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

                                if (IpforOffset != null)
                                {
                                    offsetDistance = Pickpoint.DistanceTo(IpforOffset);
                                }

                                ////direction

                                Line IpforOffsetLine = Line.CreateBound(NewGetPoint, new XYZ(IpforOffset.X, IpforOffset.Y, NewGetPoint.Z));
                                XYZ secondaryDirection = IpforOffsetLine.Direction;

                                XYZ offsetDirection = secondaryDirection.Multiply(offsetDistance);
                                XYZ secConduit = NewGetPoint;

                                XYZ endPoint = secConduit + offsetDirection;
                                XYZ conduitEndPoint = endPoint + secondaryDirection.Multiply(10);

                                Line CrossLine = Line.CreateBound(IpforOffset, new XYZ(Pickpoint.X, Pickpoint.Y, 0));
                                endPoint += CrossLine.Direction.Multiply(offsetDistance);
                                conduitEndPoint += CrossLine.Direction.Multiply(offsetDistance);

                                //
                                XYZ perpendicularDirection = secondaryDirection.CrossProduct(XYZ.BasisZ).Normalize();


                                Conduit thirdConduit = Utility.CreateConduit(doc, firstElement as Conduit, endPoint, conduitEndPoint);


                                ///@
                                //XYZ sCon = secondLine.GetEndPoint(1);
                                //XYZ secondaryDirection = secondLine.Direction;
                                //XYZ perpendicularDirection = secondaryDirection.CrossProduct(XYZ.BasisZ).Normalize();

                                //XYZ horizontalDirection = new XYZ(perpendicularDirection.X, perpendicularDirection.Y, 0).Normalize();
                                //XYZ directionToPickpoint = (Pickpoint - sCon).Normalize();
                                //if (perpendicularDirection.DotProduct(directionToPickpoint) < 0)
                                //{
                                //    horizontalDirection = horizontalDirection.Negate();
                                //}
                                //XYZ offsetDirection = horizontalDirection.Multiply(offsetDistance);
                                //XYZ endPoint = sCon + offsetDirection;
                                //XYZ conduitEndPoint = endPoint + secondaryDirection.Multiply(10); 


                                ///
                                //Conduit thirdConduit = Utility.CreateConduit(doc, firstElement as Conduit, endPoint, conduitEndPoint);
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
                            Element ElementTwo = thirdElements[0];
                            Utility.GetClosestConnectors(ElementOne, ElementTwo, out Connector ConnectorOne, out Connector ConnectorTwo);
                            XYZ axisStart = ConnectorOne.Origin;
                            XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
                            Line axisLine = Line.CreateBound(axisStart, axisEnd);
                            ElementTransformUtils.RotateElements(doc, secondaryElements.Select(r => r.Id).ToList(), axisLine, angle);
                            //Conduit rotate angle indentification
                            Conduit SecondConduit = thirdElements[0] as Conduit;                 //secondaryElements
                            Line SceondConduitLine = (SecondConduit.Location as LocationCurve).Curve as Line;
                            XYZ pt1 = SceondConduitLine.GetEndPoint(0);
                            XYZ pt2 = SceondConduitLine.GetEndPoint(1);
                            XYZ SecondLineDirection = SceondConduitLine.Direction;
                            pt1 -= SecondLineDirection.Multiply(SceondConduitLine.Length + 50);
                            pt2 += SecondLineDirection.Multiply(SceondConduitLine.Length + 50);
                            Line firstline = Line.CreateBound(pt1, pt2);

                            Conduit ThirdConduit = secondaryElements[0] as Conduit;               //thirdElements
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
                            tx.Commit();
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Horizontal Offset", Util.ProductVersion, "Draw");
                        }
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                        ParentUserControl.Instance.masterContainer.Children.Clear();
                        UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                        ParentUserControl.Instance.masterContainer.Children.Add(userControl);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Pick operation aborted.")
                        {
                            ParentUserControl.Instance._window.Close();
                            return false;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Horizontal Offset", Util.ProductVersion, "Draw");
                        }
                    }
                    return true;
                }
        */

        //new

        public static bool HOffsetDrawPointHandler(Document doc, UIApplication uiapp, List<Element> pickedElements, string offsetVariable, bool Refpiuckpoint, XYZ Pickpoint, ref List<Element> secondaryElements, ref bool fittingsfailure)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            fittingsfailure = false;
            try
            {
                List<Element> thirdElements = new List<Element>();
                if (HOffsetUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                HOffsetGP globalParam = new HOffsetGP
                {
                    OffsetValue = HOffsetUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : HOffsetUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = HOffsetUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };
                bool isVerticalConduits = false;
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

                double angle = Convert.ToDouble(HOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                //double offSet = HOffsetUserControl.Instance.txtOffsetFeet.AsDouble;

                using (SubTransaction tx = new SubTransaction(doc))
                {
                    tx.Start();
                    startDate = DateTime.UtcNow;
                    List<Element> backupsele = new List<Element>();
                    foreach (Element element in pickedElements)
                    {
                        backupsele.Add(element);
                    }
                    Properties.Settings.Default.HorizontalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();

                    HorizontalOffset.GetSecondaryElementsWithPoint(doc, ref pickedElements, angle, offsetVariable, out secondaryElements, Pickpoint, Refpiuckpoint);

                    //check the angle direction
                    LocationCurve curve = pickedElements[0].Location as LocationCurve;
                    Line l_Line = curve.Curve as Line;
                    XYZ StartPoint = l_Line.GetEndPoint(0);
                    XYZ EndPoint = l_Line.GetEndPoint(1);
                    XYZ PrimaryConduitDirection = l_Line.Direction;
                    XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                    XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1);

                    XYZ Intersectionpoint = Utility.FindIntersectionPoint(StartPoint, EndPoint, Pickpoint, PickPointTwo);
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
                    Line baseline = Line.CreateBound(ConduitEndpoint, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, ConduitEndpoint.Z));
                    for (int i = 0; i < pickedElements.Count; i++)
                    {
                        Element firstElement = pickedElements[i];
                        Element secondElement = secondaryElements[i];
                        Line firstLine = (firstElement.Location as LocationCurve).Curve as Line;
                        Line secondLine = (secondElement.Location as LocationCurve).Curve as Line;
                        Line newLine = Utility.GetParallelLine(firstElement, secondElement, ref isVerticalConduits);
                        double elevation = firstElement.LookupParameter(offsetVariable).AsDouble();

                        XYZ secondlinept2 = secondLine.GetEndPoint(1) + secondLine.Direction.Multiply(3);
                        Conduit thirdConduit = Utility.CreateConduit(doc, firstElement as Conduit, secondLine.GetEndPoint(1), secondlinept2);
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
                    XYZ axisStart = ConnectorTwo.Origin;
                    XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
                    Line axisLine = Line.CreateBound(axisStart, axisEnd);
                    ElementTransformUtils.RotateElements(doc, thirdElements.Select(r => r.Id).ToList(), axisLine, angle);

                    //Conduit rotate angle indentification
                    Conduit SecondConduit = pickedElements[0] as Conduit;
                    Line SceondConduitLine = (SecondConduit.Location as LocationCurve).Curve as Line;
                    XYZ pt1 = SceondConduitLine.GetEndPoint(0);
                    XYZ pt2 = SceondConduitLine.GetEndPoint(1);
                    XYZ SecondLineDirection = SceondConduitLine.Direction;
                    pt1 -= SecondLineDirection.Multiply(SceondConduitLine.Length + 50);
                    pt2 += SecondLineDirection.Multiply(SceondConduitLine.Length + 50);
                    Line firstline = Line.CreateBound(pt1, pt2);

                    Conduit ThirdConduit = thirdElements[0] as Conduit;
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
                        ElementTransformUtils.RotateElements(doc, thirdElements.Select(r => r.Id).ToList(), axisLine, -angle);
                    }

                    //check the fittings creations
                    LocationCurve Inclind_curve = thirdElements[0].Location as LocationCurve;
                    Line Inclind_l_Line = Inclind_curve.Curve as Line;
                    XYZ Inclind_l_Line_dir = Inclind_l_Line.Direction;
                    XYZ Inclind_l_Line_pt2 = Inclind_l_Line.GetEndPoint(0) + Inclind_l_Line_dir.Multiply(100);
                    Line Inclind_l_Line_sub = Line.CreateBound(Inclind_l_Line.GetEndPoint(0), Inclind_l_Line_pt2);
                    XYZ intersectionforfittingscheck = Utility.GetIntersection(Inclind_l_Line_sub, baseline);

                    UIDocument uidoc = uiapp.ActiveUIDocument;
                    if (intersectionforfittingscheck != null)
                    {
                        double minimumdistance = Math.Sqrt(Math.Pow((ConduitEndpoint.X - intersectionforfittingscheck.X), 2) + Math.Pow((ConduitEndpoint.Y - intersectionforfittingscheck.Y), 2));

                        if (minimumdistance > 2)
                        {
                            DeleteSupports(doc, pickedElements);
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
                            using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                            {
                                sunstransforrunsync.Start();
                                foreach (Element element in pickedElements)
                                {
                                    Conduit conduitone = element as Conduit;
                                    ElementId eid = conduitone.RunId;
                                    if (eid != null)
                                    {
                                        Element conduitrun = doc.GetElement(eid);
                                        Utility.AutoRetainParameters(element, conduitrun, doc, uiapp);
                                    }
                                }
                                sunstransforrunsync.Commit();
                            }
                            // Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(pickedElements) }, new List<ConduitsCollection> { new ConduitsCollection(secondaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(thirdElements) });
                        }
                        else
                        {

                            foreach (Element element in thirdElements)
                            {
                                if (doc.GetElement(element.Id) != null)
                                    doc.Delete(element.Id);
                            }

                            foreach (Element element in secondaryElements)
                            {
                                if (doc.GetElement(element.Id) != null)
                                    doc.Delete(element.Id);
                            }
                            fittingsfailure = true;

                            //
                            HorizontalOffset.HOffsetDrawWithPickPoint(doc, uiapp, pickedElements, offsetVariable, Refpiuckpoint, Pickpoint, ref secondaryElements, ref fittingsfailure);
                          
                             // ParentUserControl.Instance.Secondaryelst.Clear();
                             // ParentUserControl.Instance.Primaryelst.Clear();

                            //
                            //ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                            //ParentUserControl.Instance.masterContainer.Children.Clear();
                            //UserControl userControl1 = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                            //ParentUserControl.Instance.masterContainer.Children.Add(userControl1);

                            foreach (Element element in backupsele)
                            {
                                ParentUserControl.Instance.Primaryelst.Add(element);
                                //TaskDialog.Show("id",element.Id.ToString());
                            }
                        }
                    }
                    else
                    {
                        foreach (Element element in thirdElements)
                        {
                            if (doc.GetElement(element.Id) != null)
                                doc.Delete(element.Id);
                        }

                        foreach (Element element in secondaryElements)
                        {
                            if (doc.GetElement(element.Id) != null)
                                doc.Delete(element.Id);
                        }
                        fittingsfailure = true;

                        //
                        HorizontalOffset.HOffsetDrawWithPickPoint(doc, uiapp, pickedElements, offsetVariable, Refpiuckpoint, Pickpoint, ref secondaryElements, ref fittingsfailure);

                       // ParentUserControl.Instance.Secondaryelst.Clear();
                       // ParentUserControl.Instance.Primaryelst.Clear();
                       
                        //TaskDialog.Show("Warning", "Couldn't add a fitting. Please change the bend angle or enable Add bend in-place");
                        //ParentUserControl.Instance.Secondaryelst.Clear();
                          ///ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);  ///
                        //ParentUserControl.Instance.Primaryelst.Clear();

                        foreach (Element element in backupsele)
                        {
                            ParentUserControl.Instance.Primaryelst.Add(element);
                            //TaskDialog.Show("id", element.Id.ToString());
                        }
                    }

                    tx.Commit();
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Horizontal Offset", Util.ProductVersion, "Draw");
                }

                ///
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Pick operation aborted.")
                {
                    ParentUserControl.Instance._window.Close();
                    return false;
                }
                else
                {
                    //
                    HorizontalOffset.HOffsetDrawWithPickPoint(doc, uiapp, pickedElements, offsetVariable, Refpiuckpoint, Pickpoint, ref secondaryElements, ref fittingsfailure);
                  
                   // ParentUserControl.Instance.Secondaryelst.Clear();
                     // ParentUserControl.Instance.Primaryelst.Clear();

                    //System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Horizontal Offset", Util.ProductVersion, "Draw");
                }
            }
            return true;
        }

        public static bool ROffsetdrawPointhandler(Document doc, UIDocument uidoc, UIApplication uiapp, List<Element> PrimaryElements, string offsetVariable, XYZ Pickpoint, ref List<Element> SecondaryElements)
        {
            DateTime startDate = DateTime.UtcNow;
            try
            {
                startDate = DateTime.UtcNow;
                if (RollingUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                using (SubTransaction substrans2 = new SubTransaction(doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    substrans2.Commit();
                }
                string json = Utility.GetGlobalParametersManager(SettingsUserControl.Instance._uiApp, "MultiDrawSettings");
                // RollOffsetGP globalParam = JsonConvert.DeserializeObject<RollOffsetGP>(json);
                RollOffsetGP globalParam = new RollOffsetGP
                {
                    OffsetValue = RollingUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "3" : RollingUserControl.Instance.txtOffsetFeet.AsString,
                    RollOffsetValue = RollingUserControl.Instance.txtRollFeet.AsDouble == 0 ? "2" : RollingUserControl.Instance.txtRollFeet.AsString,
                    AngleValue = RollingUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };

                //ConduitElevation identification
                XYZ e1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ e1pt2 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                double Z1 = Math.Round(e1pt1.Z, 2);
                double Z2 = Math.Round(e1pt2.Z, 2);

                using (SubTransaction transreset = new SubTransaction(doc))
                {
                    transreset.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    transreset.Commit();
                }

                if (Z1 == Z2)
                {
                    double l_offSet = RollingUserControl.Instance.txtOffsetFeet.AsDouble;
                    double l_rollOffset = RollingUserControl.Instance.txtRollFeet.AsDouble;
                    double l_angle = Convert.ToDouble(RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    using SubTransaction tx = new SubTransaction(doc);
                    tx.Start();
                    Properties.Settings.Default.RollingOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    DeleteSupports(doc, PrimaryElements);
                    PointRollUp(doc, ref PrimaryElements, l_angle, l_offSet, offsetVariable, Pickpoint, uiapp, ref SecondaryElements);

                    //   Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                    tx.Commit();
                }
                else
                {
                    using SubTransaction tx = new SubTransaction(doc);
                    tx.Start();
                    foreach (Element ele in PrimaryElements)
                    {
                        Conduit cond = ele as Conduit;
                        ConnectorSet unusedconnector = Utility.GetUnusedConnectors(ele);
                        if (unusedconnector.Size == 1)
                        {
                            foreach (Connector conec in unusedconnector)
                            {
                                Pickpoint = conec.Origin;
                            }
                        }
                    }
                    Pickpoint ??= Utility.PickPoint(uidoc);
                    if (Pickpoint == null)
                        return false;

                    double l_offSet = RollingUserControl.Instance.txtOffsetFeet.AsDouble;
                    double l_angle = Convert.ToDouble(RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);

                    Properties.Settings.Default.RollingOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    DeleteSupports(doc, PrimaryElements);
                    PointRollUp(doc, ref PrimaryElements, l_angle, l_offSet, offsetVariable, Pickpoint, uiapp, ref SecondaryElements);
                    // Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                    tx.Commit();
                }

                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Pick operation aborted.")
                {
                    ParentUserControl.Instance._window.Close();
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Some error has occured. \n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Rolling Offset", Util.ProductVersion);
                }
            }
            return true;
        }
        public static bool VOffsetDrawHandler(Document _doc, UIDocument _uiDoc, UIApplication uiApp, List<Element> PrimaryElements, string offsetVariable, int RevitVersion, XYZ Pickpoint, ref List<Element> SecondaryElements)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                List<Element> thirdElements = new List<Element>();
                if (VOffsetUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
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

                //ConduitElevation identification
                XYZ e1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ e1pt2 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                double Z1 = Math.Round(e1pt1.Z, 2);
                double Z2 = Math.Round(e1pt2.Z, 2);

                if (Z1 == Z2)
                {
                    double l_angle = Convert.ToDouble(VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    double l_offSet = VOffsetUserControl.Instance.txtOffsetFeet.AsDouble;

                    //finding the direction
                    using SubTransaction subTransaction = new SubTransaction(_doc);
                    subTransaction.Start();
                    Properties.Settings.Default.VerticalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    startDate = DateTime.UtcNow;
                    VerticalOffset.GetSecondaryElements(_doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offsetVariable, Pickpoint);
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

                        Autodesk.Revit.DB.Parameter bendtype = SecondaryElements[j].LookupParameter("TIG-Bend Type");
                        Autodesk.Revit.DB.Parameter bendangle = SecondaryElements[j].LookupParameter("TIG-Bend Angle");
                        Autodesk.Revit.DB.Parameter bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                        Autodesk.Revit.DB.Parameter bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
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

                        //if (j == 0)
                        //{
                        //    PrimryConduitcoloroverride(SecondaryElements[j].Id, _doc);
                        //}
                        //else
                        //{
                        //    Conduitcoloroverride(SecondaryElements[j].Id, _doc);
                        //}
                        //Conduitcoloroverride(SecondaryElements[j].Id, _doc);

                        bOTTOMForAddtags.Add(fittings1);
                        TOPForAddtags.Add(fittings2);
                    }
                    // BOTTOMAddTags(_doc, bOTTOMForAddtags);
                    // BOTTOMAddTags(_doc, TOPForAddtags);
                    // Support.AddSupport(uiApp, _doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                    subTransaction.Commit();

                    using (SubTransaction sunstransforrunsync = new SubTransaction(_doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = _doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, _doc, uiApp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }
                    //Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Completed", "Vertical Offset", Util.ProductVersion);
                }
                else
                {
                    using Transaction trans1 = new Transaction(_doc);
                    trans1.Start("Vertical Offset Draw");
                    foreach (Element ele in PrimaryElements)
                    {
                        Conduit cond = ele as Conduit;
                        ConnectorSet unusedconnector = Utility.GetUnusedConnectors(ele);
                        if (unusedconnector.Size == 1)
                        {
                            foreach (Connector conec in unusedconnector)
                            {
                                Pickpoint = conec.Origin;
                            }
                        }
                    }
                    Pickpoint ??= Utility.PickPoint(_uiDoc);
                    if (Pickpoint == null)
                        return false;

                    double l_angle = Convert.ToDouble(VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    double l_offSet = VOffsetUserControl.Instance.txtOffsetFeet.AsDouble;

                    startDate = DateTime.UtcNow;
                    Properties.Settings.Default.VerticalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    VerticalOffset.GetSecondaryElements(_doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offsetVariable, Pickpoint);

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

                        Autodesk.Revit.DB.Parameter bendtype = SecondaryElements[j].LookupParameter("TIG-Bend Type");
                        Autodesk.Revit.DB.Parameter bendangle = SecondaryElements[j].LookupParameter("TIG-Bend Angle");
                        Autodesk.Revit.DB.Parameter bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                        Autodesk.Revit.DB.Parameter bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                        bendtype.Set("V Offset");
                        bendangle.Set(l_angle);
                        bendtype2.Set("V Offset");
                        bendangle2.Set(l_angle);
                    }

                    XYZ Ae1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ Ae2pt1 = ((PrimaryElements[1].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    Line ImageLine = Line.CreateBound(new XYZ(Ae1pt1.X, Ae1pt1.Y, Ae1pt1.Z), new XYZ(Ae2pt1.X, Ae2pt1.Y, Ae1pt1.Z));

                    XYZ VerticalLineDirection = ImageLine.Direction;
                    XYZ CrossforVerticalLine = VerticalLineDirection.CrossProduct(XYZ.BasisZ);

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
                    double PrimaryOffset = RevitVersion < 2020 ? PrimaryElements[0].LookupParameter("Offset").AsDouble() : PrimaryElements[0].LookupParameter("Middle Elevation").AsDouble();
                    double SecondaryOffset = RevitVersion < 2020 ? SecondaryElements[0].LookupParameter("Offset").AsDouble() : SecondaryElements[0].LookupParameter("Middle Elevation").AsDouble();

                    l_angle = (Math.PI / 2) - l_angle;

                    if (PrimaryOffset > SecondaryOffset)
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
                    trans1.Commit();
                    using (SubTransaction sunstransforrunsync = new SubTransaction(_doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = _doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, _doc, uiApp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }
                }
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception exception)
            {
                if (exception.Message == "Pick operation aborted.")
                {
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Failed", "Vertical Offset", Util.ProductVersion);
                }
            }
            return true;
        }
        public static bool VOffsetDrawPointHandler(Document _doc, UIDocument _uiDoc, UIApplication uiApp, List<Element> PrimaryElements, string offsetVariable, int RevitVersion, XYZ Pickpoint, ref List<Element> SecondaryElements)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                List<Element> thirdElements = new List<Element>();
                if (VOffsetUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                VerticalOffsetGP globalParam = new VerticalOffsetGP
                {
                    OffsetValue = VOffsetUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : VOffsetUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = VOffsetUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };

                //XYZ Pickpoint = null;

                //ConduitElevation identification
                XYZ e1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ e1pt2 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                double Z1 = Math.Round(e1pt1.Z, 2);
                double Z2 = Math.Round(e1pt2.Z, 2);

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

                if (Z1 == Z2)
                {

                    double l_angle = Convert.ToDouble(VOffsetUserControl.Instance.ddlAngle.SelectedItem
                    .ToString()) * (Math.PI / 180);
                    double l_offSet = VOffsetUserControl.Instance.txtOffsetFeet.AsDouble;


                    //finding the direction
                    using SubTransaction subTransaction = new SubTransaction(_doc);
                    subTransaction.Start();
                    using (SubTransaction transreset = new SubTransaction(_doc))
                    {
                        transreset.Start();
                        OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                        foreach (Element element in PrimaryElements)
                        {
                            _doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                        }
                        transreset.Commit();
                    }
                    Properties.Settings.Default.VerticalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    startDate = DateTime.UtcNow;

                    VerticalOffset.GetSecondaryPointElements(_doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offsetVariable, Pickpoint);

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
                        //if (j == 0)
                        //{
                        //    PrimryConduitcoloroverride(SecondaryElements[j].Id, _doc);
                        //}
                        //else
                        //{
                        //    Conduitcoloroverride(SecondaryElements[j].Id, _doc);
                        //}

                        //Conduitcoloroverride(SecondaryElements[j].Id, _doc);
                        bOTTOMForAddtags.Add(fittings1);
                        TOPForAddtags.Add(fittings2);
                    }
                    // BOTTOMAddTags(_doc, bOTTOMForAddtags);
                    // TOPAddTags(_doc, TOPForAddtags);
                    //  Support.AddSupport(uiApp, _doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                    subTransaction.Commit();

                    using (SubTransaction sunstransforrunsync = new SubTransaction(_doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = _doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, _doc, uiApp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }
                    // Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Completed", "Vertical Offset", Util.ProductVersion);
                }

                else
                {
                    using Transaction trans1 = new Transaction(_doc);
                    trans1.Start("Vertical Offset Draw");
                    foreach (Element ele in PrimaryElements)
                    {
                        Conduit cond = ele as Conduit;
                        ConnectorSet unusedconnector = Utility.GetUnusedConnectors(ele);
                        if (unusedconnector.Size == 1)
                        {
                            foreach (Connector conec in unusedconnector)
                            {
                                Pickpoint = conec.Origin;
                            }
                        }

                    }
                    Pickpoint ??= Utility.PickPoint(_uiDoc);
                    if (Pickpoint == null)
                        return false;

                    double l_angle = Convert.ToDouble(VOffsetUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    double l_offSet = VOffsetUserControl.Instance.txtOffsetFeet.AsDouble;

                    startDate = DateTime.UtcNow;
                    Properties.Settings.Default.VerticalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    VerticalOffset.GetSecondaryElements(_doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offsetVariable, Pickpoint);

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

                    XYZ Ae1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    XYZ Ae2pt1 = ((PrimaryElements[1].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                    Line ImageLine = Line.CreateBound(new XYZ(Ae1pt1.X, Ae1pt1.Y, Ae1pt1.Z), new XYZ(Ae2pt1.X, Ae2pt1.Y, Ae1pt1.Z));

                    XYZ VerticalLineDirection = ImageLine.Direction;
                    XYZ CrossforVerticalLine = VerticalLineDirection.CrossProduct(XYZ.BasisZ);

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
                    double PrimaryOffset = RevitVersion < 2020 ? PrimaryElements[0].LookupParameter("Offset").AsDouble() : PrimaryElements[0].LookupParameter("Middle Elevation").AsDouble();
                    double SecondaryOffset = RevitVersion < 2020 ? SecondaryElements[0].LookupParameter("Offset").AsDouble() : SecondaryElements[0].LookupParameter("Middle Elevation").AsDouble();

                    l_angle = (Math.PI / 2) - l_angle;

                    if (PrimaryOffset > SecondaryOffset)
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
                        //if (j == 0)
                        //{
                        //    //PrimryConduitcoloroverride(SecondaryElements[j].Id, _doc);
                        //}
                        //else
                        //{
                        //    Conduitcoloroverride(SecondaryElements[j].Id, _doc);
                        //}
                        // Conduitcoloroverride(SecondaryElements[j].Id, _doc);
                        bOTTOMForAddtags.Add(fittings1);
                        TOPForAddtags.Add(fittings2);
                    }
                    //BOTTOMAddTags(_doc, bOTTOMForAddtags);
                    // TOPAddTags(_doc, TOPForAddtags);
                    // Support.AddSupport(uiApp, _doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                    trans1.Commit();

                    using (SubTransaction sunstransforrunsync = new SubTransaction(_doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = _doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, _doc, uiApp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }
                }

                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception exception)
            {
                if (exception.Message == "Pick operation aborted.")
                {
                    return false;
                }
                else
                {

                    VerticalOffset.VOffsetDrawPointWithPickOffset(_doc, _uiDoc, uiApp, PrimaryElements, offsetVariable, RevitVersion, Pickpoint, ref SecondaryElements);

                    // System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Failed", "Vertical Offset", Util.ProductVersion);
                }
            }
            return true;
        }
        public static bool FourpointsaddleDrawPointHandler(Document _doc, UIDocument _uiDoc, UIApplication uiApp, List<Element> PrimaryElements, string offsetVariable, int RevitVersion, XYZ Pickpoint, ref List<Element> SecondaryElements)
        {
            PrimaryElements = PrimaryElements.Distinct().ToList();
            List<Element> forthelements = new List<Element>();
            List<Element> thirdElements = new List<Element>();
            List<Element> Fifthyelements = new List<Element>();
            thirdElements.Clear();
            forthelements.Clear();
            Fifthyelements.Clear();

            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                if (FourPointSaddleUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(FourPointSaddleUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                FourPointDrawGP globalParam = new FourPointDrawGP
                {
                    OffsetValue = FourPointSaddleUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : FourPointSaddleUserControl.Instance.txtOffsetFeet.AsString,
                    BaseOffsetValue = FourPointSaddleUserControl.Instance.txtBaseOffsetFeet.AsDouble == 0 ? "4.1" : FourPointSaddleUserControl.Instance.txtBaseOffsetFeet.AsString,
                    AngleValue = FourPointSaddleUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : FourPointSaddleUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };

                //ConduitElevation identification
                XYZ e1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ e1pt2 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                double Z1 = Math.Round(e1pt1.Z, 2);
                double Z2 = Math.Round(e1pt2.Z, 2);

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

                if (Z1 == Z2)
                {
                    SecondaryElements.Clear();
                    double l_angle = Convert.ToDouble(FourPointSaddleUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    double l_offSet = FourPointSaddleUserControl.Instance.txtOffsetFeet.AsDouble;
                    double baseoffset = FourPointSaddleUserControl.Instance.txtBaseOffsetFeet.AsDouble;

                    //finding the direction
                    using SubTransaction subTransaction = new SubTransaction(_doc);
                    subTransaction.Start();
                    using (SubTransaction transreset = new SubTransaction(_doc))
                    {
                        transreset.Start();
                        OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                        foreach (Element element in PrimaryElements)
                        {
                            _doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                        }
                        transreset.Commit();
                    }

                    Properties.Settings.Default.VerticalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    startDate = DateTime.UtcNow;
                    ConnectorSet PrimaryConnectors = null;
                    ConnectorSet SecondaryConnectors = null;

                    if (FourPointSaddleUserControl.Instance.chkFourPointSaddle.IsChecked == true)
                    {
                        Threepointsaddle.GetSecondaryPointElements(_doc, ref PrimaryElements, baseoffset, out SecondaryElements, offsetVariable, Pickpoint);

                        for (int j = 0; j < PrimaryElements.Count; j++)
                        {
                            PrimaryConnectors = Utility.GetConnectors(PrimaryElements[j]);
                            SecondaryConnectors = Utility.GetConnectors(SecondaryElements[j]);
                            Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector COne, out Connector CTwo);
                            XYZ newenpt = new XYZ(CTwo.Origin.X, CTwo.Origin.Y, COne.Origin.Z);

                            Line parallelLine = Line.CreateBound(COne.Origin, CTwo.Origin);

                            XYZ st = COne.Origin + parallelLine.Direction.Multiply(2);
                            XYZ ed = new XYZ(CTwo.Origin.X, CTwo.Origin.Y, COne.Origin.Z) - parallelLine.Direction.Multiply(2);

                            //3rd Conduit
                            Conduit thirdConduit = Utility.CreateConduit(_doc, PrimaryElements[j], st, ed);
                            Element e = _doc.GetElement(thirdConduit.Id);
                            thirdElements.Add(e);

                            //Move 3rd conduit 
                            XYZ pickDirection = Pickpoint;
                            Line thirdConduitLine = (thirdConduit.Location as LocationCurve).Curve as Line;
                            XYZ thirdSt = thirdConduitLine.GetEndPoint(0);
                            XYZ thirdEnd = thirdConduitLine.GetEndPoint(1);
                            XYZ thirdConduitDirection = thirdConduitLine.Direction;
                            XYZ CrossProduct = thirdConduitDirection.CrossProduct(XYZ.BasisZ);
                            XYZ pickPointTwo = pickDirection + CrossProduct.Multiply(10);

                            Line line1 = Line.CreateBound(pickDirection, pickPointTwo);
                            //DetailLine curve = _doc.Create.NewDetailCurve(_doc.ActiveView, line1) as DetailLine;
                            XYZ ip = Utility.FindIntersectionPoint(line1, thirdConduitLine);
                            Line verticalLine = Line.CreateBound(ip, new XYZ(Pickpoint.X, Pickpoint.Y, 0));

                            Line newConduitLine = Line.CreateBound(thirdSt + verticalLine.Direction.Multiply(l_offSet),
                            thirdEnd + verticalLine.Direction.Multiply(l_offSet));
                            (thirdConduit.Location as LocationCurve).Curve = newConduitLine;

                            Utility.RetainParameters(PrimaryElements[j], SecondaryElements[j], uiApp);
                            Utility.RetainParameters(PrimaryElements[j], e, uiApp);
                            Parameter bendtype = SecondaryElements[j].LookupParameter("TIG-Bend Type");
                            Parameter bendangle = SecondaryElements[j].LookupParameter("TIG-Bend Angle");
                            Parameter bendtype2 = thirdConduit.LookupParameter("TIG-Bend Type");
                            Parameter bendangle2 = thirdConduit.LookupParameter("TIG-Bend Angle");
                            bendtype.Set("Four Point Saddle");
                            bendangle.Set(l_angle);
                            bendtype2.Set("Four Point Saddle");
                            bendangle2.Set(l_angle);
                        }
                        Threepointsaddle.HorizontalOffsetPrimary(_doc, uiApp, PrimaryElements, thirdElements, l_angle);
                        Threepointsaddle.HorizontalOffsetPrimary(_doc, uiApp, SecondaryElements, thirdElements, l_angle);
                    }
                    else
                    {
                        //CREATE SECONDARY CONDUIT Z AIXS
                        Threepointsaddle.GetSecondaryFourPointElementZAxiz(_doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offsetVariable, Pickpoint);
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

                            bendtype.Set("Four Point Saddle");
                            bendangle.Set(l_angle);
                            bendtype2.Set("Four Point Saddle");
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

                        //second set of conduits creation 
                        for (int j = 0; j < SecondaryElements.Count(); j++)
                        {
                            double elevation = PrimaryElements[j].LookupParameter(offsetVariable).AsDouble();
                            Conduit con = SecondaryElements[j] as Conduit;
                            XYZ condir = ((con.Location as LocationCurve).Curve as Line).Direction;
                            XYZ conpt1 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                            XYZ conpt2 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0) + condir.Multiply(ncl1.Length);
                            Conduit newCon = Utility.CreateConduit(_doc, PrimaryElements[j], new XYZ(conpt1.X, conpt1.Y, elevation + l_offSet), new XYZ(conpt2.X, conpt2.Y, elevation + l_offSet));

                            Element e = _doc.GetElement(newCon.Id);
                            forthelements.Add(e);

                            Parameter bendtype = newCon.LookupParameter("TIG-Bend Type");
                            Parameter bendangle = newCon.LookupParameter("TIG-Bend Angle");
                            bendtype.Set("Four Point Saddle");
                            bendangle.Set(l_angle);
                        }
                        Element Elementthree = thirdElements[0];
                        Utility.GetClosestConnectors(ElementTwo, Elementthree, out Connector ConnectorTwo4, out Connector ConnectorThree);
                        LocationCurve newconcurve4 = forthelements[0].Location as LocationCurve;
                        Line ncl4 = newconcurve4.Curve as Line;
                        XYZ direction4 = ncl4.Direction;
                        XYZ axisStart4 = ConnectorTwo4.Origin;
                        XYZ axisEnd4 = axisStart4.Add(XYZ.BasisZ.CrossProduct(direction4));
                        Line axisLine4 = Line.CreateBound(axisStart4, axisEnd4);

                        ElementTransformUtils.RotateElements(_doc, forthelements.Select(r => r.Id).ToList(), axisLine4, l_angle);
                        //DeleteSupports(_doc, PrimaryElements);
                        List<FamilyInstance> bOTTOMForAddtags = new List<FamilyInstance>();
                        List<FamilyInstance> TOPForAddtags = new List<FamilyInstance>();


                        // create the saddle distance 
                        foreach (Element ele in forthelements)
                        {
                            LocationCurve curve = ele.Location as LocationCurve;
                            XYZ pt1 = ((ele.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                            XYZ pt2 = ((ele.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                            XYZ newdirection = ((ele.Location as LocationCurve).Curve as Line).Direction;
                            XYZ newpt1 = pt1 + newdirection.Multiply(5);
                            XYZ newpt2 = pt2 + newdirection.Multiply(5);
                            curve.Curve = Line.CreateBound(new XYZ(newpt1.X, newpt1.Y, pt1.Z), new XYZ(newpt2.X, newpt2.Y, pt2.Z));
                        }

                        for (int j = 0; j < forthelements.Count(); j++)
                        {
                            XYZ pt1 = ((forthelements[j].Location as LocationCurve).Curve).GetEndPoint(1);
                            XYZ pt2 = ((SecondaryElements[j].Location as LocationCurve).Curve).GetEndPoint(0);
                            XYZ pt3 = ((SecondaryElements[j].Location as LocationCurve).Curve).GetEndPoint(1);
                            LocationCurve curve = (SecondaryElements[j].Location as LocationCurve);
                            curve.Curve = Line.CreateBound(new XYZ(pt1.X, pt1.Y, pt3.Z), pt3);
                        }
                        //check the minimum basedistance 
                        XYZ threept1 = ((thirdElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                        XYZ threept2 = ((thirdElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                        XYZ fourpt1 = ((forthelements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                        XYZ fourpt2 = ((forthelements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                        double distanceone = Math.Sqrt(Math.Pow((threept1.X - threept2.X), 2) + Math.Pow((threept1.Y - threept2.Y), 2));
                        double ditancetwo = Math.Sqrt(Math.Pow((fourpt1.X - fourpt2.X), 2) + Math.Pow((fourpt1.Y - fourpt2.Y), 2));
                        double minimumbasedistance = distanceone + ditancetwo + 2;
                        double autualbasetance = FourPointSaddleUserControl.Instance.txtBaseOffsetFeet.AsDouble;
                        if (minimumbasedistance < autualbasetance)
                        {
                            //set the basedistance 
                            for (int k = 0; k < forthelements.Count; k++)
                            {
                                Line prilineOne = (PrimaryElements[k].Location as LocationCurve).Curve as Line;
                                XYZ pript1 = prilineOne.GetEndPoint(0);
                                XYZ pript2 = prilineOne.GetEndPoint(1);

                                Line seclineOne = (SecondaryElements[k].Location as LocationCurve).Curve as Line;
                                XYZ secpt1 = seclineOne.GetEndPoint(0);

                                double SubdistanceOne = Math.Sqrt(Math.Pow((pript1.X - secpt1.X), 2) + Math.Pow((pript1.Y - secpt1.Y), 2));
                                double SubdistanceTwo = Math.Sqrt(Math.Pow((pript2.X - secpt1.X), 2) + Math.Pow((pript2.Y - secpt1.Y), 2));
                                XYZ ConduitStartpt = null;
                                XYZ conduitEndpoint = null;
                                if (SubdistanceOne < SubdistanceTwo)
                                {
                                    ConduitStartpt = pript1;
                                    conduitEndpoint = pript2;
                                }
                                else
                                {
                                    ConduitStartpt = pript2;
                                    conduitEndpoint = pript1;
                                }
                                double basedistanceOne = Math.Sqrt(Math.Pow((ConduitStartpt.X - secpt1.X), 2) + Math.Pow((ConduitStartpt.Y - secpt1.Y), 2));
                                if (basedistanceOne > baseoffset)
                                {
                                    double distancetomove = basedistanceOne - baseoffset;
                                    XYZ ditrectiontomove = ((SecondaryElements[k].Location as LocationCurve).Curve as Line).Direction;

                                    //move the secondary conduits 
                                    XYZ secondpt1 = ((SecondaryElements[k].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                    XYZ secondpt2 = ((SecondaryElements[k].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    XYZ secobdpt1move = secondpt1 - ditrectiontomove.Multiply(distancetomove);

                                    //move the fourth conduits 
                                    XYZ fourthpt1 = ((forthelements[k].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                    XYZ fourthpt2 = ((forthelements[k].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    XYZ forthpt1move = fourthpt1 - ditrectiontomove.Multiply(distancetomove);
                                    XYZ forthpt2move = fourthpt2 - ditrectiontomove.Multiply(distancetomove);

                                    LocationCurve secondcurve = (SecondaryElements[k].Location as LocationCurve);
                                    LocationCurve forthcurve = (forthelements[k].Location as LocationCurve);

                                    secondcurve.Curve = Line.CreateBound(new XYZ(secobdpt1move.X, secobdpt1move.Y, secondpt1.Z), new XYZ(secondpt2.X, secondpt2.Y, secondpt2.Z));
                                    forthcurve.Curve = Line.CreateBound(new XYZ(forthpt1move.X, forthpt1move.Y, fourthpt1.Z), new XYZ(forthpt2move.X, forthpt2move.Y, fourthpt2.Z));
                                }
                                else
                                {
                                    double distancetomove = baseoffset - basedistanceOne;
                                    XYZ ditrectiontomove = ((SecondaryElements[k].Location as LocationCurve).Curve as Line).Direction;

                                    //move the secondary conduits 
                                    XYZ secondpt1 = ((SecondaryElements[k].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                    XYZ secondpt2 = ((SecondaryElements[k].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    XYZ secobdpt1move = secondpt1 + ditrectiontomove.Multiply(distancetomove);

                                    //move the fourth conduits 
                                    XYZ fourthpt1 = ((forthelements[k].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                    XYZ fourthpt2 = ((forthelements[k].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    XYZ forthpt1move = fourthpt1 + ditrectiontomove.Multiply(distancetomove);
                                    XYZ forthpt2move = fourthpt2 + ditrectiontomove.Multiply(distancetomove);

                                    LocationCurve secondcurve = (SecondaryElements[k].Location as LocationCurve);
                                    LocationCurve forthcurve = (forthelements[k].Location as LocationCurve);

                                    secondcurve.Curve = Line.CreateBound(new XYZ(secobdpt1move.X, secobdpt1move.Y, secondpt1.Z), new XYZ(secondpt2.X, secondpt2.Y, secondpt2.Z));
                                    forthcurve.Curve = Line.CreateBound(new XYZ(forthpt1move.X, forthpt1move.Y, fourthpt1.Z), new XYZ(forthpt2move.X, forthpt2move.Y, fourthpt2.Z));
                                }
                            }

                            //fith set conduit creation
                            for (int k = 0; k < forthelements.Count; k++)
                            {
                                XYZ startpt1 = ((thirdElements[k].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                XYZ startpt2 = ((forthelements[k].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                Conduit newCon = Utility.CreateConduit(_doc, PrimaryElements[k], startpt1, startpt2);
                                Element e = _doc.GetElement(newCon.Id);
                                Fifthyelements.Add(e);

                                Parameter bendtype = newCon.LookupParameter("TIG-Bend Type");
                                Parameter bendangle = newCon.LookupParameter("TIG-Bend Angle");
                                bendtype.Set("Four Point Saddle");
                                bendangle.Set(l_angle);
                            }

                            for (int j = 0; j < PrimaryElements.Count; j++)
                            {
                                Element firstElement = PrimaryElements[j];
                                Element secondElement = SecondaryElements[j];
                                Element thirdElement = thirdElements[j];
                                Element fourthElement = forthelements[j];
                                Element fifthElement = Fifthyelements[j];
                                ConnectorSet thirdConnectors = Utility.GetConnectors(thirdElement);
                                ConnectorSet SecondConnectors = Utility.GetConnectors(secondElement);
                                ConnectorSet firstConnectors = Utility.GetConnectors(firstElement);
                                ConnectorSet fourthConnectors = Utility.GetConnectors(fourthElement);
                                ConnectorSet fifthConnectors = Utility.GetConnectors(fifthElement);
                                FamilyInstance fittings1 = Utility.CreateElbowFittings(thirdConnectors, fifthConnectors, _doc, uiApp, PrimaryElements[j], true);
                                FamilyInstance fittings2 = Utility.CreateElbowFittings(thirdConnectors, firstConnectors, _doc, uiApp, PrimaryElements[j], true);
                                FamilyInstance fittings3 = Utility.CreateElbowFittings(SecondConnectors, fourthConnectors, _doc, uiApp, PrimaryElements[j], true);
                                FamilyInstance fittings4 = Utility.CreateElbowFittings(fifthConnectors, fourthConnectors, _doc, uiApp, PrimaryElements[j], true);
                                Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                Parameter bendtype3 = fittings3.LookupParameter("TIG-Bend Type");
                                Parameter bendangle3 = fittings3.LookupParameter("TIG-Bend Angle");
                                Parameter bendtype4 = fittings4.LookupParameter("TIG-Bend Type");
                                Parameter bendangle4 = fittings4.LookupParameter("TIG-Bend Angle");
                                bendtype.Set("Four Point Saddle");
                                bendangle.Set(l_angle);
                                bendtype2.Set("Four Point Saddle");
                                bendangle2.Set(l_angle);
                                bendtype3.Set("Four Point Saddle");
                                bendangle3.Set(l_angle);
                                bendtype4.Set("Four Point Saddle");
                                bendangle4.Set(l_angle);
                                bOTTOMForAddtags.Add(fittings1);
                                TOPForAddtags.Add(fittings2);
                            }

                            thirdElements.Clear();
                            forthelements.Clear();
                            Fifthyelements.Clear();
                        }
                        else
                        {
                            for (int i = 0; i < SecondaryElements.Count(); i++)
                            {
                                if (SecondaryElements.Count() != 0) _doc.Delete(SecondaryElements[i].Id);
                                if (thirdElements.Count() != 0) _doc.Delete(thirdElements[i].Id);
                                if (forthelements.Count() != 0) _doc.Delete(forthelements[i].Id);
                                if (Fifthyelements.Count() != 0) _doc.Delete(Fifthyelements[i].Id);
                            }
                            ParentUserControl.Instance.Primaryelst.Clear();
                            SecondaryElements.Clear();
                            ParentUserControl.Instance.Primaryelst.AddRange(PrimaryElements);
                            TaskDialog.Show("Warning", "Kindly set the minimum Base Offset is " + minimumbasedistance.ToString());
                        }
                    }
                    subTransaction.Commit();
                    using (SubTransaction sunstransforrunsync = new SubTransaction(_doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = _doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, _doc, uiApp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }
                    // Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Completed", "Vertical Offset", Util.ProductVersion);
                }
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception exception)
            {
                if (exception.Message == "Pick operation aborted.")
                {
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Failed", "Vertical Offset", Util.ProductVersion);
                }
            }
            return true;
        }
        public static bool ThreepointsaddleDrawPointHandler(Document _doc, UIDocument _uiDoc, UIApplication uiApp, List<Element> PrimaryElements, string offsetVariable, int RevitVersion, XYZ Pickpoint, ref List<Element> SecondaryElements)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                List<Element> thirdElements = new List<Element>();
                List<Element> forthelements = new List<Element>();
                if (ThreePointSaddleUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(ThreePointSaddleUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                ThreepointDrawGP globalParam = new ThreepointDrawGP
                {
                    OffsetValue = ThreePointSaddleUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : ThreePointSaddleUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = ThreePointSaddleUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : ThreePointSaddleUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };

                XYZ e1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ e1pt2 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                double Z1 = Math.Round(e1pt1.Z, 2);
                double Z2 = Math.Round(e1pt2.Z, 2);

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
                if (Z1 == Z2)
                {
                    double l_angle = Convert.ToDouble(ThreePointSaddleUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    double l_offSet = ThreePointSaddleUserControl.Instance.txtOffsetFeet.AsDouble;

                    //finding the direction
                    using SubTransaction subTransaction = new SubTransaction(_doc);
                    subTransaction.Start();
                    using (SubTransaction transreset = new SubTransaction(_doc))
                    {
                        transreset.Start();
                        OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                        foreach (Element element in PrimaryElements)
                        {
                            _doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                        }
                        transreset.Commit();
                    }
                    Properties.Settings.Default.VerticalOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    startDate = DateTime.UtcNow;

                    Threepointsaddle.GetSecondaryPointElements(_doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offsetVariable, Pickpoint);

                    ConnectorSet PrimaryConnectors = null;
                    ConnectorSet SecondaryConnectors = null;

                    //CREATE XY DIRECTION IN ACTIVE VIEW
                    if (ThreePointSaddleUserControl.Instance.ChkThreePointSaddle.IsChecked == true)
                    {
                        for (int j = 0; j < PrimaryElements.Count; j++)
                        {
                            PrimaryConnectors = Utility.GetConnectors(PrimaryElements[j]);
                            SecondaryConnectors = Utility.GetConnectors(SecondaryElements[j]);
                            Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector COne, out Connector CTwo);
                            XYZ newenpt = new XYZ(CTwo.Origin.X, CTwo.Origin.Y, COne.Origin.Z);

                            Line parallelLine = Line.CreateBound(COne.Origin, CTwo.Origin);

                            XYZ st = COne.Origin + parallelLine.Direction.Multiply(2);
                            XYZ ed = new XYZ(CTwo.Origin.X, CTwo.Origin.Y, COne.Origin.Z) - parallelLine.Direction.Multiply(2);

                            //3rd Conduit
                            Conduit thirdConduit = Utility.CreateConduit(_doc, PrimaryElements[j], st, ed);
                            Element e = _doc.GetElement(thirdConduit.Id);
                            thirdElements.Add(e);

                            //Move 3rd conduit 
                            XYZ pickDirection = Pickpoint;
                            Line thirdConduitLine = (thirdConduit.Location as LocationCurve).Curve as Line;
                            XYZ thirdSt = thirdConduitLine.GetEndPoint(0);
                            XYZ thirdEnd = thirdConduitLine.GetEndPoint(1);
                            XYZ thirdConduitDirection = thirdConduitLine.Direction;
                            XYZ CrossProduct = thirdConduitDirection.CrossProduct(XYZ.BasisZ);
                            XYZ pickPointTwo = pickDirection + CrossProduct.Multiply(10);

                            Line line1 = Line.CreateBound(pickDirection, pickPointTwo);
                            //DetailLine curve = _doc.Create.NewDetailCurve(_doc.ActiveView, line1) as DetailLine;
                            XYZ ip = Utility.FindIntersectionPoint(line1, thirdConduitLine);
                            Line verticalLine = Line.CreateBound(ip, new XYZ(Pickpoint.X, Pickpoint.Y, 0));

                            Line newConduitLine = Line.CreateBound(thirdSt + verticalLine.Direction.Multiply(l_offSet),
                            thirdEnd + verticalLine.Direction.Multiply(l_offSet));
                            (thirdConduit.Location as LocationCurve).Curve = newConduitLine;

                            Utility.RetainParameters(PrimaryElements[j], SecondaryElements[j], uiApp);
                            Utility.RetainParameters(PrimaryElements[j], e, uiApp);
                            Parameter bendtype = SecondaryElements[j].LookupParameter("TIG-Bend Type");
                            Parameter bendangle = SecondaryElements[j].LookupParameter("TIG-Bend Angle");
                            Parameter bendtype2 = thirdConduit.LookupParameter("TIG-Bend Type");
                            Parameter bendangle2 = thirdConduit.LookupParameter("TIG-Bend Angle");
                            bendtype.Set("Four Point Saddle");
                            bendangle.Set(l_angle);
                            bendtype2.Set("Four Point Saddle");
                            bendangle2.Set(l_angle);
                        }

                        List<Element> fourthElements = new List<Element>();
                        List<Element> fivthElements = new List<Element>();

                        Threepointsaddle.ThreePointSaddleConnect(_doc, uiApp, PrimaryElements, thirdElements, l_angle, out fourthElements); //4th
                        Threepointsaddle.ThreePointSaddleConnect(_doc, uiApp, SecondaryElements, thirdElements, l_angle, out fivthElements); //5th

                        _doc.Delete(thirdElements.Select(x => x.Id).ToList());

                        for (int i = 0; i < PrimaryElements.Count; i++)
                        {
                            Element firstElement = PrimaryElements[i];
                            Element secondElement = SecondaryElements[i];
                            Element fourthElement = fourthElements[i];
                            Element fifthElement = fivthElements[i];

                            Utility.CreateElbowFittings(firstElement, fourthElement, _doc, uiApp);
                            Utility.CreateElbowFittings(secondElement, fifthElement, _doc, uiApp);
                            Utility.CreateElbowFittings(fourthElement, fifthElement, _doc, uiApp);
                        }
                    }
                    else
                    {
                        //CREATE Z AIXS DIRECTION TO VIEW 3D 
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
                            bendtype.Set("Three Point Saddle");
                            bendangle.Set(l_angle);
                            bendtype2.Set("Three Point Saddle");
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

                        //second set of conduits creation 
                        for (int j = 0; j < SecondaryElements.Count(); j++)
                        {
                            double elevation = PrimaryElements[j].LookupParameter(offsetVariable).AsDouble();
                            Conduit con = SecondaryElements[j] as Conduit;
                            XYZ condir = ((con.Location as LocationCurve).Curve as Line).Direction;
                            XYZ conpt1 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                            XYZ conpt2 = ((con.Location as LocationCurve).Curve as Line).GetEndPoint(0) + condir.Multiply(ncl1.Length);
                            Conduit newCon = Utility.CreateConduit(_doc, PrimaryElements[j], new XYZ(conpt1.X, conpt1.Y, elevation + l_offSet),
                                                                                             new XYZ(conpt2.X, conpt2.Y, elevation + l_offSet));
                            Element e = _doc.GetElement(newCon.Id);
                            forthelements.Add(e);
                            Parameter bendtype = newCon.LookupParameter("TIG-Bend Type");
                            Parameter bendangle = newCon.LookupParameter("TIG-Bend Angle");
                            bendtype.Set("Three Point Saddle");
                            bendangle.Set(l_angle);
                        }
                        Element Elementthree = thirdElements[0];
                        Utility.GetClosestConnectors(ElementTwo, Elementthree, out Connector ConnectorTwo4, out Connector ConnectorThree);
                        LocationCurve newconcurve4 = forthelements[0].Location as LocationCurve;
                        Line ncl4 = newconcurve4.Curve as Line;
                        XYZ direction4 = ncl4.Direction;
                        XYZ axisStart4 = ConnectorTwo4.Origin;
                        XYZ axisEnd4 = axisStart4.Add(XYZ.BasisZ.CrossProduct(direction4));
                        Line axisLine4 = Line.CreateBound(axisStart4, axisEnd4);
                        ElementTransformUtils.RotateElements(_doc, forthelements.Select(r => r.Id).ToList(), axisLine4, l_angle);
                        DeleteSupports(_doc, PrimaryElements);
                        for (int j = 0; j < forthelements.Count(); j++)
                        {
                            XYZ pt1 = ((forthelements[j].Location as LocationCurve).Curve).GetEndPoint(1);
                            XYZ pt2 = ((SecondaryElements[j].Location as LocationCurve).Curve).GetEndPoint(0);
                            XYZ pt3 = ((SecondaryElements[j].Location as LocationCurve).Curve).GetEndPoint(1);
                            LocationCurve curve = (SecondaryElements[j].Location as LocationCurve);
                            curve.Curve = Line.CreateBound(new XYZ(pt1.X, pt1.Y, pt3.Z), pt3);
                        }
                        List<FamilyInstance> bOTTOMForAddtags = new List<FamilyInstance>();
                        List<FamilyInstance> TOPForAddtags = new List<FamilyInstance>();

                        for (int j = 0; j < PrimaryElements.Count; j++)
                        {
                            Element firstElement = PrimaryElements[j];
                            Element secondElement = SecondaryElements[j];
                            Element thirdElement = thirdElements[j];
                            Element fourthElement = forthelements[j];
                            ConnectorSet thirdConnectors = Utility.GetConnectors(thirdElement);
                            ConnectorSet SecondConnectors = Utility.GetConnectors(secondElement);
                            ConnectorSet firstConnectors = Utility.GetConnectors(firstElement);
                            ConnectorSet fourthConnectors = Utility.GetConnectors(fourthElement);

                            FamilyInstance fittings1 = Utility.CreateElbowFittings(firstConnectors, thirdConnectors, _doc, uiApp, PrimaryElements[j], true);
                            FamilyInstance fittings2 = Utility.CreateElbowFittings(SecondConnectors, fourthConnectors, _doc, uiApp, PrimaryElements[j], true);
                            FamilyInstance fittings3 = Utility.CreateElbowFittings(thirdConnectors, fourthConnectors, _doc, uiApp, PrimaryElements[j], true);

                            Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                            Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                            Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                            Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                            Parameter bendtype3 = fittings3.LookupParameter("TIG-Bend Type");
                            Parameter bendangle3 = fittings3.LookupParameter("TIG-Bend Angle");
                            bendtype.Set("Three Point Saddle");
                            bendangle.Set(l_angle);
                            bendtype2.Set("Three Point Saddle");
                            bendangle2.Set(l_angle);
                            bendtype3.Set("Three Point Saddle");
                            bendangle3.Set(l_angle);
                        }
                    }

                    subTransaction.Commit();
                    using (SubTransaction sunstransforrunsync = new SubTransaction(_doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = _doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, _doc, uiApp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }
                }
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception exception)
            {
                if (exception.Message == "Pick operation aborted.")
                {
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Failed", "Vertical Offset", Util.ProductVersion);
                }
            }
            return true;
        }

        //public static bool ConduitSync(Document doc, UIApplication uiapp, List<Element> elements)
        //{
        //    DateTime startDate = DateTime.UtcNow;
        //    try
        //    {
        //        if (elements == null)
        //        {
        //            SyncDataUserControl.Instance.btnsync.IsEnabled = true;
        //            return false;
        //        }
        //        List<MultiSelect> SelectedParameters = SyncDataUserControl.Instance.ucMultiSelect.SelectedItems;
        //        MultiSelect multiselect = SyncDataUserControl.Instance.ucMultiSelect.SelectedItems[0];
        //        if (SelectedParameters != null)
        //            _selectedSyncDataList = SyncDataUserControl.Instance.ucMultiSelect.SelectedItems.ToList().Where(x => x.Name != "All" && x.IsChecked).ToList();
        //        if (SyncDataUserControl.Instance.ucMultiSelect.ItemsSource is List<MultiSelect> selectitem)
        //        {
        //            List<SYNCDataGlobalParam> syncdata = new List<SYNCDataGlobalParam>();
        //            syncdata = selectitem.Where(r => r.IsChecked).Select(x => new SYNCDataGlobalParam { Name = x.Name }).ToList();
        //            string json = JsonConvert.SerializeObject(syncdata);
        //            GlobalParameter gp = null;
        //            string ParamName = "SyncDataParameters_" + uiapp.Application.LoginUserId;
        //            ElementId gpNWC = GlobalParametersManager.FindByName(doc, ParamName);
        //            if (gpNWC != ElementId.InvalidElementId)
        //            {
        //                gp = doc.GetElement(gpNWC) as GlobalParameter;
        //            }
        //            else
        //            {
        //                gp = GlobalParameter.Create(doc, ParamName, ParameterType.Text);
        //            }
        //            if (gp != null)
        //            {
        //                ParameterValue value = gp.GetValue();
        //                if (value.GetType() == typeof(StringParameterValue))
        //                {
        //                    StringParameterValue stpv = value as StringParameterValue;
        //                    stpv.Value = json;
        //                    gp.SetValue(stpv);
        //                }
        //            }
        //        }
        //        List<ElementId> elementIds = new List<ElementId>();
        //        foreach (Element item in elements)
        //        {
        //            List<Element> lstElements = new List<Element>();
        //            Utility.ConduitSelection(doc, item as Conduit, null, ref lstElements, true);
        //            ApplyParameters(doc, item as Conduit, lstElements);
        //        }
        //        Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Sync Data", Util.ProductVersion, "Sync Data");
        //    }
        //    catch (Exception exception)
        //    {
        //        System.Windows.MessageBox.Show("Warning. \n" + exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Sync Data", Util.ProductVersion, "Sync Data");

        //    }
        //    SyncDataUserControl.Instance.btnsync.IsEnabled = true;
        //    return true;
        //}

        /* private static void ApplyParameters(Document doc, Conduit eleConduit, List<Element> elements)
         {

             foreach (MultiSelect param in _selectedSyncDataList)
             {
                 if (param.IsChecked)
                 {
                     Parameter ConduitParam = eleConduit.LookupParameter(param.Name);
                     string paramValue = Utility.GetParameterValue(ConduitParam);
                     if (eleConduit.RunId != ElementId.InvalidElementId)
                     {
                         Element eleRun = doc.GetElement(eleConduit.RunId);
                         if (eleRun != null)
                         {
                             Parameter RunParam = eleRun.LookupParameter(param.Name);
                             if (RunParam != null && !RunParam.IsReadOnly)
                                 Utility.SetParameterValue(RunParam, ConduitParam);
                         }
                     }
                     foreach (Element e in elements.Distinct())
                     {
                         if (e.GetType() == typeof(Conduit) || (e.GetType() == typeof(FamilyInstance)))
                         {
                             Parameter lookUpParam = e.LookupParameter(param.Name);
                             if (lookUpParam != null && ConduitParam != null)
                                 Utility.SetParameterValue(lookUpParam, ConduitParam);
                         }
                     }
                 }
             }
         }*/

        public static bool RollingOffsetDrawHandler(Document doc, UIDocument uidoc, UIApplication uiapp, List<Element> PrimaryElements, string offsetVariable, XYZ Pickpoint, ref List<Element> SecondaryElements)
        {
            DateTime startDate = DateTime.UtcNow;
            try
            {
                startDate = DateTime.UtcNow;
                if (RollingUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                using (SubTransaction substrans2 = new SubTransaction(doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    substrans2.Commit();
                }
                RollOffsetGP globalParam = new RollOffsetGP
                {
                    OffsetValue = RollingUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "3" : RollingUserControl.Instance.txtOffsetFeet.AsString,
                    RollOffsetValue = RollingUserControl.Instance.txtRollFeet.AsDouble == 0 ? "2" : RollingUserControl.Instance.txtRollFeet.AsString,
                    AngleValue = RollingUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };

                //ConduitElevation identification
                XYZ e1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ e1pt2 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                double Z1 = Math.Round(e1pt1.Z, 2);
                double Z2 = Math.Round(e1pt2.Z, 2);

                if (Z1 == Z2)
                {
                    double l_offSet = RollingUserControl.Instance.txtOffsetFeet.AsDouble;
                    double l_rollOffset = RollingUserControl.Instance.txtRollFeet.AsDouble;
                    double l_angle = Convert.ToDouble(RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    using SubTransaction tx = new SubTransaction(doc);
                    tx.Start();
                    Properties.Settings.Default.RollingOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    RollUp(doc, ref PrimaryElements, l_angle, l_rollOffset, l_offSet, offsetVariable, Pickpoint, uiapp, ref SecondaryElements);
                    // Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                    tx.Commit();
                }
                else
                {
                    using SubTransaction tx = new SubTransaction(doc);
                    tx.Start();
                    foreach (Element ele in PrimaryElements)
                    {
                        Conduit cond = ele as Conduit;
                        ConnectorSet unusedconnector = Utility.GetUnusedConnectors(ele);
                        if (unusedconnector.Size == 1)
                        {
                            foreach (Connector conec in unusedconnector)
                            {
                                Pickpoint = conec.Origin;
                            }
                        }
                    }
                    Pickpoint ??= Utility.PickPoint(uidoc);
                    if (Pickpoint == null)
                        return false;

                    double l_offSet = RollingUserControl.Instance.txtOffsetFeet.AsDouble;
                    double l_rollOffset = RollingUserControl.Instance.txtRollFeet.AsDouble;
                    double l_angle = Convert.ToDouble(RollingUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);

                    Properties.Settings.Default.RollingOffsetDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    DeleteSupports(doc, PrimaryElements);
                    RollUp(doc, ref PrimaryElements, l_angle, l_rollOffset, l_offSet, offsetVariable, Pickpoint, uiapp, ref SecondaryElements);
                    // Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                    tx.Commit();
                }

                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Pick operation aborted.")
                {
                    ParentUserControl.Instance._window.Close();
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Some error has occured. \n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Rolling Offset", Util.ProductVersion);
                }
            }
            return true;
        }
        public static void RollUp(Document doc, ref List<Element> PrimaryElements, double l_angle, double l_rollOffset, double l_offSet, string offSetVar, XYZ PickPoint, UIApplication _uiapp, ref List<Element> secondaryElements)
        {
            List<FamilyInstance> bOTTOMForAddtags = new List<FamilyInstance>();
            List<FamilyInstance> TOPForAddtags = new List<FamilyInstance>();

            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            secondaryElements = new List<Element>();
            RollingOffset.GetSecondaryElements(doc, ref PrimaryElements, l_angle, l_offSet, l_rollOffset, out List<Element> SecondaryElements, offSetVar, PickPoint);
            for (int i = 0; i < PrimaryElements.Count; i++)
            {
                ConnectorSet PrimaryConnectors = Utility.GetConnectors(PrimaryElements[i]);
                ConnectorSet SecondaryConnectors = Utility.GetConnectors(SecondaryElements[i]);
                Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, ConnectorOne.Origin, ConnectorTwo.Origin);
                Element e = doc.GetElement(newCon.Id);
                Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                if (profileSetting != null)
                {
                    C_bendtype.Set(profileSetting.rOffsetValue);
                }
                C_bendangle.Set(l_angle);
                if (profileSetting != null)
                {
                    C_bendtype2.Set(profileSetting.rOffsetValue);
                }
                C_bendangle2.Set(l_angle);

                ConnectorSet thirdConnectors = Utility.GetConnectors(e);
                FamilyInstance fittings1 = Utility.CreateElbowFittings(thirdConnectors, PrimaryConnectors, doc, _uiapp, PrimaryElements[i], true);
                FamilyInstance fittings2 = Utility.CreateElbowFittings(thirdConnectors, SecondaryConnectors, doc, _uiapp, PrimaryElements[i], true);
                Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                bendtype.Set("R Offset");
                bendangle.Set(l_angle);
                bendtype2.Set("R Offset");
                bendangle2.Set(l_angle);
                //if (i == 0)
                //{
                //    PrimryConduitcoloroverride(SecondaryElements[i].Id, doc);
                //}
                //else
                //{
                //    Conduitcoloroverride(SecondaryElements[i].Id, doc);
                //}
                //Conduitcoloroverride(SecondaryElements[i].Id, doc);

                bOTTOMForAddtags.Add(fittings1);
                TOPForAddtags.Add(fittings2);
            }
            //  BOTTOMAddTags(doc, bOTTOMForAddtags);
            // TOPAddTags(doc, TOPForAddtags);
            using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
            {
                sunstransforrunsync.Start();
                foreach (Element element in PrimaryElements)
                {
                    Conduit conduitone = element as Conduit;
                    ElementId eid = conduitone.RunId;
                    if (eid != null)
                    {
                        Element conduitrun = doc.GetElement(eid);
                        Utility.AutoRetainParameters(element, conduitrun, doc, _uiapp);
                    }
                }
                sunstransforrunsync.Commit();
            }
            secondaryElements.AddRange(SecondaryElements);
        }
        public static void PointRollUp(Document doc, ref List<Element> PrimaryElements, double l_angle, double l_offSet, string offSetVar, XYZ PickPoint, UIApplication _uiapp, ref List<Element> secondaryElements)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            secondaryElements = new List<Element>();
            RollingOffset.GetSecondarypointElements(doc, ref PrimaryElements, l_angle, l_offSet, out List<Element> SecondaryElements, offSetVar, PickPoint);
            for (int i = 0; i < PrimaryElements.Count; i++)
            {
                ConnectorSet PrimaryConnectors = Utility.GetConnectors(PrimaryElements[i]);
                ConnectorSet SecondaryConnectors = Utility.GetConnectors(SecondaryElements[i]);
                Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, ConnectorOne.Origin, ConnectorTwo.Origin);
                Element e = doc.GetElement(newCon.Id);
                Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                Utility.RetainParameters(PrimaryElements[i], e, _uiapp);
                Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                if (profileSetting != null)
                {
                    C_bendtype.Set(profileSetting.rOffsetValue);
                }
                C_bendangle.Set(l_angle);
                if (profileSetting != null)
                {
                    C_bendtype2.Set(profileSetting.rOffsetValue);
                }
                C_bendangle2.Set(l_angle);
                ConnectorSet thirdConnectors = Utility.GetConnectors(e);
                FamilyInstance fittings1 = Utility.CreateElbowFittings(thirdConnectors, PrimaryConnectors, doc, _uiapp, PrimaryElements[i], true);
                FamilyInstance fittings2 = Utility.CreateElbowFittings(thirdConnectors, SecondaryConnectors, doc, _uiapp, PrimaryElements[i], true);
                Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                bendtype.Set("R Offset");
                bendangle.Set(l_angle);
                bendtype2.Set("R Offset");
                bendangle2.Set(l_angle);

                //Conduitcoloroverride(secondaryElements[i].Id, doc);
            }
            using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
            {
                sunstransforrunsync.Start();
                foreach (Element element in PrimaryElements)
                {
                    Conduit conduitone = element as Conduit;
                    ElementId eid = conduitone.RunId;
                    if (eid != null)
                    {
                        Element conduitrun = doc.GetElement(eid);
                        Utility.AutoRetainParameters(element, conduitrun, doc, _uiapp);
                    }
                }
                sunstransforrunsync.Commit();
            }
            if (SecondaryElements.Count() > 0)
            {
                secondaryElements.AddRange(SecondaryElements);
            }
        }
        public static bool KWBDrawHandler(Document doc, UIApplication uiapp, List<Element> PrimaryElements, string offsetVariable, XYZ Pickpoint, ref List<Element> secondaryElements)
        {
            DateTime startDate = DateTime.UtcNow;
            try
            {
                startDate = DateTime.UtcNow;
                if (KickUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                using (SubTransaction substrans2 = new SubTransaction(doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }

                    substrans2.Commit();
                }
                Kick90DrawGP globalParam = new Kick90DrawGP
                {
                    OffsetValue = KickUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : KickUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = KickUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : KickUserControl.Instance.ddlAngle.SelectedItem.ToString(),
                    SelectionMode = KickUserControl.Instance.rbNinetyNear.IsChecked == true ? "90° Near" : "90° Far"
                };

                double l_offSet = KickUserControl.Instance.txtOffsetFeet.AsDouble;
                double l_angle = Convert.ToDouble(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                using (SubTransaction subtrans = new SubTransaction(doc))
                {
                    subtrans.Start();
                    Properties.Settings.Default.Kick90Draw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    DeleteSupports(doc, PrimaryElements);
                    ApplyBend(doc, ref PrimaryElements, l_angle, l_offSet, offsetVariable, Pickpoint, uiapp, ref secondaryElements);
                    //Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(secondaryElements) });
                    subtrans.Commit();
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Complted", "Kick with Bend", Util.ProductVersion, "Draw");
                }
                using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                {
                    sunstransforrunsync.Start();
                    foreach (Element element in PrimaryElements)
                    {
                        Conduit conduitone = element as Conduit;
                        ElementId eid = conduitone.RunId;
                        if (eid != null)
                        {
                            Element conduitrun = doc.GetElement(eid);
                            Utility.AutoRetainParameters(element, conduitrun, doc, uiapp);
                        }
                    }
                    sunstransforrunsync.Commit();
                }

                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);

            }
            catch (Exception ex)
            {
                if (ex.Message == "Pick operation aborted.")
                {
                    ParentUserControl.Instance._window.Close();
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");
                }
            }

            return true;
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

        public static bool KWBDrawPointHandler(Document doc, UIApplication uiapp, List<Element> PrimaryElements, string offsetVariable, XYZ Pickpoint, ref List<Element> secondaryElements)
        {
            DateTime startDate = DateTime.UtcNow;
            try
            {
                startDate = DateTime.UtcNow;
                if (KickUserControl.Instance.ddlAngle.SelectedItem == null || string.IsNullOrEmpty(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()))
                {
                    return false;
                }
                using (SubTransaction substrans2 = new SubTransaction(doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    substrans2.Commit();
                }

                Kick90DrawGP globalParam = new Kick90DrawGP
                {
                    OffsetValue = KickUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "1.5" : KickUserControl.Instance.txtOffsetFeet.AsString,
                    AngleValue = KickUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : KickUserControl.Instance.ddlAngle.SelectedItem.ToString(),
                    SelectionMode = KickUserControl.Instance.rbNinetyNear.IsChecked == true ? "90° Near" : "90° Far"
                };
                using (SubTransaction transreset = new SubTransaction(doc))
                {
                    transreset.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    transreset.Commit();
                }
                double l_offSet = KickUserControl.Instance.txtOffsetFeet.AsDouble;
                double l_angle = Convert.ToDouble(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                using (SubTransaction subtrans = new SubTransaction(doc))
                {
                    subtrans.Start();
                    Properties.Settings.Default.Kick90Draw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    DeleteSupports(doc, PrimaryElements);
                    PointApplyBend(doc, ref PrimaryElements, l_angle, l_offSet, offsetVariable, Pickpoint, uiapp, ref secondaryElements);
                    ///Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(secondaryElements) });
                    subtrans.Commit();
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Complted", "Kick with Bend", Util.ProductVersion, "Draw");
                }

                using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                {
                    sunstransforrunsync.Start();
                    foreach (Element element in PrimaryElements)
                    {
                        Conduit conduitone = element as Conduit;
                        ElementId eid = conduitone.RunId;
                        if (eid != null)
                        {
                            Element conduitrun = doc.GetElement(eid);
                            Utility.AutoRetainParameters(element, conduitrun, doc, uiapp);
                        }
                    }
                    sunstransforrunsync.Commit();
                }
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Pick operation aborted.")
                {
                    ParentUserControl.Instance._window.Close();
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");
                }
            }
            return true;
        }

        public static void ApplyBend(Document doc, ref List<Element> PrimaryElements, double l_angle, double l_offSet, string offSetVar, XYZ pickpoint, UIApplication _uiapp, ref List<Element> SecondaryElements)
        {
            FamilyInstance fittings1 = null;
            FamilyInstance fittings2 = null;
            //List<FamilyInstance> bottomTag = null;
            //List<FamilyInstance> toptag = null;
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                KWBOffset.GetSecondaryElements(doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offSetVar, pickpoint);
                double NinetyAngle = 90.00 * (Math.PI / 180);
                bool isUp = PrimaryElements.FirstOrDefault().LookupParameter(offSetVar).AsDouble() <
                    SecondaryElements.FirstOrDefault().LookupParameter(offSetVar).AsDouble();
                if (!isUp)
                {
                    l_angle = Convert.ToDouble(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    try
                    {
                        ConnectorSet PrimaryConnectors = null;
                        ConnectorSet SecondaryConnectors = null;
                        ConnectorSet ThirdConnectors = null;
                        if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                        {
                            for (int i = 0; i < PrimaryElements.Count; i++)
                            {
                                double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                Line l1 = lc1.Curve as Line;
                                LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                Line l2 = lc2.Curve as Line;
                                XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                XYZ EndPoint = ConnectorTwo.Origin;
                                XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                newCon.LookupParameter(offSetVar).Set(elevation);
                                Element e = doc.GetElement(newCon.Id);
                                LocationCurve newConcurve = newCon.Location as LocationCurve;
                                Line ncl1 = newConcurve.Curve as Line;
                                XYZ ncenpt = ncl1.GetEndPoint(1);
                                XYZ direction = ncl1.Direction;
                                XYZ midPoint = ncenpt;
                                XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                Line axisLine = Line.CreateBound(midPoint, midHigh);
                                newConcurve.Rotate(axisLine, -l_angle);
                                ThirdConnectors = Utility.GetConnectorSet(e);
                                Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                C_bendtype.Set("Kick Offset");
                                C_bendangle.Set(l_angle);
                                C_bendtype2.Set("Kick Offset");
                                C_bendangle2.Set(NinetyAngle);

                                fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                bendtype.Set("Kick Offset");
                                bendangle.Set(l_angle);
                                bendtype2.Set("Kick Offset");
                                bendangle2.Set(NinetyAngle);

                                //if (i == 0)
                                //{
                                //    PrimryConduitcoloroverride(SecondaryElements[i].Id, doc);
                                //}
                                //else
                                //{
                                //    Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                //}
                                // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                // bottomTag.Add(fittings1);
                                // toptag.Add(fittings2);
                            }
                        }
                        else
                        {
                            try
                            {
                                for (int i = 0; i < SecondaryElements.Count; i++)
                                {
                                    double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                    //Direction indentification
                                    XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                    Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                    XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                    EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                    EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, -l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    if (profileSetting != null)
                                    {
                                        C_bendtype.Set(profileSetting.kOffsetValue);
                                    }
                                    C_bendangle.Set(l_angle);
                                    if (profileSetting != null)
                                    {
                                        C_bendtype2.Set(profileSetting.kOffsetValue);
                                    }
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);
                                    //if (i == 0)
                                    //{
                                    //    PrimryConduitcoloroverride(SecondaryElements[i].Id, doc);
                                    //}
                                    //else
                                    //{
                                    //    Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    //}

                                    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    // bottomTag.Add(fittings1);
                                    //toptag.Add(fittings2);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Please increase offset Value", "Warning");
                            }
                        }
                        //  _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                    }
                    catch
                    {
                        try
                        {
                            ConnectorSet PrimaryConnectors = null;
                            ConnectorSet SecondaryConnectors = null;
                            ConnectorSet ThirdConnectors = null;
                            if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                            {
                                for (int i = 0; i < PrimaryElements.Count; i++)
                                {
                                    double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);
                                    //  Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    // bottomTag.Add(fittings1);
                                    //toptag.Add(fittings2);
                                }
                            }
                            else
                            {
                                try
                                {
                                    for (int i = 0; i < SecondaryElements.Count; i++)
                                    {
                                        double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                        LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                        Line l1 = lc1.Curve as Line;
                                        LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                        Line l2 = lc2.Curve as Line;
                                        XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                        PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                        SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                        Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                        XYZ EndPoint = ConnectorTwo.Origin;
                                        XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                        //Direction indentification
                                        XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                        Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                        XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                        EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                        EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                        Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                        newCon.LookupParameter(offSetVar).Set(elevation);
                                        Element e = doc.GetElement(newCon.Id);
                                        LocationCurve newConcurve = newCon.Location as LocationCurve;
                                        Line ncl1 = newConcurve.Curve as Line;
                                        XYZ ncenpt = ncl1.GetEndPoint(1);
                                        XYZ direction = ncl1.Direction;
                                        XYZ midPoint = ncenpt;
                                        XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                        Line axisLine = Line.CreateBound(midPoint, midHigh);
                                        newConcurve.Rotate(axisLine, l_angle);
                                        ThirdConnectors = Utility.GetConnectorSet(e);
                                        Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                        Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                        Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                        Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                        C_bendtype.Set("Kick Offset");
                                        C_bendangle.Set(l_angle);
                                        C_bendtype2.Set("Kick Offset");
                                        C_bendangle2.Set(NinetyAngle);

                                        fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                        fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                        Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                        Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                        Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                        Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                        bendtype.Set("Kick Offset");
                                        bendangle.Set(l_angle);
                                        bendtype2.Set("Kick Offset");
                                        bendangle2.Set(NinetyAngle);
                                        // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                        // bottomTag.Add(fittings1);
                                        //toptag.Add(fittings2);
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Please increase offset Value", "Warning");
                                }
                            }
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                        catch (Exception exception)
                        {
                            System.Windows.MessageBox.Show("Warning. \n" + exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                            //  _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                    }
                }
                if (isUp)
                {
                    l_angle = Convert.ToDouble(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    try
                    {
                        ConnectorSet PrimaryConnectors = null;
                        ConnectorSet SecondaryConnectors = null;
                        ConnectorSet ThirdConnectors = null;
                        if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                        {
                            for (int i = 0; i < PrimaryElements.Count; i++)
                            {
                                double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                Line l1 = lc1.Curve as Line;
                                LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                Line l2 = lc2.Curve as Line;
                                XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                XYZ EndPoint = ConnectorTwo.Origin;
                                XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                newCon.LookupParameter(offSetVar).Set(elevation);
                                Element e = doc.GetElement(newCon.Id);
                                LocationCurve newConcurve = newCon.Location as LocationCurve;
                                Line ncl1 = newConcurve.Curve as Line;
                                XYZ ncenpt = ncl1.GetEndPoint(1);
                                XYZ direction = ncl1.Direction;
                                XYZ midPoint = ncenpt;
                                XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                Line axisLine = Line.CreateBound(midPoint, midHigh);
                                newConcurve.Rotate(axisLine, l_angle);
                                ThirdConnectors = Utility.GetConnectorSet(e);
                                Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                C_bendtype.Set("Kick Offset");
                                C_bendangle.Set(l_angle);
                                C_bendtype2.Set("Kick Offset");
                                C_bendangle2.Set(NinetyAngle);

                                fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                bendtype.Set("Kick Offset");
                                bendangle.Set(l_angle);
                                bendtype2.Set("Kick Offset");
                                bendangle2.Set(NinetyAngle);
                                //Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                // bottomTag.Add(fittings1);
                                // toptag.Add(fittings2);
                            }
                        }
                        else//create conduit
                        {
                            try
                            {
                                for (int i = 0; i < SecondaryElements.Count; i++)
                                {
                                    double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                    //Direction indentification
                                    XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                    Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                    XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                    EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                    EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, -l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);
                                    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    //bottomTag.Add(fittings1);
                                    //toptag.Add(fittings2);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Please increase offset Value", "Warning");
                            }
                        }
                        // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                    }
                    catch
                    {
                        try
                        {
                            ConnectorSet PrimaryConnectors = null;
                            ConnectorSet SecondaryConnectors = null;
                            ConnectorSet ThirdConnectors = null;
                            if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                            {
                                for (int i = 0; i < PrimaryElements.Count; i++)
                                {
                                    double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, -l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);
                                    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    // bottomTag.Add(fittings1);
                                    // toptag.Add(fittings2);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < SecondaryElements.Count; i++)
                                {
                                    double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                    //Direction indentification
                                    XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                    Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                    XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                    EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                    EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, -l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);
                                    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    // bottomTag.Add(fittings1);
                                    //toptag.Add(fittings2);
                                }
                            }
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                        catch (Exception exception)
                        {
                            System.Windows.MessageBox.Show("Warning. \n" + exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");

            }
            // TOPAddTags(doc,toptag);
            // BOTTOMAddTags(doc,toptag);
        }
        public static void PointApplyBend(Document doc, ref List<Element> PrimaryElements, double l_angle, double l_offSet, string offSetVar, XYZ pickpoint, UIApplication _uiapp, ref List<Element> SecondaryElements)
        {
            FamilyInstance fittings1 = null;
            FamilyInstance fittings2 = null;
            // List<FamilyInstance> bottomTag = null;
            //List<FamilyInstance> toptag = null;
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                KWBOffset.GetSecondaryPointElements(doc, ref PrimaryElements, l_angle, l_offSet, out SecondaryElements, offSetVar, pickpoint);

                double NinetyAngle = 90.00 * (Math.PI / 180);
                bool isUp = PrimaryElements.FirstOrDefault().LookupParameter(offSetVar).AsDouble() <
                    SecondaryElements.FirstOrDefault().LookupParameter(offSetVar).AsDouble();
                if (!isUp)
                {
                    l_angle = Convert.ToDouble(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    try
                    {
                        ConnectorSet PrimaryConnectors = null;
                        ConnectorSet SecondaryConnectors = null;
                        ConnectorSet ThirdConnectors = null;
                        if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                        {
                            for (int i = 0; i < PrimaryElements.Count; i++)
                            {
                                double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                Line l1 = lc1.Curve as Line;
                                LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                Line l2 = lc2.Curve as Line;
                                XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                XYZ EndPoint = ConnectorTwo.Origin;
                                XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                newCon.LookupParameter(offSetVar).Set(elevation);
                                Element e = doc.GetElement(newCon.Id);
                                LocationCurve newConcurve = newCon.Location as LocationCurve;
                                Line ncl1 = newConcurve.Curve as Line;
                                XYZ ncenpt = ncl1.GetEndPoint(1);
                                XYZ direction = ncl1.Direction;
                                XYZ midPoint = ncenpt;
                                XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                Line axisLine = Line.CreateBound(midPoint, midHigh);
                                newConcurve.Rotate(axisLine, -l_angle);
                                ThirdConnectors = Utility.GetConnectorSet(e);
                                Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                C_bendtype.Set("Kick Offset");
                                C_bendangle.Set(l_angle);
                                C_bendtype2.Set("Kick Offset");
                                C_bendangle2.Set(NinetyAngle);

                                // fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                // fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);

                                Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                bendtype.Set("Kick Offset");
                                bendangle.Set(l_angle);
                                bendtype2.Set("Kick Offset");
                                bendangle2.Set(NinetyAngle);
                                // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                //bottomTag.Add(fittings1);
                                // toptag.Add(fittings2);
                            }
                        }
                        else
                        {
                            try
                            {

                                for (int i = 0; i < SecondaryElements.Count; i++)
                                {
                                    double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                    //Direction indentification
                                    XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                    Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                    XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                    EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                    EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, -l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);
                                    //Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    // bottomTag.Add(fittings1);
                                    //toptag.Add(fittings2);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Please increase offset Value", "Warning");
                            }
                        }
                        // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                    }
                    catch
                    {
                        try
                        {
                            ConnectorSet PrimaryConnectors = null;
                            ConnectorSet SecondaryConnectors = null;
                            ConnectorSet ThirdConnectors = null;
                            if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                            {
                                for (int i = 0; i < PrimaryElements.Count; i++)
                                {
                                    double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);
                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);

                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);
                                    //  Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    // bottomTag.Add(fittings1);
                                    // toptag.Add(fittings2);
                                }
                            }
                            else
                            {
                                try
                                {
                                    for (int i = 0; i < SecondaryElements.Count; i++)
                                    {
                                        double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                        LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                        Line l1 = lc1.Curve as Line;
                                        LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                        Line l2 = lc2.Curve as Line;
                                        XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                        PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                        SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                        Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                        XYZ EndPoint = ConnectorTwo.Origin;
                                        XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                        //Direction indentification
                                        XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                        Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                        XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                        EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                        EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                        Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                        newCon.LookupParameter(offSetVar).Set(elevation);
                                        Element e = doc.GetElement(newCon.Id);
                                        LocationCurve newConcurve = newCon.Location as LocationCurve;
                                        Line ncl1 = newConcurve.Curve as Line;
                                        XYZ ncenpt = ncl1.GetEndPoint(1);
                                        XYZ direction = ncl1.Direction;
                                        XYZ midPoint = ncenpt;
                                        XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                        Line axisLine = Line.CreateBound(midPoint, midHigh);
                                        newConcurve.Rotate(axisLine, l_angle);
                                        ThirdConnectors = Utility.GetConnectorSet(e);
                                        Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                        Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                        Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                        Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                        C_bendtype.Set("Kick Offset");
                                        C_bendangle.Set(l_angle);
                                        C_bendtype2.Set("Kick Offset");
                                        C_bendangle2.Set(NinetyAngle);

                                        fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                        fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                        Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                        Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                        Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                        Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                        bendtype.Set("Kick Offset");
                                        bendangle.Set(l_angle);
                                        bendtype2.Set("Kick Offset");
                                        bendangle2.Set(NinetyAngle);
                                        // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                        // bottomTag.Add(fittings1);
                                        //toptag.Add(fittings2);

                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Please increase offset Value", "Warning");
                                }
                            }
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                        catch (Exception exception)
                        {
                            System.Windows.MessageBox.Show("Warning. \n" + exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                    }
                }
                if (isUp)
                {
                    l_angle = Convert.ToDouble(KickUserControl.Instance.ddlAngle.SelectedItem.ToString()) * (Math.PI / 180);
                    try
                    {
                        ConnectorSet PrimaryConnectors = null;
                        ConnectorSet SecondaryConnectors = null;
                        ConnectorSet ThirdConnectors = null;
                        if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                        {
                            for (int i = 0; i < PrimaryElements.Count; i++)
                            {
                                double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                Line l1 = lc1.Curve as Line;
                                LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                Line l2 = lc2.Curve as Line;
                                XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                XYZ EndPoint = ConnectorTwo.Origin;
                                XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                newCon.LookupParameter(offSetVar).Set(elevation);
                                Element e = doc.GetElement(newCon.Id);
                                LocationCurve newConcurve = newCon.Location as LocationCurve;
                                Line ncl1 = newConcurve.Curve as Line;
                                XYZ ncenpt = ncl1.GetEndPoint(1);
                                XYZ direction = ncl1.Direction;
                                XYZ midPoint = ncenpt;
                                XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                Line axisLine = Line.CreateBound(midPoint, midHigh);
                                newConcurve.Rotate(axisLine, l_angle);
                                ThirdConnectors = Utility.GetConnectorSet(e);
                                Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                Utility.RetainParameters(PrimaryElements[i], e, _uiapp);
                                Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                C_bendtype.Set("Kick Offset");
                                C_bendangle.Set(l_angle);
                                C_bendtype2.Set("Kick Offset");
                                C_bendangle2.Set(NinetyAngle);

                                fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);

                                Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                bendtype.Set("Kick Offset");
                                bendangle.Set(l_angle);
                                bendtype2.Set("Kick Offset");
                                bendangle2.Set(NinetyAngle);
                                //Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                //bottomTag.Add(fittings1);
                                //toptag.Add(fittings2);
                            }
                        }
                        else //create
                        {
                            try
                            {
                                for (int i = 0; i < SecondaryElements.Count; i++)
                                {
                                    double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                    //Direction indentification
                                    XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                    Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                    XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                    EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                    EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, -l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);
                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);

                                    //    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    //    //bottomTag.Add(fittings1);
                                    //    //toptag.Add(fittings2);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Please increase offset Value", "Warning");
                            }
                        }
                        //  _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                    }
                    catch
                    {
                        try
                        {
                            ConnectorSet PrimaryConnectors = null;
                            ConnectorSet SecondaryConnectors = null;
                            ConnectorSet ThirdConnectors = null;
                            if (KickUserControl.Instance.rbNinetyNear.IsChecked == true)
                            {
                                for (int i = 0; i < PrimaryElements.Count; i++)
                                {
                                    double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                                    LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                                    Line l1 = lc1.Curve as Line;
                                    LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                                    Line l2 = lc2.Curve as Line;
                                    XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                    PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                    SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ EndPoint = ConnectorTwo.Origin;
                                    XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);
                                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                    newCon.LookupParameter(offSetVar).Set(elevation);
                                    Element e = doc.GetElement(newCon.Id);
                                    LocationCurve newConcurve = newCon.Location as LocationCurve;
                                    Line ncl1 = newConcurve.Curve as Line;
                                    XYZ ncenpt = ncl1.GetEndPoint(1);
                                    XYZ direction = ncl1.Direction;
                                    XYZ midPoint = ncenpt;
                                    XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                    Line axisLine = Line.CreateBound(midPoint, midHigh);
                                    newConcurve.Rotate(axisLine, -l_angle);
                                    ThirdConnectors = Utility.GetConnectorSet(e);
                                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                    C_bendtype.Set("Kick Offset");
                                    C_bendangle.Set(l_angle);
                                    C_bendtype2.Set("Kick Offset");
                                    C_bendangle2.Set(NinetyAngle);

                                    fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                    bendtype.Set("Kick Offset");
                                    bendangle.Set(l_angle);
                                    bendtype2.Set("Kick Offset");
                                    bendangle2.Set(NinetyAngle);


                                    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                    //bottomTag.Add(fittings1);
                                    //toptag.Add(fittings2);
                                }
                            }
                            else
                            {
                                try
                                {
                                    for (int i = 0; i < SecondaryElements.Count; i++)
                                    {
                                        double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                                        LocationCurve lc1 = SecondaryElements[i].Location as LocationCurve;
                                        Line l1 = lc1.Curve as Line;
                                        LocationCurve lc2 = PrimaryElements[i].Location as LocationCurve;
                                        Line l2 = lc2.Curve as Line;
                                        XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                                        PrimaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                                        SecondaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                                        Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                                        XYZ EndPoint = ConnectorTwo.Origin;
                                        XYZ NewEndPoint = new XYZ(interSecPoint.X, interSecPoint.Y, EndPoint.Z);

                                        //Direction indentification
                                        XYZ Midpoint = (l2.GetEndPoint(0) + l2.GetEndPoint(1)) / 2;
                                        Line LineForconduitCreation = Line.CreateBound(interSecPoint, Midpoint);
                                        XYZ LineForconduitCreationDir = LineForconduitCreation.Direction;
                                        EndPoint = NewEndPoint + LineForconduitCreationDir.Multiply(1);
                                        EndPoint = new XYZ(EndPoint.X, EndPoint.Y, NewEndPoint.Z);

                                        Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, EndPoint, NewEndPoint);
                                        newCon.LookupParameter(offSetVar).Set(elevation);
                                        Element e = doc.GetElement(newCon.Id);
                                        LocationCurve newConcurve = newCon.Location as LocationCurve;
                                        Line ncl1 = newConcurve.Curve as Line;
                                        XYZ ncenpt = ncl1.GetEndPoint(1);
                                        XYZ direction = ncl1.Direction;
                                        XYZ midPoint = ncenpt;
                                        XYZ midHigh = midPoint.Add(XYZ.BasisZ.CrossProduct(direction));
                                        Line axisLine = Line.CreateBound(midPoint, midHigh);
                                        newConcurve.Rotate(axisLine, -l_angle);
                                        ThirdConnectors = Utility.GetConnectorSet(e);
                                        Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                                        Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                                        Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                                        Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                                        C_bendtype.Set("Kick Offset");
                                        C_bendangle.Set(l_angle);
                                        C_bendtype2.Set("Kick Offset");
                                        C_bendangle2.Set(NinetyAngle);

                                        fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                        fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                                        Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                                        Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                                        Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                                        Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                                        bendtype.Set("Kick Offset");
                                        bendangle.Set(l_angle);
                                        bendtype2.Set("Kick Offset");
                                        bendangle2.Set(NinetyAngle);


                                        // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                                        // bottomTag.Add(fittings1);
                                        // toptag.Add(fittings2);
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Please increase offset Value", "Warning");
                                }
                            }
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                        catch (Exception exception)
                        {
                            System.Windows.MessageBox.Show("Warning. \n" + exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Kick with Bend", Util.ProductVersion, "Draw");
            }
            // TOPAddTags(doc, toptag);
            // BOTTOMAddTags(doc, toptag);
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
        public static bool StrightorBend(Document doc, UIDocument uidoc, UIApplication uiApp, List<Element> PrimaryElements, string offsetVariable, XYZ Pickpoint)
        {
            PrimaryElements = PrimaryElements.Distinct().ToList();
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                startDate = DateTime.UtcNow;

                XYZ e1pt1 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ e1pt2 = ((PrimaryElements[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);

                double Z1 = Math.Round(e1pt1.Z, 2);
                double Z2 = Math.Round(e1pt2.Z, 2);

                /*  //using (SubTransaction substrans2 = new SubTransaction(doc))
                  //{
                  //    substrans2.Start();
                  //    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                  //    foreach (Element element in PrimaryElements)
                  //    {
                  //        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                  //    }
                  //    substrans2.Commit();
                  //}*/

                if (Z1 == Z2)
                {
                    if (StraightOrBendUserControl.Instance.angleList.SelectedItem.ToString() != "Auto")
                    {
                        if (Convert.ToDouble(StraightOrBendUserControl.Instance.angleList.SelectedItem) == 90.00)
                        {
                            using (SubTransaction transreset = new SubTransaction(doc))
                            {
                                transreset.Start();
                                OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                                foreach (Element element in PrimaryElements)
                                {
                                    doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                                }
                                transreset.Commit();
                            }
                            using SubTransaction subtrans = new SubTransaction(doc);
                            subtrans.Start();
                            NinetyApplyBend(doc, PrimaryElements, offsetVariable, Pickpoint, uiApp);
                            subtrans.Commit();
                            //  _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Complted", "90° Bend", Util.ProductVersion, "Draw");
                        }
                        else if (Convert.ToDouble(StraightOrBendUserControl.Instance.angleList.SelectedItem) == 0)
                        {
                            double StraightAngle = 0 * (Math.PI / 180);
                            using SubTransaction transaction = new SubTransaction(doc);
                            StraightsDrawParam globalParam = new StraightsDrawParam
                            {
                                IsAlignConduit = (bool)ParentUserControl.Instance.AlignConduits.IsChecked,
                                IsPrimaryAngle = (bool)ParentUserControl.Instance.Anglefromprimary.IsChecked
                            };
                            Properties.Settings.Default.StraightsDraw = JsonConvert.SerializeObject(globalParam);
                            Properties.Settings.Default.Save();

                            XYZ removingPoint = null;
                            transaction.Start();
                            XYZ trimPoint = null;
                            startDate = DateTime.UtcNow;
                            try
                            {
                                trimPoint = Pickpoint;
                            }
                            catch (Exception)
                            {
                                transaction.Dispose();
                                return false;
                            }

                            if (PrimaryElements.TrueForAll(x => IsBothSideUnConnectors(x) == true))
                            {
                                try
                                {
                                    bool isAllNull = true;
                                    foreach (Element element in PrimaryElements)
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
                                        // removingPoint = Utility.PickPoint(uidoc, "Select the connor to trim");
                                        int i = 0;
                                        foreach (Element element in PrimaryElements)
                                        {
                                            Line zLine = Utility.GetLineFromConduit(element);
                                            Line xyLine = Utility.GetLineFromConduit(element, true);
                                            XYZ startPoint = xyLine.GetEndPoint(0);
                                            XYZ endPoint = xyLine.GetEndPoint(1);

                                            Line trimLine = Utility.CrossProductLine(element, trimPoint, 50, true);
                                            XYZ ipTrim = Utility.GetIntersection(xyLine, trimLine);
                                            if (ipTrim != null)
                                            {
                                                try
                                                {
                                                    var PointOne = startPoint.DistanceTo(ipTrim);
                                                    var PointTwo = endPoint.DistanceTo(ipTrim);
                                                    if (startPoint.DistanceTo(ipTrim) > 0 && endPoint.DistanceTo(ipTrim) > 0)
                                                    {
                                                        Line firstLine = Line.CreateBound(startPoint, ipTrim);
                                                        Line secondLine = Line.CreateBound(endPoint, ipTrim);
                                                        Line removeLine = Utility.CrossProductLine(element, removingPoint, 50, true);

                                                        XYZ ipRemove = Utility.FindIntersectionPoint(firstLine, removeLine);
                                                        XYZ maxDisPoint = Utility.GetMaximumXYZ(xyLine.GetEndPoint(0), xyLine.GetEndPoint(1), ipRemove);
                                                        maxDisPoint = Utility.SetZvalue(maxDisPoint, zLine.GetEndPoint(0));
                                                        ipTrim = Utility.SetZvalue(ipTrim, zLine.GetEndPoint(0));

                                                        (element.Location as LocationCurve).Curve = maxDisPoint.IsAlmostEqualTo(zLine.GetEndPoint(0)) ? Line.CreateBound(maxDisPoint, ipTrim) : Line.CreateBound(ipTrim, maxDisPoint);
                                                        Parameter C_bendtype = element.LookupParameter("TIG-Bend Type");
                                                        Parameter C_bendangle = element.LookupParameter("TIG-Bend Angle");
                                                        C_bendtype?.Set("Straight");
                                                        C_bendangle?.Set(StraightAngle);
                                                    }
                                                }
                                                catch
                                                {

                                                }

                                                ///Conduitcoloroverride(element.Id, doc);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    ipTrim = Utility.FindIntersectionPoint(xyLine, trimLine);
                                                    XYZ maxDisPoint = Utility.GetMaximumXYZ(xyLine.GetEndPoint(0), xyLine.GetEndPoint(1), ipTrim);
                                                    maxDisPoint = Utility.SetZvalue(maxDisPoint, zLine.GetEndPoint(0));
                                                    ipTrim = Utility.SetZvalue(ipTrim, zLine.GetEndPoint(0));
                                                    (element.Location as LocationCurve).Curve = maxDisPoint.IsAlmostEqualTo(zLine.GetEndPoint(0)) ? Line.CreateBound(maxDisPoint, ipTrim) : Line.CreateBound(ipTrim, maxDisPoint);
                                                    Parameter C_bendtype = element.LookupParameter("TIG-Bend Type");
                                                    Parameter C_bendangle = element.LookupParameter("TIG-Bend Angle");
                                                    C_bendtype?.Set("Straight");
                                                    C_bendangle?.Set(StraightAngle);
                                                }
                                                catch
                                                {

                                                }
                                                //  Conduitcoloroverride(element.Id, doc);
                                            }
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            int i = 0;
                                            foreach (Element element in PrimaryElements)
                                            {
                                                Line xyLine = Utility.GetLineFromConduit(element, true);
                                                Line trimLine = Utility.CrossProductLine(element, trimPoint, 50, true);
                                                XYZ ipTrim = Utility.FindIntersection(element, trimLine);
                                                XYZ startPoint = xyLine.GetEndPoint(0);
                                                XYZ endPoint = xyLine.GetEndPoint(1);

                                                var PointOne = startPoint.DistanceTo(ipTrim);
                                                var PointTwo = endPoint.DistanceTo(ipTrim);

                                                if (ipTrim != null)
                                                {
                                                    try
                                                    {
                                                        if (startPoint.DistanceTo(ipTrim) > 0 && endPoint.DistanceTo(ipTrim) > 0)
                                                        {
                                                            XYZ maxDisPoint = Utility.GetMaximumXYZ(xyLine.GetEndPoint(0), xyLine.GetEndPoint(1), ipTrim);
                                                            Line line = Utility.GetLineFromConduit(element);
                                                            maxDisPoint = Utility.SetZvalue(maxDisPoint, line.GetEndPoint(0));
                                                            ipTrim = Utility.SetZvalue(ipTrim, line.GetEndPoint(0));
                                                            (element.Location as LocationCurve).Curve = maxDisPoint.IsAlmostEqualTo(line.GetEndPoint(0)) ? Line.CreateBound(maxDisPoint, ipTrim) : Line.CreateBound(ipTrim, maxDisPoint);
                                                            Parameter C_bendtype = element.LookupParameter("TIG-Bend Type");
                                                            Parameter C_bendangle = element.LookupParameter("TIG-Bend Angle");
                                                            C_bendtype?.Set("Straight");
                                                            C_bendangle?.Set(StraightAngle);
                                                        }
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                                // Conduitcoloroverride(element.Id, doc);
                                                i++;
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.StackTrace);
                                    transaction.Dispose();
                                    ParentUserControl.Instance._window.Close();
                                    return false;
                                }
                            }
                            else if (PrimaryElements.TrueForAll(x => IsOneSideConnectors(x) == true))
                            {
                                foreach (Connector connector in (PrimaryElements[0] as Conduit).ConnectorManager.UnusedConnectors)
                                    removingPoint = Utility.GetXYvalue(connector.Origin);
                                removingPoint = (removingPoint + trimPoint) / 2;
                                Line line_ = Utility.GetLineFromConduit(PrimaryElements[0]);
                                if (Utility.FindDifferentAxis(line_.GetEndPoint(0), line_.GetEndPoint(1)) == "Z")
                                {
                                    transaction.Dispose();
                                    Utility.AlertMessage("Feature not available for conduit stubs", false, MainWindow.Instance.SnackbarSeven);
                                    return false;
                                }
                                else
                                {
                                    int i = 0;
                                    bool breakloop = false;
                                    foreach (Element element in PrimaryElements)
                                    {
                                        // Conduitcoloroverride(element.Id, doc);
                                        Line zLine = Utility.GetLineFromConduit(element);
                                        Line xyLine = Utility.GetLineFromConduit(element, true);

                                        Line trimLine = Utility.CrossProductLine(element, trimPoint, 50, true);
                                        XYZ interSecPoint = Utility.FindIntersectionPoint(xyLine, trimLine);
                                        if (interSecPoint != null)
                                        {
                                            ConnectorSet connectorSet = Utility.GetConnectors(element);
                                            int j = 0;
                                            foreach (Connector con in connectorSet)
                                            {
                                                if (con.IsConnected)
                                                {
                                                    if (con.Origin.IsAlmostEqualTo(zLine.GetEndPoint(0)))
                                                    {
                                                        if (zLine.GetEndPoint(0).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)) > zLine.GetEndPoint(1).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)))
                                                        {
                                                            (element.Location as LocationCurve).Curve = Line.CreateBound(con.Origin, Utility.SetZvalue(interSecPoint, con.Origin));
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Line l1 = Line.CreateBound(con.Origin, zLine.GetEndPoint(1));
                                                            Line l2 = Line.CreateBound(new XYZ(con.Origin.X, con.Origin.Y, 0), interSecPoint);
                                                            if (Math.Sign(Math.Round(l1.Direction.X, 5)) == Math.Sign(Math.Round(l2.Direction.X, 5)) && (Math.Sign(Math.Round(l1.Direction.Y, 5)) == Math.Sign(Math.Round(l2.Direction.Y, 5))))
                                                            {

                                                                (element.Location as LocationCurve).Curve = Line.CreateBound(con.Origin, Utility.SetZvalue(interSecPoint, con.Origin));
                                                                Parameter C_bendtype = element.LookupParameter("TIG-Bend Type");
                                                                Parameter C_bendangle = element.LookupParameter("TIG-Bend Angle");
                                                                C_bendtype.Set("Straight");
                                                                C_bendangle.Set(StraightAngle);

                                                                //Conduitcoloroverride(element.Id, doc);
                                                            }
                                                            else
                                                            {
                                                                breakloop = true;
                                                                System.Windows.MessageBox.Show("Warning. \n" + "Cannot extend the conduits in opposite direction. Please pick another point", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                                break;

                                                            }
                                                            break;
                                                        }
                                                    }
                                                    if (con.Origin.IsAlmostEqualTo(zLine.GetEndPoint(1)))
                                                    {
                                                        if (zLine.GetEndPoint(1).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)) > zLine.GetEndPoint(0).DistanceTo(Utility.SetZvalue(interSecPoint, con.Origin)))
                                                        {
                                                            (element.Location as LocationCurve).Curve = Line.CreateBound(Utility.SetZvalue(interSecPoint, con.Origin), con.Origin);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Line l1 = Line.CreateBound(con.Origin, zLine.GetEndPoint(0));
                                                            Line l2 = Line.CreateBound(new XYZ(con.Origin.X, con.Origin.Y, 0), interSecPoint);
                                                            if (Math.Sign(Math.Round(l1.Direction.X, 5)) == Math.Sign(Math.Round(l2.Direction.X, 5)) && (Math.Sign(Math.Round(l1.Direction.Y, 5)) == Math.Sign(Math.Round(l2.Direction.Y, 5))))
                                                            {
                                                                (element.Location as LocationCurve).Curve = Line.CreateBound(Utility.SetZvalue(interSecPoint, con.Origin), con.Origin);
                                                                Parameter C_bendtype = element.LookupParameter("TIG-Bend Type");
                                                                Parameter C_bendangle = element.LookupParameter("TIG-Bend Angle");
                                                                C_bendtype.Set("Straight");
                                                                C_bendangle.Set(StraightAngle);
                                                            }
                                                            else
                                                            {
                                                                breakloop = true;
                                                                System.Windows.MessageBox.Show("Warning. \n" + "Cannot extend the conduits in opposite direction. Please pick another point", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                                break;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                                j++;
                                            }
                                        }
                                        else
                                        {

                                        }
                                        if (breakloop == true)
                                        {
                                            break;
                                        }
                                        i++;
                                    }
                                }
                            }

                            /* //int nk = 0;
                             //using (SubTransaction sunstransforcolorapply = new SubTransaction(doc))
                             //{
                             //    sunstransforcolorapply.Start();
                             //    foreach (Element element in PrimaryElements)
                             //    {
                             //        if (nk == 0)
                             //        {
                             //            PrimryConduitcoloroverride(element.Id, doc);
                             //        }
                             //        else
                             //        {
                             //            Conduitcoloroverride(element.Id, doc);
                             //        }

                             //        nk++;
                             //    }
                             //    sunstransforcolorapply.Commit();
                             //}
                             // Support.AddSupport(uiApp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) });*/

                            transaction.Commit();
                            // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Complted", "0° Bend", Util.ProductVersion, "Draw");
                        }
                        else if (Convert.ToDouble(StraightOrBendUserControl.Instance.angleList.SelectedItem) > 0 && Convert.ToDouble(StraightOrBendUserControl.Instance.angleList.SelectedItem) < 90)
                        {
                            if (ParentUserControl.Instance.Anglefromprimary.IsChecked == false)
                            {
                                try
                                {
                                    using SubTransaction substran3 = new SubTransaction(doc);
                                    substran3.Start();
                                    using (SubTransaction transreset = new SubTransaction(doc))
                                    {
                                        transreset.Start();
                                        OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                                        foreach (Element element in PrimaryElements)
                                        {
                                            doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                                        }
                                        transreset.Commit();
                                    }
                                    Dictionary<double, List<Element>> groupedElements = new Dictionary<double, List<Element>>();
                                    Utility.GroupByElevation(PrimaryElements, offsetVariable, ref groupedElements);
                                    PrimaryElements = new List<Element>();
                                    groupedElements = groupedElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);
                                    List<Element> inclindconduits = new List<Element>();
                                    Line baseline = null;
                                    XYZ startpoint = null;
                                    XYZ endpoint = null;
                                    foreach (KeyValuePair<double, List<Element>> valuePair in groupedElements)
                                    {
                                        List<Element> pickedelemets = valuePair.Value.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                                        Conduit conduitRef = pickedelemets[0] as Conduit;
                                        XYZ conduitRefdfirf = ((conduitRef.Location as LocationCurve).Curve as Line).Direction;
                                        XYZ conduitRefcross = conduitRefdfirf.CrossProduct(XYZ.BasisZ);
                                        XYZ basepoint = ((conduitRef.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                        XYZ bsepointtwo = ((conduitRef.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                        XYZ pt1 = Pickpoint;
                                        XYZ pt2 = Pickpoint + conduitRefcross.Multiply(5);
                                        Line crossline = Line.CreateBound(pt1, pt2);

                                        //find the specing between the conduits 
                                        XYZ intersectionone = Utility.FindIntersectionPoint(pt1, pt2, basepoint, bsepointtwo);
                                        conduitRefdfirf = Line.CreateBound(new XYZ(intersectionone.X, intersectionone.Y, basepoint.Z), basepoint).Direction;

                                        //check the angle direction
                                        LocationCurve curve = pickedelemets[0].Location as LocationCurve;
                                        Line l_Line = curve.Curve as Line;
                                        XYZ StartPoint = l_Line.GetEndPoint(0);
                                        XYZ EndPoint = l_Line.GetEndPoint(1);
                                        XYZ PrimaryConduitDirection = l_Line.Direction;
                                        XYZ CrossProduct = PrimaryConduitDirection.CrossProduct(XYZ.BasisZ);
                                        XYZ PickPointTwo = Pickpoint + CrossProduct.Multiply(1);

                                        XYZ Intersectionpoint = Utility.FindIntersectionPoint(StartPoint, EndPoint, Pickpoint, PickPointTwo);
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
                                        baseline = Line.CreateBound(ConduitEndpoint, new XYZ(Intersectionpoint.X, Intersectionpoint.Y, ConduitEndpoint.Z));

                                        for (int i = 0; i < pickedelemets.Count(); i++)
                                        {
                                            if (i == 0)
                                            {
                                                startpoint = new XYZ(Pickpoint.X, Pickpoint.Y, basepoint.Z);
                                                endpoint = startpoint + conduitRefdfirf.Multiply(5);
                                                Conduit newCon = Utility.CreateConduit(doc, pickedelemets[i] as Conduit, startpoint, endpoint);
                                                inclindconduits.Add(newCon);
                                                PrimaryElements.Add(pickedelemets[i]);
                                            }
                                            else
                                            {
                                                Conduit subconduitRef = pickedelemets[i] as Conduit;
                                                XYZ subbasepoint = ((subconduitRef.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                                XYZ subbsepointtwo = ((subconduitRef.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                                XYZ intersectiontwo = Utility.FindIntersectionPoint(pt1, pt2, subbasepoint, subbsepointtwo);
                                                double speacing = Math.Sqrt(Math.Pow((intersectionone.X - intersectiontwo.X), 2) + Math.Pow((intersectionone.Y - intersectiontwo.Y), 2));
                                                XYZ perpendiculardir = (Line.CreateBound(intersectionone, intersectiontwo).Direction);
                                                XYZ substartpoint = startpoint + perpendiculardir.Multiply(speacing);
                                                XYZ subendpoint = substartpoint + conduitRefdfirf.Multiply(5);
                                                Conduit newCon = Utility.CreateConduit(doc, pickedelemets[i] as Conduit, substartpoint, subendpoint);
                                                inclindconduits.Add(newCon);
                                                PrimaryElements.Add(pickedelemets[i]);
                                            }
                                        }
                                    }
                                    Line Baselineone = ((inclindconduits[0].Location as LocationCurve).Curve as Line);
                                    XYZ Baselinedirection = Baselineone.Direction;

                                    //Rotate Elements at Once
                                    Element ElementOne = PrimaryElements[0];
                                    Element ElementTwo = inclindconduits[0];
                                    Utility.GetClosestConnectors(ElementOne, ElementTwo, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ axisStart = ((ElementTwo.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                    XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
                                    Autodesk.Revit.DB.Line axisLine = Autodesk.Revit.DB.Line.CreateBound(axisStart, axisEnd);

                                    double angleforrotate = Convert.ToDouble(StraightOrBendUserControl.Instance.angleList.SelectedItem) * (Math.PI / 180);
                                    ElementTransformUtils.RotateElements(doc, inclindconduits.Select(r => r.Id).ToList(), axisLine, angleforrotate);

                                    //check the direction for rotating the conduit
                                    Line lineone = ((inclindconduits[0].Location as LocationCurve).Curve as Line);
                                    XYZ linedirection = lineone.Direction;
                                    XYZ linonept1 = lineone.GetEndPoint(0);
                                    XYZ linonept2 = lineone.GetEndPoint(1);
                                    linonept2 += linedirection.Multiply(100);

                                    Line linetwo = ((PrimaryElements[0].Location as LocationCurve).Curve as Line);
                                    XYZ linetwopt1 = linetwo.GetEndPoint(0);
                                    XYZ linetwopt2 = linetwo.GetEndPoint(1);
                                    linetwopt1 += Baselinedirection.Multiply(100);
                                    linetwopt2 -= Baselinedirection.Multiply(100);
                                    XYZ intesectionforrotate = Utility.GetIntersection(Line.CreateBound(linonept1, linonept2), Line.CreateBound(linetwopt1, linetwopt2));

                                    if (intesectionforrotate == null)
                                    {
                                        angleforrotate = 2 * angleforrotate;
                                        ElementTransformUtils.RotateElements(doc, inclindconduits.Select(r => r.Id).ToList(), axisLine, -angleforrotate);
                                    }

                                    //check the fittings creations
                                    LocationCurve Inclind_curve = inclindconduits[0].Location as LocationCurve;
                                    Line Inclind_l_Line = Inclind_curve.Curve as Line;
                                    XYZ Inclind_l_Line_dir = Inclind_l_Line.Direction;
                                    XYZ Inclind_l_Line_pt2 = Inclind_l_Line.GetEndPoint(0) + Inclind_l_Line_dir.Multiply(100);
                                    Line Inclind_l_Line_sub = Line.CreateBound(Inclind_l_Line.GetEndPoint(0), Inclind_l_Line_pt2);
                                    XYZ intersectionforfittingscheck = Utility.GetIntersection(Inclind_l_Line_sub, baseline);

                                    if (intersectionforfittingscheck != null)
                                    {
                                        double minimumdistance = Math.Sqrt(Math.Pow((endpoint.X - intersectionforfittingscheck.X), 2) + Math.Pow((endpoint.Y - intersectionforfittingscheck.Y), 2));

                                        if (minimumdistance > 2)
                                        {
                                            for (int i = 0; i < PrimaryElements.Count; i++)
                                            {
                                                Element firstElement = PrimaryElements[i];
                                                Element secondElement = inclindconduits[i];
                                                FamilyInstance halfbend = Utility.CreateElbowFittings(firstElement, secondElement, doc, uiApp, true);

                                                Parameter C_bendtype = secondElement.LookupParameter("TIG-Bend Type");
                                                Parameter C_bendangle = secondElement.LookupParameter("TIG-Bend Angle");
                                                C_bendtype.Set("Half Offset");
                                                C_bendangle.Set(halfbend.LookupParameter("Angle").AsDouble());

                                                Parameter C_bendtype_bd = halfbend.LookupParameter("TIG-Bend Type");
                                                Parameter C_bendangle_bd = halfbend.LookupParameter("TIG-Bend Angle");
                                                C_bendtype_bd.Set("Half Offset");
                                                C_bendangle_bd.Set(halfbend.LookupParameter("Angle").AsDouble());



                                                //Conduitcoloroverride(secondElement.Id, doc);
                                            }
                                            //fill the colors
                                            //using (SubTransaction sunstransforcolorapply = new SubTransaction(doc))
                                            //{
                                            //    ElementId conduitrunid = (PrimaryElements[PrimaryElements.Count() -1] as Conduit).RunId;
                                            //    sunstransforcolorapply.Start();
                                            //    foreach (Element element in inclindconduits)
                                            //    {
                                            //        ElementId inclindconduitrunid = (element as Conduit).RunId;

                                            //        if (inclindconduitrunid == conduitrunid)
                                            //        {
                                            //            PrimryConduitcoloroverride(element.Id, doc);
                                            //        }
                                            //        else
                                            //        {
                                            //            Conduitcoloroverride(element.Id, doc);
                                            //        }
                                            //    }
                                            //    sunstransforcolorapply.Commit();
                                            //}

                                            using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                                            {
                                                sunstransforrunsync.Start();
                                                foreach (Element element in PrimaryElements)
                                                {
                                                    Conduit conduitone = element as Conduit;
                                                    ElementId eid = conduitone.RunId;
                                                    if (eid != null)
                                                    {
                                                        Element conduitrun = doc.GetElement(eid);
                                                        Utility.AutoRetainParameters(element, conduitrun, doc, uiApp);
                                                    }
                                                }
                                                sunstransforrunsync.Commit();
                                            }
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            ParentUserControl.Instance.Primaryelst.Clear();
                                            ParentUserControl.Instance.Primaryelst.AddRange(inclindconduits);
                                        }
                                        else
                                        {
                                            foreach (Element element in inclindconduits)
                                            {
                                                doc.Delete(element.Id);
                                            }
                                            TaskDialog.Show("Warning", "Couldn't add a fitting. Please change the bend angle or enable Add bend in-place");
                                        }

                                    }
                                    else
                                    {
                                        foreach (Element element in inclindconduits)
                                        {
                                            doc.Delete(element.Id);
                                        }
                                        TaskDialog.Show("Warning", "Couldn't add a fitting. Please change the bend angle or enable Add bend in-place");
                                    }

                                    string angleuseractivity = Convert.ToString(StraightOrBendUserControl.Instance.angleList.SelectedItem);
                                    substran3.Commit();
                                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Complted", angleuseractivity + "° Bend", Util.ProductVersion, "Draw");
                                }
                                catch
                                {
                                    TaskDialog.Show("Warning", "Couldn't add a fitting. Please change the bend angle or enable Add bend in-place");
                                }

                            }
                            else
                            {
                                try
                                {
                                    using SubTransaction substran3 = new SubTransaction(doc);
                                    substran3.Start();

                                    StraightsDrawParam globalParam = new StraightsDrawParam
                                    {
                                        IsAlignConduit = (bool)ParentUserControl.Instance.AlignConduits.IsChecked,
                                        IsPrimaryAngle = (bool)ParentUserControl.Instance.Anglefromprimary.IsChecked
                                    };
                                    Properties.Settings.Default.StraightsDraw = JsonConvert.SerializeObject(globalParam);
                                    Properties.Settings.Default.Save();
                                    Conduit conduitRef = PrimaryElements[0] as Conduit;
                                    XYZ conduitRefdfirf = ((conduitRef.Location as LocationCurve).Curve as Line).Direction;
                                    XYZ conduitRefcross = conduitRefdfirf.CrossProduct(XYZ.BasisZ);
                                    XYZ pt1 = Pickpoint;
                                    XYZ pt2 = Pickpoint + conduitRefcross.Multiply(5);
                                    Line crossline = Line.CreateBound(pt1, pt2);
                                    List<Element> inclindconduits = new List<Element>();
                                    foreach (Element ele in PrimaryElements)
                                    {
                                        Conduit conduitone = ele as Conduit;
                                        XYZ prefpt1 = ((conduitone.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                        XYZ prefpt2 = ((conduitone.Location as LocationCurve).Curve as Line).GetEndPoint(1); XYZ intesectionpoint = Utility.FindIntersectionPoint(pt1, pt2, prefpt1, prefpt2); double SubdistanceOne = Math.Sqrt(Math.Pow((prefpt1.X - intesectionpoint.X), 2) + Math.Pow((prefpt1.Y - intesectionpoint.Y), 2));
                                        double SubdistanceTwo = Math.Sqrt(Math.Pow((prefpt2.X - intesectionpoint.X), 2) + Math.Pow((prefpt2.Y - intesectionpoint.Y), 2)); XYZ ConduitStartpt = null;
                                        XYZ ConduitEndpoint = null;
                                        if (SubdistanceOne < SubdistanceTwo)
                                        {
                                            ConduitStartpt = prefpt1;
                                            ConduitEndpoint = prefpt2;
                                        }
                                        else
                                        {
                                            ConduitStartpt = prefpt2;
                                            ConduitEndpoint = prefpt1;
                                        }
                                        Conduit newCon = Utility.CreateConduit(doc, ele as Conduit, ConduitStartpt, new XYZ(intesectionpoint.X, intesectionpoint.Y, ConduitStartpt.Z));
                                        inclindconduits.Add(newCon);
                                    }
                                    XYZ startpt1 = ((inclindconduits[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    double startdistance = Math.Sqrt(Math.Pow((startpt1.X - Pickpoint.X), 2) + Math.Pow((startpt1.Y - Pickpoint.Y), 2));                                 //Rotate Elements at Once
                                    Element ElementOne = PrimaryElements[0];
                                    Element ElementTwo = inclindconduits[0];
                                    Utility.GetClosestConnectors(ElementOne, ElementTwo, out Connector ConnectorOne, out Connector ConnectorTwo);
                                    XYZ axisStart = ConnectorOne.Origin;
                                    XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
                                    Autodesk.Revit.DB.Line axisLine = Autodesk.Revit.DB.Line.CreateBound(axisStart, axisEnd); double angleforrotate = Convert.ToDouble(StraightOrBendUserControl.Instance.angleList.SelectedItem) * (Math.PI / 180);
                                    ElementTransformUtils.RotateElements(doc, inclindconduits.Select(r => r.Id).ToList(), axisLine, angleforrotate); XYZ startpt2 = ((inclindconduits[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    double enddistance = Math.Sqrt(Math.Pow((startpt2.X - Pickpoint.X), 2) + Math.Pow((startpt2.Y - Pickpoint.Y), 2)); if (startdistance < enddistance)
                                    {
                                        angleforrotate = 2 * angleforrotate;
                                        ElementTransformUtils.RotateElements(doc, inclindconduits.Select(r => r.Id).ToList(), axisLine, -angleforrotate);
                                    }
                                    DeleteSupports(doc, PrimaryElements);
                                    for (int i = 0; i < PrimaryElements.Count; i++)
                                    {
                                        Element firstElement = PrimaryElements[i];
                                        Element secondElement = inclindconduits[i];
                                        FamilyInstance halfbend = Utility.CreateElbowFittings(firstElement, secondElement, doc, uiApp, true);

                                        Parameter C_bendtype = secondElement.LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle = secondElement.LookupParameter("TIG-Bend Angle");
                                        C_bendtype.Set("Half Offset");
                                        C_bendangle.Set(halfbend.LookupParameter("Angle").AsDouble());

                                        Parameter C_bendtype_bd = halfbend.LookupParameter("TIG-Bend Type");
                                        Parameter C_bendangle_bd = halfbend.LookupParameter("TIG-Bend Angle");
                                        C_bendtype_bd.Set("Half Offset");
                                        C_bendangle_bd.Set(halfbend.LookupParameter("Angle").AsDouble());
                                        //Conduitcoloroverride(secondElement.Id, doc);
                                    }

                                    /* //fill the colors
                                     //using (SubTransaction sunstransforcolorapply = new SubTransaction(doc))
                                     //{
                                     //    ElementId conduitrunid = (PrimaryElements[PrimaryElements.Count() - 1] as Conduit).RunId;
                                     //    sunstransforcolorapply.Start();
                                     //    foreach (Element element in inclindconduits)
                                     //    {
                                     //        ElementId inclindconduitrunid = (element as Conduit).RunId;

                                     //        if (conduitrunid == inclindconduitrunid)
                                     //        {
                                     //            PrimryConduitcoloroverride(element.Id, doc);
                                     //        }
                                     //        else
                                     //        {
                                     //            Conduitcoloroverride(element.Id, doc);
                                     //        }
                                     //    }
                                     //    sunstransforcolorapply.Commit();
                                     //}*/

                                    using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                                    {
                                        sunstransforrunsync.Start();
                                        foreach (Element element in PrimaryElements)
                                        {
                                            Conduit conduitone = element as Conduit;
                                            ElementId eid = conduitone.RunId;
                                            if (eid != null)
                                            {
                                                Element conduitrun = doc.GetElement(eid);
                                                Utility.AutoRetainParameters(element, conduitrun, doc, uiApp);
                                            }
                                        }
                                        sunstransforrunsync.Commit();
                                    }
                                    // Support.AddSupport(uiApp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(inclindconduits) });
                                    ParentUserControl.Instance.Secondaryelst.Clear();
                                    ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                    ParentUserControl.Instance.Primaryelst.Clear();
                                    ParentUserControl.Instance.Primaryelst.AddRange(inclindconduits);
                                    string angleuseractivity = Convert.ToString(StraightOrBendUserControl.Instance.angleList.SelectedItem);
                                    substran3.Commit();
                                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Complted", angleuseractivity + "° Bend", Util.ProductVersion, "Draw");
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {

                            Conduit conduitRef = PrimaryElements[0] as Conduit;
                            XYZ conduitRefdfirf = ((conduitRef.Location as LocationCurve).Curve as Line).Direction;
                            XYZ conduitRefcross = conduitRefdfirf.CrossProduct(XYZ.BasisZ);
                            XYZ pt1 = Pickpoint;
                            XYZ pt2 = Pickpoint + conduitRefcross.Multiply(100);

                            //order the conduit collection 
                            List<Element> Newconduitcollection = new List<Element>();
                            Newconduitcollection = Conduit_order(PrimaryElements, Line.CreateBound(pt1, pt2));

                            //first set
                            XYZ prefpt1 = ((Newconduitcollection.First().Location as LocationCurve).Curve as Line).GetEndPoint(0);
                            XYZ prefpt2 = ((Newconduitcollection.First().Location as LocationCurve).Curve as Line).GetEndPoint(1);

                            XYZ intesectionpoint = Utility.FindIntersectionPoint(pt1, pt2, prefpt1, prefpt2);

                            double SubdistanceOne = Math.Sqrt(Math.Pow((prefpt1.X - intesectionpoint.X), 2) + Math.Pow((prefpt1.Y - intesectionpoint.Y), 2));
                            double SubdistanceTwo = Math.Sqrt(Math.Pow((prefpt2.X - intesectionpoint.X), 2) + Math.Pow((prefpt2.Y - intesectionpoint.Y), 2));

                            XYZ ConduitStartpt = null;
                            XYZ ConduitEndpoint = null;
                            if (SubdistanceOne < SubdistanceTwo)
                            {
                                ConduitStartpt = prefpt1;
                                ConduitEndpoint = prefpt2;
                            }
                            else
                            {
                                ConduitStartpt = prefpt2;
                                ConduitEndpoint = prefpt1;
                            }

                            //second set
                            XYZ SECprefpt1 = ((Newconduitcollection.Last().Location as LocationCurve).Curve as Line).GetEndPoint(0);
                            XYZ SECprefpt2 = ((Newconduitcollection.Last().Location as LocationCurve).Curve as Line).GetEndPoint(1);

                            XYZ SECintesectionpoint = Utility.FindIntersectionPoint(pt1, pt2, SECprefpt1, SECprefpt2);

                            double SECSubdistanceOne = Math.Sqrt(Math.Pow((SECprefpt1.X - SECintesectionpoint.X), 2) + Math.Pow((SECprefpt1.Y - SECintesectionpoint.Y), 2));
                            double SECSubdistanceTwo = Math.Sqrt(Math.Pow((SECprefpt2.X - SECintesectionpoint.X), 2) + Math.Pow((SECprefpt2.Y - SECintesectionpoint.Y), 2));

                            XYZ SECConduitStartpt = null;
                            XYZ SECConduitEndpoint = null;
                            if (SECSubdistanceOne < SECSubdistanceTwo)
                            {
                                SECConduitStartpt = SECprefpt1;
                                SECConduitEndpoint = SECprefpt2;
                            }
                            else
                            {
                                SECConduitStartpt = SECprefpt2;
                                SECConduitEndpoint = SECprefpt1;
                            }

                            XYZ secondaryFirstpoint = ConduitStartpt;
                            XYZ secondarysecondpoint = SECConduitStartpt;
                            XYZ groupMidpoint = (secondaryFirstpoint + secondarysecondpoint) / 2;

                            Line lineAngleCheck = Line.CreateBound(new XYZ(ConduitStartpt.X, ConduitStartpt.Y, secondaryFirstpoint.Z), new XYZ(Pickpoint.X, Pickpoint.Y, secondaryFirstpoint.Z));
                            double xDir = (lineAngleCheck.GetEndPoint(1).Y - lineAngleCheck.GetEndPoint(0).Y) / (lineAngleCheck.GetEndPoint(1).X - lineAngleCheck.GetEndPoint(0).X);
                            double Angle = Math.Atan2(lineAngleCheck.GetEndPoint(1).Y - lineAngleCheck.GetEndPoint(0).Y, lineAngleCheck.GetEndPoint(1).X - lineAngleCheck.GetEndPoint(0).X)
                                - Math.Atan2(ConduitEndpoint.Y - ConduitStartpt.Y, ConduitEndpoint.X - ConduitStartpt.X);
                            double slope = Math.Abs(Angle);
                            double Angleforrotate = slope;

                            List<Element> inclindconduits = new List<Element>();
                            foreach (Element ele in Newconduitcollection)
                            {
                                Conduit conduitone = ele as Conduit;
                                XYZ Newprefpt1 = ((conduitone.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                XYZ Newprefpt2 = ((conduitone.Location as LocationCurve).Curve as Line).GetEndPoint(1);

                                XYZ Newintesectionpoint = Utility.FindIntersectionPoint(pt1, pt2, Newprefpt1, Newprefpt2);

                                double NewSubdistanceOne = Math.Sqrt(Math.Pow((prefpt1.X - Newintesectionpoint.X), 2) + Math.Pow((prefpt1.Y - Newintesectionpoint.Y), 2));
                                double NewSubdistanceTwo = Math.Sqrt(Math.Pow((prefpt2.X - Newintesectionpoint.X), 2) + Math.Pow((prefpt2.Y - Newintesectionpoint.Y), 2));

                                XYZ NewConduitStartpt = null;
                                XYZ NewConduitEndpoint = null;
                                if (NewSubdistanceOne < NewSubdistanceTwo)
                                {
                                    NewConduitStartpt = Newprefpt1;
                                    NewConduitEndpoint = Newprefpt2;
                                }
                                else
                                {
                                    NewConduitStartpt = Newprefpt2;
                                    NewConduitEndpoint = Newprefpt1;
                                }
                                //conduit line 
                                Line conduitline = Line.CreateBound(NewConduitStartpt, new XYZ(Newintesectionpoint.X, Newintesectionpoint.Y, NewConduitStartpt.Z));
                                XYZ conduitlinedirection = conduitline.Direction;
                                XYZ conduitsecondpoint = NewConduitStartpt + conduitlinedirection.Multiply(8);
                                Conduit newCon = Utility.CreateConduit(doc, ele as Conduit, NewConduitStartpt, new XYZ(conduitsecondpoint.X, conduitsecondpoint.Y, NewConduitStartpt.Z));
                                inclindconduits.Add(newCon);

                            }

                            XYZ startpt1 = ((inclindconduits[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                            double startdistance = Math.Sqrt(Math.Pow((startpt1.X - Pickpoint.X), 2) + Math.Pow((startpt1.Y - Pickpoint.Y), 2));

                            //Rotate Elements at Once
                            Element ElementOne = Newconduitcollection[0];
                            Element ElementTwo = inclindconduits[0];
                            Utility.GetClosestConnectors(ElementOne, ElementTwo, out Connector ConnectorOne, out Connector ConnectorTwo);
                            XYZ axisStart = ConnectorOne.Origin;
                            XYZ axisEnd = new XYZ(axisStart.X, axisStart.Y, axisStart.Z + 10);
                            Autodesk.Revit.DB.Line axisLine = Autodesk.Revit.DB.Line.CreateBound(axisStart, axisEnd);

                            double angleforrotate = Angleforrotate;

                            //nearest angle iondentiofication 
                            double Fordegree = angleforrotate * 180 / Math.PI;
                            double originalangle = 5;

                            if (Fordegree > 90 && Fordegree < 180)
                            {
                                Fordegree = Math.Abs(Fordegree - 180);
                            }
                            else if (Fordegree > 180 && Fordegree < 270)
                            {
                                Fordegree -= 180;
                            }
                            else if (Fordegree > 270 && Fordegree < 360)
                            {
                                Fordegree = Math.Abs(Fordegree - 360);
                            }

                            if (0 < Fordegree && Fordegree < 5)
                            {
                                originalangle = 5;
                            }
                            else if (5 < Fordegree && Fordegree < 11.25)
                            {
                                originalangle = 11.25;
                            }
                            else if (11.25 < Fordegree && Fordegree < 15)
                            {
                                originalangle = 15;
                            }
                            else if (15 < Fordegree && Fordegree < 22.5)
                            {
                                originalangle = 22.5;
                            }
                            else if (22.5 < Fordegree && Fordegree < 30)
                            {
                                originalangle = 30;
                            }
                            else if (30 < Fordegree && Fordegree < 45)
                            {
                                originalangle = 45;
                            }
                            else if (45 < Fordegree && Fordegree < 60)
                            {
                                originalangle = 60;
                            }
                            else if (60 < Fordegree && Fordegree < 90)
                            {
                                originalangle = 90;
                            }

                            originalangle = originalangle * Math.PI / 180;
                            angleforrotate = originalangle;

                            ElementTransformUtils.RotateElements(doc, inclindconduits.Select(r => r.Id).ToList(), axisLine, angleforrotate);

                            XYZ startpt2 = ((inclindconduits[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                            double enddistance = Math.Sqrt(Math.Pow((startpt2.X - Pickpoint.X), 2) + Math.Pow((startpt2.Y - Pickpoint.Y), 2));

                            if (startdistance < enddistance)
                            {
                                angleforrotate = 2 * angleforrotate;
                                ElementTransformUtils.RotateElements(doc, inclindconduits.Select(r => r.Id).ToList(), axisLine, -angleforrotate);
                            }
                            DeleteSupports(doc, PrimaryElements);
                            for (int i = 0; i < Newconduitcollection.Count; i++)
                            {
                                Element firstElement = Newconduitcollection[i];
                                Element secondElement = inclindconduits[i];
                                Utility.CreateElbowFittings(firstElement, secondElement, doc, uiApp, true);
                            }

                            ParentUserControl.Instance.Secondaryelst.Clear();
                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                            ParentUserControl.Instance.Primaryelst.Clear();
                            ParentUserControl.Instance.Primaryelst.AddRange(inclindconduits);
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    using Transaction subtrans = new Transaction(doc);
                    subtrans.Start("90 bend Draw");
                    Pickpoint ??= Utility.PickPoint(uidoc, "Click on a point to select a direction");
                    if (Pickpoint == null)
                        return false;

                    NinetyApplyBend(doc, PrimaryElements, offsetVariable, Pickpoint, uiApp);
                    subtrans.Commit();
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Complted", "90° Bend", Util.ProductVersion, "Draw");
                }
                StraightOrBendUserControl.Instance.angleList.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Pick operation aborted.")
                {
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //  _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiApp, Util.ApplicationWindowTitle, startDate, "Failed", "90° Bend", Util.ProductVersion, "Draw");
                }
            }

            return true;
        }
        public static List<Element> Conduit_order(List<Element> conduits, Line grid)
        {
            List<double> distance_collection = new List<double>();
            List<Element> conduit_order = new List<Element>();
            Line grid_line = grid as Line;
            XYZ newpoint1 = grid_line.GetEndPoint(0);
            XYZ newpoint2 = grid_line.GetEndPoint(1);
            Line grid_perdicular_line = Line.CreateBound(newpoint1, newpoint2);
            foreach (Element cond in conduits)
            {
                LocationCurve locur = cond.Location as LocationCurve;
                Line conduit_line = locur.Curve as Line;
                XYZ intersectionpoint = Utility.FindIntersectionPoint(grid_perdicular_line.GetEndPoint(0), grid_perdicular_line.GetEndPoint(1), conduit_line.GetEndPoint(0), conduit_line.GetEndPoint(1));
                double distance = (Math.Pow(newpoint1.X - intersectionpoint.X, 2) + Math.Pow(newpoint1.Y - intersectionpoint.Y, 2));
                distance = Math.Abs(Math.Sqrt(distance));
                distance_collection.Add(distance);
            }
            distance_collection.Sort();
            foreach (double dou in distance_collection)
            {
                foreach (Element cond in conduits)
                {
                    LocationCurve locur = cond.Location as LocationCurve;
                    Line conduit_line = locur.Curve as Line;
                    XYZ intersectionpoint = Utility.FindIntersectionPoint(grid_perdicular_line.GetEndPoint(0), grid_perdicular_line.GetEndPoint(1), conduit_line.GetEndPoint(0), conduit_line.GetEndPoint(1));
                    double distance = (Math.Pow(newpoint1.X - intersectionpoint.X, 2) + Math.Pow(newpoint1.Y - intersectionpoint.Y, 2));
                    distance = Math.Abs(Math.Sqrt(distance));
                    if (distance == dou)
                    {
                        conduit_order.Add(cond);
                    }
                }
            }
            return conduit_order;
        }
        public static void NinetyApplyBend(Document doc, List<Element> PrimaryElements, string offSetVar, XYZ pickpoint, UIApplication _uiapp)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                if (ParentUserControl.Instance.Anglefromprimary.IsChecked == true)
                {
                    NinetyBend.GetSecondaryElements(doc, ref PrimaryElements, out List<Element> SecondaryElements, offSetVar, pickpoint);

                    ConnectorSet PrimaryConnectors = null;
                    ConnectorSet SecondaryConnectors = null;
                    for (int i = 0; i < PrimaryElements.Count; i++)
                    {
                        double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                        LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                        Line l1 = lc1.Curve as Line;
                        LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                        Line l2 = lc2.Curve as Line;
                        XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                        PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                        SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                        Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                        Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                        FamilyInstance fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, PrimaryConnectors, doc, _uiapp, PrimaryElements[i], true);

                        Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                        Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                        C_bendtype.Set("90° Bend");
                        C_bendangle.Set(90 * (Math.PI / 180));

                        Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                        Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                        bendtype.Set("90° Bend");
                        bendangle.Set(90 * (Math.PI / 180));
                        //if (i == 0)
                        //{
                        //    PrimryConduitcoloroverride(SecondaryElements[i].Id, doc);
                        //}
                        //else
                        //{
                        //    Conduitcoloroverride(SecondaryElements[i].Id, doc);
                        //}

                        // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                    }

                    using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, doc, _uiapp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }

                    // Support.AddSupport(_uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                }
                else
                {
                    NinetyBend.GetSecondarypointElements(doc, ref PrimaryElements, out List<Element> SecondaryElements, offSetVar, pickpoint);

                    ConnectorSet PrimaryConnectors = null;
                    ConnectorSet SecondaryConnectors = null;
                    for (int i = 0; i < PrimaryElements.Count; i++)
                    {
                        double elevation = PrimaryElements[i].LookupParameter(offSetVar).AsDouble();
                        LocationCurve lc1 = PrimaryElements[i].Location as LocationCurve;
                        Line l1 = lc1.Curve as Line;
                        LocationCurve lc2 = SecondaryElements[i].Location as LocationCurve;
                        Line l2 = lc2.Curve as Line;
                        XYZ interSecPoint = Utility.FindIntersectionPoint(l1.GetEndPoint(0), l1.GetEndPoint(1), l2.GetEndPoint(0), l2.GetEndPoint(1));
                        PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                        SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                        Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                        Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                        FamilyInstance fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, PrimaryConnectors, doc, _uiapp, PrimaryElements[i], true);

                        Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                        Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                        C_bendtype.Set("90° Bend");
                        C_bendangle.Set(90 * (Math.PI / 180));

                        Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                        Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                        bendtype.Set("90° Bend");
                        bendangle.Set(90 * (Math.PI / 180));

                        //if (i == 0)
                        //{
                        //    PrimryConduitcoloroverride(SecondaryElements[i].Id, doc);
                        //}
                        //else
                        //{
                        //    Conduitcoloroverride(SecondaryElements[i].Id, doc);
                        //}

                        //Conduitcoloroverride(SecondaryElements[i].Id, doc);
                    }

                    using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in PrimaryElements)
                        {
                            Conduit conduitone = element as Conduit;
                            ElementId eid = conduitone.RunId;
                            if (eid != null)
                            {
                                Element conduitrun = doc.GetElement(eid);
                                Utility.AutoRetainParameters(element, conduitrun, doc, _uiapp);
                            }
                        }
                        sunstransforrunsync.Commit();
                    }

                    // Support.AddSupport(_uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) }, new List<ConduitsCollection> { new ConduitsCollection(SecondaryElements) });
                }



            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "90° Bend", Util.ProductVersion, "Draw");

            }
        }
        public static bool Ninetykickdrawhandler(Document doc, List<Element> PrimaryElements, string offsetVariable, UIApplication uiapp, XYZ pickpoint)
        {
            DateTime startDate = DateTime.UtcNow;
            try
            {
                NinetyKickGP globalParam = new NinetyKickGP
                {
                    OffsetValue = NinetyKickUserControl.Instance.txtOffset.AsDouble == 0 ? "1.5" : NinetyKickUserControl.Instance.txtOffset.AsString,
                    RiseValue = NinetyKickUserControl.Instance.txtRise.AsDouble == 0 ? "10.0" : NinetyKickUserControl.Instance.txtRise.AsString,
                    AngleValue = NinetyKickUserControl.Instance.ddlAngle.SelectedItem == null ? "30.00" : NinetyKickUserControl.Instance.ddlAngle.SelectedItem.ToString()
                };

                double l_angle;
                Utility.GetProjectUnits(uiapp, NinetyKickUserControl.Instance.txtOffset.Text, out double l_offSet, out _);
                Utility.GetProjectUnits(uiapp, NinetyKickUserControl.Instance.txtRise.Text, out double l_Rise, out _);

                using (SubTransaction substrans2 = new SubTransaction(doc))
                {
                    substrans2.Start();
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in PrimaryElements)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }

                    substrans2.Commit();
                }
                using (SubTransaction transKick = new SubTransaction(doc))
                {
                    transKick.Start();
                    Properties.Settings.Default.NinetyKickDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    //MultiSelect angleSelected = NinetyKickUserControl.Instance.ddlAngle.SelectedItem.ToString();
                    l_angle = Convert.ToDouble(NinetyKickUserControl.Instance.ddlAngle.SelectedItem.ToString());
                    DeleteSupports(doc, PrimaryElements);
                    ApplyKick(doc, ref PrimaryElements, l_angle, l_offSet, l_Rise, offsetVariable, uiapp, pickpoint);
                    // Support.AddSupport(uiapp, doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) });
                    transKick.Commit();
                }

                using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                {
                    sunstransforrunsync.Start();
                    foreach (Element element in PrimaryElements)
                    {
                        Conduit conduitone = element as Conduit;
                        ElementId eid = conduitone.RunId;
                        if (eid != null)
                        {
                            Element conduitrun = doc.GetElement(eid);
                            Utility.AutoRetainParameters(element, conduitrun, doc, uiapp);
                        }
                    }
                    sunstransforrunsync.Commit();
                }
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);
                //Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "90° Kick", Util.ProductVersion);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                //Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "90° Kick", Util.ProductVersion);
            }
            return true;
        }
        public static void BendAngleTagcreation(Document doc, XYZ locationpoint)
        {
            //FamilySymbol famsymid = 
            //IndependentTag newTag = IndependentTag.Create(doc,);
        }
        public static void ApplyKick(Document doc, ref List<Element> PrimaryElements, double l_angle, double l_offSet, double l_Rise, string offSetVar, UIApplication _uiapp, XYZ pickpoint)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                XYZ Pickpoint = pickpoint;
                Kick.GetSecondaryElements(doc, ref PrimaryElements, l_angle, l_offSet, l_Rise, out List<Element> SecondaryElements, offSetVar, Pickpoint);
                for (int i = 0; i < PrimaryElements.Count; i++)
                {
                    double elevation = SecondaryElements[i].LookupParameter(offSetVar).AsDouble();
                    ConnectorSet PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                    ConnectorSet SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);

                    //finding second point
                    Conduit FirestConduit = PrimaryElements[i] as Conduit;
                    Autodesk.Revit.DB.Line firstline = (FirestConduit.Location as LocationCurve).Curve as Autodesk.Revit.DB.Line;
                    XYZ firestlinedirection = firstline.Direction;
                    XYZ firestlinecross = firestlinedirection.CrossProduct(XYZ.BasisZ);
                    XYZ secondconduitfirstpt = ConnectorTwo.Origin;
                    XYZ secondconduitsecondpt = ConnectorTwo.Origin + firestlinecross.Multiply(1);
                    XYZ inetsecforthridconduuit = Utility.FindIntersectionPoint(secondconduitfirstpt, secondconduitsecondpt, firstline.GetEndPoint(0), firstline.GetEndPoint(1));

                    Conduit newCon = Utility.CreateConduit(doc, PrimaryElements[i] as Conduit, ConnectorTwo.Origin, new XYZ(inetsecforthridconduuit.X, inetsecforthridconduuit.Y, ConnectorOne.Origin.Z));
                    newCon.LookupParameter(offSetVar).Set(elevation);
                    Element e = doc.GetElement(newCon.Id);
                    ConnectorSet ThirdConnectors = Utility.GetConnectorSet(e);
                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);
                    Utility.RetainParameters(PrimaryElements[i], e, _uiapp);

                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                    Parameter C_bendtype2 = newCon.LookupParameter("TIG-Bend Type");
                    Parameter C_bendangle2 = newCon.LookupParameter("TIG-Bend Angle");
                    C_bendtype.Set("90° Kick Offset");
                    C_bendangle.Set(l_angle * (Math.PI / 180));
                    C_bendtype2.Set("90° Kick Offset");
                    C_bendangle2.Set(90 * (Math.PI / 180));

                    FamilyInstance fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                    FamilyInstance fittings2 = Utility.CreateElbowFittings(PrimaryConnectors, ThirdConnectors, doc, _uiapp, PrimaryElements[i], true);
                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                    Parameter bendtype2 = fittings2.LookupParameter("TIG-Bend Type");
                    Parameter bendangle2 = fittings2.LookupParameter("TIG-Bend Angle");
                    bendtype.Set("90° Kick Offset");
                    bendangle.Set(l_angle * (Math.PI / 180));
                    bendtype2.Set("90° Kick Offset");
                    bendangle2.Set(90 * (Math.PI / 180));

                    //if (i == 0)
                    //{
                    //    PrimryConduitcoloroverride(SecondaryElements[i].Id, doc);
                    //}
                    //else
                    //{
                    //    Conduitcoloroverride(SecondaryElements[i].Id, doc);
                    //}

                    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                }

                using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                {
                    sunstransforrunsync.Start();
                    foreach (Element element in PrimaryElements)
                    {
                        Conduit conduitone = element as Conduit;
                        ElementId eid = conduitone.RunId;
                        if (eid != null)
                        {
                            Element conduitrun = doc.GetElement(eid);
                            Utility.AutoRetainParameters(element, conduitrun, doc, _uiapp);
                        }
                    }
                    sunstransforrunsync.Commit();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "90° Kick", Util.ProductVersion);
            }


        }
        public static bool Nietystubdrawhandler(Document _doc, UIDocument _uiDoc, List<Element> PrimaryElements, string offsetVariable, UIApplication _uiapp, XYZ Pickpoint)
        {
            DateTime startDate = DateTime.UtcNow;
            try
            {
                NinetyStubGP globalParam = new NinetyStubGP
                {
                    OffsetValue = NinetyStubUserControl.Instance.txtOffsetFeet.AsDouble == 0 ? "5" : NinetyStubUserControl.Instance.txtOffsetFeet.AsString
                };

                double l_offSet = NinetyStubUserControl.Instance.txtOffsetFeet.AsDouble;

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

                using (SubTransaction tx = new SubTransaction(_doc))
                {
                    tx.Start();
                    Pickpoint ??= Utility.PickPoint(_uiDoc, "Click on a point to select a direction");
                    if (Pickpoint == null)
                        return false;
                    Properties.Settings.Default.NinetyStubDraw = JsonConvert.SerializeObject(globalParam);
                    Properties.Settings.Default.Save();
                    DeleteSupports(_doc, PrimaryElements);
                    StubApplyBend(_doc, ref PrimaryElements, l_offSet, offsetVariable, Pickpoint, _uiapp);
                    //  Support.AddSupport(_uiapp, _doc, new List<ConduitsCollection> { new ConduitsCollection(PrimaryElements) });
                    tx.Commit();
                }
                using (SubTransaction sunstransforrunsync = new SubTransaction(_doc))
                {
                    sunstransforrunsync.Start();
                    foreach (Element element in PrimaryElements)
                    {
                        Conduit conduitone = element as Conduit;
                        ElementId eid = conduitone.RunId;
                        if (eid != null)
                        {
                            Element conduitrun = _doc.GetElement(eid);
                            Utility.AutoRetainParameters(element, conduitrun, _doc, _uiapp);
                        }
                    }
                    sunstransforrunsync.Commit();
                }
                ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                ParentUserControl.Instance.masterContainer.Children.Clear();
                UserControl userControl = new StraightOrBendUserControl(ParentUserControl.Instance._externalEvents[0], ParentUserControl.Instance._window, StraightOrBendUserControl.Instance._application);
                ParentUserControl.Instance.masterContainer.Children.Add(userControl);

            }
            catch (Exception exception)
            {
                if (exception.Message == "Pick operation aborted.")
                {
                    return false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "90° Stub Bend", Util.ProductVersion);
                }
            }
            return true;
        }
        public static void StubApplyBend(Document doc, ref List<Element> PrimaryElements, double stublength, string offSetVar, XYZ pickpoint, UIApplication _uiapp)
        {
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData profileSetting = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            DateTime startDate = DateTime.UtcNow;
            try
            {
                NinetyBendstub.GetSecondaryElements(doc, ref PrimaryElements, out List<Element> SecondaryElements, stublength, offSetVar, pickpoint);
                ConnectorSet PrimaryConnectors = null;
                ConnectorSet SecondaryConnectors = null;
                for (int i = 0; i < PrimaryElements.Count; i++)
                {
                    PrimaryConnectors = Utility.GetConnectorSet(PrimaryElements[i]);
                    SecondaryConnectors = Utility.GetConnectorSet(SecondaryElements[i]);
                    Utility.GetClosestConnectors(PrimaryConnectors, SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo);
                    Utility.RetainParameters(PrimaryElements[i], SecondaryElements[i], _uiapp);

                    Parameter C_bendtype = SecondaryElements[i].LookupParameter("TIG-Bend Type");
                    Parameter C_bendangle = SecondaryElements[i].LookupParameter("TIG-Bend Angle");
                    C_bendtype.Set("90° Stub");
                    C_bendangle.Set(90 * (Math.PI / 180));

                    FamilyInstance fittings1 = Utility.CreateElbowFittings(SecondaryConnectors, PrimaryConnectors, doc, _uiapp, PrimaryElements[i], true);

                    Parameter bendtype = fittings1.LookupParameter("TIG-Bend Type");
                    Parameter bendangle = fittings1.LookupParameter("TIG-Bend Angle");
                    bendtype.Set("90° Stub");
                    bendangle.Set(90 * (Math.PI / 180));

                    //if (i == 0)
                    //{
                    //    PrimryConduitcoloroverride(SecondaryElements[i].Id, doc);
                    //}
                    //else
                    //{
                    //    Conduitcoloroverride(SecondaryElements[i].Id, doc);
                    //}

                    // Conduitcoloroverride(SecondaryElements[i].Id, doc);
                }

                using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                {
                    sunstransforrunsync.Start();
                    foreach (Element element in PrimaryElements)
                    {
                        Conduit conduitone = element as Conduit;
                        ElementId eid = conduitone.RunId;
                        if (eid != null)
                        {
                            Element conduitrun = doc.GetElement(eid);
                            Utility.AutoRetainParameters(element, conduitrun, doc, _uiapp);
                        }
                    }
                    sunstransforrunsync.Commit();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Warning. \n" + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                //  _ = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), _uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "90° Stub Bend", Util.ProductVersion, "Draw");

            }
        }
        public static Autodesk.Revit.DB.OverrideGraphicSettings SetOverrideGraphicSettings(FillPatternElement fpe, Autodesk.Revit.DB.Color color)
        {
            Autodesk.Revit.DB.OverrideGraphicSettings ogs = new Autodesk.Revit.DB.OverrideGraphicSettings();
            ogs.SetProjectionLineColor(color);
            ogs.SetCutBackgroundPatternColor(color);
            ogs.SetCutForegroundPatternColor(color);
            ogs.SetCutLineColor(color);
            ogs.SetSurfaceBackgroundPatternColor(color);
            ogs.SetSurfaceForegroundPatternColor(color);
            ogs.SetSurfaceBackgroundPatternId(fpe.Id);
            ogs.SetSurfaceBackgroundPatternVisible(true);
            ogs.SetSurfaceForegroundPatternId(fpe.Id);
            ogs.SetSurfaceForegroundPatternVisible(true);
            return ogs;
        }
    }
}




