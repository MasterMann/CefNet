using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefNet.CApi;
using CefNet.Internal;

namespace CefNet
{
	public partial class CefFrame
	{
		/// <summary>
		/// Returns the direct sub-frames of the current frame.
		/// </summary>
		/// <returns>The direct sub-frames of the current frame.</returns>
		public CefFrame[] GetFrames()
		{
			string frameid = this.Identifier;
			CefBrowser browser = this.Browser;
			if (browser is null)
				return Array.Empty<CefFrame>();

			var frames = new List<CefFrame>();
			foreach (string fid in browser.GetFrameIdentifiers())
			{
				CefFrame frame = browser.GetFrameByIdentifier(fid);
				if (frame is not null && frame.Parent?.Identifier == frameid)
					frames.Add(frame);
			}
			GC.KeepAlive(browser);
			return frames.ToArray();
		}

#pragma warning disable CS1591

		public override bool Equals(object obj)
		{
			var frame = obj as CefFrame;
			if (frame is null)
				return false;
			return this == frame;
		}

		public override int GetHashCode()
		{
			if (CefApi.UseUnsafeImplementation)
				return _instance.GetHashCode();
			return IsDisposed ? 0 : (Identifier?.GetHashCode() ?? 0);
		}

#pragma warning restore CS1591

		/// <summary>
		/// Tests whether two specified <see cref="CefFrame"/> objects are equivalent.
		/// </summary>
		/// <param name="a">The <see cref="CefFrame"/> that is to the left of the equality operator.</param>
		/// <param name="b">The <see cref="CefFrame"/> that is to the right of the equality operator.</param>
		/// <returns>true if the two <see cref="CefFrame"/> objects are equal; otherwise, false.</returns>
		public static bool operator ==(CefFrame a, CefFrame b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if (!CefApi.UseUnsafeImplementation)
			{
				if (a is null)
					return b is null;
				if (b is null)
					return false;

				try
				{
					return a.Identifier == b.Identifier;
				}
				catch (ObjectDisposedException) { }
			}
			return false;
		}

		/// <summary>
		/// Tests whether two specified <see cref="CefFrame"/> objects are not equal.
		/// </summary>
		/// <param name="a">The first object to compare.</param>
		/// <param name="b">The second object to compare..</param>
		/// <returns>true if the two <see cref="CefFrame"/> objects are not equal; otherwise, false.</returns>
		public static bool operator !=(CefFrame a, CefFrame b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Retrieves this frame&apos;s HTML source as an asynchronous operation.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>The task object that when completed returns the frame source as a string.</returns>
		public Task<string> GetSourceAsync(CancellationToken cancellationToken)
		{
			var visitor = new CefNetStringVisitor();
			this.GetSource(visitor);
			return visitor.GetAsync(cancellationToken);
		}

	}
}
