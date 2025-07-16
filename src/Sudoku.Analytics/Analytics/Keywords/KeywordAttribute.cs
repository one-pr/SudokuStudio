namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents an attribute type that describes the property is marked as special property to be filtered in reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class KeywordAttribute : Attribute
{
	/// <summary>
	/// Indicates whether the range includes minimum value. By default it's <see langword="true"/>.
	/// </summary>
	public bool IncludesMinimum { get; init; } = true;

	/// <summary>
	/// Indicates whether the range includes maximum value. By default it's <see langword="false"/>.
	/// </summary>
	public bool IncludesMaximum { get; init; } = false;

	/// <summary>
	/// Indicates the minimum value. By default it's -1.
	/// </summary>
	public int Minimum { get; init; } = -1;

	/// <summary>
	/// Indicates the maximum value. By default it's -1.
	/// </summary>
	public int Maximum { get; init; } = -1;

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
	public KeywordVerbs AllowedVerbs { get; init; }

	/// <summary>
	/// Indicates the meta type configured.
	/// </summary>
	public KeywordType MetaType { get; init; }
}
