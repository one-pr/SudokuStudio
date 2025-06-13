namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a set of candidates that can be used for filling digits.
/// </summary>
[TypeImpl(
	TypeImplFlags.AllObjectMethods | TypeImplFlags.AllEqualityComparisonOperators,
	OtherModifiersOnEquals = "sealed",
	GetHashCodeBehavior = GetHashCodeBehavior.MakeAbstract,
	ToStringBehavior = ToStringBehavior.MakeAbstract)]
public abstract partial class RankSet :
	IComparable<RankSet>,
	IComparisonOperators<RankSet, RankSet, bool>,
	IEquatable<RankSet>,
	IEqualityOperators<RankSet, RankSet, bool>
{
	/// <summary>
	/// Indicates whether the rank set is truth. Generally the value is negated from <see cref="IsLink"/>.
	/// </summary>
	/// <seealso cref="IsLink"/>
	public bool IsTruth => Type is RankSetType.CellTruth or RankSetType.HouseTruth;

	/// <summary>
	/// Indicates whether the rank set is link. Generally the value is negated from <see cref="IsTruth"/>.
	/// </summary>
	/// <seealso cref="IsTruth"/>
	public bool IsLink => Type is RankSetType.CellLink or RankSetType.HouseLink;

	/// <summary>
	/// Indicates the type.
	/// </summary>
	public abstract RankSetType Type { get; }


	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] RankSet? other);

	/// <summary>
	/// Determine whether the specified assignment is inside the set.
	/// </summary>
	/// <param name="assignment">The assignment.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public abstract bool ContainsAssignment(Candidate assignment);

	/// <inheritdoc/>
	public abstract int CompareTo(RankSet? other);

	/// <summary>
	/// Try to find all possible candidates in the current rank set.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>The candidates.</returns>
	public abstract CandidateMap GetAvailableRange(in Grid grid);

	/// <summary>
	/// Determine whether the specified assignment combination can satisfy the current rank set.
	/// </summary>
	/// <param name="assignments">The assignments.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	protected internal abstract bool IsSatisfied(in CandidateMap assignments);
}
