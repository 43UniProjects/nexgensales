using NextGenSales.Views;
using System.Windows;
using System.Windows.Input;

namespace nexgensales.Views
{
    public partial class ExportView : Window
    {
        public ExportView()
        {
            InitializeComponent();
        }

        // Handles window dragging via the custom title bar
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Minimizes the window to the taskbar
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Toggles the window state between maximized and normal
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

        // Safely terminates the application or closes the window
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Used Close() instead of Shutdown() in case it's a child window
        }

        // Handles the navigation back to the Analytics Dashboard
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            
            ProcessView processWindow = new ProcessView();
            processWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            processWindow.Left = this.Left;
            processWindow.Top = this.Top;
            processWindow.Show();
            this.Close();
        }

        // Handles quick navigation from the Export view directly back to the Home dashboard
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate and display the Home dashboard
            HomeView homeWindow = new HomeView();
            homeWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            homeWindow.Left = this.Left;
            homeWindow.Top = this.Top;
            homeWindow.Show();

            // Close the current Export view to free up system resources
            this.Close();
        }
    }
}