﻿#pragma checksum "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "16743A23C91D9F13FC7E42B87AC74BB2C06BE94BD202E895E81E1DE3E2D0C7A7"
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
    /// NinetyKickUserControl
    /// </summary>
    public partial class NinetyKickUserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 30 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock NinetyKick;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.DropDownUserControl ddlAngle;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxUserControl txtOffset;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxUserControl txtRise;
        
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
            System.Uri resourceLocater = new System.Uri("/MultiDraw;component/mvvm/view/usercontrol/ninetykickusercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
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
            
            #line 13 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
            ((MultiDraw.NinetyKickUserControl)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Control_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 22 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Grid_MouseDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.NinetyKick = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.ddlAngle = ((TIGUtility.DropDownUserControl)(target));
            
            #line 34 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
            this.ddlAngle.SelectionChanged += new TIGUtility.DropDownUserControl.SelectionChangedEventHandler(this.DdlAngle_Changed);
            
            #line default
            #line hidden
            return;
            case 5:
            this.txtOffset = ((TIGUtility.TextBoxUserControl)(target));
            
            #line 41 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
            this.txtOffset.LostFocus += new System.Windows.RoutedEventHandler(this.TextBox_LostFocus);
            
            #line default
            #line hidden
            return;
            case 6:
            this.txtRise = ((TIGUtility.TextBoxUserControl)(target));
            
            #line 48 "..\..\..\..\..\MVVM\View\UserControl\NinetyKickUserControl.xaml"
            this.txtRise.LostFocus += new System.Windows.RoutedEventHandler(this.TextBox_LostFocus);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
