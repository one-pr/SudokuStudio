namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of methods to filter the cells.
/// </summary>
[DebuggerStepThrough]
internal static class GridPredicates
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TGrid"/>,
	/// where <typeparamref name="TGrid"/> satisfies <see langword="unmanaged"/> and <see cref="IGrid{TSelf}"/> constraints.
	/// </summary>
	/// <typeparam name="TGrid">The type of the grid.</typeparam>
	extension<TGrid>(TGrid) where TGrid : unmanaged, IGrid<TGrid>
	{
		/// <summary>
		/// Determines whether the specified cell in the specified grid is a given cell.
		/// </summary>
		/// <param name="g">The grid.</param>
		/// <param name="cell">The cell to be checked.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		public static bool IsGivenCell(in TGrid g, Cell cell) => g.GetState(cell) == CellState.Given;

		/// <summary>
		/// Determines whether the specified cell in the specified grid is a modifiable cell.
		/// </summary>
		/// <inheritdoc cref="extension{TGrid}(TGrid).IsGivenCell(in TGrid, Cell)"/>
		public static bool IsModifiableCell(in TGrid g, Cell cell) => g.GetState(cell) == CellState.Modifiable;

		/// <summary>
		/// Determines whether the specified cell in the specified grid is an empty cell.
		/// </summary>
		/// <inheritdoc cref="extension{TGrid}(TGrid).IsGivenCell(in TGrid, Cell)"/>
		public static bool IsEmptyCell(in TGrid g, Cell cell) => g.GetState(cell) == CellState.Empty;

		/// <summary>
		/// Determines whether the specified cell in the specified grid is a bi-value cell, which means the cell is an empty cell,
		/// and contains and only contains 2 candidates.
		/// </summary>
		/// <inheritdoc cref="extension{TGrid}(TGrid).IsGivenCell(in TGrid, Cell)"/>
		public static bool IsBivalueCell(in TGrid g, Cell cell) => PopCount((uint)g.GetCandidates(cell)) == 2;

		/// <summary>
		/// Checks the existence of the specified digit in the specified cell.
		/// </summary>
		/// <param name="g">The grid.</param>
		/// <param name="cell">The cell to be checked.</param>
		/// <param name="digit">The digit to be checked.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		public static bool ExistsInCandidatesMap(in TGrid g, Cell cell, Digit digit) => g.Exists(cell, digit) is true;

		/// <summary>
		/// Checks the existence of the specified digit in the specified cell, or whether the cell is a value cell, being filled by the digit.
		/// </summary>
		/// <inheritdoc cref="extension{TGrid}(TGrid).ExistsInCandidatesMap(in TGrid, Cell, Digit)"/>
		public static bool ExistsInDigitsMap(in TGrid g, Cell cell, Digit digit) => (g.GetCandidates(cell) >> digit & 1) != 0;

		/// <summary>
		/// Checks whether the cell is a value cell, being filled by the digit.
		/// </summary>
		/// <inheritdoc cref="extension{TGrid}(TGrid).ExistsInCandidatesMap(in TGrid, Cell, Digit)"/>
		public static bool ExistsInValuesMap(in TGrid g, Cell cell, Digit digit) => g.GetDigit(cell) == digit;
	}
}
