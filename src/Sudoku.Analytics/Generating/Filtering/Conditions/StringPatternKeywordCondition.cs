namespace Sudoku.Generating.Filtering.Conditions;

/// <summary>
/// Represents a keyword condition.
/// </summary>
/// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class StringPatternKeywordCondition([StringSyntax(StringSyntaxAttribute.Regex)] string value) : KeywordCondition
{
	/// <summary>
	/// Indicates the value to be checked.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public string Value { get; } = value;

	/// <inheritdoc/>
	public override KeywordVerbs Verb => KeywordVerbs.StringPattern;

	/// <summary>
	/// Indicates the pattern to be used.
	/// </summary>
	private Regex Pattern => new(Value);


	/// <inheritdoc/>
	public override bool IsValueValid()
	{
		try
		{
			Regex.Match(string.Empty, Value);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] KeywordCondition? other)
		=> other is StringPatternKeywordCondition comparer && Value == comparer.Value;

	/// <inheritdoc/>
	public override bool IsSatisifed<TStep>(TStep instance, string keyword)
		=> GetValue(instance, keyword) switch
		{
			string str => Pattern.IsMatch(str),
			Enum field => Pattern.IsMatch(field.ToString()),
			_ => false
		};

	/// <inheritdoc/>
	public override StringPatternKeywordCondition Clone() => new(Value);
}
