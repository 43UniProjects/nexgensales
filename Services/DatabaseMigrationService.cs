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
        // IMPORTENT!: upcomment below line when generating .exe and remove conflicting one
        // string dbFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Database"));
        string dbFolder = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")), "Database");

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

        if (currentVersion < 1)
        {
            var salesRepo = new SalesRecordRepository(new SqliteService());
            salesRepo.InitializeTable();
            var expensesRepo = new ExpenseRecordRepository(new SqliteService());
            expensesRepo.InitializeTable();

            currentVersion = 1;
        }

        SetUserVersion(connection, TargetVersion);
    }

    private int GetUserVersion(SqliteConnection connection)
    {
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