namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="TechniqueView.SelectedTechniquesChanged"/>.
/// </summary>
/// <param name="techniqueSet"><inheritdoc cref="TechniqueSet" path="/summary"/></param>
/// <seealso cref="TechniqueView.SelectedTechniquesChanged"/>
public sealed class TechniqueViewSelectedTechniquesChangedEventArgs(params TechniqueSet techniqueSet) : EventArgs
{
	/// <summary>
	/// The technique set to be assigned.
	/// </summary>
	public TechniqueSet TechniqueSet { get; } = techniqueSet;
}
