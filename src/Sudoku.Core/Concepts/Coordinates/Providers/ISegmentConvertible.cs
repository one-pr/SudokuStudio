namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a segment converter.
/// </summary>
public interface ISegmentConvertible
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified information for a segment list.
	/// </summary>
	Func<SegmentCollection, string> SegmentConverter { get; }
}
