using System;
using System.IO;
using System.Collections.Generic;
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

namespace Omni.UserControls
{
  /// <summary>
  /// Interaction logic for DirectoryView.xaml
  /// </summary>
  public partial class DirectoryView : UserControl
  {
    // Private Variables
    string _current_directory_path;

    // Public Interface
    public DirectoryView()
    {
      InitializeComponent();

      // TODO: Testing only
      LoadDirectory("F:\\Workspaces\\Visual Studio 2017\\C# Projects\\Omni\\Omni\\bin\\Debug");
    }

    public bool LoadDirectory(string directory_path)
    {
      if(Directory.Exists(directory_path) == false)
      {
        return false;
      }

      // Clear out the current content
      LB_Content.Items.Clear();

      DirectoryInfo directory = new DirectoryInfo(directory_path);

      // Display the Folders
      DirectoryInfo[] folders = directory.GetDirectories();
      for(int i = 0; i < folders.Length; ++i)
      {
        Omni.UserControls.ContentDisplay folder_display = new ContentDisplay(this, ref folders[i]);
        LB_Content.Items.Add(folder_display);
      }

      // Display the Files
      FileInfo[] files = directory.GetFiles();
      for(int i = 0; i < files.Length; ++i)
      {
        Omni.UserControls.ContentDisplay file_display = new ContentDisplay(this, ref files[i]);
        LB_Content.Items.Add(file_display);
      }

      return true;
    }
  }
}
