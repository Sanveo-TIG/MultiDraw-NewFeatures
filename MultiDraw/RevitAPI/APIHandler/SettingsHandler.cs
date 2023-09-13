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
                Settings settings = GetSettings();
                string json = JsonConvert.SerializeObject(settings);
                Utility.SetGlobalParametersManager(uiapp, "MultiDrawSettings", json);
                Properties.Settings.Default.MultiDrawSettings = json;
                Properties.Settings.Default.Save();
                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public Settings GetSettings()
        {
            if (SettingsUserControl.Instance != null)
            {
                Settings settings = new Settings
                {
                    IsSupportNeeded = (bool)SettingsUserControl.Instance.IsSupportNeeded.IsChecked,
                    StrutType = SettingsUserControl.Instance.ddlStrutType.SelectedItem != null ? SettingsUserControl.Instance.ddlStrutType.SelectedItem.Name : string.Empty,
                    RodDiaAsDouble = SettingsUserControl.Instance.txtRodDia.AsDouble,
                    RodDiaAsString = SettingsUserControl.Instance.txtRodDia.Text,
                    RodExtensionAsDouble = SettingsUserControl.Instance.txtRodExtension.AsDouble,
                    RodExtensionAsString = SettingsUserControl.Instance.txtRodExtension.Text,
                    SupportSpacingAsString = SettingsUserControl.Instance.txtSupportSpacing.Text,
                    SupportSpacingAsDouble = SettingsUserControl.Instance.txtSupportSpacing.AsDouble
                };
              
                return settings;
            }
            return null;
        }

        public string GetName()
        {
            return "Save Settings Handler";
        }
    }
}
