using LiveCharts;
using LiveCharts.Wpf;
using NexGenSales.Core;
using NexGenSales.Models;
using NexGenSales.Services;
using System;
using System.Collections.Generic;
using System.Windows;

namespace NexGenSales.ViewModels
{
    public class AnalyticsDashboardViewModel
    {
        private readonly SalesAnalysisService _salesService;
        private readonly ExpensesAnalysisService _expensesService;
        private readonly string _reportType;

        public Visibility SalesVisibility { get; }
        public Visibility ExpenseVisibility { get; }

        // Newly added subtitle property for the dashboard
        public string DashboardSubtitle { get; }

        // Sales Properties
        public SeriesCollection SupplierSeries { get; }
        public string[] SupplierLabels { get; }
        public SeriesCollection VelocitySeries { get; }
        public string[] VelocityLabels { get; }
        public SeriesCollection RevenueSeries { get; }
        public SeriesCollection TrendSeries { get; }
        public string[] TrendLabels { get; }
        public SeriesCollection DiscountSeries { get; }
        public string[] DiscountLabels { get; }

        // Expenses Properties
        public string TotalExpensesDisplay { get; }
        public SeriesCollection ExpenseCategorySeries { get; }
        public SeriesCollection AssetMaintenanceSeries { get; }
        public string[] AssetLabels { get; }
        public List<ExpensesRecord> AnomaliesList { get; }

        // Properties for the two expense charts
        public SeriesCollection ExpenseTrendSeries { get; }
        public string[] ExpenseTrendLabels { get; }
        public SeriesCollection SpecificTypeSeries { get; }
        public string[] SpecificTypeLabels { get; }

        public Func<double, string> CurrencyFormatter { get; } = v => "Rs. " + v.ToString("N2");
        public Func<double, string> PercentFormatter { get; } = v => v.ToString("P0");
        public Func<double, string> ScoreFormatter { get; } = v => v.ToString("F1");

        // Sales Constructor
        public AnalyticsDashboardViewModel(SalesAnalysisService service, string reportType = "Sales")
        {
            _salesService = service;
            _reportType = reportType;
            SalesVisibility = Visibility.Visible;
            ExpenseVisibility = Visibility.Collapsed;

            // Subtitle for the sales dashboard
            DashboardSubtitle = "Sales simulation data — 5 analysis modules";

            var data = _salesService.GetAllChartData();
            SupplierSeries = data.SupplierSeries; SupplierLabels = data.SupplierLabels;
            VelocitySeries = data.VelocitySeries; VelocityLabels = data.VelocityLabels;
            RevenueSeries = data.RevenueSeries;
            TrendSeries = data.TrendSeries; TrendLabels = data.TrendLabels;
            DiscountSeries = data.DiscountSeries; DiscountLabels = data.DiscountLabels;
        }

        // Expenses Constructor
        public AnalyticsDashboardViewModel(ExpenseAnalyticsResult expenseData, ExpensesAnalysisService expenseService, string reportType = "Expenses")
        {
            _expensesService = expenseService;
            _reportType = reportType;
            SalesVisibility = Visibility.Collapsed;
            ExpenseVisibility = Visibility.Visible;

            // Subtitle for the expenses dashboard
            DashboardSubtitle = "Expenses simulation data — 5 analysis modules";

            TotalExpensesDisplay = CurrencyFormatter(expenseData.TotalExpenses);

            ExpenseCategorySeries = new SeriesCollection();
            foreach (var kvp in expenseData.CategoryBreakdown)
            {
                ExpenseCategorySeries.Add(new PieSeries
                {
                    Title = kvp.Key,
                    Values = new ChartValues<double> { kvp.Value },
                    DataLabels = true,

                    // Display only the percentage on the slice (Ex: 36.95%)
                    LabelPoint = cp => cp.Participation.ToString("P2"),

                    // Keep the label inside the slice for a cleaner look since the text is shorter
                    LabelPosition = PieLabelPosition.InsideSlice,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 13,

                    // Add a small gap between slices for a cleaner look
                    Stroke = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0B0F13")),
                    StrokeThickness = 2
                });
            }

            ExpenseTrendSeries = new SeriesCollection();
            var trendLabels = new List<string>();
            var trendValues = new ChartValues<double>();
            foreach (var kvp in expenseData.DailyTrend) { trendLabels.Add(kvp.Key); trendValues.Add(kvp.Value); }
            ExpenseTrendSeries.Add(new LineSeries { Title = "Daily Expense", Values = trendValues, DataLabels = true, LabelPoint = cp => CurrencyFormatter(cp.Y), Foreground = System.Windows.Media.Brushes.White });
            ExpenseTrendLabels = trendLabels.ToArray();

            SpecificTypeSeries = new SeriesCollection();
            var typeLabels = new List<string>();
            var typeValues = new ChartValues<double>();
            foreach (var kvp in expenseData.TopSpecificExpenses) { typeLabels.Add(kvp.Key); typeValues.Add(kvp.Value); }
            SpecificTypeSeries.Add(new ColumnSeries { Title = "Amount", Values = typeValues, DataLabels = true, LabelPoint = cp => CurrencyFormatter(cp.Y), Foreground = System.Windows.Media.Brushes.White });
            SpecificTypeLabels = typeLabels.ToArray();

            AssetMaintenanceSeries = new SeriesCollection();
            var labels = new List<string>();
            var values = new ChartValues<double>();
            foreach (var kvp in expenseData.AssetMaintenanceCosts) { labels.Add(kvp.Key); values.Add(kvp.Value); }
            AssetMaintenanceSeries.Add(new ColumnSeries { Title = "Maintenance Cost", Values = values, DataLabels = true, LabelPoint = cp => CurrencyFormatter(cp.Y), Foreground = System.Windows.Media.Brushes.White });
            AssetLabels = labels.ToArray();

            AnomaliesList = expenseData.Anomalies;
    
        }

        public void SetPrintMode(bool isPrintMode)
        {
            var brush = isPrintMode ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            var effect = isPrintMode ? null : new System.Windows.Media.Effects.DropShadowEffect { Color = System.Windows.Media.Colors.Black, ShadowDepth = 0, BlurRadius = 3, Opacity = 1 };

            Action<SeriesCollection> apply = (sc) =>
            {
                if (sc == null) return;

                foreach (var series in sc)
                {
                    if (series is LiveCharts.Wpf.Series lvcSeries)
                    {
                        lvcSeries.Foreground = brush;
                        lvcSeries.Effect = effect;

                        // Skip toggling DataLabels for PieSeries to prevent radius miscalculation and edge clipping
                        if (series is LiveCharts.Wpf.PieSeries)
                            continue;

                        bool hadLabels = lvcSeries.DataLabels;
                        lvcSeries.DataLabels = false;
                        lvcSeries.DataLabels = hadLabels;
                    }
                }
            };

            apply(SupplierSeries);
            apply(VelocitySeries);
            apply(RevenueSeries);
            apply(TrendSeries);
            apply(DiscountSeries);
            apply(ExpenseCategorySeries);
            apply(AssetMaintenanceSeries);
            apply(ExpenseTrendSeries);
            apply(SpecificTypeSeries);
        }

        /// <summary>
        /// Routes the report generation request to the appropriate application service 
        /// based on the active dashboard context (Sales vs. Expenses).
        /// </summary>
        /// <param name="charts">A collection of named UI chart elements to be embedded in the PDF.</param>
        /// <returns>The absolute file path of the successfully generated PDF document.</returns>
        public string GenerateReport(List<(string Title, FrameworkElement Chart)> charts)
        {
            // Generate a standardized file name with a timestamp
            string filePath = ReportFileNameHelper.Generate(_reportType);

            if (_reportType == "Sales")
            {
                // Delegate rendering to the Sales logic layer
                return _salesService.GenerateReport(charts, filePath);
            }
            else if (_reportType == "Expenses")
            {
                // Delegate rendering to the Expenses logic layer
                return _expensesService.GenerateReport(charts, TotalExpensesDisplay, AnomaliesList, filePath);
            }

            // Fallback for unsupported report types to prevent application crashes
            return string.Empty;
        }
    }
}