using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni.Utilities
{
  class ConfigUtilities
  {
    // --- Public Variables ---
    public static string kOmniAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Omni";
    public static string kDefaultConfigPath = kOmniAppDataFolder + "\\Config.xml";

    public const string kOmniConfigRoot             = "OmniConfig";

    public const string kOmniViewsSection           = "Views";
    public const string kOmniPropertyColumnSection  = "PropertyColumns";

    public const string kOmniRowElement             = "Rows";
    public const string kOmniColumnElement          = "Columns";
    public const string kOmniViewElement            = "View";
    public const string kOmniNameElement            = "Name";
    public const string kOmniDateModifiedElement    = "Date_Modified";
    public const string kOmniTypeElement            = "Type";
    public const string kOmniSizeElement            = "Size";

    public const string kOmniCountAttribute         = "Count";
    public const string kOmniRowAttribute           = "Row";
    public const string kOmniColumnAttribute        = "Column";
    public const string kOmniWidthAttribute         = "Width";
    public const string kOmniDirectoryAttribute     = "Directory";

    // --- Public Classes ---
    public class ViewConfig
    {
      // --- Public Variables ---
      //! The row this View should be assigned to
      public int _row;
      //! The column this View should be assigned to
      public int _column;
      //! The directory this View should load
      public string _directory;
      //! List of property columns and their widths
      public List<Tuple<UserControls.ContentDisplay.ContentProperties, double>> _column_widths;

      // --- Constructor ---
      public ViewConfig()
      {
        _row = -1;
        _column = -1;
        _directory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
        _column_widths = new List<Tuple<UserControls.ContentDisplay.ContentProperties, double>>();
      }
    }
  }
}
