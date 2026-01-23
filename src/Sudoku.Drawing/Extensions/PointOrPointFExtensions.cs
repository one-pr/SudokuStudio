namespace System.Drawing;

/// <summary>
/// Provides with extension methods on <see cref="Point"/> or <see cref="PointF"/>.
/// </summary>
/// <seealso cref="Point"/>
/// <seealso cref="PointF"/>
public static class PointOrPointFExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Point @this)
	{
#if !COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out int x, out int y) => (x, y) = (@this.X, @this.Y);
#endif
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(PointF @this)
	{
		/// <summary>
		/// To truncate the point.
		/// </summary>
		/// <returns>The result.</returns>
		public Point Truncate() => new((int)@this.X, (int)@this.Y);

#if !COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out float x, out float y) => (x, y) = (@this.X, @this.Y);
#endif
	}

#if COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public static void Deconstruct(this Point @this, out int x, out int y) => (x, y) = (@this.X, @this.Y);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public static void Deconstruct(this PointF @this, out float x, out float y) => (x, y) = (@this.X, @this.Y);
#endif
}
