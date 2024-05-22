#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Windows;
using MaterialDesignThemes.Wpf;
using TIGUtility;
using Window = System.Windows.Window;
#endregion

namespace MultiDraw
{
    [Transaction(TransactionMode.Manual)]

    /// <summary>
    /// External command mainline
    /// </summary>
    public class Command : IExternalCommand
    {
        /// <summary>
        /// External command mainline
        /// </summary>
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                if(true) //(Utility.HasValidLicense(Util.ProductVersion))
                {
                    if(true)//(Utility.ReadPremiumLicense(Util.ProjectName))
                    {
                        CustomUIApplication customUIApplication = new CustomUIApplication
                        {
                            CommandData = commandData
                        };
                        if (Keyboard.Modifiers.ToString() != ModifierKeys.Shift.ToString())
                        {
                            System.Windows.Window window = new MainWindow(customUIApplication);
                            window.Show();
                            window.Closed += OnClosing;
                            if (App.MultiDrawButton != null)
                                App.MultiDrawButton.Enabled = false;
                        }
                        else
                        {
                            MessageBox.Show("You dont have access to Premium Tool");
                        }
                    }
                    else
                    {
                        var e = ExternalEvent.Create(new SettingsHandler());
                        Window SettingsWindow = new Window();
                        SettingsUserControl settings = new SettingsUserControl(commandData.Application.ActiveUIDocument.Document, commandData.Application, SettingsWindow, e);
                        SettingsWindow.ResizeMode = ResizeMode.NoResize;
                        SettingsWindow.WindowStyle = WindowStyle.None;
                        SettingsWindow.Height = settings.Height;
                        SettingsWindow.Width = settings.Width;
                        SettingsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        SettingsWindow.Content = settings;
                        SettingsWindow.Show();
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        public void OnClosing(object senMultiDrawr, EventArgs e)
        {
            if (App.MultiDrawButton != null)
                App.MultiDrawButton.Enabled = true;
            Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
            foreach (Autodesk.Windows.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Title.Equals(Util.AddinRibbonTabName))
                {
                    foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
                    {

                        if (panel.Source.AutomationName == Util.AddinRibbonPanel)
                        {
                            RibbonItemCollection collctn = panel.Source.Items;
                            foreach (Autodesk.Windows.RibbonItem ri in collctn)
                            {
                                if (ri is RibbonRowPanel)
                                {
                                    foreach (var item in (ri as RibbonRowPanel).Items)
                                    {
                                        if (item is Autodesk.Windows.RibbonButton)
                                        {
                                            if (item.AutomationName == Util.AddinButtonText)
                                            {
                                                item.IsEnabled = true;
                                            }

                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }
    }


}
