using Avalonia.Input;
using System;

namespace CefNet.Avalonia
{
	/// <summary>
	/// Drag data that originates from a <see cref="WebView"/>.
	/// </summary>
	public class CefNetDragData
	{
		/// <summary>
		/// The in-process format that carries the <see cref="CefNetDragData"/> instance.
		/// </summary>
		public static readonly DataFormat<CefNetDragData> Format = DataFormat.CreateInProcessFormat<CefNetDragData>(nameof(CefNetDragData));

		internal static readonly DataFormat<string> HtmlFormat = DataFormat.CreateStringPlatformFormat("text/html");
		internal static readonly DataFormat<string> UriListFormat = DataFormat.CreateStringPlatformFormat("text/uri-list");

		private readonly WeakReference<WebView> _source;

		/// <summary>
		/// Initializes a new instance of the <see cref="CefNetDragData"/> class.
		/// </summary>
		/// <param name="source">The source of the drag event.</param>
		/// <param name="data">The original drag data.</param>
		public CefNetDragData(WebView source, CefDragData data)
		{
			_source = new WeakReference<WebView>(source);
			this.Data = data;
		}

		/// <summary>
		/// The original drag data.
		/// </summary>
		public CefDragData Data { get; }

		/// <summary>
		/// The source of the drag event.
		/// </summary>
		public WebView Source
		{
			get
			{
				if (_source.TryGetTarget(out WebView source))
					return source;
				return null;
			}
		}

		/// <summary>
		/// Returns the dragged text: the link URL for links, otherwise the fragment text.
		/// </summary>
		public string GetText()
		{
			if (Data.IsLink)
				return Data.LinkUrl;
			return Data.FragmentText;
		}

		/// <summary>
		/// Creates the data transfer for a drag-and-drop operation, exposing this instance
		/// in-process and its text and HTML to other applications.
		/// </summary>
		public DataTransfer ToDataTransfer()
		{
			var item = new DataTransferItem();
			item.Set(Format, this);

			string text = GetText();
			if (text != null)
				item.SetText(text);
			if (Data.IsLink)
				item.Set(UriListFormat, Data.LinkUrl);
			if (Data.IsFragment && Data.FragmentHtml != null)
				item.Set(HtmlFormat, Data.FragmentHtml);

			var transfer = new DataTransfer();
			transfer.Add(item);
			return transfer;
		}
	}
}
