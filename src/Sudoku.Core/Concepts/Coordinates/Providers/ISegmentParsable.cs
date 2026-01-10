namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a segment parser.
/// </summary>
public interface ISegmentParsable
{
	/// <summary>
	/// The parser method that can creates a list of <see cref="Segment"/> instances.
	/// </summary>
	/// <seealso cref="Segment"/>
	Func<string, SegmentCollection> SegmentParser { get; }
}
