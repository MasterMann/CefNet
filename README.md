# CefNet

CefNet is a .NET CLR binding for the Chromium Embedded Framework (CEF).

This repository continues the original CefNet project, whose repository is no longer available,
with support for current CEF releases, .NET 10, and Avalonia 12.

## Getting Started

Since CefNet is a wrapper, you need to have the Chromium Embedded Framework
somewhere on your development system (and redistribute it with your application).

(1) Download the Chromium Embedded Framework from:
	https://cef-builds.spotifycdn.com/index.html

*Note: CefNet is generated for one CEF API version (currently CEF 156, API version 15600; see
[Directory.Build.props](Directory.Build.props)). Use a CEF build that supports that API version,
ideally the same CEF major version.*

Place the CEF resources next to the CEF library: copy or link the contents of
`Resources/` (`icudtl.dat`, `*.pak`, `locales/`) into `Release/`.

(2) Create an instance of the CefNetApplication and initialize it with your settings:
```C#
var settings = new CefSettings();
settings.NoSandbox = true;
settings.MultiThreadedMessageLoop = false; // or true
settings.WindowlessRenderingEnabled = true;
settings.LocalesDirPath = "path_to_cef/locales";
settings.ResourcesDirPath = "path_to_cef";

var app = new CefNetApplication();
app.Initialize("path_to_cef", settings);
```

(3) Add a WebView control to the form of your application.

(4) Run event loop, for example:
```C#
CefNetApplication.Run();
```
*Note: You can use the event loop of the UI-framework you are using.*

(5) You need to explicitly call `CefNetApplication.Shutdown()` from the main
thread before you exit your application:
```C#
app.Shutdown();
app.Dispose();
```

For Avalonia applications, start the application with
`StartWithCefNetApplicationLifetime(args)` instead of `StartWithClassicDesktopLifetime(args)`.
It postpones the shutdown until all browsers have closed and then shuts down CEF.

For more information, see the sample applications.

## Features

* Cross-platform (Windows, Linux; macOS untested)
* Full managed code, generated from the CEF C API
* Controls for Avalonia, WinForms and WPF, plus `WindowlessWebView` for custom integrations
* CEF Views: native browser windows without a UI framework (see the `ViewsApp` sample)
* Shared GPU textures (accelerated paint), described below

### Shared GPU textures

Set `SharedTextureEnabled` before the browser is created (for `WindowlessWebView`, pass it to the
constructor) to receive frames as GPU textures through the `CefAcceleratedPaint` event instead of CPU
bitmaps through `CefPaint`. `CefAcceleratedPaintInfo` exposes a D3D11 shared handle on Windows, an
IOSurface on macOS, and dmabuf planes with a DRM format modifier on Linux. The texture is only valid
during the event: open or import it, copy it into a texture you own, and return.

The Avalonia control presents shared textures on Windows when Avalonia's compositor can import D3D11
textures; elsewhere it keeps using CPU bitmaps. Try it with `AvaloniaApp --shared-texture`.

### Media

The prebuilt CEF distributions play royalty-free formats: VP9, AV1 and Opus, including DASH through
Media Source Extensions players such as dash.js or Shaka Player, and Widevine DRM. H.264, AAC, HEVC
and HLS require a CEF build with `proprietary_codecs=true` and `ffmpeg_branding=Chrome`, which you must
build (and license) yourself.

### Sandbox

CefNet runs the CEF sandbox disabled (`CefSettings.NoSandbox = true`, and `--no-zygote` on Linux):
the .NET runtime cannot run in Chromium's forked Linux zygote, and the Windows sandbox requires a
native bootstrap executable. Use CefNet for trusted content, and restrict navigation (for example in
`OnBeforeBrowse`) when pages may load untrusted content.

## Warning
The API of this project is not frozen and is subject to change.

## Develop

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download).
2. Clone the repository.
3. Download a [CEF package (standard or minimal)](https://cef-builds.spotifycdn.com/index.html) for your platform.
   See [Directory.Build.props](Directory.Build.props) for the required CEF version.
4. Extract it into `cef/windows/` or `cef/linux/` (a symbolic link to an extracted package also works).
5. Copy or link the contents of `cef/<os>/Resources/` into `cef/<os>/Release/`.
6. Build `CefNet.slnx`. On Windows, executables that reference CefNet get a manifest declaring
   Windows 10 compatibility, which Chromium requires (DevTools and Views windows fail without it).
   A project with its own `ApplicationManifest` must include that declaration itself. On Linux, the WinForms and WPF projects compile through Windows targeting
   but run only on Windows.
7. Run the Avalonia sample: `dotnet run --project AvaloniaApp -- --url=https://example.com`.

## Migration to other CEF build
1. Download a [CEF package (standard or minimal)](https://cef-builds.spotifycdn.com/index.html)
   and extract it as described above.
2. Run the generator on Windows, Linux or macOS: `dotnet run --project CefGen`. It parses the C API
   headers in `cef/<os>/include` for the current OS, pins the newest stable API version from
   `cef_api_versions.h`, and rewrites `CefNet/Generated`. Watch the output for errors. The generator
   brings its own minimal C system headers, so it needs no C compiler or Windows SDK, and every host
   produces the same bindings. To read another OS's CEF package, pass `--target=windows|linux|macos`
   and the package path, for example `dotnet run --project CefGen -- --target=windows cef/windows`.
3. Update `CefVersion` in [Directory.Build.props](Directory.Build.props).
4. Build `CefNet.slnx` and update the handwritten code for API changes.
5. Compare the handwritten native structures in `CefNet/CefTypes/CApi` (such as `cef_window_info_t`)
   with the platform headers; the generator does not produce them.

## Similar projects and links

* [CefGlue](https://gitlab.com/xiliumhq/chromiumembedded/cefglue): An alternative .NET CEF wrapper built using P/Invoke.
* [CefSharp](https://github.com/cefsharp/CefSharp): Another .NET CEF wrapper built using C++/CLI.
* [CEF Bitbucket Project](https://bitbucket.org/chromiumembedded/cef/overview): The official CEF issue tracker
* The official CEF Forum: http://magpcss.org/ceforum/
* CEF API Docs: http://magpcss.org/ceforum/apidocs3/index-all.html
