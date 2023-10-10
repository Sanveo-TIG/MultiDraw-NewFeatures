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
            List<MultiSelect> paramDetails = SettingsUserControl.Instance.ucMultiSelect.SelectedItems.ToList();
            try
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Parameters Settings");
                    try
                    {
                        string json = JsonConvert.SerializeObject(paramDetails);
                        Utility.SetGlobalParametersManager(uiapp, "ParameterSettings", json);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                    tx.Commit();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }
        public string GetName()
        {
            return "Save Settings Handler";
        }
    }
}
