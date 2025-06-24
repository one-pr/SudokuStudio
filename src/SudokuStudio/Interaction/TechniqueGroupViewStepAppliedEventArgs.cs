namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="TechniqueGroupView.StepApplied"/>.
/// </summary>
/// <param name="step"><inheritdoc cref="ChosenStep" path="/summary"/></param>
/// <seealso cref="TechniqueGroupView.StepApplied"/>
public sealed class TechniqueGroupViewStepAppliedEventArgs(Step step) : EventArgs
{
	/// <summary>
	/// The step.
	/// </summary>
	public Step ChosenStep { get; } = step;
}
