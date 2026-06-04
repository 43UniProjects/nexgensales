using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NexGenSales.Core
{
    public static class ChartCaptureHelper
    {
        /// <summary>
        /// Captures a visible WPF UI Element (like a LiveCharts2 graph) and converts it to a PNG byte array.
        /// </summary>
        public static byte[] CaptureToPng(UIElement targetControl)
        {
            if (targetControl == null) return null;

            // Get the actual rendered size of the chart on the screen
            int width = (int)targetControl.RenderSize.Width;
            int height = (int)targetControl.RenderSize.Height;

            if (width == 0 || height == 0)
                throw new System.InvalidOperationException("Control must be visible and rendered to capture.");

            // Create a bitmap canvas
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);

            // Draw the WPF control onto the canvas
            renderTarget.Render(targetControl);

            // Encode the canvas to a PNG
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
    }
}