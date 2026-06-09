using System.Collections.Generic;

namespace NexGenSales.Models
{
    public class ExpenseAnalyticsResult
    {
        public double TotalExpenses { get; set; }
        public Dictionary<string, double> CategoryBreakdown { get; set; } = new Dictionary<string, double>();
        public List<ExpensesRecord> Anomalies { get; set; } = new List<ExpensesRecord>();
        public Dictionary<string, double> AssetMaintenanceCosts { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> DailyTrend { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> TopSpecificExpenses { get; set; } = new Dictionary<string, double>();
    }
}