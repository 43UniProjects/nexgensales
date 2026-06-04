using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace NextGenSales.Core
{
    /// <summary>
    /// Static factory that converts pre-calculated data arrays into LiveCharts SeriesCollection objects.
    /// One method per analysis type. No business logic, no data fetching — pure chart construction.
    /// </summary>
    public static class ChartFactory
    {
        // ── Shared colour palette ─────────────────────────────────────────────────
        private static readonly SolidColorBrush AccentTeal =
            new(Color.FromRgb(0x00, 0xE5, 0xA0));

        private static readonly SolidColorBrush AccentTealFill =
            new(Color.FromArgb(0x55, 0x00, 0xE5, 0xA0));

        private static readonly SolidColorBrush LowMarginRed =
            new(Color.FromRgb(0xFF, 0x6B, 0x35));

        private static readonly Color[] PiePalette =
        {
            Color.FromRgb(0x00, 0xE5, 0xA0), // teal
            Color.FromRgb(0x00, 0xC8, 0xE5), // cyan
            Color.FromRgb(0x8B, 0x5C, 0xF6), // purple
            Color.FromRgb(0xF5, 0x9E, 0x0B), // amber
            Color.FromRgb(0xEC, 0x48, 0x99), // pink
        };

        // ── 1. Supplier Profitability — Column chart, flagged bars in red ─────────
        /// <summary>
        /// Creates one ColumnSeries per supplier so each bar can be individually coloured.
        /// Low-margin suppliers (IsLowMargin = true) are rendered in red.
        /// </summary>
        public static SeriesCollection CreateSupplierProfitabilityChart(
            string[] suppliers, double[] ratios, bool[] isLowMargin)
        {
            var series = new SeriesCollection();
            for (int i = 0; i < suppliers.Length; i++)
            {
                series.Add(new ColumnSeries
                {
                    Title      = suppliers[i],
                    Values     = new ChartValues<double> { ratios[i] },
                    Fill       = isLowMargin[i] ? LowMarginRed : AccentTeal,
                    DataLabels = true,
                    LabelPoint = p => p.Y.ToString("P0")
                });
            }
            return series;
        }

        // ── 2. Item Velocity — Column chart ───────────────────────────────────────
        public static SeriesCollection CreateItemVelocityChart(string[] items, int[] quantities)
        {
            return new SeriesCollection
            {
                new ColumnSeries
                {
                    Title      = "Units Sold",
                    Values     = new ChartValues<int>(quantities),
                    Fill       = AccentTeal,
                    DataLabels = true
                }
            };
        }

        // ── 3. Revenue Contribution — Pie (doughnut) chart ────────────────────────
        public static SeriesCollection CreateRevenueContributionChart(
            string[] items, double[] revenues)
        {
            var series = new SeriesCollection();
            for (int i = 0; i < items.Length; i++)
            {
                series.Add(new PieSeries
                {
                    Title      = items[i],
                    Values     = new ChartValues<double> { revenues[i] },
                    Fill       = new SolidColorBrush(PiePalette[i % PiePalette.Length]),
                    DataLabels = true,
                    LabelPoint = p => $"{p.Participation:P0}"
                });
            }
            return series;
        }

        // ── 4. Trend Analysis — Line chart ────────────────────────────────────────
        public static SeriesCollection CreateTrendAnalysisChart(string[] days, double[] revenues)
        {
            return new SeriesCollection
            {
                new LineSeries
                {
                    Title            = "Total Revenue",
                    Values           = new ChartValues<double>(revenues),
                    Stroke           = AccentTeal,
                    Fill             = AccentTealFill,
                    PointGeometrySize = 10,
                    PointForeground  = new SolidColorBrush(Colors.White),
                    DataLabels       = false
                }
            };
        }

        // ── 5. Discount Effectiveness — Column chart ──────────────────────────────
        public static SeriesCollection CreateDiscountEffectivenessChart(
            string[] labels, double[] scores)
        {
            return new SeriesCollection
            {
                new ColumnSeries
                {
                    Title      = "Effectiveness Score",
                    Values     = new ChartValues<double>(scores),
                    Fill       = AccentTeal,
                    DataLabels = true,
                    LabelPoint = p => p.Y.ToString("F1")
                }
            };
        }
    }
}
