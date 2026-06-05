using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDataReader;

namespace NexGenSales.Core
{
    public class ExcelParser : IDisposable
    {
        private bool _disposed;

        public ExcelParser()
        {
            Console.WriteLine("[ExcelParser] Initializing...");
            // Required for reading older Excel formats natively
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Reads an Excel file and dynamically maps columns to the provided Enum type.
        /// </summary>
        /// <typeparam name="T">The Enum representing the expected fields (e.g., SalesField)</typeparam>
        /// <param name="filePath">Path to the .xlsx or .csv file</param>
        /// <returns>A strongly-typed list of rows mapped as Dictionary<TEnum, Value></returns>
        public List<Dictionary<T, object>> ParseFile<T>(string filePath) where T : struct, Enum
        {
            ThrowIfDisposed();

            var parsedData = new List<Dictionary<T, object>>();
            var columnMap = new Dictionary<int, T>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                bool isHeaderRow = true;


                Console.WriteLine($"[ExcelParser] Parsing the file @({filePath})...");

                while (reader.Read())
                {
                    if (isHeaderRow)
                    {
                        for (int col = 0; col < reader.FieldCount; col++)
                        {
                            string rawHeader = reader.GetValue(col)?.ToString();
                            if (string.IsNullOrWhiteSpace(rawHeader)) continue;

                            // Clean the Excel header to match standard Enum naming
                            // Example: "unit_purchase_cost" -> "unitpurchasecost"
                            string cleanHeader = rawHeader.Replace("_", "").Replace(" ", "");

                            if (Enum.TryParse<T>(cleanHeader, true, out T matchedEnum))
                            {
                                columnMap[col] = matchedEnum;
                            }
                        }
                        isHeaderRow = false;
                        continue;
                    }

                    var rowData = new Dictionary<T, object>();

                    foreach (var map in columnMap)
                    {
                        int colIndex = map.Key;
                        T systemField = map.Value;

                        object cellValue = reader.GetValue(colIndex);
                        rowData[systemField] = (cellValue == DBNull.Value) ? null : cellValue;
                    }

                    if (rowData.Values.Any(v => v != null && !string.IsNullOrWhiteSpace(v.ToString())))
                    {
                        parsedData.Add(rowData);
                    }
                }
            }

            Console.WriteLine($"[ExcelParser] Successfully Parsed the file @({filePath})...");

            return parsedData;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ExcelParser));
        }
    }
}