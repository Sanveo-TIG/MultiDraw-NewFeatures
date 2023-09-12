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
    public partial class NinetyKickUserControl : UserControl
    {
        public static  NinetyKickUserControl  Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        readonly List<string> _angleList = new List<string>() { "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00"};
        readonly ExternalEvent _externalEvents = null;
        public UIApplication _uiApp = null;
        public  NinetyKickUserControl (ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uiApp = application.UIApplication;
            _uidoc = application.UIApplication.ActiveUIDocument;
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
            NinetyKickGP globalParam = new NinetyKickGP
            {
                OffsetValue = txtOffset.AsDouble == 0 ? "1.5\'" : txtOffset.AsString,
                RiseValue = txtRise.AsDouble == 0 ? "10.0\'" : txtRise.AsString,
                AngleValue = ddlAngle.SelectedItem == null ? "30.00" : ddlAngle.SelectedItem.Name
            };
            Properties.Settings.Default.NinetyKickDraw = JsonConvert.SerializeObject(globalParam);
            Properties.Settings.Default.Save();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffset.Click_load(txtOffset);
            txtRise.Click_load(txtRise);
        }

        private void DdlAngle_Changed(object sender)
        {
            SaveSettings();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            txtOffset.UIApplication = _uiApp;
            List<MultiSelect> angleList = new List<MultiSelect>();
            foreach (string item in _angleList)
                angleList.Add(new MultiSelect() { Name = item });
            ddlAngle.ItemsSource = angleList;
            txtRise.UIApplication = _uiApp;
            Grid_MouseDown(null, null);
            string json = Properties.Settings.Default.NinetyKickDraw;
            if (!string.IsNullOrEmpty(json))
            {
                NinetyKickGP globalParam = JsonConvert.DeserializeObject<NinetyKickGP>(json);
                txtOffset.Text = Convert.ToString(globalParam.OffsetValue);
                txtRise.Text = Convert.ToString(globalParam.RiseValue);
                ddlAngle.SelectedItem = angleList[angleList.FindIndex(x => x.Name == globalParam.AngleValue)];               
            }
            else
            {
                txtOffset.Text = "1.5\'";
                txtRise.Text = "10.0\'";
                ddlAngle.SelectedItem = angleList[4];               
            }
           // _externalEvents.Raise();
        }
    }
}

