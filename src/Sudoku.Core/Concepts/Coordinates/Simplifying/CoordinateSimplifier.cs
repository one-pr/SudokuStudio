namespace Sudoku.Concepts.Coordinates.Simplifying;

/// <summary>
/// Represents a simplifier type that can simplify the coordinates.
/// </summary>
public static class CoordinateSimplifier
{
	/// <summary>
	/// Try to simplify parts of cells, by combining same rows and columns.
	/// </summary>
	/// <param name="cells">The cells to be simplified.</param>
	/// <returns>A list of parts grouped by rows and its matched columns.</returns>
	public static ReadOnlySpan<CoordinateSplit> Simplify(in CellMap cells)
		=> cells switch
		{
			// The collection is empty.
			[] => [],

			// The collection has only one element.
			[var onlyCell] => (CoordinateSplit[])[new([onlyCell / 9], [onlyCell % 9])],

			// The collection can be formed a valid biclique.
			{ RowMask: var r, ColumnMask: var c } when PopCount((uint)r) * PopCount((uint)c) == cells.Count
				=> (CoordinateSplit[])[new([.. r.AllSets], [.. c.AllSets])],

			// Otherwise.
			_ when SolveMinPartition(cells) is var (_, parts) => parts.AsSpan()
		};

	/// <summary>
	/// Enumerate all bicliques (row-set * column-set rectangles) that are fully contained in the given set of cells.
	/// </summary>
	/// <param name="cells">Input cell set represented as <see cref="CellMap"/>.</param>
	/// <returns>
	/// A span of <see cref="Biclique"/> objects. Each biclique corresponds to a non-empty set of rows and a
	/// non-empty set of columns such that every combination of these rows and columns is present in <paramref name="cells"/>.
	/// </returns>
	/// <remarks>
	/// The algorithm:
	/// <list type="number">
	/// <item>Build per-row column masks (which columns exist in each row).</item>
	/// <item>
	/// For each non-empty subset of rows, compute the intersection of their column masks
	/// to get "commonColumns" that appear in every selected row.
	/// </item>
	/// <item>Enumerate non-empty subsets of those common columns to produce bicliques.</item>
	/// </list>
	/// This solves a kind of problem called "Minimum Edge Biclique Partition Problem", which is NP-Hard.
	/// </remarks>
	private static ReadOnlySpan<Biclique> EnumerateBicliques(in CellMap cells)
	{
		// Per-row 9-bit masks describing which columns in each row are present in <paramref name="cells"/>.
		var rowMasks = (stackalloc Mask[9]);
		rowMasks.Clear();

		// Build 'rowMasks': for each cell set the corresponding bit in that row.
		foreach (var cell in cells)
		{
			// 'cell / 9' -> row index, 'cell % 9' -> column index
			rowMasks[cell / 9] |= (Mask)(1 << cell % 9);
		}

		var bicliques = new List<Biclique>();

		// Iterate all non-empty subsets of rows.
		// For each chosen row subset, compute columns common to all those rows,
		// then enumerate non-empty subsets of those common columns.
		for (var rowMask = (Mask)1; rowMask <= Grid.MaxCandidatesMask; rowMask++)
		{
			var commonColumns = Grid.MaxCandidatesMask;
			var rowsList = new List<RowIndex>();
			for (var row = 0; row < 9; row++)
			{
				if (((rowMask >> row) & 1) != 0)
				{
					// Intersect columns: only columns present in all selected rows remain.
					commonColumns &= rowMasks[row];
					rowsList.Add(row);
				}
			}
			if (commonColumns == 0)
			{
				// No columns are common for this row subset, skip.
				continue;
			}

			// Collect indices of columns that are common to the row subset.
			var columnsPositions = new List<ColumnIndex>();
			for (var column = 0; column < 9; column++)
			{
				if (((commonColumns >> column) & 1) != 0)
				{
					columnsPositions.Add(column);
				}
			}

			// Enumerate all non-empty subsets of these columns to create bicliques.
			var k = columnsPositions.Count;
			for (var mask = (Mask)1; mask < 1 << k; mask++)
			{
				var columnMask = (Mask)0;
				var columnsList = new List<ColumnIndex>();
				for (var j = 0; j < k; j++)
				{
					if (((mask >> j) & 1) != 0)
					{
						var c = columnsPositions[j];
						columnMask |= (Mask)(1 << c);
						columnsList.Add(c);
					}
				}

				// Build the Covered CellMap as the Cartesian product of <c>rowsList</c> and <c>columnsList</c>.
				var coveredCells = CellMap.Empty;
				foreach (var row in rowsList)
				{
					foreach (var column in columnsList)
					{
						coveredCells += row * 9 + column;
					}
				}

				// Heuristic printing cost: 'r' + rows digits + 'c' + columns digits.
				var cost = rowsList.Count + columnsList.Count + 2;
				bicliques.Add(new(rowMask, columnMask, coveredCells, cost, [.. rowsList], [.. columnsList]));
			}
		}

		// Sort by coverage descending then cost ascending. This ordering helps greedy heuristics and search.
		bicliques.Sort(
			static (left, right) =>
			{
				var leftCost = left.Covered.Count;
				var rightCost = right.Covered.Count;

				// First order by descending covered count; on tie, choose smaller cost.
				return -leftCost.CompareTo(rightCost) is var r1 and not 0 ? r1 : left.Cost.CompareTo(right.Cost);
			}
		);
		return bicliques.AsSpan();
	}

	/// <summary>
	/// Solve the minimum-cost partition of the provided cells into disjoint bicliques.
	/// </summary>
	/// <param name="cells">Input set of cells to cover (must be subset of the grid).</param>
	/// <returns>
	/// A tuple containing:
	/// <list type="table">
	/// <item>
	/// <term>Cost</term>
	/// <description>The minimal total cost (according to our heuristic cost metric).</description>
	/// </item>
	/// <item>
	/// <term>Parts</term>
	/// <description>A list of <see cref="CoordinateSplit"/> objects representing the chosen bicliques.</description>
	/// </item>
	/// </list>
	/// </returns>
	/// <remarks>
	/// The implementation strategy:
	/// <list type="number">
	/// <item>Enumerate all valid bicliques (precomputation).</item>
	/// <item>Build a reverse index mapping from each cell to the bicliques that cover it.</item>
	/// <item>
	/// Run a recursive search with memoization (DFS + cache) where at each step we:
	/// <list type="bullet">
	/// <item>Pick an uncovered cell, iterate all bicliques that include it.</item>
	/// <item>Recurse with the remaining uncovered cells after removing biclique coverage.</item>
	/// <item>Use a greedy upper bound as pruning to reduce the search space.</item>
	/// </list>
	/// </item>
	/// </list>
	/// This yields an exact result for small instances in 9-by-9-sized grid;
	/// worst-case complexity is exponential (NP-hard problem).
	/// </remarks>
	private static (int Cost, List<CoordinateSplit> Parts) SolveMinPartition(in CellMap cells)
	{
		var bicliques = EnumerateBicliques(cells);

		// Build reverse-index: for each of the 81 cells, list biclique indices that cover it.
		var edgeToBicliqueIndices = new List<int>[81];
		Array.InitializeArray(edgeToBicliqueIndices, static ([NotNull] ref value) => value = []);

		for (var j = 0; j < bicliques.Length; j++)
		{
			foreach (var cell in bicliques[j].Covered)
			{
				edgeToBicliqueIndices[cell].Add(j);
			}
		}

		// Cache for memoization: map remaining CellMap -> best (cost, parts) found for it.
		var cached = new Dictionary<CellMap, (int Cost, List<int> Parts)>();
		var (cost, parts) = dfs(cells, bicliques);
		var partsIndices = parts;

		// If DFS returned an empty parts list but cost > 0 (happens when we returned greedy cost as fallback),
		// reconstruct an explicit greedy partition so we always return concrete parts.
		if (partsIndices is null or { Count: 0 } && cost > 0)
		{
			var tempParts = new List<int>();
			var rem = cells;
			while (rem)
			{
				var bestJ = -1;
				var bestScore = -1D;
				for (var j = 0; j < bicliques.Length; j++)
				{
					var covered = (bicliques[j].Covered & rem).Count;
					if (covered == 0)
					{
						continue;
					}

					// Score: ratio of covered cells to cost (simple greedy heuristic).
					var score = (double)covered / bicliques[j].Cost;
					if (score > bestScore)
					{
						bestScore = score;
						bestJ = j;
					}
				}
				if (bestJ == -1)
				{
					// If no biclique covers any remaining cell (should not happen), remove single cell to progress.
					rem -= rem[0];
					continue;
				}

				tempParts.Add(bestJ);
				rem &= ~(bicliques[bestJ].Covered & rem);
			}
			partsIndices = tempParts;
		}

		Debug.Assert(partsIndices is not null);

		// Build the result as CoordinateSplit objects (rows & columns lists).
		var result = new List<CoordinateSplit>();
		foreach (var j in partsIndices)
		{
			var b = bicliques[j];
			result.Add(new(b.Rows, b.Columns));
		}
		return (cost, result);



		static int greedyUpperBound(in CellMap tempCells, ReadOnlySpan<Biclique> bicliques)
		{
			// Compute a greedy (fast) upper bound for the minimal cost of covering <paramref name="tempCells"/>.
			// This is used to prune the exact search.
			//
			// The greedy algorithm repeatedly selects the biclique with the largest
			// (coveredCells / cost) ratio until all cells are covered.
			// The returned integer is the sum of costs of selected bicliques.

			var temp = tempCells;
			var parts = new List<int>();
			while (temp)
			{
				var bestJ = -1;
				var bestScore = -1D;
				for (var j = 0; j < bicliques.Length; j++)
				{
					var coveredCellsCount = (bicliques[j].Covered & temp).Count;
					if (coveredCellsCount == 0)
					{
						continue;
					}

					// Use coverage-to-cost ratio as heuristic.
					var score = (double)coveredCellsCount / bicliques[j].Cost;
					if (score > bestScore)
					{
						bestScore = score;
						bestJ = j;
					}
				}
				if (bestJ == -1)
				{
					// Fallback: remove a single remaining cell to make progress.
					temp -= temp[0];
					continue;
				}

				parts.Add(bestJ);
				// Remove the covered cells of the chosen biclique from the remaining set.
				temp &= ~(bicliques[bestJ].Covered & temp);
			}

			var result = 0;
			foreach (var part in parts)
			{
				result += bicliques[part].Cost;
			}
			return result;
		}

		(int Cost, List<int> Parts) dfs(in CellMap remainingCells, ReadOnlySpan<Biclique> bicliques)
		{
			// Depth-first search with memoization to find the optimal partition for the remaining cells.
			//   - At each recursion we pick an uncovered cell (rem[0]) and iterate over all bicliques that cover it.
			//   - We prune using a greedy upper bound for the current remainder.
			//   - Results are cached in the outer <c>cached</c> dictionary keyed by the remaining <see cref="CellMap"/>.

			// If nothing remains, cost is zero and no parts are needed.
			if (!remainingCells)
			{
				return (0, []);
			}
			if (cached.TryGetValue(remainingCells, out var value))
			{
				// Return cached result when available.
				return value;
			}

			// Compute an upper bound quickly to allow pruning.
			var upperBound = greedyUpperBound(remainingCells, bicliques);
			var bestCost = upperBound;
			var bestSolution = default(List<int>);

			// Choose a pivot cell (first set cell in <paramref name="remainingCells"/>) to branch on.
			var edgeIndex = remainingCells[0];
			foreach (var j in edgeToBicliqueIndices[edgeIndex])
			{
				var b = bicliques[j];
				var covered = b.Covered & remainingCells;
				if (!covered)
				{
					// This biclique covers no remaining cells (should not happen here) - skip.
					continue;
				}

				// Remove the biclique coverage and recurse.
				var newRem = remainingCells & ~covered;
				var (cost, parts) = dfs(newRem, bicliques);
				var cand = b.Cost + cost;
				if (cand < bestCost)
				{
					// Found a better solution; record it.
					bestCost = cand;

					// Prepend current biclique index to the child's parts to form a complete solution.
					bestSolution = [j, .. parts];
				}
			}

			if (bestSolution is null)
			{
				// If no solution improved over the greedy upper bound, store the upper bound and empty parts list.
				// The caller will reconstruct concrete greedy parts if needed.
				cached[remainingCells] = (upperBound, []);
				return cached[remainingCells];
			}

			// Cache and return the best exact solution found.
			cached[remainingCells] = (bestCost, bestSolution);
			return cached[remainingCells];
		}
	}
}
