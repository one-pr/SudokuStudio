namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents an object that can calculate rank-related information via the specified data.
/// </summary>
/// <param name="grid">The grid.</param>
/// <param name="truths">The truths.</param>
/// <param name="links">The links.</param>
[TypeImpl(TypeImplFlags.EqualityOperators)]
public readonly ref partial struct RankPattern(in Grid grid, in SpaceSet truths, in SpaceSet links) : IEquatable<RankPattern>
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
	private readonly CandidateMap _candidates = BuildCandidates(grid, truths, links);


	/// <summary>
	/// Indicates all cells used.
	/// </summary>
	public CellMap Cells => _candidates.Cells;

	/// <summary>
	/// Indicates the candidates.
	/// </summary>
	[UnscopedRef]
	public ref readonly CandidateMap Candidates => ref _candidates;


	/// <summary>
	/// Indicates whether the current pattern is stable rank-0 pattern, i.e. all links are rank-0 links.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool GetIsRank0Pattern() => GetRank0Links() == Links;

	/// <inheritdoc/>
	[Obsolete($"This method always return false. Ref structs cannot be boxed so argument '{nameof(obj)}' must be a different instance.", false)]
	public override bool Equals(object? obj) => false;

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(in RankPattern other) => Grid == other.Grid && Truths == other.Truths && Links == other.Links;

	/// <summary>
	/// Infers links that will cover all candidates from the truths and eliminations, if links are unknown.
	/// This method doesn't use field <see cref="Links"/>.
	/// </summary>
	/// <returns>The result links found.</returns>
	/// <seealso cref="Links"/>
	public unsafe SpaceSet InferLinks()
	{
		// Find for all links, passing eliminations.
		var combinations = GetAssignmentCombinationsCore(out var fullLinks);
		var rank0Links = GetRank0LinksCore(combinations);
		if (rank0Links.Count == Truths.Count)
		{
			// Rank-0 pattern.
			return rank0Links;
		}

		var originalEliminations = GetEliminationsCore(combinations);

		// Create a dictionary to record distribution of each candidate and its containing truth.
		var truthDictionary = new Dictionary<Candidate, SpaceSet>();
		foreach (var truth in Truths)
		{
			foreach (var candidate in truth.GetAvailableRange(Grid))
			{
				if (!truthDictionary.TryAdd(candidate, [truth]))
				{
					truthDictionary[candidate].Add(truth);
				}
			}
		}

		// Iterate on each link, to check whether it is redundant or not.
		var result = SpaceSet.Empty;
		foreach (var link in fullLinks)
		{
			var isLinkRedundant = true;
			foreach (ref readonly var combination in _candidates & link.GetAvailableRange(Grid) & 2)
			{
				var first = combination[0];
				var second = combination[1];
				ref readonly var sets1 = ref truthDictionary.GetValueRef(first);
				ref readonly var sets2 = ref truthDictionary.GetValueRef(second);

				// Check whether they are in a same truth.
				if (sets1 & sets2)
				{
					// This combination disobeys the rule of truth, invalid.
					// Skip for the current combination.
					continue;
				}

				// Suppose the link is gone and make an assumption that do both candidates are true.
				// Here we should do a trick: forcely assign two candidates to the grid, regardless of conflict of grid.
				// This will also work for both digits in a same cell.
				// We know that two candidates will also clear digit appearances from peer cells,
				// which is by design of type 'Grid', so we can perform the applying rules
				// to clear irrelevant candidates.
				// Although the grid becomes invalid, we know that this type won't check validity of the grid.
				var subgrid = Grid;
				subgrid.SetDigit(first / 9, first % 9);
				subgrid.SetDigit(second / 9, second % 9);

				// Create a pattern.
				// Note that link can be empty because here we don't use any links as necessary data -
				// we just want to get all assignment combinations of the subpattern mentioned above,
				// whose relied mechanism doesn't use any links (link-free).
				//
				// You may ask me, "Why? I do see the code used links!"
				// ...Well, in fact, the mechanism is unnecessary and can be removed.
				// The backing implementation algorithm will automatically skip invalid combinations
				// to keep the pattern valid;
				// for example, it directly ignores same digit are filled into a same house with different cells.
				var subpattern = new RankPattern(subgrid, Truths & ~(sets1 | sets2), SpaceSet.Empty);
				var subpatternCombinations = subpattern.GetAssignmentCombinations();
				var subpatternEliminations = subpattern.GetEliminationsCore(subpatternCombinations/*, &otherDigitsCalc, &otherCellsCalc*/);

				// If there's a link from candidate to elimination,
				// we will get an information "those two cannot be both true".
				// Therefore, safely add elimination from original grid into the elimination set.
				foreach (var assignmentToCheck in combination)
				{
					var cell = assignmentToCheck / 9;
					var digit = assignmentToCheck % 9;

					// Check whether the candidate can see at least one elimination.
					var assignmentToCheckPeerCandidates = (Grid.CandidatesMap[digit] & PeersMap[cell]) * digit;
					foreach (var d in (Mask)(Grid.GetCandidates(cell) & ~(1 << digit)))
					{
						if (!_candidates.Contains(cell * 9 + d))
						{
							subpatternEliminations.Add(cell * 9 + d);
						}
						assignmentToCheckPeerCandidates.Add(cell * 9 + d);
					}

					foreach (var eliminationToCheck in assignmentToCheckPeerCandidates & originalEliminations)
					{
						var tempMap = link.GetAvailableRange(Grid);
						if (tempMap.Contains(assignmentToCheck) && tempMap.Contains(eliminationToCheck))
						{
							// They are in a same link, but the link is supposed to be disappeared.
							// So we cannot link they up and remove the elimination.
							subpatternEliminations.Remove(eliminationToCheck);
						}
					}
				}

				if ((subpatternEliminations & originalEliminations) != originalEliminations)
				{
					// If we removed the link and can get a same elimination set or a subset,
					// we can know that the link is redundant.
					isLinkRedundant = false;
					break;
				}
			}
			if (!isLinkRedundant)
			{
				// Otherwise, the link is required.
				result.Add(link);
			}
		}

		// Return necessary links.
		return result;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Grid, Truths, Links);

	/// <summary>
	/// Indicates the rank of the current pattern. If the pattern is unstable
	/// (sometimes assignments certain times of digits in the pattern but sometimes not),
	/// this property will return <see langword="null"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int? GetRank() => GetRankCore(GetAssignmentCombinations());

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"T{Truths.Count} = {Truths}, L{Links.Count} = {Links}";

	/// <summary>
	/// Gets the full string of the current pattern, including its details (rank, eliminations and so on).
	/// </summary>
	/// <returns>The string.</returns>
	public unsafe string ToFullString()
	{
		var combinations = GetAssignmentCombinations();
		return string.Format(
			SR.Get("RankInfo"),
			Grid.ToString("@:"),
			ToString(),
			combinations.Length,
			GetRankCore(combinations)?.ToString() ?? SR.Get("UnstableRank"),
			GetEliminationsCore(combinations).ToString(),
			GetEliminationZoneCore(combinations, EliminationZoneIgnoringOptions.None).ToString(),
			GetRank0LinksCore(combinations).ToString(),
			SR.Get(GetIsRank0PatternCore(combinations) ? "IsRank0Pattern" : "IsNotRank0Pattern")
		);
	}

	/// <summary>
	/// Indicates eliminations can be found in the current pattern.
	/// </summary>
	/// <remarks>
	/// In theory, eliminations may not require any links. All conclusions come from valid combinations of truths,
	/// keeping one valid digit filling into each truth, and find intersections of eliminations can be found from all cases.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe CandidateMap GetEliminations() => GetEliminationsCore(GetAssignmentCombinations());

	/// <summary>
	/// Returns a list of <see cref="Candidate"/> group that describes the valid assignments.
	/// </summary>
	/// <returns>Valid assignments.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<ReadOnlyMemory<Candidate>> GetAssignmentCombinations() => GetAssignmentCombinationsCore(out _);

	/// <summary>
	/// Returns a list of links indicating the connection on hatching.
	/// </summary>
	/// <returns>Links.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SpaceSet GetFullLinks()
	{
		GetAssignmentCombinationsCore(out var result);
		return result;
	}

	/// <summary>
	/// Try to find all rank-0 links. A rank-0 link is a link that will become truth
	/// because all valid combinations lead to a same result that the link must hold one correct digit.
	/// </summary>
	/// <returns>A list of links that are determined as rank-0 links.</returns>
	public SpaceSet GetRank0Links() => GetRank0LinksCore(GetAssignmentCombinations());

	/// <summary>
	/// Find elimination zones, indicating a list of candidates that can be eliminated,
	/// no matter whether they exist or not.
	/// </summary>
	/// <param name="options">The options that determines and filters the elimination zones.</param>
	/// <returns>A list of candidates.</returns>
	public CandidateMap GetEliminationZone(EliminationZoneIgnoringOptions options)
		=> GetEliminationZoneCore(GetAssignmentCombinations(), options);

	/// <inheritdoc/>
	bool IEquatable<RankPattern>.Equals(RankPattern other) => Equals(other);

	/// <summary>
	/// Determine whether the pattern is rank-0 pattern via cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool GetIsRank0PatternCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
		=> GetRank0LinksCore(combinations) == Links;

	/// <summary>
	/// Calculate rank value via cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <returns>The rank value.</returns>
	private int? GetRankCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
	{
		var factAssignmentCountValues = new HashSet<int>();
		foreach (var l in from assignment in combinations select assignment.Length)
		{
			factAssignmentCountValues.Add(l);
			if (factAssignmentCountValues.Count >= 2)
			{
				// Invalid, fast fail.
				return null;
			}
		}
		return factAssignmentCountValues.Count == 1 ? Links.Count - factAssignmentCountValues.First() : null;
	}

	/// <summary>
	/// Calculate eliminations via cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <param name="otherDigitCalculator">
	/// The calculator function that gets a range of other digits of cell-assignment eliminations.
	/// </param>
	/// <param name="otherCellsCalculator">
	/// The calculator function that gets a range of other cells of house-assignment eliminations.
	/// </param>
	/// <returns>A list of candidates.</returns>
	private unsafe CandidateMap GetEliminationsCore(
		ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations,
		delegate*<ref readonly Grid, Cell, Digit, Mask> otherDigitCalculator = null,
		delegate*<ref readonly Grid, Cell, Digit, CellMap> otherCellsCalculator = null
	)
	{
		if (otherDigitCalculator == null)
		{
			otherDigitCalculator = &otherDigitsCalc;
		}
		if (otherCellsCalculator == null)
		{
			otherCellsCalculator = &otherCellsCalc;
		}

		var result = CandidateMap.Empty;
		var i = 0;
		foreach (var assignmentGroup in combinations)
		{
			var current = CandidateMap.Empty;
			foreach (var assignment in assignmentGroup)
			{
				var cell = assignment / 9;
				var digit = assignment % 9;
				foreach (var otherDigit in otherDigitCalculator(in Grid, cell, digit))
				{
					current.Add(cell * 9 + otherDigit);
				}
				foreach (var otherCell in otherCellsCalculator(in Grid, cell, digit))
				{
					current.Add(otherCell * 9 + digit);
				}
			}

			if (i++ == 0)
			{
				result |= current;
			}
			else
			{
				result &= current;
			}
		}

		// Trim fake eliminations from truths.
		foreach (var truth in Truths)
		{
			switch (truth)
			{
				case { IsCellRelated: true, Cell: var cell }:
				{
					foreach (var digit in (Mask)(Grid.MaxCandidatesMask & ~Grid.GetCandidates(cell)))
					{
						result.Remove(cell * 9 + digit);
					}
					break;
				}
				case { IsHouseRelated: true, House: var house, Digit: var digit }:
				{
					result &= ~(HousesMap[house] * digit & ~truth.GetAvailableRange(Grid));
					break;
				}
			}
		}

		return result;


		static Mask otherDigitsCalc(ref readonly Grid grid, Cell cell, Digit digit)
			=> (Mask)(grid.GetCandidates(cell) & ~(1 << digit));

		static CellMap otherCellsCalc(ref readonly Grid grid, Cell cell, Digit digit)
			=> PeersMap[cell] & grid.CandidatesMap[digit];
	}

	/// <summary>
	/// Gets assignment combinations with full links.
	/// </summary>
	/// <param name="links">The full links.</param>
	/// <returns>Assignment combinations.</returns>
	private ReadOnlySpan<ReadOnlyMemory<Candidate>> GetAssignmentCombinationsCore(out SpaceSet links)
	{
		(links, var result) = (SpaceSet.Empty, new List<ReadOnlyMemory<Candidate>>());

		// Create a queue to record all possible cases, in BFS way.
		var queue = new LinkedList<CombinationQueueNode>();
		queue.AddLast(new CombinationQueueNode(-1, [.. SpanEnumerable.Range(Truths.Count)], null));

		// Iterate the whole queue until the queue becomes empty.
		while (queue.Count != 0)
		{
			// Dequeue a node.
			// There're the following values can be used:
			//   * currentState: The current candidates applied. To combine all of them it will be the current assignment combination.
			//   * remainingTruths: The remaining truth, as corresponding indices of truth space set.
			//   * parent: The parent node.
			var currentNode = queue.RemoveFirstNode();
			var (_, remainingTruths, parent) = currentNode;
			var currentState = currentNode.State;

			// Check whether the node has already finished.
			if (remainingTruths.Length == 0)
			{
				// Verify links.
				var flag = true;
				foreach (var link in Links)
				{
					if (!link.IsSatisfied(currentState, false))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					result.Add(currentState.ToArray());
				}

				links |= currentNode.GetProducedLinks(Grid, Truths);
				continue;
			}

			// Heuristic searching:
			// We should firstly check for truths with less number of remaining positions.
			// However, if we have found at least one house having no valid positions to be filled,
			// because we must select a candidate to be filled, so it cause a conflict,
			// meaning the current combination is invalid.
			var tempProjectedValues = new List<(int, CandidateMap Remaining)>();
			var tempGrid = Grid;
			foreach (var state in currentState)
			{
				tempGrid.SetDigit(state / 9, state % 9);
			}
			foreach (var index in remainingTruths)
			{
				tempProjectedValues.AddRef((index, Truths[index].GetAvailableRange(tempGrid)));
			}
			var sorted =
				from x in tempProjectedValues.AsSpan()
				orderby x.Remaining.Count
				select x into x
				select x;

			// Check whether the collection is valid.
			if (sorted.Length == 0 || sorted.Any(static value => value.Remaining.Count == 0))
			{
				continue;
			}

			// Valid. Now add children nodes.
			var (selectedIndex, remainingCandidates) = sorted[0];
			var newRemainingTruths = new List<int>();
			foreach (var truthIndex in remainingTruths)
			{
				if (truthIndex != selectedIndex)
				{
					newRemainingTruths.Add(truthIndex);
				}
			}
			foreach (var remainingCandidate in remainingCandidates)
			{
				var nextState = currentState + remainingCandidate;
				if (Links.TrueForAll(link => link.IsSatisfied(nextState, false)))
				{
					// Check whether the remaining truths, preventing truth overlapped cases.
					var overlapped = new List<int>();
					foreach (var truthIndex in newRemainingTruths)
					{
						var overlappingFlag = false;
						foreach (var assigned in nextState)
						{
							if (Truths[truthIndex].ContainsAssignment(assigned))
							{
								overlappingFlag = true;
								break;
							}
						}
						if (overlappingFlag)
						{
							overlapped.Add(truthIndex);
						}
					}
					foreach (var truthIndex in overlapped)
					{
						newRemainingTruths.Remove(truthIndex);
					}

					queue.AddLast(new CombinationQueueNode(remainingCandidate, [.. newRemainingTruths], currentNode));
				}
			}
		}

		return result.AsSpan();
	}

	/// <summary>
	/// Gets rank-0 links via cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <returns>Rank-0 links.</returns>
	private SpaceSet GetRank0LinksCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
	{
		var result = SpaceSet.Empty;
		var links = Links;

		var i = 0;
		foreach (var assignmentGroup in combinations)
		{
			var lightUpLinks = SpaceSet.Empty;
			foreach (var assignment in assignmentGroup)
			{
				foreach (var set in links)
				{
					if (set.ContainsAssignment(assignment))
					{
						lightUpLinks.Add(set);
					}
				}
			}

			if (i++ == 0)
			{
				result |= lightUpLinks;
			}
			else
			{
				result &= lightUpLinks;
			}
		}

		return result;
	}

	/// <summary>
	/// Calculates elimination zones via the specified ignoring options and cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <param name="options">The ignoring options.</param>
	/// <returns>The candidates.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="options"/> is out of range.
	/// </exception>
	private unsafe CandidateMap GetEliminationZoneCore(
		ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations,
		EliminationZoneIgnoringOptions options
	)
	{
		if (options
			is < EliminationZoneIgnoringOptions.None
			or > (EliminationZoneIgnoringOptions.IgnoreExternal | EliminationZoneIgnoringOptions.IgnoreSubpatterns))
		{
			throw new ArgumentOutOfRangeException(nameof(options));
		}

		var result = GetEliminationsCore(combinations, &otherDigitsCalc, &otherCellsCalc);
		if (options == EliminationZoneIgnoringOptions.None)
		{
			return result;
		}

		if (options.HasFlag(EliminationZoneIgnoringOptions.IgnoreExternal))
		{
			var candidatesSet = CandidateMap.Empty;
			foreach (var link in Links)
			{
				candidatesSet |= link.GetRange();
			}
			result &= candidatesSet;
		}

		if (options.HasFlag(EliminationZoneIgnoringOptions.IgnoreSubpatterns))
		{
			// Iterate all combinations of truths.
			var truthsArray = Truths.ToArray();
			for (var i = 1; i < Truths.Count - 1; i++)
			{
				foreach (var truthCombination in truthsArray.GetSubsets(i))
				{
					var subpattern = new RankPattern(Grid, [.. truthCombination], SpaceSet.Empty);
					result &= ~subpattern.GetEliminationZone(EliminationZoneIgnoringOptions.None);
				}
			}
		}

		return result;


		static Mask otherDigitsCalc(ref readonly Grid grid, Cell cell, Digit digit) => (Mask)(Grid.MaxCandidatesMask & ~(1 << digit));

		static CellMap otherCellsCalc(ref readonly Grid grid, Cell cell, Digit digit) => PeersMap[cell] & grid.EmptyCells;
	}


	/// <summary>
	/// Build candidates.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="truths">The truths.</param>
	/// <param name="links">The links.</param>
	private static CandidateMap BuildCandidates(in Grid grid, in SpaceSet truths, in SpaceSet links)
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
