namespace Sudoku;

/// <summary>
/// Represents a list of constants of <see cref="TechniqueType"/>.
/// </summary>
/// <seealso cref="TechniqueType"/>
public static class TechniqueTypes
{
	/// <summary>
	/// Indicates all types.
	/// </summary>
	public const TechniqueType All = TechniqueType.Direct | TechniqueType.Snyder | TechniqueType.Advanced;

	/// <summary>
	/// Indicates only for marks.
	/// </summary>
	public const TechniqueType Marks = All & ~TechniqueType.Direct;
}
