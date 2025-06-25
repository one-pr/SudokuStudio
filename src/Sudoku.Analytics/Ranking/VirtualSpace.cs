namespace Sudoku.Ranking;

/// <summary>
/// Represents a virtual set (truth or link) that cannot use a <see cref="Space"/> to include candidates.
/// </summary>
/// <param name="candidates"><inheritdoc cref="Candidates" path="/summary"/></param>
/// <seealso cref="Space"/>
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable
		| TypeImplFlags.AllEqualityComparisonOperators,
	IsLargeStructure = true)]
public readonly partial struct VirtualSpace(in CandidateMap candidates) :
	IComparable<VirtualSpace>,
	IComparisonOperators<VirtualSpace, VirtualSpace, bool>,
	IEquatable<VirtualSpace>,
	IEqualityOperators<VirtualSpace, VirtualSpace, bool>
{
	/// <summary>
	/// Indicates the backing field of candidates.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	private readonly CandidateMap _candidates = candidates;


	/// <summary>
	/// Indicates the candidates.
	/// </summary>
	[UnscopedRef]
	public ref readonly CandidateMap Candidates => ref _candidates;


	/// <summary>
	/// Determine whether the specified assignment combination can satisfy the current rank set.
	/// </summary>
	/// <param name="assignments">The assignments.</param>
	/// <param name="isTruth">Indicates whether the space is used as a truth.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool IsSatisfied(in CandidateMap assignments, bool isTruth)
		=> (assignments & Candidates).Count is var count && (isTruth ? count == 1 : count <= 1);

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public int CompareTo(in VirtualSpace other) => _candidates.CompareTo(other._candidates);

	/// <inheritdoc/>
	public override string ToString() => $$"""{{nameof(VirtualSpace)}} {{{_candidates}}}""";

	/// <inheritdoc/>
	int IComparable<VirtualSpace>.CompareTo(VirtualSpace other) => CompareTo(other);
}
