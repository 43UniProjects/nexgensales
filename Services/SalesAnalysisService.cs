using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LiveCharts;
using NexGenSales.Core;

namespace NexGenSales.Services
{
    /// <summary>
    /// Data transfer record carrying all five ready-to-bind chart datasets.
    /// Produced by SalesAnalysisService.GetAllChartData() and consumed by AnalyticsDashboardViewModel.
    /// </summary>
    public record AnalyticsChartData(
        SeriesCollection SupplierSeries,  string[] SupplierLabels,
        SeriesCollection VelocitySeries,  string[] VelocityLabels,
        SeriesCollection RevenueSeries,
        SeriesCollection TrendSeries,     string[] TrendLabels,
        SeriesCollection DiscountSeries,  string[] DiscountLabels
    );

    /// <summary>
    /// Single orchestration service for analytics output.
    /// Pulls raw arrays from DataRepository, builds chart series via ChartFactory,
    /// and produces PDF reports via ChartCaptureHelper + ReportBuilder.
    /// </summary>
    public class SalesAnalysisService
    {
        private readonly DataRepository _repository;
        private readonly ReportBuilder      _reportBuilder;

        public SalesAnalysisService(DataRepository repository)
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
            var (suppliers, ratios) = _repository.GetSupplierProfitabilityData();
            var (items,     quantities)      = _repository.GetItemVelocityData();
            var (revItems,  avgRevenues)     = _repository.GetRevenueContributionData();
            var (days,      revenues)        = _repository.GetTrendAnalysisData();
            var (labels,    scores)          = _repository.GetDiscountEffectivenessData();

            return new AnalyticsChartData(
                ChartFactory.CreateSupplierProfitabilityChart(suppliers, ratios),
                suppliers,
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
