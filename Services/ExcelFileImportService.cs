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
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _rowMapper = rowMapper ?? throw new ArgumentNullException(nameof(rowMapper));
        Records = [];
    }

    public bool ImportFiles(IEnumerable<string> filePaths)
    {
        if (filePaths == null) throw new ArgumentNullException(nameof(filePaths));

        Records.Clear();

        foreach (var filePath in filePaths)
        {
            var rawFileData = _parser.ParseFile<TEnum>(filePath);

            if (!Validate(rawFileData)) return false;

            foreach (var row in rawFileData)
            {
                Records.Add(_rowMapper(row));
            }
        }

        return true;
    }

    private static bool Validate(List<Dictionary<TEnum, object>> fileData)
    {
        if (fileData == null || fileData.Count == 0) return false;

        var requiredKeys = Enum.GetValues<TEnum>().ToHashSet();

        var firstRowKeys = fileData.First().Keys.ToHashSet();

        return requiredKeys.All(firstRowKeys.Contains);
    }


    public async Task<bool> Store()
    {

        if(Records == null || Records.Count <= 0) return false;

        Repository<TModel> repo;
        SqliteService sqliteService = new();


        if (typeof(TModel) == typeof(SalesRecord))
        {
            repo = (Repository<TModel>)(object)new SalesRecordRepository(sqliteService);
        }
        else
        {
            repo = (Repository<TModel>)(object)new ExpenseRecordRepository(sqliteService);
        }

        try
        { repo.InsertMany(Records); }
        catch (InvalidOperationException)
        {
            return false;
        }
        return true;
    }
}