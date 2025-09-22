namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Multifish</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Multifish</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_MultifishStepSearcher", Technique.Multifish)]
public sealed partial class MultifishStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		return null;
	}
}
