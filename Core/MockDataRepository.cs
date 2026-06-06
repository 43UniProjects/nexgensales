namespace NexGenSales.Core
{
    /// <summary>
    /// Provides hardcoded pre-calculated data arrays for the analytics dashboard.
    /// Acts as a stand-in for real data provided by the backend analysis layer.
    /// No dependencies on LiveCharts or WPF — pure data layer.
    /// </summary>
    public class MockDataRepository
    {
        /// <summary>
        /// Cost-to-profit ratio per supplier.
        /// IsLowMargin flags any supplier whose profit ratio is below 20%.
        /// </summary>
        public (string[] Suppliers, double[] ProfitRatios, bool[] IsLowMargin) GetSupplierProfitabilityData()
        {
            return (
                Suppliers:    new[] { "SUP-ALPHA", "SUP-BETA", "SUP-GAMMA", "SUP-DELTA", "SUP-ECHO" },
                ProfitRatios: new[] { 0.42,         0.31,        0.12,         0.38,        0.18       },
                IsLowMargin:  new[] { false,         false,       true,         false,       true       }
            );
        }

        /// <summary>
        /// Total units sold per item over the selected period.
        /// Used to rank best-sellers vs slow-movers.
        /// </summary>
        public (string[] Items, int[] QuantitiesSold) GetItemVelocityData()
        {
            return (
                Items:          new[] { "ITM-001", "ITM-002", "ITM-003", "ITM-004", "ITM-005",
                                        "ITM-006", "ITM-007", "ITM-008", "ITM-009", "ITM-010" },
                QuantitiesSold: new[] { 145, 89, 210, 67, 178, 34, 122, 56, 198, 73 }
            );
        }

        /// <summary>
        /// Average daily revenue contribution for the top 5 performing items.
        /// </summary>
        public (string[] Items, double[] AvgDailyRevenue) GetRevenueContributionData()
        {
            return (
                Items:          new[] { "ITM-003", "ITM-005", "ITM-009", "ITM-001", "ITM-007" },
                AvgDailyRevenue: new[] { 2100.00, 1875.25, 1580.75, 1250.50, 3200.00 }
            );
        }

        /// <summary>
        /// Day-wise total revenue for the selected 7-day period.
        /// Used to visualise daily performance trends.
        /// </summary>
        public (string[] Days, double[] TotalRevenue) GetTrendAnalysisData()
        {
            return (
                Days:         new[] { "Mon",    "Tue",    "Wed",    "Thu",    "Fri",    "Sat",    "Sun"    },
                TotalRevenue: new[] { 4200.50,  5100.00,  4750.75,  6200.00,  7800.25,  6500.00,  8100.00  }
            );
        }

        /// <summary>
        /// Effectiveness score per discount tier.
        /// Higher score = better balance between customer satisfaction and owner margin.
        /// </summary>
        public (string[] Labels, double[] Scores) GetDiscountEffectivenessData()
        {
            return (
                Labels: new[] { "0%",  "5%",  "10%", "15%", "20%", "25%" },
                Scores: new[] { 62.0,  74.5,  89.2,  83.1,  71.4,  58.9  }
            );
        }
    }
}
