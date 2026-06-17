using Microsoft.Data.Sqlite;
using NexGenSales.Models;

namespace NexGenSales.Services.Data.Repository;


class RecordMetadataRepository(SqliteService sqliteService) : Repository<RecordMetadata>(sqliteService)
{

    public override void InitializeTable()
    {
        string query = @"
            CREATE TABLE IF NOT EXISTS RecordMetadata (
                Record_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Record_Date DATETIME NOT NULL,
                Upload_Date DATETIME NOT NULL,
                Record_Type TEXT NOT NULL,
                Process_State TEXT NOT NULL
            );";


        ExecuteNonQuery(query);
    }

    public override async Task<List<RecordMetadata>> GetAll()
    {
        return await Get("SELECT * FROM RecordMetadata;");
    }

    public override async Task<List<RecordMetadata>> Get(string sql)
    {
        var recordMetadata = new List<RecordMetadata>();

        using var connection = sqliteService.CreateConnection();

        await connection.OpenAsync();

        using var command = new SqliteCommand(sql, connection);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var model = new RecordMetadata
            {
                Record_ID = reader.GetInt32(reader.GetOrdinal("Record_ID")),
                Record_Date = reader.GetDateTime(reader.GetOrdinal("Record_Date")),
                Upload_Date = reader.GetDateTime(reader.GetOrdinal("Upload_Date")),
                Record_Type = reader.IsDBNull(reader.GetOrdinal("Record_Type")) ? null : reader.GetString(reader.GetOrdinal("Record_Type")),
                Process_State = reader.IsDBNull(reader.GetOrdinal("Process_State")) ? null : reader.GetString(reader.GetOrdinal("Process_State")),
            };

            recordMetadata.Add(model);
        }



        return recordMetadata;
    }

    public override bool Insert(RecordMetadata newRecord, SqliteConnection connection)
    {
        string sql = @"
            INSERT INTO RecordMetadata(Record_Date, Upload_Date, Record_Type, Process_State)
            VALUES(@Record_Date, @Upload_Date, @Record_Type, @Process_State)
        ";

        connection.Open();

        using var command = new SqliteCommand(sql, connection);

        command.Parameters.AddWithValue("@Record_Date", newRecord.Record_Date);
        command.Parameters.AddWithValue("@Upload_Date", newRecord.Upload_Date);
        command.Parameters.AddWithValue("@Record_Type", newRecord.Record_Type ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Process_State", newRecord.Process_State ?? (object)DBNull.Value);

        try
        {
            command.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB ERROR] Failed to insert RecordMetadata: {ex.Message}");
            return false;
        }
        finally
        {
            connection.Close();
        }
    }

    public override void InsertMany(List<RecordMetadata> newRecords)
    {
        using var connection = sqliteService.CreateConnection();

        foreach (var record in newRecords)
        {
            if (!Insert(record, connection))
            {
                string err = $"Failed to insert record metadata of {record.Record_Date:yyyy.MM.dd}.";
                Console.WriteLine($"[DB ERROR] {err}");
                throw new InvalidOperationException(err);
            }
        }
    }

    
    public async Task UpdateRecordStateAsync(string recordType, DateTime startDate, DateTime endDate)
    {
        string sql = @"
            UPDATE RecordMetadata 
            SET Process_State = 'ANALYZED' 
            WHERE Record_Type = @Record_Type 
            AND Process_State = 'RAW'
            AND date(Record_Date) BETWEEN date(@StartDate) AND date(@EndDate)";

        using var connection = sqliteService.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Record_Type", recordType);
        // SQLite date formatting
        command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB ERROR] Failed to update RecordMetadata state: {ex.Message}");
        }
    }
}