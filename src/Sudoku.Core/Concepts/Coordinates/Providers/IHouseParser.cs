namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a house parser.
/// </summary>
public interface IHouseParser : IParser
{
	/// <summary>
	/// The parser method that can creates a <see cref="HouseMask"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="HouseMask"/>
	Func<string, HouseMask> HouseParser { get; }
}
