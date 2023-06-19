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
    public partial class NinetyStubUserControl : UserControl
    {
        public static NinetyStubUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        Document _doc = null;
        UIDocument _uidoc = null;
        string _offsetVariable = string.Empty;
        List<string> _angleList = new List<string>() { "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00" };
        ExternalEvent _externalEvents = null;
        public NinetyStubUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
            _offsetVariable = application.OffsetVariable;
            _externalEvents = externalEvents;
            InitializeComponent();
            Instance = this;
            try
            {
                _window = window;                
                txtOffsetFeet.Document = _doc;
                txtOffsetFeet.UIApplication = application.UIApplication;
                List<MultiSelect> angleList = new List<MultiSelect>();
                foreach (string item in _angleList)
                    angleList.Add(new MultiSelect() { Name = item });
                txtOffsetFeet.Text = "5";
                Grid_MouseDown(null,null);

                string json = Utility.GetGlobalParametersManager(application.UIApplication, "90's Stub Draw");
                if (!string.IsNullOrEmpty(json))
                {
                    NinetyStubGP globalParam = JsonConvert.DeserializeObject<NinetyStubGP>(json);
                    txtOffsetFeet.Text = Convert.ToString(globalParam.OffsetValue);
                }
                _externalEvents.Raise();

            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

      
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffsetFeet.Click_load(txtOffsetFeet);
        }
    }
}

