namespace Windows.UI;

/// <summary>
/// Provides with extension methods on <see cref="Color"/>.
/// </summary>
/// <seealso cref="Color"/>
public static class ColorExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Color"/>.
	/// </summary>
	extension(Color @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out byte r, out byte g, out byte b) => (r, g, b) = (@this.R, @this.G, @this.B);

		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out byte a, out byte r, out byte g, out byte b)
			=> (a, r, g, b) = (@this.A, @this.R, @this.G, @this.B);

		/// <summary>
		/// Converts the specified color into equivalent <see cref="SKColor"/> instance.
		/// </summary>
		/// <returns>Final <see cref="SKColor"/> instance.</returns>
		public SKColor AsSKColor()
		{
			var (a, r, g, b) = @this;
			return new(r, g, b, a);
		}

		/// <summary>
		/// Gets an equivalent <see cref="ColorDescriptor"/> instance via the current color.
		/// </summary>
		/// <returns>An <see cref="ColorDescriptor"/> instance.</returns>
		public ColorDescriptor GetIdentifier()
		{
			var (a, r, g, b) = @this;
			return (a, r, g, b);
		}
	}
}
