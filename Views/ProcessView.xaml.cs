using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using NexGenSales.Core;
using NexGenSales.Services;
using NexGenSales.Services.Data.Repository;
using NexGenSales.ViewModels;

namespace NexGenSales.Views
{
    public partial class ProcessView : Window
    {
        private readonly RecordMetadataRepository _metadataRepo;

        public ProcessView()
        {
            InitializeComponent();

            // Initialize the repository for the table
            var sqliteService = new SqliteService();
            _metadataRepo = new RecordMetadataRepository(sqliteService);

            // Add the Loaded event handler
            this.Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTableDataAsync();
        }

        private async Task LoadTableDataAsync()
        {
            try
            {
                var logs = await _metadataRepo.GetAll();
                DgRecordLogs.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load record logs:\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Enable dragging the window by holding the left mouse button
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Minimize Button Logic
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Maximize and Restore Button Logic
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        // Navigate to the Home window
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            HomeView homeWindow = new HomeView();
            homeWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            homeWindow.Left = this.Left;
            homeWindow.Top = this.Top;

            homeWindow.WindowState = this.WindowState;
            homeWindow.Show();
            this.Close();
        }

        // Navigate to the Export window
        private void BtnExports_Click(object sender, RoutedEventArgs e)
        {
            ExportView exportWindow = new ExportView();
            exportWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            exportWindow.Left = this.Left;
            exportWindow.Top = this.Top;
            exportWindow.WindowState = this.WindowState;
            exportWindow.Show();
            this.Close();
        }

        // Close Button Logic
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Opens the Analytics Dashboard as a modal dialog.
        /// Handles both Sales and Expenses based on the selected report type.
        /// </summary>
        private async void BtnRunAnalysis_Click(object sender, RoutedEventArgs e)
        {
            string reportType = (CmbReportType.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Sales Data";
            string dateRangeStr = (CmbDateRange.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "1 Month";

            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate;

            switch (dateRangeStr)
            {
                case "1 Week": startDate = startDate.AddDays(-7); break;
                case "1 Month": startDate = startDate.AddMonths(-1); break;
                case "3 Months": startDate = startDate.AddMonths(-3); break;
                case "6 Months": startDate = startDate.AddMonths(-6); break;
                case "1 Year": startDate = startDate.AddYears(-1); break;
                default: startDate = startDate.AddMonths(-1); break;
            }

            // Map UI ComboBox text to your exact DB Record_Type strings
            string dbRecordType = reportType == "Expense Data" ? "Expenses Record" : "Sales Record";

            if (reportType == "Expense Data")
            {
                try
                {
                    // 1. Retrieve expense data from the database for the selected date range
                    var sqliteService = new SqliteService();
                    var expenseRepo = new NexGenSales.Services.Data.Repository.ExpensesRecordRepository(sqliteService);
                    var expensesData = await expenseRepo.GetExpensesByDateRangeAsync(startDate, endDate);

                    if (expensesData == null || expensesData.Count == 0)
                    {
                        MessageBox.Show("No Expenses data found in the database for the selected date range. Please select a different Date Range.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // 2. Analyze the retrieved expense data
                    var service = new ExpensesAnalysisService();
                    var analysisResult = service.Analyze(expensesData);

                    // 3. Create the dashboard ViewModel for expenses and open the dashboard window
                    var vm = new AnalyticsDashboardViewModel(analysisResult, service, "Expenses");
                    var dashboard = new AnalyticsDashboardView
                    {
                        DataContext = vm,
                        Owner = this
                    };

                    dashboard.ShowDialog();

                    // 4. Update states in database to ANALYZED and refresh grid
                    await _metadataRepo.UpdateRecordStateAsync(dbRecordType, startDate, endDate);
                    await LoadTableDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while running the Expenses Analysis:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    // 1. Execute the existing sales analysis logic
                    var dataRepo = new DataRepository(startDate);
                    var service = new SalesAnalysisService(dataRepo);

                    // 2. Create the dashboard ViewModel for sales and open the dashboard window
                    var vm = new AnalyticsDashboardViewModel(service, "Sales");
                    var dashboard = new AnalyticsDashboardView
                    {
                        DataContext = vm,
                        Owner = this
                    };

                    dashboard.ShowDialog();

                    // 3. Update states in database to ANALYZED and refresh grid
                    await _metadataRepo.UpdateRecordStateAsync(dbRecordType, startDate, endDate);
                    await LoadTableDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while running the Sales Analysis:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}