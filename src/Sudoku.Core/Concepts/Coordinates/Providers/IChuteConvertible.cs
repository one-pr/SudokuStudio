namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a chute converter.
/// </summary>
public interface IChuteConvertible
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified list of chute.
	/// </summary>
	Func<ReadOnlySpan<Chute>, string> ChuteConverter { get; }
}
