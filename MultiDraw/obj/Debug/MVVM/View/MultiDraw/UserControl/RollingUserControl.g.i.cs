﻿#pragma checksum "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "FDFCE757753568D5F996402287DD1FF0014A353CFC63698689AC20AC5BAA6D15"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MaterialDesignThemes.Wpf;
using MultiDraw;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TIGUtility;


namespace MultiDraw {
    
    
    /// <summary>
    /// RollingUserControl
    /// </summary>
    public partial class RollingUserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 32 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock rollingoffset;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.SingleSelectDropDownControl ddlAngle;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lbo;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxControl txtOffsetFeet;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lbl;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxControl txtRollFeet;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MultiDraw;component/mvvm/view/multidraw/usercontrol/rollingusercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 13 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
            ((MultiDraw.RollingUserControl)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Control_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 23 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Grid_MouseDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.rollingoffset = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.ddlAngle = ((TIGUtility.SingleSelectDropDownControl)(target));
            
            #line 38 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
            this.ddlAngle.DropDownClosed += new TIGUtility.SingleSelectDropDownControl.RoutedEventHandler(this.DdlAngle_Changed);
            
            #line default
            #line hidden
            return;
            case 5:
            this.lbo = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.txtOffsetFeet = ((TIGUtility.TextBoxControl)(target));
            
            #line 56 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
            this.txtOffsetFeet.LostFocus += new System.Windows.RoutedEventHandler(this.TextBox_LostFocus);
            
            #line default
            #line hidden
            return;
            case 7:
            this.lbl = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.txtRollFeet = ((TIGUtility.TextBoxControl)(target));
            
            #line 81 "..\..\..\..\..\..\MVVM\View\MultiDraw\UserControl\RollingUserControl.xaml"
            this.txtRollFeet.LostFocus += new System.Windows.RoutedEventHandler(this.TextBox_LostFocus);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

