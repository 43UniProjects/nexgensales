using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using DocumentFormat.OpenXml.Drawing.Charts;


public class SqliteService
{
    private readonly string _connectionString;

    public SqliteService()
    {
        string databaseDirectory;

        #if DEBUG
        // DEVELOPMENT
        databaseDirectory = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")), "Database");
        #else
        // PRODUCTION
        databaseDirectory = Path.Combine(AppContext.BaseDirectory, "Database");
        #endif

        Directory.CreateDirectory(databaseDirectory);

        string dbPath = Path.Combine(databaseDirectory, "app.db");
        _connectionString = $"Data Source={dbPath}";
    }
    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}