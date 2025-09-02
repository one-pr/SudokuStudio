namespace Sudoku.Analytics.Primitives;

/// <summary>
/// Represents searcher hub type.
/// </summary>
internal abstract class StepSearcherHubBase
{
	/// <summary>
	/// Indicates supported step searcher types.
	/// </summary>
	public abstract ReadOnlyMemory<Type> SupportedStepSearcherTypes { get; }
}
