using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Avalonia.Media;
using CefNet.Input;
using CefNet.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CefNet.Avalonia
{
	public static class CefNetAvaloniaExtensions
	{
#pragma warning disable IDE0060
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Invoke<TEventArgs>(this RoutedEvent routedEvent, Interactive sender, TEventArgs e)
			where TEventArgs : class
		{
			sender.RaiseEvent(e as RoutedEventArgs);
		}
#pragma warning restore IDE0060

		/// <summary>
		/// Converts the specified <see cref="Key"/> into a virtual key.
		/// </summary>
		/// <param name="key">The key code.</param>
		/// <returns>The virtual key code.</returns>
		public static VirtualKeys ToVirtualKey(this Key key)
		{
			if (key >= Key.LeftShift && key <= Key.RightAlt)
				return (VirtualKeys)((key - Key.LeftShift) >> 1) | VirtualKeys.ShiftKey; // VK_SHIFT, VK_CONTROL, VK_MENU
			if (key == Key.System)
				return VirtualKeys.Menu; // VK_MENU
			return (VirtualKeys)KeyInterop.VirtualKeyFromKey(key);
		}

		public static Color ToColor(this CefColor color)
		{
			return Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>
		/// Converts a drag drop effects to the CEF dragging operation mask.
		/// </summary>
		/// <param name="self">The drag drop effects.</param>
		/// <returns></returns>
		public static CefDragOperationsMask ToCefDragOperationsMask(this DragDropEffects self)
		{
			CefDragOperationsMask effects = CefDragOperationsMask.None;
			if (self.HasFlag(DragDropEffects.Copy))
				effects |= CefDragOperationsMask.Copy;
			if (self.HasFlag(DragDropEffects.Move))
				effects |= CefDragOperationsMask.Move;
			if (self.HasFlag(DragDropEffects.Link))
				effects |= CefDragOperationsMask.Link;
			return effects;
		}

		/// <summary>
		/// Converts the CEF dragging operation mask to drag drop effects.
		/// </summary>
		/// <param name="self">The CEF dragging operation mask.</param>
		/// <returns></returns>
		public static DragDropEffects ToDragDropEffects(this CefDragOperationsMask self)
		{
			DragDropEffects effects = DragDropEffects.None;
			if (self.HasFlag(CefDragOperationsMask.Every))
				effects |= DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move;
			if (self.HasFlag(CefDragOperationsMask.Copy))
				effects |= DragDropEffects.Copy;
			if (self.HasFlag(CefDragOperationsMask.Move))
				effects |= DragDropEffects.Move;
			if (self.HasFlag(CefDragOperationsMask.Link))
				effects |= DragDropEffects.Link;
			return effects;
		}

		/// <summary>
		/// Gets the current state of the SHIFT, CTRL, and ALT keys.
		/// </summary>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		/// <returns></returns>
		public static CefEventFlags GetKeyModifiers(this DragEventArgs e)
		{
			CefEventFlags flags = CefEventFlags.None;
			KeyModifiers keyState = e.KeyModifiers;
			if (keyState.HasFlag(KeyModifiers.Shift))
				flags |= CefEventFlags.ShiftDown;
			if (keyState.HasFlag(KeyModifiers.Control))
				flags |= CefEventFlags.ControlDown;
			if (keyState.HasFlag(KeyModifiers.Alt))
				flags |= CefEventFlags.AltDown;
			if (keyState.HasFlag(KeyModifiers.Meta))
				flags |= CefEventFlags.CommandDown;
			return flags;
		}

		/// <summary>
		/// Gets the drag data.
		/// </summary>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		public static CefDragData GetCefDragData(this DragEventArgs e)
		{
			IDataTransfer transfer = e.DataTransfer;
			CefNetDragData ownData = transfer.TryGetValue(CefNetDragData.Format);
			if (ownData != null)
			{
				ownData.Data.ResetFileContents();
				return ownData.Data;
			}

			var dragData = new CefDragData();

			IStorageItem[] files = transfer.TryGetFiles();
			if (files != null)
			{
				foreach (IStorageItem file in files)
				{
					string filePath = file.TryGetLocalPath();
					if (filePath != null)
						dragData.AddFile(filePath.Replace("\\", "/"), Path.GetFileName(filePath));
				}
			}

			bool isUrl = false;
			string s = GetFirstUri(transfer.TryGetValue(CefNetDragData.UriListFormat));
			if (!string.IsNullOrWhiteSpace(s))
			{
				isUrl = true;
				dragData.LinkUrl = s;
			}

			s = transfer.TryGetText();
			if (s != null)
			{
				if (!isUrl && Uri.IsWellFormedUriString(s, UriKind.Absolute))
					dragData.LinkUrl = s;
				dragData.FragmentText = s;
			}

			s = transfer.TryGetValue(CefNetDragData.HtmlFormat);
			if (s != null)
				dragData.FragmentHtml = s;

			return dragData;
		}

		/// <summary>
		/// Returns the first URI of a text/uri-list value, skipping comment lines.
		/// </summary>
		private static string GetFirstUri(string uriList)
		{
			if (uriList is null)
				return null;
			foreach (string line in uriList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (!line.StartsWith("#", StringComparison.Ordinal))
					return line.Trim();
			}
			return null;
		}

	}
}
