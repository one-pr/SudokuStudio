namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Naked Single</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="lasting"><inheritdoc cref="Lasting" path="/summary"/></param>
/// <param name="lastingHouseType"><inheritdoc cref="LastingHouseType" path="/summary"/></param>
/// <param name="excluderHouses"><inheritdoc cref="ExcluderHouses" path="/summary"/></param>
public sealed class NakedSingleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	SingleSubtype subtype,
	Digit lasting,
	HouseType lastingHouseType,
	ReadOnlySpan<House> excluderHouses
) :
	SingleStep(conclusions, views, options, cell, digit, subtype),
	ILastingTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => Options.IsDirectMode ? 23 : 10;

	/// <inheritdoc/>
	public override Technique Code
		=> ExcluderHouses.Length == 8 ? Technique.NakedSingle : Technique.NakedSingleIndirect;

	/// <summary>
	/// Indicates the lasting house type.
	/// </summary>
	public HouseType LastingHouseType { get; } = lastingHouseType;

	/// <inheritdoc cref="ILastingTrait.Lasting"/>
	public Digit Lasting { get; } = lasting;

	/// <summary>
	/// The house to be displayed.
	/// </summary>
	public House House => Cell.GetHouse(LastingHouseType);

	/// <summary>
	/// Indicates excluder houses.
	/// </summary>
	public ReadOnlyMemory<House> ExcluderHouses { get; } = excluderHouses.ToArray();


	/// <inheritdoc/>
	public override string GetName(CultureInfo? culture)
	{
		var baseName = base.GetName(culture);
		if (!Options.IsDirectMode)
		{
			return baseName;
		}

		var lastDigitsCountString = string.Format(
			SR.Get("DirectSingleLastSuffix", culture),
			SR.Get($"{LastingHouseType}Name", culture),
			TechniqueNaming.GetDigitCharacter(culture, Lasting - 1)
		);
		if (SR.IsChinese(culture))
		{
			var centerDot = SR.Get("_Token_CenterDot", culture);
			return $"{baseName}{centerDot}{lastDigitsCountString}";
		}
		return $"{baseName} ({lastDigitsCountString})";
	}

	/// <inheritdoc/>
	protected internal override int NameCompareTo(Step other, CultureInfo? culture)
	{
		if (Code.CompareTo(other.Code) is var codeComparisonResult and not 0)
		{
			return codeComparisonResult;
		}

		var a = LastingHouseType;
		var b = ((NakedSingleStep)other).LastingHouseType;
		if (a.CompareTo(b) is var lastingHouseTypeComparisonResult and not 0)
		{
			return lastingHouseTypeComparisonResult;
		}

		if (!SR.IsChinese(culture))
		{
			return base.NameCompareTo(other, culture);
		}

		var leftName = GetName(culture);
		var rightName = other.GetName(culture);
		var leftDigit = TechniqueNaming.GetChineseDigit(TechniqueNaming.ChineseDigitsPattern.Match(leftName).Value[0]);
		var rightDigit = TechniqueNaming.GetChineseDigit(TechniqueNaming.ChineseDigitsPattern.Match(rightName).Value[0]);
		return leftDigit.CompareTo(rightDigit);
	}
}
