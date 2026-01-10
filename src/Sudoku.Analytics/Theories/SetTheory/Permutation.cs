namespace Sudoku.Theories.SetTheory;

/// <summary>
/// Represents a permutation of assigned candidates, indicating such candidates can satisfy specified truths and links.
/// </summary>
/// <param name="Assignments">Indicates the assignments.</param>
/// <param name="LightupLinks">
/// Indicates all light-up links. The value doesn't participate in any comparison operations defined in this type.
/// Check <see cref="LogicReasoner.GetPermutations(ref readonly Logic)"/> to learn more details about light-up links.
/// </param>
/// <seealso cref="LogicReasoner.GetPermutations(ref readonly Logic)"/>
public readonly record struct Permutation(ReadOnlyMemory<Candidate> Assignments, ReadOnlyMemory<Space> LightupLinks) :
	IComparable<Permutation>,
	IComparisonOperators<Permutation, Permutation, bool>,
	IEqualityOperators<Permutation, Permutation, bool>,
	IEnumerable<Candidate>
{
	/// <summary>
	/// Indicates candidates used.
	/// </summary>
	public CandidateMap Map => Assignments.Span.AsCandidateMap();


	/// <inheritdoc/>
	public bool Equals(Permutation other) => Map == other.Map;

	/// <inheritdoc/>
	public override int GetHashCode() => Map.GetHashCode();

	/// <inheritdoc/>
	public int CompareTo(Permutation other) => Map.CompareTo(other.Map);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <inheritdoc cref="ToString(ICandidateMapConverter, IFormatProvider?)"/>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <inheritdoc cref="ToString(ICandidateMapConverter, IFormatProvider?)"/>
	public string ToString(CoordinateConverter converter) => converter.CandidateConverter(Map);

	/// <inheritdoc cref="ToString(ICandidateMapConverter, IFormatProvider?)"/>
	public string ToString(ICandidateMapConverter converter) => ToString(converter, null);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <returns>The string.</returns>
	public string ToString(ICandidateMapConverter converter, IFormatProvider? formatProvider)
	{
		var map = Map;
		return converter.TryFormat(in map, formatProvider, out var result) ? result : throw new FormatException();
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Candidate> GetEnumerator() => new(Assignments.Span);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Assignments.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Candidate> IEnumerable<Candidate>.GetEnumerator() => Assignments.ToArray().AsEnumerable().GetEnumerator();


	/// <inheritdoc/>
	public static bool operator >(Permutation left, Permutation right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(Permutation left, Permutation right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(Permutation left, Permutation right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(Permutation left, Permutation right) => left.CompareTo(right) <= 0;
}
