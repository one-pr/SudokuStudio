namespace Sudoku.Ranking;

public partial struct RankPattern
{
	/// <summary>
	/// Returns a list of links indicating the connection on hatching.
	/// </summary>
	/// <returns>Links.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SpaceSet GetFullLinks()
	{
		GetAssignmentCombinationsCore(out var result);
		return result;
	}
}
