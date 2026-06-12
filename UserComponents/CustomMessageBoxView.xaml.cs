using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace NexGenSales.UserComponents
{
    public enum CustomMessageType
    {
        Info,
        Success,
        Error,
        Warning
    }

    public enum CustomMessageButtons
    {
        Ok,
        YesNo
    }

    public partial class CustomMessageBoxView : Window
    {
        // Tracks the user's choice (True = OK/Yes, False = No)
        public bool Result { get; private set; } = false;

        public CustomMessageBoxView(string message, string title, CustomMessageType type, CustomMessageButtons buttons)
        {
            InitializeComponent();

            TxtMessage.Text = message;
            TxtTitle.Text = title;

            // Configure Button Visibility and Text based on the required action
            if (buttons == CustomMessageButtons.YesNo)
            {
                BtnOk.Content = "Yes";
                BtnNo.Visibility = Visibility.Visible;
            }
            else
            {
                BtnOk.Content = "OK";
                BtnNo.Visibility = Visibility.Collapsed;
            }

            // Configure Colors and Scalable Text Icons dynamically based on the message type
            switch (type)
            {
                case CustomMessageType.Success:
                    TxtIcon.Text = "✔"; // Standard text symbol (responds to color changes)
                    var successBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896"));
                    TxtTitle.Foreground = successBrush;
                    TxtIcon.Foreground = successBrush; // Apply color to the icon
                    BtnOk.Background = successBrush;
                    break;

                case CustomMessageType.Error:
                    TxtIcon.Text = "✖";
                    var errorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123"));
                    TxtTitle.Foreground = errorBrush;
                    TxtIcon.Foreground = errorBrush;
                    BtnOk.Background = errorBrush;
                    BtnOk.Foreground = new SolidColorBrush(Colors.White);
                    break;

                case CustomMessageType.Warning:
                    TxtIcon.Text = "⚠";
                    var warningBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C400"));
                    TxtTitle.Foreground = warningBrush;
                    TxtIcon.Foreground = warningBrush;
                    BtnOk.Background = warningBrush;
                    break;

                case CustomMessageType.Info:
                default:
                    TxtIcon.Text = "ℹ";
                    // Applied a distinct professional blue color for the Info state
                    var infoBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D7"));
                    TxtTitle.Foreground = infoBrush;
                    TxtIcon.Foreground = infoBrush;
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C2127"));
                    BtnOk.Foreground = new SolidColorBrush(Colors.White);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BtnOk.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (BtnNo.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Left || e.Key == Key.Right)
                {
                    if (BtnOk.IsFocused)
                        BtnNo.Focus();
                    else
                        BtnOk.Focus();

                    e.Handled = true;
                }
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        // =========================================================================================
        // STATIC HELPER METHOD (Returns boolean based on user action)
        // =========================================================================================
        public static bool Show(string message, string title = "Notification", CustomMessageType type = CustomMessageType.Info, CustomMessageButtons buttons = CustomMessageButtons.Ok)
        {
            CustomMessageBoxView msgBox = new CustomMessageBoxView(message, title, type, buttons);

            Window activeWindow = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            if (activeWindow != null)
            {
                msgBox.Owner = activeWindow;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }
}