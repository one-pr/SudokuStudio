namespace Sudoku;

/// <summary>
/// Provides with solution-wide read-only fields used.
/// </summary>
public static class SolutionFields
{
	/// <summary>
	/// Indicates the possible house types to be iterated.
	/// </summary>
	public static readonly HouseType[] HouseTypes = [HouseType.Block, HouseType.Row, HouseType.Column];

	/// <summary>
	/// Indicates the digits used. The value can be also used for ordered houses by rows.
	/// </summary>
	public static readonly Digit[] Digits = [0, 1, 2, 3, 4, 5, 6, 7, 8];

	/// <summary>
	/// Indicates the houses ordered by column.
	/// </summary>
	public static readonly House[] HousesOrderedByColumn = [0, 3, 6, 1, 4, 7, 2, 5, 8];

	/// <summary>
	/// Indicates the first cell offset for each house.
	/// </summary>
	public static readonly Cell[] HouseFirst;

	/// <summary>
	/// <para>
	/// The map of all cell offsets in its specified house.
	/// The indices are between 0 and 26, where:
	/// <list type="table">
	/// <item>
	/// <term><c>0..9</c></term>
	/// <description>Block 1 to 9.</description>
	/// </item>
	/// <item>
	/// <term><c>9..18</c></term>
	/// <description>Row 1 to 9.</description>
	/// </item>
	/// <item>
	/// <term><c>18..27</c></term>
	/// <description>Column 1 to 9.</description>
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	/// <example>
	/// '<c>HouseCells[0]</c>': all cell offsets in the house 0 (block 1).
	/// </example>
	public static readonly Cell[][] HousesCells;

	/// <summary>
	/// Indicates a block list that each cell belongs to.
	/// </summary>
	internal static readonly BlockIndex[] BlockTable;

	/// <summary>
	/// Indicates a row list that each cell belongs to.
	/// </summary>
	internal static readonly RowIndex[] RowTable;

	/// <summary>
	/// Indicates a column list that each cell belongs to.
	/// </summary>
	internal static readonly ColumnIndex[] ColumnTable;

	/// <summary>
	/// Indicates a list of <see cref="CellMap"/> instances that are initialized as singleton element by its corresponding index.
	/// For example, <c>CellMaps[0]</c> is to <c>CellMap.Empty + 0</c>, i.e. <c>r1c1</c>.
	/// </summary>
	internal static readonly CellMap[] CellMaps;

#if CACHE_CANDIDATE_MAPS
	/// <summary>
	/// Indicates a list of <see cref="CandidateMap"/> instances that are initialized as singleton element by its corresponding index.
	/// For example, <c>CandidateMaps[0]</c> is to <c>CandidateMap.Empty + 0</c>, i.e. <c>r1c1(1)</c>.
	/// </summary>
	internal static readonly CandidateMap[] CandidateMaps;
#endif

	/// <summary>
	/// Backing field of <see cref="HousesMap"/>.
	/// </summary>
	private static readonly CellMap[] HousesMapBackingField;

	/// <summary>
	/// Backing field of <see cref="PeersMap"/>.
	/// </summary>
	private static readonly CellMap[] PeersMapBackingField;

	/// <summary>
	/// Backing field of <see cref="ChuteMaps"/>.
	/// </summary>
	private static readonly CellMap[] ChuteMapsBackingField;

	/// <summary>
	/// Backing field of <see cref="Chutes"/>.
	/// </summary>
	private static readonly Chute[] ChutesBackingField;


	/// <summary>
	/// Indicates a list of <see cref="CellMap"/> instances representing the cells belong to a house at the specified index.
	/// </summary>
	public static ReadOnlySpan<CellMap> HousesMap => HousesMapBackingField;

	/// <summary>
	/// Indicates a list of <see cref="CellMap"/> instances representing the peer cells of a cell at the specified index.
	/// </summary>
	public static ReadOnlySpan<CellMap> PeersMap => PeersMapBackingField;

	/// <summary>
	/// Indicates the chute maps.
	/// </summary>
	public static ReadOnlySpan<CellMap> ChuteMaps => ChuteMapsBackingField;

	/// <summary>
	/// Indicates a list of <see cref="Chute"/> instances representing chutes.
	/// </summary>
	public static ReadOnlySpan<Chute> Chutes => ChutesBackingField;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static SolutionFields()
	{
		//
		// RowTable
		//
		{
			RowTable = [.. from cell in SpanEnumerable.Range(0, 81) select cell / 9 + 9];
			ColumnTable = [.. from cell in SpanEnumerable.Range(0, 81) select cell % 9 + 18];
			BlockTable = [.. from cell in SpanEnumerable.Range(0, 81) select cell / 9 / 3 * 3 + cell % 9 / 3];
		}

		//
		// HousesCells
		//
		{
			HousesCells = [
				..
				from houseIndex in SpanEnumerable.Range(0, 9)
				select (from cell in BlockTable.Index() where cell.Value == houseIndex select cell.Index).ToArray(),
				..
				from houseIndex in SpanEnumerable.Range(9, 9)
				select (from cell in RowTable.Index() where cell.Value == houseIndex select cell.Index).ToArray(),
				..
				from houseIndex in SpanEnumerable.Range(18, 9)
				select (from cell in ColumnTable.Index() where cell.Value == houseIndex select cell.Index).ToArray()
			];
		}

		//
		// HouseFirst
		//
		{
			HouseFirst = from h in HousesCells select h[0];
		}

		//
		// HousesMap
		//
		{
			HousesMapBackingField = new CellMap[27];
			for (var house = 0; house < 27; house++)
			{
				HousesMapBackingField[house] = HousesCells[house].AsCellMap();
			}
		}

		//
		// CellMaps
		//
		{
			CellMaps = new CellMap[81];
			var span = CellMaps.AsSpan();
			var cell = 0;
			foreach (ref var map in span)
			{
				map += cell++;
			}
		}

		//
		// PeersMap
		//
		{
			PeersMapBackingField = new CellMap[81];
			for (var cell = 0; cell < 81; cell++)
			{
				var map = CellMap.Empty;
				for (var peerCell = 0; peerCell < 81; peerCell++)
				{
					if (cell != peerCell)
					{
						foreach (var houseType in HouseTypes)
						{
							if (HousesMap[cell >> houseType].Contains(peerCell))
							{
								map += peerCell;
								break;
							}
						}
					}
					if (map.Count == CellMap.PeersCount)
					{
						break;
					}
				}
				PeersMapBackingField[cell] = map;
			}
		}

#if CACHE_CANDIDATE_MAPS
		//
		// CandidateMaps
		//
		{
			CandidateMaps = new CandidateMap[729];
			var span = CandidateMaps.AsSpan();
			var candidate = 0;
			foreach (ref var map in span)
			{
				map.Add(candidate++);
			}
		}
#endif

		var chuteHouses = (ReadOnlySpan<(House, House, House)>)[(9, 10, 11), (12, 13, 14), (15, 16, 17), (18, 19, 20), (21, 22, 23), (24, 25, 26)];

		//
		// ChuteMaps
		//
		{
			ChuteMapsBackingField = new CellMap[6];
			for (var chute = 0; chute < 3; chute++)
			{
				var ((r1, r2, r3), (c1, c2, c3)) = (chuteHouses[chute], chuteHouses[chute + 3]);
				(ChuteMapsBackingField[chute], ChuteMapsBackingField[chute + 3]) = (HousesMap[r1] | HousesMap[r2] | HousesMap[r3], HousesMap[c1] | HousesMap[c2] | HousesMap[c3]);
			}
		}

		//
		// Chutes
		//
		{
			ChutesBackingField = new Chute[6];
			for (var chute = 0; chute < 3; chute++)
			{
				var ((r1, r2, r3), (c1, c2, c3)) = (chuteHouses[chute], chuteHouses[chute + 3]);
				(ChutesBackingField[chute], ChutesBackingField[chute + 3]) = (
					new(chute, true, 1 << r1 | 1 << r2 | 1 << r3),
					new(chute + 3, false, 1 << c1 | 1 << c2 | 1 << c3)
				);
			}
		}
	}
}
