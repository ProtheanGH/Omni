using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Omni.Utilities
{
  class DrawingUtilities
  {
    // Public Interface
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
  }
}
