namespace Sudoku.Analytics.Ranking.Queuing;

/// <summary>
/// Represents a queue node that is used for covering candidates appeared in truths.
/// </summary>
/// <param name="Link">Indicates the appended link.</param>
/// <param name="UncoveredCandidates">Indicates the uncovered candidates.</param>
/// <param name="Parent">Indicates the parent node.</param>
internal sealed record CoverQueueNode(Space Link, in CandidateMap UncoveredCandidates, CoverQueueNode? Parent)
{
	/// <summary>
	/// Indicates the links.
	/// </summary>
	public SpaceSet Links
	{
		get
		{
			var result = SpaceSet.Empty;
			for (var current = this; current.Parent is not null; current = current.Parent)
			{
				result.Add(current.Link);
			}
			return result;
		}
	}
}
