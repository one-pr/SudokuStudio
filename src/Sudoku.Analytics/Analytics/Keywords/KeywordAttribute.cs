namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents an attribute type that describes the property is marked as special property to be filtered in reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class KeywordAttribute : Attribute
{
	/// <summary>
	/// Indicates the name resource key to the property.
	/// </summary>
	public required string NameResourceKey { get; init; }

	/// <summary>
	/// Indicates the description resource key to the property. By default it's <see langword="null"/>.
	/// </summary>
	public string? DescriptionResourceKey { get; init; }

	/// <summary>
	/// Indicates the allowed verbs in runtime.
	/// </summary>
	public required KeywordVerbs AllowedVerbs { get; init; }
}
