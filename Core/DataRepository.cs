using System;
using System.Collections.Generic;

namespace NexGenSales.Core
{
    /// <summary>
    /// Provides data arrays for the analytics dashboard from the SQLite database.
    /// Acts as the data layer replacing MockDataRepository.
    /// </summary>
    public class DataRepository
    {
        private readonly DateTime _startDate; // the data 

        public DataRepository(DateTime startDate)
        {
            _startDate = startDate;
        }

        /// <summary>
        /// Cost-to-profit ratio per supplier.
        /// IsLowMargin flags any supplier whose profit ratio is below 20%.
        /// TODO: Query SalesRecord table where Date_Time >= _startDate.
        /// Group by Supplier_ID, calculate Total Revenue (Sum of Net_Revenue) and Total Cost (Sum of Quantity_Sold * Unit_Purchase_Cost).
        /// Profit Ratio = (Total Revenue - Total Cost) / Total Revenue.
        /// </summary>
        public (string[] Suppliers, double[] ProfitRatios, bool[] IsLowMargin) GetSupplierProfitabilityData()
        {
            // Dummy return for compilation. To be implemented.
            return (Array.Empty<string>(), Array.Empty<double>(), Array.Empty<bool>());
        }

        /// <summary>
        /// Total units sold per item over the selected period.
        /// Used to rank best-sellers vs slow-movers.
        /// TODO: Query SalesRecord table where Date_Time >= _startDate.
        /// Group by Item_ID and sum Quantity_Sold.
        /// </summary>
        public (string[] Items, int[] QuantitiesSold) GetItemVelocityData()
        {
            // Dummy return for compilation. To be implemented.
            return (Array.Empty<string>(), Array.Empty<int>());
        }

        /// <summary>
        /// Average daily revenue contribution for the top 5 performing items.
        /// TODO: Query SalesRecord table where Date_Time >= _startDate.
        /// Group by Item_ID and sum Net_Revenue.
        /// </summary>
        public (string[] Items, double[] AvgDailyRevenue) GetRevenueContributionData()
        {
            // Dummy return for compilation. To be implemented.
            return (Array.Empty<string>(), Array.Empty<double>());
        }

        /// <summary>
        /// Day-wise total revenue for the selected period.
        /// Used to visualise daily performance trends.
        /// TODO: Query SalesRecord table where Date_Time >= _startDate.
        /// Group by Date part of Date_Time and sum Net_Revenue.
        /// </summary>
        public (string[] Days, double[] TotalRevenue) GetTrendAnalysisData()
        {
            // Dummy return for compilation. To be implemented.
            return (Array.Empty<string>(), Array.Empty<double>());
        }

        /// <summary>
        /// Effectiveness score per discount tier.
        /// Higher score = better balance between customer satisfaction and owner margin.
        /// TODO: Query SalesRecord table where Date_Time >= _startDate.
        /// Evaluate available discount to revenues. Group by Allowed_Discount and calculate best items that satisfy both customer and owner.
        /// </summary>
        public (string[] Labels, double[] Scores) GetDiscountEffectivenessData()
        {
            // Dummy return for compilation. To be implemented.
            return (Array.Empty<string>(), Array.Empty<double>());
        }
    }
}
