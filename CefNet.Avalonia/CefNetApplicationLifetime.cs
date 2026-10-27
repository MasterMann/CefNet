using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace CefNet
{
	/// <summary>
	/// Starts an Avalonia desktop application whose shutdown waits for CEF browsers to close.
	/// </summary>
	public static class CefNetApplicationLifetimeExtensions
	{
		/// <summary>
		/// Starts the application with a <see cref="ClassicDesktopStyleApplicationLifetime"/>, runs the
		/// main loop, and shuts down CEF after it ends.
		/// </summary>
		/// <remarks>
		/// A shutdown request (last or main window closed, <see cref="IClassicDesktopStyleApplicationLifetime.TryShutdown"/>,
		/// or an OS request) is postponed until every CEF browser has closed, because CEF needs the
		/// UI message loop to finish closing them. A forced <see cref="IControlledApplicationLifetime.Shutdown"/>
		/// bypasses this wait.
		/// </remarks>
		/// <returns>The application exit code.</returns>
		public static int StartWithCefNetApplicationLifetime(this AppBuilder builder, string[] args, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
		{
			var lifetime = new ClassicDesktopStyleApplicationLifetime
			{
				Args = args,
				ShutdownMode = shutdownMode
			};

			bool browsersClosed = false;
			bool waitingForBrowsers = false;
			lifetime.ShutdownRequested += (sender, e) =>
			{
				if (browsersClosed)
					return;

				e.Cancel = true;
				if (waitingForBrowsers)
					return;

				waitingForBrowsers = true;
				foreach (Window window in lifetime.Windows.Where(w => w.Owner is null).ToArray())
				{
					window.Close();
				}
				CefNetApplication.Instance.SignalForShutdown(() => Dispatcher.UIThread.Post(() =>
				{
					browsersClosed = true;
					lifetime.Shutdown();
				}));
			};

			builder.SetupWithLifetime(lifetime);
			try
			{
				return lifetime.Start(args);
			}
			finally
			{
				CefNetApplication.Instance.Shutdown();
			}
		}
	}
}
