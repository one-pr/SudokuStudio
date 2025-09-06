using Sudoku.Transformations;

namespace Sudoku.Shuffling;

public partial record struct GenericTransform
{
	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with two digits swapped.
	/// </summary>
	/// <param name="digit1">Indicates the digit 1 to be swapped.</param>
	/// <param name="digit2">Indicates the digit 2 to be swapped.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform SwapDigit(Digit digit1, Digit digit2)
	{
		var digits = (Span<Digit>)[0, 1, 2, 3, 4, 5, 6, 7, 8];
		Unsafe.Swap(ref digits[digit1], ref digits[digit2]);
		return new(0, 0, 0, CantorExpansion.RankRelabeledDigits(digits));
	}

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with two rows swapped.
	/// </summary>
	/// <param name="rowIndex1">Indicates the row 1 to be swapped.</param>
	/// <param name="rowIndex2">Indicates the row 2 to be swapped.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform SwapRow(RowIndex rowIndex1, RowIndex rowIndex2)
	{
		var indices = (Span<RowIndex>)[0, 1, 2, 3, 4, 5, 6, 7, 8];
		Unsafe.Swap(ref indices[rowIndex1], ref indices[rowIndex2]);
		return new(0, CantorExpansion.RankRelabeledLines(indices), 0, 0);
	}

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with two columns swapped.
	/// </summary>
	/// <param name="columnIndex1">Indicates the column 1 to be swapped.</param>
	/// <param name="columnIndex2">Indicates the column 2 to be swapped.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform SwapColumn(ColumnIndex columnIndex1, ColumnIndex columnIndex2)
	{
		var indices = (Span<ColumnIndex>)[0, 1, 2, 3, 4, 5, 6, 7, 8];
		Unsafe.Swap(ref indices[columnIndex1], ref indices[columnIndex2]);
		return new(0, 0, CantorExpansion.RankRelabeledLines(indices), 0);
	}

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with two bands swapped.
	/// </summary>
	/// <param name="bandIndex1">Indicates the band 1 to be swapped.</param>
	/// <param name="bandIndex2">Indicates the band 2 to be swapped.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform SwapBand(int bandIndex1, int bandIndex2)
	{
		var indices = (Span<RowIndex>)[0, 1, 2, 3, 4, 5, 6, 7, 8];
		var a = bandIndex1 * 3;
		var b = bandIndex2 * 3;
		for (var i = 0; i < 3; i++)
		{
			var start = a + i;
			var end = b + i;
			Unsafe.Swap(ref indices[start], ref indices[end]);
		}
		return new(0, CantorExpansion.RankRelabeledLines(indices), 0, 0);
	}

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with two towers swapped.
	/// </summary>
	/// <param name="towerIndex1">Indicates the tower 1 to be swapped.</param>
	/// <param name="towerIndex2">Indicates the tower 2 to be swapped.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform SwapTower(int towerIndex1, int towerIndex2)
	{
		var indices = (Span<ColumnIndex>)[0, 1, 2, 3, 4, 5, 6, 7, 8];
		var a = towerIndex1 * 3;
		var b = towerIndex2 * 3;
		for (var i = 0; i < 3; i++)
		{
			var start = a + i;
			var end = b + i;
			Unsafe.Swap(ref indices[start], ref indices[end]);
		}
		return new(0, 0, CantorExpansion.RankRelabeledLines(indices), 0);
	}

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with specified value indicating whether to transpose.
	/// </summary>
	/// <param name="transpose">Whether to transpose.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform Transpose(bool transpose) => new(transpose ? 1 : 0, 0, 0, 0);

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with specified relabeled rows.
	/// </summary>
	/// <param name="rows">The relabeled rows.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform RelabelRows(params ReadOnlySpan<RowIndex> rows)
		=> new(0, CantorExpansion.RankRelabeledLines(rows), 0, 0);

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with specified relabeled columns.
	/// </summary>
	/// <param name="columns">The relabeled columns.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform RelabelColumns(params ReadOnlySpan<ColumnIndex> columns)
		=> new(0, 0, CantorExpansion.RankRelabeledLines(columns), 0);

	/// <summary>
	/// Creates a <see cref="GenericTransform"/> instance with specified relabeled digits.
	/// </summary>
	/// <param name="digits">The relabeled digits.</param>
	/// <returns>A <see cref="GenericTransform"/> instance.</returns>
	public static GenericTransform RelabelDigits(params ReadOnlySpan<Digit> digits)
		=> new(0, 0, 0, CantorExpansion.RankRelabeledDigits(digits));
}
