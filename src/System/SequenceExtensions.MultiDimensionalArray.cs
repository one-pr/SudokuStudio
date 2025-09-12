namespace System;

public partial class SequenceExtensions
{
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
}
