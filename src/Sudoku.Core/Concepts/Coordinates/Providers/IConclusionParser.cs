namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a conclusion parser.
/// </summary>
public interface IConclusionParser : IParser
{
	/// <summary>
	/// The parser method that can creates a <see cref="Conclusion"/> list via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Conclusion"/>
	Func<string, ConclusionSet> ConclusionParser { get; }
}
