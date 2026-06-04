using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LiveCharts;
using NextGenSales.Core;

namespace NextGenSales.Services
{
    /// <summary>
    /// Data transfer record carrying all five ready-to-bind chart datasets.
    /// Produced by SalesAnalysisService.GetAllChartData() and consumed by AnalyticsDashboardViewModel.
    /// </summary>
    public record AnalyticsChartData(
        SeriesCollection SupplierSeries,  string[] SupplierLabels,  string LowMarginWarning,
        SeriesCollection VelocitySeries,  string[] VelocityLabels,
        SeriesCollection RevenueSeries,
        SeriesCollection TrendSeries,     string[] TrendLabels,
        SeriesCollection DiscountSeries,  string[] DiscountLabels
    );

    /// <summary>
    /// Single orchestration service for analytics output.
    /// Pulls raw arrays from MockDataRepository, builds chart series via ChartFactory,
    /// and produces PDF reports via ChartCaptureHelper + ReportBuilder.
    /// </summary>
    public class SalesAnalysisService
    {
        private readonly MockDataRepository _repository;
        private readonly ReportBuilder      _reportBuilder;

        public SalesAnalysisService(MockDataRepository repository)
        {
            _repository    = repository;
            _reportBuilder = new ReportBuilder();
        }

        // ── Chart Data ────────────────────────────────────────────────────────────

        /// <summary>
        /// Fetches all five datasets and returns ready-to-bind SeriesCollections.
        /// </summary>
        public AnalyticsChartData GetAllChartData()
        {
            var (suppliers, ratios, flagged) = _repository.GetSupplierProfitabilityData();
            var (items,     quantities)      = _repository.GetItemVelocityData();
            var (revItems,  avgRevenues)     = _repository.GetRevenueContributionData();
            var (days,      revenues)        = _repository.GetTrendAnalysisData();
            var (labels,    scores)          = _repository.GetDiscountEffectivenessData();

            // Build the low-margin warning string for the UI
            var lowMarginNames = suppliers.Where((s, i) => flagged[i]).ToArray();
            string warning = lowMarginNames.Length > 0
                ? $"⚠  Low-margin suppliers flagged: {string.Join(", ", lowMarginNames)}"
                : "✓  All suppliers are within acceptable margin thresholds.";

            return new AnalyticsChartData(
                ChartFactory.CreateSupplierProfitabilityChart(suppliers, ratios, flagged),
                suppliers, warning,
                ChartFactory.CreateItemVelocityChart(items, quantities), items,
                ChartFactory.CreateRevenueContributionChart(revItems, avgRevenues),
                ChartFactory.CreateTrendAnalysisChart(days, revenues), days,
                ChartFactory.CreateDiscountEffectivenessChart(labels, scores), labels
            );
        }

        // ── Report Generation ─────────────────────────────────────────────────────

        /// <summary>
        /// Captures each chart FrameworkElement as an image, assembles a PDF report,
        /// saves it to filePath, and returns the fully-qualified file path.
        /// </summary>
        public string GenerateReport(
            List<(string Title, FrameworkElement Chart)> charts,
            string filePath)
        {
            var chartImages = new List<(string Title, byte[] Image)>();

            foreach (var (title, visual) in charts)
            {
                var image = ChartCaptureHelper.CaptureToImage(visual);
                if (image != null)
                    chartImages.Add((title, image));
            }

            _reportBuilder.GenerateReport(chartImages, filePath);
            return Path.GetFullPath(filePath);
        }
    }
}
