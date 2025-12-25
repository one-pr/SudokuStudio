namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a chute parser.
/// </summary>
public interface IChuteParsable
{
	/// <summary>
	/// The parser method that can creates a <see cref="Chute"/> list via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Chute"/>
	Func<string, ReadOnlySpan<Chute>> ChuteParser { get; }
}
