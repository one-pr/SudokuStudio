namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a house converter.
/// </summary>
public interface IHouseConverter : IConverter
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified list of houses.
	/// </summary>
	Func<HouseMask, string> HouseConverter { get; }
}
