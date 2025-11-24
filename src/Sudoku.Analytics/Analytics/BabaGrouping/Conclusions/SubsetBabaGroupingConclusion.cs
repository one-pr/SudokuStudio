namespace Sudoku.Analytics.BabaGrouping.Conclusions;

/// <summary>
/// Represents a type of conclusion that describes a list of cells forms a subset.
/// An instance of this type can represent two cases:
/// <list type="number">
/// <item>The cells inside a house form a subset (hidden subset)</item>
/// <item>
/// Every cell in the cells cannot see each other, but they fills with different digits (different symbols),
/// so they can be a distributed disjointed subset
/// </item>
/// </list>
/// </summary>
/// <param name="symbols"><inheritdoc cref="BabaGroupingConclusion(ComplexCellSymbol)" path="/param[@name='symbols']"/></param>
/// <param name="digitsMask">The digits mask.</param>
public sealed class SubsetBabaGroupingConclusion(ComplexCellSymbol symbols, Mask digitsMask) : BabaGroupingConclusion(symbols)
{
	/// <inheritdoc/>
	public override BabaGroupingConclusionType Type => BabaGroupingConclusionType.Subset;

	/// <summary>
	/// Indicates the fact digits.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] BabaGroupingConclusion? other)
		=> other is SubsetBabaGroupingConclusion comparer
		&& DigitsMask == comparer.DigitsMask && Symbols == comparer.Symbols;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(typeof(SubsetBabaGroupingConclusion), DigitsMask, Symbols);

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var digitsString = CoordinateConverter.GetInstance(formatProvider).DigitConverter(DigitsMask);
		return $"{Symbols.Values} = {digitsString}";
	}
}
