namespace Sudoku.Generating.Filtering.Annotations;

/// <summary>
/// Represents an attribute that describes the string pattern.
/// </summary>
/// <param name="pattern">The pattern.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PatternAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string pattern) : StepPropertyAttribute
{
	/// <inheritdoc/>
	public override bool IsOptional => true;

	/// <summary>
	/// Indicates the raw target pattern string.
	/// </summary>
	public string PatternString { get; } = pattern;

	/// <summary>
	/// Indicates the target pattern.
	/// </summary>
	public Regex Pattern { get; } = new(pattern);
}
