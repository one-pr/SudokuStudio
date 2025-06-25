namespace Sudoku.Ranking;

[TypeImpl(TypeImplFlags.EqualityOperators)]
public partial struct RankPattern : IEquatable<RankPattern>
{
	/// <inheritdoc/>
	[Obsolete($"This method always return false. Ref structs cannot be boxed so argument '{nameof(obj)}' must be a different instance.", false)]
	public override bool Equals(object? obj) => false;

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(in RankPattern other) => Grid == other.Grid && Truths == other.Truths && Links == other.Links;

	/// <inheritdoc/>
	bool IEquatable<RankPattern>.Equals(RankPattern other) => Equals(other);
}
