namespace Sudoku.Linq;

/// <summary>
/// Represents a map group for <see cref="CandidateMap"/> and <see cref="CellMap"/>.
/// </summary>
/// <typeparam name="TMap">The type of the map that stores the <see cref="Values"/>.</typeparam>
/// <typeparam name="TElement">The type of elements stored in <see cref="Values"/>.</typeparam>
/// <typeparam name="TKey">The type of the key in the group.</typeparam>
/// <param name="key"><inheritdoc cref="Key" path="/summary"/></param>
/// <param name="values"><inheritdoc cref="Values" path="/summary"/></param>
/// <seealso cref="CellMap"/>
/// <seealso cref="CandidateMap"/>
public readonly struct BitStateMapGrouping<TMap, TElement, TKey>(TKey key, in TMap values) :
	IEnumerable<TElement>,
	IEquatable<BitStateMapGrouping<TMap, TElement, TKey>>,
	IEqualityOperators<BitStateMapGrouping<TMap, TElement, TKey>, BitStateMapGrouping<TMap, TElement, TKey>, bool>,
	IGrouping<TKey, TElement>,
	ISelectMethod<TMap, TElement>,
	IWhereMethod<TMap, TElement>
	where TMap : unmanaged, IBitStateMap<TMap, TElement>
	where TElement : notnull
	where TKey : notnull
{
	/// <summary>
	/// Indicates the number of values stored in <see cref="Values"/>, i.e. the shorthand of expression <c>Values.Count</c>.
	/// </summary>
	/// <seealso cref="Values"/>
	public int Count => Values.Count;

	/// <summary>
	/// Indicates the key used.
	/// </summary>
	public TKey Key { get; } = key;

	/// <summary>
	/// Indicates the candidates.
	/// </summary>
	public TMap Values { get; } = values;


	/// <inheritdoc cref="IBitStateMap{TSelf, TElement}.this[TElement]"/>
	public TElement this[TElement index] => Values[index];


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out TKey key, out TMap values) => (key, values) = (Key, Values);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
		=> obj is BitStateMapGrouping<TMap, TElement, TKey> comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(in BitStateMapGrouping<TMap, TElement, TKey> other) => Values == other.Values;

	/// <inheritdoc/>
	public override int GetHashCode() => Values.GetHashCode();

	/// <summary>
	/// Returns an enumerator that iterates through a collection.
	/// </summary>
	/// <returns>An enumerator object that can be used to iterate through the collection.</returns>
	public AnonymousSpanEnumerator<TElement> GetEnumerator() => Values.GetEnumerator();

	/// <summary>
	/// Filters a sequence of values based on a predicate.
	/// </summary>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>
	/// An array of <typeparamref name="TElement"/> instances
	/// that contains elements from the input sequence that satisfy the condition.
	/// </returns>
	public ReadOnlySpan<TElement> Where(Func<TElement, bool> predicate)
	{
		var result = new TElement[Values.Count];
		var i = 0;
		foreach (var element in Values)
		{
			if (predicate(element))
			{
				result[i++] = element;
			}
		}
		return result.AsReadOnlySpan()[..i];
	}

	/// <summary>
	/// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" path="/summary"/>
	/// </summary>
	/// <typeparam name="TResult">
	/// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" path="/typeparam[@name='TResult']"/>
	/// </typeparam>
	/// <param name="selector">
	/// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" path="/param[@name='selector']"/>
	/// </param>
	/// <returns>
	/// A <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TResult"/> instances
	/// whose elements are the result of invoking the transform function on each element of the current instance.
	/// </returns>
	public ReadOnlySpan<TResult> Select<TResult>(Func<TElement, TResult> selector)
	{
		var result = new TResult[Values.Count];
		var i = 0;
		foreach (var element in Values)
		{
			result[i++] = selector(element);
		}
		return result;
	}


	/// <summary>
	/// Makes a <see cref="CellMap"/> instance that is concatenated by a list of groups
	/// of type <see cref="BitStateMapGrouping{TMap, TElement, TKey}"/>, adding their keys.
	/// </summary>
	/// <param name="groups">The groups.</param>
	/// <returns>A <see cref="CellMap"/> instance.</returns>
	public static CellMap CreateMapByKeys(ReadOnlySpan<BitStateMapGrouping<TMap, TElement, Cell>> groups)
	{
		var result = CellMap.Empty;
		foreach (ref readonly var group in groups)
		{
			result += group.Key;
		}
		return result;
	}

	/// <inheritdoc/>
	bool IEquatable<BitStateMapGrouping<TMap, TElement, TKey>>.Equals(BitStateMapGrouping<TMap, TElement, TKey> other)
		=> Equals(other);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Values.AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<TElement> IWhereMethod<TMap, TElement>.Where(Func<TElement, bool> predicate) => Where(predicate).ToArray();

	/// <inheritdoc/>
	IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => Values.AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<TMap, TElement>.Select<TResult>(Func<TElement, TResult> selector)
		=> Select(selector).ToArray();


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in BitStateMapGrouping<TMap, TElement, TKey> left, in BitStateMapGrouping<TMap, TElement, TKey> right)
		=> left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in BitStateMapGrouping<TMap, TElement, TKey> left, in BitStateMapGrouping<TMap, TElement, TKey> right)
		=> !(left == right);

	/// <inheritdoc/>
	static bool IEqualityOperators<BitStateMapGrouping<TMap, TElement, TKey>, BitStateMapGrouping<TMap, TElement, TKey>, bool>.operator ==(BitStateMapGrouping<TMap, TElement, TKey> left, BitStateMapGrouping<TMap, TElement, TKey> right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<BitStateMapGrouping<TMap, TElement, TKey>, BitStateMapGrouping<TMap, TElement, TKey>, bool>.operator !=(BitStateMapGrouping<TMap, TElement, TKey> left, BitStateMapGrouping<TMap, TElement, TKey> right)
		=> left != right;
}
