namespace Sudoku.Generating.Filtering.Conditions;

/// <summary>
/// Represents string equality condition.
/// </summary>
/// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
[TypeImpl(
	TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString,
	ToStringBehavior = ToStringBehavior.RecordLike)]
public sealed partial class StringEqualityComparisonKeywordCondition(string value) : KeywordCondition
{
	/// <summary>
	/// Indicates the value to be compared.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public string Value { get; } = value;

	/// <inheritdoc/>
	public override KeywordVerbs Verb => KeywordVerbs.StringEqualityComparison;


	/// <inheritdoc/>
	public override bool IsSatisifed<TStep>(TStep instance, string keyword)
		=> GetValue(instance, keyword) switch
		{
			string str => str == Value,
			Enum field => field.ToString() == Value,
			_ => false
		};

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] KeywordCondition? other)
		=> other is StringEqualityComparisonKeywordCondition comparer && Value == comparer.Value;

	/// <inheritdoc/>
	public override StringEqualityComparisonKeywordCondition Clone() => new(Value);
}
