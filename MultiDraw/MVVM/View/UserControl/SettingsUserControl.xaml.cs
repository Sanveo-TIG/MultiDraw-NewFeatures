using Autodesk.Revit.DB;
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
        public System.Windows.Window SettingsWindow = new System.Windows.Window();
        readonly Document _doc = null;
        public UIApplication _uiApp = null;
        public SettingsUserControl(Document doc, UIApplication uiApp,Window window, ExternalEvent saveEvent)
        {
            InitializeComponent();
            Instance = this;
            _doc = doc;
            _uiApp = uiApp;
            txtSupportSpacing.Document = _doc;
            txtSupportSpacing.UIApplication = uiApp;
            txtSupportSpacing.Text = "8\'";

            txtRodDia.Document = _doc;
            txtRodDia.UIApplication = uiApp;
            txtRodDia.Text = "3/8\"";

            txtRodExtension.Document = _doc;
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
            ParentUserControl.Instance.AlignConduits.IsEnabled = false;
            ParentUserControl.Instance.Anglefromprimary.IsEnabled = false;
            ParentUserControl.Instance.AlignConduits.IsChecked = false;
            ParentUserControl.Instance.Anglefromprimary.IsChecked = false;
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
                if(settings != null)
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
            }
            else
            {
                containerSupportSettings.Visibility = System.Windows.Visibility.Collapsed;
                containerProfileColorSettings.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
