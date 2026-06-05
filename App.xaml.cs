using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NextGenSales;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Store the directory at the class level so all methods can access it
    private static string _logDirectory = string.Empty;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 1. Setup the Log Directory automatically based on the build type
        #if DEBUG
        // Development path (Keeps logs in your project folder)
        _logDirectory = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")), "Logs");
        #else
        // Production path (Safe for the final .exe submission)
        _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextGenSales", "Logs");
        #endif
        Directory.CreateDirectory(_logDirectory);

        // 2. Start capturing Console output
        SetupConsoleLogging();

        // 3. Attach Exception Handlers
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        base.OnStartup(e);
    }

    private static void SetupConsoleLogging()
    {
        try
        {
            string consoleLogPath = Path.Combine(_logDirectory, "console.log");

            // FileShare.ReadWrite allows you to open and read the log file while the app is actively running
            var fileStream = new FileStream(consoleLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            var streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };

            // Wrap the StreamWriter in our custom dual-writer
            var dualWriter = new DualConsoleWriter(Console.Out, streamWriter);
            Console.SetOut(dualWriter);

            // This will now print to both the Visual Studio output AND the physical console.log file!
            Console.WriteLine("--- Application Session Started ---");
        }
        catch
        {
            // Silently fail if we cannot access the file
        }
    }

    private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception);
        MessageBox.Show("An unexpected error occurred. A log file has been written to the application data folder.", "NextGenSales Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            LogException(exception);
        }
    }

    private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception);
        e.SetObserved();
    }

    private static void LogException(Exception exception)
    {
        try
        {
            string errorLogPath = Path.Combine(_logDirectory, "error.log");
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {exception}\r\n\r\n";

            File.AppendAllText(errorLogPath, logEntry, Encoding.UTF8);

            // Also write the error to the console log so you can see the sequence of events
            Console.WriteLine($"[CRASH RECORDED] {exception.GetType().Name} - See error.log for stack trace.");
        }
        catch
        {
            // Avoid secondary crashes while logging the original exception.
        }
    }
}

/// <summary>
/// A custom TextWriter that writes to the Visual Studio Debugger AND a physical log file.
/// </summary>
public class DualConsoleWriter : TextWriter
{
    private readonly TextWriter _originalConsole;
    private readonly StreamWriter _fileWriter;

    public DualConsoleWriter(TextWriter originalConsole, StreamWriter fileWriter)
    {
        _originalConsole = originalConsole;
        _fileWriter = fileWriter;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        _originalConsole.Write(value);
        _fileWriter.Write(value);
    }

    public override void Write(string value)
    {
        _originalConsole.Write(value);
        _fileWriter.Write(value);
    }

    public override void WriteLine()
    {
        _originalConsole.WriteLine();
        _fileWriter.WriteLine();
    }

    public override void WriteLine(string value)
    {
        // Automatically inject timestamps into every Console.WriteLine!
        string formattedValue = $"[{DateTime.Now:HH:mm:ss.fff}] {value}";
        _originalConsole.WriteLine(formattedValue);
        _fileWriter.WriteLine(formattedValue);
    }
}