   M S B A M L   `   `   `  ����Q  MRuleArchitectPrototype, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:RuleArchitectPrototype.ViewsRuleArchitectPrototype.Views  5   	   U/clr-namespace:RuleArchitectPrototype.ViewModels!RuleArchitectPrototype.ViewModels  5   	   M+clr-namespace:RuleArchitectPrototype.ModelsRuleArchitectPrototype.Models  5   	   U/clr-namespace:RuleArchitectPrototype.Converters!RuleArchitectPrototype.Converters  5
   	   <    6RuleArchitectPrototype.Views.AddEditSpecCodeDialogView   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   <local*clr-namespace:RuleArchitectPrototype.Views     5   	   >vm/clr-namespace:RuleArchitectPrototype.ViewModels     5   	   >models+clr-namespace:RuleArchitectPrototype.Models     5   	   _ [MaterialDesignThemes.Wpf, Version=4.9.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5	   	   @
converters/clr-namespace:RuleArchitectPrototype.Converters  5
   	     � SizeToContent$  WidthAndHeight=�5   	    � WindowStartupLocation$ CenterOwner=�5   	   $	��450��5   	   $	��600��6   $	��480��6$   $	��700��64    � ShowInTaskbar .   +� FocusedElement 5   	   ��  �� ElementName$ SpecNoTextBox�� � 	Resources 5       �� MergedDictionaries �� Source<   6RuleArchitectPrototype.Converters.NullToValueConverter   NullToValueConverterD   >RuleArchitectPrototype.Converters.BooleanToVisibilityConverter   BooleanToVisibilityConverterC   =RuleArchitectPrototype.Converters.StringToVisibilityConverter  StringToVisibilityConverter?   9RuleArchitectPrototype.Converters.InverseBooleanConverter  InverseBooleanConverterK   ERuleArchitectPrototype.Converters.InverseBooleanToVisibilityConverter ' #InverseBooleanToVisibilityConverter�� 5   
    5      �� 5      $h bpack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml?�6%   %�   &	        &	      &	 &     &	 9     &	 L       5      6.     5      66     5      65     5      61     5      6=   5       � Title 5      �� 5    
   	 �� Path$(	 "SelectedExistingSpecCodeDefinition�6   
 �� 	Converter#
 [  5!       �� ConverterParameter$L FAdd Specification Code Association/Edit Specification Code Association��5"      .��5%      � $��16q�6    � RowDefinitions 5&   
   �� 5'      $
��Auto �6   �� 5(      $
��Auto �6   �� 5)      $
��Auto �6   �� 5*      $
��Auto �6   �� 5+      $
��Auto �6   �� 5,      $��* �6   �� 5-      $
��Auto �6   .+�50   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��0,0,0,10q�6;   .+�51      �� $��Machine Type Context: ��6   $
��Bold"�66   �� 52      ��6   �� *'%CurrentSoftwareOptionMachineType.Name+53      � 55   
   $��1��6   $��0,0,0,10q�6    � ColumnDefinitions 56      �� 57      $��* �6#   �� 58      $
��Auto �6#   �� 59      $��* �6#   .+�5;      �� -   5         ��Name$ SpecNoTextBox��5;      $��0��6-   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Spec No. (1-32)��5<       % !MaterialDesignFloatingHintTextBox#��[ 5=      $��0,0,8,0q�5?      ��5>      �� *
SpecCodeNo+ �� UpdateSourceTrigger$ PropertyChanged=�5?      �� 5@      $��1��6   $��-��6(   $��Center=�61   $��0,10,8,0q�6L   $��16(�6^   �� 5A      $��2��6   $ 	Bit (0-7)��5B      #��[ 5C      $��0,0,0,0q�5E      ��5D      �� *SpecCodeBit+$ PropertyChanged=�5E      5F      �� 5H   
   $��2��6   $,��&Manually Check/Update Definition State��6   $��0,5,0,5q�5J      $
��Left=�6"      MaterialDesignOutlinedButton#��[ 5K      ��5I      �� *ManualUpdateButtonCommand+5K      �� 5M   
   $��3��6   $#��Specification Code Definition��6    $��0,5,0,10q�6G   y�5N      �� *IsSpecCodeNoEntered+#
 [  �� Mode$ OneWay=�.��5O      �� $��10q�6   .+�5P      �� $ 
Horizontal=�6   $��0,0,0,5q�66   .+�5Q      �� $��Status: ��6    �� 5R      $��Creating New Definition��6    $
��Bold"�6?   y�5S   "   �� *!IsCreatingNewSpecCodeDefinition+#
 [ $ OneWay=��� 5T      $��Existing Definition Found��6    $
��Bold"�6A   y�5U   "   �� *!IsCreatingNewSpecCodeDefinition+#
 [ $ OneWay=�5V      �� 5X      $ Description��6   #��[ 5Y       �� TextWrapping$
 Wrap=�5\       �� MinLines$ 2��6.    �� MaxLines$ 3��6;   " �� VerticalScrollBarVisibility$
 Auto=�6H   $��0,8,0,8q�5]      ��5Z      �� *NewSpecCodeDescription+$ PropertyChanged=�{�5[      �� *!IsCreatingNewSpecCodeDefinition+$ OneWay=�5]      �� 5_      $ Category��6    & "MaterialDesignFloatingHintComboBox#��[ 5b      $��0,8,0,0q�5d      ��5`      �� *AvailableCategories+ �� SelectedItem 5a      �� *SelectedNewSpecCodeCategory+$ PropertyChanged=�{�5c      �� *!IsCreatingNewSpecCodeDefinition+$ OneWay=�5d      5e      5f      �� 5h   
   $��4��6   $( "Link to Activation Rule (Optional)��6     �� DisplayMemberPath$ RuleName��5k      -  'MaterialDesignThemes.Wpf.ComboBoxAssist   ShowSelectedItem . #��[ 5m      $��0,5,0,10q�5n      ��5i      �� *AvailableActivationRules+ 5j      �� *SelectedActivationRule+$ PropertyChanged=�5n      �� 5p   
   $��5��6   $6 0Specific Interpretation for this Software Option��6   #��[ 5r      $
 Wrap=�5s       �� AcceptsReturn . $ 3��6;   $ 5��6H   $
 Auto=�5t      $��0,5,0,10q�65   ��5q      �� *SpecificInterpretation+$ PropertyChanged=�5t   5   �� 5v   
   $��6��6   $ 
Horizontal=�6"   $��Right=�6;   $��0,16,0,0q�6W   .+�5w      �� $��Cancel��6    �� IsCancel . #��[ 5x      $��0,0,8,0q�6K   ��5w   &   �� *CancelCommand+5x   K   �� 5y      $
��Save��6    �� 	IsDefault .   MaterialDesignRaisedButton#��[ 5z      ��5y   $   �� *SaveCommand+5z      5{      5|      5}      