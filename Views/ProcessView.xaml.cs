using System.Windows;
using System.Windows.Input;
using NexGenSales.Core;
using NexGenSales.Services;
using NexGenSales.ViewModels;

namespace NexGenSales.Views
{
    public partial class ProcessView : Window
    {
        public ProcessView()
        {
            InitializeComponent();
        }

        // Window Dragging Logic
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
        /// Report type is always "Sales" — a separate Expenses dashboard will handle expenses.
        /// The ProcessView is blocked (ShowDialog) until the dashboard is closed.
        /// </summary>
        private void BtnRunAnalysis_Click(object sender, RoutedEventArgs e)
        {
            string reportType = (CmbReportType.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Sales Data";
            string dateRangeStr = (CmbDateRange.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "1 Month";

            DateTime startDate = DateTime.Today;
            switch (dateRangeStr)
            {
                case "1 Week": startDate = startDate.AddDays(-7); break;
                case "1 Month": startDate = startDate.AddMonths(-1); break;
                case "3 Months": startDate = startDate.AddMonths(-3); break;
                case "6 Months": startDate = startDate.AddMonths(-6); break;
                case "1 Year": startDate = startDate.AddYears(-1); break;
                default: startDate = startDate.AddMonths(-1); break;
            }

            if (reportType == "Expense Data")
            {
                var service = new ExpensesAnalysisService();
                service.RunAnalysis(startDate);
            }
            else
            {
                var dataRepo = new DataRepository(startDate);
                var service   = new SalesAnalysisService(dataRepo);
                var vm        = new AnalyticsDashboardViewModel(service, "Sales");
                var dashboard = new AnalyticsDashboardView
                {
                    DataContext = vm,
                    Owner       = this
                };

                // ShowDialog() gives the dashboard exclusive focus and blocks
                // all interaction with this window until the dashboard is closed.
                dashboard.ShowDialog();
            }
        }
    }
}