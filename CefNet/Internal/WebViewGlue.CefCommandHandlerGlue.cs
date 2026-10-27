using System;
using System.Runtime.CompilerServices;

namespace CefNet.Internal
{
	public partial class WebViewGlue
	{
		public void CreateOrDestroyCommandGlue()
		{
			if (AvoidOnChromeCommand()
				&& AvoidIsChromeAppMenuItemVisible()
				&& AvoidIsChromeAppMenuItemEnabled()
				&& AvoidIsChromePageActionIconVisible()
				&& AvoidIsChromeToolbarButtonVisible())
			{
				this.CommandGlue = null;
			}
			else if (this.CommandGlue is null)
			{
				this.CommandGlue = new CefCommandHandlerGlue(this);
			}
		}

		/// <summary>
		/// Returns value indicating that call to OnChromeCommand can be avoided.
		/// </summary>
		/// <returns></returns>
		internal bool AvoidOnChromeCommand()
		{
			return !VirtualMethods.IsOverridden(this, typeof(WebViewGlue), nameof(WebViewGlue.OnChromeCommand));
		}

		/// <summary>
		/// Called to execute a Chrome command triggered via menu selection or keyboard
		/// shortcut. Values for |command_id| can be found in the cef_command_ids.h
		/// file. |disposition| provides information about the intended command target.
		/// Return 
		/// </summary>
		/// <param name="commandId">
		/// The command identifier. Values for <paramref name="commandId"/> can be found
		/// in the cef_command_ids.h file.
		/// </param>
		/// <returns>
		/// True if the command was handled or False for the default implementation. 
		/// </returns>
		/// <remarks>
		/// For context menu commands this will be called after <see cref="OnContextMenuCommand"/>.
		/// Only used with the Chrome runtime.
		/// </remarks>
		internal protected virtual bool OnChromeCommand(CefBrowser browser, int commandId, CefWindowOpenDisposition disposition)
		{
			return false;
		}

		internal bool AvoidIsChromeAppMenuItemVisible()
		{
			return !VirtualMethods.IsOverridden(this, typeof(WebViewGlue), nameof(WebViewGlue.IsChromeAppMenuItemVisible));
		}

		/// <summary>
		/// Called to check if a Chrome app menu item should be visible. Only used with Chrome style.
		/// </summary>
		internal protected virtual bool IsChromeAppMenuItemVisible(CefBrowser browser, int commandId)
		{
			return true;
		}

		internal bool AvoidIsChromeAppMenuItemEnabled()
		{
			return !VirtualMethods.IsOverridden(this, typeof(WebViewGlue), nameof(WebViewGlue.IsChromeAppMenuItemEnabled));
		}

		/// <summary>
		/// Called to check if a Chrome app menu item should be enabled. Only used with Chrome style.
		/// </summary>
		internal protected virtual bool IsChromeAppMenuItemEnabled(CefBrowser browser, int commandId)
		{
			return true;
		}

		internal bool AvoidIsChromePageActionIconVisible()
		{
			return !VirtualMethods.IsOverridden(this, typeof(WebViewGlue), nameof(WebViewGlue.IsChromePageActionIconVisible));
		}

		/// <summary>
		/// Called during browser creation to check if a Chrome page action icon should be visible.
		/// Only used with Chrome style.
		/// </summary>
		internal protected virtual bool IsChromePageActionIconVisible(CefChromePageActionIconType iconType)
		{
			return true;
		}

		internal bool AvoidIsChromeToolbarButtonVisible()
		{
			return !VirtualMethods.IsOverridden(this, typeof(WebViewGlue), nameof(WebViewGlue.IsChromeToolbarButtonVisible));
		}

		/// <summary>
		/// Called during browser creation to check if a Chrome toolbar button should be visible.
		/// Only used with Chrome style.
		/// </summary>
		internal protected virtual bool IsChromeToolbarButtonVisible(CefChromeToolbarButtonType buttonType)
		{
			return true;
		}
	}
}
