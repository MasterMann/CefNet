using CefNet.CApi;

namespace CefNet
{
	public unsafe partial class CefCompositionUnderline
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CefCompositionUnderline"/> class.
		/// </summary>
		/// <param name="range">The underline character range.</param>
		/// <param name="color">The text color.</param>
		/// <param name="backgroundColor">The background color.</param>
		/// <param name="thick">The thick underline.</param>
		public CefCompositionUnderline(CefRange range, CefColor color, CefColor backgroundColor, bool thick)
			: this()
		{
			Range = range;
			Color = color;
			BackgroundColor = backgroundColor;
			Thick = thick;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CefCompositionUnderline"/> class.
		/// </summary>
		/// <param name="range">The underline character range.</param>
		/// <param name="color">The text color.</param>
		/// <param name="backgroundColor">The background color.</param>
		/// <param name="thick">The thick underline.</param>
		/// <param name="style">The style.</param>
		public CefCompositionUnderline(CefRange range, CefColor color, CefColor backgroundColor, bool thick, CefCompositionUnderlineStyle style)
			: this(range, color, backgroundColor, thick)
		{
			Style = style;
		}

	}
}
