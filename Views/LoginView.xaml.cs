using System.Windows;
using System.Windows.Input;

using NexGenSales.ViewModels;
using NexGenSales.Services;

namespace NexGenSales.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

        }

        // Handles the click event for the Login button
        
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            
           
        }

        

        // Enables the user to drag and move the application window via the custom title bar
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Handles the click event to minimize the application window to the taskbar
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Toggles the application window state between maximized (full screen) and normal (restored)
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

        // Handles the click event to safely terminate and close the application
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Updates the ViewModel when the password changes (Since PasswordBox doesn't support direct data binding)
        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                var viewModel = (LoginViewModel)this.DataContext;
                viewModel.Password = txtPassword.Password;
            }
        }
    }
}