using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NexGenSales.Core
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


        private static readonly Color[] PiePalette =
        {
            Color.FromRgb(0x00, 0xE5, 0xA0), // teal
            Color.FromRgb(0x00, 0xC8, 0xE5), // cyan
            Color.FromRgb(0x8B, 0x5C, 0xF6), // purple
            Color.FromRgb(0xF5, 0x9E, 0x0B), // amber
            Color.FromRgb(0xEC, 0x48, 0x99), // pink
        };

        // Unique per-supplier colour palette (8 harmonious colours)
        private static readonly Color[] SupplierPalette =
        {
            Color.FromRgb(0x00, 0xE5, 0xA0), // teal
            Color.FromRgb(0x00, 0xC8, 0xE5), // cyan
            Color.FromRgb(0x8B, 0x5C, 0xF6), // purple
            Color.FromRgb(0xF5, 0x9E, 0x0B), // amber
            Color.FromRgb(0xEC, 0x48, 0x99), // pink
            Color.FromRgb(0x3B, 0x82, 0xF6), // blue
            Color.FromRgb(0xEF, 0x44, 0x44), // red
            Color.FromRgb(0x10, 0xB9, 0x81), // emerald
        };

        // ── 1. Supplier Profitability — Column chart, unique colour per supplier ─────
        /// <summary>
        /// Creates one ColumnSeries per supplier so each bar can be individually coloured.
        /// Each supplier gets a unique colour from SupplierPalette.
        /// </summary>
        public static SeriesCollection CreateSupplierProfitabilityChart(
            string[] suppliers, double[] ratios)
        {
            var series = new SeriesCollection();
            for (int i = 0; i < suppliers.Length; i++)
            {
                // Pick a unique colour per supplier
                Color baseColor  = SupplierPalette[i % SupplierPalette.Length];
                SolidColorBrush fill = new SolidColorBrush(baseColor);

                series.Add(new ColumnSeries
                {
                    Title      = suppliers[i],
                    Values     = new ChartValues<double> { ratios[i] },
                    Fill       = fill,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    Effect     = new DropShadowEffect { Color = Colors.Black, ShadowDepth = 0, BlurRadius = 3, Opacity = 1 },
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
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    Effect     = new DropShadowEffect { Color = Colors.Black, ShadowDepth = 0, BlurRadius = 3, Opacity = 1 },
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
                    Title           = items[i],
                    Values          = new ChartValues<double> { revenues[i] },
                    Fill            = new SolidColorBrush(PiePalette[i % PiePalette.Length]),
                    Stroke          = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 2,
                    Foreground      = new SolidColorBrush(Colors.White),
                    FontWeight      = FontWeights.Bold,
                    Effect          = new DropShadowEffect { Color = Colors.Black, ShadowDepth = 0, BlurRadius = 3, Opacity = 1 },
                    DataLabels      = true,
                    LabelPoint      = p => $"{p.Participation:P0}"
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
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    Effect     = new DropShadowEffect { Color = Colors.Black, ShadowDepth = 0, BlurRadius = 3, Opacity = 1 },
                    DataLabels = true,
                    LabelPoint = p => p.Y.ToString("F1")
                }
            };
        }
    }
}
