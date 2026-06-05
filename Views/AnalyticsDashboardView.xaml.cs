using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NextGenSales.ViewModels;

namespace NextGenSales.Views
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
        /// Collects all named chart controls, passes them to the ViewModel for PDF generation.
        /// Using code-behind here is intentional — the View owns the visual references.
        /// </summary>
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AnalyticsDashboardViewModel vm) return;

            var charts = new List<(string Title, FrameworkElement Chart)>
            {
                ("Supplier Profitability",            SupplierChart),
                ("Item Velocity — Volume Ranking",    VelocityChart),
                ("Revenue Contribution by Item",      RevenueChart),
                ("Revenue Trend Analysis — 7 Days",   TrendChart),
                ("Discount Effectiveness",            DiscountChart)
            };

            try
            {
                string fullPath = vm.GenerateReport(charts);
                MessageBox.Show(
                    $"Report successfully saved to:\n{fullPath}",
                    "✓ Report Generated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to generate report:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
