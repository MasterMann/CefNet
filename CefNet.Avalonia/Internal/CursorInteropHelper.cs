using Avalonia.Input;
using System.Collections.Generic;

namespace CefNet.Internal
{
	/// <summary>
	/// Maps CEF cursor types to cached Avalonia standard cursors.
	/// </summary>
	public static class CursorInteropHelper
	{
		private static readonly Dictionary<StandardCursorType, Cursor> _StdCursors = new Dictionary<StandardCursorType, Cursor>();

		/// <summary>
		/// Returns the Avalonia cursor closest to the specified CEF cursor type.
		/// </summary>
		public static Cursor Create(CefCursorType cursorType)
		{
			return Create(ToStandardCursorType(cursorType));
		}

		public static Cursor Create(StandardCursorType cursorType)
		{
			Cursor cursor;
			lock (_StdCursors)
			{
				if (!_StdCursors.TryGetValue(cursorType, out cursor))
				{
					cursor = new Cursor(cursorType);
					_StdCursors[cursorType] = cursor;
				}
			}
			return cursor;
		}

		private static StandardCursorType ToStandardCursorType(CefCursorType cursorType)
		{
			switch (cursorType)
			{
				case CefCursorType.Cross:
				case CefCursorType.Cell:
					return StandardCursorType.Cross;
				case CefCursorType.Hand:
				case CefCursorType.Grab:
				case CefCursorType.Grabbing:
					return StandardCursorType.Hand;
				case CefCursorType.Ibeam:
				case CefCursorType.Verticaltext:
					return StandardCursorType.Ibeam;
				case CefCursorType.Wait:
					return StandardCursorType.Wait;
				case CefCursorType.Progress:
					return StandardCursorType.AppStarting;
				case CefCursorType.Help:
					return StandardCursorType.Help;
				case CefCursorType.Eastresize:
					return StandardCursorType.RightSide;
				case CefCursorType.Westresize:
					return StandardCursorType.LeftSide;
				case CefCursorType.Northresize:
					return StandardCursorType.TopSide;
				case CefCursorType.Southresize:
					return StandardCursorType.BottomSide;
				case CefCursorType.Northeastresize:
					return StandardCursorType.TopRightCorner;
				case CefCursorType.Northwestresize:
					return StandardCursorType.TopLeftCorner;
				case CefCursorType.Southeastresize:
					return StandardCursorType.BottomRightCorner;
				case CefCursorType.Southwestresize:
					return StandardCursorType.BottomLeftCorner;
				case CefCursorType.Eastwestresize:
				case CefCursorType.Columnresize:
					return StandardCursorType.SizeWestEast;
				case CefCursorType.Northsouthresize:
				case CefCursorType.Rowresize:
					return StandardCursorType.SizeNorthSouth;
				case CefCursorType.Northeastsouthwestresize:
				case CefCursorType.Northwestsoutheastresize:
				case CefCursorType.Move:
				case CefCursorType.Middlepanning:
				case CefCursorType.MiddlePanningVertical:
				case CefCursorType.MiddlePanningHorizontal:
					return StandardCursorType.SizeAll;
				case CefCursorType.Nodrop:
				case CefCursorType.Notallowed:
				case CefCursorType.DndNone:
					return StandardCursorType.No;
				case CefCursorType.DndMove:
					return StandardCursorType.DragMove;
				case CefCursorType.Copy:
				case CefCursorType.DndCopy:
					return StandardCursorType.DragCopy;
				case CefCursorType.Alias:
				case CefCursorType.DndLink:
					return StandardCursorType.DragLink;
				case CefCursorType.None:
					return StandardCursorType.None;
				default:
					return StandardCursorType.Arrow;
			}
		}
	}
}
