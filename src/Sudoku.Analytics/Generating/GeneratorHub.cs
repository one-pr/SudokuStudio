namespace Sudoku.Generating;

/// <summary>
/// Represents an entry point for generating puzzles with mixed configuration.
/// </summary>
public static partial class GeneratorHub
{
	/// <summary>
	/// Provides a way to generate a puzzle in asynchronous environment.
	/// </summary>
	/// <typeparam name="TProgressDataProvider">The type of progress data provider.</typeparam>
	/// <param name="onlyGenerateOne">Indicates whether the method only generate one puzzle and return.</param>
	/// <param name="constraintsCreator">The method that create constraints.</param>
	/// <param name="difficultyLevelCreator">The method that create a difficulty level.</param>
	/// <param name="analyzerCreator">The method that create a analyzer.</param>
	/// <param name="ittoryuFinderCreator">The method that create a ittoryu finder.</param>
	/// <param name="cancellationTokenSourceAssigner">The assigner operation for <see cref="CancellationTokenSource"/> object.</param>
	/// <param name="stateInitializer">The state-initialization operation.</param>
	/// <param name="stateReverter">The state-reverting operation.</param>
	/// <param name="bottleneckFiltersCreator">The bottleneck filters creator.</param>
	/// <param name="reportAction">The progress-report action.</param>
	/// <param name="gridStateChanger">The grid state changer.</param>
	/// <param name="gridTextConsumer">The grid text consumer, triggered on each puzzle generated.</param>
	/// <returns>A task that encapsulates asynchronous operation.</returns>
	/// <exception cref="OperationCanceledException">Throws when operation canceled.</exception>
	public static async Task GenerateAsync<TProgressDataProvider>(
		bool onlyGenerateOne,
		Func<ConstraintCollection> constraintsCreator,
		Func<ConstraintCollection, DifficultyLevel> difficultyLevelCreator,
		Func<DifficultyLevel, Analyzer> analyzerCreator,
		Func<DisorderedIttoryuFinder> ittoryuFinderCreator,
		Action<CancellationTokenSource> cancellationTokenSourceAssigner,
		Action stateInitializer,
		Action stateReverter,
		Func<BottleneckFilter[]> bottleneckFiltersCreator,
		Action<TProgressDataProvider> reportAction,
		GridStateChanger<Analyzer>? gridStateChanger,
		Action<string>? gridTextConsumer
	)
		where TProgressDataProvider : struct, IEquatable<TProgressDataProvider>, IProgressDataProvider<TProgressDataProvider>
	{
		stateInitializer();

		using var cts = new CancellationTokenSource();
		cancellationTokenSourceAssigner(cts);

		var filters = bottleneckFiltersCreator();
		var constraints = constraintsCreator();
		var difficultyLevel = difficultyLevelCreator(constraints);
		var analyzer = analyzerCreator(difficultyLevel);
		var ittoryuFinder = ittoryuFinderCreator();
		var (generatingCount, generatingFilteredCount) = (0, 0);
		try
		{
			if (onlyGenerateOne)
			{
				if (await Task.Run(taskEntry) is { IsUndefined: false } grid)
				{
					gridStateChanger?.Invoke(ref grid, analyzer);
					gridTextConsumer?.Invoke(grid.ToString("#"));
				}
			}
			else
			{
				while (true)
				{
					if (await Task.Run(taskEntry) is { IsUndefined: false } grid)
					{
						gridStateChanger?.Invoke(ref grid, analyzer);
						gridTextConsumer?.Invoke(grid.ToString("#"));

						generatingFilteredCount++;
						continue;
					}
					break;
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			stateReverter();
		}


		unsafe Grid taskEntry()
		{
			var specializedConditions = (
				HasFullHouseConstraint:
					constraints.OfType<PrimarySingleConstraint>() is [{ Primary: SingleTechniqueFlag.FullHouse }],
				HasNakedSingleConstraint:
					constraints.OfType<PrimarySingleConstraint>() is [{ Primary: SingleTechniqueFlag.NakedSingle }],
				HasFullHouseConstraintInTechniqueSet:
					constraints.OfType<TechniqueSetConstraint>() is [{ Techniques: [Technique.FullHouse] }],
				HasNakedSingleConstraintInTechniqueSet:
					constraints.OfType<TechniqueSetConstraint>() is [{ Techniques: [Technique.NakedSingle] }],
				HasIttoryuConstraint:
					constraints.OfType<IttoryuConstraint>() is [{ Operator: ComparisonOperator.Equality, Rounds: 1 }],
				HasMissingDigitConstraint:
					constraints.OfType<MissingDigitConstraint>() is [{ Digit: not -1 }],
				HasMissingHouseConstraint:
					constraints.Has<EmptyHousesCountConstraint>()
			);
			return HandlerCore(
				ref generatingCount,
				ref generatingFilteredCount,
				constraints,
				specializedConditions switch
				{
					{ HasFullHouseConstraint: true } or { HasFullHouseConstraintInTechniqueSet: true } => &Optimizer_FullHouseOnly,
					{ HasNakedSingleConstraint: true } or { HasNakedSingleConstraintInTechniqueSet: true } => &Optimizer_NakedSingleOnly,
					{ HasIttoryuConstraint: true } => &Optimizer_IttoryuMode,
					{ HasMissingDigitConstraint: true, HasIttoryuConstraint: false } => &Optimizer_MissingDigit,
					{ HasMissingHouseConstraint: true } => &Optimizer_EmptyHouses,
					_ => &DefaultGenerator
				},
				specializedConditions switch
				{
					{ HasMissingDigitConstraint: true, HasIttoryuConstraint: false } => &TransformChecker_MissingDigit,
					_ => null
				},
				specializedConditions switch
				{
					{ HasMissingDigitConstraint: true, HasIttoryuConstraint: false } => &Transformer_MissingDigit,
					_ => null
				},
				reportAction,
				specializedConditions is { HasNakedSingleConstraint: true } or { HasNakedSingleConstraintInTechniqueSet: true }
					? analyzer.WithUserDefinedOptions(analyzer.Options with { PrimarySingle = SingleTechniqueFlag.NakedSingle })
					: analyzer,
				ittoryuFinder,
				filters,
				cts.Token
			);
		}
	}

	private static unsafe Grid HandlerCore<TProgressDataProvider>(
		ref int generatingCount,
		ref int generatingFilteredCount,
		ConstraintCollection constraints,
		delegate*<int, SymmetricType, ConstraintCollection, CancellationToken, Grid> gridCreator,
		[AllowNull, MaybeNull] delegate*<in Grid, out object?, bool> gridTransformingChecker,
		[AllowNull, MaybeNull] delegate*<ref Grid, ConstraintCollection, object?, void> gridTransformer,
		Action<TProgressDataProvider> reporter,
		Analyzer analyzer,
		DisorderedIttoryuFinder finder,
		BottleneckFilter[] filters,
		CancellationToken cancellationToken
	)
		where TProgressDataProvider : struct, IEquatable<TProgressDataProvider>, IProgressDataProvider<TProgressDataProvider>
	{
		// Update generating configurations.
		if (constraints.OfType<BottleneckTechniqueConstraint>() is { Length: not 0 } list)
		{
			foreach (var element in list)
			{
				element.Filters = filters;
			}
		}

		var rng = Random.Shared;
		var symmetries = GetSymmetry(constraints);
		var chosenGivensCountSeed = GetChosenGivensCountRange(constraints);
		var givensCount = GetGivensCount(rng, chosenGivensCountSeed);
		var difficultyLevel = GetDifficultyLevel(constraints, rng);
		var progress = new SelfReportingProgress<TProgressDataProvider>(reporter);
		while (true)
		{
			var chosenSymmetricType = symmetries.Length == 0 ? SymmetricType.None : symmetries[rng.Next(0, symmetries.Length)];
			var grid = gridCreator(givensCount, chosenSymmetricType, constraints, cancellationToken);
			if (grid.IsEmpty
				|| analyzer.Analyze(grid, cancellationToken: cancellationToken) is not { IsSolved: true } analysisResult)
			{
				goto ReportState;
			}

			// Transform if worth. This transform rules may conflict with other rules so be careful to use this.
			if (gridTransformingChecker != null
				&& gridTransformingChecker(grid, out var outVariable)
				&& gridTransformer != null)
			{
				gridTransformer(ref grid, constraints, outVariable);
			}

			if (constraints.IsValidFor(new(grid, analysisResult)))
			{
				return grid;
			}

		ReportState:
			progress.Report(TProgressDataProvider.Create(++generatingCount, generatingFilteredCount));
			cancellationToken.ThrowIfCancellationRequested();
		}
	}

	private static partial DifficultyLevel GetDifficultyLevel(ConstraintCollection constraints, Random rng);
	private static partial Cell GetGivensCount(Random rng, (Cell, Cell) chosenGivensCountSeed);
	private static partial (Cell, Cell) GetChosenGivensCountRange(ConstraintCollection constraints);
	private static partial (Cell, Cell) DetermineEmptyCellsCount(BetweenRule betweenRule, Cell start, Cell end);
	private static partial ReadOnlySpan<SymmetricType> GetSymmetry(ConstraintCollection constraints);

	private static partial bool TransformChecker_MissingDigit(in Grid grid, out object? result);

	private static partial void Transformer_MissingDigit(ref Grid grid, ConstraintCollection constraints, object? variable);

	private static partial Grid Optimizer_FullHouseOnly(Cell givens, SymmetricType type, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_NakedSingleOnly(Cell givens, SymmetricType type, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_IttoryuMode(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_MissingDigit(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_EmptyHouses(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid DefaultGenerator(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
}
