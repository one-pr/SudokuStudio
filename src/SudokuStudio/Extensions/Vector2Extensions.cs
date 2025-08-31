namespace System.Numerics;

/// <summary>
/// Provides with extension methods on <see cref="Vector2"/>.
/// </summary>
/// <seealso cref="Vector2"/>
public static class Vector2Extensions
{
	/// <summary>
	/// Provides extension members on <see cref="Vector2"/>.
	/// </summary>
	extension(Vector2 @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out float width, out float height) => (width, height) = (@this.X, @this.Y);
	}
}
