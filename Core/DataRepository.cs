using System;
using System.Collections.Generic;
using System.Linq;
using NexGenSales.Models;
using NexGenSales.Services.Data.Repository;

namespace NexGenSales.Core
{
    /// <summary>
    /// Provides aggregated data arrays for the analytics dashboard using C# LINQ.
    /// Acts as an intermediate data layer, fetching raw data from the SQLite database 
    /// via the SalesRecordRepository and processing it in-memory.
    /// </summary>
    public class DataRepository
    {
        private readonly DateTime _startDate;
        private readonly List<SalesRecord> _filteredSales;

        /// <summary>
        /// Initializes a new instance of the DataRepository and fetches relevant records.
        /// </summary>
        /// <param name="startDate">The starting date for the data analysis period.</param>
        public DataRepository(DateTime startDate)
        {
            _startDate = startDate;

            // Initialize the base repository and SQLite service to fetch data
            var salesRepo = new SalesRecordRepository(new SqliteService());

            // Fetch all sales records from the database asynchronously, blocking for the result
            var allSales = salesRepo.GetAll().GetAwaiter().GetResult() ?? new List<SalesRecord>();

            // Filter the fetched records in-memory based on the selected start date
            _filteredSales = allSales.Where(s => s.Date_Time >= _startDate).ToList();
        }

        /// <summary>
        /// Cost-to-profit ratio per supplier.
        /// TODO: Query SalesRecord table where Date_Time >= _startDate.
        /// Group by Supplier_ID, calculate Total Revenue (Sum of Net_Revenue) and Total Cost (Sum of Quantity_Sold * Unit_Purchase_Cost).
        /// Profit Ratio = (Total Revenue - Total Cost) / Total Revenue.
        /// </summary>
        public (string[] Suppliers, double[] ProfitRatios) GetSupplierProfitabilityData()
        {
            // Dummy return for compilation. To be implemented.
            return (Array.Empty<string>(), Array.Empty<double>());
        }

        /// <summary>
        /// Determines the highest-volume items based on total units sold.
        /// Identifies the top 10 best-selling items for the selected period.
        /// </summary>
        /// <returns>A tuple containing arrays of Item IDs and their corresponding quantities sold.</returns>
        public (string[] Items, int[] QuantitiesSold) GetItemVelocityData()
        {
            Console.WriteLine("[DataRepository] LINQ: Calculating Item Velocity...");

            var velocityGroups = _filteredSales
                .GroupBy(s => s.Item_ID)
                .Select(g => new {
                    ItemId = g.Key,
                    TotalQty = (int)g.Sum(s => s.Quantity_Sold)
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(10) // Limit to the top 10 performing items
                .ToList();

            var items = velocityGroups.Select(x => x.ItemId).ToArray();
            var quantities = velocityGroups.Select(x => x.TotalQty).ToArray();

            return (items, quantities);
        }

        /// <summary>
        /// Calculates the average daily revenue contribution for the top 5 performing items.
        /// </summary>
        /// <returns>A tuple containing arrays of Item IDs and their average daily revenues.</returns>
        public (string[] Items, double[] AvgDailyRevenue) GetRevenueContributionData()
        {
            Console.WriteLine("[DataRepository] LINQ: Calculating Revenue Contribution...");

            // Determine the total number of days in the selected period (minimum 1 day to prevent division by zero)
            double totalDays = (DateTime.Today - _startDate).TotalDays;
            if (totalDays < 1) totalDays = 1;

            var revenueGroups = _filteredSales
                .GroupBy(s => s.Item_ID)
                .Select(g => new {
                    ItemId = g.Key,
                    AvgRev = g.Sum(s => s.Net_Revenue) / totalDays
                })
                .OrderByDescending(x => x.AvgRev)
                .Take(5) // Limit to the top 5 revenue-generating items
                .ToList();

            var items = revenueGroups.Select(x => x.ItemId).ToArray();
            var avgDaily = revenueGroups.Select(x => x.AvgRev).ToArray();

            return (items, avgDaily);
        }

        /// <summary>
        /// Aggregates total revenue on a daily basis to visualize performance trends over time.
        /// </summary>
        /// <returns>A tuple containing arrays of formatted date strings and corresponding total revenues.</returns>
        public (string[] Days, double[] TotalRevenue) GetTrendAnalysisData()
        {
            Console.WriteLine("[DataRepository] LINQ: Calculating Trend Analysis...");

            var trendGroups = _filteredSales
                .GroupBy(s => s.Date_Time.Date)
                .OrderBy(g => g.Key)
                .Select(g => new {
                    DayLabel = g.Key.ToString("MMM dd"), // Format date for UI representation (e.g., "Jun 09")
                    DailyTotal = g.Sum(s => s.Net_Revenue)
                }).ToList();

            var days = trendGroups.Select(x => x.DayLabel).ToArray();
            var revenues = trendGroups.Select(x => x.DailyTotal).ToArray();

            return (days, revenues);
        }

        /// <summary>
        /// Evaluates the profitability score for each distinct discount tier applied to sales.
        /// </summary>
        /// <returns>A tuple containing arrays of formatted discount labels and their profitability scores.</returns>
        public (string[] Labels, double[] Scores) GetDiscountEffectivenessData()
        {
            Console.WriteLine("[DataRepository] LINQ: Calculating Discount Effectiveness...");

            var discountGroups = _filteredSales
                .GroupBy(s => s.Allowed_Discount)
                .OrderBy(g => g.Key)
                .Select(g => new {
                    DiscountLabel = $"Rs. {g.Key:0.00}",
                    ProfitScore = g.Sum(s => s.Net_Revenue - (s.Quantity_Sold * s.Unit_Purchase_Cost))
                }).ToList();

            var labels = discountGroups.Select(x => x.DiscountLabel).ToArray();
            var scores = discountGroups.Select(x => x.ProfitScore).ToArray();

            return (labels, scores);
        }
    }
}