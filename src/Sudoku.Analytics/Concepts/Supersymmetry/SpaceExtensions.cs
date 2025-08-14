namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Provides with extension methods on <see cref="Space"/>.
/// </summary>
/// <seealso cref="Space"/>
public static class SpaceExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Space"/>.
	/// </summary>
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
				(true, { Cell: var cell and not -1 }) => IsPow2(assignments.GetDigitsFor(cell)),
				(true, { House: var house, Digit: var digit }) => IsPow2(assignments.GetPositionsFor(house, digit)),
				(_, { Cell: var cell and not -1 }) => PopCount((uint)assignments.GetDigitsFor(cell)) <= 1,
				(_, { House: var house, Digit: var digit }) => PopCount((uint)assignments.GetPositionsFor(house, digit)) <= 1
			};
	}
}
