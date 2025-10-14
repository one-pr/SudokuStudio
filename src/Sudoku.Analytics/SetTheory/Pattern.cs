namespace Sudoku.SetTheory;

/// <summary>
/// Represents a pattern, defining sets of truths and links.
/// </summary>
/// <remarks>
/// This type uses 496 bytes.
/// </remarks>
public struct Pattern : IEquatable<Pattern>, IEqualityOperators<Pattern, Pattern, bool>
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
	/// Initializes a <see cref="Pattern"/> instance via the specified truths, links and original grid.
	/// </summary>
	/// <param name="truths"><inheritdoc cref="Truths" path="/summary"/></param>
	/// <param name="links"><inheritdoc cref="Links" path="/summary"/></param>
	/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
	public Pattern(in SpaceSet truths, in SpaceSet links, in Grid grid)
	{
		_truths = truths;
		_links = links;
		_map = BuildMap(in truths, in links, in grid, out _mapIncludingLinks);
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
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Pattern comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(in Pattern other)
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

	/// <inheritdoc/>
	readonly bool IEquatable<Pattern>.Equals(Pattern other) => Equals(other);


	/// <summary>
	/// Creates a <see cref="CandidateMap"/> via the specified truths.
	/// </summary>
	/// <param name="truths">The truths.</param>
	/// <param name="links">The links.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="mapIncludingLinks">The map including links.</param>
	/// <returns>The candidates used only in truths.</returns>
	private static CandidateMap BuildMap(
		ref readonly SpaceSet truths,
		ref readonly SpaceSet links,
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
		foreach (var link in links)
		{
			mapIncludingLinks |= link.GetAvailableRange(grid);
		}
		return result;
	}


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in Pattern left, in Pattern right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in Pattern left, in Pattern right) => !(left == right);

	/// <inheritdoc/>
	static bool IEqualityOperators<Pattern, Pattern, bool>.operator ==(Pattern left, Pattern right) => left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<Pattern, Pattern, bool>.operator !=(Pattern left, Pattern right) => left != right;
}
