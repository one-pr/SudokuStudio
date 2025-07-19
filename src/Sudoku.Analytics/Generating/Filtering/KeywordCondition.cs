namespace Sudoku.Generating.Filtering;

/// <summary>
/// Represents a keyword condition.
/// </summary>
[TypeImpl(
	TypeImplFlags.AllObjectMethods | TypeImplFlags.EqualityOperators,
	GetHashCodeBehavior = GetHashCodeBehavior.MakeAbstract,
	OtherModifiersOnEquals = "sealed",
	OtherModifiersOnToString = "sealed")]
[JsonDerivedType(typeof(StringPatternKeywordCondition), nameof(StringPatternKeywordCondition))]
[JsonDerivedType(typeof(StringEqualityComparisonKeywordCondition), nameof(StringEqualityComparisonKeywordCondition))]
[JsonDerivedType(typeof(NumberRangeKeywordCondition), nameof(NumberRangeKeywordCondition))]
[JsonDerivedType(typeof(NumberComparisonKeywordCondition), nameof(NumberComparisonKeywordCondition))]
public abstract partial class KeywordCondition :
	ICloneable,
	IEquatable<KeywordCondition>,
	IEqualityOperators<KeywordCondition, KeywordCondition, bool>,
	IFormattable
{
	/// <summary>
	/// Indicates the verb supported.
	/// </summary>
	public abstract KeywordVerbs Verb { get; }


	/// <summary>
	/// Determine whether the condition specified is satisfied.
	/// </summary>
	/// <param name="instance">The instance.</param>
	/// <param name="keyword">The keyword (a property that will be checked, specified by its name).</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public abstract bool IsSatisifed(Step instance, string keyword);

	/// <summary>
	/// Determines whether the specified value configured is valid.
	/// </summary>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public virtual bool IsValueValid() => true;

	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] KeywordCondition? other);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public abstract string ToString(IFormatProvider? formatProvider);

	/// <inheritdoc cref="ICloneable.Clone"/>
	public abstract KeywordCondition Clone();

	/// <summary>
	/// Returns the value of the property in a step instance;
	/// if <see cref="KeywordAttribute.KeywordConverterType"/> configured,
	/// a value converter will be invoked, and cast from original value into keyword-type-compatible one.
	/// </summary>
	/// <param name="instance">The instance.</param>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The real value set. You should cast to target type if you want.</returns>
	/// <exception cref="InvalidKeywordException">
	/// Throws when the keyword is not found in its containing type, or it is not a keyword.
	/// </exception>
	protected object? GetValue(Step instance, string keyword)
	{
		var instanceType = instance.GetType();
		if (!Keyword.IsKeyword(keyword, instanceType, out var propertyInfo))
		{
			throw new InvalidKeywordException();
		}

		var rawValue = propertyInfo.GetValue(instance);
		if (Keyword.GetKeywordAttribute(keyword, instanceType)!.KeywordConverterType is { } converterType)
		{
			var converter = (KeywordValueConverter)Activator.CreateInstance(converterType)!;
			return converter.TryConvert(rawValue, instance, out var valueConverted)
				? valueConverted
				: throw new InvalidKeywordException();
		}
		return rawValue;
	}

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider as CultureInfo);
}
