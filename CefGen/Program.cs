// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------

using CppAst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CefGen
{
	partial class Program
	{
		private static readonly HashSet<string> IgnoreClasses = new HashSet<string>
		{
			"CefStringTraitsWide",
			"CefStringTraitsUTF8",
			"CefStringTraitsUTF16",
			"CefStringBase",
			"_cef_main_args_t",
			"_cef_window_info_t",
			// Platform-specific layouts; handwritten in CefTypes/CApi.
			"_cef_accelerated_paint_info_t",
			"_cef_accelerated_paint_native_pixmap_plane_info_t",
			"_XEvent",
			"_XDisplay"
		};

		static void Main(string[] args)
		{
			string cefPath = null;
			string outDirPath = null;
			string projectPath = GetProjectPath();

			bool onlyStdCall = false;
			string target = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
				: RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" : "linux";
			foreach (string arg in args)
			{
				if (arg.StartsWith("--"))
				{
					if (arg.StartsWith("--out="))
						outDirPath = arg.Substring(6);
					if (arg.StartsWith("--target="))
						target = arg.Substring(9);
					if (arg == "--stdcall")
						onlyStdCall = true;
					continue;
				}
				else if (cefPath == null)
				{
					cefPath = arg;
				}
			}
			if (target != "windows" && target != "linux" && target != "macos")
			{
				Console.WriteLine("Unknown --target; use windows, linux or macos.");
				Environment.Exit(-1);
				return;
			}
			string osPath = target == "macos" ? "osx" : target;
			if (string.IsNullOrWhiteSpace(cefPath))
				cefPath = Path.Combine(projectPath, "..", "cef",osPath);
			else
				cefPath = cefPath.Trim();
			if (!Directory.Exists(cefPath))
			{
				Console.WriteLine("Could not find the CEF directory.");
				Environment.Exit(-1);
				return;
			}

			if (string.IsNullOrWhiteSpace(outDirPath))
				outDirPath = Path.Combine(projectPath, "..", "CefNet", "Generated");
			else
				outDirPath = outDirPath.Trim();
			if (!Directory.Exists(Path.GetDirectoryName(outDirPath)))
			{
				Console.WriteLine("Could not create the output directory.");
				Environment.Exit(-1);
				return;
			}

			try
			{

				Clean(outDirPath);

				Console.WriteLine("Generate CefWrapperType.cs");
				GenerateCefWrapperTypes(cefPath, outDirPath);

				Console.WriteLine("Generate unsafe types...");
				GenerateFromCHeaders(cefPath, outDirPath, onlyStdCall, target);

				Console.WriteLine("Compile unsafe types...");
				var nativeTypes = new NativeCefApiTypes(outDirPath);
				nativeTypes.Build();

				Console.WriteLine("Generate wrappers...");
				var managedApiBuilder = new ManagedCefApiBuilder(outDirPath);
				managedApiBuilder.Imports.Add("CefNet.CApi");
				managedApiBuilder.Imports.Add("CefNet.Internal");
				managedApiBuilder.EnableCallbackOverrideCheck = true;
				managedApiBuilder.GenerateFrom(nativeTypes);

				Console.WriteLine("Generate glue classes...");
				var cefnetGen = new CefNetCodeGen(outDirPath);
				cefnetGen.Generate();

				Console.WriteLine("Compile wrappers...");
				var managedTypes = new ManagedCefApiTypes(outDirPath);
				managedTypes.Build();

				Console.WriteLine("Complete.");
			}
			catch(SEHException e)
			{
				Console.WriteLine("Error");
				Console.WriteLine($"Error: {e.Message}");
				Console.WriteLine($"ErrorCode: {e.ErrorCode}");
				throw;
			}
			catch(Exception e2)
			{
				Console.WriteLine("Error");
				Console.WriteLine($"Error: {e2.Message}");
				throw;
			}
		}

		private static string GetProjectPath()
		{
			string projectPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
			string rootPath = Path.GetPathRoot(projectPath);
			while (Path.GetFileName(projectPath) != "CefGen")
			{
				if (projectPath == rootPath)
					throw new DirectoryNotFoundException("Could not find the project directory.");
				projectPath = Path.GetDirectoryName(projectPath);
			}
			return projectPath;
		}

		/// <summary>
		/// Reads the newest stable API version and its per-platform hashes from cef_api_versions.h.
		/// </summary>
		private static (int Version, string Windows, string MacOS, string Linux) GetApiVersionInfo(string cefPath)
		{
			string[] lines = File.ReadAllLines(Path.Combine(cefPath, "include", "cef_api_versions.h"));
			string last = lines.Select(line => Regex.Match(line, @"^#define CEF_API_VERSION_LAST CEF_API_VERSION_(\d+)"))
				.FirstOrDefault(m => m.Success)?.Groups[1].Value;
			if (last == null)
			{
				Console.WriteLine("CEF_API_VERSION_LAST not found.");
				Environment.Exit(-1);
			}

			string windows = null, macOS = null, linux = null, platform = null;
			foreach (string line in lines)
			{
				if (line.StartsWith("#if defined(OS_WIN)") || line.StartsWith("#elif defined(OS_WIN)"))
					platform = "win";
				else if (line.StartsWith("#elif defined(OS_MAC)") || line.StartsWith("#if defined(OS_MAC)"))
					platform = "mac";
				else if (line.StartsWith("#elif defined(OS_LINUX)") || line.StartsWith("#if defined(OS_LINUX)"))
					platform = "linux";
				else if (line.StartsWith("#endif"))
					platform = null;

				Match m = Regex.Match(line, "^#define CEF_API_HASH_" + last + " \"([a-z0-9]+)\"");
				if (!m.Success)
					continue;
				if (platform == "win") windows = m.Groups[1].Value;
				else if (platform == "mac") macOS = m.Groups[1].Value;
				else if (platform == "linux") linux = m.Groups[1].Value;
			}
			if (windows == null || macOS == null || linux == null)
			{
				Console.WriteLine("API hashes for version " + last + " not found.");
				Environment.Exit(-1);
			}
			return (int.Parse(last), windows, macOS, linux);
		}

		private static void GenerateFromCHeaders(string basePath, string outDirPath, bool onlyStdCall, string target)
		{
			var options = new CppParserOptions { ParserKind = CppParserKind.C };
			var files = new List<string>(Directory.GetFiles(Path.Combine(basePath, "include", "capi"), "*.h"));
			files.AddRange(Directory.GetFiles(Path.Combine(basePath, "include", "capi", "views"), "*.h"));
			files.Add(Path.Combine(basePath, "include", "cef_version.h"));
			files.Add(Path.Combine(basePath, "include", "cef_api_hash.h"));
			files.Add(Path.Combine(basePath, "include", "cef_api_versions.h"));
			files.Add(Path.Combine(basePath, "include", "cef_version_info.h"));
			options.IncludeFolders.Add(basePath);

			var apiVersion = GetApiVersionInfo(basePath);
			options.Defines.Add("CEF_API_VERSION=" + apiVersion.Version);

			// Parse against the bundled minimal system headers instead of the host's C library or
			// Windows SDK: generation needs no compiler installation, and CppAst keeps typedef names
			// such as size_t and int64_t, which it loses for declarations from system headers.
			options.AdditionalArguments.Add("-nostdinc");
			options.IncludeFolders.Add(Path.Combine(AppContext.BaseDirectory, "SystemHeaders"));
			options.TargetCpu = CppTargetCpu.X86_64;
			switch (target)
			{
				case "windows":
					options.TargetSystem = "windows";
					options.TargetVendor = "pc";
					options.TargetAbi = "msvc";
					break;
				case "macos":
					options.TargetSystem = "darwin";
					options.TargetVendor = "apple";
					options.TargetAbi = string.Empty;
					break;
				default:
					options.TargetSystem = "linux";
					options.TargetVendor = "pc";
					options.TargetAbi = "gnu";
					break;
			}

			var nativeBuild = new NativeCefApiBuilder(onlyStdCall)
			{
				Namespace = "CefNet.CApi",
				BaseDirectory = basePath
			};

			var enumBuild = new NativeCefApiBuilder(onlyStdCall)
			{
				Namespace = "CefNet",
				BaseDirectory = basePath
			};

			CppCompilation compilation = CppParser.ParseFiles(files, options);
			if (compilation.HasErrors)
			{
				foreach(var msg in compilation.Diagnostics.Messages)
					Console.WriteLine(msg);

				Environment.Exit(-1);
				return;
			}
			string cefRoot = Path.GetFullPath(basePath);
			bool IsCefDeclaration(CppElement element)
			{
				string file = element.Span.Start.File;
				return file != null && Path.GetFullPath(file).StartsWith(cefRoot, StringComparison.OrdinalIgnoreCase);
			}
			var cefTypedefs = compilation.Typedefs.Where(IsCefDeclaration).ToList();
			var cefEnums = compilation.Enums.Where(IsCefDeclaration).ToList();

			var aliasResolver = new AliasResolver(cefTypedefs, cefEnums);
			nativeBuild.ResolveCefTypeDef += aliasResolver.HandleResolveEvent;
			enumBuild.ResolveCefTypeDef += aliasResolver.HandleResolveEvent;
			Func<string, string> resolveType = aliasResolver.ResolveNonFail;

			foreach (CppClass @class in compilation.Classes)
			{
				if (!IsCefDeclaration(@class))
					continue;

				if (IgnoreClasses.Contains(@class.Name))
					continue;

				string fileName = aliasResolver.ResolveNonFail(@class.Name);
				using (var csfile = new StreamWriter(Path.Combine(outDirPath, "Native", "Types", fileName + ".cs"), false, Encoding.UTF8))
				//using (var ilfile = new StreamWriter(Path.Combine(outDirPath, "Native", "MSIL", fileName + ".il"), false, Encoding.UTF8))
				{
					//nativeBuild.Format(@class, csfile, ilfile);
					nativeBuild.Format(@class, csfile, null);
					csfile.Flush();
					//ilfile.Flush();
				}
			}

			foreach (CppTypedef typedef in cefTypedefs)
			{
				string name = typedef.Name;
				if (name.StartsWith("cef_string_")
					|| name == "cef_color_t"
					//|| name == "cef_platform_thread_id_t"
					|| name == "cef_platform_thread_handle_t")
				{
					var sb = new StringBuilder();
					try
					{
						var w = new StringWriter(sb);
						nativeBuild.Format(typedef, w);
						w.Flush();
						w.Dispose();
					}
					catch (InvalidOperationException)
					{
						continue;
					}
					string fileName = aliasResolver.ResolveNonFail(typedef.Name);
					File.WriteAllText(Path.Combine(outDirPath, "Native", "Typedefs", fileName + ".cs"), sb.ToString(), Encoding.UTF8);
				}
			}

			foreach (CppEnum @enum in cefEnums)
			{
				string fileName = aliasResolver.ResolveNonFail(@enum.Name);
				using (var csfile = new StreamWriter(Path.Combine(outDirPath, "Managed", "Enums", fileName + ".cs"), false, Encoding.UTF8))
				{
					enumBuild.Format(@enum, csfile);
					csfile.Flush();
				}
			}

			var api = new CefApiClass("CefNativeApi")
			{
				ApiVersion = apiVersion.Version,
				ApiHashWindows = apiVersion.Windows,
				ApiHashMacOS = apiVersion.MacOS,
				ApiHashLinux = apiVersion.Linux
			};
			foreach (CppFunction function in compilation.Functions.Where(IsCefDeclaration).ToList())
				api.ApiFunctions.Add(function);

			using (var csfile = new StreamWriter(Path.Combine(outDirPath, "Native", "CefNativeApi.cs"), false, Encoding.UTF8))
			{
				nativeBuild.Format(api, csfile);
				csfile.Flush();
			}

		}

		private static void GenerateCefWrapperTypes(string cefPath, string outDirPath)
		{
			var cwb = new CefWrapperTypesBuilder
			{
				BaseDirectory = cefPath,
				Namespace = "CefNet.Unsafe"
			};
			var codegen = new CSharpCodeGen();
			using (var csfile = new StreamWriter(Path.Combine(outDirPath, "Managed", "Enums", "CefWrapperType.cs"), false, Encoding.UTF8))
			{
				codegen.Directives.Add("#pragma warning disable 1591");
				codegen.GenerateCode(cwb.Build(), csfile);
				csfile.Flush();
			}
		}

		private static void Clean(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
			Directory.CreateDirectory(path);
			Thread.Sleep(1000);
			Directory.CreateDirectory(Path.Combine(path, "Managed"));
			Directory.CreateDirectory(Path.Combine(path, "Managed", "Types"));
			Directory.CreateDirectory(Path.Combine(path, "Managed", "Enums"));
			Directory.CreateDirectory(Path.Combine(path, "Managed", "Internal"));
			Directory.CreateDirectory(Path.Combine(path, "Native"));
			Directory.CreateDirectory(Path.Combine(path, "Native", "Types"));
			Directory.CreateDirectory(Path.Combine(path, "Native", "Typedefs"));
		}


	}




}
