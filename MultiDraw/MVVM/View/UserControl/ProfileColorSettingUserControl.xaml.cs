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

                string json = Utility.GetGlobalParametersManager(_doc, "MultiDrawProfileSettings");
                if (!string.IsNullOrEmpty(json))
                {
                    MultiDrawProfileSettingsParam globalParam = JsonConvert.DeserializeObject<MultiDrawProfileSettingsParam>(json);
                    VoffsetValue.Text = globalParam.VoffsetValue;
                    HoffsetValue.Text = globalParam.HoffsetValue;
                    RoffsetValue.Text = globalParam.RoffsetValue;
                    KoffsetValue.Text = globalParam.KoffsetValue;
                    StraightValue.Text = globalParam.Straight;
                    NightyKickValue.Text = globalParam.NinetyKick;
                    NightystubValue.Text = globalParam.Ninetystub;

                    MultiDrawProfileSettingsParam globalParamC = JsonConvert.DeserializeObject<MultiDrawProfileSettingsParam>(json);

                    if (!string.IsNullOrEmpty(globalParamC.VoffsetValuecolor))
                    {
                        string[] strings = globalParamC.VoffsetValuecolor.Split(',');
                        VoffsetColor.Value = new Autodesk.Revit.DB.Color(Convert.ToByte(strings[0]), Convert.ToByte(strings[1]), Convert.ToByte(strings[2]));
                    }

                    if (!string.IsNullOrEmpty(globalParamC.HoffsetValuecolor))
                    {
                        string[] strings = globalParamC.HoffsetValuecolor.Split(',');
                        HoffsetColor.Value = new Autodesk.Revit.DB.Color(Convert.ToByte(strings[0]), Convert.ToByte(strings[1]), Convert.ToByte(strings[2]));
                    }

                    if (!string.IsNullOrEmpty(globalParamC.RoffsetValuecolor))
                    {
                        string[] strings = globalParamC.RoffsetValuecolor.Split(',');
                        RoffsetColor.Value = new Autodesk.Revit.DB.Color(Convert.ToByte(strings[0]), Convert.ToByte(strings[1]), Convert.ToByte(strings[2]));
                    }

                    if (!string.IsNullOrEmpty(globalParamC.KoffsetValuecolor))
                    {
                        string[] strings = globalParamC.KoffsetValuecolor.Split(',');
                        Kick90offsetColor.Value = new Autodesk.Revit.DB.Color(Convert.ToByte(strings[0]), Convert.ToByte(strings[1]), Convert.ToByte(strings[2]));
                    }

                    if (!string.IsNullOrEmpty(globalParamC.Straightcolor))
                    {
                        string[] strings = globalParamC.Straightcolor.Split(',');
                        Strightor90Color.Value = new Autodesk.Revit.DB.Color(Convert.ToByte(strings[0]), Convert.ToByte(strings[1]), Convert.ToByte(strings[2]));
                    }

                    if (!string.IsNullOrEmpty(globalParamC.NinetyKickcolor))
                    {
                        string[] strings = globalParamC.NinetyKickcolor.Split(',');
                        NinetykickdrawColor.Value = new Autodesk.Revit.DB.Color(Convert.ToByte(strings[0]), Convert.ToByte(strings[1]), Convert.ToByte(strings[2]));
                    }

                    if (!string.IsNullOrEmpty(globalParamC.Ninetystubcolor))
                    {
                        string[] strings = globalParamC.Ninetystubcolor.Split(',');
                        ninetystubColor.Value = new Autodesk.Revit.DB.Color(Convert.ToByte(strings[0]), Convert.ToByte(strings[1]), Convert.ToByte(strings[2]));
                    }
                }

                else
                {
                    VoffsetValue.Text = "V offset";
                    HoffsetValue.Text = "H offset";
                    RoffsetValue.Text = "R offset";
                    KoffsetValue.Text = "K offset";
                    StraightValue.Text = "Straight/Bend";
                    NightyKickValue.Text = "NinetyKick";
                    NightystubValue.Text = "NinetyStub";

                    VoffsetColor.Value = new Autodesk.Revit.DB.Color(255, 255, 0);
                    HoffsetColor.Value = new Autodesk.Revit.DB.Color(128, 128, 128);
                    RoffsetColor.Value = new Autodesk.Revit.DB.Color(255, 153, 255);
                    Kick90offsetColor.Value = new Autodesk.Revit.DB.Color(204, 229, 255);
                    Strightor90Color.Value = new Autodesk.Revit.DB.Color(255, 153, 204);
                    NinetykickdrawColor.Value = new Autodesk.Revit.DB.Color(204, 204, 255);
                    ninetystubColor.Value = new Autodesk.Revit.DB.Color(153, 76, 0);
                }



                List<MultiSelect> angleList = new List<MultiSelect>();



            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }



    }
}

