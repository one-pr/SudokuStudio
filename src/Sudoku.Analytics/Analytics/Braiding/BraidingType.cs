namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents a braiding type of a chute.
/// </summary>
[Flags]
public enum BraidingType
{
	/// <summary>
	/// Represents unknown braiding type.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Represents N-Rope pattern (NNN).
	/// </summary>
	NRope = 1 << 0,

	/// <summary>
	/// Represents N-Braid pattern (NNZ).
	/// </summary>
	NBraid = 1 << 1,

	/// <summary>
	/// Represents Z-Braid pattern (NZZ).
	/// </summary>
	ZBraid = 1 << 2,

	/// <summary>
	/// Represents Z-Braid pattern (ZZZ).
	/// </summary>
	ZRope = 1 << 3,

	/// <inheritdoc cref="NRope"/>
	NNN = NRope,

	/// <inheritdoc cref="NBraid"/>
	NNZ = NBraid,

	/// <inheritdoc cref="ZBraid"/>
	NZZ = ZBraid,

	/// <inheritdoc cref="ZRope"/>
	ZZZ = ZRope
}
