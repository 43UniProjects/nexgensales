using NexGenSales.Services;
using NexGenSales.Views;
using NextGenSales.Views; // Required to navigate to HomeView
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace nexgensales.ViewModels
{
    // Handles data binding and logic for the LoginView
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;

        // Property for the Username input field
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        // Property for the Password input field
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        // Command to handle the login action
        public ICommand LoginCommand { get; }

        // Command to navigate to the registration window
        public ICommand NavigateRegisterCommand { get; }

        // Constructor: Initializes commands
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            NavigateRegisterCommand = new RelayCommand(ExecuteNavigateRegister);
        }

        // Checks if the login button should be enabled (both fields must have text)
        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        // Executes the authentication process and handles window navigation
        private void ExecuteLogin(object parameter)
        {
            // Temporary hardcoded authentication logic for testing purposes
            if (Username == "nexgen" && Password == "123")
            {
                // Instantiate and display the Home dashboard upon successful login

                new DatabaseMigrationService().EnsureMigrated();
                HomeView homeWindow = new HomeView();

                if (parameter is Window loginWindow)
                {
                    homeWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    homeWindow.Left = loginWindow.Left;
                    homeWindow.Top = loginWindow.Top;

                    homeWindow.WindowState = loginWindow.WindowState;

                    homeWindow.Show();
                    loginWindow.Close();
                }
                else
                {
                    homeWindow.Show();
                }

                
            }
            else
            {
                // Display an error prompt if the provided credentials are invalid
                CustomMessageBoxView.Show(
                    "Invalid Username or Password. Please try again.",
                    "Authentication Failed",
                    CustomMessageType.Error
                );
            }
        }

        // Handles navigation to the RegisterView
        private void ExecuteNavigateRegister(object parameter)
        {
            CustomMessageBoxView.Show(
    "Account creation portal is currently under maintenance. Please contact the administrator.",
    "Feature Unavailable",
    CustomMessageType.Warning);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Notifies the UI that a property value has changed
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Standard RelayCommand implementation for MVVM command binding
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Re-evaluates command execution status when UI changes occur
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);
    }
}