namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents a value converter that can cast a value from a variant type into a keyword type
/// supported in enumeration type <see cref="KeywordType"/>.
/// </summary>
/// <seealso cref="KeywordType"/>
public abstract class KeywordValueConverter
{
	/// <summary>
	/// Indicates the target type.
	/// </summary>
	public abstract KeywordType TargetType { get; }

	/// <summary>
	/// Indicates the base type.
	/// </summary>
	public abstract Type BaseType { get; }


	/// <summary>
	/// Try to convert a value to the target type.
	/// </summary>
	/// <param name="value">The value to convert from.</param>
	/// <param name="step">The step instance.</param>
	/// <param name="result">The result converted.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the conversion success.</returns>
	public abstract bool TryConvert(object? value, Step step, [NotNullWhen(true)] out dynamic? result);

	/// <summary>
	/// Try to convert a value back to base type.
	/// </summary>
	/// <param name="value">The value to convert from.</param>
	/// <param name="step">The step instance.</param>
	/// <param name="result">The result converted.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the conversion success.</returns>
	/// <remarks>
	/// This method can keep not being implemented if the target value converter doesn't want to support this.
	/// </remarks>
	public virtual bool TryConvertBack(object? value, Step step, [NotNullWhen(true)] out dynamic? result)
	{
		result = null;
		return false;
	}
}
