namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Defines a type that represents a value from a <see cref="CellSymbol"/>,
/// like variable <c><i>x</i></c> defined in an assignment <c>r3c2 = <i>x</i></c>.
/// </summary>
/// <param name="mask">The mask.</param>
/// <seealso cref="CellSymbol"/>
public readonly struct CellSymbolValue(byte mask) :
	IEquatable<CellSymbolValue>,
	IEqualityOperators<CellSymbolValue, CellSymbolValue, bool>
{
	/// <summary>
	/// Represents an invalid instance.
	/// </summary>
	public static readonly CellSymbolValue Invalid = new(byte.MaxValue);


	/// <summary>
	/// Indicates the mask. The mask uses 5 of 8 bits:
	/// <code>
	/// .---------------------------.
	/// |  7-5 |  4 | 3 | 2 | 1 | 0 |
	/// :------+----+---------------:
	/// |Unused|Type|     Index     |
	/// '---------------------------'
	/// </code>
	/// </summary>
	private readonly byte _mask = mask;


	/// <summary>
	/// Initializes an <see cref="CellSymbolValue"/> instance via the specified type and value.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="value">The value.</param>
	public CellSymbolValue(CellSymbolType type, Digit value) : this((byte)((int)type << 4 | value))
	{
	}


	/// <summary>
	/// Indicates the assumed type.
	/// </summary>
	public CellSymbolType Type => (CellSymbolType)(_mask >> 4 & 1);

	/// <summary>
	/// Indicates the index.
	/// </summary>
	public Digit Index => _mask & 15;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is CellSymbolValue comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(CellSymbolValue other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> ToString(
			SR.IsEnglish(CultureInfo.CurrentUICulture)
				? BabaGroupInitialLetter.EnglishLetter_X
				: BabaGroupInitialLetter.EnglishLetter_A,
			BabaGroupLetterCase.Lower
		);

	/// <summary>
	/// Returns a string that represents the current instance.
	/// </summary>
	/// <param name="initialLetter">The initial letter.</param>
	/// <param name="case">The letter case.</param>
	/// <returns>A string that represents the current instance.</returns>
	public string ToString(BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> Equals(Invalid) ? "<invalid>" : initialLetter.GetSequence(@case)[Index].ToString();


	/// <inheritdoc/>
	public static bool operator ==(CellSymbolValue left, CellSymbolValue right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(CellSymbolValue left, CellSymbolValue right) => !(left == right);
}
