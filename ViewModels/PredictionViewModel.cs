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

                double sma = last7Days.Average();
                int daysInMonth = DateTime.DaysInMonth(maxDate.Year, maxDate.Month);
                int remainingDays = daysInMonth - maxDate.Day;

                double currentMonthRevenue = allSales
                    .Where(s => s.Date_Time.Year == maxDate.Year && s.Date_Time.Month == maxDate.Month)
                    .Sum(s => s.Net_Revenue);

                double projectedRemaining = sma * remainingDays;
                double totalExpected = currentMonthRevenue + projectedRemaining;

                if (totalExpected >= budget)
                {
                    StatusText = "On Track";
                    StatusDetails = $"Expected: Rs. {totalExpected:N0} | Current: Rs. {currentMonthRevenue:N0}";
                    StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896")); // Success Green
                }
                else
                {
                    StatusText = "Vulnerable";
                    StatusDetails = $"Expected: Rs. {totalExpected:N0} | Current: Rs. {currentMonthRevenue:N0}";
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
