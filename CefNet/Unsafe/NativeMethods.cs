using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CefNet.WinApi;

namespace CefNet
{
	internal static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetMonitorInfo(IntPtr monitorHandle, ref MONITORINFO mInfo);

		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorFlag flag);

		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromPoint(POINT pt, MonitorFlag flag);

		[DllImport("user32.dll")]
		public static extern uint MapVirtualKey(uint code, MapVirtualKeyType type);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct OSVERSIONINFOW
		{
			public int dwOSVersionInfoSize;
			public int dwMajorVersion;
			public int dwMinorVersion;
			public int dwBuildNumber;
			public int dwPlatformId;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string szCSDVersion;
		}

		/// <summary>
		/// Returns the Windows version as seen through the application manifest's compatibility
		/// declarations (unlike <see cref="Environment.OSVersion"/>, which reports the real version).
		/// </summary>
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetVersionExW(ref OSVERSIONINFOW versionInfo);

	}
}
