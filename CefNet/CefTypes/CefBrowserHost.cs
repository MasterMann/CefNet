namespace CefNet
{
	public partial class CefBrowserHost
	{
		/// <summary>
		/// Closes the DevTools window, if any, and then the browser.
		/// </summary>
		/// <param name="forceClose">True to close the browser without running unload handlers.</param>
		/// <remarks>
		/// CEF crashes while destroying a Chrome-style DevTools window whose inspected browser
		/// has already been closed, so DevTools must be closed first.
		/// </remarks>
		public void CloseBrowserAndDevTools(bool forceClose)
		{
			if (HasDevTools)
				CloseDevTools();
			CloseBrowser(forceClose);
		}
	}
}
