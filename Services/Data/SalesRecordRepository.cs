using System;
using DayOrder.Services.Data;
using Microsoft.Data.Sqlite;

using NexGenSales.Models;

namespace NexGenSales.Services.Data;

class SalesRecordRepository(SqliteService sqliteService) : BaseRepository(sqliteService)
{

    public override void InitializeTable()
    {
        string query = @"
            CREATE TABLE SalesRecord (
            Transaction_ID INTEGER PRIMARY KEY AUTOINCREMENT,
            Date_Time DATETIME NOT NULL,
            Item_ID TEXT NOT NULL,
            Supplier_ID TEXT NOT NULL,
            Quantity_Sold REAL NOT NULL,
            Unit_Purchase_Cost REAL NOT NULL,
            Unit_Sale_Price REAL NOT NULL,
            Allowed_Discount REAL DEFAULT 0,
            Net_Revenue REAL NOT NULL,
            Current_Stock REAL NOT NULL
        );";

        ExecuteNonQuery(query);
    }

    public async Task<List<SalesRecord>> getAllSalesRecords()
    {
        return await GetSalesRecords("SELECT * FROM SalesRecord");
    }


    public async Task<List<SalesRecord>> GetSalesRecords(string sql)
    {
        var SalesRecords = new List<SalesRecord>();

        var connection = sqliteService.CreateConnection();

        await connection.OpenAsync();

        using var command = new SqliteCommand(sql, connection);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var model = new SalesRecord
            {
                Transaction_ID = reader.GetInt32(reader.GetOrdinal("Transaction_ID")),
                Date_Time = reader.GetDateTime(reader.GetOrdinal("Date_Time")),
                Item_ID = reader.GetString(reader.GetOrdinal("Item_ID")),
                Supplier_ID = reader.GetString(reader.GetOrdinal("Supplier_ID")),
                Quantity_Sold = reader.GetDouble(reader.GetOrdinal("Quantity_Sold")),
                Unit_Purchase_Cost = reader.GetDouble(reader.GetOrdinal("Unit_Purchase_Cost")),
                Unit_Sale_Price = reader.GetDouble(reader.GetOrdinal("Unit_Sale_Price")),
                Allowed_Discount = reader.GetDouble(reader.GetOrdinal("Allowed_Discount")),
                Net_Revenue = reader.GetDouble(reader.GetOrdinal("Net_Revenue")),
                Current_Stock = reader.GetDouble(reader.GetOrdinal("Current_Stock"))
            };

            SalesRecords.Add(model);
        }

        return SalesRecords;

    }

    public async Task<bool> InsertNewRecord(SalesRecord newRecord)
    {
        

        return true;
    }



}