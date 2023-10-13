using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TIGUtility;

namespace MultiDraw
{
    /// <summary>
    /// Interaction logic for SettingsUserControl.xaml
    /// </summary>
    public partial class SettingsUserControl : UserControl
    {
        public static SettingsUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        //public System.Windows.Window SettingsWindow = new System.Windows.Window();
        readonly Document _doc = null;
        public UIApplication _uiApp = null;
        readonly List<string> _removingList = new List<string>();
        readonly string _offsetVariable = string.Empty;
        readonly List<MultiSelect> multiSelectList = new List<MultiSelect>();
        ExternalEvent eventsync = null;
        public SettingsUserControl(Document doc, UIApplication uiApp, Window window, ExternalEvent saveEvent)
        {
            try
            {
                InitializeComponent();
                Instance = this;
                eventsync = SettingsWindow.Instance._externalEvents[2];
                ddlStrutType.Attributes = new MultiSelectAttributes()
                {
                    Width = 200
                };
                ucMultiSelect.Attributes = new MultiSelectAttributes()
                {
                    IsRequired = false,
                    Width = 250,
                    Label = "Parameters Collection"
                };
                _doc = doc;
                _uiApp = uiApp;
                txtSupportSpacing.UIApplication = uiApp;
                txtSupportSpacing.Text = "8\'";

                txtRodDia.UIApplication = uiApp;
                txtRodDia.Text = "3/8\"";

                txtRodExtension.UIApplication = uiApp;
                txtRodExtension.Text = "3/8\"";
                List<MultiSelect> sType = new List<MultiSelect>
                {
                    new MultiSelect{Name = "TIG HANGER STRUT v1"},
                    new MultiSelect{Name = "TIG HANGER STRUT v2"}
            };
                int.TryParse(uiApp.Application.VersionNumber, out int RevitVersion);
                if (RevitVersion < 2022)
                {
                    sType.Remove(sType.FirstOrDefault(x => x.Name == "TIG HANGER STRUT v2"));
                }
                ddlStrutType.ItemsSource = sType;
                ddlStrutType.SelectedItem = sType.Last();
                _window = window;
                LoadTab();



                UserControl userControl = new ProfileColorSettingUserControl(saveEvent, uiApp, window);
                containerProfileColorSettings.Children.Add(userControl);
                _offsetVariable = RevitVersion < 2020 ? "Offset" : "Middle Elevation";
                _removingList = new List<string>()
            {
                _offsetVariable,
                "Horizontal Justification",
                "Vertical Justification" ,
                "Reference Level",
                "Top Elevation",
                "Bottom Elevation",
                "Upper End Top Elevation",
                "Upper End Bottom Elevation",
                "Upper End Centerline Elevation",
                "Lower End Top Elevation",
                "Lower End Bottom Elevation",
                "Lower End Centerline Elevation"
            };
                List<SYNCDataGlobalParam> globalParam = new List<SYNCDataGlobalParam>();
                string json = Utility.GetGlobalParametersManager(_uiApp, "SyncDataParameters");
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        globalParam = JsonConvert.DeserializeObject<List<SYNCDataGlobalParam>>(json);
                        /*foreach (SYNCDataGlobalParam param in globalParam)
                        {
                            List<MultiSelect> multiSelect = new List<MultiSelect>();
                            MultiSelect multiSelects = new MultiSelect();
                            if (param != null)
                            {
                                multiSelects = multiSelectList.FirstOrDefault(r => r.Name.Contains(param.Name));
                                multiSelects.IsChecked = true;
                                multiSelect.Add(multiSelects);
                                ucMultiSelect.SelectedItems = multiSelect;
                            }
                        }*/
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        MessageBox.Show("Saved parameters cannot be read. Click okay to delete the saved parameters and reset the form");
                        //externalEvents[1].Raise();
                    }
                }
             //   FilteredElementCollector conduitscollector = new FilteredElementCollector(_doc);
                List<Element> Conduits = new FilteredElementCollector(_doc).OfClass(typeof(Conduit)).ToList();
                if (Conduits.Any())
                {
                    Element e = Conduits.FirstOrDefault();
                    foreach (Parameter parameter in e.GetOrderedParameters().ToList().Where(r => !r.IsReadOnly))
                    {
                        if (!_removingList.Any(x => x == parameter.Definition.Name))
                        {
                            MultiSelect multi = new MultiSelect
                            {
                                Name = parameter.Definition.Name,
                                IsChecked = false,
                                Id = parameter.Id
                            };
                            if (globalParam != null && globalParam.Any(r => r.Name == multi.Name))
                            {
                                multi.IsChecked = true;
                            }
                            multiSelectList.Add(multi);
                        }
                    }
                    multiSelectList = multiSelectList.OrderBy(x => x.Name).ToList();
                    ucMultiSelect.ItemsSource = multiSelectList.OrderByDescending(x => x.IsChecked).ToList();


                    // ParentUserControl.Instance.AlignConduits.IsEnabled = false;
                    //ParentUserControl.Instance.Anglefromprimary.IsEnabled = false;
                    //ParentUserControl.Instance.AlignConduits.IsChecked = false;
                    //ParentUserControl.Instance.Anglefromprimary.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("War", ex.StackTrace);
            }


        }
        private void LoadTab()
        {
            List<CustomTab> customTabsList = new List<CustomTab>();
            CustomTab b = new CustomTab();
            b.Id = 1;
            b.Name = "Support Settings";

            customTabsList.Add(b);
            b = new CustomTab();
            b.Id = 2;
            b.Name = "Profile Color Settings";
            customTabsList.Add(b);

            b = new CustomTab();
            b.Id = 3;
            b.Name = "Auto Sync";
            customTabsList.Add(b);


            tagControl.ItemsSource = customTabsList;
            tagControl.SelectedIndex = 0;

        }
        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            ReadRackSettings();
            OnChangeSupportNeeded(IsSupportNeeded, null);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ReadRackSettings()
        {
            string jsonFromFile = Properties.Settings.Default.MultiDrawSettings;
            if (!string.IsNullOrEmpty(jsonFromFile))
            {
                Settings settings = JsonConvert.DeserializeObject<Settings>(jsonFromFile);
                if (settings != null)
                {
                    IsSupportNeeded.IsChecked = settings.IsSupportNeeded;
                    List<MultiSelect> sType = ddlStrutType.ItemsSource;
                    ddlStrutType.SelectedItem = sType.FirstOrDefault(r => r.Name.Trim() == settings.StrutType.Trim());
                    txtRodDia.IsEnabled = settings.IsSupportNeeded;
                    txtRodExtension.IsEnabled = settings.IsSupportNeeded;
                    txtSupportSpacing.IsEnabled = settings.IsSupportNeeded;
                    txtRodDia.Text = settings.RodDiaAsString;
                    txtRodExtension.Text = settings.RodExtensionAsString;
                    txtSupportSpacing.Text = settings.SupportSpacingAsString;
                }
            }
            /*   string json = Utility.GetGlobalParametersManager(_uiApp, "SyncDataParameters");
               if (json != null)
               {
                   List<SYNCDataGlobalParam> syncdata = JsonConvert.DeserializeObject<List<SYNCDataGlobalParam>>(json);
                   foreach (SYNCDataGlobalParam param in syncdata)
                   {
                       List<MultiSelect> multiSelect = new List<MultiSelect>();
                       MultiSelect multiSelects = new MultiSelect();
                       if (param != null)
                       {
                           multiSelects = multiSelectList.FirstOrDefault(r => r.Name.Contains(param.Name));
                           multiSelects.IsChecked = true;
                           multiSelect.Add(multiSelects);
                           ucMultiSelect.SelectedItems = multiSelect;
                       }
                   }
               }*/
        }
        private void OnChangeSupportNeeded(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            txtRodDia.IsEnabled = (bool)checkBox.IsChecked;
            txtRodExtension.IsEnabled = (bool)checkBox.IsChecked;
            txtSupportSpacing.IsEnabled = (bool)checkBox.IsChecked;
            SettingsUserControl.Instance.ddlStrutType.IsEnabled = (bool)checkBox.IsChecked;
        }

        private void TagControl_SelectionChanged(object sender)
        {
            if (tagControl.SelectedIndex == 0)
            {
                containerSupportSettings.Visibility = System.Windows.Visibility.Visible;
                containerProfileColorSettings.Visibility = System.Windows.Visibility.Collapsed;
                GridAutoSync.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (tagControl.SelectedIndex == 1)
            {
                containerProfileColorSettings.Visibility = System.Windows.Visibility.Visible;
                containerSupportSettings.Visibility = System.Windows.Visibility.Collapsed;
                GridAutoSync.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                GridAutoSync.Visibility = System.Windows.Visibility.Visible;
                containerSupportSettings.Visibility = System.Windows.Visibility.Collapsed;
                containerProfileColorSettings.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void UcMultiSelect_DropDownClosed(object sender)
        {
            eventsync.Raise();
        }


    }
}
