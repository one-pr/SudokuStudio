namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents an intersection parser.
/// </summary>
public interface IIntersectionParsable
{
	/// <summary>
	/// The parser method that can creates a list of pairs of <see cref="MinilineBase"/> and <see cref="MinilineResult"/>
	/// via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="MinilineBase"/>
	/// <seealso cref="MinilineResult"/>
	Func<string, ReadOnlySpan<Miniline>> IntersectionParser { get; }
}
