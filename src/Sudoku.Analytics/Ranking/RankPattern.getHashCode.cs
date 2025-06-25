namespace Sudoku.Ranking;

public partial struct RankPattern
{
	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Grid, Truths, Links);
}
