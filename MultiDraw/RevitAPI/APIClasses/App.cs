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
using adWin = Autodesk.Windows;
using TIGUtility;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Configuration.Assemblies;
#endregion


namespace MultiDraw
{
    class App : IExternalApplication
    {
        public static PushButton MultiDrawButton { get; set; }
        public static PushButton SettingButton { get; set; }


        public Result OnStartup(UIControlledApplication application)
        {
            OnButtonCreate(application);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(DocumentFormatAssemblyLoad);
            application.ViewActivated += Application_ViewActivated;
            application.ApplicationClosing += Application_Closing;

            return Result.Succeeded;
        }
        void ComponentManager_UIElementActivated(
  object sender,
  adWin.UIElementActivatedEventArgs e)
        {
            if (e != null
              && e.Item != null
              && e.Item.Id != null
              && e.Item.Id == "ID_TBC_BUTTON")
            {
                // Perform the button action

                // Local file

                string path = System.Reflection.Assembly
                  .GetExecutingAssembly().Location;

                path = Path.Combine(
                  Path.GetDirectoryName(path),
                  "test.html");

                // Internet URL

                path = "http://thebuildingcoder.typepad.com";

                Process.Start(path);
            }
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



            //Autodesk.Revit.UI.RibbonPanel panel = application.CreateRibbonPanel("MultiDraw");
            //panel.Title = tab.Title;

            ribbonPanel.AddSlideOut();
            /*  PushButtonData b1 = new PushButtonData(
               "MyButton", "My Button",
                 dllLocation,
               "MultiDraw.CommandSettings");
              PushButton pushButton = ribbonPanel.AddItem(b1) as PushButton;
              pushButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/MultiDraw;component/Resources/32x32.png"));
              MultiDrawButton = pushButton;*/
            PushButtonData multidrawsupportbuttondata = new PushButtonData("MultidrawSupportBtn", "MultiDrawSetting", dllLocation, "MultiDraw.CommandSettings")
            {
                ToolTip = buttonTooltip
            };
            BitmapImage multidrawsupportpb1Image = new BitmapImage(new Uri("pack://application:,,,/MultiDraw;component/Resources/32x32.png"));
            multidrawsupportbuttondata.LargeImage = multidrawsupportpb1Image;

            BitmapImage multidrawsupportpb1Image2 = new BitmapImage(new Uri("pack://application:,,,/MultiDraw;component/Resources/16x16.png"));
            multidrawsupportbuttondata.Image = multidrawsupportpb1Image2;

            MultiDrawButton = ribbonPanel.AddItem(multidrawsupportbuttondata) as PushButton;
            path = System.IO.Path.GetDirectoryName(
               System.Reflection.Assembly.GetExecutingAssembly().Location);

            ContextualHelp contextHelpMultidrawSetting = new ContextualHelp(
                ContextualHelpType.ChmFile,
                path + "\\MultiDraw.html"); // hard coding for simplicity. 

            MultiDrawButton.SetContextualHelp(contextHelpMultidrawSetting);

            //// Create primary button
            //SplitButton splitButton = ribbonPanel.AddItem(buttondata) as SplitButton;

            //// Add dropdown items
            //PushButtonData dropdownItem1 = new PushButtonData("DropdownItem1", "Dropdown Item 1", assemblyPath, "Namespace.DropdownCommand1");
            //splitButton.AddPushButton(dropdownItem1);

            //PushButtonData dropdownItem2 = new PushButtonData("DropdownItem2", "Dropdown Item 2", assemblyPath, "Namespace.DropdownCommand2");
            //splitButton.AddPushButton(dropdownItem2);
            //adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
            //foreach (adWin.RibbonTab tab in ribbon.Tabs)
            //{
            //    if (tab.Id == "Sanveo Tools-Beta")
            //    {
            //        /*string tabs = Util.AddinRibbonTabName; // Archcorp
            //        string ribbonPanelText = Util.AddinRibbonPanel;*/
            //        Autodesk.Revit.UI.RibbonPanel panel = application.CreateRibbonPanel("MultiDraw");
            //        panel.Title = tab.Title;
            //        panel.AddSlideOut();
            //        PushButtonData buttonData = new PushButtonData(
            //         "MyButton", "My Button",
            //         "MultiDraw.dll",
            //         "MultiDraw.Command");
            //        PushButton pushButton = panel.AddItem(buttonData) as PushButton;
            //        pushButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/MultiDraw;component/Resources/32x32.png"));
            //    }
            //}
        }
        public static Assembly DocumentFormatAssemblyLoad(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("resources"))
            {
                return null;
            }
            if (args.Name.Contains("TIGUtility"))
            {
                string assemblyPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\TIGUtility.dll";
                var assembly = Assembly.Load(assemblyPath);
                return assembly;
            }
            return null;
        }
    }
}
