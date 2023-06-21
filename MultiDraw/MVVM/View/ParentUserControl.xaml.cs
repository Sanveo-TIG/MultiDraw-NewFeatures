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
                string json = Properties.Settings.Default.StraightsDraw;
                if (!string.IsNullOrEmpty(json))
                {
                    StraightsDrawParam globalParam = JsonConvert.DeserializeObject<StraightsDrawParam>(json);
                    Anglefromprimary.IsChecked = globalParam.IsPrimaryAngle;
                    AlignConduits.IsChecked = globalParam.IsAlignConduit;
                   
                }
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

        public void CmbProfileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            masterContainer.Children.Clear();
            UserControl userControl = new UserControl();
            switch (((System.Windows.Controls.Primitives.Selector)sender).SelectedIndex)
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
                    userControl = new SyncDataUserControl(_application, _window);
                    break;
                case 8:
                    userControl = new SettingsUserControl(_doc, _application.UIApplication, _window);
                    settingsControl = userControl as SettingsUserControl;
                    break;
                default:
                    break;
            }
            masterContainer.Children.Add(userControl);
           
        }

        private void ReadSettings()
        {
            string Json = Properties.Settings.Default.MultiDrawSettings;
            if(!string.IsNullOrEmpty(Json))
            {
                try
                {
                    Settings settings = JsonConvert.DeserializeObject<Settings>(Json);
                    if(settings != null)
                    {
                        MultiDrawSettings = settings;
                    }
                }
                catch(Exception ex)
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
                    IsSupportNeeded = (bool)settingsControl.IsSupportNeeded.IsChecked,
                    StrutType = settingsControl.ddlStrutType.SelectedItem.Name,
                    RodDiaAsDouble = settingsControl.txtRodDia.AsDouble,
                    RodDiaAsString = settingsControl.txtRodDia.Text,
                    RodExtensionAsDouble = settingsControl.txtRodExtension.AsDouble,
                    RodExtensionAsString = settingsControl.txtRodExtension.Text,
                    SupportSpacingAsString = settingsControl.txtSupportSpacing.Text,
                    SupportSpacingAsDouble = settingsControl.txtSupportSpacing.AsDouble
                };
                MultiDrawSettings = settings;
                return settings;
            }
            return MultiDrawSettings;
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            cmbProfileType.SelectedIndex = 4;
            ReadSettings();
        }
    }
}

