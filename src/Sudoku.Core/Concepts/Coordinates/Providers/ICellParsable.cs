namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a cell parser.
/// </summary>
public interface ICellParsable
{
	/// <summary>
	/// The parser method that can creates a <see cref="CellMap"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="CellMap"/>
	Func<string, CellMap> CellParser { get; }
}
