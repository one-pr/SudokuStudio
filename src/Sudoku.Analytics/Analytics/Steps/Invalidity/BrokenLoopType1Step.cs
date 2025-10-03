namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Broken Loop</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="loop"><inheritdoc cref="BrokenLoopStep.Loop" path="/summary"/></param>
/// <param name="guardian"><inheritdoc cref="Guardian" path="/summary"/></param>
public sealed class BrokenLoopType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	ReadOnlyMemory<Candidate> loop,
	Candidate guardian
) : BrokenLoopStep(conclusions, views, options, loop, guardian.AsCandidateMap())
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <summary>
	/// Indicates the guardian.
	/// </summary>
	public Candidate Guardian { get; } = guardian;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [LoopStr, GuardiansStr]), new(SR.ChineseLanguage, [LoopStr, GuardiansStr])];
}
