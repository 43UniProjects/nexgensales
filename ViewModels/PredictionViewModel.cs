using LiveCharts;
using LiveCharts.Wpf;
using NexGenSales.Models;
using NexGenSales.Services.Data.Repository;
using NexGenSales.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NexGenSales.ViewModels
{
    public class PredictionViewModel : INotifyPropertyChanged
    {
        private string _targetBudget = "";
        private string _statusText = "Enter budget to predict";
        private string _statusDetails = "";
        private Brush _statusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#788896"));
        private SeriesCollection _predictionSeries;
        private string[] _predictionLabels;

        public Func<double, string> YFormatter { get; set; } = value => "Rs. " + value.ToString("N0");

        public event PropertyChangedEventHandler PropertyChanged;

        public string TargetBudget
        {
            get => _targetBudget;
            set { _targetBudget = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public string StatusDetails
        {
            get => _statusDetails;
            set { _statusDetails = value; OnPropertyChanged(); }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public SeriesCollection PredictionSeries
        {
            get => _predictionSeries;
            set { _predictionSeries = value; OnPropertyChanged(); }
        }

        public string[] PredictionLabels
        {
            get => _predictionLabels;
            set { _predictionLabels = value; OnPropertyChanged(); }
        }

        public async Task PredictAsync()
        {
            if (!double.TryParse(TargetBudget, out double budget))
            {
                StatusText = "Invalid budget amount.";
                StatusDetails = "";
                StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123"));
                return;
            }

            try
            {
                var sqliteService = new SqliteService();
                var salesRepo = new SalesRecordRepository(sqliteService);
                var allSales = await salesRepo.GetAll();

                if (allSales == null || !allSales.Any())
                {
                    StatusText = "No sales data available.";
                    StatusDetails = "";
                    StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C400"));
                    return;
                }

                var maxDate = allSales.Max(s => s.Date_Time).Date;

                // Last 7 days fixed window
                var last7Days = new List<double>();
                var labels = new List<string>();

                for (int i = 6; i >= 0; i--)
                {
                    var targetDate = maxDate.AddDays(-i);
                    var dailyRevenue = allSales
                        .Where(s => s.Date_Time.Date == targetDate)
                        .Sum(s => s.Net_Revenue);
                        
                    last7Days.Add(dailyRevenue);
                    labels.Add(targetDate.ToString("MMM dd"));
                }

                int daysInMonth = DateTime.DaysInMonth(maxDate.Year, maxDate.Month);
                DateTime startOfMonth = new DateTime(maxDate.Year, maxDate.Month, 1);
                DateTime endOfMonth = new DateTime(maxDate.Year, maxDate.Month, daysInMonth);

                var currentMonthSales = allSales
                    .Where(s => s.Date_Time.Year == maxDate.Year && s.Date_Time.Month == maxDate.Month)
                    .ToList();
                double currentMonthRevenue = currentMonthSales.Sum(s => s.Net_Revenue);

                // Step 1: Calculate Average Revenue by Day Type
                double totalWeekdayRev = 0, totalWeekendRev = 0;
                int elapsedWeekdays = 0, elapsedWeekends = 0;

                for (DateTime d = startOfMonth; d <= maxDate; d = d.AddDays(1))
                {
                    bool isWeekend = d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;
                    double dailyRev = currentMonthSales.Where(s => s.Date_Time.Date == d).Sum(s => s.Net_Revenue);
                    
                    if (isWeekend)
                    {
                        totalWeekendRev += dailyRev;
                        elapsedWeekends++;
                    }
                    else
                    {
                        totalWeekdayRev += dailyRev;
                        elapsedWeekdays++;
                    }
                }

                double avgWeekdayRev = elapsedWeekdays > 0 ? totalWeekdayRev / elapsedWeekdays : 0;
                double avgWeekendRev = elapsedWeekends > 0 ? totalWeekendRev / elapsedWeekends : 0;

                // Step 2: Count Remaining Days
                int remainingWeekdays = 0, remainingWeekends = 0;
                for (DateTime d = maxDate.AddDays(1); d <= endOfMonth; d = d.AddDays(1))
                {
                    bool isWeekend = d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;
                    if (isWeekend) remainingWeekends++;
                    else remainingWeekdays++;
                }

                // Step 3: Run Final Projection Formula
                double projectedRemaining = (avgWeekdayRev * remainingWeekdays) + (avgWeekendRev * remainingWeekends);
                double totalExpected = currentMonthRevenue + projectedRemaining;

                if (totalExpected >= budget)
                {
                    StatusText = "Sales Performance is On Track";
                    double surplus = totalExpected - budget;
                    StatusDetails = $"Target Revenue: Rs. {budget:N0}   |   Current Revenue: Rs. {currentMonthRevenue:N0}   |   Projected Revenue: Rs. {totalExpected:N0}\n\n" +
                                    $"Estimated Profit: Rs. {surplus:N0}";
                    StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896")); // Success Green
                }
                else
                {
                    StatusText = "Sales Performance is Vulnerable";
                    double deficit = budget - totalExpected;
                    StatusDetails = $"Target Revenue: Rs. {budget:N0}   |   Current Revenue: Rs. {currentMonthRevenue:N0}   |   Projected Revenue: Rs. {totalExpected:N0}\n\n" +
                                    $"Estimated Loss: Rs. {deficit:N0}";
                    StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C400")); // Warning Yellow
                }

                PredictionSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Daily Revenue",
                        Values = new ChartValues<double>(last7Days),
                        DataLabels = true,
                        LabelPoint = cp => "Rs. " + cp.Y.ToString("N0")
                    }
                };
                PredictionLabels = labels.ToArray();
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                StatusDetails = "";
                StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123"));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
