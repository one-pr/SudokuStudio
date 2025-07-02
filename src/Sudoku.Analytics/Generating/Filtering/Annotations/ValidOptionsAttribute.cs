namespace Sudoku.Generating.Filtering.Annotations;

/// <summary>
/// Represents an attribute that describes the valid options.
/// </summary>
/// <param name="values">The values.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ValidOptionsAttribute(params object[] values) : StepPropertyAttribute
{
	/// <summary>
	/// Indicates the values.
	/// </summary>
	public object[] Values { get; } = values;

	/// <inheritdoc/>
	public override bool IsOptional => true;
}
