﻿#pragma checksum "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "741BB2C785D15F20B0235B0912576C180F26FA18F5292B302B951B64875456FA"
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
    /// KickUserControl
    /// </summary>
    public partial class KickUserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 33 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock kick;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lbl;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border border;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel container;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton rbNinetyNear;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton rbNinetyFar;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.DropDownUserControl ddlAngle;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TIGUtility.TextBoxUserControl txtOffsetFeet;
        
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
            System.Uri resourceLocater = new System.Uri("/MultiDraw;component/mvvm/view/usercontrol/kickusercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
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
            
            #line 13 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
            ((MultiDraw.KickUserControl)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Control_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 23 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Grid_MouseDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.kick = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.lbl = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.border = ((System.Windows.Controls.Border)(target));
            return;
            case 6:
            this.container = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 7:
            this.rbNinetyNear = ((System.Windows.Controls.RadioButton)(target));
            
            #line 57 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
            this.rbNinetyNear.Click += new System.Windows.RoutedEventHandler(this.SelectionMode_Changed);
            
            #line default
            #line hidden
            return;
            case 8:
            this.rbNinetyFar = ((System.Windows.Controls.RadioButton)(target));
            
            #line 67 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
            this.rbNinetyFar.Click += new System.Windows.RoutedEventHandler(this.SelectionMode_Changed);
            
            #line default
            #line hidden
            return;
            case 9:
            this.ddlAngle = ((TIGUtility.DropDownUserControl)(target));
            
            #line 80 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
            this.ddlAngle.SelectionChanged += new TIGUtility.DropDownUserControl.SelectionChangedEventHandler(this.DdlAngle_Changed);
            
            #line default
            #line hidden
            return;
            case 10:
            this.txtOffsetFeet = ((TIGUtility.TextBoxUserControl)(target));
            
            #line 87 "..\..\..\..\..\MVVM\View\UserControl\KickUserControl.xaml"
            this.txtOffsetFeet.LostFocus += new System.Windows.RoutedEventHandler(this.TextBox_LostFocus);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

