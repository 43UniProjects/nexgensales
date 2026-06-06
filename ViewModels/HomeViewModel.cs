using Microsoft.Win32;
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Models.Enums;
using NexGenSales.Services;
using NexGenSales.Services.Data.Mapper;
using NexGenSales.Services.Data.Repository;
using NexGenSales.Views;
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

        
        // PROPERTIES
        
        // The specific Array/List requested to hold both File Paths and Record Types
        private List<ImportedFileSummary> _importedFilesArray = new List<ImportedFileSummary>();
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

                string fileNames = string.Join("\n", ImportedFilesArray.Select(f => System.IO.Path.GetFileName(f.FilePath)));

                CustomMessageBoxView.Show(
                    $"Successfully structured {ImportedFilesArray.Count} file(s) as [{SelectedRecordType}] for processing:\n\n{fileNames}",
                    "Import Configuration",
                    CustomMessageType.Info
                );

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
                        new SalesRecordRepository(new SqliteService()).InsertMany(salesImportService.Records);
                        CustomMessageBoxView.Show("Sales records imported and saved successfully!", "Import Success", CustomMessageType.Success);
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
                        new ExpensesRecordRepository(new SqliteService()).InsertMany(expensesImportService.Records);
                        CustomMessageBoxView.Show("Expenses records imported and saved successfully!", "Import Success", CustomMessageType.Success);
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
    }
}