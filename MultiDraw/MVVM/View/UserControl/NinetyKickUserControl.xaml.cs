﻿using Autodesk.Revit.DB;
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
        public  NinetyKickUserControl (ExternalEvent externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
            _externalEvents = externalEvents;
            InitializeComponent();
            Instance = this;
            try
            {
                _window = window;                
                txtOffset.Document = _doc;
                txtOffset.UIApplication = application.UIApplication;
                List<MultiSelect> angleList = new List<MultiSelect>();
                foreach (string item in _angleList)
                    angleList.Add(new MultiSelect() { Name = item });
                ddlAngle.ItemsSource = angleList;
                ddlAngle.SelectedItem = angleList[4];
                txtRise.Document = _doc;
                txtRise.UIApplication = application.UIApplication;
                txtOffset.Text = "1.5";
                txtRise.Text = "10.0";
                Grid_MouseDown(null, null);
                string json = Utility.GetGlobalParametersManager(application.UIApplication, "NinetyKickDraw");
                if (!string.IsNullOrEmpty(json))
                {
                    NinetyKickGP globalParam = JsonConvert.DeserializeObject<NinetyKickGP>(json);
                    ddlAngle.SelectedItem = angleList[angleList.FindIndex(x => x.Name ==  globalParam.AngleValue)];
                    txtOffset.Text = Convert.ToString(globalParam.OffsetValue);
                    txtRise.Text = Convert.ToString(globalParam.RiseValue);
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
            txtOffset.Click_load(txtOffset);
            txtRise.Click_load(txtRise);
        }
    }
}

