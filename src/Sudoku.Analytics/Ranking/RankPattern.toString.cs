namespace Sudoku.Ranking;

public partial struct RankPattern
{
	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"T{Truths.Count} = {Truths}, L{Links.Count} = {Links}";

	/// <summary>
	/// Gets the full string of the current pattern, including its details (rank, eliminations and so on).
	/// </summary>
	/// <returns>The string.</returns>
	public unsafe string ToFullString()
	{
		var combinations = GetAssignmentCombinations();
		return string.Format(
			SR.Get("RankInfo"),
			Grid.ToString("@:"),
			ToString(),
			combinations.Length,
			GetRankCore(combinations)?.ToString() ?? SR.Get("UnstableRank"),
			GetEliminationsCore(combinations).ToString(),
			GetRank0LinksCore(combinations).ToString(),
			SR.Get(GetIsRank0PatternCore(combinations) ? "IsRank0Pattern" : "IsNotRank0Pattern")
		);
	}
}
