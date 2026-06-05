using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Models.Enums;
using NexGenSales.Services;
using NexGenSales.Services.Data.Mapper;
using NexGenSales.Services.Data.Repository;
using NexGenSales.UserComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace nexgensales.ViewModels
{
    // =========================================================================================
    // DATA MODEL FOR THE IMPORT ARRAY
    // =========================================================================================
    /// <summary>
    /// Represents a data structure to hold both the file path and its corresponding record type.
    /// This is the custom object requested to store files in an array cleanly.
    /// </summary>
    public class ImportedFileSummary
    {
        public string FilePath { get; set; }
        public string RecordType { get; set; }
    }

    public class HomeViewModel : INotifyPropertyChanged
    {
        public ICommand ImportRecordCommand { get; }

        // =========================================================================================
        // PROPERTIES
        // =========================================================================================

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

        // =========================================================================================
        // COMMAND EXECUTION LOGIC
        // =========================================================================================

        private void ExecuteImportRecord(object parameter)
        {
            var fileOpener = new CustomFileOpener(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            // Only set owner if main window exists and is not the file opener itself
            if (Application.Current.MainWindow != null && Application.Current.MainWindow != fileOpener)
            {
                fileOpener.Owner = Application.Current.MainWindow;
            }

            if (fileOpener.ShowDialog() == true && !string.IsNullOrEmpty(fileOpener.SelectedFilePath))
            {
                // Create a temporary list to hold the structured file data
                var tempFilesList = new List<ImportedFileSummary>();

                // Add the selected file with the currently selected Record Type
                tempFilesList.Add(new ImportedFileSummary
                {
                    FilePath = fileOpener.SelectedFilePath,
                    RecordType = SelectedRecordType
                });

                // Assign the structured list to the main array property
                ImportedFilesArray = tempFilesList;

                string fileNames = string.Join("\n", ImportedFilesArray.Select(f => System.IO.Path.GetFileName(f.FilePath)));

                CustomMessageBox.Show(
                    Application.Current.MainWindow,
                    $"Successfully structured and queued {ImportedFilesArray.Count} file(s) as [{SelectedRecordType}]:\n\n{fileNames}",
                    "Import Configuration"
                );

                // Pass the structured array to the processing method
                ProcessAndImportRecords(ImportedFilesArray);
            }
        }

        // =========================================================================================
        // RECORD PROCESSING LOGIC
        // =========================================================================================

        private void ProcessAndImportRecords(List<ImportedFileSummary> filesToProcess)
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
                    CustomMessageBox.Show(Application.Current.MainWindow, "Sales records imported and saved successfully!", "Import Success");
                }
                else
                {
                    CustomMessageBox.Show(Application.Current.MainWindow, "Validation failed for one or more Sales Record files.", "Validation Error");
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
                    CustomMessageBox.Show(Application.Current.MainWindow, "Expenses records imported and saved successfully!", "Import Success");
                }
                else
                {
                    CustomMessageBox.Show(Application.Current.MainWindow, "Validation failed for one or more Expenses Record files.", "Validation Error");
                }
            }
        }

        // =========================================================================================
        // INOTIFYPROPERTYCHANGED IMPLEMENTATION
        // =========================================================================================

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}