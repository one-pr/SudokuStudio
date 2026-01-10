namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents an Eureka-formatted chain converter.
/// </summary>
public sealed class EurekaChainConverter : IChainConverter
{
	/// <summary>
	/// The backing implementation instance.
	/// </summary>
	private readonly CustomizedChainConverter _impl = new()
	{
		MakeDigitBeforeCell = true,
		FoldLinksInCell = true,
		DefaultSeparator = "|",
		StrongLinkConnector = "=",
		WeakLinkConnector = "-",
		DigitBracketInCandidateGroups = NotationBracket.None,
		DigitGroupSameCellBracket = NotationBracket.Round
	};


	/// <inheritdoc/>
	IChainConverter IChainConverter.Impl => _impl;


	/// <inheritdoc/>
	public bool TryFormat(Chain value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
		=> _impl.TryFormat(value, provider, out result);

	/// <inheritdoc/>
	/// <exception cref="NotSupportedException">Not supported. Always thrown.</exception>
	[DoesNotReturn]
	bool IChainConverter.TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, [NotNullWhen(true)] out Chain? result)
		=> throw new NotSupportedException();
}
