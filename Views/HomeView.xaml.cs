using NextGenSales.Views;
using System.Windows;
using System.Windows.Input;

namespace NextGenSales.Views
{
    public partial class HomeView : Window
    {
        public HomeView()
        {
            InitializeComponent();
        }

        // Title bar window dragging
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Minimize Button
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Maximize/Restore Button
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

        // Close Button
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Top Right Goto Process Button
        private void BtnGotoProcess_Click(object sender, RoutedEventArgs e)
        {
            
            ProcessView processWindow = new ProcessView();
            processWindow.Show();

            
            this.Close();
        }
    }
}