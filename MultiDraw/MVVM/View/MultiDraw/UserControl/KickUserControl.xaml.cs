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
    public partial class KickUserControl : UserControl
    {
        public static KickUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        readonly List<string> _angleList = new List<string>() { "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00"};
        readonly ExternalEvent _externalEvents = null;
        public UIApplication _uiApp = null;
        public KickUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _uiApp = application.UIApplication;
            _doc = _uidoc.Document;
            _externalEvents = externalEvents;
            InitializeComponent();
            Instance = this;
            ddlAngle.Attributes = new MultiSelectAttributes()
            {
                Label = "Angle",
                Width = 325
            };
            try
            {
                _window = window;
                ParentUserControl.Instance.AlignConduits.IsEnabled = false;
                ParentUserControl.Instance.Anglefromprimary.IsEnabled = true;               
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveSettings()
        {
            Kick90DrawGP globalParam = new Kick90DrawGP
            {
                AngleValue = ddlAngle == null ? "30.00" : ddlAngle.SelectedItem.Name,
                OffsetValue = txtOffsetFeet.AsDouble == 0 ? "1.5\'" : txtOffsetFeet.AsString,
                SelectionMode = rbNinetyNear.IsChecked == true ? "90° Near" : "90° Far"
            };
            Properties.Settings.Default.Kick90Draw = JsonConvert.SerializeObject(globalParam);
            Properties.Settings.Default.Save();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffsetFeet.Click_load(txtOffsetFeet);
        }

        private void DdlAngle_Changed(object sender)
        {
            SaveSettings();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SelectionMode_Changed(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            txtOffsetFeet.UIApplication = _uiApp;
            List<MultiSelect> angleList = new List<MultiSelect>();
            foreach (string item in _angleList)
                angleList.Add(new MultiSelect() { Name = item });
            ddlAngle.ItemsSource = angleList;
            Grid_MouseDown(null, null);
            string json = Properties.Settings.Default.Kick90Draw;
            if (!string.IsNullOrEmpty(json))
            {
                Kick90DrawGP globalParam = JsonConvert.DeserializeObject<Kick90DrawGP>(json);
                txtOffsetFeet.Text = Convert.ToString(globalParam.OffsetValue);
                rbNinetyNear.IsChecked = globalParam.SelectionMode == "90° Near";
                rbNinetyFar.IsChecked = string.IsNullOrEmpty(globalParam.SelectionMode) || globalParam.SelectionMode == "90° Far";
                ddlAngle.SelectedItem = angleList[angleList.FindIndex(x => x.Name == globalParam.AngleValue)];               
            }
            else
            {
                txtOffsetFeet.Text = "1.5\'";
                ddlAngle.SelectedItem = angleList[4];                
            }
          //  _externalEvents.Raise();
        }
    }
}

