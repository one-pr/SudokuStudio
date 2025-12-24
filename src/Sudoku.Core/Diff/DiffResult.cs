namespace Sudoku.Diff;

/// <summary>
/// Represents a difference result.
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(NothingChangedDiffResult), (int)DiffType.NothingChanged)]
[JsonDerivedType(typeof(ResetDiffResult), (int)DiffType.Reset)]
[JsonDerivedType(typeof(AddGivenDiffResult), (int)DiffType.AddGiven)]
[JsonDerivedType(typeof(AddModifiableDiffResult), (int)DiffType.AddModifiable)]
[JsonDerivedType(typeof(AddCandidateDiffResult), (int)DiffType.AddCandidate)]
[JsonDerivedType(typeof(RemoveGivenDiffResult), (int)DiffType.RemoveGiven)]
[JsonDerivedType(typeof(RemoveModifiableDiffResult), (int)DiffType.RemoveModifiable)]
[JsonDerivedType(typeof(RemoveCandidateDiffResult), (int)DiffType.RemoveCandidate)]
[JsonDerivedType(typeof(ChangedGivenDiffResult), (int)DiffType.ChangedGiven)]
[JsonDerivedType(typeof(ChangedModifiableDiffResult), (int)DiffType.ChangedModifiable)]
[JsonDerivedType(typeof(ChangedCandidateDiffResult), (int)DiffType.ChangedCandidate)]
public abstract class DiffResult : ICloneable, IEquatable<DiffResult>, IEqualityOperators<DiffResult, DiffResult, bool>
{
	/// <summary>
	/// Indicates the notation prefix.
	/// </summary>
	public virtual string NotationPrefix => Notation[0].ToString();

	/// <summary>
	/// Indicates the simplified notation of the current difference result.
	/// </summary>
	public abstract string Notation { get; }

	/// <summary>
	/// Indicates the type of the difference.
	/// </summary>
	public abstract DiffType Type { get; }

	/// <summary>
	/// Indicates the target type.
	/// </summary>
	protected Type EqualityContract => GetType();


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as DiffResult);

	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] DiffResult? other);

	/// <inheritdoc/>
	public abstract override int GetHashCode();

	/// <inheritdoc cref="ICloneable.Clone"/>
	public abstract DiffResult Clone();

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();


	/// <inheritdoc/>
	public static bool operator ==(DiffResult? left, DiffResult? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(DiffResult? left, DiffResult? right) => !(left == right);
}
