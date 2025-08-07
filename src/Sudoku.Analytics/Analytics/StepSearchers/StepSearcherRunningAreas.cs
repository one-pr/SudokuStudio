namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides constants of <see cref="StepSearcherRunningArea"/>.
/// </summary>
/// <seealso cref="StepSearcherRunningArea"/>
public static class StepSearcherRunningAreas
{
	/// <summary>
	/// Indicates both areas are included.
	/// </summary>
	public const StepSearcherRunningArea Both = StepSearcherRunningArea.Searching | StepSearcherRunningArea.Collecting;
}
