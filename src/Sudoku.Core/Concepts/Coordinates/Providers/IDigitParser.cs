namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a digit parser.
/// </summary>
public interface IDigitParser : IParser
{
	/// <summary>
	/// The parser method that can creates a <see cref="Mask"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Mask"/>
	Func<string, Mask> DigitParser { get; }
}
