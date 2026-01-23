namespace System.Drawing;

/// <summary>
/// Provides with extension methods on <see cref="Color"/>.
/// </summary>
/// <seealso cref="Color"/>
public static class ColorExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Color @this)
	{
		/// <summary>
		/// Gets a target <see cref="Color"/> whose <see cref="Color.A"/> value is a quarter of the original one.
		/// </summary>
		public Color QuarterAlpha => Color.FromArgb(@this.A >> 2, @this);


#if !COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out byte a, out byte r, out byte g, out byte b)
			=> (a, r, g, b) = (@this.A, @this.R, @this.G, @this.B);
#endif
	}

#if COMPATIBLE_EXTENSION_DECONSTRUCT_METHOD
	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public static void Deconstruct(this Color @this, out byte a, out byte r, out byte g, out byte b)
		=> (a, r, g, b) = (@this.A, @this.R, @this.G, @this.B);
#endif
}
