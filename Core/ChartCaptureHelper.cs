using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NexGenSales.Core
{
    /// <summary>
    /// Captures a WPF FrameworkElement as a PNG byte array suitable for embedding in a PDF report.
    /// Extracted into Core so it can be reused by any reporting pipeline.
    /// </summary>
    public static class ChartCaptureHelper
    {
        /// <summary>
        /// Renders the given visual element to an in-memory PNG image.
        /// Returns null if the element has zero dimensions (not yet laid out).
        /// </summary>
        public static byte[]? CaptureToImage(FrameworkElement visual)
        {
            if (visual == null) return null;

            // Ensure WPF has calculated final dimensions before capturing
            visual.UpdateLayout();

            int width  = (int)visual.ActualWidth;
            int height = (int)visual.ActualHeight;

            if (width == 0 || height == 0) return null;

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }
    }
}
