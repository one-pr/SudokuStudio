namespace Sudoku.Theories.DeadlyPatternTheory;

/// <summary>
/// Represents a type that adjust checking rules for deadly pattern.
/// </summary>
public readonly struct DeadlyPatternCheckingOptions
{
	/// <summary>
	/// Represents default option.
	/// </summary>
	public static readonly DeadlyPatternCheckingOptions Default = default;


	/// <summary>
	/// <para>
	/// Indicates whether an exception will be thrown if maximum solutions count is reached,
	/// if property <see cref="LimitSolutionsCount"/> is specified with non-zero value.
	/// </para>
	/// <para>By default the value is <see langword="false"/>.</para>
	/// </summary>
	public bool ThrowExceptionIfMaximumSolutionsCountReached { get; init; }

	/// <summary>
	/// <para>
	/// Indicates whether the checker ignores a digit can only be placed at one position in a house of a pattern or not.
	/// </para>
	/// <para>By default the value is <see langword="false"/>.</para>
	/// </summary>
	public bool IgnorePotentialSinglePositionInHouse { get; init; }

	/// <summary>
	/// <para>
	/// Indicates the number of solutions that the checking API can search for.
	/// If a pattern has more number of solutions than desired value specified,
	/// The method will return instead of continuing checking.
	/// </para>
	/// <para>By default the value is 0.</para>
	/// </summary>
	public int LimitSolutionsCount { get; init; }
}
