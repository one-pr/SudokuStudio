namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a digit converter.
/// </summary>
public interface IDigitConvertible
{
	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified list of digits.
	/// </summary>
	Func<Mask, string> DigitConverter { get; }
}
