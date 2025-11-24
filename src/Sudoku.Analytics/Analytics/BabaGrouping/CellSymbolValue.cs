namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Defines a type that represents a value from a <see cref="CellSymbol"/>,
/// like variable <c><i>x</i></c> defined in an assignment <c>r3c2 = <i>x</i></c>.
/// </summary>
/// <param name="mask">The mask.</param>
/// <seealso cref="CellSymbol"/>
public readonly struct CellSymbolValue(byte mask) :
	IComparable<CellSymbolValue>,
	IComparisonOperators<CellSymbolValue, CellSymbolValue, bool>,
	IEquatable<CellSymbolValue>,
	IEqualityOperators<CellSymbolValue, CellSymbolValue, bool>,
	IFormattable,
	IMinMaxValue<CellSymbolValue>,
	IParsable<CellSymbolValue>
{
	/// <summary>
	/// Represents an invalid instance.
	/// </summary>
	public static readonly CellSymbolValue Invalid = new(byte.MaxValue);

	/// <summary>
	/// Indicates all possible accurate values.
	/// </summary>
	public static readonly CellSymbolValue[] AccurateValues = [
		new(CellSymbolType.Accurate, 0), new(CellSymbolType.Accurate, 1), new(CellSymbolType.Accurate, 2),
		new(CellSymbolType.Accurate, 3), new(CellSymbolType.Accurate, 4), new(CellSymbolType.Accurate, 5),
		new(CellSymbolType.Accurate, 6), new(CellSymbolType.Accurate, 7), new(CellSymbolType.Accurate, 8)
	];

	/// <summary>
	/// Indicates all possible fuzzy values.
	/// </summary>
	public static readonly CellSymbolValue[] FuzzyValues = [
		new(CellSymbolType.Fuzzy, 0), new(CellSymbolType.Fuzzy, 1), new(CellSymbolType.Fuzzy, 2),
		new(CellSymbolType.Fuzzy, 3), new(CellSymbolType.Fuzzy, 4), new(CellSymbolType.Fuzzy, 5),
		new(CellSymbolType.Fuzzy, 6), new(CellSymbolType.Fuzzy, 7), new(CellSymbolType.Fuzzy, 8)
	];


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
	/// <param name="value">The value. The value should be in range 0..9.</param>
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
	static CellSymbolValue IMinMaxValue<CellSymbolValue>.MinValue => default;

	/// <inheritdoc/>
	static CellSymbolValue IMinMaxValue<CellSymbolValue>.MaxValue => new(CellSymbolType.Accurate, 8);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is CellSymbolValue comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(CellSymbolValue other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc/>
	public int CompareTo(CellSymbolValue other) => Index.CompareTo(other.Index);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(BabaGroupInitialLetter.CurrentCultureInstance, BabaGroupLetterCase.Lower);

	/// <summary>
	/// Returns a string that represents the current instance.
	/// </summary>
	/// <param name="initialLetter">The initial letter.</param>
	/// <param name="case">The letter case.</param>
	/// <returns>A string that represents the current instance.</returns>
	public string ToString(BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> Equals(Invalid) ? "<invalid>" : initialLetter.GetSequence(@case)[Index].ToString();

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupLetterCase)"/>
	public string ToString(IFormatProvider? formatProvider) => ToString(formatProvider, BabaGroupLetterCase.Lower);

	/// <summary>
	/// Formats the value of the current instance using the specified format.
	/// </summary>
	/// <param name="formatProvider">
	/// The provider to use to format the value, or a <see langword="null"/> reference
	/// to obtain the format information from the current locale setting of the operating system.
	/// </param>
	/// <param name="case">The case.</param>
	/// <returns>The value of the current instance in the specified format.</returns>
	public string ToString(IFormatProvider? formatProvider, BabaGroupLetterCase @case)
		=> ToString(formatProvider as CultureInfo ?? CultureInfo.CurrentUICulture, @case);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, out CellSymbolValue result) => TryParse(s, null, out result);

	/// <summary>
	/// Tries to parse a string into a value.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="initialLetter">The initial letter of sequence.</param>
	/// <param name="case">The case.</param>
	/// <param name="result">
	/// When this method returns, contains the result of successfully parsing <paramref name="s"/>
	/// or an undefined value on failure.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if <paramref name="s"/> was successfully parsed; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryParse([NotNullWhen(true)] string? s, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case, out CellSymbolValue result)
	{
		try
		{
			if (s is null)
			{
				result = default;
				return false;
			}

			result = Parse(s, initialLetter, @case);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CellSymbolValue result)
		=> TryParse(s, provider, BabaGroupLetterCase.Lower, out result);

	/// <summary>
	/// Tries to parse a string into a value.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s"/>.</param>
	/// <param name="case">The case.</param>
	/// <param name="result">
	/// When this method returns, contains the result of successfully parsing <paramref name="s"/>
	/// or an undefined value on failure.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if <paramref name="s"/> was successfully parsed; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, BabaGroupLetterCase @case, out CellSymbolValue result)
		=> TryParse(s, provider as CultureInfo ?? CultureInfo.CurrentUICulture, @case, out result);

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static CellSymbolValue Parse(string s) => Parse(s, null, BabaGroupLetterCase.Lower);

	/// <inheritdoc/>
	public static CellSymbolValue Parse(string s, IFormatProvider? provider) => Parse(s, provider, BabaGroupLetterCase.Lower);

	/// <summary>
	/// Parses a string into a value.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="initialLetter">The initial letter of sequence.</param>
	/// <param name="case">The case.</param>
	/// <returns>The result of parsing <paramref name="s"/>.</returns>
	public static CellSymbolValue Parse(string s, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		if (s.Trim() is not [var onlyCharacter])
		{
			throw new FormatException();
		}
		if (onlyCharacter is >= '1' and <= '9')
		{
			return new(CellSymbolType.Accurate, onlyCharacter - '1');
		}

		var sequence = initialLetter.GetSequence(@case);
		foreach (var (i, value) in sequence.Index())
		{
			if (value == onlyCharacter)
			{
				return new(CellSymbolType.Fuzzy, i);
			}
		}
		throw new FormatException();
	}

	/// <summary>
	/// Parses a string into a value.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s"/>.</param>
	/// <param name="case">The case.</param>
	/// <returns>The result of parsing <paramref name="s"/>.</returns>
	public static CellSymbolValue Parse(string s, IFormatProvider? provider, BabaGroupLetterCase @case)
		=> Parse(
			s,
			SR.IsEnglish(provider as CultureInfo ?? CultureInfo.CurrentUICulture)
				? BabaGroupInitialLetter.EnglishLetter_X
				: BabaGroupInitialLetter.EnglishLetter_A,
			@case
		);


	/// <inheritdoc/>
	public static bool operator ==(CellSymbolValue left, CellSymbolValue right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(CellSymbolValue left, CellSymbolValue right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(CellSymbolValue left, CellSymbolValue right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(CellSymbolValue left, CellSymbolValue right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(CellSymbolValue left, CellSymbolValue right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(CellSymbolValue left, CellSymbolValue right) => left.CompareTo(right) <= 0;
}
