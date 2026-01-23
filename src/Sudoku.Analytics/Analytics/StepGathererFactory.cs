namespace Sudoku.Analytics;

/// <summary>
/// Provides a list of methods that can be used for construct chaining method invocations for configuration,
/// applying to a <see cref="StepGatherer"/> instance.
/// </summary>
/// <seealso cref="StepGatherer"/>
public static class StepGathererFactory
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TStepGatherer">The type of step gatherer (<see cref="Analyzer"/> or <see cref="Collector"/>).</typeparam>
	/// <param name="instance">The current instance.</param>
	extension<TStepGatherer>(TStepGatherer instance) where TStepGatherer : StepGatherer
	{
		/// <summary>
		/// Sets the property <see cref="StepGatherer.StepSearchers"/> with the target value.
		/// </summary>
		/// <param name="stepSearchers">The value to be set or updated.</param>
		/// <returns>The instance same as the current instance.</returns>
		public TStepGatherer WithStepSearchers(params StepSearcher[] stepSearchers)
		{
			instance.StepSearchers = stepSearchers;
			return instance;
		}

		/// <summary>
		/// Try to set property <see cref="StepGatherer.StepSearchers"/> with the specified value.
		/// </summary>
		/// <param name="stepSearchers">The custom collection of <see cref="StepSearcher"/>s.</param>
		/// <param name="level">Indicates the difficulty level preserved.</param>
		/// <returns>The instance same as the current instance.</returns>
		/// <seealso cref="StepGatherer.StepSearchers"/>
		/// <seealso cref="StepSearcher"/>
		public TStepGatherer WithStepSearchers(StepSearcher[] stepSearchers, DifficultyLevel level = DifficultyLevel.Unknown)
			=> instance.WithStepSearchers(
				level == DifficultyLevel.Unknown
					? stepSearchers
					:
					from stepSearcher in stepSearchers
					where stepSearcher.Metadata.DifficultyLevelRange.Any(l => l <= level)
					select stepSearcher
			);

		/// <summary>
		/// Sets the property <see cref="StepGatherer.Options"/> with the target value.
		/// </summary>
		/// <param name="options">The value to be set or updated.</param>
		/// <returns>The instance same as the current instance.</returns>
		public TStepGatherer WithOptions(StepGathererOptions options)
		{
			instance.Options = options;
			return instance;
		}

		/// <summary>
		/// Appends an element into the property <see cref="StepGatherer.Setter"/>.
		/// </summary>
		/// <param name="setters">The value to be added.</param>
		/// <returns>The instance same as the current instance.</returns>
		public TStepGatherer ApplySetter(Action<StepSearcher> setters)
		{
			instance.Setter = setters;
			return instance;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="instance">The current instance.</param>
	extension(Analyzer instance)
	{
		/// <summary>
		/// Sets the property <see cref="Analyzer.RandomizedChoosing"/> with the target value.
		/// </summary>
		/// <param name="randomizedChoosing">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer WithRandomizedChoosing(bool randomizedChoosing)
		{
			instance.RandomizedChoosing = randomizedChoosing;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="Analyzer.IsFullApplying"/> with the target value.
		/// </summary>
		/// <param name="applyAll">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer WithApplyAll(bool applyAll)
		{
			instance.IsFullApplying = applyAll;
			return instance;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="instance">The current instance.</param>
	extension(Collector instance)
	{
		/// <summary>
		/// Sets the property <see cref="Collector.MaxStepsCollected"/> with the target value.
		/// </summary>
		/// <param name="count">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector WithMaxSteps(int count)
		{
			instance.MaxStepsCollected = count;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="Collector.DifficultyLevelMode"/> with the target value.
		/// </summary>
		/// <param name="collectingMode">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector WithSameLevelConfiguration(CollectorDifficultyLevelMode collectingMode)
		{
			instance.DifficultyLevelMode = collectingMode;
			return instance;
		}
	}
}
