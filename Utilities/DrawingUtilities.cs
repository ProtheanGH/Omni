using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Omni.Utilities
{
  static class DrawingUtilities
  {
    // --- Private Variables ---
    private static double s_font_size_multipier = 0.55;
    
    // --- Public Variables ---
    public static int MaxCharacterWidth { get; private set; }

    // --- Contructor ---
    static DrawingUtilities()
    {
      MaxCharacterWidth = 22;
    }

    // --- Public Interface ---
    public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
    {
      if (bitmap == null)
        throw new ArgumentNullException("bitmap");

      var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

      var bitmapData = bitmap.LockBits(
          rect,
          ImageLockMode.ReadWrite,
          System.Drawing.Imaging.PixelFormat.Format32bppArgb);

      try
      {
        var size = (rect.Width * rect.Height) * 4;

        return BitmapSource.Create(
            bitmap.Width,
            bitmap.Height,
            bitmap.HorizontalResolution,
            bitmap.VerticalResolution,
            PixelFormats.Bgra32,
            null,
            bitmapData.Scan0,
            size,
            bitmapData.Stride);
      }
      finally
      {
        bitmap.UnlockBits(bitmapData);
      }
    }

    public static int GetMaximumCharactersToDisplay(ref TextBox control)
    {
      // Rough estimation of how many characters can be displayed for the size of the textbox
      return (int)(control.ActualWidth / (control.FontSize * s_font_size_multipier));
    }

    public static int GetMaximumCharactersToDisplay(ref TextBlock control)
    {
      // Rough estimation of how many characters can be displayed for the size of the textbox
      return (int)(control.ActualWidth / (control.FontSize * s_font_size_multipier));
    }
  }
}
