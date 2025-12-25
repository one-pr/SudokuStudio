namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a conclusion converter.
/// </summary>
public interface IConclusionConvertible
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified list of conclusions.
	/// </summary>
	Func<ReadOnlySpan<Conclusion>, string> ConclusionConverter { get; }
}
