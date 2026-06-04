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

        public LoginViewModel()
        {
            // Initialize commands with their respective execution methods
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            NavigateRegisterCommand = new RelayCommand(ExecuteNavigateRegister);
        }

        // Checks if the login button should be enabled (both fields must have text)
        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        // Executes the login process
        private void ExecuteLogin(object parameter)
        {
            // TODO: Add database authentication logic here
            MessageBox.Show($"Attempting to login with Username: {Username}", "Authentication", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Handles navigation to the RegisterView
        private void ExecuteNavigateRegister(object parameter)
        {
            // TODO: Add logic to open RegisterView and close LoginView
            MessageBox.Show("Navigating to Register Window...", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ====================================================================
        // INotifyPropertyChanged Implementation for UI updates
        // ====================================================================

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