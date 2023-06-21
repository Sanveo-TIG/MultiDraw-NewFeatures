using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIGUtility;

namespace MultiDraw
{
    [Transaction(TransactionMode.Manual)]
    public class SettingsHandler : IExternalEventHandler
    {
        public void Execute(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            try
            {
                using Transaction tx = new Transaction(doc);
                tx.Start("MultiDraw Settings");
                Settings settings = ParentUserControl.Instance.GetSettings();
                string json = JsonConvert.SerializeObject(settings);
                Properties.Settings.Default.MultiDrawSettings = json;
                Properties.Settings.Default.Save();
                tx.Commit();
                ParentUserControl.Instance.MultiDrawSettings = settings;
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
