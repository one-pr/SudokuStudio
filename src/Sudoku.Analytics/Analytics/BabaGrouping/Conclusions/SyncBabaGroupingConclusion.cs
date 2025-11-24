namespace Sudoku.Analytics.BabaGrouping.Conclusions;

/// <summary>
/// Represents a type of conclusion that sync's a cell or a list of cells.
/// </summary>
/// <param name="symbols"><inheritdoc cref="BabaGroupingConclusion(ComplexCellSymbol)" path="/param[@name='symbols']"/></param>
/// <param name="digitsMask">The digit mask.</param>
public sealed class SyncBabaGroupingConclusion(ComplexCellSymbol symbols, Mask digitsMask) : BabaGroupingConclusion(symbols)
{
	/// <inheritdoc/>
	public override BabaGroupingConclusionType Type => BabaGroupingConclusionType.Sync;

	/// <summary>
	/// Indicates the fact digits.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] BabaGroupingConclusion? other)
		=> other is SyncBabaGroupingConclusion comparer
		&& DigitsMask == comparer.DigitsMask && Symbols == comparer.Symbols;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(typeof(SyncBabaGroupingConclusion), DigitsMask, Symbols);

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var digitsString = CoordinateConverter.GetInstance(formatProvider).DigitConverter(DigitsMask);
		return $"{Symbols.Values} = {digitsString}";
	}
}
