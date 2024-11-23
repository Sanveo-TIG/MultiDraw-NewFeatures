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
using MultiDraw.RevitAPI.APIHandler;
using Color = Autodesk.Revit.DB.Color;

namespace MultiDraw
{
    [Transaction(TransactionMode.Manual)]
    public class MultiDrawHandler : IExternalEventHandler
    {
        static int num = 0;
        public ProfileColorSettingsData ProfileColorSettingsData = new ProfileColorSettingsData();

        public void Execute(UIApplication uiapp)
        {
            if (num == 0)
            {
                bool isTrue = Mainfunction(uiapp);
            }
            else
            {
                return;
            }
        }

        public static bool Mainfunction(UIApplication uiapp)
        {
            if (ParentUserControl.Instance._isStopedTransaction)
            {
            }
            ElementId PrimaryConduitRunid = null;
            string filePath_SharedParams_path = null;
            string family_folder = System.IO.Path.GetDirectoryName(typeof(Command).Assembly.Location);
            DateTime startDate = DateTime.UtcNow;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            int.TryParse(uiapp.Application.VersionNumber, out int RevitVersion);
            string offsetVariable = RevitVersion < 2020 ? "Offset" : "Middle Elevation";

            List<Element> failingelement = new List<Element>();
            List<Element> secondaryElements = new List<Element>();
            List<Element> thirdElements = new List<Element>();
            bool Refpiuckpoint = false;
            bool fittingsfailure = false;
            string mainTransName = "MultiDraw-Straight/Bend";
            string json = Properties.Settings.Default.ProfileColorSettings;
            ProfileColorSettingsData data = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json);
            try
            {
                using (Transaction trans2 = new Transaction(doc))
                {
                    trans2.Start("SharedParamForMultiDraw");
                    filePath_SharedParams_path = System.IO.Path.Combine(family_folder, "MultiDrawSharedParameter.txt");
                    uiapp.Application.SharedParametersFilename = filePath_SharedParams_path;
                    DefinitionFile defFile = uiapp.Application.OpenSharedParameterFile();
                    FilteredElementCollector pointcollection = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();
                    Element collectedpoint = pointcollection.ToElements().FirstOrDefault() as Element;
                    CategorySet catSet = uiapp.Application.Create.NewCategorySet();
                    CategorySet catSet2 = uiapp.Application.Create.NewCategorySet();
                    Categories categories = doc.Settings.Categories;
                    Category point = categories.get_Item(BuiltInCategory.OST_ConduitFitting);
                    catSet.Insert(point);
                    Category point2 = categories.get_Item(BuiltInCategory.OST_Conduit);
                    catSet.Insert(point2);
                    Autodesk.Revit.DB.Binding binding = uiapp.Application.Create.NewInstanceBinding(catSet);
                    Autodesk.Revit.DB.Binding binding_ALL = uiapp.Application.Create.NewInstanceBinding(catSet);

                    Autodesk.Revit.DB.Binding binding2 = uiapp.Application.Create.NewInstanceBinding(catSet2);
                    Autodesk.Revit.DB.Binding binding_ALL2 = uiapp.Application.Create.NewInstanceBinding(catSet2);
                    if (defFile != null)
                    {
                        foreach (DefinitionGroup dG in defFile.Groups.Where(r => r.Name == "TIG-MultiDraw Parameters"))
                        {
                            foreach (Definition def in dG.Definitions)
                            {
                                if (def.Name != "TIG-Bend Angle")
                                {
                                    BindingMap map = (new UIApplication(uiapp.Application)).ActiveUIDocument.Document.ParameterBindings;
                                    map.Insert(def, binding, def.ParameterGroup); //update code
                                }
                                else
                                {
                                    BindingMap map = (new UIApplication(uiapp.Application)).ActiveUIDocument.Document.ParameterBindings;
                                    map.Insert(def, binding_ALL, def.ParameterGroup);
                                }
                            }
                        }
                    }
                    trans2.Commit();
                }
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
                                panel.CustomPanelTitleBarBackground = gB;
                            }
                        }
                    }
                    transRibbonColorChange_2.Commit();
                }
                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                if (ParentUserControl.Instance.Primaryelst == null)
                {
                    ParentUserControl.Instance.btnPlay.IsChecked = false;
                    return false;
                }
                if (PrimaryConduitRunid == null && ParentUserControl.Instance.Primaryelst != null)
                {
                    PrimaryConduitRunid = (ParentUserControl.Instance.Primaryelst[0] as Conduit).Id;
                }
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
                                                panel.CustomPanelTitleBarBackground = gB;
                                            }
                                        }
                                    }
                                    transRibbonColorChange_2.Commit();
                                }
                                ParentUserControl.Instance._window.Close();
                                return false;
                            }
                            if (ParentUserControl.Instance.Primaryelst.Count() == 0)
                            {
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
                                                panel.CustomPanelTitleBarBackground = gB;
                                            }
                                        }
                                    }
                                    transRibbonColorChange_2.Commit();
                                }
                                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                                PrimaryConduitRunid = (ParentUserControl.Instance.Primaryelst[0] as Conduit).Id;
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
                                                panel.CustomPanelTitleBarBackground = gB;
                                            }
                                        }
                                    }
                                    transRibbonColorChange_2.Commit();
                                }
                                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                                PrimaryConduitRunid = (ParentUserControl.Instance.Primaryelst[0] as Conduit).Id;
                            }
                        }
                    }
                    try
                    {
                        using Transaction tx = new Transaction(doc);
                        tx.Start(mainTransName); 
                        ParentUserControl.Instance._transaction = tx;
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
                                        panel.CustomPanelTitleBarBackground = gB;
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
                        else
                        {
                            if (!ParentUserControl.Instance._isStopedTransaction)
                            {
                            }
                            View activeview = doc.ActiveView;
                            ///coloring the primary conduits
                            List<Element> conduitList = new FilteredElementCollector(doc, activeview.Id).OfClass(typeof(Conduit)).Where(x => (x as Conduit).RunId == (doc.GetElement(PrimaryConduitRunid) as Conduit).RunId).ToList();
                            Element lastconduit = null;
                            foreach (Element item in conduitList)
                            {
                                if ((item as Conduit).ConnectorManager.UnusedConnectors.Size == 1 && item.Id != PrimaryConduitRunid && conduitList.Count() > 1)
                                {
                                    lastconduit = item;
                                }
                            }
                            using (SubTransaction Primarycolorfillsub = new SubTransaction(doc))
                            {
                                Primarycolorfillsub.Start();
                                if (PrimaryConduitRunid == null)
                                {
                                    PrimaryConduitRunid = (ParentUserControl.Instance.Primaryelst[0] as Conduit).RunId;
                                }
                                foreach (Element conduit in ParentUserControl.Instance.Primaryelst)
                                {
                                    if (conduit != null)
                                    {
                                        PrimryConduitcoloroverride(conduit.Id, doc);
                                    }
                                }
                                Primarycolorfillsub.Commit();
                            }
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
                                if (PrimaryConduitRunid == null)
                                {
                                    PrimaryConduitRunid = (ParentUserControl.Instance.Primaryelst[0] as Conduit).RunId;
                                }
                                foreach (Element item in ParentUserControl.Instance.Primaryelst)
                                {
                                    Conduit conduit = item as Conduit;
                                    XYZ pt1 = ((conduit.Location as LocationCurve).Curve as Line).GetEndPoint(0);
                                    XYZ pt2 = ((conduit.Location as LocationCurve).Curve as Line).GetEndPoint(1);
                                    XYZ intersections = Utility.FindIntersectionPoint(Pickpoint, Pickpoint_Two, pt1, pt2);
                                    if (intersections != null)
                                    {
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
                                }
                                ParentUserControl.Instance.AlignConduits.IsChecked = false;
                                subone.Commit();
                            }
                            else
                            {
                                if (k > 0)
                                {
                                }
                                FilteredElementCollector parameterelement = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                ParameterFilterElement refparameterelement = parameterelement.Where(x => x.Name == "H Offset Bends").FirstOrDefault() as ParameterFilterElement;
                                if (refparameterelement == null)
                                {
                                    ViewDrafting viewdrafting = null;
                                    FilteredElementCollector Viewscollections = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting));
                                    viewdrafting = Viewscollections.Where(x => x.Name.Equals("Multi Draw Bends Legends")).FirstOrDefault() as ViewDrafting;
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
                                            ParentUserControl.Instance.Primaryelst.Clear();
                                            ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
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
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 7)
                                {
                                    if (ParentUserControl.Instance.Anglefromprimary.IsChecked == false)
                                    {
                                        if (!APICommon.ThreepointsaddleDrawPointHandler(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, RevitVersion, Pickpoint, ref secondaryElements))
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
                                        if (!APICommon.ThreepointsaddleDrawPointHandler(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, RevitVersion, Pickpoint, ref secondaryElements))
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
                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 8)
                                {
                                    if (ParentUserControl.Instance.Anglefromprimary.IsChecked == false)
                                    {
                                        if (!APICommon.FourpointsaddleDrawPointHandler(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, RevitVersion, Pickpoint, ref secondaryElements))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            if (secondaryElements.Count() != 0)
                                            {
                                                ParentUserControl.Instance.Primaryelst.Clear();
                                                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                            }
                                            else
                                            {
                                                ParentUserControl.Instance.Primaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!APICommon.FourpointsaddleDrawPointHandler(doc, uidoc, uiapp, ParentUserControl.Instance.Primaryelst, offsetVariable, RevitVersion, Pickpoint, ref secondaryElements))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            ParentUserControl.Instance.Secondaryelst.Clear();
                                            ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            if (secondaryElements.Count() != 0)
                                            {
                                                ParentUserControl.Instance.Primaryelst.Clear();
                                                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                                            }
                                            else
                                            {
                                                ParentUserControl.Instance.Primaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                                            }
                                        }
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
                        tx.Commit();
                    }
                    catch
                    {
                        using (Transaction trans = new Transaction(doc, "Reset Conduit Colors"))
                        {
                            trans.Start();
                            try
                            {
                                foreach (Element conduit in ParentUserControl.Instance.Primaryelst)
                                {
                                    doc.ActiveView.SetElementOverrides(conduit.Id, new OverrideGraphicSettings());  //Reset overrides
                                }
                            }
                            catch
                            {
                            }
                            trans.Commit();
                        }
                        if (!ParentUserControl.Instance._isStopedTransaction)
                        {
                            Mainfunction(uiapp);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch 
            {
                ParentUserControl.Instance._window.Close();
                return false;
            }
            return false;
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
        public static void PrimryConduitcoloroverride(ElementId eid, Document doc)
        {
            var patternCollector = new FilteredElementCollector(doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");
            Autodesk.Revit.DB.OverrideGraphicSettings ogs_Hoffset = SetOverrideGraphicSettings(fpe, new Color(50, 205, 50));
            doc.ActiveView.SetElementOverrides(eid, ogs_Hoffset);
        }
        private static ElementFilter CreateElementFilterFromFilterRules(IList<FilterRule> filterRules)
        {
            // We use a LogicalAndFilter containing one ElementParameterFilter
            // for each FilterRule. We could alternatively create a single
            // ElementParameterFilter containing the entire list of FilterRules.
            IList<ElementFilter> elemFilters = new List<ElementFilter>();
            foreach (FilterRule filterRule in filterRules)
            {
                ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);
                elemFilters.Add(elemParamFilter);
            }
            LogicalAndFilter elemFilter = new LogicalAndFilter(elemFilters);
            return elemFilter;
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
            public static bool IsOneSideConnectors(Element e)
            {
                if (e is Conduit conduit)
                {
                    return conduit.ConnectorManager.UnusedConnectors.Size == 1;
                }
                return false;
            }
        }
    }
}



