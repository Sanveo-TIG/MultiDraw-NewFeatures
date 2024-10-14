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
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        readonly List<string> _angleList = new List<string>() { "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00" };
        readonly ExternalEvent _externalEvents = null;
        public UIApplication _uiApp = null;
        public NinetyStubUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uiApp = application.UIApplication;
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
            _externalEvents = externalEvents;
            InitializeComponent();
            Instance = this;
            try
            {
                _window = window;
                ParentUserControl.Instance.AlignConduits.IsEnabled = false;
                ParentUserControl.Instance.Anglefromprimary.IsEnabled = false;
                ParentUserControl.Instance.AlignConduits.IsChecked = false;
                ParentUserControl.Instance.Anglefromprimary.IsChecked = false;                
            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveSettings()
        {
            NinetyStubGP globalParam = new NinetyStubGP
            {
                OffsetValue = txtOffsetFeet.AsDouble == 0 ? "5\'" : txtOffsetFeet.AsString
            };
            Properties.Settings.Default.NinetyStubDraw = JsonConvert.SerializeObject(globalParam);
            Properties.Settings.Default.Save();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffsetFeet.Click_load(txtOffsetFeet);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings(); 
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            txtOffsetFeet.UIApplication = _uiApp;
            Grid_MouseDown(null, null);
            string json = Properties.Settings.Default.NinetyStubDraw;
            if (!string.IsNullOrEmpty(json))
            {
                NinetyStubGP globalParam = JsonConvert.DeserializeObject<NinetyStubGP>(json);
                txtOffsetFeet.Text = Convert.ToString(globalParam.OffsetValue);
            }
            else
            {
                txtOffsetFeet.Text = "5\'";
            }
        }
    }
}

