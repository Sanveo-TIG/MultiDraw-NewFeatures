   M S B A M L   `   `   `  ����D  @MultiDraw, Version=3.2.1.0, Culture=neutral, PublicKeyToken=null%clr-namespace:MultiDraw	MultiDraw  5      _ [MaterialDesignThemes.Wpf, Version=4.6.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aeHclr-namespace:MaterialDesignThemes.Wpf;assembly=MaterialDesignThemes.WpfMaterialDesignThemes.Wpf 5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5      E ATIGUtility, Version=4.2.0.0, Culture=neutral, PublicKeyToken=null;,clr-namespace:TIGUtility;assembly=TIGUtility
TIGUtility 5	          MultiDraw.KickUserControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      &ucclr-namespace:MultiDraw     5      cmaterialDesignHclr-namespace:MaterialDesignThemes.Wpf;assembly=MaterialDesignThemes.Wpf     5      9system&clr-namespace:System;assembly=mscorlib     5      <TIG,clr-namespace:TIGUtility;assembly=TIGUtility     5	      -   5        
������   � 	Resources  5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $e _pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml?�6%   �� 5      $h bpack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml?�6%   %    5      .��5      �  � RowDefinitions 5   
   �� 5      $��35 �6   6*   �� 5      $	��190 �6   6+   �� 5      $	��110 �6   6+   �� 5      $��75 �6   6*   �� 5      $��55 �6   6*   .+�5    
   �� $��0��6   .+�5!      �� -   5         ��Name$
 kick��5!      $
��Kick��5"      $��0,5,0,0q�6%   $��16(�66   6F   5#      �� 5$   
   $��1��6   .+�5(      �� -   5         ��Name$ ddlAngle��5(      $��0 0 0 0q�6(   ���#ccc$��1q�6M   ���#333333$	��310��5+      $
��Left=�5,      ��5-      Z� ��5.      �� 5/       �� 
TargetType" ��6      MaterialDesignListBoxItem �� BasedOn# [  65    �� Triggers 50      P� 51      	 P� Property	 �@��
IsSelected
 P� Value
 . . �52   "   ��  �� Property � �� �� Value �#005D9A53   #   �� 54   "    � ��
 �����6P   55       �� 	Resources 57       �� Color��%V   (��       58   .   * SystemColors.HighlightBrushKey+)�� 6   $ 	LightBlue��6`   59      .�5:      ��  � ��$ 7,1,3,5q�5;      �� 5<       � �� �#3333335=      �� 5>       � �� �#ccc5?      �� 5@       � ��$ 0,0,0,1q�5A      6   5C      5E      5F      �� 5G   
   $��2��6   .+�5I      �� -   5        $	 lbl��5I      $��Kick Options��5J      ���#005D9A$��12(�5L      $<��6/TIGUtility;component/Resources/Font/#Poppins SemiBold)�5M      $��0,0,0,4q�5N      6(   �� 5P      -   5         ��Name$ border��5P       �� Padding$ 10q�6%   $��0,4,0,0q�62   
����������#ccc$	��310��5S      $
��Left=�5T      $��1q�5U       �� CornerRadius$ 0��6%   ._�5W      �� -   5         ��Name$ 	container��5W       �� Orientation$ Vertical=�5X      $
��Left=�5Y      
�������.+�5[      	� -   5         	�Name$ rbNinetyNear��5[   "   $��	90° Near��5\   "    d� 	IsChecked$
 TrueY�5]   "   $��0,0,0,4q�5_       � ToolTip 5`      �� 5a      $ 
Horizontal=�6!   .+�5b   "   �� $0��*/MultiDraw;component/Resources/90 Near.png��6(   $	��250��6\   $	��250��6h   5c      5e      	� 5f      -   5        $ rbNinetyFar��5f   "   $��90° Far��67   $��0,0,0,4q�5g   "    5i      �� 5j      $ 
Horizontal=�6)   .+�5k   "   �� $/��)/MultiDraw;component/Resources/90 far.png��6(   $	��250��6[   $	��250��6g   5l      5n      5o      5p      5q      �� 5r   
   $��3��6   .+�5s        TIGUtility.TextBoxControl  -	   5          Name$ txtOffsetFeet��5s   !      Label$ Offset��5t   %      
UnitsLabel$ Feet and inches��5u   %      Width$	 310Q�5v   $   $
��Left=�5w   #      IsUnit . 5y   5   5z      5{      5|      