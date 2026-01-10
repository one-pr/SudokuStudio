namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a candidate parser.
/// </summary>
public interface ICandidateParsable
{
	/// <summary>
	/// The parser method that can creates a <see cref="CandidateMap"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="CandidateMap"/>
	Func<string, CandidateMap> CandidateParser { get; }
}
