using System;

namespace NexGenSales.Models.Enums
{
    public enum RecordMetadataField
    {
        RecordDate,
        UploadDate,
        RecordType,
        ProcessState
    }

    public static class RecordMetadataFieldExtensions
    {
        public static string ToColumnName(this RecordMetadataField field)
        {
            return field switch
            {
                RecordMetadataField.RecordDate => "Record_Date",
                RecordMetadataField.UploadDate => "Upload_Date",
                RecordMetadataField.RecordType => "Record_Type",
                RecordMetadataField.ProcessState => "Process_State",
                _ => throw new ArgumentOutOfRangeException(nameof(field), $"No column mapping exists for {field}")
            };
        }
    }
}