using System;
using System.IO;
using Microsoft.Data.Sqlite;
using NexGenSales.Models;

namespace NexGenSales.Services.Data;

public class DatabaseMigrationService
{
    private readonly string _connectionString;

    private const int TargetVersion = 2;

    public DatabaseMigrationService()
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public void EnsureMigrated()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Get the current version of the physical schema file
        int currentVersion = GetUserVersion(connection);

        // If the database file is newer than the code, stop to prevent corruption
        if (currentVersion > TargetVersion)
        {
            throw new InvalidOperationException($"Database version ({currentVersion}) is newer than application code ({TargetVersion}).");
        }

        // Sequential migration pipeline
        if (currentVersion < 1)
        {
            var salesRepo = new SalesRecordRepository(new SqliteService());
            salesRepo.InitializeTable();

            currentVersion = 1;
        }

        // Save the successfully reached version number back into the SQLite header
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

