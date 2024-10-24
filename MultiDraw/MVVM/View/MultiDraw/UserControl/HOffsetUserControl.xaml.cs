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
    public partial class HOffsetUserControl : UserControl
    {
        public static HOffsetUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        readonly ExternalEvent _externalEvents = null;
        public UIApplication _uiApp = null;
        readonly List<string> _angleList = new List<string>() { "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00" };
        public HOffsetUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
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
                ParentUserControl.Instance.Anglefromprimary.IsEnabled = true;
              
                if (ParentUserControl.Instance.Anglefromprimary.IsChecked == true)
                {
                    txtOffsetFeet.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    txtOffsetFeet.Visibility = System.Windows.Visibility.Hidden;
                }

               // ParentUserControl.Instance.Anglefromprimary.IsChecked = true;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            HOffsetGP globalParam = new HOffsetGP
            {
                OffsetValue = txtOffsetFeet.AsDouble == 0 ? "1.5\'" : txtOffsetFeet.AsString,
                AngleValue = ddlAngle.SelectedItem == null ? "30.00" : ddlAngle.SelectedItem.ToString()
            };
            Properties.Settings.Default.HorizontalOffsetDraw = JsonConvert.SerializeObject(globalParam);
            Properties.Settings.Default.Save();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffsetFeet.Click_load(txtOffsetFeet);
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
            string json = Properties.Settings.Default.HorizontalOffsetDraw;
            if (!string.IsNullOrEmpty(json))
            {
                HOffsetGP globalParam = JsonConvert.DeserializeObject<HOffsetGP>(json);
                txtOffsetFeet.Text = Convert.ToString(globalParam.OffsetValue);
                ddlAngle.SelectedIndex = angleList.IndexOf(angleList.FirstOrDefault(x => x.Name == globalParam.AngleValue));
            }
            else
            {
                txtOffsetFeet.Text = "1.5\'";
                ddlAngle.SelectedItem = 4;
            }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }
}





