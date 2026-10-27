using System;

namespace CefNet
{
	/// <summary>
	/// Provides data for the <see cref="IChromiumWebView.CefAcceleratedPaint"/> event.
	/// </summary>
	public class CefAcceleratedPaintEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CefAcceleratedPaintEventArgs"/> class.
		/// </summary>
		/// <param name="browser">The <see cref="CefBrowser"/> that triggered the event.</param>
		/// <param name="type">A paint element type.</param>
		/// <param name="dirtyRects">The areas of the texture that changed, in pixels.</param>
		/// <param name="info">The shared texture, valid only while the event is raised.</param>
		public CefAcceleratedPaintEventArgs(CefBrowser browser, CefPaintElementType type, CefRect[] dirtyRects, CefAcceleratedPaintInfo info)
		{
			Browser = browser;
			PaintElementType = type;
			DirtyRects = dirtyRects;
			Info = info;
		}

		/// <summary>
		/// Gets the <see cref="CefBrowser"/> that triggered the event.
		/// </summary>
		public CefBrowser Browser { get; }

		/// <summary>
		/// Gets a value indicating whether the element is the view or the popup widget.
		/// </summary>
		public CefPaintElementType PaintElementType { get; }

		/// <summary>
		/// Gets the areas of the texture that changed, in pixels.
		/// </summary>
		public CefRect[] DirtyRects { get; }

		/// <summary>
		/// Gets the shared texture. Copy its contents before the event handler returns.
		/// </summary>
		public CefAcceleratedPaintInfo Info { get; }
	}
}
