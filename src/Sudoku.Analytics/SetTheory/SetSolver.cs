#undef SORT_SOLUTION_ASSIGNMENTS

namespace Sudoku.SetTheory;

/// <summary>
/// Represents an internal entry for dancing links solver.
/// </summary>
internal static class SetSolver
{
	/// <summary>
	/// Solve for all selections of candidates that satisfy:
	/// <list type="bullet">
	/// <item>Each truth: exactly one selected inside that set</item>
	/// <item>Each link: at most one selected inside that set</item>
	/// </list>
	/// </summary>
	/// <param name="pattern">The pattern to be used.</param>
	/// <param name="maxSolutions">The limit maximum solutions can be found.</param>
	/// <returns>
	/// List of solutions; each solution is a <see cref="Permutation"/> of assigned candidates (0..728).
	/// </returns>
	/// <seealso cref="Permutation"/>
	public static ReadOnlySpan<Permutation> Solve(in Pattern pattern, int maxSolutions = int.MaxValue)
	{
		ref readonly var truths = ref pattern.Truths;
		ref readonly var links = ref pattern.Links;
		ref readonly var grid = ref pattern.Grid;
		var truthMaps = new List<CandidateMap>();
		var linkMaps = new List<CandidateMap>();
		var fullMap = CandidateMap.Empty;
		foreach (var truth in truths)
		{
			var map = truth.GetAvailableRange(grid);
			truthMaps.AddRef(map);
			fullMap |= map;
		}
		foreach (var link in links)
		{
			linkMaps.AddRef(link.GetAvailableRange(grid) & fullMap);
		}
		return Solve(fullMap, truthMaps.AsSpan(), linkMaps.AsSpan(), maxSolutions);
	}

	/// <summary>
	/// The backing method.
	/// </summary>
	private static ReadOnlySpan<Permutation> Solve(
		scoped in CandidateMap fullMap,
		ReadOnlySpan<CandidateMap> truths,
		ReadOnlySpan<CandidateMap> links,
		int maxSolutions
	)
	{
		var truthsCount = truths.Length;
		var linksCount = links.Length;
		var allColumnsCount = truthsCount + linksCount;
		var isPrimary = new bool[allColumnsCount].AsSpan();
		isPrimary[..truthsCount].Fill(true);
		isPrimary[truthsCount..].Clear();

		var dlx = new DancingLinks(allColumnsCount, isPrimary);

		// Map from candidateIndex -> list of column indices (where it's present).
		var columnsLookup = new Dictionary<Candidate, List<Candidate>>();

		// Gather mapping for truths.
		for (var r = 0; r < truthsCount; r++)
		{
			foreach (var candidate in truths[r])
			{
				if (!columnsLookup.TryGetValue(candidate, out var list))
				{
					list = [];
					columnsLookup[candidate] = list;
				}
				list.Add(r); // Primary column index.
			}
		}

		// Gather mapping for links.
		for (var r = 0; r < linksCount; r++)
		{
			foreach (var candidate in links[r])
			{
				if (!columnsLookup.TryGetValue(candidate, out var list))
				{
					list = [];
					columnsLookup[candidate] = list;
				}
				list.Add(truthsCount + r);
			}
		}

		// Add rows: each candidate present in candidateMap and considered will be a row.

		// We will use 'rowId' == 'candidateIndex' for simplicity.
		var rowId = 0;
		foreach (var candidate in fullMap)
		{
			if (!fullMap.Contains(candidate))
			{
				// Skip if not actually available.
				continue;
			}

			dlx.AddRow(
				candidate,
				columnsLookup.TryGetValue(candidate, out var columns) && columns is { Count: not 0 }
					? columns.AsSpan()
					: []
			);
			rowId++;
		}

		// Quick feasibility check:
		// If any primary column has size 0 -> no solution.
		// We can inspect columns by trying to cover via a small helper (not exposing internals).
		// For simplicity, we skip extra check here; DLX will simply produce no results.
		return dlx.Solve(maxSolutions).AsSpan();
	}
}
