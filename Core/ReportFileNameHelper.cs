using System;
using System.IO;

namespace NexGenSales.Core
{
    /// <summary>
    /// Generates standardised, timestamped file names for analytics reports.
    /// Format: NexGenSales_{Type}_{yyyyMMdd}_{HHmmss}.pdf
    /// Example: NexGenSales_Sales_20260605_143022.pdf
    /// </summary>
    public static class ReportFileNameHelper
    {
        private const string OutputDirectory = "Reports";

        /// <summary>
        /// Builds a full relative file path for a report of the given type,
        /// stamped with the current local date and time.
        /// Creates the output directory if it does not already exist.
        /// </summary>
        /// <param name="reportType">
        /// A short label describing the report content, e.g. "Sales", "Expenses", "ProfitMargin".
        /// Spaces are replaced with underscores automatically.
        /// </param>
        /// <returns>Relative path: Reports/NexGenSales_{type}_{date}_{time}.pdf</returns>
        public static string Generate(string reportType)
        {
            // Sanitise: strip spaces so the filename is clean
            string safeType = reportType.Replace(" ", "");

            string timestamp = DateTime.Now.ToString("dd-MMM-yyyy_hh-mm-tt");
            string fileName  = $"NexGenSales_{safeType}_{timestamp}.pdf";

            // Ensure the output directory exists before the caller tries to write
            Directory.CreateDirectory(OutputDirectory);

            return Path.Combine(OutputDirectory, fileName);
        }
    }
}
