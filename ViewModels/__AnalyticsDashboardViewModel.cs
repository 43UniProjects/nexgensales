using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using NexGenSales.Models;
using NexGenSales.Core;
using NexGenSales.Services.Data.Reposistory;
using NexGenSales.core;

namespace NexGenSales.ViewModels
{
    public partial class AnalyticsDashboardViewModle
    {
        private readonly SalesRecordRepository _salesRepo = new SalesRecordRepository(new SqliteService());
        private readonly ExpenseRecordRepository _expensesRepo = new ExpenseRecordRepository(new SqliteService());

        /// <summary>
        /// Executes all 9 analytical engines based on the selected date range.
        /// </summary>
        public async Task LoadDashboardData(DateTime startDate, DateTime endDate)
        {
            // 1. Fetch data from SQLite via Dapper Repository
            List<SalesRecord> rawSales = await _salesRepo.GetAll() ?? [];
            List<ExpensesRecord> rawExpenses = await _expensesRepo.GetAll() ?? [];

            // If there's no data at all, nothing to do
            if (!rawSales.Any() && !rawExpenses.Any()) return;

            int daysInRange = (endDate - startDate).Days;
            if (daysInRange == 0) daysInRange = 1; // Prevent division by zero

            // Clear previous alerts
            /*
            LowMarginSupplierAlerts.Clear();
            WastageAlerts.Clear();
            DepreciatingAssets.Clear();
            StockOutAlerts.Clear();
            */

            // Prepare dailyRevenue container so expenses logic can use it even if sales are absent
            var dailyRevenue = new Dictionary<string, double>();

            // ====================================================================
            // 1. SALES ANALYSIS CRITERIA
            // ====================================================================
            if (rawSales.Any())
            {
                // --- Criterion 1: Supplier Profitability ---
                var supplierStats = rawSales
                    .GroupBy(s => s.Supplier_ID)
                    .Select(g => new
                    {
                        Supplier = g.Key,
                        TotalRevenue = g.Sum(s => s.Net_Revenue),
                        TotalCost = g.Sum(s => s.Quantity_Sold * s.Unit_Purchase_Cost)
                    })
                    .Select(s => new
                    {
                        s.Supplier,
                        Profit = s.TotalRevenue - s.TotalCost,
                        Margin = s.TotalRevenue > 0 ? ((s.TotalRevenue - s.TotalCost) / s.TotalRevenue) : 0
                    }).ToList();

                var supplierProfits = supplierStats.ToDictionary(s => s.Supplier, s => s.Profit);
                ISeries[] SupplierProfitabilitySeries = ChartFactory.CreateContributionPieChart(supplierProfits).ToArray();

                foreach (var sup in supplierStats.Where(s => s.Margin < 0.15)) // 15% margin threshold
                {
                    //LowMarginSupplierAlerts.Add($"WARNING: Supplier '{sup.Supplier}' margin is critically low ({(sup.Margin * 100):F1}%).");
                }

                // --- Criterion 2: Item Velocity ---
                var itemVolumes = rawSales
                    .GroupBy(s => s.Item_ID)
                    .OrderByDescending(g => g.Sum(s => s.Quantity_Sold))
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity_Sold));

                //ItemVelocitySeries = ChartFactory.CreateItemVelocitySeries(itemVolumes);
                //ItemVelocityXAxis = ChartFactory.CreateStringXAxis(itemVolumes.Keys.ToArray(), "Top Items");

                // --- Criterion 3: Revenue Contribution ---
                var avgDailyRevenuePerItem = rawSales
                    .GroupBy(s => s.Item_ID)
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.Net_Revenue) / daysInRange);

                // Re-using the Item Velocity chart style for Revenue Contribution
                //RevenueContributionSeries = ChartFactory.CreateItemVelocitySeries(avgDailyRevenuePerItem);

                // --- Criterion 4: Trend Analysis ---
                dailyRevenue = rawSales
                    .GroupBy(s => s.Date_Time.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key.ToString("MMM dd"), g => g.Sum(s => s.Net_Revenue));

                //TrendAnalysisSeries = ChartFactory.CreateRevenueTrendSeries(dailyRevenue);
                //TrendAnalysisXAxis = ChartFactory.CreateStringXAxis(dailyRevenue.Keys.ToArray(), "Date");

                // --- Criterion 5: Discount Optimization ---
                // Group by discount percentage to find which discount yields the highest total profit
                var discountOptimization = rawSales
                    .GroupBy(s => s.Allowed_Discount)
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        g => $"{g.Key}%",
                        g => g.Sum(s => s.Net_Revenue - (s.Quantity_Sold * s.Unit_Purchase_Cost))
                    );

                //DiscountOptimizationSeries = ChartFactory.CreateRevenueTrendSeries(discountOptimization); // Reusing line chart
                //DiscountOptimizationXAxis = ChartFactory.CreateStringXAxis(discountOptimization.Keys.ToArray(), "Discount %");
            }

            // ====================================================================
            // 2. EXPENSES & PREDICTIONS CRITERIA
            // ====================================================================
            if (rawExpenses != null && rawExpenses.Any())
            {
                // --- Criterion 6: Anomaly Detection (Utilities) ---
                var utilityExpenses = rawExpenses
                    .Where(e => e.Expense_Category.Equals("Utility", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.Date_Recorded)
                    .ToList();

                if (utilityExpenses.Any())
                {
                    double historicalAverage = utilityExpenses.Average(e => e.Amount);
                    double[] actualAmounts = utilityExpenses.Select(e => e.Amount).ToArray();

                    //UtilityAnomalySeries = ChartFactory.CreateAnomalyDetectionSeries(actualAmounts, historicalAverage);

                    // Flag recent spikes (e.g., > 20% over average)
                    var recentSpikes = utilityExpenses.Where(e => e.Amount > (historicalAverage * 1.20));
                    foreach (var spike in recentSpikes)
                    {
                        //WastageAlerts.Add($"ANOMALY: {spike.Specific_Type} on {spike.Date_Recorded:MMM dd} spiked to ${spike.Amount:F2} (Avg: ${historicalAverage:F2})");
                    }
                }

                // --- Criterion 7: Depreciation Tracking ---
                var assetExpenses = rawExpenses.Where(e => !string.IsNullOrEmpty(e.Asset_ID));
                foreach (var asset in assetExpenses)
                {
                    // Logic: Assuming 'Amount' in this context is the logged depreciation value
                   /*
                    DepreciatingAssets.Add(new AssetDepreciationItem
                    {
                        AssetId = asset.Asset_ID,
                        Name = asset.Specific_Type,
                        PurchaseValue = asset.Amount * 10, // Simulated original value
                        CurrentValue = asset.Amount * 9    // Simulated degraded value
                    });*/
                }

                // --- Criterion 8: Profit Velocity vs End-of-Month Expenses ---
                double totalFixedExpenses = rawExpenses.Where(e => e.Expense_Category != "Utility").Sum(e => e.Amount);

                // Calculate cumulative profit day by day
                var cumulativeProfitList = new List<double>();
                double runningProfit = 0;
                foreach (var day in dailyRevenue)
                {
                    runningProfit += day.Value; // Simplified: Revenue as Profit for visualization
                    cumulativeProfitList.Add(runningProfit);
                }

                //ProfitVelocitySeries = ChartFactory.CreateMacroForecastingSeries(cumulativeProfitList.ToArray());
                //ProfitVelocityYAxis = ChartFactory.CreateMacroForecastingYAxis(totalFixedExpenses);

                double projectedEndMonthProfit = (runningProfit / daysInRange) * 30; // Project current velocity out to 30 days
                if (projectedEndMonthProfit >= totalFixedExpenses)
                    Console.WriteLine();
                    //ProfitVelocityStatusText = $"Surplus Expected: Projected ${projectedEndMonthProfit:F2} against ${totalFixedExpenses:F2} liabilities.";
                else
                    Console.WriteLine();
                    //ProfitVelocityStatusText = $"DEFICIT WARNING: Projected ${projectedEndMonthProfit:F2} will not cover ${totalFixedExpenses:F2} liabilities.";
            }

            // --- Criterion 9: Stock Availability Alerts ---
            if (rawSales.Any())
            {
                // Calculate how fast items are selling, and divide current stock by that rate
                var itemSalesRates = rawSales
                    .GroupBy(s => new { s.Item_ID, s.Current_Stock }) // Grouping by both to keep the latest stock value
                    .Select(g => new
                    {
                        ItemID = g.Key.Item_ID,
                        Stock = g.Key.Current_Stock,
                        DailySalesVelocity = g.Sum(s => s.Quantity_Sold) / daysInRange
                    })
                    .Where(x => x.DailySalesVelocity > 0);

                foreach (var item in itemSalesRates)
                {
                    int daysUntilEmpty = (int)(item.Stock / item.DailySalesVelocity);

                    if (daysUntilEmpty <= 7) // Alert if stock will deplete in a week
                    {/*
                        StockOutAlerts.Add(new StockAlertItem
                        {
                            ItemId = item.ItemID,
                            ItemName = $"Item {item.ItemID}",
                            CurrentStock = item.Stock,
                            DaysUntilEmpty = daysUntilEmpty
                        });*/
                    }
                }
            }

            // ====================================================================
            // 3. UPDATE THE UI
            // ====================================================================
            /*
            OnPropertyChanged(nameof(SupplierProfitabilitySeries));
            OnPropertyChanged(nameof(ItemVelocitySeries));
            OnPropertyChanged(nameof(ItemVelocityXAxis));
            OnPropertyChanged(nameof(RevenueContributionSeries));
            OnPropertyChanged(nameof(TrendAnalysisSeries));
            OnPropertyChanged(nameof(TrendAnalysisXAxis));
            OnPropertyChanged(nameof(DiscountOptimizationSeries));
            OnPropertyChanged(nameof(DiscountOptimizationXAxis));
            OnPropertyChanged(nameof(UtilityAnomalySeries));
            OnPropertyChanged(nameof(ProfitVelocitySeries));
            OnPropertyChanged(nameof(ProfitVelocityYAxis));
            OnPropertyChanged(nameof(ProfitVelocityStatusText)); */
        }
    }
}