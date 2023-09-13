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

        ExternalEvent _externalEvent;
        ProfileColorSettingsData ProfileColorSettingsData = new ProfileColorSettingsData();
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

                string json = Properties.Settings.Default.StraightsDraw.ToString();
                // Utility.SetGlobalParametersManager(uiApp, "MultiDrawSettings", json);

                if (!string.IsNullOrEmpty(json))
                {
                    StraightsDrawParam globalParam = JsonConvert.DeserializeObject<StraightsDrawParam>(json);
                   
                }
                string json2 = Properties.Settings.Default.ProfileColorSettings;

                if (string.IsNullOrEmpty(json2))
                {
                    ProfileColorSettingsData.vOffsetValue = "V Offset";
                    ProfileColorSettingsData.hOffsetValue = "H offset";
                    ProfileColorSettingsData.rOffsetValue = "R offset";
                    ProfileColorSettingsData.kOffsetValue = "K offset";
                    ProfileColorSettingsData.straightValue = "Straight/Bend";
                    ProfileColorSettingsData.nkOffsetValue = "NinetyKick";
                    ProfileColorSettingsData.nsOffsetValue = "NinetyStub";

                    ProfileColorSettingsData.vOffsetColor = new Autodesk.Revit.DB.Color(255, 255, 0);
                    ProfileColorSettingsData.hOffsetColor = new Autodesk.Revit.DB.Color(128, 128, 128);
                    ProfileColorSettingsData.rOffsetColor = new Autodesk.Revit.DB.Color(255, 153, 255);
                    ProfileColorSettingsData.kOffsetColor = new Autodesk.Revit.DB.Color(204, 229, 255);
                    ProfileColorSettingsData.straightColor = new Autodesk.Revit.DB.Color(255, 153, 204);
                    ProfileColorSettingsData.nkOffsetColor = new Autodesk.Revit.DB.Color(204, 204, 255);
                    ProfileColorSettingsData.nsOffsetColor = new Autodesk.Revit.DB.Color(153, 76, 0);
                }
                else
                {
                   ProfileColorSettingsData = JsonConvert.DeserializeObject<ProfileColorSettingsData>(json2);
                }

                VoffsetValue.Text = ProfileColorSettingsData.vOffsetValue;
                VoffsetColor.Value = ProfileColorSettingsData.vOffsetColor;

                HoffsetValue.Text = ProfileColorSettingsData.hOffsetValue;
                HoffsetColor.Value = ProfileColorSettingsData.hOffsetColor;

                RoffsetValue.Text = ProfileColorSettingsData.rOffsetValue;
                RoffsetColor.Value = ProfileColorSettingsData.rOffsetColor;

                KoffsetValue.Text = ProfileColorSettingsData.kOffsetValue;
                Kick90offsetColor.Value = ProfileColorSettingsData.kOffsetColor;

                StraightValue.Text = ProfileColorSettingsData.straightValue;
                Strightor90Color.Value = ProfileColorSettingsData.straightColor;

                NightyKickValue.Text = ProfileColorSettingsData.nkOffsetValue;
                NinetykickdrawColor.Value = ProfileColorSettingsData.nkOffsetColor;

                NightystubValue.Text = ProfileColorSettingsData.nsOffsetValue;
                ninetystubColor.Value = ProfileColorSettingsData.nsOffsetColor;



            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occurred. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }



        private void vOffsetvaluechange(object sender)
        {
            ProfileColorSettingsData.vOffsetValue = VoffsetValue.Text;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void vOffsetcolorchanged(object sender)
        {
            ProfileColorSettingsData.vOffsetColor = VoffsetColor.Value;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void hOffsetColor(object sender)
        {
            ProfileColorSettingsData.hOffsetColor = HoffsetColor.Value;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void hOffsetValue(object sender)
        {
            ProfileColorSettingsData.hOffsetValue = HoffsetValue.Text;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void RoffsetValue_TextBox_Changed(object sender)
        {
            ProfileColorSettingsData.rOffsetValue = RoffsetValue.Text;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void RoffsetColor_DropDownClosed(object sender)
        {
            ProfileColorSettingsData.rOffsetColor = RoffsetColor.Value;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void KoffsetValue_TextBox_Changed(object sender)
        {
            ProfileColorSettingsData.kOffsetValue = KoffsetValue.Text;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void Kick90offsetColor_DropDownClosed(object sender)
        {
            ProfileColorSettingsData.kOffsetColor = Kick90offsetColor.Value;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void StraightValue_TextBox_Changed(object sender)
        {
            ProfileColorSettingsData.straightValue = StraightValue.Text;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void Strightor90Color_DropDownClosed(object sender)
        {
            ProfileColorSettingsData.straightColor = Strightor90Color.Value;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void NightyKickValue_TextBox_Changed(object sender)
        {
            ProfileColorSettingsData.nkOffsetValue = NightyKickValue.Text;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void NinetykickdrawColor_DropDownClosed(object sender)
        {
            ProfileColorSettingsData.nkOffsetColor = NinetykickdrawColor.Value;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void NightystubValue_TextBox_Changed(object sender)
        {
            ProfileColorSettingsData.nsOffsetValue = NightystubValue.Text;
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void ninetystubColor_DropDownClosed(object sender)

        {
            ProfileColorSettingsData.nsOffsetColor = ninetystubColor.Value;
            var s = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.ProfileColorSettings = JsonConvert.SerializeObject(ProfileColorSettingsData);
            Properties.Settings.Default.Save();
        }

        private void btnSave_BtnClick(object sender)
        {
            _externalEvent.Raise();
        }
    }
}

