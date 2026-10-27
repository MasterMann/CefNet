using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace CefNet.Internal
{
	/// <summary>
	/// Presents CEF shared textures in an Avalonia visual on Windows. Each frame is copied on the
	/// CEF UI thread into a texture owned by this renderer, which Avalonia imports as a D3D11
	/// shared texture synchronized with a keyed mutex.
	/// </summary>
	/// <remarks>
	/// Keyed mutex protocol: the copy acquires key 0 and releases key 1; the compositor acquires
	/// key 1 and releases key 0. When the compositor still holds the texture, the frame is dropped
	/// and the browser is invalidated once the compositor is done, so the last frame always appears.
	/// </remarks>
	[SupportedOSPlatform("windows")]
	internal sealed class D3D11SharedTextureRenderer : IDisposable
	{
		private const int WaitTimeout = 0x102;

		private readonly ID3D11Device1 _device;
		private readonly ID3D11DeviceContext _context;
		private readonly ICompositionGpuInterop _interop;
		private readonly Visual _host;
		private readonly CompositionContainerVisual _root;
		private readonly Layer _view;
		private readonly Layer _popup;
		private bool _disposed;

		private D3D11SharedTextureRenderer(Visual host, Compositor compositor, ICompositionGpuInterop interop, ID3D11Device1 device, ID3D11DeviceContext context)
		{
			_host = host;
			_interop = interop;
			_device = device;
			_context = context;
			_root = compositor.CreateContainerVisual();
			// The container keeps a zero size; without this its children are clipped away.
			_root.ClipToBounds = false;
			_view = new Layer(compositor, CefPaintElementType.View);
			_popup = new Layer(compositor, CefPaintElementType.Popup);
			_popup.Visual.Visible = false;
			_root.Children.Add(_view.Visual);
			_root.Children.Add(_popup.Visual);
			ElementComposition.SetElementChildVisual(host, _root);
		}

		/// <summary>
		/// Gets or sets the callback that asks the browser to repaint an element after a dropped frame.
		/// </summary>
		public Action<CefPaintElementType> Invalidate { get; set; }

		/// <summary>
		/// Creates a renderer for the specified visual, or returns null if the compositor cannot
		/// import D3D11 textures with keyed mutex synchronization.
		/// </summary>
		public static async Task<D3D11SharedTextureRenderer> TryCreateAsync(Visual host)
		{
			Compositor compositor = ElementComposition.GetElementVisual(host)?.Compositor;
			if (compositor is null)
				return null;

			ICompositionGpuInterop interop = await compositor.TryGetCompositionGpuInterop();
			if (interop is null || interop.IsLost
				|| !interop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle)
				|| !interop.GetSynchronizationCapabilities(KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle)
					.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.KeyedMutex))
			{
				return null;
			}

			// The texture must live on the adapter the compositor renders with.
			using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
			IDXGIAdapter1 adapter = null;
			for (uint i = 0; factory.EnumAdapters1(i, out IDXGIAdapter1 candidate).Success; i++)
			{
				if (interop.DeviceLuid is null || LuidEquals(candidate.Description1.Luid, interop.DeviceLuid))
				{
					adapter = candidate;
					break;
				}
				candidate.Dispose();
			}
			if (adapter is null)
				return null;

			using (adapter)
			{
				if (D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.BgraSupport, null,
					out ID3D11Device device, out ID3D11DeviceContext context).Failure)
				{
					return null;
				}
				using (device)
				{
					return new D3D11SharedTextureRenderer(host, compositor, interop, device.QueryInterface<ID3D11Device1>(), context);
				}
			}
		}

		/// <summary>
		/// Copies a CEF frame. Called on the CEF UI thread while the shared texture is valid.
		/// </summary>
		public void Paint(CefAcceleratedPaintEventArgs e)
		{
			Layer layer = e.PaintElementType == CefPaintElementType.Popup ? _popup : _view;
			int generation;
			lock (layer)
			{
				if (_disposed)
					return;

				using ID3D11Texture2D source = _device.OpenSharedResource1<ID3D11Texture2D>(e.Info.SharedTextureHandle);
				Texture2DDescription sourceDesc = source.Description;
				layer.EnsureTarget(_device, sourceDesc);

				if (AcquireSync(layer.Mutex, 0, 0) != 0)
				{
					layer.FrameDropped = true;
					return;
				}
				_context.CopyResource(layer.Target, source);
				_context.Flush();
				layer.Mutex.ReleaseSync(1);
				generation = layer.Generation;
			}
			Dispatcher.UIThread.Post(() => PresentAsync(layer, generation), DispatcherPriority.Render);
		}

		/// <summary>
		/// Shows or hides the popup layer at the specified bounds in view coordinates (DIPs).
		/// </summary>
		public void SetPopup(PopupShowEventArgs e)
		{
			Dispatcher.UIThread.Post(() =>
			{
				_popup.Visual.Visible = e.Visible;
				_popup.Visual.Offset = new Vector3D(e.Bounds.X, e.Bounds.Y, 0);
			});
		}

		/// <summary>
		/// Shows the frame copied into the texture of the specified generation. A frame copied into
		/// a texture that was replaced since (after a resize) is skipped; its successor posts its own.
		/// </summary>
		private async void PresentAsync(Layer layer, int generation)
		{
			ICompositionImportedGpuImage image;
			int width, height;
			bool frameDropped;
			lock (layer)
			{
				if (_disposed || layer.Target is null || layer.Generation != generation)
					return;
				if (layer.Image is null || layer.ImageHandle != layer.SharedHandle)
				{
					layer.RetireImage();
					layer.Image = _interop.ImportImage(
						new PlatformHandle(layer.SharedHandle, KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle),
						new PlatformGraphicsExternalImageProperties
						{
							Width = layer.Width,
							Height = layer.Height,
							// D3D11 textures start at the top row; GL-based compositors (ANGLE) otherwise flip them.
							TopLeftOrigin = true,
							Format = layer.Format == Format.R8G8B8A8_UNorm ? PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm : PlatformGraphicsExternalImageFormat.B8G8R8A8UNorm,
						});
					layer.ImageHandle = layer.SharedHandle;
				}
				image = layer.Image;
				width = layer.Width;
				height = layer.Height;
				frameDropped = layer.FrameDropped;
				layer.FrameDropped = false;
			}

			double scaling = global::Avalonia.Controls.TopLevel.GetTopLevel(_host)?.RenderScaling ?? 1.0;
			layer.Visual.Size = new Vector(width / scaling, height / scaling);
			try
			{
				await layer.Surface.UpdateWithKeyedMutexAsync(image, 1, 0);
			}
			catch (Exception) when (_disposed)
			{
				return;
			}
			finally
			{
				layer.DisposeRetired();
			}

			bool repaint;
			lock (layer)
			{
				repaint = frameDropped || layer.FrameDropped;
			}
			if (repaint)
				Invalidate?.Invoke(layer.ElementType);
		}

		public void Dispose()
		{
			lock (_view)
			lock (_popup)
			{
				if (_disposed)
					return;
				_disposed = true;
				ElementComposition.SetElementChildVisual(_host, null);
				_view.Dispose();
				_popup.Dispose();
				_context.Dispose();
				_device.Dispose();
			}
		}

		private static bool LuidEquals(Vortice.Luid luid, byte[] bytes)
		{
			if (bytes.Length != 8)
				return false;
			return luid.LowPart == BitConverter.ToUInt32(bytes, 0) && luid.HighPart == BitConverter.ToInt32(bytes, 4);
		}

		/// <summary>
		/// Calls IDXGIKeyedMutex::AcquireSync and returns its HRESULT, so that WAIT_TIMEOUT (a success
		/// code that the wrapper does not report) can be distinguished from acquiring the mutex.
		/// </summary>
		private static unsafe int AcquireSync(IDXGIKeyedMutex mutex, ulong key, uint milliseconds)
		{
			IntPtr self = mutex.NativePointer;
			// IUnknown (3) + IDXGIObject (4) + IDXGIDeviceSubObject (1) precede AcquireSync.
			var acquireSync = (delegate* unmanaged[Stdcall]<IntPtr, ulong, uint, int>)(*(*(void***)self + 8));
			return acquireSync(self, key, milliseconds);
		}

		private sealed class Layer : IDisposable
		{
			private readonly List<IDisposable> _retired = new List<IDisposable>();

			public Layer(Compositor compositor, CefPaintElementType elementType)
			{
				ElementType = elementType;
				Surface = compositor.CreateDrawingSurface();
				Visual = compositor.CreateSurfaceVisual();
				Visual.Surface = Surface;
			}

			public CefPaintElementType ElementType { get; }
			public CompositionDrawingSurface Surface { get; }
			public CompositionSurfaceVisual Visual { get; }
			public ID3D11Texture2D Target { get; private set; }
			public IDXGIKeyedMutex Mutex { get; private set; }
			public IntPtr SharedHandle { get; private set; }
			public int Width { get; private set; }
			public int Height { get; private set; }
			public Format Format { get; private set; }
			public ICompositionImportedGpuImage Image { get; set; }
			public IntPtr ImageHandle { get; set; }
			public bool FrameDropped { get; set; }
			public int Generation { get; private set; }

			public void EnsureTarget(ID3D11Device1 device, Texture2DDescription source)
			{
				if (Target != null && Width == (int)source.Width && Height == (int)source.Height && Format == source.Format)
					return;

				RetireTarget();
				var desc = new Texture2DDescription
				{
					Width = source.Width,
					Height = source.Height,
					MipLevels = 1,
					ArraySize = 1,
					Format = source.Format,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
					MiscFlags = ResourceOptionFlags.SharedKeyedMutex | ResourceOptionFlags.SharedNTHandle,
				};
				Target = device.CreateTexture2D(desc);
				Mutex = Target.QueryInterface<IDXGIKeyedMutex>();
				using (IDXGIResource1 resource = Target.QueryInterface<IDXGIResource1>())
				{
					SharedHandle = resource.CreateSharedHandle(null, Vortice.DXGI.SharedResourceFlags.Read | Vortice.DXGI.SharedResourceFlags.Write, null);
				}
				Width = (int)source.Width;
				Height = (int)source.Height;
				Format = source.Format;
				Generation++;
			}

			/// <summary>
			/// Keeps the current texture alive until the compositor has switched to its replacement.
			/// </summary>
			private void RetireTarget()
			{
				if (Target is null)
					return;
				lock (_retired)
				{
					_retired.Add(Mutex);
					_retired.Add(Target);
					_retired.Add(new SharedHandleCloser(SharedHandle));
				}
				Target = null;
				Mutex = null;
				SharedHandle = IntPtr.Zero;
			}

			public void RetireImage()
			{
				if (Image is null)
					return;
				ICompositionImportedGpuImage image = Image;
				lock (_retired)
				{
					_retired.Add(new AsyncDisposer(image));
				}
				Image = null;
			}

			public void DisposeRetired()
			{
				IDisposable[] retired;
				lock (_retired)
				{
					retired = _retired.ToArray();
					_retired.Clear();
				}
				foreach (IDisposable item in retired)
					item.Dispose();
			}

			public void Dispose()
			{
				RetireImage();
				RetireTarget();
				DisposeRetired();
			}
		}

		private sealed class SharedHandleCloser : IDisposable
		{
			private IntPtr _handle;

			public SharedHandleCloser(IntPtr handle)
			{
				_handle = handle;
			}

			public void Dispose()
			{
				if (_handle != IntPtr.Zero)
					CloseHandle(_handle);
				_handle = IntPtr.Zero;
			}

			[System.Runtime.InteropServices.DllImport("kernel32.dll")]
			private static extern bool CloseHandle(IntPtr handle);
		}

		private sealed class AsyncDisposer : IDisposable
		{
			private readonly IAsyncDisposable _target;

			public AsyncDisposer(IAsyncDisposable target)
			{
				_target = target;
			}

			public void Dispose()
			{
				_ = _target.DisposeAsync();
			}
		}
	}
}
