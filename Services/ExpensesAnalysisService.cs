using System;
using System.Collections.Generic;
using System.Linq;
using NexGenSales.Models;

namespace NexGenSales.Services
{
    public class ExpensesAnalysisService
    {
        public ExpenseAnalyticsResult Analyze(List<ExpensesRecord> expenses)
        {
            var result = new ExpenseAnalyticsResult();
            if (expenses == null || !expenses.Any()) return result;

            result.TotalExpenses = expenses.Sum(e => e.Amount);

            result.CategoryBreakdown = expenses
                .Where(e => !string.IsNullOrEmpty(e.Expense_Category))
                .GroupBy(e => e.Expense_Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            result.AssetMaintenanceCosts = expenses
                .Where(e => !string.IsNullOrEmpty(e.Asset_ID))
                .GroupBy(e => e.Asset_ID)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            // New Chart 1: Daily expense trend (for Line Chart)
            result.DailyTrend = expenses
                .GroupBy(e => e.Date_Recorded.ToString("MMM dd"))
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            // New Chart 2: Top 5 highest expense items (for Bar Chart)
            result.TopSpecificExpenses = expenses
                .Where(e => !string.IsNullOrEmpty(e.Specific_Type))
                .GroupBy(e => e.Specific_Type)
                .OrderByDescending(g => g.Sum(e => e.Amount))
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));


            // ANOMALY DETECTION LOGIC (Category-Based Standard Deviation)

            // Devides the expenses into groups based on their categories
            var groupedByCategory = expenses
                .Where(e => !string.IsNullOrEmpty(e.Expense_Category))
                .GroupBy(e => e.Expense_Category);

            var tempAnomalies = new List<ExpensesRecord>();

            foreach (var group in groupedByCategory)
            {
                var categoryExpenses = group.ToList();

                if (categoryExpenses.Count < 2) continue;

                // 1. Average 
                double averageAmount = categoryExpenses.Average(e => e.Amount);

                // 2. Standard Deviation 
                double sumOfSquares = categoryExpenses.Select(val => Math.Pow(val.Amount - averageAmount, 2)).Sum();
                double standardDeviation = Math.Sqrt(sumOfSquares / categoryExpenses.Count);

                // 3. Threshold 
                double anomalyThreshold = averageAmount + (1.5 * standardDeviation);

                // 4. Grater than 5000
                foreach (var expense in categoryExpenses)
                {
                    if (expense.Amount > anomalyThreshold && expense.Amount > 5000)
                    {
                        tempAnomalies.Add(expense);
                    }
                }
            }

            // Sort anomalies by date recorded in descending order and add to result
            foreach (var anomaly in tempAnomalies.OrderByDescending(a => a.Date_Recorded))
            {
                result.Anomalies.Add(anomaly);
            }

            return result;
        }
    }
}