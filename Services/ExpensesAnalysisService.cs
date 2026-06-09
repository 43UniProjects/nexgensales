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

            // Calculate the average expense amount for each category
            var categoryAverages = expenses
                .Where(e => !string.IsNullOrEmpty(e.Expense_Category))
                .GroupBy(e => e.Expense_Category)
                .ToDictionary(g => g.Key, g => g.Average(e => e.Amount));

            foreach (var expense in expenses)
            {
                if (string.IsNullOrEmpty(expense.Expense_Category)) continue;

                double avg = categoryAverages[expense.Expense_Category];

                // Identify expenses that are significantly higher than the category average
                if (expense.Amount > (avg * 1.5) && expense.Amount > 1000)
                {
                    result.Anomalies.Add(expense);
                }
            }

            return result;
        }
    }
}