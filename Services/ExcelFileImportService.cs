using System;
using System.Collections.Generic;
using System.Linq;
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Services.Data.Repository;
using NexGenSales.Services;

namespace NexGenSales.Services
{
    /// <summary>
    /// A generic service responsible for parsing and mapping Excel files into strongly-typed domain models.
    /// Incorporates asynchronous progress tracking capabilities for enhanced UI responsiveness.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration representing the expected Excel column headers.</typeparam>
    /// <typeparam name="TModel">The target entity model to which the data will be mapped.</typeparam>
    public class ExcelFileImportService<TEnum, TModel> where TEnum : struct, Enum
    {
        private readonly ExcelParser _parser;
        private readonly Func<Dictionary<TEnum, object>, TModel> _rowMapper;

        /// <summary>
        /// Gets the collection of successfully parsed and mapped records.
        /// </summary>
        public List<TModel> Records { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ExcelFileImportService.
        /// </summary>
        /// <param name="parser">The underlying engine utilized for extracting raw data from Excel files.</param>
        /// <param name="rowMapper">A delegate function defining the mapping logic from raw dictionary rows to the target model.</param>
        public ExcelFileImportService(ExcelParser parser, Func<Dictionary<TEnum, object>, TModel> rowMapper)
        {
            Console.WriteLine($"[ExcelFileImportService] Initializing subsystem...");
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _rowMapper = rowMapper ?? throw new ArgumentNullException(nameof(rowMapper));
            Records = [];
        }

        /// <summary>
        /// Sequentially processes a collection of file paths, validates their structural integrity, 
        /// and maps them into the Records collection while reporting progress.
        /// </summary>
        /// <param name="filePaths">An enumerable collection of absolute file paths to be processed.</param>
        /// <param name="progress">An optional progress reporter to communicate the current completion percentage back to the main UI thread.</param>
        /// <returns>True if all files are validated and parsed successfully; otherwise, false.</returns>
        public bool ImportFiles(IEnumerable<string> filePaths, IProgress<int> progress = null)
        {
            if (filePaths == null || !filePaths.Any())
            {
                Console.WriteLine($"[ExcelFileImportService] Import aborted: Parameter 'filePaths' is null or empty.");
                return false;
            }

            Records.Clear();

            var filesList = filePaths.ToList();
            int totalFiles = filesList.Count;
            int processedFiles = 0;

            foreach (var filePath in filesList)
            {
                Console.WriteLine($"[ExcelFileImportService] Parsing designated file @({filePath})...");

                var rawFileData = _parser.ParseFile<TEnum>(filePath);

                if (!Validate(rawFileData))
                {
                    Console.WriteLine($"[ExcelFileImportService] Validation failure: Structural mismatch detected in headers.");
                    return false;
                }

                foreach (var row in rawFileData)
                {
                    Records.Add(_rowMapper(row));
                }

                processedFiles++;
                if (progress != null)
                {
                    //Scale the reading progress to visually max out at 90%
                    int percentage = (int)Math.Round((double)processedFiles / totalFiles * 90);
                    progress.Report(percentage);
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the structural integrity of the extracted data against the required domain schema defined by TEnum.
        /// </summary>
        private static bool Validate(List<Dictionary<TEnum, object>> fileData)
        {
            if (fileData == null || fileData.Count == 0)
            {
                Console.WriteLine($"[ExcelFileImportService] Validation failure: The provided dataset is empty or null.");
                return false;
            }

            var requiredKeys = Enum.GetValues<TEnum>().ToHashSet();
            var firstRowKeys = fileData.First().Keys.ToHashSet();

            // Strictly enforces that all required enumerated keys are actively present within the parsed file headers
            return requiredKeys.All(firstRowKeys.Contains);
        }
    }
}