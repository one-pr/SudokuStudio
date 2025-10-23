namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of methods that can calculate maximal empty area of a list of cells.
/// </summary>
/// <remarks>
/// This algorithm is called "Maximal Rectangle", using Dynamic Programming.
/// Please check <see href="https://leetcode.com/problems/maximal-rectangle">this link</see> to learn more details
/// about this problem, displayed in LeetCode.
/// See also "Maximal Square" for <see href="https://leetcode.com/problems/maximal-square">this link</see>.
/// Maximal rectangle problem is a little bit harder than maximal square.
/// </remarks>
/// <seealso href="https://leetcode.com/problems/maximal-rectangle">"Maximal Rectangle" Problem</seealso>
/// <seealso href="https://leetcode.com/problems/maximal-square">"Maximum Square" Problem</seealso>
public static class MaximalEmptyArea
{
	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Find maximal rectangle that covers empty cells as many as possible in a grid,
		/// and returns the size of the rectangle. Use <paramref name="topLeftCell"/> to get its top-left cell of the rectangle.
		/// </summary>
		/// <param name="topLeftCell">Indicates the top-left cell of the rectangle.</param>
		/// <returns>A <see cref="Cell"/> value indicating the result.</returns>
		public Cell GetMaxEmptyArea(out Cell topLeftCell) => @this.EmptyCells.GetMaxEmptyArea(out topLeftCell);

		/// <summary>
		/// Find maximal square that covers empty cells as many as possible in a grid,
		/// and returns the size of the rectangle. Use <paramref name="topLeftCell"/> to get its top-left cell of the square.
		/// </summary>
		/// <param name="topLeftCell">Indicates the top-left cell of the square.</param>
		/// <returns>A <see cref="Cell"/> value indicating the result.</returns>
		public Cell GetMaxEmptySquareArea(out Cell topLeftCell) => @this.EmptyCells.GetMaxEmptySquareArea(out topLeftCell);
	}

	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="CellMap"/>.
	/// </summary>
	extension(in CellMap @this)
	{
		/// <inheritdoc cref="extension(in Grid).GetMaxEmptyArea(out Cell)"/>
		public Cell GetMaxEmptyArea(out Cell topLeftCell)
		{
			var dp = (stackalloc Cell[9]);
			dp.Clear();

			(topLeftCell, var max) = (-1, 0);
			for (var i = 0; i < 9; i++)
			{
				for (var j = 0; j < 9; j++)
				{
					dp[j] = @this.Contains(i * 9 + j) ? dp[j] + 1 : 0;
				}

				var (currentMax, currentTopLeft) = getMaxRow(dp, i);
				if (currentMax > max)
				{
					max = currentMax;
					topLeftCell = currentTopLeft;
				}
			}
			return max;


			static (Cell, Cell) getMaxRow(ReadOnlySpan<Cell> height, RowIndex row)
			{
				var (stack, max, topLeft) = (new Stack<Cell>(), 0, -1);
				for (var i = 0; i <= 9; i++)
				{
					var h = i == 9 ? 0 : height[i];
					while (stack.Count != 0 && height[stack.Peek()] >= h)
					{
						var maxHeight = height[stack.Pop()];
						var width = stack.Count == 0 ? i : i - 1 - stack.Peek();
						var area = maxHeight * width;
						if (area > max)
						{
							max = area;
							topLeft = (row - maxHeight + 1) * 9 + (stack.Count == 0 ? 0 : stack.Peek() + 1);
						}
					}
					stack.Push(i);
				}
				return (max, topLeft);
			}
		}

		/// <inheritdoc cref="extension(in Grid).GetMaxEmptySquareArea(out Cell)"/>
		public Cell GetMaxEmptySquareArea(out Cell topLeftCell)
		{
			(topLeftCell, var maxSide) = (-1, 0);
			var dp = (stackalloc Cell[81]);

			for (var i = 0; i < 9; i++)
			{
				for (var j = 0; j < 9; j++)
				{
					var index = i * 9 + j;
					if (@this.Contains(index))
					{
						if (i == 0 || j == 0)
						{
							dp[index] = 1;
						}
						else
						{
							var up = dp[(i - 1) * 9 + j];
							var left = dp[i * 9 + (j - 1)];
							var upLeft = dp[(i - 1) * 9 + (j - 1)];
							dp[index] = Math.Min(up, left, upLeft) + 1;
						}
					}
					else
					{
						dp[index] = 0;
					}

					var currentSide = dp[index];
					if (currentSide > maxSide)
					{
						maxSide = currentSide;
						var topRow = i - maxSide + 1;
						var leftCol = j - maxSide + 1;
						topLeftCell = topRow * 9 + leftCol;
					}
					else if (currentSide == maxSide && currentSide > 0)
					{
						var currentTopRow = i - currentSide + 1;
						var currentLeftCol = j - currentSide + 1;
						var currentTopIndex = currentTopRow * 9 + currentLeftCol;

						// To choose the minimal index of the top-left cell.
						if (currentTopRow < topLeftCell / 9 || currentTopRow == topLeftCell / 9 && currentLeftCol < topLeftCell % 9)
						{
							topLeftCell = currentTopIndex;
						}
					}
				}
			}

			return maxSide * maxSide;
		}
	}
}
