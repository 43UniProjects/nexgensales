using Microsoft.Win32;
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Models.Enums;
using NexGenSales.Services;
using NexGenSales.Services.Data.Mapper;
using NexGenSales.Services.Data.Repository;
using NexGenSales.Views;
using NexGenSales.UserComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NexGenSales.ViewModels
{

    // DATA MODEL FOR THE IMPORT ARRAY

    public class ImportedFileSummary
    {
        public string FilePath { get; set; }
        public string RecordType { get; set; }
    }

    public class HomeViewModel : INotifyPropertyChanged
    {
        public ICommand ImportRecordCommand { get; }

        public event Action OnImportSuccess;

        // PROPERTIES

        // The specific Array/List requested to hold both File Paths and Record Types
        private List<ImportedFileSummary> _importedFilesArray = [];
        public List<ImportedFileSummary> ImportedFilesArray
        {
            get { return _importedFilesArray; }
            private set
            {
                _importedFilesArray = value;
                OnPropertyChanged();
            }
        }

        // Bound to the UI ComboBox to track selected record type dynamically
        private string _selectedRecordType = "Sales Record";
        public string SelectedRecordType
        {
            get { return _selectedRecordType; }
            set
            {
                _selectedRecordType = value;
                OnPropertyChanged();
            }
        }

        public HomeViewModel()
        {
            ImportRecordCommand = new RelayCommand(ExecuteImportRecord);
        }


        private void ExecuteImportRecord(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                Title = "Select Record Files to Import",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Create a temporary list to hold the structured file data
                var tempFilesList = new List<ImportedFileSummary>();

                // Map each selected file path with the currently selected Record Type
                foreach (string path in openFileDialog.FileNames)
                {
                    tempFilesList.Add(new ImportedFileSummary
                    {
                        FilePath = path,
                        RecordType = SelectedRecordType
                    });
                }

                // Assign the structured list to the main array property
                ImportedFilesArray = tempFilesList;

                string fileNames = string.Join("\n", ImportedFilesArray.Select(file => System.IO.Path.GetFileName(file.FilePath)));

                Console.WriteLine(
                    $"Successfully structured {ImportedFilesArray.Count} file(s) as [{SelectedRecordType}] for processing\n File Names: {fileNames}");

                // Pass the structured array to the processing method
                ProcessAndImportRecords(ImportedFilesArray);
            }
        }


        // RECORD PROCESSING LOGIC


        private void ProcessAndImportRecords(List<ImportedFileSummary> filesToProcess)
        {
            try
            {
                // Extract pure arrays of paths filtered by their specific Record Types
                string[] salesPaths = filesToProcess.Where(f => f.RecordType == "Sales Record").Select(f => f.FilePath).ToArray();
                string[] expensesPaths = filesToProcess.Where(f => f.RecordType == "Expenses Record").Select(f => f.FilePath).ToArray();

                // 1. Process Sales Records
                if (salesPaths.Length > 0)
                {
                    var salesImportService = new ExcelFileImportService<SalesRecordField, SalesRecord>(
                        new ExcelParser(), RecordMappers.MapToSalesRecord);

                    if (salesImportService.ImportFiles(salesPaths))
                    {
                        new RecordMetadataRepository(new SqliteService()).InsertMany(ExtractRecordMetadata(null, salesImportService.Records));

                        new SalesRecordRepository(new SqliteService()).InsertMany(salesImportService.Records);
                        CustomMessageBoxView.Show("Sales records imported and saved successfully!", "Import Success", CustomMessageType.Success);
                        OnImportSuccess?.Invoke();
                    }
                    else
                    {
                        CustomMessageBoxView.Show("Validation failed for one or more Sales Record files.", "Validation Error", CustomMessageType.Error);
                    }
                }

                // 2. Process Expenses Records
                if (expensesPaths.Length > 0)
                {

                    var expensesImportService = new ExcelFileImportService<ExpensesRecordField, ExpensesRecord>(
                        new ExcelParser(), RecordMappers.MapToExpensesRecord);

                    if (expensesImportService.ImportFiles(expensesPaths))
                    {

                        new RecordMetadataRepository(new SqliteService()).InsertMany(ExtractRecordMetadata(expensesImportService.Records, null));

                        new ExpensesRecordRepository(new SqliteService()).InsertMany(expensesImportService.Records);
                        CustomMessageBoxView.Show("Expenses records imported and saved successfully!", "Import Success", CustomMessageType.Success);
                        OnImportSuccess?.Invoke();
                    }
                    else
                    {
                        CustomMessageBoxView.Show("Validation failed for one or more Expenses Record files.", "Validation Error", CustomMessageType.Error);
                    }
                }

            }
            catch (Exception ex)
            {
                CustomMessageBoxView.Show($"An unexpected error occurred:\n{ex.Message}", "Critical Error", CustomMessageType.Error);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static List<RecordMetadata> ExtractRecordMetadata(List<ExpensesRecord> expensesRecords, List<SalesRecord> salesRecords)
        {
            List<RecordMetadata> recordMetadataList = [];

            if (expensesRecords != null && expensesRecords.Count > 0)
            {
                DateTime? lastRecordDate = null;

                foreach (var record in expensesRecords)
                {
                    Console.WriteLine("[HomeViewModel] Extracting expenses record metadata");
                    if (lastRecordDate?.Date != record.Date_Recorded.Date)
                    {
                        RecordMetadata newRecordMetadata = new()
                        {
                            Record_Type = "Expenses Record",
                            Record_Date = record.Date_Recorded,
                            Upload_Date = DateTime.Now,
                            Process_State = "RAW" // RAW = fresh uploaded data record, ANALYSED = analysed data
                        };

                        recordMetadataList.Add(newRecordMetadata);
                        lastRecordDate = record.Date_Recorded;
                    }
                }
            }

            if (salesRecords != null && salesRecords.Count > 0)
            {
                DateTime? lastRecordDate = null;

                foreach (var record in salesRecords)
                {
                    Console.WriteLine("[HomeViewModel] Extracting sales record metadata");
                    if (lastRecordDate?.Date != record.Date_Time.Date)
                    {
                        RecordMetadata newRecordMetadata = new()
                        {
                            Record_Type = "Sales Record",
                            Record_Date = record.Date_Time,
                            Upload_Date = DateTime.Now,
                            Process_State = "RAW" // RAW = fresh uploaded data record, ANALYSED = analysed data
                        };

                        recordMetadataList.Add(newRecordMetadata);

                        lastRecordDate = record.Date_Time;
                    }
                }
            }

            return recordMetadataList;
        }

    }
}
