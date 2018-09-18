using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni.Utilities
{
  // Static utility class for working with files and folders
  static class FileUtilities
  {
    // --- Public Interface ---
    public static void Copy(string source_path, string destination_path, bool sub_folders = true)
    {
      if(true == System.IO.Directory.Exists(source_path))
      {
        // Folder
        CopyFolder(source_path, destination_path + "\\" + GetFolderName(source_path), sub_folders);
      }
      else if(true == System.IO.File.Exists(source_path))
      {
        // File
        CopyFile(source_path, destination_path + "\\" + System.IO.Path.GetFileName(source_path));
      }
    }

    public static void Delete(string path)
    {
      if (true == System.IO.Directory.Exists(path))
      {
        // Folder
        DeleteFolder(path);
      }
      else if (true == System.IO.File.Exists(path))
      {
        // File
        // Todo: Ask the user if they are sure they want to delete the file
        System.IO.File.Delete(path);
      }
    }

    public static void Delete(System.Collections.Specialized.StringCollection paths)
    {
      // Todo: Ask the user if they are sure they want to delete all the files / folder
      foreach(string path in paths)
      {
        Delete(path);
      }
    }

    // --- Private Interface ---
    private static void CopyFile(string source_path, string destination_path)
    {
      // *Note* function expects that the source has already been verified that it exists

      if(System.IO.File.Exists(destination_path))
      {
        // Todo: Ask user if they want to overwrite the file
      }

      System.IO.File.Copy(source_path, destination_path, true);
    }

    private static void CopyFolder(string source_path, string destination_path, bool sub_folders = true)
    {
      // *Note* function expects that the source has already been verified that it exists

      if(false == System.IO.Directory.Exists(destination_path))
      {
        System.IO.Directory.CreateDirectory(destination_path);
      }
      
      // Copy any sub folders
      if(true == sub_folders)
      {
        string new_folder_path = "";
        string[] sub_folder_paths = System.IO.Directory.GetDirectories(source_path);
        foreach(string source_folder_path in sub_folder_paths)
        {
          new_folder_path = destination_path + "\\" + GetFolderName(source_folder_path);
          System.IO.Directory.CreateDirectory(new_folder_path);
          CopyFolder(source_folder_path, new_folder_path, true);
        }
      }

      // Copy any files
      string[] file_paths = System.IO.Directory.GetFiles(source_path);
      foreach (string file_path in file_paths)
      {
        CopyFile(file_path, destination_path + "\\" + System.IO.Path.GetFileName(file_path));
      }
    }

    private static string GetFolderName(string path)
    {
      char[] separators = { '/', '\\' };
      string[] split_path = path.Split(separators);
      return split_path[split_path.Length - 1];
    }

    private static void DeleteFolder(string path)
    {
      // Delete all sub folders
      string[] paths = System.IO.Directory.GetDirectories(path);
      foreach(string sub_folder_path in paths)
      {
        DeleteFolder(sub_folder_path);
      }

      // Delete any files
      paths = System.IO.Directory.GetFiles(path);
      foreach(string file_path in paths)
      {
        System.IO.File.Delete(file_path);
      }

      // Delete the empty folder and all empty sub folders
      System.IO.Directory.Delete(path);
    }
  }
}
