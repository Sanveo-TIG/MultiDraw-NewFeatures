﻿#pragma checksum "..\..\..\..\..\MVVM\View\UserControl\SyncDataUserControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "27C467EDD39DC9962446A63B06AF46C9850F6695A177324795815E6463599E89"
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
    /// SyncDataUserControl
    /// </summary>
    public partial class SyncDataUserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 29 "..\..\..\..\..\MVVM\View\UserControl\SyncDataUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.MultiSelectUserControl ucMultiSelect;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\..\MVVM\View\UserControl\SyncDataUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.ButtonUserControl btnsync;
        
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
            System.Uri resourceLocater = new System.Uri("/MultiDraw;component/mvvm/view/usercontrol/syncdatausercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\MVVM\View\UserControl\SyncDataUserControl.xaml"
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
            this.ucMultiSelect = ((TIGUtility.MultiSelectUserControl)(target));
            
            #line 32 "..\..\..\..\..\MVVM\View\UserControl\SyncDataUserControl.xaml"
            this.ucMultiSelect.DropDownClosed += new TIGUtility.MultiSelectUserControl.EventHandler(this.ucMultiSelect_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 2:
            this.btnsync = ((TIGUtility.ButtonUserControl)(target));
            
            #line 41 "..\..\..\..\..\MVVM\View\UserControl\SyncDataUserControl.xaml"
            this.btnsync.BtnClick += new TIGUtility.ButtonUserControl.RoutedEventHandler(this.btnCheck_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

