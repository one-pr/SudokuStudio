namespace Sudoku.Ranking;

public partial struct RankPattern
{
	/// <summary>
	/// Indicates eliminations can be found in the current pattern.
	/// </summary>
	/// <remarks>
	/// In theory, eliminations may not require any links. All conclusions come from valid combinations of truths,
	/// keeping one valid digit filling into each truth, and find intersections of eliminations can be found from all cases.
	/// </remarks>
	public unsafe CandidateMap GetEliminations() => GetEliminationsCore(GetAssignmentCombinations());

	/// <summary>
	/// Find elimination zones, indicating a list of candidates that can be eliminated,
	/// no matter whether they exist or not.
	/// </summary>
	/// <param name="options">The options that determines and filters the elimination zones.</param>
	/// <returns>A list of candidates.</returns>
	/// <exception cref="PatternTooComplexException">
	/// Throws when pattern is too complex
	/// when <paramref name="options"/> is <see cref="EliminationZoneIgnoringOptions.IgnoreSubpatterns"/>.
	/// </exception>
	public CandidateMap GetEliminationZone(EliminationZoneIgnoringOptions options)
		=> GetEliminationZoneCore(GetAssignmentCombinations(), options);

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
				candidatesSet |= link.Range;
			}
			result &= candidatesSet;
		}

		if (options.HasFlag(EliminationZoneIgnoringOptions.IgnoreSubpatterns))
		{
			var counter = 0;

			// Iterate all combinations of truths.
			var truthsArray = Truths.ToArray();
			for (var i = 1; i < Truths.Count - 1; i++)
			{
				var truthCombinations = truthsArray.GetSubsets(i);
				foreach (var truthCombination in truthCombinations)
				{
					if (counter++ >= 100000)
					{
						throw new PatternTooComplexException();
					}

					var subpatternTruths = truthCombination.AsSpaceSet();
					var subpattern = new RankPattern(in Grid, in subpatternTruths, in SpaceSet.Empty);
					result &= ~subpattern.GetEliminationZone(EliminationZoneIgnoringOptions.None);
				}
			}
		}

		return result;


		static Mask otherDigitsCalc(ref readonly Grid grid, Cell cell, Digit digit) => (Mask)(Grid.MaxCandidatesMask & ~(1 << digit));

		static CellMap otherCellsCalc(ref readonly Grid grid, Cell cell, Digit digit) => PeersMap[cell] & grid.EmptyCells;
	}
}
