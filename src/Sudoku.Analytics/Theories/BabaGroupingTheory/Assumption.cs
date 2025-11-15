namespace Sudoku.Theories.BabaGroupingTheory;

/// <summary>
/// Represents an assumption.
/// </summary>
/// <param name="mask"><inheritdoc cref="_mask" path="/summary"/></param>
public readonly struct Assumption(int mask) : IEquatable<Assumption>, IEqualityOperators<Assumption, Assumption, bool>
{
	/// <summary>
	/// Indicates the mask.
	/// </summary>
	private readonly int _mask = mask;


	/// <summary>
	/// Indicates the type of assumed value.
	/// </summary>
	public AssumedValueType AssumedValueType => (AssumedValueType)(BitOperations.Log2((uint)(_mask & (1 << 18) - 1)) / 9);

	/// <summary>
	/// Indicates the cell used.
	/// </summary>
	public Cell Cell => _mask >>> 18;

	/// <summary>
	/// Indicates assumed values.
	/// </summary>
	public ReadOnlySpan<AssumedValue> AssumedValues
	{
		get
		{
			var result = new List<AssumedValue>();
			foreach (var index in _mask & (1 << 18) - 1)
			{
				var type = index / 9;
				var value = index % 9;
				result.Add(new((AssumedValueType)type, value));
			}
			return result.AsSpan();
		}
	}


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Assumption comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Assumption other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public override string ToString() => ToString(null, BabaGroupInitialLetter.EnglishLetter_X, BabaGroupLetterCase.Lower);

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public string ToString(BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> ToString(null, initialLetter, @case);

	/// <summary>
	/// Returns a string that represents the current instance.
	/// </summary>
	/// <param name="formatProvider">The format provider.</param>
	/// <param name="initialLetter">The initial letter.</param>
	/// <param name="case">The letter case.</param>
	/// <returns>A string that represents the current instance.</returns>
	public string ToString(IFormatProvider? formatProvider, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var assumedValuesString = string.Join('|', from value in AssumedValues select value.ToString(initialLetter, @case));
		return $"{converter.CellConverter(Cell.AsCellMap())} = {assumedValuesString}";
	}


	/// <inheritdoc/>
	public static bool operator ==(Assumption left, Assumption right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Assumption left, Assumption right) => !(left == right);
}
