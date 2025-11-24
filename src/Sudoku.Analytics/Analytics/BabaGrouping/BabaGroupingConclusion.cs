namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Represents a conclusion on baba group spreading logic.
/// </summary>
/// <param name="cellSymbols">Indicates the cell symbols.</param>
public abstract class BabaGroupingConclusion(ComplexCellSymbol cellSymbols) :
	IEquatable<BabaGroupingConclusion>,
	IEqualityOperators<BabaGroupingConclusion, BabaGroupingConclusion, bool>
{
	/// <summary>
	/// Indicates the type of conclusion.
	/// </summary>
	public abstract BabaGroupingConclusionType Type { get; }

	/// <summary>
	/// Indicates the symbols.
	/// </summary>
	public ComplexCellSymbol Symbols { get; } = cellSymbols;


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as BabaGroupingConclusion);

	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] BabaGroupingConclusion? other);

	/// <inheritdoc/>
	public abstract override int GetHashCode();

	/// <inheritdoc/>
	public sealed override string ToString() => ToString(null);

	/// <summary>
	/// Converts the current instance into the string.
	/// </summary>
	/// <param name="formatProvider">The format provider.</param>
	/// <returns>The string.</returns>
	public abstract string ToString(IFormatProvider? formatProvider);


	/// <inheritdoc/>
	public static bool operator ==(BabaGroupingConclusion? left, BabaGroupingConclusion? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(BabaGroupingConclusion? left, BabaGroupingConclusion? right) => !(left == right);
}
