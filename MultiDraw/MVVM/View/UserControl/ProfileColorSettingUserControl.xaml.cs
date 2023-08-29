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
using System.Data;
using System.Collections.ObjectModel;
using TIGUtility;
using MultiDraw.RevitAPI.APIHandler;

namespace MultiDraw
{
    /// <summary>
    /// UI Events
    /// </summary>
    public partial class ProfileColorSettingUserControl : UserControl
    {
        public static ProfileColorSettingUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        Document _doc = null;
        UIDocument _uidoc = null;

        ExternalEvent _externalEvent = null;
        public ProfileColorSettingUserControl(ExternalEvent externalEvent, UIApplication uiApp, Window window)
        {
            _uidoc = uiApp.ActiveUIDocument;
            _doc = _uidoc.Document;

            _externalEvent = externalEvent;
            InitializeComponent();
            Instance = this;
            try
            {
                _window = window;
               

       
                //ProfileColorSettingsData obj = ParentUserControl.Instance._ProfileColorSettingsData;
                //VoffsetValue.Text = obj.vOffsetValue;
                //VoffsetColor.Value = obj.vOffsetColor;

                //HoffsetValue.Text = obj.hOffsetValue;
                //HoffsetColor.Value = obj.hOffsetColor;

                //RoffsetValue.Text = obj.rOffsetValue;
                //RoffsetColor.Value = obj.rOffsetColor;

                //KoffsetValue.Text = obj.kOffsetValue;
                //Kick90offsetColor.Value = obj.kOffsetColor;

                //StraightValue.Text = obj.straightValue;
                //Strightor90Color.Value = obj.straightColor;

                //NightyKickValue.Text = obj.nkOffsetValue;
                //NinetykickdrawColor.Value = obj.nkOffsetColor;

                //NightystubValue.Text = obj.nsOffsetValue;
                //ninetystubColor.Value = obj.nsOffsetColor;



            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

      

        //private void vOffsetvaluechange(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.vOffsetValue = VoffsetValue.Text;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void vOffsetcolorchanged(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.vOffsetColor = VoffsetColor.Value;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void hOffsetColor(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.hOffsetColor = HoffsetColor.Value;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void hOffsetValue(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.hOffsetValue = HoffsetValue.Text;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void RoffsetValue_TextBox_Changed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.rOffsetValue = RoffsetValue.Text;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void RoffsetColor_DropDownClosed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.rOffsetColor = RoffsetColor.Value;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void KoffsetValue_TextBox_Changed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.kOffsetValue = KoffsetValue.Text;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void Kick90offsetColor_DropDownClosed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.kOffsetColor = Kick90offsetColor.Value;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void StraightValue_TextBox_Changed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.straightValue = StraightValue.Text;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void Strightor90Color_DropDownClosed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.straightColor = Strightor90Color.Value;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void NightyKickValue_TextBox_Changed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.nkOffsetValue = NightyKickValue.Text;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void NinetykickdrawColor_DropDownClosed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.nkOffsetColor = NinetykickdrawColor.Value;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void NightystubValue_TextBox_Changed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.nsOffsetValue = NightystubValue.Text;
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}

        //private void ninetystubColor_DropDownClosed(object sender)
        //{
        //    ParentUserControl.Instance._ProfileColorSettingsData.nsOffsetColor = ninetystubColor.Value;
        //    var s = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ParentUserControl.Instance._ProfileColorSettingsData);
        //    Properties.Settings.Default.Save();
        //}
    }
}

