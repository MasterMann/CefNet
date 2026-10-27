using System;
using CefNet.CApi;

namespace CefNet
{
	/// <summary>
	/// Describes a frame that CEF rendered into a shared GPU texture.
	/// </summary>
	/// <remarks>
	/// The texture belongs to a CEF pool and is only valid during the paint callback that
	/// received this object: open the handle (or import the planes), copy the contents into a
	/// texture you own, and return. After the callback returns, accessing the handle
	/// properties throws <see cref="ObjectDisposedException"/>; the copied frame metadata
	/// stays readable. Handles must not be closed.
	/// </remarks>
	public sealed unsafe class CefAcceleratedPaintInfo
	{
		private cef_accelerated_paint_info_t* _instance;

		internal CefAcceleratedPaintInfo(cef_accelerated_paint_info_t* instance)
		{
			_instance = instance;

			cef_accelerated_paint_info_common_t* extra;
			if (PlatformInfo.IsWindows)
			{
				Format = ((cef_accelerated_paint_info_windows_t*)instance)->format;
				extra = &((cef_accelerated_paint_info_windows_t*)instance)->extra;
			}
			else if (PlatformInfo.IsMacOS)
			{
				Format = ((cef_accelerated_paint_info_mac_t*)instance)->format;
				extra = &((cef_accelerated_paint_info_mac_t*)instance)->extra;
			}
			else
			{
				var linux = (cef_accelerated_paint_info_linux_t*)instance;
				Format = linux->format;
				Modifier = linux->modifier;
				PlaneCount = Math.Clamp(linux->plane_count, 0, cef_accelerated_paint_info_linux_t.MaxPlanes);
				extra = &linux->extra;
			}

			Timestamp = extra->timestamp;
			CodedSize = extra->coded_size;
			VisibleRect = extra->visible_rect;
			ContentRect = extra->content_rect;
			SourceSize = extra->has_source_size != 0 ? extra->source_size : (CefSize?)null;
			CaptureUpdateRect = extra->has_capture_update_rect != 0 ? extra->capture_update_rect : (CefRect?)null;
			RegionCaptureRect = extra->has_region_capture_rect != 0 ? extra->region_capture_rect : (CefRect?)null;
			CaptureCounter = extra->has_capture_counter != 0 ? extra->capture_counter : (ulong?)null;
		}

		/// <summary>
		/// Gets a value indicating whether the shared texture can still be accessed
		/// (true only during the paint callback).
		/// </summary>
		public bool IsValid
		{
			get { return _instance != null; }
		}

		/// <summary>
		/// Gets the pixel format of the texture.
		/// </summary>
		public CefColorType Format { get; }

		/// <summary>
		/// Gets the shared texture handle: on Windows, an NT handle to a D3D11 texture
		/// (open it with <c>ID3D11Device1::OpenSharedResource1</c> or
		/// <c>ID3D12Device::OpenSharedHandle</c>); on macOS, an <c>IOSurfaceRef</c>.
		/// On Linux, this is <see cref="IntPtr.Zero"/>; use <see cref="GetPlane"/>.
		/// </summary>
		public IntPtr SharedTextureHandle
		{
			get
			{
				cef_accelerated_paint_info_t* instance = GetInstance();
				if (PlatformInfo.IsWindows)
					return ((cef_accelerated_paint_info_windows_t*)instance)->shared_texture_handle;
				if (PlatformInfo.IsMacOS)
					return ((cef_accelerated_paint_info_mac_t*)instance)->shared_texture_io_surface;
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Gets the number of native pixmap planes (Linux only; zero elsewhere).
		/// </summary>
		public int PlaneCount { get; }

		/// <summary>
		/// Gets the DRM format modifier of the native pixmap (Linux only), for example
		/// for <c>EGL_EXT_image_dma_buf_import_modifiers</c> or
		/// <c>VK_EXT_image_drm_format_modifier</c>.
		/// </summary>
		public ulong Modifier { get; }

		/// <summary>
		/// Returns a plane of the native pixmap (Linux only).
		/// </summary>
		/// <param name="index">The plane index, less than <see cref="PlaneCount"/>.</param>
		public CefAcceleratedPaintPlane GetPlane(int index)
		{
			if ((uint)index >= (uint)PlaneCount)
				throw new ArgumentOutOfRangeException(nameof(index));
			cef_accelerated_paint_native_pixmap_plane_t plane = ((cef_accelerated_paint_info_linux_t*)GetInstance())->planes[index];
			return new CefAcceleratedPaintPlane(plane.fd, plane.stride, plane.offset, plane.size);
		}

		/// <summary>
		/// Gets the timestamp of the frame in microseconds since capture start.
		/// </summary>
		public ulong Timestamp { get; }

		/// <summary>
		/// Gets the full dimensions of the texture.
		/// </summary>
		public CefSize CodedSize { get; }

		/// <summary>
		/// Gets the visible area of the texture.
		/// </summary>
		public CefRect VisibleRect { get; }

		/// <summary>
		/// Gets the region of the texture the capturer populates.
		/// </summary>
		public CefRect ContentRect { get; }

		/// <summary>
		/// Gets the full size of the source frame, if reported.
		/// </summary>
		public CefSize? SourceSize { get; }

		/// <summary>
		/// Gets the updated area of the frame, if reported.
		/// </summary>
		public CefRect? CaptureUpdateRect { get; }

		/// <summary>
		/// Gets where the frame contents originate from when region capture is used, if reported.
		/// </summary>
		public CefRect? RegionCaptureRect { get; }

		/// <summary>
		/// Gets the incremental frame counter, if reported.
		/// </summary>
		public ulong? CaptureCounter { get; }

		/// <summary>
		/// Gets the native structure; valid only during the paint callback.
		/// </summary>
		public cef_accelerated_paint_info_t* GetNativeInstance()
		{
			return GetInstance();
		}

		internal void Invalidate()
		{
			_instance = null;
		}

		private cef_accelerated_paint_info_t* GetInstance()
		{
			cef_accelerated_paint_info_t* instance = _instance;
			if (instance == null)
				throw new ObjectDisposedException(nameof(CefAcceleratedPaintInfo), "The shared texture is only valid during the paint callback.");
			return instance;
		}
	}

	/// <summary>
	/// A plane of a Linux native pixmap (usually a dmabuf) delivered by accelerated paint.
	/// </summary>
	public readonly struct CefAcceleratedPaintPlane
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CefAcceleratedPaintPlane"/> structure.
		/// </summary>
		public CefAcceleratedPaintPlane(int fd, uint stride, ulong offset, ulong size)
		{
			FileDescriptor = fd;
			Stride = stride;
			Offset = offset;
			Size = size;
		}

		/// <summary>
		/// Gets the file descriptor of the underlying memory object. It is owned by CEF;
		/// duplicate it (<c>dup</c>) to keep it after the callback.
		/// </summary>
		public int FileDescriptor { get; }

		/// <summary>
		/// Gets the stride in bytes.
		/// </summary>
		public uint Stride { get; }

		/// <summary>
		/// Gets the offset in bytes.
		/// </summary>
		public ulong Offset { get; }

		/// <summary>
		/// Gets the size in bytes.
		/// </summary>
		public ulong Size { get; }
	}
}
