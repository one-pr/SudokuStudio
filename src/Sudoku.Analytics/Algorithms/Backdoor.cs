namespace Sudoku.Algorithms;

/// <summary>
/// Represents a type that checks for concept "<see href="http://sudopedia.enjoysudoku.com/Backdoor.html">Backdoor</see>".
/// </summary>
/// <remarks>
/// A backdoor is a placement of one or more candidates in their respective cells,
/// which allows the player to complete the puzzle with relatively simple solving techniques.
/// </remarks>
public static class Backdoor
{
	/// <summary>
	/// Try to find backdoors of the grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>All backdoors found.</returns>
	public static ReadOnlySpan<Conclusion> GetBackdoors(in Grid grid)
	{
		if (grid.PuzzleType != SudokuType.Standard || grid.IsSolved || !grid.IsValid)
		{
			return default;
		}

		var sstsChecker = Analyzer.SstsOnly;
		return sstsChecker.Analyze(grid).IsSolved && grid.SolutionGrid is var solution
			?
			from candidate in grid
			let digit = solution.GetDigit(candidate / 9)
			where digit != -1
			select new Conclusion(digit == candidate % 9 ? Assignment : Elimination, candidate)
			: g(grid);


		ReadOnlySpan<Conclusion> g(in Grid grid)
		{
			var (assignment, elimination, solution) = (new List<Conclusion>(81), new List<Conclusion>(729), grid.SolutionGrid);
			foreach (var cell in grid.EmptyCells)
			{
				// Case 1: Assignments.
				var case1Playground = grid;
				case1Playground.SetDigit(cell, solution.GetDigit(cell));

				if (sstsChecker.Analyze(case1Playground).IsSolved)
				{
					assignment.Add(new(Assignment, cell, solution.GetDigit(cell)));

					// Case 2: Eliminations.
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~(1 << solution.GetDigit(cell))))
					{
						var case2Playground = grid;
						case2Playground.SetExistence(cell, digit, false);
						if (sstsChecker.Analyze(case2Playground).IsSolved)
						{
							elimination.Add(new(Elimination, cell, digit));
						}
					}
				}
			}
			return (Conclusion[])[.. assignment, .. elimination];
		}
	}
}
