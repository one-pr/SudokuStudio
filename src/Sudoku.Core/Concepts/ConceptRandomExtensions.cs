namespace Sudoku.Concepts;

/// <summary>
/// Provides with some methods that generates randomized elements or types defined in sudoku basic concepts.
/// </summary>
public static class ConceptRandomExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="random">The current instance.</param>
	extension(Random random)
	{
		/// <summary>
		/// Returns a random integer that is within valid digit range (0..9).
		/// </summary>
		/// <returns>
		/// An integer that represents a valid <see cref="Digit"/>.
		/// </returns>
		public Digit NextDigit() => random.Next(0, 9);

		/// <summary>
		/// Returns a random integer that is within valid cell range (0..81).
		/// </summary>
		/// <returns>
		/// An integer that represents a valid <see cref="Cell"/>.
		/// </returns>
		public Cell NextCell() => random.Next(0, 81);

		/// <summary>
		/// Returns a random integer that is within valid house range (0..27).
		/// </summary>
		/// <returns>
		/// An integer that represents a valid <see cref="House"/>.
		/// </returns>
		public House NextHouse() => random.Next(0, 27);

		/// <summary>
		/// Randomly select one cell from the specified cells.
		/// </summary>
		/// <param name="cells">The cells.</param>
		/// <returns>The chosen cell.</returns>
		public Cell Choose(in CellMap cells) => cells[random.Next(0, cells.Count)];

		/// <summary>
		/// Randomly select one candidate from the specified candidates.
		/// </summary>
		/// <param name="candidates">The candidates.</param>
		/// <returns>The chosen candidate.</returns>
		public Candidate Choose(in CandidateMap candidates) => candidates[random.Next(0, candidates.Count)];

		/// <summary>
		/// Randomly select the specified number of elements from the current collection.
		/// </summary>
		/// <param name="cells">The cells to be chosen.</param>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>The specified number of elements returned, represented as a <see cref="CellMap"/> instance.</returns>
		public CellMap RandomlySelect(in CellMap cells, int count)
		{
			var result = cells.Offsets[..];
			random.Shuffle(result);
			return [.. result[..count]];
		}

		/// <summary>
		/// Randomly select the specified number of elements from the current collection.
		/// </summary>
		/// <param name="cells">The cells to be chosen.</param>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>The specified number of elements returned, represented as a <see cref="CandidateMap"/> instance.</returns>
		public CandidateMap RandomlySelect(in CandidateMap cells, int count)
		{
			var result = cells.Offsets[..];
			random.Shuffle(result);
			return [.. result[..count]];
		}

		/// <summary>
		/// Creates a <see cref="CellMap"/> instance, with the specified number of <see cref="Cell"/>s stored in the collection.
		/// </summary>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>A <see cref="CellMap"/> instance.</returns>
		public CellMap CreateCellMap(int count) => random.RandomlySelect(CellMap.Full, count);

		/// <summary>
		/// Creates a <see cref="CandidateMap"/> instance, with the specified number of <see cref="Candidate"/>s stored in the collection.
		/// </summary>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>A <see cref="CandidateMap"/> instance.</returns>
		public CandidateMap CreateCandidateMap(int count) => random.RandomlySelect(CandidateMap.Full, count);
	}
}
