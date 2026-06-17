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

            // Ensure WPF has calculated final dimensions
            visual.UpdateLayout();

            int width = (int)visual.ActualWidth;
            int height = (int)visual.ActualHeight;

            if (width == 0 || height == 0) return null;

            // Add an extra margin at the bottom to prevent x-axis labels from being cropped.
            // LiveCharts often draws labels slightly outside the ActualHeight layout bounds.
            int captureHeight = height + 45;

            // Use 300 DPI for high-quality printing (default is 96)
            double dpi = 300;
            double scale = dpi / 96d;

            int pixelWidth = (int)(width * scale);
            int pixelHeight = (int)(captureHeight * scale);

            var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);

            // Fill with a white background so the extra margin isn't transparent
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, captureHeight));
            }
            rtb.Render(dv);
            rtb.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var stream = new MemoryStream();
            encoder.Save(stream);

            return stream.ToArray();
        }
    }
}
