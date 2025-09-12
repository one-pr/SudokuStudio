namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridAnalysisExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid @this)
	{
		/// <inheritdoc cref="op_RightShiftAssignment(ref Grid, Step)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Apply, false)]
		public void Apply(Step step) => @this >>= step;


		/// <summary>
		/// Applies for all conclusions into the current <see cref="Grid"/> instance.
		/// </summary>
		/// <param name="step">A conclusion-provider <see cref="Step"/> instance.</param>
		public void operator >>=(Step step)
		{
			foreach (var conclusion in step.Conclusions)
			{
				@this >>= conclusion;
			}
		}


		/// <summary>
		/// Applies the grid with specified step.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="step">The step.</param>
		/// <returns>The target grid.</returns>
		public static Grid operator >>(in Grid grid, Step step)
		{
			var tempGrid = grid;
			tempGrid >>= step;
			return tempGrid;
		}
	}

	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Checks whether the puzzle can be solved using only full house.
		/// </summary>
		public bool CanPrimaryFullHouse
			=> Analyzer.Default
				// ROSLYN_ISSUE: Remove null-forgiving operator
				// due to wrong analysis for Roslyn on extension member with complex nullable argument types.
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true }!)
				.WithUserDefinedOptions(new() { PrimarySingle = SingleTechniqueFlag.FullHouse })
				.Analyze(@this)
				.IsSolved;

		/// <summary>
		/// Checks whether the puzzle can be solved using only naked single.
		/// </summary>
		public bool CanPrimaryNakedSingle
			=> Analyzer.Default
				// ROSLYN_ISSUE: Remove null-forgiving operator
				// due to wrong analysis for Roslyn on extension member with complex nullable argument types.
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true }!)
				.WithUserDefinedOptions(new() { IsDirectMode = true, PrimarySingle = SingleTechniqueFlag.NakedSingle })
				.Analyze(@this)
				.IsSolved;


		/// <summary>
		/// Checks whether the puzzle can be solved using only full house and hidden single.
		/// </summary>
		/// <param name="allowHiddenSingleInLine">
		/// A <see cref="bool"/> value indicating whether hidden single includes line types.
		/// </param>
		public bool CanPrimaryHiddenSingle(bool allowHiddenSingleInLine)
			=> Analyzer.Default
				// ROSLYN_ISSUE: Remove null-forgiving operator
				// due to wrong analysis for Roslyn on extension member with complex nullable argument types.
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true, EnableLastDigit = true }!)
				.WithUserDefinedOptions(new() { IsDirectMode = true, PrimarySingle = SingleTechniqueFlag.HiddenSingleColumn })
				.Analyze(@this)
				.IsSolved;
	}
}
