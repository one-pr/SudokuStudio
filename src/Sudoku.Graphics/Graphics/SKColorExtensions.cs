namespace Sudoku.Graphics;

/// <summary>
/// Provides extension members on <see cref="SKColor"/> instances.
/// </summary>
/// <seealso cref="SKColor"/>
public static class SKColorExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(SKColor @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out byte alpha, out byte red, out byte green, out byte blue)
			=> (alpha, red, green, blue) = (@this.Alpha, @this.Red, @this.Green, @this.Blue);
	}
}
