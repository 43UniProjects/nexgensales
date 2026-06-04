using System.Windows;
using System.Windows.Input;

using nexgensales.ViewModels;

namespace NextGenSales.Views
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
            // Retrieve user inputs from the UI fields
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Temporary hardcoded authentication logic for testing purposes
            // TODO: Replace this with proper SQL database authentication later
            if (username == "nexgen" && password == "123")
            {
                // Instantiate and display the Home dashboard upon successful login
                HomeView homeWindow = new HomeView();
                homeWindow.Show();

                // Close the current Login view to release resources
                this.Close();
            }
            else
            {
                // Display an error prompt if the provided credentials are invalid
                MessageBox.Show(
                    "Invalid Username or Password. Please try again.",
                    "Authentication Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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