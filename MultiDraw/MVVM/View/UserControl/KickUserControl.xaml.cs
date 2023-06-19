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
        public KickUserControl(ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
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
                ddlAngle.ItemsSource = angleList;
                ddlAngle.SelectedItem = angleList[4];
                txtOffsetFeet.Text = "1.5";
                Grid_MouseDown(null, null);
                string json = Utility.GetGlobalParametersManager(application.UIApplication, "Kick90Draw");
                if (!string.IsNullOrEmpty(json))
                {
                    Kick90DrawGP globalParam = JsonConvert.DeserializeObject<Kick90DrawGP>(json);
                    ddlAngle.SelectedItem = angleList[angleList.FindIndex(x => x.Name == globalParam.AngleValue)];
                    txtOffsetFeet.Text = Convert.ToString(globalParam.OffsetValue);
                    rbNinetyNear.IsChecked = /*string.IsNullOrEmpty(globalParam.SelectionMode) ? true:*/ globalParam.SelectionMode == "90° Near";
                    rbNinetyFar.IsChecked = string.IsNullOrEmpty(globalParam.SelectionMode) || globalParam.SelectionMode == "90° Far";

                }
                _externalEvents.Raise();

            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void BtnDraw_btnClick(object sender)
        {
            _externalEvents.Raise();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtOffsetFeet.Click_load(txtOffsetFeet);
        }

        private void RbList_Checked(object sender)
        {

        }

        private void RbList_UnChecked(object sender)
        {

        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}

