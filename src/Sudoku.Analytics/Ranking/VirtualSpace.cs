namespace Sudoku.Ranking;

/// <summary>
/// Represents a virtual set (truth or link) that cannot use a <see cref="Space"/> to include candidates.
/// </summary>
/// <param name="candidates"><inheritdoc cref="Candidates" path="/summary"/></param>
/// <seealso cref="Space"/>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public readonly ref partial struct VirtualSpace(ref readonly CandidateMap candidates) : IEquatable<VirtualSpace>
{
	/// <summary>
	/// Indicates the backing field of candidates.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	private readonly ref readonly CandidateMap _candidates = ref candidates;


	/// <summary>
	/// Indicates the candidates.
	/// </summary>
	public ref readonly CandidateMap Candidates => ref _candidates;


	/// <inheritdoc/>
	[Obsolete($"This method always return false. Ref structs cannot be boxed so argument '{nameof(obj)}' must be a different instance.", false)]
	public override bool Equals([NotNullWhen(true)] object? obj) => false;

	/// <summary>
	/// Determine whether the specified assignment combination can satisfy the current rank set.
	/// </summary>
	/// <param name="assignments">The assignments.</param>
	/// <param name="isTruth">Indicates whether the space is used as a truth.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool IsSatisfied(in CandidateMap assignments, bool isTruth)
		=> (assignments & Candidates).Count is var count && (isTruth ? count == 1 : count <= 1);

	/// <inheritdoc/>
	public override string ToString() => $$"""{{nameof(VirtualSpace)}} {{{_candidates}}}""";
}
