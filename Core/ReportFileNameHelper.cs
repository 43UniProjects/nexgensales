using System;
using System.IO;
using System.Linq; // Required for the .Any() LINQ method

namespace NexGenSales.Core
{
    /// <summary>
    /// Generates standardised, timestamped file names for analytics reports.
    /// Format: NexGenSales_{Type}_{dd-MMM-yyyy_hh-mm-tt}.pdf
    /// Example: NexGenSales_Sales_05-Jun-2026_02-30-PM.pdf
    /// </summary>
    public static class ReportFileNameHelper
    {
        private const string OutputDirectoryName = "Reports";

        /// <summary>
        /// Builds a full absolute file path for a report of the given type,
        /// stamped with the current local date and time.
        /// Creates the output directory if it does not already exist.
        /// </summary>
        /// <param name="reportType">
        /// A short label describing the report content, e.g. "Sales", "Expenses".
        /// Spaces are replaced with underscores automatically.
        /// </param>
        /// <returns>Full absolute path to the generated PDF file.</returns>
        public static string Generate(string reportType)
        {
            // Sanitise: strip spaces so the filename is clean
            string safeType = reportType.Replace(" ", "");

            string timestamp = DateTime.Now.ToString("dd-MMM-yyyy_hh-mm-tt");
            string fileName  = $"NexGenSales_{safeType}_{timestamp}.pdf";

            string reportsDirectory;

#if DEBUG
            // DEVELOPMENT: Bulletproof search for the project folder
            reportsDirectory = Path.Combine(GetProjectRootDirectory(), OutputDirectoryName);
#else
            // PRODUCTION: Point to the compiled executable's folder
            reportsDirectory = Path.Combine(AppContext.BaseDirectory, OutputDirectoryName);
#endif

            // Ensure the output directory exists before the caller tries to write
            Directory.CreateDirectory(reportsDirectory);
       
            // Return the full, safe absolute path
            return Path.Combine(reportsDirectory, fileName);
        }

#if DEBUG
        /// <summary>
        /// Bulletproof method to find the actual project folder during Debug mode,
        /// regardless of how deep the bin/Debug/x64/netX.X folder structure is.
        /// Wrapped in a compiler flag so it isn't compiled into the final release build!
        /// </summary>
        private static string GetProjectRootDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);

            // Keep climbing up the parent directories until we see the .csproj file
            while (directory != null && !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            // If we found the project root, return it. Otherwise, fallback to the base directory.
            return directory?.FullName ?? AppContext.BaseDirectory;
        }
#endif
    }
}