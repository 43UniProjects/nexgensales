using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NexGenSales.UserComponents
{
    public partial class CustomFileOpener : Window
    {
        private string _currentDirectory;
        public string SelectedFilePath { get; private set; } = string.Empty;

        public CustomFileOpener(string initialDirectory = "")
        {
            InitializeComponent();
            
            // Wire up the Loaded event to position the window at the top-left of its owner
            this.Loaded += CustomFileOpener_Loaded;

            // Default fallback directory if empty
            _currentDirectory = string.IsNullOrEmpty(initialDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : initialDirectory;
            LoadDirectory(_currentDirectory);
        }

        private void CustomFileOpener_Loaded(object sender, RoutedEventArgs e)
        {
            // Position window relative to its assigned Owner UI container
            if (this.Owner != null)
            {
                this.Left = this.Owner.Left;
                this.Top = this.Owner.Top;
            }
        }

        private void LoadDirectory(string path)
        {
            try
            {
                _currentDirectory = path;
                txtCurrentPath.Text = path;
                lstFileSystem.Items.Clear();

                // Inject parent directory navigate link option
                if (Directory.GetParent(path) != null)
                {
                    lstFileSystem.Items.Add(new FileSystemItem { Name = "..", FullPath = Directory.GetParent(path).FullName, Icon = "📁 [Up]" });
                }

                // Append Subdirectories 
                foreach (var dir in Directory.GetDirectories(path))
                {
                    lstFileSystem.Items.Add(new FileSystemItem { Name = Path.GetFileName(dir), FullPath = dir, Icon = "📁" });
                }

                // Append Local target files
                foreach (var file in Directory.GetFiles(path))
                {
                    lstFileSystem.Items.Add(new FileSystemItem { Name = Path.GetFileName(file), FullPath = file, Icon = "📄" });
                }
            }
            catch (Exception ex)
            {
                // Note: Passing 'this' ensures your error popup maps to this dialog surface area
                CustomMessageBox.Show(this, $"Access Denied: {ex.Message}", "Error");
            }
        }

        private void LstFileSystem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstFileSystem.SelectedItem is FileSystemItem item)
            {
                if (Directory.Exists(item.FullPath))
                {
                    LoadDirectory(item.FullPath);
                }
                else if (File.Exists(item.FullPath))
                {
                    SelectedFilePath = item.FullPath;
                    txtSelectedFile.Text = item.Name;
                }
            }
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lstFileSystem.SelectedItem is FileSystemItem item && File.Exists(item.FullPath))
            {
                SelectedFilePath = item.FullPath;
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnCloseTitle_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
