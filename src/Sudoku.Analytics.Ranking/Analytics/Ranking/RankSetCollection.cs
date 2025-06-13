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
	/// Indicates the cached field for truths.
	/// </summary>
	private readonly SortedSet<RankSet> _truths = [];

	/// <summary>
	/// Indicates the cached field for links.
	/// </summary>
	private readonly SortedSet<RankSet> _links = [];


	/// <inheritdoc/>
	public int Count => _truths.Count + _links.Count;

	/// <summary>
	/// Indicates all truths.
	/// </summary>
	public RankSetCollection Truths => [.. _truths];

	/// <summary>
	/// Indicates all links.
	/// </summary>
	public RankSetCollection Links => [.. _links];

	/// <inheritdoc/>
	bool ICollection<RankSet>.IsReadOnly => false;


	/// <summary>
	/// Represents the comparer.
	/// </summary>
	private static IEqualityComparer<SortedSet<RankSet>> Comparer => field ??= SortedSet<RankSet>.CreateSetComparer();


	/// <inheritdoc/>
	public void Clear()
	{
		_truths.Clear();
		_links.Clear();
	}

	/// <inheritdoc/>
	public void CopyTo(RankSet[] array, int arrayIndex)
	{
		var tempArray = (RankSet[])[.. _truths, .. _links];
		for (var i = 0; i < tempArray.Length; i++)
		{
			array[i + arrayIndex] = tempArray[i];
		}
	}

	/// <inheritdoc cref="ICollection{T}.Add(T)"/>
	public bool Add(RankSet item) => (item.IsTruth ? _truths : _links).Add(item);

	/// <inheritdoc/>
	public bool Remove(RankSet item) => (item.IsTruth ? _truths : _links).Remove(item);

	/// <inheritdoc/>
	public bool Contains(RankSet item) => (item.IsTruth ? _truths : _links).Contains(item);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] RankSetCollection? other)
		=> other is not null && Comparer.Equals(_truths, other._truths) && Comparer.Equals(_links, other._links);

	/// <inheritdoc/>
	public override int GetHashCode() => Comparer.GetHashCode(_truths) ^ Comparer.GetHashCode(_links);

	/// <inheritdoc/>
	public int CompareTo(RankSetCollection? other)
	{
		if (other is null)
		{
			return 1;
		}

		using var e1 = GetEnumerator();
		using var e2 = GetEnumerator();
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

		var truths = toPartString(_truths).ToUpper();
		var links = toPartString(_links).ToLower();
		return (Truths, Links) switch
		{
			({ Count: not 0 }, { Count: not 0 }) => $"""{truths}\{links}""",
			({ Count: not 0 }, _) => truths,
			_ => links
		};


		static string toPartString(SortedSet<RankSet> sets)
		{
			Dictionary<Digit, Mask> rn = [], cn = [], bn = [], rc = [];
			foreach (var set in sets)
			{
				Cell targetCell;
				House targetHouse;
				Digit targetDigit;
				switch (set)
				{
					case CellTruth { Cell: var cell }:
					{
						targetCell = cell;
						goto ForCellBased;
					}
					case CellLink { Cell: var cell }:
					{
						targetCell = cell;
						goto ForCellBased;
					}
					case HouseTruth { House: var house, Digit: var digit }:
					{
						(targetHouse, targetDigit) = (house, digit);
						goto ForHouseBased;
					}
					case HouseLink { House: var house, Digit: var digit }:
					{
						(targetHouse, targetDigit) = (house, digit);
						goto ForHouseBased;
					}

				ForCellBased:
					{
						if (!rc.TryAdd(targetCell / 9, (Mask)(1 << targetCell % 9)))
						{
							rc[targetCell / 9] |= (Mask)(1 << targetCell % 9);
						}
						break;
					}

				ForHouseBased:
					{
						var houseType = (HouseType)(targetHouse / 9);
						var index = targetHouse % 9;
						var dic = houseType switch { HouseType.Block => bn, HouseType.Row => rn, _ => cn };
						if (!dic.TryAdd(index, (Mask)(1 << targetDigit)))
						{
							dic[index] |= (Mask)(1 << targetDigit);
						}
						break;
					}
				}
			}

			var t1 = (
				(
					from pair in rc.ToArray()
					let r = pair.Key
					let values = pair.Value
					select $"{r + 1}n{string.Concat([.. from c in values select c + 1])}"
				) is { Length: not 0 } rcParts ? string.Join(' ', rcParts) : null) is { } w ? $"{w} " : string.Empty;
			var t2 = (
				(
					from pair in bn.ToArray()
					let n = pair.Key
					let values = pair.Value
					select $"{string.Concat([.. from b in values select b + 1])}b{n + 1}"
				) is { Length: not 0 } bnParts ? string.Join(' ', bnParts) : null) is { } x ? $"{x} " : string.Empty;
			var t3 = (
				(
					from pair in rn.ToArray()
					let n = pair.Key
					let values = pair.Value
					select $"{string.Concat([.. from r in values select r + 1])}r{n + 1}"
				) is { Length: not 0 } rnParts ? string.Join(' ', rnParts) : null) is { } y ? $"{y} " : string.Empty;
			var t4 = (
				(
					from pair in cn.ToArray()
					let n = pair.Key
					let values = pair.Value
					select $"{string.Concat([.. from c in values select c + 1])}c{n + 1}"
				) is { Length: not 0 } cnParts ? string.Join(' ', cnParts) : null) is { } z ? $"{z} " : string.Empty;
			return $"{t1}{t2}{t3}{t4}".TrimEnd();
		}
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new([.. _truths, .. _links]);

	/// <summary>
	/// Enumerates all truths.
	/// </summary>
	/// <returns>Truths.</returns>
	public Enumerator EnumerateTruths() => new(_truths);

	/// <summary>
	/// Enumerates all links.
	/// </summary>
	/// <returns>Links.</returns>
	public Enumerator EnumerateLinks() => new(_links);

	/// <inheritdoc/>
	public RankSet[] ToArray() => [.. _truths, .. _links];

	/// <inheritdoc cref="ICloneable.Clone"/>
	public RankSetCollection Clone() => [.. _truths, .. _links];

	/// <inheritdoc/>
	void ICollection<RankSet>.Add(RankSet item) => Add(item);

	/// <inheritdoc/>
	void ISet<RankSet>.ExceptWith(IEnumerable<RankSet> other)
	{
		_truths.ExceptWith(from set in other where set.IsTruth select set);
		_links.ExceptWith(from set in other where set.IsLink select set);
	}

	/// <inheritdoc/>
	void ISet<RankSet>.IntersectWith(IEnumerable<RankSet> other)
	{
		_truths.IntersectWith(from set in other where set.IsTruth select set);
		_links.IntersectWith(from set in other where set.IsLink select set);
	}

	/// <inheritdoc/>
	void ISet<RankSet>.SymmetricExceptWith(IEnumerable<RankSet> other)
	{
		_truths.SymmetricExceptWith(from set in other where set.IsTruth select set);
		_links.SymmetricExceptWith(from set in other where set.IsLink select set);
	}

	/// <inheritdoc/>
	void ISet<RankSet>.UnionWith(IEnumerable<RankSet> other)
	{
		_truths.UnionWith(from set in other where set.IsTruth select set);
		_links.UnionWith(from set in other where set.IsLink select set);
	}

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	bool ISet<RankSet>.IsProperSubsetOf(IEnumerable<RankSet> other)
		=> _truths.IsProperSubsetOf(from set in other where set.IsTruth select set)
		&& _links.IsProperSubsetOf(from set in other where set.IsLink select set);

	/// <inheritdoc/>
	bool ISet<RankSet>.IsProperSupersetOf(IEnumerable<RankSet> other)
		=> _truths.IsProperSupersetOf(from set in other where set.IsTruth select set)
		&& _links.IsProperSupersetOf(from set in other where set.IsLink select set);

	/// <inheritdoc/>
	bool ISet<RankSet>.IsSubsetOf(IEnumerable<RankSet> other)
		=> _truths.IsSubsetOf(from set in other where set.IsTruth select set)
		&& _links.IsSubsetOf(from set in other where set.IsLink select set);

	/// <inheritdoc/>
	bool ISet<RankSet>.IsSupersetOf(IEnumerable<RankSet> other)
		=> _truths.IsSupersetOf(from set in other where set.IsTruth select set)
		&& _links.IsSupersetOf(from set in other where set.IsLink select set);

	/// <inheritdoc/>
	bool ISet<RankSet>.Overlaps(IEnumerable<RankSet> other)
		=> _truths.Overlaps(from set in other where set.IsTruth select set)
		&& _links.Overlaps(from set in other where set.IsLink select set);

	/// <inheritdoc/>
	bool ISet<RankSet>.SetEquals(IEnumerable<RankSet> other)
		=> _truths.SetEquals(from set in other where set.IsTruth select set)
		&& _links.SetEquals(from set in other where set.IsLink select set);

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
	IEnumerator IEnumerable.GetEnumerator()
	{
		foreach (var truth in _truths)
		{
			yield return truth;
		}
		foreach (var link in _links)
		{
			yield return link;
		}
	}

	/// <inheritdoc/>
	IEnumerator<RankSet> IEnumerable<RankSet>.GetEnumerator()
	{
		foreach (var truth in _truths)
		{
			yield return truth;
		}
		foreach (var link in _links)
		{
			yield return link;
		}
	}

	/// <inheritdoc/>
	RankSetCollection IInfiniteSet<RankSetCollection, RankSet>.ExceptWith(RankSetCollection other)
	{
		var copied = Clone();
		copied._truths.ExceptWith(other._truths);
		copied._links.ExceptWith(other._links);
		return [.. copied._truths, .. copied._links];
	}


	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseAnd(TSelf, TOther)"/>
	public static RankSetCollection operator &(RankSetCollection left, RankSetCollection right)
	{
		var copied = left.Clone();
		copied._truths.IntersectWith(right._truths);
		copied._links.IntersectWith(right._links);
		return [.. copied._truths, .. copied._links];
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseOr(TSelf, TOther)"/>
	public static RankSetCollection operator |(RankSetCollection left, RankSetCollection right)
	{
		var copied = left.Clone();
		copied._truths.UnionWith(right._truths);
		copied._links.UnionWith(right._links);
		return [.. copied._truths, .. copied._links];
	}

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_ExclusiveOr(TSelf, TOther)"/>
	public static RankSetCollection operator ^(RankSetCollection left, RankSetCollection right)
	{
		var copied = left.Clone();
		copied._truths.SymmetricExceptWith(right._truths);
		copied._links.SymmetricExceptWith(right._links);
		return [.. copied._truths, .. copied._links];
	}
}
