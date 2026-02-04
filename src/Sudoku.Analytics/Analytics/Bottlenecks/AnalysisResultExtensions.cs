namespace Sudoku.Analytics.Bottlenecks;

/// <summary>
/// Provides with extension methods on <see cref="AnalysisResult"/>.
/// </summary>
/// <seealso cref="AnalysisResult"/>
public static class AnalysisResultExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(AnalysisResult @this)
	{
		/// <summary>
		/// Try to get bottleneck steps under the specified rules.
		/// </summary>
		/// <param name="filters">The bottleneck filters.</param>
		/// <returns>A list of bottleneck steps.</returns>
		/// <exception cref="NotSupportedException">
		/// Throws when the filter contains invalid configuration,
		/// like <see cref="BottleneckType.SingleStepOnly"/> in full-marking mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">Throws when the puzzle is not fully solved.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Throws when argument <paramref name="filters"/> contains one filter holding an undefined <see cref="BottleneckType"/> flag.
		/// </exception>
		public ReadOnlySpan<Step> GetBottlenecks(params ReadOnlySpan<BottleneckFilter> filters)
		{
			if (!@this.IsSolved)
			{
				throw new InvalidOperationException(SR.ExceptionMessage("BottlenecksShouldBeAvailableInSolvedPuzzle"));
			}

			if (@this.StepsSpan is not { Length: not 0 } steps)
			{
				return [];
			}

			var pencilmarkMode = steps.Aggregate(
				TechniqueType.None,
				static (interim, next) => interim | next switch
				{
					AdvancedStep => TechniqueType.Advanced,
					SnyderStep => TechniqueType.Snyder,
					DirectStep => TechniqueType.Direct,
					_ => TechniqueType.None
				}
			);
			var filterMode = pencilmarkMode.HasFlag(TechniqueType.Advanced)
				? TechniqueType.Advanced
				: pencilmarkMode.HasFlag(TechniqueType.Snyder)
					? TechniqueType.Snyder
					: TechniqueType.Direct;
			return (filters.FirstRefOrNullRef((in f) => f.TechniqueType == filterMode).BottleneckType, filterMode) switch
			{
				(BottleneckType.SingleStepOnly, TechniqueType.Direct or TechniqueType.Snyder) => singleStepOnly(),
				(BottleneckType.SingleStepSameLevelOnly, TechniqueType.Snyder) => singleStepSameLevelOnly(),
				(BottleneckType.EliminationGroup, TechniqueType.Advanced) => eliminationGroup(steps),
				(BottleneckType.SequentialInversion, not TechniqueType.Direct) => sequentialInversion(steps),
				(BottleneckType.HardestRating, _) => hardestRating(steps, steps.MaxBy(static s => s.Difficulty)!),
				(BottleneckType.HardestLevel, not TechniqueType.Direct) => hardestLevel(steps, steps.MaxBy(static s => (int)s.DifficultyLevel)!),
				_ => throw new ArgumentOutOfRangeException(nameof(filters))
			};


			ReadOnlySpan<Step> singleStepOnly()
			{
				var collector = GridSnyderExtensions.Collector;
				var result = new List<Step>();
				foreach (var (g, s) in Step.Combine(@this.GridsSpan, @this.StepsSpan))
				{
					if ((
						from SingleStep step in collector.Collect(g)
						select step.Cell * 9 + step.Digit
					).AsCandidateMap().Count == 1)
					{
						result.Add(s);
					}
				}
				return result.AsSpan();
			}

			ReadOnlySpan<Step> singleStepSameLevelOnly()
			{
				var collector = GridSnyderExtensions.Collector;
				var result = new List<Step>();
				foreach (var (g, s) in Step.Combine(@this.GridsSpan, @this.StepsSpan))
				{
					var currentStepPencilmarkVisibility = s.PencilmarkType;
					if ((
						from SingleStep step in collector.Collect(g)
						where step.PencilmarkType <= currentStepPencilmarkVisibility
						select step.Cell * 9 + step.Digit
					).AsCandidateMap().Count == 1)
					{
						result.Add(s);
					}
				}
				return result.AsSpan();
			}

			static ReadOnlySpan<Step> eliminationGroup(ReadOnlySpan<Step> steps)
			{
				var result = new List<Step>();
				for (var i = 0; i < steps.Length - 1; i++)
				{
					if (steps[i].IsAssignment is false)
					{
						for (var j = i + 1; j < steps.Length; j++)
						{
							if (steps[j].IsAssignment is not false)
							{
								// Okay. Now we have a group of steps that only produce eliminations.
								// Set the outer loop pointer to skip elimination steps.
								result.Add(steps[i = j]);
								break;
							}
						}
					}
				}
				return result.AsSpan();
			}

			static ReadOnlySpan<Step> sequentialInversion(ReadOnlySpan<Step> steps)
			{
				var result = new List<Step>();
				for (var i = 0; i < steps.Length - 1; i++)
				{
					var (previous, next) = (steps[i], steps[i + 1]);
					if (previous.DifficultyLevel > next.DifficultyLevel && next.DifficultyLevel != DifficultyLevel.Unknown)
					{
						result.Add(previous);
					}
				}
				return result.AsSpan();
			}

			static ReadOnlySpan<Step> hardestRating(ReadOnlySpan<Step> steps, Step maxStep)
			{
				var result = new List<Step>();
				foreach (var element in steps)
				{
					if (element.Code == maxStep.Code)
					{
						result.Add(element);
					}
				}
				return result.AsSpan();
			}

			static ReadOnlySpan<Step> hardestLevel(ReadOnlySpan<Step> steps, Step maxStep)
			{
				var result = new List<Step>();
				foreach (var element in steps)
				{
					if (element.DifficultyLevel == maxStep.DifficultyLevel)
					{
						result.Add(element);
					}
				}
				return result.AsSpan();
			}
		}
	}
}

/// <include
///     file="../../global-doc-comments.xml"
///     path="g/csharp11/feature[@name='file-local']/target[@name='class' and @when='extension']"/>
file static class Extensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(ReadOnlySpan<T> @this) where T : struct
	{
		/// <inheritdoc cref="IFirstLastMethod{TSelf, TSource}.FirstOrDefault(Func{TSource, bool})"/>
		public ref readonly T FirstRefOrNullRef(LargePredicate<T> predicate)
		{
			foreach (ref readonly var element in @this)
			{
				if (predicate(element))
				{
					return ref element;
				}
			}
			return ref Unsafe.NullRef<T>();
		}
	}
}

/// <summary>
/// Represents a type that determine the instance satisfies the specified condition.
/// </summary>
/// <typeparam name="T">The type of instance.</typeparam>
/// <param name="instance">The instance.</param>
/// <returns>A <see cref="bool"/> result.</returns>
file delegate bool LargePredicate<T>(in T instance);
