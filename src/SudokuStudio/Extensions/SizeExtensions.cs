namespace Windows.Foundation;

/// <summary>
/// Provides with extension methods on <see cref="Size"/>.
/// </summary>
/// <seealso cref="Size"/>
public static class SizeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Size"/>.
	/// </summary>
	extension(Size @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out float width, out float height) => (width, height) = (@this._width, @this._height);
	}
}
