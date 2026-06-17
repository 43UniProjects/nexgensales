using System;
using System.Linq;
using System.Threading.Tasks;
using NexGenSales.Services.Data;
using NexGenSales.Models.Enums;
using NexGenSales.Services.Data.Repository;
using NexGenSales.Core;

namespace NexGenSales.Services
{
    public class MetadataService(RecordMetadataRepository metadataRepo, SqliteService sqliteService)
    {
        private readonly RecordMetadataRepository _metadataRepo = metadataRepo;
        private readonly SqliteService _sqliteService = sqliteService;

        public async Task MarkRecordsAsAnalyzedAsync(string recordType, DateTime startDate, DateTime endDate)
        {
            var allRecords = await _metadataRepo.GetAll();

            var recordsToUpdate = allRecords.Where(record =>
                record.Record_Type == recordType &&
                record.Process_State == "RAW" &&
                record.Record_Date.Date >= startDate.Date &&
                record.Record_Date.Date <= endDate.Date
            ).ToList();

            if (recordsToUpdate.Count == 0) return;

            // 1. Initialize the Queue using your SqliteService
            var uowQueue = new SqlTransactionQueue(_sqliteService);

            // 2. Ask the repository to build the commands and load them into the queue
            foreach (var record in recordsToUpdate)
            {
                await _metadataRepo.Update(
                    uowQueue,
                    record.Record_ID,
                    RecordMetadataField.ProcessState,
                    "ANALYZED"
                );
            }

            // 3. Fire all the queued updates in a single, lightning-fast transaction!
            await uowQueue.CommitAll();

            Console.WriteLine($"Successfully bulk-updated {recordsToUpdate.Count} records in a single transaction.");
        }
    }
}