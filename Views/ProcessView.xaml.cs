using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using NexGenSales.Core;
using NexGenSales.Services;
using NexGenSales.Services.Data.Repository;
using NexGenSales.ViewModels;
using NexGenSales.UserComponents;


namespace NexGenSales.Views
{
    public partial class ProcessView : Window
    {
        public ProcessView()
        {
            InitializeComponent();

            DataContext = new ProcessViewModel();
        }



        // Enable dragging the window by holding the left mouse button
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        // Minimize Button Logic
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Maximize and Restore Button Logic
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        // Navigate to the Home window
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            HomeView homeWindow = new HomeView();
            homeWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            homeWindow.Left = this.Left;
            homeWindow.Top = this.Top;

            homeWindow.WindowState = this.WindowState;
            homeWindow.Show();
            this.Close();
        }

        // Open the Prediction Dashboard as a dialog
        private void BtnPrediction_Click(object sender, RoutedEventArgs e)
        {
            PredictionView predictionWindow = new PredictionView
            {
                Owner = this
            };
            predictionWindow.ShowDialog();
        }

        // Navigate to the Export window
        private void BtnExports_Click(object sender, RoutedEventArgs e)
        {
            ExportView exportWindow = new ExportView();
            exportWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            exportWindow.Left = this.Left;
            exportWindow.Top = this.Top;
            exportWindow.WindowState = this.WindowState;
            exportWindow.Show();
            this.Close();
        }

        // Close Button Logic
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
    }
}