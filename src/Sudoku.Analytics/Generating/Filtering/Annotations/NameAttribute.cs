namespace Sudoku.Generating.Filtering.Annotations;

/// <summary>
/// Represents an attribute that describes the qualified name of a property in step.
/// </summary>
/// <param name="nameKey">The name key.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NameAttribute(string nameKey) : StepPropertyAttribute
{
	/// <inheritdoc/>
	public override bool IsOptional => false;

	/// <summary>
	/// Represents the key at resource file.
	/// </summary>
	public string NameKey { get; } = nameKey;

	/// <summary>
	/// Represents description key at resource file.
	/// </summary>
	public string? DescriptionKey { get; init; }
}
