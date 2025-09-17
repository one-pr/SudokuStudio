namespace System.Drawing;

/// <summary>
/// Provides with extension methods on <see cref="Size"/> or <see cref="SizeF"/>.
/// </summary>
/// <seealso cref="Size"/>
/// <seealso cref="SizeF"/>
public static class SizeOrSizeFExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Size"/>.
	/// </summary>
	extension(Size @this)
	{
#if !COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out int width, out int height) => (width, height) = (@this.Width, @this.Height);
#endif
	}

	/// <summary>
	/// Provides extension members on <see cref="SizeF"/>.
	/// </summary>
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
