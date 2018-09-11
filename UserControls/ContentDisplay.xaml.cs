﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Omni.Utilities;

namespace Omni.UserControls
{
  /// <summary>
  /// Interaction logic for ContentDisplay.xaml
  /// </summary>
  public partial class ContentDisplay : UserControl
  {
    // Private Variables:
    List<Label> _property_labels;
    DirectoryView _owning_directory_view;
    ContentType _type;
    string _content_path;

    // Private Enums
    private enum ContentType
    {
      file = 0,
      folder
    }

    private enum FileSizes
    {
      bytes = 0,
      KB,
      MB,
      GB,
      TB,
      PB
    };

    // Public Enums
    public enum ContentProperties
    {
      Property_Name = 0,
      Property_DateModified,
      Property_Type,
      Property_Size,
      Max_Count
    }

    // Public Interface:
    public ContentDisplay(DirectoryView owning_view)
    {
      InitializeComponent();

      _property_labels = new List<Label>();

      _owning_directory_view = owning_view;

      // Add all the labels into the list
      _property_labels.Add(Lbl_Name);
      _property_labels.Add(Lbl_Date);
      _property_labels.Add(Lbl_Type);
      _property_labels.Add(Lbl_Size);
    }

    public ContentDisplay(DirectoryView owning_view, ref FileInfo file_info) : this(owning_view)
    {
      // Initialize all the display info
      UpdatePropertyInfo(ref file_info);
    }

    public ContentDisplay(DirectoryView owning_view, ref DirectoryInfo folder_info) : this(owning_view)
    {
      // Initialize all the display info
      UpdatePropertyInfo(ref folder_info);
    }

    public void UpdatePropertyInfo(ref FileInfo file_info)
    {
      _content_path = file_info.FullName;
      _type = ContentType.file;

      // Update display information
      Lbl_Name.Content = file_info.Name;
      Lbl_Date.Content = file_info.LastWriteTime.ToString("MM/dd/yyyy hh:mm tt");
      Lbl_Type.Content = file_info.Extension;
      Lbl_Size.Content = ConvertFileSize(file_info.Length);

      // Update Icon
      Icon file_icon = System.Drawing.Icon.ExtractAssociatedIcon(file_info.FullName);
      Img_ContentIcon.Source = DrawingUtilities.CreateBitmapSourceFromGdiBitmap(file_icon.ToBitmap());
    }

    public void UpdatePropertyInfo(ref DirectoryInfo folder_info)
    {
      _content_path = folder_info.FullName;
      _type = ContentType.folder;

      // Update display information
      Lbl_Name.Content = folder_info.Name;
      Lbl_Date.Content = folder_info.LastWriteTime.ToString("MM/dd/yyyy hh:mm tt");
      Lbl_Type.Content = "File folder";
      Lbl_Size.Content = ""; // No size displayed for folders

      // Update Icon
      Img_ContentIcon.Source = DrawingUtilities.CreateBitmapSourceFromGdiBitmap(Properties.Resources.folder_icon);
    }

    public void SetColumnWidth(ContentProperties property, double width)
    {
      if (property < ContentProperties.Max_Count)
      {
        Grid.ColumnDefinitions[Convert.ToInt32(property)].Width = new GridLength(width, GridUnitType.Pixel);
      }
    }

    public bool OpenContent()
    {
      switch(_type)
      {
        case ContentType.file:
          try
          {
            System.Diagnostics.Process.Start(_content_path);
          }
          catch (Exception e)
          {
            Console.WriteLine(e.Message);
            return false;
          }
          break;
        case ContentType.folder:
          return _owning_directory_view.LoadDirectory(_content_path);
        default:
          return false;
      }

      return true;
    }

    // Private Interface
    private string ConvertFileSize(long file_size)
    {
      int size_level = 0;

      while(file_size >= 1024)
      {
        ++size_level;
        file_size = file_size / 1024;
      }

      return file_size.ToString() + " " + Enum.GetName(typeof(FileSizes), size_level);
    }

    // Events
    private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      OpenContent();
    }
  }
}
