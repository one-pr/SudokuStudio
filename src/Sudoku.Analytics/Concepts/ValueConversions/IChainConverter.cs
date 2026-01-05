namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides a chain converter.
/// </summary>
public interface IChainConverter : IValueConverter<Chain>
{
	/// <summary>
	/// Indicates the backing implementation converter.
	/// </summary>
	protected IChainConverter Impl { get; }


	/// <inheritdoc cref="IValueConverter{T}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out T)"/>
	new bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, [NotNullWhen(true)] out Chain? result);

	/// <inheritdoc cref="IValueConverter{T}.TryFormat(ref readonly T, IFormatProvider?, out string?)"/>
	bool TryFormat(Chain value, IFormatProvider? provider, [NotNullWhen(true)] out string? result);

	/// <inheritdoc/>
	bool IValueConverter<Chain>.TryFormat(ref readonly Chain value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
		=> TryFormat(value, provider, out result);

	/// <inheritdoc/>
	bool IValueConverter<Chain>.TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, [NotNullWhen(true)] out Chain? result)
		=> TryParse(text, provider, out result);
}
