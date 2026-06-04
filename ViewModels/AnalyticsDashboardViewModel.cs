using System;
using System.Collections.Generic;
using System.Windows;
using LiveCharts;
using NextGenSales.Services;

namespace NextGenSales.ViewModels
{
    /// <summary>
    /// Drives the AnalyticsDashboardView.
    /// Populated once on construction via SalesAnalysisService — no INotifyPropertyChanged needed
    /// since all chart data is set once and never changes after load.
    /// </summary>
    public class AnalyticsDashboardViewModel
    {
        private readonly SalesAnalysisService _service;

        // ── 1. Supplier Profitability ─────────────────────────────────────────────
        public SeriesCollection SupplierSeries  { get; }
        public string[]         SupplierLabels  { get; }
        public string           LowMarginWarning { get; }

        // ── 2. Item Velocity ──────────────────────────────────────────────────────
        public SeriesCollection VelocitySeries { get; }
        public string[]         VelocityLabels { get; }

        // ── 3. Revenue Contribution (Pie) ─────────────────────────────────────────
        public SeriesCollection RevenueSeries { get; }

        // ── 4. Trend Analysis ─────────────────────────────────────────────────────
        public SeriesCollection TrendSeries { get; }
        public string[]         TrendLabels { get; }

        // ── 5. Discount Effectiveness ─────────────────────────────────────────────
        public SeriesCollection DiscountSeries { get; }
        public string[]         DiscountLabels { get; }

        // ── Axis formatters ───────────────────────────────────────────────────────
        public Func<double, string> CurrencyFormatter { get; } = v => v.ToString("C0");
        public Func<double, string> PercentFormatter  { get; } = v => v.ToString("P0");
        public Func<double, string> ScoreFormatter    { get; } = v => v.ToString("F1");

        public AnalyticsDashboardViewModel(SalesAnalysisService service)
        {
            _service = service;

            var data = _service.GetAllChartData();

            SupplierSeries   = data.SupplierSeries;  SupplierLabels  = data.SupplierLabels;
            LowMarginWarning = data.LowMarginWarning;
            VelocitySeries   = data.VelocitySeries;  VelocityLabels  = data.VelocityLabels;
            RevenueSeries    = data.RevenueSeries;
            TrendSeries      = data.TrendSeries;      TrendLabels     = data.TrendLabels;
            DiscountSeries   = data.DiscountSeries;   DiscountLabels  = data.DiscountLabels;
        }

        /// <summary>
        /// Called by the View's code-behind to generate the PDF.
        /// Returns the absolute path of the saved file.
        /// </summary>
        public string GenerateReport(List<(string Title, FrameworkElement Chart)> charts)
        {
            return _service.GenerateReport(charts, "NexGenSales_AnalyticsReport.pdf");
        }
    }
}
