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
        Document _doc = null;
        UIDocument _uidoc = null;
        string _offsetVariable = string.Empty;
        ExternalEvent _externalEvents = null;
        UIApplication _uiApp = null;
        List<MultiSelect> multiSelectList = new List<MultiSelect>();

        List<string> _removingList = new List<string>();
        public SyncDataUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _uiApp = application.UIApplication;
            _doc = _uidoc.Document;
            _externalEvents = externalEvents;
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
                List<SYNCDataGlobalParam> globalParam = null;
                string json = Utility.GetGlobalParametersManager(_uiApp, "SyncDataParameters");
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

        private void btnsync_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void ucMultiSelect_DropDownClosed(object sender)
        {

        }

        private void btnCheck_Click(object sender)
        {
            //_externalEvents.Raise();
        }
    }
}

