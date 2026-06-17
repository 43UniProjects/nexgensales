using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using NexGenSales.Core;
using NexGenSales.UserComponents;

namespace NexGenSales.ViewModels
{


    // Updated Model for Reports
    public class ReportItem
    {
        public string FileName { get; set; } // The actual physical file name
        public string DisplayName { get; set; } // The pretty name for the UI
        public bool IsHeader { get; set; }

        public override string ToString()
        {
            // If it's a header, show the header text. Otherwise, show the pretty date/time.
            return IsHeader ? FileName : DisplayName;
        }
    }


    public class ExportViewModel : INotifyPropertyChanged
    {
        // Commands for executing UI actions
        public ICommand BackupDatabaseCommand { get; }
        public ICommand ExportReportCommand { get; }

        // Collection to hold the categorized reports
        private ObservableCollection<ReportItem> _availableReports;
        public ObservableCollection<ReportItem> AvailableReports
        {
            get { return _availableReports; }
            set { _availableReports = value; OnPropertyChanged(); }
        }

        // Bound to the user's current selection
        private ReportItem _selectedReport;
        public ReportItem SelectedReport
        {
            get { return _selectedReport; }
            set { _selectedReport = value; OnPropertyChanged(); }
        }

        public ExportViewModel()
        {
            AvailableReports = new ObservableCollection<ReportItem>();

            // Initialize commands
            BackupDatabaseCommand = new RelayCommand(ExecuteBackupDatabase);
            ExportReportCommand = new RelayCommand(ExecuteExportReport);

            // Load existing reports from the system directory
            LoadAvailableReports();
        }

        private string FormatReportName(string fileName)
        {
            try
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string[] parts = nameWithoutExt.Split('_');


                if (parts.Length >= 4)
                {

                    string datePart = parts[2].Replace("-", " ");


                    string[] timeTokens = parts[3].Split('-');
                    string timePart = timeTokens.Length == 3 ? $"{timeTokens[0]}:{timeTokens[1]} {timeTokens[2]}" : parts[3];

                    return $"{datePart}   at   {timePart}";
                }
            }
            catch
            {

            }

            return fileName;
        }



        /// <summary>
        /// Scans the local 'Reports' directory for generated PDF files and populates the UI dropdown.
        /// </summary>
        private void LoadAvailableReports()
        {
            AvailableReports.Clear();

            string reportsDirectory;

#if DEBUG
            // DEVELOPMENT: Point to the actual project folder (3 levels up from bin/Debug/...)
            reportsDirectory = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")), "Reports");
#else
            // PRODUCTION: Point to the compiled executable's folder
            reportsDirectory = Path.Combine(AppContext.BaseDirectory, "Reports");
#endif
            // Safety check: Ensure the folder actually exists before your app tries to save a CSV!
            Directory.CreateDirectory(reportsDirectory);

            if (Directory.Exists(reportsDirectory))
            {
                var allPdfFiles = Directory.GetFiles(reportsDirectory, "*.pdf")
                                           .Select(Path.GetFileName)
                                           .ToList();

                var salesReports = allPdfFiles.Where(f => f.Contains("_Sales_")).OrderByDescending(f => f).ToList();
                var expensesReports = allPdfFiles.Where(f => f.Contains("_Expenses_")).OrderByDescending(f => f).ToList();

                // Add Sales Header and Items
                if (salesReports.Any())
                {
                    AvailableReports.Add(new ReportItem { FileName = "Sales Reports", IsHeader = true });
                    foreach (var file in salesReports)
                    {
                        AvailableReports.Add(new ReportItem { FileName = file, DisplayName = FormatReportName(file), IsHeader = false });
                    }
                }

                // Add Expenses Header and Items
                if (expensesReports.Any())
                {
                    AvailableReports.Add(new ReportItem { FileName = "Expenses Reports", IsHeader = true });
                    foreach (var file in expensesReports)
                    {
                        AvailableReports.Add(new ReportItem { FileName = file, DisplayName = FormatReportName(file), IsHeader = false });
                    }
                }

                // Select the first actual file (skips headers)
                SelectedReport = AvailableReports.FirstOrDefault(r => !r.IsHeader && r.FileName != "-- No Reports Found --")
                                 ?? AvailableReports.FirstOrDefault();
            }
            else
            {
                AvailableReports.Add(new ReportItem { FileName = "-- No Reports Found --", IsHeader = false });
                SelectedReport = AvailableReports.FirstOrDefault();
            }
        }





        /// <summary>
        /// Prompts the user to select a save location and creates a backup copy of the SQLite database.
        /// </summary>
        private void ExecuteBackupDatabase(object parameter)
        {
            try
            {
                string sourceDbPath;

#if DEBUG
                // DEVELOPMENT: Look in the main project folder (3 levels up from bin/Debug/...)
                sourceDbPath = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")), "Database", "app.db");
#else
                // PRODUCTION: Look right next to the compiled executable
                sourceDbPath = Path.Combine(AppContext.BaseDirectory, "Database", "app.db");
#endif

                if (!File.Exists(sourceDbPath))
                {
                    CustomMessageBoxView.Show("The source database file (app.db) could not be found.", "File Not Found", CustomMessageType.Error);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Save Database Backup",
                    FileName = $"NexGenSales_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    Filter = "SQLite Database (*.db)|*.db|All Files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(sourceDbPath, saveFileDialog.FileName, overwrite: true);

                    // Explicitly update timestamps to reflect the exact moment of backup
                    File.SetCreationTime(saveFileDialog.FileName, DateTime.Now);
                    File.SetLastWriteTime(saveFileDialog.FileName, DateTime.Now);

                    CustomMessageBoxView.Show("Database backup created successfully!", "Backup Success", CustomMessageType.Success);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxView.Show($"An error occurred while backing up the database:\n{ex.Message}", "Backup Failed", CustomMessageType.Error);
            }
        }

        /// <summary>
        /// Prompts the user for a destination path and exports the selected PDF report.
        /// </summary>
        private void ExecuteExportReport(object parameter)
        {
            // Validation: Prevents clicking on the disabled Headers just in case
            if (SelectedReport == null || SelectedReport.IsHeader || SelectedReport.FileName == "-- No Reports Found --")
            {
                CustomMessageBoxView.Show("Please select a valid report file from the list.", "Invalid Selection", CustomMessageType.Warning);
                return;
            }

            try
            {
                string sourceReportPath;

#if DEBUG
                // DEVELOPMENT: Look in the main project folder's 'Reports' directory
                sourceReportPath = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")), "Reports", SelectedReport.FileName);
#else
                // PRODUCTION: Look in the 'Reports' directory right next to the compiled executable
                sourceReportPath = Path.Combine(AppContext.BaseDirectory, "Reports", SelectedReport.FileName);
#endif

                if (!File.Exists(sourceReportPath))
                {
                    CustomMessageBoxView.Show("The selected report file could not be found. It may have been deleted.", "File Not Found", CustomMessageType.Error);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Analytics Report",
                    FileName = SelectedReport.FileName,
                    Filter = "PDF Document (*.pdf)|*.pdf|All Files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(sourceReportPath, saveFileDialog.FileName, overwrite: true);
                    CustomMessageBoxView.Show("Report exported successfully!", "Export Success", CustomMessageType.Success);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxView.Show($"An error occurred while exporting the report:\n{ex.Message}", "Export Failed", CustomMessageType.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}