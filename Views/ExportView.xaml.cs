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
            processWindow.Show();
            this.Close();
        }
    }
}