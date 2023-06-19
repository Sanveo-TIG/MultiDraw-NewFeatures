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
using Autodesk.Revit.DB.Architecture;

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

        private HOffsetUserControl H_Offset = null;
        private VOffsetUserControl V_Offset = null;
        private RollingUserControl RollingOffset = null;
        private KickUserControl kickWithBend = null;
        private StraightOrBendUserControl striaghtcontrol = null;
        private NinetyKickUserControl ninetykickcontrol = null;
        private NinetyStubUserControl ninetystubcontrol = null;
        private SyncDataUserControl syncDataControl = null;

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
                string json = Utility.GetGlobalParametersManager(application.UIApplication, "StraightsDraw");
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
            switch (((System.Windows.Controls.Primitives.Selector)sender).SelectedIndex)
            {
                case 0:
                    if (V_Offset == null)
                        V_Offset = new VOffsetUserControl(_externalEvents[0], _application, _window);
                    masterContainer.Children.Add(VOffsetUserControl.Instance);
                    break;
                case 1:
                    if (H_Offset == null)
                        H_Offset = new HOffsetUserControl(_externalEvents[0], _application, _window);
                    masterContainer.Children.Add(HOffsetUserControl.Instance);
                    break;
                case 2:
                    if (RollingOffset == null)
                        RollingOffset = new RollingUserControl(_externalEvents[0], _application, _window);
                    masterContainer.Children.Add(RollingUserControl.Instance);
                    break;
                case 3:
                    if (kickWithBend == null)
                        kickWithBend = new KickUserControl(_externalEvents[0], _application, _window);
                    masterContainer.Children.Add(KickUserControl.Instance);
                    break;
                case 4:
                    if (striaghtcontrol == null)
                        striaghtcontrol = new StraightOrBendUserControl(_externalEvents[0], _window, _application);
                    masterContainer.Children.Add(StraightOrBendUserControl.Instance);
                    break;
                case 5:
                    if (ninetykickcontrol == null)
                        ninetykickcontrol = new NinetyKickUserControl(_externalEvents[0], _application, _window);
                    masterContainer.Children.Add(NinetyKickUserControl.Instance);
                    break;
                case 6:
                    if (ninetystubcontrol == null)
                        ninetystubcontrol = new NinetyStubUserControl(_externalEvents[0], _application, _window);
                    masterContainer.Children.Add(NinetyStubUserControl.Instance);
                    break;
                case 7:
                    if (syncDataControl == null)
                        syncDataControl = new SyncDataUserControl(_externalEvents[0], _application, _window);
                    masterContainer.Children.Add(SyncDataUserControl.Instance);
                    break;
                case 8:
                    if (settingsControl == null)
                        settingsControl = new SettingsUserControl(_doc, _application.UIApplication, _window, _externalEvents[1]);
                    masterContainer.Children.Add(SettingsUserControl.Instance);
                    break;
                default:
                    break;
            }

        }

        private void ReadSettings()
        {
            string Json = Utility.GetGlobalParametersManager(_application.CommandData.Application, "MultiDrawSettings");
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
            LoadAllFamilies();
        }

        private void LoadAllFamilies()
        {
            IList<Element> Symbols = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_ElectricalFixtures).OfClass(typeof(Family)).ToElements();
            string family_folder = System.IO.Path.GetDirectoryName(typeof(Command).Assembly.Location);
            using (Transaction tx = new Transaction(_doc))
            {
                tx.Start("Load Families");
                DirectoryInfo di = new DirectoryInfo(family_folder);
                FileInfo[] familyfiles = di.GetFiles("*.rfa");
                foreach (FileInfo fi in familyfiles)
                {
                    FamilySymbol Symbol = null;
                    Family family = null;
                    string familyName = System.IO.Path.GetFileNameWithoutExtension(fi.FullName);
                    if (!Symbols.Any(r => r.Name == familyName))
                    {
                        if (_doc.LoadFamily(fi.FullName, out family))
                        {
                            ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();
                            foreach (ElementId id in familySymbolIds)
                            {
                                Symbol = _doc.GetElement(id) as FamilySymbol;
                                if (!Symbol.IsActive)
                                {
                                    Symbol.Activate();
                                    _doc.Regenerate();
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                tx.Commit();
            }
        }
    }
}

