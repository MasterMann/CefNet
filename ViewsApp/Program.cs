using System;
using System.IO;
using System.Linq;
using CefNet;

namespace ViewsApp
{
	/// <summary>
	/// Opens a browser in a CEF Views top-level window, without any UI framework.
	/// Usage: ViewsApp [--url=&lt;address&gt;]
	/// </summary>
	static class Program
	{
		static int Main(string[] args)
		{
			string url = args.FirstOrDefault(arg => arg.StartsWith("--url=", StringComparison.Ordinal))?.Substring("--url=".Length)
				?? "https://www.wikipedia.org";

			string cefPath = Path.Combine(FindRepositoryRoot(), "cef", PlatformInfo.IsWindows ? "windows" : "linux");

			var settings = new CefSettings();
			settings.NoSandbox = true;
			settings.MultiThreadedMessageLoop = false;
			settings.LocalesDirPath = Path.Combine(cefPath, "Resources", "locales");
			settings.ResourcesDirPath = Path.Combine(cefPath, "Resources");
			settings.LogSeverity = CefLogSeverity.Warning;

			var app = new ViewsApplication(url);
			app.Initialize(Path.Combine(cefPath, "Release"), settings);
			CefApi.RunMessageLoop();
			app.Shutdown();
			return 0;
		}

		private static string FindRepositoryRoot()
		{
			string path = AppContext.BaseDirectory;
			while (path != null && !File.Exists(Path.Combine(path, "CefNet.slnx")))
				path = Path.GetDirectoryName(path);
			return path ?? throw new DirectoryNotFoundException("Could not find the repository root.");
		}
	}

	sealed class ViewsApplication : CefNetApplication
	{
		private readonly string _url;

		public ViewsApplication(string url)
		{
			_url = url;
		}

		protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
		{
			base.OnBeforeCommandLineProcessing(processType, commandLine);
			if (PlatformInfo.IsLinux)
			{
				// .NET cannot run in Chromium's forked zygote processes.
				commandLine.AppendSwitch("no-zygote");
			}
		}

		protected override void OnContextInitialized()
		{
			base.OnContextInitialized();
			CefBrowserView browserView = CefBrowserView.CreateBrowserView(new CefClient(), _url, new CefBrowserSettings(), null, null, null);
			CefWindow.CreateTopLevelWindow(new MainWindowDelegate(browserView));
		}
	}

	sealed class MainWindowDelegate : CefWindowDelegate
	{
		private CefBrowserView _browserView;

		public MainWindowDelegate(CefBrowserView browserView)
		{
			_browserView = browserView;
		}

		protected override void OnWindowCreated(CefWindow window)
		{
			window.AddChildView(_browserView);
			window.Title = "CefNet Views";
			window.CenterWindow(new CefSize(1024, 768));
			window.Show();
			_browserView.RequestFocus();
		}

		protected override void OnWindowDestroyed(CefWindow window)
		{
			_browserView = null;
			CefApi.QuitMessageLoop();
		}

		protected override bool CanClose(CefWindow window)
		{
			CefBrowser browser = _browserView?.Browser;
			return browser is null || browser.Host.TryCloseBrowser();
		}
	}
}
