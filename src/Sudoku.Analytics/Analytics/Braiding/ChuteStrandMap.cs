namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents a pair of <see cref="CellMap"/> indicating used and unused cells in a chute.
/// </summary>
/// <param name="included"><inheritdoc cref="Included" path="/summary"/></param>
/// <param name="excluded"><inheritdoc cref="Excluded" path="/summary"/></param>
public readonly struct ChuteStrandMap(in CellMap included, in CellMap excluded) :
	IEquatable<ChuteStrandMap>,
	IEqualityOperators<ChuteStrandMap, ChuteStrandMap, bool>
{
	/// <summary>
	/// Indicates cells contained for a certain type of braid mode in a chute.
	/// </summary>
	public readonly CellMap Included = included;

	/// <summary>
	/// Indicates cells not contained for a certain type of braid mode in a chute.
	/// </summary>
	public readonly CellMap Excluded = excluded;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is ChuteStrandMap comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(in ChuteStrandMap other) => Included == other.Included && Excluded == other.Excluded;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Included, Excluded);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> $$"""{{nameof(ChuteStrandMap)}} { {{nameof(Included)}} = {{Included}}, {{nameof(Excluded)}} = {{Excluded}} }""";

	/// <inheritdoc/>
	bool IEquatable<ChuteStrandMap>.Equals(ChuteStrandMap other) => Equals(other);


	/// <inheritdoc/>
	public static bool operator ==(in ChuteStrandMap left, in ChuteStrandMap right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(in ChuteStrandMap left, in ChuteStrandMap right) => !(left == right);

	/// <inheritdoc/>
	static bool IEqualityOperators<ChuteStrandMap, ChuteStrandMap, bool>.operator ==(ChuteStrandMap left, ChuteStrandMap right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<ChuteStrandMap, ChuteStrandMap, bool>.operator !=(ChuteStrandMap left, ChuteStrandMap right)
		=> left != right;
}
