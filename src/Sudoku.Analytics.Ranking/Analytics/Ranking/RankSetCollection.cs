namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a collection of rank sets.
/// </summary>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.AllEqualityComparisonOperators)]
public sealed partial class RankSetCollection :
	ICloneable,
	ICollection<RankSet>,
	IComparable<RankSetCollection>,
	IComparisonOperators<RankSetCollection, RankSetCollection, bool>,
	IEnumerable<RankSet>,
	IEquatable<RankSetCollection>,
	IEqualityOperators<RankSetCollection, RankSetCollection, bool>,
	IInfiniteSet<RankSetCollection, RankSet>,
	IReadOnlyCollection<RankSet>,
	IReadOnlySet<RankSet>,
	ISet<RankSet>,
	IToArrayMethod<RankSetCollection, RankSet>
{
	/// <summary>
	/// Indicates the backing sets.
	/// </summary>
	private readonly SortedSet<RankSet> _sets = [];


	/// <inheritdoc/>
	public int Count => _sets.Count;

	/// <summary>
	/// Indicates the minimal rank set.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when there's no elements in the collection.</exception>
	public RankSet Min => _sets.Min ?? throw new InvalidOperationException("There's no elements in the collection.");

	/// <summary>
	/// Indicates the maximal rank set.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when there's no elements in the collection.</exception>
	public RankSet Max => _sets.Max ?? throw new InvalidOperationException("There's no elements in the collection.");

	/// <summary>
	/// Indicates all truths.
	/// </summary>
	internal RankSetCollection Truths => [.. from set in _sets where set.IsTruth select set];

	/// <summary>
	/// Indicates all links.
	/// </summary>
	internal RankSetCollection Links => [.. from set in _sets where set.IsLink select set];

	/// <inheritdoc/>
	bool ICollection<RankSet>.IsReadOnly => false;


	/// <summary>
	/// Represents the comparer.
	/// </summary>
	private static IEqualityComparer<SortedSet<RankSet>> Comparer => field ??= SortedSet<RankSet>.CreateSetComparer();


	/// <inheritdoc/>
	public void Clear() => _sets.Clear();

	/// <inheritdoc/>
	public void CopyTo(RankSet[] array, int arrayIndex)
	{
		var tempArray = _sets.ToArray();
		for (var i = 0; i < tempArray.Length; i++)
		{
			array[i + arrayIndex] = tempArray[i];
		}
	}

	/// <inheritdoc cref="ICollection{T}.Add(T)"/>
	public bool Add(RankSet item) => _sets.Add(item);

	/// <inheritdoc/>
	public bool Remove(RankSet item) => _sets.Remove(item);

	/// <inheritdoc/>
	public bool Contains(RankSet item) => _sets.Contains(item);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] RankSetCollection? other) => other is not null && Comparer.Equals(_sets, other._sets);

	/// <inheritdoc/>
	public override int GetHashCode() => Comparer.GetHashCode(_sets);

	/// <inheritdoc/>
	public int CompareTo(RankSetCollection? other)
	{
		if (other is null)
		{
			return 1;
		}

		using var e1 = _sets.GetEnumerator();
		using var e2 = other._sets.GetEnumerator();
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

	/// <inheritdoc/>
	public override string ToString()
	{
		if (Count == 0)
		{
			return "<Empty>";
		}

		var truths = string.Join(' ', Truths).ToUpper();
		var links = string.Join(' ', Links).ToLower();
		return (Truths, Links) switch
		{
			({ Count: not 0 }, { Count: not 0 }) => $"""{truths}\{links}""",
			({ Count: not 0 }, _) => truths,
			_ => links
		};
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new(_sets);

	/// <summary>
	/// Enumerates all truths.
	/// </summary>
	/// <returns>Truths.</returns>
	public Enumerator EnumerateTruths() => new(_sets, false, true);

	/// <summary>
	/// Enumerates all links.
	/// </summary>
	/// <returns>Links.</returns>
	public Enumerator EnumerateLinks() => new(_sets, true, false);

	/// <inheritdoc/>
	public RankSet[] ToArray() => [.. _sets];

	/// <inheritdoc cref="ICloneable.Clone"/>
	public RankSetCollection Clone() => [.. _sets];

	/// <inheritdoc/>
	void ICollection<RankSet>.Add(RankSet item) => Add(item);

	/// <inheritdoc/>
	void ISet<RankSet>.ExceptWith(IEnumerable<RankSet> other) => _sets.ExceptWith(other);

	/// <inheritdoc/>
	void ISet<RankSet>.IntersectWith(IEnumerable<RankSet> other) => _sets.IntersectWith(other);

	/// <inheritdoc/>
	void ISet<RankSet>.SymmetricExceptWith(IEnumerable<RankSet> other) => _sets.SymmetricExceptWith(other);

	/// <inheritdoc/>
	void ISet<RankSet>.UnionWith(IEnumerable<RankSet> other) => _sets.UnionWith(other);

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	bool ISet<RankSet>.IsProperSubsetOf(IEnumerable<RankSet> other) => _sets.IsProperSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<RankSet>.IsProperSupersetOf(IEnumerable<RankSet> other) => _sets.IsProperSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<RankSet>.IsSubsetOf(IEnumerable<RankSet> other) => _sets.IsSubsetOf(other);

	/// <inheritdoc/>
	bool ISet<RankSet>.IsSupersetOf(IEnumerable<RankSet> other) => _sets.IsSupersetOf(other);

	/// <inheritdoc/>
	bool ISet<RankSet>.Overlaps(IEnumerable<RankSet> other) => _sets.Overlaps(other);

	/// <inheritdoc/>
	bool ISet<RankSet>.SetEquals(IEnumerable<RankSet> other) => _sets.SetEquals(other);

	/// <inheritdoc/>
	bool IReadOnlySet<RankSet>.IsProperSubsetOf(IEnumerable<RankSet> other) => ((ISet<RankSet>)this).IsProperSubsetOf(other);

	/// <inheritdoc/>
	bool IReadOnlySet<RankSet>.IsProperSupersetOf(IEnumerable<RankSet> other) => ((ISet<RankSet>)this).IsProperSupersetOf(other);

	/// <inheritdoc/>
	bool IReadOnlySet<RankSet>.IsSubsetOf(IEnumerable<RankSet> other) => ((ISet<RankSet>)this).IsSubsetOf(other);

	/// <inheritdoc/>
	bool IReadOnlySet<RankSet>.IsSupersetOf(IEnumerable<RankSet> other) => ((ISet<RankSet>)this).IsSupersetOf(other);

	/// <inheritdoc/>
	bool IReadOnlySet<RankSet>.Overlaps(IEnumerable<RankSet> other) => ((ISet<RankSet>)this).Overlaps(other);

	/// <inheritdoc/>
	bool IReadOnlySet<RankSet>.SetEquals(IEnumerable<RankSet> other) => ((ISet<RankSet>)this).SetEquals(other);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _sets.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<RankSet> IEnumerable<RankSet>.GetEnumerator() => _sets.ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	RankSetCollection IInfiniteSet<RankSetCollection, RankSet>.ExceptWith(RankSetCollection other)
	{
		var copied = Clone();
		copied._sets.ExceptWith(other);
		return [.. copied._sets];
	}


	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseAnd(TSelf, TOther)"/>
	public static RankSetCollection operator &(RankSetCollection left, RankSetCollection right)
	{
		var copied = left.Clone();
		copied._sets.IntersectWith(right._sets);
		return [.. copied._sets];
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseOr(TSelf, TOther)"/>
	public static RankSetCollection operator |(RankSetCollection left, RankSetCollection right)
	{
		var copied = left.Clone();
		copied._sets.UnionWith(right._sets);
		return [.. copied._sets];
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_ExclusiveOr(TSelf, TOther)"/>
	public static RankSetCollection operator ^(RankSetCollection left, RankSetCollection right)
	{
		var copied = left.Clone();
		copied._sets.SymmetricExceptWith(right._sets);
		return [.. copied._sets];
	}
}
