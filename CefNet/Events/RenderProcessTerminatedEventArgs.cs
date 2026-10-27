using System;

namespace CefNet
{
	/// <summary>
	/// Provides data for the <see cref="IChromiumWebView.LoadError"/> event.
	/// </summary>
	public sealed class RenderProcessTerminatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RenderProcessTerminatedEventArgs"/> class.
		/// </summary>
		/// <param name="status">Indicates how the process terminated</param>
		/// <param name="errorCode">The platform-specific process exit code.</param>
		/// <param name="errorString">A description of the termination reason.</param>
		public RenderProcessTerminatedEventArgs(CefTerminationStatus status, int errorCode, string errorString)
		{
			this.Status = status;
			this.ErrorCode = errorCode;
			this.ErrorString = errorString;
		}

		/// <summary>
		/// Indicates how the process terminated.
		/// </summary>
		public CefTerminationStatus Status { get; }

		/// <summary>
		/// Gets the platform-specific process exit code.
		/// </summary>
		public int ErrorCode { get; }

		/// <summary>
		/// Gets a description of the termination reason.
		/// </summary>
		public string ErrorString { get; }
	}
}
