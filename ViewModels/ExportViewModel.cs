using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using NexGenSales.Core;
using NexGenSales.UserComponents;

namespace NexGenSales.ViewModels
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        public ICommand BackupDatabaseCommand { get; }

        public ExportViewModel()
        {
            // Bind the command to the backup execution logic
            BackupDatabaseCommand = new RelayCommand(ExecuteBackupDatabase);
        }

        /// <summary>
        /// Prompts the user to select a save location and creates a backup copy of the SQLite database.
        /// </summary>
        private void ExecuteBackupDatabase(object parameter)
        {
            try
            {
                // Define the relative path to the source database file
                string sourceDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "app.db");

                // Prevent execution if the database file is missing
                if (!File.Exists(sourceDbPath))
                {
                    CustomMessageBoxView.Show("The source database file (app.db) could not be found.", "File Not Found", CustomMessageType.Error);
                    return;
                }

                // Initialize the SaveFileDialog for the backup file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Save Database Backup",
                    FileName = $"NexGenSales_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    Filter = "SQLite Database (*.db)|*.db|All Files (*.*)|*.*"
                };

                // Execute file copy if user confirms the save dialog
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(sourceDbPath, saveFileDialog.FileName, overwrite: true);
                    CustomMessageBoxView.Show("Database backup created successfully!", "Backup Success", CustomMessageType.Success);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxView.Show($"An error occurred while backing up the database:\n{ex.Message}", "Backup Failed", CustomMessageType.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}