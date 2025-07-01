namespace Sudoku.Generating.Filtering.Annotations;

/// <summary>
/// Represents an attribute that will be used by a <see cref="Step"/> derived type.
/// </summary>
/// <seealso cref="Step"/>
public abstract class StepAttribute : StepAnnotationAttribute
{
	/// <inheritdoc/>
	public sealed override BasedOn AnnotationBasedOn => BasedOn.Type;
}
