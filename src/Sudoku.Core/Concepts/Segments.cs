namespace Sudoku.Concepts;

/// <summary>
/// Provides <see cref="Segment"/>-based grouped information.
/// </summary>
public static class Segments
{
	/// <summary>
	/// Indicates the segments to be iterated, grouped by chute index.
	/// </summary>
	public static readonly CellMap[][] SegmentsGroupedByChuteIndex;

	/// <summary>
	/// Indicates all maps that forms the each segment. The pattern will be like:
	/// <code><![CDATA[
	/// .-------.-------.-------.
	/// | C C C | A A A | A A A |
	/// | B B B | . . . | . . . |
	/// | B B B | . . . | . . . |
	/// '-------'-------'-------'
	/// ]]></code>
	/// </summary>
	public static readonly FrozenDictionary<Segment, SegmentInfo> Map;

	/// <summary>
	/// Indicates the internal intersection block combinations.
	/// </summary>
	private static readonly byte[][] IntersectionBlockTable = [
		[1, 2], [0, 2], [0, 1], [1, 2], [0, 2], [0, 1], [1, 2], [0, 2], [0, 1],
		[4, 5], [3, 5], [3, 4], [4, 5], [3, 5], [3, 4], [4, 5], [3, 5], [3, 4],
		[7, 8], [6, 8], [6, 7], [7, 8], [6, 8], [6, 7], [7, 8], [6, 8], [6, 7],
		[3, 6], [0, 6], [0, 3], [3, 6], [0, 6], [0, 3], [3, 6], [0, 6], [0, 3],
		[4, 7], [1, 7], [1, 4], [4, 7], [1, 7], [1, 4], [4, 7], [1, 7], [1, 4],
		[5, 8], [2, 8], [2, 5], [5, 8], [2, 8], [2, 5], [5, 8], [2, 8], [2, 5]
	];


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static Segments()
	{
		SegmentsGroupedByChuteIndex = new CellMap[6][];
		for (var i = 0; i < 6; i++)
		{
			ref var currentSegmentGroup = ref SegmentsGroupedByChuteIndex[i];
			currentSegmentGroup = new CellMap[9];

			var ((_, _, chuteHouses), isRow, tempIndex) = (Chute.Chutes[i], i is 0 or 1 or 2, 0);
			foreach (var chuteHouse in chuteHouses)
			{
				for (var (houseCell, j) = (HouseFirst[chuteHouse], 0); j < 3; houseCell += isRow ? 3 : 27, j++)
				{
					ref var current = ref currentSegmentGroup[tempIndex++];
					current += houseCell;
					current += houseCell + (isRow ? 1 : 9);
					current += houseCell + (isRow ? 2 : 18);
				}
			}
		}

		var dic = new Dictionary<Segment, SegmentInfo>();
		for (var line = 9; line < 27; line++)
		{
			for (var j = 0; j < 3; j++)
			{
				var block = line < 18 ? Digits[(line - 9) / 3 * 3 + j] : HousesOrderedByColumn[(line - 18) / 3 * 3 + j];
				var segment = new Segment(line, block);
				var intersection = segment.Cells;
				dic.Add(segment, new(segment.LineMap & ~intersection, segment.BlockMap & ~intersection, intersection, IntersectionBlockTable[(line - 9) * 3 + j]));
			}
		}

		Map = dic.ToFrozenDictionary();
	}
}
