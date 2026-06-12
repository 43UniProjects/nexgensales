using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NexGenSales.ViewModels;
using NexGenSales.UserComponents;


namespace NexGenSales.Views
{
    public partial class AnalyticsDashboardView : Window
    {
        public AnalyticsDashboardView()
        {
            InitializeComponent();
        }

        // ── Title-bar drag ────────────────────────────────────────────────────
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // ── Title-bar controls ────────────────────────────────────────────────
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Normal
                ? WindowState.Maximized
                : WindowState.Normal;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ── PDF generation ────────────────────────────────────────────────────

        /// <summary>
        /// Adjusts the visual theme of the active charts for PDF generation.
        /// Temporarily switches foreground colors to black for printing, then reverts them.
        /// </summary>
        private void SetTheme(bool isPrint)
        {
            var brush = isPrint ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            var axisBrush = isPrint ? brush : new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#788896"));

            Action<LiveCharts.Wpf.CartesianChart> updateAxes = (chart) =>
            {
                foreach (var axis in chart.AxisX)
                {
                    axis.Foreground = axisBrush;
                    bool labels = axis.ShowLabels;
                    axis.ShowLabels = !labels;
                    axis.ShowLabels = labels;
                    string title = axis.Title;
                    axis.Title = null;
                    axis.Title = title;
                }
                foreach (var axis in chart.AxisY)
                {
                    axis.Foreground = axisBrush;
                    bool labels = axis.ShowLabels;
                    axis.ShowLabels = !labels;
                    axis.ShowLabels = labels;
                    string title = axis.Title;
                    axis.Title = null;
                    axis.Title = title;
                }
            };

            // Determine which dashboard is currently visible to the user
            bool isSalesActive = SupplierChart.IsVisible;

            if (isSalesActive)
            {
                // Apply print theme to Sales charts
                SupplierChart.Foreground = brush;
                VelocityChart.Foreground = brush;
                RevenueChart.Foreground = brush;
                TrendChart.Foreground = brush;
                DiscountChart.Foreground = brush;

                updateAxes(SupplierChart);
                updateAxes(VelocityChart);
                updateAxes(TrendChart);
                updateAxes(DiscountChart);

                SupplierChart.DisableAnimations = isPrint;
                VelocityChart.DisableAnimations = isPrint;
                RevenueChart.DisableAnimations = isPrint;
                TrendChart.DisableAnimations = isPrint;
                DiscountChart.DisableAnimations = isPrint;

                SupplierChart.Update(true, true);
                VelocityChart.Update(true, true);
                RevenueChart.Update(true, true);
                TrendChart.Update(true, true);
                DiscountChart.Update(true, true);
            }
            else
            {
                // Apply print theme to Expenses charts
                ExpenseCategoryChart.Foreground = brush;
                ExpenseTrendChart.Foreground = brush;
                ExpenseSpecificChart.Foreground = brush;
                AssetMaintenanceChart.Foreground = brush;

                updateAxes(ExpenseTrendChart);
                updateAxes(ExpenseSpecificChart);
                updateAxes(AssetMaintenanceChart);

                ExpenseCategoryChart.DisableAnimations = isPrint;
                ExpenseTrendChart.DisableAnimations = isPrint;
                ExpenseSpecificChart.DisableAnimations = isPrint;
                AssetMaintenanceChart.DisableAnimations = isPrint;

                ExpenseCategoryChart.Update(true, true);
                ExpenseTrendChart.Update(true, true);
                ExpenseSpecificChart.Update(true, true);
                AssetMaintenanceChart.Update(true, true);
            }

            if (DataContext is AnalyticsDashboardViewModel vm)
            {
                vm.SetPrintMode(isPrint);
            }

            // Force layout update and flush rendering queue
            this.UpdateLayout();

            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Render, new Action(delegate { }));
        }

        /// <summary>
        /// Handles the report generation process by identifying the active view, 
        /// packaging the relevant charts, and delegating file creation to the ViewModel.
        /// </summary>
        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AnalyticsDashboardViewModel vm) return;

            SetTheme(true);

            // Yield to the UI thread so LiveCharts can fully render the black text
            await System.Threading.Tasks.Task.Delay(400);

            List<(string Title, FrameworkElement Chart)> charts;

            // Route execution based on active dashboard visibility
            if (SupplierChart.IsVisible)
            {
                charts = new List<(string Title, FrameworkElement Chart)>
                {
                    ("Supplier Profitability",            SupplierChart),
                    ("Item Velocity — Volume Ranking",    VelocityChart),
                    ("Revenue Contribution by Item",      RevenueChart),
                    ("Revenue Trend Analysis — 7 Days",   TrendChart),
                    ("Discount Effectiveness",            DiscountChart)
                };
            }
            else
            {
                charts = new List<(string Title, FrameworkElement Chart)>
                {
                    ("Expense Category Breakdown",                  ExpenseCategoryChart),
                    ("Monthly Expense Trend",                       ExpenseTrendChart),
                    ("Highest Specific Expenses",                   ExpenseSpecificChart),
                    ("Asset Depreciation & Maintenance Costs",      AssetMaintenanceChart)
                };
            }

            try
            {
                string fullPath = vm.GenerateReport(charts);
                CustomMessageBoxView.Show(
                    $"Report successfully saved to:\n{fullPath}",
                    "✓ Report Generated",
                    CustomMessageType.Success);
            }
            catch (Exception ex)
            {
                CustomMessageBoxView.Show(
                    $"Failed to generate report:\n{ex.Message}",
                    "Error",
                    CustomMessageType.Error);
            }
            finally
            {
                // Ensure UI resets to dark mode regardless of success or failure
                SetTheme(false);
            }
        }
    }
}

