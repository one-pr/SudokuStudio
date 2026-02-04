namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of conclusions.
/// </summary>
public sealed partial class ConclusionSet :
	IAnyAllMethod<ConclusionSet, Conclusion>,
	IContainsMethod<ConclusionSet, Conclusion>,
	IEquatable<ConclusionSet>,
	IEqualityOperators<ConclusionSet, ConclusionSet, bool>,
	IFiniteSet<ConclusionSet, Conclusion>,
	IInfiniteSet<ConclusionSet, Conclusion>,
	ILogicalOperators<ConclusionSet>,
	IReadOnlySet<Conclusion>,
	ISliceMethod<ConclusionSet, Conclusion>,
	IToArrayMethod<ConclusionSet, Conclusion>
{
	/// <summary>
	/// The total length of bits.
	/// </summary>
	private const int BitsCount = HalfBitsCount << 1;

	/// <summary>
	/// The number of candidates exists in a grid in theory.
	/// </summary>
	private const int HalfBitsCount = 9 * 9 * 9;

	/// <summary>
	/// Indicates the total length of bit array.
	/// </summary>
	private const int Length = 45;


	/// <summary>
	/// Represents an instance that includes all possible conclusions that can be produced in a sudoku grid.
	/// </summary>
	/// <remarks>
	/// <b><i>This field should be used as a read-only and an immutable instance; do not modify this instance.</i></b>
	/// </remarks>
	public static readonly ConclusionSet All;

	/// <summary>
	/// Represents an instance that holds all possible assignment cases.
	/// </summary>
	/// <remarks><inheritdoc cref="All" path="/remarks"/></remarks>
	public static readonly ConclusionSet AllAssignments;

	/// <summary>
	/// Represents an instance that holds all possible elimination cases.
	/// </summary>
	/// <remarks><inheritdoc cref="All" path="/remarks"/></remarks>
	public static readonly ConclusionSet AllEliminations;

	/// <summary>
	/// The prime numbers below 100.
	/// </summary>
	private static readonly int[] PrimeNumbers = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97];


	/// <summary>
	/// The internal bit array.
	/// </summary>
	private readonly BitArray _bitArray = new(BitsCount);


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static ConclusionSet()
	{
		All = [];
		All._bitArray.SetAll(true);

		AllAssignments = createCached();
		AllEliminations = All & ~AllAssignments;


		static ConclusionSet createCached()
		{
			var result = Empty;
			for (var cell = 0; cell < 81; cell++)
			{
				for (var digit = 0; digit < 9; digit++)
				{
					result += new Conclusion(Assignment, cell, digit);
				}
			}
			return result;
		}
	}


	/// <summary>
	/// Indicates whether the collection contains any assignment conclusions.
	/// </summary>
	public bool ContainsAssignment => !!(this & Assignments);

	/// <summary>
	/// Indicates whether the collection contains any elimination conclusions.
	/// </summary>
	public bool ContainsElimination => !!(this & Eliminations);

	/// <summary>
	/// Indicates the number of bit array elements.
	/// </summary>
	public int Count => _bitArray.Cardinality;

	/// <summary>
	/// Indicates a list of candidates included in this collection, regardless of type of them.
	/// </summary>
	public CandidateMap Map
	{
		get
		{
			var result = CandidateMap.Empty;
			for (var i = 0; i < HalfBitsCount; i++)
			{
				if (_bitArray[i] || _bitArray[i + HalfBitsCount])
				{
					result.Add(i);
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates a list of candidates that are assignments.
	/// </summary>
	public CandidateMap AssignmentsMap
	{
		get
		{
			var result = CandidateMap.Empty;
			for (var i = 0; i < HalfBitsCount; i++)
			{
				if (_bitArray[i])
				{
					result.Add(i);
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates a list of candidates that are eliminations.
	/// </summary>
	public CandidateMap EliminationMap
	{
		get
		{
			var result = CandidateMap.Empty;
			for (var i = HalfBitsCount; i < BitsCount; i++)
			{
				if (_bitArray[i])
				{
					result.Add(i);
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates assignments of the conclusion set.
	/// </summary>
	public ConclusionSet Assignments => this & AllAssignments;

	/// <summary>
	/// Indicates eliminations of the conclusion set.
	/// </summary>
	public ConclusionSet Eliminations => this & AllEliminations;

	/// <inheritdoc/>
	bool ICollection<Conclusion>.IsReadOnly => false;

	/// <inheritdoc/>
	int ICollection<Conclusion>.Count => Count;


	/// <summary>
	/// Represents an empty instance that has no conclusions.
	/// </summary>
	/// <remarks>
	/// This instance can be used as a mutable object,
	/// meaning you can call any members that may update backing data of the instance.
	/// This type will always create a new instance.
	/// </remarks>
	public static ConclusionSet Empty => [];


	/// <summary>
	/// Try to get n-th element stored in the collection.
	/// </summary>
	/// <param name="index">The desired index to be checked.</param>
	/// <returns>The found <see cref="Conclusion"/> instance at the specified index.</returns>
	/// <exception cref="IndexOutOfRangeException">Throws when the index is out of range.</exception>
	public Conclusion this[int index]
		=> index < 0 || index >= Count
			? throw new IndexOutOfRangeException()
			: new((Mask)_bitArray.GetInternalArrayField().SetBitAt(index));


	/// <summary>
	/// Add a new conclusion into the current collection.
	/// </summary>
	/// <param name="item">A new collection.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the conclusion is successful to be added or not.</returns>
	public bool Add(Conclusion item)
	{
		var (type, cell, digit) = item;
		var rawIndex = (int)type * HalfBitsCount + cell * 9 + digit;
		if (_bitArray[rawIndex])
		{
			return false;
		}

		_bitArray[rawIndex] = true;
		return true;
	}

	/// <summary>
	/// Add a list of conclusions into the collection.
	/// </summary>
	/// <param name="conclusions">The conclusions to be added.</param>
	/// <returns>The number of conclusions succeeded to be added.</returns>
	public int AddRange(params ReadOnlySpan<Conclusion> conclusions)
	{
		var result = 0;
		foreach (var conclusion in conclusions)
		{
			if (Add(conclusion))
			{
				result++;
			}
		}
		return result;
	}

	/// <summary>
	/// Remove a conclusion, represented as a global index (between 0 and 1458), from the collection.
	/// </summary>
	/// <param name="item">The item to be removed.</param>
	/// <returns>
	/// A <see cref="bool"/> result indicating whether the conclusion can be removed from the collection or not.
	/// </returns>
	public bool Remove(Conclusion item)
	{
		var (type, cell, digit) = item;
		var rawIndex = (int)type * HalfBitsCount + cell * 9 + digit;
		if (!_bitArray[rawIndex])
		{
			return false;
		}

		_bitArray[rawIndex] = false;
		return true;
	}

	/// <inheritdoc/>
	public void Clear() => _bitArray.SetAll(false);

	/// <summary>
	/// Only preserve assignments; eliminations will be removed from the current instance.
	/// </summary>
	public void PreserveAssignments() => _bitArray.And((this & AllAssignments)._bitArray);

	/// <summary>
	/// Only preserve eliminations; assignments will be removed from the current instance.
	/// </summary>
	public void PreserveEliminations() => _bitArray.And((this & AllEliminations)._bitArray);

	/// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)"/>
	public void CopyTo(Span<Conclusion> span)
	{
		if (span.Length < Count)
		{
			throw new InvalidOperationException();
		}

		var i = 0;
		foreach (var conclusion in this)
		{
			span[i++] = conclusion;
		}
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ConclusionSet);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] ConclusionSet? other) => other is not null && _bitArray.SequenceEqual(other._bitArray);

	/// <inheritdoc/>
	public bool Contains(Conclusion value) => _bitArray[value.GetHashCode()];

	/// <summary>
	/// Indicates whether the collection contains the specified cell.
	/// </summary>
	/// <param name="cell">The cell to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool ContainsCell(Cell cell)
	{
		for (var bit = 0; bit < 9; bit++)
		{
			if (_bitArray[cell * 9 + bit] || _bitArray[HalfBitsCount + cell * 9 + bit])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicates whether the collection contains the specified candidate.
	/// </summary>
	/// <param name="candidate">The candidate to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool ContainsCandidate(Candidate candidate) => _bitArray[candidate] || _bitArray[candidate + HalfBitsCount];

	/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.Any(Func{TSource, bool})"/>
	public bool Exists(Func<Conclusion, bool> predicate)
	{
		foreach (var conclusion in this)
		{
			if (predicate(conclusion))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.All(Func{TSource, bool})"/>
	public bool TrueForAll(Func<Conclusion, bool> predicate)
	{
		foreach (var conclusion in this)
		{
			if (!predicate(conclusion))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether the conclusion set contains valid conclusions that can be applied to grid.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsWorthFor(in Grid grid)
	{
		foreach (var element in this)
		{
			if (grid.Exists(element.Candidate) is true)
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var (result, i) = (default(HashCode), 0);
		foreach (bool element in _bitArray)
		{
			if (element)
			{
				result.Add(PrimeNumbers[i % PrimeNumbers.Length] * i);
			}
			i++;
		}
		return result.ToHashCode();
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, using the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string representation.</returns>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, using the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string representation.</returns>
	public string ToString(CoordinateConverter converter) => converter.ConclusionConverter(ToArray());

	/// <inheritdoc/>
	public Conclusion[] ToArray()
	{
		var result = new Conclusion[Count];
		for (var (i, z) = ((short)0, 0); i < BitsCount; i++)
		{
			if (_bitArray[i])
			{
				result[z++] = new(i >= HalfBitsCount ? Elimination : Assignment, i % HalfBitsCount);
			}
		}
		return result;
	}

	/// <summary>
	/// Converts the current collection into a <see cref="ReadOnlySpan{T}"/> instance.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of <see cref="Conclusion"/> instance.</returns>
	public ReadOnlySpan<Conclusion> AsSpan() => ToArray();

	/// <summary>
	/// Create an <see cref="Enumerator"/> instance that iterates on all conclusions stored in the current collection.
	/// </summary>
	/// <returns>An <see cref="Enumerator"/> instance as an enumerator.</returns>
	public Enumerator GetEnumerator() => new(_bitArray, 0, BitsCount);

	/// <summary>
	/// Create an <see cref="Enumerator"/> instance that iterates on assignments,
	/// without creating a new instance.
	/// </summary>
	/// <returns>An <see cref="Enumerator"/> instance as an enumerator.</returns>
	public Enumerator EnumerateAssignments() => new(_bitArray, 0, HalfBitsCount);

	/// <summary>
	/// Create an <see cref="Enumerator"/> instance that iterates on eliminations,
	/// without creating a new instance.
	/// </summary>
	/// <returns>An <see cref="Enumerator"/> instance as an enumerator.</returns>
	public Enumerator EnumerateEliminations() => new(_bitArray, HalfBitsCount, BitsCount);

	/// <inheritdoc cref="ISliceMethod{TSelf, TSource}.Slice(int, int)"/>
	public ConclusionSet Slice(int start, int count) => [.. ToArray().AsReadOnlySpan().Slice(start, count)];

	/// <inheritdoc/>
	void ICollection<Conclusion>.Add(Conclusion item) => _ = Add(item);

	/// <inheritdoc/>
	void ICollection<Conclusion>.CopyTo(Conclusion[] array, int arrayIndex) => CopyTo(array.AsSpan()[arrayIndex..]);

	/// <inheritdoc/>
	void ISet<Conclusion>.ExceptWith(IEnumerable<Conclusion> other)
	{
		foreach (var element in other)
		{
			if (Contains(element))
			{
				Remove(element);
			}
		}
	}

	/// <inheritdoc/>
	void ISet<Conclusion>.IntersectWith(IEnumerable<Conclusion> other)
	{
		foreach (var element in other)
		{
			if (!Contains(element))
			{
				Remove(element);
			}
		}
	}

	/// <inheritdoc/>
	void ISet<Conclusion>.SymmetricExceptWith(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		Clear();
		foreach (var c in this & ~p | p & ~this)
		{
			Add(c);
		}
	}

	/// <inheritdoc/>
	void ISet<Conclusion>.UnionWith(IEnumerable<Conclusion> other)
	{
		foreach (var element in other)
		{
			if (!Contains(element))
			{
				Add(element);
			}
		}
	}

	/// <inheritdoc/>
	bool IAnyAllMethod<ConclusionSet, Conclusion>.Any() => Count != 0;

	/// <inheritdoc/>
	bool IAnyAllMethod<ConclusionSet, Conclusion>.Any(Func<Conclusion, bool> predicate) => Exists(predicate);

	/// <inheritdoc/>
	bool IAnyAllMethod<ConclusionSet, Conclusion>.All(Func<Conclusion, bool> predicate) => TrueForAll(predicate);

	/// <inheritdoc/>
	bool ICollection<Conclusion>.Remove(Conclusion item)
	{
		if (!Contains(item))
		{
			return false;
		}

		Remove(item);
		return true;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsProperSubsetOf(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		return (p & this) == this && p != this;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsProperSupersetOf(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		return (this & p) == p && p != this;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsSubsetOf(IEnumerable<Conclusion> other) => ([.. other] & this) == this;

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.IsSupersetOf(IEnumerable<Conclusion> other)
	{
		ConclusionSet p = [.. other];
		return (this & p) == p;
	}

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.Overlaps(IEnumerable<Conclusion> other) => this & [.. other] ? true : false;

	/// <inheritdoc/>
	bool IReadOnlySet<Conclusion>.SetEquals(IEnumerable<Conclusion> other) => this == [.. other];

	/// <inheritdoc/>
	bool ISet<Conclusion>.Add(Conclusion item)
	{
		if (Contains(item))
		{
			return false;
		}

		Add(item);
		return true;
	}

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsProperSubsetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsProperSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsProperSupersetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsProperSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsSubsetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.IsSupersetOf(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).IsSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.Overlaps(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).Overlaps(other);

	/// <inheritdoc/>
	bool ISet<Conclusion>.SetEquals(IEnumerable<Conclusion> other) => ((IReadOnlySet<Conclusion>)this).SetEquals(other);

	/// <inheritdoc/>
	ConclusionSet IFiniteSet<ConclusionSet, Conclusion>.Negate() => ~this;

	/// <inheritdoc/>
	ConclusionSet IInfiniteSet<ConclusionSet, Conclusion>.ExceptWith(ConclusionSet other) => this & ~other;

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Conclusion> IEnumerable<Conclusion>.GetEnumerator() => ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<Conclusion> ISliceMethod<ConclusionSet, Conclusion>.Slice(int start, int count) => Slice(start, count);


	/// <summary>
	/// Try to parse the string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out ConclusionSet? result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <summary>
	/// Try to parse the string into target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse(string? s, CultureInfo culture, [NotNullWhen(true)] out ConclusionSet? result)
		=> TryParse(s, CoordinateParser.GetInstance(culture), out result);

	/// <summary>
	/// Try to parse the string into target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse(string? s, CoordinateParser converter, [NotNullWhen(true)] out ConclusionSet? result)
	{
		try
		{
			if (s is null)
			{
				goto ReturnFalse;
			}
			result = Parse(s, converter);
			return true;
		}
		catch (FormatException)
		{
		}

	ReturnFalse:
		result = null;
		return false;
	}

	/// <summary>
	/// Parses the string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns>The instance.</returns>
	public static ConclusionSet Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <summary>
	/// Parses the string into target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The instance.</returns>
	public static ConclusionSet Parse(string s, CultureInfo culture) => CoordinateParser.GetInstance(culture).ConclusionParser(s);

	/// <summary>
	/// Parses the string into target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>The instance.</returns>
	public static ConclusionSet Parse(string s, CoordinateParser converter) => converter.ConclusionParser(s);


	/// <summary>
	/// Adds a new technique into the current collection.
	/// </summary>
	/// <param name="value">The technique.</param>
	public void operator +=(Conclusion value) => Add(value);

	/// <summary>
	/// Remove a technique from the current collection.
	/// </summary>
	/// <param name="value">The technique.</param>
	public void operator -=(Conclusion value) => Remove(value);

	/// <summary>
	/// Performs bitwise-and operation and assign the value to the current instance.
	/// </summary>
	/// <param name="value">The instance.</param>
	public void operator &=(ConclusionSet value) => _bitArray.And(value._bitArray);

	/// <summary>
	/// Performs bitwise-or operation and assign the value to the current instance.
	/// </summary>
	/// <param name="value">The instance.</param>
	public void operator |=(ConclusionSet value) => _bitArray.Or(value._bitArray);

	/// <summary>
	/// Performs bitwise-exclusive-or operation and assign the value to the current instance.
	/// </summary>
	/// <param name="value">The instance.</param>
	public void operator ^=(ConclusionSet value) => _bitArray.Xor(value._bitArray);


	/// <inheritdoc/>
	public static bool operator !(ConclusionSet value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator true(ConclusionSet value) => value.Count != 0;

	/// <inheritdoc/>
	public static bool operator false(ConclusionSet value) => value.Count == 0;

	/// <inheritdoc/>
	public static bool operator ==(ConclusionSet? left, ConclusionSet? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(ConclusionSet? left, ConclusionSet? right) => !(left == right);

	/// <inheritdoc/>
	public static ConclusionSet operator ~(ConclusionSet value)
	{
		var result = value[..];
		result._bitArray.Not();
		return result;
	}

	/// <inheritdoc/>
	public static ConclusionSet operator &(ConclusionSet left, ConclusionSet right)
	{
		var result = left[..];
		result._bitArray.And(right._bitArray);
		return result;
	}

	/// <inheritdoc/>
	public static ConclusionSet operator |(ConclusionSet left, ConclusionSet right)
	{
		var result = left[..];
		result._bitArray.Or(right._bitArray);
		return result;
	}

	/// <inheritdoc/>
	public static ConclusionSet operator ^(ConclusionSet left, ConclusionSet right)
	{
		var result = left[..];
		result._bitArray.Xor(right._bitArray);
		return result;
	}
}
