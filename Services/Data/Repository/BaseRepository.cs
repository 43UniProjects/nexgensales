using Microsoft.Data.Sqlite;

namespace NexGenSales.Services.Data.Repository;

public abstract class Repository<TModel>
{
    protected readonly SqliteService sqliteService;

    protected Repository(SqliteService _sqliteService)
    {
        sqliteService = _sqliteService;
        InitializeTable();
    }

    public abstract void InitializeTable();

    public abstract void InsertMany(List<TModel> newRecords);

    public abstract bool Insert(TModel newRecord, SqliteConnection connection);

    public abstract Task<List<TModel>> GetAll();

    public abstract Task<List<TModel>> Get(string sql);

    protected void ExecuteNonQuery(string sql)
    {
        using var connection = sqliteService.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
