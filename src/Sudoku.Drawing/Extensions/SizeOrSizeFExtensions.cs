namespace System.Drawing;

/// <summary>
/// Provides with extension methods on <see cref="Size"/> or <see cref="SizeF"/>.
/// </summary>
/// <seealso cref="Size"/>
/// <seealso cref="SizeF"/>
public static class SizeOrSizeFExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Size @this)
	{
#if !COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out int width, out int height) => (width, height) = (@this.Width, @this.Height);
#endif
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(SizeF @this)
	{
		/// <summary>
		/// To truncate the size.
		/// </summary>
		/// <returns>The result.</returns>
		public Size Truncate() => new((int)@this.Width, (int)@this.Height);

#if !COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out float width, out float height) => (width, height) = (@this.Width, @this.Height);
#endif
	}

#if COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public static void Deconstruct(this Size @this, out int width, out int height) => (width, height) = (@this.Width, @this.Height);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public static void Deconstruct(this SizeF @this, out float width, out float height)
		=> (width, height) = (@this.Width, @this.Height);
#endif
}
