// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------
// Source: include\internal\cef_types_win.h
//         include\internal\cef_types_linux.h
//         include\internal\cef_types_mac.h
// --------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CefNet.CApi
{
	/// <summary>
	/// Structure containing shared texture information for the OnAcceleratedPaint callback.
	/// The layout is platform-specific; see the <c>cef_accelerated_paint_info_*_t</c> structures.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct cef_accelerated_paint_info_t
	{

	}

	/// <summary>
	/// Shared texture information on Windows.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct cef_accelerated_paint_info_windows_t
	{
		/// <summary>
		/// Size of this structure.
		/// </summary>
		public UIntPtr size;

		/// <summary>
		/// Handle for the shared texture. The shared texture is instantiated
		/// without a keyed mutex.
		/// </summary>
		public IntPtr shared_texture_handle;

		/// <summary>
		/// The pixel format of the texture.
		/// </summary>
		public CefColorType format;

		/// <summary>
		/// The extra common info.
		/// </summary>
		public cef_accelerated_paint_info_common_t extra;
	}

	/// <summary>
	/// Shared texture information on macOS.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct cef_accelerated_paint_info_mac_t
	{
		/// <summary>
		/// Size of this structure.
		/// </summary>
		public UIntPtr size;

		/// <summary>
		/// Handle for the shared texture IOSurface.
		/// </summary>
		public IntPtr shared_texture_io_surface;

		/// <summary>
		/// The pixel format of the texture.
		/// </summary>
		public CefColorType format;

		/// <summary>
		/// The extra common info.
		/// </summary>
		public cef_accelerated_paint_info_common_t extra;
	}

	/// <summary>
	/// A plane of a Linux native pixmap.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct cef_accelerated_paint_native_pixmap_plane_t
	{
		/// <summary>
		/// The stride in bytes to be used when accessing the buffer via a memory mapping.
		/// </summary>
		public uint stride;

		/// <summary>
		/// The offset in bytes to be used when accessing the buffer via a memory mapping.
		/// </summary>
		public ulong offset;

		/// <summary>
		/// The size in bytes of the plane, necessary to map the buffer.
		/// </summary>
		public ulong size;

		/// <summary>
		/// File descriptor for the underlying memory object (usually dmabuf).
		/// </summary>
		public int fd;
	}

	/// <summary>
	/// Shared texture information on Linux.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct cef_accelerated_paint_info_linux_t
	{
		/// <summary>
		/// The maximum number of planes (kAcceleratedPaintMaxPlanes).
		/// </summary>
		public const int MaxPlanes = 4;

		/// <summary>
		/// Size of this structure.
		/// </summary>
		public UIntPtr size;

		/// <summary>
		/// Planes of the shared texture, usually file descriptors of dmabufs.
		/// </summary>
		public PlanesArray planes;

		/// <summary>
		/// Plane count.
		/// </summary>
		public int plane_count;

		/// <summary>
		/// Modifier could be used with EGL driver.
		/// </summary>
		public ulong modifier;

		/// <summary>
		/// The pixel format of the texture.
		/// </summary>
		public CefColorType format;

		/// <summary>
		/// The extra common info.
		/// </summary>
		public cef_accelerated_paint_info_common_t extra;

		/// <summary>
		/// The fixed-size plane array.
		/// </summary>
		[InlineArray(MaxPlanes)]
		public struct PlanesArray
		{
			private cef_accelerated_paint_native_pixmap_plane_t _element0;
		}
	}
}
