namespace Sudoku.Ranking;

/// <summary>
/// Represents an object that can calculate rank-related information via the specified grid, truths and links.
/// </summary>
/// <param name="grid">The grid.</param>
/// <param name="truths">The truths.</param>
/// <param name="links">The links.</param>
/// <remarks>
/// <para>Design of this type is a little bit different with XSudo on links.</para>
/// <para>
/// This type only uses truths to find assignment combinations, and checks eliminations after combinations.
/// The reason why this type doesn't use any links is that some nodes will become false, once an assignment is true.
/// This rule will break satisfiability on links.
/// </para>
/// <para>
/// In XSudo, links will obey a rule "all candidates pairs on link cannot be both true",
/// but it will be excluded automatically by design of this algorithm.
/// We should append an extra check on "both true" to candidates in a same link if we want to trim links.
/// </para>
/// </remarks>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.EqualityOperators, IsLargeStructure = true)]
public readonly ref partial struct RankPattern(ref readonly Grid grid, ref readonly SpaceSet truths, ref readonly SpaceSet links) :
	IEquatable<RankPattern>
{
	/// <summary>
	/// Indicates the grid.
	/// </summary>
	public readonly ref readonly Grid Grid = ref grid;

	/// <summary>
	/// Indicates the truths.
	/// </summary>
	public readonly ref readonly SpaceSet Truths = ref truths;

	/// <summary>
	/// Indicates the links.
	/// </summary>
	/// <remarks>
	/// By design, this value can be empty if you want to infer this value.
	/// </remarks>
	public readonly ref readonly SpaceSet Links = ref links;

	/// <summary>
	/// Represents all candidates used in this pattern.
	/// </summary>
	private readonly CandidateMap _candidates = BuildCandidates(in grid, in truths, in links);


	/// <summary>
	/// [Not supported] Provides parameterless constructor of this type.
	/// </summary>
	[Obsolete("Do not use parameterless constructor. If you want to create a default value, use 'default' literal instead.", true)]
	public RankPattern() : this(in Grid.nullref, in SpaceSet.nullref, in SpaceSet.nullref) => throw new NotSupportedException();

	/// <summary>
	/// Initializes a <see cref="RankPattern"/> instance via the grid and truths.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="truths">The truths.</param>
	public RankPattern(ref readonly Grid grid, ref readonly SpaceSet truths) : this(in grid, in truths, in SpaceSet.Empty)
	{
	}


	/// <summary>
	/// Indicates all cells used.
	/// </summary>
	public CellMap Cells => _candidates.Cells;

	/// <summary>
	/// Indicates the candidates.
	/// </summary>
	[UnscopedRef]
	public ref readonly CandidateMap Candidates => ref _candidates;


	/// <inheritdoc/>
	public bool Equals(in RankPattern other) => Grid == other.Grid && Truths == other.Truths && Links == other.Links;

	/// <inheritdoc/>
	bool IEquatable<RankPattern>.Equals(RankPattern other) => Equals(other);


	/// <summary>
	/// Build candidates.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="truths">The truths.</param>
	/// <param name="links">The links.</param>
	private static CandidateMap BuildCandidates(ref readonly Grid grid, ref readonly SpaceSet truths, ref readonly SpaceSet links)
	{
		var result = CandidateMap.Empty;

		var candidatesMap = grid.CandidatesMap;
		foreach (var truth in truths)
		{
			switch (truth)
			{
				case { IsCellRelated: true, Cell: var cell }:
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						result.Add(cell * 9 + digit);
					}
					break;
				}
				case { IsHouseRelated: true, House: var house, Digit: var digit }:
				{
					foreach (var cell in HousesMap[house] & candidatesMap[digit])
					{
						result.Add(cell * 9 + digit);
					}
					break;
				}
			}
		}

		return result;
	}
}
