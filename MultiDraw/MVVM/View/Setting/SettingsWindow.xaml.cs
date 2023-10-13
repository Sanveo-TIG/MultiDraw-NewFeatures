using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using MaterialDesignColors;
using Newtonsoft.Json;
using TIGUtility;

namespace MultiDraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
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
            this.Title = Utili.ApplicationWindowTitle;
            this.MinHeight = Utili.ApplicationWindowHeight;
            this.Height = Utili.ApplicationWindowHeight;
            this.Topmost = Utili.IsApplicationWindowTopMost;
            this.MinWidth = Utili.IsApplicationWindowAlowToReSize ? Utili.ApplicationWindowWidth : 100;
            this.Width = Utili.ApplicationWindowWidth;
            this.ResizeMode = Utili.IsApplicationWindowAlowToReSize ? System.Windows.ResizeMode.CanResize : System.Windows.ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None;         
            string tempfilePath = System.IO.Path.GetDirectoryName(typeof(Command).Assembly.Location);
            DirectoryInfo di = new DirectoryInfo(tempfilePath);
            string tempfileName = System.IO.Path.Combine(di.FullName, "WindowProperty.txt");
            if (File.Exists(tempfileName))
            {
                WindowProperty property = new WindowProperty();
                using (StreamReader reader = new StreamReader(tempfileName))
                {
                    string jsonFromFile = reader.ReadToEnd();
                    property = JsonConvert.DeserializeObject<WindowProperty>(jsonFromFile);
                }
                this.Top = property.Top;
                this.Left = property.Left;
                int width = Screen.PrimaryScreen.Bounds.Width;
                Screen[] Screens = Screen.AllScreens;
                if(Screens != null && this.Left >= width && Screens.Length == 1)
                {
                    foreach(Screen screen in Screens)
                    {
                        if(this.Left >= screen.Bounds.Width)
                        {
                            this.Left -= screen.Bounds.Width;
                            break;
                        }
                    }
                }
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderPanel.Instance = this;
        }
        #endregion

        public static SettingsWindow Instance;
        public UIApplication _UIApp = null;
        public List<ExternalEvent> _externalEvents = new List<ExternalEvent>();
        public SettingsWindow(CustomUIApplication application)
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
            System.Windows.Controls.UserControl userControl = new SettingsUserControl(application.UIApplication.ActiveUIDocument.Document,application.UIApplication,this, _externalEvents[1]);
            Container.Children.Add(userControl);
        }
        private void InitializeHandlers()
        {
            _externalEvents.Add(ExternalEvent.Create(new MultiDrawHandler()));
            _externalEvents.Add(ExternalEvent.Create(new SettingsHandler()));
            _externalEvents.Add(ExternalEvent.Create(new ConduitSyncHandler()));
        }

       
    }
}
