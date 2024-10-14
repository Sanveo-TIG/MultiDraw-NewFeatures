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
    public partial class RollingUserControl : UserControl
    {
        public static RollingUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        readonly List<string> _angleList = new List<string>() { "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00"};
        readonly ExternalEvent _externalEvents = null;
        public UIApplication _uiApp = null;
        public RollingUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _uiApp = application.UIApplication;
            _doc = _uidoc.Document;
            _externalEvents= externalEvents;    
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
            RollOffsetGP globalParam = new RollOffsetGP
            {
                OffsetValue = txtOffsetFeet.AsDouble == 0 ? "3\'" : txtOffsetFeet.AsString,
                RollOffsetValue = txtRollFeet.AsDouble == 0 ? "2\'" : txtRollFeet.AsString,
                AngleValue = ddlAngle.SelectedItem == null ? "30.00" : ddlAngle.SelectedItem.ToString()
            };
            Properties.Settings.Default.RollingOffsetDraw = JsonConvert.SerializeObject(globalParam);
            Properties.Settings.Default.Save();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffsetFeet.Click_load(txtOffsetFeet);
            txtRollFeet.Click_load(txtRollFeet);
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            txtOffsetFeet.UIApplication = _uiApp;
            txtRollFeet.UIApplication = _uiApp;
            List<MultiSelect> angleList = new List<MultiSelect>();
            foreach (string item in _angleList)
                angleList.Add(new MultiSelect() { Name = item });
            ddlAngle.ItemsSource = _angleList;
            ddlAngle.SelectedIndex = 4;
            Grid_MouseDown(null, null);
            string json = Properties.Settings.Default.RollingOffsetDraw;
            if (!string.IsNullOrEmpty(json))
            {
                RollOffsetGP globalParam = JsonConvert.DeserializeObject<RollOffsetGP>(json);
                txtOffsetFeet.Text = Convert.ToString(globalParam.OffsetValue);
                txtRollFeet.Text = Convert.ToString(globalParam.RollOffsetValue);
                ddlAngle.SelectedIndex = angleList.IndexOf(angleList.FirstOrDefault(x => x.Name == globalParam.AngleValue));               
            }
            else
            {
                txtOffsetFeet.Text = "3\'";
                txtRollFeet.Text = "2\'";
                ddlAngle.SelectedItem = 4;                
            }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }
}

