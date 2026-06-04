using System;
using Microsoft.Data.Sqlite;
using DayOrder.Services;

namespace DayOrder.Services.Data
{
    public abstract class BaseRepository
    {
        protected readonly SqliteService sqliteService;

        protected BaseRepository(SqliteService _sqliteService)
        {
            sqliteService = _sqliteService;
            InitializeTable();
        }

        // Every table repository must define its own setup SQL script
        public abstract void InitializeTable();

        // Helper to run a raw initialization script safely
        protected void ExecuteNonQuery(string sql)
        {
            using var connection = sqliteService.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
}
