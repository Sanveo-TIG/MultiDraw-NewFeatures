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
    public partial class VOffsetUserControl : UserControl
    {
        public static VOffsetUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        readonly List<string> _angleList = new List<string>() { "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00" };
        readonly ExternalEvent _externalEvents = null;
        public UIApplication _uiApp = null;
        public VOffsetUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
            _uiApp = application.UIApplication;
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

                //ddlAngle.Attributes = new MultiSelectAttributes()
                //{
                //    Label = "Angle",
                //    Width=310
                //};

            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void SaveSettings()
        {
            VerticalOffsetGP globalParam = new VerticalOffsetGP
            {
                OffsetValue = txtOffsetFeet.AsDouble == 0 ? "1.5\'" : txtOffsetFeet.AsString,
                AngleValue = ddlAngle.SelectedItem == null ? "30.00" : ddlAngle.SelectedItem.ToString(),
            };
            Properties.Settings.Default.VerticalOffsetDraw = JsonConvert.SerializeObject(globalParam);
            Properties.Settings.Default.Save();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffsetFeet.Click_load(txtOffsetFeet);
        }

        //private void DdlAngle_Changed(object sender)
        //{
        //    SaveSettings();
        //}

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            txtOffsetFeet.UIApplication = _uiApp;
            List<MultiSelect> angleList = new List<MultiSelect>();
            foreach (string item in _angleList)
                angleList.Add(new MultiSelect() { Name = item });
            ddlAngle.ItemsSource = _angleList;
            ddlAngle.SelectedIndex = 4;
            Grid_MouseDown(null, null);
            string json = Properties.Settings.Default.VerticalOffsetDraw;
            if (!string.IsNullOrEmpty(json))
            {
                VerticalOffsetGP globalParam = JsonConvert.DeserializeObject<VerticalOffsetGP>(json);
                txtOffsetFeet.Text = Convert.ToString(globalParam.OffsetValue);
                ddlAngle.SelectedItem = angleList.FindIndex(x => x.Name == globalParam.AngleValue);
            }
            else
            {
                txtOffsetFeet.Text = "1.5\'";
                ddlAngle.SelectedItem = 4;
            }
            // _externalEvents.Raise();
        }

        private void ddlAngle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveSettings();
        }
    }
}

