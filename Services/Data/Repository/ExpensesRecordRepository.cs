using Microsoft.Data.Sqlite;
using NexGenSales.Models;

namespace NexGenSales.Services.Data.Repository;

class ExpenseRecordRepository(SqliteService sqliteService) : Repository<ExpensesRecord>(sqliteService)
{
    public override void InitializeTable()
    {
        string query = @"
            CREATE TABLE IF NOT EXISTS ExpensesRecord (
                Transaction_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Date_Recorded DATETIME NOT NULL,
                Expense_Category TEXT,
                Specific_Type TEXT,
                Amount REAL NOT NULL,
                Asset_ID TEXT
            );";
            

        ExecuteNonQuery(query);
    }

    public override async Task<List<ExpensesRecord>> GetAll()
    {
        return await Get("SELECT * FROM ExpensesRecord;");
    }


    public override async Task<List<ExpensesRecord>> Get(string sql)
    {
        var expenseRecords = new List<ExpensesRecord>();

        var connection = sqliteService.CreateConnection();

        await connection.OpenAsync();

        using var command = new SqliteCommand(sql, connection);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var model = new ExpensesRecord
            {
                Expense_ID = reader.GetInt32(reader.GetOrdinal("Expense_ID")),
                Date_Recorded = reader.GetDateTime(reader.GetOrdinal("Date_Recorded")),
                Expense_Category = reader.IsDBNull(reader.GetOrdinal("Expense_Category")) ? null : reader.GetString(reader.GetOrdinal("Expense_Category")),
                Specific_Type = reader.IsDBNull(reader.GetOrdinal("Specific_Type")) ? null : reader.GetString(reader.GetOrdinal("Specific_Type")),
                Amount = reader.GetDouble(reader.GetOrdinal("Amount")),
                Asset_ID = reader.IsDBNull(reader.GetOrdinal("Asset_ID")) ? null : reader.GetString(reader.GetOrdinal("Asset_ID")),
            };

            expenseRecords.Add(model);
        }

        return expenseRecords;
    }

    public override bool Insert(ExpensesRecord newRecord)
    {
        string sql = @"
            INSERT INTO ExpensesRecord(Date_Recorded, Expense_Category, Specific_Type, Amount, Asset_ID)
            VALUES(@Date_Recorded, @Expense_Category, @Specific_Type, @Amount, @Asset_ID)
        ";

        var connection = sqliteService.CreateConnection();
        connection.Open();

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Date_Recorded", newRecord.Date_Recorded);
        command.Parameters.AddWithValue("@Expense_Category", newRecord.Expense_Category ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Specific_Type", newRecord.Specific_Type ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Amount", newRecord.Amount);
        command.Parameters.AddWithValue("@Asset_ID", newRecord.Asset_ID ?? (object)DBNull.Value);

        try
        {
            command.ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            connection.Close();
        }
    }

    public override void InsertMany(List<ExpensesRecord> newRecords)
    {
        foreach (var record in newRecords)
        {
            if (!Insert(record))
            {
                throw new InvalidOperationException("Failed to insert one or more expense records.");
            }
        }
    }
}