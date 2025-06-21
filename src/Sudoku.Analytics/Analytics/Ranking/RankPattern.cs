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
	public readonly ref readonly SpaceSet Links = ref links;

	/// <summary>
	/// Represents candidates.
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
	/// <param name="result">The result links found.</param>
	/// <returns>A <see cref="bool"/> result indicating whether a combination is found.</returns>
	/// <seealso cref="Links"/>
	public unsafe bool TryInferLinks(out SpaceSet result)
	{
		var availableTruthCandidates = _candidates;
		var eliminations = GetEliminations();

		// Iterate candidates to cover.
		result = SpaceSet.Empty;
		var isChanged = true;
		while (!!availableTruthCandidates && isChanged)
		{
			isChanged = false;
			foreach (var candidateToCover in availableTruthCandidates)
			{
				var cell = candidateToCover / 9;
				var digit = candidateToCover % 9;

				var validSpaces = new List<Space>(4);
				var isFallbackChecked = false;

				// Check for valid links to connect.
				if (Space.RowColumn(cell / 9, cell % 9) is var cellLink
					&& isLinkValid(this, cellLink, availableTruthCandidates, eliminations))
				{
					validSpaces.Add(cellLink);
				}
				if (Space.BlockDigit(cell.ToHouse(HouseType.Block), digit) is var blockLink
					&& isLinkValid(this, blockLink, availableTruthCandidates, eliminations))
				{
					validSpaces.Add(blockLink);
				}
				if (Space.RowDigit(cell.ToHouse(HouseType.Row) - 9, digit) is var rowLink
					&& isLinkValid(this, rowLink, availableTruthCandidates, eliminations))
				{
					validSpaces.Add(rowLink);
				}
				if (Space.ColumnDigit(cell.ToHouse(HouseType.Column) - 18, digit) is var columnLink
					&& isLinkValid(this, columnLink, availableTruthCandidates, eliminations))
				{
					validSpaces.Add(columnLink);
				}
				goto CheckStateOfValidSpaces;

			LinkTripletFallback:
				if (!isFallbackChecked)
				{
					isFallbackChecked = true;

					// Fallback to include all candidates to cover.
					if (isLinkValid(this, cellLink, _candidates, eliminations))
					{
						validSpaces.Add(cellLink);
					}
					if (isLinkValid(this, blockLink, _candidates, eliminations))
					{
						validSpaces.Add(blockLink);
					}
					if (isLinkValid(this, rowLink, _candidates, eliminations))
					{
						validSpaces.Add(rowLink);
					}
					if (isLinkValid(this, columnLink, _candidates, eliminations))
					{
						validSpaces.Add(columnLink);
					}
				}

			CheckStateOfValidSpaces:
				switch (validSpaces)
				{
					// No valid links can be found.
					// This case can be triggered if two links connect to a same candidate (a link triplet).
					case []:
					{
						if (isFallbackChecked)
						{
							goto default;
						}
						else
						{
							goto LinkTripletFallback;
						}
					}

					// If there's only one valid link to be connected, connect directly.
					case [var space]:
					{
						result.Add(space);
						availableTruthCandidates &= ~space.GetAvailableRange(Grid);
						isChanged = true;
						goto OuterWhileLoop;
					}

					// We should connect to the only link that connects to a truth.
					// If at least 2 links connect to 2 different available candidates in truths,
					// we cannot determine the target connecting state.
					case { Count: >= 2 }:
					{
						var validLinks = new List<Space>();
						foreach (var space in validSpaces)
						{
							if (space.GetAvailableRange(Grid) & availableTruthCandidates - candidateToCover)
							{
								validLinks.Add(space);
							}
						}

						switch (validLinks)
						{
							case []:
							{
								continue;
							}
							case [var onlyValidLink]:
							{
								// Valid.
								result.Add(onlyValidLink);
								availableTruthCandidates &= ~onlyValidLink.GetAvailableRange(Grid);
								isChanged = true;
								goto OuterWhileLoop;
							}
							default:
							{
								// Multiple choosing ways can be selected.
								// We should select a best one, which covers more candidates from truths.
								var gridCopied = Grid;
								validLinks.Sort(
									(left, right) =>
									{
										var a = left.GetAvailableRange(gridCopied) & availableTruthCandidates - candidateToCover;
										var b = right.GetAvailableRange(gridCopied) & availableTruthCandidates - candidateToCover;
										return b.Count - a.Count;
									}
								);

								var bestLink = validLinks[0];
								result.Add(bestLink);
								availableTruthCandidates &= ~bestLink.GetAvailableRange(Grid);
								isChanged = true;
								goto OuterWhileLoop;
							}
						}
					}

					// Otherwise, we should do BFS to perform valid connection.
					default:
					{
						// TODO: Implement later.
						continue;
					}
				}
			}

		OuterWhileLoop:
			;
		}

		// Return true if there's no left candidates.
		return !availableTruthCandidates;


		static bool isLinkValid(
			in RankPattern @this,
			Space link,
			in CandidateMap availableTruthCandidates,
			in CandidateMap eliminations
		)
		{
			if (@this.Truths.Contains(link))
			{
				return false;
			}

			ref readonly var grid = ref @this.Grid;
			var linkCovered = link.GetAvailableRange(grid);
			var covered = availableTruthCandidates & linkCovered;
			if (covered.Count == 1 && !((availableTruthCandidates | eliminations) & linkCovered & eliminations))
			{
				// If the current link can only cover one candidate of the available candidates,
				// we should check for eliminations coverage.
				// If the link cannot cover any eliminations, invalid.
				return false;
			}

			if (covered.Count >= 2)
			{
				var isFullyCoveredByOneTruth = false;
				foreach (var truth in @this.Truths)
				{
					if ((truth.GetAvailableRange(grid) & covered) == covered)
					{
						isFullyCoveredByOneTruth = true;
						break;
					}
				}
				if (isFullyCoveredByOneTruth)
				{
					// If all the covered candidates belong to a same truth, it'll be redundant.
					return false;
				}
			}

			// Seems good.
			return true;
		}
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
