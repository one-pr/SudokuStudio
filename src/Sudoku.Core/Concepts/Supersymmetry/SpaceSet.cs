namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Represents a set of <see cref="Space"/> instances.
/// </summary>
/// <remarks>
/// <para><include file="../../global-doc-comments.xml" path="/g/large-structure"/></para>
/// </remarks>
/// <seealso cref="Space"/>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.AllEqualityComparisonOperators, IsLargeStructure = true)]
public partial struct SpaceSet :
	IAdditionOperators<SpaceSet, Space, SpaceSet>,
	IAnyAllMethod<SpaceSet, Space>,
	IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>,
	ICollection<Space>,
	IComparable<SpaceSet>,
	IComparisonOperators<SpaceSet, SpaceSet, bool>,
	IEnumerable<Space>,
	IEquatable<SpaceSet>,
	IEqualityOperators<SpaceSet, SpaceSet, bool>,
	IFiniteSet<SpaceSet, Space>,
	IInfiniteSet<SpaceSet, Space>,
	ILogicalOperators<SpaceSet>,
	IParsable<SpaceSet>,
	IReadOnlyList<Space>,
	IReadOnlySet<Space>,
	ISet<Space>,
	ISubtractionOperators<SpaceSet, Space, SpaceSet>
{
	/// <summary>
	/// Indicates the empty instance.
	/// </summary>
	public static readonly SpaceSet Empty;


	/// <summary>
	/// Indicates the buffer entry field.
	/// </summary>
	private BackingBuffer _field;


	/// <inheritdoc/>
	public readonly int Count => _field[0].Count + _field[1].Count + _field[2].Count + _field[3].Count;

	/// <inheritdoc/>
	readonly bool ICollection<Space>.IsReadOnly => false;


	/// <inheritdoc/>
	public readonly Space this[int index]
	{
		get
		{
			var currentIndex = -1;
			for (var i = 0; i < 4; i++)
			{
				foreach (var bit in _field[i])
				{
					if (++currentIndex == index)
					{
						var a = bit / 9;
						var b = bit % 9;
						return i switch
						{
							0 => Space.RowColumn(b, a),
							1 => Space.BlockDigit(a, b),
							2 => Space.RowDigit(a, b),
							_ => Space.ColumnDigit(a, b)
						};
					}
				}
			}
			throw new IndexOutOfRangeException();
		}
	}


	/// <inheritdoc/>
	public readonly void CopyTo(Space[] array, int arrayIndex)
	{
		ArgumentException.ThrowIfAssertionFailed(array.Length >= Count);

		ToArray().AsReadOnlySpan().CopyTo(array.AsSpan()[arrayIndex..]);
	}

	/// <inheritdoc/>
	public readonly bool Contains(Space space)
	{
		var (type, primary, secondary) = space;
		var id = primary * 9 + secondary;
		return _field[(int)type].Contains(id);
	}

	/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.Any(Func{TSource, bool})"/>
	public readonly bool Exists(Func<Space, bool> predicate)
	{
		foreach (var space in this)
		{
			if (predicate(space))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.All(Func{TSource, bool})"/>
	public readonly bool TrueForAll(Func<Space, bool> predicate)
	{
		foreach (var space in this)
		{
			if (!predicate(space))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(in SpaceSet other) => _field[..].SequenceEqual(other._field[..]);

	/// <inheritdoc/>
	public override readonly int GetHashCode() => HashCode.Combine(_field[0], _field[1], _field[2], _field[3]);

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public readonly int CompareTo(in SpaceSet other)
	{
		var e1 = new AnonymousSpanEnumerator<Space>(ToArray());
		var e2 = new AnonymousSpanEnumerator<Space>(other.ToArray());
		while (e1.MoveNext() && e2.MoveNext())
		{
			var c1 = e1.Current;
			var c2 = e2.Current;
			if (c1.CompareTo(c2) is var r1 and not 0)
			{
				return r1;
			}
		}
		return 0;
	}

	/// <summary>
	/// Returns a <see cref="BitmapEnumerator"/> instance that can iterate on each bit state.
	/// </summary>
	/// <returns>A <see cref="BitmapEnumerator"/> instance.</returns>
	[UnscopedRef]
	public readonly BitmapEnumerator EnumerateBitStates() => new(in this);

	/// <summary>
	/// Converts the current object into an array of <see cref="Space"/> instances.
	/// </summary>
	/// <returns>An array of <see cref="Space"/> instances.</returns>
	public readonly Space[] ToArray()
		=> [
			.. from id in _field[0] select Space.RowColumn(id % 9, id / 9),
			.. from id in _field[1] select Space.BlockDigit(id / 9, id % 9),
			.. from id in _field[2] select Space.RowDigit(id / 9, id % 9),
			.. from id in _field[3] select Space.ColumnDigit(id / 9, id % 9)
		];

	/// <inheritdoc/>
	public void Clear() => _field[0] = _field[1] = _field[2] = _field[3] = CellMap.Empty;

	/// <summary>
	/// Adds a new space into the collection.
	/// </summary>
	/// <param name="space">The space.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the adding operation is success.</returns>
	public bool Add(Space space)
	{
		var (type, primary, secondary) = space;
		var id = primary * 9 + secondary;
		if (_field[(int)type].Contains(id))
		{
			return false;
		}

		_field[(int)type].Add(id);
		return true;
	}

	/// <summary>
	/// Removes a space from the current collection.
	/// </summary>
	/// <param name="space">The space.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the removing operation is success.</returns>
	public bool Remove(Space space)
	{
		var (type, primary, secondary) = space;
		var id = primary * 9 + secondary;
		if (!_field[(int)type].Contains(id))
		{
			return false;
		}

		_field[(int)type].Remove(id);
		return true;
	}

	/// <summary>
	/// Adds a list of new spaces into the collection.
	/// </summary>
	/// <param name="spaces">The spaces.</param>
	/// <returns>An <see cref="int"/> value indicating how many spaces adding successfully.</returns>
	public int AddRange(params ReadOnlySpan<Space> spaces)
	{
		var result = 0;
		foreach (var space in spaces)
		{
			result += Add(space) ? 1 : 0;
		}
		return result;
	}

	/// <inheritdoc cref="object.ToString"/>
	public override readonly string ToString() => new RxCyConverter().SpaceConverter(this);

	/// <inheritdoc/>
	readonly bool IEquatable<SpaceSet>.Equals(SpaceSet other) => Equals(other);

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsProperSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsProperSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.IsSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet;
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.Overlaps(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return !!(this & otherSet);
	}

	/// <inheritdoc/>
	readonly bool IReadOnlySet<Space>.SetEquals(IEnumerable<Space> other) => this == [.. other];

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsProperSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsProperSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet && this != otherSet;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsSubsetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (otherSet & this) == this;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.IsSupersetOf(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return (this & otherSet) == otherSet;
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.Overlaps(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		return !!(this & otherSet);
	}

	/// <inheritdoc/>
	readonly bool ISet<Space>.SetEquals(IEnumerable<Space> other) => this == [.. other];

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<SpaceSet, Space>.Any(Func<Space, bool> predicate) => Exists(predicate);

	/// <inheritdoc/>
	readonly bool IAnyAllMethod<SpaceSet, Space>.All(Func<Space, bool> predicate) => TrueForAll(predicate);

	/// <inheritdoc/>
	readonly int IComparable<SpaceSet>.CompareTo(SpaceSet other) => CompareTo(other);

	/// <inheritdoc/>
	readonly SpaceSet IFiniteSet<SpaceSet, Space>.Negate() => ~this;

	/// <inheritdoc/>
	readonly IEnumerator IEnumerable.GetEnumerator() => ToArray().GetEnumerator();

	/// <inheritdoc/>
	readonly IEnumerator<Space> IEnumerable<Space>.GetEnumerator() => ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	void ISet<Space>.ExceptWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this &= ~otherSet;
	}

	/// <inheritdoc/>
	void ISet<Space>.IntersectWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this &= otherSet;
	}

	/// <inheritdoc/>
	void ISet<Space>.SymmetricExceptWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this = (this & ~otherSet) | (otherSet & ~this);
	}

	/// <inheritdoc/>
	void ISet<Space>.UnionWith(IEnumerable<Space> other)
	{
		SpaceSet otherSet = [.. other];
		this |= otherSet;
	}

	/// <inheritdoc/>
	void ICollection<Space>.Add(Space item) => Add(item);

	/// <inheritdoc/>
	SpaceSet IInfiniteSet<SpaceSet, Space>.ExceptWith(SpaceSet other) => this &= ~other;


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string? s, out SpaceSet result)
	{
		try
		{
			if (s is null)
			{
				result = default;
				return false;
			}

			result = Parse(s);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static SpaceSet Parse(string s) => new RxCyParser().SpaceParser(s);

	/// <inheritdoc/>
	static bool IParsable<SpaceSet>.TryParse(string? s, IFormatProvider? provider, out SpaceSet result) => TryParse(s, out result);

	/// <inheritdoc/>
	static SpaceSet IParsable<SpaceSet>.Parse(string s, IFormatProvider? provider) => Parse(s);


	/// <inheritdoc cref="ILogicalOperators{TSelf}.op_LogicalNot(TSelf)"/>
	public static bool operator !(in SpaceSet value) => value.Count == 0;

	/// <inheritdoc cref="ILogicalOperators{TSelf}.op_True(TSelf)"/>
	public static bool operator true(in SpaceSet value) => value.Count != 0;

	/// <inheritdoc cref="ILogicalOperators{TSelf}.op_False(TSelf)"/>
	public static bool operator false(in SpaceSet value) => value.Count == 0;

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_OnesComplement(TSelf)"/>
	public static SpaceSet operator ~(in SpaceSet value)
	{
		var result = value;
		result._field[0] = ~result._field[0];
		result._field[1] = ~result._field[1];
		result._field[2] = ~result._field[2];
		result._field[3] = ~result._field[3];
		return result;
	}

	/// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_Addition(TSelf, TOther)"/>
	public static SpaceSet operator +(in SpaceSet left, Space right)
	{
		var result = left;
		result.Add(right);
		return result;
	}

	/// <inheritdoc cref="ISubtractionOperators{TSelf, TOther, TResult}.op_Subtraction(TSelf, TOther)"/>
	public static SpaceSet operator -(in SpaceSet left, Space right)
	{
		var result = left;
		result.Remove(right);
		return result;
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseAnd(TSelf, TOther)"/>
	public static SpaceSet operator &(in SpaceSet left, in SpaceSet right)
	{
		var result = left;
		result._field[0] &= right._field[0];
		result._field[1] &= right._field[1];
		result._field[2] &= right._field[2];
		result._field[3] &= right._field[3];
		return result;
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseOr(TSelf, TOther)"/>
	public static SpaceSet operator |(in SpaceSet left, in SpaceSet right)
	{
		var result = left;
		result._field[0] |= right._field[0];
		result._field[1] |= right._field[1];
		result._field[2] |= right._field[2];
		result._field[3] |= right._field[3];
		return result;
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_ExclusiveOr(TSelf, TOther)"/>
	public static SpaceSet operator ^(in SpaceSet left, in SpaceSet right)
	{
		var result = left;
		result._field[0] ^= right._field[0];
		result._field[1] ^= right._field[1];
		result._field[2] ^= right._field[2];
		result._field[3] ^= right._field[3];
		return result;
	}

	/// <inheritdoc/>
	static bool ILogicalOperators<SpaceSet>.operator !(SpaceSet value) => value.Count == 0;

	/// <inheritdoc/>
	static bool ILogicalOperators<SpaceSet>.operator true(SpaceSet value) => value.Count != 0;

	/// <inheritdoc/>
	static bool ILogicalOperators<SpaceSet>.operator false(SpaceSet value) => value.Count == 0;

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator ~(SpaceSet value) => ~value;

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator &(SpaceSet left, SpaceSet right) => left & right;

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator |(SpaceSet left, SpaceSet right) => left | right;

	/// <inheritdoc/>
	static SpaceSet IBitwiseOperators<SpaceSet, SpaceSet, SpaceSet>.operator ^(SpaceSet left, SpaceSet right) => left ^ right;

	/// <inheritdoc/>
	static SpaceSet IAdditionOperators<SpaceSet, Space, SpaceSet>.operator +(SpaceSet left, Space right) => left + right;

	/// <inheritdoc/>
	static SpaceSet ISubtractionOperators<SpaceSet, Space, SpaceSet>.operator -(SpaceSet left, Space right) => left - right;
}
