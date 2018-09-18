using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
  /// Interaction logic for DirectoryView.xaml
  /// </summary>
  public partial class DirectoryView : UserControl
  {
    // --- Private Variables ---
    private string _current_directory_path;
    private List<string> _back_directories;
    private List<string> _forward_directories;
    private FileSystemWatcher _directory_watcher;

    // --- Private Static Varirables ---
    private System.Windows.Threading.DispatcherTimer s_update_timer = null;
    private static Dictionary<String, List<DirectoryView>> s_active_directory_views = new Dictionary<string, List<DirectoryView>>();
    private static Queue<DirectoryView> s_reload_queue = new Queue<DirectoryView>();

    // --- Public Interface ---
    public DirectoryView()
    {
      InitializeComponent();

      _current_directory_path = "";
      _back_directories = new List<string>();
      _forward_directories = new List<string>();

      // Setup the directory watcher
      _directory_watcher = new FileSystemWatcher();
      _directory_watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName
        | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
      _directory_watcher.Changed += new FileSystemEventHandler(OnChanged);
      _directory_watcher.Created += new FileSystemEventHandler(OnChanged);
      _directory_watcher.Deleted += new FileSystemEventHandler(OnChanged);
      _directory_watcher.Renamed += new RenamedEventHandler(OnRenamed);

      // TODO: Testing only
      LoadDirectory("F:\\Workspaces\\Visual Studio 2017\\C# Projects\\Omni\\Omni\\bin\\Debug");
    }

    public bool LoadDirectory(string directory_path, bool store_previous_directory = true)
    {
      if (Directory.Exists(directory_path) == false)
      {
        return false;
      }

      if ("" != _current_directory_path && true == store_previous_directory)
      {
        StorePreviousDirectory(_current_directory_path);
      }

      string previous_directory = _current_directory_path;
      _current_directory_path = directory_path;
      TB_DirectoryPath.Text = _current_directory_path;

      UpdateActiveDirectoryViews(previous_directory);

      // Update the FileSystemWatcher
      _directory_watcher.Path = _current_directory_path;
      _directory_watcher.EnableRaisingEvents = true;

      // Clear out the current content
      LB_Content.Items.Clear();

      DirectoryInfo directory = new DirectoryInfo(directory_path);

      // Display the Folders
      DirectoryInfo[] folders = directory.GetDirectories();
      for (int i = 0; i < folders.Length; ++i)
      {
        Omni.UserControls.ContentDisplay folder_display = new ContentDisplay(this, ref folders[i]);
        LB_Content.Items.Add(folder_display);
      }

      // Display the Files
      FileInfo[] files = directory.GetFiles();
      for (int i = 0; i < files.Length; ++i)
      {
        Omni.UserControls.ContentDisplay file_display = new ContentDisplay(this, ref files[i]);
        LB_Content.Items.Add(file_display);
      }

      return true;
    }

    public void CopySelectedContent()
    {
      if (LB_Content.SelectedItems.Count > 0)
      {
        Clipboard.SetFileDropList(GetSelectedContent());
      }
    }

    public void Paste()
    {
      System.Collections.Specialized.StringCollection file_drop_list = Clipboard.GetFileDropList();
      foreach (string content_path in file_drop_list)
      {
        Omni.Utilities.FileUtilities.Copy(content_path, _current_directory_path);
      }
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

    private void UpdateActiveDirectoryViews(string previous_directory)
    {
      // Remove from the current directory list
      if (s_active_directory_views.ContainsKey(previous_directory))
      {
        if (s_active_directory_views[previous_directory].Count == 1)
        {
          // Remove the entire list, as no DirectoryViews are currently viewing that directory
          s_active_directory_views.Remove(previous_directory);
        }
        else
        {
          // Remove this DirectoryView from the list
          s_active_directory_views[previous_directory].Remove(this);
        }
      }

      // Add this DirectoryView to the list for the new directory
      if (s_active_directory_views.ContainsKey(_current_directory_path) == false)
      {
        s_active_directory_views.Add(_current_directory_path, new List<DirectoryView>());
      }
      s_active_directory_views[_current_directory_path].Add(this);
    }

    System.Collections.Specialized.StringCollection GetSelectedContent()
    {
      System.Collections.Specialized.StringCollection selected_content = new System.Collections.Specialized.StringCollection();
      foreach(object item in LB_Content.SelectedItems)
      {
        selected_content.Add(((Omni.UserControls.ContentDisplay)item).ContentPath);
      }

      return selected_content;
    }

    // --- Private Static Interface ---
    private static void ReloadDirectory(string directory)
    {
      if (s_active_directory_views.ContainsKey(directory))
      {
        s_active_directory_views[directory].ForEach(
          delegate (DirectoryView view)
          {
            if (s_reload_queue.Contains(view) == false)
            {
              s_reload_queue.Enqueue(view);
            }
          });
      }
    }

    // --- Events ---
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (s_update_timer == null)
      {
        s_update_timer = new System.Windows.Threading.DispatcherTimer();
        s_update_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        s_update_timer.Tick += new EventHandler(DirectoryView_OnTick);
        s_update_timer.Start();
      }
    }

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
      if (Key.Enter != e.Key)
      {
        return;
      }

      if (false == LoadDirectory(TB_DirectoryPath.Text))
      {
        TB_DirectoryPath.Text = _current_directory_path;
      }
    }

    private void LB_Content_MouseUp(object sender, MouseButtonEventArgs e)
    {
      switch (e.ChangedButton)
      {
        case MouseButton.Right:
          // Testing
          Peter.ShellContextMenu contxt_menu = new Peter.ShellContextMenu();
          DirectoryInfo[] directories = new DirectoryInfo[1];
          directories[0] = new DirectoryInfo(_current_directory_path);
          contxt_menu.ShowContextMenu(directories, new System.Drawing.Point(20, 20));
          break;
      }
    }

    private void LB_Content_KeyDown(object sender, KeyEventArgs e)
    {
      if (ModifierKeys.Control == Keyboard.Modifiers)
      {
        // Control Commands
        switch (e.Key)
        {
          case Key.C:
            CopySelectedContent();
            break;
          case Key.V:
            Paste();
            break;
          case Key.X:
            // Todo: Cut command
            break;
        }
      }
      else
      {
        switch (e.Key)
        {
          case Key.Delete:
            if (LB_Content.SelectedItems.Count > 0)
            {
              Omni.Utilities.FileUtilities.Delete(GetSelectedContent());
            }
            break;
        }
      }
    }

    // --- Static Events ---
    private static void DirectoryView_OnTick(object sender, EventArgs e)
    {
      while (s_reload_queue.Count > 0)
      {
        DirectoryView view = s_reload_queue.Dequeue();
        view.LoadDirectory(view._current_directory_path, false);
      }
    }

    private static void OnChanged(object source, FileSystemEventArgs e)
    {
      // Todo: Should probably only reload the changed file
      // Reload the directory
      ReloadDirectory(((FileSystemWatcher)source).Path);
    }

    private static void OnRenamed(object source, RenamedEventArgs e)
    {
      // Todo: Should probably only reload the changed file
      // Reload the directory
      ReloadDirectory(((FileSystemWatcher)source).Path);
    }
  }
}
