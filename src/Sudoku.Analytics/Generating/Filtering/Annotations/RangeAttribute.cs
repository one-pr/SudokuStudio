namespace Sudoku.Generating.Filtering.Annotations;

/// <summary>
/// Represents an attribute that describes the property value range.
/// </summary>
/// <param name="min">The minimum value.</param>
/// <param name="max">The maximum value.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RangeAttribute(object min, object max) : StepPropertyAttribute
{
	/// <summary>
	/// Indicates the minimum value.
	/// </summary>
	public object Min { get; } = min;

	/// <summary>
	/// Indicates the maximum value.
	/// </summary>
	public object Max { get; } = max;

	/// <inheritdoc/>
	public override bool IsOptional => true;
}
