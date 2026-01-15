namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents a pair of <see cref="CellMap"/> indicating used and unused cells in a chute.
/// </summary>
/// <param name="includedSegments"><inheritdoc cref="IncludedSegments" path="/summary"/></param>
/// <param name="excluded"><inheritdoc cref="Excluded" path="/summary"/></param>
public readonly struct ChuteStrandMap(CellMap[] includedSegments, in CellMap excluded) :
	IEquatable<ChuteStrandMap>,
	IEqualityOperators<ChuteStrandMap, ChuteStrandMap, bool>
{
	/// <summary>
	/// Indicates cells contained for a certain type of braid mode in a chute.
	/// </summary>
	public readonly CellMap Included = includedSegments[0] | includedSegments[1] | includedSegments[2];

	/// <summary>
	/// Indicates cells not contained for a certain type of braid mode in a chute.
	/// </summary>
	public readonly CellMap Excluded = excluded;


	/// <summary>
	/// Indicates segments of included cells.
	/// </summary>
	public ReadOnlySpan<CellMap> IncludedSegments => includedSegments;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out CellMap included, out CellMap excluded)
		=> (included, excluded) = (Included, Excluded);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out CellMap included, out CellMap excluded, out ReadOnlySpan<CellMap> includedSegments)
	{
		(included, excluded) = this;
		includedSegments = IncludedSegments;
	}

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
