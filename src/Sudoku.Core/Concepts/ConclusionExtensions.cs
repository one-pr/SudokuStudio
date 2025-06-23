namespace Sudoku.Concepts;

/// <summary>
/// Provides with extension methods on <see cref="Conclusion"/>.
/// </summary>
/// <seealso cref="Conclusion"/>
public static class ConclusionExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Conclusion"/>[].
	/// </summary>
	extension(Conclusion[] @this)
	{
		/// <summary>
		/// Converts the <see cref="Conclusion"/> array into a <see cref="ConclusionSet"/> instance.
		/// </summary>
		/// <returns>A <see cref="ConclusionSet"/> result.</returns>
		public ConclusionSet AsSet() => [.. @this];
	}

	/// <summary>
	/// Provides extension members on <see cref="ReadOnlyMemory{T}"/> of <see cref="Conclusion"/>.
	/// </summary>
	extension(ReadOnlyMemory<Conclusion> @this)
	{
		/// <inheritdoc cref="AsSet(Conclusion[])"/>
		public ConclusionSet AsSet() => [.. @this];
	}

	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <see cref="Conclusion"/>.
	/// </summary>
	extension(ReadOnlySpan<Conclusion> @this)
	{
		/// <inheritdoc cref="AsSet(Conclusion[])"/>
		public ConclusionSet AsSet() => [.. @this];
	}
}
