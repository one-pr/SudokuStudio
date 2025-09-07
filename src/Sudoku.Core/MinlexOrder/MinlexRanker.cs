namespace Sudoku.MinlexOrder;

/// <summary>
/// Represents a ranker type that can rank a min-lex grid, or unrank a value to target grid.
/// </summary>
public static partial class MinlexRanker
{
	/// <summary>
	/// Provides extension members on <see cref="Grid"/>.
	/// </summary>
	extension(Grid)
	{
		/// <summary>
		/// Indicates the minimum value in min-lex representation, in string.
		/// </summary>
		public static string MinlexMinValueGridString => Grid.MinlexMinValueGrid.ToString("0");

		/// <summary>
		/// Indicates the maximum value in min-lex representation, in string.
		/// </summary>
		public static string MinlexMaxValueGridString => Grid.MinlexMaxValueGrid.ToString("0");

		/// <summary>
		/// Indicates the minimum value in min-lex representation.
		/// </summary>
		public static Grid MinlexMinValueGrid
		{
			get
			{
				return getMinValue<Grid>();


				static T getMinValue<T>() where T : IMinMaxValue<T> => T.MinValue;
			}
		}

		/// <summary>
		/// Indicates the maximum value in min-lex representation.
		/// </summary>
		public static Grid MinlexMaxValueGrid => Grid.Parse("123456789457893612986217354274538196531964827698721435342685971715349268869172543");
	}


	/// <summary>
	/// Gets the rank of the specified grid.
	/// </summary>
	/// <param name="grid">The min-lex grid.</param>
	/// <returns>
	/// The rank returned, or 0 if the source grid is invalid or not min-lex.
	/// </returns>
	public static unsafe ulong GetRank(string grid)
	{
		if (Interop.SkvcatSetModeGetVCDESK(2, out _) != 0)
		{
			return 0;
		}

		var givens = (sbyte*)Marshal.StringToHGlobalAnsi(grid);
		var result = Interop.SkvcatGetRankFromSolCharMin(givens);
		Marshal.FreeHGlobal((nint)givens);
		return result;
	}

	/// <summary>
	/// Gets the rank of the specified grid. If the grid is not min-lex, it will transform to min-lex state and calculate rank;
	/// if the grid is not solved, it will be solved internally without returning 0.
	/// </summary>
	/// <param name="grid">The grid with 81 digit characters.</param>
	/// <param name="minlexGrid">The min-lex state if the argument <paramref name="grid"/> is not min-lex.</param>
	/// <param name="transform">
	/// <para>The transform that the grid can be transform from min-lex state to <paramref name="grid"/>.</para>
	/// <para>
	/// If the grid is not min-lex, it will transform to min-lex grid
	/// and assign its transformation to parameter <paramref name="transform"/>;
	/// otherwise, <see cref="GenericTransform.Equivalent"/> will be returned.
	/// </para>
	/// </param>
	/// <returns>
	/// The rank returned, or 0 if the source grid is invalid (not a grid string of 81 digit characters).
	/// </returns>
	/// <seealso cref="GenericTransform.Equivalent"/>
	public static unsafe ulong GetRank(string grid, out string? minlexGrid, out GenericTransform transform)
	{
		if (grid.Length != 81)
		{
			goto InvalidGrid;
		}

		if (grid.ContainsAny('.', '0'))
		{
			var buffer = stackalloc char[82];
			buffer[81] = '\0';
			if (new BitwiseSolver().SolveString(grid, buffer, 2) != 1)
			{
				goto InvalidGrid;
			}
			grid = new(buffer);
		}

		minlexGrid = new MinlexFinder().Find(grid, out transform);
		if (Interop.SkvcatSetModeGetVCDESK(2, out _) != 0)
		{
			return 0;
		}

		var givens = (sbyte*)Marshal.StringToHGlobalAnsi(minlexGrid);
		var result = Interop.SkvcatGetRankFromSolCharMin(givens);
		Marshal.FreeHGlobal((nint)givens);
		return result;

	InvalidGrid:
		transform = default;
		minlexGrid = null;
		return 0;
	}

	/// <summary>
	/// Gets the min-lex form from the specified rank.
	/// </summary>
	/// <param name="rank">The desired rank.</param>
	/// <returns>
	/// The grid returned, with 81 digit characters representing the target grid.
	/// If argument <paramref name="rank"/> is invalid, <see langword="null"/> will be returned.
	/// </returns>
	public static unsafe string? GetGrid(ulong rank)
	{
		var resultCharacters = stackalloc sbyte[82];
		resultCharacters[81] = 0;

		if (Interop.SkvcatSetModeGetVCDESK(1, out var pvcdesc) != 0)
		{
			return null;
		}

		if (Interop.SkvcatFinSolForRank(rank) != 0)
		{
			return null;
		}

		Unsafe.CopyBlock(resultCharacters, pvcdesc->g.b1, sizeof(sbyte) * 81);
		return Encoding.ASCII.GetString((byte*)resultCharacters, 81);
	}
}
