using System.Windows;
using System.Windows.Input;
using NextGenSales.Core;
using NextGenSales.Services;
using NextGenSales.ViewModels;

namespace NextGenSales.Views
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
            homeWindow.Show();
            this.Close();
        }

        private void BtnExports_Click(object sender, RoutedEventArgs e)
        {
            ExportView exportWindow = new ExportView();
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
            var service   = new SalesAnalysisService(new MockDataRepository());
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