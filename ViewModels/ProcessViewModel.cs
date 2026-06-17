using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Services.Data.Repository;
using NexGenSales.Services;
using NexGenSales.UserComponents;
using NexGenSales.Views;

namespace NexGenSales.ViewModels
{
    public class ProcessViewModel : INotifyPropertyChanged
    {
        private readonly RecordMetadataRepository _metadataRepo;

        // ObservableCollection automatically updates the DataGrid when data changes
        private ObservableCollection<RecordMetadata> _recordLogs;
        public ObservableCollection<RecordMetadata> RecordLogs
        {
            get { return _recordLogs; }
            set { _recordLogs = value; OnPropertyChanged(); }
        }

        // Bound to the Date Range ComboBox's ItemsSource
        private ObservableCollection<string> _availableDateRanges;
        public ObservableCollection<string> AvailableDateRanges
        {
            get { return _availableDateRanges; }
            set { _availableDateRanges = value; OnPropertyChanged(); }
        }

        // Bound to the currently selected string in the Date Range ComboBox
        private string _selectedDateRange;
        public string SelectedDateRange
        {
            get { return _selectedDateRange; }
            set { _selectedDateRange = value; OnPropertyChanged(); }
        }

        // Bound to the Report Type ComboBox. Triggers date range updates upon change.
        private int _selectedReportTypeIndex = 0;
        public int SelectedReportTypeIndex
        {
            get { return _selectedReportTypeIndex; }
            set
            {
                _selectedReportTypeIndex = value;
                OnPropertyChanged();

                // Refresh the available date range options whenever the report type changes
                UpdateAvailableDateRanges();
            }
        }

        public ICommand RunAnalysisCommand { get; }

        public ProcessViewModel()
        {
            _metadataRepo = new RecordMetadataRepository(new SqliteService());
            RecordLogs = new ObservableCollection<RecordMetadata>();
            AvailableDateRanges = new ObservableCollection<string>();

            // Initialize the default date ranges on view load
            UpdateAvailableDateRanges();

            // Initialize command logic for executing analysis
            RunAnalysisCommand = new RelayCommand(async (param) => await ExecuteRunAnalysisAsync());

            // Load initial data into the DataGrid asynchronously
            _ = LoadTableDataAsync();
        }

        /// <summary>
        /// Dynamically populates the Date Range ComboBox options based on the selected Report Type.
        /// Excludes the "1 Week" option when "Expense Data" is selected.
        /// </summary>
        private void UpdateAvailableDateRanges()
        {
            AvailableDateRanges.Clear();

            if (SelectedReportTypeIndex == 1) // 1 = Expense Data
            {
                AvailableDateRanges.Add("1 Month");
                AvailableDateRanges.Add("3 Months");
                AvailableDateRanges.Add("6 Months");
                AvailableDateRanges.Add("1 Year");
            }
            else // 0 = Sales Data
            {
                AvailableDateRanges.Add("1 Week");
                AvailableDateRanges.Add("1 Month");
                AvailableDateRanges.Add("3 Months");
                AvailableDateRanges.Add("6 Months");
                AvailableDateRanges.Add("1 Year");
            }

            // Explicitly set the default selected item to the first available option
            if (AvailableDateRanges.Count > 0)
            {
                SelectedDateRange = AvailableDateRanges[0];
            }
        }

        /// <summary>
        /// Fetches all record metadata from the repository and binds it to the DataGrid.
        /// </summary>
        private async Task LoadTableDataAsync()
        {
            try
            {
                var data = await _metadataRepo.GetAll();
                RecordLogs = new ObservableCollection<RecordMetadata>(data);
            }
            catch (Exception ex)
            {
                CustomMessageBoxView.Show($"Error loading data:\n{ex.Message}", "Database Error", CustomMessageType.Error);
            }
        }

        /// <summary>
        /// Executes the core analysis logic based on user-selected filters and displays the generated dashboard.
        /// </summary>
        private async Task ExecuteRunAnalysisAsync()
        {
            // Validate selection to prevent null reference exceptions
            if (string.IsNullOrEmpty(SelectedDateRange)) return;

            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate;

            // Map the selected string value to an actual DateTime calculation
            switch (SelectedDateRange)
            {
                case "1 Week": startDate = endDate.AddDays(-7); break;
                case "1 Month": startDate = endDate.AddMonths(-1); break;
                case "3 Months": startDate = endDate.AddMonths(-3); break;
                case "6 Months": startDate = endDate.AddMonths(-6); break;
                case "1 Year": startDate = endDate.AddYears(-1); break;
            }

            string reportType = SelectedReportTypeIndex == 0 ? "Sales Data" : "Expense Data";
            string dbRecordType = reportType == "Expense Data" ? "Expenses Record" : "Sales Record";

            if (reportType == "Expense Data")
            {
                try
                {
                    // 1. Retrieve expense data from the database for the selected date range
                    var sqliteService = new SqliteService();
                    var expenseRepo = new ExpensesRecordRepository(sqliteService);
                    var expenseService = new ExpensesService(expenseRepo);
                    var expensesData = await expenseService.GetExpensesByDateRange(startDate, endDate);

                    // Validate data availability before proceeding
                    if (expensesData == null || expensesData.Count == 0)
                    {
                        CustomMessageBoxView.Show("No Expenses data found in the database for the selected date range. Please select a different Date Range.", "No Data", CustomMessageType.Info);
                        return;
                    }

                    // 2. Analyze the retrieved expense data
                    var service = new ExpensesAnalysisService();
                    var analysisResult = service.Analyze(expensesData);

                    // 3. Render the dashboard with the generated analysis payload
                    var vm = new AnalyticsDashboardViewModel(analysisResult, service, "Expenses");
                    var dashboard = new AnalyticsDashboardView { DataContext = vm };
                    dashboard.ShowDialog();

                    // 4. Finalize process: Update database state and refresh UI grid
                    var metadataService = new MetadataService(_metadataRepo, new SqliteService());
                    await metadataService.MarkRecordsAsAnalyzedAsync(dbRecordType, startDate, endDate);
                    await LoadTableDataAsync();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxView.Show($"An error occurred while running the Expenses Analysis:\n{ex.Message}", "Error", CustomMessageType.Error);
                }
            }
            else
            {
                try
                {
                    // 1. Initialize data repository and fetch sales records
                    var dataRepo = new DataRepository(startDate);

                    // Validate data availability before invoking the analysis service
                    if (!dataRepo.HasData)
                    {
                        CustomMessageBoxView.Show("No Sales data found in the database for the selected date range. Please select a different Date Range.", "No Data", CustomMessageType.Info);
                        return;
                    }

                    // 2. Analyze the retrieved sales data
                    var service = new SalesAnalysisService(dataRepo);

                    // 3. Render the dashboard with the generated analysis payload
                    var vm = new AnalyticsDashboardViewModel(service, "Sales");
                    var dashboard = new AnalyticsDashboardView { DataContext = vm };
                    dashboard.ShowDialog();

                    // 4. Finalize process: Update database state and refresh UI grid
                    var metadataService = new MetadataService(_metadataRepo, new SqliteService());
                    await metadataService.MarkRecordsAsAnalyzedAsync(dbRecordType, startDate, endDate);
                    await LoadTableDataAsync();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxView.Show($"An error occurred while running the Sales Analysis:\n{ex.Message}", "Error", CustomMessageType.Error);
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