namespace Sudoku.Concepts.Coordinates.Simplifying;

/// <summary>
/// Represents a biclique (a Cartesian rectangle of cells) used as a compression unit.
/// </summary>
/// <param name="RowMask">A 9-bit mask representing which rows are included.</param>
/// <param name="ColumnMask">A 9-bit mask representing which columns are included.</param>
/// <param name="Covered">A <see cref="CellMap"/> listing the exact cells covered by this biclique (the Cartesian product).</param>
/// <param name="Cost">The heuristic string cost for printing (e.g. "r" + rows + "c" + columns).</param>
/// <param name="Rows">Arrays of row indices used to build readable output.</param>
/// <param name="Columns">Arrays of column indices used to build readable output.</param>
public sealed record Biclique(Mask RowMask, Mask ColumnMask, in CellMap Covered, int Cost, RowIndex[] Rows, ColumnIndex[] Columns);
