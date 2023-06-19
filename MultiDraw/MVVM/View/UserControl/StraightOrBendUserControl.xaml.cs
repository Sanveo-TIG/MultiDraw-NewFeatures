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
    public partial class StraightOrBendUserControl : UserControl
    {
        public static StraightOrBendUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly List<string> _angleList = new List<string>() {/*"Auto",*/"0.00", "5.00", "11.25", "15.00", "22.50", "30.00", "45.00", "60.00", "90.00" };
        readonly ExternalEvent _externalEvents = null;
        public CustomUIApplication _application;    
        public StraightOrBendUserControl(ExternalEvent externalEvents, Window window, CustomUIApplication application)
        {
            _externalEvents = externalEvents;
            InitializeComponent();
            _application = application; 
            Instance = this;
            try
            {
                _window = window;

               
            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void AngleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //event raise
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            angleList.ItemsSource = _angleList;
            angleList.SelectedIndex = 0;
            _externalEvents.Raise();
        }
    }
}

