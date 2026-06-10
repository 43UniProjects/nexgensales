using System.Windows;

namespace NexGenSales.UserComponents
{
    public partial class CustomMessageBox : Window
    {
        // Property to tracking confirmation outcomes
        public bool IsConfirmed { get; private set; } = false;

        public CustomMessageBox(string message, string title, bool showCancelButton)
        {
            InitializeComponent();
            txtMessage.Text = message;
            txtTitle.Text = title;
            btnCancel.Visibility = showCancelButton ? Visibility.Visible : Visibility.Collapsed;
        }

        // Static wrapper to simulate the standard native API call
        public static bool Show(Window owner, string message, string title = "Notification", bool showCancel = false)
        {
            // Detect the truly active window (helps if a child dialog like FileOpener is open)
            Window activeWindow = null;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.IsActive)
                {
                    activeWindow = window;
                    break;
                }
            }

            Window targetOwner = activeWindow ?? owner;
            var box = new CustomMessageBox(message, title, showCancel);

            if (targetOwner != null)
            {
                box.Owner = targetOwner;
                box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            box.ShowDialog(); // Blocks execution thread cleanly
            return box.IsConfirmed;
        }

        // Simplified overload to call without explicitly passing an owner window
        public static bool Show(string message, string title = "Notification", bool showCancel = false)
        {
            return Show(null, message, title, showCancel);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            Close();
        }
    }
}
