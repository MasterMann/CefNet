// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace CefNet.Internal
{
	/// <summary>
	/// Detects whether a runtime type overrides a virtual callback, so that CEF callbacks
	/// with only the default implementation can skip the transition into managed code.
	/// </summary>
	internal static class VirtualMethods
	{
		private const BindingFlags DeclaredInstanceMethods = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		private static readonly ConcurrentDictionary<(Type Type, Type BaseType, string Name), bool> Cache = new ConcurrentDictionary<(Type, Type, string), bool>();

		/// <summary>
		/// Returns true if a class derived from <paramref name="baseType"/> in the hierarchy of
		/// <paramref name="instance"/> overrides the virtual method <paramref name="methodName"/>
		/// declared by <paramref name="baseType"/>. The result is cached per runtime type.
		/// </summary>
		public static bool IsOverridden(object instance, Type baseType, string methodName)
		{
			return Cache.GetOrAdd((instance.GetType(), baseType, methodName), static key => FindOverride(key.Type, key.BaseType, key.Name));
		}

		private static bool FindOverride(Type type, Type baseType, string methodName)
		{
			for (; type != null && type != baseType; type = type.BaseType)
			{
				foreach (MethodInfo method in type.GetMethods(DeclaredInstanceMethods))
				{
					if (method.Name == methodName && method.GetBaseDefinition().DeclaringType == baseType)
						return true;
				}
			}
			return false;
		}
	}
}
