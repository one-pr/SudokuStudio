namespace Sudoku.Generating.Filtering;

/// <summary>
/// Represents a keyword condition.
/// </summary>
[TypeImpl(
	TypeImplFlags.AllObjectMethods | TypeImplFlags.EqualityOperators,
	GetHashCodeBehavior = GetHashCodeBehavior.MakeAbstract,
	ToStringBehavior = ToStringBehavior.MakeAbstract,
	OtherModifiersOnEquals = "sealed")]
public abstract partial class KeywordCondition :
	ICloneable,
	IEquatable<KeywordCondition>,
	IEqualityOperators<KeywordCondition, KeywordCondition, bool>
{
	/// <summary>
	/// Indicates the verb supported.
	/// </summary>
	public abstract KeywordVerbs Verb { get; }


	/// <summary>
	/// Determine whether the condition specified is satisfied.
	/// </summary>
	/// <typeparam name="TStep">The type of step.</typeparam>
	/// <param name="instance">The instance of type <typeparamref name="TStep"/>.</param>
	/// <param name="keyword">The keyword (a property that will be checked, specified by its name).</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public abstract bool IsSatisifed<TStep>(TStep instance, string keyword) where TStep : Step;

	/// <summary>
	/// Determines whether the specified value configured is valid.
	/// </summary>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public virtual bool IsValueValid() => true;

	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] KeywordCondition? other);

	/// <inheritdoc cref="ICloneable.Clone"/>
	public abstract KeywordCondition Clone();

	/// <summary>
	/// Returns the value of the property in a step instance.
	/// </summary>
	/// <typeparam name="TStep">The type of step.</typeparam>
	/// <param name="instance">The instance of type <typeparamref name="TStep"/>.</param>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The real value set. You should cast to target type if you want.</returns>
	/// <exception cref="InvalidKeywordException">
	/// Throws when the keyword is not found in type <typeparamref name="TStep"/>, or it is not a keyword.
	/// </exception>
	protected object? GetValue<TStep>(TStep instance, string keyword) where TStep : Step
		=> Keyword.IsKeyword<TStep>(keyword, out var propertyInfo)
			? propertyInfo.GetValue(instance)
			: throw new InvalidKeywordException();

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();
}
