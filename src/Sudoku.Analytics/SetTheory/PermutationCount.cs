namespace Sudoku.SetTheory;

/// <summary>
/// Represents a range of number of permutations found in a pattern.
/// Instances of this type will be returned by method <see cref="PatternReasoner.GetAssignementsCount(in Pattern)"/>.
/// </summary>
/// <param name="Min">Indicates the minimum number of a permutation.</param>
/// <param name="Max">Indicates the maximum number of a permutation.</param>
/// <seealso cref="PatternReasoner.GetAssignementsCount(in Pattern)"/>
public readonly record struct AssignmentCountRange(int Min, int Max) :
	IEqualityOperators<AssignmentCountRange, AssignmentCountRange, bool>
{
	/// <summary>
	/// Indicates whether the pattern is stable.
	/// </summary>
	public bool IsStable => Min == Max;

	/// <summary>
	/// Indicates the delta value.
	/// </summary>
	public int Delta => Max - Min;
}
