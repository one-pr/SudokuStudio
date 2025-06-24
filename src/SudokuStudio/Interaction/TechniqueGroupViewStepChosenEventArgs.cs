namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="TechniqueGroupView.StepChosen"/>.
/// </summary>
/// <param name="step"><inheritdoc cref="ChosenStep" path="/summary"/></param>
/// <seealso cref="TechniqueGroupView.StepChosen"/>
public sealed class TechniqueGroupViewStepChosenEventArgs(Step step) : EventArgs
{
	/// <summary>
	/// The step.
	/// </summary>
	public Step ChosenStep { get; } = step;
}
