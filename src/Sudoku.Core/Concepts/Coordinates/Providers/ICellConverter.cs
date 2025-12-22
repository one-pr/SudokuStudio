namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a cell converter.
/// </summary>
public interface ICellConverter : IConverter
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified list of cells.
	/// </summary>
	CellMapFormatter CellConverter { get; }
}
