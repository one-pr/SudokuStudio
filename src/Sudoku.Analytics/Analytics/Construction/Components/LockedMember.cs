namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a locked member. This type is only used by searching for exocets.
/// </summary>
/// <param name="lockedCells"><inheritdoc cref="LockedCells" path="/summary"/></param>
/// <param name="lockedBlock"><inheritdoc cref="LockedBlock" path="/summary"/></param>
public sealed class LockedMember(in CellMap lockedCells, House lockedBlock) :
	IComponent,
	IEquatable<LockedMember>,
	IEqualityOperators<LockedMember, LockedMember, bool>
{
	/// <summary>
	/// Indicates the locked cells.
	/// </summary>
	public CellMap LockedCells { get; } = lockedCells;

	/// <summary>
	/// Indicates the locked block.
	/// </summary>
	public House LockedBlock { get; } = lockedBlock;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.LockedMember;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out CellMap lockedCells, out House lockedBlock)
		=> (lockedCells, lockedBlock) = (LockedCells, LockedBlock);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as LockedMember);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(LockedCells, LockedBlock);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] LockedMember? other)
		=> other is not null && LockedCells == other.LockedCells && LockedBlock == other.LockedBlock;


	/// <inheritdoc/>
	public static bool operator ==(LockedMember? left, LockedMember? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(LockedMember? left, LockedMember? right) => !(left == right);
}
