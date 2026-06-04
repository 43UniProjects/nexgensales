using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace NexGenSales.core
{
    public static class ChartFactory
    {
        // ==========================================
        // 1. SALES ANALYSIS WRAPPERS
        // ==========================================

        /// <summary>
        /// Trend Analysis: Day-wise total revenue (Line Chart)
        /// </summary>
        public static ISeries[] CreateRevenueTrendSeries(Dictionary<string, double> dailyRevenue)
        {
            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Net Revenue",
                    Values = dailyRevenue.Values.ToArray(),
                    Fill = null, // Keeps it a clean line (no under-fill)
                    GeometrySize = 8,
                    LineSmoothness = 0.5 // Gives that modern curved look
                }
            };
        }

        /// <summary>
        /// Item Velocity: Top selling items (Bar/Column Chart)
        /// </summary>
        public static ISeries[] CreateItemVelocitySeries(Dictionary<string, double> itemVolumes)
        {
            return new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Units Sold",
                    Values = itemVolumes.Values.ToArray(),
                    MaxBarWidth = 40,
                    DataLabelsPaint = new SolidColorPaint(SKColors.DarkSlateGray),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
                }
            };
        }

        /// <summary>
        /// Supplier Profitability & Revenue Contribution (Pie/Doughnut Chart)
        /// Creates a slice for each supplier based on their generated profit.
        /// </summary>
        public static IEnumerable<ISeries> CreateContributionPieChart(Dictionary<string, double> contributions)
        {
            var series = new List<ISeries>();
            foreach (var item in contributions)
            {
                series.Add(new PieSeries<double>
                {
                    Name = item.Key,
                    Values = new double[] { item.Value },
                    InnerRadius = 50, // Makes it a modern Doughnut chart instead of a standard Pie
                    ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: ${point}"
                });
            }
            return series;
        }

        /// <summary>
        /// Discount Optimization: The "Win-Win" Bell Curve (Line Chart)
        /// Plots total profit against discount percentages to show the sweet spot peak.
        /// </summary>
        public static ISeries[] CreateDiscountOptimizationSeries(double[] profitAtVariousDiscounts)
        {
            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Projected Total Profit",
                    Values = profitAtVariousDiscounts, // e.g., index 0 = 0% discount, index 10 = 10% discount
                    Stroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 2 }
                }
            };
        }

        // ==========================================
        // 2. EXPENSES & PREDICTIONS WRAPPERS
        // ==========================================

        /// <summary>
        /// Macro-Forecasting: Will current profit cover end-of-month expenses?
        /// Uses a line chart for profit and a visual "Section" for the target line.
        /// </summary>
        public static ISeries[] CreateMacroForecastingSeries(double[] cumulativeDailyProfit)
        {
            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Cumulative Profit",
                    Values = cumulativeDailyProfit,
                    Fill = new SolidColorPaint(SKColors.LightGreen.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 }
                }
            };
        }

        /// <summary>
        /// Generates the fixed target line (L_total) for the Macro-Forecast chart.
        /// Apply this to the Y-Axis of the chart in the ViewModel.
        /// </summary>
        /// 
        /*
        public static Axis[] CreateMacroForecastingYAxis(double totalMonthlyLiabilities)
        {
            return new Axis[]
            {
                new Axis
                {
                    // Adds a hard red line across the chart indicating the survival target
                    Sections = new LiveChartsCore.Measure.RectangularSection[]
                    {
                        new LiveChartsCore.Measure.RectangularSection
                        {
                            Yi = totalMonthlyLiabilities,
                            Yj = totalMonthlyLiabilities,
                            Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2, PathEffect = new DashEffect(new float[] { 6, 6 }) }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Anomaly Detection: Utility Spikes vs Historical Average
        /// </summary>
        public static ISeries[] CreateAnomalyDetectionSeries(double[] actualExpenses, double movingAverage)
        {
            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Actual Utility Expense",
                    Values = actualExpenses,
                    Stroke = new SolidColorPaint(SKColors.DarkOrange) { StrokeThickness = 2 }
                },
                new LineSeries<double>
                {
                    Name = "Historical Average",
                    // Creates a flat line at the moving average across all points
                    Values = Enumerable.Repeat(movingAverage, actualExpenses.Length).ToArray(),
                    Stroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2, PathEffect = new DashEffect(new float[] { 4, 4 }) },
                    GeometrySize = 0 // Hide the dots on the average line
                }
            };
        }

        // ==========================================
        // 3. UTILITY METHODS (AXIS LABELS)
        // ==========================================
        
        /// <summary>
        /// Generates the X-Axis string labels (e.g., Dates, Item Names, Supplier Names).
        /// </summary>
        public static Axis[] CreateStringXAxis(string[] labels, string axisTitle = "")
        {
            return new Axis[]
            {
                new Axis
                {
                    Name = axisTitle,
                    Labels = labels,
                    LabelsRotation = 45, // Tilts labels so they don't overlap
                    TextSize = 12
                }
            };
        }
        */
    }
}