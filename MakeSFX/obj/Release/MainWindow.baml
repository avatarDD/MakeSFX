   M S B A M L   `   `   `  ���� B  >MakeSFX, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:MakeSFXMakeSFX      MakeSFX.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:MakeSFX     -     � Title$  MakeSFX��$	��600��$	��800�� J� 	AllowDrop .  � ToolTip$. (Перетащите сюда файлы�� � WindowStartupLocation$ CenterScreen=�.���  � RowDefinitions �� $��* ��� $��50 ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto � � ColumnDefinitions �� $��* �.+�&   System.Windows.Controls.DataGrid  -   $��dgFiles��$��0��$��5,5,5,5q�   SelectionMode$ Single=�   
IsReadOnly .    AutoGenerateColumns .  	   CanUserReorderColumns	 . ���� 
   RowStyle
 ��  �� 
TargetType)  #System.Windows.Controls.DataGridRow"   �� Triggers ��  �� Binding �� *
executable+ �� Value True.a���  �� Property � �� �� Value$
 Bold"���  � ��
 �������  �� *
executable+	 False.a���  � ��$ Normal"�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$) #Относительный Путь��   Width5  /System.Windows.Controls.DataGridLengthConverter$
 auto 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *relativePath+  $ Исполняемый��$
 auto  �� *
executable+� $��1�� �� $��* � �� $	��106 ��� $��* �.+��� $��0��$��0��$!��Запустить файл��$��5,5,5,5q�$
��Left=�$��Center=��� -   $��	cmbxStart��$��1��$��0��$��5,5,5,5q�$��Center=�$��Stretch=����� � $��2�� ��  �� $	��106 ��� $��* ��� $
��auto �.+��� $��0��$��Иконка��$��5,5,5,5q�$
��Left=�$��Center=��� -   $��tbxIcon��$��1��$��5,5,5,5q� �� 
IsReadOnly . $��Center=�$��Stretch=��� -   $��btnBrowseIcon��$��2��$��5,5,5,5q�$��Center=�$��Center=�.��
Обзор�� $��3�� �� Orientation$ 
Horizontal=�.+��� $#��Конфиг sfx модуля��$��5,5,5,5q�$��Center=�$��Center=��� -   $��btnCFG_Help��$��5,5,5,5q�$��Center=�$��Center=�.��?�� -   $��tbxCFG��$��4��$��5,5,5,5q�$��Stretch=�$��Stretch=� �� TextWrapping$
 Wrap=�$	��100�� �� AutoWordSelection .  �� 
AcceptsTab .  �� AcceptsReturn . �� -   $��
tbxOutName��$��5��$��5,5,5,5q�$��
MyFile.exe��$��Stretch=�$��Center=��� -	   $��
btnMakeSFX��$��6��$��5,5,5,5q�$��Center=�$��Center=�.��Создать