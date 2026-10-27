using System;
using CefNet.CApi;

namespace CefNet
{
	public partial class CefWindow
	{
		/// <summary>
		/// Creates a new top-level window that uses the specified delegate.
		/// </summary>
		/// <param name="delegate">The window delegate, or null to use the default behavior.</param>
		/// <returns>The new window. It is not shown until <see cref="CefWindow.Show"/> is called.</returns>
		public static unsafe CefWindow CreateTopLevelWindow(CefWindowDelegate @delegate)
		{
			return Wrap(Create, CefNativeApi.cef_window_create_top_level(@delegate != null ? @delegate.GetNativeInstance() : null));
		}
	}

	public partial class CefBrowserView
	{
		/// <summary>
		/// Creates a new browser view and its browser.
		/// </summary>
		/// <param name="client">The client that handles browser callbacks.</param>
		/// <param name="url">The initial URL, or null for a blank page.</param>
		/// <param name="settings">The browser settings.</param>
		/// <param name="extraInfo">Data passed to the render process with the new browser, or null.</param>
		/// <param name="requestContext">The request context, or null for the global context.</param>
		/// <param name="delegate">The browser view delegate, or null.</param>
		public static unsafe CefBrowserView CreateBrowserView(CefClient client, string url, CefBrowserSettings settings,
			CefDictionaryValue extraInfo, CefRequestContext requestContext, CefBrowserViewDelegate @delegate)
		{
			if (settings is null)
				throw new ArgumentNullException(nameof(settings));

			fixed (char* s = url)
			{
				var cstr = new cef_string_t { Str = s, Length = url != null ? url.Length : 0 };
				return Wrap(Create, CefNativeApi.cef_browser_view_create(
					client != null ? client.GetNativeInstance() : null,
					&cstr,
					settings.GetNativeInstance(),
					extraInfo != null ? extraInfo.GetNativeInstance() : null,
					requestContext != null ? requestContext.GetNativeInstance() : null,
					@delegate != null ? @delegate.GetNativeInstance() : null));
			}
		}

		/// <summary>
		/// Returns the browser view that hosts the specified browser, or null if it is not hosted in a view.
		/// </summary>
		public static unsafe CefBrowserView GetForBrowser(CefBrowser browser)
		{
			if (browser is null)
				throw new ArgumentNullException(nameof(browser));
			return Wrap(Create, CefNativeApi.cef_browser_view_get_for_browser(browser.GetNativeInstance()));
		}
	}

	public partial class CefPanel
	{
		/// <summary>
		/// Creates a new panel that uses the specified delegate.
		/// </summary>
		public static unsafe CefPanel CreatePanel(CefPanelDelegate @delegate)
		{
			return Wrap(Create, CefNativeApi.cef_panel_create(@delegate != null ? @delegate.GetNativeInstance() : null));
		}
	}

	public partial class CefScrollView
	{
		/// <summary>
		/// Creates a new scroll view that uses the specified delegate.
		/// </summary>
		public static unsafe CefScrollView CreateScrollView(CefViewDelegate @delegate)
		{
			return Wrap(Create, CefNativeApi.cef_scroll_view_create(@delegate != null ? @delegate.GetNativeInstance() : null));
		}
	}

	public partial class CefTextfield
	{
		/// <summary>
		/// Creates a new text field that uses the specified delegate.
		/// </summary>
		public static unsafe CefTextfield CreateTextfield(CefTextfieldDelegate @delegate)
		{
			return Wrap(Create, CefNativeApi.cef_textfield_create(@delegate != null ? @delegate.GetNativeInstance() : null));
		}
	}

	public partial class CefLabelButton
	{
		/// <summary>
		/// Creates a new label button with the specified text.
		/// </summary>
		public static unsafe CefLabelButton CreateLabelButton(CefButtonDelegate @delegate, string text)
		{
			if (@delegate is null)
				throw new ArgumentNullException(nameof(@delegate));

			fixed (char* s = text)
			{
				var cstr = new cef_string_t { Str = s, Length = text != null ? text.Length : 0 };
				return Wrap(Create, CefNativeApi.cef_label_button_create(@delegate.GetNativeInstance(), &cstr));
			}
		}
	}

	public partial class CefMenuButton
	{
		/// <summary>
		/// Creates a new menu button with the specified text.
		/// </summary>
		public static unsafe CefMenuButton CreateMenuButton(CefMenuButtonDelegate @delegate, string text)
		{
			if (@delegate is null)
				throw new ArgumentNullException(nameof(@delegate));

			fixed (char* s = text)
			{
				var cstr = new cef_string_t { Str = s, Length = text != null ? text.Length : 0 };
				return Wrap(Create, CefNativeApi.cef_menu_button_create(@delegate.GetNativeInstance(), &cstr));
			}
		}
	}

	public partial class CefDisplay
	{
		/// <summary>
		/// Returns the primary display.
		/// </summary>
		public static unsafe CefDisplay GetPrimary()
		{
			return Wrap(Create, CefNativeApi.cef_display_get_primary());
		}

		/// <summary>
		/// Returns the display nearest the specified point.
		/// </summary>
		/// <param name="point">The point in screen coordinates.</param>
		/// <param name="inputPixelCoords">True if <paramref name="point"/> is in pixels instead of DIPs.</param>
		public static unsafe CefDisplay GetNearestPoint(CefPoint point, bool inputPixelCoords)
		{
			return Wrap(Create, CefNativeApi.cef_display_get_nearest_point((cef_point_t*)&point, inputPixelCoords ? 1 : 0));
		}

		/// <summary>
		/// Returns the display that most closely intersects the specified bounds.
		/// </summary>
		/// <param name="bounds">The bounds in screen coordinates.</param>
		/// <param name="inputPixelCoords">True if <paramref name="bounds"/> is in pixels instead of DIPs.</param>
		public static unsafe CefDisplay GetMatchingBounds(CefRect bounds, bool inputPixelCoords)
		{
			return Wrap(Create, CefNativeApi.cef_display_get_matching_bounds((cef_rect_t*)&bounds, inputPixelCoords ? 1 : 0));
		}

		/// <summary>
		/// Returns all displays.
		/// </summary>
		public static unsafe CefDisplay[] GetAll()
		{
			UIntPtr count = CefNativeApi.cef_display_get_count();
			if (count == UIntPtr.Zero)
				return Array.Empty<CefDisplay>();

			var displays = new cef_display_t*[(int)count];
			fixed (cef_display_t** p = displays)
			{
				CefNativeApi.cef_display_get_alls(&count, p);
			}
			var result = new CefDisplay[(int)count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = Wrap(Create, displays[i]);
			}
			return result;
		}

		/// <summary>
		/// Converts a point from DIP screen coordinates to pixel screen coordinates (Windows only).
		/// </summary>
		public static unsafe CefPoint ConvertScreenPointToPixels(CefPoint point)
		{
			return CefNativeApi.cef_display_convert_screen_point_to_pixels((cef_point_t*)&point);
		}

		/// <summary>
		/// Converts a point from pixel screen coordinates to DIP screen coordinates (Windows only).
		/// </summary>
		public static unsafe CefPoint ConvertScreenPointFromPixels(CefPoint point)
		{
			return CefNativeApi.cef_display_convert_screen_point_from_pixels((cef_point_t*)&point);
		}

		/// <summary>
		/// Converts a rectangle from DIP screen coordinates to pixel screen coordinates (Windows only).
		/// </summary>
		public static unsafe CefRect ConvertScreenRectToPixels(CefRect rect)
		{
			return CefNativeApi.cef_display_convert_screen_rect_to_pixels((cef_rect_t*)&rect);
		}

		/// <summary>
		/// Converts a rectangle from pixel screen coordinates to DIP screen coordinates (Windows only).
		/// </summary>
		public static unsafe CefRect ConvertScreenRectFromPixels(CefRect rect)
		{
			return CefNativeApi.cef_display_convert_screen_rect_from_pixels((cef_rect_t*)&rect);
		}
	}
}
