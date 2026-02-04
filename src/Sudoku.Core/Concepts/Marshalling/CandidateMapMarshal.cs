namespace Sudoku.Concepts.Marshalling;

/// <summary>
/// Represents extra data set defined in <see cref="CandidateMap"/>.
/// </summary>
/// <seealso cref="CandidateMap"/>
public static class CandidateMapMarshal
{
#if CACHE_CANDIDATE_MAPS
	/// <summary>
	/// Indicates a list of <see cref="CandidateMap"/> instances that are initialized as singleton element by its corresponding index.
	/// For example, <c>CandidateMaps[0]</c> is equivalent to <c>CandidateMap.Empty + 0</c>, i.e. <c>r1c1(1)</c>.
	/// </summary>
	internal static readonly CandidateMap[] CandidateMaps;
#endif


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static CandidateMapMarshal()
	{
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
	}
}
