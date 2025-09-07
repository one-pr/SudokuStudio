namespace Sudoku.MinlexOrder;

/// <summary>
/// Represents a comparer object that checks for min-lex rank of two <see cref="Grid"/> values and compare them.
/// </summary>
/// <seealso cref="Grid"/>
public sealed class GridMinlexComparer : IComparer<Grid>
{
	/// <inheritdoc/>
	public int Compare(Grid x, Grid y)
	{
		var left = x.MinLexGrid.ToString("0");
		var right = y.MinLexGrid.ToString("0");
		return MinlexRanker.GetRank(left).CompareTo(MinlexRanker.GetRank(right));
	}
}
