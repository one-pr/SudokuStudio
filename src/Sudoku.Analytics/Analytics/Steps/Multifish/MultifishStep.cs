namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Multifish</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="truths"><inheritdoc cref="Truths" path="/summary"/></param>
/// <param name="links"><inheritdoc cref="Links" path="/summary"/></param>
public sealed class MultifishStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in SpaceSet truths,
	in SpaceSet links
) : FullPencilmarkingStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 96;

	/// <inheritdoc/>
	public override Technique Code => Technique.Multifish;

	/// <summary>
	/// Indicates the truths.
	/// </summary>
	public SpaceSet Truths { get; } = truths;

	/// <summary>
	/// Indicates the links.
	/// </summary>
	public SpaceSet Links { get; } = links;

	/// <inheritdoc/>
	public override Mask DigitsUsed
	{
		get
		{
			var result = (Mask)0;
			foreach (var truth in Truths | Links)
			{
				if (truth is { IsHouseRelated: true, Digit: var digit })
				{
					result |= (Mask)(1 << digit);
				}
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [TruthsCountStr, LinksCountStr, TruthsStr, LinksStr]),
			new(SR.ChineseLanguage, [TruthsCountStr, LinksCountStr, TruthsStr, LinksStr])
		];

	private string TruthsCountStr => Truths.Count.ToString();

	private string LinksCountStr => Links.Count.ToString();

	private string TruthsStr => Truths.ToString();

	private string LinksStr => Links.ToString();
}
