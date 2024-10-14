using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using TIGUtility;

namespace MultiDraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region UI
        bool _isCancel = false;
        bool _isStoped = false;
        public async void EscFunction(Window window)
        {
            bool isValid = await Loop();
            if (isValid)
            {
            
                _isCancel = false;
                _isStoped = false;
                window.Close();
            }

        }
        public async Task<bool> Loop()
        {
            await Task.Run(() =>
            {
                do
                {
                    try
                    {

                        _isStoped = _isCancel || _isStoped;
                    }
                    catch (Exception)
                    {
                    }
                }
                while (!_isStoped);

            });
            return _isStoped;
        }
        private void InitializeMaterialDesign()
        {
            //var card = new Card();
            var hue = new Hue("Dummy", Colors.Black, Colors.White);
        }

        private void InitializeWindowProperty()
        {
            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/MultiDraw;component/Resources/16x16.png"));
            this.Icon = pb1Image;
            this.Title = Util.ApplicationWindowTitle;
            this.MinHeight = Util.ApplicationWindowHeight;
            this.Height = Util.ApplicationWindowHeight;
            this.Topmost = Util.IsApplicationWindowTopMost;
            this.MinWidth = Util.IsApplicationWindowAlowToReSize ? Util.ApplicationWindowWidth : 100;
            this.Width = Util.ApplicationWindowWidth;
            this.ResizeMode = Util.IsApplicationWindowAlowToReSize ? System.Windows.ResizeMode.CanResize : System.Windows.ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderPanel.Instance = this;
            GetMainWindowLocation();
        }

        System.Windows.Forms.Screen Screen;
        
        private void GetMainWindowLocation()
        {
            Screen = System.Windows.Forms.Screen.FromRectangle(
               new System.Drawing.Rectangle((int)Instance.Left, (int)Instance.Top, (int)Instance.Width, (int)Instance.Height));
            Left = Screen.WorkingArea.Right - (Screen.WorkingArea.Width * 0.25);
            Top = Screen.WorkingArea.Top + (Screen.WorkingArea.Height * 0.25);
            if (Screen.Primary)
            {
                Left = Screen.WorkingArea.Right - (Screen.WorkingArea.Width * 0.4);
                Top = Screen.WorkingArea.Top + (Screen.WorkingArea.Height * 0.25);
            }

            // Load saved settings
            string locationDetails = Properties.Settings.Default.WindowLocation;
            if (!string.IsNullOrEmpty(locationDetails))
            {
                WindowProperty WP = JsonConvert.DeserializeObject<WindowProperty>(locationDetails);
                if (WP.IsPrimaryScreen && Screen.Primary)
                {
                    Left = WP.Left;
                    Top = WP.Top;
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveWindowPosition();
        }

        private void SaveWindowPosition()
        {
            Screen = System.Windows.Forms.Screen.FromRectangle(
               new System.Drawing.Rectangle((int)Instance.Left, (int)Instance.Top, (int)Instance.Width, (int)Instance.Height));
            WindowProperty pos = new WindowProperty
            {
                Left = Left,
                Top = Top,
                IsPrimaryScreen = Screen.Primary
            };
            string json = JsonConvert.SerializeObject(pos);
            Properties.Settings.Default.WindowLocation = json;
            Properties.Settings.Default.Save();
        }
        #endregion

        public static MainWindow Instance;
        public UIApplication _UIApp = null;
        readonly List<ExternalEvent> _externalEvents = new List<ExternalEvent>();
        public MainWindow(CustomUIApplication application)
        {
            InitializeWindowProperty();
            InitializeMaterialDesign();
            InitializeComponent();
            InitializeHandlers();
            Instance = this;
            HeaderPanel.Instance = this;
            HeaderPanel.HideMaxWindow = true;
            _isCancel = false;
            FooterPanel.Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            System.Windows.Controls.UserControl userControl = new ParentUserControl(_externalEvents, application, this);
            Container.Children.Add(userControl);
        }
        private void InitializeHandlers()
        {
            _externalEvents.Add(ExternalEvent.Create(new MultiDrawHandler()));
            _externalEvents.Add(ExternalEvent.Create(new SettingsHandler()));
        }

        private void Key_PressEvent(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key != Key.Enter)
            {
                switch (e.Key)
                {
                    case Key.K:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 3;
                        break;
                    case Key.D:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 4;
                        break;
                    case Key.H:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 1;
                        break;
                    case Key.S:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 7;
                        break;
                    case Key.V:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 0;
                        break;
                    case Key.R:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 2;
                        break;
                    case Key.U:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 5;
                        break;
                    case Key.N:
                        ParentUserControl.Instance.cmbProfileType.SelectedIndex = 6;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
