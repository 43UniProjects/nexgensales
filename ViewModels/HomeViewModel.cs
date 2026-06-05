using Microsoft.Win32; // Required for OpenFileDialog
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace nexgensales.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        // Command to trigger the file import process
        public ICommand ImportRecordCommand { get; }

        public HomeViewModel()
        {
            // Initialize the command
            ImportRecordCommand = new RelayCommand(ExecuteImportRecord);
        }

        // Executes the file selection dialog and handles the selected file
        private void ExecuteImportRecord(object parameter)
        {
            // Initialize the OpenFileDialog component
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set properties to filter specific file types (e.g., CSV or Excel)
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx | CSV Files (*.csv)|*.csv |All files (*.*)|*.*";
            openFileDialog.Title = "Select Record File to Import";

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Retrieve the absolute file path of the selected file
                string selectedFilePath = openFileDialog.FileName;

                // Temporary confirmation message (To be replaced with actual file parsing logic)
                MessageBox.Show(
                    $"File selected successfully:\n{selectedFilePath}",
                    "Import Configuration",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // TODO: Implement parsing logic to read data from 'selectedFilePath'
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}