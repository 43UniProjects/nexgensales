using Microsoft.Data.Sqlite;

namespace NexGenSales.Core
{
    public class SqlTransactionQueue(SqliteService sqliteService)
    {
        private readonly SqliteService _sqliteService = sqliteService;
        
        private readonly List<(string Sql, Dictionary<string, object> Parameters)> _commands = [];

        public void Enqueue(string sql, Dictionary<string, object> parameters)
        {
            _commands.Add((sql, parameters));
        }

        public async Task CommitAll()
        {
            if (_commands.Count == 0) return;

            // Use your service to generate the connection
            using var connection = _sqliteService.CreateConnection();
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var (Sql, Parameters) in _commands)
                {
                    using var command = new SqliteCommand(Sql, connection, transaction);
                    
                    foreach (var param in Parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                _commands.Clear(); 
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[TRANSACTION FAILED] Rolled back batch. Error: {ex.Message}");
                throw;
            }
        }
    }
}