using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CefNet.Internal
{
	partial class CefAppGlue
	{
		public void OnWebKitInitialized()
		{
			_application.OnWebKitInitialized();
		}

		internal bool AvoidOnBrowserCreated()
		{
			return !VirtualMethods.IsOverridden(_application, typeof(CefNetApplication), nameof(CefNetApplication.OnBrowserCreated));
		}

		public void OnBrowserCreated(CefBrowser browser, CefDictionaryValue extraInfo)
		{
			_application.OnBrowserCreated(browser, extraInfo);
		}

		internal bool AvoidOnBrowserDestroyed()
		{
			return !VirtualMethods.IsOverridden(_application, typeof(CefNetApplication), nameof(CefNetApplication.OnBrowserDestroyed));
		}

		public void OnBrowserDestroyed(CefBrowser browser)
		{
			_application.OnBrowserDestroyed(browser);
		}

		public CefLoadHandler GetLoadHandler()
		{
			return _application.GetLoadHandler();
		}

		internal bool AvoidOnContextCreated()
		{
			return false;
		}

		public void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
		{
			_application.OnContextCreated(browser, frame, context);
		}

		internal bool AvoidOnContextReleased()
		{
			return false;
		}

		public void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
		{
			_application.OnContextReleased(browser, frame, context);
		}

		internal bool AvoidOnUncaughtException()
		{
			return false;
		}

		public void OnUncaughtException(CefBrowser browser, CefFrame frame, CefV8Context context, CefV8Exception exception, CefV8StackTrace stackTrace)
		{
			_application.OnUncaughtException(new CefUncaughtExceptionEventArgs(browser, frame, context, exception, stackTrace));
		}

		internal bool AvoidOnFocusedNodeChanged()
		{
			return !VirtualMethods.IsOverridden(_application, typeof(CefNetApplication), nameof(CefNetApplication.OnFocusedNodeChanged));
		}

		public void OnFocusedNodeChanged(CefBrowser browser, CefFrame frame, CefDOMNode node)
		{
			_application.OnFocusedNodeChanged(browser, frame, node);
		}

		internal bool AvoidOnProcessMessageReceived()
		{
			return false;
		}

		public bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
		{
			var ea = new CefProcessMessageReceivedEventArgs(browser, frame, sourceProcess, message);
			_application.OnCefProcessMessageReceived(ea);
			return ea.Handled;
		}
	}
}
