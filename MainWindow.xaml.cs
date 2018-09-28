using System;
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
using System.Xml;

using Omni.Utilities;

namespace Omni
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    // --- Private Variables ---
    List<UserControls.DirectoryView> _directory_views;
    int _row_count = 1;
    int _column_count = 1;

    // --- Public Interface ---
    public MainWindow()
    {
      InitializeComponent();

      _directory_views = new List<UserControls.DirectoryView>();

      Logger.Initialize();

      ParseCommandLine();
    }

    public void ConfigureViewGrid(int rows, int columns)
    {
      if(rows <= 0)
      {
        throw new System.ArgumentOutOfRangeException("rows", "Value cannot be less than 1.");
      }
      else if (columns <= 0)
      {
        throw new System.ArgumentOutOfRangeException("columns", "Value cannot be less than 1.");
      }

      _row_count = rows;
      _column_count = columns;

      Grid_Views.Children.Clear();
      Grid_Views.RowDefinitions.Clear();
      Grid_Views.ColumnDefinitions.Clear();

      double row_height = Grid_Views.ActualWidth / rows;
      double column_width = Grid_Views.ActualHeight / columns;

      for (int i = 0; i < rows; ++i)
      {
        RowDefinition row_definition = new RowDefinition();
        row_definition.Height = new GridLength(row_height, GridUnitType.Star);
        Grid_Views.RowDefinitions.Add(row_definition);
      }

      for (int i = 0; i < columns; ++i)
      {
        ColumnDefinition column_definition = new ColumnDefinition();
        column_definition.Width = new GridLength(column_width, GridUnitType.Star);
        Grid_Views.ColumnDefinitions.Add(column_definition);
      }

      for (int i = 0; i < rows * columns; ++i)
      {
        if(i >= _directory_views.Count)
        {
          _directory_views.Add(new UserControls.DirectoryView());
        }

        Grid_Views.Children.Add(_directory_views[i]);

        int row = GetRow(i);
        int column = GetColumn(i);
        //column = column == 0 ? _column_count - 1 : column - 1;

        Grid.SetRow(_directory_views[i], row);
        Grid.SetColumn(_directory_views[i], column);
      }
    }

    public bool SaveCurrentConfiguration(string file_path)
    {
      byte[] old_config = null;
      if (System.IO.File.Exists(file_path))
      {
        old_config = System.IO.File.ReadAllBytes(file_path);
      }

      XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
      {
        Indent = true,
        IndentChars = "\t",
      };

      XmlWriter config_xml = XmlWriter.Create(file_path, xmlWriterSettings);
      try
      {
        config_xml.WriteStartDocument();
        config_xml.WriteStartElement(ConfigUtilities.kOmniConfigRoot);
        {
          config_xml.WriteStartElement(ConfigUtilities.kOmniRowElement);
          config_xml.WriteAttributeString(ConfigUtilities.kOmniCountAttribute, _row_count.ToString());
          // Todo: Should have row width as well
          config_xml.WriteEndElement();

          config_xml.WriteStartElement(ConfigUtilities.kOmniColumnElement);
          config_xml.WriteAttributeString(ConfigUtilities.kOmniCountAttribute, _column_count.ToString());
          // Todo: Should have column width as well
          config_xml.WriteEndElement();

          config_xml.WriteStartElement(ConfigUtilities.kOmniViewsSection);
          {
          string directory = "";
          double[] column_widths = new double[Convert.ToInt32(UserControls.ContentDisplay.ContentProperties.Max_Count)];
            for (int i = 0; i < _directory_views.Count; ++i)
            {
              config_xml.WriteStartElement(ConfigUtilities.kOmniViewElement);
              int row = i / _column_count;
              int column = i % _column_count;
              config_xml.WriteAttributeString(ConfigUtilities.kOmniRowAttribute, row.ToString());
              config_xml.WriteAttributeString(ConfigUtilities.kOmniColumnAttribute, column.ToString());

              _directory_views[i].GetCurrentConfiguration(ref directory, ref column_widths);

              config_xml.WriteAttributeString(ConfigUtilities.kOmniDirectoryAttribute, directory);

              config_xml.WriteStartElement(ConfigUtilities.kOmniPropertyColumnSection);
              {
                config_xml.WriteStartElement(ConfigUtilities.kOmniNameElement);
                config_xml.WriteAttributeString(ConfigUtilities.kOmniWidthAttribute, column_widths[Convert.ToInt32(UserControls.ContentDisplay.ContentProperties.Property_Name)].ToString());
                config_xml.WriteEndElement();
                config_xml.WriteStartElement(ConfigUtilities.kOmniDateModifiedElement);
                config_xml.WriteAttributeString(ConfigUtilities.kOmniWidthAttribute, column_widths[Convert.ToInt32(UserControls.ContentDisplay.ContentProperties.Property_DateModified)].ToString());
                config_xml.WriteEndElement();
                config_xml.WriteStartElement(ConfigUtilities.kOmniTypeElement);
                config_xml.WriteAttributeString(ConfigUtilities.kOmniWidthAttribute, column_widths[Convert.ToInt32(UserControls.ContentDisplay.ContentProperties.Property_Type)].ToString());
                config_xml.WriteEndElement();
                config_xml.WriteStartElement(ConfigUtilities.kOmniSizeElement);
                config_xml.WriteAttributeString(ConfigUtilities.kOmniWidthAttribute, column_widths[Convert.ToInt32(UserControls.ContentDisplay.ContentProperties.Property_Size)].ToString());
                config_xml.WriteEndElement();
              }
              config_xml.WriteEndElement(); // PropertyColumns

              config_xml.WriteEndElement(); // View
            }

            config_xml.WriteEndElement(); // Views
          }
        }
        config_xml.WriteEndElement(); // OmniConfig

        config_xml.WriteEndDocument();
        config_xml.Close();
      }
      catch (Exception e)
      {
        Logger.WriteLine(Logger.SeverityType.Error, "Exception while trying save out the configuration.\n" + e.ToString());

        config_xml.Close();

        if (null != old_config)
        {
          System.IO.File.WriteAllBytes(file_path, old_config);
        }

        return false;
      }

      return true;
    }

    // --- Private Interface ---
    private void ResizeViewGrid()
    {
      double row_height = Grid_Views.ActualWidth / _row_count;
      double column_width = Grid_Views.ActualHeight / _column_count;

      foreach (RowDefinition definition in Grid_Views.RowDefinitions)
      {
        definition.Height = new GridLength(row_height, GridUnitType.Star);
      }

      foreach (ColumnDefinition definition in Grid_Views.ColumnDefinitions)
      {
        definition.Width = new GridLength(column_width, GridUnitType.Star);
      }
    }

    private int GetRow(int index)
    {
      return index / _column_count;
    }

    private int GetColumn(int index)
    {
      return index % _column_count;
    }

    private int GetIndex(int row, int column)
    {
      return (row * _column_count) + column;
    }

    private bool LoadConfig(string config_path)
    {
      if (false == System.IO.File.Exists(config_path))
      {
        return false;
      }

      List<ConfigUtilities.ViewConfig> views = new List<ConfigUtilities.ViewConfig>();

      // Read in the values from the config file
      try
      {
        XmlReader config_reader = XmlReader.Create(config_path);
        if (false == config_reader.Read() || false == config_reader.IsStartElement(ConfigUtilities.kOmniConfigRoot))
        {
          return false;
        }

        config_reader = config_reader.ReadSubtree();
        while (config_reader.Read())
        {
          switch (config_reader.Name)
          {
            case ConfigUtilities.kOmniRowElement:
              _row_count = Convert.ToInt32(config_reader.GetAttribute(ConfigUtilities.kOmniCountAttribute));
              break;
            case ConfigUtilities.kOmniColumnElement:
              _column_count = Convert.ToInt32(config_reader.GetAttribute(ConfigUtilities.kOmniCountAttribute));
              break;
            case ConfigUtilities.kOmniViewsSection:
              XmlReader views_reader = config_reader.ReadSubtree();
              while (views_reader.Read())
              {
                switch (views_reader.Name)
                {
                  case ConfigUtilities.kOmniViewElement:
                    {
                      ConfigUtilities.ViewConfig new_view = new ConfigUtilities.ViewConfig();

                      // Row Attribute
                      string attribute = views_reader.GetAttribute(ConfigUtilities.kOmniRowAttribute);
                      if(null != attribute)
                        new_view._row = Convert.ToInt32(attribute);
                      // Column Attribute
                      attribute = views_reader.GetAttribute(ConfigUtilities.kOmniColumnAttribute);
                      if (null != attribute)
                        new_view._column = Convert.ToInt32(attribute);
                      // Directory Attribute
                      attribute = views_reader.GetAttribute(ConfigUtilities.kOmniDirectoryAttribute);
                      if (null != attribute)
                        new_view._directory = attribute;

                      XmlReader property_columns_reader = views_reader.ReadSubtree();
                      while (property_columns_reader.Read())
                      {
                        switch (property_columns_reader.Name)
                        {
                          case ConfigUtilities.kOmniNameElement:
                            {
                              Tuple<UserControls.ContentDisplay.ContentProperties, double> property_width =
                                new Tuple<UserControls.ContentDisplay.ContentProperties, double>(UserControls.ContentDisplay.ContentProperties.Property_Name, Convert.ToDouble(property_columns_reader.GetAttribute(ConfigUtilities.kOmniWidthAttribute)));

                              new_view._column_widths.Add(property_width);
                            }
                            break;
                          case ConfigUtilities.kOmniDateModifiedElement:
                            {
                              Tuple<UserControls.ContentDisplay.ContentProperties, double> property_width =
                                new Tuple<UserControls.ContentDisplay.ContentProperties, double>(UserControls.ContentDisplay.ContentProperties.Property_DateModified, Convert.ToDouble(property_columns_reader.GetAttribute(ConfigUtilities.kOmniWidthAttribute)));

                              new_view._column_widths.Add(property_width);
                            }
                            break;
                          case ConfigUtilities.kOmniTypeElement:
                            {
                              Tuple<UserControls.ContentDisplay.ContentProperties, double> property_width =
                                new Tuple<UserControls.ContentDisplay.ContentProperties, double>(UserControls.ContentDisplay.ContentProperties.Property_Type, Convert.ToDouble(property_columns_reader.GetAttribute(ConfigUtilities.kOmniWidthAttribute)));

                              new_view._column_widths.Add(property_width);
                            }
                            break;
                          case ConfigUtilities.kOmniSizeElement:
                            {
                              Tuple<UserControls.ContentDisplay.ContentProperties, double> property_width =
                                new Tuple<UserControls.ContentDisplay.ContentProperties, double>(UserControls.ContentDisplay.ContentProperties.Property_Size, Convert.ToDouble(property_columns_reader.GetAttribute(ConfigUtilities.kOmniWidthAttribute)));

                              new_view._column_widths.Add(property_width);
                            }
                            break;
                        }
                      }

                      views.Add(new_view);
                    }
                    break;
                }
              }
              break;
          }
        }
      }
      catch(Exception e)
      {
        Logger.WriteLine(Logger.SeverityType.Error, "Exception while trying load configuration file at " + config_path + ".\n" + e.ToString());
        return false;
      }

      // Apply the configuration
      ConfigureViewGrid(_row_count, _column_count);

      // Todo: Add a check to make sure the defined views are within the defined grid size

      int index;
      foreach(ConfigUtilities.ViewConfig view in views)
      {
        index = GetIndex(view._row, view._column);
        UserControls.ContentDisplay.ContentProperties[] properties = new UserControls.ContentDisplay.ContentProperties[view._column_widths.Count];
        double[] widths = new double[view._column_widths.Count];
        for(int i = 0; i < view._column_widths.Count; ++i)
        {
          properties[i] = view._column_widths[i].Item1;
          widths[i] = view._column_widths[i].Item2;
        }

        _directory_views[index].SetPropertyColumnWidths(properties, widths);
        _directory_views[index].LoadDirectory(view._directory);
      }

      return true;
    }

    private void LoadDefaultConfig()
    {
      if(false == LoadConfig(ConfigUtilities.kDefaultConfigPath))
      {
        // Config not found
        ConfigureViewGrid(1, 1);
        _directory_views[0].LoadDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));

        // Create the Omni folder in the user's Local App Data
        if(false == System.IO.Directory.Exists(ConfigUtilities.kOmniAppDataFolder))
        {
          System.IO.Directory.CreateDirectory(ConfigUtilities.kOmniAppDataFolder);
        }

        // Create the config file
        SaveCurrentConfiguration(ConfigUtilities.kDefaultConfigPath);
      }
    }

    private void ParseCommandLine()
    {
      CommandLineParser argument_parser = new CommandLineParser();

      string config = "";
      if(argument_parser.GetValue(CommandLineParser.kArgument_Config, ref config))
      {
        if(false == LoadConfig(config))
        {
          LoadDefaultConfig();
        }
      }
      else if(argument_parser.HasArgument(CommandLineParser.kArgument_Rows) || argument_parser.HasArgument(CommandLineParser.kArgument_Columns))
      {
        argument_parser.GetValue(CommandLineParser.kArgument_Rows, ref _row_count);
        argument_parser.GetValue(CommandLineParser.kArgument_Columns, ref _column_count);

        ConfigureViewGrid(_row_count, _column_count);
      }
      else
      {
        LoadDefaultConfig();
      }
    }

    // --- Private Events ---
    // --- Grid Events
    private void Grid_Views_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ResizeViewGrid();
    }

    // --- Window Events
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveCurrentConfiguration(ConfigUtilities.kDefaultConfigPath);
    }
  }
}
