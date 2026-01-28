namespace Sudoku.Graphics;

/// <summary>
/// Represents a mapper instance that projects from pixel points into sudoku elements (coordinates),
/// or from sudoku elements into points.
/// </summary>
/// <param name="size"><inheritdoc cref="Size" path="/summary"/></param>
/// <param name="margin"><inheritdoc cref="Margin" path="/summary"/></param>
public sealed class PointMapper(int size, float margin)
{
	/// <summary>
	/// Indicates size of the picture.
	/// </summary>
	public int Size { get; } = size;

	/// <summary>
	/// Indicates margin of the picture.
	/// </summary>
	public float Margin { get; } = margin;

	/// <summary>
	/// Indicates size of grid.
	/// </summary>
	public float GridSize { get; } = size - 2 * margin;

	/// <summary>
	/// Indicates size of block.
	/// </summary>
	public float BlockSize { get; } = (size - 2 * margin) / 3;

	/// <summary>
	/// Indicates size of cell.
	/// </summary>
	public float CellSize { get; } = (size - 2 * margin) / 9;

	/// <summary>
	/// Indicates size of candidate.
	/// </summary>
	public float CandidateSize { get; } = (size - 2 * margin) / 27;


	/// <summary>
	/// Gets top-left point of a candidate, specified by row and column index (in range 0..28).
	/// </summary>
	/// <param name="rowIndex">The row index.</param>
	/// <param name="columnIndex">The column index.</param>
	/// <returns>The point value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when either argument <paramref name="rowIndex"/> or <paramref name="columnIndex"/> isn't between 0 and 27.
	/// </exception>
	public SKPoint GetCandidateTopLeftPoint(int rowIndex, int columnIndex)
	{
		ArgumentOutOfRangeException.Assert(rowIndex is >= 0 and <= 27);
		ArgumentOutOfRangeException.Assert(columnIndex is >= 0 and <= 27);

		return new(CandidateSize * rowIndex + Margin, CandidateSize * columnIndex + Margin);
	}

	/// <summary>
	/// Returns center point of the specified cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The center point.</returns>
	public SKPoint GetCellCenterPoint(Cell cell) => GetCandidateCenterPoint(cell * 9 + 4);

	/// <summary>
	/// Returns center point of the specified candidate.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	/// <returns>The center point.</returns>
	public SKPoint GetCandidateCenterPoint(Candidate candidate)
	{
		var cell = candidate / 9;
		var digit = candidate % 9;
		var point = GetCandidateTopLeftPoint(cell % 9 * 3 + digit % 3, cell / 9 * 3 + digit / 3);
		return point + new SKPoint(CandidateSize / 2, CandidateSize / 2);
	}
}
