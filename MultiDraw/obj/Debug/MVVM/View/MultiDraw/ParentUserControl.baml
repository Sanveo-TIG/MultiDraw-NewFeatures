   M S B A M L   `   `   `  ����D  @MultiDraw, Version=3.2.1.0, Culture=neutral, PublicKeyToken=null%clr-namespace:MultiDraw	MultiDraw  5      _ [MaterialDesignThemes.Wpf, Version=4.6.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aeHclr-namespace:MaterialDesignThemes.Wpf;assembly=MaterialDesignThemes.WpfMaterialDesignThemes.Wpf 5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5      E ATIGUtility, Version=4.2.0.0, Culture=neutral, PublicKeyToken=null;,clr-namespace:TIGUtility;assembly=TIGUtility
TIGUtility 5	      !    MultiDraw.ParentUserControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      &ucclr-namespace:MultiDraw     5      cmaterialDesignHclr-namespace:MaterialDesignThemes.Wpf;assembly=MaterialDesignThemes.Wpf     5      9system&clr-namespace:System;assembly=mscorlib     5      <TIG,clr-namespace:TIGUtility;assembly=TIGUtility     5	      -   5        
������   � 	Resources  5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $e _pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml?�6%   �� 5      $h bpack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml?�6%   %    5      .��5      � -   5        
������  � RowDefinitions 5   
   �� 5      $��50 �6   6*   �� 5      $��* �6   6)   �� 5      $��50 �6   6*   .+�5    
   �� $��0��6   $
��Left=�6"    �� Orientation$ 
Horizontal=�6=   .+�5!      �� -   5         ��Name$ cmbProfileType��5"      
����������#ccc$��1q�5!   =   $	��242��5"   ?   $��35��6K   ��6.   Z�   5$       �� 
TargetType �� Property �� Value	 �� 
TargetType
 ��Name �� Padding J� SnapsToDevicePixels �� Triggers ]� 
Conditions �� Property �� Value �� 
TargetName��%&  '��       �� 5%      " ��6   .�5&      ��  � ��$ 0,0,1,0q�5'   !   �� 5(       � �� �#ccc5)   !   �� 5*       � �� 5+      �� 5,   "   "	 ��62   .R�5-   &   �� $
 Bd��6-   #��z��5.   -   #��z��5/   -   #��z��50   -   # z��51   -    . ._�53   *   �� #��z��6;   # z 54   ;   #��z��55   ;   56   '    57   &   ]� 58   *    59   .   �� 5:   2    �@��IsSelectionActive .  5;   <   �� 5<   2    �@��
IsSelected . 5=   <   ./�5?   .   �� $ Bd��5@   5    � �� �#9995A   5   5B   +   ]� 5C   *    5D   .   �� 5E   2    �@��IsSelectionActive . 5F   <   �� 5G   2    �@��
IsSelected . 5H   <   ./�5J   .   �� $ Bd��5K   5    � �� �#9995L   5   5M   +   5O   #   5Q      5R      5S      ��5c      �� 5d      .R�5e      �  K� IsItemsHost . 6$   5f      .@�5i      �� .��5j      �� $��25��6    � ToolTip$ Vertical Offset��5k      $��Center=�5l   $    � Cursor$
 Hand��5m   $   $A��;/MultiDraw;component/Images/icons/Vertical Draw 120x120.png��5n   $   6k   5o      �� 5p      .��5q      �� $��25��6   $ Horizontal Offset��5r      $��Center=�5s      $
 Hand��5t   $   $C��=/MultiDraw;component/Images/icons/Horizontal Draw 120x120.png��5u   $   6m   5v      �� 5w      .��5x      �� $��25��6   $��Center=�6(   $
 Hand��5y      $ Rolling Offset��5z      $@��:/MultiDraw;component/Images/icons/Rolling Draw 120x120.png��5{      6b   5|      �� 5}      .��5~      �� $��25��6   $��Center=�5   $   $
 Hand��5�      $
 Kick��5�      $=��7/MultiDraw;component/Images/icons/Kick Draw 120x120.png��5�      6_   5�      �� 5�      .��5�      �� $��25��6   $��Center=�5�   $   $
 Hand��5�      $ Straight/Bend��5�      $=��7/MultiDraw;component/Images/icons/Straights 120x120.png��5�      6_   5�      �� 5�      .��5�      �� $��25��6   $��Center =�5�      $ Kick 90��5�      $
 Hand��5�      $B��</MultiDraw;component/Images/icons/90's Kick Draw 120x120.png��5�   $   6l   5�      �� 5�      .��5�      �� $��25��6   $ Stub 90��5�      $
 Hand��5�      $B��</MultiDraw;component/Images/icons/90's Stub Draw 120x120.png��5�   $   6l   5�      �� 5�      $y�	Collapsed=�6   .��5�      �� $��25��6   $ Three Point Saddle��5�      $
 Hand��5�      $B��</MultiDraw;component/Images/icons/90's Stub Draw 120x120.png��5�      6e   5�      �� 5�      $y�	Collapsed=�6   .��5�      �� $��25��6   $ Four Point Saddle��5�      $
 Hand��5�      $B��</MultiDraw;component/Images/icons/90's Stub Draw 120x120.png��5�      6b   5�      �� 5�      $y�	Collapsed=�6   .��5�      �� $��25��6   $ 	Sync Data��5�   $   $
 Hand��5�   $   $7��1/MultiDraw;component/Images/icons/Play Button.png��5�   $   6a   5�      �� 5�      $y�	Collapsed=�6   .��5�      �� $��25��6   $<��6/MultiDraw;component/Images/icons/Settings 120x120.png��5�   $   $
 Hand��6d   $ Settings��6r   5�      5�      d� 5�      -   5         d�Name$ btnPlay��5�   -   $��24,0,0,0q�6    $   MaterialDesignActionToggleButton#��[  5�      1  +MaterialDesignThemes.Wpf.ToggleButtonAssist   	OnContent 5�      0  *MaterialDesignThemes.Wpf.PackIconExtension     Kind$ Pause=���5�        $
 Play=�5�      5�      �� 5�   
   -   5         ��Name$ masterContainer��5�      $��1��5�      5�      �� 5�   
   $��2��6   .+�5�      �� -   5         ��Name$ AlignConduits��5�      $��Align Conduits��6.    d� 	IsChecked$ FalseY�6G   $��	10,0,10,0q�6Y   6n   �� 5�      -   5        $ Anglefromprimary��5�      $��Add bend in-place��61   $ FalseY�6M   $��
10,10,10,0q�6_   6u   5�        TIGUtility.ToolTipControl  5�   
   -   5          Name$ 
btnTooltip��5�         Content. 62   5�      5�      5�      