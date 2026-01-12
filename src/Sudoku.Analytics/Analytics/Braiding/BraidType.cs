namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents a braid type of a chute.
/// </summary>
public enum BraidType
{
	/// <summary>
	/// Represents none.
	/// </summary>
	None = 0,

	/// <summary>
	/// Represents N-Rope pattern (NNN).
	/// </summary>
	NRope,

	/// <summary>
	/// Represents N-Braid pattern (NNZ).
	/// </summary>
	NBraid,

	/// <summary>
	/// Represents Z-Braid pattern (NZZ).
	/// </summary>
	ZBraid,

	/// <summary>
	/// Represents Z-Braid pattern (ZZZ).
	/// </summary>
	ZRope,

	/// <inheritdoc cref="NRope"/>
	NNN = NRope,

	/// <inheritdoc cref="NBraid"/>
	NNZ = NBraid,

	/// <inheritdoc cref="ZBraid"/>
	NZZ = ZBraid,

	/// <inheritdoc cref="ZRope"/>
	ZZZ = ZRope
}
