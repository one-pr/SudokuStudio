namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents an on/off Plot-formatted chain converter.
/// </summary>
public sealed class OnOffPlotChainConverter : IChainConverter
{
	/// <summary>
	/// The backing implementation instance.
	/// </summary>
	private readonly CustomizedChainConverter _impl = new()
	{
		MakeLettersUpperCase = true,
		MakeDigitBeforeCell = true,
		AlwaysOutputBracket = true,
		DefaultSeparator = "|",
		StrongLinkConnector = null,
		WeakLinkConnector = null,
		NodeFormatType = CoordinateType.K9,
		NotationBracket = NotationBracket.Square,
		DigitBracketInCandidateGroups = NotationBracket.None,
		OnOffNotationFix = OnOffNotationFix.Prefix,
		OnOffStateFixes = ("+", "-")
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
