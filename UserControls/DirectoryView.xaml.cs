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
    // --- Private Variables ---
    string _current_directory_path;
    List<string> _back_directories;
    List<string> _forward_directories;

    // --- Public Interface ---
    public DirectoryView()
    {
      InitializeComponent();

      _current_directory_path = "";
      _back_directories = new List<string>();
      _forward_directories = new List<string>();

      // TODO: Testing only
      LoadDirectory("F:\\Workspaces\\Visual Studio 2017\\C# Projects\\Omni\\Omni\\bin\\Debug");
    }

    public bool LoadDirectory(string directory_path, bool store_previous_directory = true)
    {
      if(Directory.Exists(directory_path) == false)
      {
        return false;
      }

      if("" != _current_directory_path && true == store_previous_directory )
      {
        StorePreviousDirectory(_current_directory_path);
      }
      _current_directory_path = directory_path;
      TB_DirectoryPath.Text = _current_directory_path;

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

    // --- Private Interface ---
    private void StorePreviousDirectory(string path)
    {
      const int max_saved_directories = 10; // TODO: This should really be a configurable option

      if (_back_directories.Count >= max_saved_directories)
      {
        _back_directories.RemoveAt(0);
      }

      _back_directories.Add(path);
    }

    private void StoreForwardDirectory(string path)
    {
      _forward_directories.Add(path);
    }

    // --- Events ---
    private void Btn_Back_Click(object sender, RoutedEventArgs e)
    {
      if (0 == _back_directories.Count)
      {
        return;
      }

      StoreForwardDirectory(_current_directory_path);

      string previous_dir = _back_directories[_back_directories.Count - 1];
      _back_directories.RemoveAt(_back_directories.Count - 1);

      LoadDirectory(previous_dir, false);
    }

    private void Btn_Forward_Click(object sender, RoutedEventArgs e)
    {
      if (0 == _forward_directories.Count)
      {
        return;
      }

      string next_dir = _forward_directories[_forward_directories.Count - 1];
      _forward_directories.RemoveAt(_forward_directories.Count - 1);

      LoadDirectory(next_dir);
    }

    private void Btn_Up_Click(object sender, RoutedEventArgs e)
    {
      int index = _current_directory_path.LastIndexOf('\\');
      if (index >= 0)
      {
        LoadDirectory(_current_directory_path.Substring(0, index));
      }
    }

    private void TB_DirectoryPath_KeyUp(object sender, KeyEventArgs e)
    {
      if(Key.Enter != e.Key)
      {
        return;
      }

      if (false == LoadDirectory(TB_DirectoryPath.Text))
      {
        TB_DirectoryPath.Text = _current_directory_path;
      }
    }
  }
}
