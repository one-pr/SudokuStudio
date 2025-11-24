namespace Sudoku.Analytics.BabaGrouping.Conclusions;

/// <summary>
/// Represents a type of conclusion that tells the cells with supposition of the specified letter must be the specified digit.
/// </summary>
/// <param name="symbols"><inheritdoc cref="BabaGroupingConclusion(ComplexCellSymbol)" path="/param[@name='symbols']"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
public sealed class OccupationBabaGroupingConclusion(ComplexCellSymbol symbols, Digit digit, House house) : BabaGroupingConclusion(symbols)
{
	/// <inheritdoc/>
	public override BabaGroupingConclusionType Type => BabaGroupingConclusionType.Occupation;

	/// <summary>
	/// Indicates the house that makes the conclusion valid.
	/// </summary>
	public House House { get; } = house;

	/// <summary>
	/// Indicates the digit.
	/// </summary>
	public Digit Digit { get; } = digit;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] BabaGroupingConclusion? other)
		=> other is OccupationBabaGroupingConclusion comparer
		&& Digit == comparer.Digit && Symbols == comparer.Symbols;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(typeof(OccupationBabaGroupingConclusion), Digit, Symbols);

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var digitsString = CoordinateConverter.GetInstance(formatProvider).DigitConverter((Mask)(1 << Digit));
		return $"{Symbols.Values} = {digitsString}";
	}
}
