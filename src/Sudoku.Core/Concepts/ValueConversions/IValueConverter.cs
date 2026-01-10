namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides a way to convert values between <see cref="string"/> and <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The target value to be converted.</typeparam>
public interface IValueConverter<T> where T : allows ref struct
{
	/// <summary>
	/// Performs parsing operation.
	/// </summary>
	/// <param name="text">The text to parse.</param>
	/// <param name="provider">The provider that effects parsing module (like culture, numeric hanlding, etc.).</param>
	/// <param name="result">The result parsed.</param>
	/// <returns>
	/// A <see cref="bool"/> result indicating whether operation is successfully done without any invalid cases encountered.
	/// </returns>
	bool TryParse(scoped ReadOnlySpan<char> text, IFormatProvider? provider, [NotNullWhen(true)] out T? result);

	/// <summary>
	/// Performs formatting operation.
	/// </summary>
	/// <param name="value">The value to format.</param>
	/// <param name="provider">The provider that effects parsing module (like culture, numeric hanlding, etc.).</param>
	/// <param name="result">The result string formatted.</param>
	/// <returns>
	/// A <see cref="bool"/> result indicating whether operation is successfully done without any invalid cases encountered.
	/// </returns>
	bool TryFormat(scoped ref readonly T value, IFormatProvider? provider, [NotNullWhen(true)] out string? result);
}
