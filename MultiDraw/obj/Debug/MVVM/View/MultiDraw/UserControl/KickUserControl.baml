   M S B A M L   `   `   `  ����D  @MultiDraw, Version=3.2.1.0, Culture=neutral, PublicKeyToken=null%clr-namespace:MultiDraw	MultiDraw  5      _ [MaterialDesignThemes.Wpf, Version=4.6.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aeHclr-namespace:MaterialDesignThemes.Wpf;assembly=MaterialDesignThemes.WpfMaterialDesignThemes.Wpf 5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5      E ATIGUtility, Version=2.3.0.0, Culture=neutral, PublicKeyToken=null;,clr-namespace:TIGUtility;assembly=TIGUtility
TIGUtility 5	          MultiDraw.KickUserControl   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      &ucclr-namespace:MultiDraw     5      cmaterialDesignHclr-namespace:MaterialDesignThemes.Wpf;assembly=MaterialDesignThemes.Wpf     5      9system&clr-namespace:System;assembly=mscorlib     5      <TIG,clr-namespace:TIGUtility;assembly=TIGUtility     5	      -   5        
������   � 	Resources  5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $e _pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml?�6%   �� 5      $h bpack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml?�6%   %    5      .��5      �  � RowDefinitions 5   
   �� 5      $��45 �6   6*   �� 5      $	��110 �6   6+   �� 5      $��75 �6   6*   �� 5      $��75 �6   6*   �� 5      $��55 �6   6*   .+�5    
   �� $��0��6   .+�5!      �� -   5         ��Name$
 kick��5!      $
��Kick��5"      $��0,5,0,0q�6%   $��16(�66   6F   5#      �� 5$   
   $��1��6   .+�5&      �� -   5        $	 lbl��5&      $��Kick Options��5'      ���#005D9A$��12(�5)      $<��6/TIGUtility;component/Resources/Font/#Poppins SemiBold)�5*      $��0,0,0,4q�5+      6(   �� 5-      -   5         ��Name$ border��5-       �� Padding$ 10q�6%   $��0,4,0,0q�62   
����������#ccc$	��310��50      $
��Left=�51      $��1q�52       �� CornerRadius$ 0��6%   ._�54      �� -   5         ��Name$ 	container��54      	 �� Orientation$	 Vertical=�55      $
��Left=�56      
�������.+�58      	� -   5        
 	�Name$
 rbNinetyNear��58   "   $��	90° Near��59   "    d� 	IsChecked$
 TrueY�5:   "   $��0,0,0,4q�5<       � ToolTip 5=      �� 5>      $	 
Horizontal=�6!   .+�5?   "   �� $0��*/MultiDraw;component/Resources/90 Near.png��6(   $	��250��6\   $	��250��6h   5@      5B      	� 5C      -   5        $
 rbNinetyFar��5C   "   $��90° Far��67   $��0,0,0,4q�5D   "    5F      �� 5G      $	 
Horizontal=�6)   .+�5H   "   �� $/��)/MultiDraw;component/Resources/90 far.png��6(   $	��250��6[   $	��250��6g   5I      5K      5L      5M      5N      �� 5O   
   $��2��6   .+�5P      ,  &TIGUtility.SingleSelectDropDownControl  -   5          Name$ ddlAngle��5Q   (   $
��Left=�6:   5R   L   5T      �� 5U   
   $��3��6   .+�5V        TIGUtility.TextBoxControl  -	   5          Name$ txtOffsetFeet��5V   !      Label$ Offset��5W   %      
UnitsLabel$ Feet and inches��5X   %      Width$	 310Q�5Y   $   $
��Left=�5Z   #      IsUnit . 5\   5   5]      5^      5_      