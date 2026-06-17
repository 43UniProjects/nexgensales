using Microsoft.Data.Sqlite;
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Models.Enums;
namespace NexGenSales.Services.Data.Repository;

class SalesRecordRepository(SqliteService sqliteService) : Repository<SalesRecord>(sqliteService)
{

    public override void InitializeTable()
    {
        string query = @"
            CREATE TABLE IF NOT EXISTS SalesRecord (
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

    public override async Task<List<SalesRecord>> GetAll()
    {
        return await Get("SELECT * FROM SalesRecord");
    }


    public override async Task<List<SalesRecord>> Get(string sql)
    {
        var SalesRecords = new List<SalesRecord>();

        using var connection = sqliteService.CreateConnection();

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

    public override bool Insert(SalesRecord newRecord, SqliteConnection connection)
    {
        const string query = @"
            INSERT INTO SalesRecord (
                Date_Time,
                Item_ID,
                Supplier_ID,
                Quantity_Sold,
                Unit_Purchase_Cost,
                Unit_Sale_Price,
                Allowed_Discount,
                Net_Revenue,
                Current_Stock
            )
            VALUES (
                @Date_Time,
                @Item_ID,
                @Supplier_ID,
                @Quantity_Sold,
                @Unit_Purchase_Cost,
                @Unit_Sale_Price,
                @Allowed_Discount,
                @Net_Revenue,
                @Current_Stock
            );";



        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Date_Time", newRecord.Date_Time);
        command.Parameters.AddWithValue("@Item_ID", newRecord.Item_ID);
        command.Parameters.AddWithValue("@Supplier_ID", newRecord.Supplier_ID);
        command.Parameters.AddWithValue("@Quantity_Sold", newRecord.Quantity_Sold);
        command.Parameters.AddWithValue("@Unit_Purchase_Cost", newRecord.Unit_Purchase_Cost);
        command.Parameters.AddWithValue("@Unit_Sale_Price", newRecord.Unit_Sale_Price);
        command.Parameters.AddWithValue("@Allowed_Discount", newRecord.Allowed_Discount);
        command.Parameters.AddWithValue("@Net_Revenue", newRecord.Net_Revenue);
        command.Parameters.AddWithValue("@Current_Stock", newRecord.Current_Stock);

        return command.ExecuteNonQuery() > 0;
    }

    public override void InsertMany(List<SalesRecord> newRecords)
    {

        using var connection = sqliteService.CreateConnection();
        connection.Open();
        foreach (var record in newRecords)
        {
            if (!Insert(record, connection))
            {
                string err = $"Failed to insert record of {record.Date_Time:yyyy.MM.dd HH:mm:ss}.";
                Console.WriteLine($"[DB ERROR] {err}");
                throw new InvalidOperationException(err);
            }
        }
    }


    public override async Task<List<SalesRecord>> Update<TENUM, TVAL>(SqlTransactionQueue queue, int recordID, TENUM fieldName, TVAL value)
    {
        if (fieldName is not SalesRecordField salesField)
        {
            throw new ArgumentException($"Invalid enum type provided to Sales update. Expected {nameof(SalesRecordField)}.");
        }

        string columnName = salesField.ToColumnName();

        string sql = $@"
            UPDATE SalesRecord 
            SET {columnName} = @Value 
            WHERE Transaction_ID = @RecordID
        ";

        var parameters = new Dictionary<string, object>{
                { "@RecordID", recordID },
                { "@Value", value != null ? value : DBNull.Value }
        };

        queue.Enqueue(sql, parameters);

        return await GetAll();
    }
}