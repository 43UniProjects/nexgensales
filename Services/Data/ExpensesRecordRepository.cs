using System;
using DayOrder.Services.Data;
using Microsoft.Data.Sqlite;

using NexGenSales.Models;

namespace NexGenSales.Services.Data;

class ExpenseRecordRepository(SqliteService sqliteService) : BaseRepository(sqliteService)
{
    public override void InitializeTable()
    {
        string query = @"
            CREATE TABLE IF NOT EXISTS ExpensesRecord (
                Transaction_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Date_Time DATETIME NOT NULL,
                Expense_Type TEXT NOT NULL,
                Amount REAL NOT NULL,
                Supplier_ID TEXT,
                Notes TEXT
            );";

        ExecuteNonQuery(query);
    }

    public async Task<List<ExpensesRecord>> GetAllExpensesRecords()
    {
        return await GetExpensesRecords("SELECT * FROM ExpensesRecord");
    }

    public async Task<List<ExpensesRecord>> GetExpensesRecords(string sql)
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
                Expense_Category = reader.GetString(reader.GetOrdinal("Expense_Category")),
                Specific_Type = reader.GetString(reader.GetOrdinal("Specific_Type")),
                Amount = reader.GetDouble(reader.GetOrdinal("Amount")),
                Asset_ID = reader.IsDBNull(reader.GetOrdinal("Asset_ID")) ? null : reader.GetString(reader.GetOrdinal("Asset_ID")),
            };

            expenseRecords.Add(model);
        }

        return expenseRecords;
    }
}