#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using TIGUtility;
#endregion


namespace MultiDraw
{
    class App : IExternalApplication
    {
        public static PushButton MultiDrawButton { get; set; }


        public Result OnStartup(UIControlledApplication application)
        {
            OnButtonCreate(application);
            application.ViewActivated += Application_ViewActivated;
            application.ApplicationClosing += Application_Closing;
            return Result.Succeeded;
        }

        private void Application_Closing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            DeleteUserDatafromLocal();
        }
        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
        }

        private void DeleteUserDatafromLocal()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string tempfilePath = Path.GetDirectoryName(assembly.Location);
            DirectoryInfo di = new DirectoryInfo(tempfilePath);
            string tempfileName = Path.Combine(new DirectoryInfo(di.Parent.FullName).Parent.FullName,
                string.Format("UserData_{0}.json", Util.ProductVersion));
            if (File.Exists(tempfileName))
            {
                File.Delete(tempfileName);
            }
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            MultiDrawButton.Enabled = true;
            return Result.Succeeded;
        }

        //*****************************RibbonPanel()*****************************
        public RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            string tab = Util.AddinRibbonTabName; // Archcorp
            string ribbonPanelText = Util.AddinRibbonPanel; // Architecture

            // Empty ribbon panel 
            RibbonPanel ribbonPanel = null;
            // Try to create ribbon tab. 
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch { }
            // Try to create ribbon panel.
            try
            {
                RibbonPanel panel = a.CreateRibbonPanel(tab, ribbonPanelText);
            }
            catch { }
            // Search existing tab for your panel.
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels)
            {
                if (p.Name == ribbonPanelText)
                {
                    ribbonPanel = p;
                }
            }
            //return panel 
            return ribbonPanel;
        }


        /// <summary>
        /// Create a ribbon and panel and add a button for the revit DMS Plugin
        /// </summary>
        /// <param name="application"></param>
        private void OnButtonCreate(UIControlledApplication application)
        {
            string buttonText = Util.AddinButtonText;
            string buttonTooltip = Util.AddinButtonTooltip;

            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dllLocation = Path.Combine(executableLocation, "MultiDraw.dll");

            // Create two push buttons

            PushButtonData buttondata = new PushButtonData("MultiDrawBtn", buttonText, dllLocation, "MultiDraw.Command")
            {
                ToolTip = buttonTooltip
            };

            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/MultiDraw;component/Resources/32x32.png"));
            buttondata.LargeImage = pb1Image;

            BitmapImage pb1Image2 = new BitmapImage(new Uri("pack://application:,,,/MultiDraw;component/Resources/16x16.png"));
            buttondata.Image = pb1Image2;

            var ribbonPanel = RibbonPanel(application);
            MultiDrawButton = ribbonPanel.AddItem(buttondata) as PushButton;
            string path;
            path = System.IO.Path.GetDirectoryName(
               System.Reflection.Assembly.GetExecutingAssembly().Location);

            ContextualHelp contextHelp = new ContextualHelp(
                ContextualHelpType.ChmFile,
                path + "\\MultiDraw.html"); // hard coding for simplicity. 

            MultiDrawButton.SetContextualHelp(contextHelp);
        }
    }
}
