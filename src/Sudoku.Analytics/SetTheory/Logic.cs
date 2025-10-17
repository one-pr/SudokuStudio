namespace Sudoku.SetTheory;

/// <summary>
/// Represents a logic pattern, defining sets of truths and links.
/// </summary>
/// <remarks>
/// This type uses 496 bytes.
/// </remarks>
public struct Logic : IEquatable<Logic>, IEqualityOperators<Logic, Logic, bool>
{
	/// <summary>
	/// Indicates original grid.
	/// </summary>
	private readonly Grid _originalGrid;

	/// <summary>
	/// Indicates truths and links.
	/// </summary>
	private SpaceSet _truths, _links;

	/// <summary>
	/// Indicates all candidates used in truths and links.
	/// </summary>
	private CandidateMap _map, _mapIncludingLinks;


	/// <summary>
	/// Initializes a <see cref="Logic"/> instance via the specified truths, links and original grid.
	/// </summary>
	/// <param name="truths"><inheritdoc cref="Truths" path="/summary"/></param>
	/// <param name="links"><inheritdoc cref="Links" path="/summary"/></param>
	/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
	public Logic(in SpaceSet truths, in SpaceSet links, in Grid grid)
	{
		_truths = truths;
		_links = links;
		_map = RebuildMap(in truths, ref _links, in grid, out _mapIncludingLinks);
		_originalGrid = grid;
	}


	/// <summary>
	/// Indicates whether all candidates used in truths are exact-covered or not.
	/// </summary>
	public readonly bool IsExactCovered => ExactCoveredCandidates == Map;

	/// <summary>
	/// Indicates the number of candidates used in the pattern.
	/// Candidates from links (but not from truths) will be ignored.
	/// </summary>
	public readonly int CandidatesCount => Map.Count;

	/// <summary>
	/// Indicates truths.
	/// </summary>
	[UnscopedRef]
	public readonly ref readonly SpaceSet Truths => ref _truths;

	/// <summary>
	/// Indicates links.
	/// </summary>
	[UnscopedRef]
	public readonly ref readonly SpaceSet Links => ref _links;

	/// <summary>
	/// Indicates all candidates used in truths.
	/// </summary>
	[UnscopedRef]
	public readonly ref readonly CandidateMap Map => ref _map;

	/// <summary>
	/// Indicates all candidates used in both truths and links.
	/// </summary>
	[UnscopedRef]
	public readonly ref readonly CandidateMap FullMap => ref _mapIncludingLinks;

	/// <summary>
	/// Find for exact-covered candidates in the pattern.
	/// </summary>
	/// <returns>All exact-covered candidates.</returns>
	public readonly CandidateMap ExactCoveredCandidates
	{
		get
		{
			var result = CandidateMap.Empty;
			foreach (var candidate in Map)
			{
				if (GetCoveredSetsCount(candidate).IsExactCovered)
				{
					result.Add(candidate);
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates original grid.
	/// </summary>
	[UnscopedRef]
	public readonly ref readonly Grid Grid => ref _originalGrid;


	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Logic comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(in Logic other)
		=> Map == other.Map && FullMap == other.FullMap
		&& Grid == other.Grid
		&& Truths == other.Truths && Links == other.Links;

	/// <inheritdoc/>
	public readonly override int GetHashCode()
		=> HashCode.Combine(
			Map.GetHashCode(),
			FullMap.GetHashCode(),
			Grid.GetHashCode(),
			Truths.GetHashCode(),
			Links.GetHashCode()
		);

	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString() => $"T{_truths.Count} = {_truths}, L{_links.Count} = {_links}";

	/// <summary>
	/// Totals up how many truths and links covered for a specified candidate.
	/// </summary>
	/// <param name="candidate">The candidate to check.</param>
	/// <returns>A pair of numbers indicating that.</returns>
	public readonly CoveredSetsCount GetCoveredSetsCount(Candidate candidate)
	{
		var truthsCount = 0;
		var linksCount = 0;
		foreach (var truth in Truths)
		{
			if (truth.Contains(candidate))
			{
				truthsCount++;
			}
		}
		foreach (var link in Links)
		{
			if (link.Contains(candidate))
			{
				linksCount++;
			}
		}
		return new(truthsCount, linksCount);
	}

	/// <summary>
	/// Add a new truth to the pattern.
	/// </summary>
	/// <param name="truth">The truth to add.</param>
	public void AddTruth(Space truth)
	{
		if (_truths.Add(truth))
		{
			_map = RebuildMap(in _truths, ref _links, in _originalGrid, out _mapIncludingLinks);
		}
	}

	/// <summary>
	/// Add a new link to the pattern.
	/// </summary>
	/// <param name="link">The link to add.</param>
	public void AddLink(Space link)
	{
		if (_links.Add(link))
		{
			_map = RebuildMap(in _truths, ref _links, in _originalGrid, out _mapIncludingLinks);
		}
	}

	/// <summary>
	/// Remove a link from the pattern.
	/// </summary>
	/// <param name="truth">The truth to remove.</param>
	public void RemoveTruth(Space truth)
	{
		if (_truths.Remove(truth))
		{
			_map = RebuildMap(in _truths, ref _links, in _originalGrid, out _mapIncludingLinks);
		}
	}

	/// <summary>
	/// Remove a link from the pattern.
	/// </summary>
	/// <param name="link">The link to remove.</param>
	public void RemoveLink(Space link)
	{
		if (_links.Remove(link))
		{
			_map = RebuildMap(in _truths, ref _links, in _originalGrid, out _mapIncludingLinks);
		}
	}

	/// <inheritdoc/>
	readonly bool IEquatable<Logic>.Equals(Logic other) => Equals(other);


	/// <summary>
	/// Creates a <see cref="CandidateMap"/> via the specified truths and links.
	/// </summary>
	/// <param name="truths">The truths.</param>
	/// <param name="links">The links. Isolated links are ignored to be added.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="mapIncludingLinks">The map including links.</param>
	/// <returns>The candidates used only in truths.</returns>
	private static CandidateMap RebuildMap(
		ref readonly SpaceSet truths,
		ref SpaceSet links,
		ref readonly Grid grid,
		out CandidateMap mapIncludingLinks
	)
	{
		(mapIncludingLinks, var result) = (CandidateMap.Empty, CandidateMap.Empty);
		foreach (var truth in truths)
		{
			var map = truth.GetAvailableRange(grid);
			result |= map;
			mapIncludingLinks |= map;
		}

		// Ignore links that is already collected in truths.
		links &= ~truths;
		var tempLinks = links;
		foreach (var link in tempLinks)
		{
			// Check whether any links are isolated - no candidates connected to map.
			var map = link.GetAvailableRange(grid);
			if (!(map & result))
			{
				// Isolated link.
				links -= link;
				continue;
			}
			mapIncludingLinks |= map;
		}
		return result;
	}


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in Logic left, in Logic right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in Logic left, in Logic right) => !(left == right);

	/// <inheritdoc/>
	static bool IEqualityOperators<Logic, Logic, bool>.operator ==(Logic left, Logic right) => left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<Logic, Logic, bool>.operator !=(Logic left, Logic right) => left != right;
}
