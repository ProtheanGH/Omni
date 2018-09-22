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


      // Todo: Just for testing, should probably start with a size of 1, 1
      ConfigureViewGrid(1, 1);
      //ConfigureViewGrid(2, 2);
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

        int row = i / columns;
        int column = i % columns;
        column = column == 0 ? columns - 1 : column - 1;

        Grid.SetRow(_directory_views[i], row);
        Grid.SetColumn(_directory_views[i], column);
      }
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

    // --- Private Events ---
    private void Grid_Views_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ResizeViewGrid();
    }
  }
}
