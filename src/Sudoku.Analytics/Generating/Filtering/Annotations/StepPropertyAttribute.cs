namespace Sudoku.Generating.Filtering.Annotations;

/// <summary>
/// Represents an attribute that will be used by a step property.
/// </summary>
public abstract class StepPropertyAttribute : StepAnnotationAttribute
{
	/// <summary>
	/// Indicates whether the attribute type is optional.
	/// </summary>
	public abstract bool IsOptional { get; }

	/// <inheritdoc/>
	public sealed override BasedOn AnnotationBasedOn => BasedOn.Member;
}
