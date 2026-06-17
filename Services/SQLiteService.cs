using System.IO;
using Microsoft.Data.Sqlite;


public class SqliteService
{
    private readonly string _connectionString;

    public SqliteService()
    {
        Console.WriteLine($"[SqliteService] Initializing...");
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
        Console.WriteLine($"[SqliteService] Creating new connection...");
        return new SqliteConnection(_connectionString);
    }
}