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
               /* if (!isTrue)
                {
                    num++;
                }*/
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
            //Selection sel = uidoc.Selection;
            //ICollection<ElementId> ecol = sel.GetElementIds();

            List<Element> failingelement = new List<Element>();
            List<Element> pickedElements = new List<Element>();
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

                    Category point
              = categories.get_Item(
                BuiltInCategory.OST_ConduitFitting);
                    catSet.Insert(point);

                    Category point2 = categories.get_Item(BuiltInCategory.OST_Conduit);
                    catSet.Insert(point2);

                    Autodesk.Revit.DB.Binding binding = uiapp.Application.Create.NewInstanceBinding(catSet);
                    Autodesk.Revit.DB.Binding binding_ALL = uiapp.Application.Create.NewInstanceBinding(catSet);

                    Autodesk.Revit.DB.Binding binding2 = uiapp.Application.Create.NewInstanceBinding(catSet2);
                    Autodesk.Revit.DB.Binding binding_ALL2 = uiapp.Application.Create.NewInstanceBinding(catSet2);
                    if (defFile!= null)
                    {
                        foreach (DefinitionGroup dG in defFile.Groups.Where(r => r.Name == "TIG-MultiDraw Parameters"))
                        {
                            foreach (Definition def in dG.Definitions)
                            {
                                if (def.Name != "TIG-Bend Angle")
                                {
                                    BindingMap map = (new UIApplication(uiapp.Application)).ActiveUIDocument.Document.ParameterBindings;
                                    map.Insert(def, binding, def.ParameterGroup);

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
                                panel.CustomPanelTitleBarBackground
                        = gB;
                            }

                        }

                    }
                    transRibbonColorChange_2.Commit();
                }

                ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);

                if (PrimaryConduitRunid == null)
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
                                                panel.CustomPanelTitleBarBackground
                                        = gB;
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
                                                panel.CustomPanelTitleBarBackground
                                        = gB;
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
                        //if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 7)
                        //{
                        //    if (!APICommon.ConduitSync(doc, uiapp, ParentUserControl.Instance.Primaryelst))
                        //    {

                        //        break;
                        //    }
                        //    else
                        //    {
                        //        tx.Commit();
                        //        using (SubTransaction transRibbonColorChange_2 = new SubTransaction(doc))
                        //        {
                        //            transRibbonColorChange_2.Start();
                        //            adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
                        //            LinearGradientBrush gB = new LinearGradientBrush
                        //            {
                        //                StartPoint = new System.Windows.Point(0, 0),
                        //                EndPoint = new System.Windows.Point(0, 1)
                        //            };
                        //            gB.GradientStops.Add(new GradientStop(Colors.Orange, 0.0));
                        //            gB.GradientStops.Add(new GradientStop(Colors.Orange, 1));
                        //            foreach (adWin.RibbonTab tab in ribbon.Tabs)
                        //            {
                        //                foreach (adWin.RibbonPanel panel in tab.Panels)
                        //                {
                        //                    if (panel.Source.AutomationName == "MultiDraw")
                        //                    {
                        //                        panel.CustomPanelTitleBarBackground
                        //                = gB;
                        //                    }
                        //                }
                        //            }
                        //            transRibbonColorChange_2.Commit();
                        //        }
                        //        ParentUserControl.Instance.Primaryelst = Utility.GetPickedElements(uidoc, "Select conduits to perform action", typeof(Conduit), true);
                        //        continue;
                        //    }
                        //}
                        else
                        {
                            if (!ParentUserControl.Instance._isStopedTransaction)
                            {

                            }

                            View activeview = doc.ActiveView;
                            //coloring the primary conduits
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

                                int nk = 0;
                                foreach (Element conduit in ParentUserControl.Instance.Primaryelst)
                                {
                                    if (lastconduit != null)
                                    {
                                        if (conduit.Id == lastconduit.Id)
                                        {
                                            PrimryConduitcoloroverride(conduit.Id, doc);
                                        }
                                        else
                                        {
                                            SubConduitcoloroverride(conduit.Id, doc);
                                        }
                                    }
                                    else
                                    {
                                        if (nk == 0)
                                        {
                                            PrimryConduitcoloroverride(conduit.Id, doc);
                                        }
                                        else
                                        {
                                            SubConduitcoloroverride(conduit.Id, doc);
                                        }

                                    }
                                    nk++;
                                }
                                Primarycolorfillsub.Commit();
                            }

                            XYZ Pickpoint = Utility.PickPoint(uidoc);
                            //if (ParentUserControl.Instance._isStopedTransaction)
                            //{
                            //    tx.Commit();
                            //    num = 0;
                            //    return true;
                            //}
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

                               if ( PrimaryConduitRunid == null)
                                {
                                    PrimaryConduitRunid = (ParentUserControl.Instance.Primaryelst[0] as Conduit).RunId;
                                }

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

                               Family fam = null;
                                View vie = fam.Document.ActiveView;


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
                                    //using (SubTransaction substrans2 = new SubTransaction(doc))
                                    //{
                                    //    substrans2.Start();
                                    //    MultiDrawProfileSettingsParam globalParam = new MultiDrawProfileSettingsParam();
                                    //    if (data != null)
                                    //    {
                                    //        globalParam.VoffsetValue = data.VoffsetValue.Text;
                                    //        globalParam.HoffsetValue = data.HoffsetValue.Text;
                                    //        globalParam.RoffsetValue = data.RoffsetValue.Text;
                                    //        globalParam.KoffsetValue = data.KoffsetValue.Text;
                                    //        globalParam.Straight = data.StraightValue.Text;
                                    //        globalParam.NinetyKick = data.NightyKickValue.Text;
                                    //        globalParam.Ninetystub = data.NightystubValue.Text;

                                    //        globalParam.VoffsetValuecolor = data.VoffsetColor.Value == null ? string.Empty : data.VoffsetColor.Value.Red.ToString() + "," + data.VoffsetColor.Value.Green.ToString() + "," + data.VoffsetColor.Value.Blue.ToString();
                                    //        globalParam.HoffsetValuecolor = data.hOffsetColor == null ? string.Empty : data.hOffsetColor.Red.ToString() + "," + data.hOffsetColor.Green.ToString() + "," + data.hOffsetColor.Blue.ToString();
                                    //        globalParam.RoffsetValuecolor = data.rOffsetColor == null ? string.Empty : data.rOffsetColor.Red.ToString() + "," + data.rOffsetColor.Green.ToString() + "," + data.rOffsetColor.Blue.ToString();
                                    //        globalParam.KoffsetValuecolor = data.nkOffsetColor == null ? string.Empty : data.nkOffsetColor.Red.ToString() + "," + data.nkOffsetColor.Green.ToString() + "," + data.nkOffsetColor.Blue.ToString();
                                    //        globalParam.Straightcolor = data.straightColor == null ? string.Empty : data.straightColor.Red.ToString() + "," + data.straightColor.Green.ToString() + "," + data.straightColor.Blue.ToString();
                                    //        globalParam.NinetyKickcolor = data.nkOffsetColor == null ? string.Empty : data.nkOffsetColor.Red.ToString() + "," + data.nkOffsetColor.Green.ToString() + "," + data.nkOffsetColor.Blue.ToString();
                                    //        globalParam.Ninetystubcolor = data.nsOffsetColor == null ? string.Empty : data.nsOffsetColor.Red.ToString() + "," + data.nsOffsetColor.Green.ToString() + "," + data.nsOffsetColor.Blue.ToString();

                                    //        Utility.SetGlobalParametersManager(doc, "MultiDrawProfileSettings", JsonConvert.SerializeObject(globalParam));
                                    //    }
                                    //    substrans2.Commit();

                                    //}



                                    ViewDrafting viewdrafting = null;
                                    FilteredElementCollector Viewscollections = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting));
                                    viewdrafting = Viewscollections.Where(x => x.Name.Equals("Multi Draw Bends Legends")).FirstOrDefault() as ViewDrafting;
                                    //if (viewdrafting == null)
                                    //{

                                    //    using (SubTransaction tranlen = new SubTransaction(doc))
                                    //    {
                                    //        tranlen.Start();
                                    //        FilteredElementCollector pointcollection = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).WhereElementIsElementType();
                                    //        ViewFamilyType collectedpoint = pointcollection.Where(x => (x as ViewFamilyType).ViewFamily == ViewFamily.Drafting).FirstOrDefault() as ViewFamilyType;
                                    //        ElementId viewfamid = collectedpoint.Id;
                                    //        ViewDrafting viewdraft = ViewDrafting.Create(doc, viewfamid);
                                    //        viewdraft.Name = "Multi Draw Bends Legends";
                                    //        //ColorFillLegend.Create(doc, viewdraft.Id);

                                    //        FilteredElementCollector fillRegionTypes = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType));
                                    //        FilledRegionType myPatterns = fillRegionTypes.Where(x => x.Name.Equals("Solid Black")).FirstOrDefault() as FilledRegionType;

                                    //        if (myPatterns == null)
                                    //        {
                                    //            myPatterns = fillRegionTypes.FirstOrDefault() as FilledRegionType;
                                    //        }

                                    //        //FilledRegionType myPatterns = (FilledRegionType)new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType))./*Where(x => x.LookupParameter("Type").AsValueString().Equals("Solid Black"))*/FirstElement();

                                    //        FilledRegionType verticaloffsetpattern = (FilledRegionType)myPatterns.Duplicate("MultiDraw Verftical Offset Bend");
                                    //        verticaloffsetpattern.ForegroundPatternColor = data != null && data.vOffsetColor != null ? data.vOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);
                                    //        verticaloffsetpattern.BackgroundPatternColor = data != null && data.vOffsetColor != null ? data.vOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);

                                    //        List<FilledRegionType> filledtypes = new List<FilledRegionType>();
                                    //        foreach (FilledRegionType frt in fillRegionTypes)
                                    //        {
                                    //            filledtypes.Add(frt);
                                    //        }

                                    //        //Verftical bends
                                    //        FilteredElementCollector textnotecollection = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType));
                                    //        TextNoteType newtextnote = textnotecollection.Where(x => x.Name == "3/4 Arial-MultiDraw").FirstOrDefault() as TextNoteType;
                                    //        if (newtextnote == null)
                                    //        {
                                    //            ElementId defaultTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                                    //            TextNoteType textnote = textnotecollection.FirstOrDefault() as TextNoteType;
                                    //            newtextnote = (TextNoteType)textnote.Duplicate("3/4 Arial-MultiDraw");
                                    //        }

                                    //        Autodesk.Revit.DB.Parameter textnoteparam = newtextnote.LookupParameter("Text Size");
                                    //        textnoteparam.Set(0.0625);

                                    //        XYZ txtposition1 = new XYZ(7, -0.75, 0);
                                    //        TextNote note1 = TextNote.Create(doc, viewdraft.Id, txtposition1, "Vertical Offset Bend", newtextnote.Id);
                                    //        note1.HorizontalAlignment = HorizontalTextAlignment.Left;
                                    //        note1.Width = 1.5;

                                    //        List<CurveLoop> profileloops = new List<CurveLoop>();
                                    //        XYZ[] points = new XYZ[5];
                                    //        points[0] = new XYZ(0.0, 0.0, 0.0);
                                    //        points[1] = new XYZ(5, 0.0, 0.0);
                                    //        points[2] = new XYZ(5, -1.5, 0.0);
                                    //        points[3] = new XYZ(0.0, -1.5, 0.0);
                                    //        points[4] = new XYZ(0.0, 0.0, 0.0);
                                    //        CurveLoop profileloop = new CurveLoop();
                                    //        for (int i = 0; i < 4; i++)
                                    //        {
                                    //            Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(points[i],
                                    //              points[i + 1]);

                                    //            profileloop.Append(line);
                                    //        }
                                    //        profileloops.Add(profileloop);

                                    //        ElementId activeViewId = doc.ActiveView.Id;

                                    //        FilledRegion filledRegions = FilledRegion.Create(
                                    //          doc, verticaloffsetpattern.Id, viewdraft.Id, profileloops);

                                    //        FilledRegionType Horizontaloffsetpattern = (FilledRegionType)myPatterns.Duplicate("MultiDraw Horizontal Offset Bend");
                                    //        Horizontaloffsetpattern.ForegroundPatternColor = data != null && data.hOffsetColor != null ? data.hOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);
                                    //        Horizontaloffsetpattern.BackgroundPatternColor = data != null && data.hOffsetColor != null ? data.hOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);

                                    //        //horizontal bends
                                    //        XYZ txtposition2 = new XYZ(7, -3.75, 0);
                                    //        TextNote note2 = TextNote.Create(doc, viewdraft.Id, txtposition2, "Horizontal offset Bend", newtextnote.Id);
                                    //        note2.HorizontalAlignment = HorizontalTextAlignment.Left;
                                    //        note2.Width = 1.5;

                                    //        List<CurveLoop> hor_profileloops = new List<CurveLoop>();
                                    //        XYZ[] Hor_points = new XYZ[5];
                                    //        Hor_points[0] = new XYZ(0.0, -3, 0.0);
                                    //        Hor_points[1] = new XYZ(5, -3, 0.0);
                                    //        Hor_points[2] = new XYZ(5, -4.5, 0.0);
                                    //        Hor_points[3] = new XYZ(0.0, -4.5, 0.0);
                                    //        Hor_points[4] = new XYZ(0.0, -3, 0.0);
                                    //        CurveLoop hor_profileloop = new CurveLoop();
                                    //        for (int i = 0; i < 4; i++)
                                    //        {
                                    //            Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(Hor_points[i],
                                    //              Hor_points[i + 1]);

                                    //            hor_profileloop.Append(line);
                                    //        }
                                    //        hor_profileloops.Add(hor_profileloop);

                                    //        FilledRegion hor_filledRegions = FilledRegion.Create(
                                    //          doc, Horizontaloffsetpattern.Id, viewdraft.Id, hor_profileloops);

                                    //        FilledRegionType rollingoffsetpattern = (FilledRegionType)myPatterns.Duplicate("MultiDraw Rolling Offset Bend");
                                    //        rollingoffsetpattern.ForegroundPatternColor = data != null && data.rOffsetColor != null ? data.rOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);
                                    //        rollingoffsetpattern.BackgroundPatternColor = data != null && data.rOffsetColor != null ? data.rOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);

                                    //        //rolling bends
                                    //        XYZ txtposition3 = new XYZ(7, -6.75, 0);
                                    //        TextNote note3 = TextNote.Create(doc, viewdraft.Id, txtposition3, "Rolling Offset Bend", newtextnote.Id);
                                    //        note3.HorizontalAlignment = HorizontalTextAlignment.Left;
                                    //        note3.Width = 1.5;

                                    //        List<CurveLoop> roll_profileloops = new List<CurveLoop>();
                                    //        XYZ[] roll_points = new XYZ[5];
                                    //        roll_points[0] = new XYZ(0.0, -6, 0.0);
                                    //        roll_points[1] = new XYZ(5, -6, 0.0);
                                    //        roll_points[2] = new XYZ(5, -7.5, 0.0);
                                    //        roll_points[3] = new XYZ(0.0, -7.5, 0.0);
                                    //        roll_points[4] = new XYZ(0.0, -6, 0.0);
                                    //        CurveLoop roll_profileloop = new CurveLoop();
                                    //        for (int i = 0; i < 4; i++)
                                    //        {
                                    //            Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(roll_points[i],
                                    //              roll_points[i + 1]);

                                    //            roll_profileloop.Append(line);
                                    //        }
                                    //        roll_profileloops.Add(roll_profileloop);

                                    //        FilledRegion roll_filledRegions = FilledRegion.Create(
                                    //          doc, rollingoffsetpattern.Id, viewdraft.Id, roll_profileloops);


                                    //        FilledRegionType Kick90offsetpattern = (FilledRegionType)myPatterns.Duplicate("MultiDraw Kick 90 Bend");
                                    //        Kick90offsetpattern.ForegroundPatternColor = data != null && data.kOffsetColor != null ? data.kOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);
                                    //        Kick90offsetpattern.BackgroundPatternColor = data != null && data.kOffsetColor != null ? data.kOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);

                                    //        //kick90 bneds 
                                    //        XYZ txtposition4 = new XYZ(7, -9.75, 0);
                                    //        TextNote note4 = TextNote.Create(doc, viewdraft.Id, txtposition4, "Kick 90 Bend", newtextnote.Id);
                                    //        note4.HorizontalAlignment = HorizontalTextAlignment.Left;
                                    //        note4.Width = 1.5;

                                    //        List<CurveLoop> kick_profileloops = new List<CurveLoop>();
                                    //        XYZ[] kick_points = new XYZ[5];
                                    //        kick_points[0] = new XYZ(0.0, -9, 0.0);
                                    //        kick_points[1] = new XYZ(5, -9, 0.0);
                                    //        kick_points[2] = new XYZ(5, -10.5, 0.0);
                                    //        kick_points[3] = new XYZ(0.0, -10.5, 0.0);
                                    //        kick_points[4] = new XYZ(0.0, -9, 0.0);
                                    //        CurveLoop kick_profileloop = new CurveLoop();
                                    //        for (int i = 0; i < 4; i++)
                                    //        {
                                    //            Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(kick_points[i],
                                    //              kick_points[i + 1]);

                                    //            kick_profileloop.Append(line);
                                    //        }
                                    //        kick_profileloops.Add(kick_profileloop);

                                    //        FilledRegion kick_filledRegions = FilledRegion.Create(
                                    //          doc, Kick90offsetpattern.Id, viewdraft.Id, kick_profileloops);

                                    //        //FilledRegionType newFilledregionType = (FilledRegionType)newType;


                                    //        FilledRegionType Straightpattern = (FilledRegionType)myPatterns.Duplicate("MultiDraw Half Offset");
                                    //        Straightpattern.ForegroundPatternColor = data != null && data.straightColor != null ? data.straightColor : new Autodesk.Revit.DB.Color(255, 0, 0);
                                    //        Straightpattern.BackgroundPatternColor = data != null && data.straightColor != null ? data.straightColor : new Autodesk.Revit.DB.Color(255, 0, 0);

                                    //        //kick90 bneds 
                                    //        XYZ txtposition5 = new XYZ(7, -12.75, 0);
                                    //        TextNote note5 = TextNote.Create(doc, viewdraft.Id, txtposition5, "Half Offset", newtextnote.Id);
                                    //        note5.HorizontalAlignment = HorizontalTextAlignment.Left;
                                    //        note5.Width = 1.5;

                                    //        List<CurveLoop> straight_profileloops = new List<CurveLoop>();
                                    //        XYZ[] straights = new XYZ[5];
                                    //        straights[0] = new XYZ(0.0, -12, 0.0);
                                    //        straights[1] = new XYZ(5, -12, 0.0);
                                    //        straights[2] = new XYZ(5, -13.5, 0.0);
                                    //        straights[3] = new XYZ(0.0, -13.5, 0.0);
                                    //        straights[4] = new XYZ(0.0, -12, 0.0);
                                    //        CurveLoop straight_profileloop = new CurveLoop();
                                    //        for (int i = 0; i < 4; i++)
                                    //        {
                                    //            Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(straights[i],
                                    //              straights[i + 1]);

                                    //            straight_profileloop.Append(line);
                                    //        }
                                    //        straight_profileloops.Add(straight_profileloop);

                                    //        FilledRegion Straight_filledRegions = FilledRegion.Create(
                                    //          doc, Straightpattern.Id, viewdraft.Id, straight_profileloops);

                                    //        FilledRegionType Ninetykickpattern = (FilledRegionType)myPatterns.Duplicate("MultiDraw NinetyKick");
                                    //        Ninetykickpattern.ForegroundPatternColor = data != null && data.nkOffsetColor != null ? data.nkOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);
                                    //        Ninetykickpattern.BackgroundPatternColor = data != null && data.nkOffsetColor != null ? data.nkOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);

                                    //        //kick90 bneds 
                                    //        XYZ txtposition6 = new XYZ(7, -15.75, 0);
                                    //        TextNote note6 = TextNote.Create(doc, viewdraft.Id, txtposition6, "Ninety Kick", newtextnote.Id);
                                    //        note6.HorizontalAlignment = HorizontalTextAlignment.Left;
                                    //        note6.Width = 1.5;

                                    //        List<CurveLoop> Ninetykick_profileloops = new List<CurveLoop>();
                                    //        XYZ[] Ninetykick = new XYZ[5];
                                    //        Ninetykick[0] = new XYZ(0.0, -15, 0.0);
                                    //        Ninetykick[1] = new XYZ(5, -15, 0.0);
                                    //        Ninetykick[2] = new XYZ(5, -16.5, 0.0);
                                    //        Ninetykick[3] = new XYZ(0.0, -16.5, 0.0);
                                    //        Ninetykick[4] = new XYZ(0.0, -15, 0.0);
                                    //        CurveLoop Ninetykick_profileloop = new CurveLoop();
                                    //        for (int i = 0; i < 4; i++)
                                    //        {
                                    //            Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(Ninetykick[i],
                                    //              Ninetykick[i + 1]);

                                    //            Ninetykick_profileloop.Append(line);
                                    //        }
                                    //        Ninetykick_profileloops.Add(Ninetykick_profileloop);

                                    //        FilledRegion Ninetykick_filledRegions = FilledRegion.Create(
                                    //          doc, Ninetykickpattern.Id, viewdraft.Id, Ninetykick_profileloops);

                                    //        FilledRegionType NinetyStubpattern = (FilledRegionType)myPatterns.Duplicate("MultiDraw NinetyStub");
                                    //        NinetyStubpattern.ForegroundPatternColor = data != null && data.nsOffsetColor != null ? data.nsOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);
                                    //        NinetyStubpattern.BackgroundPatternColor = data != null && data.nsOffsetColor != null ? data.nsOffsetColor : new Autodesk.Revit.DB.Color(255, 0, 0);

                                    //        //kick90 bneds 
                                    //        XYZ txtposition7 = new XYZ(7, -18.75, 0);
                                    //        TextNote note7 = TextNote.Create(doc, viewdraft.Id, txtposition7, "Ninety Stub", newtextnote.Id);
                                    //        note7.HorizontalAlignment = HorizontalTextAlignment.Left;
                                    //        note7.Width = 1.5;

                                    //        List<CurveLoop> NinetyStub_profileloops = new List<CurveLoop>();
                                    //        XYZ[] NinetyStub = new XYZ[5];
                                    //        NinetyStub[0] = new XYZ(0.0, -18, 0.0);
                                    //        NinetyStub[1] = new XYZ(5, -18, 0.0);
                                    //        NinetyStub[2] = new XYZ(5, -19.5, 0.0);
                                    //        NinetyStub[3] = new XYZ(0.0, -19.5, 0.0);
                                    //        NinetyStub[4] = new XYZ(0.0, -18, 0.0);
                                    //        CurveLoop NinetyStub_profileloop = new CurveLoop();
                                    //        for (int i = 0; i < 4; i++)
                                    //        {
                                    //            Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(NinetyStub[i],
                                    //              NinetyStub[i + 1]);

                                    //            NinetyStub_profileloop.Append(line);
                                    //        }
                                    //        NinetyStub_profileloops.Add(NinetyStub_profileloop);

                                    //        FilledRegion NinetyStub_filledRegions = FilledRegion.Create(
                                    //          doc, NinetyStubpattern.Id, viewdraft.Id, NinetyStub_profileloops);





                                    //        tranlen.Commit();
                                    //    }
                                    //}
                                }
                                //using (SubTransaction transfilter = new SubTransaction(doc))
                                //{
                                //    transfilter.Start();
                                //    FilteredElementCollector fittingscollections = new FilteredElementCollector(doc).OfClass(typeof(Conduit));
                                //    Conduit reffittungs = fittingscollections.OfCategory(BuiltInCategory.OST_Conduit).FirstElement() as Conduit;
                                //    Element refelement = reffittungs as Element;
                                //    Parameter param = refelement.GetOrderedParameters().Where(x => x.Definition.Name.Equals("TIG-Bend Type")).FirstOrDefault();
                                //    ElementId paramid = param.Id;

                                //    //ElementId templateId = template.Id;
                                //    List<ElementId> categories = new List<ElementId>();
                                //    categories.Add(new ElementId(BuiltInCategory.OST_ConduitFitting));
                                //    categories.Add(new ElementId(BuiltInCategory.OST_Conduit));
                                //    List<FilterRule> filterRules_hoffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_voffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_roffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_koffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_straight = new List<FilterRule>();
                                //    List<FilterRule> filterRules_ninetykick = new List<FilterRule>();
                                //    List<FilterRule> filterRules_ninetystub = new List<FilterRule>();

                                //    FilteredElementCollector fittingscollections_filters = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement filtersref = fittingscollections_filters.Where(x => x.Name.Equals("TIG-H Offset Bends")).FirstOrDefault() as ParameterFilterElement;

                                //    ParameterFilterElement parameterFilterElement = null;
                                //    ParameterFilterElement parameterFilterElement_voffset = null;
                                //    ParameterFilterElement parameterFilterElement_Roffset = null;
                                //    ParameterFilterElement parameterFilterElement_kick90 = null;
                                //    ParameterFilterElement parameterFilterElement_Straight = null;
                                //    ParameterFilterElement parameterFilterElement_NinetyKick = null;
                                //    ParameterFilterElement parameterFilterElement_Ninetystub = null;
                                //    string HoffsetValue = string.Empty;
                                //    string VoffsetValue = string.Empty;
                                //    string RoffsetValue = string.Empty;
                                //    string KoffsetValue = string.Empty;
                                //    string StraightValue = string.Empty;
                                //    string NightyKickValue = string.Empty;
                                //    string NightystubValue = string.Empty;


                                //    Autodesk.Revit.DB.Color VoffsetColor = new Autodesk.Revit.DB.Color(255, 255, 0);
                                //    Autodesk.Revit.DB.Color HoffsetColor = new Autodesk.Revit.DB.Color(128, 128, 128);
                                //    Autodesk.Revit.DB.Color RoffsetColor = new Autodesk.Revit.DB.Color(255, 153, 255);
                                //    Autodesk.Revit.DB.Color Kick90offsetColor = new Autodesk.Revit.DB.Color(204, 229, 255);
                                //    Autodesk.Revit.DB.Color Strightor90Color = new Autodesk.Revit.DB.Color(255, 153, 204);
                                //    Autodesk.Revit.DB.Color NinetykickdrawColor = new Autodesk.Revit.DB.Color(255, 0, 0);
                                //    Autodesk.Revit.DB.Color ninetystubColor = new Autodesk.Revit.DB.Color(153, 76, 0);
                                //    if (data == null)
                                //    {
                                //        HoffsetValue = "V offset";
                                //        VoffsetValue = "H offset";
                                //        RoffsetValue = "R offset";
                                //        KoffsetValue = "K offset";
                                //        StraightValue = "Straight/Bend";
                                //        NightyKickValue = "NinetyKick";
                                //        NightystubValue = "NinetyStub";
                                //    }
                                //    else
                                //    {
                                //        HoffsetValue = data.hOffsetValue;
                                //        VoffsetValue = data.vOffsetValue;
                                //        RoffsetValue = data.rOffsetValue;
                                //        KoffsetValue = data.kOffsetValue;
                                //        StraightValue = data.straightValue;
                                //        NightyKickValue = data.nkOffsetValue;
                                //        NightystubValue = data.nsOffsetValue;
                                //        HoffsetColor = data.hOffsetColor;
                                //        VoffsetColor = data.vOffsetColor;
                                //        RoffsetColor = data.rOffsetColor;
                                //        Kick90offsetColor = data.kOffsetColor;
                                //        Strightor90Color = data.straightColor;
                                //        NinetykickdrawColor = data.nkOffsetColor;
                                //        ninetystubColor = data.nsOffsetColor;
                                //    }
                                //    if (filtersref == null)
                                //    {
                                //        // Create filter element assocated to the input categories
                                //        parameterFilterElement = ParameterFilterElement.Create(doc, "TIG-H Offset Bends", categories);

                                //        //v offset 
                                //        parameterFilterElement_voffset = ParameterFilterElement.Create(doc, "TIG-V Offset Bends", categories);

                                //        //R offset 
                                //        parameterFilterElement_Roffset = ParameterFilterElement.Create(doc, "TIG-R Offset Bends", categories);

                                //        //Kick 90 
                                //        parameterFilterElement_kick90 = ParameterFilterElement.Create(doc, "TIG-Kick with 90 Bends", categories);

                                //        //Straight 
                                //        parameterFilterElement_Straight = ParameterFilterElement.Create(doc, "TIG-Straight Profiles", categories);

                                //        //Straight 
                                //        parameterFilterElement_NinetyKick = ParameterFilterElement.Create(doc, "TIG-NinetyKickBends", categories);

                                //        //90stub
                                //        parameterFilterElement_Ninetystub = ParameterFilterElement.Create(doc, "TIG-NinetystubBends", categories);

                                //        // Criterion 1 - wall type Function is "Exterior"

                                //        filterRules_hoffset.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramid, HoffsetValue, false));
                                //        ElementFilter elemFilter = CreateElementFilterFromFilterRules(filterRules_hoffset);
                                //        parameterFilterElement.SetElementFilter(elemFilter);

                                //        filterRules_voffset.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramid, VoffsetValue, false));
                                //        ElementFilter elemFilter_voffset = CreateElementFilterFromFilterRules(filterRules_voffset);
                                //        parameterFilterElement_voffset.SetElementFilter(elemFilter_voffset);

                                //        filterRules_roffset.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramid, RoffsetValue, false));
                                //        ElementFilter elemFilter_roffset = CreateElementFilterFromFilterRules(filterRules_roffset);
                                //        parameterFilterElement_Roffset.SetElementFilter(elemFilter_roffset);

                                //        filterRules_koffset.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramid, KoffsetValue, false));
                                //        ElementFilter elemFilter_koffset = CreateElementFilterFromFilterRules(filterRules_koffset);
                                //        parameterFilterElement_kick90.SetElementFilter(elemFilter_koffset);

                                //        filterRules_straight.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramid, StraightValue, false));
                                //        ElementFilter elemFilter_straight = CreateElementFilterFromFilterRules(filterRules_straight);
                                //        parameterFilterElement_Straight.SetElementFilter(elemFilter_straight);

                                //        filterRules_ninetykick.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramid, NightyKickValue, false));
                                //        ElementFilter elemFilter_ninetykick = CreateElementFilterFromFilterRules(filterRules_ninetykick);
                                //        parameterFilterElement_NinetyKick.SetElementFilter(elemFilter_ninetykick);

                                //        filterRules_ninetystub.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramid, NightystubValue, false));
                                //        ElementFilter elemFilter_ninetystub = CreateElementFilterFromFilterRules(filterRules_ninetystub);
                                //        parameterFilterElement_Ninetystub.SetElementFilter(elemFilter_ninetystub);
                                //    }
                                //    else
                                //    {
                                //        parameterFilterElement = fittingscollections_filters.Where(x => x.Name.Equals("TIG-H Offset Bends")).FirstOrDefault() as ParameterFilterElement;
                                //        parameterFilterElement_voffset = fittingscollections_filters.Where(x => x.Name.Equals("TIG-V Offset Bends")).FirstOrDefault() as ParameterFilterElement;
                                //        parameterFilterElement_Roffset = fittingscollections_filters.Where(x => x.Name.Equals("TIG-R Offset Bends")).FirstOrDefault() as ParameterFilterElement;
                                //        parameterFilterElement_kick90 = fittingscollections_filters.Where(x => x.Name.Equals("TIG-Kick with 90 Bends")).FirstOrDefault() as ParameterFilterElement;
                                //        parameterFilterElement_Straight = fittingscollections_filters.Where(x => x.Name.Equals("TIG-Straight Profiles")).FirstOrDefault() as ParameterFilterElement;
                                //        parameterFilterElement_NinetyKick = fittingscollections_filters.Where(x => x.Name.Equals("TIG-NinetyKickBends")).FirstOrDefault() as ParameterFilterElement;
                                //        parameterFilterElement_Ninetystub = fittingscollections_filters.Where(x => x.Name.Equals("TIG-NinetystubBends")).FirstOrDefault() as ParameterFilterElement;

                                //    }

                                //    var patternCollector = new FilteredElementCollector(doc);
                                //    patternCollector.OfClass(typeof(FillPatternElement));
                                //    FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");


                                //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Hoffset = SetOverrideGraphicSettings(fpe, HoffsetColor);
                                //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Voffset = SetOverrideGraphicSettings(fpe, VoffsetColor);
                                //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Roffset = SetOverrideGraphicSettings(fpe, RoffsetColor);
                                //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Koffset = SetOverrideGraphicSettings(fpe, Kick90offsetColor);
                                //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Straight = SetOverrideGraphicSettings(fpe, Strightor90Color);
                                //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_NinetyKick = SetOverrideGraphicSettings(fpe, NinetykickdrawColor);
                                //    Autodesk.Revit.DB.OverrideGraphicSettings ogs_Ninetystub = SetOverrideGraphicSettings(fpe, ninetystubColor);




                                //    //        IEnumerable<View> views = new FilteredElementCollector(doc)
                                //    //.OfClass(typeof(View))
                                //    //.Cast<View>()
                                //    //.Where(v => v.Name.Equals("Multi Draw View"));
                                //    View template = null;

                                //    template = doc.GetElement(doc.ActiveView.ViewTemplateId) as View;
                                //    if (template == null)
                                //    {
                                //        template = doc.ActiveView;
                                //    }

                                //    //FilteredElementCollector fittingscollections_filters_active = new FilteredElementCollector(doc,doc.ActiveView.Id).OfClass(typeof(ParameterFilterElement));
                                //    ICollection<ElementId> filtersref_active = template.GetFilters();
                                //    List<ParameterFilterElement> filtersref_ele = new List<ParameterFilterElement>();
                                //    foreach (ElementId item in filtersref_active)
                                //    {
                                //        filtersref_ele.Add(doc.GetElement(item) as ParameterFilterElement);
                                //    }
                                //    ParameterFilterElement filtersref_active_ref = filtersref_ele.Where(x => x.Name.Equals("TIG-H Offset Bends")).FirstOrDefault();
                                //    if (filtersref_active_ref == null)
                                //    {
                                //        template.AddFilter(parameterFilterElement.Id);
                                //        OverrideGraphicSettings overrideSettings = template.GetFilterOverrides(parameterFilterElement.Id);
                                //        template.SetFilterOverrides(parameterFilterElement.Id, ogs_Hoffset);

                                //        template.AddFilter(parameterFilterElement_voffset.Id);
                                //        OverrideGraphicSettings overrideSettings_voffset = template.GetFilterOverrides(parameterFilterElement_voffset.Id);
                                //        template.SetFilterOverrides(parameterFilterElement_voffset.Id, ogs_Voffset);

                                //        template.AddFilter(parameterFilterElement_Roffset.Id);
                                //        OverrideGraphicSettings overrideSettings_Roffset = template.GetFilterOverrides(parameterFilterElement_Roffset.Id);
                                //        template.SetFilterOverrides(parameterFilterElement_Roffset.Id, ogs_Roffset);

                                //        template.AddFilter(parameterFilterElement_kick90.Id);
                                //        OverrideGraphicSettings overrideSettings_Koffset = template.GetFilterOverrides(parameterFilterElement_kick90.Id);
                                //        template.SetFilterOverrides(parameterFilterElement_kick90.Id, ogs_Koffset);

                                //        template.AddFilter(parameterFilterElement_Straight.Id);
                                //        OverrideGraphicSettings overrideSettings_Straight = template.GetFilterOverrides(parameterFilterElement_Straight.Id);
                                //        template.SetFilterOverrides(parameterFilterElement_Straight.Id, ogs_Straight);

                                //        template.AddFilter(parameterFilterElement_NinetyKick.Id);
                                //        OverrideGraphicSettings overrideSettings_Ninetykick = template.GetFilterOverrides(parameterFilterElement_NinetyKick.Id);
                                //        template.SetFilterOverrides(parameterFilterElement_NinetyKick.Id, ogs_NinetyKick);

                                //        template.AddFilter(parameterFilterElement_Ninetystub.Id);
                                //        OverrideGraphicSettings overrideSettings_Ninetystub = template.GetFilterOverrides(parameterFilterElement_Ninetystub.Id);
                                //        template.SetFilterOverrides(parameterFilterElement_Ninetystub.Id, ogs_Ninetystub);
                                //    }




                                //    transfilter.Commit();
                                //}
                                //using (SubTransaction transfilter = new SubTransaction(doc))
                                //{
                                //    transfilter.Start();
                                //    FilteredElementCollector fittingscollections = new FilteredElementCollector(doc).OfClass(typeof(Conduit));
                                //    Element refelement = fittingscollections.OfCategory(BuiltInCategory.OST_Conduit).FirstElement();
                                //    Parameter param = refelement.GetOrderedParameters().Where(x => x.Definition.Name.Equals("TIG-Bend Type")).FirstOrDefault();
                                //    ElementId paramid = param.Id;

                                //    //ElementId templateId = template.Id;
                                //    List<ElementId> categories = new List<ElementId>
                                //        {
                                //            new ElementId(BuiltInCategory.OST_ConduitFitting),
                                //            new ElementId(BuiltInCategory.OST_Conduit)
                                //        };
                                //    List<FilterRule> filterRules_hoffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_voffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_roffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_koffset = new List<FilterRule>();
                                //    List<FilterRule> filterRules_straight = new List<FilterRule>();
                                //    List<FilterRule> filterRules_ninetykick = new List<FilterRule>();
                                //    List<FilterRule> filterRules_ninetystub = new List<FilterRule>();

                                //    // Create filter element assocated to the input categories
                                //    FilteredElementCollector paramfilterhorcol = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement parameterFilterElement = paramfilterhorcol.Where(x => x.Name.Equals("H Offset Bends")).FirstOrDefault() as ParameterFilterElement;
                                //    if (parameterFilterElement == null)
                                //    {
                                //        parameterFilterElement = ParameterFilterElement.Create(doc, "H Offset Bends", categories);
                                //    }


                                //    //v offset 
                                //    FilteredElementCollector paramfiltervercol = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement parameterFilterElement_voffset = paramfiltervercol.Where(x => x.Name.Equals("V Offset Bends")).FirstOrDefault() as ParameterFilterElement;
                                //    if (parameterFilterElement_voffset == null)
                                //    {
                                //        parameterFilterElement_voffset = ParameterFilterElement.Create(doc, "V Offset Bends", categories);
                                //    }


                                //    //R offset 
                                //    FilteredElementCollector paramfilterrollcol = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement parameterFilterElement_Roffset = paramfilterrollcol.Where(x => x.Name.Equals("R Offset Bends")).FirstOrDefault() as ParameterFilterElement;
                                //    if (parameterFilterElement_Roffset == null)
                                //    {
                                //        parameterFilterElement_Roffset = ParameterFilterElement.Create(doc, "R Offset Bends", categories);
                                //    }


                                //    //Kick 90 
                                //    FilteredElementCollector paramfilterkickcol = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement parameterFilterElement_kick90 = paramfilterkickcol.Where(x => x.Name.Equals("Kick with 90 Bends")).FirstOrDefault() as ParameterFilterElement;
                                //    if (parameterFilterElement_kick90 == null)
                                //    {
                                //        parameterFilterElement_kick90 = ParameterFilterElement.Create(doc, "Kick with 90 Bends", categories);
                                //    }


                                //    //Straight 
                                //    FilteredElementCollector paramfilterstraightcol = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement parameterFilterElement_Straight = paramfilterstraightcol.Where(x => x.Name.Equals("Straight Profiles")).FirstOrDefault() as ParameterFilterElement;
                                //    if (parameterFilterElement_Straight == null)
                                //    {
                                //        parameterFilterElement_Straight = ParameterFilterElement.Create(doc, "Straight Profiles", categories);
                                //    }


                                //    //Straight 
                                //    FilteredElementCollector paramfilternikickcol = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement parameterFilterElement_NinetyKick = paramfilternikickcol.Where(x => x.Name.Equals("NinetyKickBends")).FirstOrDefault() as ParameterFilterElement;
                                //    if (parameterFilterElement_NinetyKick == null)
                                //    {
                                //        parameterFilterElement_NinetyKick = ParameterFilterElement.Create(doc, "NinetyKickBends", categories);
                                //    }


                                //    //90stub
                                //    bool filtersexist = true;
                                //    FilteredElementCollector paramfilternicol = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
                                //    ParameterFilterElement parameterFilterElement_Ninetystub = paramfilternicol.Where(x => x.Name.Equals("NinetystubBends")).FirstOrDefault() as ParameterFilterElement;
                                //    if (parameterFilterElement_Ninetystub == null)
                                //    {
                                //        filtersexist = false;
                                //        parameterFilterElement_Ninetystub = ParameterFilterElement.Create(doc, "NinetystubBends", categories);
                                //    }
                                //}
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

                                else if (ParentUserControl.Instance.cmbProfileType.SelectedIndex == 7)
                                {
                                    //if (ParentUserControl.Instance.Anglefromprimary.IsChecked == true)
                                    //{
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
                                    //if (ParentUserControl.Instance.Anglefromprimary.IsChecked == true)
                                    //{
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
                        if (!ParentUserControl.Instance._isStopedTransaction)
                        {
                            Mainfunction(uiapp);
                            break;
                        }
                        else
                        {
                            //ParentUserControl.Instance._window.Close();
                            break;
                        }

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

        public static void SubConduitcoloroverride(ElementId eid, Document doc)
        {
            var patternCollector = new FilteredElementCollector(doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");
            Autodesk.Revit.DB.OverrideGraphicSettings ogs_Hoffset = SetOverrideGraphicSettings(fpe, new Autodesk.Revit.DB.Color(50, 205, 50));
            doc.ActiveView.SetElementOverrides(eid, ogs_Hoffset);
        }
        public static void PrimryConduitcoloroverride(ElementId eid, Document doc)
        {
            var patternCollector = new FilteredElementCollector(doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");
            Autodesk.Revit.DB.OverrideGraphicSettings ogs_Hoffset = SetOverrideGraphicSettings(fpe, new Color(1, 50, 32));
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
