using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet.Internal
{
	partial class CefAppGlue
	{
		public void OnContextInitialized()
		{
			_application.OnContextInitialized();
		}

		internal bool AvoidOnBeforeChildProcessLaunch()
		{
			return false;
		}

		public void OnBeforeChildProcessLaunch(CefCommandLine commandLine)
		{
			_application.OnBeforeChildProcessLaunch(new BeforeChildProcessLaunchEventArgs(commandLine));
		}

		internal bool AvoidOnScheduleMessagePumpWork()
		{
			return false;
		}

		public void OnScheduleMessagePumpWork(long delayMs)
		{
			_application.OnScheduleMessagePumpWork(delayMs);
		}

		public CefClient GetDefaultClient()
		{
			return _application.GetDefaultClient();
		}

		public CefRequestContextHandler GetDefaultRequestContextHandler()
		{
			return _application.GetDefaultRequestContextHandler();
		}

		internal bool AvoidOnAlreadyRunningAppRelaunch()
		{
			return !VirtualMethods.IsOverridden(_application, typeof(CefNetApplication), nameof(CefNetApplication.OnAlreadyRunningAppRelaunch));
		}

		public bool OnAlreadyRunningAppRelaunch(CefCommandLine commandLine, string currentDirectory)
		{
			return _application.OnAlreadyRunningAppRelaunch(commandLine, currentDirectory);
		}

		internal bool AvoidOnRegisterCustomPreferences()
		{
			return false;
		}

		public void OnRegisterCustomPreferences(CefPreferencesType type, CefPreferenceRegistrar registrar)
		{
			_application.OnRegisterCustomPreferences(type, registrar);
		}
	}
}
