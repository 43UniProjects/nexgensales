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
using System.Threading.Tasks;
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

        // ==========================================
        // PROPERTIES
        // ==========================================

        private List<ImportedFileSummary> _importedFilesArray = [];
        public List<ImportedFileSummary> ImportedFilesArray
        {
            get { return _importedFilesArray; }
            private set { _importedFilesArray = value; OnPropertyChanged(); }
        }

        private string _selectedRecordType = "Sales Record";
        public string SelectedRecordType
        {
            get { return _selectedRecordType; }
            set { _selectedRecordType = value; OnPropertyChanged(); }
        }

        // --- NEW ASYNC PROGRESS TRACKING PROPERTIES ---

        private bool _isImporting = false;
        /// <summary>
        /// Indicates whether an import operation is currently in progress.
        /// Useful for disabling buttons or showing/hiding progress overlays in the UI.
        /// </summary>
        public bool IsImporting
        {
            get { return _isImporting; }
            set { _isImporting = value; OnPropertyChanged(); }
        }

        private int _importProgress = 0;
        /// <summary>
        /// Tracks the percentage of completion (0 to 100) for the progress bar.
        /// </summary>
        public int ImportProgress
        {
            get { return _importProgress; }
            set { _importProgress = value; OnPropertyChanged(); }
        }

        private string _importStatusMessage = "";
        /// <summary>
        /// Provides real-time feedback to the user regarding the current background operation.
        /// </summary>
        public string ImportStatusMessage
        {
            get { return _importStatusMessage; }
            set { _importStatusMessage = value; OnPropertyChanged(); }
        }

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public HomeViewModel()
        {
            ImportRecordCommand = new RelayCommand(ExecuteImportRecord);
        }

        // ==========================================
        // COMMAND EXECUTION LOGIC
        // ==========================================

        /// <summary>
        /// Asynchronously executes the import record workflow to keep the main UI thread responsive.
        /// </summary>
        private async void ExecuteImportRecord(object parameter)
        {
            if (SelectedRecordType == "Restore Database")
            {
                ExecuteRestoreDatabase();
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                Title = "Select Record Files to Import",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var tempFilesList = new List<ImportedFileSummary>();
                foreach (string path in openFileDialog.FileNames)
                {
                    tempFilesList.Add(new ImportedFileSummary
                    {
                        FilePath = path,
                        RecordType = SelectedRecordType
                    });
                }

                ImportedFilesArray = tempFilesList;

                // 1. Prepare UI for the asynchronous background task
                IsImporting = true;
                ImportProgress = 0;
                ImportStatusMessage = $"Preparing to import {ImportedFilesArray.Count} file(s)...";

                // 2. Configure a Progress reporter to securely update the UI thread from the background
                var progressReporter = new Progress<int>(percent =>
                {
                    ImportProgress = percent;
                    ImportStatusMessage = $"Importing Records... {percent}% Completed";
                });

                // 3. Execute the heavy lifting on a separate background thread
                await Task.Run(() => ProcessAndImportRecords(ImportedFilesArray, progressReporter));

                // 4. Reset UI tracking properties once the operation concludes
                IsImporting = false;
                ImportProgress = 0;
                ImportStatusMessage = string.Empty;
            }
        }


        // ==========================================
        // RECORD PROCESSING LOGIC
        // ==========================================

        /// <summary>
        /// Processes the selected files on a background thread and updates progress.
        /// </summary>
        private void ProcessAndImportRecords(List<ImportedFileSummary> filesToProcess, IProgress<int> progress)
        {
            try
            {
                string[] salesPaths = filesToProcess.Where(f => f.RecordType == "Sales Record").Select(f => f.FilePath).ToArray();
                string[] expensesPaths = filesToProcess.Where(f => f.RecordType == "Expenses Record").Select(f => f.FilePath).ToArray();

                // Process Sales Records
                if (salesPaths.Length > 0)
                {
                    var salesImportService = new ExcelFileImportService<SalesRecordField, SalesRecord>(
                        new ExcelParser(), RecordMappers.MapToSalesRecord);

                    if (salesImportService.ImportFiles(salesPaths, progress))
                    {
                        // Alert the user that the heavy database operation is starting
                        Application.Current.Dispatcher.Invoke(() => {
                            ImportStatusMessage = "Finalizing Database Integration... Please do not close.";
                        });

                        new RecordMetadataRepository(new SqliteService()).InsertMany(ExtractRecordMetadata(null, salesImportService.Records));
                        new SalesRecordRepository(new SqliteService()).InsertMany(salesImportService.Records);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Push progress to 100% only after the database operation finishes
                            ImportProgress = 100;
                            ImportStatusMessage = "Completed!";
                            CustomMessageBoxView.Show("Sales records imported and saved successfully!", "Import Success", CustomMessageType.Success);
                            OnImportSuccess?.Invoke();
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            CustomMessageBoxView.Show("Validation failed for one or more Sales Record files.", "Validation Error", CustomMessageType.Error);
                        });
                    }
                }

                // Process Expenses Records
                if (expensesPaths.Length > 0)
                {
                    var expensesImportService = new ExcelFileImportService<ExpensesRecordField, ExpensesRecord>(
                        new ExcelParser(), RecordMappers.MapToExpensesRecord);

                    if (expensesImportService.ImportFiles(expensesPaths, progress))
                    {
                        //Alert the user that the heavy database operation is starting
                        Application.Current.Dispatcher.Invoke(() => {
                            ImportStatusMessage = "Finalizing Database Integration... Please do not close.";
                        });

                        new RecordMetadataRepository(new SqliteService()).InsertMany(ExtractRecordMetadata(expensesImportService.Records, null));
                        new ExpensesRecordRepository(new SqliteService()).InsertMany(expensesImportService.Records);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            //Push progress to 100% only after the database operation finishes
                            ImportProgress = 100;
                            ImportStatusMessage = "Completed!";
                            CustomMessageBoxView.Show("Expenses records imported and saved successfully!", "Import Success", CustomMessageType.Success);
                            OnImportSuccess?.Invoke();
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            CustomMessageBoxView.Show("Validation failed for one or more Expenses Record files.", "Validation Error", CustomMessageType.Error);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    CustomMessageBoxView.Show($"An unexpected error occurred:\n{ex.Message}", "Critical Error", CustomMessageType.Error);
                });
            }
        }

        // ==========================================
        // METADATA & RESTORE LOGIC
        // ==========================================

        private static List<RecordMetadata> ExtractRecordMetadata(List<ExpensesRecord> expensesRecords, List<SalesRecord> salesRecords)
        {
            List<RecordMetadata> recordMetadataList = [];

            if (expensesRecords != null && expensesRecords.Count > 0)
            {
                DateTime? lastRecordDate = null;
                foreach (var record in expensesRecords)
                {
                    if (lastRecordDate?.Date != record.Date_Recorded.Date)
                    {
                        RecordMetadata newRecordMetadata = new()
                        {
                            Record_Type = "Expenses Record",
                            Record_Date = record.Date_Recorded,
                            Upload_Date = DateTime.Now,
                            Process_State = "RAW"
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
                    if (lastRecordDate?.Date != record.Date_Time.Date)
                    {
                        RecordMetadata newRecordMetadata = new()
                        {
                            Record_Type = "Sales Record",
                            Record_Date = record.Date_Time,
                            Upload_Date = DateTime.Now,
                            Process_State = "RAW"
                        };
                        recordMetadataList.Add(newRecordMetadata);
                        lastRecordDate = record.Date_Time;
                    }
                }
            }
            return recordMetadataList;
        }

        private void ExecuteRestoreDatabase()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "SQLite Database Files (*.db)|*.db",
                Title = "Select Database Backup to Restore",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedBackupPath = openFileDialog.FileName;

                bool confirmRestore = CustomMessageBoxView.Show(
                    "Are you sure you want to restore the database?\nThis action will overwrite the current system data.",
                    "Confirm Restore",
                    CustomMessageType.Warning,
                    CustomMessageButtons.YesNo);

                if (!confirmRestore) return;

                string targetDbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "app.db");

                if (System.IO.File.Exists(targetDbPath))
                {
                    bool backupFirst = CustomMessageBoxView.Show(
                        "An active database currently exists.\n\nDo you want to create a BACKUP of the current database before overwriting it?\n\n(Click 'Yes' to Backup, 'No' to Force Overwrite)",
                        "Backup Current Data?",
                        CustomMessageType.Warning,
                        CustomMessageButtons.YesNo);

                    if (backupFirst)
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            Title = "Save Current Database Backup",
                            FileName = $"NexGenSales_PreRestore_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                            Filter = "SQLite Database (*.db)|*.db"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            System.IO.File.Copy(targetDbPath, saveFileDialog.FileName, overwrite: true);
                            System.IO.File.SetCreationTime(saveFileDialog.FileName, DateTime.Now);
                            System.IO.File.SetLastWriteTime(saveFileDialog.FileName, DateTime.Now);
                        }
                        else
                        {
                            CustomMessageBoxView.Show("Restore operation safely aborted. The pre-restore backup was cancelled.", "Operation Aborted", CustomMessageType.Info);
                            return;
                        }
                    }
                }

                try
                {
                    string dbDirectory = System.IO.Path.GetDirectoryName(targetDbPath);
                    if (!System.IO.Directory.Exists(dbDirectory))
                    {
                        System.IO.Directory.CreateDirectory(dbDirectory);
                    }

                    System.IO.File.Copy(selectedBackupPath, targetDbPath, overwrite: true);

                    CustomMessageBoxView.Show("Database successfully restored and integrated into the system!", "Restore Success", CustomMessageType.Success);
                    OnImportSuccess?.Invoke();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxView.Show($"A critical error occurred during restoration:\n{ex.Message}", "Restore Failed", CustomMessageType.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}