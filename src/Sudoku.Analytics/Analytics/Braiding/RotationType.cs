namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents a type of rotation.
/// </summary>
public enum RotationType : byte
{
	/// <summary>
	/// Represents none.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates the type is N (downside rotation).
	/// </summary>
	Downside,

	/// <summary>
	/// Indicates the type is Z (upside rotation).
	/// </summary>
	Upside
}
