namespace Sudoku.Generating.Filtering.Annotations;

/// <summary>
/// Represents an annotation that will be used for displaying metadata.
/// </summary>
public abstract partial class StepAnnotationAttribute : Attribute
{
	/// <summary>
	/// Indicates the item based on.
	/// </summary>
	public abstract BasedOn AnnotationBasedOn { get; }
}
