using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
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
    [Transaction(TransactionMode.Manual)]
    public class ConduitSyncHandler : IExternalEventHandler
    {
        List<MultiSelect> _selectedSyncDataList = new List<MultiSelect>();
        public void Execute(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<Element> elements = new List<Element>();
            DateTime startDate = DateTime.UtcNow;
            try
            {
                elements = Utility.GetPickedElements(uidoc, "Select the conduit route", typeof(Conduit), true);
                if (elements == null)
                {
                    SettingsUserControl.Instance.btnsync.IsEnabled = true;
                    return;
                }


                List<MultiSelect> SelectedParameters = SettingsUserControl.Instance.ucMultiSelect.SelectedItems;
                MultiSelect multiselect = SettingsUserControl.Instance.ucMultiSelect.SelectedItems[0];
                if (SelectedParameters != null)
                    _selectedSyncDataList = SettingsUserControl.Instance.ucMultiSelect.SelectedItems.ToList().Where(x => x.Name != "All" && x.IsChecked).ToList();
                using Transaction tx = new Transaction(doc);
                tx.Start("Sync Data");

                if (SettingsUserControl.Instance.ucMultiSelect.ItemsSource is List<MultiSelect> selectitem)
                {
                    List<SYNCDataGlobalParam> syncdata = new List<SYNCDataGlobalParam>();
                    syncdata = selectitem.Where(r => r.IsChecked).Select(x => new SYNCDataGlobalParam { Name = x.Name }).ToList();
                    string json = JsonConvert.SerializeObject(syncdata);
                    Utility.SetGlobalParametersManager(uiapp, "SyncDataParameters", json);
                }
                foreach (Element item in elements)
                {
                    List<Element> lstElements = new List<Element>();
                    Utility.ConduitSelection(doc, item as Conduit, null, ref lstElements, true);
                    ApplyParameters(doc, item as Conduit, lstElements);
                }
                if (elements.Count > 0)
                {
                    uidoc.Selection.SetElementIds(elements.Select(r => r.Id).ToList());
                }
                tx.Commit();
                //Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Completed", "Sync Data", Util.ProductVersion, "Sync Data");
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Warning. \n" + exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                //Task task = Utility.UserActivityLog(System.Reflection.Assembly.GetExecutingAssembly(), uiapp, Util.ApplicationWindowTitle, startDate, "Failed", "Sync Data", Util.ProductVersion, "Sync Data");

            }
            SettingsUserControl.Instance.btnsync.IsEnabled = true;

        }
        private void ApplyParameters(Document doc, Conduit eleConduit, List<Element> elements)
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


        }

        public string GetName()
        {
            return "Save Settings Handler";
        }
    }
}
