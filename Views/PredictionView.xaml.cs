using NexGenSales.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace NexGenSales.Views
{
    public partial class PredictionView : Window
    {
        private PredictionViewModel _viewModel;

        public PredictionView()
        {
            InitializeComponent();
            _viewModel = new PredictionViewModel();
            this.DataContext = _viewModel;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnPredict_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.PredictAsync();
        }
    }
}
