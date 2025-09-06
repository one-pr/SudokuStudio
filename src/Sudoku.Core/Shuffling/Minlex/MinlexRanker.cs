namespace Sudoku.Shuffling.Minlex;

/// <summary>
/// Represents a ranker type that can rank a min-lex grid, or unrank a value to target grid.
/// </summary>
public static class MinlexRanker
{
	/// <summary>
	/// Gets the rank of the specified grid.
	/// </summary>
	/// <param name="grid">The grid, min-lex.</param>
	/// <param name="transform">
	/// <para>The transform that the grid can be transform from min-lex state to <paramref name="grid"/>.</para>
	/// <para>
	/// If the grid is not min-lex, it will transform to min-lex grid
	/// and assign its transformation to parameter <paramref name="transform"/>;
	/// otherwise, <see cref="GenericTransform.Equivalent"/> will be returned.
	/// </para>
	/// </param>
	/// <returns>
	/// The rank returned, or 0 if the source grid is invalid (not a grid string of 81 digit characters, without empty cells).
	/// </returns>
	public static unsafe ulong GetRankFromGrid(string grid, out GenericTransform transform)
	{
		new MinlexFinder().Find(grid, out transform);
		VCDESC* pvcdesc;
		if (MinlexRankerInterop.SkvcatSetModeGetVCDESK(2, &pvcdesc) != 0)
		{
			return 0;
		}

		var givens = stackalloc sbyte[81];
		for (var i = 0; i < 81; i++)
		{
			givens[i] = (sbyte)grid[i];
		}
		return MinlexRankerInterop.SkvcatGetRankFromSolCharMin(givens);
	}

	/// <summary>
	/// Gets the min-lex form from the specified rank.
	/// </summary>
	/// <param name="rank">The desired rank.</param>
	/// <returns>
	/// The grid returned, with 81 digit characters representing the target grid.
	/// If argument <paramref name="rank"/> is invalid, <see langword="null"/> will be returned.
	/// </returns>
	public static unsafe string? GetGridFromRank(ulong rank)
	{
		VCDESC* pvcdesc;
		var resultCharacters = stackalloc sbyte[82];
		resultCharacters[81] = 0;

		if (MinlexRankerInterop.SkvcatSetModeGetVCDESK(1, &pvcdesc) != 0)
		{
			return null;
		}

		if (MinlexRankerInterop.SkvcatFinSolForRank(rank) != 0)
		{
			return null;
		}

		var wsSpan = new Span<sbyte>(resultCharacters, 81);
		new Span<sbyte>(pvcdesc->g.b1, 81).CopyTo(wsSpan);
		return (from e in wsSpan select (char)e).ToString();
	}
}
