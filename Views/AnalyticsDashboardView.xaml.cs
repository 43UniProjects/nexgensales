using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NexGenSales.ViewModels;

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
        /// Collects all named chart controls, passes them to the ViewModel for PDF generation.
        /// Using code-behind here is intentional — the View owns the visual references.
        /// </summary>
        private void SetTheme(bool isPrint)
        {
            var brush = isPrint ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            var axisBrush = isPrint ? brush : new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#788896"));

            SupplierChart.Foreground = brush;
            VelocityChart.Foreground = brush;
            RevenueChart.Foreground = brush;
            TrendChart.Foreground = brush;
            DiscountChart.Foreground = brush;

            Action<LiveCharts.Wpf.CartesianChart> updateAxes = (chart) => 
            {
                foreach(var axis in chart.AxisX) 
                {
                    axis.Foreground = axisBrush;
                    
                    // Force LiveCharts to recreate Axis labels and titles to apply the new color
                    bool labels = axis.ShowLabels;
                    axis.ShowLabels = !labels;
                    axis.ShowLabels = labels;
                    
                    string title = axis.Title;
                    axis.Title = null;
                    axis.Title = title;
                }
                foreach(var axis in chart.AxisY) 
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

            updateAxes(SupplierChart);
            updateAxes(VelocityChart);
            updateAxes(TrendChart);
            updateAxes(DiscountChart);

            // Disable animations temporarily so LiveCharts updates colors synchronously
            SupplierChart.DisableAnimations = isPrint;
            VelocityChart.DisableAnimations = isPrint;
            RevenueChart.DisableAnimations = isPrint;
            TrendChart.DisableAnimations = isPrint;
            DiscountChart.DisableAnimations = isPrint;

            if (DataContext is AnalyticsDashboardViewModel vm)
            {
                vm.SetPrintMode(isPrint);
            }

            // Force layout update and flush rendering queue so the new black text applies IMMEDIATELY
            this.UpdateLayout();
            
            SupplierChart.Update(true, true);
            VelocityChart.Update(true, true);
            RevenueChart.Update(true, true);
            TrendChart.Update(true, true);
            DiscountChart.Update(true, true);

            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Render, new Action(delegate { }));
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AnalyticsDashboardViewModel vm) return;

            SetTheme(true);

            // Yield to the UI thread for 400ms so LiveCharts has ample time to fully render the new black text
            await System.Threading.Tasks.Task.Delay(400);

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
            finally
            {
                SetTheme(false);
            }
        }
    }
}
