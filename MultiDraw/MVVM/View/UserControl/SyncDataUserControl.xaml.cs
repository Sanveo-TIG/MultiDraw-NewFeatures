using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using System.Collections.ObjectModel;
using TIGUtility;

namespace MultiDraw
{
    /// <summary>
    /// UI Events
    /// </summary>
    public partial class SyncDataUserControl : UserControl
    {
        public static SyncDataUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        readonly string _offsetVariable = string.Empty;
        readonly UIApplication _uiApp = null;
        readonly List<MultiSelect> multiSelectList = new List<MultiSelect>();

        readonly List<string> _removingList = new List<string>();

        public static List<MultiSelect> _selectedSyncDataList = new List<MultiSelect>();
        public SyncDataUserControl(CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _uiApp = application.UIApplication;
            _doc = _uidoc.Document;
            InitializeComponent();
            Instance = this;
            int.TryParse(application.UIApplication.Application.VersionNumber, out int RevitVersion);
            _offsetVariable = RevitVersion < 2020 ? "Offset" : "Middle Elevation";
            _removingList = new List<string>()
            {
                _offsetVariable,
                "Horizontal Justification",
                "Vertical Justification" ,
                "Reference Level",
                "Top Elevation",
                "Bottom Elevation"

            };
            try
            {
                _window = window;
                ParentUserControl.Instance.AlignConduits.IsEnabled = false;
                ParentUserControl.Instance.Anglefromprimary.IsEnabled = false;
                ParentUserControl.Instance.AlignConduits.IsChecked = false;
                ParentUserControl.Instance.Anglefromprimary.IsChecked = false;
                List<SYNCDataGlobalParam> globalParam = null;
                string json = Properties.Settings.Default.SyncDataParameters;
                if (!string.IsNullOrEmpty(json))
                {
                    globalParam = JsonConvert.DeserializeObject<List<SYNCDataGlobalParam>>(json);
                }
                FilteredElementCollector conduitscollector = new FilteredElementCollector(_doc);
                Element e = conduitscollector.OfClass(typeof(Conduit)).FirstOrDefault();
                foreach (Parameter parameter in e.GetOrderedParameters().ToList().Where(r => !r.IsReadOnly))
                {
                    if (!_removingList.Any(x => x == parameter.Definition.Name))
                    {
                        MultiSelect multi = new MultiSelect
                        {
                            Name = parameter.Definition.Name,
                            IsChecked = false,
                            Id = parameter.Id,
                            Item = parameter
                        };
                        if (globalParam != null && globalParam.Any(r => r.Name == multi.Name))
                        {
                            multi.IsChecked = true;
                        }
                        multiSelectList.Add(multi);
                    }
                }
                multiSelectList = multiSelectList.OrderBy(x => x.Name).ToList();
                ucMultiSelect.ItemsSource = multiSelectList;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveSettings()
        {
            List<SYNCDataGlobalParam> globalParam = new List<SYNCDataGlobalParam>();
            globalParam = ucMultiSelect.ItemsSource.Where(r=> r.IsChecked).Select(r=> new SYNCDataGlobalParam { Name = r.Name}).ToList();
            Properties.Settings.Default.SyncDataParameters = JsonConvert.SerializeObject(globalParam);
            Properties.Settings.Default.Save();
        }
        private void UcMultiSelect_DropDownClosed(object sender)
        {
            SaveSettings();           
        }

        private void BtnCheck_Click(object sender)
        {
        }
    }
}

