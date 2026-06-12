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
    public class ExportViewModel : INotifyPropertyChanged
    {
        // Commands for executing UI actions
        public ICommand BackupDatabaseCommand { get; }
        public ICommand ExportReportCommand { get; }

        // Collection to hold the names of generated reports for the ComboBox
        private ObservableCollection<string> _availableReports;
        public ObservableCollection<string> AvailableReports
        {
            get { return _availableReports; }
            set { _availableReports = value; OnPropertyChanged(); }
        }

        // Bound to the user's current selection in the Report ComboBox
        private string _selectedReport;
        public string SelectedReport
        {
            get { return _selectedReport; }
            set { _selectedReport = value; OnPropertyChanged(); }
        }

        public ExportViewModel()
        {
            AvailableReports = new ObservableCollection<string>();

            // Initialize commands
            BackupDatabaseCommand = new RelayCommand(ExecuteBackupDatabase);
            ExportReportCommand = new RelayCommand(ExecuteExportReport);

            // Load existing reports from the system directory
            LoadAvailableReports();
        }

        /// <summary>
        /// Scans the local 'Reports' directory for generated PDF files and populates the UI dropdown.
        /// </summary>
        private void LoadAvailableReports()
        {
            AvailableReports.Clear();
            string reportsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");

            if (Directory.Exists(reportsDirectory))
            {
                // Fetch only PDF files and extract just their file names
                var pdfFiles = Directory.GetFiles(reportsDirectory, "*.pdf")
                                        .Select(Path.GetFileName)
                                        .OrderByDescending(name => name) // Show newest first
                                        .ToList();

                if (pdfFiles.Any())
                {
                    foreach (var file in pdfFiles)
                    {
                        AvailableReports.Add(file);
                    }
                    SelectedReport = AvailableReports.FirstOrDefault();
                }
                else
                {
                    AvailableReports.Add("-- No Reports Found --");
                    SelectedReport = AvailableReports.FirstOrDefault();
                }
            }
            else
            {
                // Handle case where the Reports directory hasn't been created yet
                AvailableReports.Add("-- No Reports Found --");
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
                string sourceDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "app.db");

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
            // Validate selection
            if (string.IsNullOrEmpty(SelectedReport) || SelectedReport == "-- No Reports Found --")
            {
                CustomMessageBoxView.Show("Please select a valid report to export.", "Invalid Selection", CustomMessageType.Warning);
                return;
            }

            try
            {
                string sourceReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", SelectedReport);

                if (!File.Exists(sourceReportPath))
                {
                    CustomMessageBoxView.Show("The selected report file could not be found on the disk. It may have been deleted.", "File Not Found", CustomMessageType.Error);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Analytics Report",
                    FileName = SelectedReport, // Default to the original file name
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