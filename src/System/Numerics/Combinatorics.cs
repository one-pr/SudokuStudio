namespace System.Numerics;

/// <summary>
/// Extracts the type that includes the methods that operates with combinatorial values.
/// </summary>
public static class Combinatorics
{
	/// <summary>
	/// Indicates the <see href="https://en.wikipedia.org/wiki/Pascal%27s_triangle">Pascal's Triangle</see>
	/// (in Chinese: Yang Hui's Triangle), i.e. the combinatorial numbers from <c>C(1, 1)</c> to <c>C(30, 30)</c>.
	/// </summary>
	public static readonly int[][] PascalTriangle = Generate(30);


	/// <summary>
	/// The backing method to generate pascal triangle.
	/// </summary>
	/// <param name="rows">The desired number of rows to be generated.</param>
	/// <returns>A list of arrays indicating pascal triangle, with the first element ignored for each row.</returns>
	private static int[][] Generate(int rows)
	{
		Debug.Assert(rows > 0);

		var result = new int[rows][];
		var previous = Array.Empty<BigInteger>();
		for (var r = 0; r < rows + 1; r++)
		{
			var currentRow = new BigInteger[r + 1];
			currentRow[0] = 1;
			currentRow[r] = 1;
			for (var i = 1; i < r; i++)
			{
				currentRow[i] = previous[i - 1] + previous[i];
			}

			if (r >= 1)
			{
				var eachRow = new int[r];
				for (var i = 1; i < currentRow.Length; i++)
				{
					eachRow[i - 1] = (int)currentRow[i];
				}
				result[r - 1] = eachRow;
			}
			previous = currentRow;
		}
		return result;
	}


	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/>.
	/// </summary>
	extension<T>(ReadOnlySpan<T> @this)
	{
		/// <summary>
		/// Get all subsets from the collection.
		/// </summary>
		/// <returns>
		/// All possible subsets returned.
		/// </returns>
		public ReadOnlySpan<T[]> GetSubsets()
#if EXTENSION_OPERATORS
			=> @this | @this.Length;
#else
		{
			var result = new List<T[]>();
			for (var size = 1; size <= @this.Length; size++)
			{
				foreach (var element in @this.GetSubsets(size))
				{
					result.Add(element);
				}
			}
			return result.AsSpan();
		}
#endif

		/// <summary>
		/// Get all subsets from the specified number of the values to take.
		/// </summary>
		/// <param name="count">The number of elements you want to take.</param>
		/// <returns>
		/// The subsets of the list.
		/// For example, if the input array is <c>[1, 2, 3]</c> and the argument <paramref name="count"/> is 2, the result will be
		/// <code><![CDATA[
		/// [[1, 2], [1, 3], [2, 3]]
		/// ]]></code>
		/// 3 cases.
		/// </returns>
		/// <exception cref="ArgumentException">Throws when the argument is negative.</exception>
		public ReadOnlySpan<T[]> GetSubsets(int count)
#if EXTENSION_OPERATORS
			=> @this & count;
#else
		{
			ArgumentException.ThrowIfAssertionFailed(count >= 0);

			if (count == 0)
			{
				return [];
			}

			var result = new List<T[]>();
			GetSubsetsCore(@this.Length, count, count, stackalloc int[count], @this, result);
			return result.AsSpan();
		}
#endif

		/// <summary>
		/// Get all permutations from the collection.
		/// </summary>
		/// <returns>
		/// All possible permutations returned.
		/// </returns>
		public ReadOnlySpan<ReadOnlyMemory<T>> GetPermutations()
		{
			var result = new List<ReadOnlyMemory<T>>();
			for (var size = 1; size <= @this.Length; size++)
			{
				foreach (var element in @this.GetPermutations(size))
				{
					result.Add(element);
				}
			}
			return result.AsSpan();
		}

		/// <summary>
		/// Get all permutations from the specified number of the values to take.
		/// </summary>
		/// <param name="count">The number of elements you want to take.</param>
		/// <returns>
		/// The permutations of the list.
		/// For example, if the input array is <c>[1, 2, 3]</c> and the argument <paramref name="count"/> is 2, the result will be
		/// <code><![CDATA[
		/// [[1, 2], [2, 1], [1, 3], [3, 1], [2, 3], [3, 2]]
		/// ]]></code>
		/// 6 cases.
		/// </returns>
		public ReadOnlySpan<ReadOnlyMemory<T>> GetPermutations(int count)
		{
			if (count == 0)
			{
				return [];
			}

			var result = new List<ReadOnlyMemory<T>>(PermutationOf(@this.Length, count));
			var used = (stackalloc bool[@this.Length]);
			used.Clear();

			GetPermutationsCore(new(count), @this, used, count, result);
			return result.AsSpan();
		}


		private static void GetSubsetsCore(
			int last,
			int count,
			int index,
			Span<int> tempArray,
			ReadOnlySpan<T> thisCopied, List<T[]> resultList
		)
		{
			for (var i = last; i >= index; i--)
			{
				tempArray[index - 1] = i - 1;
				if (index > 1)
				{
					GetSubsetsCore(i - 1, count, index - 1, tempArray, thisCopied, resultList);
				}
				else
				{
					var temp = new T[count];
					for (var j = 0; j < tempArray.Length; j++)
					{
						temp[j] = thisCopied[tempArray[j]];
					}
					resultList.Add(temp);
				}
			}
		}

		private static void GetPermutationsCore(
			List<T> temp,
			ReadOnlySpan<T> array,
			Span<bool> used,
			int count,
			List<ReadOnlyMemory<T>> result
		)
		{
			if (temp.Count == count)
			{
				result.Add(temp.ToArray());
				return;
			}

			for (var i = 0; i < array.Length; i++)
			{
				if (!used[i])
				{
					used[i] = true;
					temp.Add(array[i]);

					GetPermutationsCore(temp, array, used, count, result);

					temp.RemoveAt(^1);
					used[i] = false;
				}
			}
		}


#if EXTENSION_OPERATORS
		/// <summary>
		/// Get all subsets from the specified number of the values to take.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="count">The number of elements you want to take.</param>
		/// <returns>
		/// The subsets of the list.
		/// For example, if the input array is <c>[1, 2, 3]</c> and the argument <paramref name="count"/> is 2, the result will be
		/// <code><![CDATA[
		/// [[1, 2], [1, 3], [2, 3]]
		/// ]]></code>
		/// 3 cases.
		/// </returns>
		/// <exception cref="ArgumentException">Throws when the argument is negative.</exception>
		public static ReadOnlySpan<T[]> operator &(ReadOnlySpan<T> value, int count)
		{
			ArgumentException.ThrowIfAssertionFailed(count >= 0);

			if (count == 0)
			{
				return [];
			}

			var result = new List<T[]>();
			GetSubsetsCore(value.Length, count, count, stackalloc int[count], value, result);
			return result.AsSpan();
		}

		/// <summary>
		/// Get all subsets from the limited number of the values to take,
		/// specified as maximum number of elements of each combination.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="count">The number of elements you want to take, as maximum possible value.</param>
		/// <exception cref="ArgumentException">Throws when the argument is negative.</exception>
		public static ReadOnlySpan<T[]> operator |(ReadOnlySpan<T> value, int count)
		{
			var result = new List<T[]>();
			for (var size = 1; size <= count; size++)
			{
				foreach (var element in value.GetSubsets(size))
				{
					result.Add(element);
				}
			}
			return result.AsSpan();
		}
#endif
	}

	/// <summary>
	/// Provides extension members on <typeparamref name="T"/>[][].
	/// </summary>
	extension<T>(T[][] @this)
	{
		/// <summary>
		/// Get all combinations that each sub-array only choose one.
		/// </summary>
		/// <returns>
		/// All combinations that each sub-array choose one.
		/// For example, if one array is <c>[[1, 2, 3], [1, 3], [1, 4, 7, 10]]</c>, the final combinations will be
		/// <code><![CDATA[
		/// [
		///     [1, 1, 1], [1, 1, 4], [1, 1, 7], [1, 1, 10],
		///     [1, 3, 1], [1, 3, 4], [1, 3, 7], [1, 3, 10],
		///     [2, 1, 1], [2, 1, 4], [2, 1, 7], [2, 1, 10],
		///     [2, 3, 1], [2, 3, 4], [2, 3, 7], [2, 3, 10],
		///     [3, 1, 1], [3, 1, 4], [3, 1, 7], [3, 1, 10],
		///     [3, 3, 1], [3, 3, 4], [3, 3, 7], [3, 3, 10]
		/// ]
		/// ]]></code>
		/// 24 cases.
		/// </returns>
		public T[][] GetExtractedCombinations()
		{
			var length = @this.Length;
			var resultCount = 1;
			var tempArray = (stackalloc int[length]);
			for (var i = 0; i < length; i++)
			{
				tempArray[i] = -1;
				resultCount *= @this[i].Length;
			}

			var (result, m, n) = (new T[resultCount][], -1, -1);
			do
			{
				if (m < length - 1)
				{
					m++;
				}

				ref var value = ref tempArray[m];
				value++;
				if (value > @this[m].Length - 1)
				{
					value = -1;
					m -= 2; // Backtrack.
				}

				if (m == length - 1)
				{
					n++;
					result[n] = new T[m + 1];
					for (var i = 0; i <= m; i++)
					{
						result[n][i] = @this[i][tempArray[i]];
					}
				}
			} while (m >= -1);

			return result;
		}
	}


	/// <summary>
	/// Returns the combination of (n, m).
	/// </summary>
	/// <param name="n">The number of all values.</param>
	/// <param name="m">The number of values to get.</param>
	/// <returns>An <see cref="int"/> of result.</returns>
	/// <exception cref="OverflowException">Throws when the result value is too large.</exception>
	public static int CombinationOf(int n, int m) => checked((int)(Factorial(n) / (Factorial(m) * Factorial(n - m))));

	/// <summary>
	/// Returns the permutation of (n, m).
	/// </summary>
	/// <param name="n">The number of all values.</param>
	/// <param name="m">The number of values to get.</param>
	/// <returns>An <see cref="int"/> of result.</returns>
	/// <exception cref="OverflowException">Throws when the result value is too large.</exception>
	public static int PermutationOf(int n, int m) => checked((int)(Factorial(n) / Factorial(n - m)));

	/// <summary>
	/// Returns the factorial of <paramref name="n"/> (n!).
	/// </summary>
	/// <param name="n">The value.</param>
	/// <returns>The result.</returns>
	private static BigInteger Factorial(int n)
	{
		var result = (BigInteger)1;
		for (var i = 2; i <= n; i++)
		{
			result *= i;
		}
		return result;
	}
}
