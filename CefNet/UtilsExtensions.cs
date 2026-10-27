using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace CefNet
{
	static class UtilsExtensions
	{
		/// <summary>
		/// Initializes a block of memory at the given location with a given initial value without assuming architecture dependent alignment of the address.
		/// </summary>
		/// <param name="startAddress">The address of the start of the memory block to initialize.</param>
		/// <param name="value">The value to initialize the block to.</param>
		/// <param name="size">The number of bytes to initialize.</param>
		public static unsafe void InitBlock(this IntPtr startAddress, byte value, int size)
		{
			System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned((void*)startAddress, value, (uint)size);
		}

		/// <summary>
		/// Reinterprets the given read-only reference as a reference to a value of type <typeparamref name="TTo"/>.
		/// </summary>
		/// <typeparam name="TFrom">The type of reference to reinterpret.</typeparam>
		/// <typeparam name="TTo">The desired type of the reference.</typeparam>
		/// <param name="source">The read-only reference to reinterpret.</param>
		/// <returns>A reference to a value of type <typeparamref name="TTo"/>.</returns>
		public static ref TTo AsRef<TFrom, TTo>(in TFrom source)
		{
			return ref System.Runtime.CompilerServices.Unsafe.As<TFrom, TTo>(ref System.Runtime.CompilerServices.Unsafe.AsRef(in source));
		}

		/// <summary>
		/// Casts the given object to the specified type.
		/// </summary>
		/// <typeparam name="T">The type which the object will be cast to.</typeparam>
		/// <param name="o">The object to cast.</param>
		/// <returns>The original object, casted to the given type..</returns>
		public static T As<T>(object o) where T : class
		{
			return System.Runtime.CompilerServices.Unsafe.As<T>(o);
		}
	}
}
