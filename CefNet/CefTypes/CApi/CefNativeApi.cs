using System;
using System.Runtime.InteropServices;

namespace CefNet.CApi
{
	/// <summary>
	/// Provides CEF methods.
	/// </summary>
	public static partial class CefNativeApi
	{
		/// <remarks>
		/// Defined in include/internal/cef_types_linux.h as
		/// XDisplay* cef_get_xdisplay()
		/// </remarks>
		[DllImport("libcef", CallingConvention = CallingConvention.Cdecl)]
		public static unsafe extern IntPtr cef_get_xdisplay();

		/// <summary>
		/// Returns the current platform thread handle.
		/// </summary>
		/// <remarks>
		/// Defined in include/internal/cef_thread_internal.h as
		/// cef_platform_thread_handle_t cef_get_current_platform_thread_handle()
		/// </remarks>
		[DllImport("libcef", EntryPoint = "cef_get_current_platform_thread_handle", CallingConvention = CallingConvention.Cdecl)]
		public static unsafe extern IntPtr cef_get_current_platform_thread_handle_linux();

		/// <summary>
		/// Returns the current platform thread handle.
		/// </summary>
		/// <remarks>
		/// Defined in include/internal/cef_thread_internal.h as
		/// cef_platform_thread_handle_t cef_get_current_platform_thread_handle()
		/// </remarks>
		[DllImport("libcef", EntryPoint = "cef_get_current_platform_thread_handle", CallingConvention = CallingConvention.Cdecl)]
		public static unsafe extern uint cef_get_current_platform_thread_handle_windows();

		/// <summary>
		/// Set to true (1) before calling Windows APIs like TrackPopupMenu that enter a
		/// modal message loop. Set to false (0) after exiting the modal message loop.
		/// </summary>
		/// <param name="osModalLoop">1 - is in modal window message loop, 0 - is in normal window message loop</param>
		[DllImport("libcef", CallingConvention = CallingConvention.Cdecl)]
		public static unsafe extern void cef_set_osmodal_loop(int osModalLoop);
	}
}
