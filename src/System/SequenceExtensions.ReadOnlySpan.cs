namespace System;

public partial class SequenceExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/>.
	/// </summary>
	extension<T>(ReadOnlySpan<T> @this)
	{
		/// <summary>
		/// Finds the first element satisfying the specified condition, and return its corresponding index.
		/// </summary>
		/// <param name="predicate">The condition.</param>
		/// <returns>
		/// An <see cref="int"/> indicating the found element. -1 returns if the sequence has no element satisfying the condition.
		/// </returns>
		public int FirstIndex(Func<T, bool> predicate)
		{
			for (var i = 0; i < @this.Length; i++)
			{
				if (predicate(@this[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Finds the last element satisfying the specified condition, and return its corresponding index.
		/// </summary>
		/// <param name="predicate">The condition.</param>
		/// <returns>
		/// An <see cref="int"/> indicating the found element. -1 returns if the sequence has no element satisfying the condition.
		/// </returns>
		public int LastIndex(Func<T, bool> predicate)
		{
			for (var i = @this.Length - 1; i >= 0; i--)
			{
				if (predicate(@this[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
		public int FindIndex(Func<T, bool> condition)
		{
			for (var i = 0; i < @this.Length; i++)
			{
				if (condition(@this[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Iterates on each element, in reverse order.
		/// </summary>
		/// <returns>An enumerator type that iterates on each element.</returns>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Reverse, false)]
		public ReverseEnumerator<T> EnumerateReversely() => new(@this);

		/// <summary>
		/// Retrieves all the elements that match the conditions defined by the specified predicate.
		/// </summary>
		/// <param name="match">The <see cref="Func{T, TResult}"/> that defines the conditions of the elements to search for.</param>
		/// <returns>
		/// A <see cref="ReadOnlySpan{T}"/> containing all the elements that match the conditions defined
		/// by the specified predicate, if found; otherwise, an empty <see cref="ReadOnlySpan{T}"/>.
		/// </returns>
		public ReadOnlySpan<T> FindAll(Func<T, bool> match)
		{
			var result = new List<T>(@this.Length);
			foreach (ref readonly var element in @this)
			{
				if (match(element))
				{
					result.AddRef(element);
				}
			}
			return result.AsSpan();
		}

		/// <inheritdoc cref="op_Division{T}(ReadOnlySpan{T}, int)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Chunk, false)]
		public ReadOnlySpan<T[]> Chunk(int size) => @this / size;


		/// <summary>
		/// Returns <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The instance same as <paramref name="value"/>.</returns>
		public static ReadOnlySpan<T> operator +(ReadOnlySpan<T> value) => value;

		/// <summary>
		/// Creates a reversed collection of <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The reversed collection.</returns>
		public static ReadOnlySpan<T> operator -(scoped ReadOnlySpan<T> value)
		{
			var result = new T[value.Length];
			for (var (i, j) = (value.Length - 1, 0); i >= 0; i--, j++)
			{
				result[j] = value[i];
			}
			return result;
		}

		/// <summary>
		/// Splits a sequence into multiple sub-arrays ("chunks") of the specified size.
		/// The elements are grouped in their original order, and the last chunk may contain fewer elements
		/// if the total length of the array is not evenly divisible by the chunk size.
		/// </summary>
		/// <param name="source">The one-dimensional array to be divided into chunks.</param>
		/// <param name="size">
		/// The maximum number of elements in each chunk. Must be greater than zero.
		/// </param>
		/// <returns>
		/// An array of sub-arrays, where each sub-array contains up to <paramref name="size"/> elements
		/// in the same order as they appear in the <paramref name="source"/> array.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="size"/> is less than or equal to zero.
		/// </exception>
		public static ReadOnlySpan<T[]> operator /(ReadOnlySpan<T> source, int size)
		{
			var result = new List<T[]>((source.Length + size - 1) / size);
			for (var i = 0; i < source.Length; i += size)
			{
				var subarray = new T[size];
				for (var j = 0; j < size; j++)
				{
					subarray[j] = source[i * size + j];
				}
				result.Add(subarray);
			}
			return result.AsSpan();
		}
	}

	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/>,
	/// where <typeparamref name="T"/> satisfies <see langword="notnull"/> constraint.
	/// </summary>
	extension<T>(ReadOnlySpan<T> @this) where T : notnull
	{
		/// <summary>
		/// Creates a <see cref="PairEnumerator{T}"/> instance that iterates on each element of pair elements.
		/// </summary>
		/// <returns>An enumerable collection.</returns>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Chunk, false)]
		public PairEnumerator<T> EnumeratePaired()
			=> new(
				(@this.Length & 1) == 0
					? @this
					: throw new ArgumentException(SR.ExceptionMessage("SpecifiedValueMustBeEven"), nameof(@this))
			);
	}

	/// <summary>
	/// Provides extension members on <see langword="scoped"/> <see cref="ReadOnlySpan{T}"/>.
	/// </summary>
	extension<T>(scoped ReadOnlySpan<T> @this)
	{
		/// <summary>
		/// Returns a new <see cref="ReadOnlySpan{T}"/> instance whose internal elements are all come from the current collection,
		/// with reversed order.
		/// </summary>
		/// <returns>A new collection whose elements are in reversed order.</returns>
		[OverloadResolutionPriority(1)]
		[Obsolete(DeprecatedMessages.ExtensionOperator_Reverse, false)]
		public ReadOnlySpan<T> Reverse() => -@this;
	}
}
