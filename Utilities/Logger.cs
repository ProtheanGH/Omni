using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni.Utilities
{
  static class Logger
  {
    // --- Private Variables ---
    private static string _log_file_path = ConfigUtilities.kOmniAppDataFolder + "\\Log\\OmniLog.txt";

    // --- Public Enums ---
    public enum SeverityType
    {
      Error,
      Warning,
      Debug
    }

    // --- Public Interface ---
    public static void Initialize()
    {
      System.IO.StreamWriter log = null;

      try
      {
        if (false == System.IO.Directory.Exists(ConfigUtilities.kOmniAppDataFolder + "\\Log"))
        {
          System.IO.Directory.CreateDirectory(ConfigUtilities.kOmniAppDataFolder + "\\Log");
        }

        // Todo: Should probably delete old logs, if the file is too large (probably could be a setting)

        string initial_log = "------------------------------\n";
        initial_log += System.AppDomain.CurrentDomain.FriendlyName + " | " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
        initial_log += "\nStarted from: " + System.AppDomain.CurrentDomain.BaseDirectory + "\n";

        log = new System.IO.StreamWriter(_log_file_path, true);
        log.WriteLine(FormatNewlines(initial_log));
      }
      catch (Exception e)
      {
        Debug.WriteLine("Exception while trying to write to the log file. Exception: " + e.ToString());
      }
      finally
      {
        if (null != log)
        {
          log.Close();
        }
      }
    }

    public static void WriteLine(string message, params object[] arguments)
    {
      System.IO.StreamWriter log = null;

      try
      {
        message = FormatNewlines(message);

        log = new System.IO.StreamWriter(_log_file_path, true);

        if (arguments.Length > 0)
        {
          log.WriteLine(String.Format(message, arguments));
        }
        else
        {
          log.WriteLine(message);
        }
      }
      catch(Exception e)
      {
        Debug.WriteLine("Exception while trying to write to the log file. Exception: " + e.ToString());
        Debug.WriteLine("Failed log message:");
        if (arguments.Length > 0)
        {
          Debug.WriteLine(String.Format(message, arguments));
        }
        else
        {
          Debug.WriteLine(message);
        }
      }
      finally
      {
        if (null != log)
        {
          log.Close();
        }
      }
    }

    public static void WriteLine(SeverityType type, string message, params object[] arguments)
    {
      message = type.ToString() + message;
      WriteLine(message, arguments);
    }

    // --- Private Interface ---
    private static string FormatNewlines(string message)
    {
      // Changes out the usual newline character for the environment newline character, so that it will work properly in applications such as "notepad"
      return message.Replace("\n", Environment.NewLine);
    }
  }
}
