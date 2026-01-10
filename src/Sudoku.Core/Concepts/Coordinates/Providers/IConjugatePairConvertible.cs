namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a conjugate pair converter.
/// </summary>
public interface IConjugatePairConvertible
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified conjugate.
	/// </summary>
	Func<ReadOnlySpan<Conjugate>, string> ConjugateConverter { get; }
}
