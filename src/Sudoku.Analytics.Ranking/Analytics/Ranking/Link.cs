namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a link. A link is a set of candidates that can be filled into it.
/// Different with truths, a link allows no digits filled into it.
/// </summary>
public abstract class Link : RankSet
{
	/// <summary>
	/// Try to find all possible candidates in the current rank set.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>The candidates.</returns>
	[DoesNotReturn]
	public sealed override CandidateMap GetAvailableRange(in Grid grid)
		=> throw new NotSupportedException(SR.ExceptionMessage("LinkNotSupportedToFindAvailableRange"));
}
