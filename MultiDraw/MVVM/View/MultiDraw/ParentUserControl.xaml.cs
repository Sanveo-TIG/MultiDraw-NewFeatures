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
using MultiDraw;
using System.Data;
using System.Collections.ObjectModel;
using TIGUtility;
using View = Autodesk.Revit.DB.View;
using System.ComponentModel;
using System.Drawing.Printing;

namespace MultiDraw
{
    
    /// <summary>
    /// UI Events
    /// </summary>

    public partial class ParentUserControl : UserControl
    {
        public List<Element> Primaryelst = new List<Element>();
        public List<Element> Secondaryelst = new List<Element>();
        public static ParentUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        readonly Document _doc = null;
        readonly UIDocument _uidoc = null;
        public List<ExternalEvent> _externalEvents = new List<ExternalEvent>();
        public CustomUIApplication _application = null;
        public Settings MultiDrawSettings = null;
        public SettingsUserControl settingsControl = null;
        public ProfileColorSettingsData _ProfileColorSettingsData = new ProfileColorSettingsData();
        public Transaction _transaction = null;
        public bool _isStopedTransaction = false;
        public ParentUserControl(List<ExternalEvent> externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
            _application = application;
            _externalEvents = externalEvents;
            InitializeComponent();

            Instance = this;
            try
            {
                _window = window;
                _window.LocationChanged += Window_LocationChanged;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            WindowProperty property = new WindowProperty
            {
                Top = _window.Top,
                Left = _window.Left
            };
            string strWindowProp = JsonConvert.SerializeObject(property);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string tempfilePath = System.IO.Path.GetDirectoryName(assembly.Location);
            DirectoryInfo di = new DirectoryInfo(tempfilePath);
            string tempfileName = System.IO.Path.Combine(di.FullName, "WindowProperty.txt");
            if (File.Exists(tempfileName))
            {
                File.Delete(tempfileName);
            }
            if (!File.Exists(tempfileName))
            {
                File.Create(tempfileName).Close();
            }
            File.WriteAllText(tempfileName, strWindowProp);
        }

        private void FromGlobalSearch()
        {
            try
            {
                string tempfilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                DirectoryInfo di = new DirectoryInfo(tempfilePath);
                if (new DirectoryInfo(di.Parent.FullName).Exists)
                {
                    di = new DirectoryInfo(di.Parent.FullName);
                }
                string tempfileName = System.IO.Path.Combine(di.Parent.FullName, "TriggerFile.txt");
                if (File.Exists(tempfileName))
                {
                    string lines = File.ReadLines(tempfileName).Skip(1).FirstOrDefault();
                    if (lines != null && lines.Length > 0)
                    {
                        string route = lines;

                        if (!string.IsNullOrEmpty(route) && route.EndsWith("-Vertical Offset"))
                        {
                            cmbProfileType.SelectedIndex = 0;
                        }
                        else if (!string.IsNullOrEmpty(route) && route.EndsWith("-Horizontal Offset"))
                        {
                            cmbProfileType.SelectedIndex = 1;
                        }
                        else if (!string.IsNullOrEmpty(route) && route.EndsWith("-Rolling Offset"))
                        {
                            cmbProfileType.SelectedIndex = 2;
                        }
                        else if (!string.IsNullOrEmpty(route) && route.EndsWith("-Kick"))
                        {
                            cmbProfileType.SelectedIndex = 3;
                        }
                        else if (!string.IsNullOrEmpty(route) && route.EndsWith("-Straight/Bend"))
                        {
                            cmbProfileType.SelectedIndex = 4;
                        }
                        else if (!string.IsNullOrEmpty(route) && route.EndsWith("-Kick 90"))
                        {
                            cmbProfileType.SelectedIndex = 5;
                        }
                        else if (!string.IsNullOrEmpty(route) && route.EndsWith("-Stub 90"))
                        {
                            cmbProfileType.SelectedIndex = 6;
                        }
                        else
                        {
                            cmbProfileType.SelectedIndex = 4;
                        }
                    }
                    else
                    {
                        cmbProfileType.SelectedIndex = 4;
                    }
                }
                else
                {
                    cmbProfileType.SelectedIndex = 4;
                }
            }
            catch { }
        }


        public void CmbProfileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            masterContainer.Children.Clear();
            UserControl userControl = new UserControl();
            // switch (((System.Windows.Controls.Primitives.Selector)sender).SelectedIndex)
            switch (cmbProfileType.SelectedIndex)
            {
                case 0:
                    userControl = new VOffsetUserControl(_externalEvents[0], _application, _window);
                    break;
                case 1:
                    userControl = new HOffsetUserControl(_externalEvents[0], _application, _window);
                    break;
                case 2:
                    userControl = new RollingUserControl(_externalEvents[0], _application, _window);
                    break;
                case 3:
                    userControl = new KickUserControl(_externalEvents[0], _application, _window);
                    break;
                case 4:
                    userControl = new StraightOrBendUserControl(_externalEvents[0], _window, _application);
                    break;
                case 5:
                    userControl = new NinetyKickUserControl(_externalEvents[0], _application, _window);
                    break;
                case 6:
                    userControl = new NinetyStubUserControl(_externalEvents[0], _application, _window);
                    break;
                case 7:
                    userControl = new ThreePointSaddleUserControl(_externalEvents[0], _application, _window);
                    //  userControl = new SyncDataUserControl(_application, _window);
                    break;
                case 8:
                    //userControl = new SettingsUserControl(_doc, _application.UIApplication, _window, _externalEvents[1]);
                    userControl = new FourPointSaddleUserControl(_externalEvents[0], _application, _window);
                    //settingsControl = userControl as SettingsUserControl;
                    break;
                default:
                    break;
            }
            masterContainer.Children.Add(userControl);
        }

        private void ReadSettings()
        {
            string Json = Properties.Settings.Default.MultiDrawSettings;
            if (!string.IsNullOrEmpty(Json))
            {
                try
                {
                    Settings settings = JsonConvert.DeserializeObject<Settings>(Json);
                    if (settings != null)
                    {
                        MultiDrawSettings = settings;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Properties.Settings.Default.MultiDrawSettings = string.Empty;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public Settings GetSettings()
        {
            if (settingsControl != null)
            {
                Settings settings = new Settings
                {
                    //    IsSupportNeeded = (bool)settingsControl.IsSupportNeeded.IsChecked,
                    //    StrutType = settingsControl.ddlStrutType.SelectedItem.Name,
                    //    RodDiaAsDouble = settingsControl.txtRodDia.AsDouble,
                    //    RodDiaAsString = settingsControl.txtRodDia.Text,
                    //    RodExtensionAsDouble = settingsControl.txtRodExtension.AsDouble,
                    //    RodExtensionAsString = settingsControl.txtRodExtension.Text,
                    //    SupportSpacingAsString = settingsControl.txtSupportSpacing.Text,
                    //    SupportSpacingAsDouble = settingsControl.txtSupportSpacing.AsDouble
                };
                MultiDrawSettings = settings;
                return settings;
            }
            return MultiDrawSettings;
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            FromGlobalSearch();
            //cmbProfileType.SelectedIndex = 4;
            ReadSettings();
            btnPlay.IsChecked = true;
            PlayButton_Click(null, null);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (btnPlay.IsChecked == true)
            {
                _isStopedTransaction = false;
                _externalEvents[0].Raise();
            }
            else
            {
                // _transaction.Commit();
                _isStopedTransaction = true;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

            switch (cmbProfileType.SelectedIndex)
            {
                case 0:
                    VOffsetUserControl.Instance.txtOffsetFeet.Click_load(VOffsetUserControl.Instance.txtOffsetFeet);
                    break;
                case 1:
                    HOffsetUserControl.Instance.txtOffsetFeet.Click_load(HOffsetUserControl.Instance.txtOffsetFeet);
                    break;
                case 2:
                    RollingUserControl.Instance.txtOffsetFeet.Click_load(RollingUserControl.Instance.txtOffsetFeet);
                    RollingUserControl.Instance.txtRollFeet.Click_load(RollingUserControl.Instance.txtRollFeet);
                    break;
                case 3:
                    KickUserControl.Instance.txtOffsetFeet.Click_load(KickUserControl.Instance.txtOffsetFeet);
                    break;

                case 5:
                    NinetyKickUserControl.Instance.txtOffset.Click_load(NinetyKickUserControl.Instance.txtOffset);
                    NinetyKickUserControl.Instance.txtRise.Click_load(NinetyKickUserControl.Instance.txtRise);
                    break;
                case 6:
                    NinetyStubUserControl.Instance.txtOffsetFeet.Click_load(NinetyStubUserControl.Instance.txtOffsetFeet);
                    break;
                case 7:
                    ThreePointSaddleUserControl.Instance.txtOffsetFeet.Click_load(ThreePointSaddleUserControl.Instance.txtOffsetFeet);
                    break;
                case 8:
                    FourPointSaddleUserControl.Instance.txtOffsetFeet.Click_load(FourPointSaddleUserControl.Instance.txtOffsetFeet);
                    FourPointSaddleUserControl.Instance.txtBaseOffsetFeet.Click_load(FourPointSaddleUserControl.Instance.txtBaseOffsetFeet);
                    break;
                default:
                    break;
            }
        }

        private void Anglefromprimary_Unchecked(object sender, RoutedEventArgs e)
        {
            if (HOffsetUserControl.Instance?.txtOffsetFeet != null)
            {
                HOffsetUserControl.Instance.txtOffsetFeet.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void Anglefromprimary_Checked(object sender, RoutedEventArgs e)
        {
            if (HOffsetUserControl.Instance?.txtOffsetFeet != null)
            {
                HOffsetUserControl.Instance.txtOffsetFeet.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}


