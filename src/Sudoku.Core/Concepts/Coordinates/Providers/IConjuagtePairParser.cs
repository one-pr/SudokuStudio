namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a conjugate pair parser.
/// </summary>
public interface IConjuagtePairParser : IParser
{
	/// <summary>
	/// The parser method that can creates a <see cref="Conjugate"/> list via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Conjugate"/>
	Func<string, ReadOnlySpan<Conjugate>> ConjugateParser { get; }
}
