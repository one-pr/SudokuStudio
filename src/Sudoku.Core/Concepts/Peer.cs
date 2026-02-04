namespace Sudoku.Concepts;

/// <summary>
/// Represents a concept called "Peer". A peer is a cell that can see a certain cell. Every cell has 20 peers in a grid.
/// </summary>
public static class Peer
{
	/// <summary>
	/// Indicates the number of each cell's peers. The value is a constant.
	/// </summary>
	public const Cell PeersCount = 20;

	/// <summary>
	/// Indicates the number of each candidate's peers. The value is a constant.
	/// </summary>
	public const Candidate PeersCountCandidates = 28;


	/// <summary>
	/// Backing field of <see cref="PeersMap"/>.
	/// </summary>
	private static readonly CellMap[] PeersMapBackingField;

#if CACHE_CANDIDATE_MAPS
	/// <summary>
	/// Backing field of <see cref="CandidatePeersMap"/>.
	/// </summary>
	private static readonly CandidateMap[] CandidatePeersMapBackingField;
#endif


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static Peer()
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
						if (HousesMap[cell.GetHouse(houseType)].Contains(peerCell))
						{
							map += peerCell;
							break;
						}
					}
				}
				if (map.Count == PeersCount)
				{
					break;
				}
			}
			PeersMapBackingField[cell] = map;
		}

#if CACHE_CANDIDATE_MAPS
		CandidatePeersMapBackingField = new CandidateMap[729];
		for (var candidate = 0; candidate < 729; candidate++)
		{
			var map = CandidateMap.Empty;

			var cell = candidate / 9;
			var digit = candidate % 9;

			// Check cell.
			foreach (var peerCell in PeersMapBackingField[cell])
			{
				map += peerCell * 9 + digit;
			}

			// Check digit.
			for (var peerDigit = 0; peerDigit < 9; peerDigit++)
			{
				if (peerDigit != digit)
				{
					map += cell * 9 + peerDigit;
				}
			}

			CandidatePeersMapBackingField[candidate] = map;
		}
#endif
	}


	/// <summary>
	/// Indicates a list of <see cref="CellMap"/> instances representing the peer cells of a cell at the specified index.
	/// </summary>
	public static ReadOnlySpan<CellMap> PeersMap => PeersMapBackingField;

#if CACHE_CANDIDATE_MAPS
	/// <summary>
	/// Indicates a list of <see cref="CandidateMap"/> instances representing the peer candidates
	/// of a candidate at the specified index.
	/// </summary>
	public static ReadOnlySpan<CandidateMap> CandidatePeersMap => CandidatePeersMapBackingField;
#endif
}
