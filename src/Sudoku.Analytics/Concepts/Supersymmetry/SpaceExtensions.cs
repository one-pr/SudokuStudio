namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Provides with extension methods on <see cref="Space"/>.
/// </summary>
/// <seealso cref="Space"/>
public static class SpaceExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Space @this)
	{
		/// <summary>
		/// Determine whether the specified assignment combination can satisfy the current rank set.
		/// </summary>
		/// <param name="assignments">The assignments.</param>
		/// <param name="isTruth">Indicates whether the current space is as a truth.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		public bool IsSatisfied(in CandidateMap assignments, bool isTruth)
			=> (isTruth, @this) switch
			{
				(true, { Cell: var cell and not -1 }) => BitOperations.IsPow2(assignments.GetDigitsFor(cell)),
				(true, { HouseDigit: (var house and not -1, var digit and not -1) }) => BitOperations.IsPow2(assignments.GetPositionsFor(house, digit)),
				(_, { Cell: var cell and not -1 }) => BitOperations.PopCount((uint)assignments.GetDigitsFor(cell)) <= 1,
				(_, { HouseDigit: (var house and not -1, var digit and not -1) }) => BitOperations.PopCount((uint)assignments.GetPositionsFor(house, digit)) <= 1
			};
	}
}
