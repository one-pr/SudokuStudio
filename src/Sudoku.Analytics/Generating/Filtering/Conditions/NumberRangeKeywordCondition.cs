namespace Sudoku.Generating.Filtering.Conditions;

/// <summary>
/// Represents number range keyword condition.
/// </summary>
/// <param name="minimum"><inheritdoc cref="Minimum" path="/summary"/></param>
/// <param name="maximum"><inheritdoc cref="Maximum" path="/summary"/></param>
/// <param name="includesMinimum"><inheritdoc cref="IncludesMinimum" path="/summary"/></param>
/// <param name="includesMaximum"><inheritdoc cref="IncludesMaximum" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class NumberRangeKeywordCondition(int minimum, int maximum, bool includesMinimum, bool includesMaximum) :
	KeywordCondition
{
	/// <summary>
	/// Indicates whether the range includes minimum value.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public bool IncludesMinimum { get; } = includesMinimum;

	/// <summary>
	/// Indicates whether the range includes maximum value.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public bool IncludesMaximum { get; } = includesMaximum;

	/// <summary>
	/// Indicates the minimum value.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public int Minimum { get; } = minimum;

	/// <summary>
	/// Indicates the maximum value.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public int Maximum { get; } = maximum;

	/// <inheritdoc/>
	public override KeywordVerbs Verb => KeywordVerbs.NumberRange;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] KeywordCondition? other)
		=> other is NumberRangeKeywordCondition comparer
		&& Minimum == comparer.Minimum && Maximum == comparer.Maximum
		&& IncludesMinimum == comparer.IncludesMinimum && IncludesMaximum == comparer.IncludesMaximum;

	/// <inheritdoc/>
	public override bool IsSatisifed<TStep>(TStep instance, string keyword)
		=> GetValue(instance, keyword) switch
		{
			int keywordValue => (IncludesMinimum, IncludesMaximum) switch
			{
				(true, true) => keywordValue >= Minimum && keywordValue <= Maximum,
				(true, _) => keywordValue >= Minimum && keywordValue < Maximum,
				(_, true) => keywordValue > Minimum && keywordValue <= Maximum,
				_ => keywordValue > Minimum && keywordValue < Maximum
			},
			_ => false
		};

	/// <inheritdoc/>
	public override NumberRangeKeywordCondition Clone() => new(Minimum, Maximum, IncludesMinimum, IncludesMaximum);
}
