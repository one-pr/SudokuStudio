namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents an attribute type that describes the property is marked as special property to be filtered in reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class KeywordAttribute : Attribute
{
	/// <summary>
	/// Indicates the name resource key. By default it's <c>"DefaultName"</c>,
	/// which represents string <c>"&lt;Unknown&gt;"</c> in resource dictionary.
	/// </summary>
	public string NameResourceKey { get; init; } = "DefaultName";

	/// <summary>
	/// Indicates the description resource key to the property. By default it's <see langword="null"/>.
	/// </summary>
	public string? DescriptionResourceKey { get; init; }

	/// <summary>
	/// Indicates the allowed verbs in runtime.
	/// </summary>
	public required KeywordVerb[] AllowedVerbs { get; init; }
}
