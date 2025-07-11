namespace SudokuStudio.Views.Pages.Operation;

/// <summary>
/// Indicates the generating operation command bar.
/// </summary>
public sealed partial class GeneratingOperation : Page, IOperationProviderPage
{
	/// <summary>
	/// The fields for core usages on counting puzzles.
	/// </summary>
	private int _generatingCount, _generatingFilteredCount;


	/// <summary>
	/// Initializes a <see cref="GeneratingOperation"/> instance.
	/// </summary>
	public GeneratingOperation() => InitializeComponent();


	/// <inheritdoc/>
	public AnalyzePage BasePage { get; set; } = null!;


	/// <inheritdoc/>
	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		if (e.Parameter is AnalyzePage p)
		{
			SetGeneratingStrategyTooltip(p);
			RefreshPuzzleLibraryComboBox();
		}
	}

	/// <summary>
	/// Set generating strategy tooltip.
	/// </summary>
	/// <param name="basePage">The base page.</param>
	private void SetGeneratingStrategyTooltip(AnalyzePage basePage)
	{
		var constraints = Application.Current.AsApp().Preference.ConstraintPreferences.Constraints;
		TextBlockBindable.SetInlines(
			GeneratorStrategyTooltip,
			[new Run { Text = string.Join(Environment.NewLine, from c in constraints select c.ToString(App.CurrentCulture)) }]
		);
	}

	/// <summary>
	/// Refreshes puzzle library for combo box.
	/// </summary>
	private void RefreshPuzzleLibraryComboBox()
	{
		var libs = LibrarySimpleBindableSource.GetLibraries();
		(PuzzleLibraryChoser.Visibility, LibraryPuzzleFetchButton.Visibility, LibSeparator.Visibility) = libs.Length != 0
			? (Visibility.Visible, Visibility.Visible, Visibility.Visible)
			: (Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed);
		PuzzleLibraryChoser.ItemsSource = (from lib in libs select new LibrarySimpleBindableSource { Library = lib }).ToArray();

		var lastFileId = Application.Current.AsApp().Preference.UIPreferences.FetchingPuzzleLibrary;
		PuzzleLibraryChoser.SelectedIndex = Array.FindIndex(libs, match) is var index and not -1 ? index : 0;


		bool match(Library lib) => io::Path.GetFileNameWithoutExtension(lib.LibraryPath) == lastFileId;
	}

	/// <summary>
	/// Handle generating operation.
	/// </summary>
	/// <typeparam name="TProgressDataProvider">The type of the progress data provider.</typeparam>
	/// <param name="onlyGenerateOne">Indicates whether the generator engine only generates for one puzzle.</param>
	/// <param name="gridStateChanger">
	/// The method that can change the state of the target grid. This callback method will be used for specify the grid state
	/// when a user has set the techniques that must be appeared.
	/// </param>
	/// <param name="gridTextConsumer">An action that consumes the generated <see cref="string"/> grid text code.</param>
	/// <returns>The task that holds the asynchronous operation.</returns>
	private async Task HandleGeneratingAsync<TProgressDataProvider>(
		bool onlyGenerateOne,
		GridStateChanger<Analyzer>? gridStateChanger = null,
		Action<string>? gridTextConsumer = null
	) where TProgressDataProvider : struct, IEquatable<TProgressDataProvider>, IProgressDataProvider<TProgressDataProvider>
	{
		BasePage.IsGeneratorLaunched = true;
		BasePage.ClearAnalyzeTabsData();

		var processingText = SR.Get("AnalyzePage_GeneratorIsProcessing", App.CurrentCulture);
		var constraints = Application.Current.AsApp().Preference.ConstraintPreferences.Constraints;
		var difficultyLevel = (from c in constraints.OfType<DifficultyLevelConstraint>() select c.DifficultyLevel) is [var dl] ? dl : default;
		var analyzer = Application.Current.AsApp().GetAnalyzerConfigured(BasePage.SudokuPane, difficultyLevel);
		var ittoryuFinder = new DisorderedIttoryuFinder(TechniqueIttoryuSets.IttoryuTechniques);
		var analysisPref = Application.Current.AsApp().Preference.AnalysisPreferences;
		var filters = (BottleneckFilter[])[
			new(PencilmarkVisibility.Direct, analysisPref.DirectModeBottleneckType),
			new(PencilmarkVisibility.PartialMarking, analysisPref.PartialMarkingModeBottleneckType),
			new(PencilmarkVisibility.FullMarking, analysisPref.FullMarkingModeBottleneckType)
		];

		using var cts = new CancellationTokenSource();
		BasePage._ctsForAnalyzingRelatedOperations = cts;

		try
		{
			(_generatingCount, _generatingFilteredCount) = (0, 0);
			if (onlyGenerateOne)
			{
				if (await Task.Run(taskEntry) is { IsUndefined: false } grid)
				{
					triggerEvents(ref grid, analyzer);
				}
			}
			else
			{
				while (true)
				{
					if (await Task.Run(taskEntry) is { IsUndefined: false } grid)
					{
						triggerEvents(ref grid, analyzer);

						_generatingFilteredCount++;
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
			BasePage._ctsForAnalyzingRelatedOperations = null;
			BasePage.IsGeneratorLaunched = false;
		}


		void triggerEvents(ref Grid grid, Analyzer analyzer)
		{
			gridStateChanger?.Invoke(ref grid, analyzer);
			gridTextConsumer?.Invoke(grid.ToString("#"));
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
					{ HasFullHouseConstraint: true } or { HasFullHouseConstraintInTechniqueSet: true } => &handler_FullHouse,
					{ HasNakedSingleConstraint: true } or { HasNakedSingleConstraintInTechniqueSet: true } => &handler_NakedSingle,
					{ HasIttoryuConstraint: true } => &handler_Ittoryu,
					{ HasMissingDigitConstraint: true, HasIttoryuConstraint: false } => &handler_MissingDigit,
					{ HasMissingHouseConstraint: true } => &handler_EmptyHouses,
					_ => &handler_Default
				},
				specializedConditions switch
				{
					{ HasMissingDigitConstraint: true, HasIttoryuConstraint: false } => &transformChecker_MissingDigit,
					_ => null
				},
				specializedConditions switch
				{
					{ HasMissingDigitConstraint: true, HasIttoryuConstraint: false } => &transformer_MissingDigit,
					_ => null
				},
				progress => DispatcherQueue.TryEnqueue(
					() =>
					{
						BasePage.AnalyzeProgressLabel.Text = processingText;
						BasePage.AnalyzeStepSearcherNameLabel.Text = progress.ToDisplayString();
					}
				),
				cts.Token,
				specializedConditions is { HasNakedSingleConstraint: true } or { HasNakedSingleConstraintInTechniqueSet: true }
					? analyzer.WithUserDefinedOptions(analyzer.Options with { PrimarySingle = SingleTechniqueFlag.NakedSingle })
					: analyzer,
				ittoryuFinder,
				filters
			);


			static Grid handler_FullHouse(Cell givens, SymmetricType type, ConstraintCollection _, CancellationToken ct)
				=> new FullHouseGenerator
				{
					SymmetricType = type,
					EmptyCellsCount = givens == -1 ? -1 : 81 - givens
				}.TryGenerateUnique(out var p, ct) ? p : throw new OperationCanceledException();

			static Grid handler_NakedSingle(Cell givens, SymmetricType type, ConstraintCollection _, CancellationToken ct)
				=> new NakedSingleGenerator
				{
					SymmetricType = type,
					EmptyCellsCount = givens == -1 ? -1 : 81 - givens
				}.TryGenerateUnique(out var p, ct) ? p : throw new OperationCanceledException();

			static Grid handler_Ittoryu(Cell givens, SymmetricType symmetry, ConstraintCollection _, CancellationToken ct)
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

			static Grid handler_MissingDigit(Cell givens, SymmetricType symmetry, ConstraintCollection _, CancellationToken ct)
			{
				// Set an arbitrary digit as missing digit.
				// The digit will be transformed to other one after the puzzle is satisfied the target constraints.
				var puzzle = new Generator { MissingDigit = 0 }.Generate(givens, symmetry, ct);
				return puzzle.IsUndefined ? throw new OperationCanceledException() : puzzle;
			}

			static Grid handler_EmptyHouses(Cell _, SymmetricType __, ConstraintCollection constraints, CancellationToken ct)
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

			static Grid handler_Default(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct)
			{
				var puzzle = new Generator().Generate(givens, symmetry, ct);
				return puzzle.IsUndefined ? throw new OperationCanceledException() : puzzle;
			}

			static bool transformChecker_MissingDigit(in Grid grid, [NotNullWhen(true)] out object? result)
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

			static void transformer_MissingDigit(ref Grid grid, ConstraintCollection constraints, object? variable)
			{
				var digit = (int)variable!;
				var desiredDigit = constraints.OfType<MissingDigitConstraint>()[0].Digit;
				if (desiredDigit != digit)
				{
					grid.SwapDigit(digit, desiredDigit);
				}
			}
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

				// Transform if worth.
				// This transform rules may conflict with other rules so be careful to use this.
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
				progress.Report(TProgressDataProvider.Create(++_generatingCount, _generatingFilteredCount));
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


	private async void NewPuzzleButton_ClickAsync(object sender, RoutedEventArgs e)
		=> await HandleGeneratingAsync<GeneratorProgress>(
			true,
			gridTextConsumer: gridText =>
			{
				var grid = Grid.Parse(gridText);
				if (Application.Current.AsApp().Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					Application.Current.AsApp().PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = grid });
				}

				BasePage.SudokuPane.Puzzle = grid;
				BasePage.ClearAnalyzeTabsData();
				BasePage.SudokuPane.ViewUnit = null;
			}
		);

	private async void LibraryPuzzleFetchButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var library = ((LibrarySimpleBindableSource)PuzzleLibraryChoser.SelectedValue).Library;
		if (library.IsEmpty)
		{
			// There is no puzzle can be selected.
			return;
		}

		var types = Application.Current.AsApp().Preference.LibraryPreferences.LibraryPuzzleTransformations;
		BasePage.SudokuPane.Puzzle = await library.RandomReadOneAsync(types);
		BasePage.ClearAnalyzeTabsData();
		BasePage.SudokuPane.ViewUnit = null;
	}

	private void PuzzleLibraryChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var source = ((LibrarySimpleBindableSource)PuzzleLibraryChoser.SelectedValue).Library;
		var fileId = io::Path.GetFileNameWithoutExtension(source.LibraryPath);
		Application.Current.AsApp().Preference.UIPreferences.FetchingPuzzleLibrary = fileId;
	}

	private async void BatchGeneratingToLibraryButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Title = SR.Get("AnalyzePage_AddPuzzleToLibraryDialogTitle", App.CurrentCulture),
			IsPrimaryButtonEnabled = true,
			DefaultButton = ContentDialogButton.Primary,
			PrimaryButtonText = SR.Get("AnalyzePage_AddPuzzleToLibraryDialogSure", App.CurrentCulture),
			CloseButtonText = SR.Get("AnalyzePage_AddPuzzleToLibraryDialogCancel", App.CurrentCulture),
			Content = new SaveToLibraryDialogContent { AvailableLibraries = LibraryBindableSource.GetLibrariesFromLocal() }
		};
		if (await dialog.ShowAsync() != ContentDialogResult.Primary)
		{
			return;
		}

		var appendToLibraryTask = static (string _, CancellationToken _ = default) => default(ValueTask)!;
		switch ((SaveToLibraryDialogContent)dialog.Content)
		{
			case { SelectedMode: 0, SelectedLibrary: LibraryBindableSource { Library: var lib } }:
			{
				appendToLibraryTask = lib.WriteLineAsync;
				break;
			}
			case { SelectedMode: 1, IsNameValidAsFileId: true } content:
			{
				var libraryCreated = new Library(CommonPaths.Library, content.FileId);
				if (content.LibraryName is var name and not (null or ""))
				{
					libraryCreated.WriteName(name);
				}
				if (content.LibraryAuthor is var author and not (null or ""))
				{
					libraryCreated.WriteAuthor(author);
				}
				if (content.LibraryDescription is var description and not (null or ""))
				{
					libraryCreated.WriteDescription(description);
				}
				if (content.LibraryTags is { Count: not 0 } tags)
				{
					libraryCreated.WriteTags([.. tags]);
				}
				appendToLibraryTask = libraryCreated.WriteLineAsync;
				break;
			}
		}

		await HandleGeneratingAsync<FilteredGeneratorProgress>(
			false,
			static (ref grid, analyzer) =>
			{
				var analysisResult = analyzer.Analyze(grid);
				if (analysisResult is not { IsSolved: true, GridsSpan: var grids, StepsSpan: var steps })
				{
					return;
				}

				var techniques = TechniqueSets.None;
				foreach (var constraint in Application.Current.AsApp().Preference.ConstraintPreferences.Constraints)
				{
					switch (constraint)
					{
						case TechniqueConstraint { Techniques: var t }:
						{
							techniques |= t;
							break;
						}
						case TechniqueCountConstraint { Technique: var technique } and not { Operator: ComparisonOperator.Equality, LimitCount: 0 }:
						{
							techniques.Add(technique);
							break;
						}
					}
				}

				foreach (var (g, s) in Step.Combine(grids, steps))
				{
					if (techniques.Contains(s.Code))
					{
						grid = g;
						break;
					}
				}
			},
			async gridText =>
			{
				await appendToLibraryTask($"{gridText}{Environment.NewLine}");

				var app = Application.Current.AsApp();
				if (app.Preference.UIPreferences.AlsoSaveBatchGeneratedPuzzlesIntoHistory
					&& app.Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					app.PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = Grid.Parse(gridText) });
				}
			}
		);
	}

	private async void BatchGeneratingButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		if (!BasePage.EnsureUnsnapped(true))
		{
			return;
		}

		var fsp = new FileSavePicker();
		fsp.Initialize(this);
		fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fsp.SuggestedFileName = SR.Get("Sudoku", App.CurrentCulture);
		fsp.AddFileFormat(FileFormats.PlainText);

		if (await fsp.PickSaveFileAsync() is not { Path: var filePath })
		{
			return;
		}

		await HandleGeneratingAsync<FilteredGeneratorProgress>(
			false,
			static (ref grid, analyzer) =>
			{
				var analysisResult = analyzer.Analyze(grid);
				if (analysisResult is not { IsSolved: true, GridsSpan: var grids, StepsSpan: var steps })
				{
					return;
				}

				var techniques = TechniqueSets.None;
				foreach (var constraint in Application.Current.AsApp().Preference.ConstraintPreferences.Constraints)
				{
					switch (constraint)
					{
						case TechniqueConstraint { Techniques: var t }:
						{
							techniques |= t;
							break;
						}
						case TechniqueCountConstraint { Technique: var technique } and not { Operator: ComparisonOperator.Equality, LimitCount: 0 }:
						{
							techniques.Add(technique);
							break;
						}
					}
				}

				foreach (var (g, s) in Step.Combine(grids, steps))
				{
					if (techniques.Contains(s.Code))
					{
						grid = g;
						break;
					}
				}
			},
			gridText =>
			{
				File.AppendAllText(filePath, $"{gridText}{Environment.NewLine}");

				var app = Application.Current.AsApp();
				if (app.Preference.UIPreferences.AlsoSaveBatchGeneratedPuzzlesIntoHistory
					&& app.Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					app.PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = Grid.Parse(gridText) });
				}
			}
		);
	}
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Randomly read one puzzle in the specified file, and return it.
	/// </summary>
	/// <param name="this">Indicates the current instance.</param>
	/// <param name="transformTypes">
	/// Indicates the available transform type that the chosen grid can be transformed.
	/// Use <see cref="TransformType"/>.<see langword="operator"/> |(<see cref="TransformType"/>, <see cref="TransformType"/>)
	/// to combine multiple flags.
	/// </param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task{TResult}"/> of <see cref="Grid"/> instance as the result.</returns>
	/// <exception cref="InvalidOperationException">Throw when the library file is not initialized.</exception>
	/// <seealso href="http://tinyurl.com/choose-a-random-element">Choose a random element from a sequence of unknown length</seealso>
	public static async Task<Grid> RandomReadOneAsync(
		this Library @this,
		TransformType transformTypes = TransformType.None,
		CancellationToken cancellationToken = default
	)
	{
		var chosen = Grid.Parse(await @this.SelectOneAsync(cancellationToken));
		chosen.Transform(transformTypes);
		return chosen;
	}
}
