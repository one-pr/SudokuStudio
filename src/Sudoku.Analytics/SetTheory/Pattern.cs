namespace Sudoku.SetTheory;

/// <summary>
/// Represents a pattern, defining sets of truths and links.
/// </summary>
/// <remarks>
/// This type uses 400 bytes.
/// </remarks>
public readonly struct Pattern : IEquatable<Pattern>, IEqualityOperators<Pattern, Pattern, bool>
{
	/// <summary>
	/// Indicates truths and links.
	/// </summary>
	private readonly SpaceSet _truths, _links;

	/// <summary>
	/// Indicates all candidates used in truths.
	/// </summary>
	private readonly CandidateMap _map;

	/// <summary>
	/// Indicates all candidates used in both truths and links.
	/// </summary>
	private readonly CandidateMap _mapIncludingLinks;

	/// <summary>
	/// Indicates original grid.
	/// </summary>
	private readonly Grid _originalGrid;


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
	/// Indicates truths.
	/// </summary>
	[UnscopedRef]
	public ref readonly SpaceSet Truths => ref _truths;

	/// <summary>
	/// Indicates links.
	/// </summary>
	[UnscopedRef]
	public ref readonly SpaceSet Links => ref _links;

	/// <summary>
	/// Indicates all candidates used in truths.
	/// </summary>
	[UnscopedRef]
	public ref readonly CandidateMap Map => ref _map;

	/// <summary>
	/// Indicates all candidates used in both truths and links.
	/// </summary>
	[UnscopedRef]
	public ref readonly CandidateMap FullMap => ref _mapIncludingLinks;

	/// <summary>
	/// Indicates original grid.
	/// </summary>
	[UnscopedRef]
	public ref readonly Grid Grid => ref _originalGrid;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Pattern comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(in Pattern other) => Truths == other.Truths && Links == other.Links;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Truths, Links);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"T{_truths.Count} = {_truths}, L{_links.Count} = {_links}";

	/// <inheritdoc/>
	bool IEquatable<Pattern>.Equals(Pattern other) => Equals(other);


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
