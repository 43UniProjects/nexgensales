using Microsoft.Win32; // Required for OpenFileDialog
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Models.Enums;
using NexGenSales.Services;
using NexGenSales.Services.Data.Mapper;
using NexGenSales.Services.Data.Repository;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace nexgensales.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ICommand ImportRecordCommand { get; }

        public HomeViewModel()
        {
            ImportRecordCommand = new RelayCommand(ExecuteImportRecord);
        }

        // Executes the file selection dialog and handles the selected file
        private async void ExecuteImportRecord(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Select Record File to Import"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                MessageBox.Show(
                    $"File selected successfully:\n{selectedFilePath}",
                    "Import Configuration",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                IEnumerable<string> filePaths = [selectedFilePath];

                ImportRecord(filePaths);

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void ImportRecord(IEnumerable<string> filePaths)
        {

            var excelFileImportService = new ExcelFileImportService<SalesRecordField, SalesRecord>(
                new ExcelParser(),
                RecordMappers.MapToSalesRecord
            );

            bool importSuccess = excelFileImportService.ImportFiles(filePaths);
            if (importSuccess)
            {
                try
                {
                    new SalesRecordRepository(new SqliteService()).InsertMany(excelFileImportService.Records);
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show($"DB Faliure! Storing data from {filePaths.First()} to DB was unsuccessfull");
                }
            }
            else
            {
                MessageBox.Show($"Import Faliure! Importing from {filePaths.First()} was unsuccessfull - Invalid Fields Found");
            }
        }
    }
}