namespace Sudoku.Shuffling;

/// <summary>
/// Provides a way to rank and unrank permutation. The ranking algorithm is called "Cantor Expansion".
/// </summary>
public static class CantorExpansion
{
	/// <summary>
	/// Rank the current permutation from the base order.
	/// </summary>
	/// <param name="perm">The permutation sequence.</param>
	/// <param name="baseOrder">The base-ordered sequence.</param>
	/// <returns>The rank.</returns>
	/// <exception cref="ArgumentException">
	/// Throws when the arguments <paramref name="baseOrder"/> and <paramref name="perm"/> has different lengths.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Throws when the permutation has incorrect value that cannot be found in source sequence.
	/// </exception>
	public static int RankDigit(ReadOnlySpan<Digit> perm, ReadOnlySpan<Digit> baseOrder)
	{
		var n = perm.Length;
		ArgumentException.ThrowIfAssertionFailed(baseOrder.Length == n);
		var fact = Factorials(n);
		var remaining = new List<Digit>();
		remaining.AddRange(baseOrder);

		var rank = 0;
		for (var i = 0; i < n; i++)
		{
			var j = remaining.IndexOf(perm[i]);
			InvalidOperationException.ThrowIf(j != -1);

			rank += j * fact[n - 1 - i];
			remaining.RemoveAt(j);
		}
		return rank;
	}

	/// <summary>
	/// Rank the current label permutation (row or column labels).
	/// </summary>
	/// <param name="labels">The permutation sequence.</param>
	/// <returns>The rank.</returns>
	/// <exception cref="ArgumentException">
	/// Throws when the argument <paramref name="labels"/> doesn't contain 9 elements.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Throws when the original label contains invalid digits.
	/// </exception>
	public static int RankLine(Digit[] labels)
	{
		ArgumentException.ThrowIfAssertionFailed(labels.Length == 9);

		var inversed = new int[9];
		for (var originalLabel = 0; originalLabel < 9; originalLabel++)
		{
			var n = labels[originalLabel];
			inversed[n] = n is >= 0 and < 9 ? originalLabel : throw new InvalidOperationException("map values must be 0..8");
		}

		var bperm = new int[3];
		var intra = (int[][])[new int[3], new int[3], new int[3]];
		for (var newBand = 0; newBand < 3; newBand++)
		{
			for (var j = 0; j < 3; j++)
			{
				var newRow = newBand * 3 + j;
				var origRow = inversed[newRow];
				bperm[newBand] = origRow / 3;
				intra[newBand][j] = origRow % 3;
			}

			if (intra[newBand].Length != 3
				|| inversed[newBand * 3] / 3 != bperm[newBand]
				|| inversed[newBand * 3 + 1] / 3 != bperm[newBand]
				|| inversed[newBand * 3 + 2] / 3 != bperm[newBand])
			{
				for (var j = 0; j < 3; j++)
				{
					if (inversed[newBand * 3 + j] / 3 != bperm[newBand])
					{
						throw new ArgumentException("Provided map is not a valid band-preserving line relabelling.");
					}
				}
			}
		}

		var bandIndex = Rank3(bperm);
		var intra0 = Rank3(intra[0]);
		var intra1 = Rank3(intra[1]);
		var intra2 = Rank3(intra[2]);
		return bandIndex * 216 + intra0 * 36 + intra1 * 6 + intra2;
	}

	/// <summary>
	/// Unrank the permutation via the base order.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <param name="baseOrder">The base-ordered sequence.</param>
	/// <returns>The unranked permutation.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when rank is out of range.</exception>
	public static ReadOnlySpan<Digit> UnrankDigit(int rank, ReadOnlySpan<Digit> baseOrder)
	{
		var n = baseOrder.Length;
		var fact = Factorials(n);
		if (rank < 0 || rank >= fact[n])
		{
			throw new ArgumentOutOfRangeException(nameof(rank));
		}

		var remaining = new List<Digit>();
		remaining.AddRange(baseOrder);

		var result = new List<Digit>(n);
		for (var i = 0; i < n; i++)
		{
			var f = fact[n - 1 - i];
			var idx = rank / f;
			rank %= f;
			result.Add(remaining[idx]);
			remaining.RemoveAt(idx);
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Unrank the permutation.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The unranked sequence.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when rank is out of range.</exception>
	public static Digit[] UnrankLine(int rank)
	{
		if (rank is < 0 or >= TransformIdentifier.RemapRowsPermutationsCount)
		{
			throw new ArgumentOutOfRangeException(nameof(rank));
		}

		var chuteIndex = rank / 216;
		var r = rank % 216;
		var intra0 = r / 36;
		r %= 36;
		var intra1 = r / 6;
		var intra2 = r % 6;

		var bandPerm = Unrank3(chuteIndex);
		var intraA = Unrank3(intra0);
		var intraB = Unrank3(intra1);
		var intraC = Unrank3(intra2);

		var map = new int[9];
		for (var newBand = 0; newBand < 3; newBand++)
		{
			var origBand = bandPerm[newBand];
			var intra = newBand == 0 ? intraA : (newBand == 1 ? intraB : intraC);

			for (var j = 0; j < 3; j++)
			{
				var newRow = newBand * 3 + j;
				var origRow = origBand * 3 + intra[j];
				map[origRow] = newRow;
			}
		}
		return map;
	}

	/// <summary>
	/// Extracts an array of factorial values to optimize calculation.
	/// </summary>
	/// <param name="n">The number.</param>
	/// <returns>The sequence of factorial values (1!, 2!, 3!, etc.).</returns>
	private static int[] Factorials(int n)
	{
		var fact = new int[n + 1];
		fact[0] = 1;
		for (var i = 1; i <= n; i++)
		{
			fact[i] = fact[i - 1] * i;
		}
		return fact;
	}

	/// <summary>
	/// Rank for 3 lines in a chute.
	/// </summary>
	/// <param name="perm">The permutation (row indices or column indices).</param>
	/// <returns>The rank.</returns>
	private static int Rank3(House[] perm)
	{
		ArgumentException.ThrowIfAssertionFailed(perm.Length == 3);
		var items = (int[])[0, 1, 2];
		var rank = 0;
		for (var i = 0; i < 2; i++)
		{
			var j = Array.IndexOf(items, perm[i]);
			rank += j * (i == 0 ? 2 : 1);
			items = [.. items.Where((_, k) => k != j)];
		}
		return rank;
	}

	/// <summary>
	/// Unrank for 3 lines in a chute.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The permutation.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="rank"/> is out of range.</exception>
	private static House[] Unrank3(int rank)
	{
		if (rank is < 0 or >= 6)
		{
			throw new ArgumentOutOfRangeException(nameof(rank));
		}

		var items = (int[])[0, 1, 2];
		var result = new int[3];
		var r = rank;
		var c0 = r / 2;
		r %= 2;
		var c1 = r;
		result[0] = items[c0];
		items = [.. items.Where((_, i) => i != c0)];
		result[1] = items[c1];
		result[2] = items.First(e => e != result[0] && e != result[1]);
		return result;
	}
}
