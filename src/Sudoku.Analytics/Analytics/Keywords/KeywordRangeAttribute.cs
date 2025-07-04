namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents an attribute type that describes the range of a keyword.
/// </summary>
/// <param name="minimum"><inheritdoc cref="Minimum" path="/summary"/></param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
[method: SetsRequiredMembers]
public sealed class KeywordRangeAttribute(int minimum) : KeywordConditionAttribute
{
	/// <summary>
	/// Indicates the minimum value.
	/// </summary>
	public required int Minimum { get; init; } = minimum;

	/// <summary>
	/// Indicates the maximum value. By default it's -1.
	/// </summary>
	public int Maximum { get; init; } = -1;

	/// <summary>
	/// Indicates whether the range includes minimum value. By default it's <see langword="true"/>.
	/// </summary>
	public bool IncludesMinimum { get; init; } = true;

	/// <summary>
	/// Indicates whether the range includes maximum value. By default it's <see langword="false"/>.
	/// </summary>
	public bool IncludesMaximum { get; init; } = false;
}
