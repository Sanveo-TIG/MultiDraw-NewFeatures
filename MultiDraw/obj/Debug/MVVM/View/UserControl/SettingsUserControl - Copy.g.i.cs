﻿#pragma checksum "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "FAC01B4FC1321F7E00E9FD4A651876250C5B2888AD6A51C8297189AC5660A43B"
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
    /// SettingsUserControl
    /// </summary>
    public partial class SettingsUserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 98 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock settings;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox IsSupportNeeded;
        
        #line default
        #line hidden
        
        
        #line 122 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.DropDownUserControl ddlStrutType;
        
        #line default
        #line hidden
        
        
        #line 148 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxUserControl txtSupportSpacing;
        
        #line default
        #line hidden
        
        
        #line 160 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxUserControl txtRodDia;
        
        #line default
        #line hidden
        
        
        #line 175 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxUserControl txtRodExtension;
        
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
            System.Uri resourceLocater = new System.Uri("/MultiDraw;component/mvvm/view/usercontrol/settingsusercontrol%20-%20copy.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
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
            
            #line 13 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
            ((MultiDraw.SettingsUserControl)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Control_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.settings = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.IsSupportNeeded = ((System.Windows.Controls.CheckBox)(target));
            
            #line 112 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
            this.IsSupportNeeded.Checked += new System.Windows.RoutedEventHandler(this.OnChangeSupportNeeded);
            
            #line default
            #line hidden
            
            #line 113 "..\..\..\..\..\MVVM\View\UserControl\SettingsUserControl - Copy.xaml"
            this.IsSupportNeeded.Unchecked += new System.Windows.RoutedEventHandler(this.OnChangeSupportNeeded);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ddlStrutType = ((TIGUtility.DropDownUserControl)(target));
            return;
            case 5:
            this.txtSupportSpacing = ((TIGUtility.TextBoxUserControl)(target));
            return;
            case 6:
            this.txtRodDia = ((TIGUtility.TextBoxUserControl)(target));
            return;
            case 7:
            this.txtRodExtension = ((TIGUtility.TextBoxUserControl)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

