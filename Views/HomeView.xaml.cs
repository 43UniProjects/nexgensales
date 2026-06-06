using NexGenSales.Views;
using NexGenSales.ViewModels; // Required to access the HomeViewModel
using System.Windows;
using System.Windows.Input;

namespace NexGenSales.Views
{
    public partial class HomeView : Window
    {
        public HomeView()
        {
            InitializeComponent();

            // Binds the View to its corresponding ViewModel
           
            this.DataContext = new HomeViewModel();
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
            processWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            processWindow.Left = this.Left;
            processWindow.Top = this.Top;

            processWindow.Show();
            this.Close();
        }

        // Handles the logout process, terminating the active session and returning to the authentication screen
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            //  Yes/No Custom Message Box Call
            bool isLogoutConfirmed = CustomMessageBoxView.Show(
                "Are you sure you want to log out of the current session?",
                "Confirm Logout",
                CustomMessageType.Warning,
                CustomMessageButtons.YesNo);

            if (isLogoutConfirmed)
            {
                // If click yes go to login page
                LoginView loginWindow = new LoginView();
                loginWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                loginWindow.Left = this.Left;
                loginWindow.Top = this.Top;

                loginWindow.Show();
                this.Close();
            }
        }
    }
}