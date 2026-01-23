namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Provides with extension methods on <see cref="SpaceSet"/>.
/// </summary>
/// <seealso cref="SpaceSet"/>
public static class SpaceSetExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ReadOnlySpan<Space> @this)
	{
		/// <summary>
		/// Converts <see cref="ReadOnlySpan{T}"/> of <see cref="Space"/> into <see cref="SpaceSet"/>.
		/// </summary>
		/// <returns>The space set instance.</returns>
		public SpaceSet AsSpaceSet() => [.. @this];
	}
}
