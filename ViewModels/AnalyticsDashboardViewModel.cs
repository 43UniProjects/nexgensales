using System;
using System.Collections.Generic;
using System.Windows;
using LiveCharts;
using NextGenSales.Core;
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

        /// <summary>Short label used when naming the PDF file (e.g. "Sales", "Expenses").</summary>
        private readonly string _reportType;

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

        /// <param name="reportType">Label embedded in the PDF filename, e.g. "Sales" or "Expenses".</param>
        public AnalyticsDashboardViewModel(SalesAnalysisService service, string reportType = "Sales")
        {
            _service    = service;
            _reportType = reportType;

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
        /// File name is auto-generated with the current timestamp and report type.
        /// Returns the absolute path of the saved file.
        /// </summary>
        public string GenerateReport(List<(string Title, FrameworkElement Chart)> charts)
        {
            string filePath = ReportFileNameHelper.Generate(_reportType);
            return _service.GenerateReport(charts, filePath);
        }
    }
}
