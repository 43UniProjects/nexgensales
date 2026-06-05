using System;
using System.IO;
using System.Text;
using System.Windows;

namespace NextGenSales;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		DispatcherUnhandledException += OnDispatcherUnhandledException;
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

		base.OnStartup(e);
	}

	private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
	{
		LogException(e.Exception);
		MessageBox.Show("An unexpected error occurred. A log file has been written to the project folder.", "NextGenSales Error", MessageBoxButton.OK, MessageBoxImage.Error);
		e.Handled = true;
	}

	private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception exception)
		{
			LogException(exception);
		}
	}

	private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		LogException(e.Exception);
		e.SetObserved();
	}

	private static void LogException(Exception exception)
	{
		try
		{
			string logDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
			Directory.CreateDirectory(logDirectory);

			string logPath = Path.Combine(logDirectory, "error.log");
			string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {exception}\r\n\r\n";

			File.AppendAllText(logPath, logEntry, Encoding.UTF8);
		}
		catch
		{
			// Avoid secondary crashes while logging the original exception.
		}
	}
}

