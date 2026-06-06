using System;
using System.IO;
using Microsoft.Data.Sqlite;
using NexGenSales.Services.Data.Repository;

namespace NexGenSales.Services;

public class DatabaseMigrationService
{
    private readonly string _connectionString;

    private const int TargetVersion = 2; // increment this by one for each new addition to this file

    public DatabaseMigrationService()
    {

        Console.WriteLine("[MIGRATION] Initializing...");

        string dbFolder;

#if DEBUG
        // DEVELOPMENT
        dbFolder = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")), "Database");
#else
        // PRODUCTION
        dbFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Database"));
#endif

        Directory.CreateDirectory(dbFolder);

        string dbPath = Path.Combine(dbFolder, "app.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public void EnsureMigrated()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        int currentVersion = GetUserVersion(connection);

        if (currentVersion > TargetVersion)
        {
            throw new InvalidOperationException($"Database version ({currentVersion}) is newer than application code ({TargetVersion}).");
        }

        Console.WriteLine($"[MIGRATION] Database currently at version {currentVersion}");

        if (currentVersion < 1)
        {
            Console.WriteLine("[MIGRATION] Running version 1 script");
            Console.WriteLine("[MIGRATION] Creating database tables, SalesRecords, ExpensesRecords");
            var salesRepo = new SalesRecordRepository(new SqliteService());
            salesRepo.InitializeTable();
            var expensesRepo = new ExpensesRecordRepository(new SqliteService());
            expensesRepo.InitializeTable();

            currentVersion = 1;
        }

        SetUserVersion(connection, TargetVersion);
        Console.WriteLine("[MIGRATION] Database migrated successfully");
    }

    private int GetUserVersion(SqliteConnection connection)
    {

        Console.WriteLine("[MIGRATION] Aquiring currect database version...");
        using var cmd = new SqliteCommand("PRAGMA user_version;", connection);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private void SetUserVersion(SqliteConnection connection, int version)
    {
        // PRAGMA statements do not accept parameterized inputs, string interpolation is required here
        using var cmd = new SqliteCommand($"PRAGMA user_version = {version};", connection);
        cmd.ExecuteNonQuery();
    }
}