namespace Sudoku.Generating;

/// <summary>
/// Represents an entry point for generating puzzles with mixed configuration.
/// </summary>
public static class GeneratorHub
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
			return coreHandler(
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
				cts.Token,
				specializedConditions is { HasNakedSingleConstraint: true } or { HasNakedSingleConstraintInTechniqueSet: true }
					? analyzer.WithUserDefinedOptions(analyzer.Options with { PrimarySingle = SingleTechniqueFlag.NakedSingle })
					: analyzer,
				ittoryuFinder,
				filters
			);


		}

		unsafe Grid coreHandler(
			ConstraintCollection constraints,
			delegate*<Cell, SymmetricType, ConstraintCollection, CancellationToken, Grid> gridCreator,
			[AllowNull, MaybeNull] delegate*<in Grid, out object?, bool> gridTransformingChecker,
			[AllowNull, MaybeNull] delegate*<ref Grid, ConstraintCollection, object?, void> gridTransformer,
			Action<TProgressDataProvider> reporter,
			CancellationToken cancellationToken,
			Analyzer analyzer,
			DisorderedIttoryuFinder finder,
			BottleneckFilter[] filters
		)
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
			var symmetries = getSymmetry();
			var chosenGivensCountSeed = getChosenGivensCountRange();
			var givensCount = getGivensCount();
			var difficultyLevel = getDifficultyLevel();
			var progress = new SelfReportingProgress<TProgressDataProvider>(reporter);
			while (true)
			{
				var chosenSymmetricType = symmetries.Length == 0 ? SymmetricType.None : symmetries[rng.Next(0, symmetries.Length)];
				var grid = gridCreator(givensCount, chosenSymmetricType, constraints, cancellationToken);
				if (grid.IsEmpty || analyzer.Analyze(grid) is var analysisResult && !analysisResult.IsSolved)
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


			static (Cell, Cell) b(BetweenRule betweenRule, Cell start, Cell end)
				=> betweenRule switch
				{
					BetweenRule.BothOpen => (start + 1, end - 1),
					BetweenRule.LeftOpen => (start + 1, end),
					BetweenRule.RightOpen => (start, end + 1),
					_ => (start, end)
				};

			ReadOnlySpan<SymmetricType> getSymmetry()
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

			(Cell, Cell) getChosenGivensCountRange()
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
				) is [var (br, (start, end))] ? b(br, start, end) : (-1, -1);

			Cell getGivensCount() => chosenGivensCountSeed is (var s and not -1, var e and not -1) ? rng.Next(s, e + 1) : -1;

			DifficultyLevel getDifficultyLevel()
				=> (
					from c in constraints.OfType<DifficultyLevelConstraint>()
					select c.ValidDifficultyLevels.AllFlags.ToArray()
				) is [var d] ? d[rng.Next(0, d.Length)] : DifficultyLevels.AllValid;
		}
	}

	private static bool TransformChecker_MissingDigit(in Grid grid, [NotNullWhen(true)] out object? result)
	{
		var digitsDistribution = new Dictionary<Digit, Cell>(9);
		foreach (var cell in grid.GivenCells)
		{
			var digit = grid.GetDigit(cell);
			if (!digitsDistribution.TryAdd(digit, 1))
			{
				digitsDistribution[digit]++;
			}
		}

		foreach (var digit in digitsDistribution.Keys)
		{
			if (digitsDistribution[digit] == 0)
			{
				result = digit;
				return true;
			}
		}

		result = null;
		return false;
	}

	private static void Transformer_MissingDigit(ref Grid grid, ConstraintCollection constraints, object? variable)
	{
		var digit = (int)variable!;
		var desiredDigit = constraints.OfType<MissingDigitConstraint>()[0].Digit;
		if (desiredDigit != digit)
		{
			grid.SwapDigit(digit, desiredDigit);
		}
	}

	private static Grid Optimizer_FullHouseOnly(Cell givens, SymmetricType type, ConstraintCollection constraints, CancellationToken ct)
		=> new FullHouseGenerator
		{
			SymmetricType = type,
			EmptyCellsCount = givens == -1 ? -1 : 81 - givens
		}.TryGenerateUnique(out var p, ct) ? p : throw new OperationCanceledException();

	private static Grid Optimizer_NakedSingleOnly(Cell givens, SymmetricType type, ConstraintCollection constraints, CancellationToken ct)
		=> new NakedSingleGenerator
		{
			SymmetricType = type,
			EmptyCellsCount = givens == -1 ? -1 : 81 - givens
		}.TryGenerateUnique(out var p, ct) ? p : throw new OperationCanceledException();

	private static Grid Optimizer_IttoryuMode(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct)
	{
		var finder = new DisorderedIttoryuFinder();
		var generator = new Generator();
		while (true)
		{
			var puzzle = generator.Generate(givens, symmetry, ct);
			if (puzzle.IsUndefined)
			{
				throw new OperationCanceledException();
			}

			if (finder.FindPath(puzzle, ct) is { IsComplete: true } path)
			{
				puzzle.MakeIttoryu(path);
				return puzzle;
			}
		}
	}

	private static Grid Optimizer_MissingDigit(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct)
	{
		// Set an arbitrary digit as missing digit.
		// The digit will be transformed to other one after the puzzle is satisfied the target constraints.
		var puzzle = new Generator { MissingDigit = 0 }.Generate(givens, symmetry, ct);
		return puzzle.IsUndefined ? throw new OperationCanceledException() : puzzle;
	}

	private static Grid Optimizer_EmptyHouses(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct)
	{
		var puzzle = new EmptyHouseBasedGenerator
		{
			DesiredMissingHousesMask = constraints.OfType<EmptyHousesCountConstraint>().Aggregate(
				0,
				static (interim, next) => interim | (1 << next.Count) - 1 << (int)next.HouseType * 9
			)
		}.Generate(ct);
		return puzzle.IsUndefined ? throw new OperationCanceledException() : puzzle;
	}

	private static Grid DefaultGenerator(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct)
	{
		var puzzle = new Generator().Generate(givens, symmetry, ct);
		return puzzle.IsUndefined ? throw new OperationCanceledException() : puzzle;
	}
}
