namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a candidate converter.
/// </summary>
public interface ICandidateConvertible
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified list of candidates.
	/// </summary>
	CandidateMapFormatter CandidateConverter { get; }
}
