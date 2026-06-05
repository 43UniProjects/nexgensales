using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NexGenSales.Views
{
    // find the color of message box
    public enum CustomMessageType
    {
        Info,
        Success,
        Error,
        Warning
    }

    public partial class CustomMessageBoxView : Window
    {
        public CustomMessageBoxView(string message, string title, CustomMessageType type)
        {
            InitializeComponent();

            TxtMessage.Text = message;
            TxtTitle.Text = title;

            // change the message box color
            switch (type)
            {
                case CustomMessageType.Success:
                    TxtIcon.Text = "✅";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896")); // Green
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896"));
                    break;
                case CustomMessageType.Error:
                    TxtIcon.Text = "❌";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123")); // Red
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123"));
                    BtnOk.Foreground = new SolidColorBrush(Colors.White);
                    break;
                case CustomMessageType.Warning:
                    TxtIcon.Text = "⚠️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C400")); // Yellow
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C400"));
                    break;
                case CustomMessageType.Info:
                default:
                    TxtIcon.Text = "ℹ️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C896")); // Default Accent
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C2127"));
                    BtnOk.Foreground = new SolidColorBrush(Colors.White);
                    break;
            }
        }

        // close the Window
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        // move Window 
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        //calling the message box
        public static void Show(string message, string title = "Notification", CustomMessageType type = CustomMessageType.Info)
        {
            CustomMessageBoxView msgBox = new CustomMessageBoxView(message, title, type);
            msgBox.ShowDialog();
        }
    }
}