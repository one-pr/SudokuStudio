namespace Sudoku.Concepts.Marshalling;

/// <summary>
/// Represents concept for a "House". A house is a region that includes 9 cells, with different digits filled in.
/// Types of a valid house can be "row", "column" and "block" (i.e. "box").
/// </summary>
public static class HouseMarshal
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
	/// Backing field of <see cref="HousesMap"/>.
	/// </summary>
	private static readonly CellMap[] HousesMapBackingField;


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static HouseMarshal()
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
	}


	/// <summary>
	/// Indicates a list of <see cref="CellMap"/> instances representing the cells belong to a house at the specified index.
	/// </summary>
	public static ReadOnlySpan<CellMap> HousesMap => HousesMapBackingField;


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="houseIndex">The current house index (0..27).</param>
	extension(House houseIndex)
	{
		/// <summary>
		/// Get the house type for the specified house index.
		/// </summary>
		/// <returns>
		/// The house type. The possible return values are:
		/// <list type="table">
		/// <listheader>
		/// <term>House indices</term>
		/// <description>Return value</description>
		/// </listheader>
		/// <item>
		/// <term><![CDATA[>= 0 and < 9]]></term>
		/// <description><see cref="HouseType.Block"/></description>
		/// </item>
		/// <item>
		/// <term><![CDATA[>= 9 and < 18]]></term>
		/// <description><see cref="HouseType.Row"/></description>
		/// </item>
		/// <item>
		/// <term><![CDATA[>= 18 and < 27]]></term>
		/// <description><see cref="HouseType.Column"/></description>
		/// </item>
		/// </list>
		/// </returns>
		public HouseType HouseType => (HouseType)(houseIndex / 9);
	}
}
