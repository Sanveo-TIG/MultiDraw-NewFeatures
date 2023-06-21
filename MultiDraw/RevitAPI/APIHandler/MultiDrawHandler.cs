using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using Application = Autodesk.Revit.ApplicationServices.Application;
using TIGUtility;
using System.Windows.Media.Animation;
using adWin = Autodesk.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MultiDraw
{
    [Transaction(TransactionMode.Manual)]
    public class MultiDrawHandler : IExternalEventHandler
    {
        int num = 0;
        public void Execute(UIApplication uiapp)
        {

            if (num == 0)
            {
                Mainfunction(uiapp);
                num++;
            }
            else
            {
                return;
            }
        }
        public static void Mainfunction(UIApplication uiapp)
        {
            DateTime startDate = DateTime.UtcNow;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            int.TryParse(uiapp.Application.VersionNumber, out int RevitVersion);
            string offsetVariable = RevitVersion < 2020 ? "Offset" : "Middle Elevation";
            //Selection sel = uidoc.Selection;
            //ICollection<ElementId> ecol = sel.GetElementIds();

            List<Element> failingelement = new List<Element>();
            List<Element> pickedElements = new List<Element>();
            List<Element> secondaryElements = new List<Element>();
            List<Element> thirdElements = new List<Element>();
            bool Refpiuckpoint = false;
            bool fittingsfailure = false;
            string mainTransName = "MultiDraw-Straight/Bend";

            try
            {
                using (Transaction transRibbonColorChange_2 = new Transaction(doc))
                {
                    transRibbonColorChange_2.Start("PrimaryElementColorOverride");
                    adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                    LinearGradientBrush gB = new LinearGradientBrush
                    {
                        StartPoint = new System.Windows.Point(0, 0),
                        EndPoint = new System.Windows.Point(0, 1)
                    };
                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 0.0));
                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 1));

                    foreach (adWin.RibbonTab tab in ribbon.Tabs)
                    {
                        foreach (adWin.RibbonPanel panel in tab.Panels)
                        {
                            if (panel.Source.AutomationName == "MultiDraw")
                            {
                                panel.CustomPanelTitleBarBackground
                        = gB;
                            }

                        }

                    }
                    transRibbonColorChange_2.Commit();
                }

                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                for (int k = 0; k < 100; k++)
                {
                    if (ParentUserControl.Instance.cmbProfileType.SelectedIndex != 7)
                    {
                        if (k == 0)
                        {
                            if (ParentUserControl.Instance.Primaryelst == null)
                            {
                                using (Transaction transRibbonColorChange_2 = new Transaction(doc))
                                {
                                    transRibbonColorChange_2.Start("PrimaryElementColorOverride");
                                    adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                                    LinearGradientBrush gB = new LinearGradientBrush
                                    {
                                        StartPoint = new System.Windows.Point(0, 0),
                                        EndPoint = new System.Windows.Point(0, 1)
                                    };
                                    gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 0.0));
                                    gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 1));

                                    foreach (adWin.RibbonTab tab in ribbon.Tabs)
                                    {
                                        foreach (adWin.RibbonPanel panel in tab.Panels)
                                        {
                                            if (panel.Source.AutomationName == "MultiDraw")
                                            {
                                                panel.CustomPanelTitleBarBackground
                                        = gB;
                                            }

                                        }

                                    }
                                    transRibbonColorChange_2.Commit();

                                }
                                ParentUserControl.Instance._window.Close();
                                return;
                            }
                            if (ParentUserControl.Instance.Primaryelst.Count() == 0)
                            {
                                //System.Windows.MessageBox.Show("Please select the conduits alone to perform action", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                uidoc.Selection.SetElementIds(new List<ElementId> { ElementId.InvalidElementId });
                                using (Transaction transRibbonColorChange_2 = new Transaction(doc))
                                {
                                    transRibbonColorChange_2.Start("PrimaryElementColorOverride");
                                    adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                                    LinearGradientBrush gB = new LinearGradientBrush
                                    {
                                        StartPoint = new System.Windows.Point(0, 0),
                                        EndPoint = new System.Windows.Point(0, 1)
                                    };
                                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 0.0));
                                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 1));

                                    foreach (adWin.RibbonTab tab in ribbon.Tabs)
                                    {
                                        foreach (adWin.RibbonPanel panel in tab.Panels)
                                        {
                                            if (panel.Source.AutomationName == "MultiDraw")
                                            {
                                                panel.CustomPanelTitleBarBackground
                                        = gB;
                                            }

                                        }

                                    }
                                    transRibbonColorChange_2.Commit();

                                }
                                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                                continue;

                            }

                            if (ParentUserControl.Instance.Primaryelst.Any(x => Utility.GetUnusedConnectors(x).Size == 0))
                            {
                                failingelement.Clear();
                                System.Windows.MessageBox.Show("Selection cannot be modified. \n Please select conduits with at least one end open.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                uidoc.Selection.SetElementIds(new List<ElementId> { ElementId.InvalidElementId });

                                foreach (Element ele in ParentUserControl.Instance.Primaryelst)
                                {
                                    ConnectorSet connec = Utility.GetUnusedConnectors(ele);
                                    if (connec.Size == 0)
                                    {
                                        failingelement.Add(ele);
                                    }
                                }
                                if (failingelement.Count() > 0)
                                {
                                    using Transaction substransover = new Transaction(doc);
                                    substransover.Start("ResetcolorMultiDraw");
                                    foreach (Element ele in failingelement)
                                    {
                                        Conduitcoloroverride(ele.Id, doc);
                                    }
                                    substransover.Commit();
                                }

                                ParentUserControl.Instance.Primaryelst.Clear();
                                using (Transaction transRibbonColorChange_2 = new Transaction(doc))
                                {
                                    transRibbonColorChange_2.Start("PrimaryElementColorOverride");
                                    adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                                    LinearGradientBrush gB = new LinearGradientBrush
                                    {
                                        StartPoint = new System.Windows.Point(0, 0),
                                        EndPoint = new System.Windows.Point(0, 1)
                                    };
                                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 0.0));
                                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 1));

                                    foreach (adWin.RibbonTab tab in ribbon.Tabs)
                                    {
                                        foreach (adWin.RibbonPanel panel in tab.Panels)
                                        {
                                            if (panel.Source.AutomationName == "MultiDraw")
                                            {
                                                panel.CustomPanelTitleBarBackground
                                        = gB;
                                            }

                                        }

                                    }
                                    transRibbonColorChange_2.Commit();
                                }
                                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                            }
                        }

                    }
                    try
                    {
                        using Transaction tx = new Transaction(doc);
                        tx.Start(mainTransName);
                        using (SubTransaction transRibbonColorChange = new SubTransaction(doc))
                        {
                            transRibbonColorChange.Start();
                            adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                            LinearGradientBrush gB = new LinearGradientBrush
                            {
                                StartPoint = new System.Windows.Point(0, 0),
                                EndPoint = new System.Windows.Point(0, 1)
                            };
                            gB.GradientStops.Add(new GradientStop(Colors.LimeGreen, 0.0));
                            gB.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1));

                            foreach (adWin.RibbonTab tab in ribbon.Tabs)
                            {
                                foreach (adWin.RibbonPanel panel in tab.Panels)
                                {
                                    if (panel.Source.AutomationName == "MultiDraw")
                                    {
                                        panel.CustomPanelTitleBarBackground
                                = gB;
                                    }

                                }
                            }
                            transRibbonColorChange.Commit();
                        }
                        if (failingelement.Count() > 0)
                        {
                            using SubTransaction transreset = new SubTransaction(doc);
                            transreset.Start();
                            OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                            foreach (Element element in failingelement)
                            {
                                doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                            }
                            transreset.Commit();
                        }
                        if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 7)
                        {
                            if (!APICommon.ConduitSync(doc, uiapp, ParentUserControl.Instance.Primaryelst))
                            {

                                break;
                            }
                            else
                            {
                                tx.Commit();
                                using (SubTransaction transRibbonColorChange_2 = new SubTransaction(doc))
                                {
                                    transRibbonColorChange_2.Start();
                                    adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                                    LinearGradientBrush gB = new LinearGradientBrush
                                    {
                                        StartPoint = new System.Windows.Point(0, 0),
                                        EndPoint = new System.Windows.Point(0, 1)
                                    };
                                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 0.0));
                                    gB.GradientStops.Add(new GradientStop(Colors.Orange, 1));
                                    foreach (adWin.RibbonTab tab in ribbon.Tabs)
                                    {
                                        foreach (adWin.RibbonPanel panel in tab.Panels)
                                        {
                                            if (panel.Source.AutomationName == "MultiDraw")
                                            {
                                                panel.CustomPanelTitleBarBackground
                                        = gB;
                                            }
                                        }
                                    }
                                    transRibbonColorChange_2.Commit();
                                }
                                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                                continue;
                            }
                        }
                        else
                        {
                            XYZ Pickpoint = Utility.PickPoint(uidoc);
                            if (ParentUserControl.Instance.AlignConduits.IsChecked == true)
                            {
                                XYZ Pickpoint_Two = Utility.PickPoint(uidoc);
                                using SubTransaction subone = new SubTransaction(doc);
                                subone.Start();
                                StraightsDrawParam globalParam = new StraightsDrawParam
                                {
                                    IsPrimaryAngle = (bool)ParentUserControl.Instance.Anglefromprimary.IsChecked
                                };
                                Properties.Settings.Default.StraightsDraw = JsonConvert.SerializeObject(globalParam);
                                Properties.Settings.Default.Save();
                                foreach (Element item in ParentUserControl.Instance.Primaryelst)
                                {
                                    Conduit conduit = item as Conduit;
                                    XYZ pt1 = ((conduit.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                    XYZ pt2 = ((conduit.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    XYZ intersections = Utility.FindIntersectionPoint(Pickpoint, Pickpoint_Two, pt1, pt2);
                                    double distanceone = Math.Sqrt(Math.Pow((intersections.X - pt1.X), 2) + Math.Pow((intersections.Y - pt1.Y), 2));
                                    double distancetwo = Math.Sqrt(Math.Pow((intersections.X - pt2.X), 2) + Math.Pow((intersections.Y - pt2.Y), 2));
                                    if (distanceone < distancetwo)
                                    {
                                        (conduit.Location as LocationCurve).Curve = Line.CreateBound(new XYZ(intersections.X, intersections.Y, pt1.Z), pt2);
                                    }
                                    else
                                    {
                                        (conduit.Location as LocationCurve).Curve = Line.CreateBound(pt1, new XYZ(intersections.X, intersections.Y, pt2.Z));
                                    }
                                }
                                ParentUserControl.Instance.AlignConduits.IsChecked = false;
                                subone.Commit();
                            }
                            else
                            {
                                if (k > 0)
                                {

                                }
                                if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 0)
                                {
                                    mainTransName = "MultiDraw-Vertical Offset";
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 1)
                                {
                                    mainTransName = "MultiDraw-Horizontal Offset";
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 2)
                                {
                                    mainTransName = "MultiDraw-Rolling Offset";
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 3)
                                {
                                    mainTransName = "MultiDraw-Kick";
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 4)
                                {
                                    mainTransName = "MultiDraw-Straight/Bend";
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 5)
                                {
                                    mainTransName = "MultiDraw-Kick 90";
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 6)
                                {
                                    mainTransName = "MultiDraw-Stub 90";
                                }
                                if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 1)
                                {
                                    if (ParentUserControl.Instance.Anglefromprimary.IsChecked == true)
                                    {
                                        if (!APICommon.HOffsetDrawHandler(doc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, Refpiuckpoint, Pickpoint, ref secondaryElements))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            ParentUserControl.Instance.Primaryelst.Clear();
                                            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                        }
                                    }
                                    else
                                    {
                                        if (!APICommon.HOffsetDrawPointHandler(doc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, Refpiuckpoint, Pickpoint, ref secondaryElements, ref fittingsfailure))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);


                                            if (fittingsfailure == false)
                                            {
                                                ParentUserControl.Instance.Primaryelst.Clear();
                                                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                            }

                                        }
                                    }

                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 0)
                                {
                                    if (ParentUserControl.Instance.Anglefromprimary.IsChecked == false)
                                    {
                                        if (!APICommon.VOffsetDrawPointHandler(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, RevitVersion, Pickpoint, ref secondaryElements))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            ParentUserControl.Instance.Primaryelst.Clear();
                                            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                        }
                                    }
                                    else
                                    {
                                        if (!APICommon.VOffsetDrawHandler(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, RevitVersion, Pickpoint, ref secondaryElements))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            ParentUserControl.Instance.Primaryelst.Clear();
                                            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                        }
                                    }


                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 2)
                                {
                                    //if (ParentUserControl.Instance.Anglefromprimary.IsChecked == true)
                                    //{
                                    if (!APICommon.RollingOffsetDrawHandler(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, Pickpoint, ref secondaryElements))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        ParentUserControl.Instance.Secondaryelst.Clear();
                                        ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                        ParentUserControl.Instance.Primaryelst.Clear();
                                        ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                    }
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 3)
                                {
                                    if (ParentUserControl.Instance.Anglefromprimary.IsChecked == true)
                                    {
                                        if (!APICommon.KWBDrawHandler(doc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, Pickpoint, ref secondaryElements))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            ParentUserControl.Instance.Primaryelst.Clear();
                                            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                        }
                                    }
                                    else
                                    {
                                        if (!APICommon.KWBDrawPointHandler(doc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, Pickpoint, ref secondaryElements))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            ParentUserControl.Instance.Primaryelst.Clear();
                                            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                        }
                                    }

                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 4 && !APICommon.StrightorBend(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, Pickpoint))
                                {
                                    break;
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 5 && !APICommon.Ninetykickdrawhandler(doc, ParentUserControl.Instance.Primaryelst, offsetVariable, uiapp, Pickpoint))
                                {
                                    break;
                                }
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 6 && !APICommon.Nietystubdrawhandler(doc, uidoc, ParentUserControl.Instance.Primaryelst, offsetVariable, uiapp, Pickpoint))
                                {
                                    break;
                                }
                            }


                        }
                        using (SubTransaction sub = new SubTransaction(doc))
                        {
                            sub.Start();
                            Settings settings = ParentUserControl.Instance.GetSettings();
                            Properties.Settings.Default.MultiDrawSettings = JsonConvert.SerializeObject(settings);
                            Properties.Settings.Default.Save();
                            sub.Commit();
                            ParentUserControl.Instance.MultiDrawSettings = settings;
                        }

                        using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                        {
                            sunstransforrunsync.Start();
                            foreach (Element element in ParentUserControl.Instance.Primaryelst)
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
                        using (SubTransaction transRibbonColorChange = new SubTransaction(doc))
                        {
                            transRibbonColorChange.Start();
                            adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                            LinearGradientBrush gB = new LinearGradientBrush
                            {
                                StartPoint = new System.Windows.Point(0, 0),
                                EndPoint = new System.Windows.Point(0, 1)
                            };
                            gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 0.0));
                            gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 1));
                            foreach (adWin.RibbonTab tab in ribbon.Tabs)
                            {
                                foreach (adWin.RibbonPanel panel in tab.Panels)
                                {
                                    panel.CustomPanelTitleBarBackground = gB;
                                }
                            }
                            transRibbonColorChange.Commit();
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        if (ParentUserControl.Instance.Primaryelst != null)
                        {
                            using Transaction transreset = new Transaction(doc);
                            transreset.Start("PrimaryColorReset");
                            OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                            foreach (Element element in ParentUserControl.Instance.Primaryelst)
                            {
                                doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                            }
                            using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                            {
                                sunstransforrunsync.Start();
                                foreach (Element element in ParentUserControl.Instance.Primaryelst)
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
                            using (SubTransaction transRibbonColorChange = new SubTransaction(doc))
                            {
                                transRibbonColorChange.Start();
                                adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                                LinearGradientBrush gB = new LinearGradientBrush
                                {
                                    StartPoint = new System.Windows.Point(0, 0),
                                    EndPoint = new System.Windows.Point(0, 1)
                                };
                                gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 0.0));
                                gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 1));
                                foreach (adWin.RibbonTab tab in ribbon.Tabs)
                                {
                                    foreach (adWin.RibbonPanel panel in tab.Panels)
                                    {
                                        panel.CustomPanelTitleBarBackground = gB;
                                    }
                                }
                                transRibbonColorChange.Commit();
                            }
                            transreset.Commit();
                        }
                        Mainfunction(uiapp);
                        break;
                    }
                }
            }
            catch
            {
                if (ParentUserControl.Instance.Primaryelst != null)
                {
                    using Transaction transreset = new Transaction(doc);
                    transreset.Start("PrimaryColorReset");
                    OverrideGraphicSettings orGsty = new OverrideGraphicSettings();
                    foreach (Element element in ParentUserControl.Instance.Primaryelst)
                    {
                        doc.ActiveView.SetElementOverrides(element.Id, orGsty);
                    }
                    using (SubTransaction sunstransforrunsync = new SubTransaction(doc))
                    {
                        sunstransforrunsync.Start();
                        foreach (Element element in ParentUserControl.Instance.Primaryelst)
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
                    using (SubTransaction transRibbonColorChange = new SubTransaction(doc))
                    {
                        transRibbonColorChange.Start();
                        adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                        LinearGradientBrush gB = new LinearGradientBrush
                        {
                            StartPoint = new System.Windows.Point(0, 0),
                            EndPoint = new System.Windows.Point(0, 1)
                        };
                        gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 0.0));
                        gB.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 1));
                        foreach (adWin.RibbonTab tab in ribbon.Tabs)
                        {
                            foreach (adWin.RibbonPanel panel in tab.Panels)
                            {
                                panel.CustomPanelTitleBarBackground = gB;
                            }
                        }
                        transRibbonColorChange.Commit();
                    }
                    transreset.Commit();
                }

                ParentUserControl.Instance._window.Close();
                return;
            }
        }

        public string GetName()
        {
            return "Horizontal Offset Handler";
        }
        public static void Conduitcoloroverride(ElementId eid, Document doc)
        {
            var patternCollector = new FilteredElementCollector(doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");
            Autodesk.Revit.DB.OverrideGraphicSettings ogs_Hoffset = SetOverrideGraphicSettings(fpe, new Autodesk.Revit.DB.Color(255, 0, 0));
            doc.ActiveView.SetElementOverrides(eid, ogs_Hoffset);
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

        public class SelectElementsFilter : ISelectionFilter
        {
            readonly string CategoryName = "";

            public SelectElementsFilter(string name)
            {
                CategoryName = name;
            }

            public bool AllowElement(Element e)
            {
                if (e.Category != null && e.Category.Name == CategoryName)
                    return true;
                return false;
            }

            public bool AllowReference(Reference r, XYZ p)
            {
                return false;
            }
        }
    }
}
