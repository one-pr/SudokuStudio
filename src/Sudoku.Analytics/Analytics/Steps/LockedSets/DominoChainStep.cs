namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Domino Chain</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="_patterns"><inheritdoc cref="Patterns" path="/summary"/></param>
public sealed class DominoChainStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	AlmostLockedSetPattern[] _patterns
) : LockedSetStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 80;

	/// <summary>
	/// Represents the length of the patterns.
	/// </summary>
	public int PatternsLength => Patterns.Length;

	/// <inheritdoc/>
	public override Mask DigitsUsed
	{
		get
		{
			var result = (Mask)0;
			foreach (var pattern in Patterns)
			{
				result |= pattern.DigitsMask;
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public override Technique Code => Technique.DominoChain;

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_DominoChainLengthFactor",
				[nameof(PatternsLength)],
				GetType(),
				static args => DifficultyCalculator.OeisSequences.A002024((int)args[0]!)
			)
		];

	/// <summary>
	/// Indicates the patterns.
	/// </summary>
	public ReadOnlySpan<AlmostLockedSetPattern> Patterns => _patterns;

	/// <summary>
	/// Represents a <see cref="HashSet{T}"/> instance that will be used for comparison.
	/// </summary>
	private HashSet<AlmostLockedSetPattern> PatternsSet => [.. _patterns];


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is DominoChainStep comparer && PatternsSet.SetEquals(comparer.PatternsSet);
}
