namespace Sudoku.Generating.Filtering.Conditions;

/// <summary>
/// Represents number equality keyword condition.
/// </summary>
/// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
/// <param name="operator"><inheritdoc cref="Operator" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class NumberComparisonKeywordCondition(int value, ComparisonOperator @operator) : KeywordCondition
{
	/// <summary>
	/// Indicates the value to be compared.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public int Value { get; } = value;

	/// <summary>
	/// Indicates the operator to be used.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public ComparisonOperator Operator { get; } = @operator;

	/// <inheritdoc/>
	public override KeywordVerbs Verb => KeywordVerbs.NumberComparison;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] KeywordCondition? other)
		=> other is NumberComparisonKeywordCondition comparer && Value == comparer.Value && Operator == comparer.Operator;

	/// <inheritdoc/>
	public override bool IsSatisifed<TStep>(TStep instance, string keyword)
		=> GetValue(instance, keyword) switch
		{
			int keywordValue => Operator.GetOperator<int>()(keywordValue, Value),
			_ => false
		};

	/// <inheritdoc/>
	public override NumberComparisonKeywordCondition Clone() => new(Value, Operator);
}
