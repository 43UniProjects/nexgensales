using System;
using System.Collections.Generic;
using System.Linq;
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Services.Data.Repository;
using NexGenSales.Services;

namespace NexGenSales.Services;

public class ExcelFileImportService<TEnum, TModel> where TEnum : struct, Enum
{
    private readonly ExcelParser _parser;
    private readonly Func<Dictionary<TEnum, object>, TModel> _rowMapper;

    public List<TModel> Records { get; private set; }

    /// <param name="parser">The engine that reads the Excel file</param>
    /// <param name="rowMapper">A function that knows how to convert the Dictionary into the specific Model</param>
    public ExcelFileImportService(ExcelParser parser, Func<Dictionary<TEnum, object>, TModel> rowMapper)
    {
        Console.WriteLine($"[ExcelFileImportService] Initilizing...");
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _rowMapper = rowMapper ?? throw new ArgumentNullException(nameof(rowMapper));
        Records = [];
    }

    public bool ImportFiles(IEnumerable<string> filePaths)
    {
        if (filePaths == null)
        {
            Console.WriteLine($"[ExcelFileImportService] Failed to import files - param filePaths is Null");
            return false;
        }

        Records.Clear();

        foreach (var filePath in filePaths)
        {
            Console.WriteLine($"[ExcelFileImportService] Importing file @({filePath})...");
            var rawFileData = _parser.ParseFile<TEnum>(filePath);

            if (!Validate(rawFileData))
            {
                Console.WriteLine($"[ExcelFileImportService] Validation failed - Invalid Field Names");
                return false;
            }

            foreach (var row in rawFileData)
            {
                Records.Add(_rowMapper(row));
            }
        }

        return true;
    }

    private static bool Validate(List<Dictionary<TEnum, object>> fileData)
    {
        if (fileData == null || fileData.Count == 0)
        {

            Console.WriteLine($"[ExcelFileImportService] Validation failed - No data avaiable");
            return false;
        }

        var requiredKeys = Enum.GetValues<TEnum>().ToHashSet();

        var firstRowKeys = fileData.First().Keys.ToHashSet();

        return requiredKeys.All(firstRowKeys.Contains);
    }
}