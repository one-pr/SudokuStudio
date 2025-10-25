namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Provides with extension methods on <see cref="SpaceSet"/>.
/// </summary>
/// <seealso cref="SpaceSet"/>
public static class SpaceSetExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="SpaceSet"/>.
	/// </summary>
	extension(ReadOnlySpan<Space> @this)
	{
		/// <summary>
		/// Converts <see cref="ReadOnlySpan{T}"/> of <see cref="Space"/> into <see cref="SpaceSet"/>.
		/// </summary>
		/// <returns>The space set instance.</returns>
		public SpaceSet AsSpaceSet() => [.. @this];
	}
}
