namespace Sudoku.Generating;

public partial class GeneratorHub
{
	private static partial DifficultyLevel GetDifficultyLevel(ConstraintCollection constraints, Random rng)
		=> (
			from c in constraints.OfType<DifficultyLevelConstraint>()
			select c.ValidDifficultyLevels.AllFlags.ToArray()
		) is [var d] ? d[rng.Next(0, d.Length)] : DifficultyLevels.AllValid;

	private static partial Cell GetGivensCount(Random rng, (Cell, Cell) chosenGivensCountSeed)
		=> chosenGivensCountSeed is (var s and not -1, var e and not -1) ? rng.Next(s, e + 1) : -1;

	private static partial (Cell, Cell) GetChosenGivensCountRange(ConstraintCollection constraints)
		=> (
			from c in constraints.OfType<CountBetweenConstraint>()
			let betweenRule = c.BetweenRule
			let pair = (Start: c.Range.Start.Value, End: c.Range.End.Value)
			let targetPair = c.CellState switch
			{
				CellState.Given => (pair.Start, pair.End),
				CellState.Empty => (81 - pair.End, 81 - pair.Start)
			}
			select (betweenRule, targetPair)
		) is [var (br, (start, end))] ? DetermineEmptyCellsCount(br, start, end) : (-1, -1);

	private static partial (Cell, Cell) DetermineEmptyCellsCount(BetweenRule betweenRule, Cell start, Cell end)
		=> betweenRule switch
		{
			BetweenRule.BothOpen => (start + 1, end - 1),
			BetweenRule.LeftOpen => (start + 1, end),
			BetweenRule.RightOpen => (start, end + 1),
			_ => (start, end)
		};

	private static partial ReadOnlySpan<SymmetricType> GetSymmetry(ConstraintCollection constraints)
		=> (from c in constraints.OfType<SymmetryConstraint>() select c.SymmetricTypes) switch
		{
			[var p] => p switch
			{
				SymmetryConstraint.InvalidSymmetricType => [],
				SymmetryConstraint.AllSymmetricTypes => SymmetricType.Values,
				var symmetricTypes and not 0 => symmetricTypes.AllFlags,
				_ => [SymmetricType.None]
			},
			_ => SymmetricType.Values
		};
}
