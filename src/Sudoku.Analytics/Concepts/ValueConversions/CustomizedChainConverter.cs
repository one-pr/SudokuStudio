namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides a customized chain converter; you can specify customized properties (even candidate converter) into this type.
/// </summary>
public partial class CustomizedChainConverter : IChainConverter
{
	/// <inheritdoc cref="RxCyConverter.MakeDigitBeforeCell"/>
	public bool MakeDigitBeforeCell { get; init; } = false;

	/// <inheritdoc cref="RxCyConverter.MakeLettersUpperCase"/>
	public bool MakeLettersUpperCase { get; init; } = false;

	/// <summary>
	/// Indicates whether strong and weak links inside a cell will be folded. By default it's <see langword="false"/>.
	/// </summary>
	public bool FoldLinksInCell { get; init; } = false;

	/// <summary>
	/// Indicates whether digits are inlined in links. By default it's <see langword="false"/>.
	/// </summary>
	public bool InlineDigitsInLink { get; init; } = false;

	/// <inheritdoc cref="RxCyConverter.AlwaysOutputBracket"/>
	public bool AlwaysOutputBracket { get; init; } = false;

	/// <inheritdoc cref="K9Converter.FinalRowLetter"/>
	public char FinalRowLetter { get; init; } = 'I';

	/// <summary>
	/// Indicates the default separator. By default it's a comma <c>", "</c>.
	/// </summary>
	public string DefaultSeparator { get; init; } = ", ";

	/// <summary>
	/// Indicates inlined digits separator. By default it's pipe operator <c>"|"</c>.
	/// </summary>
	public string InlinedDigitsSeparator { get; init; } = "|";

	/// <summary>
	/// Indicates the connector text for strong links. By default it's a double equal sign <c>" == "</c>.
	/// </summary>
	public string? StrongLinkConnector { get; init; } = " == ";

	/// <summary>
	/// Indicates the connector text for weak links. By default it's a double minus sign <c>" -- "</c>.
	/// </summary>
	public string? WeakLinkConnector { get; init; } = " -- ";

	/// <summary>
	/// Represents a pair of fixes that describes on or off state of a chain node respectively.
	/// By default it's <see langword="null"/>.
	/// </summary>
	public (string OnFix, string OffFix)? OnOffStateFixes { get; init; }

	/// <inheritdoc cref="CoordinateConverter.NotationBracket"/>
	public NotationBracket NotationBracket { get; init; } = NotationBracket.None;

	/// <inheritdoc cref="RxCyConverter.DigitBracketInCandidateGroups"/>
	public NotationBracket DigitBracketInCandidateGroups { get; init; } = NotationBracket.None;

	/// <summary>
	/// Indicates the bracket of same-cell digit groups, e.g. the kind of bracket of <c>(2=3)r1c1</c>.
	/// By default it's <see cref="NotationBracket.Round"/>.
	/// </summary>
	public NotationBracket DigitGroupSameCellBracket { get; init; } = NotationBracket.Round;

	/// <summary>
	/// Indicates the prefix or suffix style to describe on/off state of each chain node.
	/// By default it's <see cref="OnOffNotationFix.None"/>.
	/// </summary>
	public OnOffNotationFix OnOffNotationFix { get; init; } = OnOffNotationFix.None;

	/// <summary>
	/// Indicates a type that formats each node (a group of candidates) in the chain pattern.
	/// </summary>
	public CoordinateType NodeFormatType { get; init; } = CoordinateType.RxCy;

	/// <summary>
	/// Represents customized candidate converter. The value can be <see langword="null"/> if unspecified;
	/// if specified, <see cref="NodeFormatType"/> will be ignored.
	/// </summary>
	/// <seealso cref="NodeFormatType"/>
	public CoordinateConverter? CustomizedCandidateConverter { get; init; }

	/// <inheritdoc/>
	IChainConverter IChainConverter.Impl => this;


	[GeneratedRegex("""r[1-9]+c[1-9]+\([1-9]+\)\s*==?\s*r[1-9]+c[1-9]+\([1-9]+\)(?:\s*--?\s*r[1-9]+c[1-9]+\([1-9]+\)\s*==?\s*r[1-9]+c[1-9]+\([1-9]+\))+""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex StandardFormatPattern { get; }

	[GeneratedRegex("""(?:\([1-9]+\)|\([1-9]+\s*[-=]\s*[1-9]+\)|[1-9])r[1-9]+c[1-9]+(?:\s*[-=]\s*(?:\([1-9]+\)|\([1-9]+\s*[-=]\s*[1-9]+\)|[1-9])r[1-9]+c[1-9]+)+""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex EurekaFormatPattern { get; }

	[GeneratedRegex("""==?|--?""", RegexOptions.Compiled)]
	private static partial Regex StrongOrWeakLinkPattern { get; }

	[GeneratedRegex("""(?:\([1-9]+([=-][1-9]+)?\)|[1-9]+)r[1-9]+c[1-9]+|r[1-9]+c[1-9]+\([1-9]+\)""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex CandidatePattern { get; }


	/// <inheritdoc/>
	public bool TryFormat(Chain value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		var originalConverter = (CustomizedCandidateConverter, NodeFormatType.Converter) switch
		{
			({ } baseCandidateConverter, _) => baseCandidateConverter,
			(_, RxCyConverter c) => c with
			{
				MakeLettersUpperCase = MakeLettersUpperCase,
				MakeDigitBeforeCell = MakeDigitBeforeCell,
				AlwaysOutputBracket = AlwaysOutputBracket,
				DigitBracketInCandidateGroups = DigitBracketInCandidateGroups
			},
			(_, K9Converter c) => c with
			{
				MakeLettersUpperCase = MakeLettersUpperCase,
				MakeDigitBeforeCell = MakeDigitBeforeCell,
				AlwaysOutputBracket = AlwaysOutputBracket,
				FinalRowLetter = FinalRowLetter,
				DigitBracketInCandidateGroups = DigitBracketInCandidateGroups
			},
			(_, ExcelCoordinateConverter c) => c with { MakeLettersUpperCase = MakeLettersUpperCase },
			(_, { } tempConverter) => tempConverter,
			_ => throw new InvalidOperationException()
		};
		var candidateConverter = originalConverter with { DefaultSeparator = DefaultSeparator, NotationBracket = NotationBracket };
		var needAddingBrackets_Digits = Enum.IsDefined(DigitGroupSameCellBracket) && DigitGroupSameCellBracket != NotationBracket.None;
		var needAddingBrackets_Cells = Enum.IsDefined(NotationBracket) && NotationBracket != NotationBracket.None;
		var span = value.ValidNodes;
		var sb = new StringBuilder();
		for (var (linkIndex, i) = (value.WeakStartIdentity, 0); i < span.Length; linkIndex++, i++)
		{
			var inference = Chain.Inferences[linkIndex & 1];
			ref readonly var nodeCandidates = ref span[i].Map;
			var nodeCells = nodeCandidates.Cells;
			var nodeDigits = nodeCandidates.Digits;
			ref readonly var nextNodeCandidates = ref i + 1 >= span.Length ? ref CandidateMap.Empty : ref span[i + 1].Map;
			var nextNodeCells = nextNodeCandidates.Cells;
			var nextNodeDigits = nextNodeCandidates.Digits;
			if (FoldLinksInCell && i != span.Length - 1 && nodeCells == nextNodeCells)
			{
				// (1)a=(2)a-(2)b=(3)b => (1=2)a-(2=3)b
				if (MakeDigitBeforeCell)
				{
					_ = needAddingBrackets_Digits ? sb.Append(DigitGroupSameCellBracket.OpenBracket) : sb;
					sb.Append(candidateConverter.DigitConverter(nodeDigits));
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
					sb.Append(candidateConverter.DigitConverter(nextNodeDigits));
					_ = needAddingBrackets_Digits ? sb.Append(DigitGroupSameCellBracket.ClosedBracket) : sb;
					sb.Append(nodeCells.ToString(candidateConverter));
					i++;
				}
				else
				{
					sb.Append(nodeCells.ToString(candidateConverter));
					sb.Append(needAddingBrackets_Digits ? DigitGroupSameCellBracket.OpenBracket : "(");
					sb.Append(candidateConverter.DigitConverter(nodeDigits));
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
					sb.Append(candidateConverter.DigitConverter(nextNodeDigits));
					sb.Append(needAddingBrackets_Digits ? DigitGroupSameCellBracket.ClosedBracket : ")");
				}

				inference = Chain.Inferences[++linkIndex & 1];
				goto AppendNextLinkToken;
			}

			// Append prefix of node, e.g. r3c3(2) is on => +2r3c3
			if ((OnOffNotationFix, OnOffStateFixes) is (OnOffNotationFix.Prefix, var (onPrefix, offPrefix)))
			{
				sb.Append(span[i].IsOn ? onPrefix : offPrefix);
			}

			if (InlineDigitsInLink)
			{
				// (1)a=(2)b => [a]=1|2=[b]
				_ = needAddingBrackets_Cells ? sb.Append(NotationBracket.OpenBracket) : sb;
				sb.Append(nodeCells.ToString(candidateConverter));
				_ = needAddingBrackets_Cells ? sb.Append(NotationBracket.ClosedBracket) : sb;
			}
			else
			{
				sb.Append(nodeCandidates.ToString(candidateConverter));
			}

			// Append suffix of node, e.g. r3c3(2) is off => 2r3c3-
			if ((OnOffNotationFix, OnOffStateFixes) is (OnOffNotationFix.Suffix, var (onSuffix, offSuffix)))
			{
				sb.Append(span[i].IsOn ? onSuffix : offSuffix);
			}

		AppendNextLinkToken:
			if (i != span.Length - 1)
			{
				if (InlineDigitsInLink)
				{
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
					_ = nodeDigits == nextNodeDigits
						? sb.Append(candidateConverter.DigitConverter(nodeDigits))
						: sb
							.Append(candidateConverter.DigitConverter(nodeDigits))
							.Append(InlinedDigitsSeparator)
							.Append(candidateConverter.DigitConverter(nextNodeDigits));
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
				}
				else
				{
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
				}
			}
		}
		result = sb.ToString();
		return true;
	}

	/// <inheritdoc/>
	/// <exception cref="NotSupportedException">Not supported. Always thrown.</exception>
	[DoesNotReturn]
	bool IChainConverter.TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, [NotNullWhen(true)] out Chain? result)
		=> throw new NotSupportedException("The default implementation doesn't support for parsing.");
}
