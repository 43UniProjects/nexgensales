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

            // Configure Button Visibility and Text
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

            // Configure Colors and Icons
            switch (type)
            {
                case CustomMessageType.Success:
                    TxtIcon.Text = "✅";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896"));
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896"));
                    break;
                case CustomMessageType.Error:
                    TxtIcon.Text = "❌";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123"));
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123"));
                    BtnOk.Foreground = new SolidColorBrush(Colors.White);
                    break;
                case CustomMessageType.Warning:
                    TxtIcon.Text = "⚠️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C400"));
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C400"));
                    break;
                case CustomMessageType.Info:
                default:
                    TxtIcon.Text = "ℹ️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896"));
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