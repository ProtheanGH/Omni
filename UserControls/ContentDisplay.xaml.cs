using System;
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
  /// A Control that represents a single instance of content (file, folder, ect.) and handles displaying information about that content, 
  /// as well as providing an interface to interact with it.
  /// </summary>
  public partial class ContentDisplay : UserControl
  {
    // --- Private Structs ---
    private struct Property
    {
      public string _info;
      public byte _hidden_characters;
      public double _owning_control_width;
    }

    // --- Private Variables ---
    private DirectoryView _owning_directory_view;
    private ContentType _type;
    private Property[] _content_properties;
    private TextBlock[] _property_labels;

    // --- Properties ---
    public string ContentPath { get; private set; }

    // --- Private Enums ---
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

    // --- Public Enums ---
    public enum ContentProperties
    {
      Property_Name = 0,
      Property_DateModified,
      Property_Type,
      Property_Size,
      Max_Count
    }

    // --- Public Interface ---
    public ContentDisplay(DirectoryView owning_view)
    {
      InitializeComponent();

      _owning_directory_view = owning_view;

      _content_properties = new Property[Convert.ToInt32(ContentProperties.Max_Count)];
      for(int i = 0; i < _content_properties.Length; ++i)
      {
        _content_properties[i]._info = "";
        _content_properties[i]._hidden_characters = 0;
        _content_properties[i]._owning_control_width = 0.0;
      }

      // Add all the labels into the list
      _property_labels = new TextBlock[Convert.ToInt32(ContentProperties.Max_Count)];
      _property_labels[0] = Lbl_Name;
      _property_labels[1] = Lbl_Date;
      _property_labels[2] = Lbl_Type;
      _property_labels[3] = Lbl_Size;
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
      ContentPath = file_info.FullName;
      _type = ContentType.file;

      // Update display information
      _content_properties[Convert.ToInt32(ContentProperties.Property_Name)]._info = file_info.Name;
      _content_properties[Convert.ToInt32(ContentProperties.Property_DateModified)]._info = file_info.LastWriteTime.ToString("MM/dd/yyyy hh:mm tt");
      _content_properties[Convert.ToInt32(ContentProperties.Property_Type)]._info = file_info.Extension;
      _content_properties[Convert.ToInt32(ContentProperties.Property_Size)]._info = ConvertFileSize(file_info.Length);

      DisplayContentInfo();

      // Update Icon
      Icon file_icon = System.Drawing.Icon.ExtractAssociatedIcon(file_info.FullName);
      Img_ContentIcon.Source = DrawingUtilities.CreateBitmapSourceFromGdiBitmap(file_icon.ToBitmap());
    }

    public void UpdatePropertyInfo(ref DirectoryInfo folder_info)
    {
      ContentPath = folder_info.FullName;
      _type = ContentType.folder;

      // Update display information
      _content_properties[Convert.ToInt32(ContentProperties.Property_Name)]._info = folder_info.Name;
      _content_properties[Convert.ToInt32(ContentProperties.Property_DateModified)]._info = folder_info.LastWriteTime.ToString("MM/dd/yyyy hh:mm tt");
      _content_properties[Convert.ToInt32(ContentProperties.Property_Type)]._info = "File folder";
      _content_properties[Convert.ToInt32(ContentProperties.Property_Size)]._info = ""; // No size displayed for folders

      DisplayContentInfo();

      // Update Icon
      Img_ContentIcon.Source = DrawingUtilities.CreateBitmapSourceFromGdiBitmap(Properties.Resources.folder_icon);
    }

    public void SetColumnWidth(ContentProperties property, double width)
    {
      if (property < ContentProperties.Max_Count)
      {
        Grid.ColumnDefinitions[Convert.ToInt32(property)].Width = new GridLength(width, GridUnitType.Pixel);
      }

      DisplayContentInfo();
    }

    public void SetColumnWidths(ContentProperties[] properties, double[] widths)
    {
      if(properties.Length != widths.Length)
      {
        throw new ArgumentException("The size of the properties and widths must match.");
      }

      for (int i = 0; i < properties.Length; ++i)
      {
        if (properties[i] < ContentProperties.Max_Count)
        {
          Grid.ColumnDefinitions[Convert.ToInt32(properties[i])].Width = new GridLength(widths[i], GridUnitType.Pixel);
        }
      }

      DisplayContentInfo();
    }

    public bool OpenContent()
    {
      switch(_type)
      {
        case ContentType.file:
          try
          {
            System.Diagnostics.Process.Start(ContentPath);
          }
          catch (Exception e)
          {
            Console.WriteLine(e.Message);
            return false;
          }
          break;
        case ContentType.folder:
          return _owning_directory_view.LoadDirectory(ContentPath);
        default:
          return false;
      }

      return true;
    }

    public void CopyContent()
    {
      System.Collections.Specialized.StringCollection file_list = new System.Collections.Specialized.StringCollection();
      file_list.Add(ContentPath);
      Clipboard.SetFileDropList(file_list);
    }

    // --- Private Interface ---
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

    private void DisplayProperty(ref TextBlock control, ref Property property, double size_offset = 0)
    {
      if (control.ActualWidth <= 0)
      {
        return;
      }

      if(property._owning_control_width == 0)
      {
        property._owning_control_width = control.ActualWidth;
      }

      double control_width = control.ActualWidth - size_offset;
      string display_string = property._info;
      Font text_font = new Font(control.FontFamily.ToString(), (float)(control.FontSize * 0.75));
      int string_length = System.Windows.Forms.TextRenderer.MeasureText(display_string, text_font).Width;
      if(string_length > control_width)
      {
        bool force_shrink = false;

        while(true)
        {
          if(property._owning_control_width >= control_width || true == force_shrink)
          {
            // Control got smaller
            display_string = property._info.Substring(0, property._info.Length - (property._hidden_characters + 2)) + "...";
            string_length = System.Windows.Forms.TextRenderer.MeasureText(display_string, text_font).Width;
            if (string_length <= control_width)
            {
              break;
            }

            property._hidden_characters += 1;
          }
          else
          {
            // Control got larger
            if(property._hidden_characters == 0)
            {
              break;
            }
            property._hidden_characters -= 1;

            display_string = property._info.Substring(0, property._info.Length - (property._hidden_characters + 2)) + "...";
            string_length = System.Windows.Forms.TextRenderer.MeasureText(display_string, text_font).Width;
            if (string_length <= control_width)
            {
              break;
            }
            else
            {
              // String got too long, we need ot go the other way
              force_shrink = true;
            }
          }
        }
      }
      else
      {
        property._hidden_characters = 0;
      }

      control.Text = display_string;
      property._owning_control_width = control_width;
    }

    private void DisplayContentInfo()
    {
      DisplayProperty(ref Lbl_Name, ref _content_properties[Convert.ToInt32(ContentProperties.Property_Name)], Img_ContentIcon.ActualWidth);
      DisplayProperty(ref Lbl_Date, ref _content_properties[Convert.ToInt32(ContentProperties.Property_DateModified)]);
      DisplayProperty(ref Lbl_Type, ref _content_properties[Convert.ToInt32(ContentProperties.Property_Type)]);
      DisplayProperty(ref Lbl_Size, ref _content_properties[Convert.ToInt32(ContentProperties.Property_Size)]);
    }

    // --- Events ---
    // --- User Control Events
    private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      OpenContent();
    }

    private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      switch (e.ChangedButton)
      {
        case MouseButton.Right:
          // TODO: Temporary thing, really this should open up an options menu
          CopyContent();

          // Testing
          Peter.ShellContextMenu contxt_menu = new Peter.ShellContextMenu();
          FileInfo[] files = new FileInfo[1];
          files[0] = new FileInfo(ContentPath);
          contxt_menu.ShowContextMenu(files, new System.Drawing.Point(20, 20));
          break;
      }
    }

    private void UserControl_KeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Enter:
          OpenContent();
          break;
      }
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      DisplayContentInfo();
    }
  }
}
