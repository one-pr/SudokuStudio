namespace Sudoku.MinlexOrder;

/// <summary>
/// Represents an equality comparer that checks for min-lex transformations.
/// </summary>
public sealed class GridMinlexEqualityComparer : IEqualityComparer<Grid>
{
	/// <inheritdoc/>
	public bool Equals(Grid x, Grid y) => x.MinLexGrid == y.MinLexGrid;

	/// <inheritdoc/>
	public int GetHashCode(Grid obj) => obj.MinLexGrid.GetHashCode();
}
