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
	/// <param name="grid">
	/// The grid that represents overall available candidates (i.e. <see cref="CandidateMap"/>)
	/// - but only bits that appear in at least one set are actually added as DLX rows by default.
	/// </param>
	/// <param name="truths">Lists of <see cref="CandidateMap"/> describing which candidates belong to each set.</param>
	/// <param name="links">Lists of <see cref="CandidateMap"/> describing which candidates belong to each set.</param>
	/// <param name="maxSolutions">The limit maximum solutions can be found.</param>
	/// <param name="includeUnconstrained">
	/// Indicates whether unconstrained candidates will be included to check.
	/// By default we <b>skip</b> candidates that are not present in any set (to avoid combinatorial explosion).
	/// If you really want to include unconstrained candidates (combinatorial), set <see langword="true"/> (dangerous).
	/// </param>
	/// <returns>
	/// List of solutions; each solution is a <see cref="Permutation"/> of assigned candidates (0..728).
	/// </returns>
	/// <seealso cref="CandidateMap"/>
	/// <seealso cref="Permutation"/>
	public static ReadOnlySpan<Permutation> Solve(
		in Grid grid,
		in SpaceSet truths,
		in SpaceSet links,
		int maxSolutions = int.MaxValue,
		bool includeUnconstrained = false
	)
	{
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
		return Solve(fullMap, truthMaps, linkMaps, maxSolutions, includeUnconstrained);
	}

	/// <inheritdoc cref="Solve(in Grid, in SpaceSet, in SpaceSet, int, bool)"/>
	public static ReadOnlySpan<Permutation> Solve(in Pattern pattern, int maxSolutions = int.MaxValue, bool includeUnconstrained = false)
		=> Solve(pattern.Grid, pattern.Truths, pattern.Links, maxSolutions, includeUnconstrained);

	/// <summary>
	/// The backing method.
	/// </summary>
	private static ReadOnlySpan<Permutation> Solve(
		scoped in CandidateMap fullMap,
		List<CandidateMap> truths,
		List<CandidateMap> links,
		int maxSolutions = int.MaxValue,
		bool includeUnconstrained = false
	)
	{
		var truthsCount = truths.Count;
		var linksCount = links.Count;
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

		// Decide which candidate to actually add as rows:
		var mapToConsider = includeUnconstrained
			// Include every candidate allowed by 'fullMap'.
			? fullMap
			// Only include candidates that appear in at least one provided set.
			: [.. columnsLookup.Keys];

		// Add rows: each candidate present in candidateMap and considered will be a row.

		// We will use 'rowId' == 'candidateIndex' for simplicity.
		var rowId = 0;
		foreach (var candidate in mapToConsider)
		{
			if (!fullMap.Contains(candidate))
			{
				// Skip if not actually available.
				continue;
			}

			if (!columnsLookup.TryGetValue(candidate, out var columns) || columns is null || columns.Count == 0)
			{
				if (!includeUnconstrained)
				{
					// Skip unconstrained candidate when 'includeUnconstrained' is false.
					continue;
				}

				// Row with no columns (free choice).
				dlx.AddRow(candidate, []);
			}
			else
			{
				dlx.AddRow(candidate, columns);
			}
			rowId++;
		}

		// Quick feasibility check:
		// If any primary column has size 0 -> no solution.
		// We can inspect columns by trying to cover via a small helper (not exposing internals).
		// For simplicity, we skip extra check here; DLX will simply produce no results.

		var rawSolutions = dlx.Solve(maxSolutions);

		// Raw solutions are arrays of 'rowIds' which we set equal to candidates, so directly add them into result collection.
		var results = new List<Permutation>();
		foreach (var solution in rawSolutions)
		{
			var arr = solution.ToArray();
#if SORT_SOLUTION_ASSIGNMENTS
			Array.Sort(arr);
#endif
			results.Add(new(arr));
		}

		return results.AsSpan();
	}
}
