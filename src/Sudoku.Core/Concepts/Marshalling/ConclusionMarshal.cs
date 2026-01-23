namespace Sudoku.Concepts.Marshalling;

/// <summary>
/// Provides with extension methods on <see cref="Conclusion"/>.
/// </summary>
/// <seealso cref="Conclusion"/>
public static class ConclusionMarshal
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Conclusion[] @this)
	{
		/// <summary>
		/// Converts the <see cref="Conclusion"/> array into a <see cref="ConclusionSet"/> instance.
		/// </summary>
		/// <returns>A <see cref="ConclusionSet"/> result.</returns>
		public ConclusionSet AsSet() => [.. @this];
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ReadOnlyMemory<Conclusion> @this)
	{
		/// <inheritdoc cref="AsSet(Conclusion[])"/>
		public ConclusionSet AsSet() => [.. @this];
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ReadOnlySpan<Conclusion> @this)
	{
		/// <inheritdoc cref="AsSet(Conclusion[])"/>
		public ConclusionSet AsSet() => [.. @this];
	}
}
